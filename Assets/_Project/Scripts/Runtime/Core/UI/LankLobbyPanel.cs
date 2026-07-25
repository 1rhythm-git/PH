using LootUp.Core.Authentication;
using LootUp.Core.Characters;
using LootUp.Core.Leaderboard;
using UnityEngine;
using UnityEngine.UI;

namespace LootUp.Core.UI
{
    public sealed class LankLobbyPanel : MonoBehaviour
    {
        private enum LankMode
        {
            Floor,
            Score
        }

        private Font font;
        private Color accentColor;
        private Color textColor;
        private Color panelColor;
        private Button floorTab;
        private Button scoreTab;
        private RectTransform body;
        private Text scopeText;
        private LankMode mode;
        private bool isRefreshing;
        private CharacterDefinition[] availableCharacters;
        private CharacterDefinition bestCharacterDefinition;
        private Sprite generatedCharacterPortraitSprite;

        public static void Show(
            RectTransform parent,
            Font font,
            Color accentColor,
            Color textColor,
            Color panelColor,
            CharacterDefinition[] availableCharacters)
        {
            if (parent == null || parent.Find("LankLobbyPanel") != null)
            {
                return;
            }

            GameObject panelObject = new GameObject(
                "LankLobbyPanel",
                typeof(RectTransform),
                typeof(Image),
                typeof(LankLobbyPanel));
            panelObject.layer = parent.gameObject.layer;
            panelObject.transform.SetParent(parent, false);
            RectTransform panelRect =
                panelObject.GetComponent<RectTransform>();
            Stretch(panelRect, Vector2.zero, Vector2.one);

            Image background = panelObject.GetComponent<Image>();
            background.color = new Color(0.025f, 0.035f, 0.055f, 0.98f);

            LankLobbyPanel panel = panelObject.GetComponent<LankLobbyPanel>();
            panel.font = font;
            panel.accentColor = accentColor;
            panel.textColor = textColor;
            panel.panelColor = panelColor;
            panel.availableCharacters = availableCharacters;
            panel.Build();
        }

        private void OnDestroy()
        {
            DestroyGeneratedCharacterPortraitSprite();
        }

        private void Build()
        {
            RectTransform root = transform as RectTransform;
            Text title = CreateText(
                root,
                "Title",
                "LANK",
                new Vector2(0.055f, 0.915f),
                new Vector2(0.72f, 0.985f),
                42,
                TextAnchor.MiddleLeft,
                accentColor);
            title.fontStyle = FontStyle.Bold;
            scopeText = CreateText(
                root,
                "Scope",
                GetScopeText(),
                new Vector2(0.68f, 0.925f),
                new Vector2(0.85f, 0.975f),
                22,
                TextAnchor.MiddleRight,
                new Color(textColor.r, textColor.g, textColor.b, 0.72f));

            Button closeButton = CreateButton(
                root,
                "CloseButton",
                "X",
                new Vector2(0.875f, 0.925f),
                new Vector2(0.945f, 0.98f),
                new Color(0.34f, 0.12f, 0.14f, 1f));
            closeButton.onClick.AddListener(() => Destroy(gameObject));

            floorTab = CreateButton(
                root,
                "FloorTab",
                "FLOOR",
                new Vector2(0.055f, 0.84f),
                new Vector2(0.49f, 0.905f),
                accentColor);
            scoreTab = CreateButton(
                root,
                "ScoreTab",
                "SCORE",
                new Vector2(0.51f, 0.84f),
                new Vector2(0.945f, 0.905f),
                InactiveTabColor());
            floorTab.onClick.AddListener(() => SetMode(LankMode.Floor));
            scoreTab.onClick.AddListener(() => SetMode(LankMode.Score));

            body = CreateRect(
                root,
                "Body",
                new Vector2(0.055f, 0.055f),
                new Vector2(0.945f, 0.815f));
            SetMode(LankMode.Floor);
            _ = RefreshLeaderboardAsync();
        }

