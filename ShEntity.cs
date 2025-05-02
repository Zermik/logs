using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrokeProtocol.Collections;
using BrokeProtocol.Managers;
using BrokeProtocol.Parameters;
using BrokeProtocol.Required;
using BrokeProtocol.Utility;
using BrokeProtocol.Utility.Networking;
using ENet;
using UnityEngine;

namespace BrokeProtocol.Entities
{
	// Token: 0x02000148 RID: 328
	public class ShEntity : Serialized
	{
		// Token: 0x170000FC RID: 252
		// (get) Token: 0x0600073E RID: 1854 RVA: 0x00024284 File Offset: 0x00022484
		public virtual bool IsDead
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x0600073F RID: 1855 RVA: 0x00024287 File Offset: 0x00022487
		public virtual bool SyncTransform
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x06000740 RID: 1856 RVA: 0x0002428A File Offset: 0x0002248A
		[Obsolete]
		public virtual Collider Ground
		{
			get
			{
				return this.GetGround();
			}
		}

		// Token: 0x06000741 RID: 1857 RVA: 0x00024294 File Offset: 0x00022494
		public virtual Collider GetGround()
		{
			RaycastHit raycastHit;
			if (!Physics.Raycast(base.Position + Vector3.up * 0.5f, Vector3.down, out raycastHit, 1f, 9217))
			{
				return null;
			}
			return raycastHit.collider;
		}

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x06000742 RID: 1858 RVA: 0x000242DC File Offset: 0x000224DC
		public virtual ShEntity SpecEntity
		{
			get
			{
				return this;
			}
		}

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x06000743 RID: 1859 RVA: 0x000242DF File Offset: 0x000224DF
		public bool SpecSelf
		{
			get
			{
				return this.SpecEntity == this;
			}
		}

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x06000744 RID: 1860 RVA: 0x000242ED File Offset: 0x000224ED
		public override SerializedParameters Parameters
		{
			get
			{
				return new EntityParameters(this);
			}
		}

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x06000745 RID: 1861 RVA: 0x000242F5 File Offset: 0x000224F5
		public override Type EditorType
		{
			get
			{
				return typeof(EntityEditor);
			}
		}

