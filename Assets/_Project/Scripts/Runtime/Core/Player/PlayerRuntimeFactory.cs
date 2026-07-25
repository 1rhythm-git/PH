using LootUp.Core.Characters;
using LootUp.Core.Characters.Skills;
using LootUp.Core.Game;
using LootUp.Core.Items;
using LootUp.Core.UI;
using LootUp.Core.World;
using UnityEngine;
using UnityEngine.UI;

namespace LootUp.Core.Player
{
    public readonly struct PlayerRuntimeComponents
    {
        public PlayerRuntimeComponents(
            GameObject root,
            PlayerMotor motor,
            PlayerCharacterRuntime characterRuntime,
            CharacterSkillRuntime characterSkillRuntime,
            PlayerController controller,
            PlayerHealth health)
        {
            Root = root;
            Motor = motor;
            CharacterRuntime = characterRuntime;
            CharacterSkillRuntime = characterSkillRuntime;
            Controller = controller;
            Health = health;
        }

        public GameObject Root { get; }
        public PlayerMotor Motor { get; }
        public PlayerCharacterRuntime CharacterRuntime { get; }
        public CharacterSkillRuntime CharacterSkillRuntime { get; }
        public PlayerController Controller { get; }
        public PlayerHealth Health { get; }
    }

    public sealed class PlayerRuntimeFactory
    {
        private const string SpriteVisualName = "SpriteVisual";

        private readonly RectTransform playerLayer;
        private readonly GameObject playerPrefab;
        private readonly Vector2 playerSize;
        private readonly Color playerColor;
        private readonly Color playerOutlineColor;
        private readonly UnityEngine.Object logContext;

        public PlayerRuntimeFactory(
            RectTransform playerLayer,
            GameObject playerPrefab,
            Vector2 playerSize,
            Color playerColor,
            Color playerOutlineColor,
            UnityEngine.Object logContext)
        {
            this.playerLayer = playerLayer;
            this.playerPrefab = playerPrefab;
            this.playerSize = playerSize;
            this.playerColor = playerColor;
            this.playerOutlineColor = playerOutlineColor;
            this.logContext = logContext;
        }

        // (추가) Player 생성과 필수 컴포넌트 조립을 Spawner에서 분리한다.
        public PlayerRuntimeComponents Create(CharacterDefinition characterDefinition)
        {
            GameObject playerObject = playerPrefab != null
                ? UnityEngine.Object.Instantiate(playerPrefab, playerLayer)
                : CreateDefaultPlayer(characterDefinition);

            playerObject.name = "Player";
            playerObject.transform.SetParent(playerLayer, false);
            playerObject.transform.SetAsLastSibling();

            RectTransform playerRect = playerObject.GetComponent<RectTransform>();
            playerRect.sizeDelta = playerSize;

            PlayerMotor motor = GetOrAddComponent<PlayerMotor>(playerObject);
            PlayerCharacterRuntime characterRuntime = GetOrAddComponent<PlayerCharacterRuntime>(playerObject);
            CharacterSkillRuntime characterSkillRuntime = GetOrAddComponent<CharacterSkillRuntime>(playerObject);
            PlayerController controller = GetOrAddComponent<PlayerController>(playerObject);
            PlayerHealth playerHealth = GetOrAddComponent<PlayerHealth>(playerObject);

            GetOrAddComponent<PlayerBuffVisualFeedback>(playerObject);
            GetOrAddComponent<PlayerItemPickupFeedback>(playerObject);
            GetOrAddComponent<PlayerMovementDustFeedback>(playerObject);

            PlayerRuntimeComponents components = new PlayerRuntimeComponents(
                playerObject,
                motor,
                characterRuntime,
                characterSkillRuntime,
                controller,
                playerHealth);

            ApplyCharacterVisual(components, characterDefinition);
            return components;
        }

        private GameObject CreateDefaultPlayer(CharacterDefinition characterDefinition)
        {
            GameObject playerObject = new GameObject(
                "Player",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(PlayerShapeGraphic),
                typeof(Outline),
                typeof(PlayerMotor),
                typeof(PlayerController),
                typeof(PlayerHealth));
            playerObject.layer = playerLayer.gameObject.layer;

            Outline outline = playerObject.GetComponent<Outline>();
            outline.effectColor = characterDefinition != null
                ? characterDefinition.OutlineColor
                : playerOutlineColor;
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            return playerObject;
        }

        private void ApplyCharacterVisual(
            PlayerRuntimeComponents components,
            CharacterDefinition characterDefinition)
        {
            GameObject playerObject = components.Root;
            PlayerShapeGraphic shapeGraphic = GetOrAddComponent<PlayerShapeGraphic>(playerObject);

            Image legacyImage = playerObject.GetComponent<Image>();
            if (legacyImage != null)
            {
                legacyImage.enabled = false;
            }

            bool useSpriteVisual = HasCharacterSprites(characterDefinition);
            shapeGraphic.enabled = !useSpriteVisual;
            shapeGraphic.color = characterDefinition != null
                ? characterDefinition.BodyColor
                : playerColor;
            shapeGraphic.raycastTarget = false;

            Outline outline = playerObject.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = characterDefinition != null
                    ? characterDefinition.OutlineColor
                    : playerOutlineColor;
            }

            if (useSpriteVisual)
            {
                Image spriteImage = EnsureSpriteVisual(playerObject, characterDefinition);
                PlayerSpriteAnimator spriteAnimator = spriteImage.GetComponent<PlayerSpriteAnimator>();
                spriteAnimator.Configure(characterDefinition, components.Controller);
            }
            else
            {
                DisableSpriteVisual(playerObject);
            }

