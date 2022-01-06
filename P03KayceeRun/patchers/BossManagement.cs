using HarmonyLib;
using DiskCardGame;
using InscryptionAPI.Saves;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class BossManagement
    {
        [HarmonyPatch(typeof(HoloGameMap), "BossDefeatedSequence")]
        [HarmonyPostfix]
        public static IEnumerator AscensionP03BossDefeatedSequence(IEnumerator sequence, StoryEvent bossDefeatedStoryEvent)
        {
            if (!SaveFile.IsAscension)
            {
                yield return sequence;
                yield break;
            }

            EventManagement.AddCompletedZone(bossDefeatedStoryEvent);

            yield return FastTravelManagement.ReturnToHomeBase();

            yield break;
        }
    }
}