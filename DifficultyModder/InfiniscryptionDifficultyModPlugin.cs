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
using Infiniscryption.DifficultyMod.Sequences;
using Infiniscryption.DifficultyMod.Helpers;

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

            DifficultyManager.Register<BackpackLimiter>(harmony, Config, DifficultyManager.BindsTo.Configuration);

            DifficultyManager.Register<OneCandleMax>(harmony, Config, DifficultyManager.BindsTo.RunSetting);

            // Patch all of the toggleable difficulty mods
            harmony.PatchAll(typeof(DifficultyManager));

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
