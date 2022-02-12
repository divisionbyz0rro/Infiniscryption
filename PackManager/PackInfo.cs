using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using UnityEngine;

namespace Infiniscryption.PackManagement
{
    [HarmonyPatch]
    public class PackInfo
    {
        public string Name { get; internal set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<string> Cards { get; set; } = new();

        public string RegexMatch { get; set; }

        public Sprite PackArt { get; private set; }

        public List<Opponent.Type> ValidFor { get; set; } = new() { Opponent.Type.Default };

        private HashSet<string> _cardCollection = new();

        private List<string> _actualCardList;
        public ReadOnlyCollection<string> ActualCardList
        {
            get
            {
                if (_actualCardList == null)
                    _actualCardList = new List<string>(_cardCollection);
                
                return _actualCardList.AsReadOnly();
            }
        }

        internal void TryAddCardToPack(string card)
        {
            if (_cardCollection.Contains(card))
                return;

            if (this.Cards.Contains(card, StringComparer.OrdinalIgnoreCase) || 
               (!string.IsNullOrEmpty(this.RegexMatch) && Regex.IsMatch(card, this.RegexMatch, RegexOptions.IgnoreCase)))
            {
               _cardCollection.Add(card);
               _actualCardList = null;
            }
        }

        public double AveragePowerLevel 
        { 
            get
            {
                if (_cardCollection != null && _cardCollection.Count > 0)
                    return CardManager.AllCardsCopy.Where(ci => _cardCollection.Contains(ci.name)).Select(ci => ci.PowerLevel).Average();

                return 0d;
            }
        }

        public void SetTexture(Texture2D t)
        {
            t.filterMode = FilterMode.Point;
            this.PackArt = Sprite.Create(t, new Rect(0f, 0f, 46f, 74f), new Vector2(0.5f, 0.5f));
            this.PackArt.name = t.name + "_sprite";
        }
    }
}