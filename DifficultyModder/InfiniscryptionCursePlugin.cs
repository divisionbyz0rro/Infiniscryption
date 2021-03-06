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
using InscryptionAPI.Saves;

namespace Infiniscryption.Curses
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency("zorro.inscryption.infiniscryption.spells")]
    public class CursePlugin : BaseUnityPlugin
    {

        internal const string PluginGuid = "zorro.inscryption.infiniscryption.curses";
		internal const string PluginName = "Infiniscryption Curses";
		internal const string PluginVersion = "0.1";
        internal const string CardPrefix = "CURSES";

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
            NoOneHitKills.Register(harmony);
            DrawDynamite.Register(harmony);
            StartWithTribalTotems.Register(harmony);
            BiggerMoon.Register(harmony);
            BoonsAssist.Register(harmony);

            harmony.PatchAll(typeof(CursePlugin));

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}