		// Token: 0x06000746 RID: 1862 RVA: 0x00024301 File Offset: 0x00022501
		public virtual ShMountable GetMount()
		{
			return null;
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x06000747 RID: 1863 RVA: 0x00024304 File Offset: 0x00022504
		public virtual ShPlayer Player
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x06000748 RID: 1864 RVA: 0x00024307 File Offset: 0x00022507
		// (set) Token: 0x06000749 RID: 1865 RVA: 0x0002430E File Offset: 0x0002250E
		public virtual Vector3 Velocity
		{
			get
			{
				return Vector3.zero;
			}
			set
			{
			}
		}

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x0600074A RID: 1866 RVA: 0x00024310 File Offset: 0x00022510
		[Obsolete]
		public Vector3 GetOrigin
		{
			get
			{
				return this.Origin;
			}
		}

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x0600074B RID: 1867 RVA: 0x00024318 File Offset: 0x00022518
		public virtual Vector3 Origin
		{
			get
			{
				return this.CenterBounds;
			}
		}

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x0600074C RID: 1868 RVA: 0x00024320 File Offset: 0x00022520
		public virtual float MountHealth
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x0600074D RID: 1869 RVA: 0x00024327 File Offset: 0x00022527
		public override bool SendInitial
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x0600074E RID: 1870 RVA: 0x0002432A File Offset: 0x0002252A
		public virtual InventoryStruct[] CollectedItems
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x0600074F RID: 1871 RVA: 0x0002432D File Offset: 0x0002252D
		public bool IsClientMain
		{
			get
			{
				return SceneManager.isClient && this.clEntity.isMain;
			}
		}

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x06000750 RID: 1872 RVA: 0x00024343 File Offset: 0x00022543
		public virtual float ProcessDuration
		{
			get
			{
				return 2f;
			}
		}

		// Token: 0x06000751 RID: 1873 RVA: 0x0002434C File Offset: 0x0002254C
		public Bounds GetScaledBounds()
		{
			Vector3 center = this.bounds.center;
			center.Scale(this.mainT.localScale);
			Vector3 extents = this.bounds.extents;
			extents.Scale(this.mainT.localScale);
			return new Bounds(center, 2f * extents);
		}

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x06000752 RID: 1874 RVA: 0x000243A6 File Offset: 0x000225A6
		public Vector3 CenterBounds
		{
			get
			{
				return this.mainT.TransformPoint(this.bounds.center);
			}
		}

		// Token: 0x06000753 RID: 1875 RVA: 0x000243C0 File Offset: 0x000225C0
		public Bounds GetWorldBounds()
		{
			Vector3 size = this.bounds.size;
			Vector3 vector = this.mainT.TransformVector(size.x, 0f, 0f);
			Vector3 vector2 = this.mainT.TransformVector(0f, size.y, 0f);
			Vector3 vector3 = this.mainT.TransformVector(0f, 0f, size.z);
			size.x = Mathf.Abs(vector.x) + Mathf.Abs(vector2.x) + Mathf.Abs(vector3.x);
			size.y = Mathf.Abs(vector.y) + Mathf.Abs(vector2.y) + Mathf.Abs(vector3.y);
			size.z = Mathf.Abs(vector.z) + Mathf.Abs(vector2.z) + Mathf.Abs(vector3.z);
			return new Bounds(this.CenterBounds, size);
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x000244B8 File Offset: 0x000226B8
		public virtual void Cleanup()
		{
			if (SceneManager.isServer)
			{
				this.svEntity.destroyFrame = Time.frameCount;
				this.svEntity.videoURL = string.Empty;
				return;
			}
			if (this.clEntity.videoPlayer)
			{
				this.clEntity.VideoStop();
			}
		}

		// Token: 0x06000755 RID: 1877 RVA: 0x0002450A File Offset: 0x0002270A
		public virtual void Restore(Vector3 position, Quaternion rotation, Transform placeT)
		{
			this.Cleanup();
			base.SetParent(placeT);
			this.SetTransform(position, rotation, true);
			this.go.SetActive(true);
			if (SceneManager.isClient)
			{
				base.UpdateLights();
			}
		}

		// Token: 0x06000756 RID: 1878 RVA: 0x0002453C File Offset: 0x0002273C
		public override void CheckSave()
		{
			ItemOption[] itemOptions = this.svEntity.itemOptions;
			for (int i = 0; i < itemOptions.Length; i++)
			{
				if (string.IsNullOrWhiteSpace(itemOptions[i].itemName))
				{
					Util.Log(base.name + " has a null inventory item " + this.mainT.LogTransform(), LogLevel.Error);
					return;
				}
			}
		}

		// Token: 0x06000757 RID: 1879 RVA: 0x00024598 File Offset: 0x00022798
		public float GetLocalY(Vector3 worldPosition)
		{
			return this.mainT.InverseTransformPoint(worldPosition).y;
		}

		// Token: 0x06000758 RID: 1880 RVA: 0x000245AC File Offset: 0x000227AC
		public float GetFlatAngle(Vector3 v)
		{
			Vector3 vector = Vector3.ProjectOnPlane(v - base.Position, Vector3.up);
			return Mathf.Atan2(vector.z, vector.x);
		}

		// Token: 0x06000759 RID: 1881 RVA: 0x000245E4 File Offset: 0x000227E4
		public virtual bool InActionRange(ShEntity e)
		{
			Bounds worldBounds = e.GetWorldBounds();
			worldBounds.Expand(16f);
			return worldBounds.Contains(this.Origin);
		}

		// Token: 0x0600075A RID: 1882 RVA: 0x00024614 File Offset: 0x00022814
		public bool CanSeeEntity(ShEntity other, bool checkInFront = false, float viewRange = 362.03867f)
		{
			Vector3 vector = this.Origin;
			Vector3 origin = other.Origin;
			Vector3 vector2 = origin - vector;
			if (checkInFront && Vector3.Dot(vector2, this.RotationT.forward) < 0f)
			{
				return false;
			}
			float magnitude = vector2.magnitude;
			if (magnitude > viewRange)
			{
				return false;
			}
			Vector3 vector3 = vector2 / magnitude;
			ShMountable mount = this.GetMount();
			if (((mount != null) ? mount : this).GetWorldBounds().IntersectRay(new Ray(origin, -vector3), out magnitude))
			{
				vector = origin - vector3 * magnitude;
			}
			RaycastHit raycastHit;
			if (Physics.Raycast(vector, vector3, out raycastHit, magnitude, 534273))
			{
				ShEntity componentInParent = raycastHit.collider.GetComponentInParent<ShEntity>();
				return componentInParent && (componentInParent == other || componentInParent == other.GetMount() || componentInParent == other.controller);
			}
			return true;
		}

		// Token: 0x0600075B RID: 1883 RVA: 0x00024708 File Offset: 0x00022908
		public void DrawBounds()
		{
			Bounds worldBounds = this.GetWorldBounds();
			Vector3 center = worldBounds.center;
			Vector3 extents = worldBounds.extents;
			object obj = SceneManager.isClient ? new Action<Vector3, Vector3, Color, float>(MonoBehaviourSingleton<GLDebug>.Instance.DrawLine) : new Action<Vector3, Vector3, Color, float>(MonoBehaviourSingleton<SvManager>.Instance.DrawLine);
			Color arg = SceneManager.isClient ? Color.cyan : Color.yellow;
			object obj2 = obj;
			obj2(center, center + Vector3.Scale(extents, new Vector3(1f, 1f, 1f)), arg, 10f);
			obj2(center, center + Vector3.Scale(extents, new Vector3(-1f, 1f, 1f)), arg, 10f);
			obj2(center, center + Vector3.Scale(extents, new Vector3(1f, -1f, 1f)), arg, 10f);
			obj2(center, center + Vector3.Scale(extents, new Vector3(1f, 1f, -1f)), arg, 10f);
			obj2(center, center + Vector3.Scale(extents, new Vector3(1f, -1f, -1f)), arg, 10f);
			obj2(center, center + Vector3.Scale(extents, new Vector3(-1f, 1f, -1f)), arg, 10f);
			obj2(center, center + Vector3.Scale(extents, new Vector3(-1f, -1f, 1f)), arg, 10f);
			obj2(center, center + Vector3.Scale(extents, new Vector3(-1f, -1f, -1f)), arg, 10f);
		}

		// Token: 0x0600075C RID: 1884 RVA: 0x000248D0 File Offset: 0x00022AD0
		public bool GetOverlapEntity<T>(Vector3 position, Quaternion rotation, int maskIndex, float extentsMultiplier, Predicate<T> predicate, out T entity) where T : ShEntity
		{
			Bounds scaledBounds = this.GetScaledBounds();
			int num = Physics.OverlapBoxNonAlloc(position + scaledBounds.center, scaledBounds.extents * extentsMultiplier, Util.colliderBuffer, rotation, maskIndex);
			for (int i = 0; i < num; i++)
			{
				entity = Util.colliderBuffer[i].GetComponentInParent<T>();
				if (entity != null && predicate(entity))
				{
					return true;
				}
			}
			entity = default(T);
			return false;
		}

		// Token: 0x0600075D RID: 1885 RVA: 0x0002495C File Offset: 0x00022B5C
		public bool CanSpawn(Vector3 position, Quaternion rotation, ShEntity[] ignoreArray)
		{
			ShEntity shEntity;
			return !this.GetOverlapEntity<ShEntity>(position, rotation, 9217, 0.9f, (ShEntity e) => !ignoreArray.Contains(e), out shEntity);
		}

		// Token: 0x0600075E RID: 1886 RVA: 0x0002499C File Offset: 0x00022B9C
		public bool CanSee(Vector3 position)
		{
			Vector3 direction = position - this.Origin;
			return !Physics.Raycast(this.Origin, direction, direction.magnitude, 534273);
		}

		// Token: 0x0600075F RID: 1887 RVA: 0x000249D1 File Offset: 0x00022BD1
		public virtual void DestroyExtras()
		{
			if (!this.isWorldEntity || !this.syncAnimator)
			{
				Object.Destroy(this.animator);
				this.animator = null;
			}
			this.mainT.DestroyExtras();
		}

		// Token: 0x06000760 RID: 1888 RVA: 0x00024A00 File Offset: 0x00022C00
		public virtual void Destroy()
		{
			this.Cleanup();
			if (SceneManager.isServer)
			{
				this.svEntity.Destroy();
			}
			else
			{
				this.clEntity.Destroy();
			}
			this.go.SetActive(false);
			Object.Destroy(this.go);
		}

		// Token: 0x06000761 RID: 1889 RVA: 0x00024A3E File Offset: 0x00022C3E
		public override bool IgnorePhysics(Collider collider)
		{
			return base.IgnorePhysics(collider) || !EntityCollections.Entities.Contains(this.ID);
		}

		// Token: 0x06000762 RID: 1890 RVA: 0x00024A60 File Offset: 0x00022C60
		public virtual void PreInitialize(int ID)
		{
			this.ID = ID;
			EntityCollections.Entities.Add(this);
			this.manager = MonoBehaviourSingleton<ShManager>.Instance;
			if (SceneManager.isServer)
			{
				this.svEntity.svManager = MonoBehaviourSingleton<SvManager>.Instance;
				this.svEntity.InitializeSender();
			}
			else
			{
				this.clEntity.clManager = MonoBehaviourSingleton<ClManager>.Instance;
			}
			this.UpdateMainColliders();
		}

		// Token: 0x06000763 RID: 1891 RVA: 0x00024AC4 File Offset: 0x00022CC4
		protected virtual void UpdateMainColliders()
		{
			foreach (Collider item in this.mainT.GetComponents<Collider>())
			{
				this.colliders.Add(item);
			}
		}

		// Token: 0x06000764 RID: 1892 RVA: 0x00024AFC File Offset: 0x00022CFC
		public virtual void Initialize()
		{
			this.isWorldEntity = true;
			this.bounds = this.GetLocalBounds(false);
			if (SceneManager.isServer)
			{
				this.svEntity.Initialize();
				Object.Destroy(this.clEntity);
			}
			else
			{
				this.clEntity.Initialize();
				Object.Destroy(this.svEntity);
			}
			bool activeSelf = this.go.activeSelf;
			this.go.SetActive(false);
			base.enabled = true;
			if (SceneManager.isServer)
			{
				this.svEntity.enabled = true;
				if (activeSelf)
				{
					this.Spawn(base.Position, base.Rotation, base.Parent);
					return;
				}
			}
			else
			{
				this.clEntity.enabled = true;
			}
		}

		// Token: 0x06000765 RID: 1893 RVA: 0x00024BAC File Offset: 0x00022DAC
		public virtual void Spawn(Vector3 position, Quaternion rotation, Transform placeT)
		{
			base.StopAllCoroutines();
			if (this.clEntity)
			{
				this.clEntity.StopAllCoroutines();
			}
			if (this.svEntity)
			{
				this.svEntity.StopAllCoroutines();
			}
			this.go.SetActive(true);
			this.Restore(position, rotation, placeT);
			if (SceneManager.isServer)
			{
				this.svEntity.Events.Spawn(this);
				if (this.svEntity.subscribers == null)
				{
					this.svEntity.NewSector();
				}
				if (this.svEntity.defaultItems == null)
				{
					this.svEntity.UpdateDefaultItems();
				}
				if (this.animator && this.syncAnimator)
				{
					base.StartCoroutine(this.svEntity.SyncAnimator());
				}
			}
		}

		// Token: 0x06000766 RID: 1894 RVA: 0x00024C74 File Offset: 0x00022E74
		public void DeactivateEntity()
		{
			this.Cleanup();
			this.go.SetActive(false);
			base.SetParent(null);
		}

		// Token: 0x06000767 RID: 1895 RVA: 0x00024C90 File Offset: 0x00022E90
		protected void IgnoreCollision(ShPlayer p)
		{
			foreach (Collider collider in this.colliders)
			{
				foreach (Collider collider2 in p.colliders)
				{
					Physics.IgnoreCollision(collider, collider2, true);
					this.ignoredColliders.Add(collider2);
				}
				if (p.curMount)
				{
					foreach (Collider collider3 in p.curMount.colliders)
					{
						Physics.IgnoreCollision(collider, collider3, true);
						this.ignoredColliders.Add(collider3);
					}
				}
			}
		}

		// Token: 0x06000768 RID: 1896 RVA: 0x00024D98 File Offset: 0x00022F98
		protected IEnumerator ResetCollisions()
		{
			yield return new WaitForSeconds(1f);
			foreach (Collider collider in this.colliders)
			{
				foreach (Collider collider2 in this.ignoredColliders)
				{
					if (collider2)
					{
						Physics.IgnoreCollision(collider, collider2, false);
					}
				}
			}
			this.ignoredColliders.Clear();
			yield break;
		}

		// Token: 0x06000769 RID: 1897 RVA: 0x00024DA8 File Offset: 0x00022FA8
		public int InventoryValue()
		{
			int num = 0;
			foreach (InventoryItem inventoryItem in this.myItems.Values)
			{
				num += inventoryItem.count * inventoryItem.item.value;
			}
			return num;
		}

		// Token: 0x0600076A RID: 1898 RVA: 0x00024E14 File Offset: 0x00023014
		public int GetMyItemValue(ShItem item, bool markup)
		{
			if (!this.ShopCanBuy(item))
			{
				return 0;
			}
			return item.GetValue(this.MyItemCount(item.index), markup);
		}

		// Token: 0x0600076B RID: 1899 RVA: 0x00024E34 File Offset: 0x00023034
		public void SendViewerDelta(byte delta, int itemIndex, int amount, ShPlayer ignorePlayer)
		{
			if (delta != 0)
			{
				foreach (ShPlayer shPlayer in this.viewers)
				{
					if (shPlayer != ignorePlayer)
					{
						shPlayer.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.TransferItem, new object[]
						{
							delta,
							itemIndex,
							amount
						});
					}
				}
			}
			ShItem item;
			if (this.Shop && MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShItem>(itemIndex, out item) && this.HasItem(item) && this.ShopCanBuy(item))
			{
				int myItemValue = this.GetMyItemValue(item, true);
				foreach (ShPlayer shPlayer2 in this.viewers)
				{
					shPlayer2.svPlayer.Send(SvSendType.Self, PacketFlags.Reliable, ClPacket.UpdateShopValue, new object[]
					{
						itemIndex,
						myItemValue
					});
				}
			}
		}

		// Token: 0x0600076C RID: 1900 RVA: 0x00024F50 File Offset: 0x00023150
		public void TransferMoney(byte deltaType, int amount = 1, bool dispatch = true)
		{
			this.TransferItem(deltaType, this.manager.money.index, amount, dispatch);
		}

		// Token: 0x0600076D RID: 1901 RVA: 0x00024F6B File Offset: 0x0002316B
		public void TransferItem(byte deltaType, ShItem item, int amount = 1, bool dispatch = true)
		{
			this.TransferItem(deltaType, item.index, amount, dispatch);
		}

		// Token: 0x0600076E RID: 1902 RVA: 0x00024F80 File Offset: 0x00023180
		public bool ValidTransfer(int itemIndex, int amount)
		{
			if (amount <= 0)
			{
				Util.Log("Transfer amount must be positive", LogLevel.Log);
				return false;
			}
			ShItem shItem;
			if (!MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShItem>(itemIndex, out shItem))
			{
				Util.Log("Trying to transfer invalid Item: " + itemIndex.ToString(), LogLevel.Log);
				return false;
			}
			return true;
		}

		// Token: 0x0600076F RID: 1903 RVA: 0x00024FC8 File Offset: 0x000231C8
		public virtual void TransferItem(byte deltaType, int itemIndex, int amount = 1, bool dispatch = true)
		{
			if (!this.ValidTransfer(itemIndex, amount))
			{
				return;
			}
			if (deltaType != 1)
			{
				if (deltaType == 2)
				{
					if (this.MyItemCount(itemIndex) < amount)
					{
						return;
					}
					this.RemoveFromMyItems(itemIndex, amount, dispatch);
				}
			}
			else
			{
				this.AddToMyItems(itemIndex, amount, dispatch);
			}
			if (SceneManager.isServer)
			{
				if (this.viewers.Count > 0)
				{
					this.SendViewerDelta(DeltaInv.ViewerDelta[(int)deltaType], itemIndex, amount, null);
				}
				this.svEntity.Events.TransferItem(this, deltaType, itemIndex, amount, dispatch);
			}
		}

		// Token: 0x06000770 RID: 1904 RVA: 0x0002504C File Offset: 0x0002324C
		protected InventoryItem DeserializeItem(byte[] itemArray)
		{
			int num = 0;
			int num2 = MyConverter.ToInt32(itemArray, num);
			num += 4;
			int count = MyConverter.ToInt32(itemArray, num);
			ShItem item;
			if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShItem>(num2, out item))
			{
				return new InventoryItem(item, count, 0);
			}
			return null;
		}

