using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Core.Helpers;
using Infiniscryption.PackManagement.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Regions;
using InscryptionAPI.Saves;
using UnityEngine;

namespace Infiniscryption.PackManagement
{
    [HarmonyPatch]
    public static class PackManager
    {
        private static List<CardMetaCategory> protectedMetacategories = new();
        private static List<AbilityMetaCategory> protectedAbilityMetacategories = new () 
        {
            AbilityMetaCategory.Part1Rulebook,
            AbilityMetaCategory.Part3Rulebook,
            AbilityMetaCategory.GrimoraRulebook,
            AbilityMetaCategory.MagnificusRulebook
        };

        public static void AddProtectedMetacategory(CardMetaCategory category) => protectedMetacategories.Add(category);
        public static void AddProtectedMetacategory(AbilityMetaCategory category) => protectedAbilityMetacategories.Add(category);

        static PackManager()
        {
            AllPacks = new();

            // Add all of the default packs
            PackInfo beastly = new PackInfo(CardTemple.Nature);
            beastly.Title = "Inscryption: Beastly Card Expansion Pack";
            beastly.Description = "The original set of cards featured in Leshy's cabin. Featuring wolves, mantises, and the occasional cockroach.";
            beastly.SetTexture(AssetHelper.LoadTexture("beastly"));
            AllPacks.Add(beastly);

            PackInfo techno = new PackInfo(CardTemple.Tech);
            techno.Title = "Inscryption: Techno Card Expansion Pack";
            techno.Description = "The original set of robotic cards, exclusively using the energy mechanic.";
            techno.SetTexture(AssetHelper.LoadTexture("tech"));
            AllPacks.Add(techno);

            PackInfo undead = new PackInfo(CardTemple.Undead);
            undead.Title = "Inscryption: Undead Card Expansion Pack";
            undead.Description = "Powered by the bones of the dead, these cards have come back from the grave to fight for you.";
            undead.SetTexture(AssetHelper.LoadTexture("undead")); 
            AllPacks.Add(undead);

            PackInfo wizard = new PackInfo(CardTemple.Wizard);
            wizard.Title = "Inscryption: Magickal Card Expansion Pack";
            wizard.Description = "Harness the might of the moxen to summon forth apprentices and fight in the most honorable of duels.";
            wizard.SetTexture(AssetHelper.LoadTexture("wizard")); 
            AllPacks.Add(wizard);

            // Add the leftovers pack as well
            PackInfo leftovers = new PackInfo(true);
            leftovers.Title = "Miscellaneous Community Cards";
            leftovers.Description = "The unusual, unsorted, and unruly cards that have been added by mods but not properly sorted into packs.";
            leftovers.SetTexture(AssetHelper.LoadTexture("leftovers"));
            AllPacks.Add(leftovers);

            // Temple metacategories
            TempleMetacategories = new Dictionary<CardTemple, List<CardMetaCategory>>();
            TempleMetacategories.Add(CardTemple.Nature, new() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer, CardMetaCategory.Rare });
            TempleMetacategories.Add(CardTemple.Tech, new() { CardMetaCategory.ChoiceNode, CardMetaCategory.Part3Random, CardMetaCategory.Rare });
            TempleMetacategories.Add(CardTemple.Wizard, new() { CardMetaCategory.ChoiceNode, CardMetaCategory.Rare });
            TempleMetacategories.Add(CardTemple.Undead, new() { CardMetaCategory.ChoiceNode, CardMetaCategory.Rare });
        }

        public static Dictionary<CardTemple, List<CardMetaCategory>> TempleMetacategories;

        internal static List<PackInfo> AllPacks;        
        
        private static HashSet<string> ActiveCards = new();

        private static HashSet<Ability> ActiveAbilities = new();

        public static IEnumerable<PackInfo> AllRegisteredPacks
        {
            get
            {
                return AllPacks.Where(pi => pi.IsBaseGameCardPack)
                               .Concat(AllPacks.Where(pi => pi.IsStandardCardPack))
                               .Concat(AllPacks.Where(pi => pi.IsLeftoversPack));
            }
        }

