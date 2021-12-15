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
	public class DirectHeal : AbilityBehaviour
	{
		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06001358 RID: 4952 RVA: 0x000438A9 File Offset: 0x00041AA9
		public override Ability Ability => _ability;
        private static Ability _ability;
        
        public static AbilityIdentifier Identifier 
        { 
            get
            {
                return AbilityIdentifier.GetAbilityIdentifier("zorro.infiniscryption.sigils.directheal", "Direct Heal");
            }
        }

        public static void Register()
        {
            AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
                "Direct Heal",
                "When this card is targeted at a friendly card, it heals one point of damage. This can overheal."
            );
            info.canStack = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part1Rulebook };

            NewAbility ability = new NewAbility(
                info,
                typeof(DirectHeal),
                AssetHelper.LoadTexture("ability_health_up"),
                Identifier
            );

            DirectHeal._ability = ability.ability;
        }

		// Token: 0x0600135B RID: 4955 RVA: 0x0000F57E File Offset: 0x0000D77E
		public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
		{
			if (slot.Card == null)
                return false;

            if (slot.IsPlayerSlot)
                return true;

            return false;
		}

		// Token: 0x0600135C RID: 4956 RVA: 0x000438AD File Offset: 0x00041AAD
		public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
		{
			if (slot.Card != null)yield return base.LearnAbility(0.5f);
            {
                slot.Card.HealDamage(1);
                yield return new WaitForSeconds(0.44f);
            }

            yield return base.LearnAbility(0.5f);
            yield break;
		}
	}
}
