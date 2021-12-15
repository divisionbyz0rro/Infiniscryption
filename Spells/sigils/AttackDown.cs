using System;
using System.Collections;
using System.Collections.Generic;
using APIPlugin;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using UnityEngine;

namespace Infiniscryption.Spells.Sigils
{
	public class AttackNerf : AbilityBehaviour
	{
		public override Ability Ability => _ability;
        private static Ability _ability;
        
        public static AbilityIdentifier Identifier 
        { 
            get
            {
                return AbilityIdentifier.GetAbilityIdentifier("zorro.infiniscryption.sigils.attackdown", "Attack Down");
            }
        }

        public static void Register()
        {
            AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
                "Attack Down",
                "When this card is targeted at an enemy card, it decreases that card's attack by 1 for the rest of the battle."
            );
            info.canStack = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part1Rulebook };

            NewAbility ability = new NewAbility(
                info,
                typeof(AttackNerf),
                AssetHelper.LoadTexture("ability_attack_down"),
                Identifier
            );

            AttackNerf._ability = ability.ability;
        }

		public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
		{
			if (slot.Card == null)
                return false;

            if (slot.IsPlayerSlot)
                return false;

            return true;
		}

		public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
		{
			if (slot.Card != null)
                slot.Card.AddTemporaryMod(new CardModificationInfo(-1, 0));

            yield return base.LearnAbility(0.5f);

            yield break;
		}
	}
}
