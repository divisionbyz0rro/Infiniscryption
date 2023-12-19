using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Ascension;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace Infiniscryption.Achievements
{
    [HarmonyPatch]
    public class AchievementScreen : AscensionRunSetupScreenBase
    {
        public override string headerText => "Choose Active Packs";
        public override bool showCardDisplayer => false;
        public override bool showCardPanel => false;

        public static readonly AscensionMenuScreens.Screen ScreenID = GuidManager.GetEnumValue<AscensionMenuScreens.Screen>(AchievementsPlugin.PluginGuid, "AchievementsScreen");

        public static AchievementScreen Instance { get; private set; }

        private List<AchievementBadge> Badges = new();

        private ModdedAchievementManager.AchievementGroup ActiveGroup => this.AllGroups[this.ActiveGroupIndex];
        private int ActiveGroupIndex { get; set; } = 0;
        private int GroupPage { get; set; } = 0;

        private MainInputInteractable leftPageButton = null;
        private MainInputInteractable rightPageButton = null;
        private GBC.PixelText pageNumbers = null;

        internal static AchievementScreen CreateInstance()
        {
            if (Instance != null)
                GameObject.DestroyImmediate(Instance.gameObject);

            Instance = AscensionRunSetupScreenBase.BuildScreen(typeof(AchievementScreen), AscensionMenuScreens.Screen.UnlockSummary, AscensionMenuScreens.Screen.UnlockSummary) as AchievementScreen;

            // Now we need to modify the unlock screen to have a button for this
            var unlockScreen = AscensionMenuScreens.Instance.unlockSummaryScreen;
            var unlockOptionParent = unlockScreen.transform.Find("Stats/ScreenAnchor");
            List<Transform> buttons = new();
            foreach (Transform child in unlockOptionParent.transform)
                buttons.Add(child);

            // Duplicate it
            GameObject button = GameObject.Instantiate(buttons[buttons.Count - 1].gameObject, buttons[buttons.Count - 1].parent);
            button.transform.localPosition = new(button.transform.localPosition.x, button.transform.localPosition.y - 0.11f, button.transform.localPosition.z);
            button.GetComponentInChildren<GBC.PixelText>().SetText(Localization.Translate("- ACHIEVEMENTS -"));
            button.GetComponentInChildren<AscensionMenuInteractable>().CursorSelectStarted = (mii) => AscensionMenuScreens.Instance.SwitchToScreen(ScreenID);

            return Instance;
        }

        public override void InitializeScreen(GameObject partialScreen)
        {
            GameObject iconContainer = new GameObject("GameIcons");
            iconContainer.transform.SetParent(partialScreen.transform);
            AnchorToScreenEdge anchor = iconContainer.AddComponent<AnchorToScreenEdge>();
            anchor.anchorToMidpoint = true;
            anchor.anchorToBottom = false;
            anchor.anchorYOffset = -.25f;
            anchor.worldAnchor = partialScreen.transform.Find("Footer");

            // We need 8 achievement displayers
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 2; c++)
                {
                    AchievementBadge badge = AchievementBadge.Create(iconContainer.transform, Camera.main, rightAnchor: c == 0 ? .5f : 1f);
                    badge.gameObject.transform.localScale = new(1f, 1f, 1f);
                    ViewportRelativePosition vrp = badge.GetComponent<ViewportRelativePosition>();
                    vrp.viewportAnchor = new(0.09f + 0.5f * c, 0.75f - 0.175f * r);
                    vrp.offset = new(0f, 0f);

                    float rAnchor = c == 0 ? 0.5f : 1.0f;

                    badge.gameObject.SetActive(false);

                    Badges.Add(badge);
                }
            }


            // I need some arrow buttons
            var pageTuple = AscensionRunSetupScreenBase.BuildPaginators(iconContainer.transform);

            this.leftButton = pageTuple.Item1;
            this.rightButton = pageTuple.Item2;

            Action<MainInputInteractable> leftClickAction = (mii) => this.LeftButtonClicked(mii);
            Action<MainInputInteractable> rightClickAction = (mii) => this.RightButtonClicked(mii);

            this.leftButton.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(this.leftButton.CursorSelectStarted, leftClickAction);
            this.rightButton.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(this.rightButton.CursorSelectStarted, rightClickAction);

            this.leftButton.gameObject.GetComponent<ViewportRelativePosition>().viewportAnchor = new(.239f, .92f);
            this.rightButton.gameObject.GetComponent<ViewportRelativePosition>().viewportAnchor = new(.761f, .92f);

            // I need another set of arrow buttons
            var subPageTuple = AscensionRunSetupScreenBase.BuildPaginators(iconContainer.transform);

            this.leftPageButton = subPageTuple.Item1;
            this.rightPageButton = subPageTuple.Item2;

            Action<MainInputInteractable> leftSubClickAction = (mii) => this.LeftPageButtonClicked(mii);
            Action<MainInputInteractable> rightSubClickAction = (mii) => this.RightPageButtonClicked(mii);

            this.leftPageButton.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(this.leftPageButton.CursorSelectStarted, leftSubClickAction);
            this.rightPageButton.CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(this.rightPageButton.CursorSelectStarted, rightSubClickAction);

            this.leftPageButton.gameObject.GetComponent<ViewportRelativePosition>().viewportAnchor = new(.65f, .1f);
            this.rightPageButton.gameObject.GetComponent<ViewportRelativePosition>().viewportAnchor = new(.8f, .1f);

            // And I need one more text container
            GameObject pageNumberObj = GameObject.Instantiate(ResourceBank.Get<GameObject>("prefabs/gbcui/PixelTextCanvas"), iconContainer.transform);
            pageNumberObj.name = "PageNumber";
            pageNumberObj.layer = this.screenTitle.gameObject.layer;
            this.pageNumbers = pageNumberObj.GetComponent<GBC.PixelText>();
            this.pageNumbers.SetText("1 / 1");
            this.pageNumbers.SetColor(GameColors.Instance.red);
            ViewportRelativePosition pnVrp = pageNumberObj.AddComponent<ViewportRelativePosition>();
            pnVrp.viewportCam = Camera.main;
            pnVrp.viewportAnchor = new(0.725f, 0.105f);
            pnVrp.enabled = true;
            pageNumberObj.SetActive(true);

            // We don't need a continue button
            this.continueButton.gameObject.SetActive(false);
            this.challengeHeaderDisplay.headerPointsLines.lines.ForEach(pt => pt.gameObject.SetActive(false));
            this.challengeFooterLines.lines.ForEach(pt => pt.gameObject.SetActive(false));
            this.screenTitle.gameObject.transform.localPosition = new(0f, -0.17f, 0f);
        }

        private void SyncComponents()
        {
            var grp = ModdedAchievementManager.GroupById(this.ActiveGroup);
            this.screenTitle.SetText(Localization.Translate(grp.EnglishGroupName));
            this.screenTitle.SetText(Localization.Translate(grp.EnglishGroupName));

            var numPages = Mathf.CeilToInt(((float)grp.Achievements.Count()) / ((float)this.Badges.Count));
            this.pageNumbers.SetText($"{this.GroupPage + 1} / {numPages}");

            int startIdx = this.Badges.Count * this.GroupPage;
            int numToShow = Math.Min(this.Badges.Count, grp.Achievements.Count - startIdx);
            var achs = grp.Achievements.GetRange(startIdx, numToShow);

            var achString = String.Join(", ", achs);
            AchievementsPlugin.Log.LogDebug($"Displaying achievements {achString}");

            this.Badges.ForEach(b => b.gameObject.SetActive(false));

            for (int i = 0; i < achs.Count; i++)
            {
                try
                {
                    AchievementsPlugin.Log.LogDebug($"Setting achievement {achs[i].EnglishName} active");
                    this.Badges[i].DisplayAchievement(achs[i].ID);
                    this.Badges[i].gameObject.SetActive(true);
                }
                catch (Exception ex)
                {
                    AchievementsPlugin.Log.LogError(ex);
                    this.Badges[i].gameObject.SetActive(false);
                }
            }
        }

        private void LeftPageButtonClicked(MainInputInteractable button)
        {
            try
            {
                if (this.GroupPage > 0)
                {
                    this.GroupPage -= 1;
                    this.SyncComponents();
                }
            }
            catch
            {
                this.ActiveGroupIndex = 0;
                this.GroupPage = 0;
                this.SyncComponents();
            }
        }

        private void RightPageButtonClicked(MainInputInteractable button)
        {
            try
            {
                int numberOfPages = Mathf.CeilToInt(((float)ModdedAchievementManager.GroupById(this.ActiveGroup).Achievements.Count) / ((float)this.Badges.Count));
                if (this.GroupPage < numberOfPages - 1)
                {
                    this.GroupPage += 1;
                    this.SyncComponents();
                }
            }
            catch
            {
                this.ActiveGroupIndex = 0;
                this.GroupPage = 0;
                this.SyncComponents();
            }
        }

        public override void LeftButtonClicked(MainInputInteractable button)
        {
            try
            {
                if (this.ActiveGroupIndex > 0)
                {
                    this.ActiveGroupIndex -= 1;
                    this.GroupPage = 0;
                    this.SyncComponents();
                }
            }
            catch
            {
                this.ActiveGroupIndex = 0;
                this.GroupPage = 0;
                this.SyncComponents();
            }
        }

        public override void RightButtonClicked(MainInputInteractable button)
        {
            try
            {
                if (this.ActiveGroupIndex < this.AllGroups.Count - 1)
                {
                    this.ActiveGroupIndex += 1;
                    this.GroupPage = 0;
                    this.SyncComponents();
                }
            }
            catch
            {
                this.ActiveGroupIndex = 0;
                this.GroupPage = 0;
                this.SyncComponents();
            }
        }

        List<ModdedAchievementManager.AchievementGroup> AllGroups = new();

        public override void OnEnable()
        {
            Badges.ForEach(b => b.ReAlign());

            AllGroups.Clear();
            AllGroups.AddRange(GuidManager.GetValues<ModdedAchievementManager.AchievementGroup>().Where(
                ag => ag != ModdedAchievementManager.AchievementGroup.StoryAchievements
                      && ModdedAchievementManager.GroupById(ag) != null
            ));

            this.ActiveGroupIndex = 0;
            this.GroupPage = 0;
            this.SyncComponents();
            this.SyncComponents();

            base.OnEnable();
        }

        // Because we aren't setting ourselves up as a "run setup screen" the API's assumptions don't hold
        // So we can't actually rely on the patches from the API to handle this boilerplate stuff for us
        [HarmonyPatch(typeof(AscensionMenuScreens), "ConfigurePostGameScreens")]
        [HarmonyPostfix]
        private static void InitializeScreensOnStart()
        {
            CreateInstance();
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), "DeactivateAllScreens")]
        [HarmonyPostfix]
        private static void DeactivateAllCustomScreens()
        {
            if (Instance != null)
                Instance.gameObject.SetActive(false);
        }

        [HarmonyPatch(typeof(AscensionMenuScreens), "ScreenSwitchSequence")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.VeryLow)]
        private static IEnumerator SwitchToScreen(IEnumerator sequenceEvent, AscensionMenuScreens.Screen screen)
        {
            while (sequenceEvent.MoveNext())
                yield return sequenceEvent.Current;

            yield return new WaitForSeconds(0.05f);

            if (Instance != null && ScreenID == screen)
            {
                // I don't get it. Something isn't triggering correctly the
                // first time the screen is activated, so what if I do it twice?
                Instance.gameObject.SetActive(true);
                Instance.gameObject.SetActive(false);
                Instance.gameObject.SetActive(true);
            }

            yield break;
        }
    }
}