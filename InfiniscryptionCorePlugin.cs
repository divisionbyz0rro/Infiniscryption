using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption.Helpers;
using Infiniscryption.Patchers;

namespace Infiniscryption
{
    // This plugin handles core logic that has to be in place for any of the other plugins to work.

    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class InfiniscryptionCorePlugin : BaseUnityPlugin
    {

        private const string PluginGuid = "zorro.inscryption.infiniscryption.core";
		private const string PluginName = "Infiniscryption Core";
		private const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll(typeof(SaveGameHelper));

            Log = base.Logger;

            // And we're loaded
            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
