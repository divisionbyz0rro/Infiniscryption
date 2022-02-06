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

        public static Opponent.Type ScreenState 
        { 
            get
            {
                string value = ModdedSaveManager.SaveData.GetValue("zorro.inscryption.infiniscryption.p03kayceerun", "ScreenState");
                if (string.IsNullOrEmpty(value))
                    return Opponent.Type.Default;

                return (Opponent.Type)Enum.Parse(typeof(Opponent.Type), value);
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
                if (ScreenState == Opponent.Type.Default)
                {
                    return CardManager.AllCardsCopy.Where(card => card.metaCategories.Contains(SIDE_DECK) && card.temple == CardTemple.Nature)
                                               .Select(card => card.name).ToList();
                }
                else if (ScreenState == Opponent.Type.P03Boss)
                {
                    return new() { "EmptyVessel" };
                }
                else if (ScreenState == Opponent.Type.GrimoraBoss)
                {
                    return new() { "Skeleton" };
                }
            }
            else
            {
                if (ScreenState == Opponent.Type.Default)
                {
                    return new() { "AquaSquirrel" };
                }
                else if (ScreenState == Opponent.Type.P03Boss)
                {
                    return new() { "EmptyVessel" };
                }
                else if (ScreenState == Opponent.Type.GrimoraBoss)
                {
                    return new() { "Skeleton" };
                }
            }

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
    }
}