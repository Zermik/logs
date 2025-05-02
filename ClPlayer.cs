using System;
using System.Collections;
using System.Collections.Generic;
using BrokeProtocol.Client.UI;
using BrokeProtocol.Collections;
using BrokeProtocol.Managers;
using BrokeProtocol.Required;
using BrokeProtocol.Utility;
using BrokeProtocol.Utility.Jobs;
using BrokeProtocol.Utility.Networking;
using ENet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace BrokeProtocol.Entities
{
	// Token: 0x020000A2 RID: 162
	public class ClPlayer : ClMovable
	{
		// Token: 0x06000296 RID: 662 RVA: 0x0000CCA3 File Offset: 0x0000AEA3
		public void UpdateVoiceVolume()
		{
			if (this.voice)
			{
				this.voice.audioSource.volume = this.voiceVolume * MonoBehaviourSingleton<ClManager>.Instance.VoiceVolume;
			}
		}

		// Token: 0x06000297 RID: 663 RVA: 0x0000CCD3 File Offset: 0x0000AED3
		public GameObject GetIdentity()
		{
			if (this.identityObject == null)
			{
				this.identityObject = Util.identityBuffer.Execute(this.player);
			}
			return this.identityObject;
		}

		// Token: 0x06000298 RID: 664 RVA: 0x0000CCFF File Offset: 0x0000AEFF
		private IEnumerator DisableTextTimer(Text t, float delay)
		{
			t.enabled = true;
			yield return new WaitForSeconds(delay);
			if (t)
			{
				t.enabled = false;
			}
			yield break;
		}

		// Token: 0x06000299 RID: 665 RVA: 0x0000CD15 File Offset: 0x0000AF15
		private IEnumerator DisableIconTimer(SpriteRenderer r, float delay)
		{
			r.enabled = true;
			yield return new WaitForSeconds(delay);
			if (r)
			{
				r.enabled = false;
			}
			yield break;
		}

		// Token: 0x0600029A RID: 666 RVA: 0x0000CD2B File Offset: 0x0000AF2B
		public void NameLoop(Text t)
		{
			if (this.nameCoroutine != null)
			{
				base.StopCoroutine(this.nameCoroutine);
			}
			this.nameCoroutine = base.StartCoroutine(this.DisableTextTimer(t, 1f));
		}

		// Token: 0x0600029B RID: 667 RVA: 0x0000CD59 File Offset: 0x0000AF59
		public void MessageLoop(Text t)
		{
			if (this.messageCoroutine != null)
			{
				base.StopCoroutine(this.messageCoroutine);
			}
			this.messageCoroutine = base.StartCoroutine(this.DisableTextTimer(t, 10f));
		}

		// Token: 0x0600029C RID: 668 RVA: 0x0000CD87 File Offset: 0x0000AF87
		public void IconLoop(SpriteRenderer r)
		{
			if (this.iconCoroutine != null)
			{
				base.StopCoroutine(this.iconCoroutine);
			}
			this.iconCoroutine = base.StartCoroutine(this.DisableIconTimer(r, 0.12f));
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x0600029D RID: 669 RVA: 0x0000CDB5 File Offset: 0x0000AFB5
		public bool SpecMain
		{
			get
			{
				return this.clManager.myPlayer != null && this.clManager.myPlayer.specPlayer == this.player;
			}
		}

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x0600029E RID: 670 RVA: 0x0000CDE7 File Offset: 0x0000AFE7
		public bool FirstPerson
		{
			get
			{
				return MonoBehaviourSingleton<MainCamera>.Instance.cameraMode == CameraMode.FirstPerson && this.player.IsUp;
			}
		}

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x0600029F RID: 671 RVA: 0x0000CE02 File Offset: 0x0000B002
		public bool EquipmentAimed
		{
			get
			{
				return Quaternion.Angle(this.player.Rotation, this.player.curEquipable.mainT.rotation) <= 25f;
			}
		}

		// Token: 0x060002A0 RID: 672 RVA: 0x0000CE34 File Offset: 0x0000B034
		public override void RefreshActions()
		{
			base.RefreshActions();
			this.actions.Add(new ActionInfo(ButtonIndex.Player, () => this.player.CanFollow, new Action(this.FollowerAction)));
			this.actions.Add(new ActionInfo(ButtonIndex.Player, new Func<bool>(this.InviteTest), new Action(this.InviteAction)));
			this.actions.Add(new ActionInfo(ButtonIndex.Player, new Func<bool>(this.KickOutTest), new Action(this.KickOutAction)));
			this.actions.Add(new ActionInfo(ButtonIndex.Player, new Func<bool>(this.GetJobTest), new Action(this.GetJobAction)));
			this.actions.Add(new ActionInfo(ButtonIndex.Player, new Func<bool>(this.QuitJobTest), new Action(this.QuitJobAction)));
			this.actions.Add(new ActionInfo(ButtonIndex.Player, new Func<bool>(this.TradeTest), new Action(this.TradeRequestAction)));
			this.actions.Add(new ActionInfo(ButtonIndex.Player, new Func<bool>(this.FreeTest), new Action(this.FreeAction)));
			this.actions.Add(new ActionInfo(ButtonIndex.Player, () => !this.player.IsDead, new Action(this.ShowHealthAction)));
			this.actions.Add(new ActionInfo(ButtonIndex.Player, new Func<bool>(this.DisembarkTest), new Action(this.DisembarkAction)));
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x0000CFC1 File Offset: 0x0000B1C1
		protected void FollowerAction()
		{
			this.clManager.SendToServer(PacketFlags.Reliable, SvPacket.Follower, new object[]
			{
				this.player.ID
			});
		}

		// Token: 0x060002A2 RID: 674 RVA: 0x0000CFEC File Offset: 0x0000B1EC
		protected bool InviteTest()
		{
			if (this.player.isHuman && this.player.IsUp && this.clManager.myPlayer.IsMobile && !this.clManager.myPlayer.InOwnApartment)
			{
				using (Dictionary<ShApartment, Place>.KeyCollection.Enumerator enumerator = this.clManager.myPlayer.ownedApartments.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.DistanceSqr(this.player.Position) <= 900f)
						{
							return true;
						}
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x060002A3 RID: 675 RVA: 0x0000D0A8 File Offset: 0x0000B2A8
		protected void InviteAction()
		{
			this.clManager.SendToServer(PacketFlags.Reliable, SvPacket.Invite, new object[]
			{
				this.player.ID
			});
		}

		// Token: 0x060002A4 RID: 676 RVA: 0x0000D0D1 File Offset: 0x0000B2D1
		protected bool KickOutTest()
		{
			return this.player.isHuman && this.player.IsUp && this.clManager.myPlayer.IsMobile && this.clManager.myPlayer.InOwnApartment;
		}

		// Token: 0x060002A5 RID: 677 RVA: 0x0000D111 File Offset: 0x0000B311
		protected void KickOutAction()
		{
			this.clManager.SendToServer(PacketFlags.Reliable, SvPacket.KickOut, new object[]
			{
				this.player.ID
			});
		}

		// Token: 0x060002A6 RID: 678 RVA: 0x0000D13A File Offset: 0x0000B33A
		protected override bool ShopTest()
		{
			return !this.player.IsDead && base.ShopTest();
		}

		// Token: 0x060002A7 RID: 679 RVA: 0x0000D151 File Offset: 0x0000B351
		protected bool TradeTest()
		{
			return base.InventoryTest && !this.entity.Shop;
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x0000D16B File Offset: 0x0000B36B
		protected void TradeRequestAction()
		{
			this.clManager.SendToServer(PacketFlags.Reliable, SvPacket.TradeRequest, new object[]
			{
				this.player.ID
			});
		}

		// Token: 0x060002A9 RID: 681 RVA: 0x0000D194 File Offset: 0x0000B394
		protected bool ActiveBossTest()
		{
			return this.player.boss && !this.player.IsDead;
		}

		// Token: 0x060002AA RID: 682 RVA: 0x0000D1B3 File Offset: 0x0000B3B3
		protected bool GetJobTest()
		{
			return this.ActiveBossTest() && this.job != this.clManager.myPlayer.clPlayer.job;
		}

		// Token: 0x060002AB RID: 683 RVA: 0x0000D1DF File Offset: 0x0000B3DF
		protected bool QuitJobTest()
		{
			return this.ActiveBossTest() && this.job == this.clManager.myPlayer.clPlayer.job;
		}

		// Token: 0x060002AC RID: 684 RVA: 0x0000D208 File Offset: 0x0000B408
		protected void GetJobAction()
		{
			this.clManager.SendToServer(PacketFlags.Reliable, SvPacket.GetJob, new object[]
			{
				this.player.ID
			});
		}

		// Token: 0x060002AD RID: 685 RVA: 0x0000D231 File Offset: 0x0000B431
		protected void QuitJobAction()
		{
			this.clManager.SendToServer(PacketFlags.Reliable, SvPacket.QuitJob, new object[]
			{
				this.player.ID
			});
		}

		// Token: 0x060002AE RID: 686 RVA: 0x0000D25A File Offset: 0x0000B45A
		protected bool FreeTest()
		{
			return !this.player.IsDead && this.player.IsRestrained;
		}

		// Token: 0x060002AF RID: 687 RVA: 0x0000D276 File Offset: 0x0000B476
		protected void FreeAction()
		{
			this.clManager.SendToServer(PacketFlags.Reliable, SvPacket.Free, new object[]
			{
				this.player.ID
			});
		}

		// Token: 0x060002B0 RID: 688 RVA: 0x0000D29F File Offset: 0x0000B49F
		protected void ShowHealthAction()
		{
			this.clManager.SendToServer(PacketFlags.Reliable, SvPacket.ShowHealth, new object[]
			{
				this.player.ID
			});
		}

		// Token: 0x060002B1 RID: 689 RVA: 0x0000D2C8 File Offset: 0x0000B4C8
		protected bool DisembarkTest()
		{
			return !this.clManager.myPlayer.IsRestrained && this.player.curMount && this.player.IsRestrained;
		}

		// Token: 0x060002B2 RID: 690 RVA: 0x0000D2FB File Offset: 0x0000B4FB
		protected void DisembarkAction()
		{
			this.clManager.SendToServer(PacketFlags.Reliable, SvPacket.Disembark, new object[]
			{
				this.player.ID
			});
		}

		// Token: 0x060002B3 RID: 691 RVA: 0x0000D324 File Offset: 0x0000B524
		public void Jump()
		{
			this.entity.animator.SetTrigger(Animations.jump);
			this.player.Jump();
		}

		// Token: 0x060002B4 RID: 692 RVA: 0x0000D348 File Offset: 0x0000B548
		public override void ClMove()
		{
			base.ClMove();
			if (this.player.stance.ragdoll)
			{
				this.ragdollBodies[0].AddForce(100f * (this.player.Position - this.ragdollBodies[0].position), ForceMode.Acceleration);
				foreach (Rigidbody rigidbody in this.ragdollBodies)
				{
					if (this.player.IsOutside && Util.InWater(rigidbody.worldCenterOfMass))
					{
						rigidbody.linearDamping = (rigidbody.angularDamping = 2f);
						Util.DoBuoyancy(rigidbody.worldCenterOfMass, rigidbody);
					}
					else
					{
						rigidbody.linearDamping = (rigidbody.angularDamping = 0.1f);
					}
				}
			}
		}

		// Token: 0x060002B5 RID: 693 RVA: 0x0000D410 File Offset: 0x0000B610
		protected void Update()
		{
			if (this.player.IsUp)
			{
				if (!this.player.controller.clPlayer.isMain)
				{
					float t = (MonoBehaviourSingleton<SceneManager>.Instance.time - 0.1f - this.previousState.timestamp) * 10f;
					this.player.RotationT.rotation = Quaternion.Slerp(this.previousState.rotation, this.latestState.rotation, t);
				}
				Vector3 vector = this.player.mainT.InverseTransformDirection(this.player.RotationT.forward);
				this.player.animator.SetFloat(Animations.viewVertical, Mathf.Atan2(vector.y, vector.z) / -1.5707964f);
				if (this.player.stance.fixedForward)
				{
					Vector3 direction = Vector3.ProjectOnPlane(this.player.mainT.forward, this.player.RotationT.up);
					vector = this.player.RotationT.InverseTransformDirection(direction);
					this.player.animator.SetFloat(Animations.viewHorizontal, Mathf.Atan2(vector.x, vector.z) / -2.1048672f);
				}
				else
				{
					this.player.animator.SetFloat(Animations.viewHorizontal, 0f);
				}
				if (this.player.relativeVelocity.sqrMagnitude >= 0.1f)
				{
					Vector3 vector2 = this.player.mainT.InverseTransformDirection(Vector3.ProjectOnPlane(this.player.relativeVelocity, Vector3.up));
					float num = this.player.stance.input * this.player.maxSpeed * 1.25f;
					if (num > 0.1f)
					{
						this.player.animator.SetFloat(Animations.velocityForward, Util.SafePow(vector2.z / num, 0.65f), 0.1f, Time.deltaTime);
						this.player.animator.SetFloat(Animations.velocityRight, Util.SafePow(vector2.x / num, 0.65f), 0.1f, Time.deltaTime);
					}
					else
					{
						this.player.animator.SetFloat(Animations.velocityForward, 0f);
						this.player.animator.SetFloat(Animations.velocityRight, 0f);
					}
				}
				else
				{
					this.player.animator.SetFloat(Animations.velocityForward, 0f, 0.1f, Time.deltaTime);
					this.player.animator.SetFloat(Animations.velocityRight, 0f, 0.1f, Time.deltaTime);
				}
				this.player.animator.SetBool(Animations.sprint, this.player.mode == 1);
				this.player.animator.SetBool(Animations.zoom, this.player.mode == 3);
				this.player.animator.SetBool(Animations.point, this.player.pointing);
				this.player.animator.SetBool(Animations.swimming, this.player.InWater);
			}
			Bounds bounds = new Bounds(this.player.originT.position, Vector3.one);
			foreach (Transform transform in this.skinnedMeshRenderer.bones)
			{
				bounds.Encapsulate(transform.position);
			}
			this.skinnedMeshRenderer.bounds = bounds;
			foreach (ShWearable shWearable in this.player.curWearables)
			{
				if (shWearable.skinnedMesh)
				{
					shWearable.skinnedMesh.bounds = bounds;
				}
			}
		}

		// Token: 0x060002B6 RID: 694 RVA: 0x0000D7EC File Offset: 0x0000B9EC
		protected void LateUpdate()
		{
			if (this.player.StanceIndex == StanceIndex.Recovering)
			{
				float blend = Mathf.Clamp01((Time.time - this.blendStartTime) / 0.5f);
				foreach (ClPlayer.Bone bone in this.bones)
				{
					bone.MoveTransform(blend);
				}
			}
			this.headBone.localScale = ((this.SpecMain && this.FirstPerson) ? Vector3.zero : Vector3.one);
		}

		// Token: 0x060002B7 RID: 695 RVA: 0x0000D86C File Offset: 0x0000BA6C
		public void BuyApartment(int apartmentID, int placeIndex)
		{
			ShApartment shApartment;
			if (EntityCollections.TryFindByID<ShApartment>(apartmentID, out shApartment))
			{
				this.player.ownedApartments[shApartment] = MonoBehaviourSingleton<SceneManager>.Instance.places[placeIndex];
				this.clManager.ShowGameMessage((shApartment.isGarage ? "Garage" : "Apartment") + " bought!");
			}
		}

		// Token: 0x060002B8 RID: 696 RVA: 0x0000D8D0 File Offset: 0x0000BAD0
		public void SellApartment(int apartmentID)
		{
			ShApartment key;
			if (EntityCollections.TryFindByID<ShApartment>(apartmentID, out key))
			{
				this.player.ownedApartments.Remove(key);
				this.clManager.ShowGameMessage("Property sold!");
			}
		}

		// Token: 0x060002B9 RID: 697 RVA: 0x0000D90C File Offset: 0x0000BB0C
		public void Experience(int experience, bool showMessage)
		{
			if (showMessage)
			{
				if (experience > this.player.experience)
				{
					int num = experience - this.player.experience;
					this.clManager.ShowGameMessage("Gained " + num.ToString() + " XP");
				}
				else if (experience < this.player.experience)
				{
					int num2 = this.player.experience - experience;
					this.clManager.ShowGameMessage("Lost " + num2.ToString() + " XP");
				}
			}
			this.player.experience = experience;
			this.clManager.hud.UpdateExperience();
		}

		// Token: 0x060002BA RID: 698 RVA: 0x0000D9B4 File Offset: 0x0000BBB4
		public void Rank(int rank)
		{
			if (rank > this.player.rank)
			{
				this.clManager.ShowGameMessage("Rank up!");
			}
			else if (rank < this.player.rank)
			{
				this.clManager.ShowGameMessage("Rank down!");
			}
			this.player.rank = rank;
			this.clManager.hud.UpdateJob();
		}

		// Token: 0x060002BB RID: 699 RVA: 0x0000DA1B File Offset: 0x0000BC1B
		private IEnumerator SmoothStance(StanceIndex stanceIndex, float animationLength)
		{
			float startTime = Time.time;
			float endTime = startTime + animationLength;
			Vector3 originalOffset = this.player.RotationT.localPosition;
			float originalCapsuleHeight = this.player.capsule.height;
			while (Time.time < endTime)
			{
				yield return null;
				if (stanceIndex != this.player.StanceIndex)
				{
					yield break;
				}
				float t = (Time.time - startTime) / animationLength;
				this.player.RotationT.localPosition = Vector3.Lerp(originalOffset, this.player.stance.offset, t);
				this.player.SetCapsuleHeight(Mathf.Lerp(originalCapsuleHeight, this.player.stance.capsuleHeight, t));
			}
			this.player.SetStance(stanceIndex);
			yield break;
		}

		// Token: 0x060002BC RID: 700 RVA: 0x0000DA38 File Offset: 0x0000BC38
		public void SetStance(StanceIndex stanceIndex)
		{
			StanceType stanceType = this.player.stances[(int)stanceIndex];
			if (stanceType.ragdoll)
			{
				this.SetRagdoll(true);
				this.player.SetStance(stanceIndex);
				return;
			}
			this.SetRagdoll(false);
			float animationLength;
			if (stanceIndex == StanceIndex.Recovering)
			{
				this.blendStartTime = Time.time;
				for (int i = 0; i < this.bones.Length; i++)
				{
					this.bones[i].UpdateStoredTransform();
				}
				this.entity.animator.SetTrigger((this.skinnedMeshRenderer.bones[0].forward.y > 0f) ? Animations.getUpBack : Animations.getUpFront);
				animationLength = 2f;
			}
			else
			{
				animationLength = 0.1f;
			}
			if (base.isActiveAndEnabled)
			{
				this.player.stance = stanceType;
				base.StartCoroutine(this.SmoothStance(stanceIndex, animationLength));
				this.entity.animator.SetInteger(Animations.stance, (int)stanceIndex);
				this.entity.animator.SetTrigger(Animations.changeStance);
				this.entity.animator.SetBool(Animations.fixedForward, !this.player.CanAltFire());
				return;
			}
			this.player.SetStance(stanceIndex);
		}

		// Token: 0x060002BD RID: 701 RVA: 0x0000DB70 File Offset: 0x0000BD70
		public void SetJob(int jobIndex)
		{
			this.job = this.clManager.jobs[jobIndex];
			if (this.player == this.clManager.myPlayer)
			{
				this.clManager.ShowGameMessage("New Role: " + this.job.jobName);
				this.clManager.ShowGameMessage(this.job.jobDescription);
				this.player.experience = 0;
				this.player.rank = 0;
				this.clManager.hud.UpdateJob();
				this.clManager.hud.UpdateExperience();
			}
		}

		// Token: 0x060002BE RID: 702 RVA: 0x0000DC1C File Offset: 0x0000BE1C
		public override void Initialize()
		{
			base.Initialize();
			if (this.isMain)
			{
				Array.Copy(MonoBehaviourSingleton<SceneManager>.Instance.entityData, Buffers.readBuffer, MonoBehaviourSingleton<SceneManager>.Instance.entityData.Length);
				Buffers.reader.SeekZero();
				while (Buffers.reader.Position < MonoBehaviourSingleton<SceneManager>.Instance.entityData.Length)
				{
					MonoBehaviourSingleton<ClManager>.Instance.AddEntity();
				}
				this.clManager.SendToServer(PacketFlags.Reliable, SvPacket.Initialized, Array.Empty<object>());
				this.SetControlledPhysics(true);
			}
			else if (this.player.isHuman)
			{
				this.clManager.ShowGameMessage(this.player.displayName + " connected");
			}
			this.bones = new ClPlayer.Bone[this.skinnedMeshRenderer.bones.Length];
			for (int i = 0; i < this.bones.Length; i++)
			{
				this.bones[i] = new ClPlayer.Bone(this.skinnedMeshRenderer.bones[i]);
			}
			Transform transform = this.skinnedMeshRenderer.bones[0];
			this.SetIgnoreForce(transform);
			this.ragdollBodies = transform.GetComponentsInChildren<Rigidbody>(true);
			this.ragdollColliders = transform.GetComponentsInChildren<Collider>(true);
			this.SetRagdoll(false);
		}

		// Token: 0x060002BF RID: 703 RVA: 0x0000DD50 File Offset: 0x0000BF50
		private void SetIgnoreForce(Transform t)
		{
			t.tag = "IgnoreForce";
			foreach (object obj in t)
			{
				Transform transform = (Transform)obj;
				CharacterJoint characterJoint;
				if (!transform.TryGetComponent<CharacterJoint>(out characterJoint) || characterJoint.breakForce == float.PositiveInfinity)
				{
					this.SetIgnoreForce(transform);
				}
			}
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x0000DDC8 File Offset: 0x0000BFC8
		public override void Destroy()
		{
			PlayersMenu playersMenu = this.clManager.CurrentMenu as PlayersMenu;
			if (playersMenu != null)
			{
				playersMenu.ClearButtons(new object[]
				{
					this.player
				});
			}
			if (this.player.isHuman)
			{
				this.clManager.ShowGameMessage(this.player.displayName + " disconnected");
			}
			if (this.voice)
			{
				Object.Destroy(this.voice.gameObject);
				this.voice = null;
			}
			base.Destroy();
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x0000DE58 File Offset: 0x0000C058
		private void SetRagdoll(bool setting)
		{
			if (this.entity.animator.enabled != setting)
			{
				return;
			}
			this.entity.animator.enabled = !setting;
			Collider[] array = this.ragdollColliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = setting;
			}
			ShPlayer entity = MonoBehaviourSingleton<SceneManager>.Instance.GetEntity<ShPlayer>(this.player.index);
			if (setting)
			{
				for (int j = 0; j < this.bones.Length; j++)
				{
					Transform transform = this.skinnedMeshRenderer.bones[j];
					CharacterJoint characterJoint;
					CharacterJoint characterJoint2;
					if (entity.clPlayer.skinnedMeshRenderer.bones[j].TryGetComponent<CharacterJoint>(out characterJoint) && characterJoint.breakForce < float.PositiveInfinity && transform.TryGetComponent<CharacterJoint>(out characterJoint2))
					{
						if (this.player.IsDead)
						{
							characterJoint2.breakForce = characterJoint.breakForce;
							characterJoint2.breakTorque = characterJoint.breakTorque;
						}
						else
						{
							characterJoint2.breakForce = (characterJoint2.breakTorque = float.PositiveInfinity);
						}
					}
				}
				foreach (Rigidbody rigidbody in this.ragdollBodies)
				{
					rigidbody.constraints = RigidbodyConstraints.None;
					rigidbody.isKinematic = false;
					rigidbody.linearVelocity = this.player.Velocity;
					rigidbody.angularVelocity = Vector3.zero;
				}
				return;
			}
			foreach (Rigidbody rigidbody2 in this.ragdollBodies)
			{
				rigidbody2.constraints = RigidbodyConstraints.FreezeAll;
				rigidbody2.isKinematic = true;
			}
			for (int k = 1; k < this.bones.Length; k++)
			{
				Transform transform2 = this.skinnedMeshRenderer.bones[k];
				Transform transform3 = entity.clPlayer.skinnedMeshRenderer.bones[k];
				transform2.localPosition = transform3.localPosition;
				CharacterJoint characterJoint3;
				CharacterJoint characterJoint4;
				if (!transform2.TryGetComponent<CharacterJoint>(out characterJoint3) && transform3.TryGetComponent<CharacterJoint>(out characterJoint4))
				{
					CharacterJoint characterJoint5 = transform2.gameObject.AddComponent<CharacterJoint>();
					characterJoint5.connectedBody = transform2.parent.GetComponentInParent<Rigidbody>();
					characterJoint5.anchor = characterJoint4.anchor;
					characterJoint5.axis = characterJoint4.axis;
					characterJoint5.autoConfigureConnectedAnchor = characterJoint4.autoConfigureConnectedAnchor;
					characterJoint5.connectedAnchor = characterJoint4.connectedAnchor;
					characterJoint5.swingAxis = characterJoint4.swingAxis;
					characterJoint5.twistLimitSpring = characterJoint4.twistLimitSpring;
					characterJoint5.lowTwistLimit = characterJoint4.lowTwistLimit;
					characterJoint5.highTwistLimit = characterJoint4.highTwistLimit;
					characterJoint5.swingLimitSpring = characterJoint4.swingLimitSpring;
					characterJoint5.swing1Limit = characterJoint4.swing1Limit;
					characterJoint5.swing2Limit = characterJoint4.swing2Limit;
					characterJoint5.breakForce = characterJoint4.breakForce;
					characterJoint5.breakTorque = characterJoint4.breakTorque;
					characterJoint5.enablePreprocessing = characterJoint4.enablePreprocessing;
				}
			}
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x0000E113 File Offset: 0x0000C313
		public IEnumerator Footsteps()
		{
			while (!this.player.IsDead)
			{
				AudioClip[] arr;
				if (this.player.IsUp && !this.player.curMount && this.player.GetGround() && !this.player.maxSpeed.IsZero() && this.clManager.footsteps.TryGetValue(this.player.GetGround().tag, out arr))
				{
					float num = this.player.relativeVelocity.magnitude / this.player.maxSpeed;
					if (num > 0.1f)
					{
						this.footstepSource.volume = num;
						this.footstepSource.clip = arr.GetRandom<AudioClip>();
						this.footstepSource.PlayRandomPitch(0.05f);
					}
					yield return new WaitForSeconds(0.5f * this.player.animator.GetCurrentAnimatorStateInfo(0).length);
				}
				else
				{
					yield return null;
				}
			}
			yield break;
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x0000E124 File Offset: 0x0000C324
		public override void ReadActivateData()
		{
			base.ReadActivateData();
			this.entity.animator.SetBool(Animations.gesture, false);
			int equipableIndex = Buffers.reader.ReadInt32();
			this.player.SetEquipable(equipableIndex, !this.isMain);
			this.DeserializeAttachments(Buffers.reader.ReadBytesAndSize());
			this.DeserializeWearables(Buffers.reader.ReadBytesAndSize());
			this.player.pointing = Buffers.reader.ReadBoolean();
			this.player.mode = Buffers.reader.ReadByte();
			int num = Buffers.reader.ReadInt32();
			byte b = Buffers.reader.ReadByte();
			if (num > 0)
			{
				if (this.player.curMount && (num != this.player.curMount.ID || this.player.seat != b))
				{
					this.Dismount();
				}
				this.Mount(num, b, -1);
				return;
			}
			if (!this.player.IsDead)
			{
				if (this.player.curMount)
				{
					this.Dismount();
				}
				this.SetStance((StanceIndex)b);
			}
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x0000E241 File Offset: 0x0000C441
		public IEnumerator SendUpdatesToServer()
		{
			WaitForSeconds delay = new WaitForSeconds(0.1f);
			while (!this.player.IsDead)
			{
				if (this.player.curMount)
				{
					ShPhysical shPhysical;
					if (this.player.IsControlledMount<ShPhysical>(out shPhysical) && shPhysical.clPhysical.ClientsideController)
					{
						int num;
						Vector3 vector;
						Quaternion quaternion;
						if (shPhysical.InWater)
						{
							this.clManager.SendToServer(PacketFlags.Unthrottled, SvPacket.UpdateMountWater, new object[]
							{
								shPhysical.Position - new Vector3(0f, MonoBehaviourSingleton<SceneManager>.Instance.WaterLevel(shPhysical.Position), 0f),
								shPhysical.Rotation
							});
						}
						else if (shPhysical.GroundEntityOffsets(out num, out vector, out quaternion))
						{
							this.clManager.SendToServer(PacketFlags.Unthrottled, SvPacket.UpdateMountOffset, new object[]
							{
								num,
								vector,
								quaternion
							});
						}
						else
						{
							this.clManager.SendToServer(PacketFlags.Unthrottled, SvPacket.UpdateMount, new object[]
							{
								shPhysical.Position,
								shPhysical.Rotation
							});
						}
					}
					this.clManager.SendToServer(PacketFlags.Unthrottled, SvPacket.UpdateRotation, new object[]
					{
						this.player.Rotation
					});
				}
				else if (!this.player.OutsideController)
				{
					int num2;
					Vector3 vector2;
					Quaternion quaternion2;
					if (this.player.InWater)
					{
						this.clManager.SendToServer(PacketFlags.Unthrottled, SvPacket.UpdatePlayerWater, new object[]
						{
							this.player.Position - new Vector3(0f, MonoBehaviourSingleton<SceneManager>.Instance.WaterLevel(this.player.Position), 0f),
							this.player.Rotation
						});
					}
					else if (this.player.GroundEntityOffsets(out num2, out vector2, out quaternion2))
					{
						this.clManager.SendToServer(PacketFlags.Unthrottled, SvPacket.UpdatePlayerOffset, new object[]
						{
							num2,
							vector2,
							quaternion2
						});
					}
					else
					{
						this.clManager.SendToServer(PacketFlags.Unthrottled, SvPacket.UpdatePlayer, new object[]
						{
							this.player.Position,
							this.player.Rotation
						});
					}
				}
				yield return delay;
			}
			yield break;
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x0000E250 File Offset: 0x0000C450
		public void AssignToActor(SkinnedMeshRenderer wearableRenderer)
		{
			Transform[] array = this.skinnedMeshRenderer.bones;
			Transform[] array2 = wearableRenderer.bones;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = array[i];
			}
			wearableRenderer.bones = array2;
			wearableRenderer.rootBone = this.skinnedMeshRenderer.rootBone;
			wearableRenderer.localBounds = this.skinnedMeshRenderer.localBounds;
			Cloth cloth;
			if (wearableRenderer.TryGetComponent<Cloth>(out cloth))
			{
				cloth.enabled = true;
			}
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x0000E2C0 File Offset: 0x0000C4C0
		public void ChatVoice(int sourceID, byte[] data, ChatMode mode = ChatMode.Public)
		{
			if (!this.voice)
			{
				this.voice = Object.Instantiate<Voice>(MonoBehaviourSingleton<ClManager>.Instance.voicePrefab);
				this.UpdateVoiceVolume();
			}
			this.voice.Decode(data);
			if (mode == ChatMode.Public)
			{
				this.voice.myT.SetParent(this.player.mainT, false);
				Util.identityBuffer.SetIcon(this.GetIdentity());
				return;
			}
			this.voice.myT.SetParent(this.clManager.myPlayer.specPlayer.mainT, false);
			ChatVoiceLabel chatVoiceLabel;
			if (this.clManager.chatVoiceLabels.TryGetValue(sourceID, out chatVoiceLabel))
			{
				chatVoiceLabel.IncrementLifetime();
				return;
			}
			chatVoiceLabel = Object.Instantiate<ChatVoiceLabel>(this.clManager.chatVoiceLabel, this.clManager.radioBarT);
			this.clManager.chatVoiceLabels.Add(sourceID, chatVoiceLabel);
			chatVoiceLabel.Initialize(sourceID, mode);
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x0000E3AC File Offset: 0x0000C5AC
		public void Spectate(int targetID)
		{
			this.player.specPlayer = EntityCollections.FindByID<ShPlayer>(targetID);
			bool specSelf = this.player.SpecSelf;
			if (!specSelf)
			{
				this.player.Cleanup();
			}
			this.clManager.AllowHUD = specSelf;
			if (this.clManager.uiClone != null)
			{
				this.clManager.uiClone.visible = specSelf;
			}
			MonoBehaviourSingleton<MainCamera>.Instance.SetCamera();
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x0000E418 File Offset: 0x0000C618
		protected override void SetVelocities()
		{
			if (!this.player.curMount)
			{
				base.LinearVelocity();
			}
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x0000E432 File Offset: 0x0000C632
		public void View(int entityID, byte[] inventoryArray)
		{
			this.player.otherEntity = EntityCollections.FindByID(entityID);
			this.player.otherEntity.DeserializeMyItems(inventoryArray);
			this.clManager.ShowViewInventory();
		}

		// Token: 0x060002CA RID: 714 RVA: 0x0000E461 File Offset: 0x0000C661
		public void Shopping(int entityID, byte[] inventoryArray)
		{
			this.player.otherEntity = EntityCollections.FindByID(entityID);
			this.player.otherEntity.DeserializeShop(inventoryArray);
			this.clManager.ShowShoppingingInventory();
		}

		// Token: 0x060002CB RID: 715 RVA: 0x0000E490 File Offset: 0x0000C690
		public void ShowTradeInventory(int entityID)
		{
			this.player.otherEntity = EntityCollections.FindByID(entityID);
			this.clManager.ShowTradeInventory();
		}

		// Token: 0x060002CC RID: 716 RVA: 0x0000E4AE File Offset: 0x0000C6AE
		public void ShowSearchedInventory()
		{
			this.clManager.ShowSearchedInventory();
		}

		// Token: 0x060002CD RID: 717 RVA: 0x0000E4BC File Offset: 0x0000C6BC
		public void SetEquipable(int equipableIndex)
		{
			if (this.player.SetEquipable(equipableIndex, true))
			{
				if (this.isMain)
				{
					MonoBehaviourSingleton<MainCamera>.Instance.PlayerEquipSound();
					this.clManager.hud.ScaleCrosshairs(1f);
				}
				if (base.isActiveAndEnabled)
				{
					this.player.curEquipable.clEquipable.StartDelayRender();
				}
			}
		}

		// Token: 0x060002CE RID: 718 RVA: 0x0000E51C File Offset: 0x0000C71C
		public void FinishTrade(bool isGood)
		{
			this.player.FinishTrade(isGood);
			this.player.tradeItems.Clear();
			this.clManager.DestroyMenu("Default");
		}

		// Token: 0x060002CF RID: 719 RVA: 0x0000E54C File Offset: 0x0000C74C
		public void UpdateShopValue(int itemIndex, int newValue)
		{
			InventoryItem inventoryItem;
			if (this.player.otherEntity && this.player.otherEntity.myItems.TryGetValue(itemIndex, out inventoryItem))
			{
				inventoryItem.currentValue = newValue;
				this.clManager.RefreshListMenu<InventoryMenu>(new object[]
				{
					inventoryItem.item.index
				});
			}
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x0000E5B0 File Offset: 0x0000C7B0
		public void UpdateMountAmmo(int ammoCount)
		{
			this.player.curMount.SetCurrentClip(ammoCount, (int)this.player.seat);
			this.clManager.hud.UpdateSecondaryAmmo();
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x0000E5E0 File Offset: 0x0000C7E0
		public void Mount(int mountableID, byte enterSeat, int ammoCount = -1)
		{
			ShMountable shMountable;
			if (EntityCollections.TryFindByID<ShMountable>(mountableID, out shMountable))
			{
				this.player.Mount(shMountable, enterSeat);
				if (this.player.IsMountController && shMountable.isActiveAndEnabled)
				{
					shMountable.clMountable.TurnOn();
				}
				if (this.player.HiddenInterior)
				{
					this.player.HiddenInterior.SetVisibility(!this.SpecMain || !this.FirstPerson);
				}
				if (this.isMain)
				{
					this.clManager.hud.UpdateMountHealth();
					if (ammoCount >= 0)
					{
						this.UpdateMountAmmo(ammoCount);
					}
					VisualTreeAsset uiAsset = shMountable.seats[(int)enterSeat].uiAsset;
					if (uiAsset)
					{
						this.clManager.uiClone = uiAsset.Instantiate();
						this.clManager.uiClone.pickingMode = PickingMode.Ignore;
						this.clManager.uiClone.visible = this.FirstPerson;
						MonoBehaviourSingleton<SceneManager>.Instance.uiDocument.rootVisualElement.Add(this.clManager.uiClone);
						this.clManager.uiClone.SendToBack();
						this.clManager.uiClone.StretchToParentSize();
					}
				}
				if (!this.player.IsDead)
				{
					this.SetStance(shMountable.seats[(int)enterSeat].stanceIndex);
					this.entity.animator.SetTrigger(Animations.startSwitch);
				}
				this.SetVisibility();
			}
		}

		// Token: 0x060002D2 RID: 722 RVA: 0x0000E74C File Offset: 0x0000C94C
		public void Dismount()
		{
			ShMountable curMount = this.player.curMount;
			if (curMount)
			{
				if (this.player.HiddenInterior)
				{
					this.player.HiddenInterior.SetVisibility(true);
				}
				Seat seat = curMount.seats[(int)this.player.seat];
				if (seat.turretT)
				{
					seat.turretT.localRotation = Quaternion.identity;
				}
				if (seat.barrelT)
				{
					seat.barrelT.localRotation = Quaternion.identity;
				}
				bool isMountController = this.player.IsMountController;
				this.player.Dismount();
				if (isMountController)
				{
					if (curMount.isActiveAndEnabled)
					{
						curMount.clMountable.TurnOff();
					}
					if (this.isMain)
					{
						ShPhysical shPhysical = curMount as ShPhysical;
						if (shPhysical != null)
						{
							shPhysical.clPhysical.ForceLatestState();
						}
					}
				}
				if (this.isMain)
				{
					this.clManager.hud.UpdateMountHealth();
					this.clManager.hud.UpdateSecondaryAmmo();
					if (this.clManager.uiClone != null)
					{
						this.clManager.uiClone.RemoveFromHierarchy();
						this.clManager.uiClone = null;
					}
				}
				if (!this.player.IsDead)
				{
					this.SetStance(StanceIndex.Stand);
					this.entity.animator.SetTrigger(Animations.startSwitch);
				}
				this.SetVisibility();
			}
		}

		// Token: 0x060002D3 RID: 723 RVA: 0x0000E8A8 File Offset: 0x0000CAA8
		private void SetVisibility()
		{
			bool flag = !this.player.HiddenInterior;
			if (this.skinnedMeshRenderer.enabled != flag)
			{
				this.skinnedMeshRenderer.enabled = flag;
				if (this.player.curEquipable)
				{
					this.player.curEquipable.SetVisible(flag);
				}
				foreach (ShWearable shWearable in this.player.curWearables)
				{
					if (shWearable && shWearable.skinnedMesh)
					{
						shWearable.skinnedMesh.enabled = flag;
					}
				}
			}
		}

		// Token: 0x060002D4 RID: 724 RVA: 0x0000E948 File Offset: 0x0000CB48
		public override void UpdateHealth(float newHealth, bool activate)
		{
			float num = newHealth - this.player.health;
			base.UpdateHealth(newHealth, activate);
			if (!activate)
			{
				if (this.player.IsDead)
				{
					if (num < 0f)
					{
						float num2 = newHealth / (-10f * this.player.maxStat);
						Transform[] array = this.skinnedMeshRenderer.bones;
						for (int i = 0; i < array.Length; i++)
						{
							CharacterJoint characterJoint;
							if (array[i].TryGetComponent<CharacterJoint>(out characterJoint) && characterJoint.breakForce < float.PositiveInfinity && Random.value < num2)
							{
								Object.Destroy(characterJoint);
							}
						}
					}
				}
				else if (num < 0f)
				{
					this.hurtSound.clip = ((this.player.health - newHealth > 10f) ? this.clManager.playerHurt : this.clManager.playerHurtMinor);
					this.hurtSound.PlayRandomPitch(0.05f);
				}
				else if (num > 0f)
				{
					this.hurtSound.clip = ((newHealth - this.player.health > 10f) ? this.clManager.playerHeal : this.clManager.playerHealMinor);
					this.hurtSound.PlayRandomPitch(0.05f);
				}
			}
			if (this.isMain)
			{
				this.clManager.hud.healthVisual.UpdateHealth();
				return;
			}
			if (this.identityObject)
			{
				Util.identityBuffer.SetColor(this.identityObject, this.player.HealthToColor);
			}
		}

		// Token: 0x060002D5 RID: 725 RVA: 0x0000EACB File Offset: 0x0000CCCB
		public IEnumerator RaycastEntities()
		{
			WaitForSeconds delay = new WaitForSeconds(1f);
			yield return delay;
			while (!this.player.IsDead)
			{
				if (this.clManager.menus.Count == 0 && !MonoBehaviourSingleton<SceneManager>.Instance.inputSystemActions.UI.Submit.IsPressed() && this.player.IsUp)
				{
					RaycastHit raycastHit;
					if (Physics.Raycast(this.player.originT.position, this.player.originT.forward, out raycastHit, 24f, 9985))
					{
						ClPlayer componentInParent = raycastHit.collider.GetComponentInParent<ClPlayer>();
						if (componentInParent && componentInParent != this)
						{
							Util.identityBuffer.SetName(componentInParent.GetIdentity(), componentInParent.player.displayName);
						}
						if (componentInParent != this && raycastHit.distance < 8f)
						{
							ShEntity componentInParent2 = raycastHit.collider.GetComponentInParent<ShEntity>();
							if (componentInParent2)
							{
								if (componentInParent2 != this.clManager.highlightEntity)
								{
									this.clManager.ClearHighlightEntity();
									this.clManager.highlightEntity = componentInParent2;
									this.clManager.EntityActionMenu();
								}
								yield return delay;
								continue;
							}
						}
					}
					if (this.clManager.highlightEntity)
					{
						this.clManager.ClearHighlightEntity();
					}
				}
				yield return null;
			}
			yield break;
		}

		// Token: 0x060002D6 RID: 726 RVA: 0x0000EADC File Offset: 0x0000CCDC
		public override void ReadInitData()
		{
			base.ReadInitData();
			this.player.isHuman = Buffers.reader.ReadBoolean();
			this.isMain = Buffers.reader.ReadBoolean();
			this.player.username = Buffers.reader.ReadString();
			this.DisplayName(Buffers.reader.ReadString().ParseColorCodes());
			this.SetJob(Buffers.reader.ReadInt32());
			this.player.profile = Buffers.reader.ReadString();
			this.player.language = Util.languages[Buffers.reader.ReadInt32()];
		}

		// Token: 0x060002D7 RID: 727 RVA: 0x0000EB7E File Offset: 0x0000CD7E
		public void DisplayName(string displayName)
		{
			this.player.displayName = displayName.ParseColorCodes();
		}

		// Token: 0x060002D8 RID: 728 RVA: 0x0000EB94 File Offset: 0x0000CD94
		public void DeserializeHealth(byte[] healthArray)
		{
			this.player.injuries.Clear();
			if (healthArray == null)
			{
				return;
			}
			int i = 0;
			for (int j = 0; j < this.player.stats.Length; j++)
			{
				this.player.stats[j] = MyConverter.ToSingle(healthArray, i);
				i += 4;
			}
			while (i < healthArray.Length)
			{
				byte part = healthArray[i];
				i++;
				byte effect = healthArray[i];
				i++;
				byte amount = healthArray[i];
				i++;
				this.player.injuries.Add(new Injury((BodyPart)part, (BodyEffect)effect, amount));
			}
			this.player.UpdateInjuries();
		}

		// Token: 0x060002D9 RID: 729 RVA: 0x0000EC2C File Offset: 0x0000CE2C
		public void DeserializeAttachments(byte[] attachmentArray)
		{
			UnderbarrelSetting underbarrelSetting = (attachmentArray[0] != 0) ? UnderbarrelSetting.Enabled : UnderbarrelSetting.Disabled;
			for (int i = 1; i < attachmentArray.Length; i += 4)
			{
				int attachmentIndex = MyConverter.ToInt32(attachmentArray, i);
				this.player.SetAttachment(attachmentIndex, underbarrelSetting);
			}
		}

		// Token: 0x060002DA RID: 730 RVA: 0x0000EC68 File Offset: 0x0000CE68
		public void DeserializeWearables(byte[] wearableArray)
		{
			for (int i = 0; i < wearableArray.Length; i += 4)
			{
				int wearable = MyConverter.ToInt32(wearableArray, i);
				this.player.SetWearable(wearable);
			}
		}

		// Token: 0x04000504 RID: 1284
		[SerializeField]
		protected ShPlayer player;

		// Token: 0x04000505 RID: 1285
		[NonSerialized]
		public JobInfoShared job;

		// Token: 0x04000506 RID: 1286
		public SkinnedMeshRenderer skinnedMeshRenderer;

		// Token: 0x04000507 RID: 1287
		public Transform headBone;

		// Token: 0x04000508 RID: 1288
		protected Rigidbody[] ragdollBodies;

		// Token: 0x04000509 RID: 1289
		protected Collider[] ragdollColliders;

		// Token: 0x0400050A RID: 1290
		public AudioSource hurtSound;

		// Token: 0x0400050B RID: 1291
		[NonSerialized]
		public Voice voice;

		// Token: 0x0400050C RID: 1292
		[SerializeField]
		protected AudioSource footstepSource;

		// Token: 0x0400050D RID: 1293
		protected float blendStartTime;

		// Token: 0x0400050E RID: 1294
		protected ClPlayer.Bone[] bones;

		// Token: 0x0400050F RID: 1295
		protected Coroutine nameCoroutine;

		// Token: 0x04000510 RID: 1296
		protected Coroutine messageCoroutine;

		// Token: 0x04000511 RID: 1297
		protected Coroutine iconCoroutine;

		// Token: 0x04000512 RID: 1298
		public RectTransform textTransform;

		// Token: 0x04000513 RID: 1299
		[NonSerialized]
		public GameObject identityObject;

		// Token: 0x04000514 RID: 1300
		[NonSerialized]
		public float voiceVolume = 1f;

		// Token: 0x02000371 RID: 881
		protected struct Bone
		{
			// Token: 0x0600124A RID: 4682 RVA: 0x0005584A File Offset: 0x00053A4A
			public Bone(Transform transform)
			{
				this.t = transform;
				this.storedPosition = Vector3.zero;
				this.storedRotation = Quaternion.identity;
			}

			// Token: 0x0600124B RID: 4683 RVA: 0x00055869 File Offset: 0x00053A69
			public void UpdateStoredTransform()
			{
				this.storedPosition = this.t.localPosition;
				this.storedRotation = this.t.localRotation;
			}

			// Token: 0x0600124C RID: 4684 RVA: 0x0005588D File Offset: 0x00053A8D
			public readonly void MoveTransform(float blend)
			{
				this.t.SetLocalPositionAndRotation(Vector3.Lerp(this.storedPosition, this.t.localPosition, blend), Quaternion.Slerp(this.storedRotation, this.t.localRotation, blend));
			}

			// Token: 0x0400165A RID: 5722
			public Transform t;

			// Token: 0x0400165B RID: 5723
			private Vector3 storedPosition;

			// Token: 0x0400165C RID: 5724
			private Quaternion storedRotation;
		}
	}
}
