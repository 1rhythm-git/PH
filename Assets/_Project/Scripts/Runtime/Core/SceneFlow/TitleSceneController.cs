using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PH.Core.SceneFlow
{
    public sealed class TitleSceneController : MonoBehaviour
    {
        private const string RuntimeRootName = "TitleRuntimeRoot";

        [SerializeField]
        private Sprite backgroundSprite;

        [SerializeField, Min(0f)]
        private float minimumLoadingDuration = 1.5f;

        [SerializeField, Min(0.05f)]
        private float touchBlinkInterval = 0.45f;

        [SerializeField]
        private Color loadingBarBackgroundColor = new Color(0f, 0f, 0f, 0.72f);

        [SerializeField]
        private Color loadingBarFillColor = new Color(0.96f, 0.82f, 0.22f, 1f);

        private RectTransform loadingBarFillRect;
        private Text statusText;
        private AsyncOperation lobbyLoadOperation;
        private bool isReadyForTouch;
        private bool sceneActivationRequested;
        private float blinkElapsed;

        private IEnumerator Start()
        {
            BuildTitleUI();

            lobbyLoadOperation = SceneManager.LoadSceneAsync(SceneFlowManager.LobbySceneName, LoadSceneMode.Single);
            if (lobbyLoadOperation != null)
            {
                lobbyLoadOperation.allowSceneActivation = false;
            }

            float elapsed = 0f;
            while (!IsLobbyReady() || elapsed < minimumLoadingDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float loadProgress = lobbyLoadOperation != null
                    ? Mathf.Clamp01(lobbyLoadOperation.progress / 0.9f)
                    : 1f;
                float minimumDurationProgress = minimumLoadingDuration > 0f
                    ? Mathf.Clamp01(elapsed / minimumLoadingDuration)
                    : 1f;
                SetLoadingProgress(Mathf.Min(loadProgress, minimumDurationProgress));
                yield return null;
            }

            SetLoadingProgress(1f);
            isReadyForTouch = true;
            statusText.text = "TOUCH";
        }

        private void Update()
        {
            if (!isReadyForTouch || sceneActivationRequested)
            {
                return;
            }

            blinkElapsed += Time.unscaledDeltaTime;
            bool isBright = Mathf.FloorToInt(blinkElapsed / touchBlinkInterval) % 2 == 0;
            Color textColor = statusText.color;
            textColor.a = isBright ? 1f : 0.25f;
            statusText.color = textColor;

            if (!WasTouchPressed())
            {
                return;
            }

            sceneActivationRequested = true;
            if (lobbyLoadOperation != null)
            {
                lobbyLoadOperation.allowSceneActivation = true;
                return;
            }

            SceneManager.LoadScene(SceneFlowManager.LobbySceneName, LoadSceneMode.Single);
        }

        private bool IsLobbyReady()
        {
            return lobbyLoadOperation == null || lobbyLoadOperation.progress >= 0.9f;
        }

        private static bool WasTouchPressed()
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            return Keyboard.current != null &&
                (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame);
        }

        private void BuildTitleUI()
        {
            Transform existingRoot = transform.Find(RuntimeRootName);
            if (existingRoot != null)
            {
                Destroy(existingRoot.gameObject);
            }

            GameObject rootObject = new GameObject(RuntimeRootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            rootObject.transform.SetParent(transform, false);

            Canvas canvas = rootObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = rootObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2400f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform root = rootObject.GetComponent<RectTransform>();
            Image background = CreateImage(root, "Background", Vector2.zero, Vector2.one, Color.white, backgroundSprite);
            if (backgroundSprite != null)
            {
                AspectRatioFitter backgroundFitter = background.gameObject.AddComponent<AspectRatioFitter>();
                backgroundFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                backgroundFitter.aspectRatio = backgroundSprite.rect.width / backgroundSprite.rect.height;
            }

            RectTransform loadingBarRoot = CreateImage(
                root,
                "LoadingBar",
                new Vector2(0.18f, 0.225f),
                new Vector2(0.82f, 0.255f),
                loadingBarBackgroundColor,
                null).rectTransform;

            Image fill = CreateImage(loadingBarRoot, "Fill", Vector2.zero, new Vector2(0f, 1f), loadingBarFillColor, null);
            loadingBarFillRect = fill.rectTransform;
            loadingBarFillRect.pivot = new Vector2(0f, 0.5f);
            loadingBarFillRect.offsetMin = new Vector2(5f, 5f);
            loadingBarFillRect.offsetMax = new Vector2(-5f, -5f);

            statusText = CreateText(
                root,
                "StatusText",
                "LOADING 0%",
                new Vector2(0.18f, 0.16f),
                new Vector2(0.82f, 0.22f),
                48);
        }

        private void SetLoadingProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (loadingBarFillRect != null)
            {
                loadingBarFillRect.anchorMax = new Vector2(progress, 1f);
            }

            if (statusText != null && !isReadyForTouch)
            {
                statusText.text = $"LOADING {Mathf.RoundToInt(progress * 100f)}%";
            }
        }

        private static Image CreateImage(
            RectTransform parent,
            string objectName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color,
            Sprite sprite)
        {
            GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);

            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.sprite = sprite;
            image.preserveAspect = false;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(
            RectTransform parent,
            string objectName,
            string content,
            Vector2 anchorMin,
            Vector2 anchorMax,
            int fontSize)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Text text = textObject.GetComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }
    }
}
