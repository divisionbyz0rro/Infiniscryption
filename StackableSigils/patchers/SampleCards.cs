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

namespace Infiniscryption.Spells.Patchers
{
    public static class SampleCards
    {
        internal static void RegisterCustomCards()
        {
            // Create the Kettle
            NewCard.Add(
                "Super_Sharp_Porcupine",
                "Smelly Pokeypine",
                1, 2,
                new List<CardMetaCategory>() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer },
                CardComplexity.Advanced,
                CardTemple.Nature,
                "Super sharp!",
                bloodCost: 2,
                defaultTex: Resources.Load<Texture2D>("art/cards/portraits/portrait_porcupine"),
                abilities: new List<Ability>() { Ability.Sharp, Ability.Sharp, Ability.DebuffEnemy, Ability.DebuffEnemy }
            );
        }
    }
}