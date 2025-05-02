using System;

namespace BrokeProtocol.API
{
	// Token: 0x02000350 RID: 848
	public static class GameSourceEvent
	{
		// Token: 0x04001465 RID: 5221
		public const int ManagerStart = 100;

		// Token: 0x04001466 RID: 5222
		public const int ManagerUpdate = 101;

		// Token: 0x04001467 RID: 5223
		public const int ManagerFixedUpdate = 102;

		// Token: 0x04001468 RID: 5224
		public const int ManagerCustomPacket = 103;

		// Token: 0x04001469 RID: 5225
		public const int ManagerTryLogin = 104;

		// Token: 0x0400146A RID: 5226
		public const int ManagerTryRegister = 105;

		// Token: 0x0400146B RID: 5227
		public const int ManagerSave = 106;

		// Token: 0x0400146C RID: 5228
		public const int ManagerLoad = 107;

		// Token: 0x0400146D RID: 5229
		public const int ManagerReadGroups = 108;

		// Token: 0x0400146E RID: 5230
		public const int ManagerPlayerLoaded = 109;

		// Token: 0x0400146F RID: 5231
		public const int ManagerPrepareMap = 110;

		// Token: 0x04001470 RID: 5232
		public const int ManagerTryDelete = 111;

		// Token: 0x04001471 RID: 5233
		public const int EntityInitialize = 200;

		// Token: 0x04001472 RID: 5234
		public const int EntityDestroy = 201;

		// Token: 0x04001473 RID: 5235
		public const int EntityAddItem = 202;

		// Token: 0x04001474 RID: 5236
		public const int EntityRemoveItem = 203;

		// Token: 0x04001475 RID: 5237
		public const int EntityRespawn = 204;

		// Token: 0x04001476 RID: 5238
		public const int EntityTransferItem = 205;

		// Token: 0x04001477 RID: 5239
		public const int EntitySpawn = 206;

		// Token: 0x04001478 RID: 5240
		public const int EntitySecurityTrigger = 207;

		// Token: 0x04001479 RID: 5241
		public const int EntityNewSector = 208;

		// Token: 0x0400147A RID: 5242
		public const int EntitySameSector = 209;

		// Token: 0x0400147B RID: 5243
		public const int EntityMissileLocked = 210;

		// Token: 0x0400147C RID: 5244
		public const int EntitySetParent = 211;

		// Token: 0x0400147D RID: 5245
		public const int DamageableDamage = 900;

		// Token: 0x0400147E RID: 5246
		public const int VoxelDamage = 1000;

		// Token: 0x0400147F RID: 5247
		public const int MountableSpawn = 600;

		// Token: 0x04001480 RID: 5248
		public const int DestroyableDamage = 300;

		// Token: 0x04001481 RID: 5249
		public const int DestroyableDeath = 301;

		// Token: 0x04001482 RID: 5250
		public const int DestroyableDestroySelf = 302;

		// Token: 0x04001483 RID: 5251
		public const int DestroyableSpawn = 303;

		// Token: 0x04001484 RID: 5252
		public const int DestroyableHeal = 304;

		// Token: 0x04001485 RID: 5253
		public const int PhysicalSpawn = 700;

		// Token: 0x04001486 RID: 5254
		public const int MovableDamage = 400;

		// Token: 0x04001487 RID: 5255
		public const int MovableDeath = 401;

		// Token: 0x04001488 RID: 5256
		public const int MovableRespawn = 402;

		// Token: 0x04001489 RID: 5257
		public const int MovableSpawn = 403;

		// Token: 0x0400148A RID: 5258
		public const int PlayerInitialize = 500;

		// Token: 0x0400148B RID: 5259
		public const int PlayerDestroy = 501;

		// Token: 0x0400148C RID: 5260
		public const int PlayerCommand = 502;

		// Token: 0x0400148D RID: 5261
		public const int PlayerChatGlobal = 503;

		// Token: 0x0400148E RID: 5262
		public const int PlayerChatLocal = 504;

		// Token: 0x0400148F RID: 5263
		public const int PlayerChatVoice = 505;

		// Token: 0x04001490 RID: 5264
		public const int PlayerSetChatMode = 506;

		// Token: 0x04001491 RID: 5265
		public const int PlayerSetChatChannel = 507;

		// Token: 0x04001492 RID: 5266
		public const int PlayerBuyApartment = 508;

		// Token: 0x04001493 RID: 5267
		public const int PlayerSellApartment = 509;

		// Token: 0x04001494 RID: 5268
		public const int PlayerInvite = 510;

		// Token: 0x04001495 RID: 5269
		public const int PlayerKickOut = 511;

		// Token: 0x04001496 RID: 5270
		public const int PlayerRespawn = 512;

		// Token: 0x04001497 RID: 5271
		public const int PlayerReward = 513;

		// Token: 0x04001498 RID: 5272
		public const int PlayerCollect = 514;

		// Token: 0x04001499 RID: 5273
		public const int PlayerStopInventory = 515;

		// Token: 0x0400149A RID: 5274
		public const int PlayerViewInventory = 516;

		// Token: 0x0400149B RID: 5275
		public const int PlayerKick = 517;

		// Token: 0x0400149C RID: 5276
		public const int PlayerBan = 518;

		// Token: 0x0400149D RID: 5277
		public const int PlayerAddItem = 519;

		// Token: 0x0400149E RID: 5278
		public const int PlayerRemoveItem = 520;

		// Token: 0x0400149F RID: 5279
		public const int PlayerRemoveItemsDeath = 521;

		// Token: 0x040014A0 RID: 5280
		public const int PlayerLoad = 522;

		// Token: 0x040014A1 RID: 5281
		public const int PlayerSave = 523;

		// Token: 0x040014A2 RID: 5282
		public const int PlayerInjury = 524;

