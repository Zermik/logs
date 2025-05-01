using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrokeProtocol.Client.UI;
using BrokeProtocol.Collections;
using BrokeProtocol.Managers;
using BrokeProtocol.Parameters;
using BrokeProtocol.Required;
using BrokeProtocol.Utility;
using BrokeProtocol.Utility.Jobs;
using BrokeProtocol.Utility.Networking;
using ENet;
using UnityEngine;

namespace BrokeProtocol.Entities
{
	// Token: 0x020000A3 RID: 163
	public class ShPlayer : ShMovable
	{
		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060002DE RID: 734 RVA: 0x0000ECC8 File Offset: 0x0000CEC8
		public bool InOwnApartment
		{
			get
			{
				return this.ownedApartments.ContainsValue(base.Place);
			}
		}

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060002DF RID: 735 RVA: 0x0000ECDB File Offset: 0x0000CEDB
		public int PlaceItemCount
		{
			get
			{
				return base.Place.GetItemCount();
			}
		}

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x060002E0 RID: 736 RVA: 0x0000ECE8 File Offset: 0x0000CEE8
		public override ShEntity SpecEntity
		{
			get
			{
				return this.specPlayer;
			}
		}

		// Token: 0x060002E1 RID: 737 RVA: 0x0000ECF0 File Offset: 0x0000CEF0
		public JobInfoShared GetJobInfoShared()
		{
			if (!SceneManager.isServer)
			{
				return this.clPlayer.job;
			}
			return this.svPlayer.job.info.shared;
		}

		// Token: 0x060002E2 RID: 738 RVA: 0x0000ED1C File Offset: 0x0000CF1C
		public int GetMaxExperience()
		{
			Upgrades[] upgrades = this.GetJobInfoShared().upgrades;
			if (this.rank >= upgrades.Length)
			{
				return 0;
			}
			return upgrades[this.rank].maxExperience;
		}

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x060002E3 RID: 739 RVA: 0x0000ED4F File Offset: 0x0000CF4F
		public bool LockOnValid
		{
			get
			{
				return this.lockOnTarget && Time.time - this.lockOnTime >= 1f;
			}
		}

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x060002E4 RID: 740 RVA: 0x0000ED78 File Offset: 0x0000CF78
		public ShWeapon Hands
		{
			get
			{
				ShWeapon result;
				if (!MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShWeapon>(this.hands.GetPrefabIndex(), out result))
				{
					return this.manager.hands;
				}
				return result;
			}
		}

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x060002E5 RID: 741 RVA: 0x0000EDAC File Offset: 0x0000CFAC
		public ShEquipable Surrender
		{
			get
			{
				ShEquipable result;
				if (!MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShEquipable>(this.surrender.GetPrefabIndex(), out result))
				{
					return this.manager.surrender;
				}
				return result;
			}
		}

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x060002E6 RID: 742 RVA: 0x0000EDDF File Offset: 0x0000CFDF
		public ShRestraint Handcuffs
		{
			get
			{
				return MonoBehaviourSingleton<SceneManager>.Instance.GetEntity<ShRestraint>((this.characterType == CharacterType.Mob) ? "Muzzle" : "Handcuffs");
			}
		}

