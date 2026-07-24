using System.Collections;
using System.Threading.Tasks;
using LootUp.Core.Authentication;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LootUp.Core.SceneFlow
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
        private RectTransform loginPanelRoot;
        private Text loginMessageText;
        private InputField accountIdInput;
        private InputField passwordInput;
        private Button guestButton;
        private Button accountLoginButton;
        private AsyncOperation lobbyLoadOperation;
        private bool isReadyForTouch;
        private bool isLoginPanelVisible;
        private bool isAuthenticationOperationActive;
        private bool sceneActivationRequested;
        private float blinkElapsed;

        private IEnumerator Start()
        {
            BuildTitleUI();
            EnsureEventSystem();

            Task<AuthenticationResult> authenticationTask = AuthenticationManager.InitializeAsync(false);

            lobbyLoadOperation = SceneManager.LoadSceneAsync(SceneFlowManager.LobbySceneName, LoadSceneMode.Single);
            if (lobbyLoadOperation != null)
            {
                lobbyLoadOperation.allowSceneActivation = false;
            }

            float elapsed = 0f;
            while (!IsLobbyReady() || elapsed < minimumLoadingDuration || !authenticationTask.IsCompleted)
            {
                elapsed += Time.unscaledDeltaTime;
                float loadProgress = lobbyLoadOperation != null
                    ? Mathf.Clamp01(lobbyLoadOperation.progress / 0.9f)
                    : 1f;
                float minimumDurationProgress = minimumLoadingDuration > 0f
                    ? Mathf.Clamp01(elapsed / minimumLoadingDuration)
                    : 1f;
                SetLoadingProgress(Mathf.Min(loadProgress, minimumDurationProgress));

                if (IsLobbyReady() && elapsed >= minimumLoadingDuration && !authenticationTask.IsCompleted)
                {
                    statusText.text = "SIGNING IN";
                }

                yield return null;
            }

            SetLoadingProgress(1f);
            ApplyInitialAuthenticationResult(authenticationTask.Result);
        }

        private void Update()
        {
            if (isLoginPanelVisible || !isReadyForTouch || sceneActivationRequested)
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

        private void OnGuestButtonPressed()
        {
            if (isAuthenticationOperationActive)
            {
                return;
            }

            if (AuthenticationManager.IsAuthenticated)
            {
                ShowReadyForTouch("TOUCH");
                return;
            }

            StartCoroutine(RunAuthenticationOperation(
                AuthenticationManager.SignInAsGuestAsync(),
                "CREATING GUEST SESSION"));
        }

        private void OnAccountLoginButtonPressed()
        {
            if (isAuthenticationOperationActive)
            {
                return;
            }

            string accountId = accountIdInput != null ? accountIdInput.text.Trim() : string.Empty;
            string password = passwordInput != null ? passwordInput.text : string.Empty;
            if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(password))
            {
                SetLoginMessage("ENTER ID AND PASSWORD");
                return;
            }

            StartCoroutine(RunAuthenticationOperation(
                AuthenticationManager.SignInAsync(accountId, password),
                "SIGNING IN"));
        }

        private IEnumerator RunAuthenticationOperation(Task<AuthenticationResult> authenticationTask, string progressMessage)
        {
            isAuthenticationOperationActive = true;
            isReadyForTouch = false;
            SetLoginControlsInteractable(false);
            SetLoginMessage(progressMessage);
            statusText.text = progressMessage;
            SetStatusTextAlpha(1f);

            while (!authenticationTask.IsCompleted)
            {
                yield return null;
            }

            isAuthenticationOperationActive = false;
            ApplyLoginAuthenticationResult(authenticationTask.Result);
        }

        private void ApplyInitialAuthenticationResult(AuthenticationResult result)
        {
            if (result.Succeeded)
            {
                ShowLoginPanel(result.Session.IsGuest ? "GUEST SESSION READY" : "ONLINE SESSION READY");
                return;
            }

            string message = result.Failure == AuthenticationFailure.NoSavedSession
                ? "SIGN IN TO CONTINUE"
                : GetAuthenticationFailureMessage(result);
            ShowLoginPanel(message);
        }

        private void ApplyLoginAuthenticationResult(AuthenticationResult result)
        {
            if (result.Succeeded)
            {
                ShowReadyForTouch("TOUCH");
                return;
            }

            ShowLoginPanel(GetAuthenticationFailureMessage(result));
        }

        private void ShowReadyForTouch(string message)
        {
            HideLoginPanel();
            isReadyForTouch = true;
            blinkElapsed = 0f;
            statusText.text = message;
            SetStatusTextAlpha(1f);
        }

        private void ShowLoginPanel(string message)
        {
            isReadyForTouch = false;
            isLoginPanelVisible = true;
            if (loginPanelRoot != null)
            {
                loginPanelRoot.gameObject.SetActive(true);
            }

            SetLoginControlsInteractable(true);
            SetLoginMessage(message);
            SetButtonLabel(guestButton, AuthenticationManager.IsAuthenticated ? "CONTINUE" : "GUEST");
            statusText.text = AuthenticationManager.IsAuthenticated ? "LOGIN READY" : "LOGIN REQUIRED";
            SetStatusTextAlpha(1f);
        }

        private void HideLoginPanel()
        {
            isLoginPanelVisible = false;
            isAuthenticationOperationActive = false;
            if (loginPanelRoot != null)
            {
                loginPanelRoot.gameObject.SetActive(false);
            }
        }

        private void SetLoginControlsInteractable(bool interactable)
        {
            if (guestButton != null)
            {
                guestButton.interactable = interactable;
            }

            if (accountLoginButton != null)
            {
                accountLoginButton.interactable = interactable;
            }

            if (accountIdInput != null)
            {
                accountIdInput.interactable = interactable;
            }

            if (passwordInput != null)
            {
                passwordInput.interactable = interactable;
            }
        }

        private void SetLoginMessage(string message)
        {
            if (loginMessageText != null)
            {
                loginMessageText.text = string.IsNullOrWhiteSpace(message) ? "SIGN IN TO CONTINUE" : message;
            }
        }

        private static string GetAuthenticationFailureMessage(AuthenticationResult result)
        {
            switch (result.Failure)
            {
                case AuthenticationFailure.InvalidCredentials:
                    return "INVALID ID OR PASSWORD";
                case AuthenticationFailure.NetworkUnavailable:
                    return "NETWORK UNAVAILABLE";
                case AuthenticationFailure.ProviderUnavailable:
                    return "SERVER LOGIN IS NOT READY";
                case AuthenticationFailure.OperationInProgress:
                    return "LOGIN IS ALREADY RUNNING";
                case AuthenticationFailure.NoSavedSession:
                    return "SIGN IN TO CONTINUE";
                default:
                    return string.IsNullOrWhiteSpace(result.Message) ? "LOGIN FAILED" : result.Message;
            }
        }

        private void SetStatusTextAlpha(float alpha)
        {
            if (statusText == null)
            {
                return;
            }

            Color textColor = statusText.color;
            textColor.a = Mathf.Clamp01(alpha);
            statusText.color = textColor;
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

            CreateLoginPanel(root);

            RectTransform loadingBarRoot = CreateImage(
                root,
                "LoadingBar",
                new Vector2(0.18f, 0.07f),
                new Vector2(0.82f, 0.095f),
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
                new Vector2(0.18f, 0.105f),
                new Vector2(0.82f, 0.16f),
                48);
        }

        private void SetLoadingProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (loadingBarFillRect != null)
            {
                loadingBarFillRect.anchorMax = new Vector2(progress, 1f);
            }

            if (statusText != null && !isReadyForTouch && !isLoginPanelVisible && !isAuthenticationOperationActive)
            {
                statusText.text = $"LOADING {Mathf.RoundToInt(progress * 100f)}%";
            }
        }

        private void CreateLoginPanel(RectTransform root)
        {
            Image panelImage = CreateImage(
                root,
                "LoginPanel",
                new Vector2(0.16f, 0.31f),
                new Vector2(0.84f, 0.56f),
                new Color(0.02f, 0.025f, 0.035f, 0.76f),
                null);
            panelImage.raycastTarget = true;
            loginPanelRoot = panelImage.rectTransform;

            CreateText(loginPanelRoot, "LoginTitle", "LOGIN", new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.96f), 42);
            loginMessageText = CreateText(loginPanelRoot, "LoginMessage", "SIGN IN TO CONTINUE", new Vector2(0.08f, 0.64f), new Vector2(0.92f, 0.78f), 24);
            accountIdInput = CreateInputField(loginPanelRoot, "AccountIdInput", "ID", new Vector2(0.08f, 0.47f), new Vector2(0.92f, 0.61f), false);
            passwordInput = CreateInputField(loginPanelRoot, "PasswordInput", "PASSWORD", new Vector2(0.08f, 0.31f), new Vector2(0.92f, 0.45f), true);
            accountLoginButton = CreateButton(loginPanelRoot, "AccountLoginButton", "LOGIN", new Vector2(0.08f, 0.14f), new Vector2(0.48f, 0.27f), OnAccountLoginButtonPressed);
            guestButton = CreateButton(loginPanelRoot, "GuestLoginButton", "GUEST", new Vector2(0.52f, 0.14f), new Vector2(0.92f, 0.27f), OnGuestButtonPressed);

            loginPanelRoot.gameObject.SetActive(false);
        }

        private static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            Text labelText = button.GetComponentInChildren<Text>();
            if (labelText != null)
            {
                labelText.text = label;
            }
        }

        private static InputField CreateInputField(
            RectTransform parent,
            string objectName,
            string placeholderText,
            Vector2 anchorMin,
            Vector2 anchorMax,
            bool isPassword)
        {
            GameObject inputObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField));
            inputObject.transform.SetParent(parent, false);

            RectTransform rect = inputObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = inputObject.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.14f);

            Text text = CreateText(rect, "Text", string.Empty, new Vector2(0.04f, 0f), new Vector2(0.96f, 1f), 28);
            text.alignment = TextAnchor.MiddleLeft;
            text.fontStyle = FontStyle.Normal;

            Text placeholder = CreateText(rect, "Placeholder", placeholderText, new Vector2(0.04f, 0f), new Vector2(0.96f, 1f), 28);
            placeholder.alignment = TextAnchor.MiddleLeft;
            placeholder.fontStyle = FontStyle.Normal;
            placeholder.color = new Color(1f, 1f, 1f, 0.45f);

            InputField inputField = inputObject.GetComponent<InputField>();
            inputField.textComponent = text;
            inputField.placeholder = placeholder;
            inputField.targetGraphic = image;
            inputField.contentType = isPassword ? InputField.ContentType.Password : InputField.ContentType.Standard;
            return inputField;
        }

        private static Button CreateButton(
            RectTransform parent,
            string objectName,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.96f, 0.82f, 0.22f, 0.95f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            Text buttonText = CreateText(rect, "Label", label, Vector2.zero, Vector2.one, 30);
            buttonText.color = new Color(0.05f, 0.06f, 0.08f, 1f);
            return button;
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

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
    }
}
