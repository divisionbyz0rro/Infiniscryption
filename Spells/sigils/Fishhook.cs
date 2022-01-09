using System;
using System.Collections;
using System.Collections.Generic;
using APIPlugin;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using UnityEngine;

namespace Infiniscryption.Spells.Sigils
{
	// Token: 0x02000324 RID: 804
	public class Fishhook : AbilityBehaviour
	{
		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06001358 RID: 4952 RVA: 0x000438A9 File Offset: 0x00041AA9
		public override Ability Ability => _ability;
        private static Ability _ability;
        
        public static AbilityIdentifier Identifier 
        { 
            get
            {
                return AbilityIdentifier.GetAbilityIdentifier("zorro.infiniscryption.sigils.fishhook", "Gain Control");
            }
        }

        public static void Register()
        {
            AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
                "Gain Control",
                "Gains control of the targeted creature."
            );
            info.powerLevel = 8;
            info.canStack = false;            
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part1Rulebook };
            info.pixelIcon = Sprite.Create(
                AssetHelper.LoadTexture("fishhook_pixel", FilterMode.Point),
                new Rect(0f, 0f, 17f, 17f),
                new Vector2(0.5f, 0.5f)
            );

            NewAbility ability = new NewAbility(
                info,
                typeof(Fishhook),
                AssetHelper.LoadTexture("ability_fishhook"),
                Identifier
            );

            Fishhook._ability = ability.ability;
        }

        

		// Token: 0x0600135B RID: 4955 RVA: 0x0000F57E File Offset: 0x0000D77E
		public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
		{
			if (slot.Card == null)
                return false;

            if (slot.IsPlayerSlot)
                return false;

            if (slot.opposingSlot.Card != null)
                return false;

            return true;
		}

		// Token: 0x0600135C RID: 4956 RVA: 0x000438AD File Offset: 0x00041AAD
		public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
		{
			if (slot.Card == null)
                yield break;

            if (slot.IsPlayerSlot)
                yield break;

            if (slot.opposingSlot.Card != null)
                yield break;

            PlayableCard targetCard = slot.Card;

            targetCard.SetIsOpponentCard(false);
            targetCard.transform.eulerAngles += new Vector3(0f, 0f, -180f);
            yield return BoardManager.Instance.AssignCardToSlot(targetCard, slot.opposingSlot, 0.33f, null, true);

            if (targetCard.FaceDown)
            {
                targetCard.SetFaceDown(false, false);
                targetCard.UpdateFaceUpOnBoardEffects();
            }

            yield return new WaitForSeconds(0.66f);
		}
	}
}
