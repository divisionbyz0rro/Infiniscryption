using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using InscryptionAPI.Card;

namespace Infiniscryption.Curses.Cards
{
    public class DynamiteAppearance : CardAppearanceBehaviour
    {
        public static CardAppearanceBehaviour.Appearance ID { get; private set; }

        internal static Texture _emptyDynamite = Resources.Load<Texture>("art/cards/card_terrain_empty");

        public override void ApplyAppearance()
        {
            base.Card.RenderInfo.baseTextureOverride = _emptyDynamite;
            base.Card.RenderInfo.forceEmissivePortrait = true;
			base.Card.StatsLayer.SetEmissionColor(GameColors.Instance.glowRed);
        }

        public static void Register()
        {
            ID = CardAppearanceBehaviourManager.Add(CursePlugin.PluginGuid, "DynamiteAppearance", typeof(DynamiteAppearance)).Id;
        }
    }
}