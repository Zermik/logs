using System;
using System.Collections.Generic;
using BrokeProtocol.API;
using UnityEngine;

namespace BrokeProtocol.LiteDB
{
	// Token: 0x02000193 RID: 403
	public class Character
	{
		// Token: 0x1700017B RID: 379
		// (get) Token: 0x06000B6F RID: 2927 RVA: 0x00039838 File Offset: 0x00037A38
		// (set) Token: 0x06000B70 RID: 2928 RVA: 0x00039840 File Offset: 0x00037A40
		public int BankBalance { get; set; }

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x06000B71 RID: 2929 RVA: 0x00039849 File Offset: 0x00037A49
		// (set) Token: 0x06000B72 RID: 2930 RVA: 0x00039851 File Offset: 0x00037A51
		public string AppTransactions { get; set; } = string.Empty;

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x06000B73 RID: 2931 RVA: 0x0003985A File Offset: 0x00037A5A
		// (set) Token: 0x06000B74 RID: 2932 RVA: 0x00039862 File Offset: 0x00037A62
		public JobSave Job { get; set; } = new JobSave();

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x06000B75 RID: 2933 RVA: 0x0003986B File Offset: 0x00037A6B
		// (set) Token: 0x06000B76 RID: 2934 RVA: 0x00039873 File Offset: 0x00037A73
		public Vector3 Position { get; set; }

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x06000B77 RID: 2935 RVA: 0x0003987C File Offset: 0x00037A7C
		// (set) Token: 0x06000B78 RID: 2936 RVA: 0x00039884 File Offset: 0x00037A84
		public Quaternion Rotation { get; set; }

		// Token: 0x17000180 RID: 384
		// (get) Token: 0x06000B79 RID: 2937 RVA: 0x0003988D File Offset: 0x00037A8D
		// (set) Token: 0x06000B7A RID: 2938 RVA: 0x00039895 File Offset: 0x00037A95
		public string MapName { get; set; } = string.Empty;

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x06000B7B RID: 2939 RVA: 0x0003989E File Offset: 0x00037A9E
		// (set) Token: 0x06000B7C RID: 2940 RVA: 0x000398A6 File Offset: 0x00037AA6
		public int SkinIndex { get; set; }

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x06000B7D RID: 2941 RVA: 0x000398AF File Offset: 0x00037AAF
		// (set) Token: 0x06000B7E RID: 2942 RVA: 0x000398B7 File Offset: 0x00037AB7
		public int PlaceIndex { get; set; }

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x06000B7F RID: 2943 RVA: 0x000398C0 File Offset: 0x00037AC0
		// (set) Token: 0x06000B80 RID: 2944 RVA: 0x000398C8 File Offset: 0x00037AC8
		public int EquipableIndex { get; set; }

		// Token: 0x17000184 RID: 388
		// (get) Token: 0x06000B81 RID: 2945 RVA: 0x000398D1 File Offset: 0x00037AD1
		// (set) Token: 0x06000B82 RID: 2946 RVA: 0x000398D9 File Offset: 0x00037AD9
		public List<int> Attachments { get; set; } = new List<int>();

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x06000B83 RID: 2947 RVA: 0x000398E2 File Offset: 0x00037AE2
		// (set) Token: 0x06000B84 RID: 2948 RVA: 0x000398EA File Offset: 0x00037AEA
		public List<int> Wearables { get; set; } = new List<int>();

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x06000B85 RID: 2949 RVA: 0x000398F3 File Offset: 0x00037AF3
		// (set) Token: 0x06000B86 RID: 2950 RVA: 0x000398FB File Offset: 0x00037AFB
		public Dictionary<int, int> Items { get; set; } = new Dictionary<int, int>();

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x06000B87 RID: 2951 RVA: 0x00039904 File Offset: 0x00037B04
		// (set) Token: 0x06000B88 RID: 2952 RVA: 0x0003990C File Offset: 0x00037B0C
		public List<BindingSave> Bindings { get; set; } = new List<BindingSave>();

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x06000B89 RID: 2953 RVA: 0x00039915 File Offset: 0x00037B15
		// (set) Token: 0x06000B8A RID: 2954 RVA: 0x0003991D File Offset: 0x00037B1D
		public List<ApartmentSave> Apartments { get; set; } = new List<ApartmentSave>();

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x06000B8B RID: 2955 RVA: 0x00039926 File Offset: 0x00037B26
		// (set) Token: 0x06000B8C RID: 2956 RVA: 0x0003992E File Offset: 0x00037B2E
		public List<InjurySave> Injuries { get; set; } = new List<InjurySave>();

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x06000B8D RID: 2957 RVA: 0x00039937 File Offset: 0x00037B37
		// (set) Token: 0x06000B8E RID: 2958 RVA: 0x0003993F File Offset: 0x00037B3F
		public List<float> Stats { get; set; } = new List<float>();

		// Token: 0x1700018B RID: 395
		// (get) Token: 0x06000B8F RID: 2959 RVA: 0x00039948 File Offset: 0x00037B48
		// (set) Token: 0x06000B90 RID: 2960 RVA: 0x00039950 File Offset: 0x00037B50
		public float Health { get; set; }

		// Token: 0x1700018C RID: 396
		// (get) Token: 0x06000B91 RID: 2961 RVA: 0x00039959 File Offset: 0x00037B59
		// (set) Token: 0x06000B92 RID: 2962 RVA: 0x00039961 File Offset: 0x00037B61
		public bool KnockedOut { get; set; }

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x06000B93 RID: 2963 RVA: 0x0003996A File Offset: 0x00037B6A
		// (set) Token: 0x06000B94 RID: 2964 RVA: 0x00039972 File Offset: 0x00037B72
		public CustomData CustomData { get; set; } = new CustomData();
	}
}