		// Token: 0x06000771 RID: 1905 RVA: 0x00025088 File Offset: 0x00023288
		protected InventoryItem DeserializeShopItem(byte[] itemArray)
		{
			int num = 0;
			int num2 = MyConverter.ToInt32(itemArray, num);
			num += 4;
			int count = MyConverter.ToInt32(itemArray, num);
			num += 4;
			int currentValue = MyConverter.ToInt32(itemArray, num);
			ShItem item;
			if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShItem>(num2, out item))
			{
				return new InventoryItem(item, count, currentValue);
			}
			return null;
		}

		// Token: 0x06000772 RID: 1906 RVA: 0x000250D4 File Offset: 0x000232D4
		public byte[] SerializeMyItems()
		{
			byte[] array = new byte[this.myItems.Count * Util.inventoryElement.Length];
			int num = 0;
			foreach (KeyValuePair<int, InventoryItem> keyValuePair in this.myItems)
			{
				MyConverter.GetBytes(keyValuePair.Key).CopyTo(Util.inventoryElement, 0);
				MyConverter.GetBytes(keyValuePair.Value.count).CopyTo(Util.inventoryElement, 4);
				Util.inventoryElement.CopyTo(array, num);
				num += Util.inventoryElement.Length;
			}
			return array;
		}