        public static PackInfo GetDefaultPackInfo(CardTemple temple) => AllPacks.FirstOrDefault(pi => pi.DefaultPackTemple == temple);

        public static PackInfo GetPackInfo(string modPrefix)
        {
            PackInfo retval = AllPacks.FirstOrDefault(pi => pi.IsStandardCardPack && modPrefix.Equals(pi.ModPrefix));
            if (retval == null)
            {
                retval = new PackInfo(modPrefix);
                AllPacks.Add(retval);
            }

            return retval;
        }

        public static PackInfo GetLeftoversPack()
        {
            return AllPacks.FirstOrDefault(pi => pi.IsLeftoversPack);
        }

        internal static CardTemple ScreenState 
        { 
            get
            {
                string value = ModdedSaveManager.SaveData.GetValue("zorro.inscryption.infiniscryption.p03kayceerun", "ScreenState");
                if (string.IsNullOrEmpty(value))
                    return CardTemple.Nature;

                return (CardTemple)Enum.Parse(typeof(CardTemple), value);
            }
        }

        internal static void ForceSyncOfAllPacks()
        {
            foreach (var grp in CardManager.AllCardsCopy.Where(ci => !ci.IsBaseGameCard()).GroupBy(ci => ci.GetModPrefix()))
            {
                if (string.IsNullOrEmpty(grp.Key))
                    continue;
                    
                if (grp.Count() > 5)
                    PackManager.GetPackInfo(grp.Key);
            }
        }

        internal static bool CardIsValidForScreenState(this CardInfo info)
        {
            try
            {
                int x = info.PowerLevel;

                foreach (CardMetaCategory cat in TempleMetacategories[ScreenState])
                    if (info.metaCategories.Contains(cat))
                        return true;

                return false;
            } catch (Exception ex)
            {
                PackPlugin.Log.LogError($"Error checking {info.name}");
                PackPlugin.Log.LogError(ex);
                return false;
            }
        }

        internal static void SavePackList(List<PackInfo> packs, bool activeList, CardTemple state = CardTemple.Nature)
        {
            if (state == CardTemple.Nature)
                state = ScreenState;

            string packString = string.Join("|", packs.Select(pi => pi.Key));
            string packKey = activeList ? $"AscensionData_ActivePackList" : $"{state.ToString()}_InactivePackList";

            ModdedSaveManager.SaveData.SetValue(PackPlugin.PluginGuid, packKey, packString);
        }

        internal static List<PackInfo> RetrievePackList(bool activeList, CardTemple state = CardTemple.Nature)
        {
            if (state == CardTemple.Nature)
                state = ScreenState;

            string packKey = activeList ? $"AscensionData_ActivePackList" : $"{state.ToString()}_InactivePackList";
            string packString = ModdedSaveManager.SaveData.GetValue(PackPlugin.PluginGuid, packKey);

            if (packString == default(string))
                return new();

            return packString.Split('|').Select(k => AllPacks.FirstOrDefault(pi => pi.Key == k)).Where(pi => pi != null).ToList();
        }

        public static List<PackInfo> GetActivePacks()
        {
            return RetrievePackList(true);
        }

        internal static List<CardInfo> FilterCardsInPacks(List<CardInfo> cards)
        {
            if (!ShouldFilterCards)
                return cards;

            List<PackInfo> activePacks = RetrievePackList(true);
            ActiveCards = new();
            ActiveAbilities = new();

            foreach(PackInfo pack in activePacks)
            {
                List<CardInfo> cardsInPack = pack.Cards.ToList();
                foreach (string cardName in cardsInPack.Select(ci => ci.name))
                    ActiveCards.Add(cardName);
            }

            PackPlugin.Log.LogInfo($"The final card pool has {ActiveCards.Count} cards and {ActiveAbilities.Count} abilities");

            foreach (CardInfo card in cards)
            {
                if (ActiveCards.Contains(card.name))
                    card.temple = ScreenState;
                else
                    card.metaCategories = new(card.metaCategories.Where(c => protectedMetacategories.Contains(c)));

                if (ActiveCards.Contains(card.name))
                    foreach(Ability ab in card.Abilities)
                        if (!ActiveAbilities.Contains(ab))
                            ActiveAbilities.Add(ab);
            }

            return cards;
        }

