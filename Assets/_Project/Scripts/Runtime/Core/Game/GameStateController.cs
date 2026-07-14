using PH.Core.Items;
using PH.Core.Player;
using PH.Core.SceneFlow;
using PH.Core.UI;
using PH.Core.World;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PH.Core.Game
{
    public sealed class GameStateController : MonoBehaviour
    {
        private const string GameOverOverlayName = "GameOverOverlay";

        [SerializeField]
        private TopHUDController topHUDController;

        [SerializeField]
        private PlayerSpawner playerSpawner;

        [SerializeField]
        private ElevatorController elevatorController;

        [SerializeField]
        private ItemSpawner itemSpawner;

        [SerializeField]
        private Canvas targetCanvas;

        [SerializeField]
        private Color overlayColor = new Color(0f, 0f, 0f, 0.58f);

        [SerializeField]
        private Color gameOverTextColor = Color.white;

        [SerializeField]
        private int gameOverFontSize = 74;

        [SerializeField]
        private bool clickToLobby = true;

        [SerializeField]
        private bool logGameOver = true;

        private bool isGameOver;
        private bool exitRequested;
        private GameOverReason gameOverReason = GameOverReason.None;
        private RectTransform gameOverOverlay;

        public bool IsGameOver => isGameOver;
        public GameOverReason GameOverReason => gameOverReason;

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            EnsureReferences();

            if (topHUDController != null)
            {
                topHUDController.GameOverRequested += HandleGameOverRequested;
            }
        }

        private void OnDisable()
        {
            if (topHUDController != null)
            {
                topHUDController.GameOverRequested -= HandleGameOverRequested;
            }
        }

        public void RequestGameOver(GameOverReason reason)
        {
            if (isGameOver)
            {
                return;
            }

            isGameOver = true;
            gameOverReason = reason == GameOverReason.None ? GameOverReason.TimeOver : reason;

            StopInGameSystems();
            ShowGameOverOverlay();

            if (logGameOver)
            {
                Debug.Log($"Game Over: {gameOverReason}", this);
            }
        }

        [ContextMenu("Debug/Game Over - Time Over")]
        private void DebugTimeOver()
        {
            RequestGameOver(GameOverReason.TimeOver);
        }

        [ContextMenu("Debug/Game Over - Life Depleted")]
        private void DebugLifeDepleted()
        {
            RequestGameOver(GameOverReason.LifeDepleted);
        }

        private void HandleGameOverRequested(GameOverReason reason)
        {
            RequestGameOver(reason);
        }

        public void ExitInGame()
        {
            if (exitRequested)
            {
                return;
            }

            exitRequested = true;

            if (!clickToLobby)
            {
                return;
            }

            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadLobby();
                return;
            }

            SceneManager.LoadScene(SceneFlowManager.LobbySceneName, LoadSceneMode.Single);
        }

        private void StopInGameSystems()
        {
            if (topHUDController != null)
            {
                topHUDController.SetTimerPaused(true);
                topHUDController.SetItemStatus($"GAME OVER - {gameOverReason}");
            }

            PlayerController playerController = ResolvePlayerController();
            if (playerController != null)
            {
                playerController.SetControlEnabled(false);
            }

            PlayerMotor playerMotor = ResolvePlayerMotor();
            if (playerMotor != null)
            {
                playerMotor.SetMovementLocked(true);
            }

            if (elevatorController != null)
            {
                elevatorController.enabled = false;
            }

            if (itemSpawner != null)
            {
                itemSpawner.enabled = false;
            }
        }

        private void ShowGameOverOverlay()
        {
            Canvas canvas = ResolveTargetCanvas();
            if (canvas == null)
            {
                return;
            }

            gameOverOverlay = FindExistingOverlay(canvas.transform);
            if (gameOverOverlay == null)
            {
                gameOverOverlay = CreateGameOverOverlay(canvas.transform);
            }

            gameOverOverlay.gameObject.SetActive(true);
            gameOverOverlay.SetAsLastSibling();
        }

        private RectTransform FindExistingOverlay(Transform parent)
        {
            Transform existing = parent.Find(GameOverOverlayName);
            return existing as RectTransform;
        }

        private RectTransform CreateGameOverOverlay(Transform parent)
        {
            GameObject overlayObject = new GameObject(GameOverOverlayName, typeof(RectTransform), typeof(Image), typeof(Button));
            overlayObject.layer = parent.gameObject.layer;
            overlayObject.transform.SetParent(parent, false);

            RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            overlayRect.pivot = new Vector2(0.5f, 0.5f);

            Image overlayImage = overlayObject.GetComponent<Image>();
            overlayImage.color = overlayColor;
            overlayImage.raycastTarget = true;

            Button overlayButton = overlayObject.GetComponent<Button>();
            overlayButton.transition = Selectable.Transition.None;
            overlayButton.onClick.AddListener(ExitInGame);

            CreateGameOverText(overlayRect);

            return overlayRect;
        }

        private void CreateGameOverText(RectTransform overlayRect)
        {
            GameObject textObject = new GameObject("GameOverText", typeof(RectTransform), typeof(Text));
            textObject.layer = overlayRect.gameObject.layer;
            textObject.transform.SetParent(overlayRect, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.pivot = new Vector2(0.5f, 0.5f);

            Text text = textObject.GetComponent<Text>();
            text.text = "GAME OVER";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = gameOverTextColor;
            text.fontSize = Mathf.Max(1, gameOverFontSize);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private PlayerController ResolvePlayerController()
        {
            if (playerSpawner != null && playerSpawner.SpawnedPlayer != null)
            {
                return playerSpawner.SpawnedPlayer.GetComponent<PlayerController>();
            }

            return FindFirstObjectByType<PlayerController>();
        }

        private PlayerMotor ResolvePlayerMotor()
        {
            if (playerSpawner != null && playerSpawner.SpawnedPlayer != null)
            {
                return playerSpawner.SpawnedPlayer.GetComponent<PlayerMotor>();
            }

            return FindFirstObjectByType<PlayerMotor>();
        }

        private void EnsureReferences()
        {
            if (topHUDController == null)
            {
                topHUDController = FindFirstObjectByType<TopHUDController>();
            }

            if (playerSpawner == null)
            {
                playerSpawner = FindFirstObjectByType<PlayerSpawner>();
            }

            if (elevatorController == null)
            {
                elevatorController = FindFirstObjectByType<ElevatorController>();
            }

            if (itemSpawner == null)
            {
                itemSpawner = FindFirstObjectByType<ItemSpawner>();
            }

            if (targetCanvas == null)
            {
                targetCanvas = FindFirstObjectByType<Canvas>();
            }
        }

        private Canvas ResolveTargetCanvas()
        {
            if (targetCanvas == null)
            {
                targetCanvas = FindFirstObjectByType<Canvas>();
            }

            return targetCanvas;
        }
    }
}
