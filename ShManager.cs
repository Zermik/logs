using System;
using System.Collections.Generic;
using System.Threading;
using BrokeProtocol.Client.UI;
using BrokeProtocol.Collections;
using BrokeProtocol.Entities;
using BrokeProtocol.Required;
using BrokeProtocol.Utility;
using BrokeProtocol.Utility.Votes;
using ENet;
using UnityEngine;

namespace BrokeProtocol.Managers
{
	// Token: 0x0200018A RID: 394
	public class ShManager : MonoBehaviourSingleton<ShManager>
	{
		// Token: 0x17000157 RID: 343
		// (get) Token: 0x06000AAD RID: 2733 RVA: 0x0003458C File Offset: 0x0003278C
		public string VersionURL
		{
			get
			{
				if (!this.testVersion)
				{
					return this.versionURL;
				}
				return this.versionURL + "test";
			}
		}

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x06000AAE RID: 2734 RVA: 0x000345AD File Offset: 0x000327AD
		public string ServerlistSourceURL
		{
			get
			{
				if (!this.testVersion)
				{
					return this.serverlistSourceURL;
				}
				return this.serverlistSourceURL + "test";
			}
		}

		// Token: 0x17000159 RID: 345
		// (get) Token: 0x06000AAF RID: 2735 RVA: 0x000345CE File Offset: 0x000327CE
		public string NewsURL
		{
			get
			{
				if (!this.testVersion)
				{
					return this.newsURL;
				}
				return this.newsURL + "test";
			}
		}

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x06000AB0 RID: 2736 RVA: 0x000345EF File Offset: 0x000327EF
		public string MasterHostURL
		{
			get
			{
				if (!this.testVersion)
				{
					return this.masterHostURL;
				}
				return this.masterHostURL + "test";
			}
		}

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x06000AB1 RID: 2737 RVA: 0x00034610 File Offset: 0x00032810
		public string Version
		{
			get
			{
				if (!this.testVersion)
				{
					return Application.version;
				}
				return Application.version + "test";
			}
		}

		// Token: 0x1700015C RID: 348
		// (get) Token: 0x06000AB2 RID: 2738 RVA: 0x0003462F File Offset: 0x0003282F
		public ushort MasterPort
		{
			get
			{
				if (!this.testVersion)
				{
					return 5558;
				}
				return 5556;
			}
		}

		// Token: 0x06000AB3 RID: 2739 RVA: 0x00034644 File Offset: 0x00032844
		public Host StartHost(int connectionCount, ushort port, string hostName)
		{
			Host host = new Host();
			if (port > 0)
			{
				Address value = new Address
				{
					Port = port
				};
				if (!string.IsNullOrWhiteSpace(hostName) && !value.SetHost(hostName))
				{
					Util.Log("Invalid hostName", LogLevel.Error);
				}
				try
				{
					host.Create(new Address?(value), connectionCount);
					goto IL_9F;
				}
				catch (Exception ex)
				{
					string str = "Failed creating host: ";
					Exception ex2 = ex;
					Util.Log(str + ((ex2 != null) ? ex2.ToString() : null), LogLevel.Error);
					goto IL_9F;
				}
			}
			try
			{
				host.Create(null, connectionCount);
			}
			catch (Exception ex3)
			{
				string str2 = "Failed creating host: ";
				Exception ex4 = ex3;
				Util.Log(str2 + ((ex4 != null) ? ex4.ToString() : null), LogLevel.Error);
			}
			IL_9F:
			if (!host.IsSet)
			{
				Util.Log("Host not set", LogLevel.Error);
			}
			return host;
		}

		// Token: 0x06000AB4 RID: 2740 RVA: 0x00034720 File Offset: 0x00032920
		public bool Connect(Host host, string hostname, ushort port, out Peer peer)
		{
			Address address = new Address
			{
				Port = port
			};
			if (host.IsSet && address.SetHost(hostname))
			{
				peer = host.Connect(address, 0, (uint)this.Version.GetPrefabIndex());
				return true;
			}
			Util.Log("Connection error", LogLevel.Error);
			peer = default(Peer);
			return false;
		}

		// Token: 0x06000AB5 RID: 2741 RVA: 0x00034781 File Offset: 0x00032981
		public void ConfigurePeer(Peer peer)
		{
			peer.Timeout(32U, 10000U, 30000U);
			peer.ConfigureThrottle(5000U, 2U, 2U, 40U);
		}

		// Token: 0x06000AB6 RID: 2742 RVA: 0x000347A6 File Offset: 0x000329A6
		public void HostCleanup()
		{
			this.host.PreventConnections(true);
			this.host.Flush();
			this.host.Dispose();
			this.host = null;
		}

		// Token: 0x06000AB7 RID: 2743 RVA: 0x000347D1 File Offset: 0x000329D1
		public void SetTimeScale(float timeScale)
		{
			Time.timeScale = timeScale;
			Time.fixedDeltaTime = timeScale * this.baseFixedDelta;
		}