		// Token: 0x060002E7 RID: 743 RVA: 0x0000EE00 File Offset: 0x0000D000
		public bool CanEquip(ShEquipable e)
		{
			return e && (e.characterType == CharacterType.All || e.characterType == this.characterType);
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x0000EE25 File Offset: 0x0000D025
		public bool CanWear(ShWearable w)
		{
			return w && (w.characterType == CharacterType.All || w.characterType == this.characterType);
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x0000EE4C File Offset: 0x0000D04C
		public int GetPlaceItemLimit()
		{
			Place place = base.Place;
			if (place != null)
			{
				foreach (KeyValuePair<ShApartment, Place> keyValuePair in this.ownedApartments)
				{
					if (keyValuePair.Value == place)
					{
						return keyValuePair.Key.limit;
					}
				}
				return 0;
			}
			return 0;
		}

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x060002EA RID: 746 RVA: 0x0000EEC0 File Offset: 0x0000D0C0
		public bool IsMountArmed
		{
			get
			{
				return this.curMount && this.curMount.HasWeaponSet((int)this.seat);
			}
		}

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x060002EB RID: 747 RVA: 0x0000EEE2 File Offset: 0x0000D0E2
		public ShMountable ActiveWeapon
		{
			get
			{
				if (!this.IsMountArmed)
				{
					return this.curEquipable;
				}
				return this.curMount;
			}
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x060002EC RID: 748 RVA: 0x0000EEF9 File Offset: 0x0000D0F9
		public bool CanUpdateInputs
		{
			get
			{
				return this.IsUp && base.SpecSelf && !this.OutsideController && (!this.curMount || this.IsMountController);
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x060002ED RID: 749 RVA: 0x0000EF2A File Offset: 0x0000D12A
		public bool OutsideController
		{
			get
			{
				return this.controller != this;
			}
		}

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x060002EE RID: 750 RVA: 0x0000EF38 File Offset: 0x0000D138
		public override bool ServersidePhysics
		{
			get
			{
				return this.OutsideController;
			}
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x060002EF RID: 751 RVA: 0x0000EF40 File Offset: 0x0000D140
		public bool IsMountController
		{
			get
			{
				return this.curMount && this.seat == 0;
			}
		}

		// Token: 0x060002F0 RID: 752 RVA: 0x0000EF5C File Offset: 0x0000D15C
		public bool IsControlledMount<T>(out T mount)
		{
			ShMountable shMountable = this.curMount;
			if (shMountable is T)
			{
				T t = shMountable as T;
				mount = t;
				return this.IsMountController;
			}
			mount = default(T);
			return false;
		}

		// Token: 0x060002F1 RID: 753 RVA: 0x0000EF9A File Offset: 0x0000D19A
		public bool IsPassenger(out ShMovable mount)
		{
			return !this.IsControlledMount<ShMovable>(out mount) && this.curMount;
		}

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x060002F2 RID: 754 RVA: 0x0000EFB2 File Offset: 0x0000D1B2
		public GameObject HiddenInterior
		{
			get
			{
				if (!this.curMount)
				{
					return null;
				}
				return this.curMount.seats[(int)this.seat].hideInterior;
			}
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x060002F3 RID: 755 RVA: 0x0000EFDA File Offset: 0x0000D1DA
		public bool OnFoot
		{
			get
			{
				return !this.curMount && this.IsUp && (this.GetGround() || this.InWater);
			}
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x060002F4 RID: 756 RVA: 0x0000F008 File Offset: 0x0000D208
		public bool CanJump
		{
			get
			{
				return this.OnFoot && !this.OutsideController;
			}
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x060002F5 RID: 757 RVA: 0x0000F01D File Offset: 0x0000D21D
		public bool CanCrouch
		{
			get
			{
				return !this.curMount && this.IsUp && this.GetGround() && !this.InWater && !this.OutsideController;
			}
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x060002F6 RID: 758 RVA: 0x0000F054 File Offset: 0x0000D254
		public bool CanDeploy
		{
			get
			{
				return !this.curMount && this.IsMobile && !this.GetGround() && !this.InWater && this.positionRB.linearVelocity.magnitude > 1.1f * this.jumpVelocity;
			}
		}

		// Token: 0x060002F7 RID: 759 RVA: 0x0000F0B0 File Offset: 0x0000D2B0
		public override Collider GetGround()
		{
			if (!base.IsSimulated)
			{
				if (!this.curMount)
				{
					return base.GetGround();
				}
				return this.curMount.GetGround();
			}
			else
			{
				if (!this.positionRB.IsSleeping() && Time.time > this.groundTime + 0.1f)
				{
					return null;
				}
				return this.ground;
			}
		}

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x060002F8 RID: 760 RVA: 0x0000F10D File Offset: 0x0000D30D
		// (set) Token: 0x060002F9 RID: 761 RVA: 0x0000F12E File Offset: 0x0000D32E
		public override Vector3 Velocity
		{
			get
			{
				if (!this.curMount)
				{
					return base.Velocity;
				}
				return this.curMount.Velocity;
			}
			set
			{
				if (this.curMount)
				{
					this.curMount.Velocity = value;
					return;
				}
				base.Velocity = value;
			}
		}

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x060002FA RID: 762 RVA: 0x0000F151 File Offset: 0x0000D351
		public override StanceIndex StanceIndex
		{
			get
			{
				return this.stance.index;
			}
		}

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x060002FB RID: 763 RVA: 0x0000F15E File Offset: 0x0000D35E
		public bool IsCapable
		{
			get
			{
				return base.isActiveAndEnabled && !this.IsRestrained && !this.IsDead;
			}
		}

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x060002FC RID: 764 RVA: 0x0000F17B File Offset: 0x0000D37B
		public bool IsUp
		{
			get
			{
				return !this.stance.ragdoll && this.stance.index != StanceIndex.Recovering;
			}
		}

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x060002FD RID: 765 RVA: 0x0000F19E File Offset: 0x0000D39E
		public bool IsMobile
		{
			get
			{
				return this.IsUp && !this.IsRestrained;
			}
		}

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x060002FE RID: 766 RVA: 0x0000F1B3 File Offset: 0x0000D3B3
		public bool CanFollow
		{
			get
			{
				return !this.isHuman && !this.boss && !base.Shop && this.IsMobile;
			}
		}

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x060002FF RID: 767 RVA: 0x0000F1D5 File Offset: 0x0000D3D5
		public bool InventoryLocked
		{
			get
			{
				return !this.IsMobile || this.otherEntity;
			}
		}

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000300 RID: 768 RVA: 0x0000F1EC File Offset: 0x0000D3EC
		public bool IsRestrained
		{
			get
			{
				return this.curEquipable is ShRestrained;
			}
		}

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000301 RID: 769 RVA: 0x0000F1FC File Offset: 0x0000D3FC
		public bool IsSurrendered
		{
			get
			{
				return this.curEquipable.index == this.Surrender.index;
			}
		}

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x06000302 RID: 770 RVA: 0x0000F216 File Offset: 0x0000D416
		public bool IsKnockedOut
		{
			get
			{
				return this.stance.index == StanceIndex.KnockedOut;
			}
		}

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x06000303 RID: 771 RVA: 0x0000F227 File Offset: 0x0000D427
		public override Vector3 CenterBuoyancy
		{
			get
			{
				return this.Origin + new Vector3(0f, -1.9f * this.mainT.localScale.y, 0f);
			}
		}

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000304 RID: 772 RVA: 0x0000F259 File Offset: 0x0000D459
		public override Vector3 Origin
		{
			get
			{
				return this.originT.position;
			}
		}

		// Token: 0x06000305 RID: 773 RVA: 0x0000F268 File Offset: 0x0000D468
		public Vector3 OriginOffset()
		{
			Vector3 origin = this.Origin;
			RaycastHit raycastHit;
			if (!Physics.Raycast(origin, this.originT.forward, out raycastHit, 2f, 26373))
			{
				return origin + this.rotationT.forward * 2f;
			}
			return raycastHit.point;
		}

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000306 RID: 774 RVA: 0x0000F2BE File Offset: 0x0000D4BE
		public override Transform RotationT
		{
			get
			{
				return this.rotationT;
			}
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000307 RID: 775 RVA: 0x0000F2C6 File Offset: 0x0000D4C6
		public override SerializedParameters Parameters
		{
			get
			{
				return new PlayerParameters(this);
			}
		}

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000308 RID: 776 RVA: 0x0000F2CE File Offset: 0x0000D4CE
		public override Type EditorType
		{
			get
			{
				return typeof(PlayerEditor);
			}
		}

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x06000309 RID: 777 RVA: 0x0000F2DA File Offset: 0x0000D4DA
		public override ShPlayer Player
		{
			get
			{
				return this;
			}
		}

		// Token: 0x0600030A RID: 778 RVA: 0x0000F2DD File Offset: 0x0000D4DD
		public override ShMountable GetMount()
		{
			if (!this.curMount)
			{
				return this;
			}
			return this.curMount;
		}

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x0600030B RID: 779 RVA: 0x0000F2F4 File Offset: 0x0000D4F4
		public override bool InWater
		{
			get
			{
				return !this.curMount && base.InWater;
			}
		}

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x0600030C RID: 780 RVA: 0x0000F30B File Offset: 0x0000D50B
		public override float MountHealth
		{
			get
			{
				if (!this.curMount)
				{
					return this.health;
				}
				return this.curMount.MountHealth;
			}
		}

		// Token: 0x0600030D RID: 781 RVA: 0x0000F32C File Offset: 0x0000D52C
		public ShMovable GetControlled()
		{
			ShMovable result;
			if (!this.IsControlledMount<ShMovable>(out result))
			{
				return this;
			}
			return result;
		}

		// Token: 0x0600030E RID: 782 RVA: 0x0000F348 File Offset: 0x0000D548
		public ShTransport GetDeployable()
		{
			ShDeployable shDeployable;
			if (!this.CanDeploy || !this.TryGetCachedItem<ShDeployable>(out shDeployable))
			{
				return null;
			}
			return shDeployable.transport;
		}

		// Token: 0x0600030F RID: 783 RVA: 0x0000F370 File Offset: 0x0000D570
		public bool TryGetCachedItem<T>(out T item) where T : ShItem
		{
			int index;
			if (this.cachedItems.TryGetValue(typeof(T), ref index))
			{
				return MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<T>(index, out item);
			}
			item = default(T);
			return false;
		}

		// Token: 0x06000310 RID: 784 RVA: 0x0000F3AC File Offset: 0x0000D5AC
		public void Bind(int equipableIndex, byte slot)
		{
			int equipable = this.bindings[(int)slot].equipable;
			if (equipable != equipableIndex && base.HasItem(equipableIndex))
			{
				this.bindings[(int)slot].equipable = equipableIndex;
				foreach (int attachmentIndex in this.bindings[(int)slot].attachments)
				{
					this.UnbindAttachment(attachmentIndex, slot);
				}
				if (SceneManager.isServer)
				{
					this.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.Bind, new object[]
					{
						equipableIndex,
						slot
					});
					return;
				}
				InventoryMenu inventoryMenu = MonoBehaviourSingleton<ClManager>.Instance.CurrentMenu as InventoryMenu;
				if (inventoryMenu != null)
				{
					inventoryMenu.Refill(new object[]
					{
						equipableIndex
					});
					inventoryMenu.Refill(new object[]
					{
						equipable
					});
				}
				MonoBehaviourSingleton<ClManager>.Instance.hud.RefreshEquipmentBar();
			}
		}

		// Token: 0x06000311 RID: 785 RVA: 0x0000F49C File Offset: 0x0000D69C
		public void Unbind(byte slot)
		{
			int equipable = this.bindings[(int)slot].equipable;
			if (equipable != this.Hands.index)
			{
				this.bindings[(int)slot].equipable = this.Hands.index;
				foreach (int attachmentIndex in this.bindings[(int)slot].attachments)
				{
					this.UnbindAttachment(attachmentIndex, slot);
				}
				if (SceneManager.isServer)
				{
					this.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.Unbind, new object[]
					{
						slot
					});
					return;
				}
				InventoryMenu inventoryMenu = MonoBehaviourSingleton<ClManager>.Instance.CurrentMenu as InventoryMenu;
				if (inventoryMenu != null)
				{
					inventoryMenu.Refill(new object[]
					{
						equipable
					});
					inventoryMenu.Refill(new object[]
					{
						this.Hands.index
					});
				}
				MonoBehaviourSingleton<ClManager>.Instance.hud.RefreshEquipmentBar();
			}
		}

		// Token: 0x06000312 RID: 786 RVA: 0x0000F598 File Offset: 0x0000D798
		public override void PreInitialize(int ID)
		{
			base.PreInitialize(ID);
			this.bindings = new Binding[]
			{
				new Binding(this),
				new Binding(this),
				new Binding(this),
				new Binding(this)
			};
		}

		// Token: 0x06000313 RID: 787 RVA: 0x0000F5EC File Offset: 0x0000D7EC
		public override void Initialize()
		{
			Vector3 localPosition = this.rotationT.localPosition;
			Vector3 zero = Vector3.zero;
			CharacterType characterType = this.characterType;
			float num;
			Vector3 zero2;
			Vector3 offset;
			float num2;
			float capsuleHeight;
			float capsuleHeight2;
			if (characterType != CharacterType.Humanoid)
			{
				if (characterType != CharacterType.Mob)
				{
					num = 0f;
					zero2 = Vector3.zero;
					offset = Vector3.zero;
					num2 = 0f;
					capsuleHeight = 0f;
					capsuleHeight2 = 0f;
				}
				else
				{
					num = this.capsule.radius;
					zero2 = new Vector3(0f, 1f, 0.63f);
					offset = localPosition;
					num2 = 0.5f;
					capsuleHeight = num;
					capsuleHeight2 = num2;
				}
			}
			else
			{
				num = this.capsule.height;
				zero2 = new Vector3(0f, 2.695f, 0f);
				offset = new Vector3(0f, 1.8f, 0f);
				num2 = 3f;
				capsuleHeight = 2.4f;
				capsuleHeight2 = this.capsule.radius * 2f;
			}
			this.stances = new StanceType[]
			{
				new StanceType(StanceIndex.Stand, localPosition, num, 1f, true, false, false),
				new StanceType(StanceIndex.Crouch, zero2, num2, (this.characterType == CharacterType.Humanoid) ? 0.6f : 0f, true, false, false),
				new StanceType(StanceIndex.SitMovable, offset, capsuleHeight, 1f, false, false, false),
				new StanceType(StanceIndex.SitFixed, offset, capsuleHeight, 1f, false, false, true),
				new StanceType(StanceIndex.SitMotorcycle, offset, capsuleHeight, 1f, false, false, true),
				new StanceType(StanceIndex.SitKart, offset, capsuleHeight, 1f, false, false, true),
				new StanceType(StanceIndex.Parachute, localPosition, num, 1f, false, false, true),
				new StanceType(StanceIndex.GunnerFixed, localPosition, num, 1f, false, false, true),
				new StanceType(StanceIndex.Sleep, localPosition, num, 1f, false, false, true),
				new StanceType(StanceIndex.KnockedDown, zero, capsuleHeight2, 0f, false, true, false),
				new StanceType(StanceIndex.Recovering, localPosition, num, 0f, false, false, false),
				new StanceType(StanceIndex.KnockedOut, zero, capsuleHeight2, 0f, false, true, false),
				new StanceType(StanceIndex.Dead, zero, capsuleHeight2, 0f, false, true, false),
				new StanceType(StanceIndex.StandStill, localPosition, num, 1f, false, false, false),
				new StanceType(StanceIndex.CrouchStill, zero2, num2, 1f, false, false, false),
				new StanceType(StanceIndex.StandFixed, localPosition, num, 1f, false, false, true),
				new StanceType(StanceIndex.CrouchFixed, zero2, num2, 1f, false, false, true),
				new StanceType(StanceIndex.Gunner, localPosition, num, 1f, false, false, false)
			};
			this.stance = this.stances[0];
			this.controller = this;
			this.specPlayer = this;
			EntityCollections.Players.Add(this);
			if (this.isHuman)
			{
				EntityCollections.Accounts[this.username] = this;
				EntityCollections.Humans.Add(this);
			}
			else
			{
				EntityCollections.NPCs.Add(this);
			}
			if (base.IsClientMain)
			{
				MonoBehaviourSingleton<ClManager>.Instance.myID = this.ID;
				MonoBehaviourSingleton<ClManager>.Instance.myPlayer = this;
				MonoBehaviourSingleton<ClManager>.Instance.StartHUD();
			}
			this.SetEquipable(this.Hands.index, true);
			foreach (ShWearable shWearable in MonoBehaviourSingleton<ShManager>.Instance.nullWearable)
			{
				this.SetWearable(shWearable.index);
			}
			base.Initialize();
			this.UpdateLayer();
		}

		// Token: 0x06000314 RID: 788 RVA: 0x0000F94C File Offset: 0x0000DB4C
		public override void Destroy()
		{
			base.Destroy();
			EntityCollections.Players.Remove(this);
			if (this.isHuman)
			{
				EntityCollections.Accounts.Remove(this.username);
				EntityCollections.Humans.Remove(this);
				return;
			}
			EntityCollections.NPCs.Remove(this);
		}

		// Token: 0x06000315 RID: 789 RVA: 0x0000F99D File Offset: 0x0000DB9D
		public void ZeroInputs()
		{
			this.input = Vector3.zero;
		}

		// Token: 0x06000316 RID: 790 RVA: 0x0000F9AA File Offset: 0x0000DBAA
		public override void ResetInputs()
		{
			this.ZeroInputs();
			if (base.IsClientMain)
			{
				MonoBehaviourSingleton<ClManager>.Instance.lastInput = this.input;
			}
			this.ResetMode();
		}

		// Token: 0x06000317 RID: 791 RVA: 0x0000F9D0 File Offset: 0x0000DBD0
		public float Perlin(float scale)
		{
			return Mathf.PerlinNoise(scale * MonoBehaviourSingleton<SceneManager>.Instance.time, (float)this.ID);
		}

		// Token: 0x06000318 RID: 792 RVA: 0x0000F9EA File Offset: 0x0000DBEA
		public void TrySetInput(float x, float y, float z)
		{
			if (this.IsUp)
			{
				this.input = new Vector3(x, y, z);
			}
		}

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x06000319 RID: 793 RVA: 0x0000FA02 File Offset: 0x0000DC02
		public bool IsSlow
		{
			get
			{
				return this.weight > this.weightLimit || this.stats[2] <= 0.25f;
			}
		}

		// Token: 0x0600031A RID: 794 RVA: 0x0000FA28 File Offset: 0x0000DC28
		public bool TryUpdateMode(byte modeInput)
		{
			byte b = this.mode;
			if (modeInput >= 2)
			{
				this.mode = modeInput;
			}
			else if (this.IsSlow)
			{
				this.mode = 2;
			}
			else
			{
				this.mode = modeInput;
			}
			if (b != this.mode)
			{
				if (base.IsClientMain)
				{
					MonoBehaviourSingleton<ClManager>.Instance.hud.SetCrosshairs();
				}
				return true;
			}
			return false;
		}

		// Token: 0x0600031B RID: 795 RVA: 0x0000FA84 File Offset: 0x0000DC84
		public override float GetSpeedLimit()
		{
			float num = this.maxSpeed * this.curEquipable.moveSpeed * this.stance.input;
			if (this.controller.mode >= 2)
			{
				num *= 0.5f;
			}
			else if (this.controller.mode == 1)
			{
				num *= 1.25f;
			}
			return num * (1f - this.injuryAmount[4] * 0.005f);
		}

		// Token: 0x0600031C RID: 796 RVA: 0x0000FAF8 File Offset: 0x0000DCF8
		protected override void MoveControlled()
		{
			base.MoveControlled();
			if (!this.curMount && this.IsUp)
			{
				Vector3 forward = this.mainT.forward;
				Vector3 right = this.mainT.right;
				float num = Vector3.Dot(this.relativeVelocity, forward);
				float num2 = Vector3.Dot(this.relativeVelocity, right);
				float speedLimit = this.GetSpeedLimit();
				Vector3 vector = Vector3.ClampMagnitude(this.controller.input, 1f) * speedLimit;
				Vector3 vector2 = (vector.x - num) * this.moveFactor.x * forward + (-vector.z - num2) * this.moveFactor.z * right;
				if (this.InWater)
				{
					vector2 *= 0.5f;
				}
				else if (!this.GetGround())
				{
					vector2 *= 0.1f;
				}
				if (this.climbState >= ClimbState.Mantling)
				{
					vector2 -= Physics.gravity;
					if (this.climbState == ClimbState.Climbing)
					{
						vector2 -= 0.8f * speedLimit * this.moveFactor.x * this.climbableNormal;
					}
					float y = this.positionRB.linearVelocity.y;
					float num3 = -Vector3.Dot(this.climbableNormal, this.rotationT.forward);
					float num4 = this.controller.input.x * (this.rotationT.forward.y + num3 * 0.25f);
					vector2 += (speedLimit * num4 - y) * this.moveFactor.y * Vector3.up;
				}
				this.positionRB.AddForce(vector2, ForceMode.Acceleration);
			}
		}

		// Token: 0x0600031D RID: 797 RVA: 0x0000FCC8 File Offset: 0x0000DEC8
		protected void Update()
		{
			if (this.IsUp)
			{
				if (this.OutsideController)
				{
					this.rotationT.rotation = Quaternion.Slerp(this.rotationT.rotation, this.controller.rotationT.rotation, Time.deltaTime * 10f);
				}
				this.SetBody();
			}
		}

		// Token: 0x0600031E RID: 798 RVA: 0x0000FD21 File Offset: 0x0000DF21
		public override void SetRotation(Quaternion rotation)
		{
			this.rotationT.rotation = rotation;
			this.SetBody();
		}

		// Token: 0x0600031F RID: 799 RVA: 0x0000FD38 File Offset: 0x0000DF38
		protected override void Move()
		{
			base.Move();
			if (this.IsUp)
			{
				if (base.IsSimulated)
				{
					this.SetFriction(this.input.sqrMagnitude < 0.1f);
				}
			}
			else if (base.IsSimulated)
			{
				this.SetFriction(true);
			}
			this.climbState = ClimbState.None;
		}

		// Token: 0x06000320 RID: 800 RVA: 0x0000FD8C File Offset: 0x0000DF8C
		public bool SetEquipable(int equipableIndex, bool resetAmmo)
		{
			ShEquipable equipablePrefab = MonoBehaviourSingleton<SceneManager>.Instance.GetEntity<ShEquipable>(equipableIndex);
			if (this.CanEquip(equipablePrefab))
			{
				this.ResetMode();
				Transform parent = this.mainT;
				if (SceneManager.isServer)
				{
					this.svPlayer.DismountAll();
				}
				else
				{
					parent = Enumerable.FirstOrDefault<Transform>(this.clPlayer.skinnedMeshRenderer.bones, (Transform b) => b.name == equipablePrefab.attachBone);
				}
				if (this.curEquipable && this.curEquipable.index != equipableIndex)
				{
					Object.Destroy(this.curEquipable.go);
					this.curEquipable = null;
				}
				if (!this.curEquipable)
				{
					this.curEquipable = Object.Instantiate<ShEquipable>(equipablePrefab, parent, false);
				}
				this.curEquipable.InitializeOnPlayer(this);
				if (resetAmmo)
				{
					this.curEquipable.ResetAmmo();
				}
				foreach (ShAttachment a in this.manager.nullAttachment)
				{
					this.SetAttachment(a, UnderbarrelSetting.Default);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000321 RID: 801 RVA: 0x0000FE9C File Offset: 0x0000E09C
		public ShAttachment GetAttachment(int type)
		{
			ShAttachment result;
			switch (type)
			{
			case 0:
				result = this.curEquipable.curMuzzle;
				break;
			case 1:
				result = this.curEquipable.curSight;
				break;
			case 2:
				result = this.curEquipable.curUnderbarrel;
				break;
			default:
				result = null;
				break;
			}
			return result;
		}

		// Token: 0x06000322 RID: 802 RVA: 0x0000FEEC File Offset: 0x0000E0EC
		public bool SetAttachment(int attachmentIndex, UnderbarrelSetting underbarrelSetting = UnderbarrelSetting.Default)
		{
			ShAttachment a;
			return MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShAttachment>(attachmentIndex, out a) && this.SetAttachment(a, underbarrelSetting);
		}

		// Token: 0x06000323 RID: 803 RVA: 0x0000FF14 File Offset: 0x0000E114
		public bool SetAttachment(ShAttachment a, UnderbarrelSetting underbarrelSetting = UnderbarrelSetting.Default)
		{
			Transform transform = this.curEquipable.AcceptAttachment(a);
			if (transform)
			{
				this.ResetMode();
				ShAttachment attachment = this.GetAttachment(a.AttachmentType);
				if (attachment)
				{
					if (attachment.index == a.index)
					{
						if (base.isActiveAndEnabled)
						{
							base.StartCoroutine(this.SetEquipableFinish());
						}
						return true;
					}
					Object.Destroy(attachment.go);
				}
				ShAttachment shAttachment = Object.Instantiate<ShAttachment>(a, transform, false);
				switch (shAttachment.AttachmentType)
				{
				case 0:
					this.curEquipable.curMuzzle = (shAttachment as ShMuzzle);
					break;
				case 1:
					this.curEquipable.curSight = (shAttachment as ShSight);
					break;
				case 2:
					this.curEquipable.curUnderbarrel = (shAttachment as ShUnderbarrel);
					if (underbarrelSetting == UnderbarrelSetting.Enabled)
					{
						this.curEquipable.curUnderbarrel.setting = true;
					}
					else if (underbarrelSetting == UnderbarrelSetting.Disabled)
					{
						this.curEquipable.curUnderbarrel.setting = false;
					}
					break;
				default:
					return false;
				}
				shAttachment.InitializeOnPlayer(this);
				shAttachment.ResetAmmo();
				this.curEquipable.RecalulcateFactors();
				if (base.isActiveAndEnabled)
				{
					base.StartCoroutine(this.SetEquipableFinish());
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000324 RID: 804 RVA: 0x00010040 File Offset: 0x0000E240
		public bool BindAttachment(int attachmentIndex, byte slot)
		{
			ShAttachment shAttachment;
			if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShAttachment>(attachmentIndex, out shAttachment) && MonoBehaviourSingleton<SceneManager>.Instance.GetEntity<ShEquipable>(this.bindings[(int)slot].equipable).AcceptAttachment(shAttachment))
			{
				int num = this.bindings[(int)slot].attachments[shAttachment.AttachmentType];
				if (num != attachmentIndex)
				{
					this.bindings[(int)slot].attachments[shAttachment.AttachmentType] = attachmentIndex;
					if (base.IsClientMain)
					{
						MonoBehaviourSingleton<ClManager>.Instance.RefreshListMenu<InventoryMenu>(new object[]
						{
							num
						});
						MonoBehaviourSingleton<ClManager>.Instance.RefreshListMenu<InventoryMenu>(new object[]
						{
							attachmentIndex
						});
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000325 RID: 805 RVA: 0x000100FC File Offset: 0x0000E2FC
		public bool UnbindAttachment(int attachmentIndex, byte slot)
		{
			ShAttachment shAttachment;
			if (!MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShAttachment>(attachmentIndex, out shAttachment))
			{
				return false;
			}
			if (this.bindings[(int)slot].attachments[shAttachment.AttachmentType] != attachmentIndex)
			{
				return false;
			}
			int index = this.manager.nullAttachment[shAttachment.AttachmentType].index;
			this.bindings[(int)slot].attachments[shAttachment.AttachmentType] = index;
			if (base.IsClientMain)
			{
				MonoBehaviourSingleton<ClManager>.Instance.RefreshListMenu<InventoryMenu>(new object[]
				{
					attachmentIndex
				});
				MonoBehaviourSingleton<ClManager>.Instance.RefreshListMenu<InventoryMenu>(new object[]
				{
					index
				});
			}
			return true;
		}

		// Token: 0x06000326 RID: 806 RVA: 0x000101A8 File Offset: 0x0000E3A8
		public bool SetWearable(int wearableIndex)
		{
			ShWearable entity = MonoBehaviourSingleton<SceneManager>.Instance.GetEntity<ShWearable>(wearableIndex);
			if (this.CanWear(entity))
			{
				int type = (int)entity.type;
				ShWearable shWearable = this.curWearables[type];
				if (shWearable)
				{
					if (shWearable.index == wearableIndex)
					{
						return false;
					}
					Object.Destroy(shWearable.go);
				}
				(this.curWearables[type] = Object.Instantiate<ShWearable>(entity, this.mainT, false)).InitializeOnPlayer(this);
				if (base.IsSimulated)
				{
					this.armorLevel = 0f;
					this.weightLimit = this.baseWeightLimit;
					foreach (ShWearable shWearable2 in this.curWearables)
					{
						if (shWearable2)
						{
							this.armorLevel += shWearable2.protection;
							this.weightLimit += shWearable2.capacity;
						}
					}
					this.armorLevel = Mathf.Min(this.maxStat, this.armorLevel);
					if (SceneManager.isClient)
					{
						if (shWearable)
						{
							MonoBehaviourSingleton<ClManager>.Instance.RefreshListMenu<InventoryMenu>(new object[]
							{
								shWearable.index
							});
						}
						MonoBehaviourSingleton<ClManager>.Instance.RefreshListMenu<InventoryMenu>(new object[]
						{
							wearableIndex
						});
					}
					else
					{
						SvPlayer.events.SetWearable(this, entity);
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000327 RID: 807 RVA: 0x00010300 File Offset: 0x0000E500
		public void ResetMode()
		{
			if (SceneManager.isServer)
			{
				this.svPlayer.SvUpdateMode((this.svPlayer.currentState != null) ? this.svPlayer.currentState.StateMoveMode : 0);
				return;
			}
			this.TryUpdateMode(0);
			if (this.clPlayer.isMain)
			{
				MonoBehaviourSingleton<ClManager>.Instance.lastMode = this.mode;
			}
		}

		// Token: 0x06000328 RID: 808 RVA: 0x00010368 File Offset: 0x0000E568
		public void SendFire(int mountableIndex)
		{
			if (SceneManager.isServer)
			{
				this.svPlayer.Send(SvSendType.LocalOthers, PacketFlags.Unsequenced, ClPacket.Fire, new object[]
				{
					this.ID,
					mountableIndex
				});
				return;
			}
			if (this.clPlayer.isMain)
			{
				this.fireIndex += 1;
				this.clPlayer.clManager.SendToServer(PacketFlags.Reliable, SvPacket.Fire, new object[]
				{
					base.Rotation,
					mountableIndex
				});
			}
		}

		// Token: 0x06000329 RID: 809 RVA: 0x000103F8 File Offset: 0x0000E5F8
		public void Fire(int mountableIndex)
		{
			if (!this.curEquipable)
			{
				return;
			}
			if (mountableIndex == this.curEquipable.index)
			{
				if (this.curEquipable.CanUse(0))
				{
					this.SendFire(this.curEquipable.index);
					this.curEquipable.Use(0);
					return;
				}
				this.UpdateAmmo(this.curEquipable);
				return;
			}
			else if (mountableIndex == this.curEquipable.curUnderbarrel.index)
			{
				if (this.curEquipable.curUnderbarrel.CanUse(0))
				{
					this.curEquipable.curUnderbarrel.MountFire(0);
					return;
				}
				this.UpdateAmmo(this.curEquipable.curUnderbarrel);
				return;
			}
			else
			{
				if (!this.curMount || mountableIndex != this.curMount.index)
				{
					ShItem weapon;
					if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShItem>(mountableIndex, out weapon))
					{
						this.UpdateAmmo(weapon);
					}
					return;
				}
				if (this.curMount.CanUse((int)this.seat))
				{
					this.curMount.MountFire((int)this.seat);
					return;
				}
				this.UpdateAmmo(this.curMount);
				return;
			}
		}

		// Token: 0x0600032A RID: 810 RVA: 0x00010508 File Offset: 0x0000E708
		public void UpdateAmmo(ShMountable weapon)
		{
			if (SceneManager.isServer && !weapon.ServersideAmmo)
			{
				ShItem ammoItem = weapon.AmmoItem;
				this.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.UpdateAmmo, new object[]
				{
					1,
					ammoItem ? ammoItem.index : 0
				});
			}
		}

		// Token: 0x0600032B RID: 811 RVA: 0x00010562 File Offset: 0x0000E762
		public ShItem GetAmmoItem(bool underbarrel)
		{
			if (!underbarrel)
			{
				return this.curEquipable.AmmoItem;
			}
			return this.curEquipable.curUnderbarrel.AmmoItem;
		}

		// Token: 0x0600032C RID: 812 RVA: 0x00010584 File Offset: 0x0000E784
		public void Consume(int index)
		{
			ShConsumable shConsumable;
			if (!MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShConsumable>(index, out shConsumable))
			{
				return;
			}
			if (SceneManager.isClient)
			{
				this.animator.SetTrigger(Animations.consume);
				if (this.clPlayer.isMain)
				{
					MonoBehaviourSingleton<MainCamera>.Instance.PlayerConsumeSound(shConsumable.consumableIndex);
					return;
				}
			}
			else if (base.HasItem(index) && this.svPlayer.SvConsume(shConsumable, null))
			{
				this.TransferItem(2, index, 1, true);
			}
		}

		// Token: 0x0600032D RID: 813 RVA: 0x000105F8 File Offset: 0x0000E7F8
		protected IEnumerator ArmSway()
		{
			this.armSway = true;
			Vector3 currentDirection = Vector3.zero;
			Vector3 goalDirection = Vector3.zero;
			float nextGoalReset = 0f;
			for (;;)
			{
				float num = this.injuryAmount[3];
				if (num == 0f)
				{
					break;
				}
				if (Time.time >= nextGoalReset)
				{
					currentDirection = goalDirection;
					goalDirection = Random.insideUnitCircle;
					nextGoalReset = Time.time + 1f;
				}
				this.Orient(0.5f * Time.deltaTime * num * Vector2.Lerp(goalDirection, currentDirection, nextGoalReset - Time.time));
				yield return null;
			}
			this.armSway = false;
			yield break;
			yield break;
		}

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x0600032E RID: 814 RVA: 0x00010607 File Offset: 0x0000E807
		public float ViewAngleLimit
		{
			get
			{
				if (!this.curMount)
				{
					return 89f;
				}
				return this.curMount.seats[(int)this.seat].viewAngleLimit;
			}
		}

		// Token: 0x0600032F RID: 815 RVA: 0x00010634 File Offset: 0x0000E834
		public void Orient(Vector2 delta)
		{
			if (this.stance.fixedForward)
			{
				this.rotationT.Rotate(Vector3.left, delta.y, Space.Self);
				this.rotationT.Rotate(Vector3.up, delta.x, Space.Self);
				this.rotationT.localRotation = Quaternion.RotateTowards(Quaternion.identity, this.rotationT.localRotation, this.ViewAngleLimit);
				return;
			}
			this.rotationT.DeltaRotate(delta.x, delta.y, this.ViewAngleLimit);
		}

		// Token: 0x06000330 RID: 816 RVA: 0x000106C0 File Offset: 0x0000E8C0
		protected override void UpdateMainColliders()
		{
			base.UpdateMainColliders();
			this.headCollider = this.rotationT.GetComponent<Collider>();
			this.colliders.Add(this.headCollider);
		}

		// Token: 0x06000331 RID: 817 RVA: 0x000106EC File Offset: 0x0000E8EC
		public override void Die(ShPlayer attacker = null)
		{
			this.ClearInjuries();
			if (SceneManager.isClient)
			{
				if (this.clPlayer.isMain)
				{
					MonoBehaviourSingleton<MainCamera>.Instance.PlayerDeathSound();
				}
				this.clPlayer.SetStance(StanceIndex.Dead);
			}
			else
			{
				this.SetStance(StanceIndex.Dead);
				this.svPlayer.job.OnDie();
			}
			base.Die(attacker);
		}

		// Token: 0x06000332 RID: 818 RVA: 0x0001074B File Offset: 0x0000E94B
		public bool IsShielded(DamageIndex damageIndex, Collider collider)
		{
			return damageIndex == DamageIndex.Gun && this.shieldCollider && this.shieldCollider.enabled && collider == this.shieldCollider;
		}

		// Token: 0x06000333 RID: 819 RVA: 0x00010779 File Offset: 0x0000E979
		public bool IsBlocking(DamageIndex damageIndex)
		{
			return damageIndex == DamageIndex.Melee && this.curEquipable.grip == Grip.None && this.mode == 3;
		}

		// Token: 0x06000334 RID: 820 RVA: 0x00010798 File Offset: 0x0000E998
		public override void HitEffect(Collider collider, DamageIndex damageIndex, Vector3 point, Vector3 normal)
		{
			int num;
			if (this.IsBlocking(damageIndex))
			{
				num = 2;
			}
			else if (this.IsShielded(damageIndex, collider))
			{
				num = 1;
			}
			else
			{
				num = 0;
			}
			Util.hitEffects[num].Execute(point, normal);
		}

		// Token: 0x06000335 RID: 821 RVA: 0x000107D2 File Offset: 0x0000E9D2
		public override void Mounted(ShPlayer occupant, int seatIndex)
		{
			if (SceneManager.isClient)
			{
				this.clPlayer.SetStance(StanceIndex.Stand);
			}
			else
			{
				this.SetStance(StanceIndex.Stand);
			}
			base.Mounted(occupant, seatIndex);
		}

		// Token: 0x06000336 RID: 822 RVA: 0x000107F8 File Offset: 0x0000E9F8
		public void SetStanceColliders(StanceIndex stanceIndex)
		{
			this.stance = this.stances[(int)stanceIndex];
			this.headCollider.enabled = !this.stance.ragdoll;
			if (this.shieldCollider)
			{
				this.UpdateShieldEnabled();
			}
			this.RotationT.localPosition = this.stance.offset;
			this.SetCapsuleHeight(this.stance.capsuleHeight);
		}

		// Token: 0x06000337 RID: 823 RVA: 0x00010866 File Offset: 0x0000EA66
		public void UpdateShieldEnabled()
		{
			this.shieldCollider.enabled = this.stance.setable;
		}

		// Token: 0x06000338 RID: 824 RVA: 0x0001087E File Offset: 0x0000EA7E
		public void SetStance(StanceIndex stanceIndex)
		{
			this.SetStanceColliders(stanceIndex);
			if (!this.IsUp)
			{
				this.ResetInputs();
			}
		}

		// Token: 0x06000339 RID: 825 RVA: 0x00010895 File Offset: 0x0000EA95
		public bool CanFire()
		{
			return this.IsMountArmed || !this.stance.fixedForward;
		}

		// Token: 0x0600033A RID: 826 RVA: 0x000108AF File Offset: 0x0000EAAF
		public bool CanAltFire()
		{
			return !this.IsMountArmed && !this.stance.fixedForward;
		}

		// Token: 0x0600033B RID: 827 RVA: 0x000108CC File Offset: 0x0000EACC
		public void SetBody()
		{
			if (!this.curMount)
			{
				Vector3 forward = this.rotationT.forward;
				this.positionRB.MoveRotation(this.mainT.rotation = Vector3.ProjectOnPlane(forward, Vector3.up).SafeLookRotation(Vector3.up));
				this.rotationT.rotation = forward.SafeLookRotation(Vector3.up);
				return;
			}
			this.mainT.position = this.curMountT.position;
			if (this.stance.fixedForward)
			{
				this.mainT.rotation = this.curMountT.rotation;
				this.rotationT.rotation = this.rotationT.forward.SafeLookRotation(Vector3.ProjectOnPlane(this.rotationT.up, this.mainT.right));
				return;
			}
			Vector3 forward2 = this.rotationT.forward;
			this.mainT.rotation = Vector3.ProjectOnPlane(forward2, this.curMountT.up).SafeLookRotation(this.curMountT.up);
			Physics.SyncTransforms();
			this.rotationT.rotation = forward2.SafeLookRotation(this.curMountT.up);
		}

		// Token: 0x0600033C RID: 828 RVA: 0x00010A04 File Offset: 0x0000EC04
		public void Mount(ShMountable mount, byte enterSeat)
		{
			if (SceneManager.isServer && base.Parent != mount.Parent)
			{
				this.svPlayer.SvSetParent(mount.Parent);
			}
			this.seat = enterSeat;
			this.curMount = mount;
			this.curMountT = mount.seats[(int)this.seat].seatPositionT;
			this.ResetInputs();
			this.SetTransform(this.curMountT.position, this.curMountT.rotation, true);
			this.UpdateLayer();
			if (this.IsMountController)
			{
				mount.controller = this;
			}
			mount.Mounted(this, (int)enterSeat);
		}

		// Token: 0x0600033D RID: 829 RVA: 0x00010AA4 File Offset: 0x0000ECA4
		public override bool InActionRange(ShEntity e)
		{
			if (!e.animator)
			{
				return base.InActionRange(e);
			}
			RaycastHit raycastHit;
			return Physics.Raycast(this.originT.position, this.originT.forward, out raycastHit, 8f, 9985) && raycastHit.collider.GetComponentInParent<ShEntity>() == e;
		}

		// Token: 0x0600033E RID: 830 RVA: 0x00010B04 File Offset: 0x0000ED04
		protected void SetFriction(bool highFriction)
		{
			PhysicsMaterial sharedMaterial = highFriction ? this.manager.highFrictionMaterial : this.manager.noFrictionMaterial;
			foreach (Collider collider in this.colliders)
			{
				collider.sharedMaterial = sharedMaterial;
			}
		}

		// Token: 0x0600033F RID: 831 RVA: 0x00010B74 File Offset: 0x0000ED74
		public int GetLayer()
		{
			if (this.HiddenInterior)
			{
				return 3;
			}
			if (SceneManager.isClient && this.clPlayer.SpecMain && this.clPlayer.FirstPerson)
			{
				if (!this.curMount)
				{
					return 2;
				}
				return 18;
			}
			else
			{
				if (!this.curMount)
				{
					return 9;
				}
				return 8;
			}
		}

		// Token: 0x06000340 RID: 832 RVA: 0x00010BD4 File Offset: 0x0000EDD4
		public void UpdateLayer()
		{
			int layer = this.GetLayer();
			foreach (Collider collider in this.colliders)
			{
				collider.gameObject.layer = layer;
			}
			this.positionRB.isKinematic = this.curMount;
			foreach (ShWearable shWearable in this.curWearables)
			{
				if (shWearable)
				{
					shWearable.go.layer = layer;
				}
			}
			this.curEquipable.mainT.SetLayer(layer);
		}

		// Token: 0x06000341 RID: 833 RVA: 0x00010C88 File Offset: 0x0000EE88
		public void Dismount()
		{
			Transform seatPositionT = this.curMount.seats[(int)this.seat].seatPositionT;
			if (this.curMount.exitTransforms.Length != 0)
			{
				Transform transform = this.curMount.exitTransforms[0];
				for (int i = 1; i < this.curMount.exitTransforms.Length; i++)
				{
					if (Vector3.SqrMagnitude(seatPositionT.position - this.curMount.exitTransforms[i].position) < Vector3.SqrMagnitude(seatPositionT.position - transform.position))
					{
						transform = this.curMount.exitTransforms[i];
					}
				}
				base.SetPositionSafe(transform.position);
			}
			else if (!(this.curMount is ShTransport))
			{
				base.SetPositionSafe(this.curMountT.position);
			}
			else
			{
				Vector3 vector = Mathf.Sign(seatPositionT.localPosition.x) * this.curMountT.right;
				Vector3 vector2 = this.curMountT.position;
				if (!Physics.Raycast(vector2, vector, 4f + this.capsule.radius, 1))
				{
					vector2 += vector * 4f;
				}
				base.SetPositionSafe(vector2);
			}
			seatPositionT.gameObject.SetActive(false);
			this.curMount.occupants[(int)this.seat] = null;
			if (this.IsMountController)
			{
				this.curMount.controller = this.curMount.Player;
				this.curMount.ResetInputs();
			}
			Vector3 velocity = this.curMount.Velocity;
			this.curMount = null;
			this.curMountT = null;
			this.UpdateLayer();
			this.ResetInputs();
			base.SetVelocity(velocity);
			this.relativeVelocity = Vector3.zero;
		}

		// Token: 0x06000342 RID: 834 RVA: 0x00010E44 File Offset: 0x0000F044
		protected void OnCollisionStay(Collision collision)
		{
			if (this.IgnorePhysics(collision.collider) || !base.IsSimulated)
			{
				return;
			}
			float y = this.mainT.localScale.y;
			float num = 2f * y;
			float num2 = 0.5f * y;
			float num3 = 0f;
			int contacts = collision.GetContacts(Util.contactBuffer);
			for (int i = 0; i < contacts; i++)
			{
				ContactPoint contactPoint = Util.contactBuffer[i];
				float num4 = contactPoint.point.y - this.lastPosition.y;
				if (num4 < num2)
				{
					this.ground = collision.collider;
					this.groundTime = Time.fixedTime;
				}
				if (this.climbState != ClimbState.Climbing)
				{
					if (contactPoint.otherCollider.CompareTag("Climbable"))
					{
						this.climbState = ClimbState.Climbing;
						this.climbableNormal = contactPoint.normal;
					}
					else if (num4 > num3)
					{
						num3 = num4;
						this.climbableNormal = contactPoint.normal;
					}
				}
			}
			if (this.climbState != ClimbState.Climbing && num3 > num2 && num3 < num)
			{
				Bounds bounds = this.capsule.bounds;
				float num5 = 0.1f * y;
				bounds.Expand(num5);
				if (!Physics.CheckBox(bounds.center + new Vector3(0f, num + num5, 0f), bounds.extents, default(Quaternion), 9217))
				{
					this.climbState = ClimbState.Mantling;
				}
			}
		}

		// Token: 0x06000343 RID: 835 RVA: 0x00010FB1 File Offset: 0x0000F1B1
		public void Jump()
		{
			this.positionRB.AddForce((this.jumpVelocity - this.positionRB.linearVelocity.y * 0.25f) * Vector3.up, ForceMode.VelocityChange);
		}

		// Token: 0x06000344 RID: 836 RVA: 0x00010FE8 File Offset: 0x0000F1E8
		public void FinishTrade(bool isGood)
		{
			if (isGood)
			{
				using (Dictionary<int, InventoryItem>.Enumerator enumerator = this.otherEntity.tradeItems.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<int, InventoryItem> keyValuePair = enumerator.Current;
						this.TransferItem(1, keyValuePair.Key, keyValuePair.Value.count, false);
					}
					goto IL_9C;
				}
			}
			foreach (KeyValuePair<int, InventoryItem> keyValuePair2 in this.tradeItems)
			{
				this.TransferItem(1, keyValuePair2.Key, keyValuePair2.Value.count, false);
			}
			IL_9C:
			this.lockedTrade = false;
		}

		// Token: 0x06000345 RID: 837 RVA: 0x000110B4 File Offset: 0x0000F2B4
		public void SetCapsuleHeight(float height)
		{
			if (this.capsule.direction == 1)
			{
				this.capsule.height = height;
				this.capsule.center = new Vector3(0f, height * 0.5f, 0f);
				return;
			}
			this.capsule.radius = height;
			this.capsule.center = new Vector3(0f, this.capsule.radius, 0f);
		}

		// Token: 0x06000346 RID: 838 RVA: 0x0001112E File Offset: 0x0000F32E
		protected IEnumerator SetEquipableFinish()
		{
			if (SceneManager.isClient)
			{
				this.animator.SetInteger(Animations.grip, (int)this.curEquipable.grip);
				this.animator.SetTrigger(Animations.startSwitch);
			}
			this.switchFinishTime = Time.time + 1f;
			if (this.switching)
			{
				yield break;
			}
			this.switching = true;
			while (Time.time < this.switchFinishTime)
			{
				yield return null;
			}
			this.switching = false;
			if (SceneManager.isServer)
			{
				this.svPlayer.prevEquipables.Clear();
			}
			if (base.IsSimulated)
			{
				this.curEquipable.curUnderbarrel.ResetAmmo();
			}
			yield break;
		}

		// Token: 0x06000347 RID: 839 RVA: 0x00011140 File Offset: 0x0000F340
		public override void Cleanup()
		{
			base.Cleanup();
			if (this.curMount)
			{
				if (SceneManager.isServer)
				{
					this.Dismount();
				}
				else
				{
					this.clPlayer.Dismount();
				}
			}
			if (SceneManager.isServer)
			{
				if (!this.isHuman)
				{
					this.svPlayer.SetState(0);
				}
				this.svPlayer.SvStopInventory(false);
				this.svPlayer.SvMinigameStop(false);
				this.svPlayer.CleanupCall(this.svPlayer.callTarget);
				this.svPlayer.SvLockOn(null);
				ShPlayer follower = this.svPlayer.follower;
				if (follower)
				{
					follower.svPlayer.ClearLeader();
					follower.svPlayer.ResetAI();
				}
				else if (this.svPlayer.leader)
				{
					this.svPlayer.ClearLeader();
					this.svPlayer.ResetAI();
				}
				this.svPlayer.DestroyGoalMarker();
				this.svPlayer.DestroySelfMarker();
				return;
			}
			if (this.clPlayer.isMain)
			{
				if (!(MonoBehaviourSingleton<ClManager>.Instance.CurrentMenu is ChatMenu))
				{
					MonoBehaviourSingleton<ClManager>.Instance.DestroyMenu("Default");
				}
				MonoBehaviourSingleton<ClManager>.Instance.ClearHighlightEntity();
				return;
			}
			if (this.clPlayer.identityObject)
			{
				Util.identityBuffer.Disable(this.clPlayer.identityObject);
			}
		}

		// Token: 0x06000348 RID: 840 RVA: 0x0001129D File Offset: 0x0000F49D
		public override void Restore(Vector3 position, Quaternion rotation, Transform placeT)
		{
			base.Restore(position, rotation, placeT);
			if (SceneManager.isClient)
			{
				this.clPlayer.SetStance(StanceIndex.Stand);
				return;
			}
			this.SetStance(StanceIndex.Stand);
			if (!this.isHuman)
			{
				this.svPlayer.ResetAI();
			}
		}

		// Token: 0x06000349 RID: 841 RVA: 0x000112D6 File Offset: 0x0000F4D6
		public bool CanCollectEntity(ShEntity e)
		{
			if (this.IsMobile && e && (!(e is ShFurniture) || this.InOwnApartment))
			{
				InventoryStruct[] collectedItems = e.CollectedItems;
				return collectedItems != null && collectedItems.Length != 0;
			}
			return false;
		}

		// Token: 0x0600034A RID: 842 RVA: 0x0001130C File Offset: 0x0000F50C
		public override void Spawn(Vector3 position, Quaternion rotation, Transform placeT)
		{
			this.switching = false;
			this.armSway = false;
			base.Spawn(position, rotation, placeT);
			if (SceneManager.isServer)
			{
				if (!this.isHuman)
				{
					base.StartCoroutine(this.svPlayer.RunState());
				}
				this.svPlayer.job.OnSpawn();
				base.StartCoroutine(this.Maintenance());
				for (int i = 0; i < this.stats.Length; i++)
				{
					this.stats[i] = 1f;
				}
				return;
			}
			base.StartCoroutine(this.clPlayer.Footsteps());
			if (this.clPlayer.isMain)
			{
				MonoBehaviourSingleton<ClManager>.Instance.hud.RefreshEquipmentBar();
				base.StartCoroutine(this.clPlayer.SendUpdatesToServer());
				base.StartCoroutine(this.Maintenance());
				base.StartCoroutine(this.clPlayer.RaycastEntities());
			}
			if (this.clPlayer.SpecMain)
			{
				MonoBehaviourSingleton<MainCamera>.Instance.SetCamera();
			}
		}

		// Token: 0x0600034B RID: 843 RVA: 0x00011405 File Offset: 0x0000F605
		public IEnumerator Maintenance()
		{
			yield return null;
			WaitForSeconds delay = new WaitForSeconds(5f);
			while (!this.IsDead)
			{
				for (int i = this.injuries.Count - 1; i >= 0; i--)
				{
					Injury injury = this.injuries[i];
					int num = (int)injury.amount - this.injuryDecay[(int)injury.effect];
					if (num > 0)
					{
						this.injuries[i] = new Injury(injury.part, injury.effect, (byte)num);
					}
					else if (SceneManager.isServer)
					{
						this.injuries.RemoveAt(i);
						this.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.RemoveInjury, new object[]
						{
							i
						});
					}
				}
				this.UpdateInjuries();
				yield return delay;
			}
			yield break;
		}

		// Token: 0x0600034C RID: 844 RVA: 0x00011414 File Offset: 0x0000F614
		public void AddInjury(BodyPart part, BodyEffect effect, byte amount)
		{
			this.injuries.Add(new Injury(part, effect, amount));
			this.UpdateInjuries();
		}

		// Token: 0x0600034D RID: 845 RVA: 0x0001142F File Offset: 0x0000F62F
		public void ClearInjuries()
		{
			this.injuries.Clear();
			this.UpdateInjuries();
		}

		// Token: 0x0600034E RID: 846 RVA: 0x00011442 File Offset: 0x0000F642
		public void RemoveInjury(int index)
		{
			this.injuries.RemoveAt(index);
			this.UpdateInjuries();
		}

		// Token: 0x0600034F RID: 847 RVA: 0x00011458 File Offset: 0x0000F658
		public void UpdateInjuries()
		{
			for (int i = 0; i < 5; i++)
			{
				this.injuryAmount[i] = 0f;
			}
			foreach (Injury injury in this.injuries)
			{
				int part = (int)injury.part;
				this.injuryAmount[part] = Mathf.Min(100f, this.injuryAmount[part] + (float)injury.amount);
			}
			if (base.IsSimulated)
			{
				if (!this.armSway && this.injuryAmount[3] > 0f)
				{
					base.StartCoroutine(this.ArmSway());
				}
				if (base.IsClientMain)
				{
					MonoBehaviourSingleton<ClManager>.Instance.hud.healthVisual.UpdateBodyVisual();
					HealthMenu healthMenu = MonoBehaviourSingleton<ClManager>.Instance.CurrentMenu as HealthMenu;
					if (healthMenu != null && healthMenu.player == this)
					{
						healthMenu.Refill(Array.Empty<object>());
					}
				}
			}
		}

		// Token: 0x06000350 RID: 848 RVA: 0x00011560 File Offset: 0x0000F760
		public override bool IsAccessible(ShPlayer attempter, bool checkOwner)
		{
			return base.IsAccessible(attempter, checkOwner) && !this.curMount && this.IsRestrained && this.IsUp && this != attempter && this != attempter.curMount;
		}

		// Token: 0x06000351 RID: 849 RVA: 0x000115A0 File Offset: 0x0000F7A0
		public bool CanMount(ShMountable mount, bool checkOwner, bool checkRestrained, out byte seatIndex)
		{
			seatIndex = 0;
			float num = float.PositiveInfinity;
			if (this.IsUp && (!checkRestrained || !this.IsRestrained) && mount.IsAccessible(this, checkOwner))
			{
				for (int i = (this.IsRestrained && mount is ShMovable) ? 1 : 0; i < mount.occupants.Length; i++)
				{
					if (!mount.occupants[i])
					{
						float sqrMagnitude = (this.mainT.position - mount.seats[i].seatPositionT.position).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							num = sqrMagnitude;
							seatIndex = (byte)i;
						}
					}
				}
			}
			return num != float.PositiveInfinity;
		}

		// Token: 0x06000352 RID: 850 RVA: 0x00011650 File Offset: 0x0000F850
		public override void DestroyExtras()
		{
			Object.Destroy(this.clPlayer.textTransform.gameObject);
			Object.Destroy(this.clPlayer.skinnedMeshRenderer.bones[0].gameObject);
			Object.Destroy(this.clPlayer.skinnedMeshRenderer);
			this.clPlayer.skinnedMeshRenderer = null;
			base.DestroyExtras();
		}

		// Token: 0x06000353 RID: 851 RVA: 0x000116B0 File Offset: 0x0000F8B0
		public override bool CanBeSearched(ShPlayer p)
		{
			return this.IsSurrendered || this.IsRestrained;
		}

		// Token: 0x06000354 RID: 852 RVA: 0x000116C2 File Offset: 0x0000F8C2
		public void SendSelfTransfer(byte deltaType, int itemIndex, int amount)
		{
			this.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.TransferItem, new object[]
			{
				deltaType,
				itemIndex,
				amount
			});
		}

		// Token: 0x06000355 RID: 853 RVA: 0x000116F4 File Offset: 0x0000F8F4
		public override void TransferItem(byte deltaType, int itemIndex, int amount = 1, bool dispatch = true)
		{
			if (!base.ValidTransfer(itemIndex, amount))
			{
				return;
			}
			switch (deltaType)
			{
			case 1:
				this.AddToMyItems(itemIndex, amount, dispatch);
				break;
			case 2:
				if (!this.SafeRemove(itemIndex, amount, dispatch))
				{
					return;
				}
				break;
			case 3:
				if (!this.otherEntity)
				{
					return;
				}
				this.otherEntity.AddToMyItems(itemIndex, amount, dispatch);
				break;
			case 4:
				if (!this.otherEntity || this.otherEntity.MyItemCount(itemIndex) < amount)
				{
					return;
				}
				this.otherEntity.RemoveFromMyItems(itemIndex, amount, dispatch);
				break;
			case 5:
			case 11:
				if (!this.SafeRemove(itemIndex, amount, dispatch))
				{
					return;
				}
				if (this.otherEntity)
				{
					this.otherEntity.AddToMyItems(itemIndex, amount, dispatch);
				}
				break;
			case 6:
			case 12:
				if (this.otherEntity && this.otherEntity.MyItemCount(itemIndex) >= amount)
				{
					this.otherEntity.RemoveFromMyItems(itemIndex, amount, dispatch);
					this.AddToMyItems(itemIndex, amount, dispatch);
				}
				else
				{
					if (!SceneManager.isClient)
					{
						return;
					}
					this.AddToMyItems(itemIndex, amount, dispatch);
				}
				break;
			case 7:
				if (!this.SafeRemove(itemIndex, amount, dispatch))
				{
					return;
				}
				base.AddToTradeItems(itemIndex, amount);
				break;
			case 8:
				if (base.TradeItemCount(itemIndex) < amount)
				{
					return;
				}
				base.RemoveFromTradeItems(itemIndex, amount);
				this.AddToMyItems(itemIndex, amount, dispatch);
				break;
			case 9:
				if (!this.otherEntity)
				{
					return;
				}
				if (SceneManager.isServer)
				{
					if (this.otherEntity.MyItemCount(itemIndex) < amount)
					{
						return;
					}
					this.otherEntity.RemoveFromMyItems(itemIndex, amount, dispatch);
				}
				this.otherEntity.AddToTradeItems(itemIndex, amount);
				break;
			case 10:
				if (!this.otherEntity || this.otherEntity.TradeItemCount(itemIndex) < amount)
				{
					return;
				}
				this.otherEntity.RemoveFromTradeItems(itemIndex, amount);
				if (SceneManager.isServer)
				{
					this.otherEntity.AddToMyItems(itemIndex, amount, dispatch);
				}
				break;
			}
			if (SceneManager.isServer)
			{
				if (this.viewers.Count > 0)
				{
					base.SendViewerDelta(DeltaInv.ViewerDelta[(int)deltaType], itemIndex, amount, null);
				}
				if (dispatch && this.isHuman)
				{
					this.SendSelfTransfer(deltaType, itemIndex, amount);
				}
				if (this.otherEntity)
				{
					byte b = DeltaInv.InverseDelta[(int)deltaType];
					ShPlayer shPlayer = this.otherEntity as ShPlayer;
					if (shPlayer != null && b != 0 && shPlayer.isHuman)
					{
						shPlayer.SendSelfTransfer(b, itemIndex, amount);
					}
					if (this.otherEntity.viewers.Count > 0)
					{
						b = DeltaInv.ViewerDelta[(int)b];
						if (b != 0)
						{
							this.otherEntity.SendViewerDelta(b, itemIndex, amount, this);
						}
					}
				}
				this.svPlayer.Events.TransferItem(this, deltaType, itemIndex, amount, dispatch);
				return;
			}
			MonoBehaviourSingleton<ClManager>.Instance.RefreshListMenu<InventoryMenu>(new object[]
			{
				this.Hands.index
			});
			MonoBehaviourSingleton<ClManager>.Instance.RefreshListMenu<InventoryMenu>(new object[]
			{
				itemIndex
			});
		}

		// Token: 0x06000356 RID: 854 RVA: 0x000119F8 File Offset: 0x0000FBF8
		protected bool SafeRemove(int itemIndex, int amount, bool dispatch)
		{
			if (SceneManager.isServer)
			{
				if (base.MyItemCount(itemIndex) < amount)
				{
					Util.Log(this.username + " tried to remove an invalid amount from inventory", LogLevel.Warn);
					return false;
				}
				this.RemoveFromMyItems(itemIndex, amount, dispatch);
			}
			else if (this.clPlayer.isMain)
			{
				this.RemoveFromMyItems(itemIndex, amount, dispatch);
			}
			return true;
		}

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x06000357 RID: 855 RVA: 0x00011A50 File Offset: 0x0000FC50
		public int CurrentAmmoTotal
		{
			get
			{
				return this.curEquipable.AmmoTotal;
			}
		}

		// Token: 0x06000358 RID: 856 RVA: 0x00011A5D File Offset: 0x0000FC5D
		public void CorrectMoveMode()
		{
			if (this.mode < 2 && this.IsSlow)
			{
				this.mode = 2;
			}
		}

		// Token: 0x06000359 RID: 857 RVA: 0x00011A78 File Offset: 0x0000FC78
		public void CheckEquipment(int itemIndex, int delta)
		{
			this.weight = 0f;
			foreach (InventoryItem inventoryItem in this.myItems.Values)
			{
				this.weight += inventoryItem.item.weight * (float)inventoryItem.count;
			}
			this.CorrectMoveMode();
			if (SceneManager.isClient)
			{
				ProcessMenu processMenu = MonoBehaviourSingleton<ClManager>.Instance.CurrentMenu as ProcessMenu;
				if (processMenu != null)
				{
					processMenu.Refill(Array.Empty<object>());
				}
				else if (itemIndex == this.manager.money.index)
				{
					AppBankingMenu appBankingMenu = MonoBehaviourSingleton<ClManager>.Instance.CurrentMenu as AppBankingMenu;
					if (appBankingMenu != null)
					{
						appBankingMenu.UpdateBalance();
					}
				}
			}
			if (this.curEquipable)
			{
				ShItem ammoItem = this.GetAmmoItem(false);
				if (ammoItem && ammoItem.index == itemIndex)
				{
					this.curEquipable.UpdatePrimaryAmmo(delta, (int)this.seat);
				}
				ammoItem = this.GetAmmoItem(true);
				if (ammoItem && ammoItem.index == itemIndex)
				{
					this.curEquipable.curUnderbarrel.UpdateSecondaryAmmo(delta, (int)this.seat);
				}
			}
			ShEntity shEntity;
			if (!MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity(itemIndex, out shEntity))
			{
				return;
			}
			Type type = shEntity.GetType();
			int itemIndex2;
			if (this.cachedItems.TryGetValue(type, ref itemIndex2))
			{
				if (delta >= 0 || base.HasItem(itemIndex2))
				{
					return;
				}
				this.cachedItems.Remove(type);
				using (Dictionary<int, InventoryItem>.ValueCollection.Enumerator enumerator = this.myItems.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						InventoryItem inventoryItem2 = enumerator.Current;
						if (inventoryItem2.item.GetType() == type)
						{
							this.cachedItems[type] = inventoryItem2.item.index;
							break;
						}
					}
					return;
				}
			}
			this.cachedItems[type] = itemIndex;
		}

		// Token: 0x0600035A RID: 858 RVA: 0x00011C88 File Offset: 0x0000FE88
		public override void AddToMyItems(int itemIndex, int amount, bool dispatch)
		{
			base.AddToMyItems(itemIndex, amount, dispatch);
			if (SceneManager.isServer)
			{
				this.CheckEquipment(itemIndex, amount);
				return;
			}
			if (this.clPlayer.isMain)
			{
				this.CheckEquipment(itemIndex, amount);
				if (dispatch)
				{
					MonoBehaviourSingleton<ClManager>.Instance.ShowInventoryMessage(itemIndex, amount);
				}
			}
		}

		// Token: 0x0600035B RID: 859 RVA: 0x00011CC8 File Offset: 0x0000FEC8
		public override void RemoveFromMyItems(int itemIndex, int amount, bool dispatch)
		{
			base.RemoveFromMyItems(itemIndex, amount, dispatch);
			if (SceneManager.isServer)
			{
				this.svPlayer.CheckServerEquipment(itemIndex);
				this.CheckEquipment(itemIndex, -amount);
				return;
			}
			if (this.clPlayer.isMain)
			{
				this.CheckEquipment(itemIndex, -amount);
				if (dispatch)
				{
					MonoBehaviourSingleton<ClManager>.Instance.ShowInventoryMessage(itemIndex, -amount);
				}
			}
		}

		// Token: 0x04000515 RID: 1301
		public CharacterType characterType;

		// Token: 0x04000516 RID: 1302
		[NonSerialized]
		public StanceType[] stances;

		// Token: 0x04000517 RID: 1303
		protected readonly int[] injuryDecay = new int[]
		{
			20,
			4,
			2,
			1,
			0
		};

		// Token: 0x04000518 RID: 1304
		[SerializeField]
		protected Transform rotationT;

		// Token: 0x04000519 RID: 1305
		public ClPlayer clPlayer;

		// Token: 0x0400051A RID: 1306
		public SvPlayer svPlayer;

		// Token: 0x0400051B RID: 1307
		public string hands;

		// Token: 0x0400051C RID: 1308
		public string surrender;

		// Token: 0x0400051D RID: 1309
		[NonSerialized]
		public ShPlayer specPlayer;

		// Token: 0x0400051E RID: 1310
		[NonSerialized]
		public string username;

		// Token: 0x0400051F RID: 1311
		[NonSerialized]
		public string displayName;

		// Token: 0x04000520 RID: 1312
		[NonSerialized]
		public Language language;

		// Token: 0x04000521 RID: 1313
		public WearableOptions[] wearableOptions;

		// Token: 0x04000522 RID: 1314
		public CapsuleCollider capsule;

		// Token: 0x04000523 RID: 1315
		public Dictionary<Type, int> cachedItems = new Dictionary<Type, int>();

		// Token: 0x04000524 RID: 1316
		public bool boss;

		// Token: 0x04000525 RID: 1317
		public Vector3 moveFactor = new Vector3(12f, 12f, 12f);

		// Token: 0x04000526 RID: 1318
		[NonSerialized]
		public Vector3 input;

		// Token: 0x04000527 RID: 1319
		[NonSerialized]
		public byte mode;

		// Token: 0x04000528 RID: 1320
		[NonSerialized]
		public ushort chatChannel;

		// Token: 0x04000529 RID: 1321
		[NonSerialized]
		public ChatMode chatMode;

		// Token: 0x0400052A RID: 1322
		[NonSerialized]
		public bool pointing;

		// Token: 0x0400052B RID: 1323
		protected bool armSway;

		// Token: 0x0400052C RID: 1324
		[NonSerialized]
		public Binding[] bindings;

		// Token: 0x0400052D RID: 1325
		[NonSerialized]
		public ShEquipable curEquipable;

		// Token: 0x0400052E RID: 1326
		[NonSerialized]
		public ShWearable[] curWearables = new ShWearable[Enum.GetNames(typeof(WearableType)).Length];

		// Token: 0x0400052F RID: 1327
		[NonSerialized]
		public ShMountable curMount;

		// Token: 0x04000530 RID: 1328
		[NonSerialized]
		public Transform curMountT;

		// Token: 0x04000531 RID: 1329
		[NonSerialized]
		public byte seat;

		// Token: 0x04000532 RID: 1330
		[NonSerialized]
		public StanceType stance;

		// Token: 0x04000533 RID: 1331
		[NonSerialized]
		public float armorLevel;

		// Token: 0x04000534 RID: 1332
		[NonSerialized]
		public Collider headCollider;

		// Token: 0x04000535 RID: 1333
		[NonSerialized]
		public BoxCollider shieldCollider;

		// Token: 0x04000536 RID: 1334
		[NonSerialized]
		public float[] stats = new float[]
		{
			1f,
			1f,
			1f
		};

		// Token: 0x04000537 RID: 1335
		[NonSerialized]
		public bool lockedTrade;

		// Token: 0x04000538 RID: 1336
		public Transform originT;

		// Token: 0x04000539 RID: 1337
		[NonSerialized]
		public bool switching;

		// Token: 0x0400053A RID: 1338
		[NonSerialized]
		public int experience;

		// Token: 0x0400053B RID: 1339
		[NonSerialized]
		public int rank;

		// Token: 0x0400053C RID: 1340
		protected float switchFinishTime;

		// Token: 0x0400053D RID: 1341
		[NonSerialized]
		public ShEntity otherEntity;

		// Token: 0x0400053E RID: 1342
		[NonSerialized]
		public float lockOnTime;

		// Token: 0x0400053F RID: 1343
		[NonSerialized]
		public ShEntity lockOnTarget;

		// Token: 0x04000540 RID: 1344
		[SerializeField]
		protected float jumpVelocity = 14f;

		// Token: 0x04000541 RID: 1345
		[NonSerialized]
		public ClimbState climbState;

		// Token: 0x04000542 RID: 1346
		protected Vector3 climbableNormal;

		// Token: 0x04000543 RID: 1347
		[NonSerialized]
		public float[] injuryAmount = new float[5];

		// Token: 0x04000544 RID: 1348
		[NonSerialized]
		public List<Injury> injuries = new List<Injury>();

		// Token: 0x04000545 RID: 1349
		[NonSerialized]
		public byte fireIndex;

		// Token: 0x04000546 RID: 1350
		[NonSerialized]
		public Dictionary<ShApartment, Place> ownedApartments = new Dictionary<ShApartment, Place>();

		// Token: 0x04000547 RID: 1351
		[NonSerialized]
		public HashSet<ShTransport> ownedTransports = new HashSet<ShTransport>();

		// Token: 0x04000548 RID: 1352
		[NonSerialized]
		public float weight;

		// Token: 0x04000549 RID: 1353
		public float baseWeightLimit = 100f;

		// Token: 0x0400054A RID: 1354
		[NonSerialized]
		public float weightLimit;

		// Token: 0x0400054B RID: 1355
		private Collider ground;

		// Token: 0x0400054C RID: 1356
		private float groundTime;
	}
}
