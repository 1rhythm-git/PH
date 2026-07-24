using LootUp.Core.Items;
using UnityEngine;
using UnityEngine.UI;

namespace LootUp.Core.UI
{
    public sealed class ArtifactLobbyPanel : MonoBehaviour
    {
        private Font font;
        private RectTransform body;
        private Color accentColor;
        private Color textColor;
        private Color panelColor;
        private readonly Color buttonOutlineColor = new Color(0.015f, 0.08f, 0.2f, 0.96f);

        public static void Show(RectTransform parent, Font font, Color accentColor, Color textColor, Color panelColor)
        {
            if (parent == null || parent.Find("ArtifactLobbyPanel") != null)
            {
                return;
            }

            GameObject panelObject = new GameObject("ArtifactLobbyPanel", typeof(RectTransform), typeof(Image), typeof(ArtifactLobbyPanel));
            panelObject.layer = parent.gameObject.layer;
            panelObject.transform.SetParent(parent, false);
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            Stretch(panelRect, Vector2.zero, Vector2.one);

            Image background = panelObject.GetComponent<Image>();
            background.color = new Color(0.025f, 0.035f, 0.055f, 0.98f);

            ArtifactLobbyPanel panel = panelObject.GetComponent<ArtifactLobbyPanel>();
            panel.font = font;
            panel.accentColor = accentColor;
            panel.textColor = textColor;
            panel.panelColor = panelColor;
            panel.Build();
        }

        private void Build()
        {
            RectTransform root = transform as RectTransform;
            Text title = CreateText(root, "Title", "ARTIFACT ARCHIVE", new Vector2(0.055f, 0.915f), new Vector2(0.72f, 0.985f), 40, TextAnchor.MiddleLeft, accentColor);
            title.fontStyle = FontStyle.Bold;

            ArtifactCatalog catalog = ArtifactCatalog.Instance;
            Text progress = CreateText(
                root,
                "Progress",
                $"{catalog.GetOwnedArtifactCount()} / {catalog.Artifacts.Count}",
                new Vector2(0.7f, 0.915f),
                new Vector2(0.86f, 0.985f),
                28,
                TextAnchor.MiddleRight,
                textColor);
            progress.fontStyle = FontStyle.Bold;

            Button closeButton = CreateButton(root, "CloseButton", "X", new Vector2(0.875f, 0.925f), new Vector2(0.945f, 0.98f), new Color(0.34f, 0.12f, 0.14f, 1f));
            closeButton.onClick.AddListener(() => Destroy(gameObject));

            Button effectsTab = CreateButton(root, "EffectsTab", "EFFECTS", new Vector2(0.055f, 0.84f), new Vector2(0.49f, 0.905f), accentColor);
            Button collectionTab = CreateButton(root, "CollectionTab", "COLLECTION", new Vector2(0.51f, 0.84f), new Vector2(0.945f, 0.905f), new Color(0.2f, 0.24f, 0.3f, 1f));

            GameObject bodyObject = new GameObject("Body", typeof(RectTransform));
            bodyObject.layer = gameObject.layer;
            bodyObject.transform.SetParent(root, false);
            body = bodyObject.GetComponent<RectTransform>();
            Stretch(body, new Vector2(0.055f, 0.035f), new Vector2(0.945f, 0.825f));

            collectionTab.onClick.AddListener(() =>
            {
                BuildCollectionView();
                SetTabColor(collectionTab, accentColor);
                SetTabColor(effectsTab, new Color(0.2f, 0.24f, 0.3f, 1f));
            });
            effectsTab.onClick.AddListener(() =>
            {
                BuildEffectsView();
                SetTabColor(collectionTab, new Color(0.2f, 0.24f, 0.3f, 1f));
                SetTabColor(effectsTab, accentColor);
            });

            BuildEffectsView();
        }

