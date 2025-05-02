using System;
using BrokeProtocol.Client.Buttons;
using UnityEngine;

namespace BrokeProtocol.Entities
{
	// Token: 0x02000132 RID: 306
	public class ClWearable : ClItem
	{
		// Token: 0x170000DD RID: 221
		// (get) Token: 0x06000694 RID: 1684 RVA: 0x00021B4B File Offset: 0x0001FD4B
		public override Type ItemButtonType
		{
			get
			{
				return typeof(WearableActions);
			}
		}

		// Token: 0x0400066D RID: 1645
		[SerializeField]
		protected ShWearable wearable;
	}
}
