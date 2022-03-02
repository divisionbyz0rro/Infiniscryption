using UnityEngine;
using DiskCardGame;
using System;
using InscryptionAPI.Card;
using System.Collections.Generic;
using System.Linq;
using GBC;

namespace Infiniscryption.PackManagement.UserInterface
{
    public class PackIcon : MainInputInteractable
    {
        public void AssignPackInfo(PackInfo info, PackContentCache cache)
        {
            Info = info;
            Selected = !PackManager.RetrievePackList(false).Contains(info);
            CoveredRenderer.gameObject.SetActive(!Selected);
            ActualCards = cache.GetCardsForPack(info);
            if (info.PackArt != null)
            {
                IconRenderer.gameObject.SetActive(true);
                IconRenderer.sprite = info.PackArt;
                Text.gameObject.SetActive(false);
                SampleCardRenderer.gameObject.SetActive(false);
            }
            else
            {
                IconRenderer.gameObject.SetActive(true);
                IconRenderer.sprite = PackSelectorScreen.DefaultPackSprite;
                Text.gameObject.SetActive(true);
                Text.SetText(info.ModPrefix.Length > 6 ? info.ModPrefix.Substring(0, 6) : info.ModPrefix);
                SampleCardRenderer.gameObject.SetActive(true);
                SampleCardRenderer.sprite = IconCard.portraitTex;
            }
        }

        internal SpriteRenderer IconRenderer;

        internal SpriteRenderer CoveredRenderer;

        internal PixelText Text;

        internal SpriteRenderer SampleCardRenderer;

        private List<string> ActualCards { get; set; }

        private CardInfo IconCard
        {
            get
            {
                int maxPowerLevel = 0;
                CardInfo maxCard = null;
                foreach (CardInfo card in CardManager.AllCardsCopy.Where(ci => ActualCards.Contains(ci.name)))
                {
                    if (card.PowerLevel > maxPowerLevel)
                    {
                        maxPowerLevel = card.PowerLevel;
                        maxCard = card;
                    }
                }
                return maxCard;
            }
        }

        public bool Selected { get; private set; }

        public PackInfo Info { get; private set; }

        private PackSelectorScreen ScreenParent => base.GetComponentInParent<PackSelectorScreen>();

        public override void OnCursorSelectEnd()
        {
            base.OnCursorSelectEnd();

            List<PackInfo> activePacks = PackManager.RetrievePackList(true);

            if (Selected && activePacks.Count == 1)
                return; // You cannot unselect the last active pack

            List<PackInfo> inactivePacks = PackManager.RetrievePackList(false);
            
            if (Selected)
            {
                activePacks.Remove(this.Info);
                inactivePacks.Add(this.Info);
            }
            else
            {
                activePacks.Add(this.Info);
                inactivePacks.Remove(this.Info);
            }

            PackManager.SavePackList(activePacks, true);
            PackManager.SavePackList(inactivePacks, false);

            Selected = !Selected;
            CoveredRenderer.gameObject.SetActive(!Selected);
        }

        private string RandomCardName() => CardManager.AllCardsCopy.CardByName(ActualCards[UnityEngine.Random.Range(0, ActualCards.Count)]).DisplayedNameLocalized;

        private double AveragePowerLevel => CardManager.AllCardsCopy.Where(ci => ActualCards.Contains(ci.name)).Where(ci => ci != null).Select(ci => ci.PowerLevel).Average();

        private string ReplaceRandom(string text)
        {
            while (true)
            {
                int pos = text.IndexOf("[randomcard]");
                if (pos < 0)
                    return text;
                
                text = text.Substring(0, pos) + RandomCardName() + text.Substring(pos + 12);
            }
        }

        private string FormatString(string description)
        {
            string repSring = description.Replace("[count]", ActualCards.Count.ToString())
                                         .Replace("[name]", this.Info.Title)
                                         .Replace("[powerlevel]", Math.Round(this.AveragePowerLevel, 2).ToString());

            repSring = ReplaceRandom(repSring);
            return Localization.Translate(repSring);
        }

        public override void OnCursorEnter()
        {
            ScreenParent.DisplayCardInfo(null, Localization.Translate(this.Info.Title), FormatString(this.Info.Description + $"\nAverage Power Level: [powerlevel]"));
        }

        public override void OnCursorExit()
        {
            ScreenParent.ClearMessage();
        }

        public override bool CollisionIs2D => true;
    }
}