        private void BuildCollectionView()
        {
            ClearBody();
            var artifacts = ArtifactCatalog.Instance.Artifacts;
            const int columns = 4;
            const int rows = 4;
            const float gap = 0.018f;
            float cellWidth = (1f - gap * (columns - 1)) / columns;
            float cellHeight = (1f - gap * (rows - 1)) / rows;

            for (int i = 0; i < artifacts.Count; i++)
            {
                int column = i % columns;
                int row = i / columns;
                float minX = column * (cellWidth + gap);
                float maxY = 1f - row * (cellHeight + gap);
                ArtifactDefinition artifact = artifacts[i];
                bool owned = ItemCollectionManager.GetOwnedAmount(artifact.ArtifactId) > 0;

                RectTransform cell = CreatePanel(
                    body,
                    $"Artifact_{artifact.ArtifactId}",
                    new Vector2(minX, maxY - cellHeight),
                    new Vector2(minX + cellWidth, maxY),
                    owned ? panelColor : new Color(0.08f, 0.09f, 0.11f, 0.94f));

                Image icon = CreateImage(cell, "Icon", new Vector2(0.16f, 0.28f), new Vector2(0.84f, 0.94f), Color.white);
                icon.sprite = Resources.Load<Sprite>(artifact.IconPath);
                icon.preserveAspect = true;
                icon.color = owned ? Color.white : new Color(0.12f, 0.13f, 0.15f, 0.72f);

                Text label = CreateText(
                    cell,
                    "Label",
                    artifact.DisplayName.ToUpperInvariant(),
                    new Vector2(0.04f, 0.03f),
                    new Vector2(0.96f, 0.22f),
                    20,
                    TextAnchor.MiddleCenter,
                    owned ? textColor : new Color(0.48f, 0.5f, 0.54f, 1f));
                label.fontStyle = FontStyle.Bold;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 12;
                label.resizeTextMaxSize = 20;

                Image status = CreateImage(
                    cell,
                    "Status",
                    new Vector2(0.78f, 0.79f),
                    new Vector2(0.9f, 0.91f),
                    owned ? new Color(0.18f, 0.92f, 0.7f, 1f) : new Color(0.32f, 0.34f, 0.38f, 1f));
            }
        }

        private void BuildEffectsView()
        {
            ClearBody();
            var effects = ArtifactCatalog.Instance.Effects;
            const float gap = 20f;
            float baseRowHeight = Mathf.Max(350f, body.rect.height * 0.48f);
            float contentHeight = Mathf.Max(0, effects.Count - 1) * gap;
            for (int i = 0; i < effects.Count; i++)
            {
                contentHeight += GetEffectRowHeight(effects[i], baseRowHeight);
            }

            ScrollRect scrollRect = CreateVerticalScrollView(body, out RectTransform content);
            content.sizeDelta = new Vector2(0f, contentHeight);
            float topOffset = 0f;

            for (int i = 0; i < effects.Count; i++)
            {
                ArtifactEffectDefinition effect = effects[i];
                float rowHeight = GetEffectRowHeight(effect, baseRowHeight);
                bool active = effect.IsActive;
                int ownedCount = effect.GetOwnedRequirementCount();
                RectTransform row = CreateFixedPanel(
                    content,
                    $"Effect_{effect.EffectId}",
                    topOffset,
                    rowHeight,
                    active ? new Color(0.08f, 0.22f, 0.2f, 0.95f) : new Color(0.08f, 0.09f, 0.11f, 0.94f));

                Text name = CreateText(row, "Name", effect.DisplayName, new Vector2(0.035f, 0.84f), new Vector2(0.63f, 0.97f), 29, TextAnchor.MiddleLeft, active ? accentColor : textColor);
                name.fontStyle = FontStyle.Bold;
                Text value = CreateText(row, "Value", FormatEffectValue(effect), new Vector2(0.64f, 0.84f), new Vector2(0.965f, 0.97f), 27, TextAnchor.MiddleRight, active ? accentColor : textColor);
                value.fontStyle = FontStyle.Bold;

                CreateText(
                    row,
                    "Description",
                    effect.Description,
                    new Vector2(0.035f, 0.7f),
                    new Vector2(0.965f, 0.84f),
                    21,
                    TextAnchor.MiddleLeft,
                    new Color(textColor.r, textColor.g, textColor.b, 0.92f));

                Text state = CreateText(
                    row,
                    "State",
                    active ? "ACTIVE" : "INACTIVE",
                    new Vector2(0.035f, 0.59f),
                    new Vector2(0.27f, 0.7f),
                    21,
                    TextAnchor.MiddleLeft,
                    active ? new Color(0.18f, 0.92f, 0.7f, 1f) : new Color(0.48f, 0.5f, 0.54f, 1f));
                state.fontStyle = FontStyle.Bold;
                CreateText(
                    row,
                    "Requirement",
                    $"REQUIRED  {ownedCount} / {effect.RequiredCount}",
                    new Vector2(0.29f, 0.59f),
                    new Vector2(0.965f, 0.7f),
                    21,
                    TextAnchor.MiddleRight,
                    new Color(textColor.r, textColor.g, textColor.b, 0.8f));

                BuildRequirementGrid(row, effect);
                topOffset += rowHeight + gap;
            }

            scrollRect.verticalNormalizedPosition = 1f;
        }

