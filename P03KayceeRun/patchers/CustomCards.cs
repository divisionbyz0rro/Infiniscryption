using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using System;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Guid;
using System.Linq;
using Infiniscryption.P03KayceeRun.Cards;
using InscryptionAPI.Card;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class CustomCards
    {
        public static readonly CardMetaCategory NeutralRegion = (CardMetaCategory)GuidManager.GetEnumValue<CardMetaCategory>(P03Plugin.PluginGuid, "NeutralRegionCards");
        public static readonly CardMetaCategory WizardRegion = (CardMetaCategory)GuidManager.GetEnumValue<CardMetaCategory>(P03Plugin.PluginGuid, "WizardRegionCards");
        public static readonly CardMetaCategory TechRegion = (CardMetaCategory)GuidManager.GetEnumValue<CardMetaCategory>(P03Plugin.PluginGuid, "TechRegionCards");
        public static readonly CardMetaCategory NatureRegion = (CardMetaCategory)GuidManager.GetEnumValue<CardMetaCategory>(P03Plugin.PluginGuid, "NatureRegionCards");
        public static readonly CardMetaCategory UndeadRegion = (CardMetaCategory)GuidManager.GetEnumValue<CardMetaCategory>(P03Plugin.PluginGuid, "UndeadRegionCards");

        public const string DRAFT_TOKEN = "P03_Draft_Token";
        public const string RARE_DRAFT_TOKEN = "P03_Draft_Token_Rare";
        public const string GOLLYCOIN = "P03_GollyCoin";
        public const string BLOCKCHAIN = "P03_Blockchain";
        public const string NFT = "P03_NFT";

        private readonly static List<CardMetaCategory> GBC_RARE_PLAYABLES = new() { CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable, CardMetaCategory.Rare, CardMetaCategory.ChoiceNode };

        internal static Sprite GetSprite(string textureKey)
        {
            return Sprite.Create(
                AssetHelper.LoadTexture(textureKey),
                new Rect(0f, 0f, 114f, 94f),
                new Vector2(0.5f, 0.5f)
            );
        }

        private static void UpdateExistingCard(string name, string textureKey, string pixelTextureKey, string regionCode, string decalTextureKey)
        {
            if (string.IsNullOrEmpty(name))
                return;

            CardInfo card = CardManager.AllCards.CardByName(name);
            if (card == null)
                return;


            if (!string.IsNullOrEmpty(textureKey))
                card.SetPortrait(AssetHelper.LoadTexture(textureKey));

            if (!string.IsNullOrEmpty(pixelTextureKey))
                card.SetPixelPortrait(AssetHelper.LoadTexture(pixelTextureKey));

            if (!string.IsNullOrEmpty(regionCode))
            {
                card.metaCategories = card.metaCategories ?? new();
                card.metaCategories.Add(GuidManager.GetEnumValue<CardMetaCategory>(P03Plugin.PluginGuid, regionCode));
            }

            if (!string.IsNullOrEmpty(decalTextureKey))
                card.decals = new() { AssetHelper.LoadTexture(decalTextureKey) };
        }

        internal static void RegisterCustomCards(Harmony harmony)
        {
            // Register all the custom ability
            ConduitSpawnCrypto.Register();
            HighResAlternatePortrait.Register();
            RandomStupidAssApePortrait.Register();

            // Load the custom cards from the CSV database
            string database = AssetHelper.GetResourceString("card_database", "csv");
            string[] lines = database.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines.Skip(1))
            {
                string[] cols = line.Split(new char[] { ',' }, StringSplitOptions.None);
                //InfiniscryptionP03Plugin.Log.LogInfo($"I see line {string.Join(";", cols)}");
                UpdateExistingCard(cols[0], cols[1], cols[2], cols[3], cols[4]);
            }

            // This creates all the sprites behind the scenes so we're ready to go
            RandomStupidAssApePortrait.RandomApePortrait.GenerateApeSprites();

            AbilityManager.AllAbilityInfos.AbilityByID(Ability.DrawVesselOnHit).SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_drawvessel"));
            AbilityManager.AllAbilityInfos.AbilityByID(Ability.Sniper).SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_sniper"));
            AbilityManager.AllAbilityInfos.AbilityByID(Ability.RandomAbility).SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_random"));
            AbilityManager.AllAbilityInfos.AbilityByID(Ability.DrawRandomCardOnDeath).SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_randomcard"));
            AbilityManager.AllAbilityInfos.AbilityByID(Ability.LatchDeathShield).SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_shieldlatch"));


            CardInfo card = ScriptableObject.CreateInstance<CardInfo>();
            card.name = DRAFT_TOKEN;
            card.SetBasic("Draft Token", 0, 1);
            card.temple = CardTemple.Tech;
            card.metaCategories = new();
            card.SetPortrait(AssetHelper.LoadTexture("portrait_drafttoken"));
            card.SetPixelPortrait(AssetHelper.LoadTexture("pixel_drafttoken"));
            CardManager.Add(card);

            card = ScriptableObject.CreateInstance<CardInfo>();
            card.name = RARE_DRAFT_TOKEN;
            card.SetBasic("Rare Draft Token", 0, 1);
            card.temple = CardTemple.Tech;
            card.metaCategories = new();
            card.SetPortrait(AssetHelper.LoadTexture("portrait_drafttoken_plusplus"));
            card.SetPixelPortrait(AssetHelper.LoadTexture("pixel_drafttoken"));
            CardManager.Add(card);

            card = ScriptableObject.CreateInstance<CardInfo>();
            card.name = BLOCKCHAIN;
            card.SetBasic("Blockchain", 0, 5);
            card.temple = CardTemple.Tech;
            card.metaCategories = new();
            card.SetAltPortrait(AssetHelper.LoadTexture("portrait_blockchain"), FilterMode.Trilinear);
            card.abilities = new() { Ability.DebuffEnemy, Ability.DeathShield, Ability.ConduitNull, ConduitSpawnCrypto.AbilityID };
            card.appearanceBehaviour = new () { HighResAlternatePortrait.ID };
            CardManager.Add(card);

            card = ScriptableObject.CreateInstance<CardInfo>();
            card.name = GOLLYCOIN;
            card.SetBasic("GollyCoin", 0, 2);
            card.temple = CardTemple.Tech;
            card.metaCategories = new();
            card.SetAltPortrait(AssetHelper.LoadTexture("portrait_gollycoin"), FilterMode.Trilinear);
            card.appearanceBehaviour = new () { HighResAlternatePortrait.ID };
            CardManager.Add(card);

            card = ScriptableObject.CreateInstance<CardInfo>();
            card.name = NFT;
            card.SetBasic("Stupid-Ass Ape", 0, 1);
            card.temple = CardTemple.Tech;
            card.metaCategories = new();
            card.appearanceBehaviour = new () { RandomStupidAssApePortrait.ID };
            CardManager.Add(card);
        }

        [HarmonyPatch(typeof(Card), "ApplyAppearanceBehaviours")]
        [HarmonyPostfix]
        public static void SpellBackground(ref Card __instance)
        {
            if (__instance.Info.name == BLOCKCHAIN || __instance.Info.name == GOLLYCOIN)
            {
                __instance.gameObject.AddComponent<HighResAlternatePortrait>().ApplyAppearance();
            }

            if (__instance.Info.name == NFT)
            {
                __instance.gameObject.AddComponent<RandomStupidAssApePortrait>().ApplyAppearance();
            }
        }
    }
}