using System.Collections.Generic;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System;
using GBC;
using InscryptionAPI.Ascension;
using Infiniscryption.Core.Helpers;

namespace Infiniscryption.PackManagement.UserInterface
{
    [AscensionScreenSort(AscensionScreenSort.Direction.RequiresStart)]
    public class PackSelectorScreen : AscensionRunSetupScreenBase
    {
        public override string headerText => "Choose Active Packs";
        public override bool showCardDisplayer => true;
        public override bool showCardPanel => false;

        public static PackSelectorScreen Instance;

        private static GameObject _textPseudoPrefab;
        private static GameObject TextPseudoPrefab
        {
            get
            {
                if (_textPseudoPrefab == null)
                    _textPseudoPrefab = AscensionMenuScreens.Instance.startScreen.transform.Find("Center/Subtitle/PixelTextLine_DIV").gameObject;
                
                return _textPseudoPrefab;
            }
        }

        private static Sprite _coveredSprite;
        private static Sprite CoveredSprite
        {
            get
            {
                if (_coveredSprite == null)
                {
                    _coveredSprite = Sprite.Create(AssetHelper.LoadTexture("deselected"), new Rect(0f, 0f, 46f, 74f), new Vector2(0.5f, 0.5f));
                    _coveredSprite.name = "CoveredSprite";
                }
                
                return _coveredSprite;
            }
        }

        private static Sprite _defaultPackSprite;
        internal static Sprite DefaultPackSprite
        {
            get
            {
                if (_defaultPackSprite == null)
                {
                    _defaultPackSprite = Sprite.Create(AssetHelper.LoadTexture("default_window"), new Rect(0f, 0f, 46f, 74f), new Vector2(0.5f, 0.5f));
                    _defaultPackSprite.name = "DefaultPackSprite";
                }
                
                return _defaultPackSprite;
            }
        }

        private static Vector2 FIRST_CARD_OFFSET = new Vector2(-1.5f, 0f);

        private static Vector2 BETWEEN_CARD_OFFSET = new Vector2(0.5f, 0f);

        private static PackIcon GeneratePackIcon(Transform parent)
        {
            GameObject obj = new GameObject("Pack");
            obj.transform.SetParent(parent);
            obj.layer = LayerMask.NameToLayer("GBCUI");
            PackIcon retval = obj.AddComponent<PackIcon>();

            GameObject pack = new GameObject("PackSprite");
            pack.transform.SetParent(obj.transform);
            pack.layer = LayerMask.NameToLayer("GBCUI");
            retval.IconRenderer = pack.AddComponent<SpriteRenderer>();
            retval.IconRenderer.sprite = DefaultPackSprite;
            retval.IconRenderer.enabled = true;
            retval.IconRenderer.sortingOrder = 200;

            GameObject title = GameObject.Instantiate(TextPseudoPrefab, obj.transform);
            retval.Text = title.GetComponentInChildren<PixelText>();
            retval.Text.SetColor(Color.black);
            retval.Text.SetSortingOrder(225);
            title.transform.localPosition = new Vector3(0f, 0.2f, 0f);

            GameObject packIcon = new GameObject("PackCardIcon");
            packIcon.transform.SetParent(obj.transform);
            packIcon.layer = LayerMask.NameToLayer("GBCUI");
            retval.SampleCardRenderer = packIcon.AddComponent<SpriteRenderer>();
            retval.SampleCardRenderer.sprite = CardLoader.GetCardByName("Grizzly").portraitTex;
            retval.SampleCardRenderer.enabled = true;
            retval.SampleCardRenderer.sortingOrder = 230;
            packIcon.transform.localPosition = new Vector3(0f, -.07f, 0f);
            packIcon.transform.localScale = new Vector3(0.36f, 0.36f, 0.36f);

            GameObject selected = new GameObject("Covered");
            selected.transform.SetParent(obj.transform);
            selected.layer = LayerMask.NameToLayer("GBCUI");
            retval.CoveredRenderer = selected.AddComponent<SpriteRenderer>();
            retval.CoveredRenderer.sprite = CoveredSprite;
            retval.CoveredRenderer.enabled = true;
            retval.CoveredRenderer.sortingOrder = 250;

            BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
            collider.size = retval.IconRenderer.size;

            return retval;
        }

