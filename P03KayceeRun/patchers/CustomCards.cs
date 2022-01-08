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
using InscryptionAPI.Guid;
using System.Linq;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    public static class CustomCards
    {
        public static readonly CardMetaCategory NeutralRegion = (CardMetaCategory)GuidManager.GetEnumValue<CardMetaCategory>(InfiniscryptionP03Plugin.PluginGuid, "NeutralRegionCards");
        public static readonly CardMetaCategory WizardRegion = (CardMetaCategory)GuidManager.GetEnumValue<CardMetaCategory>(InfiniscryptionP03Plugin.PluginGuid, "WizardRegionCards");
        public static readonly CardMetaCategory TechRegion = (CardMetaCategory)GuidManager.GetEnumValue<CardMetaCategory>(InfiniscryptionP03Plugin.PluginGuid, "TechRegionCards");
        public static readonly CardMetaCategory NatureRegion = (CardMetaCategory)GuidManager.GetEnumValue<CardMetaCategory>(InfiniscryptionP03Plugin.PluginGuid, "NatureRegionCards");
        public static readonly CardMetaCategory UndeadRegion = (CardMetaCategory)GuidManager.GetEnumValue<CardMetaCategory>(InfiniscryptionP03Plugin.PluginGuid, "UndeadRegionCards");

        public const string DRAFT_TOKEN = "P03_Draft_Token";
        public const string RARE_DRAFT_TOKEN = "P03_Draft_Token_Rare";

        private readonly static List<CardMetaCategory> GBC_RARE_PLAYABLES = new() { CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable, CardMetaCategory.Rare, CardMetaCategory.ChoiceNode };

        private static void UpdateExistingCard(string name, string textureKey, string pixelTextureKey, string regionCode, string decalTextureKey)
        {
            if (string.IsNullOrEmpty(name))
                return;

            CustomCard customCard = new CustomCard(name);
            CardInfo card = null;

            if (!string.IsNullOrEmpty(textureKey))
                customCard.tex = AssetHelper.LoadTexture(textureKey);

            if (!string.IsNullOrEmpty(pixelTextureKey))
                customCard.pixelTex = AssetHelper.LoadTexture(pixelTextureKey);

            if (!string.IsNullOrEmpty(regionCode))
            {
                card = card ?? CardLoader.GetCardByName(name);
                List<CardMetaCategory> cats = card.metaCategories;
                cats.Add((CardMetaCategory)GuidManager.GetEnumValue<CardMetaCategory>(InfiniscryptionP03Plugin.PluginGuid, regionCode));
                customCard.metaCategories = cats;
            }

            if (!string.IsNullOrEmpty(decalTextureKey))
                customCard.decals = new () { AssetHelper.LoadTexture(decalTextureKey) };
        }

        internal static void RegisterCustomCards(Harmony harmony)
        {
            // Load the custom cards from the CSV database
            string database = AssetHelper.GetResourceString("card_database", "csv");
            string[] lines = database.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string line in lines.Skip(1))
            {
                string[] cols = line.Split(new char[] { ',' } , StringSplitOptions.None);
                //InfiniscryptionP03Plugin.Log.LogInfo($"I see line {string.Join(";", cols)}");
                UpdateExistingCard(cols[0], cols[1], cols[2], cols[3], cols[4]);
            }


            NewCard.Add(
                DRAFT_TOKEN,
                "Draft Token",
                0, 1,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Tech,
                "It's worth a card",
                defaultTex: AssetHelper.LoadTexture("portrait_drafttoken"),
                pixelTex: AssetHelper.LoadTexture("pixel_drafttoken")
            );

            NewCard.Add(
                RARE_DRAFT_TOKEN,
                "Rare Draft Token",
                0, 2,
                new List<CardMetaCategory>() { },
                CardComplexity.Vanilla,
                CardTemple.Tech,
                "It's worth a card",
                defaultTex: AssetHelper.LoadTexture("portrait_drafttoken_plusplus"),
                pixelTex: AssetHelper.LoadTexture("pixel_drafttoken")
            );
        }
    }
}