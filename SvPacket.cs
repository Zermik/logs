using System;

namespace BrokeProtocol.Utility.Networking
{
	// Token: 0x0200007C RID: 124
	public enum SvPacket : byte
	{
		// Token: 0x040003D4 RID: 980
		Ready,
		// Token: 0x040003D5 RID: 981
		Cache,
		// Token: 0x040003D6 RID: 982
		OldPacket0,
		// Token: 0x040003D7 RID: 983
		Register,
		// Token: 0x040003D8 RID: 984
		Login,
		// Token: 0x040003D9 RID: 985
		Delete,
		// Token: 0x040003DA RID: 986
		Loaded,
		// Token: 0x040003DB RID: 987
		TransferView,
		// Token: 0x040003DC RID: 988
		TransferShop,
		// Token: 0x040003DD RID: 989
		TransferTrade,
		// Token: 0x040003DE RID: 990
		View,
		// Token: 0x040003DF RID: 991
		Drop,
		// Token: 0x040003E0 RID: 992
		TradeRequest,
		// Token: 0x040003E1 RID: 993
		FinalizeTrade,
		// Token: 0x040003E2 RID: 994
		ConfirmTrade,
		// Token: 0x040003E3 RID: 995
		StopInventory,
		// Token: 0x040003E4 RID: 996
		CrackStart,
		// Token: 0x040003E5 RID: 997
		MinigameStop,
		// Token: 0x040003E6 RID: 998
		HackingProbe,
		// Token: 0x040003E7 RID: 999
		HackingMark,
		// Token: 0x040003E8 RID: 1000
		CrackAttempt,
		// Token: 0x040003E9 RID: 1001
		Reload,
		// Token: 0x040003EA RID: 1002
		AltFire,
		// Token: 0x040003EB RID: 1003
		Mount,
		// Token: 0x040003EC RID: 1004
		GetEntityValue,
		// Token: 0x040003ED RID: 1005
		BuyTransport,
		// Token: 0x040003EE RID: 1006
		BuyApartment,
		// Token: 0x040003EF RID: 1007
		SellApartment,
		// Token: 0x040003F0 RID: 1008
		ShowHealth,
		// Token: 0x040003F1 RID: 1009
		Shop,
		// Token: 0x040003F2 RID: 1010
		GetJob,
		// Token: 0x040003F3 RID: 1011
		QuitJob,
		// Token: 0x040003F4 RID: 1012
		BuyFurniture,
		// Token: 0x040003F5 RID: 1013
		Dismount,
		// Token: 0x040003F6 RID: 1014
		Free,
		// Token: 0x040003F7 RID: 1015
		TriggerEvent,
		// Token: 0x040003F8 RID: 1016
		EntityAction,
		// Token: 0x040003F9 RID: 1017
		InventoryAction,
		// Token: 0x040003FA RID: 1018
		SelfAction,
		// Token: 0x040003FB RID: 1019
		UpdateInput,
		// Token: 0x040003FC RID: 1020
		UpdatePlayer,
		// Token: 0x040003FD RID: 1021
		UpdatePlayerWater,
		// Token: 0x040003FE RID: 1022
		UpdatePlayerOffset,
		// Token: 0x040003FF RID: 1023
		UpdateMount,
		// Token: 0x04000400 RID: 1024
		UpdateMountWater,
		// Token: 0x04000401 RID: 1025
		UpdateMountOffset,
		// Token: 0x04000402 RID: 1026
		UpdateRotation,
		// Token: 0x04000403 RID: 1027
		Jump,
		// Token: 0x04000404 RID: 1028
		Embark,
		// Token: 0x04000405 RID: 1029
		Disembark,
		// Token: 0x04000406 RID: 1030
		ProgressAction,
		// Token: 0x04000407 RID: 1031
		ProcessAction,
		// Token: 0x04000408 RID: 1032
		SetSiren,
		// Token: 0x04000409 RID: 1033
		TrySetEquipable,
		// Token: 0x0400040A RID: 1034
		StartVote,
		// Token: 0x0400040B RID: 1035
		VoteYes,
		// Token: 0x0400040C RID: 1036
		VoteNo,
		// Token: 0x0400040D RID: 1037
		KillSelf,
		// Token: 0x0400040E RID: 1038
		Crouch,
		// Token: 0x0400040F RID: 1039
		Collect,
		// Token: 0x04000410 RID: 1040
		Consume,
		// Token: 0x04000411 RID: 1041
		OldPacket9,
		// Token: 0x04000412 RID: 1042
		Kick,
		// Token: 0x04000413 RID: 1043
		Ban,
		// Token: 0x04000414 RID: 1044
		Restrain,
		// Token: 0x04000415 RID: 1045
		Teleport,
		// Token: 0x04000416 RID: 1046
		Summon,
		// Token: 0x04000417 RID: 1047
		RemoveJob,
		// Token: 0x04000418 RID: 1048
		OldPacket7,
		// Token: 0x04000419 RID: 1049
		OldPacket8,
		// Token: 0x0400041A RID: 1050
		Follower,
		// Token: 0x0400041B RID: 1051
		Invite,
		// Token: 0x0400041C RID: 1052
		KickOut,
		// Token: 0x0400041D RID: 1053
		Point,
		// Token: 0x0400041E RID: 1054
		Alert,
		// Token: 0x0400041F RID: 1055
		SellFurniture,
		// Token: 0x04000420 RID: 1056
		ServiceCall,
		// Token: 0x04000421 RID: 1057
		SetWearable,
		// Token: 0x04000422 RID: 1058
		Deploy,
		// Token: 0x04000423 RID: 1059
		OldPacket1,
		// Token: 0x04000424 RID: 1060
		OldPacket4,
		// Token: 0x04000425 RID: 1061
		Fire,
		// Token: 0x04000426 RID: 1062
		RequestServerInfo,
		// Token: 0x04000427 RID: 1063
		OldPacket5,
		// Token: 0x04000428 RID: 1064
		OldPacket6,
		// Token: 0x04000429 RID: 1065
		BanAccount,
		// Token: 0x0400042A RID: 1066
		UnbanIP,
		// Token: 0x0400042B RID: 1067
		DropEquipable,
		// Token: 0x0400042C RID: 1068
		DropItem,
		// Token: 0x0400042D RID: 1069
		Heal,
		// Token: 0x0400042E RID: 1070
		Park,
		// Token: 0x0400042F RID: 1071
		Tow,
		// Token: 0x04000430 RID: 1072
		CheckHitscan,
		// Token: 0x04000431 RID: 1073
		CheckBallistic,
		// Token: 0x04000432 RID: 1074
		Bind,
		// Token: 0x04000433 RID: 1075
		Unbind,
		// Token: 0x04000434 RID: 1076
		BindAttachment,
		// Token: 0x04000435 RID: 1077
		UnbindAttachment,
		// Token: 0x04000436 RID: 1078
		SetAttachment,
		// Token: 0x04000437 RID: 1079
		UseBind,
		// Token: 0x04000438 RID: 1080
		EnterDoor,
		// Token: 0x04000439 RID: 1081
		Disarm,
		// Token: 0x0400043A RID: 1082
		Initialized,
		// Token: 0x0400043B RID: 1083
		TransportState,
		// Token: 0x0400043C RID: 1084
		Apps,
		// Token: 0x0400043D RID: 1085
		AppContacts,
		// Token: 0x0400043E RID: 1086
		AppBlocked,
		// Token: 0x0400043F RID: 1087
		AppCalls,
		// Token: 0x04000440 RID: 1088
		AppInbox,
		// Token: 0x04000441 RID: 1089
		AppServices,
		// Token: 0x04000442 RID: 1090
		AppDeposit,
		// Token: 0x04000443 RID: 1091
		AppWithdraw,
		// Token: 0x04000444 RID: 1092
		AppRadio,
		// Token: 0x04000445 RID: 1093
		AppMessage,
		// Token: 0x04000446 RID: 1094
		AppAddContact,
		// Token: 0x04000447 RID: 1095
		AppRemoveContact,
		// Token: 0x04000448 RID: 1096
		AppAddBlocked,
		// Token: 0x04000449 RID: 1097
		AppRemoveBlocked,
		// Token: 0x0400044A RID: 1098
		AppAddMessage,
		// Token: 0x0400044B RID: 1099
		ReadMessage,
		// Token: 0x0400044C RID: 1100
		ReadAll,
		// Token: 0x0400044D RID: 1101
		Call,
		// Token: 0x0400044E RID: 1102
		CallAccept,
		// Token: 0x0400044F RID: 1103
		CallReject,
		// Token: 0x04000450 RID: 1104
		Deposit,
		// Token: 0x04000451 RID: 1105
		Withdraw,
		// Token: 0x04000452 RID: 1106
		ChatChannel,
		// Token: 0x04000453 RID: 1107
		ChatMode,
		// Token: 0x04000454 RID: 1108
		ChatGlobal,
		// Token: 0x04000455 RID: 1109
		ChatLocal,
		// Token: 0x04000456 RID: 1110
		ChatVoice,
		// Token: 0x04000457 RID: 1111
		OptionAction,
		// Token: 0x04000458 RID: 1112
		TextPanelButton,
		// Token: 0x04000459 RID: 1113
		SubmitInput,
		// Token: 0x0400045A RID: 1114
		OldPacket2,
		// Token: 0x0400045B RID: 1115
		OldPacket3,
		// Token: 0x0400045C RID: 1116
		SecurityPanel,
		// Token: 0x0400045D RID: 1117
		MenuClosed,
		// Token: 0x0400045E RID: 1118
		VideoPanel,
		// Token: 0x0400045F RID: 1119
		MoveSeatUp,
		// Token: 0x04000460 RID: 1120
		MoveSeatDown,
		// Token: 0x04000461 RID: 1121
		ToggleWeapon,
		// Token: 0x04000462 RID: 1122
		UpdateTextDisplay,
		// Token: 0x04000463 RID: 1123
		Spectate,
		// Token: 0x04000464 RID: 1124
		ButtonClickedEvent,
		// Token: 0x04000465 RID: 1125
		GetTextFieldText,
		// Token: 0x04000466 RID: 1126
		GetSliderValue,
		// Token: 0x04000467 RID: 1127
		GetToggleValue,
		// Token: 0x04000468 RID: 1128
		GetRadioButtonGroupValue,
		// Token: 0x04000469 RID: 1129
		GetDropdownFieldValue
	}
}
