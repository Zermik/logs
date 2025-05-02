using System;
using System.Collections.Generic;
using BrokeProtocol.Entities;
using BrokeProtocol.Required;
using BrokeProtocol.Utility;
using BrokeProtocol.Utility.Jobs;
using UnityEngine;

namespace BrokeProtocol.API
{
	// Token: 0x0200034E RID: 846
	public class PlayerEvents : MovableEvents
	{
		// Token: 0x06001180 RID: 4480 RVA: 0x000525F6 File Offset: 0x000507F6
		[Target(500)]
		public override bool Initialize(ShEntity entity)
		{
			return base.Initialize(entity) && SourceHandler.Exec(500, new object[]
			{
				entity as ShPlayer
			});
		}

		// Token: 0x06001181 RID: 4481 RVA: 0x0005261C File Offset: 0x0005081C
		[Target(501)]
		public override bool Destroy(ShEntity entity)
		{
			return base.Destroy(entity) && SourceHandler.Exec(501, new object[]
			{
				entity as ShPlayer
			});
		}

		// Token: 0x06001182 RID: 4482 RVA: 0x00052642 File Offset: 0x00050842
		[Target(502)]
		public virtual bool Command(ShPlayer player, string message)
		{
			return SourceHandler.Exec(502, new object[]
			{
				player,
				message
			});
		}

		// Token: 0x06001183 RID: 4483 RVA: 0x0005265C File Offset: 0x0005085C
		[Target(503)]
		public virtual bool ChatGlobal(ShPlayer player, string message)
		{
			return SourceHandler.Exec(503, new object[]
			{
				player,
				message
			});
		}

		// Token: 0x06001184 RID: 4484 RVA: 0x00052676 File Offset: 0x00050876
		[Target(504)]
		public virtual bool ChatLocal(ShPlayer player, string message)
		{
			return SourceHandler.Exec(504, new object[]
			{
				player,
				message
			});
		}

		// Token: 0x06001185 RID: 4485 RVA: 0x00052690 File Offset: 0x00050890
		[Target(505)]
		public virtual bool ChatVoice(ShPlayer player, byte[] voiceData)
		{
			return SourceHandler.Exec(505, new object[]
			{
				player,
				voiceData
			});
		}

		// Token: 0x06001186 RID: 4486 RVA: 0x000526AA File Offset: 0x000508AA
		[Target(506)]
		public virtual bool SetChatMode(ShPlayer player, ChatMode chatMode)
		{
			return SourceHandler.Exec(506, new object[]
			{
				player,
				chatMode
			});
		}

		// Token: 0x06001187 RID: 4487 RVA: 0x000526C9 File Offset: 0x000508C9
		[Target(507)]
		public virtual bool SetChatChannel(ShPlayer player, ushort channel)
		{
			return SourceHandler.Exec(507, new object[]
			{
				player,
				channel
			});
		}

		// Token: 0x06001188 RID: 4488 RVA: 0x000526E8 File Offset: 0x000508E8
		[Target(508)]
		public virtual bool BuyApartment(ShPlayer player, ShApartment apartment)
		{
			return SourceHandler.Exec(508, new object[]
			{
				player,
				apartment
			});
		}

		// Token: 0x06001189 RID: 4489 RVA: 0x00052702 File Offset: 0x00050902
		[Target(509)]
		public virtual bool SellApartment(ShPlayer player, ShApartment apartment)
		{
			return SourceHandler.Exec(509, new object[]
			{
				player,
				apartment
			});
		}

		// Token: 0x0600118A RID: 4490 RVA: 0x0005271C File Offset: 0x0005091C
		[Target(510)]
		public virtual bool Invite(ShPlayer player, ShPlayer other)
		{
			return SourceHandler.Exec(510, new object[]
			{
				player,
				other
			});
		}

		// Token: 0x0600118B RID: 4491 RVA: 0x00052736 File Offset: 0x00050936
		[Target(511)]
		public virtual bool KickOut(ShPlayer player, ShPlayer other)
		{
			return SourceHandler.Exec(511, new object[]
			{
				player,
				other
			});
		}

