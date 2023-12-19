using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DiskCardGame;
using HarmonyLib;
using Pixelplacement;
using InscryptionCommunityPatch.Card;
using Unity.Cloud;
using UnityEngine;

namespace Infiniscryption.DefaultRenderers
{
    [HarmonyPatch]
    public class DefaultCardRenderer : MonoBehaviour
    {
        private static readonly Texture MAGNIFICUS_CARD_BACK_TEXTURE = Resources.Load<Texture2D>("art/cards/card_back_magnificus");

        private static readonly Dictionary<CardTemple, GameObject> CardPrefabs = new()
        {
            { CardTemple.Undead, ResourceBank.Get<GameObject>("Prefabs/Cards/PlayableCard_Grimora") },
            { CardTemple.Wizard, ResourceBank.Get<GameObject>("Prefabs/Cards/PlayableCard_Magnificus") },
            { CardTemple.Nature, ResourceBank.Get<GameObject>("Prefabs/Cards/PlayableCard") },
            { CardTemple.Tech, ResourceBank.Get<GameObject>("Prefabs/Cards/PlayableCard_Part3") }
        };

        private static readonly Dictionary<CardTemple, GameObject> SelectableCardPrefabs = new()
        {
            { CardTemple.Undead, ResourceBank.Get<GameObject>("Prefabs/Cards/SelectableCard_Grimora") },
            { CardTemple.Wizard, ResourceBank.Get<GameObject>("Prefabs/Cards/SelectableCard") },
            { CardTemple.Nature, ResourceBank.Get<GameObject>("Prefabs/Cards/SelectableCard") },
            { CardTemple.Tech, ResourceBank.Get<GameObject>("Prefabs/Cards/SelectableCard_Part3") }
        };

        private static readonly Dictionary<CardTemple, GameObject> CardRenderCameraPrefabs = new()
        {
            { CardTemple.Undead, ResourceBank.Get<GameObject>("Prefabs/Cards/CardRenderCamera_Grimora") },
            { CardTemple.Wizard, ResourceBank.Get<GameObject>("Prefabs/Cards/CardRenderCamera_Magnificus") },
            { CardTemple.Nature, ResourceBank.Get<GameObject>("Prefabs/Cards/CardRenderCamera") },
            { CardTemple.Tech, ResourceBank.Get<GameObject>("Prefabs/Cards/CardRenderCamera_Part3") }
        };

        internal readonly List<string> ActiveZoneOverrides = new();

        private static bool _enabledForAllCards = false;
        public static bool EnabledForAllCards
        {
            get => DefaultRenderersPlugin.Instance.RendererAlwaysActive || _enabledForAllCards || (Instance != null && Instance.ActiveZoneOverrides.Count > 0);
            set => _enabledForAllCards = value;
        }

        /// <summary>
        /// Temporarily activates the renderer for all cards
        /// </summary>
        /// <param name="pluginGuid">The guid of the plugin that wans to activate the renderer</param>
        public void EnableGlobalRenderer(string pluginGuid)
        {
            if (!ActiveZoneOverrides.Contains(pluginGuid))
                ActiveZoneOverrides.Add(pluginGuid);
        }

        /// <summary>
        /// Undoes a temporary activation of the renderer
        /// </summary>
        /// <param name="pluginGuid">The guid of the plugin that wants to de-activate the renderer</param>
        /// <param name="force">If true, this will deactivate rendering even if other plugins have asked for it</param>
        public void CancelGlobalRenderer(string pluginGuid, bool force = false)
        {
            if (force)
                ActiveZoneOverrides.Clear();
            else
                ActiveZoneOverrides.Remove(pluginGuid);
        }

        private Dictionary<CardTemple, CardRenderCamera> CardRenderCameras = null;

        private CardTemple? SaveFileOverride = null;

        public static CardTemple? ActiveTemple = SaveManager.SaveFile == null ? null :
                                      SaveManager.SaveFile.IsPart1 ? CardTemple.Nature :
                                      SaveManager.SaveFile.IsPart3 ? CardTemple.Tech :
                                      SaveManager.SaveFile.IsGrimora ? CardTemple.Undead :
                                      SaveManager.SaveFile.IsMagnificus ? CardTemple.Wizard
                                      : null;

