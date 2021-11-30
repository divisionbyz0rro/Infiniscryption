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

namespace Infiniscryption.SideDecks.Patchers
{
    public static class SideDeckPatcher
    {
        public enum SideDecks
        {
            Squirrels = 0,
            Bees = 1,
            Ants = 2
        }
    }
}