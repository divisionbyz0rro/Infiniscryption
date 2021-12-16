using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption.Core.Helpers;
using APIPlugin;

namespace Infiniscryption.KayceeStarters.Sigils
{
    public static class TailOnHit
    {
        private static Sprite TAIL_SPRITE = Sprite.Create(
                    AssetHelper.LoadTexture("pixelability_tailonhit"),
                    new Rect(0f, 0f, 17f, 17f),
                    new Vector2(0.5f, 0.5f)
                );

        [HarmonyPatch(typeof(AbilitiesUtil), "GetInfo")]
        [HarmonyPostfix]
        public static void TailOnHitPixelIcon(Ability ability, ref AbilityInfo __result)
        {
            if (ability == Ability.TailOnHit && __result.pixelIcon == null)
            {
                __result.pixelIcon = TAIL_SPRITE;
            }   
        }
    }
}