using System;
using BrokeProtocol.Required;
using UnityEngine;

namespace BrokeProtocol.Properties
{
	// Token: 0x02000231 RID: 561
	public class ShPlayerProperties : ShProperties
	{
		// Token: 0x04000ECC RID: 3788
		public string referencePrefab;

		// Token: 0x04000ECD RID: 3789
		public GameObject lightsObject;

		// Token: 0x04000ECE RID: 3790
		public Animator animator;

		// Token: 0x04000ECF RID: 3791
		public bool syncAnimator;

		// Token: 0x04000ED0 RID: 3792
		public AppIndex[] availableApps;

		// Token: 0x04000ED1 RID: 3793
		public InventoryType inventoryType;

		// Token: 0x04000ED2 RID: 3794
		public int value;

		// Token: 0x04000ED3 RID: 3795
		public Seat[] seats;

		// Token: 0x04000ED4 RID: 3796
		public Transform[] exitTransforms;

		// Token: 0x04000ED5 RID: 3797
		public float maxStat = 20f;

		// Token: 0x04000ED6 RID: 3798
		public Rigidbody positionRB;

		// Token: 0x04000ED7 RID: 3799
		public float maxSpeed;

		// Token: 0x04000ED8 RID: 3800
		public CharacterType characterType;

		// Token: 0x04000ED9 RID: 3801
		public Transform rotationT;

		// Token: 0x04000EDA RID: 3802
		public string hands;

		// Token: 0x04000EDB RID: 3803
		public string surrender;

		// Token: 0x04000EDC RID: 3804
		public WearableOptions[] wearableOptions;

		// Token: 0x04000EDD RID: 3805
		public CapsuleCollider capsule;

		// Token: 0x04000EDE RID: 3806
		public bool boss;

		// Token: 0x04000EDF RID: 3807
		public Vector3 moveFactor = new Vector3(12f, 12f, 12f);

		// Token: 0x04000EE0 RID: 3808
		public Transform originT;

		// Token: 0x04000EE1 RID: 3809
		public float jumpVelocity = 14f;

		// Token: 0x04000EE2 RID: 3810
		public float baseWeightLimit = 100f;
	}
}
