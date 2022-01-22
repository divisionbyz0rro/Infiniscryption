using HarmonyLib;
using DiskCardGame;
using System.Collections.Generic;
using Infiniscryption.Core.Helpers;
using InscryptionAPI.Helpers;
using System.Linq;
using UnityEngine;
using InscryptionAPI.Ascension;

namespace Infiniscryption.P03KayceeRun.Patchers
{
    [HarmonyPatch]
    public static class AscensionChallengeManagement
    {
        public static Dictionary<Opponent.Type, List<AscensionChallengeInfo>> PatchedChallengesReference;

        public static Dictionary<Opponent.Type, List<AscensionChallenge>> ValidChallenges;

        static AscensionChallengeManagement()
        {
            PatchedChallengesReference = new();

            PatchedChallengesReference.Add(Opponent.Type.P03Boss, new());

            PatchedChallengesReference[Opponent.Type.P03Boss].Add(new() {
                challengeType = AscensionChallenge.NoHook,
                title = "No Remote",
                description = "You do not start with Mrs. Bomb's Remote",
                iconSprite = TextureHelper.ConvertTexture(AssetHelper.LoadTexture("ascensionicon_nohook"), TextureHelper.SpriteType.ChallengeIcon),
                activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                pointValue = 5
            });

            PatchedChallengesReference[Opponent.Type.P03Boss].Add(new() {
                challengeType = AscensionChallenge.ExpensivePelts,
                title = "Pricey Upgrades",
                description = "All upgrades cost more",
                iconSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_expensivepelts"), TextureHelper.SpriteType.ChallengeIcon),
                activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                pointValue = 5
            });

            ValidChallenges = new();
            ValidChallenges.Add(Opponent.Type.P03Boss, new () {
                AscensionChallenge.BaseDifficulty,
                AscensionChallenge.ExpensivePelts,
                AscensionChallenge.LessConsumables,
                AscensionChallenge.LessLives,
                AscensionChallenge.NoBossRares,
                AscensionChallenge.NoHook,
                AscensionChallenge.StartingDamage,
                AscensionChallenge.WeakStarterDeck
            });
        }

        [HarmonyPatch(typeof(AscensionChallengesUtil), nameof(AscensionChallengesUtil.GetInfo))]
        [HarmonyPostfix]
        public static void ModifyChallengeInfo(ref AscensionChallengeInfo __result)
        {
            if (ScreenManagement.ScreenState == Opponent.Type.P03Boss || SceneLoader.ActiveSceneName == "Part3_Cabin")
            {
                AscensionChallengeInfo orig = __result;
                AscensionChallengeInfo patch = PatchedChallengesReference[Opponent.Type.P03Boss].FirstOrDefault(ci => ci.challengeType == orig.challengeType);
                if (patch != null)
                    __result = patch;
            }
        }

        [HarmonyPatch(typeof(AscensionChallengeScreen), "OnEnable")]
        [HarmonyPrefix]
        public static void SetP03Challenges(ref AscensionChallengeScreen __instance)
        {
            AscensionChallengePaginator pageController = __instance.gameObject.GetComponent<AscensionChallengePaginator>();

            pageController.availableChallenges = pageController.availableChallenges.Select(aci => AscensionChallengesUtil.GetInfo(aci.challengeType)).ToList();
            pageController.pages = pageController.pages.Select(p => p.Select(aci => AscensionChallengesUtil.GetInfo(aci.challengeType)).ToList()).ToList();
            pageController.ShowVisibleChallenges();
        }

        [HarmonyPatch(typeof(AscensionUnlockSchedule), nameof(AscensionUnlockSchedule.ChallengeIsUnlockedForLevel))]
        [HarmonyAfter(new string[] { "cyantist.inscryption.api" })]
        [HarmonyPostfix]
        public static void ValidP03Challenges(ref bool __result, AscensionChallenge challenge)
        {
            if (ScreenManagement.ScreenState != Opponent.Type.Default && ValidChallenges.ContainsKey(ScreenManagement.ScreenState) && !ValidChallenges[ScreenManagement.ScreenState].Contains(challenge))
            {
                __result = false;
            }
        }
    }
}