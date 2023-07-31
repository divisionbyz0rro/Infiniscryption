using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using InscryptionAPI.Sound;
using UnityEngine.Networking;
using System;
using BepInEx;
using System.Linq;
using Infiniscryption.Curses.Cards;
using Infiniscryption.Curses.Patchers;

namespace Infiniscryption.Curses.Helpers
{
    [HarmonyPatch]
    internal static class AudioHelper
    {
        [HarmonyPatch(typeof(AudioController), nameof(AudioController.Awake))]
        [HarmonyPostfix]
        internal static void LoadMyCustomAudio(ref AudioController __instance)
        {
            // I'm allowed to hack this together if I want just leave me be :((((
            foreach (string clipName in new string[] { Dynamite.EXPLOSION_SOUND, Digester.GULP_SOUND, DeathcardHaunt.DEATHCARD_INTRO_CLIP })
            {
                if (!__instance.SFX.Any(ac => ac.name.Equals(clipName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    AudioClip expl = SoundManager.LoadAudioClip(CursePlugin.PluginGuid, $"{clipName}.wav");
                    expl.name = clipName;

                    if (clipName.Equals(DeathcardHaunt.DEATHCARD_INTRO_CLIP))
                        __instance.Loops.Add(expl);
                    else
                        __instance.SFX.Add(expl);
                }
            }
        }
    }
}