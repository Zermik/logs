using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BrokeProtocol.API;
using BrokeProtocol.Collections;
using BrokeProtocol.LiteDB;
using BrokeProtocol.Managers;
using BrokeProtocol.Required;
using BrokeProtocol.Utility;
using BrokeProtocol.Utility.AI;
using BrokeProtocol.Utility.Jobs;
using BrokeProtocol.Utility.Networking;
using ENet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pathfinding;
using UnityEngine;

namespace BrokeProtocol.Entities
{
	// Token: 0x020000A4 RID: 164
	public class SvPlayer : SvMovable
	{
		// Token: 0x1700007D RID: 125
		// (get) Token: 0x0600035D RID: 861 RVA: 0x00011DE9 File Offset: 0x0000FFE9
		public override EntityEvents Events
		{
			get
			{
				return SvPlayer.events;
			}
		}

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x0600035E RID: 862 RVA: 0x00011DF0 File Offset: 0x0000FFF0
		public override float RespawnTime
		{
			get
			{
				if (!this.player.isHuman)
				{
					return 40f;
				}
				return 10f;
			}
		}

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x0600035F RID: 863 RVA: 0x00011E0A File Offset: 0x0001000A
		protected override float DamageScalar
		{
			get
			{
				return 0.022f;
			}
		}

		// Token: 0x06000360 RID: 864 RVA: 0x00011E14 File Offset: 0x00010014
		public void QueuePacket()
		{
			byte[] array = new byte[Buffers.writer.Position()];
			Buffer.BlockCopy(Buffers.writeBuffer, 0, array, 0, Buffers.writer.Position());
			this.packetQueue.Enqueue(array);
		}

		// Token: 0x06000361 RID: 865 RVA: 0x00011E54 File Offset: 0x00010054
		public void InitializeHuman(ConnectData connectData)
		{
			this.packetQueue = new Queue<byte[]>();
			this.player.isHuman = true;
			this.connectData = connectData;
			this.connection = connectData.connection;
			connectData.connectionStatus = ConnectionStatus.LoggedIn;
			this.player.profile = connectData.profileURL.CleanProfile();
			this.player.username = connectData.username;
			this.player.language = Util.languages[connectData.languageIndex];
		}

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x06000362 RID: 866 RVA: 0x00011ED0 File Offset: 0x000100D0
		public override float SpawnRate
		{
			get
			{
				return this.job.GetSpawnRate();
			}
		}

		// Token: 0x06000363 RID: 867 RVA: 0x00011EDD File Offset: 0x000100DD
		public bool IsFollower(ShPlayer target)
		{
			return target == this.leader && this.leader == this.targetEntity;
		}

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x06000364 RID: 868 RVA: 0x00011F00 File Offset: 0x00010100
		public ShEntity TargetMount
		{
			get
			{
				if (!(this.targetEntity.GetMount() != null))
				{
					return this.targetEntity;
				}
				return this.targetEntity.GetMount();
			}
		}

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x06000365 RID: 869 RVA: 0x00011F27 File Offset: 0x00010127
		public IEnumerable<Group> Groups
		{
			get
			{
				return from @group in GroupHandler.Groups.Values
				where @group.IsMember(this.player)
				select @group;
			}
		}

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x06000366 RID: 870 RVA: 0x00011F44 File Offset: 0x00010144
		public Group PrimaryGroup
		{
			get
			{
				return this.Groups.FirstOrDefault<Group>();
			}
		}

		// Token: 0x06000367 RID: 871 RVA: 0x00011F51 File Offset: 0x00010151
		public void SendGameMessage(string message)
		{
			if (!string.IsNullOrWhiteSpace(message))
			{
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.GameMessage, new object[]
				{
					message
				});
			}
		}

		// Token: 0x06000368 RID: 872 RVA: 0x00011F70 File Offset: 0x00010170
		public void StartGoalMarker(ShEntity goalEntity)
		{
			if (this.goalMarker)
			{
				this.DestroyGoalMarker();
			}
			this.goalEntity = goalEntity;
			this.goalMarker = this.svManager.AddNewEntity<ShEntity>(this.svManager.markerGoalPrefab, MonoBehaviourSingleton<SceneManager>.Instance.ExteriorPlace, goalEntity.Position, Quaternion.identity, new IDCollection<ShPlayer>
			{
				this.player
			});
			base.StartCoroutine(this.HandleGoalMarker());
		}

		// Token: 0x06000369 RID: 873 RVA: 0x00011FE6 File Offset: 0x000101E6
		public void DestroyGoalMarker()
		{
			if (this.goalMarker)
			{
				this.goalMarker.Destroy();
				this.goalMarker = null;
				this.goalEntity = null;
			}
		}

		// Token: 0x0600036A RID: 874 RVA: 0x0001200E File Offset: 0x0001020E
		protected IEnumerator HandleGoalMarker()
		{
			WaitForSeconds delay = new WaitForSeconds(1f);
			while (this.goalMarker && this.goalEntity)
			{
				this.goalMarker.svEntity.SvRelocate(this.goalEntity.svEntity.ExteriorPosition, default(Quaternion), null);
				yield return delay;
			}
			this.DestroyGoalMarker();
			yield break;
		}

		// Token: 0x0600036B RID: 875 RVA: 0x00012020 File Offset: 0x00010220
		public void StartSelfMarker(IDCollection<ShPlayer> subscribedPlayers)
		{
			if (this.selfMarker)
			{
				this.DestroySelfMarker();
			}
			this.selfMarker = this.svManager.AddNewEntity<ShEntity>(this.svManager.markerSelfPrefab, MonoBehaviourSingleton<SceneManager>.Instance.ExteriorPlace, this.physical.Position, Quaternion.identity, subscribedPlayers);
			base.StartCoroutine(this.HandleSelfMarker());
		}

		// Token: 0x0600036C RID: 876 RVA: 0x00012084 File Offset: 0x00010284
		public void DestroySelfMarker()
		{
			if (this.selfMarker)
			{
				this.selfMarker.Destroy();
				this.selfMarker = null;
			}
		}

		// Token: 0x0600036D RID: 877 RVA: 0x000120A5 File Offset: 0x000102A5
		protected IEnumerator HandleSelfMarker()
		{
			float killTime = Time.time + 30f;
			WaitForSeconds delay = new WaitForSeconds(1f);
			while (this.selfMarker && Time.time < killTime)
			{
				this.selfMarker.svEntity.SvRelocate(base.ExteriorPosition, default(Quaternion), null);
				yield return delay;
			}
			this.DestroySelfMarker();
			yield break;
		}

