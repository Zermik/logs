using System;
using System.Text;
using BrokeProtocol.Managers;
using BrokeProtocol.Required;
using BrokeProtocol.Utility;
using UnityEngine;

namespace BrokeProtocol.Entities
{
	// Token: 0x02000133 RID: 307
	public class ShWearable : ShItem
	{
		// Token: 0x06000696 RID: 1686 RVA: 0x00021B60 File Offset: 0x0001FD60
		public override void Initialize()
		{
			Mesh mesh;
			this.go.GetMesh(out mesh);
			base.Initialize();
		}

		// Token: 0x06000697 RID: 1687 RVA: 0x00021B84 File Offset: 0x0001FD84
		public override StringBuilder GetTooltip()
		{
			StringBuilder tooltip = base.GetTooltip();
			if (this.protection > 0f)
			{
				tooltip.Append("Armor: ").AppendLine(this.protection.ToString());
			}
			if (this.capacity > 0f)
			{
				tooltip.Append("Capacity: ").AppendLine(this.capacity.ToString());
			}
			return tooltip;
		}

		// Token: 0x06000698 RID: 1688 RVA: 0x00021BEC File Offset: 0x0001FDEC
		public override void InitializeOnPlayer(ShPlayer player)
		{
			base.InitializeOnPlayer(player);
			this.mainT.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			if (this.skinnedMesh && SceneManager.isClient)
			{
				player.clPlayer.AssignToActor(this.skinnedMesh);
				if (player.clPlayer.SpecMain)
				{
					this.skinnedMesh.enabled = !player.HiddenInterior;
				}
			}
		}

		// Token: 0x0400066E RID: 1646
		public CharacterType characterType;

		// Token: 0x0400066F RID: 1647
		public SkinnedMeshRenderer skinnedMesh;

		// Token: 0x04000670 RID: 1648
		public WearableType type;

		// Token: 0x04000671 RID: 1649
		public float protection;

		// Token: 0x04000672 RID: 1650
		public float capacity;
	}
}
