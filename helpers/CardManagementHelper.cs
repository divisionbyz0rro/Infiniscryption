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
using Infiniscryption;
using Infiniscryption.Helpers;
using Infiniscryption.Patchers;

namespace Infiniscryption.Helpers
{
    public static class CardManagementHelper
    {
        // The purpose of this class is to translate the card evolution
        // instructions into actual cards
        //
        // Deck evolution instructions are stored in a weird single-line
        // text instruction to make them simply fit into the game's save file
        //
        // This helper class lets us translate from that 'language' to actual cards

        public static CardInfo EvolveCard(CardInfo card, string evoCommand)
        {
            // You can have multiple commands in one evolution, combined with &
            CardInfo currentCard = card;
            foreach (string cmd in evoCommand.Split('&'))
            {
                // Replace a card?
                if (cmd[0].Equals('='))
                {
                    currentCard = CardLoader.GetCardByName(cmd.Replace("=",""));
                }
                
                // Add to a card?
                if (cmd[0].Equals('+'))
                {
                    string cmdInner = cmd.Substring(1, cmd.Length - 2);
                    // We can either add Health (H), Attack (A), or a Sigil (S)
                    if (cmd[cmd.Length - 1] == 'H')
                    {
                        // Add health
                        currentCard.Mods.Add(new CardModificationInfo(0, int.Parse(cmdInner)));
                    }
                    if (cmd[cmd.Length - 1] == 'A')
                    {
                        // Add attack
                        currentCard.Mods.Add(new CardModificationInfo(int.Parse(cmdInner), 0));
                    }
                    if (cmd[cmd.Length - 1] == 'S')
                    {
                        // Add a Sigil
                        currentCard.Mods.Add(new CardModificationInfo((Ability)Enum.Parse(typeof(Ability), cmdInner)));
                        currentCard.Mods[currentCard.Mods.Count - 1].fromCardMerge = true;
                    }
                    if (cmd[cmd.Length - 1] == 'B')
                    {
                        // Update blood cost
                        currentCard.Mods.Add(new CardModificationInfo { bloodCostAdjustment = int.Parse(cmdInner)});
                    }
                    if (cmd[cmd.Length - 1] == 'O')
                    {
                        // Update blood cost
                        currentCard.Mods.Add(new CardModificationInfo { bonesCostAdjustment = int.Parse(cmdInner)});
                    }
                }
            }
            return currentCard;
        }

        public static List<CardInfo> EvolveDeck(int index)
        {
            // Note that this won't give useful results if the starter decks plugin is not activated
            return EvolveDeck(
                DeckConstructionPatches.StarterDecks[index],
                DeckConstructionPatches.StarterDeckEvolutions[index],
                DeckConstructionPatches.DeckEvolutionProgress[index]
            );
        }

        public static string GetEvolutionCommand(string[] commands, int step)
        {
            // This gets the next evolution.
            // If it's in the list? Awesome. We do what the list says.
            // If it's past the list? We follow this pattern:
            //
            // +1 health to each card (1-4)
            // +1 power to each card (1-4)
            // 'Random' sigil to each card (1-4)
            // Then rotate between +1 health and +1 power.
            if (step < commands.Length)
                return commands[step];

            // Get the card index
            int cardIndex = (step - commands.Length) % 4;
            int rotationIndex = (step - commands.Length) / 4;

            if (rotationIndex == 2)
                return $"{cardIndex}+RandomAbilityS";
            if (rotationIndex == 0 || (rotationIndex > 2 && (rotationIndex - 2) % 2 == 0))
                return $"{cardIndex}+1H";
            if (rotationIndex == 1 || (rotationIndex > 2 && (rotationIndex - 2) % 2 == 1))
                return $"{cardIndex}+1A";

            // We shouldn't ever get here? But just in case:
            return $"1+1A";
        }

        public static List<CardInfo> EvolveDeck(string starterDeck, string evolutions, int stepsToEvolve)
        {
            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Asked to get the current evolution for {starterDeck}, with evolution path {evolutions} and at step {stepsToEvolve}");

            // Start by getting the starter deck.
            // We clone cards that we get off of the card info 
            List<CardInfo> retVal = starterDeck.Split(',').Select(cardname => CardLoader.GetCardByName(cardname)).ToList();

            // Step through each evolution.
            // An evolution can only affect a card at a time.
            // It can replace a card or buff a card.
            string[] allEvolutions = evolutions.Split(',');
            for (int i = 0; i < stepsToEvolve; i++)
            {
                string curEvolution = GetEvolutionCommand(allEvolutions, i);

                // The first character is the card to change
                int cardIdx = int.Parse(curEvolution[0].ToString());
                string evoCommand = curEvolution.Substring(1);

                // Evolve the card
                retVal[cardIdx] = EvolveCard(retVal[cardIdx], evoCommand);
            }

            // And we're done!
            return retVal;
        }

        public static CardInfo GetNextEvolution(int index)
        {
            // Note that this won't give useful results if the starter decks plugin is not activated
            return GetNextEvolution(
                DeckConstructionPatches.StarterDecks[index],
                DeckConstructionPatches.StarterDeckEvolutions[index],
                DeckConstructionPatches.DeckEvolutionProgress[index]
            );
        }

        public static CardInfo GetNextEvolution(string starterDeck, string evolutions, int stepsToEvolve)
        {
            InfiniscryptionStarterDecksPlugin.Log.LogInfo($"Asked to get the next evolution for {starterDeck}, with evolution path {evolutions} and at step {stepsToEvolve}");

            List<CardInfo> evolvedDeck = EvolveDeck(starterDeck, evolutions, stepsToEvolve);

            // Can only do this if there's an evolution left
            string[] allEvolutions = evolutions.Split(',');
            string curEvolution = GetEvolutionCommand(allEvolutions, stepsToEvolve);

            // The first character is the card to change
            int cardIdx = int.Parse(curEvolution[0].ToString());
            string evoCommand = curEvolution.Substring(1);

            // Evolve the card and return it
            return EvolveCard(evolvedDeck[cardIdx], evoCommand);
        }

        /*
        public static void AddCardToCurrentDeck(CardInfo card)
        {
            DeckInfo deck = SaveManager.SaveFile.CurrentDeck;

            List<CardModificationInfo> mods = card.Mods;
            CardInfo rawCard = card.Clone() as CardInfo;
            deck.AddCard(rawCard);

            foreach (var mod in mods)
                deck.ModifyCard(rawCard, mod);
        }
        */
    }
}