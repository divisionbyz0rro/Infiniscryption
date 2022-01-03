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

namespace Infiniscryption.P03KayceeRun.Patchers
{
    public static class CustomCards
    {
        public const string DRAFT_TOKEN = "P03_Draft_Token";

        internal static void RegisterCustomCards(Harmony harmony)
        {
            NewCard.Add(
                DRAFT_TOKEN,
                "Draft Token",
                0, 1,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Tech,
                "It's worth a card",
                defaultTex: AssetHelper.LoadTexture("portrait_drafttoken")
            );

        }
    }
}