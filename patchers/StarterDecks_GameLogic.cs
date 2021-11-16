using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption;
using Infiniscryption.Helpers;

namespace Infiniscryption.Patchers
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

        private static List<String> _starterDecks;
        public static List<String> StarterDecks
        {
            get { return _starterDecks ?? new InfiniscryptionStarterDecksPlugin().DeckSpecs; }
        }

        [HarmonyPatch(typeof(SaveManager), "LoadFromFile")]
        [HarmonyPostfix]
        public static void LoadSavedStarterDecks()
        {
            // Load the current state of the starter decks
            string deckSpec = SaveGameHelper.GetValue("StarterDecks");
            if (deckSpec != default(string))
                _starterDecks = new List<String>(deckSpec.Split('|'));
        }

        [HarmonyPatch(typeof(SaveManager), "SaveToFile")]
        [HarmonyPrefix]
        public static void SaveStarterDecks()
        {
            // Save the current state of the starter decks
            SaveGameHelper.SetValue("StarterDecks", String.Join("|", StarterDecks));
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
                InfiniscryptionStarterDecksPlugin.Log.LogInfo($"In map initialization");
                PredefinedNodes nodes = ScriptableObject.CreateInstance<PredefinedNodes>();
                nodes.nodeRows.Add(new List<NodeData>() { new NodeData() });

                CardChoicesNodeData tribeNode = new CardChoicesNodeData();
                tribeNode.choicesType = CardChoicesType.Tribe;
                tribeNode.overrideChoices = new List<CardChoice>();

                // Set up the decks from configuration
                foreach (string deckSpec in StarterDecks)
                {
                    string leader = deckSpec.Split(',')[0];
                    tribeNode.overrideChoices.Add(
                        new CardChoice{
                            CardInfo = CardLoader.GetCardByName(leader)
                        }
                    );
                }

                nodes.nodeRows.Add(new List<NodeData>() { tribeNode });
                __instance.PredefinedNodes = nodes;
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
            // during the tutorial. 
            DeckInfo deck = SaveManager.SaveFile.CurrentDeck;
            if (deck.Cards.Count == 1)
            {
                // We need to add the necessary cards
                // Set up the decks from configuration
                foreach (string deckSpec in StarterDecks)
                {
                    string[] specList = deckSpec.Split(',');
                    if (deck.Cards[0].name == specList[0])
                    {
                        deck.AddCard(CardLoader.GetCardByName(specList[1]));
                        deck.AddCard(CardLoader.GetCardByName(specList[2]));
                        deck.AddCard(CardLoader.GetCardByName(specList[3]));
                        break;
                    }
                }
            }
        }
    }
}