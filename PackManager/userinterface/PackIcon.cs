using UnityEngine;
using DiskCardGame;
using System;
using InscryptionAPI.Card;

namespace Infiniscryption.PackManagement.UserInterface
{
    public class PackIcon : MainInputInteractable
    {
        public void AssignPackInfo(PackInfo info)
        {
            IconRenderer.sprite = info.PackArt;
            Info = info;
            Selected = PackManager.GetActivePacks().Contains(info);
            CoveredRenderer.gameObject.SetActive(!Selected);
        }

        internal SpriteRenderer IconRenderer;

        internal SpriteRenderer CoveredRenderer;

        public bool Selected { get; private set; }

        public PackInfo Info { get; private set; }

        private PackSelectorScreen ScreenParent
        {
            get
            {
                return base.GetComponentInParent<PackSelectorScreen>();
            }
        }

        public override void OnCursorSelectEnd()
        {
            base.OnCursorSelectEnd();
            Selected = !Selected;
            CoveredRenderer.gameObject.SetActive(!Selected);
            PackManager.SetActive(this.Info, Selected);
        }

        private string RandomCardName()
        {
            return CardManager.AllCardsCopy.CardByName(this.Info.ActualCardList[UnityEngine.Random.Range(0, this.Info.ActualCardList.Count)]).DisplayedNameLocalized;
        }

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
            string repSring = description.Replace("[count]", this.Info.ActualCardList.Count.ToString())
                                         .Replace("[name]", this.Info.Title)
                                         .Replace("[powerlevel]", Math.Round(this.Info.AveragePowerLevel, 2).ToString());

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