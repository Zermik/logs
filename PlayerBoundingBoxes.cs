using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BrokeProtocol.Entities;
using HarmonyLib;
using UnityEngine;

namespace GlowingPlayersPlugin
{
	// Token: 0x02000002 RID: 2
	[BepInPlugin("com.yourname.playerboxes", "Player Bounding Boxes", "1.0.5")]
	public class PlayerBoundingBoxes : BaseUnityPlugin
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		private void Awake()
		{
			base.Logger.LogInfo("[PlayerBoxes] Awake");
			this.boxesEnabled = base.Config.Bind<bool>("General", "EnableBoxes", true, "Enable or disable player bounding boxes");
			this.toggleBoxesKey = base.Config.Bind<KeyCode>("General", "ToggleBoxesKey", 292, "Key to toggle bounding boxes");
			this.toggleMenuKey = base.Config.Bind<KeyCode>("General", "ToggleMenuKey", 291, "Key to show/hide the settings menu");
			PlayerBoundingBoxes.whiteTexture = new Texture2D(1, 1);
			PlayerBoundingBoxes.whiteTexture.SetPixel(0, 0, Color.white);
			PlayerBoundingBoxes.whiteTexture.Apply();
			base.Logger.LogInfo("[PlayerBoxes] Config loaded");
		}

