using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption.DifficultyMod.Patchers;

namespace Infiniscryption.DifficultyMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class InfiniscryptionDifficultyModPlugin : BaseUnityPlugin
    {

        private const string PluginGuid = "zorro.inscryption.infiniscryption.difficultymodder";
		private const string PluginName = "Infiniscryption Difficulty Modder";
		private const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;
            Harmony harmony = new Harmony(PluginGuid);

            if (BackpackLimiter.Active(Config))
            {
                harmony.PatchAll(typeof(BackpackLimiter));
            }

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
