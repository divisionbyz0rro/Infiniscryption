using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using APIPlugin;
using System.Linq;

namespace Infiniscryption.Curses.Cards
{
    public class Bitten : AbilityBehaviour
    {
        // This ability does **absolutely nothing**
        // The purposes is to be able to track characteristics of a card when you save and load a run
        // Thanks to the way that the game stores saved decks.

        public static AbilityIdentifier Identifier 
        { 
            get
            {
                return AbilityIdentifier.GetAbilityIdentifier("zorro.infiniscryption.sigils.bitten", "Bitten");
            }
        }
        public static Ability _ability { get; private set; }
        public override Ability Ability => _ability;

        public static void RegisterCardAndAbilities(Harmony harmony)
        {
            AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
                "Bitten",
                "This card has been bitten."
            );
            info.canStack = false;
            info.powerLevel = -2;
            info.opponentUsable = false;
            info.passive = false;
            info.metaCategories = new List<AbilityMetaCategory>() { };

            NewAbility ability = new NewAbility(
                info,
                typeof(Bitten),
                Resources.Load<Texture2D>("art/cards/abilityicons/ability_deathtouch"),
                Identifier
            );

            Bitten._ability = ability.ability;

            // Patch this class
            harmony.PatchAll(typeof(Bitten));
        }

        // This patch makes the card have the right background
        [HarmonyPatch(typeof(Card), "ApplyAppearanceBehaviours")]
        [HarmonyPostfix]
        public static void SpellBackground(ref Card __instance)
        {
            if (__instance.Info.Abilities.Any(sp => (int)sp == (int)_ability))
            {
                __instance.gameObject.AddComponent<BittenCardAppearance>().ApplyAppearance();
            }
        }

        [HarmonyPatch(typeof(CardAbilityIcons), "GetDistinctShownAbilities")]
        [HarmonyPostfix]
        public static void DontShowThisIcon(ref List<Ability> __result)
        {
            __result.Remove(_ability);
        }
    }
}
