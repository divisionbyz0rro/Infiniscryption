using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Infiniscryption.Terrain
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class TerrainPlugin : BaseUnityPlugin
    {

        internal const string PluginGuid = "zorro.inscryption.infiniscryption.terrain";
		internal const string PluginName = "Infiniscryption Terrain Mod";
		internal const string PluginVersion = "1.0";
        internal const string CardPrefix = "TERR";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;
            Harmony harmony = new Harmony(PluginGuid);

            harmony.PatchAll();

            foreach (Type t in typeof(TerrainPlugin).Assembly.GetTypes())
            {
                try
                {
                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle);
                } catch (TypeLoadException ex)
                {
                    Log.LogWarning("Failed to force load static constructor!");
                    Log.LogWarning(ex);
                }
            }

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }
    }
}

