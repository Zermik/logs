using System;
using System.Collections.Generic;
using System.Linq;
using BrokeProtocol.Entities;
using BrokeProtocol.Managers;
using BrokeProtocol.Required;
using BrokeProtocol.Utility;
using UnityEngine;

namespace BrokeProtocol.Client.UI
{
	// Token: 0x02000320 RID: 800
	public class PlayerRenderer : MonoBehaviour
	{
		// Token: 0x06001004 RID: 4100 RVA: 0x0004ABAC File Offset: 0x00048DAC
		public void Refresh(int playerIndex, IEnumerable<ShWearable> wearableList)
		{
			if (this.playerIndex != playerIndex)
			{
				this.playerIndex = playerIndex;
				this.DestroyProxy();
				ShPlayer entity = MonoBehaviourSingleton<SceneManager>.Instance.GetEntity<ShPlayer>(playerIndex);
				float num = entity.clPlayer.skinnedMeshRenderer.localBounds.extents.y * entity.mainT.lossyScale.y;
				this.cam.orthographicSize = num * 1.1f;
				this.proxy = Object.Instantiate<ShPlayer>(entity, this.mainT.position - this.mainT.up * num + 0.5f * this.cam.farClipPlane * this.mainT.forward, Quaternion.LookRotation(-this.mainT.forward, this.mainT.up), this.mainT);
				this.proxy.name = "Proxy";
				this.proxy.go.layer = 6;
			}
			using (IEnumerator<ShWearable> enumerator = wearableList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ShWearable w = enumerator.Current;
					if (!this.wearables.Any((ShWearable x) => x && x.name == w.name))
					{
						int type = (int)w.type;
						if (this.wearables[type])
						{
							Object.Destroy(this.wearables[type].gameObject);
						}
						ShWearable shWearable = Object.Instantiate<ShWearable>(MonoBehaviourSingleton<SceneManager>.Instance.GetEntity<ShWearable>(w.index), this.proxy.mainT, false);
						shWearable.go.layer = 6;
						shWearable.name = w.name;
						this.wearables[type] = shWearable;
						if (shWearable.skinnedMesh)
						{
							this.proxy.clPlayer.AssignToActor(shWearable.skinnedMesh);
						}
					}
				}
			}
		}

		// Token: 0x06001005 RID: 4101 RVA: 0x0004ADC8 File Offset: 0x00048FC8
		protected void Update()
		{
			if (this.proxy)
			{
				this.proxy.mainT.Rotate(new Vector3(0f, Time.deltaTime * 25f, 0f), Space.Self);
			}
		}

		// Token: 0x06001006 RID: 4102 RVA: 0x0004AE02 File Offset: 0x00049002
		private void OnDestroy()
		{
			this.DestroyProxy();
		}

		// Token: 0x06001007 RID: 4103 RVA: 0x0004AE0A File Offset: 0x0004900A
		private void DestroyProxy()
		{
			if (this.proxy)
			{
				Object.Destroy(this.proxy.go);
				this.proxy = null;
			}
			Array.Clear(this.wearables, 0, this.wearables.Length);
		}

		// Token: 0x04001398 RID: 5016
		[SerializeField]
		private Transform mainT;

		// Token: 0x04001399 RID: 5017
		public Camera cam;

		// Token: 0x0400139A RID: 5018
		private int playerIndex;

		// Token: 0x0400139B RID: 5019
		private ShPlayer proxy;

		// Token: 0x0400139C RID: 5020
		private readonly ShWearable[] wearables = new ShWearable[Enum.GetNames(typeof(WearableType)).Length];
	}
}
