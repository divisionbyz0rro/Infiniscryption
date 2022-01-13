using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.Spells.Sigils
{
	public class AttackNerf : AbilityBehaviour
	{
		public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public static void Register()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Attack Down";
            info.rulebookDescription = "Decreases the target's attack for the rest of the battle.";
            info.canStack = true;
            info.powerLevel = 1;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part1Rulebook };
            info.SetPixelAbilityIcon(AssetHelper.LoadTexture("attack_down_pixel"));

            AttackNerf.AbilityID = AbilityManager.Add(
                InfiniscryptionSpellsPlugin.PluginGuid,
                info,
                typeof(AttackNerf),
                AssetHelper.LoadTexture("ability_attack_down")
            ).Id;
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
