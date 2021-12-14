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

        internal static bool PullFromPool { get; private set; }
        private bool _pullFromPool
        {
            get
            {
                return Config.Bind("InfiniscryptionSideDecks", "PullFromCardPool", true, new BepInEx.Configuration.ConfigDescription("If this is set to true, the side deck selection event at the start of the run will look at the whole card pool for cards that could potentially be in the side deck. If false, it only shows the side deck cards specifically added by this mod. ")).Value;
            }
        }

        private void Awake()
        {
            Log = base.Logger;
            PullFromPool = _pullFromPool;

            Harmony harmony = new Harmony(PluginGuid);

            CustomCards.RegisterCustomCards(harmony);
            harmony.PatchAll(typeof(SideDeckPatcher));

            RunStateHelper.Initialize(harmony);
            CustomNodeHelper.Initialize(harmony, Log);

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
