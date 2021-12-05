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
using System.Linq;

namespace Infiniscryption.Spells.Sigils
{
    public class TargetedSpellAbility :  VariableStatBehaviour
    {
        // Why is this a stat behavior when these cards have no stats?
        // Simple. I want to cover over the health and attack icons.
        // I want these cards to have 0 health and 0 attack at all times in all zones.
        // This is the best way to do that.

        // I'm following the pattern of HealthForAnts

        internal static SpecialStatIcon _icon;
        protected override SpecialStatIcon IconType => _icon;

        private static SpecialAbilityIdentifier _id;
        public static SpecialAbilityIdentifier ID
        {
            get
            {
                if (_id == null)
                {
                    _id = SpecialAbilityIdentifier.GetID(
                            "zorro.infiniscryption.sigils.targetedspell",
                            "Spell (Targeted)"
                    );
                }
                return _id;
            }
        }

        public static NewSpecialAbility Instance;
        public static void Register()
        {
            if (Instance == null)
            {
                StatIconInfo info = ScriptableObject.CreateInstance<StatIconInfo>();
                info.appliesToAttack = true;
                info.appliesToHealth = true;
                info.rulebookName = "Spell (Targeted)";
                info.rulebookDescription = "This card is not a creature and dies immediately when played. When played, it will target and affect a single chosen space on the board.";
                info.iconGraphic = AssetHelper.LoadTexture("targeted_spell_stat_icon");

                Instance = new NewSpecialAbility(typeof(TargetedSpellAbility), ID, info);
                _icon = Instance.statIconInfo.iconType;
            }
        }

        // No stats for these cards!
        protected override int[] GetStatValues()
        {
            return new int[] { 0, 0 };
        }
    }
}