		// Token: 0x06000AB8 RID: 2744 RVA: 0x000347E8 File Offset: 0x000329E8
		protected override void Awake()
		{
			base.Awake();
			Paths.EnsureCache();
			this.forwardFriction["Untagged"] = Friction.forwardFriction;
			this.forwardFriction["Grass"] = Friction.forwardFrictionGrass;
			this.sidewaysFriction["Untagged"] = Friction.sidewaysFriction;
			this.sidewaysFriction["Grass"] = Friction.sidewaysFrictionGrass;
			Screen.sleepTimeout = -2;
			if (this.baseFixedDelta < 0f)
			{
				this.baseFixedDelta = Time.fixedDeltaTime;
			}
			this.SetTimeScale(1f);
			this.votes = new Vote[]
			{
				new VoteKick(this.voteKickPrefab),
				new VoteLaunch(this.voteLaunchPrefab)
			};
		}

		// Token: 0x06000AB9 RID: 2745 RVA: 0x000348A6 File Offset: 0x00032AA6
		protected void OnDestroy()
		{
			this.Cleanup();
		}

		// Token: 0x06000ABA RID: 2746 RVA: 0x000348B0 File Offset: 0x00032AB0
		public void Cleanup()
		{
			if (this.clManager && this.clManager.connection.State == PeerState.Connected)
			{
				this.clManager.connection.DisconnectNow(0U);
			}
			if (this.host != null)
			{
				this.HostCleanup();
			}
			EntityCollections.ClearAll();
		}

		// Token: 0x06000ABB RID: 2747 RVA: 0x00034904 File Offset: 0x00032B04
		public void InitializeClient(string host, ushort port)
		{
			SceneManager.isClient = true;
			Object.Destroy(this.svManager.aStarPath);
			Object.Destroy(this.svManager);
			Object.Destroy(this.menuObjects);
			Screen.sleepTimeout = -1;
			this.clManager.StartClient(host, port);
		}

		// Token: 0x06000ABC RID: 2748 RVA: 0x00034950 File Offset: 0x00032B50
		public void InitializeServer()
		{
			SceneManager.isServer = true;
			AudioListener.volume = 0f;
			Application.targetFrameRate = 72;
			this.clManager.DestroyMenu("Default");
			Object.Destroy(this.clManager);
			Object.Destroy(this.clientObjects);
			Object.Destroy(this.menuObjects);
			Thread.Sleep(1000);
			this.svManager.StartServer();
		}

		// Token: 0x04000840 RID: 2112
		public PhysicsMaterial noFrictionMaterial;

		// Token: 0x04000841 RID: 2113
		public PhysicsMaterial highFrictionMaterial;

		// Token: 0x04000842 RID: 2114
		public ClManager clManager;

		// Token: 0x04000843 RID: 2115
		public SvManager svManager;

		// Token: 0x04000844 RID: 2116
		[SerializeField]
		private VoteKickPanel voteKickPrefab;

		// Token: 0x04000845 RID: 2117
		[SerializeField]
		private VoteLaunchPanel voteLaunchPrefab;

		// Token: 0x04000846 RID: 2118
		public Host host;

		// Token: 0x04000847 RID: 2119
		[SerializeField]
		private bool testVersion;

		// Token: 0x04000848 RID: 2120
		private readonly string versionURL = "https://brokeprotocol.com/version";

		// Token: 0x04000849 RID: 2121
		private readonly string serverlistSourceURL = "https://brokeprotocol.com/serverlistsource";

		// Token: 0x0400084A RID: 2122
		private readonly string newsURL = "https://brokeprotocol.com/news";

		// Token: 0x0400084B RID: 2123
		private readonly string masterHostURL = "https://brokeprotocol.com/master";

		// Token: 0x0400084C RID: 2124
		[SerializeField]
		private GameObject clientObjects;

		// Token: 0x0400084D RID: 2125
		public GameObject menuObjects;

		// Token: 0x0400084E RID: 2126
		public ShWearable[] nullWearable;

		// Token: 0x0400084F RID: 2127
		public ShAttachment[] nullAttachment;

		// Token: 0x04000850 RID: 2128
		public ShEquipable surrender;

		// Token: 0x04000851 RID: 2129
		public ShWeapon hands;

		// Token: 0x04000852 RID: 2130
		public ShItem bomb;

		// Token: 0x04000853 RID: 2131
		public ShItem money;

		// Token: 0x04000854 RID: 2132
		public ShItem toolkit;

		// Token: 0x04000855 RID: 2133
		public ShItem lockpick;

		// Token: 0x04000856 RID: 2134
		public ShEquipable defibrillator;

		// Token: 0x04000857 RID: 2135
		public ShEquipable healthPack;

		// Token: 0x04000858 RID: 2136
		public ShEquipable extinguisher;

		// Token: 0x04000859 RID: 2137
		public Vote[] votes;

		// Token: 0x0400085A RID: 2138
		private float baseFixedDelta = -1f;

		// Token: 0x0400085B RID: 2139
		public DamageType[] damageTypes;

		// Token: 0x0400085C RID: 2140
		public Dictionary<string, WheelFrictionCurve> forwardFriction = new Dictionary<string, WheelFrictionCurve>();

		// Token: 0x0400085D RID: 2141
		public Dictionary<string, WheelFrictionCurve> sidewaysFriction = new Dictionary<string, WheelFrictionCurve>();
	}
}