        private void SetMode(LankMode nextMode)
        {
            mode = nextMode;
            SetButtonColor(
                floorTab,
                mode == LankMode.Floor
                    ? accentColor
                    : InactiveTabColor());
            SetButtonColor(
                scoreTab,
                mode == LankMode.Score
                    ? accentColor
                    : InactiveTabColor());
            BuildLankView();
        }

        private void BuildLankView()
        {
            ClearBody();
            LeaderboardSnapshot snapshot = LeaderboardManager.Snapshot;
            LeaderboardRecord myRecord = snapshot.MyRecord;
            UpdateMyPortrait(myRecord);
            if (scopeText != null)
            {
                scopeText.text = GetScopeText();
            }

            RectTransform summary = CreatePanel(
                body,
                "MyLankSummary",
                new Vector2(0f, 0.78f),
                Vector2.one,
                panelColor);
            Text summaryTitle = CreateText(
                summary,
                "SummaryTitle",
                "MY LANK",
                new Vector2(0.04f, 0.65f),
                new Vector2(0.96f, 0.94f),
                24,
                TextAnchor.MiddleLeft,
                accentColor);
            summaryTitle.fontStyle = FontStyle.Bold;

            Image summaryPortrait = CreateImage(
                summary,
                "CharacterPortrait",
                new Vector2(0.04f, 0.08f),
                new Vector2(0.15f, 0.62f),
                Color.white);
            summaryPortrait.sprite = generatedCharacterPortraitSprite;
            summaryPortrait.preserveAspect = true;
            summaryPortrait.enabled =
                HasRecord(myRecord)
                && generatedCharacterPortraitSprite != null;

            CreateText(
                summary,
                "SummaryRank",
                HasRecord(myRecord) && myRecord.Rank > 0
                    ? $"# {myRecord.Rank}"
                    : "-",
                new Vector2(0.17f, 0.08f),
                new Vector2(0.3f, 0.62f),
                40,
                TextAnchor.MiddleLeft,
                textColor);
            CreateText(
                summary,
                "SummaryValue",
                $"{GetPrimaryValue(myRecord)}  |  {GetCharacterLevelText(myRecord)}",
                new Vector2(0.31f, 0.08f),
                new Vector2(0.96f, 0.62f),
                30,
                TextAnchor.MiddleRight,
                textColor);

            RectTransform table = CreatePanel(
                body,
                "LankTable",
                new Vector2(0f, 0f),
                new Vector2(1f, 0.75f),
                new Color(panelColor.r, panelColor.g, panelColor.b, 0.72f));
            CreateText(
                table,
                "RankHeader",
                "LANK",
                new Vector2(0.03f, 0.84f),
                new Vector2(0.13f, 0.98f),
                20,
                TextAnchor.MiddleLeft,
                accentColor);
            CreateText(
                table,
                "CharacterHeader",
                "CHAR",
                new Vector2(0.13f, 0.84f),
                new Vector2(0.24f, 0.98f),
                20,
                TextAnchor.MiddleCenter,
                accentColor);
            CreateText(
                table,
                "PlayerHeader",
                "PLAYER",
                new Vector2(0.25f, 0.84f),
                new Vector2(0.5f, 0.98f),
                20,
                TextAnchor.MiddleLeft,
                accentColor);
            CreateText(
                table,
                "FloorHeader",
                "FLOOR",
                new Vector2(0.51f, 0.84f),
                new Vector2(0.64f, 0.98f),
                20,
                TextAnchor.MiddleRight,
                accentColor);
            CreateText(
                table,
                "ScoreHeader",
                "SCORE",
                new Vector2(0.65f, 0.84f),
                new Vector2(0.84f, 0.98f),
                20,
                TextAnchor.MiddleRight,
                accentColor);
            CreateText(
                table,
                "LevelHeader",
                "LV.",
                new Vector2(0.85f, 0.84f),
                new Vector2(0.97f, 0.98f),
                20,
                TextAnchor.MiddleRight,
                accentColor);

            int visibleCount = Mathf.Min(5, snapshot.Records.Count);
            for (int i = 0; i < visibleCount; i++)
            {
                CreateRankRow(
                    table,
                    snapshot.Records[i],
                    i);
            }

            if (visibleCount <= 0)
            {
                string emptyMessage = snapshot.State switch
                {
                    LeaderboardLoadState.Loading => "LOADING...",
                    LeaderboardLoadState.Error =>
                        GetErrorMessage(snapshot.Message),
                    _ => "NO RECORDS"
                };
                CreateText(
                    table,
                    "EmptyState",
                    emptyMessage,
                    new Vector2(0.05f, 0.24f),
                    new Vector2(0.95f, 0.58f),
                    25,
                    TextAnchor.MiddleCenter,
                    new Color(
                        textColor.r,
                        textColor.g,
                        textColor.b,
                        0.62f));

                if (snapshot.State == LeaderboardLoadState.Error)
                {
                    Button retryButton = CreateButton(
                        table,
                        "RetryButton",
                        "RETRY",
                        new Vector2(0.34f, 0.08f),
                        new Vector2(0.66f, 0.22f),
                        accentColor);
                    retryButton.onClick.AddListener(
                        () => _ = RefreshLeaderboardAsync());
                }
            }
        }

