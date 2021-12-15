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
    public class BittenCardAppearance : CardAppearanceBehaviour
    {
        private static Texture2D _sharkBiteDecal = AssetHelper.LoadTexture("shark_bite_decal");
        private static Texture2D _sharkBiteBackground = AssetHelper.LoadTexture("card_empty_sharkbite");

        public override void ApplyAppearance()
        {
            Card.RenderInfo.baseTextureOverride = _sharkBiteBackground;

            if (Card.Info.TempDecals.Any(t => t.name == _sharkBiteDecal.name))
                return;

            Card.Info.TempDecals.Add(_sharkBiteDecal);
        }
    }
}