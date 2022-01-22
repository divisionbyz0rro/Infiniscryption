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
        public const string UNC_TOKEN = "P03_Draft_Token_Uncommon";
        public const string RARE_DRAFT_TOKEN = "P03_Draft_Token_Rare";
        public const string GOLLYCOIN = "P03_GollyCoin";
        public const string BLOCKCHAIN = "P03_Blockchain";
        public const string NFT = "P03_NFT";
        public const string OLD_DATA = "P03_OLD_DATA";
        public const string VIRUS_SCANNER = "P03_VIRUS_SCANNER";

        private readonly static List<CardMetaCategory> GBC_RARE_PLAYABLES = new() { CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable, CardMetaCategory.Rare, CardMetaCategory.ChoiceNode };

        internal static Sprite GetSprite(string textureKey)
        {
            return Sprite.Create(
                AssetHelper.LoadTexture(textureKey),
                new Rect(0f, 0f, 114f, 94f),
                new Vector2(0.5f, 0.5f)
            );
        }

        public static CardInfo ModifyCardForAscension(CardInfo info)
        {
            if (info.name.ToLowerInvariant().StartsWith("sentinel") || info.name == "TechMoxTriple")
                info.mods.Add(new() { gemify = true });

            return info;
        }

        private static void UpdateExistingCard(string name, string textureKey, string pixelTextureKey, string regionCode, string decalTextureKey)
        {
            if (string.IsNullOrEmpty(name))
                return;

            CardInfo card = CardManager.BaseGameCards.FirstOrDefault(c => c.name == name);
            if (card == null)
            {
                P03Plugin.Log.LogInfo($"COULD NOT MODIFY CARD {name} BECAUSE I COULD NOT FIND IT");
                return;
            }

            P03Plugin.Log.LogInfo($"MODIFYING {name} -> {card.displayedName}");

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
            LoseOnDeath.Register();

            // Load the custom cards from the CSV database
            string database = AssetHelper.GetResourceString("card_database", "csv");
            string[] lines = database.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines.Skip(1))
            {
                string[] cols = line.Split(new char[] { ',' }, StringSplitOptions.None);
                //InfiniscryptionP03Plugin.Log.LogInfo($"I see line {string.Join(";", cols)}");
                UpdateExistingCard(cols[0], cols[1], cols[2], cols[3], cols[4]);
            }

            // Handle the triplemox portrait special
            CardManager.BaseGameCards.CardByName("TechMoxTriple")
                .AddAppearances(HighResAlternatePortrait.ID)
                .SetAltPortrait(AssetHelper.LoadTexture("portrait_triplemox_color"));

            // This creates all the sprites behind the scenes so we're ready to go
            RandomStupidAssApePortrait.RandomApePortrait.GenerateApeSprites();

            // Update the librarian to display its size
            CardManager.BaseGameCards.CardByName("Librarian").AddAppearances(LibrarianSizeTitle.ID);

            AbilityManager.AllAbilityInfos.AbilityByID(Ability.DrawVesselOnHit).SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_drawvessel"));
            AbilityManager.AllAbilityInfos.AbilityByID(Ability.Sniper).SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_sniper"));
            AbilityManager.AllAbilityInfos.AbilityByID(Ability.RandomAbility).SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_random"));
            AbilityManager.AllAbilityInfos.AbilityByID(Ability.DrawRandomCardOnDeath).SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_randomcard"));
            AbilityManager.AllAbilityInfos.AbilityByID(Ability.LatchDeathShield).SetPixelAbilityIcon(AssetHelper.LoadTexture("pixelability_shieldlatch"));

            CardManager.New(DRAFT_TOKEN, "Basic Token", 0, 1)
                .SetPortrait(AssetHelper.LoadTexture("portrait_drafttoken"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixel_drafttoken"));

            CardManager.New(UNC_TOKEN, "Improved Token", 0, 2)
                .SetPortrait(AssetHelper.LoadTexture("portrait_drafttoken_plus"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixel_drafttoken_plus"));

            CardManager.New(RARE_DRAFT_TOKEN, "Rare Token", 0, 3)
                .SetPortrait(AssetHelper.LoadTexture("portrait_drafttoken_plusplus"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixel_drafttoken"));

            CardManager.New(BLOCKCHAIN, "Blockchain", 0, 5)
                .SetAltPortrait(AssetHelper.LoadTexture("portrait_blockchain"), FilterMode.Trilinear)
                .AddAbilities(Ability.ConduitNull, ConduitSpawnCrypto.AbilityID)
                .AddAppearances(HighResAlternatePortrait.ID);

            CardManager.New(GOLLYCOIN, "GollyCoin", 0, 2)
                .SetAltPortrait(AssetHelper.LoadTexture("portrait_gollycoin"), FilterMode.Trilinear)
                .AddAppearances(HighResAlternatePortrait.ID );

            CardManager.New(NFT, "Stupid-Ass Ape", 0, 1)
                .AddAppearances(RandomStupidAssApePortrait.ID);

            CardManager.New(OLD_DATA, "UNSAFE.DAT", 0, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_captivefile"))
                .AddAbilities(LoseOnDeath.AbilityID);

            CardManager.New(VIRUS_SCANNER, "VIRSCAN.EXE", 1, 7)
                .SetPortrait(AssetHelper.LoadTexture("portrait_virusscanner"))
                .AddAbilities(Ability.Deathtouch, Ability.StrafeSwap);

            // Community tribute cards
            CardManager.New("MADH95_LEFT", "MADH95", 0, 4)
                .SetAltPortrait(AssetHelper.LoadTexture("madh95-left", FilterMode.Trilinear))
                .AddAbilities(Ability.ConduitBuffAttack, Ability.Sentry)
                .AddAppearances(HighResAlternatePortrait.ID);

            CardManager.New("MADH95_RIGHT", "MADH95", 0, 4)
                .SetAltPortrait(AssetHelper.LoadTexture("madh95-right", FilterMode.Trilinear))
                .AddAbilities(Ability.ConduitNull, Ability.Sentry)
                .AddAppearances(HighResAlternatePortrait.ID);

            CardManager.New("KOPIE_SMALL", "Kopie", 2, 2)
                .SetAltPortrait(AssetHelper.LoadTexture("kopie", FilterMode.Trilinear))
                .AddAppearances(HighResAlternatePortrait.ID);

            CardManager.New("KOPIE", "Kopie", 0, 6)
                .SetAltPortrait(AssetHelper.LoadTexture("kopie", FilterMode.Trilinear))
                .AddAbilities(Ability.IceCube, Ability.WhackAMole)
                .SetIceCube("KOPIE_SMALL")
                .AddAppearances(HighResAlternatePortrait.ID);

            CardManager.New("TRIBUTE", "Tribute", 0, 1)
                .AddAppearances(HighResAlternatePortrait.ID);
        }

        [HarmonyPatch(typeof(PermaDeath), nameof(PermaDeath.OnDie))]
        [HarmonyPrefix]
        public static bool EasterEggFecundityNoPermadie(ref PermaDeath __instance)
        {
            if (SaveFile.IsAscension && (__instance.Card.HasAbility(Ability.DrawCopy) || __instance.Card.HasAbility(Ability.DrawCopyOnDeath)))
                return false;

            return true;
        }
    }
}