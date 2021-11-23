using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

namespace Infiniscryption.Core.Helpers
{
    public static class AssetHelper
    {
        internal static PluginInfo Info;

        public static Texture2D LoadTexture(string texture)
        {
            Texture2D retval = new Texture2D(2, 2);
            byte[] imgBytes = File.ReadAllBytes(Path.Combine(Info.Location.Replace("Infiniscryption.Core.dll",""), $"Infiniscryption/assets/{texture}.png"));
            retval.LoadImage(imgBytes);
            return retval;
        }
    }
}