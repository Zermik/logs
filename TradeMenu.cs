using System;
using System.Collections.Generic;
using BrokeProtocol.Client.Buttons;
using BrokeProtocol.Managers;
using BrokeProtocol.Required;
using BrokeProtocol.Utility;
using BrokeProtocol.Utility.Networking;
using ENet;
using UnityEngine.UIElements;

namespace BrokeProtocol.Client.UI
{
	// Token: 0x0200030E RID: 782
	public class TradeMenu : InventoryMenu
	{
		// Token: 0x06000F8A RID: 3978 RVA: 0x00048BCE File Offset: 0x00046DCE
		public override void Initialize(params object[] args)
		{
			base.Initialize(Array.Empty<object>());
			this.finalizeButton = this.subClone.Q("Finalize", null);
			this.finalizeButton.clicked += this.FinalizeAction;
		}

		// Token: 0x06000F8B RID: 3979 RVA: 0x00048C09 File Offset: 0x00046E09
		public void DisableButton()
		{
			this.finalizeButton.SetEnabled(false);
			this.myPlayer.lockedTrade = true;
			this.finalizeButton.text = "Waiting..";
			this.finalized = true;
		}

		// Token: 0x06000F8C RID: 3980 RVA: 0x00048C3A File Offset: 0x00046E3A
		public void EnableButton(bool lockedTrade)
		{
			this.finalizeButton.SetEnabled(true);
			this.myPlayer.lockedTrade = lockedTrade;
			this.finalizeButton.text = "Confirm";
			this.finalized = true;
		}

		// Token: 0x06000F8D RID: 3981 RVA: 0x00048C6B File Offset: 0x00046E6B
		public void FinalizeAction()
		{
			if (this.finalized)
			{
				MonoBehaviourSingleton<ClManager>.Instance.SendToServer(PacketFlags.Reliable, SvPacket.ConfirmTrade, Array.Empty<object>());
				return;
			}
			MonoBehaviourSingleton<ClManager>.Instance.SendToServer(PacketFlags.Reliable, SvPacket.FinalizeTrade, Array.Empty<object>());
		}

		// Token: 0x06000F8E RID: 3982 RVA: 0x00048C9C File Offset: 0x00046E9C
		protected override void FillChildItems(int replaceIndex)
		{
			foreach (KeyValuePair<int, InventoryItem> keyValuePair in this.myPlayer.otherEntity.tradeItems)
			{
				if (replaceIndex == 0 || keyValuePair.Key == replaceIndex)
				{
					base.CreateButton(new ItemInfo(this.subClone.Q("OtherItems", null).Q("unity-content-container", null), keyValuePair.Value, false, keyValuePair.Value.item.SortableName, ButtonType.Disabled, 1f));
				}
			}
			foreach (KeyValuePair<int, InventoryItem> keyValuePair2 in this.myPlayer.tradeItems)
			{
				if (replaceIndex == 0 || keyValuePair2.Key == replaceIndex)
				{
					base.CreateButton(new ItemInfo(this.subClone.Q("TradeItems", null).Q("unity-content-container", null), keyValuePair2.Value, false, keyValuePair2.Value.item.SortableName, ButtonType.Others, 1f));
				}
			}
		}

		// Token: 0x06000F8F RID: 3983 RVA: 0x00048DDC File Offset: 0x00046FDC
		public override void GiveAmount(int index, int amount)
		{
			MonoBehaviourSingleton<ClManager>.Instance.SendToServer(PacketFlags.Reliable, SvPacket.TransferTrade, new object[]
			{
				7,
				index,
				amount
			});
		}

		// Token: 0x06000F90 RID: 3984 RVA: 0x00048E0C File Offset: 0x0004700C
		public override void TakeAmount(int index, int amount)
		{
			MonoBehaviourSingleton<ClManager>.Instance.SendToServer(PacketFlags.Reliable, SvPacket.TransferTrade, new object[]
			{
				8,
				index,
				amount
			});
		}

		// Token: 0x06000F91 RID: 3985 RVA: 0x00048E3C File Offset: 0x0004703C
		public override List<DragAndDropContainer> GetDragContainers(ItemActions itemActions)
		{
			List<DragAndDropContainer> list = new List<DragAndDropContainer>();
			if (itemActions.MyButtonTest())
			{
				VisualElement containerElement = this.subClone.Q("TradeItems", null);
				list.Add(new DragAndDropContainer(containerElement, null, delegate(int x)
				{
					this.GiveAmount(x, 1);
				}));
			}
			else if (itemActions.OtherButtonTest())
			{
				VisualElement containerElement2 = this.uiClone.Q("InventoryScrollView", null);
				list.Add(new DragAndDropContainer(containerElement2, null, delegate(int x)
				{
					this.TakeAmount(x, 1);
				}));
			}
			return list;
		}

		// Token: 0x04001351 RID: 4945
		protected Button finalizeButton;

		// Token: 0x04001352 RID: 4946
		protected bool finalized;
	}
}
