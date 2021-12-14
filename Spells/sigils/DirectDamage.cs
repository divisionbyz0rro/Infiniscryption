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
	public class DirectDamage : AbilityBehaviour
	{
		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06001358 RID: 4952 RVA: 0x000438A9 File Offset: 0x00041AA9
		public override Ability Ability => _ability;
        private static Ability _ability;
        
        public static AbilityIdentifier Identifier 
        { 
            get
            {
                return AbilityIdentifier.GetAbilityIdentifier("zorro.infiniscryption.sigils.directdamage", "Direct Damage");
            }
        }

        public static void Register()
        {
            AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
                "Direct Damage",
                "When this card attacks a slot, it deals 1 extra damage to that slot."
            );
            info.canStack = true;
            info.powerLevel = 1;            
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part1Rulebook };

            NewAbility ability = new NewAbility(
                info,
                typeof(DirectDamage),
                AssetHelper.LoadTexture("ability_damage"),
                Identifier
            );

            DirectDamage._ability = ability.ability;
        }

        

		// Token: 0x0600135B RID: 4955 RVA: 0x0000F57E File Offset: 0x0000D77E
		public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
		{
			if (slot.Card == null)
                return false;

            if (slot.IsPlayerSlot)
                return false;

            return true;
		}

		// Token: 0x0600135C RID: 4956 RVA: 0x000438AD File Offset: 0x00041AAD
		public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
		{
			if (slot.Card != null)
                yield return slot.Card.TakeDamage(1, attacker);

            yield return base.LearnAbility(0.5f);
		}
	}
}
