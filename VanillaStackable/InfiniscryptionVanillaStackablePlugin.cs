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
using Infiniscryption.VanillaStackable.Patchers;

namespace Infiniscryption.VanillaStackable
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api", "1.13.0.0")]
    public class InfiniscryptionVanillaStackablePlugin : BaseUnityPlugin
    {

        private const string PluginGuid = "zorro.inscryption.infiniscryption.vanillastackablesigils";
		private const string PluginName = "Infiniscryption Vanilla Stackable Abilities";
		private const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private bool AddCards
        {
            get
            {
                return Config.Bind("InfiniscryptionVanillaStackable", "DebugMode", false, new BepInEx.Configuration.ConfigDescription("If true, this will add debug cards to the pool and will start the game with a bunch of them.")).Value;
            }
        }

        private void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);

            harmony.PatchAll(typeof(PatchAbilities));

            if (AddCards)
            {
                SampleCards.RegisterCustomCards(harmony);
            }

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