        private static DefaultCardRenderer m_instance = null;
        public static DefaultCardRenderer Instance
        {
            get
            {
                if (m_instance != null)
                    return m_instance;
                Instantiate();
                return m_instance;
            }
            private set => m_instance = value;
        }

        public static void Instantiate()
        {
            try
            {
                if (ActiveTemple == null)
                    return;

                if (m_instance != null)
                    return;

                if (CardRenderCamera.Instance == null)
                    return;
            }
            catch
            {
                return;
            }

            // Instantiate a new game object that lives with the card render cameras
            GameObject gameObject = new("DefaultCardRenderers");
            gameObject.transform.SetParent(CardRenderCamera.Instance.transform.parent);

            Instance = gameObject.AddComponent<DefaultCardRenderer>();

            Instance.CardRenderCameras = new();
            Instance.CardRenderCameras[ActiveTemple.Value] = CardRenderCamera.Instance;

            int idx = 2;
            foreach (KeyValuePair<CardTemple, GameObject> cameraInfo in CardRenderCameraPrefabs)
            {
                if (cameraInfo.Key == ActiveTemple.Value)
                    continue;

                GameObject obj = Instantiate(cameraInfo.Value, Part3GameFlowManager.Instance.transform);
                obj.transform.position = obj.transform.position + (Vector3.down * 10f * idx++);
                CardRenderCamera camera = obj.GetComponentInChildren<CardRenderCamera>();
                camera.gameObject.name = $"SpecialRenderCamera{cameraInfo.Key}";


                RenderTexture newRendTex = new(camera.snapshotRenderTexture.width, camera.snapshotRenderTexture.height, camera.snapshotRenderTexture.depth, camera.snapshotRenderTexture.format);
                newRendTex.Create();

                RenderTexture newEmTex = new(camera.snapshotEmissionRenderTexture.width, camera.snapshotEmissionRenderTexture.height, camera.snapshotEmissionRenderTexture.depth, camera.snapshotEmissionRenderTexture.format);
                newEmTex.Create();

                foreach (Camera unityCamera in camera.GetComponentsInChildren<Camera>())
                {
                    if (unityCamera.targetTexture == camera.snapshotRenderTexture)
                        unityCamera.targetTexture = camera.snapshotRenderTexture = newRendTex;
                    if (unityCamera.targetTexture == camera.snapshotEmissionRenderTexture)
                        unityCamera.targetTexture = camera.snapshotEmissionRenderTexture = newEmTex;
                }

                Instance.CardRenderCameras[cameraInfo.Key] = camera;
            }
            CardRenderCamera.m_Instance = Instance.CardRenderCameras[ActiveTemple.Value];

            PlayerHand.Instance.cardZSpacing = 0.05f;
        }

