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

        private void Awake()
        {
            EnsureReferences();
            EnsureSelectedCharacter();
            BuildLobby();
        }

        [ContextMenu("Debug/Rebuild Lobby")]
        private void DebugRebuildLobby()
        {
            EnsureReferences();
            BuildLobby();
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

            RectTransform recordPanel = CreatePanel(root, "RecordPanel", new Vector2(0.08f, 0.58f), new Vector2(0.92f, 0.92f), panelColor);
            Text recordTitle = CreateText(recordPanel, "RecordTitleText", "BEST RECORD", new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.92f), TextAnchor.MiddleLeft, 38, primaryTextColor);
            recordTitle.fontStyle = FontStyle.Bold;
            CreateText(recordPanel, "BestFloorText", $"Highest Floor  {Mathf.Max(0, bestHighestFloor)}F", new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.64f), TextAnchor.MiddleLeft, 34, primaryTextColor);
            CreateText(recordPanel, "BestScoreText", $"Best Score     {Mathf.Max(0, bestScore)}", new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.38f), TextAnchor.MiddleLeft, 34, primaryTextColor);

            RectTransform characterPanel = CreatePanel(root, "CharacterPanel", new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.54f), panelColor);
            Text characterTitle = CreateText(characterPanel, "CharacterTitleText", "CHARACTER", new Vector2(0.06f, 0.64f), new Vector2(0.46f, 0.9f), TextAnchor.MiddleLeft, 30, primaryTextColor);
            characterTitle.fontStyle = FontStyle.Bold;
            selectedCharacterText = CreateText(characterPanel, "SelectedCharacterText", GetSelectedCharacterLabel(), new Vector2(0.46f, 0.64f), new Vector2(0.94f, 0.9f), TextAnchor.MiddleRight, 26, secondaryTextColor);
            BuildCharacterButtons(characterPanel);

            CreateButton(root, "StartButton", "START", new Vector2(0.12f, 0.22f), new Vector2(0.88f, 0.32f), startButtonColor, Color.black, StartGame, true);

            RectTransform menuPanel = CreatePanel(root, "MenuPanel", new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.18f), panelColor);
            CreateButton(menuPanel, "RankingButton", "RANKING", new Vector2(0.04f, 0.16f), new Vector2(0.27f, 0.84f), disabledButtonColor, secondaryTextColor, null, false);
            CreateButton(menuPanel, "ShopButton", "SHOP", new Vector2(0.29f, 0.16f), new Vector2(0.52f, 0.84f), disabledButtonColor, secondaryTextColor, null, false);
            CreateButton(menuPanel, "OptionsButton", "OPTIONS", new Vector2(0.54f, 0.16f), new Vector2(0.77f, 0.84f), disabledButtonColor, secondaryTextColor, null, false);
            CreateButton(menuPanel, "AdSlotButton", "AD", new Vector2(0.79f, 0.16f), new Vector2(0.96f, 0.84f), disabledButtonColor, secondaryTextColor, null, false);
        }

        private void BuildCharacterButtons(RectTransform parent)
        {
            if (availableCharacters == null || availableCharacters.Length == 0)
            {
                CreateText(parent, "NoCharacterText", "No character data", new Vector2(0.06f, 0.14f), new Vector2(0.94f, 0.56f), TextAnchor.MiddleLeft, 28, secondaryTextColor);
                return;
            }

            int buttonCount = Mathf.Min(availableCharacters.Length, 3);
            float gap = 0.025f;
            float startX = 0.06f;
            float endX = 0.94f;
            float width = (endX - startX - gap * (buttonCount - 1)) / buttonCount;

            for (int i = 0; i < buttonCount; i++)
            {
                CharacterDefinition characterDefinition = availableCharacters[i];
                float minX = startX + (width + gap) * i;
                float maxX = minX + width;
                string label = characterDefinition != null ? characterDefinition.DisplayName : "EMPTY";
                bool interactable = characterDefinition != null;

                Button characterButton = CreateButton(parent, $"CharacterButton{i + 1}", label, new Vector2(minX, 0.12f), new Vector2(maxX, 0.54f), disabledButtonColor, primaryTextColor, () => SelectCharacter(characterDefinition), interactable);
                Text characterButtonText = characterButton.GetComponentInChildren<Text>();
                if (characterButtonText != null)
                {
                    characterButtonText.fontSize = 24;
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

            if (selectedCharacterText != null)
            {
                selectedCharacterText.text = GetSelectedCharacterLabel();
            }

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

        private string GetSelectedCharacterLabel()
        {
            return selectedCharacter != null ? $"Selected  {selectedCharacter.DisplayName}" : "Selected  None";
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
