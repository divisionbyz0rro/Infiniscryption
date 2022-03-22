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
using Infiniscryption.PackManagement;
using System.Collections;
using System.Runtime.CompilerServices;

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

        public const string DRAFT_TOKEN = "P03KCM_Draft_Token";
        public const string UNC_TOKEN = "P03KCM_Draft_Token_Uncommon";
        public const string RARE_DRAFT_TOKEN = "P03KCM_Draft_Token_Rare";
        public const string GOLLYCOIN = "P03KCM_GollyCoin";
        public const string BLOCKCHAIN = "P03KCM_Blockchain";
        public const string NFT = "P03KCM_NFT";
        public const string OLD_DATA = "P03KCM_OLD_DATA";
        public const string VIRUS_SCANNER = "P03KCM_VIRUS_SCANNER";
        public const string CODE_BLOCK = "P03KCM_CODE_BLOCK";
        public const string CODE_BUG = "P03KCM_CODE_BUG";
        public const string PROGRAMMER = "P03KCM_PROGRAMMER";
        public const string ARTIST = "P03KCM_ARTIST";
        public const string FIREWALL = "P03KCM_FIREWALL";

        private static List<string> PackCardNames = new();

        private readonly static List<CardMetaCategory> GBC_RARE_PLAYABLES = new() { CardMetaCategory.GBCPack, CardMetaCategory.GBCPlayable, CardMetaCategory.Rare, CardMetaCategory.ChoiceNode };

        internal static Sprite GetSprite(string textureKey)
        {
            return Sprite.Create(
                AssetHelper.LoadTexture(textureKey),
                new Rect(0f, 0f, 114f, 94f),
                new Vector2(0.5f, 0.5f)
            );
        }

        [HarmonyPatch(typeof(CardLoader), nameof(CardLoader.Clone))]
        [HarmonyPostfix]
        private static void ModifyCardForAscension(ref CardInfo __result)
        {
            if (P03AscensionSaveData.IsP03Run || ScreenManagement.ScreenState == CardTemple.Tech)
            {
                if (__result.name.ToLowerInvariant().StartsWith("sentinel") || __result.name == "TechMoxTriple")
                    __result.mods.Add(new() { gemify = true });

                else if (__result.name.ToLowerInvariant().Equals("automaton"))
                    __result.energyCost = 2;

                else if (__result.name.ToLowerInvariant().Equals("thickbot"))
                    __result.baseHealth = 6;

                else if (__result.name.ToLowerInvariant().Equals("energyconduit"))
                {
                    __result.baseAttack = 0;
                    __result.abilities = new () { NewConduitEnergy.AbilityID };
                    __result.appearanceBehaviour = new(__result.appearanceBehaviour);
                    __result.appearanceBehaviour.Add(EnergyConduitAppearnace.ID);
                }
            }
        }

        private static void UpdateExistingCard(string name, string textureKey, string pixelTextureKey, string regionCode, string decalTextureKey, string colorPortraitKey)
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

            if (!string.IsNullOrEmpty(colorPortraitKey))
            {
                card.SetAltPortrait(AssetHelper.LoadTexture(colorPortraitKey));
                card.AddAppearances(HighResAlternatePortrait.ID);
            }

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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WriteP03PackInner(List<string> cardNames)
        {
            // Start by creating the pack:
            PackInfo packInfo = PackManager.GetDefaultPackInfo(CardTemple.Tech);
            packInfo.ValidFor.Add(PackInfo.PackMetacategory.LeshyPack);

            // Awesome! Since there hasn't been an error, I can start modifying cards:
            CardManager.ModifyCardList += delegate(List<CardInfo> cards)
            {
                if (P03Plugin.Initialized)
                    if (ScreenManagement.ScreenState == CardTemple.Nature && PackManager.GetActivePacks().Contains(packInfo))
                        foreach (CardInfo card in cards)
                            if (card.temple == CardTemple.Tech)
                                if (!card.metaCategories.Contains(CardMetaCategory.Rare))
                                    if (cardNames.Contains(card.name))
                                        card.AddMetaCategories(CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer);

                return cards;
            };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void WriteP03Pack()
        {
            try
            {
                WriteP03PackInner(PackCardNames);
            } 
            catch (Exception ex)
            {
                P03Plugin.Log.LogInfo("Failed to write the pack information. This probably means that the pack plugin doesn't exist; if that's the case, you can ignore this error.");
                P03Plugin.Log.LogInfo(ex);
            }
        }

        internal static void RegisterCustomCards(Harmony harmony)
        {
            // Register all the custom ability
            ConduitSpawnCrypto.Register();
            HighResAlternatePortrait.Register();
            RandomStupidAssApePortrait.Register();
            ForceRevolverAppearance.Register();
            LoseOnDeath.Register();
            NewPermaDeath.Register();
            Artist.Register();
            Programmer.Register();
            RareDiscCardAppearance.Register();
            EnergyConduitAppearnace.Register();
            NewConduitEnergy.Register();

            // Load the custom cards from the CSV database
            string database = AssetHelper.GetResourceString("card_database", "csv");
            string[] lines = database.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines.Skip(1))
            {
                string[] cols = line.Split(new char[] { ',' }, StringSplitOptions.None);
                //InfiniscryptionP03Plugin.Log.LogInfo($"I see line {string.Join(";", cols)}");
                UpdateExistingCard(cols[0], cols[1], cols[2], cols[3], cols[4], cols[6]);

                if (cols[5] == "Y")
                    PackCardNames.Add(cols[0]);
            }

            CardManager.BaseGameCards.CardByName("PlasmaGunner")
                .AddAppearances(ForceRevolverAppearance.ID);

            // This creates all the sprites behind the scenes so we're ready to go
            RandomStupidAssApePortrait.RandomApePortrait.GenerateApeSprites();

            // Update the librarian to display its size
            CardManager.BaseGameCards.CardByName("Librarian").AddAppearances(LibrarianSizeTitle.ID);

            CardManager.ModifyCardList += delegate(List<CardInfo> cards)
            {
                if (P03AscensionSaveData.IsP03Run)
                    cards.CardByName("EnergyRoller").AddMetaCategories(CardMetaCategory.Rare);

                return cards;
            };

            CardManager.New(P03Plugin.CardPrefx, DRAFT_TOKEN, "Basic Token", 0, 1)
                .SetPortrait(AssetHelper.LoadTexture("portrait_drafttoken"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixel_drafttoken"))
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, UNC_TOKEN, "Improved Token", 0, 2)
                .SetPortrait(AssetHelper.LoadTexture("portrait_drafttoken_plus"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixel_drafttoken_plus"))
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, RARE_DRAFT_TOKEN, "Rare Token", 0, 3)
                .SetPortrait(AssetHelper.LoadTexture("portrait_drafttoken_plusplus"))
                .SetPixelPortrait(AssetHelper.LoadTexture("pixel_drafttoken"))
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, BLOCKCHAIN, "Blockchain", 0, 5)
                .SetAltPortrait(AssetHelper.LoadTexture("portrait_blockchain"), FilterMode.Trilinear)
                .AddAbilities(Ability.ConduitNull, ConduitSpawnCrypto.AbilityID)
                .AddAppearances(HighResAlternatePortrait.ID)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, GOLLYCOIN, "GollyCoin", 0, 2)
                .SetAltPortrait(AssetHelper.LoadTexture("portrait_gollycoin"), FilterMode.Trilinear)
                .AddAppearances(HighResAlternatePortrait.ID)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, NFT, "Stupid-Ass Ape", 0, 1)
                .AddAppearances(RandomStupidAssApePortrait.ID)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, OLD_DATA, "UNSAFE.DAT", 0, 1)
                .SetPortrait(Resources.Load<Texture2D>("art/cards/part 3 portraits/portrait_captivefile"))
                .AddAbilities(LoseOnDeath.AbilityID)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, CODE_BLOCK, "Code Snippet", 1, 2)
                .SetPortrait(AssetHelper.LoadTexture("portrait_code"))
                .AddTraits(Programmer.CodeTrait)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, CODE_BUG, "Bug", 2, 1)
                .SetPortrait(AssetHelper.LoadTexture("portrait_bug"))
                .AddTraits(Programmer.CodeTrait)
                .AddAbilities(Ability.Brittle)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, VIRUS_SCANNER, "VIRSCAN.EXE", 1, 7)
                .SetPortrait(AssetHelper.LoadTexture("portrait_virusscanner"))
                .AddAbilities(Ability.Deathtouch, Ability.StrafeSwap)
                .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, PROGRAMMER, "Programmer", 0, 2)
                .SetPortrait(AssetHelper.LoadTexture("portrait_codemonkey"))
                .AddAbilities(Programmer.AbilityID)
                .temple = CardTemple.Tech;

            // CardManager.New(ARTIST, "Artist", 1, 2)
            //     .SetPortrait(AssetHelper.LoadTexture("portrait_artist"))
            //     .AddAbilities(Artist.AbilityID)
            //     .temple = CardTemple.Tech;

            CardManager.New(P03Plugin.CardPrefx, FIREWALL, "Firewall", 0, 3)
                .SetPortrait(AssetHelper.LoadTexture("portrait_firewall"))
                .AddAbilities(Ability.PreventAttack)
                .temple = CardTemple.Tech;

            // This should patch the rulebook
            AbilityManager.ModifyAbilityList += delegate(List<AbilityManager.FullAbility> abilities)
            {
                List<Ability> allP3Abs = CardManager.AllCardsCopy.Where(c => c.temple == CardTemple.Tech).SelectMany(c => c.abilities).Distinct().ToList();

                foreach (AbilityManager.FullAbility ab in abilities)
                {
                    if (allP3Abs.Contains(ab.Id))
                        ab.Info.AddMetaCategories(AbilityMetaCategory.Part3Rulebook);
                }
                return abilities;
            };

            CardManager.ModifyCardList += delegate(List<CardInfo> cards)
            {
                foreach (CardInfo ci in cards.Where(ci => ci.temple == CardTemple.Tech && ci.metaCategories.Contains(CardMetaCategory.Rare)))
                    ci.AddAppearances(RareDiscCardAppearance.ID);

                return cards;
            };
        }

        public static CardInfo SetNeutralP03Card(this CardInfo info)
        {
            info.AddMetaCategories(CardMetaCategory.ChoiceNode);
            info.AddMetaCategories(NeutralRegion);
            return info;
        }

        public static CardInfo SetRegionalP03Card(this CardInfo info, CardTemple region)
        {
            info.AddMetaCategories(CardMetaCategory.ChoiceNode);
            switch (region)
            {
                case CardTemple.Nature:
                    info.AddMetaCategories(NatureRegion);
                    break;
                case CardTemple.Undead:
                    info.AddMetaCategories(UndeadRegion);
                    break;
                case CardTemple.Tech:
                    info.AddMetaCategories(TechRegion);
                    break;
                case CardTemple.Wizard:
                    info.AddMetaCategories(WizardRegion);
                    break;
            }
            return info;
        }

        [HarmonyPatch(typeof(CardSpawner), nameof(CardSpawner.SpawnCardToHand), typeof(CardInfo), typeof(List<CardModificationInfo>), typeof(Vector3), typeof(float), typeof(Action<PlayableCard>))]
        [HarmonyPrefix]
        public static void LogCardToDraw(CardInfo info, List<CardModificationInfo> temporaryMods, Vector3 spawnOffset, float onDrawnTriggerDelay, Action<PlayableCard> cardSpawnedCallback = null)
        {
            P03Plugin.Log.LogInfo($"CardSpawner: {info.name}");
        }

        [HarmonyPatch(typeof(DrawRandomCardOnDeath), nameof(DrawRandomCardOnDeath.CardToDraw), MethodType.Getter)]
        [HarmonyPostfix]
        public static void LogCardToGenerate(ref CardInfo __result)
        {
            P03Plugin.Log.LogInfo($"DrawRandomCardOnDeath: {__result}");
        }

        [HarmonyPatch(typeof(DrawRandomCardOnDeath), nameof(DrawRandomCardOnDeath.OnDie))]
        [HarmonyPrefix]
        public static void OnDie(ref DrawRandomCardOnDeath __instance)
        {
            P03Plugin.Log.LogInfo($"Card died {__instance.Card.name}, abilities [{String.Join(",", __instance.Card.Info.Abilities.Select(ab => ab.ToString()))}]");
        }

        [HarmonyPatch(typeof(DrawCreatedCard), nameof(DrawCreatedCard.CreateDrawnCard))]
        [HarmonyPrefix]
        public static void DrawPrefix()
        {
            P03Plugin.Log.LogInfo($"Card draw");
        }
    }
}