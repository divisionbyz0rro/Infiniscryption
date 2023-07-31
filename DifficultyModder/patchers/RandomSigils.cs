using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using InscryptionAPI.Helpers;
using InscryptionAPI.Ascension;

namespace Infiniscryption.Curses.Patchers
{
    public static class RandomSigils
    {
        private static Ability[] EXCLUDED_SIGILS = new Ability[]
        {
            Ability.TriStrike,
            Ability.SplitStrike,
            Ability.AllStrike,
            Ability.DoubleStrike,
            Ability.CellDrawRandomCardOnDeath
        };

        public static AscensionChallenge ID {get; private set;}

        public static void Register(Harmony harmony)
        {
            var fc = ChallengeManager.Add
            (
                CursePlugin.PluginGuid,
                "Chaotic Enemies",
                "Opposing creatures gain random abilities",
                15,
                TextureHelper.GetImageAsTexture("challenge_random_sigils.png", typeof(RandomSigils).Assembly),
                ChallengeManager.DEFAULT_ACTIVATED_SPRITE,
                stackable:true
            );

            fc.SetFlags("P03");
            ID = fc.Challenge.challengeType;

            harmony.PatchAll(typeof(RandomSigils));
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        [HarmonyPrefix]
        private static void AlwaysInstantiateConduitManager()
        {
            if (ConduitCircuitManager.Instance == null)
                BoardManager.Instance.gameObject.AddComponent<ConduitCircuitManager>();
        }

        [HarmonyPatch(typeof(Part1Opponent), "ShowDifficultyChallengeUIIfTurnIsHarder")]
        [HarmonyPrefix]
        public static void ActivateRandomSigilsAlert(ref Part1Opponent __instance)
        {
            if (AscensionSaveData.Data.ChallengeIsActive(ID))
                ChallengeActivationUI.Instance.ShowActivation(ID);
        }

        [HarmonyPatch(typeof(EncounterBuilder), "BuildOpponentTurnPlan")]
        [HarmonyPostfix]
        public static void AddSigilsToCards(ref List<List<CardInfo>> __result)
        {
            int seed = SaveManager.SaveFile.GetCurrentRandomSeed() + 10;
            if (AscensionSaveData.Data.ChallengeIsActive(ID))
            {
                foreach (List<CardInfo> turn in __result)
                {
                    for (int i = 0; i < turn.Count; i++)
                    {
                        // Some cards get skipped

                        // We won't add sigils to the pack mule from the first boss
                        if (turn[i].SpecialAbilities.Where(tr => tr == SpecialTriggeredAbility.PackMule).Count() > 0)
                            continue;

                        // We won't add sigils to deathcards
                        if (turn[i].Mods.Where(mod => mod.deathCardInfo != null).Count() > 0)
                            continue;

                        // We won't add sigils to giant cards
                        // Right now this is just the moon, but it could be anything.
                        if (turn[i].HasTrait(Trait.Giant))
                            continue;

                        CardModificationInfo mod = new();
                        mod.fromCardMerge = SaveManager.SaveFile.IsPart1;
                        mod.abilities = new();

                        CardInfo card = turn[i] = turn[i].Clone() as CardInfo;

                        for (int fu = 0; fu < AscensionSaveData.Data.GetNumChallengesOfTypeActive(ID); fu++)
                        {

                            List<Ability> possibles = AbilitiesUtil.AllData
                                .Where(ab => ab.PositiveEffect &&
                                                ab.opponentUsable &&
                                                !card.Abilities.Contains(ab.ability) &&
                                                !EXCLUDED_SIGILS.Contains(ab.ability))
                                .Select(ab => ab.ability)
                                .ToList();

                            if (possibles.Count == 0)
                            {
                                CursePlugin.Log.LogDebug($"Could not add any ability to {card.name}");
                                continue;
                            }

                            mod.abilities.Add(possibles[SeededRandom.Range(0, possibles.Count, seed)]);
                            CursePlugin.Log.LogDebug($"Adding {mod.abilities.Last()} to {card.name}");
                            seed += 1;

                        }
                        card.Mods.Add(mod);
                    }
                }
            }
        }
    }
}