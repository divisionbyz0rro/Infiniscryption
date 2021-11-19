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

        public static int ExcessTeeth
        {
            get { return SaveGameHelper.GetInt("MetaCurrency.Teeth", 0); }
            set { SaveGameHelper.SetValue("MetaCurrency.Teeth", value.ToString()); }
        }

        public static int Quills
        {
            get { return SaveGameHelper.GetInt("MetaCurrency.Quills", 0); }
            set { SaveGameHelper.SetValue("MetaCurrency.Quills", value.ToString()); }
        }
    }
}