		// Token: 0x06000773 RID: 1907 RVA: 0x00025188 File Offset: 0x00023388
		public SizedArray SerializeShop()
		{
			byte[] array = new byte[this.myItems.Count * Util.shopElement.Length];
			int num = 0;
			foreach (KeyValuePair<int, InventoryItem> keyValuePair in this.myItems)
			{
				if (this.ShopCanBuy(keyValuePair.Value.item) || keyValuePair.Key == this.manager.money.index)
				{
					MyConverter.GetBytes(keyValuePair.Key).CopyTo(Util.shopElement, 0);
					MyConverter.GetBytes(keyValuePair.Value.count).CopyTo(Util.shopElement, 4);
					MyConverter.GetBytes(this.GetMyItemValue(keyValuePair.Value.item, true)).CopyTo(Util.shopElement, 8);
					Util.shopElement.CopyTo(array, num);
					num += Util.shopElement.Length;
				}
			}
			return new SizedArray(array, num);
		}

		// Token: 0x06000774 RID: 1908 RVA: 0x00025298 File Offset: 0x00023498
		public void DeserializeMyItems(byte[] inventoryArray)
		{
			this.myItems.Clear();
			if (inventoryArray == null)
			{
				return;
			}
			for (int i = 0; i < inventoryArray.Length; i += Util.inventoryElement.Length)
			{
				Array.Copy(inventoryArray, i, Util.inventoryElement, 0, Util.inventoryElement.Length);
				InventoryItem inventoryItem = this.DeserializeItem(Util.inventoryElement);
				if (inventoryItem != null)
				{
					this.AddToMyItems(inventoryItem.item.index, inventoryItem.count, false);
				}
			}
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x00025304 File Offset: 0x00023504
		public void DeserializeShop(byte[] inventoryArray)
		{
			this.myItems.Clear();
			if (inventoryArray == null)
			{
				return;
			}
			for (int i = 0; i < inventoryArray.Length; i += Util.shopElement.Length)
			{
				Array.Copy(inventoryArray, i, Util.shopElement, 0, Util.shopElement.Length);
				InventoryItem inventoryItem = this.DeserializeShopItem(Util.shopElement);
				if (inventoryItem != null)
				{
					this.AddToMyItems(inventoryItem.item.index, inventoryItem.count, false);
					this.myItems[inventoryItem.item.index].currentValue = inventoryItem.currentValue;
				}
			}
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x00025390 File Offset: 0x00023590
		protected void AddToInventory(Dictionary<int, InventoryItem> inventory, int itemIndex, int amount)
		{
			InventoryItem inventoryItem;
			ShItem item;
			if (inventory.TryGetValue(itemIndex, out inventoryItem))
			{
				inventoryItem.count += amount;
				if (inventoryItem.count == 0)
				{
					inventory.Remove(itemIndex);
					return;
				}
			}
			else if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShItem>(itemIndex, out item))
			{
				inventory.Add(itemIndex, new InventoryItem(item, amount, 0));
			}
		}

		// Token: 0x06000777 RID: 1911 RVA: 0x000253E5 File Offset: 0x000235E5
		public virtual void AddToMyItems(int itemIndex, int amount, bool dispatch)
		{
			this.AddToInventory(this.myItems, itemIndex, amount);
			if (SceneManager.isServer)
			{
				this.svEntity.Events.AddItem(this, itemIndex, amount, dispatch);
			}
		}

		// Token: 0x06000778 RID: 1912 RVA: 0x00025411 File Offset: 0x00023611
		public void AddToTradeItems(int itemIndex, int amount)
		{
			this.AddToInventory(this.tradeItems, itemIndex, amount);
		}

		// Token: 0x06000779 RID: 1913 RVA: 0x00025424 File Offset: 0x00023624
		protected void RemoveFromInventory(Dictionary<int, InventoryItem> inventory, int itemIndex, int amount)
		{
			InventoryItem inventoryItem;
			ShItem item;
			if (inventory.TryGetValue(itemIndex, out inventoryItem))
			{
				inventoryItem.count -= amount;
				if (inventoryItem.count == 0)
				{
					inventory.Remove(itemIndex);
					return;
				}
			}
			else if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShItem>(itemIndex, out item))
			{
				inventory.Add(itemIndex, new InventoryItem(item, -amount, 0));
			}
		}

