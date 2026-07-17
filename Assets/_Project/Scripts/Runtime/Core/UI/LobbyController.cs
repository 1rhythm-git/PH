using PH.Core.Characters;
using PH.Core.SceneFlow;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PH.Core.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class LobbyController : MonoBehaviour
    {
        private const string RuntimeRootName = "LobbyRuntimeRoot";

        [SerializeField]
        private RectTransform headerRoot;

        [SerializeField]
        private RectTransform contentRoot;

        [SerializeField]
        private RectTransform footerRoot;

        [SerializeField]
        private CharacterDefinition[] availableCharacters;

        [SerializeField]
        private string playerNickname = "Player";

        [SerializeField]
        private int playerLevel = 1;

        [SerializeField]
        private int bestHighestFloor;

        [SerializeField]
        private int bestScore;

        [SerializeField]
        private Color primaryTextColor = Color.white;

        [SerializeField]
        private Color secondaryTextColor = new Color(1f, 1f, 1f, 0.72f);

        [SerializeField]
        private Color panelColor = new Color(0.05f, 0.07f, 0.1f, 0.78f);

        [SerializeField]
        private Color startButtonColor = new Color(0.96f, 0.82f, 0.22f, 1f);

        [SerializeField]
        private Color disabledButtonColor = new Color(0.18f, 0.2f, 0.24f, 0.92f);

        private Font lobbyFont;

        private CharacterDefinition selectedCharacter;

        private Text selectedCharacterText;

        private Text selectedCharacterNameText;

        private Text selectedCharacterSkillText;

        private Image selectedCharacterPortraitImage;

        private static Sprite leftArrowSprite;

        private static Sprite rightArrowSprite;

        private void Awake()
        {
            EnsureReferences();
            EnsureSelectedCharacter();
            BuildLobby();
        }

        private void OnEnable()
        {
            CharacterProgressionState.ProgressChanged += HandleCharacterProgressChanged;
        }

        private void OnDisable()
        {
            CharacterProgressionState.ProgressChanged -= HandleCharacterProgressChanged;
        }

        [ContextMenu("Debug/Rebuild Lobby")]
        private void DebugRebuildLobby()
        {
            EnsureReferences();
            BuildLobby();
        }

        [ContextMenu("Debug/Add 100 XP To Selected Character")]
        private void DebugAddSelectedCharacterExperience()
        {
            CharacterProgressionState.AddExperience(selectedCharacter, 100);
        }

        public void StartGame()
        {
            EnsureSelectedCharacter();
            CharacterSelectionState.Select(selectedCharacter);

            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadInGame();
                return;
            }

            SceneManager.LoadScene(SceneFlowManager.InGameSceneName, LoadSceneMode.Single);
        }

        private void BuildLobby()
        {
            lobbyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureSelectedCharacter();

            ClearRuntimeRoot(headerRoot);
            ClearRuntimeRoot(contentRoot);
            ClearRuntimeRoot(footerRoot);

            BuildHeader();
            BuildContent();
            BuildFooter();
        }

        private void BuildHeader()
        {
            RectTransform root = CreateRuntimeRoot(headerRoot);
            if (root == null)
            {
                return;
            }

            Text titleText = CreateText(root, "TitleText", "PHANTOM HEIST", new Vector2(0.06f, 0.34f), new Vector2(0.94f, 0.96f), TextAnchor.LowerLeft, 64, primaryTextColor);
            titleText.fontStyle = FontStyle.Bold;

            CreateText(root, "ProfileText", $"Lv. {Mathf.Max(1, playerLevel)}  {playerNickname}", new Vector2(0.06f, 0.08f), new Vector2(0.62f, 0.34f), TextAnchor.MiddleLeft, 32, secondaryTextColor);
            CreateText(root, "LoginStateText", "Guest", new Vector2(0.62f, 0.08f), new Vector2(0.94f, 0.34f), TextAnchor.MiddleRight, 30, secondaryTextColor);
        }

        private void BuildContent()
        {
            RectTransform root = CreateRuntimeRoot(contentRoot);
            if (root == null)
            {
                return;
            }

            RectTransform recordPanel = CreatePanel(root, "RecordPanel", new Vector2(0.08f, 0.75f), new Vector2(0.92f, 0.92f), panelColor);
            Text recordTitle = CreateText(recordPanel, "RecordTitleText", "BEST RECORD", new Vector2(0.06f, 0.16f), new Vector2(0.4f, 0.84f), TextAnchor.MiddleLeft, 30, primaryTextColor);
            recordTitle.fontStyle = FontStyle.Bold;
            CreateText(recordPanel, "BestFloorText", $"Floor  {Mathf.Max(0, bestHighestFloor)}F", new Vector2(0.4f, 0.16f), new Vector2(0.66f, 0.84f), TextAnchor.MiddleRight, 28, primaryTextColor);
            CreateText(recordPanel, "BestScoreText", $"Score  {Mathf.Max(0, bestScore)}", new Vector2(0.66f, 0.16f), new Vector2(0.94f, 0.84f), TextAnchor.MiddleRight, 28, primaryTextColor);

            RectTransform characterPanel = CreatePanel(root, "CharacterPanel", new Vector2(0.08f, 0.29f), new Vector2(0.92f, 0.73f), panelColor);
            Text characterTitle = CreateText(characterPanel, "CharacterTitleText", "CHARACTER", new Vector2(0.06f, 0.84f), new Vector2(0.46f, 0.96f), TextAnchor.MiddleLeft, 30, primaryTextColor);
            characterTitle.fontStyle = FontStyle.Bold;
            selectedCharacterText = CreateText(characterPanel, "SelectedCharacterLevelText", GetSelectedCharacterLevelLabel(), new Vector2(0.58f, 0.84f), new Vector2(0.94f, 0.96f), TextAnchor.MiddleRight, 26, secondaryTextColor);
            BuildCharacterPortrait(characterPanel);
            BuildCharacterNavigationButtons(characterPanel);
            selectedCharacterNameText = CreateText(characterPanel, "SelectedCharacterNameText", GetSelectedCharacterName(), new Vector2(0.24f, 0.19f), new Vector2(0.76f, 0.29f), TextAnchor.MiddleCenter, 27, primaryTextColor);
            selectedCharacterNameText.fontStyle = FontStyle.Bold;
            selectedCharacterSkillText = CreateText(characterPanel, "SelectedCharacterSkillText", GetSelectedCharacterSkillLabel(), new Vector2(0.06f, 0.02f), new Vector2(0.94f, 0.18f), TextAnchor.MiddleLeft, 18, secondaryTextColor);

            CreateButton(root, "StartButton", "START", new Vector2(0.12f, 0.19f), new Vector2(0.88f, 0.27f), startButtonColor, Color.black, StartGame, true);

            RectTransform menuPanel = CreatePanel(root, "MenuPanel", new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.18f), panelColor);
            CreateButton(menuPanel, "RankingButton", "RANKING", new Vector2(0.04f, 0.16f), new Vector2(0.27f, 0.84f), disabledButtonColor, secondaryTextColor, null, false);
            CreateButton(menuPanel, "ShopButton", "SHOP", new Vector2(0.29f, 0.16f), new Vector2(0.52f, 0.84f), disabledButtonColor, secondaryTextColor, null, false);
            CreateButton(menuPanel, "OptionsButton", "OPTIONS", new Vector2(0.54f, 0.16f), new Vector2(0.77f, 0.84f), disabledButtonColor, secondaryTextColor, null, false);
            CreateButton(menuPanel, "AdSlotButton", "AD", new Vector2(0.79f, 0.16f), new Vector2(0.96f, 0.84f), disabledButtonColor, secondaryTextColor, null, false);
        }

        private void BuildCharacterPortrait(RectTransform parent)
        {
            selectedCharacterPortraitImage = CreateImage(parent, "SelectedCharacterPortrait", new Vector2(0.28f, 0.29f), new Vector2(0.72f, 0.83f), Color.white);
            selectedCharacterPortraitImage.preserveAspect = true;
            RefreshSelectedCharacterPortrait();
        }

        private void BuildCharacterNavigationButtons(RectTransform parent)
        {
            bool canNavigate = GetAvailableCharacterCount() > 1;
            CreateIconButton(parent, "PreviousCharacterButton", GetArrowSprite(false), new Vector2(0.07f, 0.42f), new Vector2(0.22f, 0.65f), SelectPreviousCharacter, canNavigate, "Previous Character");
            CreateIconButton(parent, "NextCharacterButton", GetArrowSprite(true), new Vector2(0.78f, 0.42f), new Vector2(0.93f, 0.65f), SelectNextCharacter, canNavigate, "Next Character");
        }

        private void SelectCharacter(CharacterDefinition characterDefinition)
        {
            if (characterDefinition == null)
            {
                return;
            }

            selectedCharacter = characterDefinition;
            CharacterSelectionState.Select(selectedCharacter);

            RefreshSelectedCharacterInfo();

            Debug.Log($"Lobby character selected: {selectedCharacter.DisplayName} ({selectedCharacter.CharacterId})", this);
        }

        private void EnsureSelectedCharacter()
        {
            if (selectedCharacter != null)
            {
                CharacterSelectionState.Select(selectedCharacter);
                return;
            }

            CharacterDefinition fallbackCharacter = GetFirstAvailableCharacter();
            selectedCharacter = CharacterSelectionState.Resolve(fallbackCharacter);

            if (selectedCharacter == null)
            {
                return;
            }

            CharacterSelectionState.Select(selectedCharacter);
        }

        private CharacterDefinition GetFirstAvailableCharacter()
        {
            if (availableCharacters == null)
            {
                return null;
            }

            for (int i = 0; i < availableCharacters.Length; i++)
            {
                if (availableCharacters[i] != null)
                {
                    return availableCharacters[i];
                }
            }

            return null;
        }

        private string GetSelectedCharacterLevelLabel()
        {
            if (selectedCharacter == null)
            {
                return "Lv. -";
            }

            CharacterProgressionSnapshot progression = CharacterProgressionState.GetSnapshot(selectedCharacter);
            return $"Lv. {progression.Level}";
        }

        private string GetSelectedCharacterName()
        {
            return selectedCharacter != null ? selectedCharacter.DisplayName : "No Character";
        }

        private string GetSelectedCharacterSkillLabel()
        {
            if (selectedCharacter == null)
            {
                return "SKILL  None";
            }

            bool isUnlocked = CharacterProgressionState.IsSkillUnlocked(selectedCharacter);
            string status = isUnlocked ? "ACTIVE" : $"LOCKED Lv.{selectedCharacter.SkillUnlockLevel}";
            int chancePercent = Mathf.RoundToInt(selectedCharacter.SkillItemPageSpawnChance * 100f);
            string description = string.IsNullOrWhiteSpace(selectedCharacter.UnlockableSkillDescription)
                ? "No skill description"
                : selectedCharacter.UnlockableSkillDescription;
            return $"{status}  {selectedCharacter.UnlockableSkillName}\n{description}  Chance +{chancePercent}%";
        }

        private void SelectPreviousCharacter()
        {
            SelectAdjacentCharacter(-1);
        }

        private void SelectNextCharacter()
        {
            SelectAdjacentCharacter(1);
        }

        private void SelectAdjacentCharacter(int direction)
        {
            if (availableCharacters == null || availableCharacters.Length == 0 || direction == 0)
            {
                return;
            }

            int currentIndex = System.Array.IndexOf(availableCharacters, selectedCharacter);
            int startIndex = currentIndex >= 0 ? currentIndex : 0;

            for (int step = 1; step <= availableCharacters.Length; step++)
            {
                int index = (startIndex + direction * step) % availableCharacters.Length;
                if (index < 0)
                {
                    index += availableCharacters.Length;
                }

                CharacterDefinition candidate = availableCharacters[index];
                if (candidate != null && candidate != selectedCharacter)
                {
                    SelectCharacter(candidate);
                    return;
                }
            }
        }

        private int GetAvailableCharacterCount()
        {
            if (availableCharacters == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < availableCharacters.Length; i++)
            {
                if (availableCharacters[i] != null)
                {
                    count++;
                }
            }

            return count;
        }

        private void RefreshSelectedCharacterInfo()
        {
            if (selectedCharacterText != null)
            {
                selectedCharacterText.text = GetSelectedCharacterLevelLabel();
            }

            if (selectedCharacterNameText != null)
            {
                selectedCharacterNameText.text = GetSelectedCharacterName();
            }

            if (selectedCharacterSkillText != null)
            {
                selectedCharacterSkillText.text = GetSelectedCharacterSkillLabel();
            }

            RefreshSelectedCharacterPortrait();
        }

        private void RefreshSelectedCharacterPortrait()
        {
            if (selectedCharacterPortraitImage == null)
            {
                return;
            }

            Sprite portrait = selectedCharacter != null ? selectedCharacter.PortraitSprite : null;
            selectedCharacterPortraitImage.sprite = portrait;
            selectedCharacterPortraitImage.enabled = portrait != null;
        }

        private void HandleCharacterProgressChanged(string characterId)
        {
            if (selectedCharacter == null || selectedCharacter.CharacterId != characterId)
            {
                return;
            }

            BuildLobby();
        }

        private void BuildFooter()
        {
            RectTransform root = CreateRuntimeRoot(footerRoot);
            if (root == null)
            {
                return;
            }

            CreateText(root, "FooterText", "Banner Ad Area / BackND Status", new Vector2(0.06f, 0.18f), new Vector2(0.94f, 0.82f), TextAnchor.MiddleCenter, 28, secondaryTextColor);
        }

        private RectTransform CreateRuntimeRoot(RectTransform parent)
        {
            if (parent == null)
            {
                return null;
            }

            Transform existing = parent.Find(RuntimeRootName);
            RectTransform root = existing as RectTransform;

            if (root == null)
            {
                GameObject rootObject = new GameObject(RuntimeRootName, typeof(RectTransform));
                rootObject.layer = parent.gameObject.layer;
                rootObject.transform.SetParent(parent, false);
                root = rootObject.GetComponent<RectTransform>();
            }

            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            root.pivot = new Vector2(0.5f, 0.5f);
            return root;
        }

        private RectTransform CreatePanel(RectTransform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject panelObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Outline));
            panelObject.layer = parent.gameObject.layer;
            panelObject.transform.SetParent(parent, false);

            RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            Image image = panelObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            Outline outline = panelObject.GetComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.22f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            return rectTransform;
        }

        private Text CreateText(RectTransform parent, string objectName, string message, Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment, int fontSize, Color color)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            textObject.layer = parent.gameObject.layer;
            textObject.transform.SetParent(parent, false);

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            Text text = textObject.GetComponent<Text>();
            text.text = message;
            text.alignment = alignment;
            text.color = color;
            text.font = lobbyFont;
            text.fontSize = Mathf.Max(1, fontSize);
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;

            return text;
        }

        private Image CreateImage(RectTransform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.layer = parent.gameObject.layer;
            imageObject.transform.SetParent(parent, false);

            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Button CreateIconButton(RectTransform parent, string objectName, Sprite icon, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick, bool interactable, string accessibleName)
        {
            GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.layer = parent.gameObject.layer;
            buttonObject.transform.SetParent(parent, false);

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = icon;
            image.preserveAspect = true;
            image.color = interactable ? primaryTextColor : secondaryTextColor * 0.45f;
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = image;
            button.interactable = interactable;

            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            // (추가) 별도 접근성 패키지 도입 전까지 오브젝트 이름에 버튼 목적을 유지한다.
            buttonObject.name = string.IsNullOrWhiteSpace(accessibleName) ? objectName : $"{objectName}_{accessibleName}";
            return button;
        }

        private static Sprite GetArrowSprite(bool pointsRight)
        {
            Sprite cachedSprite = pointsRight ? rightArrowSprite : leftArrowSprite;
            if (cachedSprite != null)
            {
                return cachedSprite;
            }

            const int textureSize = 32;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                name = pointsRight ? "LobbyArrowRightTexture" : "LobbyArrowLeftTexture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color32[] pixels = new Color32[textureSize * textureSize];
            Color32 arrowColor = new Color32(255, 255, 255, 255);

            for (int y = 5; y <= 27; y++)
            {
                int headStart = 5 + Mathf.Abs(y - 16);
                for (int x = headStart; x <= 16; x++)
                {
                    SetArrowPixel(pixels, textureSize, x, y, pointsRight, arrowColor);
                }
            }

            for (int y = 12; y <= 20; y++)
            {
                for (int x = 14; x <= 27; x++)
                {
                    SetArrowPixel(pixels, textureSize, x, y, pointsRight, arrowColor);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
            sprite.name = pointsRight ? "LobbyArrowRight" : "LobbyArrowLeft";
            sprite.hideFlags = HideFlags.HideAndDontSave;

            if (pointsRight)
            {
                rightArrowSprite = sprite;
            }
            else
            {
                leftArrowSprite = sprite;
            }

            return sprite;
        }

        private static void SetArrowPixel(Color32[] pixels, int textureSize, int x, int y, bool mirrorHorizontally, Color32 color)
        {
            int targetX = mirrorHorizontally ? textureSize - 1 - x : x;
            pixels[y * textureSize + targetX] = color;
        }

        private Button CreateButton(RectTransform parent, string objectName, string label, Vector2 anchorMin, Vector2 anchorMax, Color backgroundColor, Color textColor, UnityEngine.Events.UnityAction onClick, bool interactable)
        {
            GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.layer = parent.gameObject.layer;
            buttonObject.transform.SetParent(parent, false);

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = backgroundColor;
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.interactable = interactable;

            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            Text buttonText = CreateText(rectTransform, $"{objectName}Text", label, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 34, textColor);
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.verticalOverflow = VerticalWrapMode.Overflow;

            return button;
        }

        private void ClearRuntimeRoot(RectTransform parent)
        {
            if (parent == null)
            {
                return;
            }

            Transform existing = parent.Find(RuntimeRootName);
            if (existing == null)
            {
                return;
            }

            for (int i = existing.childCount - 1; i >= 0; i--)
            {
                Transform child = existing.GetChild(i);
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

        private void EnsureReferences()
        {
            if (headerRoot == null)
            {
                headerRoot = transform.Find("HeaderUI") as RectTransform;
            }

            if (contentRoot == null)
            {
                contentRoot = transform.Find("ContentUI") as RectTransform;
            }

            if (footerRoot == null)
            {
                footerRoot = transform.Find("FooterUI") as RectTransform;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            playerLevel = Mathf.Max(1, playerLevel);
            bestHighestFloor = Mathf.Max(0, bestHighestFloor);
            bestScore = Mathf.Max(0, bestScore);
        }
#endif
    }
}
