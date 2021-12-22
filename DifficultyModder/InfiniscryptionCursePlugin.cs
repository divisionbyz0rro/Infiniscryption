using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption.Curses.Patchers;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.Curses
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency("zorro.inscryption.infiniscryption.spells")]
    public class InfiniscryptionCursePlugin : BaseUnityPlugin
    {

        internal const string PluginGuid = "zorro.inscryption.infiniscryption.curses";
		internal const string PluginName = "Infiniscryption Curses";
		internal const string PluginVersion = "0.1";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;
            Harmony harmony = new Harmony(PluginGuid);

            RandomSigils.Register(harmony);
            HarderBosses.Register(harmony);
            DeathcardHaunt.Register(harmony);
            ThreeCandles.Register(harmony);
            GoldenPeltStart.Register(harmony);

            // Initialize the RunStateHelper
            RunStateHelper.Initialize(harmony);
            CustomNodeHelper.Initialize(harmony, Log);

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}

