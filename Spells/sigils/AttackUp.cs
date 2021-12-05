using System;
using System.Collections;
using System.Collections.Generic;
using APIPlugin;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using UnityEngine;

namespace Infiniscryption.Spells.Sigils
{
	public class AttackBuff : AbilityBehaviour
	{
		public override Ability Ability => _ability;
        private static Ability _ability;
        
        public static AbilityIdentifier Identifier 
        { 
            get
            {
                return AbilityIdentifier.GetAbilityIdentifier("zorro.infiniscryption.sigils.attackup", "Attack Up");
            }
        }

        public static void Register()
        {
            AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
                "Attack Up",
                "When this card is targeted at a friendly card, it increases that card's attack by 1 for the rest of the battle."
            );
            info.canStack = true;
            info.passive = false;

            NewAbility ability = new NewAbility(
                info,
                typeof(AttackBuff),
                AssetHelper.LoadTexture("ability_attack_up"),
                Identifier
            );

            AttackBuff._ability = ability.ability;
        }

		public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
		{
			if (slot.Card == null)
                return false;

            if (slot.IsPlayerSlot)
                return true;

            return false;
		}

		public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
		{
			if (slot.Card != null)
                slot.Card.AddTemporaryMod(new CardModificationInfo(1, 0));

            yield return base.LearnAbility(0.5f);

            yield break;
		}
	}
}