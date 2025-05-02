using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BrokeProtocol.Client.Buttons;
using BrokeProtocol.Client.Opus;
using BrokeProtocol.Client.UI;
using BrokeProtocol.Collections;
using BrokeProtocol.Entities;
using BrokeProtocol.LiteDB;
using BrokeProtocol.Required;
using BrokeProtocol.Utility;
using BrokeProtocol.Utility.Jobs;
using BrokeProtocol.Utility.Networking;
using ENet;
using Newtonsoft.Json;
using Proyecto26;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace BrokeProtocol.Managers
{
	// Token: 0x02000188 RID: 392
	public class ClManager : MonoBehaviourSingleton<ClManager>
	{
		// Token: 0x06000928 RID: 2344 RVA: 0x0002B1EC File Offset: 0x000293EC
		public ClientSector GetSector(int placeIndex, Vector3 position)
		{
			Vector2Int item = new Vector2Int(Mathf.FloorToInt(position.x / 32f), Mathf.FloorToInt(position.z / 32f));
			ValueTuple<int, Vector2Int> key = new ValueTuple<int, Vector2Int>(placeIndex, item);
			ClientSector result;
			if (!this.sectors.TryGetValue(key, out result))
			{
				return new ClientSector(key, null);
			}
			return result;
		}

		// Token: 0x06000929 RID: 2345 RVA: 0x0002B244 File Offset: 0x00029444
		public void UpdateSectors(Transform t)
		{
			Renderer componentInChildren = t.GetComponentInChildren<Renderer>();
			Vector3 position;
			Vector2Int zero;
			if (componentInChildren)
			{
				position = componentInChildren.bounds.center;
				Vector3 extents = componentInChildren.bounds.extents;
				zero = new Vector2Int(this.clientSectorRange + Mathf.FloorToInt(extents.x / 32f), this.clientSectorRange + Mathf.FloorToInt(extents.z / 32f));
			}
			else
			{
				position = t.position;
				zero = Vector2Int.zero;
			}
			int siblingIndex = t.parent.GetSiblingIndex();
			ClientSector clientSector = this.GetSector(siblingIndex, position);
			StreamingObject key = new StreamingObject(t);
			if (clientSector.controlled.ContainsKey(key))
			{
				Util.Log("Duplicate Object: " + t.name + " at " + t.LogTransform(), LogLevel.Warn);
				return;
			}
			Vector2Int vector2Int = clientSector.key.Item2 - zero;
			Vector2Int vector2Int2 = clientSector.key.Item2 + zero;
			GameObjectContainer value = new GameObjectContainer(t.gameObject);
			for (int i = vector2Int.x; i <= vector2Int2.x; i++)
			{
				for (int j = vector2Int.y; j <= vector2Int2.y; j++)
				{
					ValueTuple<int, Vector2Int> key2 = new ValueTuple<int, Vector2Int>(siblingIndex, new Vector2Int(i, j));
					ClientSector clientSector2;
					if (!this.sectors.TryGetValue(key2, out clientSector2))
					{
						clientSector2 = (this.sectors[key2] = new ClientSector(key2, null));
					}
					clientSector2.controlled.TryAdd(key, value);
				}
			}
		}

		// Token: 0x0600092A RID: 2346 RVA: 0x0002B3D4 File Offset: 0x000295D4
		public void NewSector()
		{
			ShPlayer target = MonoBehaviourSingleton<MainCamera>.Instance.target;
			if (!target)
			{
				return;
			}
			Place place = target.Place;
			ClientSector clientSector = this.GetSector(place.parentPlace.Index, MonoBehaviourSingleton<MainCamera>.Instance.worldCameraT.position);
			if (this.sector == clientSector)
			{
				return;
			}
			if (this.sector != null)
			{
				foreach (KeyValuePair<StreamingObject, GameObjectContainer> keyValuePair in this.sector.controlled)
				{
					if (!clientSector.controlled.ContainsKey(keyValuePair.Key))
					{
						Object.Destroy(keyValuePair.Value.go);
					}
				}
				using (Dictionary<StreamingObject, GameObjectContainer>.Enumerator enumerator = clientSector.controlled.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<StreamingObject, GameObjectContainer> pair = enumerator.Current;
						if (!this.sector.controlled.ContainsKey(pair.Key))
						{
							this.AddStreamingObject(place, pair);
						}
					}
					goto IL_136;
				}
			}
			foreach (KeyValuePair<StreamingObject, GameObjectContainer> pair2 in clientSector.controlled)
			{
				this.AddStreamingObject(place, pair2);
			}
			IL_136:
			this.sector = clientSector;
		}

		// Token: 0x0600092B RID: 2347 RVA: 0x0002B548 File Offset: 0x00029748
		private void AddStreamingObject(Place place, KeyValuePair<StreamingObject, GameObjectContainer> pair)
		{
			GameObject gameObject;
			if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetPrefab(pair.Key.prefabIndex, out gameObject))
			{
				TransformStruct transformStruct = pair.Key.transformStruct;
				Transform mTransform = place.mTransform;
				GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, mTransform.TransformPoint(transformStruct.position), transformStruct.rotation, mTransform);
				gameObject2.name = gameObject.name;
				gameObject2.transform.localScale = transformStruct.scale;
				pair.Value.go = gameObject2;
			}
		}

		// Token: 0x0600092C RID: 2348 RVA: 0x0002B5C8 File Offset: 0x000297C8
		public bool GetConnectAtStartup(out string hostname, out ushort port)
		{
			if (ClManager.reconnect)
			{
				ClManager.reconnect = false;
				hostname = ClManager.lastHostname;
				port = ClManager.lastPort;
				return true;
			}
			if (!ClManager.alreadyStarted)
			{
				ClManager.alreadyStarted = true;
				string serverString;
				if (Util.GetCommandArgument("-connect", out serverString))
				{
					ServerAddress serverAddress = new ServerAddress(serverString);
					hostname = serverAddress.ip;
					port = serverAddress.port;
					return true;
				}
			}
			hostname = null;
			port = 0;
			return false;
		}

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x0600092D RID: 2349 RVA: 0x0002B62E File Offset: 0x0002982E
		// (set) Token: 0x0600092E RID: 2350 RVA: 0x0002B638 File Offset: 0x00029838
		public int LimitFPS
		{
			get
			{
				return this.framerateLimit;
			}
			set
			{
				this.framerateLimit = value;
				Application.targetFrameRate = value;
			}
		}

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x0600092F RID: 2351 RVA: 0x0002B654 File Offset: 0x00029854
		// (set) Token: 0x06000930 RID: 2352 RVA: 0x0002B65B File Offset: 0x0002985B
		public int RenderQuality
		{
			get
			{
				return QualitySettings.GetQualityLevel();
			}
			set
			{
				QualitySettings.SetQualityLevel(value);
			}
		}

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x06000931 RID: 2353 RVA: 0x0002B663 File Offset: 0x00029863
		// (set) Token: 0x06000932 RID: 2354 RVA: 0x0002B674 File Offset: 0x00029874
		public float RenderDistance
		{
			get
			{
				return MonoBehaviourSingleton<MainCamera>.Instance.worldCamera.farClipPlane;
			}
			set
			{
				MonoBehaviourSingleton<MainCamera>.Instance.worldCamera.farClipPlane = value;
				MonoBehaviourSingleton<World>.Instance.SetupTiles();
			}
		}

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x06000933 RID: 2355 RVA: 0x0002B690 File Offset: 0x00029890
		// (set) Token: 0x06000934 RID: 2356 RVA: 0x0002B698 File Offset: 0x00029898
		public int LanguageIndex
		{
			get
			{
				return this.languageIndex;
			}
			set
			{
				this.languageIndex = value;
			}
		}

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x06000935 RID: 2357 RVA: 0x0002B6A1 File Offset: 0x000298A1
		// (set) Token: 0x06000936 RID: 2358 RVA: 0x0002B6AC File Offset: 0x000298AC
		public float MainVolume
		{
			get
			{
				return this.masterVolume;
			}
			set
			{
				this.masterVolume = value;
				AudioListener.volume = value;
			}
		}

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x06000937 RID: 2359 RVA: 0x0002B6C8 File Offset: 0x000298C8
		// (set) Token: 0x06000938 RID: 2360 RVA: 0x0002B6D0 File Offset: 0x000298D0
		public float MusicVolume
		{
			get
			{
				return this.musicVolume;
			}
			set
			{
				this.musicVolume = value;
				if (this.music)
				{
					this.music.volume = this.musicVolume;
				}
			}
		}

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x06000939 RID: 2361 RVA: 0x0002B6F7 File Offset: 0x000298F7
		// (set) Token: 0x0600093A RID: 2362 RVA: 0x0002B700 File Offset: 0x00029900
		public float VoiceVolume
		{
			get
			{
				return this.voiceVolume;
			}
			set
			{
				this.voiceVolume = value;
				foreach (ShPlayer shPlayer in EntityCollections.Players)
				{
					shPlayer.clPlayer.UpdateVoiceVolume();
				}
			}
		}

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x0600093B RID: 2363 RVA: 0x0002B758 File Offset: 0x00029958
		// (set) Token: 0x0600093C RID: 2364 RVA: 0x0002B760 File Offset: 0x00029960
		public bool AllowHUD
		{
			get
			{
				return this.allowHUD;
			}
			set
			{
				this.allowHUD = value;
				if (this.allowHUD != this.hud.Visible)
				{
					this.ToggleHUD();
				}
			}
		}

		// Token: 0x0600093D RID: 2365 RVA: 0x0002B782 File Offset: 0x00029982
		public bool HasPermissionClient(PermEnum p)
		{
			return this.permissions[(int)p] > 0;
		}

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x0600093E RID: 2366 RVA: 0x0002B790 File Offset: 0x00029990
		public Menu CurrentMenu
		{
			get
			{
				Menu result;
				if (!this.menus.TryGetValue("Default", out result))
				{
					return null;
				}
				return result;
			}
		}

		// Token: 0x0600093F RID: 2367 RVA: 0x0002B7B4 File Offset: 0x000299B4
		public void GetProfileImage(VisualElement image, string profile, string name = "")
		{
			if (this.customDownloads && !string.IsNullOrWhiteSpace(profile) && profile.StartsWith("https"))
			{
				this.GetImage(image, profile);
				return;
			}
			if (!string.IsNullOrEmpty(name))
			{
				image.style.unityBackgroundImageTintColor = name.ToRandomColor();
			}
		}

		// Token: 0x06000940 RID: 2368 RVA: 0x0002B805 File Offset: 0x00029A05
		public void GetImage(VisualElement image, string URL)
		{
			if (!string.IsNullOrWhiteSpace(URL))
			{
				this.imageQueue.Enqueue(new ImageItem(image, URL));
			}
		}

		// Token: 0x06000941 RID: 2369 RVA: 0x0002B824 File Offset: 0x00029A24
		public void ReadSettings()
		{
			string text = null;
			try
			{
				text = File.ReadAllText(Paths.persistentFile);
			}
			catch (Exception)
			{
			}
			finally
			{
				if (text != null)
				{
					this.persistentSettings = JsonConvert.DeserializeObject<PersistentSettings>(text);
				}
				if (this.persistentSettings == null)
				{
					this.persistentSettings = new PersistentSettings();
				}
			}
		}

		// Token: 0x06000942 RID: 2370 RVA: 0x0002B884 File Offset: 0x00029A84
		public void WriteSettings()
		{
			File.WriteAllText(Paths.persistentFile, JsonConvert.SerializeObject(this.persistentSettings, Formatting.Indented));
		}

		// Token: 0x06000943 RID: 2371 RVA: 0x0002B89C File Offset: 0x00029A9C
		public IEnumerator TargetLockedLoop()
		{
			CircleGraphic targetGraphic = Object.Instantiate<CircleGraphic>(this.targetGraphicPrefab, this.canvas.transform);
			ShEntity target = this.myPlayer.lockOnTarget;
			while (this.myPlayer.lockOnTarget && this.myPlayer.lockOnTarget == target)
			{
				Vector3 vector = MonoBehaviourSingleton<MainCamera>.Instance.worldCamera.WorldToScreenPoint(this.myPlayer.lockOnTarget.CenterBounds);
				if (vector.z > 0f)
				{
					targetGraphic.rectT.position = vector;
					targetGraphic.color = (this.myPlayer.LockOnValid ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f));
				}
				else
				{
					targetGraphic.color = Color.clear;
				}
				yield return null;
			}
			Object.Destroy(targetGraphic);
			yield break;
		}

		// Token: 0x06000944 RID: 2372 RVA: 0x0002B8AC File Offset: 0x00029AAC
		private void Update()
		{
			AudioListener.volume = (MonoBehaviourSingleton<MainCamera>.Instance.isActiveAndEnabled ? this.masterVolume : 0f);
			if (this.myPlayer)
			{
				this.HandleInput();
				this.NewSector();
				if (this.myPlayer.curMount && this.uiClone != null)
				{
					Label label = this.uiClone.Q("speed-value", null);
					if (label != null)
					{
						label.text = ((int)(1.467f * this.myPlayer.curMount.mainT.InverseTransformVector(this.myPlayer.curMount.relativeVelocity).z)).ToString() + " MPH";
					}
					Label label2 = this.uiClone.Q("weapon-value", null);
					if (label2 != null)
					{
						WeaponSet weaponSet = this.myPlayer.curMount.GetWeaponSet((int)this.myPlayer.seat);
						label2.text = weaponSet.thrownName;
					}
					Label label3 = this.uiClone.Q("ammo-value", null);
					if (label3 != null)
					{
						WeaponSet weaponSet2 = this.myPlayer.curMount.GetWeaponSet((int)this.myPlayer.seat);
						label3.text = weaponSet2.curAmmo.ToString();
					}
					Label label4 = this.uiClone.Q("range-value", null);
					if (label4 != null)
					{
						Seat seat = this.myPlayer.curMount.seats[(int)this.myPlayer.seat];
						Transform barrelT = seat.barrelT;
						RaycastHit raycastHit;
						if (Physics.Raycast(seat.barrelT.position, seat.barrelT.forward, out raycastHit, 362.03867f, 26373))
						{
							label4.text = ((int)raycastHit.distance).ToString() + " FT";
						}
						else
						{
							label4.text = "---";
						}
					}
					Label label5 = this.uiClone.Q("elevation-value", null);
					if (label5 != null)
					{
						Transform barrelT2 = this.myPlayer.curMount.seats[(int)this.myPlayer.seat].barrelT;
						int num = (int)Mathf.DeltaAngle(0f, -barrelT2.localEulerAngles.x);
						string text = num.ToString() + "°";
						if (num >= 0)
						{
							text = "+" + text;
						}
						label5.text = text;
					}
					Label label6 = this.uiClone.Q("bearing-value", null);
					if (label6 != null)
					{
						Transform barrelT3 = this.myPlayer.curMount.seats[(int)this.myPlayer.seat].barrelT;
						Vector3 vector = this.myPlayer.curMount.mainT.InverseTransformDirection(barrelT3.forward);
						Vector3 normalized = new Vector3(vector.x, 0f, vector.z).normalized;
						label6.text = ((int)Vector3.SignedAngle(Vector3.forward, normalized, Vector3.up)).ToString() + "°";
					}
				}
			}
			while (!this.imageDownloading && this.imageQueue.Count > 0)
			{
				ImageItem imageItem = this.imageQueue.Dequeue();
				if (imageItem.image.panel != null)
				{
					Texture2D texture2D;
					if (this.imageSet.TryGetValue(imageItem.URL, out texture2D))
					{
						if (texture2D)
						{
							imageItem.image.style.backgroundImage = texture2D;
						}
					}
					else
					{
						this.imageDownloading = true;
						RestClient.Get(new RequestHelper
						{
							RedirectLimit = new int?(0),
							Retries = 0,
							Timeout = new int?(4),
							Uri = imageItem.URL,
							DownloadHandler = new DownloadHandlerTexture()
						}).Then(delegate(ResponseHelper res)
						{
							Texture2D texture = ((DownloadHandlerTexture)res.Request.downloadHandler).texture;
							this.imageSet[imageItem.URL] = texture;
							if (imageItem.image.panel != null)
							{
								imageItem.image.style.backgroundImage = texture;
							}
						}).Catch(delegate(Exception _)
						{
							this.imageSet[imageItem.URL] = null;
						}).Finally(delegate
						{
							this.imageDownloading = false;
						});
					}
				}
			}
		}

		// Token: 0x06000945 RID: 2373 RVA: 0x0002BCD7 File Offset: 0x00029ED7
		private void CleanupMic()
		{
			if (this.microphoneClip)
			{
				Object.Destroy(this.microphoneClip);
				this.microphoneClip = null;
				Microphone.End(this.inputDevice);
			}
		}

		// Token: 0x06000946 RID: 2374 RVA: 0x0002BD04 File Offset: 0x00029F04
		protected void OnDestroy()
		{
			this.CleanupMic();
			if (this.mapTextures != null)
			{
				RenderTexture[] array = this.mapTextures;
				for (int i = 0; i < array.Length; i++)
				{
					Object.Destroy(array[i]);
				}
			}
		}

		// Token: 0x06000947 RID: 2375 RVA: 0x0002BD3C File Offset: 0x00029F3C
		private void EncodeSample()
		{
			this.microphoneClip.GetData(this.recordData, this.recordIndex);
			float num = 0.0010416667f;
			for (int i = 0; i < 959; i++)
			{
				float num2 = (float)(i * this.recordSampleSize) * num;
				int num3 = (int)num2;
				float t = num2 - (float)num3;
				ClManager.sampleBuffer[i] = 2f * Mathf.Lerp(this.recordData[num3], this.recordData[num3 + 1], t);
			}
			ClManager.sampleBuffer[959] = 2f * this.recordData[this.recordSampleSize - 1];
			this.recordIndex += this.recordSampleSize;
			int num4 = this.encoder.EncodeFloat(ClManager.sampleBuffer, ref ClManager.encodedBuffer);
			if (num4 > 1)
			{
				this.SendToServer(PacketFlags.Unthrottled, SvPacket.ChatVoice, new object[]
				{
					new SizedArray(ClManager.encodedBuffer, num4)
				});
			}
		}

		// Token: 0x06000948 RID: 2376 RVA: 0x0002BE24 File Offset: 0x0002A024
		private void MyAltFire()
		{
			ShUnderbarrel curUnderbarrel = this.myPlayer.curEquipable.curUnderbarrel;
			if (this.myPlayer.IsMountArmed)
			{
				this.SendToServer(PacketFlags.Reliable, SvPacket.ToggleWeapon, Array.Empty<object>());
				return;
			}
			if (curUnderbarrel.HasWeaponSet(0))
			{
				this.myPlayer.Fire(curUnderbarrel.index);
				return;
			}
			if (!this.myPlayer.stance.fixedForward)
			{
				this.SendToServer(PacketFlags.Reliable, SvPacket.AltFire, Array.Empty<object>());
			}
		}

		// Token: 0x06000949 RID: 2377 RVA: 0x0002BE9C File Offset: 0x0002A09C
		public void ToggleCursor()
		{
			this.toggleCursor = !this.toggleCursor;
			MonoBehaviourSingleton<SceneManager>.Instance.ShowCursor = this.toggleCursor;
		}

		// Token: 0x0600094A RID: 2378 RVA: 0x0002BEC0 File Offset: 0x0002A0C0
		public void HandleInput()
		{
			if (this.GetButton(InputType.Cancel, false) && !MonoBehaviourSingleton<SceneManager>.Instance.CloseTopPanel())
			{
				this.ShowPauseMenu();
			}
			if (!MonoBehaviourSingleton<SceneManager>.Instance.IsTyping())
			{
				if (this.myPlayer.IsUp && this.myPlayer.SpecSelf)
				{
					if (this.GetButton(InputType.Forward, false))
					{
						this.myPlayer.input.x = 1f;
					}
					else if (this.GetButton(InputType.Backward, false))
					{
						this.myPlayer.input.x = -1f;
					}
					else
					{
						this.myPlayer.input.x = 0f;
					}
					if (this.GetButton(InputType.YawLeft, false))
					{
						this.myPlayer.input.y = -1f;
					}
					else if (this.GetButton(InputType.YawRight, false))
					{
						this.myPlayer.input.y = 1f;
					}
					else
					{
						this.myPlayer.input.y = 0f;
					}
					if (this.GetButton(InputType.Left, false))
					{
						this.myPlayer.input.z = 1f;
					}
					else if (this.GetButton(InputType.Right, false))
					{
						this.myPlayer.input.z = -1f;
					}
					else
					{
						this.myPlayer.input.z = 0f;
					}
					this.fire = false;
					if (!MonoBehaviourSingleton<SceneManager>.Instance.ShowCursor)
					{
						ShAircraft shAircraft;
						if (this.GetButton(InputType.Zoom, false) && this.myPlayer.curEquipable.CanZoom)
						{
							this.myPlayer.TryUpdateMode(3);
						}
						else if (this.myPlayer.mode == 3)
						{
							this.myPlayer.TryUpdateMode(0);
						}
						else if (this.GetButton(InputType.Fast, false))
						{
							this.myPlayer.TryUpdateMode(1);
						}
						else if (this.GetButton(InputType.Slow, false))
						{
							this.myPlayer.TryUpdateMode(2);
						}
						else if (this.myPlayer.mode == 2 || (this.myPlayer.mode == 1 && (this.myPlayer.input.IsZero() || this.myPlayer.IsControlledMount<ShAircraft>(out shAircraft))))
						{
							this.myPlayer.TryUpdateMode(0);
						}
						float num = Time.deltaTime * 400f;
						if (this.GetButton(InputType.Fire, false) || this.GetButton(InputType.FireStatic, false))
						{
							if (this.myPlayer.mode == 1 && !this.GetButton(InputType.Fast, false) && !this.myPlayer.curMount)
							{
								this.myPlayer.TryUpdateMode(0);
							}
							this.fire = true;
						}
						else if ((this.GetButton(InputType.Use1, false) || this.GetButton(InputType.Use2, false)) && !this.currentActionMenu)
						{
							this.EntityActionMenu();
						}
						else if (this.GetButton(InputType.Map, false))
						{
							this.hud.SetMapSize(!this.hud.isFullMap);
						}
						else if (this.GetButton(InputType.Hands, false))
						{
							this.SendEquipable(this.myPlayer.Hands.index);
						}
						else if (this.GetButton(InputType.Surrender, false))
						{
							this.SendEquipable(this.myPlayer.Surrender.index);
						}
						else if (this.GetButton(InputType.Reload, false))
						{
							this.SendToServer(PacketFlags.Reliable, SvPacket.Reload, Array.Empty<object>());
						}
						else if (this.GetButton(InputType.AltFire, false))
						{
							this.MyAltFire();
						}
						else if (this.GetButton(InputType.Alert, false))
						{
							this.myPlayer.GetControlled().clMountable.PlayAlert();
						}
						else if (this.GetButton(InputType.Drop, false))
						{
							this.SendToServer(PacketFlags.Reliable, SvPacket.DropEquipable, Array.Empty<object>());
						}
						else if (this.GetButton(InputType.EquipmentMenu, false))
						{
							this.ShowEquipmentInventory();
						}
						else if (this.GetButton(InputType.HealthMenu, false))
						{
							this.ShowHealthMenu(this.myPlayer);
						}
						else if (this.GetButton(InputType.Equipment1, false))
						{
							this.SendToServer(PacketFlags.Reliable, SvPacket.UseBind, new object[]
							{
								0
							});
						}
						else if (this.GetButton(InputType.Equipment2, false))
						{
							this.SendToServer(PacketFlags.Reliable, SvPacket.UseBind, new object[]
							{
								1
							});
						}
						else if (this.GetButton(InputType.Equipment3, false))
						{
							this.SendToServer(PacketFlags.Reliable, SvPacket.UseBind, new object[]
							{
								2
							});
						}
						else if (this.GetButton(InputType.Equipment4, false))
						{
							this.SendToServer(PacketFlags.Reliable, SvPacket.UseBind, new object[]
							{
								3
							});
						}
						if (this.GetButton(InputType.Crouch, false))
						{
							if (this.myPlayer.curMount && this.GetButton(InputType.Crouch, true))
							{
								this.SendToServer(PacketFlags.Reliable, SvPacket.MoveSeatDown, Array.Empty<object>());
							}
							else if (this.myPlayer.CanCrouch && this.myPlayer.StanceIndex == StanceIndex.Stand)
							{
								this.myPlayer.clPlayer.SetStance(StanceIndex.Crouch);
								this.SendToServer(PacketFlags.Reliable, SvPacket.Crouch, new object[]
								{
									true
								});
							}
						}
						else if (this.myPlayer.StanceIndex == StanceIndex.Crouch)
						{
							this.myPlayer.clPlayer.SetStance(StanceIndex.Stand);
							this.SendToServer(PacketFlags.Reliable, SvPacket.Crouch, new object[]
							{
								false
							});
						}
						else if (this.GetButton(InputType.Jump, false))
						{
							if (this.myPlayer.curMount)
							{
								this.SendToServer(PacketFlags.Reliable, SvPacket.MoveSeatUp, Array.Empty<object>());
							}
							else if (this.myPlayer.CanJump)
							{
								this.myPlayer.clPlayer.Jump();
								this.SendToServer(PacketFlags.Reliable, SvPacket.Jump, Array.Empty<object>());
							}
						}
						if (this.GetButton(InputType.Point, false) != this.myPlayer.pointing)
						{
							this.myPlayer.pointing = !this.myPlayer.pointing;
							this.SendToServer(PacketFlags.Reliable, SvPacket.Point, new object[]
							{
								this.myPlayer.pointing
							});
						}
						if (!this.myPlayer.OutsideController)
						{
							Vector2 vector = this.lookInput = Mouse.current.delta.ReadValue();
							if (this.autoaim && this.aimTarget)
							{
								Vector3 a = MonoBehaviourSingleton<MainCamera>.Instance.worldCamera.WorldToViewportPoint(this.aimTarget.Origin) - Vector3.one * 0.5f;
								vector += Vector2.ClampMagnitude(15f * num * a, num * 0.75f);
							}
							if (vector.x != 0f || vector.y != 0f)
							{
								this.myPlayer.Orient(Vector2.Scale(vector, this.sensitivity) * MonoBehaviourSingleton<MainCamera>.Instance.GetZoomFactor);
								this.orientTime = Time.time + 1.5f;
							}
							else if (this.myPlayer.curMountT && Time.time > this.orientTime)
							{
								this.myPlayer.RotationT.rotation = Quaternion.Slerp(this.myPlayer.Rotation, this.myPlayer.curMountT.rotation, Time.deltaTime * Mathf.Min(10f, 2f * (Time.time - this.orientTime)));
							}
						}
					}
					if (Time.time > this.nextInputSend && (this.myPlayer.input != this.lastInput || this.myPlayer.mode != this.lastMode) && this.myPlayer.CanUpdateInputs)
					{
						this.SendToServer(PacketFlags.Unthrottled, SvPacket.UpdateInput, new object[]
						{
							this.myPlayer.input,
							this.myPlayer.mode
						});
						this.lastInput = this.myPlayer.input;
						this.lastMode = this.myPlayer.mode;
						this.nextInputSend = Time.time + 0.1f;
					}
					if (this.fire)
					{
						this.myPlayer.Fire(this.myPlayer.ActiveWeapon.index);
					}
				}
				if (this.GetButton(InputType.ChatMode, false))
				{
					this.SendToServer(PacketFlags.Reliable, SvPacket.ChatMode, Array.Empty<object>());
				}
				else if (this.GetButton(InputType.Screenshot, false))
				{
					MonoBehaviourSingleton<MainCamera>.Instance.Screenshot();
				}
				else if (this.GetButton(InputType.ToggleCursor, false))
				{
					this.ToggleCursor();
				}
				else if (this.GetButton(InputType.CycleCamera, false))
				{
					MonoBehaviourSingleton<MainCamera>.Instance.CycleCamera();
				}
				else if (this.GetButton(InputType.ToggleHUD, false))
				{
					this.ToggleHUD();
				}
				else if (this.GetButton(InputType.ToggleInterface, false))
				{
					this.ToggleInterface();
				}
				else if (this.GetButton(InputType.ChatGlobal, false))
				{
					this.ShowChatMenu(false);
				}
				else if (this.GetButton(InputType.ChatLocal, false))
				{
					this.ShowChatMenu(true);
				}
				else if (this.GetButton(InputType.ChatHistory, false))
				{
					this.ShowChatHistoryMenu();
				}
				else if (this.GetButton(InputType.PlayerList, false))
				{
					this.ShowPlayersMenu();
				}
				if (this.GetButton(InputType.ChatVoice, false))
				{
					if (!this.microphoneClip)
					{
						if (this.recordData == null)
						{
							this.recordRate = this.GetRecordRate();
							this.recordSampleSize = this.recordRate * 960 / 16000;
							this.recordData = new float[this.recordSampleSize];
						}
						this.microphoneClip = Microphone.Start(this.inputDevice, true, 1, this.recordRate);
						this.previousPosition = 0;
						this.recordIndex = 0;
						return;
					}
					if (this.encoder != null)
					{
						int position = Microphone.GetPosition(this.inputDevice);
						if (position < this.previousPosition)
						{
							while (this.recordIndex < this.recordRate)
							{
								this.EncodeSample();
							}
							this.recordIndex = 0;
						}
						this.previousPosition = position;
						while (this.recordIndex + this.recordSampleSize <= position)
						{
							this.EncodeSample();
						}
						return;
					}
				}
				else
				{
					this.CleanupMic();
				}
			}
		}

		// Token: 0x0600094B RID: 2379 RVA: 0x0002C898 File Offset: 0x0002AA98
		public int GetRecordRate()
		{
			int min;
			int num;
			Microphone.GetDeviceCaps(this.inputDevice, out min, out num);
			if (num > 0)
			{
				return Mathf.Clamp(16000, min, num);
			}
			return 16000;
		}

		// Token: 0x0600094C RID: 2380 RVA: 0x0002C8CA File Offset: 0x0002AACA
		public void SendEquipable(int equipableIndex)
		{
			if (this.myPlayer.curEquipable.index == equipableIndex)
			{
				return;
			}
			this.SendToServer(PacketFlags.Reliable, SvPacket.TrySetEquipable, new object[]
			{
				equipableIndex
			});
		}

		// Token: 0x0600094D RID: 2381 RVA: 0x0002C8F8 File Offset: 0x0002AAF8
		private void AppendChatHistory(string input)
		{
			this.chatHistory.Enqueue(input);
			if (this.chatHistory.Count > 100)
			{
				this.chatHistory.Dequeue();
			}
		}

		// Token: 0x0600094E RID: 2382 RVA: 0x0002C924 File Offset: 0x0002AB24
		private string GetChatHistory()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string value in this.chatHistory)
			{
				stringBuilder.AppendLine(value);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x0600094F RID: 2383 RVA: 0x0002C984 File Offset: 0x0002AB84
		private void UpdateSmooth()
		{
			while (Buffers.reader.Position < this.netEvent.Packet.Length)
			{
				ShPhysical shPhysical;
				if (EntityCollections.TryFindByID<ShPhysical>(Buffers.reader.ReadInt32(), out shPhysical) && shPhysical.isActiveAndEnabled && !shPhysical.clPhysical.ClientsideController)
				{
					shPhysical.clPhysical.UpdateLatestState(Buffers.reader.ReadVector3(), Buffers.reader.ReadQuaternion());
				}
				else
				{
					Buffers.reader.BaseStream.Seek(28L, SeekOrigin.Current);
				}
			}
		}

		// Token: 0x06000950 RID: 2384 RVA: 0x0002CA10 File Offset: 0x0002AC10
		private void SetColor()
		{
			ShTrigger shTrigger;
			if (EntityCollections.TryFindByID<ShTrigger>(Buffers.reader.ReadInt32(), out shTrigger))
			{
				shTrigger.clTrigger.SetColor(Buffers.reader.ReadColor());
			}
		}

		// Token: 0x06000951 RID: 2385 RVA: 0x0002CA48 File Offset: 0x0002AC48
		private void SetScale()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity))
			{
				shEntity.mainT.localScale = Buffers.reader.ReadVector3();
			}
		}

		// Token: 0x06000952 RID: 2386 RVA: 0x0002CA80 File Offset: 0x0002AC80
		private void TransferInfo()
		{
			int num = Buffers.reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				MonoBehaviourSingleton<SceneManager>.Instance.assetFiles[Buffers.reader.ReadString()] = default(AssetData);
			}
			MonoBehaviourSingleton<SceneManager>.Instance.mapHash = Buffers.reader.ReadString();
			MonoBehaviourSingleton<SceneManager>.Instance.SetMinPlaces(Buffers.reader.ReadInt32());
			base.StartCoroutine(this.CacheCheck());
		}

		// Token: 0x06000953 RID: 2387 RVA: 0x0002CAFB File Offset: 0x0002ACFB
		public IEnumerator CacheCheck()
		{
			int count = MonoBehaviourSingleton<SceneManager>.Instance.assetFiles.Count;
			MonoBehaviourSingleton<SceneManager>.Instance.assetCacheExists = new byte[count];
			int index = 0;
			foreach (string text in MonoBehaviourSingleton<SceneManager>.Instance.assetFiles.Keys)
			{
				byte[] array;
				if (MonoBehaviourSingleton<SceneManager>.Instance.RegisterAssetBundle(Paths.absoluteCachePath + text, true, out array) == text)
				{
					MonoBehaviourSingleton<SceneManager>.Instance.assetCacheExists[index] = 1;
					int num = index;
					index = num + 1;
					this.ShowGameMessage(string.Format("Loading asset {0}/{1} from cache", index, count));
					yield return null;
				}
				else
				{
					MonoBehaviourSingleton<SceneManager>.Instance.assetCacheExists[index] = 0;
					int num = index;
					index = num + 1;
				}
			}
			Dictionary<string, AssetData>.KeyCollection.Enumerator enumerator = default(Dictionary<string, AssetData>.KeyCollection.Enumerator);
			try
			{
				MonoBehaviourSingleton<SceneManager>.Instance.mapData = File.ReadAllBytes(MonoBehaviourSingleton<SceneManager>.Instance.GetMapCachePath);
				MonoBehaviourSingleton<SceneManager>.Instance.mapCacheExists = true;
			}
			catch (Exception)
			{
				MonoBehaviourSingleton<SceneManager>.Instance.mapCacheExists = false;
			}
			this.SendToServer(PacketFlags.Reliable, SvPacket.Cache, new object[]
			{
				MonoBehaviourSingleton<SceneManager>.Instance.assetCacheExists,
				MonoBehaviourSingleton<SceneManager>.Instance.mapCacheExists
			});
			yield break;
			yield break;
		}

		// Token: 0x06000954 RID: 2388 RVA: 0x0002CB0C File Offset: 0x0002AD0C
		private void ProcessJobs()
		{
			this.jobs = JsonConvert.DeserializeObject<List<JobInfoShared>>(Buffers.reader.ReadString());
			for (int i = 0; i < this.jobs.Count; i++)
			{
				this.jobs[i].jobIndex = i;
			}
		}

		// Token: 0x06000955 RID: 2389 RVA: 0x0002CB58 File Offset: 0x0002AD58
		public void AddEntity()
		{
			ShEntity shEntity;
			if (MonoBehaviourSingleton<SceneManager>.Instance.TryGetEntity(Buffers.reader.ReadInt32(), out shEntity))
			{
				ShEntity shEntity2 = Object.Instantiate<ShEntity>(shEntity, Vector3.zero, Quaternion.identity, null);
				shEntity2.name = shEntity.name;
				shEntity2.PreInitialize(Buffers.reader.ReadInt32());
				shEntity2.clEntity.ReadInitData();
				shEntity2.Initialize();
				return;
			}
			Util.Log("Server using an unsupported AssetBundle", LogLevel.Error);
			MonoBehaviourSingleton<SceneManager>.Instance.ReloadGame(1U);
		}

		// Token: 0x06000956 RID: 2390 RVA: 0x0002CBD4 File Offset: 0x0002ADD4
		private void AddEntityArray()
		{
			while (Buffers.reader.Position < this.netEvent.Packet.Length)
			{
				this.AddEntity();
			}
		}

		// Token: 0x06000957 RID: 2391 RVA: 0x0002CC08 File Offset: 0x0002AE08
		private void AssetData()
		{
			byte[] array = Buffers.reader.ReadBytesAndSize();
			if (this.assetBuffer == null)
			{
				this.assetBuffer = new byte[MonoBehaviourSingleton<SceneManager>.Instance.loadTotal];
			}
			Array.Copy(array, 0, this.assetBuffer, MonoBehaviourSingleton<SceneManager>.Instance.loadProgress, array.Length);
			MonoBehaviourSingleton<SceneManager>.Instance.IncrementTransferProgress(array.Length);
			if (MonoBehaviourSingleton<SceneManager>.Instance.loadProgress >= MonoBehaviourSingleton<SceneManager>.Instance.loadTotal)
			{
				string text = null;
				try
				{
					text = MonoBehaviourSingleton<SceneManager>.Instance.RegisterAssetBundle(this.assetBuffer, true);
				}
				catch (Exception arg)
				{
					string message = string.Format("AssetBundle Error: {0}", arg);
					this.ShowGameMessage(message);
					Util.Log(message, LogLevel.Error);
				}
				if (text != null)
				{
					File.WriteAllBytes(Paths.absoluteCachePath + text, this.assetBuffer);
				}
				else
				{
					MonoBehaviourSingleton<SceneManager>.Instance.ReloadGame(1U);
				}
				this.assetBuffer = null;
			}
		}

		// Token: 0x06000958 RID: 2392 RVA: 0x0002CCEC File Offset: 0x0002AEEC
		private void MapData()
		{
			byte[] array = Buffers.reader.ReadBytesAndSize();
			if (MonoBehaviourSingleton<SceneManager>.Instance.mapData == null)
			{
				MonoBehaviourSingleton<SceneManager>.Instance.mapData = new byte[MonoBehaviourSingleton<SceneManager>.Instance.loadTotal];
			}
			Array.Copy(array, 0, MonoBehaviourSingleton<SceneManager>.Instance.mapData, MonoBehaviourSingleton<SceneManager>.Instance.loadProgress, array.Length);
			MonoBehaviourSingleton<SceneManager>.Instance.IncrementTransferProgress(array.Length);
		}

		// Token: 0x06000959 RID: 2393 RVA: 0x0002CD54 File Offset: 0x0002AF54
		private void EntityData()
		{
			byte[] array = Buffers.reader.ReadBytesAndSize();
			if (MonoBehaviourSingleton<SceneManager>.Instance.entityData == null)
			{
				MonoBehaviourSingleton<SceneManager>.Instance.entityData = new byte[MonoBehaviourSingleton<SceneManager>.Instance.loadTotal];
			}
			Array.Copy(array, 0, MonoBehaviourSingleton<SceneManager>.Instance.entityData, MonoBehaviourSingleton<SceneManager>.Instance.loadProgress, array.Length);
			MonoBehaviourSingleton<SceneManager>.Instance.IncrementTransferProgress(array.Length);
		}

		// Token: 0x0600095A RID: 2394 RVA: 0x0002CDBC File Offset: 0x0002AFBC
		private IEnumerator ReceivedLevel()
		{
			ClManager.<ReceivedLevel>d__213 <ReceivedLevel>d__ = new ClManager.<ReceivedLevel>d__213(0);
			<ReceivedLevel>d__.<>4__this = this;
			return <ReceivedLevel>d__;
		}

		// Token: 0x0600095B RID: 2395 RVA: 0x0002CDCC File Offset: 0x0002AFCC
		private void UpdateMode()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.mode = Buffers.reader.ReadByte();
			}
		}

		// Token: 0x0600095C RID: 2396 RVA: 0x0002CDFC File Offset: 0x0002AFFC
		private void Disarm()
		{
			ShExplosion shExplosion;
			if (EntityCollections.TryFindByID<ShExplosion>(Buffers.reader.ReadInt32(), out shExplosion))
			{
				shExplosion.Disarm();
			}
		}

		// Token: 0x0600095D RID: 2397 RVA: 0x0002CE24 File Offset: 0x0002B024
		private void DestroyEntity()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity))
			{
				shEntity.Destroy();
			}
		}

		// Token: 0x0600095E RID: 2398 RVA: 0x0002CE4C File Offset: 0x0002B04C
		private void ActivateEntity()
		{
			Transform transform = this.activateParent;
			Place place = MonoBehaviourSingleton<SceneManager>.Instance.places[Buffers.reader.ReadInt32()];
			this.activateParent = place.mTransform;
			this.activateParent.gameObject.SetActive(true);
			if (transform != this.activateParent)
			{
				this.bakedMap.texture = this.mapTextures[place.parentPlace.Index];
				if (transform != null)
				{
					transform.gameObject.SetActive(false);
				}
			}
			while (Buffers.reader.Position < this.netEvent.Packet.Length)
			{
				int id = Buffers.reader.ReadInt32();
				ShEntity shEntity;
				if (!EntityCollections.TryFindByID(id, out shEntity))
				{
					Util.Log("Activate entity not found: " + id.ToString(), LogLevel.Error);
					return;
				}
				shEntity.clEntity.ReadActivateData();
			}
		}

		// Token: 0x0600095F RID: 2399 RVA: 0x0002CF34 File Offset: 0x0002B134
		private void DeactivateEntity()
		{
			while (Buffers.reader.Position < this.netEvent.Packet.Length)
			{
				int id = Buffers.reader.ReadInt32();
				ShEntity shEntity;
				if (EntityCollections.TryFindByID(id, out shEntity))
				{
					shEntity.DeactivateEntity();
				}
				else
				{
					Util.Log("Deactivate entity not found: " + id.ToString(), LogLevel.Error);
				}
			}
		}

		// Token: 0x06000960 RID: 2400 RVA: 0x0002CF98 File Offset: 0x0002B198
		private void Relocate()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity))
			{
				shEntity.clEntity.Relocate(Buffers.reader.ReadVector3(), Buffers.reader.ReadQuaternion(), true);
			}
		}

		// Token: 0x06000961 RID: 2401 RVA: 0x0002CFD8 File Offset: 0x0002B1D8
		private void Jump()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.clPlayer.Jump();
			}
		}

		// Token: 0x06000962 RID: 2402 RVA: 0x0002D004 File Offset: 0x0002B204
		private void Stance()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.clPlayer.SetStance((StanceIndex)Buffers.reader.ReadByte());
			}
		}

		// Token: 0x06000963 RID: 2403 RVA: 0x0002D039 File Offset: 0x0002B239
		private void TimeInfo()
		{
			MonoBehaviourSingleton<SceneManager>.Instance.dayLength = Buffers.reader.ReadSingle();
			this.manager.SetTimeScale(Buffers.reader.ReadSingle());
		}

		// Token: 0x06000964 RID: 2404 RVA: 0x0002D064 File Offset: 0x0002B264
		private void VoteUpdate()
		{
			if (this.votePanel)
			{
				this.votePanel.UpdateResult(Buffers.reader.ReadInt32(), Buffers.reader.ReadInt32());
			}
		}

		// Token: 0x06000965 RID: 2405 RVA: 0x0002D094 File Offset: 0x0002B294
		private void Restore()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity))
			{
				shEntity.Restore(Buffers.reader.ReadVector3(), Buffers.reader.ReadQuaternion(), MonoBehaviourSingleton<SceneManager>.Instance.places[Buffers.reader.ReadInt32()].mTransform);
			}
		}

		// Token: 0x06000966 RID: 2406 RVA: 0x0002D0EC File Offset: 0x0002B2EC
		private void Stats()
		{
			for (int i = 0; i < this.myPlayer.stats.Length; i++)
			{
				this.hud.healthVisual.UpdateStat(i, Buffers.reader.ReadSingle());
			}
			this.myPlayer.UpdateInjuries();
			this.myPlayer.CorrectMoveMode();
		}

		// Token: 0x06000967 RID: 2407 RVA: 0x0002D144 File Offset: 0x0002B344
		private void DestroyEffect()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity))
			{
				shEntity.clEntity.ClDestroyEffect(Buffers.reader.ReadInt32());
			}
		}

		// Token: 0x06000968 RID: 2408 RVA: 0x0002D17C File Offset: 0x0002B37C
		private void AltFire()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.curEquipable.curUnderbarrel.ToggleSetting();
			}
		}

		// Token: 0x06000969 RID: 2409 RVA: 0x0002D1AC File Offset: 0x0002B3AC
		private void ShowHealth()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.clPlayer.DeserializeHealth(Buffers.reader.ReadBytesAndSize());
				this.ShowHealthMenu(shPlayer);
			}
		}

		// Token: 0x0600096A RID: 2410 RVA: 0x0002D1E8 File Offset: 0x0002B3E8
		private void Mount()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.clPlayer.Mount(Buffers.reader.ReadInt32(), Buffers.reader.ReadByte(), Buffers.reader.ReadInt32());
			}
		}

		// Token: 0x0600096B RID: 2411 RVA: 0x0002D234 File Offset: 0x0002B434
		private void Dismount()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.clPlayer.Dismount();
			}
		}

		// Token: 0x0600096C RID: 2412 RVA: 0x0002D260 File Offset: 0x0002B460
		private void Fire()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.Fire(Buffers.reader.ReadInt32());
			}
		}

		// Token: 0x0600096D RID: 2413 RVA: 0x0002D290 File Offset: 0x0002B490
		private void SetJob()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.clPlayer.SetJob(Buffers.reader.ReadInt32());
			}
		}

		// Token: 0x0600096E RID: 2414 RVA: 0x0002D2C8 File Offset: 0x0002B4C8
		private void SetEquipable()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.clPlayer.SetEquipable(Buffers.reader.ReadInt32());
			}
		}

		// Token: 0x0600096F RID: 2415 RVA: 0x0002D300 File Offset: 0x0002B500
		private void SetAttachment()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.SetAttachment(Buffers.reader.ReadInt32(), UnderbarrelSetting.Default);
			}
		}

		// Token: 0x06000970 RID: 2416 RVA: 0x0002D334 File Offset: 0x0002B534
		private void SetWearable()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.SetWearable(Buffers.reader.ReadInt32());
			}
		}

		// Token: 0x06000971 RID: 2417 RVA: 0x0002D368 File Offset: 0x0002B568
		private void Spawn()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity))
			{
				shEntity.Spawn(Buffers.reader.ReadVector3(), Buffers.reader.ReadQuaternion(), MonoBehaviourSingleton<SceneManager>.Instance.places[Buffers.reader.ReadInt32()].mTransform);
			}
		}

		// Token: 0x06000972 RID: 2418 RVA: 0x0002D3C0 File Offset: 0x0002B5C0
		private void UpdateHealth()
		{
			ShDestroyable shDestroyable;
			if (EntityCollections.TryFindByID<ShDestroyable>(Buffers.reader.ReadInt32(), out shDestroyable))
			{
				shDestroyable.clDestroyable.UpdateHealth(Buffers.reader.ReadSingle(), false);
			}
		}

		// Token: 0x06000973 RID: 2419 RVA: 0x0002D3F8 File Offset: 0x0002B5F8
		private void TransportState()
		{
			ShTransport shTransport;
			if (EntityCollections.TryFindByID<ShTransport>(Buffers.reader.ReadInt32(), out shTransport))
			{
				shTransport.clTransport.ClTransportState(Buffers.reader.ReadByte());
			}
		}

		// Token: 0x06000974 RID: 2420 RVA: 0x0002D430 File Offset: 0x0002B630
		private void TransportOwner()
		{
			ShTransport shTransport;
			if (EntityCollections.TryFindByID<ShTransport>(Buffers.reader.ReadInt32(), out shTransport))
			{
				shTransport.owner = this.myPlayer;
				this.myPlayer.ownedTransports.Add(shTransport);
			}
		}

		// Token: 0x06000975 RID: 2421 RVA: 0x0002D470 File Offset: 0x0002B670
		private void ChatGlobal(ChatMode mode = ChatMode.Public)
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				string text = shPlayer.displayName + ": " + Buffers.reader.ReadString();
				if (mode != ChatMode.Public)
				{
					text = text.PrefixChatString(mode);
				}
				this.ShowMessage(text);
			}
		}

		// Token: 0x06000976 RID: 2422 RVA: 0x0002D4C0 File Offset: 0x0002B6C0
		private void ChatLocal()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				string text = Buffers.reader.ReadString();
				if (shPlayer == this.myPlayer)
				{
					this.ShowGameMessage(text);
					return;
				}
				Util.identityBuffer.SetMessage(shPlayer.clPlayer.GetIdentity(), text);
			}
		}

		// Token: 0x06000977 RID: 2423 RVA: 0x0002D518 File Offset: 0x0002B718
		private void Reload()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.curEquipable.Reload();
			}
		}

		// Token: 0x06000978 RID: 2424 RVA: 0x0002D544 File Offset: 0x0002B744
		private void Load()
		{
			ShGun shGun = this.myPlayer.curEquipable as ShGun;
			if (shGun != null)
			{
				shGun.Load(Buffers.reader.ReadInt32());
			}
		}

		// Token: 0x06000979 RID: 2425 RVA: 0x0002D575 File Offset: 0x0002B775
		private void ShowTimer(string id, float timeout)
		{
			this.DestroyText(id);
			Object.Instantiate<TextDisplay>(this.textPrefab, this.textDisplaysT, false).Initialize(new object[]
			{
				id,
				timeout
			});
		}

		// Token: 0x0600097A RID: 2426 RVA: 0x0002D5A8 File Offset: 0x0002B7A8
		private void ShowText(string id, float timeout, string text)
		{
			this.DestroyText(id);
			Object.Instantiate<TextDisplay>(this.textPrefab, this.textDisplaysT, false).Initialize(new object[]
			{
				id,
				timeout,
				text
			});
		}

		// Token: 0x0600097B RID: 2427 RVA: 0x0002D5DF File Offset: 0x0002B7DF
		private void StartProgress(string id, float position, float speed)
		{
			this.StopProgress(id);
			Object.Instantiate<ProgressDisplay>(this.progressDisplayPrefab, this.progressBarsT, false).Initialize(new object[]
			{
				id,
				position,
				speed
			});
		}

		// Token: 0x0600097C RID: 2428 RVA: 0x0002D61C File Offset: 0x0002B81C
		private void StopProgress(string id)
		{
			ProgressDisplay progressDisplay;
			if (this.progressDisplays.TryGetValue(id, out progressDisplay))
			{
				progressDisplay.Destroy();
			}
		}

		// Token: 0x0600097D RID: 2429 RVA: 0x0002D640 File Offset: 0x0002B840
		private void SetSiren()
		{
			ShTransport shTransport;
			if (EntityCollections.TryFindByID<ShTransport>(Buffers.reader.ReadInt32(), out shTransport))
			{
				shTransport.SetSiren(Buffers.reader.ReadBoolean());
			}
		}

		// Token: 0x0600097E RID: 2430 RVA: 0x0002D670 File Offset: 0x0002B870
		private void RegisterFail()
		{
			this.ShowGameMessage(Buffers.reader.ReadString());
			RegisterMenu registerMenu = this.CurrentMenu as RegisterMenu;
			if (registerMenu != null)
			{
				registerMenu.Fail();
			}
		}

		// Token: 0x0600097F RID: 2431 RVA: 0x0002D6A4 File Offset: 0x0002B8A4
		private void SetVault()
		{
			ShVault shVault;
			if (EntityCollections.TryFindByID<ShVault>(Buffers.reader.ReadInt32(), out shVault))
			{
				shVault.clVault.SetVault(Buffers.reader.ReadByte(), true);
			}
		}

		// Token: 0x06000980 RID: 2432 RVA: 0x0002D6DC File Offset: 0x0002B8DC
		private void SetTerritory()
		{
			ShTerritory shTerritory;
			if (EntityCollections.TryFindByID<ShTerritory>(Buffers.reader.ReadInt32(), out shTerritory))
			{
				shTerritory.clTerritory.SetTerritory(Buffers.reader.ReadInt32(), Buffers.reader.ReadInt32());
			}
		}

		// Token: 0x06000981 RID: 2433 RVA: 0x0002D71C File Offset: 0x0002B91C
		private void BanState()
		{
			string text = Buffers.reader.ReadString();
			bool flag = Buffers.reader.ReadBoolean();
			if (!flag)
			{
				BanRecordsMenu banRecordsMenu = this.CurrentMenu as BanRecordsMenu;
				if (banRecordsMenu != null)
				{
					banRecordsMenu.ClearButtons(new object[]
					{
						text
					});
				}
			}
			this.ShowGameMessage("IP: " + text + " Ban State: " + flag.ToString());
		}

		// Token: 0x06000982 RID: 2434 RVA: 0x0002D780 File Offset: 0x0002B980
		private void Point()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.pointing = Buffers.reader.ReadBoolean();
			}
		}

		// Token: 0x06000983 RID: 2435 RVA: 0x0002D7B0 File Offset: 0x0002B9B0
		private void Alert()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.GetControlled().clMountable.PlayAlert();
			}
		}

		// Token: 0x06000984 RID: 2436 RVA: 0x0002D7E0 File Offset: 0x0002B9E0
		private void ClConsume()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.Consume(Buffers.reader.ReadInt32());
			}
		}

		// Token: 0x06000985 RID: 2437 RVA: 0x0002D810 File Offset: 0x0002BA10
		public void UpdateAmmo(int delta, int ammoIndex)
		{
			if (ammoIndex == 0 && this.myPlayer.IsMountArmed)
			{
				this.myPlayer.curMount.UpdateSecondaryAmmo(delta, (int)this.myPlayer.seat);
				return;
			}
			if (delta > 0)
			{
				this.myPlayer.TransferItem(1, ammoIndex, delta, false);
				return;
			}
			if (delta < 0)
			{
				this.myPlayer.TransferItem(2, ammoIndex, -delta, false);
			}
		}

		// Token: 0x06000986 RID: 2438 RVA: 0x0002D874 File Offset: 0x0002BA74
		public void StartClient(string hostname, ushort port)
		{
			ClManager.lastHostname = hostname;
			ClManager.lastPort = port;
			this.clientSectorRange = (int)(this.RenderDistance / 32f);
			Action[] array = new Action[180];
			array[0] = delegate()
			{
				MonoBehaviourSingleton<SceneManager>.Instance.referenceTime = Time.time - Buffers.reader.ReadSingle();
			};
			array[1] = new Action(this.AddEntity);
			array[2] = new Action(this.DestroyEntity);
			array[3] = new Action(this.ActivateEntity);
			array[4] = new Action(this.DeactivateEntity);
			array[5] = new Action(this.Relocate);
			array[6] = delegate()
			{
				this.myPlayer.GetControlled().clEntity.Relocate(Buffers.reader.ReadVector3(), Buffers.reader.ReadQuaternion(), false);
			};
			array[7] = new Action(this.SetJob);
			array[8] = delegate()
			{
				this.ShowGameMessage(Buffers.reader.ReadString());
			};
			array[9] = delegate()
			{
				this.myPlayer.TransferItem(Buffers.reader.ReadByte(), Buffers.reader.ReadInt32(), Buffers.reader.ReadInt32(), true);
			};
			array[10] = delegate()
			{
				this.myPlayer.clPlayer.View(Buffers.reader.ReadInt32(), Buffers.reader.ReadBytesAndSize());
			};
			array[11] = delegate()
			{
				this.myPlayer.clPlayer.Shopping(Buffers.reader.ReadInt32(), Buffers.reader.ReadBytesAndSize());
			};
			array[12] = delegate()
			{
				this.myPlayer.clPlayer.ShowTradeInventory(Buffers.reader.ReadInt32());
			};
			array[13] = delegate()
			{
				this.myPlayer.clPlayer.ShowSearchedInventory();
			};
			array[14] = new Action(this.SetEquipable);
			array[15] = new Action(this.SetAttachment);
			array[16] = new Action(this.SetWearable);
			array[17] = delegate()
			{
				this.FinalizeTrade(true);
			};
			array[18] = delegate()
			{
				this.WaitTrade();
			};
			array[19] = delegate()
			{
				this.FinalizeTrade(false);
			};
			array[20] = delegate()
			{
				this.myPlayer.clPlayer.FinishTrade(Buffers.reader.ReadBoolean());
			};
			array[21] = delegate()
			{
				this.myPlayer.clPlayer.UpdateShopValue(Buffers.reader.ReadInt32(), Buffers.reader.ReadInt32());
			};
			array[22] = new Action(this.Mount);
			array[23] = new Action(this.Dismount);
			array[24] = new Action(this.TransportState);
			array[25] = new Action(this.TransportOwner);
			array[26] = new Action(this.UpdateHealth);
			array[27] = new Action(this.ShowHealth);
			array[28] = delegate()
			{
				this.myPlayer.AddInjury((BodyPart)Buffers.reader.ReadByte(), (BodyEffect)Buffers.reader.ReadByte(), Buffers.reader.ReadByte());
			};
			array[29] = delegate()
			{
				this.myPlayer.ClearInjuries();
			};
			array[30] = delegate()
			{
				this.myPlayer.RemoveInjury(Buffers.reader.ReadInt32());
			};
			array[31] = new Action(this.Spawn);
			array[32] = new Action(this.Reload);
			array[33] = new Action(this.Load);
			array[34] = new Action(this.Stats);
			array[35] = new Action(this.ShowRegisterMenu);
			array[36] = new Action(this.RegisterFail);
			array[37] = delegate()
			{
				MonoBehaviourSingleton<SceneManager>.Instance.ShowCursor = Buffers.reader.ReadBoolean();
			};
			array[38] = new Action(this.SetMaxSpeed);
			array[39] = new Action(this.UpdateTextDisplay);
			array[40] = new Action(this.AddVoxels);
			array[41] = new Action(this.RemoveVoxels);
			array[42] = new Action(this.Tow);
			array[43] = new Action(this.SetTerritory);
			array[44] = delegate()
			{
				this.ShowTextMenu(Buffers.reader.ReadSingle(), Buffers.reader.ReadSingle(), Buffers.reader.ReadSingle(), Buffers.reader.ReadSingle(), Buffers.reader.ReadString(), Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[45] = delegate()
			{
				this.ShowOptionMenu(Buffers.reader.ReadSingle(), Buffers.reader.ReadSingle(), Buffers.reader.ReadSingle(), Buffers.reader.ReadSingle(), Buffers.reader.ReadString(), Buffers.reader.ReadString(), Buffers.reader.ReadInt32(), Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[46] = delegate()
			{
				this.ShowInputMenu(Buffers.reader.ReadSingle(), Buffers.reader.ReadSingle(), Buffers.reader.ReadSingle(), Buffers.reader.ReadSingle(), Buffers.reader.ReadString(), Buffers.reader.ReadString(), Buffers.reader.ReadInt32(), Buffers.reader.ReadInt32());
			};
			array[47] = delegate()
			{
				this.ShowTextPanel(Buffers.reader.ReadString(), Buffers.reader.ReadString(), Buffers.reader.ReadString(), Buffers.reader.ReadInt32());
			};
			array[48] = delegate()
			{
				this.DestroyTextPanel(Buffers.reader.ReadString());
			};
			array[49] = delegate()
			{
				this.ShowHackingMenu(Buffers.reader.ReadString(), Buffers.reader.ReadInt32(), Buffers.reader.ReadString(), Buffers.reader.ReadString(), Buffers.reader.ReadSingle());
			};
			array[50] = delegate()
			{
				this.ShowCrackingMenu(Buffers.reader.ReadString(), Buffers.reader.ReadInt32(), Buffers.reader.ReadString(), Buffers.reader.ReadString(), Buffers.reader.ReadSingle());
			};
			array[51] = delegate()
			{
				HackingMenu hackingMenu = this.CurrentMenu as HackingMenu;
				if (hackingMenu != null)
				{
					hackingMenu.RecievedProbe(Buffers.reader.ReadVector2(), Buffers.reader.ReadSingle(), Buffers.reader.ReadBoolean(), Buffers.reader.ReadByte());
				}
			};
			array[52] = delegate()
			{
				HackingMenu hackingMenu = this.CurrentMenu as HackingMenu;
				if (hackingMenu != null)
				{
					hackingMenu.RecievedMark(Buffers.reader.ReadVector2(), Buffers.reader.ReadBytesAndSize());
				}
			};
			array[53] = delegate()
			{
				CrackingMenu crackingMenu = this.CurrentMenu as CrackingMenu;
				if (crackingMenu != null)
				{
					crackingMenu.ReceivedAttempt(Buffers.reader.ReadSingle(), Buffers.reader.ReadSingle());
				}
			};
			array[54] = delegate()
			{
				MinigameMenu minigameMenu = this.CurrentMenu as MinigameMenu;
				if (minigameMenu != null)
				{
					minigameMenu.GameOver(Buffers.reader.ReadBoolean());
				}
			};
			array[55] = delegate()
			{
				this.myPlayer.GetControlled().Force(Buffers.reader.ReadVector3());
			};
			array[56] = new Action(this.Jump);
			array[57] = new Action(this.Stance);
			array[58] = delegate()
			{
				this.myPlayer.DeserializeMyItems(Buffers.reader.ReadBytesAndSize());
			};
			array[59] = delegate()
			{
				this.myPlayer.clPlayer.DeserializeHealth(Buffers.reader.ReadBytesAndSize());
			};
			array[60] = delegate()
			{
				this.ShowVotePanel(Buffers.reader.ReadInt32(), Buffers.reader.ReadInt32());
			};
			array[61] = new Action(this.VoteUpdate);
			array[62] = new Action(this.SetVault);
			array[63] = delegate()
			{
				MonoBehaviourSingleton<SceneManager>.Instance.ClonePlace(MonoBehaviourSingleton<SceneManager>.Instance.places[Buffers.reader.ReadInt32()], Buffers.reader.ReadInt32(), Buffers.reader.ReadInt32());
			};
			array[64] = delegate()
			{
				MonoBehaviourSingleton<SceneManager>.Instance.DestroyPlace(MonoBehaviourSingleton<SceneManager>.Instance.places[Buffers.reader.ReadInt32()]);
			};
			array[65] = delegate()
			{
				this.myPlayer.clPlayer.BuyApartment(Buffers.reader.ReadInt32(), Buffers.reader.ReadInt32());
			};
			array[66] = delegate()
			{
				this.myPlayer.clPlayer.SellApartment(Buffers.reader.ReadInt32());
			};
			array[67] = new Action(this.ClConsume);
			array[68] = delegate()
			{
				this.StartProgress(Buffers.reader.ReadString(), Buffers.reader.ReadSingle(), Buffers.reader.ReadSingle());
			};
			array[69] = delegate()
			{
				this.StopProgress(Buffers.reader.ReadString());
			};
			array[70] = new Action(this.AddEntityArray);
			array[71] = delegate()
			{
				this.UpdateAmmo(Buffers.reader.ReadInt32(), Buffers.reader.ReadInt32());
			};
			array[72] = new Action(this.UpdateMode);
			array[73] = new Action(this.UpdateSmooth);
			array[74] = delegate()
			{
				MonoBehaviourSingleton<SceneManager>.Instance.defaultEnvironment = true;
			};
			array[75] = new Action(MonoBehaviourSingleton<SceneManager>.Instance.CustomEnvironment);
			array[76] = new Action(this.SetColor);
			array[77] = new Action(this.SetScale);
			array[78] = new Action(this.TransferInfo);
			array[79] = new Action(this.ProcessJobs);
			array[80] = new Action(this.AssetData);
			array[81] = new Action(this.MapData);
			array[82] = new Action(this.EntityData);
			array[83] = delegate()
			{
				base.StartCoroutine(this.ReceivedLevel());
			};
			array[84] = new Action(this.SetSiren);
			array[85] = new Action(this.LoadingWindow);
			array[86] = delegate()
			{
				this.ShowPlayerRecordsMenu(Buffers.reader.ReadString());
			};
			array[87] = delegate()
			{
				this.ShowBanRecordsMenu(Buffers.reader.ReadString());
			};
			array[88] = new Action(this.BanState);
			array[89] = new Action(this.Point);
			array[90] = new Action(this.Alert);
			array[91] = new Action(this.Disarm);
			array[92] = delegate()
			{
				MonoBehaviourSingleton<GLDebug>.Instance.DrawLine(Buffers.reader.ReadVector3(), Buffers.reader.ReadVector3(), Buffers.reader.ReadColor(), Buffers.reader.ReadSingle());
			};
			array[93] = delegate()
			{
				this.ShowTimer(Buffers.reader.ReadString(), Buffers.reader.ReadSingle());
			};
			array[94] = delegate()
			{
				this.ShowText(Buffers.reader.ReadString(), Buffers.reader.ReadSingle(), Buffers.reader.ReadString());
			};
			array[95] = delegate()
			{
				this.DestroyText(Buffers.reader.ReadString());
			};
			array[96] = delegate()
			{
				this.permissions = Buffers.reader.ReadBytesAndSize();
			};
			array[97] = delegate()
			{
				this.DestroyMenu(Buffers.reader.ReadString());
			};
			array[98] = new Action(this.TimeInfo);
			array[99] = delegate()
			{
				this.myPlayer.clPlayer.Experience(Buffers.reader.ReadInt32(), Buffers.reader.ReadBoolean());
			};
			array[100] = delegate()
			{
				this.myPlayer.clPlayer.Rank(Buffers.reader.ReadInt32());
			};
			array[101] = new Action(this.Restore);
			array[102] = delegate()
			{
				this.myPlayer.Bind(Buffers.reader.ReadInt32(), Buffers.reader.ReadByte());
			};
			array[103] = delegate()
			{
				this.myPlayer.Unbind(Buffers.reader.ReadByte());
			};
			array[104] = delegate()
			{
				this.myPlayer.BindAttachment(Buffers.reader.ReadInt32(), Buffers.reader.ReadByte());
			};
			array[105] = delegate()
			{
				this.myPlayer.UnbindAttachment(Buffers.reader.ReadInt32(), Buffers.reader.ReadByte());
			};
			array[106] = new Action(this.Fire);
			array[107] = new Action(this.DestroyEffect);
			array[108] = new Action(this.AltFire);
			array[109] = delegate()
			{
				this.ShowAppsMenu(Buffers.reader.ReadInt32(), Buffers.reader.ReadString());
			};
			array[110] = delegate()
			{
				this.ShowAppContactsMenu(Buffers.reader.ReadInt32(), Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[111] = delegate()
			{
				this.ShowAppBlockedMenu(Buffers.reader.ReadInt32(), Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[112] = delegate()
			{
				this.ShowAppCallsMenu(Buffers.reader.ReadInt32(), Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[113] = delegate()
			{
				this.ShowAppInboxMenu(Buffers.reader.ReadInt32(), Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[114] = delegate()
			{
				this.ShowAppServicesMenu(Buffers.reader.ReadInt32(), Buffers.reader.ReadString());
			};
			array[115] = delegate()
			{
				this.ShowAppDepositMenu(Buffers.reader.ReadInt32(), Buffers.reader.ReadString(), Buffers.reader.ReadInt32(), Buffers.reader.ReadString());
			};
			array[116] = delegate()
			{
				this.ShowAppWithdrawMenu(Buffers.reader.ReadInt32(), Buffers.reader.ReadString(), Buffers.reader.ReadInt32(), Buffers.reader.ReadString());
			};
			array[117] = delegate()
			{
				this.ShowAppRadioMenu(Buffers.reader.ReadInt32(), Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[118] = delegate()
			{
				this.ShowAppMessageMenu(Buffers.reader.ReadInt32(), Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[119] = delegate()
			{
				this.AppAddMessage(Buffers.reader.ReadString(), Buffers.reader.ReadInt32(), Buffers.reader.ReadInt32(), Buffers.reader.ReadString());
			};
			array[120] = delegate()
			{
				this.ShowCallPanel(Buffers.reader.ReadInt32(), Buffers.reader.ReadBoolean());
			};
			array[121] = delegate()
			{
				this.callPanel.Accepted();
			};
			array[122] = delegate()
			{
				this.callPanel.Destroy();
			};
			array[123] = new Action(this.SetChatChannel);
			array[124] = new Action(this.SetChatMode);
			array[125] = delegate()
			{
				this.ChatGlobal(ChatMode.Public);
			};
			array[126] = delegate()
			{
				this.ChatGlobal(ChatMode.Job);
			};
			array[127] = delegate()
			{
				this.ChatGlobal(ChatMode.Channel);
			};
			array[128] = new Action(this.ChatLocal);
			array[129] = delegate()
			{
				this.ChatVoice(ChatMode.Public);
			};
			array[130] = delegate()
			{
				this.ChatVoice(ChatMode.Channel);
			};
			array[131] = delegate()
			{
				this.ChatVoice(ChatMode.Job);
			};
			array[132] = delegate()
			{
				this.ChatVoice(ChatMode.Channel);
			};
			array[133] = delegate()
			{
				this.hud.UpdateMessages(Buffers.reader.ReadInt32(), true);
			};
			array[134] = new Action(this.DisplayName);
			array[135] = delegate()
			{
				this.myPlayer.clPlayer.Spectate(Buffers.reader.ReadInt32());
			};
			array[136] = delegate()
			{
				this.ShowOpenURLMenu(Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[137] = new Action(this.AnimatorEnabled);
			array[138] = new Action(this.AnimatorFloat);
			array[139] = new Action(this.AnimatorInt);
			array[140] = new Action(this.AnimatorBool);
			array[141] = new Action(this.AnimatorTrigger);
			array[142] = new Action(this.AnimatorState);
			array[143] = new Action(this.VideoPlay);
			array[144] = new Action(this.VideoStop);
			array[145] = new Action(this.AddDynamicAction);
			array[146] = new Action(this.RemoveDynamicAction);
			array[147] = new Action(this.AddSelfAction);
			array[148] = new Action(this.RemoveSelfAction);
			array[149] = new Action(this.AddTypeAction);
			array[150] = new Action(this.RemoveTypeAction);
			array[151] = new Action(this.AddInventoryAction);
			array[152] = new Action(this.RemoveInventoryAction);
			array[153] = new Action(this.TargetLocked);
			array[154] = new Action(this.ToggleMountWeapon);
			array[155] = delegate()
			{
				base.StartCoroutine(this.Connect());
			};
			array[156] = delegate()
			{
				this.Explosion(Buffers.reader.ReadVector3(), Buffers.reader.ReadSingle(), Buffers.reader.ReadSingle());
			};
			array[157] = delegate()
			{
				this.VisualTreeAssetClone(Buffers.reader.ReadString(), Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[158] = delegate()
			{
				this.VisualElementRemove(Buffers.reader.ReadString());
			};
			array[159] = delegate()
			{
				this.VisualElementOpacity(Buffers.reader.ReadString(), Buffers.reader.ReadSingle());
			};
			array[160] = delegate()
			{
				this.VisualElementDisplay(Buffers.reader.ReadString(), Buffers.reader.ReadBoolean());
			};
			array[161] = delegate()
			{
				this.VisualElementVisibility(Buffers.reader.ReadString(), Buffers.reader.ReadBoolean());
			};
			array[162] = delegate()
			{
				this.VisualElementOverflow(Buffers.reader.ReadString(), Buffers.reader.ReadBoolean());
			};
			array[163] = delegate()
			{
				this.AddButtonClickedEvent(Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[164] = delegate()
			{
				this.GetTextFieldText(Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[165] = delegate()
			{
				this.SetTextElementText(Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[166] = delegate()
			{
				this.GetSliderValue(Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[167] = delegate()
			{
				this.SetSliderValue(Buffers.reader.ReadString(), Buffers.reader.ReadSingle());
			};
			array[168] = delegate()
			{
				this.SetProgressBarValue(Buffers.reader.ReadString(), Buffers.reader.ReadSingle());
			};
			array[169] = delegate()
			{
				this.GetToggleValue(Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[170] = delegate()
			{
				this.SetToggleValue(Buffers.reader.ReadString(), Buffers.reader.ReadBoolean());
			};
			array[171] = delegate()
			{
				this.GetRadioButtonGroupValue(Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[172] = delegate()
			{
				this.SetRadioButtonGroupValue(Buffers.reader.ReadString(), Buffers.reader.ReadInt32());
			};
			array[173] = delegate()
			{
				this.SetRadioButtonGroupChoices(Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[174] = delegate()
			{
				this.GetDropdownFieldValue(Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[175] = delegate()
			{
				this.SetDropdownFieldValue(Buffers.reader.ReadString(), Buffers.reader.ReadInt32());
			};
			array[176] = delegate()
			{
				this.SetDropdownFieldChoices(Buffers.reader.ReadString(), Buffers.reader.ReadString());
			};
			array[177] = delegate()
			{
				this.VisualElementCursorVisibility(Buffers.reader.ReadString());
			};
			array[178] = delegate()
			{
				ShPlayer shPlayer;
				if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
				{
					shPlayer.clPlayer.DeserializeWearables(Buffers.reader.ReadBytesAndSize());
				}
			};
			array[179] = delegate()
			{
				ShPlayer shPlayer;
				if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
				{
					shPlayer.clPlayer.DeserializeAttachments(Buffers.reader.ReadBytesAndSize());
				}
			};
			this.handler = array;
			try
			{
				this.encoder = new OpusEncoder(16000);
			}
			catch (Exception ex)
			{
				string str = "OPUS Recording library error: ";
				Exception ex2 = ex;
				Util.Log(str + ((ex2 != null) ? ex2.ToString() : null), LogLevel.Error);
			}
			Library.Initialize();
			MonoBehaviourSingleton<SceneManager>.Instance.ShowLoadingWindow();
			this.ConnectHost(2);
		}

		// Token: 0x06000987 RID: 2439 RVA: 0x0002E578 File Offset: 0x0002C778
		public void WaitTrade()
		{
			TradeMenu tradeMenu = this.CurrentMenu as TradeMenu;
			if (tradeMenu != null)
			{
				tradeMenu.DisableButton();
			}
		}

		// Token: 0x06000988 RID: 2440 RVA: 0x0002E59C File Offset: 0x0002C79C
		public void FinalizeTrade(bool lockedTrade)
		{
			TradeMenu tradeMenu = this.CurrentMenu as TradeMenu;
			if (tradeMenu != null)
			{
				tradeMenu.EnableButton(lockedTrade);
			}
		}

		// Token: 0x06000989 RID: 2441 RVA: 0x0002E5C0 File Offset: 0x0002C7C0
		private void ConnectHost(int timeouts)
		{
			this.manager.host = this.manager.StartHost(1, 0, null);
			if (this.manager.Connect(this.manager.host, ClManager.lastHostname, ClManager.lastPort, out this.connection))
			{
				base.StartCoroutine(this.ClientLoop(timeouts));
				return;
			}
			Util.Log("Client host could not connect to: " + ClManager.lastHostname, LogLevel.Error);
			MonoBehaviourSingleton<SceneManager>.Instance.ReloadGame(2U);
		}

		// Token: 0x0600098A RID: 2442 RVA: 0x0002E63D File Offset: 0x0002C83D
		private IEnumerator ClientLoop(int timeoutsLeft)
		{
			uint data = 0U;
			bool reconnect = false;
			while (this.manager.host != null && this.manager.host.Service(0, out this.netEvent) >= 0)
			{
				switch (this.netEvent.Type)
				{
				case ENet.EventType.Connect:
					timeoutsLeft = -1;
					this.manager.ConfigurePeer(this.netEvent.Peer);
					this.SendToServer(PacketFlags.Reliable, SvPacket.Ready, new object[]
					{
						(byte)Application.platform
					});
					break;
				case ENet.EventType.Disconnect:
					data = this.netEvent.Data;
					this.manager.HostCleanup();
					break;
				case ENet.EventType.Receive:
				{
					this.netEvent.Packet.CopyTo(Buffers.readBuffer);
					Buffers.reader.SeekZero();
					ClPacket clPacket = Buffers.reader.ReadClPacket();
					try
					{
						this.handler[(int)clPacket]();
					}
					catch (Exception arg)
					{
						Util.Log(string.Format("Error on packet {0}: {1}", clPacket, arg), LogLevel.Error);
					}
					this.netEvent.Packet.Dispose();
					break;
				}
				case ENet.EventType.Timeout:
				{
					this.ShowGameMessage(string.Format("Timeout: {0} retries left", timeoutsLeft));
					int num = timeoutsLeft - 1;
					timeoutsLeft = num;
					if (num >= 0)
					{
						reconnect = true;
					}
					data = 4U;
					this.manager.HostCleanup();
					break;
				}
				default:
					yield return null;
					break;
				}
			}
			if (reconnect)
			{
				this.ShowGameMessage(string.Format("Reconnecting: {0}:{1}", ClManager.lastHostname, ClManager.lastPort));
				this.ConnectHost(timeoutsLeft);
			}
			else
			{
				MonoBehaviourSingleton<SceneManager>.Instance.ReloadGame(data);
			}
			yield break;
		}

		// Token: 0x0600098B RID: 2443 RVA: 0x0002E653 File Offset: 0x0002C853
		private void LoadingWindow()
		{
			this.DestroyMenu("Default");
			MonoBehaviourSingleton<SceneManager>.Instance.ResetLoadingWindow(Buffers.reader.ReadInt32());
		}

		// Token: 0x0600098C RID: 2444 RVA: 0x0002E674 File Offset: 0x0002C874
		public void SendToServer(PacketFlags channel, SvPacket packet, params object[] args)
		{
			if (this.manager.host == null)
			{
				return;
			}
			Buffers.writer.SeekZero();
			Buffers.writer.Write((byte)packet);
			Buffers.WriteObject(args);
			Packet packet2 = default(Packet);
			packet2.Create(Buffers.writeBuffer, Buffers.writer.Position(), channel);
			this.connection.Send(ref packet2);
		}

		// Token: 0x0600098D RID: 2445 RVA: 0x0002E6D6 File Offset: 0x0002C8D6
		public IEnumerator ConnectOfflineModeDelay(string map, Dictionary<string, bool> pluginSettings)
		{
			if (File.Exists(Paths.serverStartedFile))
			{
				File.Delete(Paths.serverStartedFile);
			}
			using (Process process = new Process())
			{
				process.StartInfo.FileName = Paths.AbsolutePath("BrokeProtocol.exe");
				process.StartInfo.Arguments = "-batchmode -singleplayer -map \"" + map + "\" -logfile \"Server.log\"";
				IEnumerable<string> enumerable = from x in pluginSettings
				where !x.Value
				select x.Key;
				if (enumerable.Count<string>() > 0)
				{
					string str = string.Join<string>(':', enumerable);
					ProcessStartInfo startInfo = process.StartInfo;
					startInfo.Arguments = startInfo.Arguments + " -ignorePlugins \"" + str + "\"";
				}
				process.StartInfo.WindowStyle = (this.offlineConsole ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden);
				if (!process.Start())
				{
					MonoBehaviourSingleton<SceneManager>.Instance.ReloadGame(2U);
					yield break;
				}
				process.PriorityClass = ProcessPriorityClass.High;
			}
			WaitForSeconds delay = new WaitForSeconds(0.1f);
			float timeout = Time.time + 120f;
			for (;;)
			{
				yield return delay;
				if (Time.time >= timeout)
				{
					break;
				}
				if (File.Exists(Paths.serverStartedFile))
				{
					goto Block_4;
				}
			}
			MonoBehaviourSingleton<SceneManager>.Instance.ReloadGame(4U);
			yield break;
			Block_4:
			this.manager.InitializeClient("127.0.0.1", 5557);
			yield break;
		}

		// Token: 0x0600098E RID: 2446 RVA: 0x0002E6F3 File Offset: 0x0002C8F3
		public void ToggleHUD()
		{
			this.hud.Visible = (!this.hud.Visible && this.AllowHUD);
		}

		// Token: 0x0600098F RID: 2447 RVA: 0x0002E716 File Offset: 0x0002C916
		public void ToggleInterface()
		{
			this.canvas.enabled = !this.canvas.enabled;
			MonoBehaviourSingleton<SceneManager>.Instance.uiDocument.rootVisualElement.visible = this.canvas.enabled;
		}

		// Token: 0x06000990 RID: 2448 RVA: 0x0002E750 File Offset: 0x0002C950
		public void ShowMainMenu()
		{
			this.ShowMenu(this.mainMenu, Array.Empty<object>());
		}

		// Token: 0x06000991 RID: 2449 RVA: 0x0002E764 File Offset: 0x0002C964
		protected void Start()
		{
			Util.tracerBuffer = new TracerBuffer(this.effectPrefabs[28], 24);
			string text;
			if (this.startAsServer || Util.GetCommandArgument("-batchmode", out text))
			{
				this.manager.InitializeServer();
				return;
			}
			Object.Instantiate<World>(this.worldPrefab, this.manager.menuObjects.transform);
			this.chatChannelLabel = MonoBehaviourSingleton<SceneManager>.Instance.uiDocument.rootVisualElement.Q("ChatChannel", null);
			this.chatModeLabel = MonoBehaviourSingleton<SceneManager>.Instance.uiDocument.rootVisualElement.Q("ChatMode", null);
			this.ReadSettings();
			int i;
			if (PlayerPrefs.HasKey("RenderQuality"))
			{
				i = PlayerPrefs.GetInt("RenderQuality");
			}
			else
			{
				int num;
				switch (Graphics.activeTier)
				{
				case GraphicsTier.Tier1:
					num = 1;
					break;
				case GraphicsTier.Tier2:
					num = 2;
					break;
				case GraphicsTier.Tier3:
					num = 3;
					break;
				default:
					num = 1;
					break;
				}
				i = num;
			}
			this.RenderQuality = i;
			this.RenderDistance = (PlayerPrefs.HasKey("RenderDistance") ? PlayerPrefs.GetFloat("RenderDistance") : 256f);
			this.profileURL = (PlayerPrefs.HasKey("ProfileURL") ? PlayerPrefs.GetString("ProfileURL") : string.Empty);
			this.LimitFPS = (PlayerPrefs.HasKey("LimitFPS") ? PlayerPrefs.GetInt("LimitFPS") : 72);
			this.sensitivity.x = (PlayerPrefs.HasKey("SensitivityX") ? PlayerPrefs.GetFloat("SensitivityX") : 0.15f);
			this.sensitivity.y = (PlayerPrefs.HasKey("SensitivityY") ? PlayerPrefs.GetFloat("SensitivityY") : 0.15f);
			this.LanguageIndex = (PlayerPrefs.HasKey("Language") ? Math.Clamp(PlayerPrefs.GetInt("Language"), 0, Util.languages.Length - 1) : 0);
			this.MainVolume = (PlayerPrefs.HasKey("MainVolume") ? PlayerPrefs.GetFloat("MainVolume") : 0.5f);
			this.MusicVolume = (PlayerPrefs.HasKey("MusicVolume") ? PlayerPrefs.GetFloat("MusicVolume") : 0.25f);
			this.VoiceVolume = (PlayerPrefs.HasKey("VoiceVolume") ? PlayerPrefs.GetFloat("VoiceVolume") : 1f);
			this.customDownloads = (!PlayerPrefs.HasKey("CustomDownloads") || Convert.ToBoolean(PlayerPrefs.GetInt("CustomDownloads")));
			this.offlineConsole = (!PlayerPrefs.HasKey("OfflineConsole") || Convert.ToBoolean(PlayerPrefs.GetInt("OfflineConsole")));
			this.autoaim = (PlayerPrefs.HasKey("Autoaim") && Convert.ToBoolean(PlayerPrefs.GetInt("Autoaim")));
			this.inputDevice = (PlayerPrefs.HasKey("InputDevice") ? PlayerPrefs.GetString("InputDevice") : null);
			this.buttonInput = new InputButton[]
			{
				new InputButton(InputType.Fire, "Fire", true, 1, true),
				new InputButton(InputType.FireStatic, "Fire 2", true, 2, true),
				new InputButton(InputType.Zoom, "Zoom", true, 3, true),
				new InputButton(InputType.Use1, "Use 1", false, 11, true),
				new InputButton(InputType.Use2, "Use 2", false, -1, false),
				new InputButton(InputType.Forward, "Forward", true, -1, false),
				new InputButton(InputType.Backward, "Backward", true, -1, false),
				new InputButton(InputType.YawLeft, "Yaw Left", true, -1, false),
				new InputButton(InputType.YawRight, "Yaw Right", true, -1, false),
				new InputButton(InputType.Left, "Left", true, -1, false),
				new InputButton(InputType.Right, "Right", true, -1, false),
				new InputButton(InputType.Jump, "Jump", false, 6, false),
				new InputButton(InputType.Slow, "Slow / Low Throttle", true, -1, false),
				new InputButton(InputType.Fast, "Fast / High Throttle", true, -1, false),
				new InputButton(InputType.Crouch, "Crouch", true, 5, false),
				new InputButton(InputType.Drop, "Drop", false, -1, false),
				new InputButton(InputType.EquipmentMenu, "Equipment Menu", false, -1, false),
				new InputButton(InputType.HealthMenu, "Health Menu", false, -1, false),
				new InputButton(InputType.Equipment1, "Equipment 1", false, -1, false),
				new InputButton(InputType.Equipment2, "Equipment 2", false, -1, false),
				new InputButton(InputType.Equipment3, "Equipment 3", false, -1, false),
				new InputButton(InputType.Equipment4, "Equipment 4", false, -1, false),
				new InputButton(InputType.Hands, "Hands", false, -1, false),
				new InputButton(InputType.Surrender, "Surrender", false, -1, false),
				new InputButton(InputType.Point, "Point", true, 15, false),
				new InputButton(InputType.Alert, "Alert/Horn", false, 16, false),
				new InputButton(InputType.Reload, "Reload", false, 4, false),
				new InputButton(InputType.AltFire, "Alt Fire", false, 17, false),
				new InputButton(InputType.ChatMode, "Chat Mode Toggle", false, 7, false),
				new InputButton(InputType.ChatGlobal, "Chat (All)", false, 8, false),
				new InputButton(InputType.ChatLocal, "Chat (Local)", false, 9, false),
				new InputButton(InputType.ChatVoice, "Voice Chat", true, 10, false),
				new InputButton(InputType.ChatHistory, "Chat Log", false, -1, false),
				new InputButton(InputType.PlayerList, "Player List", false, -1, false),
				new InputButton(InputType.CycleCamera, "Cycle Camera", false, 14, false),
				new InputButton(InputType.Map, "Map", false, 12, false),
				new InputButton(InputType.CameraUp, "Camera Up", true, -1, false),
				new InputButton(InputType.CameraDown, "Camera Down", true, -1, false),
				new InputButton(InputType.CameraLeft, "Camera Left", true, -1, false),
				new InputButton(InputType.CameraRight, "Camera Right", true, -1, false),
				new InputButton(InputType.CameraIn, "Camera In", true, -1, false),
				new InputButton(InputType.CameraOut, "Camera Out", true, -1, false),
				new InputButton(InputType.CameraZoomIn, "Camera Zoom In", true, -1, false),
				new InputButton(InputType.CameraZoomOut, "Camera Zoom Out", true, -1, false),
				new InputButton(InputType.Screenshot, "Screenshot (in _Data folder)", false, -1, false),
				new InputButton(InputType.ToggleHUD, "Toggle HUD", false, -1, false),
				new InputButton(InputType.ToggleInterface, "Toggle Interface", false, -1, false),
				new InputButton(InputType.ToggleCursor, "Toggle Cursor", false, -1, false),
				new InputButton(InputType.Cancel, "Cancel", false, 13, false)
			};
			foreach (InputButton inputButton in this.buttonInput)
			{
				int key;
				if (inputButton.GetPrefKey(out key))
				{
					inputButton.SetKey(key);
				}
				else
				{
					inputButton.SetDefault();
				}
			}
			Util.identityBuffer = new IdentityBuffer(this.effectPrefabs[1], 8);
			Util.smokeTrailBuffer = new FollowBuffer(this.effectPrefabs[2], 12);
			Util.fireEffectBuffer = new FollowBuffer(this.effectPrefabs[3], 12);
			Util.lightningEffectBuffer = new LightningBuffer(this.effectPrefabs[26], 2, 7f, 1.5f);
			Util.hitscanTrailBuffer = new TrailBuffer(this.effectPrefabs[4], 24, 0.3f);
			Util.laserBuffer = new LaserBuffer(this.effectPrefabs[5], 8);
			Util.skidMarkBuffer = new BaseBuffer(this.effectPrefabs[6], 48);
			Util.damageMarkerBuffer = new CanvasBuffer(this.effectPrefabs[7], 5);
			Util.hitBuffer = new TimedBuffer(this.effectPrefabs[8], 32, 8f);
			Util.hitDirtBuffer = new TimedBuffer(this.effectPrefabs[9], 16, 8f);
			Util.hitEffects = new TimedBuffer[]
			{
				new SplatBuffer(this.effectPrefabs[10], 32, 8f),
				new TimedBuffer(this.effectPrefabs[11], 8, 3f),
				new TimedBuffer(this.effectPrefabs[12], 8, 3f)
			};
			Util.fireEffects = new FollowBuffer[]
			{
				null,
				new FollowBuffer(this.effectPrefabs[13], 12),
				new FollowBuffer(this.effectPrefabs[14], 3),
				new FollowBuffer(this.effectPrefabs[15], 3),
				new FollowBuffer(this.effectPrefabs[16], 3),
				new FollowBuffer(this.effectPrefabs[17], 3)
			};
			Util.destroyEffects = new TimedBuffer[]
			{
				null,
				new TimedBuffer(this.effectPrefabs[18], 4, 3f),
				new TimedBuffer(this.effectPrefabs[19], 8, 3f),
				new TimedBuffer(this.effectPrefabs[20], 2, 3f),
				new TimedBuffer(this.effectPrefabs[21], 3, 2f),
				new TimedBuffer(this.effectPrefabs[22], 3, 10f)
			};
			Util.thrownEffects = new FollowBuffer[]
			{
				null,
				new FollowBuffer(this.effectPrefabs[23], 8),
				new FollowBuffer(this.effectPrefabs[24], 8),
				new FollowBuffer(this.effectPrefabs[25], 16)
			};
			this.footsteps["Untagged"] = this.footstepClips;
			this.footsteps["Grass"] = this.footstepGrassClips;
			this.skid["Untagged"] = this.skidClip;
			this.skid["Grass"] = this.skidGrassClip;
			RestClient.GetArray<ActionableButtons>(Paths.translationsFile).Then(delegate(ActionableButtons[] response)
			{
				this.buttons = response;
				this.LoadActions();
			}).Catch(delegate(Exception err)
			{
				Util.Log("Translations parsing error: " + ((err != null) ? err.ToString() : null), LogLevel.Log);
			});
			string host;
			ushort port;
			if (this.GetConnectAtStartup(out host, out port))
			{
				this.manager.InitializeClient(host, port);
				return;
			}
			if (ClManager.disconnectIndex != 0U)
			{
				this.ShowGameMessage(DisconnectTypes.disconnectMessages[(int)ClManager.disconnectIndex]);
				ClManager.disconnectIndex = 0U;
				this.ShowMainMenu();
				return;
			}
			this.ShowMainMenu();
		}

		// Token: 0x06000992 RID: 2450 RVA: 0x0002F148 File Offset: 0x0002D348
		public void LoadActions()
		{
			this.actions = new ActionInfo[]
			{
				new ActionInfo(ButtonIndex.ActionMenu, null, delegate()
				{
					this.SendToServer(PacketFlags.Reliable, SvPacket.Dismount, Array.Empty<object>());
				}),
				new ActionInfo(ButtonIndex.ActionMenu, null, delegate()
				{
					this.SendToServer(PacketFlags.Reliable, SvPacket.Deploy, Array.Empty<object>());
				}),
				new ActionInfo(ButtonIndex.ActionMenu, null, new Action(this.ShowFurnitureInventory)),
				new ActionInfo(ButtonIndex.ActionMenu, null, new Action(this.ShowEquipmentInventory)),
				new ActionInfo(ButtonIndex.ActionMenu, null, delegate()
				{
					this.ShowHealthMenu(this.myPlayer);
				})
			};
		}

		// Token: 0x06000993 RID: 2451 RVA: 0x0002F1D4 File Offset: 0x0002D3D4
		public void ClearHighlightEntity()
		{
			this.highlightEntity = null;
			if (this.currentActionMenu)
			{
				this.DestroyActionMenu();
			}
		}

		// Token: 0x06000994 RID: 2452 RVA: 0x0002F1F0 File Offset: 0x0002D3F0
		public void EntityActionMenu()
		{
			this.DestroyActionMenu();
			this.currentActionMenu = Object.Instantiate<EntityActionMenu>(this.entityActionMenu, MonoBehaviourSingleton<SceneManager>.Instance.panelsT);
			this.currentActionMenu.Initialize(new object[]
			{
				(this.highlightEntity != null) ? this.highlightEntity.clEntity : null,
				new Vector2((float)Screen.width * 0.5f, (float)Screen.height)
			});
		}

		// Token: 0x06000995 RID: 2453 RVA: 0x0002F270 File Offset: 0x0002D470
		public void ButtonActionMenu(ClActionable target)
		{
			this.DestroyActionMenu();
			this.currentActionMenu = Object.Instantiate<ButtonActionMenu>(this.buttonActionMenu, MonoBehaviourSingleton<SceneManager>.Instance.panelsT);
			this.currentActionMenu.Initialize(new object[]
			{
				target,
				MonoBehaviourSingleton<SceneManager>.Instance.inputSystemActions.UI.Point.ReadValue<Vector2>()
			});
		}

		// Token: 0x06000996 RID: 2454 RVA: 0x0002F2D7 File Offset: 0x0002D4D7
		public void StartHUD()
		{
			this.hud = Object.Instantiate<HUD>(this.hudPrefab);
		}

		// Token: 0x06000997 RID: 2455 RVA: 0x0002F2EA File Offset: 0x0002D4EA
		public MobileInput InstantiateMobileInput()
		{
			return Object.Instantiate<MobileInput>(this.mobileInputPrefab, this.canvas.transform, false);
		}

		// Token: 0x06000998 RID: 2456 RVA: 0x0002F304 File Offset: 0x0002D504
		public void DestroyText(string id)
		{
			TextDisplay textDisplay;
			if (this.texts.TryGetValue(id, out textDisplay))
			{
				textDisplay.Destroy();
			}
		}

		// Token: 0x06000999 RID: 2457 RVA: 0x0002F327 File Offset: 0x0002D527
		public void ShowPlayerRecordsMenu(string jsonRecords)
		{
			this.playerRecords = JsonConvert.DeserializeObject<List<PlayerRecord>>(jsonRecords);
			this.ShowMenu(this.playerRecordsMenu, Array.Empty<object>());
		}

		// Token: 0x0600099A RID: 2458 RVA: 0x0002F346 File Offset: 0x0002D546
		public void ShowBanRecordsMenu(string jsonRecords)
		{
			this.banRecords = JsonConvert.DeserializeObject<List<Ban>>(jsonRecords);
			this.ShowMenu(this.banRecordsMenu, Array.Empty<object>());
		}

		// Token: 0x0600099B RID: 2459 RVA: 0x0002F365 File Offset: 0x0002D565
		public void ShowEquipmentInventory()
		{
			this.ShowMenu(this.equipmentInventoryMenu, Array.Empty<object>());
		}

		// Token: 0x0600099C RID: 2460 RVA: 0x0002F378 File Offset: 0x0002D578
		public void ShowFurnitureInventory()
		{
			this.ShowMenu(this.furnitureInventoryMenu, Array.Empty<object>());
		}

		// Token: 0x0600099D RID: 2461 RVA: 0x0002F38B File Offset: 0x0002D58B
		public void ShowViewInventory()
		{
			this.ShowMenu(this.viewInventoryMenu, Array.Empty<object>());
		}

		// Token: 0x0600099E RID: 2462 RVA: 0x0002F39E File Offset: 0x0002D59E
		public void ShowShoppingingInventory()
		{
			this.ShowMenu(this.shoppingInventoryMenu, Array.Empty<object>());
		}

		// Token: 0x0600099F RID: 2463 RVA: 0x0002F3B1 File Offset: 0x0002D5B1
		public void ShowSearchedInventory()
		{
			this.ShowMenu(this.searchedInventoryMenu, Array.Empty<object>());
		}

		// Token: 0x060009A0 RID: 2464 RVA: 0x0002F3C4 File Offset: 0x0002D5C4
		public void ShowTradeInventory()
		{
			this.ShowMenu(this.tradeInventoryMenu, Array.Empty<object>());
		}

		// Token: 0x060009A1 RID: 2465 RVA: 0x0002F3D7 File Offset: 0x0002D5D7
		public void ShowPlayersMenu()
		{
			this.ShowMenu(this.playersMenu, Array.Empty<object>());
		}

		// Token: 0x060009A2 RID: 2466 RVA: 0x0002F3EA File Offset: 0x0002D5EA
		public void ShowAppsMenu(int entityID, string time)
		{
			this.ShowMenu(this.appsMenu, new object[]
			{
				entityID,
				time
			});
		}

		// Token: 0x060009A3 RID: 2467 RVA: 0x0002F40B File Offset: 0x0002D60B
		public void ShowAppContactsMenu(int entityID, string time, string jsonString)
		{
			this.ShowMenu(this.appContactsMenu, new object[]
			{
				entityID,
				time,
				JsonConvert.DeserializeObject<List<AppContact>>(jsonString)
			});
		}

		// Token: 0x060009A4 RID: 2468 RVA: 0x0002F435 File Offset: 0x0002D635
		public void ShowAppBlockedMenu(int entityID, string time, string jsonString)
		{
			this.ShowMenu(this.appBlockedMenu, new object[]
			{
				entityID,
				time,
				JsonConvert.DeserializeObject<List<AppContact>>(jsonString)
			});
		}

		// Token: 0x060009A5 RID: 2469 RVA: 0x0002F45F File Offset: 0x0002D65F
		public void ShowAppCallsMenu(int entityID, string time, string jsonString)
		{
			this.ShowMenu(this.appCallsMenu, new object[]
			{
				entityID,
				time,
				JsonConvert.DeserializeObject<List<AppCall>>(jsonString)
			});
		}

		// Token: 0x060009A6 RID: 2470 RVA: 0x0002F489 File Offset: 0x0002D689
		public void ShowAppInboxMenu(int entityID, string time, string jsonString)
		{
			this.ShowMenu(this.appInboxMenu, new object[]
			{
				entityID,
				time,
				JsonConvert.DeserializeObject<List<AppInbox>>(jsonString)
			});
		}

		// Token: 0x060009A7 RID: 2471 RVA: 0x0002F4B3 File Offset: 0x0002D6B3
		public void ShowAppServicesMenu(int entityID, string time)
		{
			this.ShowMenu(this.appServicesMenu, new object[]
			{
				entityID,
				time
			});
		}

		// Token: 0x060009A8 RID: 2472 RVA: 0x0002F4D4 File Offset: 0x0002D6D4
		public void ShowAppDepositMenu(int entityID, string time, int bankBalance, string transactions)
		{
			this.ShowMenu(this.appDepositMenu, new object[]
			{
				entityID,
				time,
				bankBalance,
				JsonConvert.DeserializeObject<List<AppTransaction>>(transactions)
			});
		}

		// Token: 0x060009A9 RID: 2473 RVA: 0x0002F508 File Offset: 0x0002D708
		public void ShowAppWithdrawMenu(int entityID, string time, int bankBalance, string transactions)
		{
			this.ShowMenu(this.appWithdrawMenu, new object[]
			{
				entityID,
				time,
				bankBalance,
				JsonConvert.DeserializeObject<List<AppTransaction>>(transactions)
			});
		}

		// Token: 0x060009AA RID: 2474 RVA: 0x0002F53C File Offset: 0x0002D73C
		public void ShowAppRadioMenu(int entityID, string time, string jsonString)
		{
			this.ShowMenu(this.appRadioMenu, new object[]
			{
				entityID,
				time,
				JsonConvert.DeserializeObject<List<AppContact>>(jsonString)
			});
		}

		// Token: 0x060009AB RID: 2475 RVA: 0x0002F566 File Offset: 0x0002D766
		public void ShowAppMessageMenu(int entityID, string time, string jsonString)
		{
			this.ShowMenu(this.appMessageMenu, new object[]
			{
				entityID,
				time,
				JsonConvert.DeserializeObject<AppMessages>(jsonString)
			});
		}

		// Token: 0x060009AC RID: 2476 RVA: 0x0002F590 File Offset: 0x0002D790
		public void AppAddMessage(string chatName, int senderID, int unreadCount, string jsonString)
		{
			AppMessage appMessage = JsonConvert.DeserializeObject<AppMessage>(jsonString);
			AppMessageMenu appMessageMenu = this.CurrentMenu as AppMessageMenu;
			if (appMessageMenu != null && appMessageMenu.appMessages.playerName == chatName)
			{
				if (!appMessage.self)
				{
					this.SendToServer(PacketFlags.Reliable, SvPacket.ReadMessage, new object[]
					{
						appMessageMenu.entityID,
						chatName
					});
				}
				appMessageMenu.AppendMessage(appMessage);
				return;
			}
			AppInboxMenu appInboxMenu = this.CurrentMenu as AppInboxMenu;
			if (appInboxMenu != null)
			{
				appInboxMenu.RefreshInbox(chatName, EntityCollections.Humans[senderID].profile, appMessage);
			}
			if (!appMessage.self)
			{
				this.ShowGameMessage("Message received from " + chatName);
				this.hud.UpdateMessages(unreadCount, true);
			}
		}

		// Token: 0x060009AD RID: 2477 RVA: 0x0002F648 File Offset: 0x0002D848
		public void ShowVotePanel(int voteIndex, int ID)
		{
			if (this.votePanel)
			{
				this.votePanel.Destroy();
			}
			this.votePanel = Object.Instantiate<VotePanel>(this.manager.votes[voteIndex].votePanelPrefab, MonoBehaviourSingleton<SceneManager>.Instance.panelsT);
			this.votePanel.Initialize(new object[]
			{
				ID
			});
		}

		// Token: 0x060009AE RID: 2478 RVA: 0x0002F6B0 File Offset: 0x0002D8B0
		public void ShowCallPanel(int ID, bool incoming)
		{
			if (this.callPanel)
			{
				this.callPanel.Destroy();
			}
			this.callPanel = Object.Instantiate<CallPanel>(this.callPanelPrefab, MonoBehaviourSingleton<SceneManager>.Instance.panelsT);
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(ID, out shPlayer))
			{
				this.callPanel.Initialize(new object[]
				{
					shPlayer,
					incoming
				});
			}
		}

		// Token: 0x060009AF RID: 2479 RVA: 0x0002F718 File Offset: 0x0002D918
		private void SetChatChannel()
		{
			this.myPlayer.chatChannel = Buffers.reader.ReadUInt16();
			this.chatChannelLabel.text = "#" + this.myPlayer.chatChannel.ToString();
		}

		// Token: 0x060009B0 RID: 2480 RVA: 0x0002F754 File Offset: 0x0002D954
		private void SetChatMode()
		{
			this.myPlayer.chatMode = (ChatMode)Buffers.reader.ReadByte();
			this.chatModeLabel.text = this.myPlayer.chatMode.ToString();
		}

		// Token: 0x060009B1 RID: 2481 RVA: 0x0002F78C File Offset: 0x0002D98C
		private void ChatVoice(ChatMode mode = ChatMode.Public)
		{
			int num = Buffers.reader.ReadInt32();
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(num, out shPlayer))
			{
				shPlayer.clPlayer.ChatVoice(num, Buffers.reader.ReadBytesAndSize(), mode);
			}
		}

		// Token: 0x060009B2 RID: 2482 RVA: 0x0002F7C8 File Offset: 0x0002D9C8
		public void DisplayName()
		{
			ShPlayer shPlayer;
			if (EntityCollections.TryFindByID<ShPlayer>(Buffers.reader.ReadInt32(), out shPlayer))
			{
				shPlayer.clPlayer.DisplayName(Buffers.reader.ReadString());
			}
		}

		// Token: 0x060009B3 RID: 2483 RVA: 0x0002F800 File Offset: 0x0002DA00
		public void AnimatorEnabled()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity) && shEntity.animator)
			{
				shEntity.animator.enabled = Buffers.reader.ReadBoolean();
			}
		}

		// Token: 0x060009B4 RID: 2484 RVA: 0x0002F844 File Offset: 0x0002DA44
		public void AnimatorFloat()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity) && shEntity.animator)
			{
				shEntity.animator.SetFloat(Buffers.reader.ReadInt32(), Buffers.reader.ReadSingle());
			}
		}

		// Token: 0x060009B5 RID: 2485 RVA: 0x0002F890 File Offset: 0x0002DA90
		public void AnimatorInt()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity) && shEntity.animator)
			{
				shEntity.animator.SetInteger(Buffers.reader.ReadInt32(), Buffers.reader.ReadInt32());
			}
		}

		// Token: 0x060009B6 RID: 2486 RVA: 0x0002F8DC File Offset: 0x0002DADC
		public void AnimatorBool()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity) && shEntity.animator)
			{
				shEntity.animator.SetBool(Buffers.reader.ReadInt32(), Buffers.reader.ReadBoolean());
			}
		}

		// Token: 0x060009B7 RID: 2487 RVA: 0x0002F928 File Offset: 0x0002DB28
		public void AnimatorTrigger()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity) && shEntity.animator)
			{
				shEntity.animator.SetTrigger(Animator.StringToHash(Buffers.reader.ReadString()));
			}
		}

		// Token: 0x060009B8 RID: 2488 RVA: 0x0002F970 File Offset: 0x0002DB70
		public void AnimatorState()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity))
			{
				shEntity.clEntity.DeserializeAnimator(Buffers.reader.ReadBytesAndSize());
			}
		}

		// Token: 0x060009B9 RID: 2489 RVA: 0x0002F9A8 File Offset: 0x0002DBA8
		public void VideoPlay()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity))
			{
				shEntity.clEntity.VideoPlay(Buffers.reader.ReadString(), Buffers.reader.ReadSingle());
			}
		}

		// Token: 0x060009BA RID: 2490 RVA: 0x0002F9E8 File Offset: 0x0002DBE8
		public void AddDynamicAction()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity))
			{
				shEntity.dynamicActions[Buffers.reader.ReadString()] = Buffers.reader.ReadString();
			}
		}

		// Token: 0x060009BB RID: 2491 RVA: 0x0002FA28 File Offset: 0x0002DC28
		public void RemoveDynamicAction()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity))
			{
				shEntity.dynamicActions.Remove(Buffers.reader.ReadString());
			}
		}

		// Token: 0x060009BC RID: 2492 RVA: 0x0002FA5E File Offset: 0x0002DC5E
		private void AddSelfAction()
		{
			this.selfActions[Buffers.reader.ReadString()] = Buffers.reader.ReadString();
		}

		// Token: 0x060009BD RID: 2493 RVA: 0x0002FA7F File Offset: 0x0002DC7F
		private void RemoveSelfAction()
		{
			this.selfActions.Remove(Buffers.reader.ReadString());
		}

		// Token: 0x060009BE RID: 2494 RVA: 0x0002FA98 File Offset: 0x0002DC98
		private void AddTypeAction()
		{
			string key = Buffers.reader.ReadString();
			string item = Buffers.reader.ReadString();
			string item2 = Buffers.reader.ReadString();
			this.typeActions[key] = new ValueTuple<string, string>(item, item2);
		}

		// Token: 0x060009BF RID: 2495 RVA: 0x0002FADC File Offset: 0x0002DCDC
		private void RemoveTypeAction()
		{
			string key = Buffers.reader.ReadString();
			this.typeActions.Remove(key);
		}

		// Token: 0x060009C0 RID: 2496 RVA: 0x0002FB01 File Offset: 0x0002DD01
		private void AddInventoryAction()
		{
			this.inventoryActions[Buffers.reader.ReadString()] = new ValueTuple<string, byte, string>(Buffers.reader.ReadString(), Buffers.reader.ReadByte(), Buffers.reader.ReadString());
		}

		// Token: 0x060009C1 RID: 2497 RVA: 0x0002FB3B File Offset: 0x0002DD3B
		private void RemoveInventoryAction()
		{
			this.inventoryActions.Remove(Buffers.reader.ReadString());
		}

		// Token: 0x060009C2 RID: 2498 RVA: 0x0002FB54 File Offset: 0x0002DD54
		private void TargetLocked()
		{
			int num = Buffers.reader.ReadInt32();
			if (num > 0)
			{
				this.myPlayer.lockOnTarget = EntityCollections.FindByID(num);
				this.myPlayer.lockOnTime = Time.time;
				base.StartCoroutine(this.TargetLockedLoop());
				return;
			}
			this.myPlayer.lockOnTarget = null;
		}

		// Token: 0x060009C3 RID: 2499 RVA: 0x0002FBAC File Offset: 0x0002DDAC
		private void ToggleMountWeapon()
		{
			this.myPlayer.curMount.seats[(int)this.myPlayer.seat].weaponIndex = Buffers.reader.ReadInt32();
			this.myPlayer.clPlayer.UpdateMountAmmo(Buffers.reader.ReadInt32());
		}

		// Token: 0x060009C4 RID: 2500 RVA: 0x0002FC00 File Offset: 0x0002DE00
		private void Explosion(Vector3 center, float range, float damage)
		{
			HashSet<Rigidbody> hashSet = new HashSet<Rigidbody>();
			int num = Physics.OverlapSphereNonAlloc(center, range, Util.colliderBuffer, 26373);
			for (int i = 0; i < num; i++)
			{
				Collider collider = Util.colliderBuffer[i];
				Rigidbody rigidbody;
				if (!collider.CompareTag("IgnoreForce") && collider.TryGetComponent<Rigidbody>(out rigidbody) && !rigidbody.isKinematic)
				{
					hashSet.Add(rigidbody);
				}
			}
			foreach (Rigidbody rigidbody2 in hashSet)
			{
				ShPhysical shPhysical;
				Vector3 a;
				if (rigidbody2.TryGetComponent<ShPhysical>(out shPhysical))
				{
					if (!shPhysical.IsSimulated)
					{
						continue;
					}
					a = shPhysical.CenterBounds;
				}
				else
				{
					Destructible destructible;
					if (rigidbody2.TryGetComponent<Destructible>(out destructible))
					{
						destructible.BreakJoint();
					}
					a = rigidbody2.worldCenterOfMass;
				}
				Vector3 a2 = a - center;
				float magnitude = a2.magnitude;
				if (magnitude < range)
				{
					rigidbody2.AddForceAtPosition(0.75f * ((range - magnitude) / range * damage) * (a2 / magnitude), rigidbody2.worldCenterOfMass, ForceMode.Impulse);
				}
			}
		}

		// Token: 0x060009C5 RID: 2501 RVA: 0x0002FD24 File Offset: 0x0002DF24
		private void SetMaxSpeed()
		{
			ShMovable shMovable;
			if (EntityCollections.TryFindByID<ShMovable>(Buffers.reader.ReadInt32(), out shMovable))
			{
				shMovable.maxSpeed = Buffers.reader.ReadSingle();
			}
		}

		// Token: 0x060009C6 RID: 2502 RVA: 0x0002FD54 File Offset: 0x0002DF54
		private void UpdateTextDisplay()
		{
			ShTextDisplay shTextDisplay;
			if (EntityCollections.TryFindByID<ShTextDisplay>(Buffers.reader.ReadInt32(), out shTextDisplay))
			{
				shTextDisplay.UpdateText(Buffers.reader.ReadString());
			}
		}

		// Token: 0x060009C7 RID: 2503 RVA: 0x0002FD84 File Offset: 0x0002DF84
		private void AddVoxels()
		{
			ShVoxel shVoxel;
			if (EntityCollections.TryFindByID<ShVoxel>(Buffers.reader.ReadInt32(), out shVoxel))
			{
				int num = Buffers.reader.ReadInt32();
				ValueTuple<int3, byte>[] array = new ValueTuple<int3, byte>[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = new ValueTuple<int3, byte>(Buffers.reader.ReadInt3(), Buffers.reader.ReadByte());
				}
				shVoxel.AddVoxels(array);
			}
		}

		// Token: 0x060009C8 RID: 2504 RVA: 0x0002FDEC File Offset: 0x0002DFEC
		private void RemoveVoxels()
		{
			ShVoxel shVoxel;
			if (EntityCollections.TryFindByID<ShVoxel>(Buffers.reader.ReadInt32(), out shVoxel))
			{
				int num = Buffers.reader.ReadInt32();
				int3[] array = new int3[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = Buffers.reader.ReadInt3();
				}
				shVoxel.RemoveVoxels(array);
			}
		}

		// Token: 0x060009C9 RID: 2505 RVA: 0x0002FE44 File Offset: 0x0002E044
		private void Tow()
		{
			ShTransport shTransport;
			if (EntityCollections.TryFindByID<ShTransport>(Buffers.reader.ReadInt32(), out shTransport))
			{
				shTransport.Tow(EntityCollections.FindByID<ShTransport>(Buffers.reader.ReadInt32()));
			}
		}

		// Token: 0x060009CA RID: 2506 RVA: 0x0002FE79 File Offset: 0x0002E079
		public IEnumerator Connect()
		{
			yield return new WaitForSecondsRealtime(3f);
			ClManager.reconnect = true;
			MonoBehaviourSingleton<SceneManager>.Instance.ReloadGame(0U);
			yield break;
		}

		// Token: 0x060009CB RID: 2507 RVA: 0x0002FE84 File Offset: 0x0002E084
		private void VideoStop()
		{
			ShEntity shEntity;
			if (EntityCollections.TryFindByID(Buffers.reader.ReadInt32(), out shEntity))
			{
				shEntity.clEntity.VideoStop();
			}
		}

		// Token: 0x060009CC RID: 2508 RVA: 0x0002FEB0 File Offset: 0x0002E0B0
		private void ShowRegisterMenu()
		{
			bool flag = Buffers.reader.ReadBoolean();
			int num = Buffers.reader.ReadInt32();
			List<string> list = new List<string>();
			for (int i = 0; i < num; i++)
			{
				list.Add(Buffers.reader.ReadString());
			}
			this.ShowMenu(this.registerMenu, new object[]
			{
				flag,
				list.ToEntityList<ShPlayer>()
			});
		}

		// Token: 0x060009CD RID: 2509 RVA: 0x0002FF19 File Offset: 0x0002E119
		public void ShowChatMenu(bool local)
		{
			this.ShowMenu(this.chatMenu, new object[]
			{
				local
			});
		}

		// Token: 0x060009CE RID: 2510 RVA: 0x0002FF38 File Offset: 0x0002E138
		public void ShowMessage(string message)
		{
			if (this.gameMessagesT.childCount >= 6)
			{
				this.gameMessagesT.GetChild(0).GetComponent<GameMessage>().Destroy();
			}
			Object.Instantiate<GameMessage>(this.gameMessagePrefab, this.gameMessagesT, false).Initialize(new object[]
			{
				message
			});
			this.AppendChatHistory(message);
		}

		// Token: 0x060009CF RID: 2511 RVA: 0x0002FF91 File Offset: 0x0002E191
		public void ShowGameMessage(string message)
		{
			this.ShowMessage(message.ParseColorCodes());
		}

		// Token: 0x060009D0 RID: 2512 RVA: 0x0002FFA0 File Offset: 0x0002E1A0
		public void ShowInventoryMessage(int itemIndex, int amount)
		{
			string itemName = MonoBehaviourSingleton<SceneManager>.Instance.GetEntity<ShItem>(itemIndex).itemName;
			string arg = (amount > 0) ? "green" : "red";
			string message = string.Format("<color={0}>{1} {2}</color>", arg, amount, itemName);
			this.ShowMessage(message);
		}

		// Token: 0x060009D1 RID: 2513 RVA: 0x0002FFE9 File Offset: 0x0002E1E9
		public void ShowHealthMenu(ShPlayer player)
		{
			this.ShowMenu(this.healthMenu, new object[]
			{
				player
			});
		}

		// Token: 0x060009D2 RID: 2514 RVA: 0x00030001 File Offset: 0x0002E201
		public void ShowProcessMenu(ShProcessor processor)
		{
			this.ShowMenu(this.processMenu, new object[]
			{
				processor
			});
		}

		// Token: 0x060009D3 RID: 2515 RVA: 0x00030019 File Offset: 0x0002E219
		public void ShowPauseMenu()
		{
			this.ShowMenu(this.pauseMenu, Array.Empty<object>());
		}

		// Token: 0x060009D4 RID: 2516 RVA: 0x0003002C File Offset: 0x0002E22C
		public void ShowControlsMenu()
		{
			this.ShowMenu(this.controlsMenu, Array.Empty<object>());
		}

		// Token: 0x060009D5 RID: 2517 RVA: 0x0003003F File Offset: 0x0002E23F
		public void ShowCustomizeMenu()
		{
			this.ShowMenu(this.customizeMenu, Array.Empty<object>());
		}

		// Token: 0x060009D6 RID: 2518 RVA: 0x00030052 File Offset: 0x0002E252
		public void ShowSettingsMenu()
		{
			this.ShowMenu(this.settingsMenu, Array.Empty<object>());
		}

		// Token: 0x060009D7 RID: 2519 RVA: 0x00030068 File Offset: 0x0002E268
		public void VisualTreeAssetClone(string visualTreeAssetName, string parentVisualElementName, string newVisualElementName)
		{
			VisualTreeAsset visualTreeAsset;
			if (this.customUIs.TryGetValue(visualTreeAssetName, out visualTreeAsset))
			{
				TemplateContainer templateContainer = visualTreeAsset.Instantiate();
				VisualElement visualElement = MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<VisualElement>(parentVisualElementName);
				if (visualElement == MonoBehaviourSingleton<SceneManager>.Instance.uiDocument.rootVisualElement)
				{
					MonoBehaviourSingleton<SceneManager>.Instance.customUIElement.Add(templateContainer);
					templateContainer.StretchToParentSize();
				}
				else
				{
					visualElement.Add(templateContainer);
				}
				templateContainer.pickingMode = PickingMode.Ignore;
				templateContainer.name = (string.IsNullOrEmpty(newVisualElementName) ? visualTreeAssetName : newVisualElementName);
			}
		}

		// Token: 0x060009D8 RID: 2520 RVA: 0x000300E2 File Offset: 0x0002E2E2
		public void VisualElementRemove(string element)
		{
			MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<VisualElement>(element).RemoveFromHierarchy();
		}

		// Token: 0x060009D9 RID: 2521 RVA: 0x000300F4 File Offset: 0x0002E2F4
		public void VisualElementOpacity(string element, float setting)
		{
			MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<VisualElement>(element).style.opacity = setting;
		}

		// Token: 0x060009DA RID: 2522 RVA: 0x00030111 File Offset: 0x0002E311
		public void VisualElementDisplay(string element, bool setting)
		{
			MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<VisualElement>(element).style.display = (setting ? DisplayStyle.Flex : DisplayStyle.None);
		}

		// Token: 0x060009DB RID: 2523 RVA: 0x00030134 File Offset: 0x0002E334
		public void VisualElementVisibility(string element, bool setting)
		{
			MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<VisualElement>(element).style.visibility = (setting ? Visibility.Visible : Visibility.Hidden);
		}

		// Token: 0x060009DC RID: 2524 RVA: 0x00030157 File Offset: 0x0002E357
		public void VisualElementOverflow(string element, bool setting)
		{
			MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<VisualElement>(element).style.overflow = (setting ? Overflow.Visible : Overflow.Hidden);
		}

		// Token: 0x060009DD RID: 2525 RVA: 0x0003017C File Offset: 0x0002E37C
		public void AddButtonClickedEvent(string element, string eventName)
		{
			UnityEngine.UIElements.Button visualElement = MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<UnityEngine.UIElements.Button>(element);
			if (visualElement != null)
			{
				visualElement.clicked += delegate()
				{
					this.SendToServer(PacketFlags.Reliable, SvPacket.ButtonClickedEvent, new object[]
					{
						eventName,
						element
					});
				};
				return;
			}
			Util.Log("Could not find button " + element, LogLevel.Log);
		}

		// Token: 0x060009DE RID: 2526 RVA: 0x000301E4 File Offset: 0x0002E3E4
		public void GetTextFieldText(string element, string eventName)
		{
			string text = MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<TextField>(element).text;
			this.SendToServer(PacketFlags.Reliable, SvPacket.GetTextFieldText, new object[]
			{
				eventName,
				element,
				text
			});
		}

		// Token: 0x060009DF RID: 2527 RVA: 0x00030220 File Offset: 0x0002E420
		public void SetTextElementText(string element, string text)
		{
			MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<TextElement>(element).text = text;
		}

		// Token: 0x060009E0 RID: 2528 RVA: 0x00030234 File Offset: 0x0002E434
		public void GetSliderValue(string element, string eventName)
		{
			float value = MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<UnityEngine.UIElements.Slider>(element).value;
			this.SendToServer(PacketFlags.Reliable, SvPacket.GetSliderValue, new object[]
			{
				eventName,
				element,
				value
			});
		}

		// Token: 0x060009E1 RID: 2529 RVA: 0x00030275 File Offset: 0x0002E475
		public void SetSliderValue(string element, float value)
		{
			MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<UnityEngine.UIElements.Slider>(element).value = value;
		}

		// Token: 0x060009E2 RID: 2530 RVA: 0x00030288 File Offset: 0x0002E488
		public void SetProgressBarValue(string element, float value)
		{
			MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<ProgressBar>(element).value = value;
		}

		// Token: 0x060009E3 RID: 2531 RVA: 0x0003029C File Offset: 0x0002E49C
		public void GetToggleValue(string element, string eventName)
		{
			bool value = MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<UnityEngine.UIElements.Toggle>(element).value;
			this.SendToServer(PacketFlags.Reliable, SvPacket.GetToggleValue, new object[]
			{
				eventName,
				element,
				value
			});
		}

		// Token: 0x060009E4 RID: 2532 RVA: 0x000302DD File Offset: 0x0002E4DD
		public void SetToggleValue(string element, bool value)
		{
			MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<UnityEngine.UIElements.Toggle>(element).value = value;
		}

		// Token: 0x060009E5 RID: 2533 RVA: 0x000302F0 File Offset: 0x0002E4F0
		public void GetRadioButtonGroupValue(string element, string eventName)
		{
			int value = MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<RadioButtonGroup>(element).value;
			this.SendToServer(PacketFlags.Reliable, SvPacket.GetRadioButtonGroupValue, new object[]
			{
				eventName,
				element,
				value
			});
		}

		// Token: 0x060009E6 RID: 2534 RVA: 0x00030331 File Offset: 0x0002E531
		public void SetRadioButtonGroupValue(string element, int value)
		{
			MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<RadioButtonGroup>(element).value = value;
		}

		// Token: 0x060009E7 RID: 2535 RVA: 0x00030344 File Offset: 0x0002E544
		public void SetRadioButtonGroupChoices(string element, string choicesData)
		{
			List<string> choices = JsonConvert.DeserializeObject<List<string>>(choicesData);
			MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<RadioButtonGroup>(element).choices = choices;
		}

		// Token: 0x060009E8 RID: 2536 RVA: 0x0003036C File Offset: 0x0002E56C
		public void GetDropdownFieldValue(string element, string eventName)
		{
			int index = MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<DropdownField>(element).index;
			this.SendToServer(PacketFlags.Reliable, SvPacket.GetDropdownFieldValue, new object[]
			{
				eventName,
				element,
				index
			});
		}

		// Token: 0x060009E9 RID: 2537 RVA: 0x000303AD File Offset: 0x0002E5AD
		public void SetDropdownFieldValue(string element, int value)
		{
			MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<DropdownField>(element).index = value;
		}

		// Token: 0x060009EA RID: 2538 RVA: 0x000303C0 File Offset: 0x0002E5C0
		public void SetDropdownFieldChoices(string element, string choicesData)
		{
			List<string> choices = JsonConvert.DeserializeObject<List<string>>(choicesData);
			MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<DropdownField>(element).choices = choices;
		}

		// Token: 0x060009EB RID: 2539 RVA: 0x000303E8 File Offset: 0x0002E5E8
		public void VisualElementCursorVisibility(string element)
		{
			VisualElement visualElement = MonoBehaviourSingleton<SceneManager>.Instance.GetVisualElement<VisualElement>(element);
			if (visualElement != null && MonoBehaviourSingleton<SceneManager>.Instance.CursorContainers.Add(visualElement))
			{
				MonoBehaviourSingleton<SceneManager>.Instance.ShowCursor = true;
			}
		}

		// Token: 0x060009EC RID: 2540 RVA: 0x00030424 File Offset: 0x0002E624
		public void ShowTextMenu(float xMin, float yMin, float xMax, float yMax, string id, string title, string text)
		{
			this.ShowMenu(this.textMenu, new object[]
			{
				xMin,
				yMin,
				xMax,
				yMax,
				id,
				title,
				text
			});
		}

		// Token: 0x060009ED RID: 2541 RVA: 0x00030478 File Offset: 0x0002E678
		public void ShowOptionMenu(float xMin, float yMin, float xMax, float yMax, string id, string title, int targetID, string optionData, string actionData)
		{
			this.ShowMenu(this.optionMenu, new object[]
			{
				xMin,
				yMin,
				xMax,
				yMax,
				id,
				title,
				targetID,
				optionData,
				actionData
			});
		}

		// Token: 0x060009EE RID: 2542 RVA: 0x000304DC File Offset: 0x0002E6DC
		public void ShowInputMenu(float xMin, float yMin, float xMax, float yMax, string id, string title, int targetID, int characterLimit)
		{
			this.ShowMenu(this.inputMenu, new object[]
			{
				xMin,
				yMin,
				xMax,
				yMax,
				id,
				title,
				targetID,
				characterLimit
			});
		}

		// Token: 0x060009EF RID: 2543 RVA: 0x00030540 File Offset: 0x0002E740
		public void ShowTextPanel(string id, string text, string options, int initialOptionIndex)
		{
			TextPanel textPanel;
			if (this.textPanels.TryGetValue(id, out textPanel))
			{
				if (textPanel.optionsString == options)
				{
					textPanel.UpdateText(text, initialOptionIndex);
					return;
				}
				this.DestroyTextPanel(id);
			}
			TextPanel textPanel2 = Object.Instantiate<TextPanel>(this.textPanel, this.textPanelsT, false);
			textPanel2.Initialize(new object[]
			{
				id,
				text,
				options,
				initialOptionIndex
			});
			this.textPanels[id] = textPanel2;
		}

		// Token: 0x060009F0 RID: 2544 RVA: 0x000305C0 File Offset: 0x0002E7C0
		public void DestroyTextPanel(string id)
		{
			TextPanel textPanel;
			if (this.textPanels.TryGetValue(id, out textPanel))
			{
				textPanel.Destroy();
				this.textPanels.Remove(id);
			}
		}

		// Token: 0x060009F1 RID: 2545 RVA: 0x000305F0 File Offset: 0x0002E7F0
		public void ShowHackingMenu(string title, int targetID, string menuData, string optionID, float difficulty)
		{
			this.ShowMenu(this.hackingMenu, new object[]
			{
				title,
				targetID,
				menuData,
				optionID,
				difficulty
			});
		}

		// Token: 0x060009F2 RID: 2546 RVA: 0x00030624 File Offset: 0x0002E824
		public void ShowCrackingMenu(string title, int targetID, string menuData, string optionID, float difficulty)
		{
			this.ShowMenu(this.crackingMenu, new object[]
			{
				title,
				targetID,
				menuData,
				optionID,
				difficulty
			});
		}

		// Token: 0x060009F3 RID: 2547 RVA: 0x00030658 File Offset: 0x0002E858
		public void ShowChatHistoryMenu()
		{
			this.ShowMenu(this.chatHistoryMenu, new object[]
			{
				this.GetChatHistory()
			});
		}

		// Token: 0x060009F4 RID: 2548 RVA: 0x00030675 File Offset: 0x0002E875
		public void ShowOpenURLMenu(string url, string title)
		{
			this.ShowMenu(this.openURLMenu, new object[]
			{
				url,
				title
			});
		}

		// Token: 0x060009F5 RID: 2549 RVA: 0x00030691 File Offset: 0x0002E891
		public void ShowPlayerKickMenu(ShPlayer targetPlayer)
		{
			this.ShowMenu(this.playerKickMenu, new object[]
			{
				targetPlayer
			});
		}

		// Token: 0x060009F6 RID: 2550 RVA: 0x000306A9 File Offset: 0x0002E8A9
		public void ShowPlayerBanMenu(ShPlayer targetPlayer)
		{
			this.ShowMenu(this.playerBanMenu, new object[]
			{
				targetPlayer
			});
		}

		// Token: 0x060009F7 RID: 2551 RVA: 0x000306C1 File Offset: 0x0002E8C1
		public void ShowPlayerBanRecordMenu(PlayerRecord playerRecord)
		{
			this.ShowMenu(this.playerBanRecordMenu, new object[]
			{
				playerRecord
			});
		}

		// Token: 0x060009F8 RID: 2552 RVA: 0x000306E0 File Offset: 0x0002E8E0
		public void RefreshListMenu<T>(params object[] args) where T : ListMenu
		{
			T t = this.CurrentMenu as T;
			if (t != null)
			{
				t.Refill(args);
			}
		}

		// Token: 0x060009F9 RID: 2553 RVA: 0x00030712 File Offset: 0x0002E912
		public void ShowMenu(Menu menu, params object[] args)
		{
			this.ClearHighlightEntity();
			Menu menu2 = Object.Instantiate<Menu>(menu, MonoBehaviourSingleton<SceneManager>.Instance.panelsT, false);
			menu2.name = menu.name;
			menu2.Initialize(args);
		}

		// Token: 0x060009FA RID: 2554 RVA: 0x0003073D File Offset: 0x0002E93D
		public void DestroyActionMenu()
		{
			if (this.currentActionMenu)
			{
				this.currentActionMenu.Close();
				this.currentActionMenu = null;
			}
		}

		// Token: 0x060009FB RID: 2555 RVA: 0x00030760 File Offset: 0x0002E960
		public void DestroyMenu(string id = "Default")
		{
			Menu menu;
			if (this.menus.TryGetValue(id, out menu))
			{
				menu.Destroy();
			}
		}

		// Token: 0x060009FC RID: 2556 RVA: 0x00030784 File Offset: 0x0002E984
		public bool GetButton(InputType index, bool thisFrame = false)
		{
			InputButton inputButton = this.buttonInput[(int)index];
			if (inputButton.button == 0)
			{
				return false;
			}
			if (inputButton.repeat && !thisFrame)
			{
				if (!inputButton.mouse)
				{
					return Keyboard.current[(Key)inputButton.button].isPressed;
				}
				switch (inputButton.button)
				{
				case 1:
					return Mouse.current.leftButton.isPressed;
				case 2:
					return Mouse.current.rightButton.isPressed;
				case 3:
					return Mouse.current.middleButton.isPressed;
				default:
					return false;
				}
			}
			else
			{
				if (!inputButton.mouse)
				{
					return Keyboard.current[(Key)inputButton.button].wasPressedThisFrame;
				}
				switch (inputButton.button)
				{
				case 1:
					return Mouse.current.leftButton.wasPressedThisFrame;
				case 2:
					return Mouse.current.rightButton.wasPressedThisFrame;
				case 3:
					return Mouse.current.middleButton.wasPressedThisFrame;
				default:
					return false;
				}
			}
		}

		// Token: 0x060009FD RID: 2557 RVA: 0x00030888 File Offset: 0x0002EA88
		public void StartDamageMarker(float angle)
		{
			base.StartCoroutine(this.DamageMarker(angle));
		}

		// Token: 0x060009FE RID: 2558 RVA: 0x00030898 File Offset: 0x0002EA98
		private IEnumerator DamageMarker(float angle)
		{
			float time = Time.time;
			float endTime = time + 0.4f;
			GameObject damageMarker = Util.damageMarkerBuffer.Execute();
			QuadGraphic quad = damageMarker.GetComponent<QuadGraphic>();
			while (Time.time < endTime)
			{
				Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
				Vector3 vector = MonoBehaviourSingleton<MainCamera>.Instance.worldCameraT.InverseTransformDirection(direction);
				quad.rectTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(vector.z, vector.x) * 57.29578f);
				quad.Initialize(new Vector2(300f, 0f), new Vector2(200f * (endTime - Time.time), 10f), new Color(1f, 0f, 0f, 0.4f));
				yield return null;
			}
			Util.damageMarkerBuffer.Release(damageMarker);
			yield break;
		}

		// Token: 0x060009FF RID: 2559 RVA: 0x000308A8 File Offset: 0x0002EAA8
		public TemplateContainer InstantiateUIAsset(VisualTreeAsset uiAsset)
		{
			VisualTreeAsset visualTreeAsset;
			return (this.customUIs.TryGetValue(uiAsset.name, out visualTreeAsset) ? visualTreeAsset : uiAsset).Instantiate();
		}

		// Token: 0x0400075D RID: 1885
		[SerializeField]
		private bool startAsServer;

		// Token: 0x0400075E RID: 1886
		[NonSerialized]
		public Dictionary<string, string> selfActions = new Dictionary<string, string>();

		// Token: 0x0400075F RID: 1887
		[NonSerialized]
		public Dictionary<string, ValueTuple<string, string>> typeActions = new Dictionary<string, ValueTuple<string, string>>();

		// Token: 0x04000760 RID: 1888
		[NonSerialized]
		public Dictionary<string, ValueTuple<string, byte, string>> inventoryActions = new Dictionary<string, ValueTuple<string, byte, string>>();

		// Token: 0x04000761 RID: 1889
		public const float sampleDuration = 0.06f;

		// Token: 0x04000762 RID: 1890
		public const int sampleSize = 960;

		// Token: 0x04000763 RID: 1891
		private const float autoAimLookScale = 15f;

		// Token: 0x04000764 RID: 1892
		public ShManager manager;

		// Token: 0x04000765 RID: 1893
		public Canvas canvas;

		// Token: 0x04000766 RID: 1894
		public Label chatChannelLabel;

		// Token: 0x04000767 RID: 1895
		public Label chatModeLabel;

		// Token: 0x04000768 RID: 1896
		[NonSerialized]
		public PersistentSettings persistentSettings;

		// Token: 0x04000769 RID: 1897
		[NonSerialized]
		public ShEntity highlightEntity;

		// Token: 0x0400076A RID: 1898
		[SerializeField]
		private World worldPrefab;

		// Token: 0x0400076B RID: 1899
		public PlayerRenderer playerRendererPrefab;

		// Token: 0x0400076C RID: 1900
		[SerializeField]
		private Camera bakedMapCamera;

		// Token: 0x0400076D RID: 1901
		[SerializeField]
		private RawImage bakedMap;

		// Token: 0x0400076E RID: 1902
		[SerializeField]
		private RectTransform bakedMapT;

		// Token: 0x0400076F RID: 1903
		public RenderTexture videoTexture;

		// Token: 0x04000770 RID: 1904
		public Texture2D loadingTexture;

		// Token: 0x04000771 RID: 1905
		[NonSerialized]
		public bool customDownloads;

		// Token: 0x04000772 RID: 1906
		[NonSerialized]
		public Dictionary<string, VisualTreeAsset> customUIs = new Dictionary<string, VisualTreeAsset>();

		// Token: 0x04000773 RID: 1907
		[NonSerialized]
		public bool offlineConsole;

		// Token: 0x04000774 RID: 1908
		[NonSerialized]
		public bool autoaim;

		// Token: 0x04000775 RID: 1909
		[NonSerialized]
		public ShPlayer aimTarget;

		// Token: 0x04000776 RID: 1910
		public Voice voicePrefab;

		// Token: 0x04000777 RID: 1911
		[SerializeField]
		private AudioClip[] footstepClips;

		// Token: 0x04000778 RID: 1912
		[SerializeField]
		private AudioClip[] footstepGrassClips;

		// Token: 0x04000779 RID: 1913
		[SerializeField]
		private AudioClip skidClip;

		// Token: 0x0400077A RID: 1914
		[SerializeField]
		private AudioClip skidGrassClip;

		// Token: 0x0400077B RID: 1915
		public AudioClip playerHurt;

		// Token: 0x0400077C RID: 1916
		public AudioClip playerHurtMinor;

		// Token: 0x0400077D RID: 1917
		public AudioClip playerHeal;

		// Token: 0x0400077E RID: 1918
		public AudioClip playerHealMinor;

		// Token: 0x0400077F RID: 1919
		public Dictionary<string, AudioClip[]> footsteps = new Dictionary<string, AudioClip[]>();

		// Token: 0x04000780 RID: 1920
		public Dictionary<string, AudioClip> skid = new Dictionary<string, AudioClip>();

		// Token: 0x04000781 RID: 1921
		[NonSerialized]
		public Dictionary<ValueTuple<int, Vector2Int>, ClientSector> sectors = new Dictionary<ValueTuple<int, Vector2Int>, ClientSector>();

		// Token: 0x04000782 RID: 1922
		private int recordRate;

		// Token: 0x04000783 RID: 1923
		private int previousPosition;

		// Token: 0x04000784 RID: 1924
		private int recordIndex;

		// Token: 0x04000785 RID: 1925
		private AudioClip microphoneClip;

		// Token: 0x04000786 RID: 1926
		private int recordSampleSize;

		// Token: 0x04000787 RID: 1927
		private float[] recordData;

		// Token: 0x04000788 RID: 1928
		public Material goodMaterial;

		// Token: 0x04000789 RID: 1929
		public Material badMaterial;

		// Token: 0x0400078A RID: 1930
		public GameObject[] effectPrefabs;

		// Token: 0x0400078B RID: 1931
		[NonSerialized]
		public SvPacket[] appPackets = new SvPacket[]
		{
			SvPacket.Apps,
			SvPacket.AppContacts,
			SvPacket.AppBlocked,
			SvPacket.AppCalls,
			SvPacket.AppInbox,
			SvPacket.AppServices,
			SvPacket.AppDeposit,
			SvPacket.AppWithdraw,
			SvPacket.AppRadio
		};

		// Token: 0x0400078C RID: 1932
		[NonSerialized]
		public List<JobInfoShared> jobs;

		// Token: 0x0400078D RID: 1933
		[NonSerialized]
		public ShPlayer myPlayer;

		// Token: 0x0400078E RID: 1934
		private float nextInputSend;

		// Token: 0x0400078F RID: 1935
		[NonSerialized]
		public Vector2 lookInput;

		// Token: 0x04000790 RID: 1936
		[NonSerialized]
		public Vector3 lastInput;

		// Token: 0x04000791 RID: 1937
		[NonSerialized]
		public byte lastMode;

		// Token: 0x04000792 RID: 1938
		[NonSerialized]
		public int myID;

		// Token: 0x04000793 RID: 1939
		public Action[] handler;

		// Token: 0x04000794 RID: 1940
		[SerializeField]
		private AudioSource music;

		// Token: 0x04000795 RID: 1941
		public Peer connection;

		// Token: 0x04000796 RID: 1942
		[NonSerialized]
		public ClientSector sector;

		// Token: 0x04000797 RID: 1943
		[NonSerialized]
		public int clientSectorRange;

		// Token: 0x04000798 RID: 1944
		[NonSerialized]
		public string profileURL;

		// Token: 0x04000799 RID: 1945
		private int framerateLimit;

		// Token: 0x0400079A RID: 1946
		public static float[] sampleBuffer = new float[960];

		// Token: 0x0400079B RID: 1947
		private static byte[] encodedBuffer = new byte[960];

		// Token: 0x0400079C RID: 1948
		public static uint disconnectIndex = 0U;

		// Token: 0x0400079D RID: 1949
		private static bool alreadyStarted;

		// Token: 0x0400079E RID: 1950
		private static bool reconnect;

		// Token: 0x0400079F RID: 1951
		private static string lastHostname;

		// Token: 0x040007A0 RID: 1952
		private static ushort lastPort;

		// Token: 0x040007A1 RID: 1953
		[NonSerialized]
		public Vector2 sensitivity;

		// Token: 0x040007A2 RID: 1954
		private int languageIndex;

		// Token: 0x040007A3 RID: 1955
		private float masterVolume;

		// Token: 0x040007A4 RID: 1956
		private float musicVolume;

		// Token: 0x040007A5 RID: 1957
		private float voiceVolume;

		// Token: 0x040007A6 RID: 1958
		[NonSerialized]
		public string inputDevice;

		// Token: 0x040007A7 RID: 1959
		private bool allowHUD = true;

		// Token: 0x040007A8 RID: 1960
		private readonly Queue<string> chatHistory = new Queue<string>();

		// Token: 0x040007A9 RID: 1961
		[NonSerialized]
		public List<PlayerRecord> playerRecords;

		// Token: 0x040007AA RID: 1962
		[NonSerialized]
		public List<Ban> banRecords;

		// Token: 0x040007AB RID: 1963
		[SerializeField]
		private VisualTreeAsset elementCacheAsset;

		// Token: 0x040007AC RID: 1964
		[SerializeField]
		private CallPanel callPanelPrefab;

		// Token: 0x040007AD RID: 1965
		[SerializeField]
		private MainMenu mainMenu;

		// Token: 0x040007AE RID: 1966
		[SerializeField]
		private EntityActionMenu entityActionMenu;

		// Token: 0x040007AF RID: 1967
		[SerializeField]
		private ButtonActionMenu buttonActionMenu;

		// Token: 0x040007B0 RID: 1968
		[SerializeField]
		private PlayerRecordsMenu playerRecordsMenu;

		// Token: 0x040007B1 RID: 1969
		[SerializeField]
		private BanRecordsMenu banRecordsMenu;

		// Token: 0x040007B2 RID: 1970
		[SerializeField]
		private EquipmentMenu equipmentInventoryMenu;

		// Token: 0x040007B3 RID: 1971
		[SerializeField]
		private FurnitureMenu furnitureInventoryMenu;

		// Token: 0x040007B4 RID: 1972
		[SerializeField]
		private ViewMenu viewInventoryMenu;

		// Token: 0x040007B5 RID: 1973
		[SerializeField]
		private ShoppingMenu shoppingInventoryMenu;

		// Token: 0x040007B6 RID: 1974
		[SerializeField]
		private SearchedMenu searchedInventoryMenu;

		// Token: 0x040007B7 RID: 1975
		[SerializeField]
		private TradeMenu tradeInventoryMenu;

		// Token: 0x040007B8 RID: 1976
		[SerializeField]
		private PlayersMenu playersMenu;

		// Token: 0x040007B9 RID: 1977
		[SerializeField]
		private AppsMenu appsMenu;

		// Token: 0x040007BA RID: 1978
		[SerializeField]
		private AppContactsMenu appContactsMenu;

		// Token: 0x040007BB RID: 1979
		[SerializeField]
		private AppContactsMenu appBlockedMenu;

		// Token: 0x040007BC RID: 1980
		[SerializeField]
		private AppCallsMenu appCallsMenu;

		// Token: 0x040007BD RID: 1981
		[SerializeField]
		private AppInboxMenu appInboxMenu;

		// Token: 0x040007BE RID: 1982
		[SerializeField]
		private AppMessageMenu appMessageMenu;

		// Token: 0x040007BF RID: 1983
		[SerializeField]
		private AppServicesMenu appServicesMenu;

		// Token: 0x040007C0 RID: 1984
		[SerializeField]
		private AppBankingMenu appDepositMenu;

		// Token: 0x040007C1 RID: 1985
		[SerializeField]
		private AppBankingMenu appWithdrawMenu;

		// Token: 0x040007C2 RID: 1986
		[SerializeField]
		private AppContactsMenu appRadioMenu;

		// Token: 0x040007C3 RID: 1987
		[SerializeField]
		private RegisterMenu registerMenu;

		// Token: 0x040007C4 RID: 1988
		[SerializeField]
		private ChatMenu chatMenu;

		// Token: 0x040007C5 RID: 1989
		[SerializeField]
		private PauseMenu pauseMenu;

		// Token: 0x040007C6 RID: 1990
		[SerializeField]
		private ControlsMenu controlsMenu;

		// Token: 0x040007C7 RID: 1991
		[SerializeField]
		private CustomizeMenu customizeMenu;

		// Token: 0x040007C8 RID: 1992
		[SerializeField]
		private SettingsMenu settingsMenu;

		// Token: 0x040007C9 RID: 1993
		[SerializeField]
		private TextMenu textMenu;

		// Token: 0x040007CA RID: 1994
		[SerializeField]
		private OptionMenu optionMenu;

		// Token: 0x040007CB RID: 1995
		[SerializeField]
		private InputMenu inputMenu;

		// Token: 0x040007CC RID: 1996
		[SerializeField]
		private HackingMenu hackingMenu;

		// Token: 0x040007CD RID: 1997
		[SerializeField]
		private CrackingMenu crackingMenu;

		// Token: 0x040007CE RID: 1998
		[SerializeField]
		private TextPanel textPanel;

		// Token: 0x040007CF RID: 1999
		[SerializeField]
		private ChatHistoryMenu chatHistoryMenu;

		// Token: 0x040007D0 RID: 2000
		[SerializeField]
		private OpenURLMenu openURLMenu;

		// Token: 0x040007D1 RID: 2001
		[SerializeField]
		private PlayerKickMenu playerKickMenu;

		// Token: 0x040007D2 RID: 2002
		[SerializeField]
		private PlayerBanMenu playerBanMenu;

		// Token: 0x040007D3 RID: 2003
		[SerializeField]
		private PlayerBanRecordMenu playerBanRecordMenu;

		// Token: 0x040007D4 RID: 2004
		[SerializeField]
		private GameMessage gameMessagePrefab;

		// Token: 0x040007D5 RID: 2005
		public ChatVoiceLabel chatVoiceLabel;

		// Token: 0x040007D6 RID: 2006
		[SerializeField]
		private HealthMenu healthMenu;

		// Token: 0x040007D7 RID: 2007
		[SerializeField]
		private ProcessMenu processMenu;

		// Token: 0x040007D8 RID: 2008
		[SerializeField]
		private HUD hudPrefab;

		// Token: 0x040007D9 RID: 2009
		[NonSerialized]
		public HUD hud;

		// Token: 0x040007DA RID: 2010
		[NonSerialized]
		public TemplateContainer uiClone;

		// Token: 0x040007DB RID: 2011
		[SerializeField]
		private MobileInput mobileInputPrefab;

		// Token: 0x040007DC RID: 2012
		private float orientTime;

		// Token: 0x040007DD RID: 2013
		[SerializeField]
		private TextDisplay textPrefab;

		// Token: 0x040007DE RID: 2014
		[SerializeField]
		private ProgressDisplay progressDisplayPrefab;

		// Token: 0x040007DF RID: 2015
		[NonSerialized]
		public ActionMenu currentActionMenu;

		// Token: 0x040007E0 RID: 2016
		public Dictionary<string, TextDisplay> texts = new Dictionary<string, TextDisplay>();

		// Token: 0x040007E1 RID: 2017
		public readonly Dictionary<string, ProgressDisplay> progressDisplays = new Dictionary<string, ProgressDisplay>();

		// Token: 0x040007E2 RID: 2018
		public readonly Dictionary<string, TextPanel> textPanels = new Dictionary<string, TextPanel>();

		// Token: 0x040007E3 RID: 2019
		public Dictionary<string, Menu> menus = new Dictionary<string, Menu>();

		// Token: 0x040007E4 RID: 2020
		public Dictionary<int, ChatVoiceLabel> chatVoiceLabels = new Dictionary<int, ChatVoiceLabel>();

		// Token: 0x040007E5 RID: 2021
		public Transform progressBarsT;

		// Token: 0x040007E6 RID: 2022
		public Transform textPanelsT;

		// Token: 0x040007E7 RID: 2023
		public Transform textDisplaysT;

		// Token: 0x040007E8 RID: 2024
		public Transform radioBarT;

		// Token: 0x040007E9 RID: 2025
		public Transform gameMessagesT;

		// Token: 0x040007EA RID: 2026
		private VotePanel votePanel;

		// Token: 0x040007EB RID: 2027
		private CallPanel callPanel;

		// Token: 0x040007EC RID: 2028
		public OpusEncoder encoder;

		// Token: 0x040007ED RID: 2029
		[NonSerialized]
		public InputButton[] buttonInput;

		// Token: 0x040007EE RID: 2030
		private byte[] assetBuffer;

		// Token: 0x040007EF RID: 2031
		public ActionInfo[] actions;

		// Token: 0x040007F0 RID: 2032
		[NonSerialized]
		public byte[] permissions;

		// Token: 0x040007F1 RID: 2033
		private bool imageDownloading;

		// Token: 0x040007F2 RID: 2034
		private readonly Queue<ImageItem> imageQueue = new Queue<ImageItem>();

		// Token: 0x040007F3 RID: 2035
		private readonly Dictionary<string, Texture2D> imageSet = new Dictionary<string, Texture2D>();

		// Token: 0x040007F4 RID: 2036
		[SerializeField]
		private CircleGraphic targetGraphicPrefab;

		// Token: 0x040007F5 RID: 2037
		[NonSerialized]
		public bool fire;

		// Token: 0x040007F6 RID: 2038
		[NonSerialized]
		public bool toggleCursor;

		// Token: 0x040007F7 RID: 2039
		[NonSerialized]
		public Transform activateParent;

		// Token: 0x040007F8 RID: 2040
		public ENet.Event netEvent;

		// Token: 0x040007F9 RID: 2041
		[NonSerialized]
		public ActionableButtons[] buttons;

		// Token: 0x040007FA RID: 2042
		private RenderTexture[] mapTextures;
	}
}
