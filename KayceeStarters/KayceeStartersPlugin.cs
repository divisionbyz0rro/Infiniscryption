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

        private const string PluginGuid = "zorro.inscryption.infiniscryption.kayceestarters";
		private const string PluginName = "Infiniscryption Kaycees Starters";
		private const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;

            Harmony harmony = new Harmony(PluginGuid);

            CustomCards.RegisterCustomCards(harmony);

            harmony.PatchAll(typeof(KayceesDeckboxPatcher));
            harmony.PatchAll(typeof(SideDeckSelectorScreen));
            harmony.PatchAll(typeof(NumberOfPeltsSelectionScreen));

            AscensionScreenController.RegisterScreen<SideDeckSelectorScreen>();
            AscensionScreenController.RegisterScreen<NumberOfPeltsSelectionScreen>();

            RunStateHelper.Initialize(harmony);
            CustomNodeHelper.Initialize(harmony, Log);

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}
