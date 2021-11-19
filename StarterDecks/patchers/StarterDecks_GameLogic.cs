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
using Infiniscryption.StarterDecks;
using Infiniscryption.StarterDecks.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Infiniscryption.StarterDecks.Patchers
{
    public static partial class DeckConstructionPatches
    {
        // This class contains all of the patches that affect the actual gameplay
        // Deck construction, etc
        [HarmonyPatch(typeof(DeckInfo), "InitializeAsPlayerDeck")]
        [HarmonyPrefix]
        public static bool PreventDeckInitialization(ref DeckInfo __instance)
        {
            // This patch starts you with an empty deck.
            // You will select your deck on the first node of the map.

            // Be a good citizen - if you haven't completed the tutorial, this should have no effect:
            if (StoryEventsData.EventCompleted(StoryEvent.TutorialRunCompleted))
            {
                InfiniscryptionStarterDecksPlugin.Log.LogInfo($"In deck init prefix");
                return false;
            } else {
                return true;
            }
        }

        // Here, we establish the starter decks
        // They either come from the save file, or they come from configuration
        // By putting them in the save file, this lets us modify them through the
        // course of a save (i.e., letting players level up their starter decks)
        public static List<string> StarterDecks
        {
            get 
            {
                string starterDecks = SaveGameHelper.GetValue("StarterDecks");

                if (starterDecks == default(string))
                {
                    string[] retval = InfiniscryptionStarterDecksPlugin.DeckSpecs;
                    SaveGameHelper.SetValue("StarterDecks", string.Join("|", retval));
                    return retval.ToList();
                }

                return starterDecks.Split('|').ToList();
            }
        }

        public static List<string> StarterDeckEvolutions
        {
            get 
            {
                string starterDecks = SaveGameHelper.GetValue("StarterDeckEvolutions");

                if (starterDecks == default(string))
                {
                    string[] retval = InfiniscryptionStarterDecksPlugin.DeckEvolutions;
                    SaveGameHelper.SetValue("StarterDeckEvolutions", string.Join("|", retval));
                    return retval.ToList();
                }

                return starterDecks.Split('|').ToList();
            }
        }

        public static List<int> DeckEvolutionProgress
        {
            get 
            {
                string evolutions = SaveGameHelper.GetValue("StarterDeckProgress");

                if (evolutions == default(string))
                {
                    int[] retval = new int[InfiniscryptionStarterDecksPlugin.DeckSpecs.Length];
                    SaveGameHelper.SetValue("StarterDeckProgress", string.Join("|", retval));
                    return retval.ToList();
                }

                return evolutions.Split('|').Select(str => int.Parse(str)).ToList();
            }
        }

        public static void UpdateDeckEvolutionProgress(int deckId, int newLevel)
        {
            List<int> curProgress = DeckEvolutionProgress;
            curProgress[deckId] = newLevel;
            SaveGameHelper.SetValue("StarterDeckProgress", string.Join("|", curProgress));
        }

        [HarmonyPatch(typeof(DeckInfo), "AddCard")]
        [HarmonyPrefix]
        public static void LogNewCard(CardInfo card)
        {
            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Adding card {card.name} with {card.Mods.Count} mods");
        }
            
        [HarmonyPatch(typeof(PaperGameMap), "TryInitializeMapData")]
        [HarmonyPrefix]
        public static void StartWithTribeSelection(ref PaperGameMap __instance)
        {
            // This patch ensures that the first node of the map is always
            // a tribe selection node. It also sets up the cards that you can
            // select from to start your deck

            // Be a good citizen - if you haven't completed the tutorial, this should have no effect:
            if (StoryEventsData.EventCompleted(StoryEvent.TutorialRunCompleted))
            {
                if (RunState.Run.map == null) // Only do this when the map is empty
                {
                    InfiniscryptionStarterDecksPlugin.Log.LogInfo($"In map initialization");
                    PredefinedNodes nodes = ScriptableObject.CreateInstance<PredefinedNodes>();
                    nodes.nodeRows.Add(new List<NodeData>() { new NodeData() });

                    CardChoicesNodeData tribeNode = new CardChoicesNodeData();
                    tribeNode.choicesType = CardChoicesType.Tribe;
                    tribeNode.overrideChoices = new List<CardChoice>();

                    // Set up the decks from configuration
                    for (int i = 0; i < StarterDecks.Count; i++)
                    {
                        List<CardInfo> deck = CardManagementHelper.EvolveDeck(i);
                        InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Adding choice {i}: {deck[0].name} with {deck[0].Mods.Count} mods");
                        tribeNode.overrideChoices.Add(new CardChoice{CardInfo = deck[0]});
                    }

                    nodes.nodeRows.Add(new List<NodeData>() { tribeNode });
                    __instance.PredefinedNodes = nodes;
                }
            }
        }

        [HarmonyPatch(typeof(CardSingleChoicesSequencer), "AddChosenCardToDeck")]
        [HarmonyPostfix]
        public static void BuildEntireStartingDeck(ref CardSingleChoicesSequencer __instance)
        {
            // This turns out to be pretty simple.
            // If the current deck has only a single card, we know that the user has just
            // selected their starting card.
            // So we build the starter deck for them!
            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"In starter deck initializer");

            // We don't have to do a tutorial check because your deck has multiple cards in it already
            // during te tutorial. 
            DeckInfo deck = RunState.Run.playerDeck;
            if (deck.Cards.Count == 1)
            {
                // We need to add the necessary cards
                // Set up the decks from configuration
                for (int i = 0; i < StarterDecks.Count; i++)
                {
                    List<CardInfo> evolvedDeck = CardManagementHelper.EvolveDeck(StarterDecks[i], StarterDeckEvolutions[i], DeckEvolutionProgress[i]);
                    if (deck.Cards[0].name == evolvedDeck[0].name)
                    {
                        deck.AddCard(evolvedDeck[1]);
                        deck.AddCard(evolvedDeck[2]);
                        deck.AddCard(evolvedDeck[3]);
                        
                        break;
                    }
                }
            }
        }
    }
}