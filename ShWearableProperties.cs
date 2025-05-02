using System;
using BrokeProtocol.Required;
using UnityEngine;

namespace BrokeProtocol.Properties
{
	// Token: 0x02000248 RID: 584
	public class ShWearableProperties : ShProperties
	{
		// Token: 0x04001096 RID: 4246
		public string referencePrefab;

		// Token: 0x04001097 RID: 4247
		public GameObject lightsObject;

		// Token: 0x04001098 RID: 4248
		public Animator animator;

		// Token: 0x04001099 RID: 4249
		public bool syncAnimator;

		// Token: 0x0400109A RID: 4250
		public AppIndex[] availableApps;

		// Token: 0x0400109B RID: 4251
		public InventoryType inventoryType;

		// Token: 0x0400109C RID: 4252
		public int value;

		// Token: 0x0400109D RID: 4253
		public Seat[] seats;

		// Token: 0x0400109E RID: 4254
		public Transform[] exitTransforms;

		// Token: 0x0400109F RID: 4255
		public string itemName;

		// Token: 0x040010A0 RID: 4256
		public bool needItem;

		// Token: 0x040010A1 RID: 4257
		public bool illegal;

		// Token: 0x040010A2 RID: 4258
		public ShReference license;

		// Token: 0x040010A3 RID: 4259
		public float weight;

		// Token: 0x040010A4 RID: 4260
		public ShReference ammoItem;

		// Token: 0x040010A5 RID: 4261
		public CharacterType characterType;

		// Token: 0x040010A6 RID: 4262
		public SkinnedMeshRenderer skinnedMesh;

		// Token: 0x040010A7 RID: 4263
		public WearableType type;

		// Token: 0x040010A8 RID: 4264
		public float protection;

		// Token: 0x040010A9 RID: 4265
		public float capacity;
	}
}
