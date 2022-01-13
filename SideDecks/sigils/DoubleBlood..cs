using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Card;

namespace Infiniscryption.SideDecks.Sigils
{
    public class DoubleBlood : AbilityBehaviour
    {
		public override Ability Ability => AbilityID;
        public static Ability AbilityID { get; private set; }

        // This ability...does nothing? That's right!

        public static void Register(Harmony harmony)
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Acceptable Sacrifice";
            info.rulebookDescription = "Sacrificing [creature] will count as if two creatures were sacrificed";
            info.canStack = true;
            info.powerLevel = 2;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { AbilityMetaCategory.Part1Rulebook };
            info.SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_doubleblood"));

            DoubleBlood.AbilityID = AbilityManager.Add(
                SideDecksPlugin.PluginGuid,
                info,
                typeof(DoubleBlood),
                AssetHelper.LoadTexture("ability_doubleblood")
            ).Id;

            harmony.PatchAll(typeof(DoubleBlood));
        }

        [HarmonyPatch(typeof(BoardManager), "GetValueOfSacrifices")]
        [HarmonyPostfix]
        public static void AdjustForDoubleBlood(List<CardSlot> sacrifices, ref int __result)
        {
            foreach (CardSlot slot in sacrifices)
                if (slot != null && slot.Card != null && slot.Card.gameObject.GetComponent<DoubleBlood>() != null)
                    __result++;
        }
    }
}