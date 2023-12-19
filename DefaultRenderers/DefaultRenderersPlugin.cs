using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infiniscryption.DefaultRenderers
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api")]
    public class DefaultRenderersPlugin : BaseUnityPlugin
    {

        internal const string PluginGuid = "zorro.inscryption.infiniscryption.defaultrenderers";
        internal const string PluginName = "Infiniscryption Default Renderers";
        internal const string PluginVersion = "1.0";
        internal const string CardPrefix = "DEFREN";

        internal static DefaultRenderersPlugin Instance;

        internal static ManualLogSource Log;

        internal bool RendererAlwaysActive => Config.Bind("DefaultRenderersPlugin", "RendererAlwaysActive", false, new BepInEx.Configuration.ConfigDescription("Set this to true to make cards always render according to their temple.")).Value;

        private void Awake()
        {
            try
            {
                Log = base.Logger;
                Harmony harmony = new Harmony(PluginGuid);

                harmony.PatchAll();

                CardManager.ModifyCardList += delegate (List<CardInfo> cards)
                {
                    foreach (var card in cards)
                    {
                        card.traits ??= new();
                    }

                    return cards;
                };

                SceneManager.sceneLoaded += OnSceneLoaded;

                Instance = this;

                Logger.LogInfo($"Plugin {PluginName} is loaded!");
            }
            catch (Exception ex)
            {
                Log.LogError(ex);
                throw;
            }
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Need to *guarantee* that all of our card mod patches take hold
            CardManager.SyncCardList();
            AbilityManager.SyncAbilityList();
            EncounterManager.SyncEncounterList();
            try
            {
                DefaultCardRenderer.Instantiate();
            }
            catch { }
        }
    }
}

