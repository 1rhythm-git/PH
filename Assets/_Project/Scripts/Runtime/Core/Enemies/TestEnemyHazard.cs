using PH.Core.Player;
using PH.Core.World;
using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.Enemies
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class TestEnemyHazard : MonoBehaviour
    {
        [SerializeField]
        private int damage = 1;

        [SerializeField]
        private float hitCooldownSeconds = 0.65f;

        [SerializeField]
        private float guideLineThickness = 4f;

        [SerializeField]
        private Color guideLineColor = new Color(1f, 1f, 1f, 0.68f);

        [SerializeField]
        private float collisionSweepPadding = 4f;

        [SerializeField]
        private Vector2 collisionHitboxScale = new Vector2(0.84f, 0.92f);

        [SerializeField]
        private bool showHitboxDebug = false;

        [SerializeField]
        private Color hitboxDebugColor = new Color(0f, 1f, 0.25f, 0.22f);

        private RectTransform rectTransform;
        private RectTransform parentRectTransform;
        private RectTransform guideLineRectTransform;
        private RectTransform guideLineLayer;
        private RectTransform hitboxDebugRectTransform;
        private Image guideLineImage;
        private Image hitboxDebugImage;
        private BuildingGridUI buildingGridUI;
        private PlayerSpawner playerSpawner;
        private int lineIndex;
        private float minVerticalSpeed;
        private float maxVerticalSpeed;
        private float currentVerticalSpeed;
        private int verticalDirection = -1;
        private float nextHitAllowedTime;
        private float lastMoveDistance;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentRectTransform = transform.parent as RectTransform;
        }

        private void Update()
        {
            MoveVertical(Time.deltaTime);

            PlayerHealth playerHealth = ResolvePlayerHealth();
            if (playerHealth == null || Time.time < nextHitAllowedTime)
            {
                return;
            }

            if (!IsPlayerOverlapping(playerHealth))
            {
                return;
            }

            if (playerHealth.TakeDamage(damage))
            {
                nextHitAllowedTime = Time.time + Mathf.Max(0f, hitCooldownSeconds);
            }
        }

        private void OnDestroy()
        {
            DestroyLinkedRuntimeObject(guideLineRectTransform);
            DestroyLinkedRuntimeObject(hitboxDebugRectTransform);
        }

        private void DestroyLinkedRuntimeObject(RectTransform target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target.gameObject);
            }
            else
            {
                DestroyImmediate(target.gameObject);
            }
        }

        public void Configure(
            BuildingGridUI gridUI,
            PlayerSpawner spawner,
            int targetLineIndex,
            int damageAmount,
            float hitCooldown,
            float minSpeed,
            float maxSpeed,
            float lineThickness,
            Color lineColor,
            RectTransform lineLayer)
        {
            buildingGridUI = gridUI;
            playerSpawner = spawner;
            lineIndex = Mathf.Max(1, targetLineIndex);
            damage = Mathf.Max(1, damageAmount);
            hitCooldownSeconds = Mathf.Max(0f, hitCooldown);
            guideLineThickness = Mathf.Max(1f, lineThickness);
            guideLineColor = lineColor;
            guideLineLayer = lineLayer;
            minVerticalSpeed = Mathf.Max(0f, Mathf.Min(minSpeed, maxSpeed));
            maxVerticalSpeed = Mathf.Max(minVerticalSpeed, Mathf.Max(minSpeed, maxSpeed));
            currentVerticalSpeed = RollVerticalSpeed();
            verticalDirection = -1;
            EnsureGuideLine();
            EnsureHitboxDebug();
            ApplyInitialPosition();
            UpdateGuideLine();
            UpdateHitboxDebug();
        }

        private void ApplyInitialPosition()
        {
            CacheRectTransforms();

            if (buildingGridUI == null || rectTransform == null || parentRectTransform == null)
            {
                return;
            }

            Rect parentRect = parentRectTransform.rect;
            float x = GetLineCenterX(parentRect);
            float y = parentRect.yMax - rectTransform.rect.height * 0.5f;

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(x, y);
            UpdateGuideLine();
            UpdateHitboxDebug();
        }

        private void MoveVertical(float deltaTime)
        {
            CacheRectTransforms();

            if (rectTransform == null || parentRectTransform == null)
            {
                return;
            }

            Rect parentRect = parentRectTransform.rect;
            float halfHeight = rectTransform.rect.height * 0.5f;
            float minY = parentRect.yMin + halfHeight;
            float maxY = parentRect.yMax - halfHeight;
            Vector2 position = rectTransform.anchoredPosition;
            float previousY = position.y;
            position.x = GetLineCenterX(parentRect);
            position.y += verticalDirection * currentVerticalSpeed * Mathf.Max(0f, deltaTime);

            if (position.y <= minY)
            {
                position.y = minY;
                verticalDirection = 1;
                currentVerticalSpeed = RollVerticalSpeed();
            }
            else if (position.y >= maxY)
            {
                position.y = maxY;
                verticalDirection = -1;
                currentVerticalSpeed = RollVerticalSpeed();
            }

            rectTransform.anchoredPosition = position;
            lastMoveDistance = Mathf.Abs(position.y - previousY);
            UpdateGuideLine();
            UpdateHitboxDebug();
        }

        private void EnsureGuideLine()
        {
            if (guideLineRectTransform != null)
            {
                return;
            }

            CacheRectTransforms();

            RectTransform lineParent = guideLineLayer != null ? guideLineLayer : parentRectTransform;
            if (lineParent == null)
            {
                return;
            }

            GameObject guideLineObject = new GameObject($"{name}_GuideLine", typeof(RectTransform), typeof(Image));
            guideLineObject.layer = gameObject.layer;
            guideLineObject.transform.SetParent(lineParent, false);
            guideLineObject.transform.SetAsFirstSibling();

            guideLineRectTransform = guideLineObject.GetComponent<RectTransform>();
            guideLineImage = guideLineObject.GetComponent<Image>();
            guideLineImage.color = guideLineColor;
            guideLineImage.raycastTarget = false;
        }

        private void UpdateGuideLine()
        {
            CacheRectTransforms();
            EnsureGuideLine();

            RectTransform lineParent = guideLineLayer != null ? guideLineLayer : parentRectTransform;
            if (guideLineRectTransform == null || lineParent == null || rectTransform == null)
            {
                return;
            }

            Rect parentRect = lineParent.rect;
            Vector2 enemyPosition = rectTransform.anchoredPosition;
            float halfEnemyHeight = rectTransform.rect.height * 0.5f;
            float enemyHeadY = enemyPosition.y + halfEnemyHeight;
            float topY = parentRect.yMax;
            float lineHeight = Mathf.Max(0f, topY - enemyHeadY);

            guideLineRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            guideLineRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            guideLineRectTransform.pivot = new Vector2(0.5f, 1f);
            guideLineRectTransform.anchoredPosition = new Vector2(enemyPosition.x, topY);
            guideLineRectTransform.sizeDelta = new Vector2(guideLineThickness, lineHeight);

            if (guideLineImage != null)
            {
                guideLineImage.color = guideLineColor;
            }
        }

        private float GetLineCenterX(Rect parentRect)
        {
            int columns = buildingGridUI != null ? Mathf.Max(1, buildingGridUI.Columns) : BuildingGridUI.DefaultColumns;
            int clampedLineIndex = Mathf.Clamp(lineIndex, 1, Mathf.Max(1, columns - 1));
            float normalizedX = (float)clampedLineIndex / columns;

            return Mathf.Lerp(parentRect.xMin, parentRect.xMax, normalizedX);
        }

        private float RollVerticalSpeed()
        {
            if (Mathf.Approximately(minVerticalSpeed, maxVerticalSpeed))
            {
                return minVerticalSpeed;
            }

            return Random.Range(minVerticalSpeed, maxVerticalSpeed);
        }

        private void CacheRectTransforms()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (parentRectTransform == null && transform.parent != null)
            {
                parentRectTransform = transform.parent as RectTransform;
            }
        }

        private PlayerHealth ResolvePlayerHealth()
        {
            if (playerSpawner != null && playerSpawner.SpawnedPlayer != null)
            {
                return playerSpawner.SpawnedPlayer.GetComponent<PlayerHealth>();
            }

            return FindFirstObjectByType<PlayerHealth>();
        }

        private bool IsPlayerOverlapping(PlayerHealth playerHealth)
        {
            RectTransform playerRect = playerHealth != null ? playerHealth.transform as RectTransform : null;
            if (playerRect == null || rectTransform == null)
            {
                return false;
            }

            if (parentRectTransform == null)
            {
                return false;
            }

            Vector2 enemyPosition = rectTransform.anchoredPosition;
            float playerY = ConvertWorldPointToParentLocal(playerRect.position).y;
            float playerX = ConvertWorldPointToParentLocal(playerRect.position).x;
            Vector2 scale = new Vector2(Mathf.Clamp01(collisionHitboxScale.x), Mathf.Clamp01(collisionHitboxScale.y));
            float halfEnemyWidth = rectTransform.rect.width * 0.5f * scale.x;
            float halfPlayerWidth = playerRect.rect.width * 0.5f * scale.x;
            float halfEnemyHeight = rectTransform.rect.height * 0.5f * scale.y;
            float halfPlayerHeight = playerRect.rect.height * 0.5f * scale.y;
            float horizontalTolerance = halfEnemyWidth + halfPlayerWidth;
            float verticalTolerance = halfEnemyHeight + halfPlayerHeight + Mathf.Max(collisionSweepPadding, lastMoveDistance);

            return Mathf.Abs(enemyPosition.x - playerX) <= horizontalTolerance
                && Mathf.Abs(enemyPosition.y - playerY) <= verticalTolerance;
        }

        private Vector2 ConvertWorldPointToParentLocal(Vector3 worldPosition)
        {
            Vector3 localPoint = parentRectTransform.InverseTransformPoint(worldPosition);
            return new Vector2(localPoint.x, localPoint.y);
        }

        private void EnsureHitboxDebug()
        {
            if (!showHitboxDebug || hitboxDebugRectTransform != null)
            {
                return;
            }

            CacheRectTransforms();

            if (parentRectTransform == null)
            {
                return;
            }

            GameObject debugObject = new GameObject($"{name}_HitboxDebug", typeof(RectTransform), typeof(Image));
            debugObject.layer = gameObject.layer;
            debugObject.transform.SetParent(parentRectTransform, false);
            debugObject.transform.SetSiblingIndex(transform.GetSiblingIndex());

            hitboxDebugRectTransform = debugObject.GetComponent<RectTransform>();
            hitboxDebugImage = debugObject.GetComponent<Image>();
            hitboxDebugImage.color = hitboxDebugColor;
            hitboxDebugImage.raycastTarget = false;
        }

        private void UpdateHitboxDebug()
        {
            if (!showHitboxDebug)
            {
                return;
            }

            EnsureHitboxDebug();

            if (hitboxDebugRectTransform == null || rectTransform == null)
            {
                return;
            }

            Vector2 scale = new Vector2(Mathf.Clamp01(collisionHitboxScale.x), Mathf.Clamp01(collisionHitboxScale.y));
            hitboxDebugRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            hitboxDebugRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            hitboxDebugRectTransform.pivot = new Vector2(0.5f, 0.5f);
            hitboxDebugRectTransform.anchoredPosition = rectTransform.anchoredPosition;
            hitboxDebugRectTransform.sizeDelta = new Vector2(rectTransform.rect.width * scale.x, rectTransform.rect.height * scale.y);

            if (hitboxDebugImage != null)
            {
                hitboxDebugImage.color = hitboxDebugColor;
            }
        }
    }
}
