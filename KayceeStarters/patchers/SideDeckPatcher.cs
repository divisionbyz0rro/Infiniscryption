using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption.Core.Helpers;
using Infiniscryption.KayceeStarters.Cards;

namespace Infiniscryption.KayceeStarters.Patchers
{
    public static class SideDeckPatcher
    {
        public const int SIDE_DECK_SIZE = 10;

        public static string SelectedSideDeck
        {
            get 
            { 
                string sideDeck = SaveGameHelper.GetValue("SideDeck.SelectedDeck");
                if (String.IsNullOrEmpty(sideDeck))
                    return CustomCards.SideDecks.Squirrel.ToString();

                return sideDeck; 
            }
            set { SaveGameHelper.SetValue("SideDeck.SelectedDeck", value.ToString()); }
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
            if (SelectedSideDeck != CustomCards.SideDecks.Squirrel.ToString())
            {
                __result = __result - 5;
            }
        }
    }
}