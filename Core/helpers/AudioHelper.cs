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
    public static class AudioHelper
    {
        public struct AudioState
        {
            public int sourceNum;
            public string clipName;
            public float position;
            public bool isPlaying;
        }

        public static List<AudioState> PauseAllLoops()
        {
            Traverse controller = Traverse.Create(AudioController.Instance);
            List<AudioSource> sources = controller.Field("loopSources").GetValue<List<AudioSource>>();

            List<AudioState> retval = new List<AudioState>();
            for (int i = 0; i < sources.Count; i++)
            {
                AudioSource source = sources[i];

                if (source == null || source.clip == null)
                {
                    retval.Add(new AudioState {
                        sourceNum = i,
                        position = 0f,
                        clipName = default(string),
                        isPlaying = false
                    });    
                    continue;
                }

                retval.Add(new AudioState {
                    sourceNum = i,
                    position = source.isPlaying ? source.time / source.clip.length : 0f,
                    clipName = source.clip.name,
                    isPlaying = source.isPlaying
                });
            }

            AudioController.Instance.StopAllLoops();
            return retval;
        }

        public static void ResumeAllLoops(List<AudioState> states)
        {
            for (int i = 0; i < states.Count; i++)
            {
                if (states[i].isPlaying)
                {
                    AudioController.Instance.SetLoopAndPlay(states[i].clipName, i, true, true);
                    AudioController.Instance.SetLoopVolumeImmediate(0f, i);
                    AudioController.Instance.SetLoopTimeNormalized(states[i].position, i);
                    AudioController.Instance.FadeInLoop(1f, 0.7f, new int[] { i });
                } else {
                    AudioController.Instance.StopLoop(i);
                }
            }
        }
    }
}