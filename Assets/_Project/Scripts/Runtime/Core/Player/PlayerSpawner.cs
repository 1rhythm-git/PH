using PH.Core.Characters;
using PH.Core.Game;
using PH.Core.UI;
using PH.Core.World;
using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.Player
{
    public sealed class PlayerSpawner : MonoBehaviour
    {
        private const string SpriteVisualName = "SpriteVisual";

        [SerializeField]
        private bool spawnOnStart = true;

        [SerializeField]
        private BuildingGridUI buildingGridUI;

        [SerializeField]
        private InfiniteFloorManager floorManager;

        [SerializeField]
        private RectTransform playerLayer;

        [SerializeField]
        private RectTransform touchArea;

        [SerializeField]
        private GameObject playerPrefab;

        [SerializeField]
        private CharacterDefinition characterDefinition;

        [SerializeField]
        private int startColumn;

        [SerializeField]
        private float moveSpeedColumnsPerSecond = 4f;

        [SerializeField]
        private Vector2 playerSize = new Vector2(61f, 72f);

        [SerializeField]
        private Color playerColor = new Color(0.95f, 0.78f, 0.22f, 1f);

        [SerializeField]
        private Color playerOutlineColor = new Color(0.05f, 0.05f, 0.06f, 0.85f);

        private GameObject spawnedPlayer;

        private CharacterDefinition activeCharacterDefinition;

        public GameObject SpawnedPlayer => spawnedPlayer;

        private void Awake()
        {
            EnsureReferences();
        }

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnPlayer();
            }
        }

        [ContextMenu("Debug/Spawn Player")]
        public void SpawnPlayer()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Player는 Play 모드에서만 런타임 생성합니다.", this);
                return;
            }

            EnsureReferences();

            if (spawnedPlayer != null)
            {
                Destroy(spawnedPlayer);
            }

            if (playerLayer == null || buildingGridUI == null || floorManager == null)
            {
                Debug.LogWarning("PlayerSpawner 참조가 부족해 Player를 생성할 수 없습니다.", this);
                return;
            }

            activeCharacterDefinition = CharacterSelectionState.Resolve(characterDefinition);

            spawnedPlayer = playerPrefab != null
                ? Instantiate(playerPrefab, playerLayer)
                : CreateDefaultPlayer();

            spawnedPlayer.name = "Player";
            spawnedPlayer.transform.SetParent(playerLayer, false);
            spawnedPlayer.transform.SetAsLastSibling();

            RectTransform playerRect = spawnedPlayer.GetComponent<RectTransform>();
            playerRect.sizeDelta = playerSize;

            PlayerMotor motor = spawnedPlayer.GetComponent<PlayerMotor>();
            if (motor == null)
            {
                motor = spawnedPlayer.AddComponent<PlayerMotor>();
            }

            PlayerCharacterRuntime characterRuntime = spawnedPlayer.GetComponent<PlayerCharacterRuntime>();
            if (characterRuntime == null)
            {
                characterRuntime = spawnedPlayer.AddComponent<PlayerCharacterRuntime>();
            }

            PlayerController controller = spawnedPlayer.GetComponent<PlayerController>();
            if (controller == null)
            {
                controller = spawnedPlayer.AddComponent<PlayerController>();
            }

            PlayerHealth playerHealth = spawnedPlayer.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = spawnedPlayer.AddComponent<PlayerHealth>();
            }

            if (spawnedPlayer.GetComponent<PlayerBuffVisualFeedback>() == null)
            {
                spawnedPlayer.AddComponent<PlayerBuffVisualFeedback>();
            }

            if (spawnedPlayer.GetComponent<PlayerItemPickupFeedback>() == null)
            {
                spawnedPlayer.AddComponent<PlayerItemPickupFeedback>();
            }

            TopHUDController topHUDController = FindFirstObjectByType<TopHUDController>();
            GameStateController gameStateController = FindFirstObjectByType<GameStateController>();
            ElevatorController elevatorController = FindFirstObjectByType<ElevatorController>();

            characterRuntime.Configure(activeCharacterDefinition);
            ApplyCharacterVisual(spawnedPlayer);
            controller.Configure(motor, touchArea, characterRuntime, characterRuntime.PivotCooldownSeconds);
            motor.SetCharacterRuntime(characterRuntime);
            motor.Configure(buildingGridUI, floorManager, startColumn, characterRuntime.MoveSpeedColumnsPerSecond);
            playerHealth.Configure(characterRuntime.MaxLife, topHUDController, gameStateController, elevatorController, startColumn);

            if (topHUDController != null)
            {
                topHUDController.BindCharacterRuntime(characterRuntime);
                topHUDController.BindPlayerMotor(motor);
                topHUDController.SetHearts(playerHealth.MaxLife, playerHealth.CurrentLife);
            }
        }

        private GameObject CreateDefaultPlayer()
        {
            GameObject playerObject = new GameObject("Player", typeof(RectTransform), typeof(CanvasRenderer), typeof(PlayerShapeGraphic), typeof(Outline), typeof(PlayerMotor), typeof(PlayerController), typeof(PlayerHealth));
            playerObject.layer = playerLayer.gameObject.layer;

            Outline outline = playerObject.GetComponent<Outline>();
            outline.effectColor = activeCharacterDefinition != null ? activeCharacterDefinition.OutlineColor : playerOutlineColor;
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            return playerObject;
        }

        private void ApplyCharacterVisual(GameObject playerObject)
        {
            PlayerShapeGraphic shapeGraphic = playerObject.GetComponent<PlayerShapeGraphic>();
            if (shapeGraphic == null)
            {
                shapeGraphic = playerObject.AddComponent<PlayerShapeGraphic>();
            }

            Image legacyImage = playerObject.GetComponent<Image>();
            if (legacyImage != null)
            {
                legacyImage.enabled = false;
            }

            CharacterBodyShape shape = activeCharacterDefinition != null ? activeCharacterDefinition.BodyShape : CharacterBodyShape.Square;
            bool useSpriteVisual = HasCharacterSprites(activeCharacterDefinition);
            shapeGraphic.enabled = !useSpriteVisual;
            shapeGraphic.color = activeCharacterDefinition != null ? activeCharacterDefinition.BodyColor : playerColor;
            shapeGraphic.raycastTarget = false;
            shapeGraphic.SetShape(shape);

            Outline outline = playerObject.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = activeCharacterDefinition != null ? activeCharacterDefinition.OutlineColor : playerOutlineColor;
            }

            if (useSpriteVisual)
            {
                Image spriteImage = EnsureSpriteVisual(playerObject);
                PlayerSpriteAnimator spriteAnimator = spriteImage.GetComponent<PlayerSpriteAnimator>();
                PlayerController controller = playerObject.GetComponent<PlayerController>();
                spriteAnimator.Configure(activeCharacterDefinition, controller);
            }
            else
            {
                DisableSpriteVisual(playerObject);
            }

            Debug.Log($"Player character applied: {(activeCharacterDefinition != null ? activeCharacterDefinition.DisplayName : "Fallback")} shape={shape} sprite={useSpriteVisual}", this);
        }

        private Image EnsureSpriteVisual(GameObject playerObject)
        {
            Transform existing = playerObject.transform.Find(SpriteVisualName);
            GameObject visualObject = existing != null ? existing.gameObject : null;
            if (visualObject == null)
            {
                visualObject = new GameObject(SpriteVisualName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline), typeof(PlayerSpriteAnimator));
                visualObject.layer = playerObject.layer;
                visualObject.transform.SetParent(playerObject.transform, false);
            }
            else
            {
                if (visualObject.GetComponent<Image>() == null)
                {
                    visualObject.AddComponent<Image>();
                }

                if (visualObject.GetComponent<Outline>() == null)
                {
                    visualObject.AddComponent<Outline>();
                }

                if (visualObject.GetComponent<PlayerSpriteAnimator>() == null)
                {
                    visualObject.AddComponent<PlayerSpriteAnimator>();
                }
            }

            RectTransform visualRect = visualObject.GetComponent<RectTransform>();
            Vector2 visualScale = activeCharacterDefinition != null ? activeCharacterDefinition.SpriteVisualScale : Vector2.one;
            visualRect.anchorMin = new Vector2(0.5f, 0.5f);
            visualRect.anchorMax = new Vector2(0.5f, 0.5f);
            visualRect.sizeDelta = new Vector2(playerSize.x * Mathf.Max(0.01f, visualScale.x), playerSize.y * Mathf.Max(0.01f, visualScale.y));
            visualRect.anchoredPosition = Vector2.zero;
            visualRect.pivot = new Vector2(0.5f, 0.5f);

            Image spriteImage = visualObject.GetComponent<Image>();
            spriteImage.enabled = true;
            spriteImage.color = Color.white;
            spriteImage.preserveAspect = true;
            spriteImage.raycastTarget = false;

            Outline visualOutline = visualObject.GetComponent<Outline>();
            visualOutline.effectColor = activeCharacterDefinition != null ? activeCharacterDefinition.OutlineColor : playerOutlineColor;
            visualOutline.effectDistance = new Vector2(2f, -2f);
            visualOutline.useGraphicAlpha = true;

            return spriteImage;
        }

        private void DisableSpriteVisual(GameObject playerObject)
        {
            Transform existing = playerObject.transform.Find(SpriteVisualName);
            if (existing == null)
            {
                return;
            }

            Image spriteImage = existing.GetComponent<Image>();
            if (spriteImage != null)
            {
                spriteImage.enabled = false;
            }
        }

        private bool HasCharacterSprites(CharacterDefinition definition)
        {
            return definition != null
                && (HasSprites(definition.IdleSprites) || HasSprites(definition.WalkSprites) || HasSprites(definition.RunSprites));
        }

        private bool HasSprites(Sprite[] sprites)
        {
            if (sprites == null || sprites.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureReferences()
        {
            if (buildingGridUI == null)
            {
                buildingGridUI = FindFirstObjectByType<BuildingGridUI>();
            }

            if (floorManager == null)
            {
                floorManager = FindFirstObjectByType<InfiniteFloorManager>();
            }

            if (touchArea == null && buildingGridUI != null)
            {
                touchArea = buildingGridUI.transform.parent as RectTransform;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            startColumn = Mathf.Max(0, startColumn);
            moveSpeedColumnsPerSecond = Mathf.Max(0f, moveSpeedColumnsPerSecond);
            playerSize.x = Mathf.Max(1f, playerSize.x);
            playerSize.y = Mathf.Max(1f, playerSize.y);
        }
#endif
    }
}
