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
using Infiniscryption.Curses.Helpers;
using Infiniscryption.Curses.Sequences;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.Curses.Patchers
{
    public class RandomSigils : CurseBase
    {
        public override string Description => "Opposing creatures will gain random abilities";

        public override string Title => "Chaotic Opposition";

        Texture2D _iconTexture = AssetHelper.LoadTexture("random_ability_icon");

        public RandomSigils(string id, GetActiveDelegate getActive, SetActiveDelegate setActive) : base(id, getActive, setActive) { }

        public override Texture2D IconTexture => _iconTexture;

        public override void Reset()
        {
            // We do nothing here
        }

        [HarmonyPatch(typeof(EncounterBuilder), "Build")]
        [HarmonyPostfix]
        public static void AddSigilsToCards(ref EncounterData __result)
        {
            int seed = SaveManager.SaveFile.GetCurrentRandomSeed() + 10;
            if (CurseManager.IsActive<RandomSigils>())
            {
                foreach (List<CardInfo> turn in __result.opponentTurnPlan)
                {
                    for (int i = 0; i < turn.Count; i++)
                    {
                        CardInfo card = turn[i];

                        int abilityPowerLevel = card.Abilities.Select(ab => AbilitiesUtil.GetInfo(ab).powerLevel).Sum();
                        if (abilityPowerLevel <= 1)
                        {
                            card = turn[i] = card.Clone() as CardInfo;

                            List<Ability> possibles = AbilitiesUtil.GetAbilities(false, true, 1, 10, SaveManager.SaveFile.IsPart1 ? AbilityMetaCategory.Part1Modular : AbilityMetaCategory.Part3Modular);
                            possibles = possibles.Where(ab => !card.Abilities.Contains(ab)).ToList();

                            if (possibles.Count == 0)
                                continue;

                            CardModificationInfo mod = new CardModificationInfo(possibles[SeededRandom.Range(0, possibles.Count, seed)]);

                            seed += 1;
                            card.Mods.Add(mod);
                        }
                    }
                }
            }
        }
    }
}