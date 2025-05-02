using System;
using System.Collections.Generic;
using BrokeProtocol.Client.Buttons;
using BrokeProtocol.Entities;
using BrokeProtocol.Managers;
using BrokeProtocol.Required;
using BrokeProtocol.Utility;
using BrokeProtocol.Utility.Networking;
using ENet;
using UnityEngine;
using UnityEngine.UIElements;

namespace BrokeProtocol.Client.UI
{
	// Token: 0x020002F1 RID: 753
	public class EquipmentMenu : InventoryMenu
	{
		// Token: 0x06000ECD RID: 3789 RVA: 0x00044B60 File Offset: 0x00042D60
		public override void Initialize(params object[] args)
		{
			this.playerRenderer = Object.Instantiate<PlayerRenderer>(MonoBehaviourSingleton<ClManager>.Instance.playerRendererPrefab);
			base.Initialize(args);
			this.uiClone.Q("EquipmentPanel", null).style.backgroundImage = new StyleBackground(Background.FromRenderTexture(this.playerRenderer.cam.targetTexture));
			this.subClone.Q("Drop", null).clicked += this.DropInventory;
		}

		// Token: 0x06000ECE RID: 3790 RVA: 0x00044BE0 File Offset: 0x00042DE0
		public override void Destroy()
		{
			base.Destroy();
			Object.Destroy(this.playerRenderer.gameObject);
		}

		// Token: 0x06000ECF RID: 3791 RVA: 0x00044BF8 File Offset: 0x00042DF8
		public void DropInventory()
		{
			MonoBehaviourSingleton<ClManager>.Instance.SendToServer(PacketFlags.Reliable, SvPacket.Drop, Array.Empty<object>());
			this.Destroy();
		}

		// Token: 0x06000ED0 RID: 3792 RVA: 0x00044C14 File Offset: 0x00042E14
		protected override void FillChildItems(int replaceIndex)
		{
			this.subClone.Q("ArmorLevel", null).text = "Armor: " + Mathf.CeilToInt(this.myPlayer.armorLevel).ToString() + "%";
			this.playerRenderer.Refresh(this.myPlayer.index, this.myPlayer.curWearables);
			for (int i = 0; i < this.myPlayer.bindings.Length; i++)
			{
				int equipable = this.myPlayer.bindings[i].equipable;
				if (replaceIndex == 0 || replaceIndex == equipable)
				{
					base.CreateButton(new ItemInfo(this.subClone.Q("Binding" + (i + 1).ToString(), null), new InventoryItem(MonoBehaviourSingleton<SceneManager>.Instance.GetEntity<ShItem>(equipable), 0, 0), false, i.ToString(), ButtonType.Equipped, 1f));
				}
				foreach (int num in this.myPlayer.bindings[i].attachments)
				{
					ShAttachment shAttachment;
					if ((replaceIndex == 0 || replaceIndex == num) && MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity<ShAttachment>(num, out shAttachment) && shAttachment.needItem)
					{
						base.CreateButton(new ItemInfo(this.subClone.Q("Binding" + (i + 1).ToString(), null).Q("Attachments", null), new InventoryItem(shAttachment, 0, 0), false, i.ToString() + (shAttachment.AttachmentType + 1).ToString(), ButtonType.Equipped, 0.33333334f));
					}
				}
			}
			foreach (ShWearable shWearable in this.myPlayer.curWearables)
			{
				if (shWearable && (replaceIndex == 0 || shWearable.index == replaceIndex))
				{
					VisualElement parent = this.subClone.Q(shWearable.type.ToString(), null);
					InventoryItem invItem = new InventoryItem(MonoBehaviourSingleton<SceneManager>.Instance.GetEntity<ShItem>(shWearable.index), 0, 0);
					bool showValue = false;
					int type = (int)shWearable.type;
					base.CreateButton(new ItemInfo(parent, invItem, showValue, type.ToString(), ButtonType.Equipped, 1f));
				}
			}
		}

		// Token: 0x06000ED1 RID: 3793 RVA: 0x00044E54 File Offset: 0x00043054
		public override List<DragAndDropContainer> GetDragContainers(ItemActions itemActions)
		{
			List<DragAndDropContainer> list = new List<DragAndDropContainer>();
			if (itemActions.EquipTest())
			{
				byte b = 0;
				while ((int)b < this.myPlayer.bindings.Length)
				{
					byte currentIndex = b;
					EquipableActions equipableActions = itemActions as EquipableActions;
					if (equipableActions != null)
					{
						VisualElement containerElement = this.subClone.Q("Binding" + ((int)(b + 1)).ToString(), null);
						list.Add(new DragAndDropContainer(containerElement, null, delegate(int x)
						{
							equipableActions.SubMenuAction(currentIndex);
						}));
					}
					else
					{
						AttachmentActions attachmentActions = itemActions as AttachmentActions;
						if (attachmentActions != null)
						{
							VisualElement containerElement2 = this.subClone.Q("Binding" + ((int)(b + 1)).ToString(), null);
							list.Add(new DragAndDropContainer(containerElement2, () => MonoBehaviourSingleton<SceneManager>.Instance.GetEntity<ShEquipable>(this.myPlayer.bindings[(int)currentIndex].equipable).AcceptAttachment(attachmentActions.attachment) != null, delegate(int x)
							{
								attachmentActions.SubMenuAction(currentIndex);
							}));
						}
						else
						{
							WearableActions wearableActions = itemActions as WearableActions;
							if (wearableActions != null)
							{
								VisualElement containerElement3 = this.subClone.Q(wearableActions.wearable.type.ToString(), null);
								list.Add(new DragAndDropContainer(containerElement3, null, delegate(int x)
								{
									wearableActions.PutOnAction();
								}));
							}
						}
					}
					b += 1;
				}
			}
			else if (itemActions.UnequipTest())
			{
				VisualElement containerElement4 = this.uiClone.Q("InventoryScrollView", null);
				EquipableActions equipableActions = itemActions as EquipableActions;
				if (equipableActions != null)
				{
					list.Add(new DragAndDropContainer(containerElement4, null, delegate(int x)
					{
						equipableActions.UnbindAction();
					}));
				}
				else
				{
					AttachmentActions attachmentActions = itemActions as AttachmentActions;
					if (attachmentActions != null)
					{
						list.Add(new DragAndDropContainer(containerElement4, null, delegate(int x)
						{
							attachmentActions.UnbindAttachmentAction();
						}));
					}
					else
					{
						WearableActions wearableActions = itemActions as WearableActions;
						if (wearableActions != null)
						{
							list.Add(new DragAndDropContainer(containerElement4, null, delegate(int x)
							{
								wearableActions.TakeOffAction();
							}));
						}
					}
				}
			}
			return list;
		}

		// Token: 0x040012F1 RID: 4849
		private PlayerRenderer playerRenderer;

		// Token: 0x040012F2 RID: 4850
		private const string bindingElement = "Binding";
	}
}
