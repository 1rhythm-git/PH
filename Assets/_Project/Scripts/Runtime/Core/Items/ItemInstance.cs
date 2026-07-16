using PH.Core.Characters;
using PH.Core.Player;
using PH.Core.UI;
using PH.Core.World;
using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.Items
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
        private PlayerHealth playerHealth;
        private RunItemEventRecorder eventRecorder;
        private TopHUDController topHUDController;
        private ItemEffectResolver effectResolver;
        private RectTransform rectTransform;
        private int absoluteFloor;
        private int pageIndex;
        private int pageFloorIndex;
        private int columnIndex;
        private int remainingPassCount;
        private int lastPassDirection;
        private float spawnedAtTime;
        private float lifetimeSeconds;
        private bool isPlayerInside;
        private bool acquired;
        private bool hasPlayerReachedFloor;

        public bool Acquired => acquired;

        private void Awake()
        {
            CacheComponents();
        }

        private void Update()
        {
            if (playerMotor == null)
            {
                playerMotor = FindFirstObjectByType<PlayerMotor>();
            }

            if (playerCharacterRuntime == null && playerMotor != null)
            {
                playerCharacterRuntime = playerMotor.GetComponent<PlayerCharacterRuntime>();
            }

            if (playerHealth == null && playerMotor != null)
            {
                playerHealth = playerMotor.GetComponent<PlayerHealth>();
            }

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
            remainingPassCount = Mathf.Max(1, definition.RequiredPassCount);
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
            if (playerMotor == null)
            {
                playerMotor = FindFirstObjectByType<PlayerMotor>();
            }

            if (playerCharacterRuntime == null && playerMotor != null)
            {
                playerCharacterRuntime = playerMotor.GetComponent<PlayerCharacterRuntime>();
            }

            if (playerHealth == null && playerMotor != null)
            {
                playerHealth = playerMotor.GetComponent<PlayerHealth>();
            }

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
            UpdateProgressText();

            if (remainingPassCount <= 0)
            {
                Acquire();
            }
        }

        private void Acquire()
        {
            if (acquired)
            {
                return;
            }

            acquired = true;

            if (itemImage != null)
            {
                itemImage.color = acquiredColor;
            }

            eventRecorder?.Record(new ItemRunEvent(definition, absoluteFloor, pageIndex, pageFloorIndex, columnIndex, Time.time));
            ApplyHUDItemEffect();
            gameObject.SetActive(false);
        }

        private void ApplyHUDItemEffect()
        {
            if (topHUDController == null)
            {
                topHUDController = FindFirstObjectByType<TopHUDController>();
            }

            if (topHUDController == null || definition == null)
            {
                return;
            }

            if (effectResolver == null)
            {
                effectResolver = new ItemEffectResolver();
            }

            effectResolver.Execute(definition, new ItemEffectContext(topHUDController, playerHealth));
        }

        private void Expire()
        {
            if (acquired)
            {
                return;
            }

            gameObject.SetActive(false);
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

        private static Rect GetWorldRect(RectTransform target)
        {
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            Vector3 bottomLeft = corners[0];
            Vector3 topRight = corners[2];

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