		// Token: 0x040014A3 RID: 5283
		public const int PlayerRestrain = 525;

		// Token: 0x040014A4 RID: 5284
		public const int PlayerUnrestrain = 526;

		// Token: 0x040014A5 RID: 5285
		public const int PlayerServerInfo = 527;

		// Token: 0x040014A6 RID: 5286
		public const int PlayerDisplayName = 528;

		// Token: 0x040014A7 RID: 5287
		public const int PlayerEnterDoor = 529;

		// Token: 0x040014A8 RID: 5288
		public const int PlayerFollower = 530;

		// Token: 0x040014A9 RID: 5289
		public const int PlayerOptionAction = 531;

		// Token: 0x040014AA RID: 5290
		public const int PlayerSubmitInput = 532;

		// Token: 0x040014AB RID: 5291
		public const int PlayerReady = 533;

		// Token: 0x040014AC RID: 5292
		public const int PlayerPoint = 534;

		// Token: 0x040014AD RID: 5293
		public const int PlayerAlert = 535;

		// Token: 0x040014AE RID: 5294
		public const int PlayerMinigameFinished = 536;

		// Token: 0x040014AF RID: 5295
		public const int PlayerDestroySelf = 537;

		// Token: 0x040014B0 RID: 5296
		public const int PlayerTransferItem = 538;

		// Token: 0x040014B1 RID: 5297
		public const int PlayerMenuClosed = 539;

		// Token: 0x040014B2 RID: 5298
		public const int PlayerSecurityPanel = 540;

		// Token: 0x040014B3 RID: 5299
		public const int PlayerVideoPanel = 541;

		// Token: 0x040014B4 RID: 5300
		public const int PlayerTextPanelButton = 542;

		// Token: 0x040014B5 RID: 5301
		public const int PlayerSetEquipable = 543;

		// Token: 0x040014B6 RID: 5302
		public const int PlayerCrackStart = 544;

		// Token: 0x040014B7 RID: 5303
		public const int PlayerMount = 545;

		// Token: 0x040014B8 RID: 5304
		public const int PlayerDismount = 546;

		// Token: 0x040014B9 RID: 5305
		public const int PlayerSpawn = 547;

		// Token: 0x040014BA RID: 5306
		public const int PlayerPlaceItem = 548;

		// Token: 0x040014BB RID: 5307
		public const int PlayerResetAI = 549;

		// Token: 0x040014BC RID: 5308
		public const int PlayerSetWearable = 550;

		// Token: 0x040014BD RID: 5309
		public const int PlayerRestrainOther = 551;

		// Token: 0x040014BE RID: 5310
		public const int PlayerDeposit = 552;

		// Token: 0x040014BF RID: 5311
		public const int PlayerWithdraw = 553;

		// Token: 0x040014C0 RID: 5312
		public const int PlayerTryGetJob = 554;

		// Token: 0x040014C1 RID: 5313
		public const int PlayerBomb = 555;

		// Token: 0x040014C2 RID: 5314
		public const int PlayerRepair = 556;

		// Token: 0x040014C3 RID: 5315
		public const int PlayerLockpick = 557;

		// Token: 0x040014C4 RID: 5316
		public const int PlayerConsume = 558;

		// Token: 0x040014C5 RID: 5317
		public const int PlayerTransferShop = 559;

		// Token: 0x040014C6 RID: 5318
		public const int PlayerTransferTrade = 560;

		// Token: 0x040014C7 RID: 5319
		public const int PlayerNewSector = 561;

		// Token: 0x040014C8 RID: 5320
		public const int PlayerSameSector = 562;

		// Token: 0x040014C9 RID: 5321
		public const int PlayerFire = 563;

		// Token: 0x040014CA RID: 5322
		public const int PlayerSetParent = 564;

		// Token: 0x040014CB RID: 5323
		public const int PlayerDamage = 565;

		// Token: 0x040014CC RID: 5324
		public const int PlayerDeath = 566;

		// Token: 0x040014CD RID: 5325
		public const int PlayerUpdateTextDisplay = 567;

		// Token: 0x040014CE RID: 5326
		public const int PlayerSetJob = 568;

		// Token: 0x040014CF RID: 5327
		public const int PlayerPark = 569;

		// Token: 0x040014D0 RID: 5328
		public const int PlayerTow = 570;

		// Token: 0x040014D1 RID: 5329
		public const int TransportInitialize = 800;

		// Token: 0x040014D2 RID: 5330
		public const int TransportDestroy = 801;

		// Token: 0x040014D3 RID: 5331
		public const int TransportAddItem = 802;

		// Token: 0x040014D4 RID: 5332
		public const int TransportRemoveItem = 803;

		// Token: 0x040014D5 RID: 5333
		public const int TransportRespawn = 804;

		// Token: 0x040014D6 RID: 5334
		public const int TransportTransferItem = 805;

		// Token: 0x040014D7 RID: 5335
		public const int TransportSpawn = 806;

		// Token: 0x040014D8 RID: 5336
		public const int TransportSecurityTrigger = 807;

		// Token: 0x040014D9 RID: 5337
		public const int TransportNewSector = 808;

		// Token: 0x040014DA RID: 5338
		public const int TransportSameSector = 809;

		// Token: 0x040014DB RID: 5339
		public const int TransportMissileLocked = 810;

		// Token: 0x040014DC RID: 5340
		public const int TransportSetParent = 811;

		// Token: 0x040014DD RID: 5341
		public const int TransportDamage = 812;

		// Token: 0x040014DE RID: 5342
		public const int TransportDeath = 813;

		// Token: 0x040014DF RID: 5343
		public const int TransportDestroySelf = 814;

		// Token: 0x040014E0 RID: 5344
		public const int TransportHeal = 815;
	}
}
