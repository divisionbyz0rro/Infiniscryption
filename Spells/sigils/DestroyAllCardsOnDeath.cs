using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.Spells.Sigils
{
	public class DestroyAllCardsOnDeath : AbilityBehaviour
	{
		public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public static void Register()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Cataclysm";
            info.rulebookDescription = "Destroys every other creature on board when this card dies.";
            info.canStack = false;
            info.powerLevel = 6;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part1Rulebook };
            info.SetPixelAbilityIcon(AssetHelper.LoadTexture("nuke_pixel"));

            DestroyAllCardsOnDeath.AbilityID = AbilityManager.Add(
                InfiniscryptionSpellsPlugin.OriginalPluginGuid,
                info,
                typeof(DestroyAllCardsOnDeath),
                AssetHelper.LoadTexture("ability_nuke")
            ).Id;
        }

		public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
		{
			return true;
		}

		public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
		{
			yield return base.PreSuccessfulTriggerSequence();
			ViewManager.Instance.SwitchToView(View.Board);

            // Kill EVERYTHING
            foreach (var slot in BoardManager.Instance.OpponentSlotsCopy)
                if (slot.Card != null)
                    yield return slot.Card.Die(true, null, true);

            foreach (var slot in BoardManager.Instance.PlayerSlotsCopy)
                if (slot.Card != null)
                    yield return slot.Card.Die(true, null, true);

			yield return base.LearnAbility(0.5f);

            ViewManager.Instance.SwitchToView(View.Default);

			yield break;
		}
	}
}
