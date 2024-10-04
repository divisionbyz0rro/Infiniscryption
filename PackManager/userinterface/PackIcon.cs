using UnityEngine;
using DiskCardGame;
using System;
using InscryptionAPI.Card;
using System.Collections.Generic;
using System.Linq;
using GBC;
using InscryptionAPI.Helpers;
using InscryptionAPI.Ascension;

namespace Infiniscryption.PackManagement.UserInterface
{
    public class PackIcon : MainInputInteractable
    {
        private static Dictionary<string, Sprite> _defaultPackSprites = new();
        internal static Sprite GetDefaultPackSprite(Type type)
        {
            if (!_defaultPackSprites.ContainsKey(type.Name))
            {
                _defaultPackSprites[type.Name] = Sprite.Create(TextureHelper.GetImageAsTexture($"default_window_{type.Name}.png", typeof(PackPlugin).Assembly), new Rect(0f, 0f, 46f, 74f), new Vector2(0.5f, 0.5f));
            }
            return _defaultPackSprites[type.Name];
        }

        public void AssignPackInfo(PackInfoBase info, PackContentCache cache)
        {
            Info = info;
            Cache = cache;
            List<PackInfoBase> activePacks = PackManager.RetrievePackList(info.GetType(), true);
            Selected = activePacks.Contains(info);
            if (!Selected)
            {
                Locked = false;
            }
            else
            {
                activePacks.Remove(info);
                Locked = !info.SetOfPacksIsValid(activePacks, cache);
            }
            CoveredRenderer.gameObject.SetActive(!Selected);
            LockedRenderer.gameObject.SetActive(Locked);
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
                IconRenderer.sprite = GetDefaultPackSprite(info.GetType());
                Text.gameObject.SetActive(true);
                Text.SetText(info.ModPrefix.Length > 6 ? info.ModPrefix.Substring(0, 6) : info.ModPrefix);
                SampleCardRenderer.gameObject.SetActive(true);
                SampleCardRenderer.sprite = Info.IconCard.portraitTex;
            }
        }

        internal SpriteRenderer IconRenderer;

        internal SpriteRenderer CoveredRenderer;

        internal SpriteRenderer LockedRenderer;

        internal PixelText Text;

        internal SpriteRenderer SampleCardRenderer;

        private PackContentCache Cache { get; set; }

        public bool Selected { get; private set; }

        public bool Locked { get; private set; }

        public PackInfoBase Info { get; private set; }

        private AscensionRunSetupScreenBase ScreenParent => base.GetComponentInParent<AscensionRunSetupScreenBase>();

        public override void OnCursorSelectEnd()
        {
            base.OnCursorSelectEnd();

            if (Locked)
                return;

            List<PackInfoBase> activePacks = PackManager.RetrievePackList(Info.GetType(), true);

            if (Selected)
            {
                while (activePacks.Contains(this.Info))
                    activePacks.Remove(this.Info);
            }
            else
            {
                activePacks.Add(this.Info);
            }

            PackManager.SavePackList(Info.GetType(), activePacks, true);

            Selected = !Selected;
            CoveredRenderer.gameObject.SetActive(!Selected);

            IconSelectedCallback?.Invoke();
        }

        internal Action IconSelectedCallback = null;

        private string FormatString(string description)
        {
            return Localization.Translate(this.Info.ReplaceMarkers(description, this.Cache));
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