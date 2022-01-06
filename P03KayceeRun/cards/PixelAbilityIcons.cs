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

namespace Infiniscryption.P03KayceeRun.Cards
{
    [HarmonyPatch]
    public static class PixelAbilityIcons
    {
        private static Dictionary<Ability, Sprite> abilitySprites = new() {
            { Ability.DrawVesselOnHit, 
              Sprite.Create(
                AssetHelper.LoadTexture("pixelability_drawvessel"),
                new Rect(0f, 0f, 17f, 17f),
                new Vector2(0.5f, 0.5f)
            )},
            { Ability.Sniper, 
              Sprite.Create(
                AssetHelper.LoadTexture("pixelability_sniper"),
                new Rect(0f, 0f, 17f, 17f),
                new Vector2(0.5f, 0.5f)
            )},
            { Ability.RandomAbility, 
              Sprite.Create(
                AssetHelper.LoadTexture("pixelability_random"),
                new Rect(0f, 0f, 17f, 17f),
                new Vector2(0.5f, 0.5f)
            )},
            { Ability.DrawRandomCardOnDeath, 
              Sprite.Create(
                AssetHelper.LoadTexture("pixelability_randomcard"),
                new Rect(0f, 0f, 17f, 17f),
                new Vector2(0.5f, 0.5f)
            )},
            { Ability.LatchDeathShield, 
              Sprite.Create(
                AssetHelper.LoadTexture("pixelability_shieldlatch"),
                new Rect(0f, 0f, 17f, 17f),
                new Vector2(0.5f, 0.5f)
            )}
        };

        [HarmonyPatch(typeof(AbilitiesUtil), "GetInfo")]
        [HarmonyPostfix]
        public static void AddPixelIcon(Ability ability, ref AbilityInfo __result)
        {
            if (abilitySprites.ContainsKey(ability) && __result.pixelIcon == null)
            {
                __result.pixelIcon = abilitySprites[ability];
            }   
        }
    }
}