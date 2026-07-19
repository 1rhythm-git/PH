using PH.Core.Characters;
using PH.Core.Profile;
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

        private static readonly Vector2 HeaderBandMin = new Vector2(0f, 0.88f);
        private static readonly Vector2 HeaderBandMax = Vector2.one;
        private static readonly Vector2 ContentBandMin = new Vector2(0f, 0.12f);
        private static readonly Vector2 ContentBandMax = new Vector2(1f, 0.88f);
        private static readonly Vector2 FooterBandMin = Vector2.zero;
        private static readonly Vector2 FooterBandMax = new Vector2(1f, 0.12f);

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

        [SerializeField]
        private Color experienceColor = new Color(0.2f, 0.78f, 0.72f, 1f);

        private Font lobbyFont;
        private CharacterDefinition selectedCharacter;
        private Text profileText;
        private Text currencyText;
        private Text bestFloorText;
        private Text bestScoreText;
        private Text selectedCharacterLevelText;
        private Text selectedCharacterPositionText;
        private Text selectedCharacterNameText;
        private Text selectedCharacterSkillText;
        private Text selectedCharacterExperienceText;
        private Image selectedCharacterPortraitImage;
        private Image selectedCharacterExperienceFill;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private static Sprite leftArrowSprite;
        private static Sprite rightArrowSprite;

        private void Awake()
        {
            EnsureReferences();
            ApplySafeAreaLayout();
            EnsureSelectedCharacter();
            BuildLobby();
        }

        private void OnEnable()
        {
            CharacterProgressionState.ProgressChanged += HandleCharacterProgressChanged;
            UserProfileManager.ProfileChanged += HandleUserProfileChanged;
        }

        private void OnDisable()
        {
            CharacterProgressionState.ProgressChanged -= HandleCharacterProgressChanged;
            UserProfileManager.ProfileChanged -= HandleUserProfileChanged;
        }

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
            {
                ApplySafeAreaLayout();
            }
        }

        [ContextMenu("Debug/Rebuild Lobby")]
        private void DebugRebuildLobby()
        {
            EnsureReferences();
            ApplySafeAreaLayout();
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
            RefreshLobbyData();
        }

        private void BuildHeader()
        {
            RectTransform root = CreateRuntimeRoot(headerRoot);
            if (root == null)
            {
                return;
            }

            Text titleText = CreateText(root, "TitleText", "PHANTOM HEIST", new Vector2(0.055f, 0.43f), new Vector2(0.945f, 0.92f), TextAnchor.MiddleLeft, 58, primaryTextColor);
            titleText.fontStyle = FontStyle.Bold;
            profileText = CreateText(root, "ProfileText", string.Empty, new Vector2(0.06f, 0.08f), new Vector2(0.7f, 0.43f), TextAnchor.MiddleLeft, 29, secondaryTextColor);
            currencyText = CreateText(root, "CurrencyText", string.Empty, new Vector2(0.43f, 0.08f), new Vector2(0.7f, 0.43f), TextAnchor.MiddleRight, 23, secondaryTextColor);
            CreateText(root, "LoginStateText", "GUEST", new Vector2(0.7f, 0.08f), new Vector2(0.94f, 0.43f), TextAnchor.MiddleRight, 26, secondaryTextColor);
        }

        private void BuildContent()
        {
            RectTransform root = CreateRuntimeRoot(contentRoot);
            if (root == null)
            {
                return;
            }

            BuildRecordStrip(root);
            BuildCharacterStage(root);
            CreateButton(root, "StartButton", "START", new Vector2(0.1f, 0.025f), new Vector2(0.9f, 0.125f), startButtonColor, Color.black, StartGame, true);
        }

        private void BuildRecordStrip(RectTransform parent)
        {
            RectTransform panel = CreatePanel(parent, "RecordStrip", new Vector2(0.055f, 0.855f), new Vector2(0.945f, 0.985f), panelColor, true);
            Text title = CreateText(panel, "RecordTitleText", "BEST", new Vector2(0.045f, 0.18f), new Vector2(0.25f, 0.82f), TextAnchor.MiddleLeft, 27, secondaryTextColor);
            title.fontStyle = FontStyle.Bold;
            bestFloorText = CreateText(panel, "BestFloorText", string.Empty, new Vector2(0.25f, 0.16f), new Vector2(0.6f, 0.84f), TextAnchor.MiddleCenter, 31, primaryTextColor);
            bestScoreText = CreateText(panel, "BestScoreText", string.Empty, new Vector2(0.6f, 0.16f), new Vector2(0.955f, 0.84f), TextAnchor.MiddleRight, 31, primaryTextColor);
            CreateDivider(panel, "RecordDivider", new Vector2(0.595f, 0.23f), new Vector2(0.598f, 0.77f));
        }

        private void BuildCharacterStage(RectTransform parent)
        {
            RectTransform stage = CreatePanel(parent, "CharacterStage", new Vector2(0.055f, 0.15f), new Vector2(0.945f, 0.83f), panelColor, false);

            Text sectionTitle = CreateText(stage, "CharacterTitleText", "CHARACTER", new Vector2(0.055f, 0.9f), new Vector2(0.42f, 0.98f), TextAnchor.MiddleLeft, 27, secondaryTextColor);
            sectionTitle.fontStyle = FontStyle.Bold;
            selectedCharacterPositionText = CreateText(stage, "CharacterPositionText", string.Empty, new Vector2(0.58f, 0.9f), new Vector2(0.945f, 0.98f), TextAnchor.MiddleRight, 25, secondaryTextColor);

            selectedCharacterPortraitImage = CreateImage(stage, "SelectedCharacterPortrait", new Vector2(0.2f, 0.37f), new Vector2(0.8f, 0.9f), Color.white);
            selectedCharacterPortraitImage.preserveAspect = true;

            bool canNavigate = GetAvailableCharacterCount() > 1;
            CreateIconButton(stage, "PreviousCharacterButton", GetArrowSprite(false), new Vector2(0.045f, 0.53f), new Vector2(0.18f, 0.68f), SelectPreviousCharacter, canNavigate, "Previous Character");
            CreateIconButton(stage, "NextCharacterButton", GetArrowSprite(true), new Vector2(0.82f, 0.53f), new Vector2(0.955f, 0.68f), SelectNextCharacter, canNavigate, "Next Character");

            selectedCharacterNameText = CreateText(stage, "SelectedCharacterNameText", string.Empty, new Vector2(0.08f, 0.295f), new Vector2(0.7f, 0.37f), TextAnchor.MiddleLeft, 34, primaryTextColor);
            selectedCharacterNameText.fontStyle = FontStyle.Bold;
            selectedCharacterLevelText = CreateText(stage, "SelectedCharacterLevelText", string.Empty, new Vector2(0.7f, 0.295f), new Vector2(0.92f, 0.37f), TextAnchor.MiddleRight, 27, primaryTextColor);

            BuildExperienceGauge(stage);
            CreateDivider(stage, "InfoDivider", new Vector2(0.055f, 0.18f), new Vector2(0.945f, 0.184f));
            selectedCharacterSkillText = CreateText(stage, "SelectedCharacterSkillText", string.Empty, new Vector2(0.055f, 0.015f), new Vector2(0.945f, 0.17f), TextAnchor.MiddleLeft, 21, secondaryTextColor);
        }

        private void BuildExperienceGauge(RectTransform parent)
        {
            RectTransform background = CreatePanel(parent, "ExperienceGauge", new Vector2(0.055f, 0.235f), new Vector2(0.945f, 0.275f), new Color(0f, 0f, 0f, 0.5f), false);
            selectedCharacterExperienceFill = CreateImage(background, "Fill", new Vector2(0.008f, 0.16f), new Vector2(0.992f, 0.84f), experienceColor);
            selectedCharacterExperienceFill.type = Image.Type.Filled;
            selectedCharacterExperienceFill.fillMethod = Image.FillMethod.Horizontal;
            selectedCharacterExperienceFill.fillOrigin = 0;
            selectedCharacterExperienceText = CreateText(background, "ExperienceText", string.Empty, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 18, primaryTextColor);
            selectedCharacterExperienceText.fontStyle = FontStyle.Bold;
        }

        private void BuildFooter()
        {
            RectTransform root = CreateRuntimeRoot(footerRoot);
            if (root == null)
            {
                return;
            }

            RectTransform adArea = CreatePanel(root, "BannerAdArea", new Vector2(0.055f, 0.14f), new Vector2(0.945f, 0.86f), disabledButtonColor, false);
            CreateText(adArea, "AdLabel", "AD", Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 24, secondaryTextColor);
        }

        private void RefreshLobbyData()
        {
            ApplyUserProfileData();

            if (profileText != null)
            {
                profileText.text = playerNickname;
            }

            if (currencyText != null)
            {
                currencyText.text = $"G {UserProfileManager.GameMoney:N0}  R {UserProfileManager.Ruby:N0}";
            }

            if (bestFloorText != null)
            {
                bestFloorText.text = $"{Mathf.Max(0, bestHighestFloor)}F";
            }

            if (bestScoreText != null)
            {
                bestScoreText.text = Mathf.Max(0, bestScore).ToString("N0");
            }

            RefreshSelectedCharacterInfo();
        }

        private void ApplyUserProfileData()
        {
            string profileNickname = UserProfileManager.Nickname;
            if (!string.IsNullOrWhiteSpace(profileNickname))
            {
                playerNickname = profileNickname;
            }
        }

        private void RefreshSelectedCharacterInfo()
        {
            CharacterProgressionSnapshot progression = CharacterProgressionState.GetSnapshot(selectedCharacter);

            if (selectedCharacterLevelText != null)
            {
                selectedCharacterLevelText.text = selectedCharacter != null ? $"Lv. {progression.Level}" : "Lv. -";
            }

            if (selectedCharacterNameText != null)
            {
                selectedCharacterNameText.text = selectedCharacter != null ? selectedCharacter.DisplayName : "No Character";
            }

            if (selectedCharacterPositionText != null)
            {
                selectedCharacterPositionText.text = GetSelectedCharacterPositionLabel();
            }

            if (selectedCharacterSkillText != null)
            {
                selectedCharacterSkillText.text = GetSelectedCharacterSkillLabel();
            }

            if (selectedCharacterExperienceText != null)
            {
                selectedCharacterExperienceText.text = progression.IsMaxLevel
                    ? "MAX LEVEL"
                    : $"XP  {progression.CurrentExperience:N0} / {progression.RequiredExperience:N0}";
            }

            if (selectedCharacterExperienceFill != null)
            {
                selectedCharacterExperienceFill.fillAmount = progression.NormalizedExperience;
            }

            RefreshSelectedCharacterPortrait();
        }

        private string GetSelectedCharacterPositionLabel()
        {
            int total = GetAvailableCharacterCount();
            if (selectedCharacter == null || total <= 0)
            {
                return "0 / 0";
            }

            int visibleIndex = 0;
            for (int i = 0; i < availableCharacters.Length; i++)
            {
                if (availableCharacters[i] == null)
                {
                    continue;
                }

                visibleIndex++;
                if (availableCharacters[i] == selectedCharacter)
                {
                    return $"{visibleIndex} / {total}";
                }
            }

            return $"1 / {total}";
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

            selectedCharacter = CharacterSelectionState.Resolve(GetFirstAvailableCharacter());
            if (selectedCharacter != null)
            {
                CharacterSelectionState.Select(selectedCharacter);
            }
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

        private void HandleCharacterProgressChanged(string characterId)
        {
            if (selectedCharacter != null && selectedCharacter.CharacterId == characterId)
            {
                RefreshSelectedCharacterInfo();
            }
        }

        private void HandleUserProfileChanged()
        {
            RefreshLobbyData();
        }

        private void ApplySafeAreaLayout()
        {
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            Vector2 safeMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
            Vector2 safeMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);

            ApplyBand(headerRoot, safeMin, safeMax, HeaderBandMin, HeaderBandMax);
            ApplyBand(contentRoot, safeMin, safeMax, ContentBandMin, ContentBandMax);
            ApplyBand(footerRoot, safeMin, safeMax, FooterBandMin, FooterBandMax);

            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }

        private static void ApplyBand(RectTransform target, Vector2 safeMin, Vector2 safeMax, Vector2 bandMin, Vector2 bandMax)
        {
            if (target == null)
            {
                return;
            }

            Vector2 safeSize = safeMax - safeMin;
            target.anchorMin = safeMin + Vector2.Scale(safeSize, bandMin);
            target.anchorMax = safeMin + Vector2.Scale(safeSize, bandMax);
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
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

        private RectTransform CreatePanel(RectTransform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Color color, bool useOutline)
        {
            GameObject panelObject = useOutline
                ? new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Outline))
                : new GameObject(objectName, typeof(RectTransform), typeof(Image));
            panelObject.layer = parent.gameObject.layer;
            panelObject.transform.SetParent(parent, false);

            RectTransform rectTransform = ConfigureRect(panelObject.GetComponent<RectTransform>(), anchorMin, anchorMax);
            Image image = panelObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            if (useOutline)
            {
                Outline outline = panelObject.GetComponent<Outline>();
                outline.effectColor = new Color(1f, 1f, 1f, 0.18f);
                outline.effectDistance = new Vector2(2f, -2f);
                outline.useGraphicAlpha = true;
            }

            return rectTransform;
        }

        private Text CreateText(RectTransform parent, string objectName, string message, Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment, int fontSize, Color color)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            textObject.layer = parent.gameObject.layer;
            textObject.transform.SetParent(parent, false);
            ConfigureRect(textObject.GetComponent<RectTransform>(), anchorMin, anchorMax);

            Text text = textObject.GetComponent<Text>();
            text.text = message;
            text.alignment = alignment;
            text.color = color;
            text.font = lobbyFont;
            text.fontSize = Mathf.Max(1, fontSize);
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(12, fontSize / 2);
            text.resizeTextMaxSize = Mathf.Max(1, fontSize);
            return text;
        }

        private Image CreateImage(RectTransform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.layer = parent.gameObject.layer;
            imageObject.transform.SetParent(parent, false);
            ConfigureRect(imageObject.GetComponent<RectTransform>(), anchorMin, anchorMax);

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void CreateDivider(RectTransform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax)
        {
            CreateImage(parent, objectName, anchorMin, anchorMax, new Color(1f, 1f, 1f, 0.14f));
        }

        private Button CreateIconButton(RectTransform parent, string objectName, Sprite icon, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick, bool interactable, string accessibleName)
        {
            GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.layer = parent.gameObject.layer;
            buttonObject.transform.SetParent(parent, false);
            ConfigureRect(buttonObject.GetComponent<RectTransform>(), anchorMin, anchorMax);

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = icon;
            image.preserveAspect = true;
            image.color = interactable ? primaryTextColor : secondaryTextColor * 0.45f;

            Button button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = image;
            button.interactable = interactable;
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            // 별도 접근성 패키지 도입 전까지 오브젝트 이름에 버튼 목적을 유지한다.
            buttonObject.name = string.IsNullOrWhiteSpace(accessibleName) ? objectName : $"{objectName}_{accessibleName}";
            return button;
        }

        private Button CreateButton(RectTransform parent, string objectName, string label, Vector2 anchorMin, Vector2 anchorMax, Color backgroundColor, Color textColor, UnityEngine.Events.UnityAction onClick, bool interactable)
        {
            GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.layer = parent.gameObject.layer;
            buttonObject.transform.SetParent(parent, false);
            RectTransform rectTransform = ConfigureRect(buttonObject.GetComponent<RectTransform>(), anchorMin, anchorMax);

            Image image = buttonObject.GetComponent<Image>();
            image.color = backgroundColor;

            Button button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = image;
            button.interactable = interactable;
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            Text buttonText = CreateText(rectTransform, $"{objectName}Text", label, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter, 38, textColor);
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.verticalOverflow = VerticalWrapMode.Overflow;
            return button;
        }

        private static RectTransform ConfigureRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            return rectTransform;
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
            bestHighestFloor = Mathf.Max(0, bestHighestFloor);
            bestScore = Mathf.Max(0, bestScore);
        }
#endif
    }
}