        private List<PackIcon> PackIcons;

        public override void InitializeScreen(GameObject partialScreen)
        {
            // I need a row of packs with hovers
            GameObject iconContainer = new GameObject("GameIcons");
            iconContainer.transform.SetParent(partialScreen.transform);
            AnchorToScreenEdge anchor = iconContainer.AddComponent<AnchorToScreenEdge>();
            anchor.anchorToMidpoint = true;
            anchor.anchorToBottom = false;
            anchor.anchorYOffset = -.25f;
            anchor.worldAnchor = partialScreen.transform.Find("Footer");

            PackIcons = new List<PackIcon>();
            for (int i = 0; i < 7; i++)
            {
                PackIcon icon = GeneratePackIcon(iconContainer.transform);
                icon.gameObject.transform.localPosition = FIRST_CARD_OFFSET + i * BETWEEN_CARD_OFFSET;
                PackIcons.Add(icon);
            }

            // Save the instance
            Instance = this;
        }

        public override void LeftButtonClicked(MainInputInteractable button)
        {
            if (scrollIndex > 0)
            {
                scrollIndex -= 1;
                ShowPage();
            }
        }

        public override void RightButtonClicked(MainInputInteractable button)
        {
            if ((scrollIndex + 1) * this.PackIcons.Count < this.cache.OrderedPacks.Count)
            {
                scrollIndex += 1;
                ShowPage();
            }
        }

        private int scrollIndex = 0;

        public bool resetSelection = true;

        public void ShowPage()
        {
            int startIdx = this.cache.OrderedPacks.Count * scrollIndex;
            int numToShow = Math.Min(this.PackIcons.Count, this.cache.OrderedPacks.Count - startIdx);
            this.ShowPacks(this.cache.OrderedPacks.GetRange(startIdx, numToShow));
        }

        public void InitializeCardSelection()
        {
            // Hide the left and right buttons if the number of available side deck cards is <= the number of card panels
            this.challengeHeaderDisplay.UpdateText();

            // this.leftButton.gameObject.SetActive(this.PackIcons.Count < ScreenActivePacks.Count);
            // this.rightButton.gameObject.SetActive(this.PackIcons.Count < ScreenActivePacks.Count);
        }

        public void ShowPacks(List<PackInfo> packsToDisplay)
        {
            foreach (PackIcon pack in this.PackIcons)
                pack.gameObject.SetActive(false);

            int numToShow = Math.Min(this.PackIcons.Count, packsToDisplay.Count);

            float gaps = ((float)(this.PackIcons.Count - numToShow)) / 2f;
            Vector2 startPos = FIRST_CARD_OFFSET + BETWEEN_CARD_OFFSET * gaps;

            for (int i = 0; i < numToShow; i++)
            {
                this.PackIcons[i].AssignPackInfo(packsToDisplay[i], this.cache);
                this.PackIcons[i].gameObject.transform.localPosition = startPos + (float)i * BETWEEN_CARD_OFFSET;
                this.PackIcons[i].gameObject.SetActive(true);
            }
        }

        private PackContentCache cache;

        public override void OnEnable()
        {
            base.OnEnable();

            this.cache = new();

            // Reset active from inactive
            List<PackInfo> inactivePacks = PackManager.RetrievePackList(false);
            PackManager.SavePackList(this.cache.OrderedPacks.Where(pi => !inactivePacks.Contains(pi)).ToList(), true);

            scrollIndex = 0;
            InitializeCardSelection();
            ShowPage();
        }
    }
}