using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using Infiniscryption.Core.Helpers;
using APIPlugin;
using System.Linq;

namespace Infiniscryption.Curses.Cards
{
    public class MegaSharkAppearance : CardAppearanceBehaviour
    {
        private static Texture _emptyNoStats = Resources.Load<Texture>("art/cards/card_empty_nostats");

        private static Texture2D _sharkBaseDecal = AssetHelper.LoadTexture("shark_no_mouth");
        private static Texture2D _sharkMouthOpenDecal = AssetHelper.LoadTexture("shark_mouth_open");
        private static Texture2D _sharkMouthClosedDecal = AssetHelper.LoadTexture("shark_mouth_closed");

        public static Texture2D SHARK_OPEN_PORTRAIT { get; private set; }
        public static Texture2D SHARK_CLOSED_PORTRAIT { get; private set; }
        public static Texture2D SHARK_OPEN_EMISSION { get; private set; }
        public static Texture2D SHARK_CLOSED_EMISSION { get; private set; }

        public static Sprite SHARK_OPEN_PORTRAIT_SPRITE { get; private set; }
        public static Sprite SHARK_CLOSED_PORTRAIT_SPRITE { get; private set; }
        public static Sprite SHARK_OPEN_EMISSION_SPRITE { get; private set; }
        public static Sprite SHARK_CLOSED_EMISSION_SPRITE { get; private set; }

        static MegaSharkAppearance()
        {
            SHARK_OPEN_PORTRAIT = AssetHelper.LoadTexture("empty_shark_open");
            SHARK_CLOSED_PORTRAIT = AssetHelper.LoadTexture("empty_shark_closed");
            SHARK_OPEN_EMISSION = AssetHelper.LoadTexture("shark_open_emission");
            SHARK_CLOSED_EMISSION = AssetHelper.LoadTexture("shark_closed_emission");

            SHARK_OPEN_PORTRAIT_SPRITE = Sprite.Create(MegaSharkAppearance.SHARK_OPEN_PORTRAIT, CardUtils.DefaultCardArtRect, CardUtils.DefaultVector2);
            SHARK_CLOSED_PORTRAIT_SPRITE = Sprite.Create(MegaSharkAppearance.SHARK_CLOSED_PORTRAIT, CardUtils.DefaultCardArtRect, CardUtils.DefaultVector2);
            SHARK_OPEN_EMISSION_SPRITE = Sprite.Create(MegaSharkAppearance.SHARK_OPEN_EMISSION, CardUtils.DefaultCardArtRect, CardUtils.DefaultVector2);
            SHARK_CLOSED_EMISSION_SPRITE = Sprite.Create(MegaSharkAppearance.SHARK_CLOSED_EMISSION, CardUtils.DefaultCardArtRect, CardUtils.DefaultVector2);

            SHARK_OPEN_PORTRAIT_SPRITE.name = $"{SHARK_OPEN_PORTRAIT.name}_sprite";
            SHARK_CLOSED_PORTRAIT_SPRITE.name = $"{SHARK_CLOSED_PORTRAIT.name}_sprite";
            SHARK_OPEN_EMISSION_SPRITE.name = $"{SHARK_OPEN_EMISSION.name}_sprite";
            SHARK_CLOSED_EMISSION_SPRITE.name = $"{SHARK_CLOSED_EMISSION.name}_sprite";
        }

        public override void ApplyAppearance()
        {
            PlayableCard playCard = this.Card as PlayableCard;

            this.Card.RenderInfo.baseTextureOverride = _emptyNoStats;
            this.Card.RenderInfo.forceEmissivePortrait = true;

            if (playCard == null)
                return;

            if (playCard.Attack == 0)
            {
                playCard.Info.TempDecals.Clear();
                playCard.Info.TempDecals.Add(_sharkBaseDecal);
                playCard.Info.TempDecals.Add(_sharkMouthClosedDecal);

                this.Card.RenderInfo.portraitOverride = SHARK_CLOSED_PORTRAIT_SPRITE;
                this.Card.StatsLayer.SetEmissionColor(GameColors.Instance.purple);
            }
            else
            {
                playCard.Info.TempDecals.Clear();
                playCard.Info.TempDecals.Add(_sharkBaseDecal);
                playCard.Info.TempDecals.Add(_sharkMouthOpenDecal);

                this.Card.RenderInfo.portraitOverride = SHARK_OPEN_PORTRAIT_SPRITE;
                this.Card.StatsLayer.SetEmissionColor(GameColors.Instance.darkRed);
            }
        }
    }
}