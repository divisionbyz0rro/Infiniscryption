using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

namespace Infiniscryption.Achievements
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class AchievementsPlugin : BaseUnityPlugin
    {

        public const string PluginGuid = "zorro.inscryption.infiniscryption.achievements";
		public const string PluginName = "Achievements Plugin";
		internal const string PluginVersion = "1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = base.Logger;
            
            ModdedAchievementManager.AchievementById(Achievement.KMOD_CHALLENGELEVEL1);

            Harmony harmony = new Harmony(PluginGuid);
            SceneManager.sceneLoaded += this.OnSceneLoaded;

            harmony.PatchAll();

            Logger.LogInfo($"Plugin {PluginName} is loaded!");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log.LogDebug($"Scene loaded {scene.name}. Initializing achievement popup handler");
            AchievementPopupHandler.Initialize(scene.name);
        }
    }
}

