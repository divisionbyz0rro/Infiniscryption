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
        private static Ability[] EXCLUDED_SIGILS = new Ability[]
        {
            Ability.TriStrike,
            Ability.SplitStrike,
            Ability.AllStrike
        };

        public override string Description => "Opposing creatures will gain random abilities";

        public override string Title => "Chaos";

        Texture2D _iconTexture = AssetHelper.LoadTexture("random_ability_icon");

        public RandomSigils(string id, GetActiveDelegate getActive, SetActiveDelegate setActive) : base(id, getActive, setActive) { }

        public override Texture2D IconTexture => _iconTexture;

        public override void Reset()
        {
            // We do nothing here
        }

        [HarmonyPatch(typeof(EncounterBuilder), "BuildOpponentTurnPlan")]
        [HarmonyPostfix]
        public static void AddSigilsToCards(ref List<List<CardInfo>> __result)
        {
            int seed = SaveManager.SaveFile.GetCurrentRandomSeed() + 10;
            if (CurseManager.IsActive<RandomSigils>())
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

                        CardInfo card = turn[i] = turn[i].Clone() as CardInfo;

                        List<Ability> possibles = AbilitiesUtil.GetAbilities(false, true, 1, 10, SaveManager.SaveFile.IsPart1 ? AbilityMetaCategory.Part1Modular : AbilityMetaCategory.Part3Modular);                        
                        possibles = possibles.Where(ab => !card.Abilities.Contains(ab) && !EXCLUDED_SIGILS.Contains(ab)).ToList();

                        if (possibles.Count == 0)
                            continue;

                        CardModificationInfo mod = new CardModificationInfo(possibles[SeededRandom.Range(0, possibles.Count, seed)]);
                        mod.fromCardMerge = true;

                        seed += 1;
                        card.Mods.Add(mod);
                    }
                }
            }
        }
    }
}