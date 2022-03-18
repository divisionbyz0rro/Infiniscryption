using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Ascension;
using UnityEngine;
using InscryptionAPI.Guid;

namespace Infiniscryption.Curses.Patchers
{
    public static class StartWithTribalTotems
    {
        public static AscensionChallenge ID {get; private set;}

        public static void Register(Harmony harmony)
        {
            ID = ChallengeManager.Add
            (
                CursePlugin.PluginGuid,
                "Totem Collector",
                "You start with all tribal totem tops",
                -15,
                Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_totems")
            ).challengeType;

            harmony.PatchAll(typeof(StartWithTribalTotems));
        }

        [HarmonyPatch(typeof(AscensionSaveData), "NewRun")]
        [HarmonyPostfix]
        public static void SetNumberOfLives(ref AscensionSaveData __instance)
        {
            if (AscensionSaveData.Data.ChallengeIsActive(ID))
            {
                __instance.currentRun.totemTops.Clear();
                __instance.currentRun.totemTops.AddRange(GuidManager.GetValues<Tribe>().Where(t => t != Tribe.None && t != Tribe.NUM_TRIBES));
            }
        }
    }
}