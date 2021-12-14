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
using Infiniscryption.Curses.Sequences;
using Infiniscryption.Curses.Helpers;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.Curses
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency("zorro.inscryption.infiniscryption.spells")]
    public class InfiniscryptionCursePlugin : BaseUnityPlugin
    {

        private const string PluginGuid = "zorro.inscryption.infiniscryption.curses";
		private const string PluginName = "Infiniscryption Curses";
		private const string PluginVersion = "0.1";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;
            Harmony harmony = new Harmony(PluginGuid);

            CurseManager.Register<BackpackLimiter>(harmony, Config, CurseManager.BindsTo.RunSetting);

            CurseManager.Register<CampfireHarder>(harmony, Config, CurseManager.BindsTo.RunSetting);

            CurseManager.Register<OneCandleMax>(harmony, Config, CurseManager.BindsTo.RunSetting);

            CurseManager.Register<DeathcardHaunt>(harmony, Config, CurseManager.BindsTo.RunSetting);

            CurseManager.Register<LooseTeeth>(harmony, Config, CurseManager.BindsTo.RunSetting);

            CurseManager.Register<RandomSigils>(harmony, Config, CurseManager.BindsTo.RunSetting);

            CurseManager.Register<CloverLimiter>(harmony, Config, CurseManager.BindsTo.RunSetting);

            CurseManager.Register<HarderBosses>(harmony, Config, CurseManager.BindsTo.RunSetting);
            HarderBosses.RegisterCustomCards(harmony);

            // Patch all of the toggleable difficulty mods
            harmony.PatchAll(typeof(CardExtensions));
            harmony.PatchAll(typeof(CurseManager));

            // Initialize the RunStateHelper
            RunStateHelper.Initialize(harmony);
            CustomNodeHelper.Initialize(harmony);

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}