		// Token: 0x0600036E RID: 878 RVA: 0x000120B4 File Offset: 0x000102B4
		public void UpdateStatsDelta(float d0, float d1, float d2)
		{
			if (d0 != 0f || d1 != 0f || d2 != 0f)
			{
				this.player.stats[0] = Mathf.Clamp01(this.player.stats[0] + d0);
				this.player.stats[1] = Mathf.Clamp01(this.player.stats[1] + d1);
				this.player.stats[2] = Mathf.Clamp01(this.player.stats[2] + d2);
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.Stats, new object[]
				{
					this.player.stats[0],
					this.player.stats[1],
					this.player.stats[2]
				});
				this.player.CorrectMoveMode();
			}
		}

		// Token: 0x0600036F RID: 879 RVA: 0x00012199 File Offset: 0x00010399
		protected IEnumerator SendEntityData()
		{
			foreach (Place place in MonoBehaviourSingleton<SceneManager>.Instance.places)
			{
				if (!place.IsClone)
				{
					foreach (KeyValuePair<int, Place> keyValuePair in place.clones)
					{
						Buffers.writer.SeekZero();
						Buffers.writer.WriteClPacket(ClPacket.ClonePlace);
						Buffers.writer.Write(place.Index);
						Buffers.writer.Write(keyValuePair.Value.Index);
						Buffers.writer.Write(keyValuePair.Value.offset);
						MonoBehaviourSingleton<SvManager>.Instance.SendToConnection(this.connection, PacketFlags.Reliable);
					}
				}
			}
			List<object[]> arguments = new List<object[]>();
			foreach (ShEntity shEntity in EntityCollections.Entities)
			{
				if (shEntity.svEntity != this && shEntity.svEntity.subscribers == null)
				{
					object[] item = null;
					shEntity.svEntity.WriteInitData(ref item, false);
					arguments.Add(item);
				}
			}
			yield return null;
			if (!this.svManager.Connected(this.connectData.connection))
			{
				yield break;
			}
			Stopwatch watch = Stopwatch.StartNew();
			List<byte[]> bufferList = new List<byte[]>();
			int totalLength = 0;
			foreach (object[] args in arguments)
			{
				Buffers.writer.SeekZero();
				Buffers.WriteObject(args);
				int num = Buffers.writer.Position();
				totalLength += num;
				byte[] array = new byte[num];
				Array.Copy(Buffers.writeBuffer, array, num);
				bufferList.Add(array);
				if (watch.ElapsedMilliseconds > 15L)
				{
					yield return null;
					if (!this.svManager.Connected(this.connectData.connection))
					{
						yield break;
					}
					watch.Restart();
				}
			}
			List<object[]>.Enumerator enumerator4 = default(List<object[]>.Enumerator);
			yield return null;
			if (!this.svManager.Connected(this.connectData.connection))
			{
				yield break;
			}
			byte[] entityData = new byte[totalLength];
			int num2 = 0;
			foreach (byte[] array2 in bufferList)
			{
				int num3 = array2.Length;
				Array.Copy(array2, 0, entityData, num2, num3);
				num2 += num3;
			}
			yield return null;
			if (!this.svManager.Connected(this.connectData.connection))
			{
				yield break;
			}
			this.svManager.SendGameMessageToConnection(this.connection, "Entity Data: " + totalLength.ToStringKB());
			yield return MonoBehaviourSingleton<SceneManager>.Instance.StartCoroutine(this.svManager.TransferData(this.connectData, entityData, ClPacket.EntityData));
			if (!this.svManager.Connected(this.connectData.connection))
			{
				yield break;
			}
			object[] args2 = null;
			this.WriteInitData(ref args2, true);
			Buffers.writer.SeekZero();
			Buffers.writer.WriteClPacket(ClPacket.AddEntity);
			Buffers.WriteObject(args2);
			MonoBehaviourSingleton<SvManager>.Instance.SendToConnection(this.connection, PacketFlags.Reliable);
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.TimeInfo, new object[]
			{
				MonoBehaviourSingleton<SceneManager>.Instance.dayLength,
				Time.timeScale
			});
			if (!MonoBehaviourSingleton<SceneManager>.Instance.defaultEnvironment)
			{
				this.svManager.SvCustomEnvironmentPacket(this.player);
			}
			yield break;
			yield break;
		}

		// Token: 0x06000370 RID: 880 RVA: 0x000121A8 File Offset: 0x000103A8
		public void Initialized()
		{
			this.holdPackets = false;
			byte[] array;
			while (this.packetQueue.TryDequeue(out array))
			{
				Buffers.writer.SeekZero();
				foreach (byte v in array)
				{
					Buffers.writer.Write(v);
				}
				MonoBehaviourSingleton<SvManager>.Instance.SendToConnection(this.connection, PacketFlags.Reliable);
			}
			this.packetQueue = null;
			SvPlayer.events.Ready(this.player);
		}

		// Token: 0x06000371 RID: 881 RVA: 0x0001221F File Offset: 0x0001041F
		public void StartLockOn(ShMountable e, int seatIndex)
		{
			base.StartCoroutine(this.LockOn(e, seatIndex));
		}

		// Token: 0x06000372 RID: 882 RVA: 0x00012230 File Offset: 0x00010430
		public void SendServerInfo()
		{
			SvPlayer.events.ServerInfo(this.player);
		}

		// Token: 0x06000373 RID: 883 RVA: 0x00012243 File Offset: 0x00010443
		public void RemoveItemsDeath(bool dropItems)
		{
			SvPlayer.events.RemoveItemsDeath(this.player, dropItems);
		}

		// Token: 0x06000374 RID: 884 RVA: 0x00012257 File Offset: 0x00010457
		public void DisplayName(string username)
		{
			SvPlayer.events.DisplayName(this.player, username);
		}

		// Token: 0x06000375 RID: 885 RVA: 0x0001226B File Offset: 0x0001046B
		public void SvUpdateDisplayName(string displayName)
		{
			this.player.displayName = displayName;
			base.Send(SvSendType.All, PacketFlags.Reliable, ClPacket.DisplayName, new object[]
			{
				this.player.ID,
				displayName
			});
		}

		// Token: 0x06000376 RID: 886 RVA: 0x000122A4 File Offset: 0x000104A4
		public void ApplyWearableIndices(WearableOptions[] wearableOptions)
		{
			int num = 0;
			foreach (int value in this.wearableIndices)
			{
				string[] wearableNames = wearableOptions[num].wearableNames;
				int num2 = Math.Clamp(value, 0, wearableNames.Length - 1);
				string s = wearableNames[num2];
				this.AddSetWearable(s.GetPrefabIndex());
				num++;
			}
		}

		// Token: 0x06000377 RID: 887 RVA: 0x000122FC File Offset: 0x000104FC
		public override void Initialize()
		{
			this.progressHandler.Add(new ProgressAction(this.player.Hands.name, new Func<int, bool>(this.BombTest), new Action<int>(this.BombAction)));
			this.progressHandler.Add(new ProgressAction(this.player.Hands.name, new Func<int, bool>(this.RepairTest), new Action<int>(this.RepairAction)));
			this.progressHandler.Add(new ProgressAction(this.player.Hands.name, new Func<int, bool>(this.LockpickTest), new Action<int>(this.LockpickAction)));
			if (this.player.isHuman)
			{
				this.svManager.connectedPlayers.Add(this.connection.ID, this.player);
				this.DisplayName(this.player.username);
				Util.Log(this.player.username + " connected", LogLevel.Log);
			}
			else
			{
				this.DisplayName(this.svManager.names.GetRandom<string>());
				this.seeker = this.player.go.AddComponent<Seeker>();
				this.seeker.startEndModifier.exactStartPoint = (this.seeker.startEndModifier.exactEndPoint = StartEndModifier.Exactness.ClosestOnNode);
				this.player.go.AddComponent<FunnelModifier>();
				int count = BPAPI.States.Count;
				this.states = new State[count];
				for (int i = 0; i < count; i++)
				{
					State state = (State)Activator.CreateInstance(BPAPI.States[i].GetType());
					state.Initialize(this.player, i);
					this.states[i] = state;
				}
				this.currentState = this.states[0];
				this.buyerType = this.svManager.buyerTypes.GetRandom<BuyerType>();
			}
			if (this.player.isHuman)
			{
				this.SyncPermissions();
				if (this.wearableIndices != null)
				{
					this.ApplyWearableIndices(this.player.wearableOptions);
				}
			}
			else
			{
				foreach (WearableOptions wearableOptions2 in this.player.wearableOptions)
				{
					int num = Random.Range(0, wearableOptions2.wearableNames.Length);
					this.AddSetWearable(wearableOptions2.wearableNames[num].GetPrefabIndex());
				}
			}
			this.SvResetJob();
			this.SvSetChatChannel(0);
			this.SvSetChatMode(ChatMode.Public);
			base.Initialize();
			if (this.player.isHuman)
			{
				this.svManager.StartCoroutine(this.SendEntityData());
			}
		}

		// Token: 0x06000378 RID: 888 RVA: 0x000125A4 File Offset: 0x000107A4
		public void SyncPermissions()
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.Permissions, new object[]
			{
				Util.permArray.Select(new Func<PermEnum, byte>(this.HasPermissionByte)).ToArray<byte>()
			});
		}

		// Token: 0x06000379 RID: 889 RVA: 0x000125E0 File Offset: 0x000107E0
		public override void Destroy()
		{
			if (this.player.SpecSelf)
			{
				this.ClearSpectators();
			}
			else
			{
				this.SvSpectate(this.player);
			}
			this.svManager.votedYes.Remove(this.player);
			this.svManager.votedNo.Remove(this.player);
			if (this.player.Place.IsClone)
			{
				this.SvEnterDoor(this.player.Place.mainDoor.ID, this.player, true);
			}
			if (this.player.isHuman)
			{
				Util.Log(this.player.username + " disconnected", LogLevel.Log);
				try
				{
					this.Save();
				}
				catch (Exception ex)
				{
					string str = "Error on Save: ";
					Exception ex2 = ex;
					Util.Log(str + ((ex2 != null) ? ex2.ToString() : null), LogLevel.Error);
				}
				foreach (Place apartmentPlace in this.player.ownedApartments.Values.Reverse<Place>().ToArray<Place>())
				{
					this.SellApartment(apartmentPlace);
				}
				this.svManager.connectedPlayers.Remove(this.connection.ID);
			}
			foreach (ShEntity shEntity in this.spawnedEntities.ToArray<ShEntity>())
			{
				if (shEntity)
				{
					shEntity.Destroy();
				}
			}
			this.spawnedEntities.Clear();
			this.job.RemoveJob();
			foreach (ShTransport shTransport in this.player.ownedTransports)
			{
				shTransport.svTransport.SvSetTransportOwner(null);
			}
			this.player.ownedTransports.Clear();
			foreach (ShEntity shEntity2 in this.subscribedEntities)
			{
				shEntity2.svEntity.RemoveSubscribedPlayer(this.player, false);
			}
			this.subscribedEntities.Clear();
			base.Destroy();
		}

		// Token: 0x0600037A RID: 890 RVA: 0x00012824 File Offset: 0x00010A24
		protected IEnumerator LockOn(ShMountable source, int seatIndex)
		{
			ShThrown thrown;
			if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShThrown>(source.GetWeaponSet(seatIndex).thrownName, out thrown))
			{
				bool equipable = source == this.player.curEquipable;
				WeaponSet originalWeaponSet = this.player.curEquipable.GetWeaponSet(seatIndex);
				WaitForSeconds delay = new WaitForSeconds(0.2f);
				yield return delay;
				while (source && source.isActiveAndEnabled && !source.IsDead && source.controller == this.player)
				{
					if (equipable)
					{
						if (this.player.curMount)
						{
							break;
						}
					}
					else if (originalWeaponSet != source.GetWeaponSet(seatIndex))
					{
						break;
					}
					float num3;
					if (this.player.lockOnTarget == null)
					{
						if (this.player.IsUp && this.sector != null)
						{
							ShEntity shEntity = null;
							float num = float.MaxValue;
							foreach (ShEntity shEntity2 in this.sector.controlled)
							{
								float num2;
								if (shEntity2 != this.entity && shEntity2 != this.player.GetMount() && shEntity2.GetComponent(thrown.targetType) && source.svMountable.CanLockOnTarget(shEntity2, thrown, seatIndex, out num2) && num2 < num)
								{
									num = num2;
									shEntity = shEntity2;
								}
							}
							if (shEntity)
							{
								this.SvLockOn(shEntity);
							}
						}
					}
					else if (!this.player.IsUp || !this.player.lockOnTarget.isActiveAndEnabled || !source.svMountable.CanLockOnTarget(this.player.lockOnTarget, thrown, seatIndex, out num3))
					{
						this.SvLockOn(null);
					}
					yield return delay;
				}
				if (this.player.lockOnTarget)
				{
					this.SvLockOn(null);
				}
				originalWeaponSet = null;
				delay = null;
			}
			yield break;
		}

		// Token: 0x0600037B RID: 891 RVA: 0x00012844 File Offset: 0x00010A44
		public void SvLockOn(ShEntity target)
		{
			if (target)
			{
				this.player.lockOnTime = Time.time;
				this.player.lockOnTarget = target;
				if (this.player.isHuman)
				{
					base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.LockOn, new object[]
					{
						target.ID
					});
					return;
				}
			}
			else
			{
				this.player.lockOnTarget = null;
				if (this.player.isHuman)
				{
					base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.LockOn, new object[]
					{
						0
					});
				}
			}
		}

		// Token: 0x0600037C RID: 892 RVA: 0x000128DC File Offset: 0x00010ADC
		public void ClearSpectators()
		{
			foreach (ShPlayer shPlayer in this.spectators.ToArray<ShPlayer>())
			{
				shPlayer.svPlayer.SvSpectate(shPlayer);
			}
			this.spectators.Clear();
		}

		// Token: 0x0600037D RID: 893 RVA: 0x00012920 File Offset: 0x00010B20
		public override void WriteActivateData()
		{
			if (this.activateArgs == null)
			{
				this.activateArgs = new object[17];
			}
			this.activateArgs[10] = this.player.curEquipable.index;
			this.activateArgs[11] = this.SerializeAttachments();
			this.activateArgs[12] = this.SerializeWearables();
			this.activateArgs[13] = this.player.pointing;
			this.activateArgs[14] = this.player.mode;
			if (this.player.curMount)
			{
				this.activateArgs[15] = this.player.curMount.ID;
				this.activateArgs[16] = this.player.seat;
			}
			else
			{
				this.activateArgs[15] = 0;
				this.activateArgs[16] = (byte)this.player.StanceIndex;
			}
			base.WriteActivateData();
		}

		// Token: 0x0600037E RID: 894 RVA: 0x00012A2C File Offset: 0x00010C2C
		public bool SvTrySetEquipable(int equipableIndex, bool stopQuickswitch = false)
		{
			if (!this.player.IsMobile)
			{
				return false;
			}
			if (stopQuickswitch && this.prevEquipables.Contains(equipableIndex))
			{
				return false;
			}
			if (!this.player.switching && this.player.curEquipable)
			{
				this.prevEquipables.Enqueue(this.player.curEquipable.index);
			}
			this.prevEquipables.Enqueue(equipableIndex);
			ShEquipable shEquipable;
			if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShEquipable>(equipableIndex, out shEquipable) && (!shEquipable.needItem || this.player.HasItem(equipableIndex)))
			{
				this.SvSetEquipable(shEquipable);
			}
			return true;
		}

		// Token: 0x0600037F RID: 895 RVA: 0x00012AD1 File Offset: 0x00010CD1
		public void SvForceEquipable(int equipableIndex)
		{
			if (this.player.SetEquipable(equipableIndex, true))
			{
				base.Send(SvSendType.Local, PacketFlags.Reliable, ClPacket.SetEquipable, new object[]
				{
					this.player.ID,
					equipableIndex
				});
			}
		}

		// Token: 0x06000380 RID: 896 RVA: 0x00012B0E File Offset: 0x00010D0E
		public void SvSetEquipable(ShEquipable equipable)
		{
			SvPlayer.events.SetEquipable(this.player, equipable);
		}

		// Token: 0x06000381 RID: 897 RVA: 0x00012B24 File Offset: 0x00010D24
		public void CheckServerEquipment(int itemIndex)
		{
			ShEntity entity = MonoBehaviourSingleton<SceneManager>.Instance.GetEntity(itemIndex);
			ShWearable shWearable = entity as ShWearable;
			if (shWearable != null)
			{
				int type = (int)shWearable.type;
				if (this.player.curWearables[type].index == itemIndex && shWearable.needItem && !this.player.HasItem(itemIndex))
				{
					this.SvSetWearable(MonoBehaviourSingleton<ShManager>.Instance.nullWearable[type].index);
				}
			}
			else
			{
				ShAttachment shAttachment = entity as ShAttachment;
				if (shAttachment != null)
				{
					ShEquipable curEquipable = this.player.curEquipable;
					if (shAttachment.needItem && !this.player.HasItem(itemIndex) && (curEquipable.curMuzzle.index == itemIndex || curEquipable.curSight.index == itemIndex || curEquipable.curUnderbarrel.index == itemIndex))
					{
						this.SvSetAttachment(MonoBehaviourSingleton<ShManager>.Instance.nullAttachment[shAttachment.AttachmentType].index);
					}
				}
				else if (this.player.curEquipable.index == itemIndex && this.player.curEquipable.needItem && !this.player.HasItem(itemIndex))
				{
					this.SvSetEquipable(this.player.Hands);
				}
			}
			byte b = 0;
			while ((int)b < this.player.bindings.Length)
			{
				if (this.player.bindings[(int)b].equipable == itemIndex && !this.player.HasItem(itemIndex))
				{
					this.player.Unbind(b);
				}
				this.SvUnbindAttachment(itemIndex, b);
				b += 1;
			}
		}

		// Token: 0x06000382 RID: 898 RVA: 0x00012CBC File Offset: 0x00010EBC
		public void SvBindAttachment(int attachmentIndex, byte slot)
		{
			ShAttachment shAttachment;
			if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShAttachment>(attachmentIndex, out shAttachment) && (!shAttachment.needItem || this.player.HasItem(attachmentIndex)) && this.player.BindAttachment(attachmentIndex, slot))
			{
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.BindAttachment, new object[]
				{
					attachmentIndex,
					slot
				});
			}
		}

		// Token: 0x06000383 RID: 899 RVA: 0x00012D1F File Offset: 0x00010F1F
		public void SvUnbindAttachment(int attachmentIndex, byte slot)
		{
			if (this.player.UnbindAttachment(attachmentIndex, slot))
			{
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.UnbindAttachment, new object[]
				{
					attachmentIndex,
					slot
				});
			}
		}

		// Token: 0x06000384 RID: 900 RVA: 0x00012D54 File Offset: 0x00010F54
		public void SvSetAttachment(int attachmentIndex)
		{
			ShAttachment shAttachment;
			if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShAttachment>(attachmentIndex, out shAttachment) && (!shAttachment.needItem || this.player.HasItem(attachmentIndex)) && this.player.SetAttachment(shAttachment, UnderbarrelSetting.Default))
			{
				base.Send(SvSendType.Local, PacketFlags.Reliable, ClPacket.SetAttachment, new object[]
				{
					this.player.ID,
					attachmentIndex
				});
			}
		}

		// Token: 0x06000385 RID: 901 RVA: 0x00012DC4 File Offset: 0x00010FC4
		public void SvSetWearable(int wearableIndex)
		{
			ShWearable shWearable;
			if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShWearable>(wearableIndex, out shWearable) && (!shWearable.needItem || this.player.HasItem(wearableIndex)) && this.player.SetWearable(wearableIndex))
			{
				base.Send(SvSendType.Local, PacketFlags.Reliable, ClPacket.SetWearable, new object[]
				{
					this.player.ID,
					wearableIndex
				});
			}
		}

		// Token: 0x06000386 RID: 902 RVA: 0x00012E30 File Offset: 0x00011030
		public void SvUseBind(byte bindIndex)
		{
			if ((int)bindIndex < this.player.bindings.Length && this.SvTrySetEquipable(this.player.bindings[(int)bindIndex].equipable, true))
			{
				foreach (int attachmentIndex in this.player.bindings[(int)bindIndex].attachments)
				{
					this.SvSetAttachment(attachmentIndex);
				}
			}
		}

		// Token: 0x06000387 RID: 903 RVA: 0x00012E9C File Offset: 0x0001109C
		public void SvDeploy()
		{
			ShTransport deployable = this.player.GetDeployable();
			if (deployable)
			{
				ShTransport mount = this.svManager.AddNewEntity<ShTransport>(deployable, this.player.Place, this.player.Position, this.player.Rotation, false, this.player, false);
				this.SvMount(mount, 0);
			}
		}

		// Token: 0x06000388 RID: 904 RVA: 0x00012EFC File Offset: 0x000110FC
		public void SvServiceCall(int entityID, int jobIndex)
		{
			if (!this.CanUseApp(entityID, AppIndex.Services))
			{
				return;
			}
			if (this.selfMarker)
			{
				this.SendGameMessage("Please wait");
				return;
			}
			if (jobIndex >= 0 && jobIndex < BPAPI.Jobs.Count)
			{
				JobInfo jobInfo = BPAPI.Jobs[jobIndex];
				string message = this.player.username + " requesting " + jobInfo.shared.jobName + " (Marked Red)";
				IDCollection<ShPlayer> idcollection = new IDCollection<ShPlayer>();
				foreach (ShPlayer shPlayer in jobInfo.members)
				{
					if (shPlayer != this.player && shPlayer.isHuman)
					{
						idcollection.Add(shPlayer);
						shPlayer.svPlayer.SendGameMessage(message);
					}
				}
				if (idcollection.Count > 0)
				{
					this.StartSelfMarker(idcollection);
					this.SendGameMessage("Call sent");
					return;
				}
				this.SendGameMessage("No responders available");
			}
		}

		// Token: 0x06000389 RID: 905 RVA: 0x0001300C File Offset: 0x0001120C
		public override void Restock(float fraction = 1f)
		{
			base.Restock(fraction);
			this.AddJobItems(this.job.info, this.player.rank, false);
			if (this.defaultItems != null)
			{
				foreach (InventoryItem inventoryItem in this.defaultItems.Values)
				{
					ShWearable shWearable = inventoryItem.item as ShWearable;
					if (shWearable != null)
					{
						int type = (int)shWearable.type;
						if (this.player.curWearables[type].index == MonoBehaviourSingleton<ShManager>.Instance.nullWearable[type].index && this.player.CanWear(shWearable))
						{
							this.SvSetWearable(shWearable.index);
						}
					}
				}
			}
		}

		// Token: 0x0600038A RID: 906 RVA: 0x000130E0 File Offset: 0x000112E0
		public void SvTransferView(byte deltaType, int itemIndex, int amount)
		{
			if (this.player.otherEntity && (deltaType == 5 || deltaType == 6))
			{
				this.player.TransferItem(deltaType, itemIndex, amount, true);
				return;
			}
			Util.Log("Invalid DeltaInv in TransferView", LogLevel.Log);
		}

		// Token: 0x0600038B RID: 907 RVA: 0x00013118 File Offset: 0x00011318
		public void AddSetWearable(int wearableIndex)
		{
			ShWearable shWearable;
			if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShWearable>(wearableIndex, out shWearable))
			{
				if (shWearable.needItem && !this.player.HasItem(wearableIndex))
				{
					this.player.TransferItem(1, shWearable, 1, true);
				}
				this.SvSetWearable(shWearable.index);
			}
		}

		// Token: 0x0600038C RID: 908 RVA: 0x00013165 File Offset: 0x00011365
		public void CollectItem(ShItem collectedItem, int count)
		{
			if (collectedItem.needItem)
			{
				this.player.TransferItem(1, collectedItem, count, true);
			}
		}

		// Token: 0x0600038D RID: 909 RVA: 0x00013180 File Offset: 0x00011380
		public void SvCollect(int collectedID, bool consume)
		{
			ShEntity shEntity = EntityCollections.FindByID(collectedID);
			if (!this.player.CanCollectEntity(shEntity) || !this.player.InActionRange(shEntity))
			{
				return;
			}
			if (shEntity.HasInventory && shEntity.myItems.Count > 0)
			{
				this.SendGameMessage("Must be empty to collect");
				return;
			}
			SvPlayer.events.Collect(this.player, shEntity, consume);
		}

		// Token: 0x0600038E RID: 910 RVA: 0x000131E6 File Offset: 0x000113E6
		protected void FinishTradeComplete(bool isGood, ShPlayer otherPlayer)
		{
			this.player.FinishTrade(isGood);
			otherPlayer.FinishTrade(isGood);
			this.FinishTradeServer(isGood);
			otherPlayer.svPlayer.FinishTradeServer(isGood);
		}

		// Token: 0x0600038F RID: 911 RVA: 0x0001320E File Offset: 0x0001140E
		protected void FinishTradeServer(bool isGood)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.FinishTrade, new object[]
			{
				isGood
			});
			this.tradeConfirmed = false;
			this.player.otherEntity = null;
			this.player.tradeItems.Clear();
		}

		// Token: 0x06000390 RID: 912 RVA: 0x0001324C File Offset: 0x0001144C
		public void SvStopInventory(bool sendToSelf)
		{
			SvPlayer.events.StopInventory(this.player, sendToSelf);
			ShPlayer shPlayer = this.player.otherEntity as ShPlayer;
			if (shPlayer != null)
			{
				if (shPlayer.otherEntity == this.player)
				{
					this.FinishTradeComplete(false, shPlayer);
				}
				else
				{
					shPlayer.RemoveViewer(this.player);
					if (shPlayer.isHuman && shPlayer.viewers.Count == 0)
					{
						shPlayer.svPlayer.DestroyMenu("Default");
					}
				}
				if (!shPlayer.isHuman && shPlayer.viewers.Count == 0 && !shPlayer.svPlayer.targetEntity)
				{
					shPlayer.svPlayer.ResetAI();
				}
			}
			else if (this.player.otherEntity)
			{
				this.player.otherEntity.RemoveViewer(this.player);
			}
			this.player.ClearAllViewers();
			this.player.otherEntity = null;
			if (sendToSelf && this.player.isHuman)
			{
				this.DestroyMenu("Default");
			}
		}

		// Token: 0x06000391 RID: 913 RVA: 0x0001335E File Offset: 0x0001155E
		public void SvMinigameStop(bool sendToSelf)
		{
			this.minigame = null;
			if (sendToSelf)
			{
				this.DestroyMenu("Default");
			}
		}

		// Token: 0x06000392 RID: 914 RVA: 0x00013378 File Offset: 0x00011578
		public void SvFinalizeTrade()
		{
			if (!this.player.lockedTrade)
			{
				ShPlayer shPlayer = this.player.otherEntity as ShPlayer;
				if (shPlayer != null && !shPlayer.lockedTrade)
				{
					if (shPlayer.isHuman)
					{
						this.player.lockedTrade = true;
						base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.WaitTrade, Array.Empty<object>());
						shPlayer.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.OtherFinalized, Array.Empty<object>());
						return;
					}
					this.FinishTradeComplete(true, shPlayer);
					shPlayer.svPlayer.ResetAI();
					return;
				}
			}
		}

		// Token: 0x06000393 RID: 915 RVA: 0x000133FC File Offset: 0x000115FC
		public void SvConfirmTrade()
		{
			ShPlayer shPlayer = this.player.otherEntity as ShPlayer;
			if (shPlayer == null)
			{
				return;
			}
			if (shPlayer.svPlayer.tradeConfirmed)
			{
				this.FinishTradeComplete(true, shPlayer);
				return;
			}
			this.tradeConfirmed = true;
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.WaitTrade, Array.Empty<object>());
			shPlayer.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.OtherConfirmed, Array.Empty<object>());
		}

		// Token: 0x06000394 RID: 916 RVA: 0x00013460 File Offset: 0x00011660
		protected void PerformHit(ShHitscan currentHitscan)
		{
			RaycastHit raycastHit;
			currentHitscan.HitCheck(out raycastHit);
		}

		// Token: 0x06000395 RID: 917 RVA: 0x00013478 File Offset: 0x00011678
		public void SvCheckHit(int hitID, byte burst, byte fireIndex)
		{
			ShHitscan shHitscan = this.player.curEquipable as ShHitscan;
			if (shHitscan == null)
			{
				return;
			}
			shHitscan.checkHitscan = true;
			ShDamageable shDamageable;
			if (hitID < 0)
			{
				NetState netState = new NetState(this.player, 0, 0, default(Vector2));
				NetState netState2;
				if (shHitscan.GetHitscanState(fireIndex, burst, out netState2))
				{
					this.SetNetState(netState2);
					this.PerformHit(shHitscan);
					this.SetNetState(netState);
				}
				else
				{
					this.PerformHit(shHitscan);
				}
			}
			else if (EntityCollections.TryFindByID<ShDamageable>(hitID, out shDamageable) && shDamageable.isActiveAndEnabled && hitID != this.player.ID)
			{
				ShPlayer shPlayer = shDamageable.Player;
				ShMountable mount = shDamageable.GetMount();
				ShPhysical shPhysical = mount as ShPhysical;
				if (shPhysical != null)
				{
					shDamageable = shPhysical;
				}
				NetState netState3;
				if (shHitscan.GetHitscanState(fireIndex, burst, out netState3) && this.player.GetMount() != mount)
				{
					NetState netState4 = new NetState(this.player, 0, 0, default(Vector2));
					this.SetNetState(netState3);
					float rollbackTime = this.connection.RTT() + 0.1f;
					float time = MonoBehaviourSingleton<SceneManager>.Instance.time;
					ShPhysical shPhysical2 = shDamageable as ShPhysical;
					NetState netState5;
					if (shPhysical2 != null && shPhysical2.svPhysical.PreviousNetState(rollbackTime, out netState5))
					{
						NetState netState6 = new NetState(shPhysical2, 0, 0, default(Vector2));
						shPhysical2.svPhysical.SetNetState(netState5);
						this.PerformHit(shHitscan);
						shPhysical2.svPhysical.SetNetState(netState6);
					}
					else
					{
						this.PerformHit(shHitscan);
					}
					this.SetNetState(netState4);
				}
				else
				{
					this.PerformHit(shHitscan);
				}
			}
			shHitscan.checkHitscan = false;
		}

		// Token: 0x06000396 RID: 918 RVA: 0x00013618 File Offset: 0x00011818
		public override void SetNetState(NetState s)
		{
			base.SetNetState(s);
			if (this.player.stance.setable && this.player.stances[(int)s.stance].setable)
			{
				this.player.SetStanceColliders(s.stance);
			}
		}

		// Token: 0x06000397 RID: 919 RVA: 0x00013668 File Offset: 0x00011868
		protected bool RepairTest(int transportID)
		{
			ShTransport shTransport;
			if (EntityCollections.TryFindByID<ShTransport>(transportID, out shTransport) && shTransport.CanHeal && this.player.HasItem(MonoBehaviourSingleton<ShManager>.Instance.toolkit))
			{
				return true;
			}
			this.SendGameMessage("Cannot repair for some reason");
			return false;
		}

		// Token: 0x06000398 RID: 920 RVA: 0x000136AC File Offset: 0x000118AC
		protected void RepairAction(int transportID)
		{
			ShTransport transport;
			if (EntityCollections.TryFindByID<ShTransport>(transportID, out transport))
			{
				SvPlayer.events.Repair(this.player, transport);
			}
		}

		// Token: 0x06000399 RID: 921 RVA: 0x000136D5 File Offset: 0x000118D5
		public IEnumerator KnockedDown()
		{
			this.SvForceStance(StanceIndex.KnockedDown);
			float endTime = Time.time + 1f;
			while (Time.time < endTime || !this.player.GetGround())
			{
				yield return null;
				if (this.player.stance.index != StanceIndex.KnockedDown)
				{
					yield break;
				}
			}
			this.StartRecover();
			yield break;
		}

		// Token: 0x0600039A RID: 922 RVA: 0x000136E4 File Offset: 0x000118E4
		public void StartRecover()
		{
			base.StartCoroutine(this.Recover());
		}

		// Token: 0x0600039B RID: 923 RVA: 0x000136F3 File Offset: 0x000118F3
		public IEnumerator Recover()
		{
			this.SvForceStance(StanceIndex.Recovering);
			float endTime = Time.time + 2f;
			while (Time.time < endTime)
			{
				yield return null;
				if (this.player.stance.index != StanceIndex.Recovering)
				{
					yield break;
				}
			}
			this.SvForceStance(StanceIndex.Stand);
			if (!this.player.isHuman && this.currentState.index == 0)
			{
				this.ResetAI();
			}
			yield break;
		}

		// Token: 0x0600039C RID: 924 RVA: 0x00013702 File Offset: 0x00011902
		public void SvAddInjury(BodyPart part, BodyEffect effect, byte amount)
		{
			SvPlayer.events.Injury(this.player, part, effect, amount);
		}

		// Token: 0x0600039D RID: 925 RVA: 0x00013718 File Offset: 0x00011918
		public void SvReload()
		{
			if (this.player.curEquipable.CanReload)
			{
				this.player.curEquipable.Reload();
			}
		}

		// Token: 0x0600039E RID: 926 RVA: 0x0001373C File Offset: 0x0001193C
		public void SvAltFire(bool force = false)
		{
			if (force || this.player.CanAltFire())
			{
				this.player.curEquipable.curUnderbarrel.ToggleSetting();
				base.Send(SvSendType.Local, PacketFlags.Reliable, ClPacket.AltFire, new object[]
				{
					this.player.ID
				});
			}
		}

		// Token: 0x0600039F RID: 927 RVA: 0x00013791 File Offset: 0x00011991
		public void SvUpdatePlayer(Vector3 position, Quaternion rotation)
		{
			if (!this.player.curMount && !this.player.OutsideController)
			{
				base.SvTryUpdateSmooth(position, rotation);
			}
		}

		// Token: 0x060003A0 RID: 928 RVA: 0x000137BA File Offset: 0x000119BA
		public void SvUpdatePlayerWater(Vector3 position, Quaternion rotation)
		{
			this.SvUpdatePlayer(position + new Vector3(0f, MonoBehaviourSingleton<SceneManager>.Instance.WaterLevel(position), 0f), rotation);
		}

		// Token: 0x060003A1 RID: 929 RVA: 0x000137E4 File Offset: 0x000119E4
		public void SvUpdatePlayerOffset(int groundID, Vector3 position, Quaternion rotation)
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(groundID, out shEntity))
			{
				this.SvUpdatePlayer(shEntity.Position + position, shEntity.Rotation * rotation);
			}
		}

		// Token: 0x060003A2 RID: 930 RVA: 0x0001381C File Offset: 0x00011A1C
		public void SvUpdateMount(Vector3 position, Quaternion rotation)
		{
			ShPhysical shPhysical;
			if (this.player.IsControlledMount<ShPhysical>(out shPhysical) && !shPhysical.ServersidePhysics)
			{
				shPhysical.svPhysical.SvTryUpdateSmooth(position, rotation);
			}
		}

		// Token: 0x060003A3 RID: 931 RVA: 0x0001384D File Offset: 0x00011A4D
		public void SvUpdateMountWater(Vector3 position, Quaternion rotation)
		{
			this.SvUpdateMount(position + new Vector3(0f, MonoBehaviourSingleton<SceneManager>.Instance.WaterLevel(position), 0f), rotation);
		}

		// Token: 0x060003A4 RID: 932 RVA: 0x00013878 File Offset: 0x00011A78
		public void SvUpdateMountOffset(int groundID, Vector3 position, Quaternion rotation)
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(groundID, out shEntity))
			{
				this.SvUpdateMount(shEntity.Position + position, shEntity.Rotation * rotation);
			}
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x000138B0 File Offset: 0x00011AB0
		public void SvJump()
		{
			if (this.player.CanJump && Time.frameCount != this.jumpFrame)
			{
				this.jumpFrame = Time.frameCount;
				this.player.Jump();
				base.Send(SvSendType.LocalOthers, PacketFlags.Reliable, ClPacket.Jump, new object[]
				{
					this.player.ID
				});
			}
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x00013910 File Offset: 0x00011B10
		public void SvCrouch(bool crouch)
		{
			if (crouch)
			{
				if (this.player.CanCrouch)
				{
					this.SvSetStance(StanceIndex.Crouch);
					return;
				}
			}
			else
			{
				this.SvSetStance(StanceIndex.Stand);
			}
		}

		// Token: 0x060003A7 RID: 935 RVA: 0x00013934 File Offset: 0x00011B34
		protected void SvSetStance(StanceIndex newStance)
		{
			if (this.player.stance.setable && this.player.StanceIndex != newStance)
			{
				this.player.SetStance(newStance);
				base.Send(SvSendType.LocalOthers, PacketFlags.Reliable, ClPacket.Stance, new object[]
				{
					this.player.ID,
					(byte)newStance
				});
			}
		}

		// Token: 0x060003A8 RID: 936 RVA: 0x0001399A File Offset: 0x00011B9A
		public void SvForceStance(StanceIndex stance)
		{
			this.player.SetStance(stance);
			base.Send(SvSendType.Local, PacketFlags.Reliable, ClPacket.Stance, new object[]
			{
				this.player.ID,
				(byte)stance
			});
		}

		// Token: 0x060003A9 RID: 937 RVA: 0x000139D8 File Offset: 0x00011BD8
		public void SvUpdateInput(Vector3 input, byte mode)
		{
			if (this.player.CanUpdateInputs)
			{
				this.player.input = new Vector3(Mathf.Clamp(input.x, -1f, 1f), Mathf.Clamp(input.y, -1f, 1f), Mathf.Clamp(input.z, -1f, 1f));
				this.SvUpdateMode(mode);
			}
		}

		// Token: 0x060003AA RID: 938 RVA: 0x00013A48 File Offset: 0x00011C48
		public void SvUpdateMode(byte newMode)
		{
			if (this.player.TryUpdateMode(newMode))
			{
				base.Send(SvSendType.LocalOthers, PacketFlags.Reliable, ClPacket.UpdateMode, new object[]
				{
					this.player.ID,
					newMode
				});
			}
		}

		// Token: 0x060003AB RID: 939 RVA: 0x00013A84 File Offset: 0x00011C84
		public void SvPoint(bool pointing)
		{
			SvPlayer.events.Point(this.player, pointing);
		}

		// Token: 0x060003AC RID: 940 RVA: 0x00013A98 File Offset: 0x00011C98
		public void SvAlert()
		{
			base.Send(SvSendType.LocalOthers, PacketFlags.Reliable, ClPacket.Alert, new object[]
			{
				this.player.ID
			});
			SvPlayer.events.Alert(this.player);
		}

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x060003AD RID: 941 RVA: 0x00013AD0 File Offset: 0x00011CD0
		public bool ServerPlacement
		{
			get
			{
				return !this.svManager.placedItem.Limit(this.player, true) && (!this.player.InOwnApartment || this.player.PlaceItemCount < this.player.GetPlaceItemLimit());
			}
		}

		// Token: 0x060003AE RID: 942 RVA: 0x00013B20 File Offset: 0x00011D20
		public void SvDropItem(int itemIndex)
		{
			ShItem shItem;
			if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShItem>(itemIndex, out shItem) && shItem.needItem && this.player.HasItem(shItem.index) && this.ServerPlacement && this.DropEntity(shItem) && this.placementValid)
			{
				this.player.TransferItem(2, shItem, 1, true);
				return;
			}
			this.SendGameMessage("Unable to drop item");
		}

		// Token: 0x060003AF RID: 943 RVA: 0x00013B8C File Offset: 0x00011D8C
		public bool DropEntity(ShEntity e)
		{
			Vector3 position = this.player.originT.position;
			Vector3 forward = this.player.originT.forward;
			RaycastHit raycastHit;
			if (Physics.Raycast(position, forward, out raycastHit, 8f, 1) || Physics.Raycast(position, Vector3.down, out raycastHit, 8f, 1))
			{
				Bounds localBounds = e.GetLocalBounds(true);
				Vector3 extents = localBounds.extents;
				char c;
				if (extents.x < extents.y && extents.x < extents.z)
				{
					c = 'x';
				}
				else if (extents.y < extents.z)
				{
					c = 'y';
				}
				else
				{
					c = 'z';
				}
				Vector3 center = localBounds.center;
				Vector3 zero;
				float d;
				switch (c)
				{
				case 'x':
					zero = new Vector3(0f, -90f, -90f);
					d = extents.x + center.x;
					break;
				case 'y':
					zero = new Vector3(0f, 0f, 0f);
					d = extents.y - center.y;
					break;
				case 'z':
					zero = new Vector3(-90f, 0f, 180f);
					d = extents.z - center.z;
					break;
				default:
					zero = Vector3.zero;
					d = 0f;
					break;
				}
				Vector3 position2 = raycastHit.point + raycastHit.normal * d;
				Quaternion rotation = Vector3.ProjectOnPlane(forward, raycastHit.normal).SafeLookRotation(raycastHit.normal) * Quaternion.Euler(zero);
				SvPlayer.events.PlaceItem(this.player, e, position2, rotation, 0f);
				return this.placementValid;
			}
			return false;
		}

		// Token: 0x060003B0 RID: 944 RVA: 0x00013D48 File Offset: 0x00011F48
		public bool SvTryMount(int mountableID, bool checkRestrained)
		{
			ShMountable shMountable;
			byte seat;
			if (EntityCollections.TryFindByID<ShMountable>(mountableID, out shMountable) && this.player.CanMount(shMountable, true, checkRestrained, out seat) && this.player.InActionRange(shMountable))
			{
				this.SvMount(shMountable, seat);
				return true;
			}
			return false;
		}

		// Token: 0x060003B1 RID: 945 RVA: 0x00013D8A File Offset: 0x00011F8A
		public void SvMount(ShMountable mount, byte seat)
		{
			if (mount != this.player && mount.isActiveAndEnabled)
			{
				SvPlayer.events.Mount(this.player, mount, seat);
			}
		}

		// Token: 0x060003B2 RID: 946 RVA: 0x00013DB5 File Offset: 0x00011FB5
		public bool SvTryDismount()
		{
			if (this.player.curMount && !this.player.IsRestrained)
			{
				this.SvDismount(false);
				return true;
			}
			return false;
		}

		// Token: 0x060003B3 RID: 947 RVA: 0x00013DE0 File Offset: 0x00011FE0
		public void SvDismount(bool resetAI = false)
		{
			if (this.player.curMount)
			{
				SvPlayer.events.Dismount(this.player);
			}
			if (!this.player.isHuman && resetAI)
			{
				this.ResetAI();
			}
		}

		// Token: 0x060003B4 RID: 948 RVA: 0x00013E1D File Offset: 0x0001201D
		public override void SvRelocate(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			if (this.player.curMount)
			{
				this.player.Dismount();
			}
			base.SvRelocate(position, rotation, parent);
		}

		// Token: 0x060003B5 RID: 949 RVA: 0x00013E48 File Offset: 0x00012048
		public void SvMoveSeat(int delta)
		{
			ShMountable curMount = this.player.curMount;
			byte b;
			if (curMount && this.player.CanMount(curMount, true, true, out b))
			{
				int b2 = curMount.seats.Length;
				byte seat = this.player.seat;
				for (int num = ((int)seat + delta).Mod(b2); num != (int)seat; num = (num + delta).Mod(b2))
				{
					if (!curMount.occupants[num])
					{
						this.SvMount(curMount, (byte)num);
						return;
					}
				}
			}
		}

		// Token: 0x060003B6 RID: 950 RVA: 0x00013ECC File Offset: 0x000120CC
		public void SvToggleWeapon()
		{
			if (this.player.IsMountArmed)
			{
				Seat seat = this.player.curMount.seats[(int)this.player.seat];
				int num = (seat.weaponIndex + 1) % seat.weaponSets.Length;
				this.SetMountWeapon(num);
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.ToggleWeapon, new object[]
				{
					num,
					this.player.curMount.GetCurrentClip((int)this.player.seat)
				});
			}
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x00013F5C File Offset: 0x0001215C
		public void SetMountWeapon(int weaponIndex)
		{
			this.player.curMount.seats[(int)this.player.seat].weaponIndex = weaponIndex;
			if (this.player.curMount.CanLockOn((int)this.player.seat))
			{
				this.StartLockOn(this.player.curMount, (int)this.player.seat);
			}
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x00013FC4 File Offset: 0x000121C4
		protected bool LockpickTest(int transportID)
		{
			ShTransport mount;
			byte b;
			if (EntityCollections.TryFindByID<ShTransport>(transportID, out mount) && this.player.CanMount(mount, false, true, out b) && this.player.HasItem(MonoBehaviourSingleton<ShManager>.Instance.lockpick))
			{
				return true;
			}
			this.SendGameMessage("Cannot lockpick for some reason");
			return false;
		}

		// Token: 0x060003B9 RID: 953 RVA: 0x00014014 File Offset: 0x00012214
		protected void LockpickAction(int transportID)
		{
			ShTransport transport;
			if (EntityCollections.TryFindByID<ShTransport>(transportID, out transport))
			{
				SvPlayer.events.Lockpick(this.player, transport);
			}
		}

		// Token: 0x060003BA RID: 954 RVA: 0x00014040 File Offset: 0x00012240
		public void SvTransportState(int transportID, bool locked)
		{
			ShTransport shTransport;
			if (EntityCollections.TryFindByID<ShTransport>(transportID, out shTransport) && shTransport.owner == this.player)
			{
				shTransport.state = (locked ? EntityState.Locked : EntityState.Unlocked);
				shTransport.svTransport.SendTransportState();
			}
		}

		// Token: 0x060003BB RID: 955 RVA: 0x00014084 File Offset: 0x00012284
		public bool CanUseApp(int entityID, AppIndex appIndex)
		{
			if (this.player.IsMobile)
			{
				ShEntity shEntity;
				if (entityID == 0)
				{
					if (this.player.curEquipable.availableApps.Contains(appIndex))
					{
						return true;
					}
				}
				else if (EntityCollections.TryFindByID(entityID, out shEntity) && this.player.InActionRange(shEntity) && shEntity.availableApps.Contains(appIndex))
				{
					return true;
				}
			}
			this.SendGameMessage(string.Format("Device for App:'{0}' unavailable", appIndex));
			return false;
		}

		// Token: 0x060003BC RID: 956 RVA: 0x000140FA File Offset: 0x000122FA
		public void SvApps(int entityID)
		{
			if (!this.CanUseApp(entityID, AppIndex.Apps))
			{
				return;
			}
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.Apps, new object[]
			{
				entityID,
				Util.FormattedTime
			});
		}

		// Token: 0x060003BD RID: 957 RVA: 0x00014128 File Offset: 0x00012328
		public void SvAppContacts(int entityID)
		{
			if (!this.CanUseApp(entityID, AppIndex.Contacts))
			{
				return;
			}
			List<AppContact> list = new List<AppContact>();
			List<string> list2 = new List<string>();
			foreach (string text in this.appContacts)
			{
				User user;
				if (this.svManager.TryGetUserData(text, out user))
				{
					list.Add(new AppContact(text, user.Profile, EntityCollections.Accounts.ContainsKey(text) ? "Online" : user.LastUpdated.FormatTime()));
				}
				else
				{
					list2.Add(text);
				}
			}
			foreach (string item in list2)
			{
				this.appContacts.Remove(item);
			}
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AppContacts, new object[]
			{
				entityID,
				Util.FormattedTime,
				JsonConvert.SerializeObject(list)
			});
		}

		// Token: 0x060003BE RID: 958 RVA: 0x00014248 File Offset: 0x00012448
		public void SvAppBlocked(int entityID)
		{
			if (!this.CanUseApp(entityID, AppIndex.Blocked))
			{
				return;
			}
			List<AppContact> list = new List<AppContact>();
			List<string> list2 = new List<string>();
			foreach (string text in this.appBlocked)
			{
				User user;
				if (this.svManager.TryGetUserData(text, out user))
				{
					list.Add(new AppContact(text, user.Profile, EntityCollections.Accounts.ContainsKey(text) ? "Online" : user.LastUpdated.FormatTime()));
				}
				else
				{
					list2.Add(text);
				}
			}
			foreach (string item in list2)
			{
				this.appBlocked.Remove(item);
			}
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AppBlocked, new object[]
			{
				entityID,
				Util.FormattedTime,
				JsonConvert.SerializeObject(list)
			});
		}

		// Token: 0x060003BF RID: 959 RVA: 0x00014368 File Offset: 0x00012568
		public void SvAppCalls(int entityID)
		{
			if (!this.CanUseApp(entityID, AppIndex.Calls))
			{
				return;
			}
			foreach (AppCall appCall in this.appCalls)
			{
				appCall.profileURL = this.GetLatestPlayerProfileURL(appCall.playerName);
			}
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AppCalls, new object[]
			{
				entityID,
				Util.FormattedTime,
				JsonConvert.SerializeObject(this.appCalls)
			});
		}

		// Token: 0x060003C0 RID: 960 RVA: 0x00014400 File Offset: 0x00012600
		public void SvAppInbox(int entityID)
		{
			if (!this.CanUseApp(entityID, AppIndex.Inbox))
			{
				return;
			}
			List<AppInbox> list = new List<AppInbox>();
			foreach (string text in this.lastMessengers)
			{
				AppMessages appMessages;
				if (this.appMessages.TryGetValue(text, out appMessages))
				{
					string preview;
					DateTimeOffset time;
					if (appMessages.messages.Count > 0)
					{
						AppMessage appMessage = appMessages.messages.Last<AppMessage>();
						preview = appMessage.message;
						time = appMessage.time;
					}
					else
					{
						preview = string.Empty;
						time = Util.CurrentTime;
					}
					list.Add(new AppInbox(text, this.GetLatestPlayerProfileURL(appMessages.playerName), appMessages.unreadCount, preview, time));
				}
			}
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AppInbox, new object[]
			{
				entityID,
				Util.FormattedTime,
				JsonConvert.SerializeObject(list)
			});
		}

		// Token: 0x060003C1 RID: 961 RVA: 0x000144F4 File Offset: 0x000126F4
		protected string GetLatestPlayerProfileURL(string playerName)
		{
			ShPlayer shPlayer;
			if (EntityCollections.Accounts.TryGetValue(playerName, out shPlayer))
			{
				return shPlayer.profile;
			}
			User user;
			if (this.svManager.TryGetUserData(playerName, out user))
			{
				return user.Profile;
			}
			return string.Empty;
		}

		// Token: 0x060003C2 RID: 962 RVA: 0x00014533 File Offset: 0x00012733
		public void SvAppServices(int entityID)
		{
			if (!this.CanUseApp(entityID, AppIndex.Services))
			{
				return;
			}
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AppServices, new object[]
			{
				entityID,
				Util.FormattedTime
			});
		}

		// Token: 0x060003C3 RID: 963 RVA: 0x00014564 File Offset: 0x00012764
		public void SvAppDeposit(int entityID)
		{
			if (!this.CanUseApp(entityID, AppIndex.Deposit))
			{
				return;
			}
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AppDeposit, new object[]
			{
				entityID,
				Util.FormattedTime,
				this.bankBalance,
				JsonConvert.SerializeObject(this.appTransactions)
			});
		}

		// Token: 0x060003C4 RID: 964 RVA: 0x000145BC File Offset: 0x000127BC
		public void SvAppWithdraw(int entityID)
		{
			if (!this.CanUseApp(entityID, AppIndex.Withdraw))
			{
				return;
			}
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AppWithdraw, new object[]
			{
				entityID,
				Util.FormattedTime,
				this.bankBalance,
				JsonConvert.SerializeObject(this.appTransactions)
			});
		}

		// Token: 0x060003C5 RID: 965 RVA: 0x00014614 File Offset: 0x00012814
		public void SvAppRadio(int entityID)
		{
			if (!this.CanUseApp(entityID, AppIndex.Radio))
			{
				return;
			}
			List<AppContact> list = new List<AppContact>();
			foreach (ShPlayer shPlayer in EntityCollections.Humans)
			{
				if (shPlayer.chatChannel == this.player.chatChannel)
				{
					list.Add(new AppContact(shPlayer.username, shPlayer.profile, "Online"));
				}
			}
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AppRadio, new object[]
			{
				entityID,
				Util.FormattedTime,
				JsonConvert.SerializeObject(list)
			});
		}

		// Token: 0x060003C6 RID: 966 RVA: 0x000146C4 File Offset: 0x000128C4
		public void SvMessage(int entityID, string playerName)
		{
			if (!this.CanUseApp(entityID, AppIndex.Inbox))
			{
				return;
			}
			if (playerName == this.player.username)
			{
				this.SendGameMessage("Cannot message self");
				return;
			}
			User user;
			if (this.svManager.TryGetUserData(playerName, out user))
			{
				AppMessages appMessages;
				AppMessages value;
				if (this.appMessages.TryGetValue(playerName, out appMessages))
				{
					this.ReadMessages(appMessages);
					value = new AppMessages(appMessages.playerName, this.GetLatestPlayerProfileURL(playerName), this.unreadMessages, appMessages.messages);
				}
				else
				{
					value = new AppMessages(playerName, this.GetLatestPlayerProfileURL(playerName), this.unreadMessages);
				}
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AppMessage, new object[]
				{
					entityID,
					Util.FormattedTime,
					JsonConvert.SerializeObject(value)
				});
				return;
			}
			this.SendGameMessage("Player doesn't exist");
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x0001478D File Offset: 0x0001298D
		public void SvAddContact(int entityID, string nameOrID)
		{
			if (!this.CanUseApp(entityID, AppIndex.Contacts) || !this.SvAddToSet(nameOrID, this.appContacts))
			{
				return;
			}
			this.SvAppContacts(entityID);
		}

		// Token: 0x060003C8 RID: 968 RVA: 0x000147B0 File Offset: 0x000129B0
		public void SvAddBlocked(int entityID, string nameOrID)
		{
			if (!this.CanUseApp(entityID, AppIndex.Blocked) || !this.SvAddToSet(nameOrID, this.appBlocked))
			{
				return;
			}
			this.SvAppBlocked(entityID);
		}

		// Token: 0x060003C9 RID: 969 RVA: 0x000147D4 File Offset: 0x000129D4
		protected bool SvAddToSet(string nameOrID, HashSet<string> set)
		{
			if (set.Count >= 30)
			{
				this.SendGameMessage("List full");
				return false;
			}
			ShPlayer shPlayer;
			if (EntityCollections.TryGetPlayerByNameOrID(nameOrID, out shPlayer))
			{
				return !(shPlayer == this.player) && set.Add(shPlayer.username);
			}
			User user;
			if (this.svManager.TryGetUserData(nameOrID, out user))
			{
				return !(user.ID == this.player.username) && set.Add(user.ID);
			}
			this.SendGameMessage("Player not found");
			return false;
		}

		// Token: 0x060003CA RID: 970 RVA: 0x00014862 File Offset: 0x00012A62
		public void SvRemoveContact(int entityID, string playerName)
		{
			if (!this.CanUseApp(entityID, AppIndex.Contacts))
			{
				return;
			}
			if (this.appContacts.Remove(playerName))
			{
				this.SvAppContacts(entityID);
				return;
			}
			this.SendGameMessage("Contact doesn't exist");
		}

		// Token: 0x060003CB RID: 971 RVA: 0x00014890 File Offset: 0x00012A90
		public void SvRemoveBlocked(int entityID, string playerName)
		{
			if (!this.CanUseApp(entityID, AppIndex.Blocked))
			{
				return;
			}
			if (this.appBlocked.Remove(playerName))
			{
				this.SvAppBlocked(entityID);
				return;
			}
			this.SendGameMessage("Contact doesn't exist");
		}

		// Token: 0x060003CC RID: 972 RVA: 0x000148C0 File Offset: 0x00012AC0
		public void SvAddMessage(int entityID, string otherUsername, string message)
		{
			if (!this.CanUseApp(entityID, AppIndex.Inbox))
			{
				return;
			}
			if (otherUsername == this.player.username)
			{
				this.SendGameMessage("Cannot message self");
				return;
			}
			User user;
			if (this.svManager.TryGetUserData(otherUsername, out user))
			{
				message = message.CleanMessage();
				ShPlayer shPlayer;
				if (EntityCollections.Accounts.TryGetValue(otherUsername, out shPlayer))
				{
					if (shPlayer.svPlayer.appBlocked.Contains(this.player.username))
					{
						this.SendGameMessage("Message Blocked");
						shPlayer.svPlayer.SendGameMessage("Message from " + this.player.username + " blocked");
						return;
					}
					shPlayer.svPlayer.unreadMessages += this.RecordMessage(this.player.username, this.player.profile, message, false, ref shPlayer.svPlayer.appMessages, ref shPlayer.svPlayer.lastMessengers);
					shPlayer.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AppAddMessage, new object[]
					{
						this.player.username,
						this.player.ID,
						shPlayer.svPlayer.unreadMessages,
						JsonConvert.SerializeObject(new AppMessage(false, Util.CurrentTime, message))
					});
				}
				else
				{
					if (user.Persistent.AppBlocked.Contains(this.player.username))
					{
						this.SendGameMessage("Message Blocked");
						return;
					}
					Dictionary<string, AppMessages> dictionary = JsonConvert.DeserializeObject<Dictionary<string, AppMessages>>(user.Persistent.AppMessages);
					if (dictionary == null)
					{
						dictionary = new Dictionary<string, AppMessages>();
					}
					List<string> list = user.Persistent.LastMessengers;
					user.Persistent.UnreadMessages += this.RecordMessage(this.player.username, this.player.profile, message, false, ref dictionary, ref list);
					user.Persistent.AppMessages = JsonConvert.SerializeObject(dictionary);
					user.Persistent.LastMessengers = list;
					this.svManager.database.Users.Upsert(user);
				}
				this.unreadMessages += this.RecordMessage(otherUsername, user.Profile, message, true, ref this.appMessages, ref this.lastMessengers);
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AppAddMessage, new object[]
				{
					otherUsername,
					this.player.ID,
					this.unreadMessages,
					JsonConvert.SerializeObject(new AppMessage(true, Util.CurrentTime, message))
				});
				return;
			}
			this.SendGameMessage("Player doesn't exist");
		}

		// Token: 0x060003CD RID: 973 RVA: 0x00014B58 File Offset: 0x00012D58
		protected int RecordMessage(string playerName, string profileURL, string message, bool self, ref Dictionary<string, AppMessages> messages, ref List<string> messengers)
		{
			int num = 0;
			AppMessages appMessages;
			if (messages.TryGetValue(playerName, out appMessages))
			{
				messengers.RemoveAll((string x) => x == playerName);
				if (!self)
				{
					appMessages.unreadCount++;
					num++;
				}
			}
			else
			{
				int unreadCount;
				if (self)
				{
					unreadCount = 0;
				}
				else
				{
					num++;
					unreadCount = 1;
				}
				appMessages = new AppMessages(playerName, profileURL, unreadCount);
				messages[playerName] = appMessages;
			}
			messengers.Add(playerName);
			int num2 = messengers.Count - 30;
			if (num2 > 0)
			{
				for (int i = 0; i < num2; i++)
				{
					string key = messengers[i];
					num -= messages[key].unreadCount;
					messages.Remove(key);
				}
				messengers.RemoveRange(0, num2);
			}
			List<AppMessage> messages2 = appMessages.messages;
			messages2.Add(new AppMessage(self, Util.CurrentTime, message));
			while (messages2.Count > 30)
			{
				messages2.RemoveAt(0);
				if (!self && appMessages.unreadCount > 30)
				{
					appMessages.unreadCount--;
					num--;
				}
			}
			return num;
		}

		// Token: 0x060003CE RID: 974 RVA: 0x00014C94 File Offset: 0x00012E94
		public void SvReadMessage(int entityID, string playerName)
		{
			if (!this.CanUseApp(entityID, AppIndex.Inbox))
			{
				return;
			}
			AppMessages messages;
			if (this.appMessages.TryGetValue(playerName, out messages))
			{
				this.ReadMessages(messages);
			}
		}

		// Token: 0x060003CF RID: 975 RVA: 0x00014CC3 File Offset: 0x00012EC3
		protected void ReadMessages(AppMessages messages)
		{
			this.unreadMessages -= messages.unreadCount;
			messages.unreadCount = 0;
		}

		// Token: 0x060003D0 RID: 976 RVA: 0x00014CE0 File Offset: 0x00012EE0
		public void SvReadAll(int entityID)
		{
			if (!this.CanUseApp(entityID, AppIndex.Inbox))
			{
				return;
			}
			foreach (AppMessages appMessages in this.appMessages.Values)
			{
				appMessages.unreadCount = 0;
			}
			this.unreadMessages = 0;
			this.SvAppInbox(entityID);
		}

		// Token: 0x060003D1 RID: 977 RVA: 0x00014D50 File Offset: 0x00012F50
		public void SvCall(int entityID, string playerName)
		{
			if (!this.CanUseApp(entityID, AppIndex.Calls))
			{
				return;
			}
			if (playerName == this.player.username || this.callTarget)
			{
				this.SendGameMessage("Cannot call");
				return;
			}
			ShPlayer shPlayer;
			if (!EntityCollections.Accounts.TryGetValue(playerName, out shPlayer))
			{
				this.SendGameMessage("Player isn't online");
				return;
			}
			if (shPlayer.svPlayer.appBlocked.Contains(this.player.username))
			{
				this.SendGameMessage("Call blocked");
				this.MissedCallMessage(shPlayer);
				return;
			}
			if (shPlayer.svPlayer.callTarget)
			{
				this.SendGameMessage("Phone is busy");
				this.MissedCallMessage(shPlayer);
				return;
			}
			if (!shPlayer.IsMobile)
			{
				this.SendGameMessage("Player is incapacitated");
				this.MissedCallMessage(shPlayer);
				return;
			}
			ShPhone phone;
			if (shPlayer.TryGetCachedItem<ShPhone>(out phone))
			{
				base.StartCoroutine(this.StartCall(entityID, shPlayer, phone));
				return;
			}
			this.SendGameMessage("Player doesn't have phone");
			this.MissedCallMessage(shPlayer);
		}

		// Token: 0x060003D2 RID: 978 RVA: 0x00014E50 File Offset: 0x00013050
		public void SvCallAccept()
		{
			if (this.callTarget && !this.callActive && !this.caller)
			{
				this.callActive = true;
				this.callTarget.svPlayer.callActive = true;
				this.AppendCall(this.callTarget, true);
				this.callTarget.svPlayer.AppendCall(this.player, true);
			}
		}

		// Token: 0x060003D3 RID: 979 RVA: 0x00014EB6 File Offset: 0x000130B6
		public void SvCallReject()
		{
			if (this.callTarget)
			{
				this.callTarget.svPlayer.callTarget = null;
				this.callTarget = null;
			}
		}

		// Token: 0x060003D4 RID: 980 RVA: 0x00014EDD File Offset: 0x000130DD
		protected IEnumerator StartCall(int entityID, ShPlayer otherPlayer, ShPhone phone)
		{
			this.caller = true;
			this.callTarget = otherPlayer;
			this.callTarget.svPlayer.callTarget = this.player;
			this.callActive = (this.callTarget.svPlayer.callActive = false);
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.Call, new object[]
			{
				this.callTarget.ID,
				false
			});
			this.callTarget.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.Call, new object[]
			{
				this.player.ID,
				true
			});
			float endTime = Time.time + 10f;
			while (this.CanUseApp(entityID, AppIndex.Calls) && this.callTarget && this.callTarget.IsMobile && !this.callActive && Time.time < endTime)
			{
				yield return null;
			}
			if (!this.callActive)
			{
				this.CleanupCall(otherPlayer);
				yield break;
			}
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.CallAccepted, Array.Empty<object>());
			this.callTarget.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.CallAccepted, Array.Empty<object>());
			this.callTarget.svPlayer.SvTrySetEquipable(phone.index, false);
			ShPhone shPhone;
			while (this.CanUseApp(entityID, AppIndex.Calls) && this.callTarget && this.callTarget.IsMobile && this.callTarget.TryGetCachedItem<ShPhone>(out shPhone))
			{
				yield return null;
			}
			this.CleanupCall(otherPlayer);
			yield break;
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x00014F04 File Offset: 0x00013104
		public void CleanupCall(ShPlayer otherPlayer)
		{
			if (this.caller)
			{
				if (!this.callActive)
				{
					this.AppendCall(otherPlayer, false);
					if (otherPlayer)
					{
						otherPlayer.svPlayer.AppendCall(this.player, false);
					}
				}
				if (otherPlayer)
				{
					otherPlayer.svPlayer.callTarget = null;
					otherPlayer.svPlayer.callActive = false;
					otherPlayer.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.CallCanceled, Array.Empty<object>());
				}
				this.caller = false;
				this.callTarget = null;
				this.callActive = false;
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.CallCanceled, Array.Empty<object>());
			}
		}

		// Token: 0x060003D6 RID: 982 RVA: 0x00014F9E File Offset: 0x0001319E
		public void SvDeposit(int entityID, int amount)
		{
			SvPlayer.events.Deposit(this.player, entityID, amount);
		}

		// Token: 0x060003D7 RID: 983 RVA: 0x00014FB3 File Offset: 0x000131B3
		public void SvWithdraw(int entityID, int amount)
		{
			SvPlayer.events.Withdraw(this.player, entityID, amount);
		}

		// Token: 0x060003D8 RID: 984 RVA: 0x00014FC8 File Offset: 0x000131C8
		public void AppendCall(ShPlayer other, bool success)
		{
			this.appCalls.Add(new AppCall(other.username, other.profile, success, Util.CurrentTime));
			int num = this.appCalls.Count - 30;
			if (num > 0)
			{
				this.appCalls.RemoveRange(0, num);
			}
		}

		// Token: 0x060003D9 RID: 985 RVA: 0x00015018 File Offset: 0x00013218
		public void AppendTransaction(int amount)
		{
			this.appTransactions.Add(new AppTransaction(amount, Util.CurrentTime));
			int num = this.appTransactions.Count - 30;
			if (num > 0)
			{
				this.appTransactions.RemoveRange(0, num);
			}
		}

		// Token: 0x060003DA RID: 986 RVA: 0x0001505C File Offset: 0x0001325C
		public void SvChatChannel(int entityID, string chatChannel)
		{
			if (!this.player.svPlayer.CanUseApp(entityID, AppIndex.Radio))
			{
				return;
			}
			ushort channel;
			if (ushort.TryParse(chatChannel, out channel))
			{
				this.SvSetChatChannel(channel);
				this.SvAppRadio(entityID);
				return;
			}
			this.SendGameMessage(string.Format("Keep channel between {0} and {1}", 0, ushort.MaxValue));
		}

		// Token: 0x060003DB RID: 987 RVA: 0x000150B7 File Offset: 0x000132B7
		public void SvChatGlobal(string message)
		{
			SvPlayer.events.ChatGlobal(this.player, message);
		}

		// Token: 0x060003DC RID: 988 RVA: 0x000150CB File Offset: 0x000132CB
		public void SvChatLocal(string message)
		{
			SvPlayer.events.ChatLocal(this.player, message);
		}

		// Token: 0x060003DD RID: 989 RVA: 0x000150DF File Offset: 0x000132DF
		public void SvChatVoice(byte[] voiceData)
		{
			SvPlayer.events.ChatVoice(this.player, voiceData);
		}

		// Token: 0x060003DE RID: 990 RVA: 0x000150F3 File Offset: 0x000132F3
		public void SvSetChatMode(ChatMode chatMode)
		{
			SvPlayer.events.SetChatMode(this.player, chatMode);
		}

		// Token: 0x060003DF RID: 991 RVA: 0x00015107 File Offset: 0x00013307
		public void SvSetChatChannel(ushort channel)
		{
			SvPlayer.events.SetChatChannel(this.player, channel);
		}

		// Token: 0x060003E0 RID: 992 RVA: 0x0001511C File Offset: 0x0001331C
		public void SvOptionAction()
		{
			int targetID;
			string id;
			string optionID;
			string actionID;
			Util.ParseOptionAction(out targetID, out id, out optionID, out actionID);
			SvPlayer.events.OptionAction(this.player, targetID, id, optionID, actionID);
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x0001514C File Offset: 0x0001334C
		public void SvTextPanelButton()
		{
			string id;
			string optionID;
			Util.ParseTextPanelButton(out id, out optionID);
			SvPlayer.events.TextPanelButton(this.player, id, optionID);
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x00015178 File Offset: 0x00013378
		public void SvSubmitInput()
		{
			int targetID;
			string id;
			string input;
			Util.ParseSubmitInput(out targetID, out id, out input);
			SvPlayer.events.SubmitInput(this.player, targetID, id, input);
		}

		// Token: 0x060003E3 RID: 995 RVA: 0x000151A4 File Offset: 0x000133A4
		protected void MissedCallMessage(ShPlayer other)
		{
			other.svPlayer.SendGameMessage("Missed call from " + this.player.username);
		}

		// Token: 0x060003E4 RID: 996 RVA: 0x000151C8 File Offset: 0x000133C8
		public void SvEnterDoor(int doorID, ShPlayer sender, bool forceEnter)
		{
			ShDoor shDoor;
			if (EntityCollections.TryFindByID<ShDoor>(doorID, out shDoor) && shDoor.svDoor.other)
			{
				SvPlayer.events.EnterDoor(this.player, shDoor, sender, forceEnter);
			}
		}

		// Token: 0x060003E5 RID: 997 RVA: 0x00015205 File Offset: 0x00013405
		public void SvOpenURL(string url, string title = "Open URL Confirmation")
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.OpenURL, new object[]
			{
				url,
				title
			});
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x00015224 File Offset: 0x00013424
		public void SvBuyTransport(int transportID)
		{
			ShTransport shTransport;
			if (EntityCollections.TryFindByID<ShTransport>(transportID, out shTransport) && !shTransport.owner && shTransport.state == EntityState.ForSale && shTransport.svTransport.BuyEntity(this.player))
			{
				shTransport.svTransport.SvSetTransportOwner(this.player);
			}
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x00015278 File Offset: 0x00013478
		public void TryBuyApartment(int apartmentID)
		{
			ShApartment apartment;
			if (EntityCollections.TryFindByID<ShApartment>(apartmentID, out apartment))
			{
				SvPlayer.events.BuyApartment(this.player, apartment);
			}
		}

		// Token: 0x060003E8 RID: 1000 RVA: 0x000152A4 File Offset: 0x000134A4
		public void TrySellApartment(int apartmentID)
		{
			ShApartment apartment;
			if (EntityCollections.TryFindByID<ShApartment>(apartmentID, out apartment))
			{
				SvPlayer.events.SellApartment(this.player, apartment);
			}
		}

		// Token: 0x060003E9 RID: 1001 RVA: 0x000152D0 File Offset: 0x000134D0
		public void SvBuyFurniture(int itemIndex)
		{
			ShFurniture shFurniture;
			if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShFurniture>(itemIndex, out shFurniture) && this.player.InOwnApartment && this.player.MyMoneyCount >= shFurniture.value && this.player.CanEquip(shFurniture) && this.SvTrySetEquipable(shFurniture.index, false))
			{
				this.player.TransferMoney(2, shFurniture.value, true);
				return;
			}
			this.SendGameMessage("Unable");
		}

		// Token: 0x060003EA RID: 1002 RVA: 0x00015348 File Offset: 0x00013548
		public void SvSellFurniture(int furnitureID)
		{
			ShFurniture shFurniture;
			if (this.player.InOwnApartment && EntityCollections.TryFindByID<ShFurniture>(furnitureID, out shFurniture) && shFurniture.GetPlaceIndex() == this.player.GetPlaceIndex())
			{
				if (shFurniture.HasInventory && shFurniture.myItems.Count > 0)
				{
					this.SendGameMessage("Must be empty to sell");
					return;
				}
				this.player.TransferMoney(1, shFurniture.value / 2, true);
				shFurniture.Destroy();
			}
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x000153BC File Offset: 0x000135BC
		public void SvGetEntityValue(int entityID)
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(entityID, out shEntity))
			{
				this.SendGameMessage("$" + shEntity.value.ToString());
			}
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x000153F0 File Offset: 0x000135F0
		public void SvShowHealth(int playerID)
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(playerID, out shPlayer) && shPlayer.isActiveAndEnabled && !shPlayer.IsDead)
			{
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.ShowHealth, new object[]
				{
					shPlayer.ID,
					shPlayer.svPlayer.SerializeHealth()
				});
			}
		}

		// Token: 0x060003ED RID: 1005 RVA: 0x00015444 File Offset: 0x00013644
		public void SvView(int otherID, bool force = false)
		{
			ShEntity shEntity;
			if (!this.player.InventoryLocked && EntityCollections.TryFindByID(otherID, out shEntity) && (force || shEntity.CanBeSearched(this.player)) && this.player.InActionRange(shEntity))
			{
				ShPlayer shPlayer = shEntity as ShPlayer;
				if (shPlayer != null)
				{
					if (shPlayer.otherEntity)
					{
						shPlayer.svPlayer.SvStopInventory(true);
						shPlayer.svPlayer.SvMinigameStop(true);
					}
					if (shEntity.isHuman && shEntity.viewers.Count == 0)
					{
						shPlayer.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.ShowSearchedInventory, Array.Empty<object>());
					}
				}
				this.player.otherEntity = shEntity;
				shEntity.viewers.Add(this.player);
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.View, new object[]
				{
					otherID,
					shEntity.SerializeMyItems()
				});
				SvPlayer.events.ViewInventory(this.player, shEntity, force);
			}
		}

		// Token: 0x060003EE RID: 1006 RVA: 0x00015540 File Offset: 0x00013740
		public void SvShop(int shopID)
		{
			ShEntity shEntity;
			if (!this.player.InventoryLocked && EntityCollections.TryFindByID(shopID, out shEntity) && shEntity.Shop && this.player.InActionRange(shEntity))
			{
				shEntity.svEntity.Shop(this.player);
			}
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x0001558B File Offset: 0x0001378B
		public override void Shop(ShPlayer customer)
		{
			if (this.player.InventoryLocked || this.targetEntity)
			{
				return;
			}
			this.SetState(0);
			base.Shop(customer);
		}

		// Token: 0x060003F0 RID: 1008 RVA: 0x000155B8 File Offset: 0x000137B8
		public ShEntity SpawnBriefcase()
		{
			RaycastHit raycastHit;
			if (Physics.Raycast(this.player.Position + Vector3.up, Vector3.down, out raycastHit, 10f, 1))
			{
				return this.svManager.AddNewEntity<ShEntity>(this.svManager.briefcasePrefabs.GetRandom<ShEntity>(), this.player.Place, raycastHit.point, Quaternion.LookRotation(Vector3.ProjectOnPlane(this.player.mainT.forward, raycastHit.normal), raycastHit.normal), false, null, false);
			}
			return null;
		}

		// Token: 0x060003F1 RID: 1009 RVA: 0x00015648 File Offset: 0x00013848
		public void SvDrop()
		{
			if (this.player.InventoryLocked || this.player.viewers.Count > 0)
			{
				return;
			}
			ShEntity shEntity = this.SpawnBriefcase();
			if (shEntity)
			{
				this.player.otherEntity = shEntity;
				shEntity.viewers.Add(this.player);
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.View, new object[]
				{
					shEntity.ID,
					shEntity.SerializeMyItems()
				});
			}
		}

		// Token: 0x060003F2 RID: 1010 RVA: 0x000156CA File Offset: 0x000138CA
		protected void SetTradePartner(ShPlayer otherPlayer)
		{
			this.tradePlayer = null;
			otherPlayer.svPlayer.tradePlayer = null;
			this.player.otherEntity = otherPlayer;
			otherPlayer.otherEntity = this.player;
		}

		// Token: 0x060003F3 RID: 1011 RVA: 0x000156F8 File Offset: 0x000138F8
		public void SvTradeRequest(int otherID)
		{
			ShPlayer shPlayer;
			if (this.player.InventoryLocked || !EntityCollections.TryFindByID<ShPlayer>(otherID, out shPlayer) || !shPlayer.isActiveAndEnabled || !shPlayer.IsUp || shPlayer.otherEntity || shPlayer.Shop)
			{
				this.SendGameMessage("Player is occupied");
				return;
			}
			this.tradePlayer = shPlayer;
			if (!shPlayer.isHuman)
			{
				if (shPlayer.svPlayer.currentState.IsBusy)
				{
					return;
				}
				this.SendGameMessage(shPlayer.svPlayer.buyerType.buyMessage);
				if (shPlayer.svPlayer.buyerType.type != null)
				{
					this.SetTradePartner(shPlayer);
					shPlayer.svPlayer.SetState(0);
					base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.ShowTradeInventory, new object[]
					{
						shPlayer.ID
					});
					return;
				}
			}
			else
			{
				if (shPlayer.svPlayer.tradePlayer == this.player)
				{
					this.SetTradePartner(shPlayer);
					base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.ShowTradeInventory, new object[]
					{
						shPlayer.ID
					});
					shPlayer.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.ShowTradeInventory, new object[]
					{
						this.player.ID
					});
					return;
				}
				shPlayer.svPlayer.SendGameMessage(this.player.username + " wants to trade");
			}
		}

		// Token: 0x060003F4 RID: 1012 RVA: 0x00015859 File Offset: 0x00013A59
		public void Reward(int experienceDelta, int moneyDelta)
		{
			SvPlayer.events.Reward(this.player, experienceDelta, moneyDelta);
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x00015870 File Offset: 0x00013A70
		public void SetExperience(int experience, bool showMessage)
		{
			this.player.experience = Mathf.Clamp(experience, 0, this.player.GetMaxExperience());
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.Experience, new object[]
			{
				this.player.experience,
				showMessage
			});
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x000158C8 File Offset: 0x00013AC8
		public void SetRank(int rank)
		{
			this.player.rank = this.job.info.LimitRank(rank);
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.Rank, new object[]
			{
				this.player.rank
			});
		}

		// Token: 0x060003F7 RID: 1015 RVA: 0x00015914 File Offset: 0x00013B14
		public bool AddJobItems(JobInfo newJob, int rank, bool collectCost)
		{
			if (rank < newJob.shared.upgrades.Length)
			{
				int num = 0;
				List<InventoryItem> list = new List<InventoryItem>();
				for (int i = 0; i <= rank; i++)
				{
					foreach (InventoryStruct inventoryStruct in newJob.shared.upgrades[i].items)
					{
						ShItem shItem;
						if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShItem>(inventoryStruct.itemName, out shItem))
						{
							int num2 = inventoryStruct.count - this.player.MyItemCount(shItem);
							list.Add(new InventoryItem(shItem, num2, 0));
							if (collectCost && num2 > 0)
							{
								num += num2 * shItem.value;
							}
						}
					}
				}
				if (collectCost && num > 0)
				{
					if (this.player.MyMoneyCount < num)
					{
						this.SendGameMessage("You need $" + num.ToString() + " for equipment");
						return false;
					}
					this.player.TransferMoney(2, num, true);
				}
				foreach (InventoryItem inventoryItem in list)
				{
					if (inventoryItem.count > 0)
					{
						this.player.TransferItem(1, inventoryItem.item, inventoryItem.count, true);
					}
					ShWearable shWearable = inventoryItem.item as ShWearable;
					if (shWearable != null && this.player.CanWear(shWearable))
					{
						this.SvSetWearable(inventoryItem.item.index);
					}
				}
			}
			return true;
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x00015AA4 File Offset: 0x00013CA4
		public void SvTrySetJob(int jobIndex, bool addItems, bool collectCost)
		{
			if (jobIndex < 0 || jobIndex >= BPAPI.Jobs.Count)
			{
				jobIndex = 0;
			}
			JobInfo jobInfo = BPAPI.Jobs[jobIndex];
			if (!this.player.isHuman)
			{
				this.SvSetJob(jobInfo, addItems, false);
				return;
			}
			if (jobInfo.maxCount <= 0 || jobInfo.members.HumanCount() < jobInfo.maxCount)
			{
				this.SvSetJob(jobInfo, addItems, collectCost);
				return;
			}
			this.SendGameMessage("Job positions are full");
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x00015B1C File Offset: 0x00013D1C
		public void SvSetJob(JobInfo newJob, bool addItems, bool collectCost)
		{
			int rank = newJob.LimitRank(this.spawnJobRank);
			if (!addItems || this.AddJobItems(newJob, rank, collectCost))
			{
				int jobIndex = newJob.shared.jobIndex;
				if (this.job == null || jobIndex != this.job.info.shared.jobIndex)
				{
					Job job = this.job;
					if (job != null)
					{
						job.RemoveJob();
					}
					this.InitializeJob(newJob, rank);
					this.job.SetJob();
					base.Send(SvSendType.All, PacketFlags.Reliable, ClPacket.SetJob, new object[]
					{
						this.player.ID,
						jobIndex
					});
				}
				this.job.ResetJob();
				SvPlayer.events.SetJob(this.player, newJob, rank, addItems, collectCost);
			}
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x00015BE8 File Offset: 0x00013DE8
		protected void InitializeJob(JobInfo info, int rank)
		{
			this.job = (Job)Activator.CreateInstance(info.jobType);
			this.job.Initialize(this.player, info);
			this.player.experience = 0;
			this.player.rank = rank;
		}

		// Token: 0x17000085 RID: 133
		// (get) Token: 0x060003FB RID: 1019 RVA: 0x00015C35 File Offset: 0x00013E35
		public bool IsValidBoss
		{
			get
			{
				return this.player.boss && this.job.info.shared.jobIndex == this.spawnJobIndex;
			}
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x00015C64 File Offset: 0x00013E64
		public void SvGetJob(int employerID)
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(employerID, out shPlayer) && shPlayer.svPlayer.IsValidBoss && shPlayer.svPlayer.job != this.job && this.player.InActionRange(shPlayer))
			{
				SvPlayer.events.TryGetJob(this.player, shPlayer);
			}
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x00015CC0 File Offset: 0x00013EC0
		public void SvResetJob()
		{
			this.SvTrySetJob(this.spawnJobIndex, true, false);
		}

		// Token: 0x060003FE RID: 1022 RVA: 0x00015CD0 File Offset: 0x00013ED0
		public void SvQuitJob(int employerID)
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(employerID, out shPlayer) && shPlayer.svPlayer.IsValidBoss && shPlayer.svPlayer.job == this.job && this.player.InActionRange(shPlayer))
			{
				this.SvResetJob();
			}
		}

		// Token: 0x060003FF RID: 1023 RVA: 0x00015D20 File Offset: 0x00013F20
		public void Restrain(ShPlayer initiator, ShRestrained restrained)
		{
			SvPlayer.events.Restrain(this.player, initiator, restrained);
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x00015D35 File Offset: 0x00013F35
		public void Unrestrain(ShPlayer initiator)
		{
			SvPlayer.events.Unrestrain(this.player, initiator);
		}

		// Token: 0x06000401 RID: 1025 RVA: 0x00015D4C File Offset: 0x00013F4C
		public void SvFree(int restrainedID)
		{
			ShPlayer shPlayer;
			if (this.player.IsUp && EntityCollections.TryFindByID<ShPlayer>(restrainedID, out shPlayer) && this.player != shPlayer && !shPlayer.IsDead && this.player.InActionRange(shPlayer))
			{
				ShRestrained shRestrained = shPlayer.curEquipable as ShRestrained;
				if (shRestrained != null)
				{
					this.player.TransferItem(1, shRestrained.restraint, 1, true);
					shPlayer.svPlayer.Unrestrain(this.player);
				}
			}
		}

		// Token: 0x06000402 RID: 1026 RVA: 0x00015DC8 File Offset: 0x00013FC8
		public void SvTriggerEvent(string eventName, string jsonString)
		{
			JToken jtoken = null;
			if (!string.IsNullOrWhiteSpace(jsonString))
			{
				try
				{
					jtoken = JToken.Parse(jsonString);
				}
				catch (Exception ex)
				{
					string str = "Trigger Event parse error: ";
					Exception ex2 = ex;
					Util.Log(str + ((ex2 != null) ? ex2.ToString() : null), LogLevel.Error);
					return;
				}
			}
			EventsHandler.Exec(eventName, new object[]
			{
				this.player,
				jtoken
			});
		}

		// Token: 0x06000403 RID: 1027 RVA: 0x00015E34 File Offset: 0x00014034
		public void SvEntityAction(int targetID, string eventName)
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(targetID, out shEntity) && this.player.InActionRange(shEntity))
			{
				EventsHandler.Exec(eventName, new object[]
				{
					shEntity,
					this.player
				});
			}
		}

		// Token: 0x06000404 RID: 1028 RVA: 0x00015E74 File Offset: 0x00014074
		public void SvInventoryAction(int itemIndex, string eventName)
		{
			ShItem shItem;
			if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShItem>(itemIndex, out shItem))
			{
				EventsHandler.Exec(eventName, new object[]
				{
					this.player,
					shItem
				});
			}
		}

		// Token: 0x06000405 RID: 1029 RVA: 0x00015EA9 File Offset: 0x000140A9
		public void SvSelfAction(string eventName)
		{
			EventsHandler.Exec(eventName, new object[]
			{
				this.player
			});
		}

		// Token: 0x06000406 RID: 1030 RVA: 0x00015EC0 File Offset: 0x000140C0
		public void SvAddSelfAction(string eventName, string label)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AddSelfAction, new object[]
			{
				eventName,
				label
			});
		}

		// Token: 0x06000407 RID: 1031 RVA: 0x00015EDD File Offset: 0x000140DD
		public void SvRemoveSelfAction(string eventName)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.RemoveSelfAction, new object[]
			{
				eventName
			});
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x00015EF6 File Offset: 0x000140F6
		public void SvAddTypeAction(string eventName, string type, string label)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AddTypeAction, new object[]
			{
				eventName,
				type,
				label
			});
		}

		// Token: 0x06000409 RID: 1033 RVA: 0x00015F17 File Offset: 0x00014117
		public void SvRemoveTypeAction(string eventName)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.RemoveTypeAction, new object[]
			{
				eventName
			});
		}

		// Token: 0x0600040A RID: 1034 RVA: 0x00015F30 File Offset: 0x00014130
		public void SvAddInventoryAction(string eventName, string type, ButtonType buttonType, string label)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AddInventoryAction, new object[]
			{
				eventName,
				type,
				(byte)buttonType,
				label
			});
		}

		// Token: 0x0600040B RID: 1035 RVA: 0x00015F5B File Offset: 0x0001415B
		public void SvRemoveInventoryAction(string eventName)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.RemoveInventoryAction, new object[]
			{
				eventName
			});
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x00015F74 File Offset: 0x00014174
		public void SvEmbark(int mountID)
		{
			ShPlayer shPlayer;
			if (this.player.IsControlledMount<ShPlayer>(out shPlayer))
			{
				this.SvDismount(false);
				shPlayer.svPlayer.SvTryMount(mountID, false);
			}
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x00015FA8 File Offset: 0x000141A8
		public void SvDisembark(int passengerID)
		{
			ShPlayer shPlayer;
			if (!this.player.IsRestrained && EntityCollections.TryFindByID<ShPlayer>(passengerID, out shPlayer) && shPlayer.IsRestrained)
			{
				shPlayer.svPlayer.SvDismount(false);
			}
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x00015FE0 File Offset: 0x000141E0
		public void SvSpectate(ShPlayer target)
		{
			if (target && this.player.specPlayer != target && (target == this.player || (target.isActiveAndEnabled && target.svPlayer.spectators.Add(this.player))))
			{
				if (!this.player.SpecSelf)
				{
					this.player.specPlayer.svPlayer.spectators.Remove(this.player);
				}
				this.player.specPlayer = target;
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.Spectate, new object[]
				{
					target.ID
				});
				if (this.player.SpecSelf)
				{
					this.Respawn();
					this.WriteActivateData();
					using (Dictionary<ValueTuple<int, Vector2Int>, NetSector>.ValueCollection.Enumerator enumerator = this.localSectors.Values.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							NetSector netSector = enumerator.Current;
							foreach (ShPlayer other in netSector.humans)
							{
								base.SendActivateToOther(other);
							}
						}
						goto IL_1B0;
					}
				}
				if (this.player.isActiveAndEnabled)
				{
					this.Deactivate(false);
					foreach (NetSector netSector2 in this.localSectors.Values)
					{
						foreach (ShPlayer other2 in netSector2.humans)
						{
							base.SendDeactivateToOther(other2);
						}
					}
				}
				IL_1B0:
				base.NewSector();
			}
		}

		// Token: 0x0600040F RID: 1039 RVA: 0x000161D8 File Offset: 0x000143D8
		public void SvClearInjuries()
		{
			this.player.ClearInjuries();
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.ClearInjuries, Array.Empty<object>());
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x000161F4 File Offset: 0x000143F4
		public override void Deactivate(bool removeSectors)
		{
			this.ClearSpectators();
			if (!this.player.isHuman)
			{
				ShMountable curMount = this.player.curMount;
				if (curMount)
				{
					this.SvDismount(false);
					curMount.svEntity.Respawn();
				}
				this.SvResetJob();
			}
			base.Deactivate(removeSectors);
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x00016248 File Offset: 0x00014448
		protected bool BombTest(int vaultID)
		{
			ShVault shVault;
			if (EntityCollections.TryFindByID<ShVault>(vaultID, out shVault) && shVault.vaultState == 1 && this.player.HasItem(MonoBehaviourSingleton<ShManager>.Instance.bomb))
			{
				return true;
			}
			this.SendGameMessage("Cannot bomb for some reason");
			return false;
		}

		// Token: 0x06000412 RID: 1042 RVA: 0x00016290 File Offset: 0x00014490
		protected void BombAction(int vaultID)
		{
			ShVault vault;
			if (EntityCollections.TryFindByID<ShVault>(vaultID, out vault))
			{
				SvPlayer.events.Bomb(this.player, vault);
			}
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x000162BC File Offset: 0x000144BC
		public void SvInvite(int otherID)
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(otherID, out shPlayer) && shPlayer.isActiveAndEnabled)
			{
				SvPlayer.events.Invite(this.player, shPlayer);
			}
		}

		// Token: 0x06000414 RID: 1044 RVA: 0x000162F0 File Offset: 0x000144F0
		public void SvKickOut(int otherID)
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(otherID, out shPlayer) && shPlayer.isActiveAndEnabled)
			{
				SvPlayer.events.KickOut(this.player, shPlayer);
			}
		}

		// Token: 0x06000415 RID: 1045 RVA: 0x00016324 File Offset: 0x00014524
		public Place BuyApartment(ShApartment apartment)
		{
			if (!apartment.IsOutside && apartment.Place.EntranceDoor is ShApartment)
			{
				return null;
			}
			Place place = MonoBehaviourSingleton<SceneManager>.Instance.ClonePlace(apartment.svApartment.OtherPlace);
			if (place != null)
			{
				place.owner = this.player;
				this.player.ownedApartments.Add(apartment, place);
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.BuyApartment, new object[]
				{
					apartment.ID,
					place.Index
				});
			}
			return place;
		}

		// Token: 0x06000416 RID: 1046 RVA: 0x000163B4 File Offset: 0x000145B4
		public void SellApartment(Place apartmentPlace)
		{
			ShApartment shApartment = apartmentPlace.EntranceDoor as ShApartment;
			if (shApartment)
			{
				Transform mTransform = apartmentPlace.mTransform;
				List<ShEntity> list = new List<ShEntity>();
				foreach (object obj in mTransform)
				{
					Transform transform = (Transform)obj;
					ShEntity shEntity;
					if (transform && transform.TryGetComponent<ShEntity>(out shEntity) && shEntity != apartmentPlace.mainDoor)
					{
						list.Add(shEntity);
					}
				}
				foreach (ShEntity shEntity2 in list)
				{
					if (shEntity2 && shEntity2.Parent == mTransform)
					{
						ShPlayer shPlayer = shEntity2 as ShPlayer;
						if (shPlayer != null)
						{
							shPlayer.svPlayer.SvEnterDoor(apartmentPlace.mainDoor.ID, null, true);
						}
						else if (shEntity2.svEntity.respawnable)
						{
							shEntity2.svEntity.Respawn();
						}
						else
						{
							shEntity2.SetParent(null);
							shEntity2.Destroy();
						}
					}
				}
				apartmentPlace.owner = null;
				apartmentPlace.passcode = null;
				apartmentPlace.security = 0f;
				this.player.ownedApartments.Remove(shApartment);
				MonoBehaviourSingleton<SceneManager>.Instance.DestroyPlace(apartmentPlace);
			}
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x00016530 File Offset: 0x00014730
		public void SvHelp()
		{
			HashSet<string> hashSet = new HashSet<string>();
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Command command in CommandHandler.Commands)
			{
				if (command.PlayerHasPermission(this.player) && !hashSet.Contains(command.Name))
				{
					hashSet.Add(command.Name);
					stringBuilder.AppendLine(command.GetUsage());
				}
			}
			this.SendTextMenu("&7Commands", stringBuilder.ToString(), "", 0.25f, 0.1f, 0.75f, 0.9f);
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x000165E8 File Offset: 0x000147E8
		public void SvPing()
		{
			this.SendGameMessage("Ping: " + this.connection.PrettyRTT() + " Server FPS: " + Util.PrettyFPS());
		}

		// Token: 0x06000419 RID: 1049 RVA: 0x00016610 File Offset: 0x00014810
		public void SvPingAll()
		{
			if (this.HasPermissionBP(PermEnum.PingAll))
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ShPlayer shPlayer in EntityCollections.Humans)
				{
					stringBuilder.AppendLine(shPlayer.username + ": " + shPlayer.svPlayer.connection.PrettyRTT());
				}
				this.SendTextMenu("&7Pings", stringBuilder.ToString(), "", 0.25f, 0.1f, 0.75f, 0.9f);
			}
		}

		// Token: 0x0600041A RID: 1050 RVA: 0x000166B8 File Offset: 0x000148B8
		public void SvSecurityPanel(int apartmentID)
		{
			ShApartment apartment;
			if (EntityCollections.TryFindByID<ShApartment>(apartmentID, out apartment))
			{
				SvPlayer.events.SecurityPanel(this.player, apartment);
			}
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x000166E4 File Offset: 0x000148E4
		public void SvVideoPanel(int entityID)
		{
			ShEntity entity;
			if (EntityCollections.TryFindByID(entityID, out entity))
			{
				SvPlayer.events.VideoPanel(this.player, entity);
			}
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x0001670D File Offset: 0x0001490D
		public void CursorVisibility(bool setting)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.CursorVisibility, new object[]
			{
				setting
			});
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x00016728 File Offset: 0x00014928
		public void VisualTreeAssetClone(string visualTreeAssetName, string parentVisualElementName = "", string newVisualElementName = "")
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.VisualTreeAssetClone, new object[]
			{
				visualTreeAssetName,
				parentVisualElementName,
				newVisualElementName
			});
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x00016749 File Offset: 0x00014949
		public void VisualElementRemove(string element)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.VisualElementRemove, new object[]
			{
				element
			});
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x00016762 File Offset: 0x00014962
		public void VisualElementOpacity(string element, float setting)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.VisualElementOpacity, new object[]
			{
				element,
				setting
			});
		}

		// Token: 0x06000420 RID: 1056 RVA: 0x00016784 File Offset: 0x00014984
		public void VisualElementDisplay(string element, bool setting)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.VisualElementDisplay, new object[]
			{
				element,
				setting
			});
		}

		// Token: 0x06000421 RID: 1057 RVA: 0x000167A6 File Offset: 0x000149A6
		public void VisualElementVisibility(string element, bool setting)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.VisualElementVisibility, new object[]
			{
				element,
				setting
			});
		}

		// Token: 0x06000422 RID: 1058 RVA: 0x000167C8 File Offset: 0x000149C8
		public void VisualElementOverflow(string element, bool setting)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.VisualElementOverflow, new object[]
			{
				element,
				setting
			});
		}

		// Token: 0x06000423 RID: 1059 RVA: 0x000167EA File Offset: 0x000149EA
		public void AddButtonClickedEvent(string element, string eventName)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.AddButtonClickedEvent, new object[]
			{
				element,
				eventName
			});
		}

		// Token: 0x06000424 RID: 1060 RVA: 0x00016807 File Offset: 0x00014A07
		public void GetTextFieldText(string element, string eventName)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.GetTextFieldText, new object[]
			{
				element,
				eventName
			});
		}

		// Token: 0x06000425 RID: 1061 RVA: 0x00016824 File Offset: 0x00014A24
		public void SetTextElementText(string element, string text)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.SetTextElementText, new object[]
			{
				element,
				text
			});
		}

		// Token: 0x06000426 RID: 1062 RVA: 0x00016841 File Offset: 0x00014A41
		public void GetSliderValue(string element, string eventName)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.GetSliderValue, new object[]
			{
				element,
				eventName
			});
		}

		// Token: 0x06000427 RID: 1063 RVA: 0x0001685E File Offset: 0x00014A5E
		public void SetSliderValue(string element, float value)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.SetSliderValue, new object[]
			{
				element,
				value
			});
		}

		// Token: 0x06000428 RID: 1064 RVA: 0x00016880 File Offset: 0x00014A80
		public void SetProgressBarValue(string element, float value)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.SetProgressBarValue, new object[]
			{
				element,
				value
			});
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x000168A2 File Offset: 0x00014AA2
		public void GetToggleValue(string element, string eventName)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.GetToggleValue, new object[]
			{
				element,
				eventName
			});
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x000168BF File Offset: 0x00014ABF
		public void SetToggleValue(string element, bool value)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.SetToggleValue, new object[]
			{
				element,
				value
			});
		}

		// Token: 0x0600042B RID: 1067 RVA: 0x000168E1 File Offset: 0x00014AE1
		public void GetRadioButtonGroupValue(string element, string eventName)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.GetRadioButtonGroupValue, new object[]
			{
				element,
				eventName
			});
		}

		// Token: 0x0600042C RID: 1068 RVA: 0x000168FE File Offset: 0x00014AFE
		public void SetRadioButtonGroupValue(string element, int value)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.SetRadioButtonGroupValue, new object[]
			{
				element,
				value
			});
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x00016920 File Offset: 0x00014B20
		public void SetRadioButtonGroupChoices(string element, List<string> choices)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.SetRadioButtonGroupChoices, new object[]
			{
				element,
				JsonConvert.SerializeObject(choices)
			});
		}

		// Token: 0x0600042E RID: 1070 RVA: 0x00016942 File Offset: 0x00014B42
		public void GetDropdownFieldValue(string element, string eventName)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.GetDropdownFieldValue, new object[]
			{
				element,
				eventName
			});
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x0001695F File Offset: 0x00014B5F
		public void SetDropdownFieldValue(string element, int value)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.SetDropdownFieldValue, new object[]
			{
				element,
				value
			});
		}

		// Token: 0x06000430 RID: 1072 RVA: 0x00016981 File Offset: 0x00014B81
		public void SetDropdownFieldChoices(string element, List<string> choices)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.SetDropdownFieldChoices, new object[]
			{
				element,
				JsonConvert.SerializeObject(choices)
			});
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x000169A3 File Offset: 0x00014BA3
		public void VisualElementCursorVisibility(string element)
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.VisualElementCursorVisibility, new object[]
			{
				element
			});
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x000169BC File Offset: 0x00014BBC
		public void SendTextMenu(string title, string text, string id = "", float xMin = 0.25f, float yMin = 0.1f, float xMax = 0.75f, float yMax = 0.9f)
		{
			this.svManager.SendTextMenu(this.connection, title, text, id, xMin, yMin, xMax, yMax);
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x000169E8 File Offset: 0x00014BE8
		public void SendOptionMenu(string title, int targetID, string id, LabelID[] options, LabelID[] actions, float xMin = 0.25f, float yMin = 0.1f, float xMax = 0.75f, float yMax = 0.9f)
		{
			this.svManager.SendOptionMenu(this.connection, title, targetID, id, options, actions, xMin, yMin, xMax, yMax);
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x00016A18 File Offset: 0x00014C18
		public void SendInputMenu(string title, int targetID, string id, int characterLimit = 16, float xMin = 0.25f, float yMin = 0.35f, float xMax = 0.75f, float yMax = 0.65f)
		{
			this.svManager.SendInputMenu(this.connection, title, targetID, id, characterLimit, xMin, yMin, xMax, yMax);
		}

		// Token: 0x06000435 RID: 1077 RVA: 0x00016A43 File Offset: 0x00014C43
		public void SendTextPanel(string text, string id = "", LabelID[] options = null, int initialOptionIndex = 0)
		{
			this.svManager.SendTextPanel(this.connection, text, id, options, initialOptionIndex);
		}

		// Token: 0x06000436 RID: 1078 RVA: 0x00016A5B File Offset: 0x00014C5B
		public void SendTimer(float timeout, string id = "")
		{
			this.svManager.SendTimer(this.connection, timeout, id);
		}

		// Token: 0x06000437 RID: 1079 RVA: 0x00016A70 File Offset: 0x00014C70
		public void SendText(string text, float timeout, string id = "")
		{
			this.svManager.SendText(this.connection, text, timeout, id);
		}

		// Token: 0x06000438 RID: 1080 RVA: 0x00016A86 File Offset: 0x00014C86
		public void DestroyMenu(string id = "Default")
		{
			this.svManager.DestroyMenu(this.connection, id);
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x00016A9A File Offset: 0x00014C9A
		public void DestroyTextPanel(string id = "")
		{
			this.svManager.DestroyTextPanel(this.connection, id);
		}

		// Token: 0x0600043A RID: 1082 RVA: 0x00016AAE File Offset: 0x00014CAE
		public void DestroyText(string id = "")
		{
			this.svManager.DestroyText(this.connection, id);
		}

		// Token: 0x0600043B RID: 1083 RVA: 0x00016AC4 File Offset: 0x00014CC4
		public void StartHackingMenu(string title, int targetID, string id, string optionID, float difficulty)
		{
			this.minigame = new HackingGame(this.player, targetID, id, optionID, difficulty);
			this.minigame.StartServerside();
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.HackingMenu, new object[]
			{
				title,
				targetID,
				id,
				optionID,
				difficulty
			});
		}

		// Token: 0x0600043C RID: 1084 RVA: 0x00016B24 File Offset: 0x00014D24
		public void StartCrackingMenu(string title, int targetID, string id, string optionID, float difficulty)
		{
			this.minigame = new CrackingGame(this.player, targetID, id, optionID, difficulty);
			this.minigame.StartServerside();
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.CrackingMenu, new object[]
			{
				title,
				targetID,
				id,
				optionID,
				difficulty
			});
		}

		// Token: 0x0600043D RID: 1085 RVA: 0x00016B83 File Offset: 0x00014D83
		public void SvSaveAll()
		{
			if (this.HasPermissionBP(PermEnum.Save))
			{
				this.svManager.SaveAll();
			}
		}

		// Token: 0x0600043E RID: 1086 RVA: 0x00016B99 File Offset: 0x00014D99
		public void SvFire(Quaternion rotation, int mountableIndex)
		{
			ShPlayer shPlayer = this.player;
			shPlayer.fireIndex += 1;
			this.player.RotationT.rotation = rotation;
			this.player.Fire(mountableIndex);
		}

		// Token: 0x0600043F RID: 1087 RVA: 0x00016BCC File Offset: 0x00014DCC
		public void SvTimeScale(float timeScale)
		{
			if (this.HasPermissionBP(PermEnum.TimeScale))
			{
				timeScale = Mathf.Clamp(timeScale, 0f, 2f);
				this.svManager.SvSetTimeScale(timeScale);
			}
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x00016BF8 File Offset: 0x00014DF8
		public void SvStartVote(int voteIndex, int ID)
		{
			if (this.svManager.vote == null && this.HasPermissionBP(PermEnum.VoteStart) && voteIndex < MonoBehaviourSingleton<ShManager>.Instance.votes.Length && !this.svManager.startedVote.Limit(this.player, true))
			{
				this.svManager.vote = MonoBehaviourSingleton<ShManager>.Instance.votes[voteIndex];
				if (this.svManager.vote.CheckVote(ID))
				{
					InterfaceHandler.SendGameMessageToAll(this.player.username + " started a vote");
					base.Send(SvSendType.All, PacketFlags.Reliable, ClPacket.StartVote, new object[]
					{
						voteIndex,
						ID
					});
					this.svManager.StartCoroutine(this.svManager.StartVote());
					return;
				}
				this.svManager.vote = null;
			}
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x00016CDC File Offset: 0x00014EDC
		public void SvVoteYes()
		{
			if (this.svManager.vote != null && !this.svManager.votedYes.Contains(this.player))
			{
				this.svManager.votedYes.Add(this.player);
				base.Send(SvSendType.All, PacketFlags.Reliable, ClPacket.VoteUpdate, new object[]
				{
					this.svManager.votedYes.Count,
					this.svManager.votedNo.Count
				});
			}
		}

		// Token: 0x06000442 RID: 1090 RVA: 0x00016D68 File Offset: 0x00014F68
		public void SvVoteNo()
		{
			if (this.svManager.vote != null && !this.svManager.votedNo.Contains(this.player))
			{
				this.svManager.votedNo.Add(this.player);
				base.Send(SvSendType.All, PacketFlags.Reliable, ClPacket.VoteUpdate, new object[]
				{
					this.svManager.votedYes.Count,
					this.svManager.votedNo.Count
				});
			}
		}

		// Token: 0x06000443 RID: 1091 RVA: 0x00016DF4 File Offset: 0x00014FF4
		public void SvProgressAction(int entityID, int progressIndex)
		{
			ShEntity e;
			if (this.currentProgress == null && EntityCollections.TryFindByID(entityID, out e))
			{
				progressIndex = Mathf.Clamp(progressIndex, 0, this.progressHandler.Count - 1);
				base.StartCoroutine(this.ProgressLoop(e, this.progressHandler[progressIndex]));
			}
		}

		// Token: 0x06000444 RID: 1092 RVA: 0x00016E44 File Offset: 0x00015044
		public void SvProcessAction(int entityID, int processIndex)
		{
			ShProcessor processor;
			if (this.currentProgress == null && EntityCollections.TryFindByID<ShProcessor>(entityID, out processor))
			{
				processIndex = Mathf.Clamp(processIndex, 0, processor.processOptions.Length - 1);
				string equipableName = processor.processOptions[processIndex].equipableName;
				ProgressAction progress = new ProgressAction(string.IsNullOrWhiteSpace(equipableName) ? this.player.Hands.name : equipableName, (int x) => processor.ProcessTest(this.player, processIndex), delegate(int x)
				{
					processor.ProcessAction(this.player, processIndex);
				});
				base.StartCoroutine(this.ProgressLoop(processor, progress));
			}
		}

		// Token: 0x06000445 RID: 1093 RVA: 0x00016F0B File Offset: 0x0001510B
		public void SvProgressBar(float normalizedTime, float normalizedSpeed, string id = "")
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.StartProgress, new object[]
			{
				id,
				normalizedTime,
				normalizedSpeed
			});
		}

		// Token: 0x06000446 RID: 1094 RVA: 0x00016F33 File Offset: 0x00015133
		public void SvProgressStop(string id = "")
		{
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.StopProgress, new object[]
			{
				id
			});
		}

		// Token: 0x06000447 RID: 1095 RVA: 0x00016F49 File Offset: 0x00015149
		protected IEnumerator ProgressLoop(ShEntity e, ProgressAction progress)
		{
			int progressEquipableIndex = progress.equipableName.GetPrefabIndex();
			if (this.SvTrySetEquipable(progressEquipableIndex, false))
			{
				this.currentProgress = progress;
				float processDuration = e.ProcessDuration;
				float endTime = Time.time + processDuration;
				this.SvProgressBar(0f, 1f / processDuration, "");
				base.Send(SvSendType.Local, PacketFlags.Reliable, ClPacket.AnimatorBool, new object[]
				{
					this.player.ID,
					Animations.gesture,
					true
				});
				while (e && this.player.IsUp && this.player.InActionRange(e))
				{
					if (Time.time < endTime)
					{
						if (this.player.curEquipable.index != progressEquipableIndex)
						{
							break;
						}
						if (!this.currentProgress.TestMethod(e.ID))
						{
							break;
						}
						yield return null;
					}
					else
					{
						if (this.currentProgress.TestMethod(e.ID))
						{
							this.currentProgress.ActionMethod(e.ID);
							break;
						}
						break;
					}
				}
				this.SvProgressStop("");
				base.Send(SvSendType.Local, PacketFlags.Reliable, ClPacket.AnimatorBool, new object[]
				{
					this.player.ID,
					Animations.gesture,
					false
				});
				this.currentProgress = null;
			}
			yield break;
		}

		// Token: 0x06000448 RID: 1096 RVA: 0x00016F68 File Offset: 0x00015168
		public void SvSetSiren(bool setting)
		{
			ShTransport shTransport;
			if (this.player.IsControlledMount<ShTransport>(out shTransport) && shTransport.siren != setting)
			{
				shTransport.SetSiren(setting);
				shTransport.svTransport.Send(SvSendType.LocalOthers, PacketFlags.Reliable, ClPacket.SetSiren, new object[]
				{
					shTransport.ID,
					setting
				});
			}
		}

		// Token: 0x06000449 RID: 1097 RVA: 0x00016FC0 File Offset: 0x000151C0
		public void SvLaunch()
		{
			InterfaceHandler.SendGameMessageToAll(this.player.displayName + " launched");
			this.player.svPlayer.SvDismount(true);
			this.player.svPlayer.SvForce(100f * this.player.positionRB.mass * Vector3.up);
		}

		// Token: 0x0600044A RID: 1098 RVA: 0x00017028 File Offset: 0x00015228
		public bool HasPermission(string permission)
		{
			using (Dictionary<string, Group>.ValueCollection.Enumerator enumerator = GroupHandler.Groups.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.HasPermission(this.player, permission, false))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600044B RID: 1099 RVA: 0x00017090 File Offset: 0x00015290
		public bool HasPermissionBP(PermEnum p)
		{
			return this.HasPermission("bp." + Enum.GetName(typeof(PermEnum), p));
		}

		// Token: 0x0600044C RID: 1100 RVA: 0x000170B7 File Offset: 0x000152B7
		public byte HasPermissionByte(PermEnum p)
		{
			if (!this.HasPermissionBP(p))
			{
				return 0;
			}
			return 1;
		}

		// Token: 0x0600044D RID: 1101 RVA: 0x000170C5 File Offset: 0x000152C5
		public void SvKick(ShPlayer other, string reason)
		{
			if (this.HasPermissionBP(PermEnum.Kick))
			{
				if (!other || !other.isHuman)
				{
					this.SendGameMessage("Cannot kick NPC");
					return;
				}
				SvPlayer.events.Kick(this.player, other, reason);
			}
		}

		// Token: 0x0600044E RID: 1102 RVA: 0x000170FF File Offset: 0x000152FF
		public void SvBan(ShPlayer other, string reason)
		{
			if (this.HasPermissionBP(PermEnum.Ban))
			{
				if (!other || !other.isHuman)
				{
					this.SendGameMessage("Cannot ban NPC");
					return;
				}
				SvPlayer.events.Ban(this.player, other, reason);
			}
		}

		// Token: 0x0600044F RID: 1103 RVA: 0x00017139 File Offset: 0x00015339
		public void SvPlayerRecords()
		{
			if (this.HasPermissionBP(PermEnum.PlayerRecords))
			{
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.PlayerRecords, new object[]
				{
					JsonConvert.SerializeObject(this.svManager.playerRecords.ToArray())
				});
				this.SendServerTime();
			}
		}

		// Token: 0x06000450 RID: 1104 RVA: 0x00017172 File Offset: 0x00015372
		public void SvBanRecords()
		{
			if (this.HasPermissionBP(PermEnum.BanRecords))
			{
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.BanRecords, new object[]
				{
					JsonConvert.SerializeObject(this.svManager.database.Bans.FindAll())
				});
				this.SendServerTime();
			}
		}

		// Token: 0x06000451 RID: 1105 RVA: 0x000171B0 File Offset: 0x000153B0
		public void SendServerTime()
		{
			this.SendGameMessage("Server Time: " + Util.FormattedTime);
		}

		// Token: 0x06000452 RID: 1106 RVA: 0x000171C7 File Offset: 0x000153C7
		public void SvBanAccount(string accountID, string reason)
		{
			if (this.HasPermissionBP(PermEnum.BanAccount))
			{
				this.SvBanDatabase(accountID, reason);
			}
		}

		// Token: 0x06000453 RID: 1107 RVA: 0x000171DC File Offset: 0x000153DC
		public void SvDeleteAccount(string accountID)
		{
			if (this.HasPermissionBP(PermEnum.DeleteAccount))
			{
				if (this.svManager.database.Users.Delete(accountID))
				{
					this.SendGameMessage("Deleted Account: " + accountID);
					return;
				}
				this.SendGameMessage("Account Not Found: " + accountID);
			}
		}

		// Token: 0x06000454 RID: 1108 RVA: 0x00017234 File Offset: 0x00015434
		public void SvBanDatabase(string accountID, string reason)
		{
			User user;
			if (this.svManager.TryGetUserData(accountID, out user))
			{
				Ban ban = new Ban
				{
					IP = user.IP,
					Username = user.ID,
					Reason = reason,
					Time = Util.CurrentTime
				};
				if (this.svManager.database.Bans.Upsert(ban))
				{
					base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.BanState, new object[]
					{
						ban.IP,
						true
					});
				}
			}
		}

		// Token: 0x06000455 RID: 1109 RVA: 0x000172BC File Offset: 0x000154BC
		public void SvUnbanIP(string IP)
		{
			if (this.HasPermissionBP(PermEnum.UnbanIP) && this.svManager.database.Bans.Delete(IP))
			{
				base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.BanState, new object[]
				{
					IP,
					false
				});
			}
		}

		// Token: 0x06000456 RID: 1110 RVA: 0x0001730D File Offset: 0x0001550D
		public void SvRestrain(ShPlayer other)
		{
			if (this.HasPermissionBP(PermEnum.Restrain) && other && other.isActiveAndEnabled)
			{
				other.svPlayer.Restrain(this.player, other.Handcuffs.restrained);
			}
		}

		// Token: 0x06000457 RID: 1111 RVA: 0x00017345 File Offset: 0x00015545
		public override void SvRestore(Vector3 position, Quaternion rotation, int placeIndex)
		{
			if (!this.player.SpecSelf)
			{
				this.SvSpectate(this.player);
			}
			base.SvRestore(position, rotation, placeIndex);
		}

		// Token: 0x06000458 RID: 1112 RVA: 0x00017369 File Offset: 0x00015569
		public void SvTeleport(ShPlayer other)
		{
			if (this.HasPermissionBP(PermEnum.Teleport) && other && other.isActiveAndEnabled)
			{
				this.SvRestore(other.Position, other.Rotation, other.GetPlaceIndex());
			}
		}

		// Token: 0x06000459 RID: 1113 RVA: 0x000173A0 File Offset: 0x000155A0
		public void SvSummon(ShPlayer other)
		{
			if (this.HasPermissionBP(PermEnum.Summon) && other && other.isActiveAndEnabled)
			{
				other.svPlayer.SvRestore(this.player.Position, this.player.Rotation, this.player.GetPlaceIndex());
			}
		}

		// Token: 0x0600045A RID: 1114 RVA: 0x000173F3 File Offset: 0x000155F3
		public void SvRemoveJob(ShPlayer other)
		{
			if (this.HasPermissionBP(PermEnum.RemoveJob) && other && other.isActiveAndEnabled)
			{
				other.svPlayer.SvResetJob();
			}
		}

		// Token: 0x0600045B RID: 1115 RVA: 0x0001741C File Offset: 0x0001561C
		public void SvHeal(ShPlayer other)
		{
			if (this.HasPermissionBP(PermEnum.Heal) && other && other.isActiveAndEnabled)
			{
				other.svPlayer.HealFull(null);
				other.svPlayer.UpdateStatsDelta(1f, 1f, 1f);
				other.svPlayer.SvClearInjuries();
			}
		}

		// Token: 0x0600045C RID: 1116 RVA: 0x00017475 File Offset: 0x00015675
		public void SvSpectateCommand(ShPlayer other)
		{
			if (this.HasPermissionBP(PermEnum.Spectate) && other && other.isActiveAndEnabled)
			{
				this.SvSpectate(other);
			}
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x00017498 File Offset: 0x00015698
		public void SvStopServer()
		{
			if (this.HasPermissionBP(PermEnum.StopServer))
			{
				this.svManager.ForceQuit(null, false);
			}
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x000174B1 File Offset: 0x000156B1
		public void SvGodMode()
		{
			if (this.HasPermissionBP(PermEnum.GodMode))
			{
				this.godMode = !this.godMode;
				this.SendGameMessage("God Mode = " + this.godMode.ToString());
			}
		}

		// Token: 0x0600045F RID: 1119 RVA: 0x000174E8 File Offset: 0x000156E8
		public void SvMaxSpeed(float maxSpeed)
		{
			if (this.HasPermissionBP(PermEnum.MaxSpeed))
			{
				ShMovable controlled = this.player.GetControlled();
				controlled.svMovable.SetMaxSpeed(maxSpeed);
				this.SendGameMessage(string.Format("{0} Max Speed = {1}", controlled.name, maxSpeed));
			}
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x00017533 File Offset: 0x00015733
		public void SvTimeLeft()
		{
			if (this.HasPermissionBP(PermEnum.TimeLeft))
			{
				this.SendGameMessage("Time left: " + (this.svManager.settings.timeLimit - Time.realtimeSinceStartup).TimeStringFromSeconds());
			}
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x0001756A File Offset: 0x0001576A
		public void SvDisarm(ShExplosion explosion)
		{
			if (explosion && explosion.armed && this.player.InActionRange(explosion))
			{
				explosion.Disarm();
			}
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x00017590 File Offset: 0x00015790
		public override void WriteInitData(ref object[] initializeArgs, bool isPlayer)
		{
			if (initializeArgs == null)
			{
				initializeArgs = new object[12];
			}
			base.WriteInitData(ref initializeArgs, isPlayer);
			initializeArgs[5] = this.player.isHuman;
			initializeArgs[6] = isPlayer;
			initializeArgs[7] = this.player.username;
			initializeArgs[8] = this.player.displayName;
			initializeArgs[9] = this.job.info.shared.jobIndex;
			initializeArgs[10] = this.player.profile;
			initializeArgs[11] = ((this.player.language != null) ? this.player.language.index : 0);
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x00017648 File Offset: 0x00015848
		public void Save()
		{
			User user = new User
			{
				ID = this.player.username,
				IP = this.connection.IP,
				PasswordHash = this.connectData.passwordHash,
				LastUpdated = Util.CurrentTime
			};
			user.Character.CustomData = base.CustomData;
			if (this.player.Place.IsClone)
			{
				Transform spawnPoint = this.player.Place.EntranceDoor.spawnPoint;
				user.Character.Position = spawnPoint.position;
				user.Character.Rotation = spawnPoint.rotation;
				user.Character.PlaceIndex = this.player.Place.EntranceDoor.GetPlaceIndex();
			}
			else
			{
				user.Character.Position = this.player.Position;
				user.Character.Rotation = this.player.Rotation;
				user.Character.PlaceIndex = this.player.GetPlaceIndex();
			}
			user.Character.Job.JobIndex = this.job.info.shared.jobIndex;
			user.Character.Job.Rank = this.player.rank;
			user.Character.Job.Experience = this.player.experience;
			user.Character.Items = this.player.myItems.ToDictionary((KeyValuePair<int, InventoryItem> x) => x.Key, (KeyValuePair<int, InventoryItem> y) => y.Value.count);
			user.Character.Bindings.Clear();
			foreach (Binding binding in this.player.bindings)
			{
				user.Character.Bindings.Add(new BindingSave(binding));
			}
			user.Character.EquipableIndex = this.player.curEquipable.index;
			user.Character.Attachments = new List<int>
			{
				this.player.curEquipable.curMuzzle.index,
				this.player.curEquipable.curSight.index,
				this.player.curEquipable.curUnderbarrel.index
			};
			user.Character.Wearables = (from x in this.player.curWearables
			select x.index).ToList<int>();
			user.Character.SkinIndex = this.player.index;
			user.Character.MapName = this.svManager.settings.map;
			user.Character.Injuries.Clear();
			foreach (Injury injury in this.player.injuries)
			{
				user.Character.Injuries.Add(new InjurySave(injury.part, injury.effect, injury.amount));
			}
			user.Character.BankBalance = this.bankBalance;
			user.Character.AppTransactions = JsonConvert.SerializeObject(this.appTransactions);
			user.Character.Health = this.player.health;
			user.Character.Stats = this.player.stats.ToList<float>();
			user.Character.KnockedOut = this.player.IsKnockedOut;
			user.Profile = this.player.profile;
			user.Persistent.AppContacts = this.appContacts.ToList<string>();
			user.Persistent.AppBlocked = this.appBlocked.ToList<string>();
			user.Persistent.AppCalls = JsonConvert.SerializeObject(this.appCalls);
			user.Persistent.AppMessages = JsonConvert.SerializeObject(this.appMessages);
			user.Persistent.LastMessengers = this.lastMessengers;
			user.Persistent.UnreadMessages = this.unreadMessages;
			user.Character.Apartments.Clear();
			foreach (KeyValuePair<ShApartment, Place> keyValuePair in this.player.ownedApartments)
			{
				ApartmentSave apartmentSave = new ApartmentSave(keyValuePair.Key.svApartment.OtherPlaceIndex, keyValuePair.Key.value, keyValuePair.Value.security, keyValuePair.Value.passcode);
				user.Character.Apartments.Add(apartmentSave);
				apartmentSave.Entities = new List<ApartmentEntity>();
				apartmentSave.Transports = new List<ApartmentTransport>();
				foreach (object obj in keyValuePair.Value.mTransform)
				{
					Transform transform = (Transform)obj;
					ShEntity shEntity;
					if (transform.TryGetComponent<ShEntity>(out shEntity) && !(shEntity is ShDoor) && !(shEntity is ShPlayer) && shEntity.svEntity.destroyAfter == 0f)
					{
						ShTransport shTransport = shEntity as ShTransport;
						if (shTransport != null)
						{
							ApartmentTransport apartmentTransport = new ApartmentTransport(shEntity.index, transform.localPosition, transform.localRotation, shEntity.svEntity.CustomData);
							if (shEntity.HasInventory)
							{
								apartmentTransport.Inventory = shEntity.myItems.ToDictionary((KeyValuePair<int, InventoryItem> x) => x.Key, (KeyValuePair<int, InventoryItem> y) => y.Value.count);
							}
							apartmentTransport.Health = shTransport.health;
							apartmentTransport.Owned = shTransport.owner;
							apartmentSave.Transports.Add(apartmentTransport);
						}
						else
						{
							ApartmentEntity apartmentEntity = new ApartmentEntity(shEntity.index, transform.localPosition, transform.localRotation, shEntity.svEntity.CustomData);
							if (shEntity.HasInventory)
							{
								apartmentEntity.Inventory = shEntity.myItems.ToDictionary((KeyValuePair<int, InventoryItem> x) => x.Key, (KeyValuePair<int, InventoryItem> y) => y.Value.count);
							}
							apartmentSave.Entities.Add(apartmentEntity);
						}
					}
				}
			}
			SvPlayer.events.Save(this.player);
			this.svManager.database.Users.Upsert(user);
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x00017DAC File Offset: 0x00015FAC
		protected byte[] SerializeAttachments()
		{
			if (this.attachmentArray == null)
			{
				this.attachmentArray = new byte[13];
			}
			this.attachmentArray[0] = (this.player.curEquipable.curUnderbarrel.setting ? 1 : 0);
			int num = 1;
			MyConverter.GetBytes(this.player.curEquipable.curMuzzle.index).CopyTo(this.attachmentArray, num);
			num += 4;
			MyConverter.GetBytes(this.player.curEquipable.curSight.index).CopyTo(this.attachmentArray, num);
			num += 4;
			MyConverter.GetBytes(this.player.curEquipable.curUnderbarrel.index).CopyTo(this.attachmentArray, num);
			return this.attachmentArray;
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x00017E74 File Offset: 0x00016074
		protected byte[] SerializeWearables()
		{
			if (this.wearableArray == null)
			{
				this.wearableArray = new byte[this.player.curWearables.Length * 4];
			}
			int num = 0;
			ShWearable[] curWearables = this.player.curWearables;
			for (int i = 0; i < curWearables.Length; i++)
			{
				MyConverter.GetBytes(curWearables[i].index).CopyTo(this.wearableArray, num);
				num += 4;
			}
			return this.wearableArray;
		}

		// Token: 0x06000466 RID: 1126 RVA: 0x00017EE4 File Offset: 0x000160E4
		protected byte[] SerializeHealth()
		{
			byte[] array = new byte[this.player.stats.Length * 4 + this.player.injuries.Count * 3];
			int num = 0;
			float[] stats = this.player.stats;
			for (int i = 0; i < stats.Length; i++)
			{
				MyConverter.GetBytes(stats[i]).CopyTo(array, num);
				num += 4;
			}
			foreach (Injury injury in this.player.injuries)
			{
				array[num] = (byte)injury.part;
				num++;
				array[num] = (byte)injury.effect;
				num++;
				array[num] = injury.amount;
				num++;
			}
			return array;
		}

		// Token: 0x06000467 RID: 1127 RVA: 0x00017FBC File Offset: 0x000161BC
		public void RecordPlayer()
		{
			this.svManager.playerRecords.Enqueue(new PlayerRecord(this.player.username, this.connection.IP, Util.CurrentTime));
			if (this.svManager.playerRecords.Count > 100)
			{
				this.svManager.playerRecords.Dequeue();
			}
			this.Save();
		}

		// Token: 0x06000468 RID: 1128 RVA: 0x00018024 File Offset: 0x00016224
		public override void Respawn()
		{
			if (!this.player.SpecSelf)
			{
				this.SvSpectate(this.player);
				return;
			}
			base.Respawn();
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x00018048 File Offset: 0x00016248
		public void ApplyPlayerData(User PlayerData)
		{
			this.bankBalance = PlayerData.Character.BankBalance;
			if (PlayerData.Character.AppTransactions != null)
			{
				List<AppTransaction> list = JsonConvert.DeserializeObject<List<AppTransaction>>(PlayerData.Character.AppTransactions);
				if (list != null)
				{
					this.appTransactions = list;
				}
			}
			if (this.svManager.settings.map == PlayerData.Character.MapName)
			{
				IDCollection<ShPlayer> subscribedPlayers = new IDCollection<ShPlayer>();
				using (List<ApartmentSave>.Enumerator enumerator = PlayerData.Character.Apartments.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ApartmentSave apartmentSave = enumerator.Current;
						ShApartment apartment;
						if (this.svManager.apartments.TryGetValue(apartmentSave.Index, out apartment))
						{
							ShEntity[] array = new ShEntity[apartmentSave.Entities.Count];
							object[][] array2 = new object[apartmentSave.Entities.Count][];
							int num = 0;
							Place place = this.BuyApartment(apartment);
							if (place != null)
							{
								place.security = apartmentSave.Security;
								place.passcode = apartmentSave.Passcode;
								foreach (ApartmentEntity apartmentEntity in apartmentSave.Entities)
								{
									ShEntity prefab;
									if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity(apartmentEntity.Index, out prefab))
									{
										array[num] = this.svManager.AddNewEntity<ShEntity>(prefab, place, apartmentEntity.Position + place.mTransform.position, apartmentEntity.Rotation, subscribedPlayers);
										if (array[num])
										{
											foreach (KeyValuePair<int, int> keyValuePair in apartmentEntity.Inventory)
											{
												ShItem item;
												if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShItem>(keyValuePair.Key, out item))
												{
													array[num].myItems[keyValuePair.Key] = new InventoryItem(item, keyValuePair.Value, 0);
												}
											}
											array[num].svEntity.CustomData = apartmentEntity.CustomData;
											array[num].svEntity.WriteInitData(ref array2[num], false);
											num++;
										}
									}
								}
								Buffers.writer.SeekZero();
								Buffers.writer.WriteClPacket(ClPacket.AddEntityArray);
								for (int i = 0; i < num; i++)
								{
									Buffers.WriteObject(array2[i]);
								}
								base.Send(SvSendType.All, PacketFlags.Reliable);
								for (int j = 0; j < num; j++)
								{
									SvEntity svEntity = array[j].svEntity;
									svEntity.subscribers = null;
									svEntity.NewSector();
								}
								foreach (ApartmentTransport apartmentTransport in apartmentSave.Transports)
								{
									ShTransport prefab2;
									if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShTransport>(apartmentTransport.Index, out prefab2))
									{
										ShTransport shTransport = this.svManager.AddNewEntity<ShTransport>(prefab2, place, apartmentTransport.Position + place.mTransform.position, apartmentTransport.Rotation, false, this.player, false);
										if (shTransport)
										{
											foreach (KeyValuePair<int, int> keyValuePair2 in apartmentTransport.Inventory)
											{
												ShItem item2;
												if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShItem>(keyValuePair2.Key, out item2))
												{
													shTransport.myItems[keyValuePair2.Key] = new InventoryItem(item2, keyValuePair2.Value, 0);
												}
											}
											shTransport.svTransport.CustomData = apartmentTransport.CustomData;
											if (apartmentTransport.Health != shTransport.maxStat)
											{
												shTransport.svTransport.Damage(DamageIndex.Null, shTransport.maxStat - apartmentTransport.Health, null, null, default(Vector3), default(Vector3));
											}
											if (apartmentTransport.Owned)
											{
												shTransport.svTransport.SvSetTransportOwner(this.player);
											}
										}
									}
								}
							}
						}
					}
					goto IL_55C;
				}
			}
			this.Respawn();
			if (PlayerData.Character.Apartments.Count > 0)
			{
				int num2 = 0;
				foreach (ApartmentSave apartmentSave2 in PlayerData.Character.Apartments)
				{
					num2 += apartmentSave2.PurchasePrice;
					foreach (ApartmentEntity apartmentEntity2 in apartmentSave2.Entities)
					{
						ShEntity shEntity;
						if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity(apartmentEntity2.Index, out shEntity))
						{
							num2 += shEntity.value;
							foreach (KeyValuePair<int, int> keyValuePair3 in apartmentEntity2.Inventory)
							{
								ShItem shItem;
								if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShItem>(keyValuePair3.Key, out shItem))
								{
									num2 += shItem.value * keyValuePair3.Value;
								}
							}
						}
					}
				}
				this.bankBalance += num2;
				this.SendGameMessage("New map: Received $" + num2.ToString() + " from old apartments");
			}
			IL_55C:
			this.SvTrySetJob(PlayerData.Character.Job.JobIndex, false, false);
			this.SetRank(PlayerData.Character.Job.Rank);
			this.SetExperience(PlayerData.Character.Job.Experience, false);
			foreach (KeyValuePair<int, int> keyValuePair4 in PlayerData.Character.Items)
			{
				ShItem item3;
				if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShItem>(keyValuePair4.Key, out item3))
				{
					this.player.myItems[keyValuePair4.Key] = new InventoryItem(item3, keyValuePair4.Value, 0);
					this.player.CheckEquipment(keyValuePair4.Key, keyValuePair4.Value);
				}
			}
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.SerializedItems, new object[]
			{
				this.player.SerializeMyItems()
			});
			foreach (int wearable in PlayerData.Character.Wearables)
			{
				this.player.SetWearable(wearable);
			}
			base.Send(SvSendType.Local, PacketFlags.Reliable, ClPacket.SerializedWearables, new object[]
			{
				this.player.ID,
				this.SerializeWearables()
			});
			byte b = 0;
			while ((int)b < PlayerData.Character.Bindings.Count)
			{
				BindingSave bindingSave = PlayerData.Character.Bindings[(int)b];
				this.player.Bind(bindingSave.Equipable, b);
				foreach (int attachmentIndex in bindingSave.Attachments)
				{
					this.SvBindAttachment(attachmentIndex, b);
				}
				b += 1;
			}
			this.SvForceEquipable(PlayerData.Character.EquipableIndex);
			foreach (ShAttachment a in MonoBehaviourSingleton<ShManager>.Instance.nullAttachment)
			{
				this.player.SetAttachment(a, UnderbarrelSetting.Default);
			}
			foreach (int attachmentIndex2 in PlayerData.Character.Attachments)
			{
				this.player.SetAttachment(attachmentIndex2, UnderbarrelSetting.Default);
			}
			base.Send(SvSendType.Local, PacketFlags.Reliable, ClPacket.SerializedAttachments, new object[]
			{
				this.player.ID,
				this.SerializeAttachments()
			});
			for (int l = 0; l < this.player.stats.Length; l++)
			{
				this.player.stats[l] = ((l < PlayerData.Character.Stats.Count) ? PlayerData.Character.Stats[l] : 1f);
			}
			if (PlayerData.Character.Injuries.Count > 0)
			{
				foreach (InjurySave injurySave in PlayerData.Character.Injuries)
				{
					this.player.injuries.Add(new Injury(injurySave.Part, injurySave.Effect, injurySave.Amount));
				}
				this.player.UpdateInjuries();
			}
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.SerializedHealth, new object[]
			{
				this.SerializeHealth()
			});
			float num3 = this.player.maxStat - PlayerData.Character.Health;
			if (num3 > 0f)
			{
				base.Damage(DamageIndex.Null, num3, null, null, default(Vector3), default(Vector3));
			}
			if (PlayerData.Character.KnockedOut && !this.player.IsKnockedOut)
			{
				this.SvForceStance(StanceIndex.KnockedOut);
			}
			this.LoadPersistentData(PlayerData.Persistent);
			SvPlayer.events.Load(this.player);
			this.RecordPlayer();
		}

		// Token: 0x0600046A RID: 1130 RVA: 0x00018AFC File Offset: 0x00016CFC
		public void LoadPersistentData(Persistent persistent)
		{
			this.appContacts = new HashSet<string>(persistent.AppContacts);
			this.appBlocked = new HashSet<string>(persistent.AppBlocked);
			if (persistent.AppMessages != null)
			{
				Dictionary<string, AppMessages> dictionary = JsonConvert.DeserializeObject<Dictionary<string, AppMessages>>(persistent.AppMessages);
				if (dictionary != null)
				{
					this.appMessages = dictionary;
				}
			}
			if (persistent.AppCalls != null)
			{
				List<AppCall> list = JsonConvert.DeserializeObject<List<AppCall>>(persistent.AppCalls);
				if (list != null)
				{
					this.appCalls = list;
				}
			}
			this.lastMessengers = persistent.LastMessengers;
			this.unreadMessages = persistent.UnreadMessages;
			base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.UnreadMessages, new object[]
			{
				this.unreadMessages
			});
		}

		// Token: 0x0600046B RID: 1131 RVA: 0x00018BA0 File Offset: 0x00016DA0
		protected override void SendToSelf(PacketFlags channel)
		{
			base.SendToPlayer(this.player, channel);
		}

		// Token: 0x0600046C RID: 1132 RVA: 0x00018BAF File Offset: 0x00016DAF
		protected override void SendToLocal(PacketFlags channel)
		{
			base.SendToPlayer(this.player, channel);
			base.SendToLocalOthers(channel);
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x00018BC8 File Offset: 0x00016DC8
		public bool HealFromConsumable(ShConsumable consumable)
		{
			bool result = false;
			foreach (BodyEffect bodyEffect in consumable.healedEffects)
			{
				for (int j = this.player.injuries.Count - 1; j >= 0; j--)
				{
					if (this.player.injuries[j].effect == bodyEffect)
					{
						this.player.injuries.RemoveAt(j);
						base.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.RemoveInjury, new object[]
						{
							j
						});
						result = true;
					}
				}
			}
			return result;
		}

		// Token: 0x0600046E RID: 1134 RVA: 0x00018C5A File Offset: 0x00016E5A
		public bool SvConsume(ShConsumable consumable, ShPlayer healer = null)
		{
			return SvPlayer.events.Consume(this.player, consumable, healer);
		}

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x0600046F RID: 1135 RVA: 0x00018C6E File Offset: 0x00016E6E
		public Vector3 PredictedTarget
		{
			get
			{
				return this.player.GetMount().GetWeaponPosition((int)this.player.seat) + this.aim * this.player.Distance(this.targetEntity);
			}
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x00018CAC File Offset: 0x00016EAC
		public bool AimSmart()
		{
			Vector3 deltaP = this.targetEntity.Origin + Random.insideUnitSphere * 4f - this.player.ActiveWeapon.GetWeaponPosition((int)this.player.seat);
			Vector3 deltaV = this.targetEntity.Velocity - this.player.Velocity;
			float weaponVelocity = this.player.ActiveWeapon.GetWeaponVelocity((int)this.player.seat);
			float weaponGravity = this.player.ActiveWeapon.GetWeaponGravity((int)this.player.seat);
			if (Util.AimVector(deltaP, deltaV, weaponVelocity, weaponGravity, false, out this.aim))
			{
				this.aimFrame = Time.frameCount;
				this.LookAt(this.aim);
				return true;
			}
			this.aim = (this.targetEntity.Origin - this.player.Origin).normalized;
			this.LookAt(this.aim);
			return false;
		}

		// Token: 0x06000471 RID: 1137 RVA: 0x00018DAC File Offset: 0x00016FAC
		public void LookAt(Vector3 direction)
		{
			this.LookAt(direction.SafeLookRotation(this.player.mainT.up));
		}

		// Token: 0x06000472 RID: 1138 RVA: 0x00018DCC File Offset: 0x00016FCC
		public void LookAt(Quaternion direction)
		{
			if (this.player.IsUp)
			{
				this.player.RotationT.rotation = Quaternion.RotateTowards(this.player.Rotation, direction, 18f);
				this.player.RotationT.LimitEuler(this.player.ViewAngleLimit);
			}
		}

		// Token: 0x06000473 RID: 1139 RVA: 0x00018E28 File Offset: 0x00017028
		public void FireLogic()
		{
			ShMountable activeWeapon = this.player.ActiveWeapon;
			if (activeWeapon.GetCurrentClip((int)this.player.seat) == 0)
			{
				if (activeWeapon.CanReload)
				{
					activeWeapon.Reload();
					return;
				}
			}
			else if (this.aimFrame == Time.frameCount && activeWeapon.CanUse((int)this.player.seat) && this.player.Perlin(0.5f) < 0.5f && Vector3.Angle(activeWeapon.GetWeaponForward((int)this.player.seat), this.aim) < 25f && activeWeapon.GetWeaponRecoil() < 16f / this.player.DistanceSqr(this.targetEntity))
			{
				this.player.Fire(activeWeapon.index);
			}
		}

		// Token: 0x06000474 RID: 1140 RVA: 0x00018EF0 File Offset: 0x000170F0
		public void ResetAI()
		{
			SvPlayer.events.ResetAI(this.player);
		}

		// Token: 0x06000475 RID: 1141 RVA: 0x00018F04 File Offset: 0x00017104
		public bool SetState(int index)
		{
			State state = this.states[index];
			if (state.EnterTest())
			{
				this.currentState.ExitState(state);
				this.currentState = state;
				this.currentState.EnterState();
				return true;
			}
			return false;
		}

		// Token: 0x06000476 RID: 1142 RVA: 0x00018F43 File Offset: 0x00017143
		public IEnumerator RunState()
		{
			WaitForSeconds delay = new WaitForSeconds(0.1f);
			yield return null;
			while (!this.player.IsDead)
			{
				if (this.player.IsUp)
				{
					this.currentState.UpdateState();
				}
				yield return delay;
			}
			yield break;
		}

		// Token: 0x06000477 RID: 1143 RVA: 0x00018F52 File Offset: 0x00017152
		public void ClearLeader()
		{
			if (this.leader)
			{
				this.leader.svPlayer.follower = null;
				this.leader = null;
				this.targetEntity = null;
			}
		}

		// Token: 0x06000478 RID: 1144 RVA: 0x00018F80 File Offset: 0x00017180
		public void SvFollower(int otherID)
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(otherID, out shPlayer) && shPlayer.isActiveAndEnabled)
			{
				SvPlayer.events.Follower(this.player, shPlayer);
			}
		}

		// Token: 0x06000479 RID: 1145 RVA: 0x00018FB1 File Offset: 0x000171B1
		public void SpawnBot(Vector3 position, Quaternion rotation, Place place, Waypoint waypoint, ShPlayer spawner, ShPlayer enemy)
		{
			this.nextWaypoint = waypoint;
			this.onWaypoints = (waypoint != null);
			this.spawner = spawner;
			this.spawnTarget = enemy;
			this.player.Spawn(position, rotation, place.mTransform);
		}

		// Token: 0x0600047A RID: 1146 RVA: 0x00018FEC File Offset: 0x000171EC
		public void UpdateNextWaypoint()
		{
			this.nextWaypoint = this.nextWaypoint.RandomNeighbor;
		}

		// Token: 0x0600047B RID: 1147 RVA: 0x00019000 File Offset: 0x00017200
		public void UpdateNextNode()
		{
			List<GraphNode> list = new List<GraphNode>();
			this.currentNode.GetConnections(new Action<GraphNode>(list.Add), 32);
			if (list.Count > 0)
			{
				this.SetCurrentNode(list.GetRandom<GraphNode>());
				return;
			}
			this.ResetAI();
		}

		// Token: 0x0600047C RID: 1148 RVA: 0x0001904C File Offset: 0x0001724C
		public override void MoveTo(Vector3 next)
		{
			if (this.player.IsMobile)
			{
				Vector3 vector = this.player.mainT.InverseTransformPoint(next);
				this.player.TrySetInput(Mathf.Clamp(vector.z + 2f * Mathf.Max(0f, vector.y), -1f, 1f), 0f, Mathf.Clamp(-vector.x, -1f, 1f));
				if (this.player.climbState == ClimbState.None)
				{
					float magnitude = vector.magnitude;
					if (magnitude > 1f && (vector / magnitude).y > 0.5f)
					{
						this.SvJump();
					}
				}
			}
		}

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x0600047D RID: 1149 RVA: 0x00019103 File Offset: 0x00017303
		public NodeLink2 NodeLink
		{
			get
			{
				if (this.currentNode == null)
				{
					return null;
				}
				return NodeLink2.GetNodeLink(this.currentNode);
			}
		}

		// Token: 0x0600047E RID: 1150 RVA: 0x0001911A File Offset: 0x0001731A
		public GraphNode NodeNear(Vector3 destination)
		{
			RecastGraph graph = this.player.GetControlled().svMovable.GetGraph();
			if (graph == null)
			{
				return null;
			}
			return graph.GetNearest(destination, null).node;
		}

		// Token: 0x0600047F RID: 1151 RVA: 0x00019143 File Offset: 0x00017343
		public override bool IsValidTarget(ShPlayer chaser)
		{
			return base.IsValidTarget(chaser) && this.job.IsValidTarget(chaser);
		}

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x06000480 RID: 1152 RVA: 0x0001915C File Offset: 0x0001735C
		public bool BadPath
		{
			get
			{
				return this.lastPathState < PathCompleteState.Complete;
			}
		}

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x06000481 RID: 1153 RVA: 0x00019167 File Offset: 0x00017367
		public bool IncompletePath
		{
			get
			{
				return this.lastPathState != PathCompleteState.Complete;
			}
		}

		// Token: 0x06000482 RID: 1154 RVA: 0x00019178 File Offset: 0x00017378
		public void GetPathAvoidance(Vector3 destination)
		{
			ShMountable shMountable = this.targetEntity as ShMountable;
			if (shMountable != null && shMountable.controller && this.targetEntity != this.leader && this.player.DistanceSqr(destination) > 144f)
			{
				RaycastHit raycastHit;
				float d = Physics.Raycast(shMountable.controller.Origin, shMountable.controller.RotationT.forward, out raycastHit, 60f, 1) ? raycastHit.distance : 60f;
				this.GetPath(destination, shMountable.controller.RotationT, d * Vector3.one);
				return;
			}
			this.GetPath(destination, null, default(Vector3));
		}

		// Token: 0x06000483 RID: 1155 RVA: 0x00019232 File Offset: 0x00017432
		public void ResetPath()
		{
			this.currentPath = null;
			this.lastPathState = PathCompleteState.Complete;
			this.player.ZeroInputs();
		}

		// Token: 0x06000484 RID: 1156 RVA: 0x0001924D File Offset: 0x0001744D
		public void GetPath(Vector3 destination, Transform penaltyT = null, Vector3 penaltyS = default(Vector3))
		{
			this.lastDestination = destination;
			this.penaltyTransform = penaltyT;
			this.penaltyScale = penaltyS;
			this.ResetPath();
			this.svManager.pathQueue[this.player] = destination;
		}

		// Token: 0x06000485 RID: 1157 RVA: 0x00019284 File Offset: 0x00017484
		internal bool StartPath(Vector3 destination)
		{
			ShMovable controlled = this.player.GetControlled();
			RecastGraph graph = controlled.svMovable.GetGraph();
			if (graph != null && graph.GetTiles() != null)
			{
				if (this.svManager.penaltyModifier.penaltyDelta > 0)
				{
					this.svManager.penaltyModifier.penaltyDelta = -3000;
					this.svManager.penaltyModifier.Apply();
				}
				if (this.penaltyTransform)
				{
					this.svManager.penaltyModifierT.SetPositionAndRotation(this.penaltyTransform.position, this.penaltyTransform.rotation);
					this.svManager.penaltyModifierT.localScale = this.penaltyScale;
					this.svManager.penaltyModifier.penaltyDelta = 3000;
					this.svManager.penaltyModifier.Apply();
				}
				ABPath abpath = ABPath.Construct(controlled.Position, destination, null);
				abpath.calculatePartial = true;
				this.seeker.graphMask = 1 << (int)graph.graphIndex;
				this.seeker.StartPath(abpath, new OnPathDelegate(this.PathComplete));
				return true;
			}
			return false;
		}

		// Token: 0x06000486 RID: 1158 RVA: 0x000193AC File Offset: 0x000175AC
		public bool PathingLimit()
		{
			if (this.svManager.pathing.Limit(this.player, true))
			{
				base.DestroySelf();
				return true;
			}
			return false;
		}

		// Token: 0x06000487 RID: 1159 RVA: 0x000193D0 File Offset: 0x000175D0
		protected bool TryAltPath()
		{
			if (this.player.curMount)
			{
				this.SvDismount(false);
				this.GetPath(this.lastDestination, null, default(Vector3));
				return true;
			}
			this.PathingLimit();
			return false;
		}

		// Token: 0x06000488 RID: 1160 RVA: 0x00019418 File Offset: 0x00017618
		public void LogPathing(Path newPath)
		{
			if (this.targetEntity && this.targetEntity.Player && this.targetEntity.Player.isHuman)
			{
				if (newPath != null)
				{
					base.Send(SvSendType.Local, PacketFlags.Reliable, ClPacket.ChatLocal, new object[]
					{
						this.player.ID,
						newPath.CompleteState.ToString()
					});
					Util.Log(string.Format("Path Complete State: {0} {1}", base.name, newPath.CompleteState), LogLevel.Log);
					return;
				}
				Util.Log(string.Format("Path Start: {0} {1} {2}", base.name, this.player.username, this.currentState), LogLevel.Log);
			}
		}

		// Token: 0x06000489 RID: 1161 RVA: 0x000194E8 File Offset: 0x000176E8
		protected void PathComplete(Path newPath)
		{
			this.svManager.pathPlayer = null;
			this.nodeIndex = 0;
			this.vectorIndex = 1;
			if (newPath.CompleteState < PathCompleteState.Complete)
			{
				this.TryAltPath();
				return;
			}
			this.svManager.aStarPath.AddWorkItem(delegate()
			{
				foreach (GraphNode graphNode in newPath.path)
				{
					if (!graphNode.Destroyed && graphNode.Penalty < 10000U)
					{
						uint num = Math.Min(10000U - graphNode.Penalty, 1000U);
						graphNode.Penalty += num;
						uint num2;
						if (this.svManager.decayNodes.TryGetValue(graphNode, out num2))
						{
							this.svManager.decayNodes[graphNode] = num2 + num;
						}
						else
						{
							this.svManager.decayNodes.Add(graphNode, num);
						}
					}
				}
			});
			this.currentPath = newPath;
			this.currentVector = newPath.vectorPath[this.vectorIndex];
			this.SetCurrentNode(this.currentPath.path[this.nodeIndex]);
			this.lastPathState = (((newPath.vectorPath.Last<Vector3>() - this.lastDestination).sqrMagnitude < 2f * this.player.GetControlled().svMovable.NavRangeSqr) ? PathCompleteState.Complete : PathCompleteState.Partial);
		}

		// Token: 0x0600048A RID: 1162 RVA: 0x000195E4 File Offset: 0x000177E4
		public Waypoint GetClosestWaypoint()
		{
			List<Waypoint> waypointList = this.player.GetControlled().svMovable.WaypointList;
			if (waypointList.Count == 0)
			{
				return null;
			}
			Waypoint result = waypointList[0];
			float num = float.PositiveInfinity;
			foreach (Waypoint waypoint in waypointList)
			{
				Vector3 other = waypoint.mainT.position + Vector3.up;
				float num2 = this.player.DistanceSqr(other);
				if (num2 < num)
				{
					result = waypoint;
					num = num2;
				}
			}
			return result;
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x00019690 File Offset: 0x00017890
		public bool SelectNextWaypoint()
		{
			Waypoint closestWaypoint = this.GetClosestWaypoint();
			if (!closestWaypoint)
			{
				Util.Log(base.name + " has no waypoints nearby", LogLevel.Error);
				base.DestroySelf();
				return false;
			}
			this.nextWaypoint = closestWaypoint;
			return true;
		}

		// Token: 0x0600048C RID: 1164 RVA: 0x000196D4 File Offset: 0x000178D4
		public bool SelectNextNode()
		{
			GraphNode graphNode = this.NodeNear(this.player.Position);
			if (graphNode == null)
			{
				Util.Log(base.name + " has no nodes nearby", LogLevel.Error);
				base.DestroySelf();
				return false;
			}
			this.SetCurrentNode(graphNode);
			return true;
		}

		// Token: 0x0600048D RID: 1165 RVA: 0x0001971C File Offset: 0x0001791C
		public void GetPathToWaypoints()
		{
			if (this.onWaypoints)
			{
				return;
			}
			if ((!this.nextWaypoint || this.nextWaypoint.mainT.parent != this.player.Parent || !this.player.CanSee(this.nextWaypoint.mainT.position + Vector3.up)) && !this.SelectNextWaypoint())
			{
				return;
			}
			if (this.player.GetControlled().UsePathfinding)
			{
				this.GetPath(this.nextWaypoint.mainT.position, null, default(Vector3));
				return;
			}
			this.onWaypoints = true;
		}

		// Token: 0x0600048E RID: 1166 RVA: 0x000197CC File Offset: 0x000179CC
		public bool GetOverwatchNear(Vector3 target, out Vector3 position)
		{
			position = default(Vector3);
			GraphNode node = this.svManager.MainGraph.GetNearest(target, null).node;
			VisNode[] array;
			if (node != null && this.svManager.visMap.TryGetValue(node.NodeIndex, out array))
			{
				float num = float.PositiveInfinity;
				uint num2 = uint.MaxValue;
				foreach (VisNode visNode in array)
				{
					Vector3 vector = (Vector3)node.position + visNode.delta;
					float sqrMagnitude = (vector - this.player.Position).sqrMagnitude;
					if (sqrMagnitude < num && !this.svManager.overwatchPositions.Limit(visNode.graphNodeIndex, false))
					{
						num2 = visNode.graphNodeIndex;
						num = sqrMagnitude;
						position = vector;
					}
				}
				if (num2 != 4294967295U)
				{
					this.svManager.overwatchPositions.Limit(num2, true);
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600048F RID: 1167 RVA: 0x000198CC File Offset: 0x00017ACC
		public bool GetOverwatchBest(Vector3 target, out Vector3 position)
		{
			position = default(Vector3);
			GraphNode node = this.svManager.MainGraph.GetNearest(target, null).node;
			VisNode[] array;
			if (node != null && this.svManager.visMap.TryGetValue(node.NodeIndex, out array))
			{
				Vector3 lhs = this.player.Position - target;
				int num = 0;
				uint num2 = uint.MaxValue;
				foreach (VisNode visNode in array)
				{
					VisNode[] array3;
					if (this.svManager.visMap.TryGetValue(visNode.graphNodeIndex, out array3))
					{
						int num3 = array3.Length;
						if (num3 > num && !this.svManager.overwatchPositions.Limit(visNode.graphNodeIndex, false) && Vector3.Dot(lhs, visNode.delta) > 0f)
						{
							num2 = visNode.graphNodeIndex;
							num = num3;
							position = (Vector3)node.position + visNode.delta;
						}
					}
				}
				if (num2 != 4294967295U)
				{
					this.svManager.overwatchPositions.Limit(num2, true);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000490 RID: 1168 RVA: 0x000199F4 File Offset: 0x00017BF4
		public bool GetOverwatchSafe(Vector3 target, Bounds area, out Vector3 position)
		{
			position = default(Vector3);
			GraphNode node = this.svManager.MainGraph.GetNearest(target, null).node;
			VisNode[] array;
			if (node != null && this.svManager.visMap.TryGetValue(node.NodeIndex, out array))
			{
				int num = int.MaxValue;
				uint num2 = uint.MaxValue;
				foreach (VisNode visNode in array)
				{
					VisNode[] array3;
					if (this.svManager.visMap.TryGetValue(visNode.graphNodeIndex, out array3))
					{
						Vector3 vector = (Vector3)node.position + visNode.delta;
						int num3 = array3.Length;
						if (num3 < num && area.Contains(vector) && !this.svManager.overwatchPositions.Limit(visNode.graphNodeIndex, false))
						{
							num2 = visNode.graphNodeIndex;
							num = num3;
							position = vector;
						}
					}
				}
				if (num2 != 4294967295U)
				{
					this.svManager.overwatchPositions.Limit(num2, true);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000491 RID: 1169 RVA: 0x00019B04 File Offset: 0x00017D04
		public bool GetOverwatchDirection(GraphNode node, out Vector3 delta)
		{
			VisNode[] array;
			if (node != null && this.svManager.visMap.TryGetValue(node.NodeIndex, out array) && array.Length != 0)
			{
				int num = array.Length / 10 + 1;
				int num2 = (int)(this.player.Perlin(1f / (float)num) * (float)num);
				delta = array[num2].delta;
				return true;
			}
			delta = default(Vector3);
			return false;
		}

		// Token: 0x06000492 RID: 1170 RVA: 0x00019B70 File Offset: 0x00017D70
		public void LookTactical(Vector3 fallback = default(Vector3))
		{
			Vector3 direction;
			if ((fallback == default(Vector3) || this.player.Perlin(0.25f) < 0.5f) && this.GetOverwatchDirection(this.currentNode, out direction))
			{
				this.LookAt(direction);
				return;
			}
			this.LookAt(fallback);
		}

		// Token: 0x06000493 RID: 1171 RVA: 0x00019BC4 File Offset: 0x00017DC4
		public void LookTarget()
		{
			ShMovable controlled = this.player.GetControlled();
			if (!this.targetEntity || this.NodeLink || this.player.climbState == ClimbState.Climbing)
			{
				this.LookAt(this.currentVector - controlled.Position);
				return;
			}
			if (this.targetEntity != this.leader && this.player.CanSeeEntity(this.targetEntity, false, 362.03867f))
			{
				this.AimSmart();
				return;
			}
			this.LookTactical(this.currentVector - controlled.Position);
		}

		// Token: 0x06000494 RID: 1172 RVA: 0x00019C68 File Offset: 0x00017E68
		public bool MoveLookNavPath()
		{
			this.LookTarget();
			ShMovable controlled = this.player.GetControlled();
			if (this.svManager.pathQueue.ContainsKey(this.player) || !this.seeker.IsDone())
			{
				return true;
			}
			if (this.currentPath == null)
			{
				this.player.ZeroInputs();
				this.PathingLimit();
				return false;
			}
			int num = this.nodeIndex;
			float num2 = controlled.DistanceSqr((Vector3)this.currentNode.position);
			while (++num < this.currentPath.path.Count)
			{
				GraphNode graphNode = this.currentPath.path[num];
				if (controlled.DistanceSqr((Vector3)graphNode.position) < num2)
				{
					this.nodeIndex = num;
					this.SetCurrentNode(graphNode);
				}
				else if (num - this.nodeIndex <= 4)
				{
					continue;
				}
				IL_134:
				while (controlled.DistanceSqr2D(this.currentVector) < controlled.svMovable.NavRangeSqr)
				{
					if (this.vectorIndex >= this.currentPath.vectorPath.Count - 1)
					{
						this.player.ZeroInputs();
						return this.IncompletePath && this.TryAltPath();
					}
					this.vectorIndex++;
					this.currentVector = this.currentPath.vectorPath[this.vectorIndex];
				}
				controlled.svMovable.MoveTo(this.currentVector);
				return true;
			}
			goto IL_134;
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x00019DD4 File Offset: 0x00017FD4
		public void SetCurrentNode(GraphNode node)
		{
			this.currentNode = node;
			DoorLink doorLink = this.NodeLink as DoorLink;
			if (doorLink != null)
			{
				base.SvRelocate(doorLink.end, MonoBehaviourSingleton<SceneManager>.Instance.places[doorLink.offsetIndex].mTransform);
			}
		}

		// Token: 0x06000496 RID: 1174 RVA: 0x00019E20 File Offset: 0x00018020
		public void MoveLookWaypointPath()
		{
			ShMovable controlled = this.player.GetControlled();
			Vector3 position = this.nextWaypoint.mainT.position;
			if (Time.time > this.waypointDelayEnd && controlled.DistanceSqr2D(position) < controlled.svMovable.NavRangeSqr)
			{
				if (this.nextWaypoint.delay > 0f && this.waypointDelayEnd == 0f)
				{
					this.waypointDelayEnd = Time.time + this.nextWaypoint.delay;
				}
				else
				{
					this.waypointDelayEnd = 0f;
					this.UpdateNextWaypoint();
				}
			}
			this.LookAt(position - controlled.Position);
			controlled.svMountable.MoveTo(position);
		}

		// Token: 0x06000497 RID: 1175 RVA: 0x00019ED4 File Offset: 0x000180D4
		public void MoveLookNodePath()
		{
			ShMovable controlled = this.player.GetControlled();
			Vector3 vector = (Vector3)this.currentNode.position;
			if (controlled.DistanceSqr2D(vector) < controlled.svMovable.NavRangeSqr)
			{
				this.UpdateNextNode();
			}
			this.LookAt(vector - controlled.Position);
			controlled.svMountable.MoveTo(vector);
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x00019F38 File Offset: 0x00018138
		public void SetBestEquipable()
		{
			ShUsable shUsable = this.player.Hands;
			ShUsable bestJobEquipable = this.job.GetBestJobEquipable();
			if (bestJobEquipable)
			{
				shUsable = bestJobEquipable;
			}
			else
			{
				bool flag = false;
				ShEntity shEntity = null;
				float num = 0f;
				if (this.targetEntity)
				{
					shEntity = ((this.targetEntity.GetMount() != null) ? this.targetEntity.GetMount() : this.targetEntity);
					num = this.player.DistanceSqr(shEntity);
				}
				float num2 = shUsable.GetRange(0) * shUsable.GetDamage(0) * (float)shUsable.burstSize / shUsable.GetUseDelay(0);
				foreach (InventoryItem inventoryItem in this.player.myItems.Values)
				{
					ShUsable shUsable2 = inventoryItem.item as ShUsable;
					if (shUsable2 != null && (!shUsable2.AmmoItem || this.player.HasItem(shUsable2.AmmoItem)) && (!shEntity || shEntity.GetComponent(shUsable2.GetTargetType())))
					{
						float range = shUsable2.GetRange(0);
						bool flag2 = range * range > num;
						if (!flag || flag2)
						{
							float num3 = range * shUsable2.GetDamage(0) * (float)shUsable2.burstSize / shUsable2.GetUseDelay(0);
							if (num3 > num2)
							{
								if (flag2)
								{
									flag = true;
								}
								num2 = num3;
								shUsable = shUsable2;
							}
						}
					}
				}
			}
			this.SvSetEquipable(shUsable);
			foreach (InventoryItem inventoryItem2 in this.player.myItems.Values)
			{
				ShAttachment shAttachment = inventoryItem2.item as ShAttachment;
				if (shAttachment != null && this.player.GetAttachment(shAttachment.AttachmentType).index == MonoBehaviourSingleton<ShManager>.Instance.nullAttachment[shAttachment.AttachmentType].index && this.player.curEquipable.AcceptAttachment(shAttachment))
				{
					this.SvSetAttachment(shAttachment.index);
				}
			}
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x0001A17C File Offset: 0x0001837C
		public void SetBestMountWeapon()
		{
			ShMountable curMount = this.player.curMount;
			if (curMount)
			{
				MountWeaponSet[] weaponSets = curMount.seats[(int)this.player.seat].weaponSets;
				if (weaponSets.Length > 1 && this.targetEntity)
				{
					ShEntity shEntity = (this.targetEntity.GetMount() != null) ? this.targetEntity.GetMount() : this.targetEntity;
					int mountWeapon = 0;
					ShThrown shThrown = MonoBehaviourSingleton<SceneManager>.Instance.GetEntity<ShThrown>(weaponSets[0].thrownName);
					for (int i = 1; i < weaponSets.Length; i++)
					{
						ShThrown shThrown2;
						if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShThrown>(weaponSets[i].thrownName, out shThrown2) && (curMount.GetWeaponSet((int)this.player.seat).curAmmo == 0 || (weaponSets[i].curAmmo > 0 && (!shThrown2.IsGuided || shEntity.GetComponent(shThrown2.targetType)) && (shEntity.GetGround() || shEntity.InWater || shThrown2.spawnVelocity > 50f) && shThrown2.GetDamage((int)this.player.seat) > shThrown.GetDamage((int)this.player.seat))))
						{
							shThrown = shThrown2;
							mountWeapon = i;
						}
					}
					this.SetMountWeapon(mountWeapon);
				}
			}
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x0001A2D8 File Offset: 0x000184D8
		public void SetBestWeapons()
		{
			this.SetBestEquipable();
			this.SetBestMountWeapon();
		}

		// Token: 0x0400054D RID: 1357
		protected readonly List<ProgressAction> progressHandler = new List<ProgressAction>();

		// Token: 0x0400054E RID: 1358
		protected ProgressAction currentProgress;

		// Token: 0x0400054F RID: 1359
		public ShPlayer player;

		// Token: 0x04000550 RID: 1360
		public new static readonly PlayerEvents events = new PlayerEvents();

		// Token: 0x04000551 RID: 1361
		[NonSerialized]
		public string tagname;

		// Token: 0x04000552 RID: 1362
		[NonSerialized]
		public Job job;

		// Token: 0x04000553 RID: 1363
		public int spawnJobIndex;

		// Token: 0x04000554 RID: 1364
		[NonSerialized]
		public int spawnJobRank;

		// Token: 0x04000555 RID: 1365
		public readonly IDCollection<ShEntity> subscribedEntities = new IDCollection<ShEntity>();

		// Token: 0x04000556 RID: 1366
		public readonly HashSet<ShEntity> spawnedEntities = new HashSet<ShEntity>();

		// Token: 0x04000557 RID: 1367
		[NonSerialized]
		public Queue<int> prevEquipables = new Queue<int>(2);

		// Token: 0x04000558 RID: 1368
		[NonSerialized]
		public HashSet<string> appContacts = new HashSet<string>();

		// Token: 0x04000559 RID: 1369
		[NonSerialized]
		public HashSet<string> appBlocked = new HashSet<string>();

		// Token: 0x0400055A RID: 1370
		[NonSerialized]
		public List<AppCall> appCalls = new List<AppCall>();

		// Token: 0x0400055B RID: 1371
		[NonSerialized]
		public Dictionary<string, AppMessages> appMessages = new Dictionary<string, AppMessages>();

		// Token: 0x0400055C RID: 1372
		[NonSerialized]
		public List<string> lastMessengers = new List<string>();

		// Token: 0x0400055D RID: 1373
		[NonSerialized]
		public int unreadMessages;

		// Token: 0x0400055E RID: 1374
		[NonSerialized]
		public List<AppTransaction> appTransactions = new List<AppTransaction>();

		// Token: 0x0400055F RID: 1375
		protected ShPlayer tradePlayer;

		// Token: 0x04000560 RID: 1376
		[NonSerialized]
		public ShPlayer callTarget;

		// Token: 0x04000561 RID: 1377
		[NonSerialized]
		public bool callActive;

		// Token: 0x04000562 RID: 1378
		[NonSerialized]
		public bool caller;

		// Token: 0x04000563 RID: 1379
		[NonSerialized]
		public bool tradeConfirmed;

		// Token: 0x04000564 RID: 1380
		[NonSerialized]
		public int bankBalance;

		// Token: 0x04000565 RID: 1381
		[NonSerialized]
		public BuyerType buyerType;

		// Token: 0x04000566 RID: 1382
		[NonSerialized]
		public byte[] wearableIndices;

		// Token: 0x04000567 RID: 1383
		[NonSerialized]
		public bool placementValid;

		// Token: 0x04000568 RID: 1384
		[NonSerialized]
		public ShEntity spawnTarget;

		// Token: 0x04000569 RID: 1385
		[NonSerialized]
		public ShEntity goalEntity;

		// Token: 0x0400056A RID: 1386
		[NonSerialized]
		public ShEntity goalMarker;

		// Token: 0x0400056B RID: 1387
		[NonSerialized]
		public bool godMode;

		// Token: 0x0400056C RID: 1388
		public bool stop;

		// Token: 0x0400056D RID: 1389
		protected byte[] attachmentArray;

		// Token: 0x0400056E RID: 1390
		protected byte[] wearableArray;

		// Token: 0x0400056F RID: 1391
		public ConnectData connectData;

		// Token: 0x04000570 RID: 1392
		public Peer connection;

		// Token: 0x04000571 RID: 1393
		[NonSerialized]
		public bool holdPackets = true;

		// Token: 0x04000572 RID: 1394
		private Queue<byte[]> packetQueue;

		// Token: 0x04000573 RID: 1395
		[NonSerialized]
		public Minigame minigame;

		// Token: 0x04000574 RID: 1396
		[NonSerialized]
		public State currentState;

		// Token: 0x04000575 RID: 1397
		public State[] states;

		// Token: 0x04000576 RID: 1398
		[NonSerialized]
		public ShEntity targetEntity;

		// Token: 0x04000577 RID: 1399
		[NonSerialized]
		public ShPlayer leader;

		// Token: 0x04000578 RID: 1400
		[NonSerialized]
		public ShPlayer follower;

		// Token: 0x04000579 RID: 1401
		[NonSerialized]
		public bool onWaypoints;

		// Token: 0x0400057A RID: 1402
		[NonSerialized]
		public Waypoint nextWaypoint;

		// Token: 0x0400057B RID: 1403
		[NonSerialized]
		public Seeker seeker;

		// Token: 0x0400057C RID: 1404
		[NonSerialized]
		public PathCompleteState lastPathState;

		// Token: 0x0400057D RID: 1405
		[NonSerialized]
		public Vector3 lastDestination;

		// Token: 0x0400057E RID: 1406
		[NonSerialized]
		public Path currentPath;

		// Token: 0x0400057F RID: 1407
		[NonSerialized]
		public GraphNode currentNode;

		// Token: 0x04000580 RID: 1408
		[NonSerialized]
		public Vector3 currentVector;

		// Token: 0x04000581 RID: 1409
		[NonSerialized]
		public int nodeIndex;

		// Token: 0x04000582 RID: 1410
		[NonSerialized]
		public int vectorIndex;

		// Token: 0x04000583 RID: 1411
		public const float aiDelta = 0.1f;

		// Token: 0x04000584 RID: 1412
		protected const float orientSpeed = 18f;

		// Token: 0x04000585 RID: 1413
		protected Vector3 aim;

		// Token: 0x04000586 RID: 1414
		protected int aimFrame;

		// Token: 0x04000587 RID: 1415
		protected int jumpFrame;

		// Token: 0x04000588 RID: 1416
		[NonSerialized]
		public Transform penaltyTransform;

		// Token: 0x04000589 RID: 1417
		[NonSerialized]
		public Vector3 penaltyScale;

		// Token: 0x0400058A RID: 1418
		private float waypointDelayEnd;

		// Token: 0x0200037C RID: 892
		protected struct ApartmentInfo
		{
			// Token: 0x06001285 RID: 4741 RVA: 0x0005656B File Offset: 0x0005476B
			public ApartmentInfo(int originalIndex, int copyIndex, int offset)
			{
				this.originalIndex = originalIndex;
				this.copyIndex = copyIndex;
				this.offset = offset;
			}

			// Token: 0x04001687 RID: 5767
			public int originalIndex;

			// Token: 0x04001688 RID: 5768
			public int copyIndex;

			// Token: 0x04001689 RID: 5769
			public int offset;
		}
	}
}
