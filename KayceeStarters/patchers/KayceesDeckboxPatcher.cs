using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Infiniscryption.Core.Helpers;
using Infiniscryption.KayceeStarters.Cards;
using Infiniscryption.KayceeStarters.UserInterface;
using InscryptionAPI.Saves;

namespace Infiniscryption.KayceeStarters.Patchers
{
    public static class KayceesDeckboxPatcher
    {
        public const int SIDE_DECK_SIZE = 10;

        public static string SelectedSideDeck
        {
            get 
            { 
                string sideDeck = ModdedSaveManager.SaveData.GetValue(InfiniscryptionKayceeStartersPlugin.PluginGuid, "SideDeck.SelectedDeck");
                if (String.IsNullOrEmpty(sideDeck))
                    return CustomCards.SideDecks.Squirrel.ToString();

                return sideDeck; 
            }
            set { ModdedSaveManager.SaveData.SetValue(InfiniscryptionKayceeStartersPlugin.PluginGuid, "SideDeck.SelectedDeck", value.ToString()); }
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
            if (SideDeckSelectorScreen.Instance != null)
                __result += SideDeckSelectorScreen.Instance.SideDeckPoints;

            if (NumberOfPeltsSelectionScreen.Instance != null)
                __result += NumberOfPeltsSelectionScreen.Instance.deckScore;
        }

        [HarmonyPatch(typeof(AscensionSaveData), "NewRun")]
        [HarmonyPostfix]
        public static void ResetDeck(ref AscensionSaveData __instance)
        {
            if (NumberOfPeltsSelectionScreen.Instance != null)
            {
                __instance.currentRun.playerDeck = new DeckInfo();
                foreach (CardInfo card in NumberOfPeltsSelectionScreen.Instance.currentDeck)
                {
                    __instance.currentRun.playerDeck.AddCard(card);
                }
            }
        }

        [HarmonyPatch(typeof(MapGenerator), "ForceFirstNodeTraderForAscension")]
        [HarmonyPostfix]
        public static void OverrideTraderBehavior(ref bool __result, int rowIndex)
        {
            __result = SaveFile.IsAscension && rowIndex == 1 && RunState.Run.regionTier == 0 && 
                       AscensionSaveData.Data.currentRun.playerDeck.Cards.Where(c => c.name.ToLowerInvariant().Contains("pelt")).Count() > 0;
        }
    }
}