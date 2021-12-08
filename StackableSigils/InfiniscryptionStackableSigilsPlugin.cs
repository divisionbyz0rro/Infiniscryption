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
using Infiniscryption.StackableSigils.Patchers;
using Infiniscryption.Spells.Patchers;

namespace Infiniscryption.StackableSigils
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class InfiniscryptionStackableSigilsPlugin : BaseUnityPlugin
    {

        private const string PluginGuid = "zorro.inscryption.infiniscryption.stackablesigils";
		private const string PluginName = "Infiniscryption Stackable Sigils";
		private const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private bool AddCards
        {
            get
            {
                return Config.Bind("InfiniscryptionStackableSigils", "AddCards", false, new BepInEx.Configuration.ConfigDescription("If true, this will add the sample cards to the card pool.")).Value;
            }
        }

        private void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);

            harmony.PatchAll(typeof(PatchAbilities));
            harmony.PatchAll(typeof(StackAbilityIcons));
            harmony.PatchAll(typeof(StackableSigilDefectFix));

            if (AddCards)
            {
                SampleCards.RegisterCustomCards();
            }

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
