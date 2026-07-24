using System.Collections;
using System.Collections.Generic;
using LootUp.Core.SceneFlow;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LootUp.Core.UI
{
    public sealed class BannerAdCoordinator : MonoBehaviour
    {
        private const string RuntimeObjectName = "BannerAdCoordinator";
        private const string BannerAreaName = "BannerAdArea";
        private const string BannerLabelName = "AdLabel";
        private const string BottomUIName = "BottomUI";
        private const float BannerHeightRatio = 0.31f;

        private static readonly Color BannerBackgroundColor = new Color(0.18f, 0.2f, 0.24f, 0.92f);
        private static BannerAdCoordinator instance;

        private readonly List<RectTransform> registeredBannerAreas = new List<RectTransform>();
        private Font bannerFont;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateRuntimeInstance()
        {
            if (instance != null)
            {
                return;
            }

            GameObject coordinatorObject = new GameObject(RuntimeObjectName);
            instance = coordinatorObject.AddComponent<BannerAdCoordinator>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            bannerFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += HandleSceneLoaded;
            BannerAdState.AdsRemovedChanged += HandleAdsRemovedChanged;
        }

        private void Start()
        {
            ConfigureScene(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            if (instance != this)
            {
                return;
            }

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            BannerAdState.AdsRemovedChanged -= HandleAdsRemovedChanged;
            instance = null;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            ConfigureScene(scene);
            // 런타임 UI가 Start에서 늦게 생성되는 경우까지 한 프레임 뒤 다시 연결한다.
            StartCoroutine(ConfigureSceneAfterRuntimeUiBuild(scene));
        }

        private IEnumerator ConfigureSceneAfterRuntimeUiBuild(Scene scene)
        {
            yield return null;

            if (scene.isLoaded)
            {
                ConfigureScene(scene);
            }
        }

        private void HandleAdsRemovedChanged(bool adsRemoved)
        {
            ApplySharedBannerState();
        }

        private void ConfigureScene(Scene scene)
        {
            RemoveInvalidRegistrations(scene);
            RegisterExistingBannerAreas(scene);

            if (scene.name == SceneFlowManager.InGameSceneName)
            {
                EnsureInGameBannerArea(scene);
            }

            ApplySharedBannerState();
        }

        private void RemoveInvalidRegistrations(Scene scene)
        {
            for (int i = registeredBannerAreas.Count - 1; i >= 0; i--)
            {
                RectTransform bannerArea = registeredBannerAreas[i];
                if (bannerArea == null || bannerArea.gameObject.scene != scene)
                {
                    registeredBannerAreas.RemoveAt(i);
                }
            }
        }

        private void RegisterExistingBannerAreas(Scene scene)
        {
            RectTransform[] rectTransforms = FindObjectsByType<RectTransform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < rectTransforms.Length; i++)
            {
                RectTransform rectTransform = rectTransforms[i];
                if (rectTransform.gameObject.scene == scene && rectTransform.name == BannerAreaName)
                {
                    RegisterBannerArea(rectTransform);
                }
            }
        }

        private void EnsureInGameBannerArea(Scene scene)
        {
            RectTransform bottomUI = FindSceneRectTransform(scene, BottomUIName);
            if (bottomUI == null)
            {
                return;
            }

            RectTransform bannerArea = bottomUI.Find(BannerAreaName) as RectTransform;
            if (bannerArea == null)
            {
                bannerArea = CreateBannerArea(bottomUI);
            }

            RegisterBannerArea(bannerArea);
        }

        private RectTransform FindSceneRectTransform(Scene scene, string objectName)
        {
            RectTransform[] rectTransforms = FindObjectsByType<RectTransform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            for (int i = 0; i < rectTransforms.Length; i++)
            {
                RectTransform rectTransform = rectTransforms[i];
                if (rectTransform.gameObject.scene == scene && rectTransform.name == objectName)
                {
                    return rectTransform;
                }
            }

            return null;
        }

        private RectTransform CreateBannerArea(RectTransform parent)
        {
            GameObject bannerObject = new GameObject(BannerAreaName, typeof(RectTransform), typeof(Image));
            bannerObject.layer = parent.gameObject.layer;
            bannerObject.transform.SetParent(parent, false);

            RectTransform bannerArea = bannerObject.GetComponent<RectTransform>();
            bannerArea.anchorMin = Vector2.zero;
            bannerArea.anchorMax = new Vector2(1f, BannerHeightRatio);
            bannerArea.offsetMin = Vector2.zero;
            bannerArea.offsetMax = Vector2.zero;
            bannerArea.pivot = new Vector2(0.5f, 0.5f);

            Image background = bannerObject.GetComponent<Image>();
            background.color = BannerBackgroundColor;
            background.raycastTarget = false;

            GameObject labelObject = new GameObject(BannerLabelName, typeof(RectTransform), typeof(Text));
            labelObject.layer = parent.gameObject.layer;
            labelObject.transform.SetParent(bannerArea, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            labelRect.pivot = new Vector2(0.5f, 0.5f);

            Text label = labelObject.GetComponent<Text>();
            label.font = bannerFont;
            label.fontSize = 28;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 12;
            label.resizeTextMaxSize = 28;

            return bannerArea;
        }

        private void RegisterBannerArea(RectTransform bannerArea)
        {
            if (bannerArea != null && !registeredBannerAreas.Contains(bannerArea))
            {
                registeredBannerAreas.Add(bannerArea);
            }
        }

        private void ApplySharedBannerState()
        {
            for (int i = registeredBannerAreas.Count - 1; i >= 0; i--)
            {
                RectTransform bannerArea = registeredBannerAreas[i];
                if (bannerArea == null)
                {
                    registeredBannerAreas.RemoveAt(i);
                    continue;
                }

                Transform labelTransform = bannerArea.Find(BannerLabelName);
                Text label = labelTransform != null ? labelTransform.GetComponent<Text>() : null;
                if (label != null)
                {
                    label.text = BannerAdState.BannerLabel;
                }

                bannerArea.gameObject.SetActive(BannerAdState.IsBannerVisible);
            }
        }
    }
}