        private void BuildRequirementGrid(RectTransform parent, ArtifactEffectDefinition effect)
        {
            const int columns = 3;
            const float horizontalGap = 0.018f;
            const float verticalGap = 0.04f;
            int rows = Mathf.Max(1, Mathf.CeilToInt(effect.CandidateArtifactIds.Count / (float)columns));
            float cellWidth = (1f - horizontalGap * (columns - 1)) / columns;
            float cellHeight = (1f - verticalGap * (rows - 1)) / rows;
            RectTransform grid = CreateRect(parent, "RequiredArtifacts", new Vector2(0.035f, 0.055f), new Vector2(0.965f, 0.57f));

            for (int i = 0; i < effect.CandidateArtifactIds.Count; i++)
            {
                string artifactId = effect.CandidateArtifactIds[i];
                bool owned = ItemCollectionManager.GetOwnedAmount(artifactId) > 0;
                ArtifactCatalog.Instance.TryGetArtifact(artifactId, out ArtifactDefinition artifact);
                int column = i % columns;
                int row = i / columns;
                float minX = column * (cellWidth + horizontalGap);
                float maxY = 1f - row * (cellHeight + verticalGap);

                RectTransform requirement = CreatePanel(
                    grid,
                    $"Required_{artifactId}",
                    new Vector2(minX, maxY - cellHeight),
                    new Vector2(minX + cellWidth, maxY),
                    owned ? new Color(0.08f, 0.31f, 0.27f, 1f) : new Color(0.055f, 0.06f, 0.075f, 1f));

                Image icon = CreateImage(
                    requirement,
                    "Icon",
                    new Vector2(0.04f, 0.14f),
                    new Vector2(0.35f, 0.86f),
                    owned ? Color.white : new Color(0.22f, 0.23f, 0.26f, 0.78f));
                icon.sprite = artifact != null ? Resources.Load<Sprite>(artifact.IconPath) : null;
                icon.preserveAspect = true;

                Text artifactName = CreateText(
                    requirement,
                    "Name",
                    artifact != null ? artifact.DisplayName.ToUpperInvariant() : artifactId.ToUpperInvariant(),
                    new Vector2(0.39f, 0.14f),
                    new Vector2(0.93f, 0.86f),
                    19,
                    TextAnchor.MiddleLeft,
                    owned ? textColor : new Color(0.42f, 0.44f, 0.48f, 1f));
                artifactName.fontStyle = FontStyle.Bold;
                artifactName.resizeTextForBestFit = true;
                artifactName.resizeTextMinSize = 12;
                artifactName.resizeTextMaxSize = 19;

                CreateImage(
                    requirement,
                    "OwnedState",
                    new Vector2(0.9f, 0.72f),
                    new Vector2(0.97f, 0.88f),
                    owned ? new Color(0.18f, 0.92f, 0.7f, 1f) : new Color(0.3f, 0.32f, 0.35f, 1f));
            }
        }

        private static float GetEffectRowHeight(ArtifactEffectDefinition effect, float baseRowHeight)
        {
            const int columns = 3;
            int requirementCount = effect != null ? effect.CandidateArtifactIds.Count : 0;
            int rows = Mathf.Max(1, Mathf.CeilToInt(requirementCount / (float)columns));
            return baseRowHeight + Mathf.Max(0, rows - 2) * 110f;
        }

        private ScrollRect CreateVerticalScrollView(RectTransform parent, out RectTransform content)
        {
            GameObject scrollObject = new GameObject("EffectsScrollView", typeof(RectTransform), typeof(ScrollRect));
            scrollObject.layer = gameObject.layer;
            scrollObject.transform.SetParent(parent, false);
            RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
            Stretch(scrollRectTransform, Vector2.zero, Vector2.one);

            GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewportObject.layer = gameObject.layer;
            viewportObject.transform.SetParent(scrollRectTransform, false);
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            Stretch(viewport, Vector2.zero, new Vector2(0.974f, 1f));
            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);

            GameObject contentObject = new GameObject("Content", typeof(RectTransform));
            contentObject.layer = gameObject.layer;
            contentObject.transform.SetParent(viewport, false);
            content = contentObject.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            Scrollbar scrollbar = CreateVerticalScrollbar(scrollRectTransform);
            ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
            scrollRect.content = content;
            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.14f;
            scrollRect.scrollSensitivity = 58f;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scrollRect.verticalScrollbarSpacing = 6f;
            return scrollRect;
        }

