using System;
using BrokeProtocol.Entities;
using UnityEngine.UIElements;

namespace BrokeProtocol.Utility
{
	// Token: 0x0200000F RID: 15
	public class InjuryInfo : ButtonInfo
	{
		// Token: 0x0600002D RID: 45 RVA: 0x00002DE0 File Offset: 0x00000FE0
		public InjuryInfo(VisualElement parent, ShPlayer player, Injury injury) : base(parent)
		{
			this.player = player;
			this.injury = injury;
		}

		// Token: 0x04000034 RID: 52
		public ShPlayer player;

		// Token: 0x04000035 RID: 53
		public Injury injury;
	}
}
