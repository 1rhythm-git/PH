using System;
using System.Text;
using PH.Core.Game;
using PH.Core.World;
using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class TopHUDController : MonoBehaviour
    {
        private const string RuntimeRootName = "TopHUDRuntimeRoot";

        [SerializeField]
        private InfiniteFloorManager floorManager;

        [SerializeField]
        private Sprite characterPortrait;

        [SerializeField]
        private Color portraitFallbackColor = new Color(0.22f, 0.26f, 0.32f, 1f);

        [SerializeField]
        private string playerNickname = "Player";

        [SerializeField]
        private int playerLevel = 1;

        [SerializeField]
        private int maxHearts = 3;

        [SerializeField]
        private int currentHearts = 3;

        [SerializeField]
        private bool useTimer = true;

        [SerializeField]
        private float runDurationSeconds = 30f;

        [SerializeField]
        private int currentScore;

        [SerializeField]
        private int fullHeartScoreBonusPerHeart = 100;

        [SerializeField]
        private string itemStatusText = "ITEM -";

        [SerializeField]
        private Color primaryTextColor = Color.white;

        [SerializeField]
        private Color secondaryTextColor = new Color(1f, 1f, 1f, 0.74f);

        [SerializeField]
        private Color heartColor = new Color(1f, 0.18f, 0.24f, 1f);

        private RectTransform runtimeRoot;
        private Image portraitImage;
        private Text nicknameText;
        private Text levelText;
        private Text heartsText;
        private Text timerText;
        private Text floorText;
        private Text scoreText;
        private Text itemText;
        private Font hudFont;
        private float remainingSeconds;
        private bool timerPaused;
        private bool gameOverRequested;

        public event Action<GameOverReason> GameOverRequested;

        public int CurrentHearts => currentHearts;
        public int MaxHearts => maxHearts;
        public float RemainingSeconds => remainingSeconds;
        public int CurrentScore => currentScore;

        private void Awake()
        {
            EnsureReferences();
            remainingSeconds = Mathf.Max(0f, runDurationSeconds);
            BuildHUD();
            RefreshAll();
        }

        private void OnEnable()
        {
            if (floorManager != null)
            {
                floorManager.CurrentFloorChanged += HandleCurrentFloorChanged;
            }
        }

        private void Start()
        {
            EnsureReferences();
            RefreshAll();
        }

        private void Update()
        {
            if (!useTimer || timerPaused || remainingSeconds <= 0f)
            {
                return;
            }

            remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.deltaTime);
            RefreshTimer();

            if (remainingSeconds <= 0f)
            {
                RequestGameOver(GameOverReason.TimeOver);
            }
        }

        private void OnDisable()
        {
            if (floorManager != null)
            {
                floorManager.CurrentFloorChanged -= HandleCurrentFloorChanged;
            }
        }

        public void AddScore(int amount)
        {
            currentScore = Mathf.Max(0, currentScore + amount);
            RefreshScore();
        }

        public void AddTime(float seconds)
        {
            remainingSeconds = Mathf.Max(0f, remainingSeconds + seconds);
            RefreshTimer();
        }

        public void HealHeart(int amount)
        {
            currentHearts = Mathf.Min(maxHearts, currentHearts + Mathf.Max(0, amount));
            RefreshHearts();
        }

        public void DamageHeart(int amount)
        {
            currentHearts = Mathf.Max(0, currentHearts - Mathf.Max(0, amount));
            RefreshHearts();

            if (currentHearts <= 0)
            {
                RequestGameOver(GameOverReason.LifeDepleted);
            }
        }

        public int ApplyHealOrScoreBonus(int amount)
        {
            int healAmount = Mathf.Max(0, amount);
            if (healAmount <= 0)
            {
                return 0;
            }

            if (currentHearts >= maxHearts)
            {
                int scoreBonus = healAmount * Mathf.Max(0, fullHeartScoreBonusPerHeart);
                AddScore(scoreBonus);
                return scoreBonus;
            }

            HealHeart(healAmount);
            return 0;
        }

        public void SetItemStatus(string message)
        {
            itemStatusText = string.IsNullOrWhiteSpace(message) ? "ITEM -" : message;
            RefreshItemStatus();
        }

        public void SetTimerPaused(bool isPaused)
        {
            timerPaused = isPaused;
        }

        [ContextMenu("Debug/Add 100 Score")]
        private void DebugAddScore()
        {
            AddScore(100);
        }

        [ContextMenu("Debug/Damage Heart")]
        private void DebugDamageHeart()
        {
            DamageHeart(1);
        }

        [ContextMenu("Debug/Heal Heart")]
        private void DebugHealHeart()
        {
            currentHearts = Mathf.Min(maxHearts, currentHearts + 1);
            RefreshHearts();
        }

        [ContextMenu("Debug/Reset Timer")]
        private void DebugResetTimer()
        {
            remainingSeconds = Mathf.Max(0f, runDurationSeconds);
            timerPaused = false;
            gameOverRequested = false;
            RefreshTimer();
        }

        private void RequestGameOver(GameOverReason reason)
        {
            if (gameOverRequested)
            {
                return;
            }

            gameOverRequested = true;
            timerPaused = true;
            GameOverRequested?.Invoke(reason);
        }

        private void HandleCurrentFloorChanged(int currentAbsoluteFloor)
        {
            RefreshFloor(currentAbsoluteFloor);
        }

        private void BuildHUD()
        {
            hudFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            runtimeRoot = FindExistingRuntimeRoot();

            if (runtimeRoot == null)
            {
                GameObject rootObject = new GameObject(RuntimeRootName, typeof(RectTransform));
                rootObject.layer = gameObject.layer;
                rootObject.transform.SetParent(transform, false);
                runtimeRoot = rootObject.GetComponent<RectTransform>();
            }

            ClearRuntimeRootChildren();

            runtimeRoot.anchorMin = Vector2.zero;
            runtimeRoot.anchorMax = Vector2.one;
            runtimeRoot.offsetMin = Vector2.zero;
            runtimeRoot.offsetMax = Vector2.zero;
            runtimeRoot.pivot = new Vector2(0.5f, 0.5f);

            portraitImage = CreateImage("Portrait", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -104f), new Vector2(110f, -12f), portraitFallbackColor);
            AddOutline(portraitImage.gameObject, new Color(1f, 1f, 1f, 0.35f), new Vector2(2f, -2f));

            nicknameText = CreateText("NicknameText", new Vector2(0f, 1f), new Vector2(0.4f, 1f), new Vector2(124f, -58f), new Vector2(-8f, -12f), TextAnchor.UpperLeft, 43, primaryTextColor);
            levelText = CreateText("LevelText", new Vector2(0f, 1f), new Vector2(0.4f, 1f), new Vector2(124f, -104f), new Vector2(-8f, -58f), TextAnchor.UpperLeft, 36, secondaryTextColor);

            heartsText = CreateText("HeartsText", new Vector2(0.36f, 1f), new Vector2(0.68f, 1f), new Vector2(0f, -82f), new Vector2(0f, -4f), TextAnchor.MiddleCenter, 46, heartColor);
            floorText = CreateText("FloorText", new Vector2(0.4f, 1f), new Vector2(0.64f, 1f), new Vector2(0f, -128f), new Vector2(0f, -82f), TextAnchor.UpperCenter, 38, primaryTextColor);

            timerText = CreateText("TimerText", new Vector2(0.64f, 1f), new Vector2(1f, 1f), new Vector2(8f, -58f), new Vector2(-18f, -12f), TextAnchor.UpperRight, 43, primaryTextColor);
            scoreText = CreateText("ScoreText", new Vector2(0.64f, 1f), new Vector2(1f, 1f), new Vector2(8f, -104f), new Vector2(-18f, -60f), TextAnchor.UpperRight, 36, secondaryTextColor);
            itemText = CreateText("ItemStatusText", new Vector2(0.64f, 1f), new Vector2(1f, 1f), new Vector2(8f, -138f), new Vector2(-18f, -106f), TextAnchor.UpperRight, 29, secondaryTextColor);
            heartsText.transform.SetAsLastSibling();
        }

        private RectTransform FindExistingRuntimeRoot()
        {
            Transform existing = transform.Find(RuntimeRootName);
            return existing as RectTransform;
        }

        private void ClearRuntimeRootChildren()
        {
            for (int i = runtimeRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = runtimeRoot.GetChild(i);
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

        private Image CreateImage(string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.layer = gameObject.layer;
            imageObject.transform.SetParent(runtimeRoot, false);

            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            return image;
        }

        private Text CreateText(string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, TextAnchor alignment, int fontSize, Color color)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            textObject.layer = gameObject.layer;
            textObject.transform.SetParent(runtimeRoot, false);

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            Text text = textObject.GetComponent<Text>();
            text.alignment = alignment;
            text.font = hudFont;
            text.fontSize = fontSize;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return text;
        }

        private void AddOutline(GameObject target, Color color, Vector2 distance)
        {
            Outline outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        private void RefreshAll()
        {
            RefreshIdentity();
            RefreshPortrait();
            RefreshHearts();
            RefreshTimer();
            RefreshFloor(floorManager != null ? floorManager.CurrentAbsoluteFloor : 1);
            RefreshScore();
            RefreshItemStatus();
        }

        private void RefreshIdentity()
        {
            playerLevel = Mathf.Max(1, playerLevel);

            if (nicknameText != null)
            {
                nicknameText.text = playerNickname;
            }

            if (levelText != null)
            {
                levelText.text = $"Lv. {playerLevel}";
            }
        }

        private void RefreshPortrait()
        {
            if (portraitImage == null)
            {
                return;
            }

            portraitImage.sprite = characterPortrait;
            portraitImage.color = characterPortrait == null ? portraitFallbackColor : Color.white;
            portraitImage.preserveAspect = true;
        }

        private void RefreshHearts()
        {
            maxHearts = Mathf.Max(1, maxHearts);
            currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);

            if (heartsText == null)
            {
                return;
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < maxHearts; i++)
            {
                builder.Append(i < currentHearts ? '♥' : '♡');
                if (i < maxHearts - 1)
                {
                    builder.Append(' ');
                }
            }

            heartsText.text = builder.ToString();
        }

        private void RefreshTimer()
        {
            if (timerText == null)
            {
                return;
            }

            int seconds = Mathf.CeilToInt(remainingSeconds);
            int minutes = seconds / 60;
            int remainSeconds = seconds % 60;
            timerText.text = $"{minutes:00}:{remainSeconds:00}";
        }

        private void RefreshFloor(int currentAbsoluteFloor)
        {
            if (floorText != null)
            {
                floorText.text = $"{Mathf.Max(1, currentAbsoluteFloor)}F";
            }
        }

        private void RefreshScore()
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE {Mathf.Max(0, currentScore)}";
            }
        }

        private void RefreshItemStatus()
        {
            if (itemText != null)
            {
                itemText.text = itemStatusText;
            }
        }

        private void EnsureReferences()
        {
            if (floorManager == null)
            {
                floorManager = FindFirstObjectByType<InfiniteFloorManager>();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            playerLevel = Mathf.Max(1, playerLevel);
            maxHearts = Mathf.Max(1, maxHearts);
            currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);
            runDurationSeconds = Mathf.Max(0f, runDurationSeconds);
            currentScore = Mathf.Max(0, currentScore);
            fullHeartScoreBonusPerHeart = Mathf.Max(0, fullHeartScoreBonusPerHeart);
        }
#endif
    }
}
