using DiskCardGame;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Ascension;

namespace Infiniscryption.Curses.Patchers
{
    public static class BoonsAssist
    {
        public static AscensionChallenge ID {get; private set;}

        public static void Register(Harmony harmony)
        {
            ID = ChallengeManager.Add
            (
                CursePlugin.PluginGuid,
                "Minor Boon of the Bone Lord",
                "Start the game with a Minor Boon of the Bone Lord",
                -5,
                AssetHelper.LoadTexture("assist_bones_boon"),
                AssetHelper.LoadTexture("activated_assist_bones_boon"),
                stackable: true
            ).challengeType;

            harmony.PatchAll(typeof(BoonsAssist));
        }

        [HarmonyPatch(typeof(AscensionSaveData), "NewRun")]
        [HarmonyPostfix]
        [HarmonyAfter(new string[] { "zorro.inscryption.infiniscryption.kayceestarters" })]
        public static void AddBoonToStart(ref AscensionSaveData __instance)
        {
            for (int i = 0; i < AscensionSaveData.Data.GetNumChallengesOfTypeActive(ID); i++)
                __instance.currentRun.playerDeck.AddBoon(BoonData.Type.MinorStartingBones);
            
        }
    }
}