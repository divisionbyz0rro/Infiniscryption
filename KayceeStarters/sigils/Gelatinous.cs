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
    public class Gelatinous : AbilityBehaviour
    {
        public override Ability Ability => _ability;
        internal static Ability _ability;
        internal static AbilityIdentifier Identifier;

        // This ability...does nothing? That's right!

        public static void Register(Harmony harmony)
        {
            AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
                "Gelatinous",
                "[creature] has no bones"
            );

            Identifier = AbilityIdentifier.GetAbilityIdentifier("zorro.infiniscryption.sigils.gelatinous", "Gelatinous");

            NewAbility ability = new NewAbility(
                info,
                typeof(Gelatinous),
                AssetHelper.LoadTexture("ability_gelatinous"),
                Identifier
            );
            ability.info.pixelIcon = Sprite.Create(
                AssetHelper.LoadTexture("pixelability_gelatinous"),
                new Rect(0f, 0f, 17f, 17f),
                new Vector2(0.5f, 0.5f)
            );

            Gelatinous._ability = ability.ability;

            harmony.PatchAll(typeof(Gelatinous));
        }

        [HarmonyPatch(typeof(ResourcesManager), "AddBones")]
        [HarmonyPostfix]
        public static IEnumerator StopAddingBonesIfGelatinous(IEnumerator sequenceResult, CardSlot slot)
        {
            if (slot != null 
                && slot.Card != null 
                && slot.Card.gameObject != null 
                && slot.Card.gameObject.GetComponent<Gelatinous>() != null)
            {
                yield break;
            }
            else
            {
                while (sequenceResult.MoveNext())
                    yield return sequenceResult.Current;
            }
            
        }
    }
}