		// Token: 0x0600118C RID: 4492 RVA: 0x00052750 File Offset: 0x00050950
		[Target(512)]
		public override bool Respawn(ShEntity entity)
		{
			return base.Respawn(entity) && SourceHandler.Exec(512, new object[]
			{
				entity as ShPlayer
			});
		}

		// Token: 0x0600118D RID: 4493 RVA: 0x00052776 File Offset: 0x00050976
		[Target(513)]
		public virtual bool Reward(ShPlayer player, int experienceDelta, int moneyDelta)
		{
			return SourceHandler.Exec(513, new object[]
			{
				player,
				experienceDelta,
				moneyDelta
			});
		}

		// Token: 0x0600118E RID: 4494 RVA: 0x0005279E File Offset: 0x0005099E
		[Target(514)]
		public virtual bool Collect(ShPlayer player, ShEntity e, bool consume)
		{
			return SourceHandler.Exec(514, new object[]
			{
				player,
				e,
				consume
			});
		}

		// Token: 0x0600118F RID: 4495 RVA: 0x000527C1 File Offset: 0x000509C1
		[Target(515)]
		public virtual bool StopInventory(ShPlayer player, bool sendToSelf)
		{
			return SourceHandler.Exec(515, new object[]
			{
				player,
				sendToSelf
			});
		}

		// Token: 0x06001190 RID: 4496 RVA: 0x000527E0 File Offset: 0x000509E0
		[Target(516)]
		public virtual bool ViewInventory(ShPlayer player, ShEntity searchee, bool force)
		{
			return SourceHandler.Exec(516, new object[]
			{
				player,
				searchee,
				force
			});
		}

		// Token: 0x06001191 RID: 4497 RVA: 0x00052803 File Offset: 0x00050A03
		[Target(517)]
		public virtual bool Kick(ShPlayer player, ShPlayer other, string reason)
		{
			return SourceHandler.Exec(517, new object[]
			{
				player,
				other,
				reason
			});
		}

		// Token: 0x06001192 RID: 4498 RVA: 0x00052821 File Offset: 0x00050A21
		[Target(518)]
		public virtual bool Ban(ShPlayer player, ShPlayer other, string reason)
		{
			return SourceHandler.Exec(518, new object[]
			{
				player,
				other,
				reason
			});
		}

		// Token: 0x06001193 RID: 4499 RVA: 0x00052840 File Offset: 0x00050A40
		[Target(519)]
		public override bool AddItem(ShEntity entity, int itemIndex, int amount, bool dispatch)
		{
			return base.AddItem(entity, itemIndex, amount, dispatch) && SourceHandler.Exec(519, new object[]
			{
				entity as ShPlayer,
				itemIndex,
				amount,
				dispatch
			});
		}

		// Token: 0x06001194 RID: 4500 RVA: 0x00052894 File Offset: 0x00050A94
		[Target(520)]
		public override bool RemoveItem(ShEntity entity, int itemIndex, int amount, bool dispatch)
		{
			return base.RemoveItem(entity, itemIndex, amount, dispatch) && SourceHandler.Exec(520, new object[]
			{
				entity as ShPlayer,
				itemIndex,
				amount,
				dispatch
			});
		}

		// Token: 0x06001195 RID: 4501 RVA: 0x000528E5 File Offset: 0x00050AE5
		[Target(521)]
		public virtual bool RemoveItemsDeath(ShPlayer player, bool dropItems)
		{
			return SourceHandler.Exec(521, new object[]
			{
				player,
				dropItems
			});
		}

		// Token: 0x06001196 RID: 4502 RVA: 0x00052904 File Offset: 0x00050B04
		[Target(522)]
		public virtual bool Load(ShPlayer player)
		{
			return SourceHandler.Exec(522, new object[]
			{
				player
			});
		}

		// Token: 0x06001197 RID: 4503 RVA: 0x0005291A File Offset: 0x00050B1A
		[Target(523)]
		public virtual bool Save(ShPlayer player)
		{
			return SourceHandler.Exec(523, new object[]
			{
				player
			});
		}

