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

namespace Infiniscryption.StackableSigils
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api", "1.13.0.0")]
    public class InfiniscryptionStackableSigilsPlugin : BaseUnityPlugin
    {

        private const string PluginGuid = "zorro.inscryption.infiniscryption.stackablesigils";
		private const string PluginName = "Infiniscryption Stackable Sigils";
		private const string PluginVersion = "1.0.4";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);

            harmony.PatchAll(typeof(StackAbilityIcons));



            System.Type testType = AccessTools.TypeByName("DiskCardGame.AscensionMenuScreens");
            if (testType == null)
                harmony.PatchAll(typeof(StackAbilityIcons.OldSchoolDescriptionPatch));
            else
                harmony.PatchAll(typeof(StackAbilityIcons.KayceesDescriptionPatch));

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
