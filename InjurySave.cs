using System;
using BrokeProtocol.Required;

namespace BrokeProtocol.LiteDB
{
	// Token: 0x02000194 RID: 404
	public class InjurySave
	{
		// Token: 0x06000B95 RID: 2965 RVA: 0x0003997B File Offset: 0x00037B7B
		public InjurySave()
		{
		}

		// Token: 0x06000B96 RID: 2966 RVA: 0x00039983 File Offset: 0x00037B83
		public InjurySave(BodyPart part, BodyEffect effect, byte amount)
		{
			this.Part = part;
			this.Effect = effect;
			this.Amount = amount;
		}

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x06000B97 RID: 2967 RVA: 0x000399A0 File Offset: 0x00037BA0
		// (set) Token: 0x06000B98 RID: 2968 RVA: 0x000399A8 File Offset: 0x00037BA8
		public BodyPart Part { get; set; }

		// Token: 0x1700018F RID: 399
		// (get) Token: 0x06000B99 RID: 2969 RVA: 0x000399B1 File Offset: 0x00037BB1
		// (set) Token: 0x06000B9A RID: 2970 RVA: 0x000399B9 File Offset: 0x00037BB9
		public BodyEffect Effect { get; set; }

		// Token: 0x17000190 RID: 400
		// (get) Token: 0x06000B9B RID: 2971 RVA: 0x000399C2 File Offset: 0x00037BC2
		// (set) Token: 0x06000B9C RID: 2972 RVA: 0x000399CA File Offset: 0x00037BCA
		public byte Amount { get; set; }
	}
}