		// Token: 0x06001198 RID: 4504 RVA: 0x00052930 File Offset: 0x00050B30
		[Target(524)]
		public virtual bool Injury(ShPlayer player, BodyPart part, BodyEffect effect, byte amount)
		{
			return SourceHandler.Exec(524, new object[]
			{
				player,
				part,
				effect,
				amount
			});
		}

		// Token: 0x06001199 RID: 4505 RVA: 0x00052962 File Offset: 0x00050B62
		[Target(525)]
		public virtual bool Restrain(ShPlayer player, ShPlayer initiator, ShRestrained restrained)
		{
			return SourceHandler.Exec(525, new object[]
			{
				player,
				initiator,
				restrained
			});
		}

		// Token: 0x0600119A RID: 4506 RVA: 0x00052980 File Offset: 0x00050B80
		[Target(526)]
		public virtual bool Unrestrain(ShPlayer player, ShPlayer initiator)
		{
			return SourceHandler.Exec(526, new object[]
			{
				player,
				initiator
			});
		}

		// Token: 0x0600119B RID: 4507 RVA: 0x0005299A File Offset: 0x00050B9A
		[Target(527)]
		public virtual bool ServerInfo(ShPlayer player)
		{
			return SourceHandler.Exec(527, new object[]
			{
				player
			});
		}

		// Token: 0x0600119C RID: 4508 RVA: 0x000529B0 File Offset: 0x00050BB0
		[Target(528)]
		public virtual bool DisplayName(ShPlayer player, string username)
		{
			return SourceHandler.Exec(528, new object[]
			{
				player,
				username
			});
		}

		// Token: 0x0600119D RID: 4509 RVA: 0x000529CA File Offset: 0x00050BCA
		[Target(529)]
		public virtual bool EnterDoor(ShPlayer player, ShDoor door, ShPlayer sender, bool forceEnter)
		{
			return SourceHandler.Exec(529, new object[]
			{
				player,
				door,
				sender,
				forceEnter
			});
		}

		// Token: 0x0600119E RID: 4510 RVA: 0x000529F2 File Offset: 0x00050BF2
		[Target(530)]
		public virtual bool Follower(ShPlayer player, ShPlayer other)
		{
			return SourceHandler.Exec(530, new object[]
			{
				player,
				other
			});
		}

		// Token: 0x0600119F RID: 4511 RVA: 0x00052A0C File Offset: 0x00050C0C
		[Target(531)]
		public virtual bool OptionAction(ShPlayer player, int targetID, string id, string optionID, string actionID)
		{
			return SourceHandler.Exec(531, new object[]
			{
				player,
				targetID,
				id,
				optionID,
				actionID
			});
		}

		// Token: 0x060011A0 RID: 4512 RVA: 0x00052A39 File Offset: 0x00050C39
		[Target(532)]
		public virtual bool SubmitInput(ShPlayer player, int targetID, string id, string input)
		{
			return SourceHandler.Exec(532, new object[]
			{
				player,
				targetID,
				id,
				input
			});
		}

		// Token: 0x060011A1 RID: 4513 RVA: 0x00052A61 File Offset: 0x00050C61
		[Target(533)]
		public virtual bool Ready(ShPlayer player)
		{
			return SourceHandler.Exec(533, new object[]
			{
				player
			});
		}

		// Token: 0x060011A2 RID: 4514 RVA: 0x00052A77 File Offset: 0x00050C77
		[Target(534)]
		public virtual bool Point(ShPlayer player, bool pointing)
		{
			return SourceHandler.Exec(534, new object[]
			{
				player,
				pointing
			});
		}

		// Token: 0x060011A3 RID: 4515 RVA: 0x00052A96 File Offset: 0x00050C96
		[Target(535)]
		public virtual bool Alert(ShPlayer player)
		{
			return SourceHandler.Exec(535, new object[]
			{
				player
			});
		}

