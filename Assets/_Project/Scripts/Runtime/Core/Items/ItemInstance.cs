using System;
using LootUp.Core.Audio;
using LootUp.Core.Characters;
using LootUp.Core.Characters.Skills;
using LootUp.Core.Player;
using LootUp.Core.UI;
using LootUp.Core.World;
using UnityEngine;
using UnityEngine.UI;

namespace LootUp.Core.Items
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class ItemInstance : MonoBehaviour
    {
        [SerializeField]
        private Image itemImage;

        [SerializeField]
        private Text progressText;

        [SerializeField]
        private Color acquiredColor = new Color(1f, 1f, 1f, 0.25f);

        private ItemDefinition definition;
        private InfiniteFloorManager floorManager;
        private PlayerMotor playerMotor;
        private PlayerCharacterRuntime playerCharacterRuntime;
        private CharacterSkillRuntime characterSkillRuntime;
        private PlayerHealth playerHealth;
        private PlayerBuffVisualFeedback playerBuffVisualFeedback;
        private PlayerItemPickupFeedback playerItemPickupFeedback;
        private RunItemEventRecorder eventRecorder;
        private TopHUDController topHUDController;
        private ItemEffectResolver effectResolver;
        private RectTransform rectTransform;
        private int absoluteFloor;
        private int pageIndex;
        private int pageFloorIndex;
        private int columnIndex;
        private int requiredPassCount;
        private int remainingPassCount;
        private int scoreBonusPercent;
        private int lastPassDirection;
        private float spawnedAtTime;
        private float lifetimeSeconds;
        private bool isPlayerInside;
        private bool acquired;
        private bool hasPlayerReachedFloor;
        private readonly Vector3[] worldCorners = new Vector3[4];

        public bool Acquired => acquired;
        public bool IsAvailable => !acquired && gameObject.activeSelf;
        public int PageFloorIndex => pageFloorIndex;
        public int ColumnIndex => columnIndex;
        public event Action<ItemInstance> AvailabilityChanged;

        private void Awake()
        {
            CacheComponents();
        }

        private void Update()
        {
            ResolvePlayerRuntimeComponents();

            if (acquired || playerMotor == null || floorManager == null)
            {
                return;
            }

            if (floorManager.CurrentAbsoluteFloor != absoluteFloor)
            {
                if (hasPlayerReachedFloor)
                {
                    Expire();
                }

                isPlayerInside = false;
                return;
            }

            if (!hasPlayerReachedFloor)
            {
                hasPlayerReachedFloor = true;
                spawnedAtTime = Time.time;
            }

            if (lifetimeSeconds > 0f && Time.time - spawnedAtTime >= lifetimeSeconds)
            {
                Expire();
                return;
            }

            bool inside = IsPlayerInside();
            if (inside && !isPlayerInside)
            {
                TryAddPass();
            }

            isPlayerInside = inside;
        }

        public void Configure(
            ItemDefinition itemDefinition,
            InfiniteFloorManager manager,
            PlayerMotor motor,
            RunItemEventRecorder recorder,
            int itemAbsoluteFloor,
            int itemPageIndex,
            int itemPageFloorIndex,
            int itemColumnIndex,
            Color itemColor)
        {
            Configure(itemDefinition, manager, motor, recorder, itemAbsoluteFloor, itemPageIndex, itemPageFloorIndex, itemColumnIndex, itemColor, itemDefinition != null ? itemDefinition.RequiredPassCount : 1, 0);
        }

        public void Configure(
            ItemDefinition itemDefinition,
            InfiniteFloorManager manager,
            PlayerMotor motor,
            RunItemEventRecorder recorder,
            int itemAbsoluteFloor,
            int itemPageIndex,
            int itemPageFloorIndex,
            int itemColumnIndex,
            Color itemColor,
            int runtimeRequiredPassCount,
            int runtimeScoreBonusPercent)
        {
            CacheComponents();

            definition = itemDefinition;
            floorManager = manager;
            playerMotor = motor;
            eventRecorder = recorder;
            topHUDController = FindFirstObjectByType<TopHUDController>();
            effectResolver = new ItemEffectResolver();
            absoluteFloor = itemAbsoluteFloor;
            pageIndex = itemPageIndex;
            pageFloorIndex = itemPageFloorIndex;
            columnIndex = itemColumnIndex;
            requiredPassCount = Mathf.Max(1, runtimeRequiredPassCount);
            remainingPassCount = requiredPassCount;
            scoreBonusPercent = Mathf.Max(0, runtimeScoreBonusPercent);
            lastPassDirection = 0;
            spawnedAtTime = Time.time;
            lifetimeSeconds = definition.LifetimeSeconds;
            isPlayerInside = false;
            acquired = false;
            hasPlayerReachedFloor = floorManager != null && floorManager.CurrentAbsoluteFloor == absoluteFloor;
            name = $"Item_{definition.ItemId}_{absoluteFloor}_{columnIndex}";

            if (itemImage != null)
            {
                itemImage.color = itemColor;
                itemImage.raycastTarget = false;
            }

            UpdateProgressText();
        }

        private void TryAddPass()
        {
            ResolvePlayerRuntimeComponents();

            if (playerMotor == null)
            {
                return;
            }

            int passDirection = GetPassDirection();
            if (!IsPassDirectionValid(passDirection))
            {
                return;
            }

            bool instantAcquire = playerCharacterRuntime != null && playerCharacterRuntime.RollInstantItemAcquire();
            remainingPassCount = instantAcquire ? 0 : Mathf.Max(0, remainingPassCount - 1);
            lastPassDirection = passDirection;
            GameSfxPlayer.Play(GameSfxId.ItemPass);
            UpdateProgressText();

            if (remainingPassCount <= 0)
            {
                Acquire();
                return;
            }

            ShowPassFeedback();
        }

        private void Acquire()
        {
            if (acquired)
            {
                return;
            }

            acquired = true;
            GameSfxPlayer.Play(GameSfxId.ItemGain);

            if (itemImage != null)
            {
                itemImage.color = acquiredColor;
            }

            string eventId = Guid.NewGuid().ToString("N");
            ItemEffectResult effectResult = ApplyHUDItemEffect(eventId);
            characterSkillRuntime?.TryActivate(definition, effectResult, topHUDController, playerMotor);
            eventRecorder?.Record(new ItemRunEvent(eventId, definition, absoluteFloor, pageIndex, pageFloorIndex, columnIndex, Time.time, effectResult));
            ShowPickupFeedback(effectResult);
            gameObject.SetActive(false);
            AvailabilityChanged?.Invoke(this);
        }

        private void ShowPickupFeedback(ItemEffectResult effectResult)
        {
            if (definition == null || playerMotor == null)
            {
                return;
            }

            if (!EnsurePickupFeedback())
            {
                return;
            }

            playerItemPickupFeedback.Show(GetPickupFeedbackMessage(effectResult), GetPickupFeedbackColor(effectResult), 1.5f);
        }

        private void ShowPassFeedback()
        {
            if (playerMotor == null)
            {
                return;
            }

            if (!EnsurePickupFeedback())
            {
                return;
            }

            playerItemPickupFeedback.Show("PASS", new Color(1f, 0.92f, 0.35f, 1f));
        }

        private void ResolvePlayerRuntimeComponents()
        {
            if (playerMotor == null)
            {
                playerMotor = FindFirstObjectByType<PlayerMotor>();
            }

            if (playerMotor == null)
            {
                return;
            }

            if (playerCharacterRuntime == null)
            {
                playerCharacterRuntime = playerMotor.GetComponent<PlayerCharacterRuntime>();
            }

            if (characterSkillRuntime == null)
            {
                characterSkillRuntime = playerMotor.GetComponent<CharacterSkillRuntime>();
            }

            if (playerHealth == null)
            {
                playerHealth = playerMotor.GetComponent<PlayerHealth>();
            }

            if (playerBuffVisualFeedback == null)
            {
                playerBuffVisualFeedback = playerMotor.GetComponent<PlayerBuffVisualFeedback>();
                if (playerBuffVisualFeedback == null)
                {
                    playerBuffVisualFeedback = playerMotor.gameObject.AddComponent<PlayerBuffVisualFeedback>();
                }
            }

            if (playerItemPickupFeedback == null)
            {
                EnsurePickupFeedback();
            }
        }

        private bool EnsurePickupFeedback()
        {
            if (playerItemPickupFeedback != null)
            {
                return true;
            }

            if (playerMotor == null)
            {
                return false;
            }

            playerItemPickupFeedback = playerMotor.GetComponent<PlayerItemPickupFeedback>();
            if (playerItemPickupFeedback == null)
            {
                playerItemPickupFeedback = playerMotor.gameObject.AddComponent<PlayerItemPickupFeedback>();
            }

            return playerItemPickupFeedback != null;
        }

        private string GetPickupFeedbackMessage(ItemEffectResult effectResult)
        {
            switch (effectResult.Outcome)
            {
                case ItemEffectOutcome.ScoreAdded:
                    return "+SCORE";
                case ItemEffectOutcome.TimeAdded:
                    return "TIME UP";
                case ItemEffectOutcome.LifeHealed:
                    return "GET Life";
                case ItemEffectOutcome.MaxLifeIncreased:
                    return "MAX LIFE";
                case ItemEffectOutcome.MoveSpeedIncreased:
                    return "SPEED UP";
                case ItemEffectOutcome.CollectionAdded:
                    return $"GET {definition.DisplayName}";
                case ItemEffectOutcome.CollectionAlreadyOwned:
                    return "ALREADY OWNED";
                case ItemEffectOutcome.CollectionOwnedLimitReached:
                    return "OWNED LIMIT";
                case ItemEffectOutcome.CollectionRunLimitReached:
                    return "RUN LIMIT";
                case ItemEffectOutcome.CollectionDuplicateEvent:
                    return "ALREADY PROCESSED";
                case ItemEffectOutcome.RunGameMoneyAdded:
                    return $"+{effectResult.Value} MONEY";
            }

            switch (definition.ItemType)
            {
                case ItemType.Time:
                    return "TIME UP";
                case ItemType.Heal:
                    return "GET Life";
                case ItemType.Score:
                    return "+SCORE";
                case ItemType.Currency:
                    return "+MONEY";
                default:
                    return definition.AffectsScore ? "+SCORE" : definition.DisplayName;
            }
        }

        private Color GetPickupFeedbackColor(ItemEffectResult effectResult)
        {
            switch (effectResult.Outcome)
            {
                case ItemEffectOutcome.ScoreAdded:
                    return Color.white;
                case ItemEffectOutcome.TimeAdded:
                    return new Color(0.28f, 0.67f, 1f, 1f);
                case ItemEffectOutcome.LifeHealed:
                case ItemEffectOutcome.MaxLifeIncreased:
                    return new Color(1f, 0.18f, 0.16f, 1f);
                case ItemEffectOutcome.MoveSpeedIncreased:
                    return new Color(1f, 0.86f, 0.16f, 1f);
                case ItemEffectOutcome.CollectionAdded:
                    return new Color(0.35f, 1f, 0.72f, 1f);
                case ItemEffectOutcome.CollectionAlreadyOwned:
                case ItemEffectOutcome.CollectionOwnedLimitReached:
                case ItemEffectOutcome.CollectionRunLimitReached:
                case ItemEffectOutcome.CollectionDuplicateEvent:
                    return new Color(0.72f, 0.72f, 0.72f, 1f);
                case ItemEffectOutcome.RunGameMoneyAdded:
                    return new Color(1f, 0.78f, 0.12f, 1f);
            }

            switch (definition.ItemType)
            {
                case ItemType.Time:
                    return new Color(0.28f, 0.67f, 1f, 1f);
                case ItemType.Heal:
                    return new Color(1f, 0.18f, 0.16f, 1f);
                case ItemType.Score:
                    return Color.white;
                case ItemType.Currency:
                    return new Color(1f, 0.78f, 0.12f, 1f);
                default:
                    return definition.AffectsScore ? Color.white : new Color(1f, 0.86f, 0.16f, 1f);
            }
        }

        private ItemEffectResult ApplyHUDItemEffect(string eventId)
        {
            if (topHUDController == null)
            {
                topHUDController = FindFirstObjectByType<TopHUDController>();
            }

            if (definition == null)
            {
                return ItemEffectResult.None;
            }

            if (effectResolver == null)
            {
                effectResolver = new ItemEffectResolver();
            }

            return effectResolver.Execute(
                definition,
                new ItemEffectContext(
                    topHUDController,
                    playerHealth,
                    playerMotor,
                    playerBuffVisualFeedback,
                    requiredPassCount,
                    scoreBonusPercent,
                    eventRecorder,
                    eventId));
        }

        private void Expire()
        {
            if (acquired)
            {
                return;
            }

            gameObject.SetActive(false);
            AvailabilityChanged?.Invoke(this);
        }

        private bool IsPlayerInside()
        {
            RectTransform playerRect = playerMotor.RectTransform;
            Rect itemRect = GetWorldRect(rectTransform);
            Rect playerWorldRect = GetWorldRect(playerRect);

            return itemRect.Overlaps(playerWorldRect);
        }

        private int GetPassDirection()
        {
            float playerX = playerMotor.RectTransform.anchoredPosition.x;
            float itemX = rectTransform.anchoredPosition.x;

            return playerX < itemX ? 1 : -1;
        }

        private bool IsPassDirectionValid(int passDirection)
        {
            switch (definition.PassDirection)
            {
                case ItemPassDirection.LeftToRightOnly:
                    return passDirection > 0;
                case ItemPassDirection.RightToLeftOnly:
                    return passDirection < 0;
                case ItemPassDirection.Alternating:
                    return lastPassDirection == 0 || lastPassDirection != passDirection;
                default:
                    return true;
            }
        }

        private void UpdateProgressText()
        {
            if (progressText == null || definition == null)
            {
                return;
            }

            bool showProgress = !acquired;
            progressText.gameObject.SetActive(showProgress);
            progressText.text = showProgress ? remainingPassCount.ToString() : string.Empty;
        }

        private Rect GetWorldRect(RectTransform target)
        {
            target.GetWorldCorners(worldCorners);
            Vector3 bottomLeft = worldCorners[0];
            Vector3 topRight = worldCorners[2];

            return new Rect(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
        }

        private void CacheComponents()
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }

            if (itemImage == null)
            {
                itemImage = GetComponent<Image>();
            }

            if (progressText == null)
            {
                progressText = GetComponentInChildren<Text>(true);
            }
        }
    }
}
