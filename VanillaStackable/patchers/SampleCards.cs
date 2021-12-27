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
using APIPlugin;

namespace Infiniscryption.VanillaStackable.Patchers
{
    public static class SampleCards
    {
        internal static void RegisterCustomCards(Harmony harmony)
        {
            // Create the Kettle
            NewCard.Add(
                "Super_Sharp_Porcupine",
                "Smelly Pokeypine",
                0, 10,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "Super sharp!",
                bloodCost: 0,
                defaultTex: Resources.Load<Texture2D>("art/cards/portraits/portrait_porcupine"),
                abilities: new List<Ability>() { Ability.Sharp, Ability.DebuffEnemy }
            );

            harmony.PatchAll(typeof(SampleCards));
        }

        [HarmonyPatch(typeof(RunState), "InitializeStarterDeckAndItems")]
        [HarmonyPostfix]
        public static void AddPokeypine()
        {
            for (int i = 0; i < 8; i++)
                RunState.Run.playerDeck.AddCard(CardLoader.GetCardByName("Super_Sharp_Porcupine"));
        }
    }
}