		// Token: 0x0600077A RID: 1914 RVA: 0x0002547A File Offset: 0x0002367A
		public virtual void RemoveFromMyItems(int itemIndex, int amount, bool dispatch)
		{
			this.RemoveFromInventory(this.myItems, itemIndex, amount);
			if (SceneManager.isServer)
			{
				this.svEntity.Events.RemoveItem(this, itemIndex, amount, dispatch);
			}
		}

		// Token: 0x0600077B RID: 1915 RVA: 0x000254A6 File Offset: 0x000236A6
		public void RemoveFromTradeItems(int itemIndex, int amount)
		{
			this.RemoveFromInventory(this.tradeItems, itemIndex, amount);
		}

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x0600077C RID: 1916 RVA: 0x000254B6 File Offset: 0x000236B6
		public bool HasInventory
		{
			get
			{
				return this.inventoryType > InventoryType.None;
			}
		}

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x0600077D RID: 1917 RVA: 0x000254C1 File Offset: 0x000236C1
		public bool Shop
		{
			get
			{
				return this.inventoryType == InventoryType.Shop;
			}
		}

		// Token: 0x0600077E RID: 1918 RVA: 0x000254CC File Offset: 0x000236CC
		public virtual bool CanBeSearched(ShPlayer p)
		{
			return this.inventoryType == InventoryType.Normal || (this.inventoryType == InventoryType.Locked && p.InOwnApartment);
		}