		// Token: 0x06000002 RID: 2 RVA: 0x0000210F File Offset: 0x0000030F
		private void OnDestroy()
		{
			if (PlayerBoundingBoxes.whiteTexture != null)
			{
				Object.Destroy(PlayerBoundingBoxes.whiteTexture);
			}
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002128 File Offset: 0x00000328
		private void UpdateHumanPlayers()
		{
			if (Time.time < this.nextPlayerUpdate)
			{
				return;
			}
			this.nextPlayerUpdate = Time.time + 1f;
			this.humanPlayers.Clear();
			ClPlayer[] array = Object.FindObjectsByType<ClPlayer>(0);
			if (this.localPlayer == null)
			{
				foreach (ClPlayer clPlayer in array)
				{
					if (!(clPlayer == null))
					{
						object value = AccessTools.Field(typeof(ClPlayer), "player").GetValue(clPlayer);
						if (value != null && !(value.GetType().Name != "ShPlayer"))
						{
							FieldInfo fieldInfo = AccessTools.Field(typeof(ClPlayer), "isMain");
							if (!(fieldInfo == null) && (bool)fieldInfo.GetValue(clPlayer))
							{
								this.localPlayer = clPlayer;
								break;
							}
						}
					}
				}
			}
			foreach (ClPlayer clPlayer2 in array)
			{
				if (!(clPlayer2 == null) && !(clPlayer2 == this.localPlayer))
				{
					object value2 = AccessTools.Field(typeof(ClPlayer), "player").GetValue(clPlayer2);
					if (value2 != null && !(value2.GetType().Name != "ShPlayer"))
					{
						FieldInfo fieldInfo2 = AccessTools.Field(value2.GetType(), "isHuman");
						if (fieldInfo2 == null)
						{
							base.Logger.LogWarning("[PlayerBoxes] Field isHuman not found on ShPlayer");
						}
						else if ((bool)fieldInfo2.GetValue(value2))
						{
							this.humanPlayers.Add(clPlayer2);
						}
					}
				}
			}
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000022CC File Offset: 0x000004CC
		private void OnGUI()
		{
			Event current = Event.current;
			if (current.type == 4)
			{
				if (current.keyCode == this.toggleBoxesKey.Value)
				{
					this.showBoxes = !this.showBoxes;
					base.Logger.LogInfo(string.Format("[PlayerBoxes] showBoxes = {0}", this.showBoxes));
					current.Use();
				}
				else if (current.keyCode == this.toggleMenuKey.Value)
				{
					this.showMenu = !this.showMenu;
					base.Logger.LogInfo(string.Format("[PlayerBoxes] showMenu = {0}", this.showMenu));
					current.Use();
				}
			}
			if (this.showMenu)
			{
				this.menuRect = GUILayout.Window(654321, this.menuRect, new GUI.WindowFunction(this.DrawMenuWindow), "Player Boxes Settings", Array.Empty<GUILayoutOption>());
			}
			if (!this.boxesEnabled.Value || !this.showBoxes)
			{
				return;
			}
			this.UpdateHumanPlayers();
			this.DrawAllBoxes();
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000023D4 File Offset: 0x000005D4
		private void DrawMenuWindow(int id)
		{
			GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
			this.boxesEnabled.Value = GUILayout.Toggle(this.boxesEnabled.Value, "Enable Bounding Boxes", Array.Empty<GUILayoutOption>());
			GUILayout.Space(10f);
			GUILayout.Label(string.Format("Toggle Boxes Key: {0}", this.toggleBoxesKey.Value), Array.Empty<GUILayoutOption>());
			GUILayout.Label(string.Format("Toggle Menu Key: {0}", this.toggleMenuKey.Value), Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("Close", Array.Empty<GUILayoutOption>()))
			{
				this.showMenu = false;
			}
			GUILayout.EndVertical();
			GUI.DragWindow();
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002484 File Offset: 0x00000684
		private void DrawAllBoxes()
		{
			Camera camera = Camera.main;
			if (camera == null)
			{
				camera = Camera.current;
			}
			if (camera == null && Camera.allCamerasCount > 0)
			{
				camera = Camera.allCameras[0];
			}
			if (camera == null)
			{
				base.Logger.LogInfo("[PlayerBoxes] No camera found");
				return;
			}
			foreach (ClPlayer clPlayer in this.humanPlayers)
			{
				if (!(clPlayer == null))
				{
					SkinnedMeshRenderer skinnedMeshRenderer = clPlayer.skinnedMeshRenderer;
					if (!(skinnedMeshRenderer == null))
					{
						Bounds bounds = skinnedMeshRenderer.bounds;
						Vector3[] array = new Vector3[]
						{
							new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
							new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
							new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
							new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
							new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
							new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
							new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
							new Vector3(bounds.max.x, bounds.max.y, bounds.max.z)
						};
						float num = float.MaxValue;
						float num2 = float.MaxValue;
						float num3 = float.MinValue;
						float num4 = float.MinValue;
						foreach (Vector3 vector in array)
						{
							Vector3 vector2 = camera.WorldToScreenPoint(vector);
							if (vector2.z >= 0f)
							{
								float x = vector2.x;
								float num5 = (float)Screen.height - vector2.y;
								num = Mathf.Min(num, x);
								num2 = Mathf.Min(num2, num5);
								num3 = Mathf.Max(num3, x);
								num4 = Mathf.Max(num4, num5);
							}
						}
						if (num3 >= num && num4 >= num2)
						{
							Rect rect;
							rect..ctor(num, num2, num3 - num, num4 - num2);
							this.DrawOutline(rect, Color.red);
						}
					}
				}
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000027A0 File Offset: 0x000009A0
		private void DrawOutline(Rect rect, Color color)
		{
			Color color2 = GUI.color;
			GUI.color = color;
			GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 1f), PlayerBoundingBoxes.whiteTexture);
			GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height, rect.width, 1f), PlayerBoundingBoxes.whiteTexture);
			GUI.DrawTexture(new Rect(rect.x, rect.y, 1f, rect.height), PlayerBoundingBoxes.whiteTexture);
			GUI.DrawTexture(new Rect(rect.x + rect.width, rect.y, 1f, rect.height), PlayerBoundingBoxes.whiteTexture);
			GUI.color = color2;
		}

		// Token: 0x04000001 RID: 1
		private ConfigEntry<bool> boxesEnabled;

		// Token: 0x04000002 RID: 2
		private ConfigEntry<KeyCode> toggleBoxesKey;

		// Token: 0x04000003 RID: 3
		private ConfigEntry<KeyCode> toggleMenuKey;

		// Token: 0x04000004 RID: 4
		private bool showBoxes = true;

		// Token: 0x04000005 RID: 5
		private bool showMenu;

		// Token: 0x04000006 RID: 6
		private Rect menuRect = new Rect(50f, 50f, 240f, 120f);

		// Token: 0x04000007 RID: 7
		private static Texture2D whiteTexture;

		// Token: 0x04000008 RID: 8
		private List<ClPlayer> humanPlayers = new List<ClPlayer>();

		// Token: 0x04000009 RID: 9
		private float nextPlayerUpdate;

		// Token: 0x0400000A RID: 10
		private const float PLAYER_UPDATE_INTERVAL = 1f;

		// Token: 0x0400000B RID: 11
		private ClPlayer localPlayer;
	}
}
