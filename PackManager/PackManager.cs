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
        private static HashSet<CardInfo> ProcessedCards = new();
        public readonly static ReadOnlyCollection<PackInfo> BaseGamePacks = new(GenBaseGamePackList());
        private readonly static ObservableCollection<PackInfo> NewPacks = new();

        private static List<CardMetaCategory> protectedMetacategories = new();
        public static void AddProtectedMetacategory(CardMetaCategory category)
        {
            protectedMetacategories.Add(category);
        }

        public static Opponent.Type ScreenState 
        { 
            get
            {
                string value = ModdedSaveManager.SaveData.GetValue("zorro.inscryption.infiniscryption.p03kayceerun", "ScreenState");
                if (string.IsNullOrEmpty(value))
                    return Opponent.Type.Default;

                return (Opponent.Type)Enum.Parse(typeof(Opponent.Type), value);
            }
        }
        
        public static List<PackInfo> AllPacks { get; private set; } = BaseGamePacks.ToList();
        public static List<PackInfo> ValidPacks { get; private set; } = BaseGamePacks.ToList();

        static PackManager()
        {
            NewPacks.CollectionChanged += static (_, _) =>
            {
                AllPacks = BaseGamePacks.Concat(NewPacks).ToList();
                ValidPacks = AllPacks.Where(pi => pi.ActualCardList.Count > 0).ToList();
            };

            CardManager.ModifyCardList += SyncPackCardLists;
        }

        private static List<PackInfo> GenBaseGamePackList()
        {           
            List<PackInfo> baseGame = new();

            // These are the default card packs
            PackInfo defaultPack = new PackInfo();
            defaultPack.Name = "Beastly";
            defaultPack.Title = "Inscryption: Beastly Card Expansion Pack";
            defaultPack.Description = "The original set of cards. Featuring wolves, mantises, and the occasional cockroach.";
            defaultPack.SetTexture(AssetHelper.LoadTexture("beastly"));
            defaultPack.Cards = Resources.LoadAll<CardInfo>("data/cards/nature").Select(c => c.name).Concat(
                                Resources.LoadAll<CardInfo>("data/cards/specialpart1").Select(c => c.name)).Concat(
                                Resources.LoadAll<CardInfo>("data/cards/ascensionunlocks").Select(c => c.name)).ToList();

            
            baseGame.Add(defaultPack);

            return baseGame;
        }

        public static PackInfo Add(string guid, PackInfo info)
        {
            info.Name = guid + "_" + info.Title;

            if (!AllPacks.Exists(pi => pi.Name == info.Name)) // Prevent accidentally duplicating packs
                NewPacks.Add(info);

            return info;
        }

        private static List<string> ActiveCards = new();

        private static List<Ability> ActiveAbilities = new();

        internal static void SavePackList(List<PackInfo> packs, bool activeList, Opponent.Type state = Opponent.Type.Default)
        {
            if (state == Opponent.Type.Default)
                state = ScreenState;

            string packString = string.Join("|", packs.Select(pi => pi.Name));
            string packKey = activeList ? $"AscensionData_ActivePackList" : $"{state.ToString()}_InactivePackList";

            ModdedSaveManager.SaveData.SetValue(PackPlugin.PluginGuid, packKey, packString);
        }

        internal static List<PackInfo> RetrievePackList(bool activeList, Opponent.Type state = Opponent.Type.Default)
        {
            if (state == Opponent.Type.Default)
                state = ScreenState;

            string packKey = activeList ? $"AscensionData_ActivePackList" : $"{state.ToString()}_InactivePackList";
            string packString = ModdedSaveManager.SaveData.GetValue(PackPlugin.PluginGuid, packKey);

            if (packString == default(string))
                return new();

            return packString.Split('|').Select(k => AllPacks.FirstOrDefault(pi => pi.Name == k)).Where(pi => pi != null).ToList();
        }

        public static List<PackInfo> GetActivePacks()
        {
            return RetrievePackList(true);
        }
        
        internal static List<CardInfo> SyncPackCardLists(List<CardInfo> cards)
        {
            foreach (CardInfo card in cards.Where(c => !ProcessedCards.Contains(c)))
            {
                foreach (PackInfo pack in AllPacks)
                    pack.TryAddCardToPack(card.name);

                ProcessedCards.Add(card);
            }

            ValidPacks = AllPacks.Where(pi => pi.ActualCardList.Count > 0).ToList();

            return cards;
        }

        private static bool HasAddedFiltersToEvent = false;

        private static CardTemple GetTempleForActiveOpponent()
        {
            switch(ScreenState)
            {
                case Opponent.Type.P03Boss:
                    return CardTemple.Tech;
                case Opponent.Type.GrimoraBoss:
                    return CardTemple.Undead;
                case Opponent.Type.MagnificusBoss:
                    return CardTemple.Wizard;
                default:
                    return CardTemple.Nature;
            }
        }

        internal static List<CardInfo> FilterCardsInPacks(List<CardInfo> cards)
        {
            List<PackInfo> activePacks = RetrievePackList(true);
            ActiveCards = new();
            ActiveAbilities = new();

            foreach(PackInfo pack in activePacks)
            {
                ActiveCards.AddRange(pack.ActualCardList);
                foreach (CardInfo info in CardManager.AllCardsCopy.Where(ci => pack.ActualCardList.Contains(ci.name)))
                    foreach(Ability ab in info.Abilities)
                        if (!ActiveAbilities.Contains(ab))
                            ActiveAbilities.Add(ab);
            }

            ActiveCards = ActiveCards.Distinct().ToList();

            PackPlugin.Log.LogInfo($"The final card pool has {ActiveCards.Count} cards and {ActiveAbilities.Count} abilities");

            foreach (CardInfo card in cards)
            {
                if (ActiveCards.Contains(card.name))
                    card.temple = GetTempleForActiveOpponent();
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