		// Token: 0x060011A4 RID: 4516 RVA: 0x00052AAC File Offset: 0x00050CAC
		[Target(536)]
		public virtual bool MinigameFinished(ShPlayer player, bool successful, int targetID, string id, string optionID)
		{
			return SourceHandler.Exec(536, new object[]
			{
				player,
				successful,
				targetID,
				id,
				optionID
			});
		}

		// Token: 0x060011A5 RID: 4517 RVA: 0x00052ADE File Offset: 0x00050CDE
		[Target(537)]
		public override bool DestroySelf(ShDestroyable destroyable)
		{
			return base.DestroySelf(destroyable) && SourceHandler.Exec(537, new object[]
			{
				destroyable as ShPlayer
			});
		}

		// Token: 0x060011A6 RID: 4518 RVA: 0x00052B04 File Offset: 0x00050D04
		[Target(538)]
		public override bool TransferItem(ShEntity entity, byte deltaType, int itemIndex, int amount, bool dispatch)
		{
			return base.TransferItem(entity, deltaType, itemIndex, amount, dispatch) && SourceHandler.Exec(538, new object[]
			{
				entity as ShPlayer,
				deltaType,
				itemIndex,
				amount,
				dispatch
			});
		}

		// Token: 0x060011A7 RID: 4519 RVA: 0x00052B61 File Offset: 0x00050D61
		[Target(539)]
		public virtual bool MenuClosed(ShPlayer player, string id)
		{
			return SourceHandler.Exec(539, new object[]
			{
				player,
				id
			});
		}

		// Token: 0x060011A8 RID: 4520 RVA: 0x00052B7B File Offset: 0x00050D7B
		[Target(540)]
		public virtual bool SecurityPanel(ShPlayer player, ShApartment apartment)
		{
			return SourceHandler.Exec(540, new object[]
			{
				player,
				apartment
			});
		}

		// Token: 0x060011A9 RID: 4521 RVA: 0x00052B95 File Offset: 0x00050D95
		[Target(541)]
		public virtual bool VideoPanel(ShPlayer player, ShEntity entity)
		{
			return SourceHandler.Exec(541, new object[]
			{
				player,
				entity
			});
		}

		// Token: 0x060011AA RID: 4522 RVA: 0x00052BAF File Offset: 0x00050DAF
		[Target(542)]
		public virtual bool TextPanelButton(ShPlayer player, string id, string optionID)
		{
			return SourceHandler.Exec(542, new object[]
			{
				player,
				id,
				optionID
			});
		}

		// Token: 0x060011AB RID: 4523 RVA: 0x00052BCD File Offset: 0x00050DCD
		[Target(543)]
		public virtual bool SetEquipable(ShPlayer player, ShEquipable equipable)
		{
			return SourceHandler.Exec(543, new object[]
			{
				player,
				equipable
			});
		}

		// Token: 0x060011AC RID: 4524 RVA: 0x00052BE7 File Offset: 0x00050DE7
		[Target(544)]
		public virtual bool CrackStart(ShPlayer player, int entityID)
		{
			return SourceHandler.Exec(544, new object[]
			{
				player,
				entityID
			});
		}

		// Token: 0x060011AD RID: 4525 RVA: 0x00052C06 File Offset: 0x00050E06
		[Target(545)]
		public virtual bool Mount(ShPlayer player, ShMountable mount, byte seat)
		{
			return SourceHandler.Exec(545, new object[]
			{
				player,
				mount,
				seat
			});
		}

		// Token: 0x060011AE RID: 4526 RVA: 0x00052C29 File Offset: 0x00050E29
		[Target(546)]
		public virtual bool Dismount(ShPlayer player)
		{
			return SourceHandler.Exec(546, new object[]
			{
				player
			});
		}

		// Token: 0x060011AF RID: 4527 RVA: 0x00052C3F File Offset: 0x00050E3F
		[Target(547)]
		public override bool Spawn(ShEntity entity)
		{
			return base.Spawn(entity) && SourceHandler.Exec(547, new object[]
			{
				entity as ShPlayer
			});
		}

