using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using System;
using InscryptionAPI.Saves;
using InscryptionAPI.Card;
using System.Linq;
using InscryptionAPI.Guid;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.SideDecks.Patchers
{
    public static class SideDeckManager
    {
        public static Trait BACKWARDS_COMPATIBLE_SIDE_DECK_MARKER = (Trait)5103;
        public static CardMetaCategory SIDE_DECK = GuidManager.GetEnumValue<CardMetaCategory>(SideDecksPlugin.PluginGuid, "SideDeck");
        
        public static string SelectedSideDeck
        {
            get 
            { 
                string sideDeck = ModdedSaveManager.SaveData.GetValue(SideDecksPlugin.PluginGuid, "SideDeck.SelectedDeck");
                if (String.IsNullOrEmpty(sideDeck))
                    return CustomCards.SideDecks.Squirrel.ToString();

                return sideDeck; 
            }
            set { ModdedSaveManager.SaveData.SetValue(SideDecksPlugin.PluginGuid, "SideDeck.SelectedDeck", value.ToString()); }
        }

        public static int SelectedSideDeckCost
        {
            get
            {
                CardInfo info = CardManager.AllCardsCopy.CardByName(SelectedSideDeck);
                return info.GetSideDeckValue();
            }
        }

        public static CardTemple ScreenState 
        { 
            get
            {
                string value = ModdedSaveManager.SaveData.GetValue("zorro.inscryption.infiniscryption.p03kayceerun", "ScreenState");
                if (string.IsNullOrEmpty(value))
                    return CardTemple.Nature;

                return (CardTemple)Enum.Parse(typeof(CardTemple), value);
            }
        }

        public const int SIDE_DECK_SIZE = 10;

        public enum SideDecks
        {
            Squirrel = 0,
            INF_Bee_Drone = 1,
            INF_Ant_Worker = 2,
            INF_Puppy = 3,
            INF_Spare_Tentacle = 4,
            INF_One_Eyed_Goat = 5
        }

        public static List<string> GetAllValidSideDeckCards()
        {
            if (!AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.SubmergeSquirrels))
            {
                if (SceneLoader.ActiveSceneName.ToLowerInvariant().Contains("part1") || ScreenState == CardTemple.Nature)
                {
                    SideDecksPlugin.Log.LogInfo($"Getting cards: Screenstate {ScreenState}, IsPart1 {SceneLoader.ActiveSceneName.ToLowerInvariant().Contains("part1")}");
                    return CardManager.AllCardsCopy.Where(card => card.metaCategories.Contains(SIDE_DECK) && card.temple == CardTemple.Nature)
                                               .Select(card => card.name).ToList();
                }
                else if (SaveManager.saveFile.IsPart3 || ScreenState == CardTemple.Tech)
                {
                    return CardManager.AllCardsCopy.Where(card => card.metaCategories.Contains(SIDE_DECK) && card.temple == CardTemple.Tech)
                                               .Select(card => card.name).ToList();
                }
                else if (SaveManager.saveFile.IsGrimora || ScreenState == CardTemple.Undead)
                {
                    return new() { "Skeleton" };
                }
            }
            else
            {
                if (SceneLoader.ActiveSceneName.ToLowerInvariant().Contains("part1") || ScreenState == CardTemple.Nature)
                {
                    return new() { "AquaSquirrel" };
                }
                else if (SaveManager.saveFile.IsPart3 || ScreenState == CardTemple.Tech)
                {
                    return new() { "EmptyVesselSubmerge" };
                }
                else if (SaveManager.saveFile.IsGrimora || ScreenState == CardTemple.Undead)
                {
                    return new() { "Skeleton" };
                }
            }
            SideDecksPlugin.Log.LogInfo($"Fallback: giving only a squirrel. Screenstate {ScreenState}, IsPart1 {SaveManager.saveFile.IsPart1}");
            return new() { "Squirrel" };
        }

        [HarmonyPatch(typeof(Part1CardDrawPiles), "SideDeckData", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool ReplaceSideDeck(ref List<CardInfo> __result)
        {
            __result = new List<CardInfo>();
            string selectedDeck = SelectedSideDeck;
            for (int i = 0; i < SIDE_DECK_SIZE; i++)
                __result.Add(CardLoader.GetCardByName(selectedDeck));

            return false;
        }

        [HarmonyPatch(typeof(AscensionSaveData), "GetActiveChallengePoints")]
        [HarmonyPostfix]
        public static void ReduceChallengeIfCustomSideDeckSelected(ref int __result)
        {
            __result += SelectedSideDeckCost;
        }

        [HarmonyPatch(typeof(DialogueDataUtil), "ReadDialogueData")]
        [HarmonyPostfix]
        public static void CurseDialogue()
        {
            // Here, we replace dialogue from Leshy based on the starter decks plugin being installed
            // And add new dialogue
            DialogueHelper.AddOrModifySimpleDialogEvent("SideDeckIntro", new string []
            {
                "Here you may replace the creatures in your [c:bR]side deck[c:]"
            });
        }

        [HarmonyPatch(typeof(Part3SaveData), "Initialize")]
        [HarmonyPostfix]
        private static void AddSideDeckAbilitiesWithMesh(ref Part3SaveData __instance)
        {
            if (SaveFile.IsAscension && AscensionSaveData.Data.currentRun != null)
            {
                CardInfo info = CardManager.AllCardsCopy.CardByName(SelectedSideDeck);
                foreach(Ability ab in info.Abilities)
                {
                    AbilityInfo abInfo = AbilitiesUtil.GetInfo(ab);

                    if (abInfo.mesh3D != null)
                        __instance.sideDeckAbilities.Add(ab);
                }
            }
        }

        [HarmonyPatch(typeof(Part3CardDrawPiles), nameof(Part3CardDrawPiles.AddModsToVessel))]
        [HarmonyPostfix]
        private static void AddSideDeckAbilitiesWithoutMesh(CardInfo info)
        {
            if (info != null)
            {
                CardInfo sideDeckCard = CardManager.AllCardsCopy.CardByName(SelectedSideDeck);
                foreach(Ability ab in sideDeckCard.Abilities)
                {
                    if (info.HasAbility(ab))
                        continue;

                    AbilityInfo abInfo = AbilitiesUtil.GetInfo(ab);

                    if (abInfo.mesh3D == null)
                    {
                        CardModificationInfo abMod = new(ab);
                        abMod.sideDeckMod = true;
                        info.mods.Add(abMod);
                    }
                }
            }
        }
    }
}