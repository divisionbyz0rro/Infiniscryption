using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using InscryptionAPI.Card;

namespace Infiniscryption.Curses.Cards
{
    public class Bitten : AbilityBehaviour
    {
        // This ability does **absolutely nothing**
        // The purposes is to be able to track characteristics of a card when you save and load a run
        // Thanks to the way that the game stores saved decks.
        public static Ability AbilityID { get; private set; }
        public override Ability Ability => AbilityID;

        public static void RegisterCardAndAbilities(Harmony harmony)
        {
            AbilityInfo info = ScriptableObject.CreateInstance<AbilityInfo>();
            info.rulebookName = "Bitten By Predator";
            info.rulebookDescription = "This card was bitten by a predator";
            info.canStack = false;
            info.powerLevel = -2;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { };

            Bitten.AbilityID = AbilityManager.Add(
                CursePlugin.PluginGuid,
                info,
                typeof(Bitten),
                Resources.Load<Texture2D>("art/cards/abilityicons/ability_deathtouch")
            ).Id;

            // Patch this class
            harmony.PatchAll(typeof(Bitten));
        }

        // This patch makes the card have the right background
        [HarmonyPatch(typeof(Card), "ApplyAppearanceBehaviours")]
        [HarmonyPostfix]
        public static void SpellBackground(ref Card __instance)
        {
            if (__instance.Info.Abilities.Any(sp => sp == AbilityID))
            {
                __instance.gameObject.AddComponent<BittenCardAppearance>().ApplyAppearance();
            }
        }

        [HarmonyPatch(typeof(CardAbilityIcons), "GetDistinctShownAbilities")]
        [HarmonyPostfix]
        public static void DontShowThisIcon(ref List<Ability> __result)
        {
            __result.Remove(AbilityID);
        }
    }
}
