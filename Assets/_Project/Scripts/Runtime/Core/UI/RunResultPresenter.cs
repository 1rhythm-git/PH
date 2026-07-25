using LootUp.Core.Game;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LootUp.Core.UI
{
    public readonly struct RunResultPresenterSettings
    {
        public RunResultPresenterSettings(
            Color overlayColor,
            Color gameOverTextColor,
            Color resultPanelColor,
            Color confirmButtonColor,
            Color confirmButtonTextColor,
            int gameOverFontSize,
            int resultFontSize)
        {
            OverlayColor = overlayColor;
            GameOverTextColor = gameOverTextColor;
            ResultPanelColor = resultPanelColor;
            ConfirmButtonColor = confirmButtonColor;
            ConfirmButtonTextColor = confirmButtonTextColor;
            GameOverFontSize = gameOverFontSize;
            ResultFontSize = resultFontSize;
        }

        public Color OverlayColor { get; }
        public Color GameOverTextColor { get; }
        public Color ResultPanelColor { get; }
        public Color ConfirmButtonColor { get; }
        public Color ConfirmButtonTextColor { get; }
        public int GameOverFontSize { get; }
        public int ResultFontSize { get; }
    }

    public sealed class RunResultPresenter
    {
        private const string GameOverOverlayName = "GameOverOverlay";
        private const string ResultPanelName = "RunResultPanel";
        private const string ConfirmButtonName = "ConfirmButton";

        private readonly RunResultPresenterSettings settings;
        private RectTransform gameOverOverlay;

        public RunResultPresenter(RunResultPresenterSettings settings)
        {
            this.settings = settings;
        }

        // (추가) 결과 화면 생성과 버튼 연결을 게임 종료 흐름에서 분리한다.
        public void Show(Canvas canvas, RunResultData resultData, UnityAction confirmAction)
        {
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
            RebuildGameOverOverlay(gameOverOverlay, resultData, confirmAction);
        }

        private static RectTransform FindExistingOverlay(Transform parent)
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
            overlayImage.color = settings.OverlayColor;
            overlayImage.raycastTarget = true;

            return overlayRect;
        }

        private void RebuildGameOverOverlay(
            RectTransform overlayRect,
            RunResultData resultData,
            UnityAction confirmAction)
        {
            Button overlayButton = overlayRect.GetComponent<Button>();
            if (overlayButton != null)
            {
                overlayButton.onClick.RemoveAllListeners();
                overlayButton.interactable = false;
            }

            ClearChildren(overlayRect);
            CreateGameOverText(overlayRect);
            CreateResultPanel(overlayRect, resultData, confirmAction);
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
            text.color = settings.GameOverTextColor;
            text.fontSize = Mathf.Max(1, settings.GameOverFontSize);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private void CreateResultPanel(
            RectTransform overlayRect,
            RunResultData resultData,
            UnityAction confirmAction)
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
            panelImage.color = settings.ResultPanelColor;
            panelImage.raycastTarget = true;

            Outline outline = panelObject.GetComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.28f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            CreateResultSummaryText(panelRect, resultData);
            CreateConfirmButton(panelRect, confirmAction);
        }

        private void CreateResultSummaryText(RectTransform panelRect, RunResultData resultData)
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
            text.text = BuildResultSummaryText(resultData);
            text.alignment = TextAnchor.UpperLeft;
            text.color = settings.GameOverTextColor;
            text.fontSize = Mathf.Max(1, settings.ResultFontSize);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.lineSpacing = 1.05f;
        }

        private void CreateConfirmButton(RectTransform panelRect, UnityAction confirmAction)
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
            buttonImage.color = settings.ConfirmButtonColor;
            buttonImage.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            if (confirmAction != null)
            {
                button.onClick.AddListener(confirmAction);
            }

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
            label.color = settings.ConfirmButtonTextColor;
            label.fontSize = 51;
            label.fontStyle = FontStyle.Bold;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private static string BuildResultSummaryText(RunResultData resultData)
        {
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

        private static void ClearChildren(RectTransform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (Application.isPlaying)
                {
                    Object.Destroy(child.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }
    }
}