        private void CreateRankRow(
            RectTransform table,
            LeaderboardRecord record,
            int index)
        {
            const float rowHeight = 0.155f;
            const float rowGap = 0.01f;
            float rowMax = 0.83f - index * (rowHeight + rowGap);
            float rowMin = rowMax - rowHeight;
            bool isMine = AuthenticationManager.IsAuthenticated
                          && string.Equals(
                              AuthenticationManager.CurrentSession.UserId,
                              record.UserId,
                              System.StringComparison.Ordinal);
            RectTransform row = CreatePanel(
                table,
                $"RankRow_{index}",
                new Vector2(0.025f, rowMin),
                new Vector2(0.975f, rowMax),
                isMine
                    ? new Color(0.08f, 0.22f, 0.2f, 0.95f)
                    : new Color(
                        panelColor.r,
                        panelColor.g,
                        panelColor.b,
                        0.84f));
            CreateText(
                row,
                "Rank",
                record.Rank > 0 ? $"#{record.Rank}" : "-",
                new Vector2(0.02f, 0f),
                new Vector2(0.11f, 1f),
                27,
                TextAnchor.MiddleLeft,
                isMine ? accentColor : textColor);

            CharacterDefinition definition =
                FindCharacterDefinition(record.CharacterId);
            Image portrait = CreateImage(
                row,
                "CharacterFullBodyPortrait",
                new Vector2(0.12f, 0.06f),
                new Vector2(0.23f, 0.94f),
                Color.white);
            portrait.sprite = definition != null
                ? definition.PortraitSprite
                : null;
            portrait.preserveAspect = true;
            portrait.enabled = portrait.sprite != null;

            CreateText(
                row,
                "Player",
                record.Nickname,
                new Vector2(0.25f, 0f),
                new Vector2(0.5f, 1f),
                25,
                TextAnchor.MiddleLeft,
                textColor);
            CreateText(
                row,
                "Floor",
                $"{record.HighestFloor}F",
                new Vector2(0.51f, 0f),
                new Vector2(0.64f, 1f),
                25,
                TextAnchor.MiddleRight,
                textColor);
            CreateText(
                row,
                "Score",
                record.Score.ToString("N0"),
                new Vector2(0.65f, 0f),
                new Vector2(0.84f, 1f),
                25,
                TextAnchor.MiddleRight,
                textColor);
            CreateText(
                row,
                "Level",
                record.CharacterLevel.ToString(),
                new Vector2(0.85f, 0f),
                new Vector2(0.97f, 1f),
                25,
                TextAnchor.MiddleRight,
                textColor);
        }