		// Token: 0x060011B0 RID: 4528 RVA: 0x00052C65 File Offset: 0x00050E65
		[Target(548)]
		public virtual bool PlaceItem(ShPlayer player, ShEntity placeableEntity, Vector3 position, Quaternion rotation, float spawnDelay)
		{
			return SourceHandler.Exec(548, new object[]
			{
				player,
				placeableEntity,
				position,
				rotation,
				spawnDelay
			});
		}

		// Token: 0x060011B1 RID: 4529 RVA: 0x00052C9C File Offset: 0x00050E9C
		[Target(549)]
		public virtual bool ResetAI(ShPlayer player)
		{
			return SourceHandler.Exec(549, new object[]
			{
				player
			});
		}

		// Token: 0x060011B2 RID: 4530 RVA: 0x00052CB2 File Offset: 0x00050EB2
		[Target(550)]
		public virtual bool SetWearable(ShPlayer player, ShWearable wearable)
		{
			return SourceHandler.Exec(550, new object[]
			{
				player,
				wearable
			});
		}

		// Token: 0x060011B3 RID: 4531 RVA: 0x00052CCC File Offset: 0x00050ECC
		[Target(551)]
		public virtual bool RestrainOther(ShPlayer player, ShPlayer hitPlayer, ShRestraint restraint)
		{
			return SourceHandler.Exec(551, new object[]
			{
				player,
				hitPlayer,
				restraint
			});
		}

		// Token: 0x060011B4 RID: 4532 RVA: 0x00052CEA File Offset: 0x00050EEA
		[Target(552)]
		public virtual bool Deposit(ShPlayer player, int entityID, int amount)
		{
			return SourceHandler.Exec(552, new object[]
			{
				player,
				entityID,
				amount
			});
		}

		// Token: 0x060011B5 RID: 4533 RVA: 0x00052D12 File Offset: 0x00050F12
		[Target(553)]
		public virtual bool Withdraw(ShPlayer player, int entityID, int amount)
		{
			return SourceHandler.Exec(553, new object[]
			{
				player,
				entityID,
				amount
			});
		}

		// Token: 0x060011B6 RID: 4534 RVA: 0x00052D3A File Offset: 0x00050F3A
		[Target(554)]
		public virtual bool TryGetJob(ShPlayer player, ShPlayer employer)
		{
			return SourceHandler.Exec(554, new object[]
			{
				player,
				employer
			});
		}

		// Token: 0x060011B7 RID: 4535 RVA: 0x00052D54 File Offset: 0x00050F54
		[Target(555)]
		public virtual bool Bomb(ShPlayer player, ShVault vault)
		{
			return SourceHandler.Exec(555, new object[]
			{
				player,
				vault
			});
		}

		// Token: 0x060011B8 RID: 4536 RVA: 0x00052D6E File Offset: 0x00050F6E
		[Target(556)]
		public virtual bool Repair(ShPlayer player, ShTransport transport)
		{
			return SourceHandler.Exec(556, new object[]
			{
				player,
				transport
			});
		}

		// Token: 0x060011B9 RID: 4537 RVA: 0x00052D88 File Offset: 0x00050F88
		[Target(557)]
		public virtual bool Lockpick(ShPlayer player, ShTransport transport)
		{
			return SourceHandler.Exec(557, new object[]
			{
				player,
				transport
			});
		}

		// Token: 0x060011BA RID: 4538 RVA: 0x00052DA2 File Offset: 0x00050FA2
		[Target(558)]
		public virtual bool Consume(ShPlayer player, ShConsumable consumable, ShPlayer healer)
		{
			return SourceHandler.Exec(558, new object[]
			{
				player,
				consumable,
				healer
			});
		}

		// Token: 0x060011BB RID: 4539 RVA: 0x00052DC0 File Offset: 0x00050FC0
		[Target(559)]
		public virtual bool TransferShop(ShPlayer player, byte deltaType, int itemIndex, int amount)
		{
			return SourceHandler.Exec(559, new object[]
			{
				player,
				deltaType,
				itemIndex,
				amount
			});
		}