		// Token: 0x0600077F RID: 1919 RVA: 0x000254EA File Offset: 0x000236EA
		public bool CanBeCracked(ShPlayer p)
		{
			return this.inventoryType == InventoryType.Locked && !p.InOwnApartment;
		}

		// Token: 0x06000780 RID: 1920 RVA: 0x00025500 File Offset: 0x00023700
		public int ItemCount(Dictionary<int, InventoryItem> inventory, int itemIndex)
		{
			InventoryItem inventoryItem;
			if (!inventory.TryGetValue(itemIndex, out inventoryItem))
			{
				return 0;
			}
			return inventoryItem.count;
		}

		// Token: 0x06000781 RID: 1921 RVA: 0x00025520 File Offset: 0x00023720
		public bool HasItem(ShItem item)
		{
			return this.HasItem(item.index);
		}

		// Token: 0x06000782 RID: 1922 RVA: 0x00025530 File Offset: 0x00023730
		public bool HasItem(int itemIndex)
		{
			InventoryItem inventoryItem;
			return this.myItems.TryGetValue(itemIndex, out inventoryItem) && inventoryItem.count > 0;
		}

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x06000783 RID: 1923 RVA: 0x00025558 File Offset: 0x00023758
		public int MyMoneyCount
		{
			get
			{
				return this.MyItemCount(this.manager.money);
			}
		}

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x06000784 RID: 1924 RVA: 0x0002556B File Offset: 0x0002376B
		public int TradeMoneyCount
		{
			get
			{
				return this.TradeItemCount(this.manager.money);
			}
		}

