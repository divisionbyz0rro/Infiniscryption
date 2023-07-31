using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using InscryptionAPI.Helpers;
using System.Linq;
using InscryptionAPI.Card;

namespace Infiniscryption.Curses.Cards
{
    public class MegaSharkAppearance : CardAppearanceBehaviour
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        private static Texture _emptyNoStats = Resources.Load<Texture>("art/cards/card_empty_nostats");

        private static Texture2D _sharkBaseDecal = TextureHelper.GetImageAsTexture("shark_no_mouth.png", typeof(MegaSharkAppearance).Assembly);
        private static Texture2D _sharkMouthOpenDecal = TextureHelper.GetImageAsTexture("shark_mouth_open.png", typeof(MegaSharkAppearance).Assembly);
        private static Texture2D _sharkMouthClosedDecal = TextureHelper.GetImageAsTexture("shark_mouth_closed.png", typeof(MegaSharkAppearance).Assembly);

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
            SHARK_OPEN_PORTRAIT = TextureHelper.GetImageAsTexture("empty_shark_open.png", typeof(MegaSharkAppearance).Assembly);
            SHARK_CLOSED_PORTRAIT = TextureHelper.GetImageAsTexture("empty_shark_closed.png", typeof(MegaSharkAppearance).Assembly);
            SHARK_OPEN_EMISSION = TextureHelper.GetImageAsTexture("shark_open_emission.png", typeof(MegaSharkAppearance).Assembly);
            SHARK_CLOSED_EMISSION = TextureHelper.GetImageAsTexture("shark_closed_emission.png", typeof(MegaSharkAppearance).Assembly);

            SHARK_OPEN_PORTRAIT_SPRITE = Sprite.Create(MegaSharkAppearance.SHARK_OPEN_PORTRAIT, new Rect(0.0f, 0.0f, 114.0f, 94.0f), new Vector2(0.5f, 0.5f));
            SHARK_CLOSED_PORTRAIT_SPRITE = Sprite.Create(MegaSharkAppearance.SHARK_CLOSED_PORTRAIT, new Rect(0.0f, 0.0f, 114.0f, 94.0f), new Vector2(0.5f, 0.5f));
            SHARK_OPEN_EMISSION_SPRITE = Sprite.Create(MegaSharkAppearance.SHARK_OPEN_EMISSION, new Rect(0.0f, 0.0f, 114.0f, 94.0f), new Vector2(0.5f, 0.5f));
            SHARK_CLOSED_EMISSION_SPRITE = Sprite.Create(MegaSharkAppearance.SHARK_CLOSED_EMISSION, new Rect(0.0f, 0.0f, 114.0f, 94.0f), new Vector2(0.5f, 0.5f));

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

        public static void Register()
        {
            ID = CardAppearanceBehaviourManager.Add(CursePlugin.PluginGuid, "MegaSharkAppearance", typeof(MegaSharkAppearance)).Id;
        }
    }
}