		// Token: 0x060011BC RID: 4540 RVA: 0x00052DF2 File Offset: 0x00050FF2
		[Target(560)]
		public virtual bool TransferTrade(ShPlayer player, byte deltaType, int itemIndex, int amount)
		{
			return SourceHandler.Exec(560, new object[]
			{
				player,
				deltaType,
				itemIndex,
				amount
			});
		}

		// Token: 0x060011BD RID: 4541 RVA: 0x00052E24 File Offset: 0x00051024
		[Target(561)]
		public override bool NewSector(ShEntity entity, List<NetSector> newSectors)
		{
			return base.NewSector(entity, newSectors) && SourceHandler.Exec(561, new object[]
			{
				entity as ShPlayer,
				newSectors
			});
		}

		// Token: 0x060011BE RID: 4542 RVA: 0x00052E4F File Offset: 0x0005104F
		[Target(562)]
		public override bool SameSector(ShEntity entity)
		{
			return base.SameSector(entity) && SourceHandler.Exec(562, new object[]
			{
				entity as ShPlayer
			});
		}

		// Token: 0x060011BF RID: 4543 RVA: 0x00052E75 File Offset: 0x00051075
		[Target(563)]
		public virtual bool Fire(ShPlayer player)
		{
			return SourceHandler.Exec(563, new object[]
			{
				player
			});
		}

		// Token: 0x060011C0 RID: 4544 RVA: 0x00052E8B File Offset: 0x0005108B
		[Target(564)]
		public override bool SetParent(ShEntity entity, Transform parent)
		{
			return base.SetParent(entity, parent) && SourceHandler.Exec(564, new object[]
			{
				entity,
				parent
			});
		}

		// Token: 0x060011C1 RID: 4545 RVA: 0x00052EB4 File Offset: 0x000510B4
		[Target(565)]
		public override bool Damage(ShDamageable damageable, DamageIndex damageIndex, float amount, ShPlayer attacker, Collider collider, Vector3 hitPoint, Vector3 hitNormal)
		{
			return base.Damage(damageable, damageIndex, amount, attacker, collider, hitPoint, hitNormal) && SourceHandler.Exec(565, new object[]
			{
				damageable as ShPlayer,
				damageIndex,
				amount,
				attacker,
				collider,
				hitPoint,
				hitNormal
			});
		}

		// Token: 0x060011C2 RID: 4546 RVA: 0x00052F1F File Offset: 0x0005111F
		[Target(566)]
		public override bool Death(ShDestroyable destroyable, ShPlayer attacker)
		{
			return base.Death(destroyable, attacker) && SourceHandler.Exec(566, new object[]
			{
				destroyable as ShPlayer,
				attacker
			});
		}

		// Token: 0x060011C3 RID: 4547 RVA: 0x00052F4A File Offset: 0x0005114A
		[Target(567)]
		public virtual bool UpdateTextDisplay(ShPlayer player, ShTextDisplay textDisplay)
		{
			return SourceHandler.Exec(567, new object[]
			{
				player,
				textDisplay
			});
		}

		// Token: 0x060011C4 RID: 4548 RVA: 0x00052F64 File Offset: 0x00051164
		[Target(568)]
		public virtual bool SetJob(ShPlayer player, JobInfo newJob, int rank, bool addItems, bool collectCost)
		{
			return SourceHandler.Exec(568, new object[]
			{
				player,
				newJob,
				rank,
				addItems,
				collectCost
			});
		}

		// Token: 0x060011C5 RID: 4549 RVA: 0x00052F9B File Offset: 0x0005119B
		[Target(569)]
		public virtual bool Park(ShPlayer player, ShTransport transport)
		{
			return SourceHandler.Exec(569, new object[]
			{
				player,
				transport
			});
		}

		// Token: 0x060011C6 RID: 4550 RVA: 0x00052FB5 File Offset: 0x000511B5
		[Target(570)]
		public virtual bool Tow(ShPlayer player, bool setting)
		{
			return SourceHandler.Exec(570, new object[]
			{
				player,
				setting
			});
		}
	}
}