        private async System.Threading.Tasks.Task
            RefreshLeaderboardAsync()
        {
            if (isRefreshing)
            {
                return;
            }

            isRefreshing = true;
            try
            {
                await LeaderboardManager.RefreshAsync(20);
                if (this != null)
                {
                    BuildLankView();
                }
            }
            finally
            {
                isRefreshing = false;
            }
        }

        private string GetPrimaryValue(LeaderboardRecord record)
        {
            if (!HasRecord(record))
            {
                return "NO RECORD";
            }

            return mode == LankMode.Floor
                ? $"BEST FLOOR  {record.HighestFloor}F"
                : $"BEST SCORE  {record.Score:N0}";
        }

        private static bool HasRecord(LeaderboardRecord record)
        {
            return record != null && record.HasRecord;
        }

        private static string GetCharacterLevelText(
            LeaderboardRecord record)
        {
            return HasRecord(record)
                ? $"LV. {record.CharacterLevel}"
                : "LV. -";
        }

        private string GetScopeText()
        {
            return LeaderboardManager.Snapshot.State switch
            {
                LeaderboardLoadState.Online => "ONLINE",
                LeaderboardLoadState.Loading => "SYNC",
                LeaderboardLoadState.Error => "ERROR",
                _ => "LOCAL"
            };
        }

        private static string GetErrorMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "SERVER ERROR";
            }

