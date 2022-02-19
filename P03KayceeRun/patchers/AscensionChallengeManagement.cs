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
        public static List<AscensionChallengeInfo> PatchedChallengesReference;

        public static List<AscensionChallenge> ValidChallenges;

        public static void UpdateP03Challenges()
        {
            PatchedChallengesReference = new();

            PatchedChallengesReference.Add(new() {
                challengeType = AscensionChallenge.NoHook,
                title = "No Remote",
                description = "You do not start with Mrs. Bomb's Remote",
                iconSprite = TextureHelper.ConvertTexture(AssetHelper.LoadTexture("ascensionicon_nohook"), TextureHelper.SpriteType.ChallengeIcon),
                activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                pointValue = 5
            });

            PatchedChallengesReference.Add(new() {
                challengeType = AscensionChallenge.ExpensivePelts,
                title = "Pricey Upgrades",
                description = "All upgrades cost more",
                iconSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_expensivepelts"), TextureHelper.SpriteType.ChallengeIcon),
                activatedSprite = TextureHelper.ConvertTexture(Resources.Load<Texture2D>("art/ui/ascension/ascensionicon_activated_default"), TextureHelper.SpriteType.ChallengeIcon),
                pointValue = 5
            });

            ValidChallenges = new() {
                AscensionChallenge.BaseDifficulty,
                AscensionChallenge.ExpensivePelts,
                AscensionChallenge.LessConsumables,
                AscensionChallenge.LessLives,
                AscensionChallenge.NoBossRares,
                AscensionChallenge.NoHook,
                AscensionChallenge.StartingDamage,
                AscensionChallenge.WeakStarterDeck
            };

            ChallengeManager.ModifyChallenges += delegate(List<AscensionChallengeInfo> challenges)
            {
                if (P03AscensionSaveData.IsP03Run)
                {
                    for (int i = 0; i < challenges.Count; i++)
                    {
                        AscensionChallengeInfo patchInfo = PatchedChallengesReference.FirstOrDefault(asci => asci.challengeType == challenges[i].challengeType);
                        if (patchInfo != null)
                            challenges[i] = patchInfo;
                    }
                }

                return challenges;
            };
        }

        // [HarmonyPatch(typeof(AscensionChallengeScreen), "OnEnable")]
        // [HarmonyPrefix]
        // public static void SetP03Challenges(ref AscensionChallengeScreen __instance)
        // {
        //     AscensionChallengePaginator pageController = __instance.gameObject.GetComponent<AscensionChallengePaginator>();

        //     pageController.availableChallenges = pageController.availableChallenges.Select(aci => AscensionChallengesUtil.GetInfo(aci.challengeType)).ToList();
        //     pageController.pages = pageController.pages.Select(p => p.Select(aci => AscensionChallengesUtil.GetInfo(aci.challengeType)).ToList()).ToList();
        //     pageController.ShowVisibleChallenges();
        // }

        [HarmonyPatch(typeof(AscensionUnlockSchedule), nameof(AscensionUnlockSchedule.ChallengeIsUnlockedForLevel))]
        [HarmonyAfter(new string[] { "cyantist.inscryption.api" })]
        [HarmonyPostfix]
        public static void ValidP03Challenges(ref bool __result, AscensionChallenge challenge)
        {
            if (ScreenManagement.ScreenState == Opponent.Type.P03Boss && !ValidChallenges.Contains(challenge))
            {
                __result = false;
            }
        }
    }
}