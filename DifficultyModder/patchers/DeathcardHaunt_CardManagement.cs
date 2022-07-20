using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using DiskCardGame;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;
using Infiniscryption.Curses.Sequences;
using Infiniscryption.Core.Helpers;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Infiniscryption.Curses.Patchers
{
    public static partial class DeathcardHaunt
    {
        // This part of the class manages the creation of deathcards

        public class IntWrapper 
        {
            public int Value { get; set; }
        }

        private static ConditionalWeakTable<CardInfo, IntWrapper> deathcardAnimationPlayTable = new ConditionalWeakTable<CardInfo, IntWrapper>();

        private static void SetHauntedCardFlag(CardInfo card)
        {
            IntWrapper currentValue;            
            if (!deathcardAnimationPlayTable.TryGetValue(card, out currentValue))
                deathcardAnimationPlayTable.Add(card, new IntWrapper { Value = -1 });
        }

        private static bool GetHauntedCardFlag(CardInfo card)
        {
            IntWrapper dummy;
            return deathcardAnimationPlayTable.TryGetValue(card, out dummy);
        }

        private static void SetHauntedCardSlot(CardInfo card, int slot)
        {
            IntWrapper currentValue;            
            if (!deathcardAnimationPlayTable.TryGetValue(card, out currentValue))
                deathcardAnimationPlayTable.Add(card, new IntWrapper { Value = slot });
            else
                currentValue.Value = slot;
        }

        private static int GetHauntedCardSlot(CardInfo card)
        {
            IntWrapper dummy;
            if (deathcardAnimationPlayTable.TryGetValue(card, out dummy))
                return dummy.Value;
            else
                return -1;
        }

        public static CardInfo GetRandomDeathcard()
        {
            // Build the base card
            // int seed = SaveManager.SaveFile.GetCurrentRandomSeed();
            // List<CardModificationInfo> modList = SaveManager.SaveFile.deathCardMods.Where(
            //     mod => mod.attackAdjustment < 10 && mod.healthAdjustment < 10
            // )
            // .ToList(); // Try to prevent some of the really nasty deathcards from one-shotting you
            // CardModificationInfo mod = modList[SeededRandom.Range(0, modList.Count, seed)];

            CardModificationInfo mod = DeathcardGenerator.GenerateMod(HauntLevel);
            CardInfo deathcard = CardLoader.CreateDeathCard(mod);
            SetHauntedCardFlag(deathcard);

            return deathcard;
        }

        public class TalkWhenGhostDiesHandler : TriggerReceiver
        {
            public override bool RespondsToPreDeathAnimation(bool wasSacrifice)
            {
                return true;
            }

            public override IEnumerator OnPreDeathAnimation(bool wasSacrifice)
            {
                return DeathcardOuttroSequence();
            }
        }

        private static ConditionalWeakTable<CardTriggerHandler, List<TriggerReceiver>> _customReceivers = new ConditionalWeakTable<CardTriggerHandler, List<TriggerReceiver>>();

        public static void AddReceiver(CardTriggerHandler handler, TriggerReceiver receiver)
        {
            List<TriggerReceiver> list = null;
            if (!_customReceivers.TryGetValue(handler, out list))
            {
                list = new List<TriggerReceiver>();
                _customReceivers.Add(handler, list);
            }
            list.Add(receiver);
        }

        public static List<TriggerReceiver> GetReceivers(CardTriggerHandler handler)
        {
            List<TriggerReceiver> list = null;
            _customReceivers.TryGetValue(handler, out list);
            return list;
        }

        [HarmonyPatch(typeof(PlayableCard), "AttachAbilities")]
        [HarmonyPostfix]
        public static void AttachDeathListener(CardInfo info, ref PlayableCard __instance)
        {
            CursePlugin.Log.LogInfo($"In Opponent.CreateCard {info.name}");
            if (GetHauntedCardFlag(info))
            {
                CursePlugin.Log.LogInfo("Adding ghostdieshandler");
                TalkWhenGhostDiesHandler handler = __instance.gameObject.AddComponent<TalkWhenGhostDiesHandler>();
                AddReceiver(__instance.TriggerHandler, handler);
            }
        }

        [HarmonyPatch(typeof(CardTriggerHandler), "GetAllReceivers")]
        [HarmonyPostfix]
        public static void GetCustomListeners(ref List<TriggerReceiver> __result, ref CardTriggerHandler __instance)
        {
            List<TriggerReceiver> list = GetReceivers(__instance);
            if (list != null)
                __result.AddRange(list);
        }
    }
}