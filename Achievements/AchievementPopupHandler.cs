using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BepInEx.Bootstrap;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Regions;
using InscryptionAPI.Saves;
using Pixelplacement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infiniscryption.Achievements
{
    [HarmonyPatch]
    public class AchievementPopupHandler : ManagedBehaviour
    {
        public static AchievementPopupHandler Instance { get; private set; }

        public const float POPUP_TIMER = 5f;

        private List<Achievement> showQueue = new();

        private AchievementBadge PopupBadge;

        /// <summary>
        /// Displays the achievement unlocked popup. Does NOT check to see if the achievement has already been unlocked.
        /// </summary>
        /// <param name="id">The achievement to display</param>
        public void TryShowUnlockAchievement(Achievement id)
        {
            var def = ModdedAchievementManager.AchievementById(id);
            var grp = ModdedAchievementManager.GroupByAchievementId(id);

            if (def.IconSprite == null)
                return;

            if (this.gameObject.activeSelf)
            {
                showQueue.Add(id);
                return;
            }

            AchievementsPlugin.Log.LogDebug($"Showing achievement popup for {id}");
            this.PopupBadge.ToastAchievement(id);

            this.gameObject.SetActive(true);
            Tween.Value(-0.3f, 0.4f, (v) => this.PopupBadge.ViewportPosition.offset = new(0f, v), 0.2f, 0f);

            AudioController.Instance.PlaySound2D(grp.AudioCue, volume: 0.75f);

            AchievementsPlugin.Log.LogDebug($"Scheduling achievement popup close for {id}");
            CustomCoroutine.WaitThenExecute(POPUP_TIMER, delegate ()
            {
                AchievementsPlugin.Log.LogDebug($"Closing achievement popup for {id}");
                Tween.Value(0.4f, -0.3f, (v) => this.PopupBadge.ViewportPosition.offset = new(0f, v), 0.2f, 0f, completeCallback: () => this.gameObject.SetActive(false));
                if (this.showQueue.Count > 0)
                {
                    Achievement next = this.showQueue[0];
                    this.showQueue.RemoveAt(0);
                    CustomCoroutine.WaitThenExecute(1f, () => this.TryShowUnlockAchievement(next));
                }
            });
        }

        internal static void Initialize(string sceneName)
        {
            try
            {
                // We need three things: a sprite for the achievement icon, a GBC text line for "Achievement Unlocked", and
                // a GBC text line for the acheivement name
                GameObject cameraContainer = null;
                Camera camera = null;
                Vector3 scale = new(2f, 2f, 1f);
                if (sceneName.Equals("Ascension_Configure", StringComparison.InvariantCultureIgnoreCase))
                {
                    cameraContainer = AscensionMenuScreens.Instance.gameObject;
                    camera = Camera.main;
                    scale = new(1f, 1f, 1f);
                }
                else
                {
                    cameraContainer = UIManager.Instance.Canvas.transform.parent.gameObject;
                    camera = cameraContainer.GetComponent<Camera>();
                }

                AchievementBadge popup = AchievementBadge.Create(cameraContainer.transform, camera);
                popup.gameObject.transform.localScale = scale;
                popup.ViewportPosition.offset = new(0f, -0.3f);
                AchievementPopupHandler handler = popup.gameObject.AddComponent<AchievementPopupHandler>();
                handler.PopupBadge = popup;

                AchievementPopupHandler.Instance = handler;
                popup.gameObject.SetActive(false);
            }
            catch (Exception ex)
            {
                AchievementsPlugin.Log.LogInfo($"Could not create singleton Achievement Popup Handler for scene {sceneName}");
                AchievementsPlugin.Log.LogInfo(ex);
            }
        }
    }
}