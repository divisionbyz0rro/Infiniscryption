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

namespace Infiniscryption.Core
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class InfiniscryptionCorePlugin : BaseUnityPlugin
    {

        private const string PluginGuid = "zorro.inscryption.infiniscryption.core";
		private const string PluginName = "Infiniscryption Starter Decks";
		private const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll(typeof(SaveGameHelper));
            AssetHelper.Info = this.Info;

            Log = base.Logger;

            // And we're loaded
            Logger.LogInfo($"Plugin {PluginName} is loaded!");    
        }
    }
}
