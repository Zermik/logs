using System;
using System.Collections.Generic;
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
	// Token: 0x02000306 RID: 774
	public class RegisterMenu : Menu
	{
		// Token: 0x06000F4A RID: 3914 RVA: 0x00046F8C File Offset: 0x0004518C
		public override void Initialize(params object[] args)
		{
			base.Initialize(args);
			this.selectionPanel = this.uiClone.Q("SelectionPanel", null);
			this.credentialPanel = this.uiClone.Q("CredentialPanel", null);
			this.customizationPanel = this.uiClone.Q("CustomizationPanel", null);
			this.goButton = this.uiClone.Q("Go", null);
			this.goButton.clicked += this.Go;
			this.backButton = this.uiClone.Q("Back", null);
			this.backButton.clicked += this.OpenSelectionMenu;
			this.deleteButton = this.uiClone.Q("Delete", null);
			this.deleteButton.clicked += this.Delete;
			this.loginButton = this.uiClone.Q("Login", null);
			this.loginButton.clicked += this.OpenLoginMenu;
			base.AddButtonAction("Register", new Action(this.OpenRegisterMenu));
			base.AddButtonAction("Exit1", new Action(this.Exit));
			base.AddButtonAction("Exit2", new Action(this.Exit));
			this.usernameField = this.uiClone.Q("Username", null);
			this.passwordField = this.uiClone.Q("Password", null);
			this.skinDropdown = this.uiClone.Q("Skin", null);
			this.skinDropdown.RegisterValueChangedCallback(delegate(ChangeEvent<string> x)
			{
				this.RefreshSkin();
			});
			string[] names = Enum.GetNames(typeof(WearableType));
			this.wearableDropdowns = new DropdownField[names.Length];
			for (int i = 0; i < names.Length; i++)
			{
				DropdownField dropdownField = this.uiClone.Q(names[i], null);
				this.wearableDropdowns[i] = dropdownField;
				dropdownField.RegisterValueChangedCallback(delegate(ChangeEvent<string> x)
				{
					this.RefreshWearable();
				});
			}
			if (!(bool)args[0])
			{
				this.loginButton.SetEnabled(false);
			}
			this.skinPrefabs = (List<ShPlayer>)args[1];
			foreach (ShPlayer shPlayer in this.skinPrefabs)
			{
				this.skinDropdown.choices.Add(shPlayer.name);
			}
			if (PlayerPrefs.HasKey("Username"))
			{
				this.usernameField.value = PlayerPrefs.GetString("Username");
			}
			if (PlayerPrefs.HasKey("Password"))
			{
				this.passwordField.value = PlayerPrefs.GetString("Password");
			}
			this.playerRenderer = Object.Instantiate<PlayerRenderer>(MonoBehaviourSingleton<ClManager>.Instance.playerRendererPrefab);
			this.uiClone.Q("PlayerPreview", null).style.backgroundImage = new StyleBackground(Background.FromRenderTexture(this.playerRenderer.cam.targetTexture));
		}

		// Token: 0x06000F4B RID: 3915 RVA: 0x000472A0 File Offset: 0x000454A0
		public void OpenRegisterMenu()
		{
			this.login = false;
			this.selectionPanel.visible = false;
			this.credentialPanel.visible = true;
			this.customizationPanel.visible = true;
			this.skinDropdown.index = -1;
			this.skinDropdown.index = Random.Range(0, this.skinDropdown.choices.Count);
		}

		// Token: 0x06000F4C RID: 3916 RVA: 0x00047305 File Offset: 0x00045505
		public void OpenLoginMenu()
		{
			this.login = true;
			this.selectionPanel.visible = false;
			this.credentialPanel.visible = true;
		}

		// Token: 0x06000F4D RID: 3917 RVA: 0x00047326 File Offset: 0x00045526
		public void OpenSelectionMenu()
		{
			this.credentialPanel.visible = false;
			this.customizationPanel.visible = false;
			this.selectionPanel.visible = true;
		}

		// Token: 0x06000F4E RID: 3918 RVA: 0x0004734C File Offset: 0x0004554C
		public void Go()
		{
			this.usernameField.value = this.usernameField.value.CleanCredential();
			this.passwordField.value = this.passwordField.value.CleanCredential();
			this.goButton.SetEnabled(false);
			this.backButton.SetEnabled(false);
			this.deleteButton.SetEnabled(false);
			if (this.login)
			{
				MonoBehaviourSingleton<ClManager>.Instance.SendToServer(PacketFlags.Reliable, SvPacket.Login, new object[]
				{
					this.usernameField.text,
					Animator.StringToHash(this.passwordField.text),
					MonoBehaviourSingleton<ClManager>.Instance.LanguageIndex,
					SystemInfo.deviceUniqueIdentifier,
					MonoBehaviourSingleton<ClManager>.Instance.profileURL
				});
				return;
			}
			byte[] array = new byte[this.wearableDropdowns.Length];
			for (int i = 0; i < this.wearableDropdowns.Length; i++)
			{
				array[i] = (byte)this.wearableDropdowns[i].index;
			}
			MonoBehaviourSingleton<ClManager>.Instance.SendToServer(PacketFlags.Reliable, SvPacket.Register, new object[]
			{
				this.usernameField.value,
				Animator.StringToHash(this.passwordField.value),
				this.skinDropdown.index,
				array,
				MonoBehaviourSingleton<ClManager>.Instance.LanguageIndex,
				SystemInfo.deviceUniqueIdentifier,
				MonoBehaviourSingleton<ClManager>.Instance.profileURL
			});
		}

		// Token: 0x06000F4F RID: 3919 RVA: 0x000474C3 File Offset: 0x000456C3
		public void Exit()
		{
			MonoBehaviourSingleton<SceneManager>.Instance.ReloadGame(0U);
		}

		// Token: 0x06000F50 RID: 3920 RVA: 0x000474D0 File Offset: 0x000456D0
		public void Delete()
		{
			MonoBehaviourSingleton<ClManager>.Instance.SendToServer(PacketFlags.Reliable, SvPacket.Delete, new object[]
			{
				this.usernameField.value,
				Animator.StringToHash(this.passwordField.value)
			});
		}

		// Token: 0x06000F51 RID: 3921 RVA: 0x0004750A File Offset: 0x0004570A
		public void RefreshSkin()
		{
			this.RefreshView(true);
		}

		// Token: 0x06000F52 RID: 3922 RVA: 0x00047513 File Offset: 0x00045713
		public void RefreshWearable()
		{
			this.RefreshView(false);
		}

		// Token: 0x06000F53 RID: 3923 RVA: 0x0004751C File Offset: 0x0004571C
		private void RefreshView(bool refreshSkin)
		{
			ShPlayer shPlayer = this.skinPrefabs[this.skinDropdown.index];
			List<ShWearable> list = new List<ShWearable>();
			if (refreshSkin)
			{
				int num = 0;
				foreach (WearableOptions ptr in shPlayer.wearableOptions)
				{
					DropdownField dropdownField = this.wearableDropdowns[num];
					dropdownField.choices.Clear();
					foreach (string name in ptr.wearableNames)
					{
						ShWearable entity = MonoBehaviourSingleton<SceneManager>.Instance.GetEntity<ShWearable>(name);
						dropdownField.choices.Add(entity.itemName);
					}
					dropdownField.index = -1;
					dropdownField.index = Random.Range(0, dropdownField.choices.Count);
					num++;
				}
			}
			int num2 = 0;
			foreach (WearableOptions wearableOptions2 in shPlayer.wearableOptions)
			{
				DropdownField dropdownField2 = this.wearableDropdowns[num2];
				list.Add(MonoBehaviourSingleton<SceneManager>.Instance.GetEntity<ShWearable>(wearableOptions2.wearableNames[dropdownField2.index]));
				num2++;
			}
			this.playerRenderer.Refresh(shPlayer.index, list);
		}

		// Token: 0x06000F54 RID: 3924 RVA: 0x0004765C File Offset: 0x0004585C
		public void Fail()
		{
			this.goButton.SetEnabled(true);
			this.backButton.SetEnabled(true);
			this.deleteButton.SetEnabled(true);
		}

		// Token: 0x06000F55 RID: 3925 RVA: 0x00047684 File Offset: 0x00045884
		public override void Destroy()
		{
			PlayerPrefs.SetString("Username", this.usernameField.value);
			PlayerPrefs.SetString("Password", this.passwordField.value);
			PlayerPrefs.Save();
			Object.Destroy(this.playerRenderer.gameObject);
			base.Destroy();
		}

		// Token: 0x04001326 RID: 4902
		protected Button goButton;

		// Token: 0x04001327 RID: 4903
		protected Button backButton;

		// Token: 0x04001328 RID: 4904
		protected Button deleteButton;

		// Token: 0x04001329 RID: 4905
		protected Button loginButton;

		// Token: 0x0400132A RID: 4906
		protected TextField usernameField;

		// Token: 0x0400132B RID: 4907
		protected TextField passwordField;

		// Token: 0x0400132C RID: 4908
		protected DropdownField skinDropdown;

		// Token: 0x0400132D RID: 4909
		protected DropdownField[] wearableDropdowns;

		// Token: 0x0400132E RID: 4910
		protected VisualElement selectionPanel;

		// Token: 0x0400132F RID: 4911
		protected VisualElement credentialPanel;

		// Token: 0x04001330 RID: 4912
		protected VisualElement customizationPanel;

		// Token: 0x04001331 RID: 4913
		protected VisualElement playerPreviewElement;

		// Token: 0x04001332 RID: 4914
		protected PlayerRenderer playerRenderer;

		// Token: 0x04001333 RID: 4915
		protected bool login;

		// Token: 0x04001334 RID: 4916
		protected List<ShPlayer> skinPrefabs;
	}
}