        [HarmonyPatch(typeof(SaveFile), nameof(SaveFile.IsPart1), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool HackPart1(ref bool __result)
        {
            if (Instance != null && Instance.SaveFileOverride.HasValue && Instance.SaveFileOverride.Value == CardTemple.Nature)
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SaveFile), nameof(SaveFile.IsPart3), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool HackPart3(ref bool __result)
        {
            if (Instance != null && Instance.SaveFileOverride.HasValue && Instance.SaveFileOverride.Value == CardTemple.Tech)
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SaveFile), nameof(SaveFile.IsGrimora), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool HackGrimora(ref bool __result)
        {
            if (Instance != null && Instance.SaveFileOverride.HasValue && Instance.SaveFileOverride.Value == CardTemple.Undead)
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(SaveFile), nameof(SaveFile.IsMagnificus), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool HackMagnificus(ref bool __result)
        {
            if (Instance != null && Instance.SaveFileOverride.HasValue && Instance.SaveFileOverride.Value == CardTemple.Wizard)
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(RenderStatsLayer), nameof(RenderStatsLayer.RenderCard))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        private static bool OverrideRender(RenderStatsLayer __instance, CardRenderInfo info)
        {
            if (Instance != null)
            {
                CardTemple renderTemple = info.baseInfo.GetRendererTemple();
                if (renderTemple == ActiveTemple)
                    return true;

                if (__instance is PaperRenderStatsLayer)
                {
                    if (info.baseInfo.GetRendererTemple() == CardTemple.Wizard)
                    {
                        info.portraitColor = new Color(1f, 1f, 1f, 0.9f);
                    }
                }

                Instance.SaveFileOverride = renderTemple;
                bool emissionEnabled = CardDisplayer3D.EmissionEnabledForCard(info, __instance.PlayableCard);
                if (!emissionEnabled)
                    __instance.DisableEmission();

                DefaultRenderersPlugin.Log.LogInfo($"Rendering {info.baseInfo.name} {renderTemple} {Instance.CardRenderCameras[renderTemple].gameObject.name}");
                Instance.CardRenderCameras[renderTemple].QueueStatsLayerForRender(info, __instance, __instance.PlayableCard, __instance.RenderToMainTexture, emissionEnabled);
                Instance.SaveFileOverride = null;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(GravestoneCardAnimationController), nameof(GravestoneCardAnimationController.SetCardRendererFlipped))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        private static bool HackForGrimAnim(GravestoneCardAnimationController __instance, bool flipped)
        {
            if (ActiveTemple == CardTemple.Tech)
            {
                __instance.armAnim.transform.localEulerAngles = !flipped ? new Vector3(-270f, 90f, -90f) : new Vector3(-90f, 0f, 0f);
                __instance.armAnim.transform.localPosition = !flipped ? new Vector3(0f, -0.1f, -0.1f) : new Vector3(0f, 0.24f, -0.1f);
                __instance.damageMarks.transform.localPosition = !flipped ? new Vector3(0.19f, -0.37f, -0.01f) : new Vector3(-0.21f, -0.1f, -0.01f);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CardSpawner), nameof(CardSpawner.SpawnPlayableCard))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        private static bool MakeCardWithAppropriatePrefab(CardInfo info, ref PlayableCard __result)
        {
            if (Instance != null)
            {
                CardTemple renderTemple = info.GetRendererTemple();
                if (renderTemple == ActiveTemple)
                    return true;

                DefaultRenderersPlugin.Log.LogInfo("In Custom Make Card");
                Instance.SaveFileOverride = renderTemple;
                GameObject card = Instantiate(CardPrefabs[renderTemple]);

                // Add in the wizard card model
                if (renderTemple == CardTemple.Wizard && ActiveTemple != CardTemple.Wizard)
                {
                    info.appearanceBehaviour = new(info.appearanceBehaviour)
                    {
                        OnboardWizardCardModel.ID
                    };
                }

                PlayableCard playableCard = card.GetComponent<PlayableCard>();
                playableCard.SetInfo(info);
                __result = playableCard;

                // Kind of a funny hack...there's got to be a better way to fix this
                // If I don't do this, the gravestone cards are upside down.
                if (renderTemple == CardTemple.Undead && ActiveTemple == CardTemple.Tech)
                {
                    GameObject newParent = new("Part3Parent");
                    newParent.transform.SetParent(card.transform);
                    //card.transform.Find("SkeletonAttackAnim").SetParent(newParent.transform);
                    card.transform.Find("RotatingParent").SetParent(newParent.transform);
                    newParent.transform.localPosition = Vector3.zero;
                    newParent.transform.localScale = Vector3.one;
                    newParent.transform.localEulerAngles = new(90f, 180f, 0f);
                }

                Instance.SaveFileOverride = null;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CardAnimationController3D), nameof(CardAnimationController3D.Awake))]
        [HarmonyPrefix]
        [HarmonyPriority(HarmonyLib.Priority.VeryHigh)]
        private static bool DontFireAwakeDuringCustomRebuild(CardAnimationController3D __instance)
        {
            // During the custom rebuild, some properties simply don't exist yet and this will NRF.
            // We bypass this and then do it later
            if (__instance.cardRenderer == null)
                return false;
            return true;
        }

        [HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.GetEmissivePortrait))]
        [HarmonyFinalizer]
        private static Exception PreventErrorForMissingArt(Exception __exception, ref Sprite __result)
        {
            if (__exception != null)
            {
                __result = null;
            }
            return null;
        }

        // private static IEnumerator ReRenderCard(SelectableCard card, CardInfo info)
        // {
        //     yield return new WaitForEndOfFrame();
        //     card.Anim.Anim.Rebind();
        //     yield return new WaitForEndOfFrame();
        //     Instance.SaveFileOverride = info.GetRendererTemple();
        //     card.SetFaceDown(card.Flipped, true);
        //     card.SetInfo(info);
        //     if (info.GetRendererTemple() == CardTemple.Tech)
        //     {
        //         yield return new WaitForEndOfFrame();
        //         card.StatsLayer.Material.SetColor("_EmissionColor", Color.white);
        //     }
        //     Instance.SaveFileOverride = null;
        // }

        private static ConditionalWeakTable<SelectableCard, GameObject> InnerCardTable = new();
        private static GameObject GetInnerCard(SelectableCard card)
        {
            if (InnerCardTable.TryGetValue(card, out GameObject innerCard))
                return innerCard;
            return null;
        }
        private static void SetInnerCard(SelectableCard card, GameObject innerCard)
        {
            InnerCardTable.Remove(card);
            InnerCardTable.Add(card, innerCard);
        }

        private static ConditionalWeakTable<CardAnimationController, Card> OuterCardAnimatorTable = new();
        private static Card GetOuterCard(CardAnimationController card)
        {
            if (OuterCardAnimatorTable.TryGetValue(card, out Card innerCard))
                return innerCard;
            return null;
        }
        private static void SetOuterCard(CardAnimationController card, Card innerCard)
        {
            OuterCardAnimatorTable.Remove(card);
            OuterCardAnimatorTable.Add(card, innerCard);
        }

        [HarmonyPatch(typeof(Card), nameof(Card.Anim), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool GetAnimFromChild(Card __instance, ref CardAnimationController __result)
        {
            if (__instance is SelectableCard scard && Instance != null)
            {
                GameObject child = GetInnerCard(scard);
                if (child != null)
                {
                    __result = child.GetComponent<CardAnimationController>();
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(CardAnimationController), nameof(CardAnimationController.Card), MethodType.Getter)]
        [HarmonyPrefix]
        private static bool GetCardFromAnim(CardAnimationController __instance, ref Card __result)
        {
            if (Instance != null)
            {
                Card child = GetOuterCard(__instance);
                if (child != null)
                {
                    __result = child;
                    return false;
                }
            }
            return true;
        }

        private static void ReplaceGutsOfSelectableCard(SelectableCard __instance, CardInfo info, Action<SelectableCard> cardSelected = null, Action<SelectableCard> cardFlipped = null, bool? startFlipped = null, Action<SelectableCard> cardInspected = null)
        {
            Instance.SaveFileOverride = info.GetRendererTemple();
            GameObject gameObject = GameObject.Instantiate(SelectableCardPrefabs[info.GetRendererTemple()], __instance.transform.parent);

            // Go ahead and move the new card to be at our precise location, scale, and rotation
            // So that the reparenting works right (at least, I really hope so)
            gameObject.transform.localPosition = __instance.transform.localPosition;
            gameObject.transform.localScale = __instance.transform.localScale;
            gameObject.transform.localEulerAngles = __instance.transform.localEulerAngles;

            GameObject.Destroy(gameObject.GetComponent<Collider>());

            // Transfer all of the fields of the new selectable card to this guy
            SelectableCard newSelectableCard = gameObject.GetComponent<SelectableCard>();
            newSelectableCard.SetInfo(info);
            foreach (var fieldType in typeof(SelectableCard).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fieldType.FieldType == typeof(CardChoice))
                    continue;

                DefaultRenderersPlugin.Log.LogInfo($"Setting {fieldType} on new card");
                fieldType.SetValue(__instance, fieldType.GetValue(newSelectableCard));
            }
            foreach (var fieldType in typeof(Card).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                DefaultRenderersPlugin.Log.LogInfo($"Setting {fieldType} on new card");
                fieldType.SetValue(__instance, fieldType.GetValue(newSelectableCard));
            }

            GameObject.Destroy(newSelectableCard);

            // Now destroy all of the children game objects of this selectable card
            List<Transform> allMyChildren = new();
            foreach (Transform trans in __instance.gameObject.transform)
                allMyChildren.Add(trans);

            foreach (Transform child in allMyChildren)
                GameObject.Destroy(child.gameObject);

            // Delete the animator from this thing
            Animator anim = __instance.GetComponent<Animator>();
            if (anim != null)
                GameObject.Destroy(anim);

            CardAnimationController cardAnim = __instance.GetComponent<CardAnimationController>();
            if (cardAnim != null)
                GameObject.Destroy(cardAnim);

            // Make the new gameobject a child of this one
            gameObject.transform.SetParent(__instance.transform);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.localEulerAngles = Vector3.zero;

            SetInnerCard(__instance, gameObject);
            SetOuterCard(gameObject.GetComponent<CardAnimationController>(), __instance);

            if (info.GetRendererTemple() == CardTemple.Wizard && __instance.StatsLayer is PaperRenderStatsLayer prsl)
            {
                __instance.defaultCardback = MAGNIFICUS_CARD_BACK_TEXTURE;

                if (prsl.RigidRenderer != null && prsl.RigidRenderer.materials.Length > 1)
                    prsl.RigidRenderer.materials[1].SetTexture("_MainTex", MAGNIFICUS_CARD_BACK_TEXTURE);

                if (prsl.bendableCardRenderer != null && prsl.bendableCardRenderer.materials.Length > 1)
                    prsl.bendableCardRenderer.materials[1].SetTexture("_MainTex", MAGNIFICUS_CARD_BACK_TEXTURE);
            }
        }

        [HarmonyPatch(typeof(SelectableCard), nameof(SelectableCard.Initialize), typeof(CardInfo), typeof(Action<SelectableCard>), typeof(Action<SelectableCard>), typeof(bool), typeof(Action<SelectableCard>))]
        [HarmonyPrefix]
        [HarmonyPriority(HarmonyLib.Priority.VeryHigh)]
        private static bool ReplacingGutsIsTheOnlyScalableWayButItsAHugeFuggingPain(CardInfo info, SelectableCard __instance, Action<SelectableCard> cardSelected = null, Action<SelectableCard> cardFlipped = null, bool startFlipped = false, Action<SelectableCard> cardInspected = null)
        {
            if (Instance != null && info.GetRendererTemple() != ActiveTemple)
            {
                ReplaceGutsOfSelectableCard(__instance, info);//, cardSelected, cardFlipped, startFlipped, cardInspected);
                //return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Card), nameof(Card.SetInfo))]
        [HarmonyPrefix]
        private static void SwapOutGutsHereTooFFS(Card __instance, CardInfo info)
        {
            SelectableCard card = __instance as SelectableCard;
            if (card == null || info == null)
                return;

            CardTemple renderTemple = info.GetRendererTemple();

            // Only do this if the current card doesn't already match the temple
            if (renderTemple == CardTemple.Nature && card.Anim is PaperCardAnimationController)
                return;
            if (renderTemple == CardTemple.Tech && card.Anim is DiskCardAnimationController)
                return;
            if (renderTemple == CardTemple.Undead && card.Anim is GravestoneCardAnimationController)
                return;
            if (renderTemple == CardTemple.Wizard && card.Anim is PaperCardAnimationController)
                return;

            ReplaceGutsOfSelectableCard(card, info);
        }

        [HarmonyPatch(typeof(Card), nameof(Card.SetFaceDown))]
        [HarmonyPrefix]
        private static bool DifferentAnimationWhenChangingTempleOfCard(Card __instance, bool faceDown, bool immediate = false)
        {
            SelectableCard card = __instance as SelectableCard;
            if (card == null || card.Info == null)
                return true;

            CardTemple renderTemple = card.Info.GetRendererTemple();

            // Special case for wizard cards in the non wizard area
            // The flip animation changes the texture for some goddamn reason
            // So we have to wait for the animation to finish and then set the cardback to good ol' magnificus
            if (faceDown && renderTemple == CardTemple.Wizard && card.Anim is PaperCardAnimationController)
                CustomCoroutine.WaitThenExecute(0.084f, () => card.SetCardback(MAGNIFICUS_CARD_BACK_TEXTURE));

            if (!card.FaceDown || faceDown || Instance == null)
                return true;

            // Only do this if the current card doesn't already match the temple
            if (renderTemple == CardTemple.Nature && card.Anim is PaperCardAnimationController)
                return true;
            if (renderTemple == CardTemple.Tech && card.Anim is DiskCardAnimationController)
                return true;
            if (renderTemple == CardTemple.Undead && card.Anim is GravestoneCardAnimationController)
                return true;
            if (renderTemple == CardTemple.Wizard && card.Anim is PaperCardAnimationController)
                return true;

            // Okay, so instead of flipping it over, we're going to fly it out of frame,
            // swap it, and then fly it back into frame
            Vector3 currentPos = card.gameObject.transform.localPosition;
            Tween.LocalPosition(card.transform, currentPos + Vector3.forward * 3f, 0.25f, 0f, completeCallback: delegate ()
            {
                CardInfo info = card.Info;
                ReplaceGutsOfSelectableCard(card, card.Info);
                card.SetInfo(info);
                card.transform.localPosition = currentPos - Vector3.forward * 3f;
            });

            Tween.LocalPosition(card.transform, currentPos, 0.25f, 0.35f);
            return false;
        }

        [HarmonyPatch(typeof(GravestoneCardAnimationController), nameof(GravestoneCardAnimationController.PlayDeathAnimation))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        private static bool PreventDefectWithGravestoneCards(GravestoneCardAnimationController __instance, bool playSound = true)
        {
            if (Instance == null || ActiveTemple == CardTemple.Undead)
                return true;

            if (UnityEngine.Random.value < 0.33f)
            {
                __instance.PlayGlitchOutAnimation();
                return false;
            }
            Tween.ShaderColor(__instance.PlayableCard.StatsLayer.Material, "_FadeColor", Color.black, 0.35f, 0f, Tween.EaseIn, Tween.LoopType.None, null, delegate ()
            {
                __instance.Card.StatsLayer.Material = __instance.fadeMaterial;
                Tween.ShaderColor(__instance.Card.StatsLayer.Material, "_Color", new Color(0f, 0f, 0f, 0f), 0.35f, 0f, Tween.EaseLinear, Tween.LoopType.None, null, null, true);
            }, true);
            __instance.Anim.Play("die", 0, 0f);
            __instance.PlayDeathParticles();
            if (playSound)
            {
                AudioController.Instance.PlaySound3D("card_death", MixerGroup.TableObjectsSFX, __instance.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.VerySmall), new AudioParams.Repetition(0.05f, ""), null, null, false);
            }
            return false;
        }

        [HarmonyPatch(typeof(SelectableCardArray), nameof(SelectableCardArray.GetCardYPos))]
        [HarmonyPostfix]
        private static void MakeCardArraysHigherWhenCardPoolIsMixed(ref float __result)
        {
            if (Instance != null)
                __result += 0.007f;
        }

        [HarmonyPatch(typeof(SelectableCardArray), nameof(SelectableCardArray.TweenInCard))]
        [HarmonyPrefix]
        private static bool TweenWithAppropriateRotation(SelectableCardArray __instance, Transform cardTransform, Vector3 cardPos, float zRot, bool tiltCard = false)
        {
            if (!tiltCard || Instance == null)
                return true;

            float newTilt = 2f;

            // From here we run all the original code with some modifications:
            cardTransform.localPosition = cardPos;
            cardTransform.eulerAngles = new Vector3(90f + newTilt, 90f, 90f);
            Vector3 position = cardTransform.position;
            Vector3 position2 = position + __instance.offscreenPositionOffset;
            cardTransform.position = position2;
            Tween.Position(cardTransform, position, 0.15f, 0f, Tween.EaseOut, Tween.LoopType.None, null, null, true);

            return false;
        }

        [HarmonyPatch(typeof(Part3CardCostRender), nameof(Part3CardCostRender.GetPiece))]
        [HarmonyPostfix]
        private static void FixTheApiAndGetPieceForPatchedCard(Card card, string key, ref GameObject __result)
        {
            if (Instance == null || ActiveTemple == CardTemple.Tech)
                return;

            if (__result != null)
                return;

            if (card.transform.childCount == 0)
                return;

            // If we are *not* in Part 3, then this won't work
            Transform firstChild = card.transform.GetChild(0);
            Transform foundTransform = firstChild.Find(key);
            __result = foundTransform?.gameObject;
        }
    }
}