            Debug.Log(
                $"Player character applied: {(characterDefinition != null ? characterDefinition.DisplayName : "Fallback")} sprite={useSpriteVisual}",
                logContext);
        }

        private Image EnsureSpriteVisual(
            GameObject playerObject,
            CharacterDefinition characterDefinition)
        {
            Transform existing = playerObject.transform.Find(SpriteVisualName);
            GameObject visualObject = existing != null ? existing.gameObject : null;
            if (visualObject == null)
            {
                visualObject = new GameObject(
                    SpriteVisualName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(Outline),
                    typeof(PlayerSpriteAnimator));
                visualObject.layer = playerObject.layer;
                visualObject.transform.SetParent(playerObject.transform, false);
            }
            else
            {
                GetOrAddComponent<Image>(visualObject);
                GetOrAddComponent<Outline>(visualObject);
                GetOrAddComponent<PlayerSpriteAnimator>(visualObject);
            }

            RectTransform visualRect = visualObject.GetComponent<RectTransform>();
            Vector2 visualScale = characterDefinition != null
                ? characterDefinition.SpriteVisualScale
                : Vector2.one;
            Vector2 visualSize = new Vector2(
                playerSize.x * Mathf.Max(0.01f, visualScale.x),
                playerSize.y * Mathf.Max(0.01f, visualScale.y));
            visualRect.anchorMin = new Vector2(0.5f, 0.5f);
            visualRect.anchorMax = new Vector2(0.5f, 0.5f);
            visualRect.sizeDelta = visualSize;
            visualRect.anchoredPosition = new Vector2(0f, (visualSize.y - playerSize.y) * 0.5f);
            visualRect.pivot = new Vector2(0.5f, 0.5f);

            Image spriteImage = visualObject.GetComponent<Image>();
            spriteImage.enabled = true;
            spriteImage.color = Color.white;
            spriteImage.preserveAspect = true;
            spriteImage.raycastTarget = false;

            Outline visualOutline = visualObject.GetComponent<Outline>();
            visualOutline.effectColor = characterDefinition != null
                ? characterDefinition.OutlineColor
                : playerOutlineColor;
            visualOutline.effectDistance = new Vector2(2f, -2f);
            visualOutline.useGraphicAlpha = true;

            return spriteImage;
        }

        private static void DisableSpriteVisual(GameObject playerObject)
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

        private static bool HasCharacterSprites(CharacterDefinition definition)
        {
            return definition != null
                && (HasSprites(definition.IdleSprites)
                    || HasSprites(definition.WalkSprites)
                    || HasSprites(definition.RunSprites));
        }

        private static bool HasSprites(Sprite[] sprites)
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

        private static T GetOrAddComponent<T>(GameObject target)
            where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }
    }

    public sealed class PlayerRuntimeBinder
    {
        // (추가) 생성된 Player와 게임 시스템 간 런타임 연결을 한곳에서 구성한다.
        public void Bind(
            GameObject host,
            PlayerRuntimeComponents components,
            CharacterDefinition characterDefinition,
            RectTransform touchArea,
            BuildingGridUI buildingGridUI,
            InfiniteFloorManager floorManager,
            int startColumn)
        {
            TopHUDController topHUDController = UnityEngine.Object.FindFirstObjectByType<TopHUDController>();
            GameStateController gameStateController = UnityEngine.Object.FindFirstObjectByType<GameStateController>();
            ElevatorController elevatorController = UnityEngine.Object.FindFirstObjectByType<ElevatorController>();
            ItemSpawner itemSpawner = UnityEngine.Object.FindFirstObjectByType<ItemSpawner>();

            components.CharacterRuntime.Configure(characterDefinition);
            components.CharacterSkillRuntime.Configure(characterDefinition);
            components.Controller.Configure(
                components.Motor,
                touchArea,
                components.CharacterRuntime,
                components.CharacterRuntime.PivotCooldownSeconds);
            components.Motor.SetCharacterRuntime(components.CharacterRuntime);
            components.Motor.Configure(
                buildingGridUI,
                floorManager,
                startColumn,
                components.CharacterRuntime.MoveSpeedColumnsPerSecond);
            components.Health.Configure(
                components.CharacterRuntime.MaxLife,
                topHUDController,
                gameStateController,
                elevatorController,
                startColumn);

            BindFeverController(
                host,
                components.CharacterRuntime,
                components.Motor,
                topHUDController,
                buildingGridUI,
                floorManager,
                itemSpawner);

            if (topHUDController != null)
            {
                topHUDController.BindCharacterRuntime(components.CharacterRuntime);
                topHUDController.BindPlayerMotor(components.Motor);
                topHUDController.SetHearts(components.Health.MaxLife, components.Health.CurrentLife);
            }
        }

        private static void BindFeverController(
            GameObject host,
            PlayerCharacterRuntime characterRuntime,
            PlayerMotor motor,
            TopHUDController topHUDController,
            BuildingGridUI buildingGridUI,
            InfiniteFloorManager floorManager,
            ItemSpawner itemSpawner)
        {
            FeverGoldFieldController feverController = host.GetComponent<FeverGoldFieldController>();
            if (feverController == null)
            {
                feverController = host.AddComponent<FeverGoldFieldController>();
            }

            feverController.Configure(
                characterRuntime,
                buildingGridUI,
                floorManager,
                itemSpawner,
                motor,
                topHUDController);
        }
    }
}
