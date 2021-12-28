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
using Infiniscryption.KayceeStarters.Cards;
using Infiniscryption.KayceeStarters.UserInterface;
using Infiniscryption.KayceeStarters.Patchers;
using InscryptionAPI.AscensionScreens;

namespace Infiniscryption.KayceeStarters
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    [BepInDependency("julianperge.inscryption.cards.healthForAnts")]
    public class InfiniscryptionKayceeStartersPlugin : BaseUnityPlugin
    {

        public const string PluginGuid = "zorro.inscryption.infiniscryption.kayceestarters";
		public const string PluginName = "Infiniscryption Kaycees Starters";
		public const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);

            CustomCards.RegisterCustomCards(harmony);

            harmony.PatchAll(typeof(KayceesDeckboxPatcher));
            harmony.PatchAll(typeof(SideDeckSelectorScreen));
            harmony.PatchAll(typeof(NumberOfPeltsSelectionScreen));

            AscensionScreenManager.RegisterScreen<SideDeckSelectorScreen>();
            AscensionScreenManager.RegisterScreen<NumberOfPeltsSelectionScreen>();

            CustomNodeHelper.Initialize(harmony, Log);

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
