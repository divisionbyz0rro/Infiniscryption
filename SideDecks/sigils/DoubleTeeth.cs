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

namespace Infiniscryption.SideDecks.Sigils
{
    public class DoubleTeeth : AbilityBehaviour
    {
        public override Ability Ability => _ability;
        internal static Ability _ability;
        internal static AbilityIdentifier Identifier;

        // This ability...does nothing? That's right!

        public static void Register(Harmony harmony)
        {
            AbilityInfo info = AbilityInfoUtils.CreateInfoWithDefaultSettings(
                "Painful Entrance",
                "This creature will take two of your teeth when it comes into play."
            );

            Identifier = AbilityIdentifier.GetAbilityIdentifier("zorro.infiniscryption.sigils.doubleteeth", "DoubleTeeth");

            NewAbility ability = new NewAbility(
                info,
                typeof(DoubleTeeth),
                AssetHelper.LoadTexture("ability_doubleteeth"),
                Identifier
            );

            DoubleTeeth._ability = ability.ability;

            harmony.PatchAll(typeof(DoubleTeeth));
        }

        public override bool RespondsToResolveOnBoard()
        {
            return true;
        }

        public override IEnumerator OnResolveOnBoard()
        {
            yield return LifeManager.Instance.ShowDamageSequence(2, 2, true, 0f, null, 0f);
            yield return new WaitForSeconds(0.5f);
            ViewManager.Instance.SwitchToView(View.Default);
        }
    }
}