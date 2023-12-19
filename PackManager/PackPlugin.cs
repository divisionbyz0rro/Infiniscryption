using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.PackManagement.Patchers;
using Infiniscryption.PackManagement.UserInterface;
using InscryptionAPI.Ascension;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;

namespace Infiniscryption.PackManagement
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class PackPlugin : BaseUnityPlugin
    {

        public static readonly CardMetaCategory NeutralRegion = GuidManager.GetEnumValue<CardMetaCategory>("zorro.inscryption.infiniscryption.p03kayceerun", "NeutralRegionCards");
        public static readonly CardMetaCategory WizardRegion = GuidManager.GetEnumValue<CardMetaCategory>("zorro.inscryption.infiniscryption.p03kayceerun", "WizardRegionCards");
        public static readonly CardMetaCategory TechRegion = GuidManager.GetEnumValue<CardMetaCategory>("zorro.inscryption.infiniscryption.p03kayceerun", "TechRegionCards");
        public static readonly CardMetaCategory NatureRegion = GuidManager.GetEnumValue<CardMetaCategory>("zorro.inscryption.infiniscryption.p03kayceerun", "NatureRegionCards");
        public static readonly CardMetaCategory UndeadRegion = GuidManager.GetEnumValue<CardMetaCategory>("zorro.inscryption.infiniscryption.p03kayceerun", "UndeadRegionCards");

        public const string PluginGuid = "zorro.inscryption.infiniscryption.packmanager";
        public const string PluginName = "Infiniscryption Pack Manager";
        public const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        internal static PackPlugin Instance;

        internal bool ToggleEncounters
        {
            get
            {
                return Config.Bind("EncounterManagement", "ToggleEncounters", true, new BepInEx.Configuration.ConfigDescription("If true, toggling off a card pack will also remove all encounters from the encounter pool that use cards in that pack.")).Value;
            }
        }

        internal bool RemoveDefaultEncounters
        {
            get
            {
                return Config.Bind("EncounterManagement", "RemoveDefaultEncounters", false, new BepInEx.Configuration.ConfigDescription("If true, toggling off the 'default' card pack will remove default encounters from the pool.")).Value;
            }
        }

        internal bool CrossOverAllPacks
        {
            get
            {
                return Config.Bind("DefaultSettings", "CrossOverAllPacks", false, new BepInEx.Configuration.ConfigDescription("If true, all of the game's default packs will be made available for all types of runs.")).Value;
            }
        }

        private void Awake()
        {
            Log = base.Logger;
            Instance = this;

            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll();

            JSONLoader.LoadFromJSON();
            CreatePacks.CreatePacksForOtherMods();
            AscensionScreenManager.RegisterScreen<PackSelectorScreen>();

            if (CrossOverAllPacks)
            {
                PackInfo techPackInfo = PackManager.GetDefaultPackInfo(CardTemple.Tech);
                techPackInfo.ValidFor.Add(PackInfo.PackMetacategory.LeshyPack);
                techPackInfo.ValidFor.Add(PackInfo.PackMetacategory.MagnificusPack);
                techPackInfo.ValidFor.Add(PackInfo.PackMetacategory.GrimoraPack);

                PackInfo wizardPackInfo = PackManager.GetDefaultPackInfo(CardTemple.Wizard);
                wizardPackInfo.ValidFor.Add(PackInfo.PackMetacategory.LeshyPack);
                wizardPackInfo.ValidFor.Add(PackInfo.PackMetacategory.P03Pack);
                wizardPackInfo.ValidFor.Add(PackInfo.PackMetacategory.GrimoraPack);

                PackInfo undeadPackInfo = PackManager.GetDefaultPackInfo(CardTemple.Undead);
                undeadPackInfo.ValidFor.Add(PackInfo.PackMetacategory.LeshyPack);
                undeadPackInfo.ValidFor.Add(PackInfo.PackMetacategory.MagnificusPack);
                undeadPackInfo.ValidFor.Add(PackInfo.PackMetacategory.P03Pack);

                PackInfo naturePackInfo = PackManager.GetDefaultPackInfo(CardTemple.Nature);
                naturePackInfo.ValidFor.Add(PackInfo.PackMetacategory.GrimoraPack);
                naturePackInfo.ValidFor.Add(PackInfo.PackMetacategory.MagnificusPack);
                naturePackInfo.ValidFor.Add(PackInfo.PackMetacategory.P03Pack);

                CardManager.ModifyCardList += delegate (List<CardInfo> cards)
                {
                    foreach (var card in cards)
                    {
                        if (card.temple != CardTemple.Nature)
                        {
                            if (card.HasCardMetaCategory(CardMetaCategory.ChoiceNode) && !card.HasCardMetaCategory(CardMetaCategory.Rare) && !card.HasCardMetaCategory(CardMetaCategory.TraderOffer))
                                card.AddMetaCategories(CardMetaCategory.TraderOffer);
                        }
                        if (card.temple != CardTemple.Tech)
                        {
                            if (card.HasCardMetaCategory(CardMetaCategory.ChoiceNode) || card.HasCardMetaCategory(CardMetaCategory.Rare))
                            {
                                if (card.temple == CardTemple.Wizard)
                                    card.AddMetaCategories(WizardRegion);
                                if (card.temple == CardTemple.Nature)
                                    card.AddMetaCategories(NatureRegion);
                                if (card.temple == CardTemple.Undead)
                                    card.AddMetaCategories(UndeadRegion);
                            }
                        }
                    }

                    return cards;
                };
            }

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }

        private void Start()
        {
            PackManager.ForceSyncOfAllPacks();
        }
    }
}
