using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Saves;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infiniscryption.SideDecks.Patchers
{
    public static class SideDeckManager
    {
        public static Trait BACKWARDS_COMPATIBLE_SIDE_DECK_MARKER = (Trait)5103;
        public static CardMetaCategory SIDE_DECK = GuidManager.GetEnumValue<CardMetaCategory>(SideDecksPlugin.PluginGuid, "SideDeck");

        /// <summary>
        /// Hook into this event to modify which cards are valid side deck selections based on context.
        /// </summary>
        public static event Action<CardTemple, List<string>> ModifyValidSideDeckCards;

        public static string SelectedSideDeck
        {
            get
            {
                string sideDeck = ModdedSaveManager.SaveData.GetValue(SideDecksPlugin.PluginGuid, $"SideDeck.{ScreenState}.SelectedDeck");
                if (String.IsNullOrEmpty(sideDeck))
                    return "Squirrel";

                return sideDeck;
            }
            internal set { ModdedSaveManager.SaveData.SetValue(SideDecksPlugin.PluginGuid, $"SideDeck.{ScreenState}.SelectedDeck", value.ToString()); }
        }

        public static int SelectedSideDeckCost
        {
            get
            {
                CardInfo info = CardManager.AllCardsCopy.CardByName(SelectedSideDeck);
                return info.GetSideDeckValue();
            }
        }

        // Mod compatibility
        private const string GRIMORA_MOD = "arackulele.inscryption.grimoramod";
        private const string P03_MOD = "zorro.inscryption.infiniscryption.p03kayceerun";
        private const string MAGNIFICUS_MOD = "silenceman.inscryption.magnificusmod";

        private static readonly AscensionChallenge LEEPBOT_SIDEDECK = GuidManager.GetEnumValue<AscensionChallenge>("zorro.inscryption.infiniscryption.p03kayceerun", "LeepbotSidedeck");

        internal static Dictionary<string, string> AcceptedScreenStates = new()
        {
            { P03_MOD, P03_MOD },
            { GRIMORA_MOD, GRIMORA_MOD },
            { MAGNIFICUS_MOD, $"{MAGNIFICUS_MOD}starterdecks" }
        };

        internal static CardTemple ScreenState
        {
            get
            {
                Scene activeScene = SceneManager.GetActiveScene();
                if (activeScene != null && !string.IsNullOrEmpty(activeScene.name))
                {
                    string sceneName = activeScene.name.ToLowerInvariant();
                    if (sceneName.Contains("magnificus"))
                        return CardTemple.Wizard;
                    if (sceneName.Contains("part3"))
                        return CardTemple.Tech;
                    if (sceneName.Contains("grimora"))
                        return CardTemple.Undead;
                    if (sceneName.Contains("part1"))
                        return CardTemple.Nature;
                }

                foreach (string guid in AcceptedScreenStates.Keys)
                {
                    if (!Chainloader.PluginInfos.ContainsKey(guid))
                        continue;

                    string value = ModdedSaveManager.SaveData.GetValue(AcceptedScreenStates[guid], "ScreenState");
                    if (string.IsNullOrEmpty(value))
                        continue;

                    return (CardTemple)Enum.Parse(typeof(CardTemple), value);
                }

                return CardTemple.Nature;
            }
        }

        public const int SIDE_DECK_SIZE = 10;

        public static List<string> GetAllValidSideDeckCards()
        {
            List<string> allSideDeckCards = CardManager.AllCardsCopy
                                               .Where(card => card.metaCategories.Contains(SIDE_DECK)
                                                              && card.temple == ScreenState)
                                               .Select(card => card.name).ToList();

            if (AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.SubmergeSquirrels))
            {
                if (ScreenState == CardTemple.Nature)
                    allSideDeckCards = new() { "AquaSquirrel" };
                else if (ScreenState == CardTemple.Tech)
                    allSideDeckCards = new() { SideDecksPlugin.CardPrefix + "_EmptyVesselSubmerge" };
            }

            if (ScreenState == CardTemple.Tech)
            {
                if (AscensionSaveData.Data.ChallengeIsActive(LEEPBOT_SIDEDECK))
                    allSideDeckCards.RemoveAll(s => s.Contains("EmptyVessel"));
                else
                    allSideDeckCards.RemoveAll(s => s.Contains("LeapBot"));
            }


            ModifyValidSideDeckCards?.Invoke(ScreenState, allSideDeckCards);

            return allSideDeckCards;
        }

        [HarmonyPatch(typeof(Part1CardDrawPiles), "SideDeckData", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool ReplaceSideDeck(ref List<CardInfo> __result)
        {
            if (SaveFile.IsAscension)
            {
                __result = new List<CardInfo>();
                string selectedDeck = SelectedSideDeck;
                for (int i = 0; i < SIDE_DECK_SIZE; i++)
                    __result.Add(CardLoader.GetCardByName(selectedDeck));

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(GrimoraCardDrawPiles), "SideDeckData", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool ReplaceGrimoraSideDeck(ref List<CardInfo> __result)
        {
            if (SaveFile.IsAscension)
            {
                __result = new List<CardInfo>();
                string selectedDeck = SelectedSideDeck;
                for (int i = 0; i < SIDE_DECK_SIZE; i++)
                    __result.Add(CardLoader.GetCardByName(selectedDeck));

                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(AscensionSaveData), "GetActiveChallengePoints")]
        [HarmonyPostfix]
        public static void ReduceChallengeIfCustomSideDeckSelected(ref int __result)
        {
            __result -= SelectedSideDeckCost;
        }

        [HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        [HarmonyPostfix]
        public static void CurseDialogue()
        {
            // Here, we replace dialogue from Leshy based on the starter decks plugin being installed
            // And add new dialogue
            DialogueHelper.AddOrModifySimpleDialogEvent("SideDeckIntro", new string[]
            {
                "Here you may replace the creatures in your [c:bR]side deck[c:]"
            });
        }

        [HarmonyPatch(typeof(Part3SaveData), "Initialize")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.VeryLow)]
        private static void AddSideDeckAbilitiesWithMesh(ref Part3SaveData __instance)
        {
            if (SaveFile.IsAscension)
            {
                CardInfo info = CardManager.AllCardsCopy.CardByName(SelectedSideDeck);
                foreach (Ability ab in info.Abilities)
                {
                    AbilityInfo abInfo = AbilitiesUtil.GetInfo(ab);

                    if (abInfo.mesh3D != null)
                        __instance.sideDeckAbilities.Add(ab);
                }
            }
        }

        [HarmonyPatch(typeof(Part3CardDrawPiles), nameof(Part3CardDrawPiles.AddModsToVessel))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.VeryLow)]
        private static void EnsureAllModsOnVessel(CardInfo info)
        {
            if (SaveFile.IsAscension)
            {
                if (info != null)
                {
                    CardInfo sideDeckCard = CardManager.AllCardsCopy.CardByName(SelectedSideDeck);

                    string currentAbilityString = String.Join(", ", info.Abilities);
                    string needsAbilityString = String.Join(", ", sideDeckCard.Abilities);
                    SideDecksPlugin.Log.LogDebug($"Side deck card has {currentAbilityString} and needs {needsAbilityString}");
                    foreach (var group in sideDeckCard.Abilities.GroupBy(a => a))
                    {
                        int currentCount = info.Abilities.Where(a => a == group.Key).Count();
                        int targetCount = group.Count();

                        if (currentCount < targetCount)
                        {
                            CardModificationInfo mod = new();
                            mod.sideDeckMod = true;
                            mod.abilities = new();
                            mod.abilities.AddRange(Enumerable.Repeat(group.Key, targetCount - currentCount));
                            info.mods.Add(mod);
                        }
                    }
                }
            }
        }
    }
}