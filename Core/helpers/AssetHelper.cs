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
using System.Reflection;

namespace Infiniscryption.Core.Helpers
{
    public static class AssetHelper
    {
        public static Texture2D LoadTexture(string texture)
        {
            Texture2D retval = new Texture2D(2, 2);

            string manualPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Infiniscryption", "assets", $"{texture}.png");
            byte[] imgBytes = File.ReadAllBytes(manualPath);
            retval.LoadImage(imgBytes);
            return retval;
        }
    }
}