using System;
using System.Collections.Generic;
using BrokeProtocol.Client.UI;
using BrokeProtocol.Entities;
using BrokeProtocol.Managers;
using BrokeProtocol.Utility;
using BrokeProtocol.Utility.Networking;
using ENet;

namespace BrokeProtocol.Client.Buttons
{
	// Token: 0x020002C3 RID: 707
	public class WearableActions : ItemActions
	{
		// Token: 0x06000DDF RID: 3551 RVA: 0x0003F942 File Offset: 0x0003DB42
		public override void Initialize(ClItemButton itemButton, ItemInfo itemInfo)
		{
			base.Initialize(itemButton, itemInfo);
			this.wearable = (itemInfo.invItem.item as ShWearable);
		}

		// Token: 0x06000DE0 RID: 3552 RVA: 0x0003F964 File Offset: 0x0003DB64
		public override List<ActionInfo> GetActions()
		{
			List<ActionInfo> list = new List<ActionInfo>();
			list.Add(new ActionInfo(ButtonIndex.Wearable, new Func<bool>(this.EquipTest), new Action(this.PutOnAction)));
			list.Add(new ActionInfo(ButtonIndex.Wearable, new Func<bool>(this.UnequipTest), new Action(this.TakeOffAction)));
			list.AddRange(base.GetActions());
			return list;
		}

		// Token: 0x06000DE1 RID: 3553 RVA: 0x0003F9CE File Offset: 0x0003DBCE
		public override bool EquipTest()
		{
			return base.EquipTest() && MonoBehaviourSingleton<ClManager>.Instance.myPlayer.CanWear(this.wearable);
		}

		// Token: 0x06000DE2 RID: 3554 RVA: 0x0003F9EF File Offset: 0x0003DBEF
		public void PutOnAction()
		{
			MonoBehaviourSingleton<ClManager>.Instance.SendToServer(PacketFlags.Reliable, SvPacket.SetWearable, new object[]
			{
				this.wearable.index
			});
		}

		// Token: 0x06000DE3 RID: 3555 RVA: 0x0003FA17 File Offset: 0x0003DC17
		public void TakeOffAction()
		{
			MonoBehaviourSingleton<ClManager>.Instance.SendToServer(PacketFlags.Reliable, SvPacket.SetWearable, new object[]
			{
				MonoBehaviourSingleton<ShManager>.Instance.nullWearable[(int)this.wearable.type].index
			});
		}

		// Token: 0x04001233 RID: 4659
		[NonSerialized]
		public ShWearable wearable;
	}
}
