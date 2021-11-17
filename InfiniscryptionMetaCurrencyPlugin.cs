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
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class InfiniscryptionMetaCurrencyPlugin : BaseUnityPlugin
    {

        private const string PluginGuid = "zorro.inscryption.infiniscryption.metacurrency";
		private const string PluginName = "Infiniscryption Metacurrency";
		private const string PluginVersion = "1.0";

        public bool Active
        {
            get
            {
                return Config.Bind("InfiniscryptionMetaCurrency", "Active", true, new BepInEx.Configuration.ConfigDescription("Activates the Metacurrency plugin for Infiniscryption. If false, the player cannot earn or track metacurrency over time.")).Value;
            }
        }

        internal static ManualLogSource Log;

        private void Awake()
        {
            if (this.Active)
            {
                Harmony harmony = new Harmony(PluginGuid);
                harmony.PatchAll(typeof(MetaCurrencyPatches));

                InfiniscryptionMetaCurrencyPlugin.Log = base.Logger;

                // And we're loaded
                Logger.LogInfo($"Plugin {PluginName} is loaded!");
            }
            else
            {
                Logger.LogInfo($"Plugin {PluginName} is loaded, but DEACTIVATED due to configuration!");
            }     
        }
    }
}
