using System.Linq;
using InscryptionAPI.Card;
using DiskCardGame;
using BepInEx.Bootstrap;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Infiniscryption.PackManagement
{
    public class PackInfo
    {
        public enum PackMetacategory
        {
            LeshyPack = 0,
            P03Pack = 1,
            GrimoraPack = 2,
            MagnificusPack = 3
        }

        public static PackMetacategory GetTempleDefaultMetacategory(CardTemple temple)
        {
            switch(temple)
            {
                case CardTemple.Nature:
                    return PackMetacategory.LeshyPack;
                case CardTemple.Tech:
                    return PackMetacategory.P03Pack;
                case CardTemple.Undead:
                    return PackMetacategory.GrimoraPack;
                case CardTemple.Wizard:
                    return PackMetacategory.MagnificusPack;
                default:
                    return PackMetacategory.LeshyPack;
            }
        }

        public const string JSONLOADER = "MADH.inscryption.JSONLoader";
        public const string TEMPLEINDICATOR = "DiskCardGame.CardTemple.";
        public const string LEFTOVERSINDICATOR = "PackManager.Leftovers";

        internal PackInfo(CardTemple temple)
        {
            this.DefaultPackTemple = temple;
            this.ValidFor = new List<PackMetacategory>() { GetTempleDefaultMetacategory(temple) };
        }

        internal PackInfo(bool leftovers)
        {
            // Why this instead of an internal default constructor? 
            // To prevent any weird edge cases where someone is using reflection improperly and
            // creates a pack using a default contructor without realizing what that would mean
            if (!leftovers)
                throw new InvalidOperationException("Can only use bool constructor if bool is true.");

            // This creates the "leftovers" pack
            IsLeftoversPack = leftovers;
            this.ValidFor = new () { PackMetacategory.GrimoraPack, PackMetacategory.MagnificusPack,
                                     PackMetacategory.P03Pack, PackMetacategory.LeshyPack };
        }

        internal PackInfo(string modPrefix, PackMetacategory category = PackMetacategory.LeshyPack)
        {
            this.ModPrefix = modPrefix;
            this.ValidFor = new() { category };
        }

        private bool initialized = false;
        private void TryInitialize()
        {
            if (initialized)
                return;

            initialized = true;
            List<CardInfo> cards = Cards.ToList();
            if (cards != null && cards.Count > 0)
            {
                // Build the title
                string modGuid = cards[0].GetModTag();
                if (!string.IsNullOrEmpty(modGuid) && 
                    !modGuid.Equals(JSONLOADER, System.StringComparison.OrdinalIgnoreCase) && 
                    Chainloader.PluginInfos.ContainsKey(modGuid))
                {
                    var modInfo = Chainloader.PluginInfos[modGuid];
                    _autoGeneratedTitle = modInfo.Metadata.Name;
                }
                else
                {
                    _autoGeneratedTitle = $"Card Pack: {this.ModPrefix}";
                }

                // Build the description
                int cardsToList = Math.Min(7, cards.Count);
                _autoGeneratedDescription = $"Cards in this pack: {string.Join(", ", cards.Take(cardsToList).Select(ci => ci.DisplayedNameLocalized))} and {cards.Count - 7} other{(cards.Count - 7 > 1 ? "s" : String.Empty)}.";
            }
        }

        public CardTemple? DefaultPackTemple { get; private set; }

        public string ModPrefix { get; private set; }

        public bool IsLeftoversPack { get; private set; }

        public bool IsBaseGameCardPack => DefaultPackTemple.HasValue;

        public bool IsStandardCardPack => !DefaultPackTemple.HasValue && !IsLeftoversPack;

        private string _title;
        private string _autoGeneratedTitle;
        public string Title 
        { 
            get 
            { 
                if (!string.IsNullOrEmpty(_title))
                    return Localization.Translate(_title);

                if (!initialized)
                    TryInitialize();

                return Localization.Translate(_autoGeneratedTitle);
            }
            set { _title = value; }
        }

        private string _description;
        private string _autoGeneratedDescription;
        public string Description
        {
            get 
            { 
                if (!string.IsNullOrEmpty(_description))
                    return Localization.Translate(_description);

                if (!initialized)
                    TryInitialize();

                return Localization.Translate(_autoGeneratedDescription);

            }
            set { _description = value; }
        }

        public Sprite PackArt { get; private set; }

        public List<PackMetacategory> ValidFor { get; private set; }

        public void SetTexture(Texture2D t)
        {
            t.filterMode = FilterMode.Point;
            this.PackArt = Sprite.Create(t, new Rect(0f, 0f, 46f, 74f), new Vector2(0.5f, 0.5f));
            this.PackArt.name = t.name + "_sprite";
        }

        public IEnumerable<CardInfo> Cards
        {
            get
            {
                if (IsBaseGameCardPack)
                    return CardManager.AllCardsCopy.Where(ci => ci.IsBaseGameCard() && ci.temple == this.DefaultPackTemple);
                else if (IsStandardCardPack)
                    return CardManager.AllCardsCopy.Where(ci => this.ModPrefix.Equals(ci.GetModPrefix()));
                else
                {
                    // Get all currently defined packs from the pack manager:
                    List<string> allKnownPrefixes = PackManager.AllPacks.Where(pi => pi.IsStandardCardPack).Select(pi => pi.ModPrefix).ToList();
                    return CardManager.AllCardsCopy.Where(ci => !ci.IsBaseGameCard() && !allKnownPrefixes.Contains(ci.GetModPrefix()));
                }
            }
        }

        public string Key
        {
            get
            {
                if (this.IsBaseGameCardPack)
                    return $"{TEMPLEINDICATOR}{this.DefaultPackTemple.Value.ToString()}";

                if (this.IsLeftoversPack)
                    return LEFTOVERSINDICATOR;
                                    
                return this.ModPrefix;
            }
        }
    }
}