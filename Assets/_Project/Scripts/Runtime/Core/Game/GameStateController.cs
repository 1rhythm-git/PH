using System.Collections.Generic;
using LootUp.Core.Characters;
using LootUp.Core.Items;
using LootUp.Core.Player;
using LootUp.Core.Profile;
using LootUp.Core.SceneFlow;
using LootUp.Core.UI;
using LootUp.Core.World;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LootUp.Core.Game
{
    public sealed class GameStateController : MonoBehaviour
    {
        private const string GameOverOverlayName = "GameOverOverlay";
        private const string ResultPanelName = "RunResultPanel";
        private const string ConfirmButtonName = "ConfirmButton";

        [SerializeField]
        private TopHUDController topHUDController;

        [SerializeField]
        private PlayerSpawner playerSpawner;

        [SerializeField]
        private ElevatorController elevatorController;

        [SerializeField]
        private ItemSpawner itemSpawner;

        [SerializeField]
        private InfiniteFloorManager floorManager;

        [SerializeField]
        private RunItemEventRecorder itemEventRecorder;

        [SerializeField]
        private Canvas targetCanvas;

        [SerializeField]
        private Color overlayColor = new Color(0f, 0f, 0f, 0.58f);

        [SerializeField]
        private Color gameOverTextColor = Color.white;

        [SerializeField]
        private Color resultPanelColor = new Color(0.08f, 0.1f, 0.13f, 0.94f);

        [SerializeField]
        private Color confirmButtonColor = new Color(0.96f, 0.82f, 0.22f, 1f);

        [SerializeField]
        private Color confirmButtonTextColor = Color.black;

        [SerializeField]
        private int gameOverFontSize = 111;

        [SerializeField]
        private int resultFontSize = 48;

        [SerializeField]
        private bool clickToLobby = true;

        [SerializeField]
        private bool logGameOver = true;

        [SerializeField]
        private RunRewardSettings rewardSettings = new RunRewardSettings();

        private bool isGameOver;
        private bool exitRequested;
        private bool rewardsSettled;
        private GameOverReason gameOverReason = GameOverReason.None;

        [SerializeField]
        private RunResultData lastRunResultData;

        private RectTransform gameOverOverlay;

        public bool IsGameOver => isGameOver;
        public GameOverReason GameOverReason => gameOverReason;
        public RunResultData LastRunResultData => lastRunResultData;

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
            lastRunResultData = CreateRunResultData(gameOverReason);

            StopInGameSystems();
            ShowGameOverOverlay();

            if (logGameOver)
            {
                Debug.Log($"Game Over: {gameOverReason}, highestFloor={lastRunResultData.HighestFloor}, score={lastRunResultData.Score}, items={lastRunResultData.AcquiredItemEvents.Count}", this);
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
            SettleRunRewards();

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

            PauseGameplayParticipants();
        }

        private void PauseGameplayParticipants()
        {
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IGameplayPausable pausable)
                {
                    pausable.SetGameplayPaused(true);
                }
            }
        }

        private RunResultData CreateRunResultData(GameOverReason reason)
        {
            EnsureReferences();

            int highestFloor = floorManager != null ? floorManager.RunHighestFloor : 1;
            int score = topHUDController != null ? topHUDController.CurrentScore : 0;
            float remainingSeconds = topHUDController != null ? topHUDController.RemainingSeconds : 0f;
            int remainingHearts = topHUDController != null ? topHUDController.CurrentHearts : 0;
            int acquiredGameMoney = topHUDController != null ? topHUDController.CurrentRunGameMoney : 0;
            IReadOnlyList<ItemRunEvent> acquiredItemEvents = itemEventRecorder != null ? itemEventRecorder.AcquiredItemEvents : null;

            CharacterDefinition characterDefinition = ResolveActiveCharacterDefinition();
            CharacterProgressionSnapshot progression = CharacterProgressionState.GetSnapshot(characterDefinition);
            int startFloor = floorManager != null ? floorManager.StartAbsoluteFloor : 1;
            RunRewardBreakdown rewards = RunRewardCalculator.Calculate(
                rewardSettings,
                characterDefinition,
                progression.Level,
                startFloor,
                highestFloor,
                score,
                remainingHearts,
                acquiredGameMoney);

            return new RunResultData(
                reason,
                highestFloor,
                rewards,
                characterDefinition != null ? characterDefinition.CharacterId : string.Empty,
                progression.Level,
                remainingSeconds,
                remainingHearts,
                acquiredItemEvents);
        }

        // (추가) 결과 확인은 여러 번 호출돼도 한 런의 보상을 한 번만 반영한다.
        private void SettleRunRewards()
        {
            if (rewardsSettled || lastRunResultData == null)
            {
                return;
            }

            rewardsSettled = true;

            if (lastRunResultData.TotalGameMoney > 0)
            {
                UserProfileManager.AddCurrency(UserCurrencyType.GameMoney, lastRunResultData.TotalGameMoney);
            }

            CharacterDefinition characterDefinition = ResolveActiveCharacterDefinition();
            if (characterDefinition != null && lastRunResultData.TotalExperience > 0)
            {
                CharacterProgressionState.AddExperience(characterDefinition, lastRunResultData.TotalExperience);
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
            RebuildGameOverOverlay(gameOverOverlay);
        }

        private RectTransform FindExistingOverlay(Transform parent)
        {
            Transform existing = parent.Find(GameOverOverlayName);
            return existing as RectTransform;
        }

        private RectTransform CreateGameOverOverlay(Transform parent)
        {
            GameObject overlayObject = new GameObject(GameOverOverlayName, typeof(RectTransform), typeof(Image));
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

            return overlayRect;
        }

        private void RebuildGameOverOverlay(RectTransform overlayRect)
        {
            Button overlayButton = overlayRect.GetComponent<Button>();
            if (overlayButton != null)
            {
                overlayButton.onClick.RemoveAllListeners();
                overlayButton.interactable = false;
            }

            ClearChildren(overlayRect);
            CreateGameOverText(overlayRect);
            CreateResultPanel(overlayRect);
        }

        private void CreateGameOverText(RectTransform overlayRect)
        {
            GameObject textObject = new GameObject("GameOverText", typeof(RectTransform), typeof(Text));
            textObject.layer = overlayRect.gameObject.layer;
            textObject.transform.SetParent(overlayRect, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.08f, 0.84f);
            textRect.anchorMax = new Vector2(0.92f, 0.97f);
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

        private void CreateResultPanel(RectTransform overlayRect)
        {
            GameObject panelObject = new GameObject(ResultPanelName, typeof(RectTransform), typeof(Image), typeof(Outline));
            panelObject.layer = overlayRect.gameObject.layer;
            panelObject.transform.SetParent(overlayRect, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.08f, 0.1f);
            panelRect.anchorMax = new Vector2(0.92f, 0.82f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelRect.pivot = new Vector2(0.5f, 0.5f);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = resultPanelColor;
            panelImage.raycastTarget = true;

            Outline outline = panelObject.GetComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.28f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            CreateResultSummaryText(panelRect);
            CreateConfirmButton(panelRect);
        }

        private void CreateResultSummaryText(RectTransform panelRect)
        {
            GameObject textObject = new GameObject("ResultSummaryText", typeof(RectTransform), typeof(Text));
            textObject.layer = panelRect.gameObject.layer;
            textObject.transform.SetParent(panelRect, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0.28f);
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(34f, 12f);
            textRect.offsetMax = new Vector2(-34f, -28f);
            textRect.pivot = new Vector2(0.5f, 0.5f);

            Text text = textObject.GetComponent<Text>();
            text.text = BuildResultSummaryText();
            text.alignment = TextAnchor.UpperLeft;
            text.color = gameOverTextColor;
            text.fontSize = Mathf.Max(1, resultFontSize);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.lineSpacing = 1.05f;
        }

        private void CreateConfirmButton(RectTransform panelRect)
        {
            GameObject buttonObject = new GameObject(ConfirmButtonName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.layer = panelRect.gameObject.layer;
            buttonObject.transform.SetParent(panelRect, false);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.22f, 0.06f);
            buttonRect.anchorMax = new Vector2(0.78f, 0.22f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            buttonRect.pivot = new Vector2(0.5f, 0.5f);

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = confirmButtonColor;
            buttonImage.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.onClick.AddListener(ExitInGame);

            GameObject labelObject = new GameObject("ConfirmButtonText", typeof(RectTransform), typeof(Text));
            labelObject.layer = buttonObject.layer;
            labelObject.transform.SetParent(buttonRect, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            labelRect.pivot = new Vector2(0.5f, 0.5f);

            Text label = labelObject.GetComponent<Text>();
            label.text = "CONFIRM";
            label.alignment = TextAnchor.MiddleCenter;
            label.color = confirmButtonTextColor;
            label.fontSize = 51;
            label.fontStyle = FontStyle.Bold;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private string BuildResultSummaryText()
        {
            RunResultData resultData = lastRunResultData;
            if (resultData == null)
            {
                return "RUN RESULT\nReason: Unknown\nHighest Floor: 1F\nAcquired Score: 0\nFloor Bonus Score: 0\nLife Bonus Score: 0\nTotal Score: 0\nLevel XP: 0\nFloor XP: 0\nBonus XP: 0\nTotal XP: 0\nMoney: 0\nItems: 0";
            }

            return $"RUN RESULT\nReason: {FormatGameOverReason(resultData.GameOverReason)}\nHighest Floor: {resultData.HighestFloor}F\nAcquired Score: {resultData.GameplayScore:N0}\nFloor Bonus Score: +{resultData.FloorScore:N0}\nLife Bonus Score: +{resultData.LifeScore:N0}\nArtifact Score: +{resultData.ArtifactBonusScore:N0}\nTotal Score: {resultData.Score:N0}\nLevel XP: +{resultData.LevelExperience:N0}\nFloor XP: +{resultData.FloorExperience:N0}\nScore XP: +{resultData.ScoreExperience:N0}\nArtifact XP: +{resultData.ArtifactBonusExperience:N0}\nTotal XP: +{resultData.TotalExperience:N0}\nMoney: +{resultData.TotalGameMoney:N0} ({resultData.AcquiredGameMoney:N0}+{resultData.BonusGameMoney:N0})\nItems: {resultData.AcquiredItemEvents.Count}";
        }

        private static string FormatGameOverReason(GameOverReason reason)
        {
            switch (reason)
            {
                case GameOverReason.TimeOver:
                    return "Time Over";
                case GameOverReason.LifeDepleted:
                    return "Life Depleted";
                default:
                    return "Unknown";
            }
        }

        private static string FormatRemainingTime(float remainingSeconds)
        {
            int seconds = Mathf.CeilToInt(Mathf.Max(0f, remainingSeconds));
            int minutes = seconds / 60;
            int remainSeconds = seconds % 60;
            return $"{minutes:00}:{remainSeconds:00}";
        }

        private static void ClearChildren(RectTransform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
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

        private CharacterDefinition ResolveActiveCharacterDefinition()
        {
            if (topHUDController != null && topHUDController.ActiveCharacterDefinition != null)
            {
                return topHUDController.ActiveCharacterDefinition;
            }

            PlayerController playerController = ResolvePlayerController();
            PlayerCharacterRuntime characterRuntime = playerController != null
                ? playerController.GetComponent<PlayerCharacterRuntime>()
                : FindFirstObjectByType<PlayerCharacterRuntime>();
            return characterRuntime != null ? characterRuntime.CharacterDefinition : null;
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

            if (floorManager == null)
            {
                floorManager = FindFirstObjectByType<InfiniteFloorManager>();
            }

            if (itemEventRecorder == null)
            {
                itemEventRecorder = FindFirstObjectByType<RunItemEventRecorder>();
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
