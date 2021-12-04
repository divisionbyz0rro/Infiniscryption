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
        
        public static AbilityIdentifier Identifier { get; private set; }

        public static void Register()
        {
            AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
                "Direct Damage",
                "When this card is directed at a slot, it deals damage to that slot."
            );
            info.canStack = true;

            Identifier = AbilityIdentifier.GetAbilityIdentifier("zorro.infiniscryption.sigils.directdamage", "Direct Damage");

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
			return true;
		}

		// Token: 0x0600135C RID: 4956 RVA: 0x000438AD File Offset: 0x00041AAD
		public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
		{
			if (slot.Card != null)
            {
                yield return slot.Card.TakeDamage(this.Card.Info.Attack > 3 ? 3 : this.Card.Info.Attack, attacker);
            }
		}
	}
}