        private static AbilityMetaCategory GetRulebookMetacategory(this CardTemple temple)
        {
            if (temple == CardTemple.Nature)
                return AbilityMetaCategory.Part1Rulebook;
            if (temple == CardTemple.Tech)
                return AbilityMetaCategory.Part3Rulebook;
            if (temple == CardTemple.Wizard)
                return AbilityMetaCategory.MagnificusRulebook;
            if (temple == CardTemple.Undead)
                return AbilityMetaCategory.GrimoraRulebook;

            return AbilityMetaCategory.Part1Rulebook;
        }

        internal static List<AbilityManager.FullAbility> FilterAbilitiesInPacks(List<AbilityManager.FullAbility> abilities)
        {
            if (!ShouldFilterCards)
                return abilities;

            // If the ability doesn't appear on any of the cards, 
            // it shouldn't appear anywhere
            foreach(var fab in abilities)
            {
                if (!ActiveAbilities.Contains(fab.Id))
                    fab.Info.metaCategories = new(fab.Info.metaCategories.Where(c => protectedAbilityMetacategories.Contains(c)));
                else
                {
                    fab.Info.metaCategories = new List<AbilityMetaCategory>(fab.Info.metaCategories);
                    fab.Info.AddMetaCategories(ScreenState.GetRulebookMetacategory());
                }
            }

            return abilities;
        }

        private static bool EncounterValid(EncounterBlueprintData data)
        {
            foreach (CardInfo c in data.turns.SelectMany(l => l).Select(cb => cb.card)
                                   .Concat(data.turns.SelectMany(l => l).Select(cb => cb.replacement))
                                   .Concat(data.randomReplacementCards)
                                   .Where(ci => ci != null))
            {
                if (!ActiveCards.Contains(c.name))
                    return false;
            }
            return true;
        }

        private static bool EncounterAllDefault(EncounterBlueprintData data)
        {
            foreach (CardInfo c in data.turns.SelectMany(l => l).Select(cb => cb.card)
                                   .Concat(data.turns.SelectMany(l => l).Select(cb => cb.replacement))
                                   .Concat(data.randomReplacementCards)
                                   .Where(ci => ci != null))
            {
                if (!c.IsBaseGameCard())
                    return false;
            }
            return true;
        }

        internal static List<RegionData> FilterEncountersInRegions(List<RegionData> regions)
        {
            if (ShouldFilterCards)
            {
                foreach (RegionData region in regions)
                {
                    List<EncounterBlueprintData> activeBps = region.encounters.Where(bp => EncounterValid(bp)).ToList();
                    if (activeBps.Count == 0)
                        activeBps = region.encounters.Where(bp => EncounterAllDefault(bp)).ToList();
                    region.encounters = activeBps;
                }
            }
            return regions;
        }

        private static bool HasAddedFiltersToEvent = false;
        private static bool ShouldFilterCards = false;

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
        [HarmonyPrefix]
        internal static void FilterCardAndAbilityList()
        {
            PackPlugin.Log.LogInfo($"Filtering card list");
            if (!HasAddedFiltersToEvent)
            {
                CardManager.ModifyCardList += FilterCardsInPacks;
                AbilityManager.ModifyAbilityList += FilterAbilitiesInPacks;
                RegionManager.ModifyRegionsList += FilterEncountersInRegions;
                HasAddedFiltersToEvent = true;
            }

            ShouldFilterCards = true;
            CardManager.SyncCardList();
            AbilityManager.SyncAbilityList();
            RegionManager.SyncRegionList();
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.Start))]
        [HarmonyPrefix]
        internal static void UnfilterCardAndAbilityList()
        {
            PackPlugin.Log.LogInfo($"Unfiltering card list");

            JSONLoader.LoadFromJSON();

            ShouldFilterCards = false;
            CardManager.SyncCardList();
            AbilityManager.SyncAbilityList();
            RegionManager.SyncRegionList();
        }
    }
}