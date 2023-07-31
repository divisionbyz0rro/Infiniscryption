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
using Infiniscryption.Curses.Helpers;
using InscryptionAPI.Saves;

namespace Infiniscryption.Curses
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency("zorro.inscryption.infiniscryption.spells")]
    [BepInDependency("zorro.inscryption.infiniscryption.achievements")]
    public class CursePlugin : BaseUnityPlugin
    {

        internal const string PluginGuid = "zorro.inscryption.infiniscryption.curses";
		internal const string PluginName = "Infiniscryption Curses";
		internal const string PluginVersion = "0.1";
        internal const string CardPrefix = "CURSES";

        internal static ManualLogSource Log;

        internal static CursePlugin Instance;  

        internal string DebugCode
        {
            get
            {
                return Config.Bind("CurseMod", "DebugCode", "nothing", new BepInEx.Configuration.ConfigDescription("A special code to use for debugging purposes only. Don't change this unless your name is DivisionByZorro or he told you how it works.")).Value;
            }
        }

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

            CursedAchievements.Register();

            harmony.PatchAll(typeof(AudioHelper));
            harmony.PatchAll(typeof(CursePlugin));

            Instance = this;

            Logger.LogInfo($"Plugin {PluginName} is loaded with debug code {DebugCode}");
        }
    }
}