		// Token: 0x06000785 RID: 1925 RVA: 0x0002557E File Offset: 0x0002377E
		public int MyItemCount(ShEntity item)
		{
			return this.ItemCount(this.myItems, item.index);
		}

		// Token: 0x06000786 RID: 1926 RVA: 0x00025592 File Offset: 0x00023792
		public int TradeItemCount(ShEntity item)
		{
			return this.ItemCount(this.tradeItems, item.index);
		}

		// Token: 0x06000787 RID: 1927 RVA: 0x000255A6 File Offset: 0x000237A6
		public int MyItemCount(int itemIndex)
		{
			return this.ItemCount(this.myItems, itemIndex);
		}

		// Token: 0x06000788 RID: 1928 RVA: 0x000255B5 File Offset: 0x000237B5
		public int TradeItemCount(int itemIndex)
		{
			return this.ItemCount(this.tradeItems, itemIndex);
		}

		// Token: 0x06000789 RID: 1929 RVA: 0x000255C4 File Offset: 0x000237C4
		public void ClearAllViewers()
		{
			foreach (ShPlayer shPlayer in this.viewers)
			{
				shPlayer.svPlayer.DestroyMenu("Default");
				shPlayer.otherEntity = null;
			}
			this.viewers.Clear();
		}

		// Token: 0x0600078A RID: 1930 RVA: 0x00025630 File Offset: 0x00023830
		public void RemoveViewer(ShPlayer viewer)
		{
			this.viewers.Remove(viewer);
			if (this.viewers.Count == 0 && this.svEntity.destroyEmpty && this.myItems.Count == 0)
			{
				this.Destroy();
			}
		}