        private Scrollbar CreateVerticalScrollbar(RectTransform parent)
        {
            GameObject scrollbarObject = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            scrollbarObject.layer = gameObject.layer;
            scrollbarObject.transform.SetParent(parent, false);
            RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
            Stretch(scrollbarRect, new Vector2(0.982f, 0f), Vector2.one);
            Image background = scrollbarObject.GetComponent<Image>();
            background.color = new Color(0.12f, 0.14f, 0.17f, 0.9f);

            GameObject slidingAreaObject = new GameObject("SlidingArea", typeof(RectTransform));
            slidingAreaObject.layer = gameObject.layer;
            slidingAreaObject.transform.SetParent(scrollbarRect, false);
            RectTransform slidingArea = slidingAreaObject.GetComponent<RectTransform>();
            Stretch(slidingArea, new Vector2(0.12f, 0.01f), new Vector2(0.88f, 0.99f));

            GameObject handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleObject.layer = gameObject.layer;
            handleObject.transform.SetParent(slidingArea, false);
            RectTransform handleRect = handleObject.GetComponent<RectTransform>();
            Stretch(handleRect, Vector2.zero, Vector2.one);
            Image handleImage = handleObject.GetComponent<Image>();
            handleImage.color = accentColor;

            Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();
            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handleRect;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            return scrollbar;
        }

        private static string FormatEffectValue(ArtifactEffectDefinition effect)
        {
            switch (effect.EffectType)
            {
                case ArtifactEffectType.ScoreItemDoubleChancePercent:
                case ArtifactEffectType.TimeItemDoubleChancePercent:
                    return $"{effect.ValuePercent:0.#}% DOUBLE";
                default:
                    return $"+{effect.ValuePercent:0.#}%";
            }
        }

        private void ClearBody()
        {
            for (int i = body.childCount - 1; i >= 0; i--)
            {
                GameObject child = body.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        private RectTransform CreateFixedPanel(RectTransform parent, string objectName, float topOffset, float height, Color color)
        {
            GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            panel.layer = gameObject.layer;
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -topOffset);
            rect.sizeDelta = new Vector2(0f, height);
            panel.GetComponent<Image>().color = color;
            return rect;
        }

        private RectTransform CreateRect(RectTransform parent, string objectName, Vector2 min, Vector2 max)
        {
            GameObject rectObject = new GameObject(objectName, typeof(RectTransform));
            rectObject.layer = gameObject.layer;
            rectObject.transform.SetParent(parent, false);
            RectTransform rect = rectObject.GetComponent<RectTransform>();
            Stretch(rect, min, max);
            return rect;
        }

        private RectTransform CreatePanel(RectTransform parent, string objectName, Vector2 min, Vector2 max, Color color)
        {
            GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            panel.layer = gameObject.layer;
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            Stretch(rect, min, max);
            panel.GetComponent<Image>().color = color;
            return rect;
        }

        private Image CreateImage(RectTransform parent, string objectName, Vector2 min, Vector2 max, Color color)
        {
            GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.layer = gameObject.layer;
            imageObject.transform.SetParent(parent, false);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            Stretch(rect, min, max);
            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Text CreateText(RectTransform parent, string objectName, string value, Vector2 min, Vector2 max, int size, TextAnchor alignment, Color color)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            textObject.layer = gameObject.layer;
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            Stretch(rect, min, max);
            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private Button CreateButton(RectTransform parent, string objectName, string label, Vector2 min, Vector2 max, Color color)
        {
            GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Shadow), typeof(Outline));
            buttonObject.layer = gameObject.layer;
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            Stretch(rect, min, max);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Shadow shadow = buttonObject.GetComponent<Shadow>();
            shadow.effectColor = new Color(buttonOutlineColor.r, buttonOutlineColor.g, buttonOutlineColor.b, 0.9f);
            shadow.effectDistance = new Vector2(0f, -7f);
            shadow.useGraphicAlpha = true;
            Outline outline = buttonObject.GetComponent<Outline>();
            outline.effectColor = buttonOutlineColor;
            outline.effectDistance = new Vector2(4f, -4f);
            outline.useGraphicAlpha = true;
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            Text text = CreateText(rect, "Label", label, Vector2.zero, Vector2.one, 24, TextAnchor.MiddleCenter, textColor);
            text.fontStyle = FontStyle.Bold;
            Outline textOutline = text.gameObject.AddComponent<Outline>();
            textOutline.effectColor = buttonOutlineColor;
            textOutline.effectDistance = new Vector2(2f, -2f);
            textOutline.useGraphicAlpha = true;
            return button;
        }

        private static void SetTabColor(Button button, Color color)
        {
            if (button != null && button.targetGraphic != null)
            {
                button.targetGraphic.color = color;
            }
        }

        private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
