using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DiskCardGame;
using HarmonyLib;
using Infiniscryption.Core.Helpers;
using Infiniscryption.PackManagement.Patchers;
using InscryptionAPI.Card;
using InscryptionAPI.Saves;
using UnityEngine;

namespace Infiniscryption.PackManagement
{
    [HarmonyPatch]
    public static class PackManager
    {
        private static List<CardMetaCategory> protectedMetacategories = new();
        public static void AddProtectedMetacategory(CardMetaCategory category)
        {
            protectedMetacategories.Add(category);
        }

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
            TempleMetacategories.Add(CardTemple.Nature, new() { CardMetaCategory.ChoiceNode, CardMetaCategory.TraderOffer });
            TempleMetacategories.Add(CardTemple.Tech, new() { CardMetaCategory.ChoiceNode, CardMetaCategory.Part3Random });
            TempleMetacategories.Add(CardTemple.Wizard, new() { CardMetaCategory.ChoiceNode });
            TempleMetacategories.Add(CardTemple.Undead, new() { CardMetaCategory.ChoiceNode });
        }

        public static Dictionary<CardTemple, List<CardMetaCategory>> TempleMetacategories;

        internal static List<PackInfo> AllPacks;        
        
        private static List<string> ActiveCards = new();

        private static List<Ability> ActiveAbilities = new();

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
            List<PackInfo> activePacks = RetrievePackList(true);
            ActiveCards = new();
            ActiveAbilities = new();

            foreach(PackInfo pack in activePacks)
            {
                List<CardInfo> cardsInPack = pack.Cards.ToList();
                ActiveCards.AddRange(cardsInPack.Select(ci => ci.name));
                foreach (CardInfo info in cardsInPack)
                    foreach(Ability ab in info.Abilities)
                        if (!ActiveAbilities.Contains(ab))
                            ActiveAbilities.Add(ab);
            }

            ActiveCards = ActiveCards.Distinct().ToList();

            PackPlugin.Log.LogInfo($"The final card pool has {ActiveCards.Count} cards and {ActiveAbilities.Count} abilities");

            foreach (CardInfo card in cards)
            {
                if (ActiveCards.Contains(card.name))
                    card.temple = ScreenState;
                else
                    card.metaCategories.RemoveAll(c => !protectedMetacategories.Contains(c));
            }

            return cards;
        }

        internal static List<AbilityManager.FullAbility> FilterAbilitiesInPacks(List<AbilityManager.FullAbility> abilities)
        {
            // If the ability doesn't appear on any of the cards, 
            // it shouldn't appear anywhere
            foreach(var fab in abilities)
                if (!ActiveAbilities.Contains(fab.Id))
                    fab.Info.metaCategories = new();

            return abilities;
        }

        private static bool HasAddedFiltersToEvent = false;

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.TransitionToGame))]
        [HarmonyPrefix]
        internal static void FilterCardAndAbilityList()
        {
            if (HasAddedFiltersToEvent)
            {
                CardManager.ModifyCardList -= FilterCardsInPacks;
                AbilityManager.ModifyAbilityList -= FilterAbilitiesInPacks;
            }

            CardManager.ModifyCardList += FilterCardsInPacks;
            AbilityManager.ModifyAbilityList += FilterAbilitiesInPacks;
            HasAddedFiltersToEvent = true;

            CardManager.SyncCardList();
            AbilityManager.SyncAbilityList();
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), nameof(AscensionMenuScreens.Start))]
        [HarmonyPrefix]
        internal static void UnfilterCardAndAbilityList()
        {
            if (HasAddedFiltersToEvent)
            {
                CardManager.ModifyCardList -= FilterCardsInPacks;
                AbilityManager.ModifyAbilityList -= FilterAbilitiesInPacks;
                HasAddedFiltersToEvent = false;
            }

            JSONLoader.LoadFromJSON();

            CardManager.SyncCardList();
            AbilityManager.SyncAbilityList();
        }
    }
}