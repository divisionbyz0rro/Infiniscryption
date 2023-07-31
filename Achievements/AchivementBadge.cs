using UnityEngine;
using DiskCardGame;
using System;
using InscryptionAPI.Card;
using System.Collections.Generic;
using System.Linq;
using GBC;
using UnityEngine.SceneManagement;

namespace Infiniscryption.Achievements
{
    public class AchievementBadge : ManagedBehaviour
    {
        private static int ORTHO_LAYER => SceneManager.GetActiveScene().name.Equals("Ascension_Configure", StringComparison.InvariantCultureIgnoreCase) ? LayerMask.NameToLayer("GBCUI") : LayerMask.NameToLayer("OrthographicUI");

        public ViewportRelativePosition ViewportPosition { get; set; }

        private SpriteRenderer AchievementSprite { get; set; }
        private GBC.PixelText HeaderDisplayer { get; set; }
        private GBC.PixelText AchievementTitleDisplayer { get; set; }

        /// <summary>
        /// Sets the badge to display the achievement toast (Achievement Unlocked)
        /// </summary>
        public void ToastAchievement(Achievement id)
        {
            var def = ModdedAchievementManager.AchievementById(id);
            var grp = ModdedAchievementManager.GroupByAchievementId(id);
            this.AchievementSprite.sprite = def.IconSprite;
            this.AchievementTitleDisplayer.SetText(Localization.Translate(def.EnglishName));
            this.HeaderDisplayer.SetText(Localization.Translate("Achievement Unlocked"));
        }

        /// <summary>
        /// Sets the badge to display the achievement description
        /// </summary>
        public void DisplayAchievement(Achievement id)
        {
            var def = ModdedAchievementManager.AchievementById(id);
            var grp = ModdedAchievementManager.GroupByAchievementId(id);

            if (def == null || grp == null)
                return;

            AchievementsPlugin.Log.LogDebug($"Displaying {def.EnglishName}");
            AchievementsPlugin.Log.LogDebug($"Unlocked? {def.IsUnlocked}");

            if (def.Secret && !def.IsUnlocked)
            {
                this.AchievementSprite.sprite = grp.LockedSprite;
                this.HeaderDisplayer.SetText(Localization.Translate("Secret Achievement"));
                this.AchievementTitleDisplayer.SetText(Localization.Translate("Revealed Once Unlocked"));
            }
            else
            {
                this.AchievementSprite.sprite = def.IsUnlocked ? def.IconSprite : grp.LockedSprite;
                this.AchievementTitleDisplayer.SetText(Localization.Translate(def.EnglishDescription));
                this.HeaderDisplayer.SetText(Localization.Translate(def.EnglishName));
            }
        }

        public static AchievementBadge Create(Transform parent, Camera viewportCam = null, bool wide = true)
        {
            GameObject achievementContainer = new("AchievementDisplayer");
            achievementContainer.transform.SetParent(parent);
            achievementContainer.transform.localPosition = new (0, 0, 1);
            achievementContainer.transform.localScale = new(2.5f, 2.5f, 1f);
            achievementContainer.SetActive(false);

            AchievementBadge handler = achievementContainer.AddComponent<AchievementBadge>();

            if (viewportCam != null)
            {
                achievementContainer.layer = ORTHO_LAYER;
                handler.ViewportPosition = achievementContainer.AddComponent<ViewportRelativePosition>();
                handler.ViewportPosition.viewportCam = viewportCam;
                handler.ViewportPosition.viewportAnchor = new (0.4f, 0f);
                handler.ViewportPosition.offset = new (0f, 0.5f);
                handler.ViewportPosition.enabled = true;
            }

            GameObject icon = new("AchievementIcon");
            icon.transform.SetParent(achievementContainer.transform);

            if (viewportCam != null)
                icon.layer = ORTHO_LAYER;

            icon.transform.localPosition = new (-0.25f, 0f, 0f);
            icon.transform.localScale = new (1f, 1f, 1f);
            handler.AchievementSprite = icon.AddComponent<SpriteRenderer>();
            handler.AchievementSprite.sprite = ModdedAchievementManager.AchievementById(Achievement.KMOD_CHALLENGELEVEL1).IconSprite;
            handler.AchievementSprite.enabled = true;
            handler.AchievementSprite.sortingOrder = 230;

            GameObject congrats = GameObject.Instantiate(ResourceBank.Get<GameObject>("prefabs/gbcui/PixelTextCanvas"), achievementContainer.transform);
            congrats.name = "CongratsTitle";

            if (viewportCam != null)
                congrats.layer = ORTHO_LAYER;

            congrats.transform.Find("Text").GetComponent<RectTransform>().sizeDelta = new(wide ? 500f : 200f, 50f);
            congrats.transform.localPosition = new(wide ? 2.4f : 0.9f, 0.06f, 0f);
            handler.HeaderDisplayer = congrats.GetComponent<GBC.PixelText>();
            handler.HeaderDisplayer.mainText.alignment = TextAnchor.MiddleLeft;
            handler.HeaderDisplayer.SetColor(GameColors.Instance.nearWhite);
            handler.HeaderDisplayer.SetText(Localization.Translate("Achievement Unlocked"));

            GameObject achievementName = GameObject.Instantiate(ResourceBank.Get<GameObject>("prefabs/gbcui/PixelTextCanvas"), achievementContainer.transform);
            achievementName.name = "AchievementTitle";

            if (viewportCam != null)
                achievementName.layer = ORTHO_LAYER;

            achievementName.transform.Find("Text").GetComponent<RectTransform>().sizeDelta = new(wide ? 500f : 200f, 50f);
            achievementName.transform.localPosition = new(wide ? 2.4f : 0.9f, -0.23f, 0f);
            handler.AchievementTitleDisplayer = achievementName.GetComponent<GBC.PixelText>();
            handler.AchievementTitleDisplayer.mainText.alignment = TextAnchor.UpperLeft;
            handler.AchievementTitleDisplayer.SetColor(GameColors.Instance.red);
            handler.AchievementTitleDisplayer.SetText(Localization.Translate(ModdedAchievementManager.AchievementById(Achievement.KMOD_CHALLENGELEVEL1).EnglishName));

            return handler;
        }
    }
}