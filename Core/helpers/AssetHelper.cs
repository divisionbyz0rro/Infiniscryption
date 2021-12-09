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
using UnityEngine.Networking;

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
            retval.name = $"Infiniscryption_{texture}";
            return retval;
        }

        public static void LoadAudioClip(string clipname, ManualLogSource log = null, string group = "Loops")
        {
            Traverse audioController = Traverse.Create(AudioController.Instance);
            List<AudioClip> clips = audioController.Field(group).GetValue<List<AudioClip>>();

            if (clips.Find(clip => clip.name.Equals(clipname)) != null)
                return;

            string manualPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Infiniscryption", "assets", $"{clipname}.wav");

            if (log != null)
                log.LogInfo($"About to get audio clip at file://{manualPath}");

            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip($"file://{manualPath}", AudioType.WAV))
            {
                request.SendWebRequest();
                while (request.IsExecuting()); // Wait for this thing to finish

                if (request.isHttpError)
                {
                    throw new InvalidOperationException($"Bad request getting audio clip {request.error}");
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                    clip.name = clipname;
                    
                    clips.Add(clip);
                }
            }
        }
    }
}