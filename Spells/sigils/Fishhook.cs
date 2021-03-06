using System.Collections;
using System.Collections.Generic;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.Spells.Sigils
{
	public class Fishhook : AbilityBehaviour
	{
		public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        public static void Register()
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Gain Control";
            info.rulebookDescription = "Gains control of the targeted creature.";
            info.powerLevel = 8;
            info.canStack = false;            
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part1Rulebook };
            info.SetPixelAbilityIcon(AssetHelper.LoadTexture("fishhook_pixel"));

            Fishhook.AbilityID = AbilityManager.Add(
                InfiniscryptionSpellsPlugin.OriginalPluginGuid,
                info,
                typeof(Fishhook),
                AssetHelper.LoadTexture("ability_fishhook")
            ).Id;
        }

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
