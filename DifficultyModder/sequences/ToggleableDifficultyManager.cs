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

namespace Infiniscryption.DifficultyMod.Sequences
{
    public static class ToggleableDifficultyManager
    {
        // Each of these are used to reset behaviors whenever there is
        // a new run or when the user steps away from the table
        public delegate void ResetBehavior();
        private static List<ResetBehavior> Resetters = new List<ResetBehavior>();

        public static void Register(ResetBehavior resetter)
        {
            Resetters.Add(resetter);
        }

        [HarmonyPatch(typeof(RunState), "Initialize")]
        [HarmonyPostfix]
        public static void ResetAll()
        {
            foreach (ResetBehavior resetter in Resetters)
            {
                resetter();
            }
        }
    }
}