using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;
using Infiniscryption.Curses.Helpers;
using Infiniscryption.Core.Helpers;
using APIPlugin;
using System.Linq;

namespace Infiniscryption.Curses.Cards
{
    public class DynamiteAppearance : CardAppearanceBehaviour
    {
        private static Texture _emptyDynamite = Resources.Load<Texture>("art/cards/card_terrain_empty");

        public override void ApplyAppearance()
        {
            base.Card.RenderInfo.baseTextureOverride = _emptyDynamite;
            base.Card.RenderInfo.forceEmissivePortrait = true;
			base.Card.StatsLayer.SetEmissionColor(GameColors.Instance.glowRed);
        }
    }
}