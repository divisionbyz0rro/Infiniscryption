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
    public class DoubleBlood : AbilityBehaviour
    {
        public override Ability Ability => _ability;
        internal static Ability _ability;
        internal static AbilityIdentifier Identifier;

        // This ability...does nothing? That's right!

        public static void Register(Harmony harmony)
        {
            AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
                "Acceptable Sacrifice",
                "Sacrificing [creature] will count as if two creatures were sacrificed"
            );

            Identifier = AbilityIdentifier.GetAbilityIdentifier("zorro.infiniscryption.sigils.doubleblood", "DoubleBlood");

            NewAbility ability = new NewAbility(
                info,
                typeof(DoubleBlood),
                AssetHelper.LoadTexture("ability_doubleblood"),
                Identifier
            );
            ability.info.pixelIcon = Sprite.Create(
                AssetHelper.LoadTexture("pixelability_doubleblood"),
                new Rect(0f, 0f, 17f, 17f),
                new Vector2(0.5f, 0.5f)
            );

            DoubleBlood._ability = ability.ability;

            harmony.PatchAll(typeof(DoubleBlood));
        }

        [HarmonyPatch(typeof(BoardManager), "GetValueOfSacrifices")]
        [HarmonyPostfix]
        public static void AdjustForDoubleBlood(List<CardSlot> sacrifices, ref int __result)
        {
            foreach (CardSlot slot in sacrifices)
                if (slot != null && slot.Card != null && slot.Card.gameObject.GetComponent<DoubleBlood>() != null)
                    __result++;
        }
    }
}