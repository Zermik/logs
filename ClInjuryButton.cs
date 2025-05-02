using System;
using BrokeProtocol.Utility;
using UnityEngine.UIElements;

namespace BrokeProtocol.Client.Buttons
{
	// Token: 0x020002B3 RID: 691
	public class ClInjuryButton : ClButton
	{
		// Token: 0x06000D52 RID: 3410 RVA: 0x0003D46C File Offset: 0x0003B66C
		public override void Initialize(ButtonInfo buttonInfo)
		{
			base.Initialize(buttonInfo);
			this.injuryInfo = (InjuryInfo)buttonInfo;
			Injury injury = this.injuryInfo.injury;
			this.uiClone.Q("Part", null).text = injury.part.ToString();
			this.uiClone.Q("Injury", null).text = injury.effect.ToString();
			this.uiClone.Q("Amount", null).text = injury.amount.ToPercent();
		}

		// Token: 0x0400121B RID: 4635
		protected InjuryInfo injuryInfo;
	}
}
