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
using Infiniscryption.Curses.Components;
using InscryptionAPI.Helpers;
using System.Linq;
using System.Runtime.CompilerServices;
using InscryptionAPI.Saves;

namespace Infiniscryption.Curses.Patchers
{
    public static partial class DeathcardHaunt
    {
        // This part manages the haunt level

        private const int MAX_HAUNT_LEVEL = 11;

        private static int HauntLevel
        {
            get 
            { 
                // Your haunt level is the base haunt level (which increases by winning, gets reset to 0 by losing, and 
                // decreases whenever you kill an opposing deathcard) plus a whopping 3 if you have killed the survivors,
                // plus 1 for even 'minor starting bones' in your boons and 2 for every 'starting bones' in your boons.
                return ModdedSaveManager.RunState.GetValueAsInt(CursePlugin.PluginGuid, "Curse.BaseHauntLevel")
                + (RunState.Run.survivorsDead ? 3 : 0)
                + (RunState.Run.playerDeck.Boons.FindAll(boon => boon.type == BoonData.Type.MinorStartingBones).Count)
                + (RunState.Run.playerDeck.Boons.FindAll(boon => boon.type == BoonData.Type.StartingBones).Count * 2);
            }
        }

        public static void ResetHaunt(int value=0)
        {
            CursePlugin.Log.LogInfo($"Resetting haunt to {value}");
            ModdedSaveManager.RunState.SetValue(CursePlugin.PluginGuid, "Curse.BaseHauntLevel", value);
        }

        public static void IncreaseHaunt(int by=1)
        {
            // Make sure the haunt level 
            int newHauntLevel = Mathf.Clamp(ModdedSaveManager.RunState.GetValueAsInt(CursePlugin.PluginGuid, "Curse.BaseHauntLevel") + by, 0, MAX_HAUNT_LEVEL);
            CursePlugin.Log.LogInfo($"Updated haunt by {by} to {newHauntLevel}");
            ModdedSaveManager.RunState.SetValue(CursePlugin.PluginGuid, "Curse.BaseHauntLevel", newHauntLevel.ToString());
        }

        private static List<GameObject> _orbitingFace = null;

        private static Vector3 UP_REF = Vector3.up;
        private static Vector3 START_REF = Vector3.forward;

        private static void BuildOrbiters(AnimatedGameMapMarker marker)
        {
            CursePlugin.Log.LogInfo("Adding rotating sprite");

            _orbitingFace = new List<GameObject>();

            for (int i = 0; i < 3; i++)
            {

                GameObject orbitingFace = new GameObject();
                orbitingFace.transform.SetParent(marker.Anim.gameObject.transform);
                orbitingFace.transform.localPosition = new Vector3(0f, 0f, 0f);
                orbitingFace.transform.rotation = Quaternion.LookRotation(-UP_REF);
                
                // Add a sprite renderer
                SpriteRenderer spriteRenderer = orbitingFace.AddComponent<SpriteRenderer>();
                Texture2D dct = TextureHelper.GetImageAsTexture("orbit_icon.png", typeof(DeathcardHaunt).Assembly);
                spriteRenderer.sprite = Sprite.Create(dct, new Rect(0f, 0f, dct.width, dct.height), new Vector2(0.5f, 0.5f));

                Orbiter orbitController = orbitingFace.AddComponent<Orbiter>();
                orbitController.Cos0Vector = START_REF;
                orbitController.Sin0Vector = Vector3.Cross(START_REF, UP_REF);
                orbitController.OrbitRadius = 0.35f;
                orbitController.OrbitSpeed = 0.25f;
                orbitController.ThetaOffset = i * 120;
                orbitController.StartFromBeginning();

                _orbitingFace.Add(orbitingFace);
            }

            SyncOrbiters(marker);
        }

        // Here, we apply an effect to the player to indicate that they're haunted.
        [HarmonyPatch(typeof(PlayerMarker), "Awake")]
        [HarmonyPostfix]
        public static void AttachAnimationToPlayerMarker(ref PlayerMarker __instance)
        {
            if (AscensionSaveData.Data.ChallengeIsActive(ID))
            {
                // We need to create an animated sprite game object and lay it underneath the player's feet
                if (_orbitingFace == null)
                {
                    BuildOrbiters(__instance);
                }
            }
        }

        private static void SyncOrbiters(AnimatedGameMapMarker marker)
        {
            if (_orbitingFace == null)
            {
                BuildOrbiters(marker);
                return;
            }
            
            foreach (GameObject obj in _orbitingFace)
            {
                if (obj == null)
                {
                    BuildOrbiters(marker);
                    return;
                }
            }

            int hauntLevel = HauntLevel;
            CursePlugin.Log.LogInfo($"Setting orbit for haunt level {hauntLevel}");

            if (!_orbitingFace[2].activeSelf && hauntLevel >= 8 ||
                !_orbitingFace[1].activeSelf && hauntLevel >= 5 ||
                !_orbitingFace[0].activeSelf && hauntLevel >= 2)
                ChallengeActivationUI.TryShowActivation(ID);
                
            _orbitingFace[2].SetActive(hauntLevel >= 8);
            _orbitingFace[1].SetActive(hauntLevel >= 5);
            _orbitingFace[0].SetActive(hauntLevel >= 2);

            if (hauntLevel >= 8)
                AchievementManager.Unlock(CursedAchievements.TRIPLE_HAUNT);
        }

        [HarmonyPatch(typeof(AnimatedGameMapMarker), "Show")]
        [HarmonyPostfix]
        public static void SetActiveOrbiters(ref AnimatedGameMapMarker __instance)
        {
            CursePlugin.Log.LogInfo("Showing map marker");
            if (AscensionSaveData.Data.ChallengeIsActive(ID) && __instance is PlayerMarker)
            {
                CursePlugin.Log.LogInfo("Is player marker");
                if (_orbitingFace != null)
                {
                    SyncOrbiters(__instance);
                }
                else
                {
                    BuildOrbiters(__instance);
                }
            }
        }

        private static bool HasExplainedHaunt
        {
            get { return ModdedSaveManager.SaveData.GetValueAsBoolean(CursePlugin.PluginGuid, "Curses.HauntExplanation"); }
            set { ModdedSaveManager.SaveData.SetValue(CursePlugin.PluginGuid, "Curses.HauntExplanation", value); }
        }

        [HarmonyPatch(typeof(GameMap), "ShowMapSequence")]
        [HarmonyPostfix]
        public static IEnumerator GiveHauntExplanation(IEnumerator sequenceResult)
        {
            while (sequenceResult.MoveNext())
                yield return sequenceResult.Current;

            if (AscensionSaveData.Data.ChallengeIsActive(ID) && !HasExplainedHaunt && HauntLevel >= 2)
            {
                yield return TextDisplayer.Instance.PlayDialogueEvent("HauntedExplanation", TextDisplayer.MessageAdvanceMode.Input, TextDisplayer.EventIntersectMode.Wait, null, null);
                HasExplainedHaunt = true;
            }
        }
    }
}