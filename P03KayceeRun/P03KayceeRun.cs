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
using InscryptionAPI.AscensionScreens;

namespace Infiniscryption.P03KayceeRun
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class InfiniscryptionP03Plugin : BaseUnityPlugin
    {

        private const string PluginGuid = "zorro.inscryption.infiniscryption.p03kayceerun";
		private const string PluginName = "Infiniscryption P03 in Kaycee's Mod";
		private const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll();

            RunStateHelper.Initialize(harmony);

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