		// Token: 0x0600078B RID: 1931 RVA: 0x0002566C File Offset: 0x0002386C
		public bool ShopCanBuy(ShItem item)
		{
			ItemOption[] itemOptions = this.svEntity.itemOptions;
			for (int i = 0; i < itemOptions.Length; i++)
			{
				if (itemOptions[i].itemName.GetPrefabIndex() == item.index)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600078C RID: 1932 RVA: 0x000256B0 File Offset: 0x000238B0
		public virtual void SetVisible(bool setting)
		{
			Renderer[] array = this.renderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = setting;
			}
		}

		// Token: 0x040006AE RID: 1710
		public ClEntity clEntity;

		// Token: 0x040006AF RID: 1711
		public SvEntity svEntity;

		// Token: 0x040006B0 RID: 1712
		public Animator animator;

		// Token: 0x040006B1 RID: 1713
		public bool syncAnimator;

		// Token: 0x040006B2 RID: 1714
		[NonSerialized]
		public ShPlayer controller;

		// Token: 0x040006B3 RID: 1715
		[NonSerialized]
		public Dictionary<string, string> dynamicActions = new Dictionary<string, string>();

		// Token: 0x040006B4 RID: 1716
		[NonSerialized]
		public bool isHuman;

		// Token: 0x040006B5 RID: 1717
		[NonSerialized]
		public float videoStartTime;

		// Token: 0x040006B6 RID: 1718
		[NonSerialized]
		public string profile = string.Empty;

		// Token: 0x040006B7 RID: 1719
		[NonSerialized]
		public bool isWorldEntity;

		// Token: 0x040006B8 RID: 1720
		[NonSerialized]
		public Renderer[] renderers;

		// Token: 0x040006B9 RID: 1721
		public AppIndex[] availableApps;

		// Token: 0x040006BA RID: 1722
		public InventoryType inventoryType;

		// Token: 0x040006BB RID: 1723
		public Dictionary<int, InventoryItem> myItems = new Dictionary<int, InventoryItem>();

		// Token: 0x040006BC RID: 1724
		public Dictionary<int, InventoryItem> tradeItems = new Dictionary<int, InventoryItem>();

		// Token: 0x040006BD RID: 1725
		[NonSerialized]
		public HashSet<ShPlayer> viewers = new HashSet<ShPlayer>();

		// Token: 0x040006BE RID: 1726
		[NonSerialized]
		public HashSet<Collider> colliders = new HashSet<Collider>();

		// Token: 0x040006BF RID: 1727
		[NonSerialized]
		public HashSet<Collider> ignoredColliders = new HashSet<Collider>();

		// Token: 0x040006C0 RID: 1728
		[NonSerialized]
		public int ID;

		// Token: 0x040006C1 RID: 1729
		[HideInInspector]
		public int index;

		// Token: 0x040006C2 RID: 1730
		[NonSerialized]
		public ShManager manager;

		// Token: 0x040006C3 RID: 1731
		public int value;

		// Token: 0x040006C4 RID: 1732
		[NonSerialized]
		public Vector3 relativeVelocity;

		// Token: 0x040006C5 RID: 1733
		[NonSerialized]
		public Bounds bounds;
	}
}