            string normalized = message.Trim().ToUpperInvariant();
            return normalized.Length <= 72
                ? normalized
                : normalized.Substring(0, 72);
        }

        private void UpdateMyPortrait(LeaderboardRecord record)
        {
            DestroyGeneratedCharacterPortraitSprite();
            bestCharacterDefinition =
                FindCharacterDefinition(
                    record != null ? record.CharacterId : string.Empty);
            generatedCharacterPortraitSprite =
                CreateFacePortraitSprite(bestCharacterDefinition);
        }

        private CharacterDefinition FindCharacterDefinition(
            string characterId)
        {
            if (availableCharacters == null
                || string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            for (int i = 0; i < availableCharacters.Length; i++)
            {
                CharacterDefinition candidate = availableCharacters[i];
                if (candidate != null
                    && string.Equals(
                        candidate.CharacterId,
                        characterId,
                        System.StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static Sprite CreateFacePortraitSprite(
            CharacterDefinition definition)
        {
            Sprite source =
                definition != null ? definition.PortraitSprite : null;
            if (source == null)
            {
                return null;
            }

            Rect normalizedRect =
                AddPortraitTopPadding(
                    ClampNormalizedRect(definition.IngamePortraitFaceRect),
                    0.05f);
            Rect sourceRect = source.textureRect;
            Rect faceRect = new Rect(
                sourceRect.x + sourceRect.width * normalizedRect.x,
                sourceRect.y + sourceRect.height * normalizedRect.y,
                Mathf.Max(1f, sourceRect.width * normalizedRect.width),
                Mathf.Max(1f, sourceRect.height * normalizedRect.height));
            Sprite portrait = Sprite.Create(
                source.texture,
                PixelSnapRect(faceRect),
                new Vector2(0.5f, 0.5f),
                source.pixelsPerUnit,
                0,
                SpriteMeshType.FullRect,
                Vector4.zero,
                false);
            portrait.name = $"{source.name}_LankFace";
            portrait.hideFlags = HideFlags.HideAndDontSave;
            return portrait;
        }

        private void DestroyGeneratedCharacterPortraitSprite()
        {
            if (generatedCharacterPortraitSprite == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(generatedCharacterPortraitSprite);
            }
            else
            {
                DestroyImmediate(generatedCharacterPortraitSprite);
            }

            generatedCharacterPortraitSprite = null;
        }

        private static Rect ClampNormalizedRect(Rect rect)
        {
            float width = Mathf.Clamp(rect.width, 0.01f, 1f);
            float height = Mathf.Clamp(rect.height, 0.01f, 1f);
            float x = Mathf.Clamp(rect.x, 0f, 1f - width);
            float y = Mathf.Clamp(rect.y, 0f, 1f - height);
            return new Rect(x, y, width, height);
        }

        private static Rect AddPortraitTopPadding(
            Rect rect,
            float normalizedPadding)
        {
            float top = Mathf.Min(
                1f,
                rect.yMax + Mathf.Max(0f, normalizedPadding));
            return new Rect(rect.x, rect.y, rect.width, top - rect.y);
        }

        private static Rect PixelSnapRect(Rect rect)
        {
            return new Rect(
                Mathf.Round(rect.x),
                Mathf.Round(rect.y),
                Mathf.Max(1f, Mathf.Round(rect.width)),
                Mathf.Max(1f, Mathf.Round(rect.height)));
        }

        private Color InactiveTabColor()
        {
            return new Color(0.2f, 0.24f, 0.3f, 1f);
        }

        private static void SetButtonColor(Button button, Color color)
        {
            if (button != null && button.targetGraphic is Image image)
            {
                image.color = color;
            }
        }

        private void ClearBody()
        {
            if (body == null)
            {
                return;
            }

            for (int i = body.childCount - 1; i >= 0; i--)
            {
                Destroy(body.GetChild(i).gameObject);
            }
        }

        private RectTransform CreatePanel(
            RectTransform parent,
            string objectName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color)
        {
            GameObject panelObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Image),
                typeof(Outline));
            panelObject.layer = gameObject.layer;
            panelObject.transform.SetParent(parent, false);
            RectTransform rect = panelObject.GetComponent<RectTransform>();
            Stretch(rect, anchorMin, anchorMax);

            Image image = panelObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            Outline outline = panelObject.GetComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.14f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;
            return rect;
        }

        private Button CreateButton(
            RectTransform parent,
            string objectName,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color)
        {
            GameObject buttonObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(Outline));
            buttonObject.layer = gameObject.layer;
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            Stretch(rect, anchorMin, anchorMax);

            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation
            {
                mode = Navigation.Mode.None
            };
            Outline outline = buttonObject.GetComponent<Outline>();
            outline.effectColor = new Color(0.015f, 0.08f, 0.2f, 0.96f);
            outline.effectDistance = new Vector2(3f, -3f);
            outline.useGraphicAlpha = true;

            Text labelText = CreateText(
                rect,
                "Label",
                label,
                Vector2.zero,
                Vector2.one,
                25,
                TextAnchor.MiddleCenter,
                textColor);
            labelText.fontStyle = FontStyle.Bold;
            return button;
        }

        private Text CreateText(
            RectTransform parent,
            string objectName,
            string message,
            Vector2 anchorMin,
            Vector2 anchorMax,
            int fontSize,
            TextAnchor alignment,
            Color color)
        {
            GameObject textObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Text));
            textObject.layer = gameObject.layer;
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            Stretch(rect, anchorMin, anchorMax);

            Text text = textObject.GetComponent<Text>();
            text.text = message;
            text.font = font != null
                ? font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 13;
            text.resizeTextMaxSize = fontSize;
            return text;
        }

        private Image CreateImage(
            RectTransform parent,
            string objectName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color)
        {
            GameObject imageObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Image));
            imageObject.layer = gameObject.layer;
            imageObject.transform.SetParent(parent, false);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            Stretch(rect, anchorMin, anchorMax);

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static RectTransform CreateRect(
            RectTransform parent,
            string objectName,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject rectObject =
                new GameObject(objectName, typeof(RectTransform));
            rectObject.layer = parent.gameObject.layer;
            rectObject.transform.SetParent(parent, false);
            RectTransform rect = rectObject.GetComponent<RectTransform>();
            Stretch(rect, anchorMin, anchorMax);
            return rect;
        }

        private static void Stretch(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
