using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.DifficultyMod.Helpers
{
    public static class DifficultyManager
    {
        private static Dictionary<Type, DifficultyModBase> DifficultyMods = new Dictionary<Type, DifficultyModBase>();

        public enum BindsTo
        {
            Configuration = 0,
            RunSetting = 1
        }

        public static void Register<T>(Harmony harmony, ConfigFile config, BindsTo behavior = BindsTo.Configuration) where T : DifficultyModBase, new()
        {
            T instance = new T();

            if (!config.Bind("InfiniscryptionDifficultyMod", typeof(T).Name, true, new BepInEx.Configuration.ConfigDescription(instance.Description)).Value)
            {
                return; // Don't bind or store - config said it was inactive
            }

            if (behavior == BindsTo.RunSetting)
            {
                instance.SetActive = delegate(bool active) { SaveGameHelper.SetValue($"Difficulty.{typeof(T).Name}", active.ToString()); }; 
                instance.GetActive = delegate() { return SaveGameHelper.GetBool($"Difficulty.{typeof(T).Name}"); };
            }
            else
            {
                instance.SetActive = delegate(bool active) {  };
                instance.GetActive = delegate() { return true; }; // Globally on - not run-by-run
            }

            harmony.PatchAll(typeof(T));
            DifficultyMods.Add(typeof(T), instance);
        }

        public static DifficultyModBase GetInstance<T>() where T : DifficultyModBase
        {
            if (DifficultyMods.ContainsKey(typeof(T)))
                return DifficultyMods[typeof(T)];

            return null;
        }

        public static bool IsActive<T>() where T : DifficultyModBase
        {
            DifficultyModBase mod = GetInstance<T>();
            return (mod == null) ? false : mod.Active;
        }

        [HarmonyPatch(typeof(RunState), "Initialize")]
        [HarmonyPostfix]
        public static void ResetAll()
        {
            foreach (DifficultyModBase mod in DifficultyMods.Values)
            {
                mod.Reset();
            }
        }
    }
}