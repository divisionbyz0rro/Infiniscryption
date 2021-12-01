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
using Infiniscryption.SideDecks.Patchers;

namespace Infiniscryption.SideDecks
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency("julianperge.inscryption.cards.healthForAnts")]
    public class InfiniscryptionSideDecksPlugin : BaseUnityPlugin
    {

        private const string PluginGuid = "zorro.inscryption.infiniscryption.sidedecks";
		private const string PluginName = "Infiniscryption Side Decks";
		private const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;
            Harmony harmony = new Harmony(PluginGuid);

            CustomCards.RegisterCustomCards(harmony);
            harmony.PatchAll(typeof(SideDeckPatcher));

            RunStateHelper.Initialize(harmony);
            CustomNodeHelper.Initialize(harmony);

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
