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
using Infiniscryption.Spells.Sigils;
using Infiniscryption.Spells.Patchers;

namespace Infiniscryption.Spells
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class InfiniscryptionSpellsPlugin : BaseUnityPlugin
    {

        private const string PluginGuid = "zorro.inscryption.infiniscryption.spells";
		private const string PluginName = "Infiniscryption Spells";
		private const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private bool AddCards
        {
            get
            {
                return Config.Bind("InfiniscryptionSpells", "AddCards", false, new BepInEx.Configuration.ConfigDescription("If true, this will add the sample cards to the card pool.")).Value;
            }
        }

        private void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);

            harmony.PatchAll(typeof(SpellBehavior));
            harmony.PatchAll(typeof(StackableSigilDefectFix));

            TargetedSpellAbility.Register();
            GlobalSpellAbility.Register();
            DrawTwoCards.Register();
            DestroyAllCardsOnDeath.Register();
            DirectDamage.Register();
            DirectHeal.Register();

            if (AddCards)
            {
                SpellCards.RegisterCustomCards();
            }

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
