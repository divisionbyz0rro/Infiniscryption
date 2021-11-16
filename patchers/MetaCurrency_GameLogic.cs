using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using Infiniscryption;
using Infiniscryption.Helpers;

namespace Infiniscryption.Patchers
{
    public static partial class MetaCurrencyPatches
    {
        // Here, we establish the metacurrency
        // There are two metacurrencies: excess teeth and quills
        // Excess teeth are teeth that are leftover when you die

        public static int ExcessTeeth = 0;
        
        public static int Quills = 0;

        [HarmonyPatch(typeof(SaveManager), "LoadFromFile")]
        [HarmonyPostfix]
        public static void LoadSavedMetaCurrency()
        {
            // Load the current state of the metacurrency
            string teeth = SaveGameHelper.GetValue("Metacurrency.Teeth");
            if (teeth != default(string))
                ExcessTeeth = int.Parse(teeth);
            
            string quills = SaveGameHelper.GetValue("Metacurrency.Quills");
            if (quills != default(string))
                Quills = int.Parse(quills);
        }

        [HarmonyPatch(typeof(SaveManager), "SaveToFile")]
        [HarmonyPrefix]
        public static void SaveStarterDecks()
        {
            // Save the current state of the starter decks
            SaveGameHelper.SetValue("Metacurrency.Teeth", ExcessTeeth.ToString());
            SaveGameHelper.SetValue("Metacurrency.Quills", Quills.ToString());
        }
    }
}