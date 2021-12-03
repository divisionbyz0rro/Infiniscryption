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
using Infiniscryption.Spells.Sigils;
using Infiniscryption.Spells.Patchers;

namespace Infiniscryption.Spells
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class InfiniscryptionSpellsPlugin : BaseUnityPlugin
    {

        private const string PluginGuid = "zorro.inscryption.infiniscryption.spells";
		private const string PluginName = "Infiniscryption Spells";
		private const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);

            GlobalSpellAbility.Register(harmony);
            SpellCards.RegisterCustomCards();

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
