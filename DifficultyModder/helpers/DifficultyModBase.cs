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

namespace Infiniscryption.DifficultyMod.Helpers
{
    public abstract class DifficultyModBase
    {
        // Don't mess with these. They should only be set by the manager
        internal delegate bool GetActiveDelegate();
        internal delegate void SetActiveDelegate(bool active);
        internal GetActiveDelegate GetActive;
        internal SetActiveDelegate SetActive;

        // This tells whether or not the difficult mod is active
        public bool Active
        {
            get { return GetActive(); }
            set { SetActive(value); }
        }

        // Describes what the mod does
        internal abstract string Description { get; }

        // This is executed whenever a run resets.
        public abstract void Reset();
    }
}