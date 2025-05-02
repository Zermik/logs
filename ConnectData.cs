using System;
using System.Collections.Generic;
using BrokeProtocol.API;
using ENet;
using UnityEngine;

namespace BrokeProtocol.Utility
{
	// Token: 0x02000039 RID: 57
	public class ConnectData
	{
		// Token: 0x06000075 RID: 117 RVA: 0x000041E0 File Offset: 0x000023E0
		public ConnectData(Peer connection)
		{
			this.connection = connection;
			this.connectionStatus = ConnectionStatus.Initial;
		}

		// Token: 0x04000126 RID: 294
		public readonly Peer connection;

		// Token: 0x04000127 RID: 295
		public string username;

		// Token: 0x04000128 RID: 296
		public int passwordHash;

		// Token: 0x04000129 RID: 297
		public int skinIndex;

		// Token: 0x0400012A RID: 298
		public byte[] wearableIndices;

		// Token: 0x0400012B RID: 299
		public int languageIndex;

		// Token: 0x0400012C RID: 300
		public string deviceID;

		// Token: 0x0400012D RID: 301
		public string profileURL;

		// Token: 0x0400012E RID: 302
		public RuntimePlatform platform;

		// Token: 0x0400012F RID: 303
		public byte[] cachedAssets;

		// Token: 0x04000130 RID: 304
		public bool cachedMap;

		// Token: 0x04000131 RID: 305
		public ConnectionStatus connectionStatus;

		// Token: 0x04000132 RID: 306
		public uint dataSent;

		// Token: 0x04000133 RID: 307
		public uint maxTransferRate = 100000U;

		// Token: 0x04000134 RID: 308
		public readonly Queue<TransferData> transferHistory = new Queue<TransferData>();

		// Token: 0x04000135 RID: 309
		public readonly byte[] payload = new byte[1357];

		// Token: 0x04000136 RID: 310
		public CustomData customData = new CustomData();
	}
}
