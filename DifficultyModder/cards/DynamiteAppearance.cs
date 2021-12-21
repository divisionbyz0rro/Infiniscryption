using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;

namespace Infiniscryption.Curses.Cards
{
    public class DynamiteAppearance : CardAppearanceBehaviour
    {
        internal static Texture _emptyDynamite = Resources.Load<Texture>("art/cards/card_terrain_empty");

        public override void ApplyAppearance()
        {
            base.Card.RenderInfo.baseTextureOverride = _emptyDynamite;
            base.Card.RenderInfo.forceEmissivePortrait = true;
			base.Card.StatsLayer.SetEmissionColor(GameColors.Instance.glowRed);
        }
    }
}