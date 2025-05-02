using System;

namespace BrokeProtocol.Utility.Networking
{
	// Token: 0x0200007B RID: 123
	public enum ClPacket : byte
	{
		// Token: 0x0400031F RID: 799
		UpdateTime,
		// Token: 0x04000320 RID: 800
		AddEntity,
		// Token: 0x04000321 RID: 801
		DestroyEntity,
		// Token: 0x04000322 RID: 802
		ActivateEntity,
		// Token: 0x04000323 RID: 803
		DeactivateEntity,
		// Token: 0x04000324 RID: 804
		Relocate,
		// Token: 0x04000325 RID: 805
		RelocateSelf,
		// Token: 0x04000326 RID: 806
		SetJob,
		// Token: 0x04000327 RID: 807
		GameMessage,
		// Token: 0x04000328 RID: 808
		TransferItem,
		// Token: 0x04000329 RID: 809
		View,
		// Token: 0x0400032A RID: 810
		Shopping,
		// Token: 0x0400032B RID: 811
		ShowTradeInventory,
		// Token: 0x0400032C RID: 812
		ShowSearchedInventory,
		// Token: 0x0400032D RID: 813
		SetEquipable,
		// Token: 0x0400032E RID: 814
		SetAttachment,
		// Token: 0x0400032F RID: 815
		SetWearable,
		// Token: 0x04000330 RID: 816
		OtherConfirmed,
		// Token: 0x04000331 RID: 817
		WaitTrade,
		// Token: 0x04000332 RID: 818
		OtherFinalized,
		// Token: 0x04000333 RID: 819
		FinishTrade,
		// Token: 0x04000334 RID: 820
		UpdateShopValue,
		// Token: 0x04000335 RID: 821
		Mount,
		// Token: 0x04000336 RID: 822
		Dismount,
		// Token: 0x04000337 RID: 823
		TransportState,
		// Token: 0x04000338 RID: 824
		TransportOwner,
		// Token: 0x04000339 RID: 825
		UpdateHealth,
		// Token: 0x0400033A RID: 826
		ShowHealth,
		// Token: 0x0400033B RID: 827
		AddInjury,
		// Token: 0x0400033C RID: 828
		ClearInjuries,
		// Token: 0x0400033D RID: 829
		RemoveInjury,
		// Token: 0x0400033E RID: 830
		Spawn,
		// Token: 0x0400033F RID: 831
		Reload,
		// Token: 0x04000340 RID: 832
		Load,
		// Token: 0x04000341 RID: 833
		Stats,
		// Token: 0x04000342 RID: 834
		RegisterMenu,
		// Token: 0x04000343 RID: 835
		RegisterFail,
		// Token: 0x04000344 RID: 836
		CursorVisibility,
		// Token: 0x04000345 RID: 837
		MaxSpeed,
		// Token: 0x04000346 RID: 838
		UpdateTextDisplay,
		// Token: 0x04000347 RID: 839
		AddVoxels,
		// Token: 0x04000348 RID: 840
		RemoveVoxels,
		// Token: 0x04000349 RID: 841
		Tow,
		// Token: 0x0400034A RID: 842
		SetTerritory,
		// Token: 0x0400034B RID: 843
		TextMenu,
		// Token: 0x0400034C RID: 844
		OptionMenu,
		// Token: 0x0400034D RID: 845
		InputMenu,
		// Token: 0x0400034E RID: 846
		ShowTextPanel,
		// Token: 0x0400034F RID: 847
		DestroyTextPanel,
		// Token: 0x04000350 RID: 848
		HackingMenu,
		// Token: 0x04000351 RID: 849
		CrackingMenu,
		// Token: 0x04000352 RID: 850
		HackingProbe,
		// Token: 0x04000353 RID: 851
		HackingMark,
		// Token: 0x04000354 RID: 852
		CrackingAttempt,
		// Token: 0x04000355 RID: 853
		MinigameOver,
		// Token: 0x04000356 RID: 854
		Force,
		// Token: 0x04000357 RID: 855
		Jump,
		// Token: 0x04000358 RID: 856
		Stance,
		// Token: 0x04000359 RID: 857
		SerializedItems,
		// Token: 0x0400035A RID: 858
		SerializedHealth,
		// Token: 0x0400035B RID: 859
		StartVote,
		// Token: 0x0400035C RID: 860
		VoteUpdate,
		// Token: 0x0400035D RID: 861
		SetVault,
		// Token: 0x0400035E RID: 862
		ClonePlace,
		// Token: 0x0400035F RID: 863
		DestroyPlace,
		// Token: 0x04000360 RID: 864
		BuyApartment,
		// Token: 0x04000361 RID: 865
		SellApartment,
		// Token: 0x04000362 RID: 866
		Consume,
		// Token: 0x04000363 RID: 867
		StartProgress,
		// Token: 0x04000364 RID: 868
		StopProgress,
		// Token: 0x04000365 RID: 869
		AddEntityArray,
		// Token: 0x04000366 RID: 870
		UpdateAmmo,
		// Token: 0x04000367 RID: 871
		UpdateMode,
		// Token: 0x04000368 RID: 872
		UpdateSmooth,
		// Token: 0x04000369 RID: 873
		DefaultEnvironment,
		// Token: 0x0400036A RID: 874
		CustomEnvironment,
		// Token: 0x0400036B RID: 875
		SetColor,
		// Token: 0x0400036C RID: 876
		SetScale,
		// Token: 0x0400036D RID: 877
		TransferInfo,
		// Token: 0x0400036E RID: 878
		JobData,
		// Token: 0x0400036F RID: 879
		AssetData,
		// Token: 0x04000370 RID: 880
		MapData,
		// Token: 0x04000371 RID: 881
		EntityData,
		// Token: 0x04000372 RID: 882
		FinishedStaticData,
		// Token: 0x04000373 RID: 883
		SetSiren,
		// Token: 0x04000374 RID: 884
		LoadingWindow,
		// Token: 0x04000375 RID: 885
		PlayerRecords,
		// Token: 0x04000376 RID: 886
		BanRecords,
		// Token: 0x04000377 RID: 887
		BanState,
		// Token: 0x04000378 RID: 888
		Point,
		// Token: 0x04000379 RID: 889
		Alert,
		// Token: 0x0400037A RID: 890
		Disarm,
		// Token: 0x0400037B RID: 891
		DrawLine,
		// Token: 0x0400037C RID: 892
		ShowTimer,
		// Token: 0x0400037D RID: 893
		ShowText,
		// Token: 0x0400037E RID: 894
		DestroyText,
		// Token: 0x0400037F RID: 895
		Permissions,
		// Token: 0x04000380 RID: 896
		DestroyMenu,
		// Token: 0x04000381 RID: 897
		TimeInfo,
		// Token: 0x04000382 RID: 898
		Experience,
		// Token: 0x04000383 RID: 899
		Rank,
		// Token: 0x04000384 RID: 900
		Restore,
		// Token: 0x04000385 RID: 901
		Bind,
		// Token: 0x04000386 RID: 902
		Unbind,
		// Token: 0x04000387 RID: 903
		BindAttachment,
		// Token: 0x04000388 RID: 904
		UnbindAttachment,
		// Token: 0x04000389 RID: 905
		Fire,
		// Token: 0x0400038A RID: 906
		DestroyEffect,
		// Token: 0x0400038B RID: 907
		AltFire,
		// Token: 0x0400038C RID: 908
		Apps,
		// Token: 0x0400038D RID: 909
		AppContacts,
		// Token: 0x0400038E RID: 910
		AppBlocked,
		// Token: 0x0400038F RID: 911
		AppCalls,
		// Token: 0x04000390 RID: 912
		AppInbox,
		// Token: 0x04000391 RID: 913
		AppServices,
		// Token: 0x04000392 RID: 914
		AppDeposit,
		// Token: 0x04000393 RID: 915
		AppWithdraw,
		// Token: 0x04000394 RID: 916
		AppRadio,
		// Token: 0x04000395 RID: 917
		AppMessage,
		// Token: 0x04000396 RID: 918
		AppAddMessage,
		// Token: 0x04000397 RID: 919
		Call,
		// Token: 0x04000398 RID: 920
		CallAccepted,
		// Token: 0x04000399 RID: 921
		CallCanceled,
		// Token: 0x0400039A RID: 922
		SetChatChannel,
		// Token: 0x0400039B RID: 923
		SetChatMode,
		// Token: 0x0400039C RID: 924
		ChatGlobal,
		// Token: 0x0400039D RID: 925
		ChatJob,
		// Token: 0x0400039E RID: 926
		ChatChannel,
		// Token: 0x0400039F RID: 927
		ChatLocal,
		// Token: 0x040003A0 RID: 928
		ChatVoice,
		// Token: 0x040003A1 RID: 929
		ChatVoiceCall,
		// Token: 0x040003A2 RID: 930
		ChatVoiceJob,
		// Token: 0x040003A3 RID: 931
		ChatVoiceChannel,
		// Token: 0x040003A4 RID: 932
		UnreadMessages,
		// Token: 0x040003A5 RID: 933
		DisplayName,
		// Token: 0x040003A6 RID: 934
		Spectate,
		// Token: 0x040003A7 RID: 935
		OpenURL,
		// Token: 0x040003A8 RID: 936
		AnimatorEnabled,
		// Token: 0x040003A9 RID: 937
		AnimatorFloat,
		// Token: 0x040003AA RID: 938
		AnimatorInt,
		// Token: 0x040003AB RID: 939
		AnimatorBool,
		// Token: 0x040003AC RID: 940
		AnimatorTrigger,
		// Token: 0x040003AD RID: 941
		AnimatorState,
		// Token: 0x040003AE RID: 942
		VideoPlay,
		// Token: 0x040003AF RID: 943
		VideoStop,
		// Token: 0x040003B0 RID: 944
		AddDynamicAction,
		// Token: 0x040003B1 RID: 945
		RemoveDynamicAction,
		// Token: 0x040003B2 RID: 946
		AddSelfAction,
		// Token: 0x040003B3 RID: 947
		RemoveSelfAction,
		// Token: 0x040003B4 RID: 948
		AddTypeAction,
		// Token: 0x040003B5 RID: 949
		RemoveTypeAction,
		// Token: 0x040003B6 RID: 950
		AddInventoryAction,
		// Token: 0x040003B7 RID: 951
		RemoveInventoryAction,
		// Token: 0x040003B8 RID: 952
		LockOn,
		// Token: 0x040003B9 RID: 953
		ToggleWeapon,
		// Token: 0x040003BA RID: 954
		Connect,
		// Token: 0x040003BB RID: 955
		Explosion,
		// Token: 0x040003BC RID: 956
		VisualTreeAssetClone,
		// Token: 0x040003BD RID: 957
		VisualElementRemove,
		// Token: 0x040003BE RID: 958
		VisualElementOpacity,
		// Token: 0x040003BF RID: 959
		VisualElementDisplay,
		// Token: 0x040003C0 RID: 960
		VisualElementVisibility,
		// Token: 0x040003C1 RID: 961
		VisualElementOverflow,
		// Token: 0x040003C2 RID: 962
		AddButtonClickedEvent,
		// Token: 0x040003C3 RID: 963
		GetTextFieldText,
		// Token: 0x040003C4 RID: 964
		SetTextElementText,
		// Token: 0x040003C5 RID: 965
		GetSliderValue,
		// Token: 0x040003C6 RID: 966
		SetSliderValue,
		// Token: 0x040003C7 RID: 967
		SetProgressBarValue,
		// Token: 0x040003C8 RID: 968
		GetToggleValue,
		// Token: 0x040003C9 RID: 969
		SetToggleValue,
		// Token: 0x040003CA RID: 970
		GetRadioButtonGroupValue,
		// Token: 0x040003CB RID: 971
		SetRadioButtonGroupValue,
		// Token: 0x040003CC RID: 972
		SetRadioButtonGroupChoices,
		// Token: 0x040003CD RID: 973
		GetDropdownFieldValue,
		// Token: 0x040003CE RID: 974
		SetDropdownFieldValue,
		// Token: 0x040003CF RID: 975
		SetDropdownFieldChoices,
		// Token: 0x040003D0 RID: 976
		VisualElementCursorVisibility,
		// Token: 0x040003D1 RID: 977
		SerializedWearables,
		// Token: 0x040003D2 RID: 978
		SerializedAttachments
	}
}
