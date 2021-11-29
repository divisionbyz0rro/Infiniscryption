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
using Infiniscryption.Core.Helpers;
using System.IO;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Infiniscryption.Curses.Helpers
{
    public static class CardExtensions
    {
        private static ConditionalWeakTable<CardInfo, CurseBase> _cardInfoCurses = new ConditionalWeakTable<CardInfo, CurseBase>();
        private static ConditionalWeakTable<CardAbilityIcons, CurseIconInteractable> _cardCurses = new ConditionalWeakTable<CardAbilityIcons, CurseIconInteractable>();

        // Extension method to add curses to card info
        public static void SetCurse(this CardInfo cardInfo, CurseBase curse)
        {
            _cardInfoCurses.Add(cardInfo, curse);
        }

        public static CurseBase GetCurse(this CardInfo cardInfo)
        {
            CurseBase retval;
            if (!_cardInfoCurses.TryGetValue(cardInfo, out retval))
                return null;
            return retval;
        }

        // Extension method to add curse icons to cards
        public static void SetCurseIcon(this CardAbilityIcons card, CurseIconInteractable curseIcon)
        {
            _cardCurses.Add(card, curseIcon);
        }

        public static CurseIconInteractable GetCurseIcon(this CardAbilityIcons cardInfo)
        {
            CurseIconInteractable retval;
            if (!_cardCurses.TryGetValue(cardInfo, out retval))
                return null;
            return retval;
        }

        public static void SetBackTexture (this SelectableCard card, Texture texture)
        {
            Traverse cardTraverse = Traverse.Create(card);
            cardTraverse.Field("flippedBackTexture").SetValue(texture);
        }

        [HarmonyPatch(typeof(CardAbilityIcons), "SetInteractionEnabled")]
        [HarmonyPostfix]
        public static void CurseIteractable(bool interactionEnabled, ref CardAbilityIcons __instance)
        {
            CurseIconInteractable interactable = __instance.GetCurseIcon();
            if (interactable != null)
                interactable.SetEnabled(interactionEnabled && interactable.CurseAssigned);
        }

        [HarmonyPatch(typeof(CardAbilityIcons), "DisplayBoonIcon")]
        [HarmonyPostfix]
        public static void DisplayCurseIcon(CardInfo cardInfo, ref CardAbilityIcons __instance)
        {
            CurseIconInteractable interactable = __instance.GetCurseIcon();
            if (interactable != null)
                interactable.AssignCurse(cardInfo.GetCurse());
        }

        public static GameObject BoonIconPrefab = Resources.Load<GameObject>("prefabs/cards/cardsurfaceinteraction/boonicon");

        [HarmonyPatch(typeof(Card), "SetInfo")]
        [HarmonyPrefix]
        public static void CreateCurseIconInteractable(CardInfo info, ref Card __instance)
        {
            if (__instance.AbilityIcons.GetCurseIcon() == null)
            {
                Traverse abilityIconTraver = Traverse.Create(__instance.AbilityIcons);
                BoonIconInteractable boonInteractable = abilityIconTraver.Field("boonIcon").GetValue<BoonIconInteractable>();
                
                GameObject curseIcon = GameObject.Instantiate(BoonIconPrefab);
                curseIcon.name = "CurseIcon";
                curseIcon.transform.SetParent(boonInteractable.gameObject.transform.parent);

                CurseIconInteractable interactable = curseIcon.AddComponent<CurseIconInteractable>();
                __instance.AbilityIcons.SetCurseIcon(interactable);
                curseIcon.transform.localPosition = boonInteractable.gameObject.transform.localPosition;
                curseIcon.transform.localRotation = boonInteractable.gameObject.transform.localRotation;
                curseIcon.transform.localScale = boonInteractable.gameObject.transform.localScale;

                Component.Destroy(curseIcon.GetComponent<BoonIconInteractable>()); // The new curse icon doesn't need a boon icon component
            }

            __instance.AbilityIcons.GetCurseIcon().AssignCurse(info.GetCurse());
        }

        [HarmonyPatch(typeof(Card), "SetFaceDown")]
        [HarmonyPostfix]
        public static void HideCurseIconOnFacedown(bool faceDown, ref Card __instance)
        {
            CurseIconInteractable icon = __instance.AbilityIcons.GetCurseIcon();
            if (icon != null)
            {
                icon.gameObject.SetActive(!faceDown);
            }
        }
    }
}