using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.Spells.Sigils
{
	public class DirectHeal : AbilityBehaviour
	{
		public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public static void Register()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Direct Heal";
            info.rulebookDescription = "Heals the target. This can heal the target beyond its original max health.";
            info.canStack = true;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part1Rulebook };
            info.SetPixelAbilityIcon(AssetHelper.LoadTexture("direct_heal_pixel"));

            DirectHeal.AbilityID = AbilityManager.Add(
                InfiniscryptionSpellsPlugin.PluginGuid,
                info,
                typeof(DirectHeal),
                AssetHelper.LoadTexture("ability_health_up")
            ).Id;
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
            {
                slot.Card.HealDamage(1);
                yield return new WaitForSeconds(0.44f);
            }

            yield return base.LearnAbility(0.5f);
            yield break;
		}
	}
}
