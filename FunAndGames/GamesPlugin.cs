using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption.FunAndGames.Cards;

namespace Infiniscryption.FunAndGames
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class GamesPlugin : BaseUnityPlugin
    {

        internal const string PluginGuid = "zorro.inscryption.infiniscryption.funandgames";
		internal const string PluginName = "Infiniscryption Fun and Games";
		internal const string PluginVersion = "1.0";
        internal const string CardPrefix = "ZHOG";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;
            Harmony harmony = new Harmony(PluginGuid);

            harmony.PatchAll();

            KnightStrike.Register();
            PawnStrike.Register();
            PawnAppearance.Register();
            RenderOnSlotChanges.Register();
            SquirrelFriend.Register();
            CustomCards.RegisterCards();

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}

