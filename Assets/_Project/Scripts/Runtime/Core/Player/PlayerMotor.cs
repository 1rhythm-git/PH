using PH.Core.Characters;
using PH.Core.World;
using System.Collections.Generic;
using UnityEngine;

namespace PH.Core.Player
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        private RectTransform rectTransform;
        private RectTransform parentRectTransform;
        private BuildingGridUI buildingGridUI;
        private InfiniteFloorManager floorManager;
        private PlayerCharacterRuntime characterRuntime;
        private float normalizedX;
        private float baseMoveSpeedColumnsPerSecond;
        private float moveSpeedColumnsPerSecond;
        private bool isConfigured;
        private bool movementLocked;
        private readonly List<MoveSpeedBuff> moveSpeedBuffs = new List<MoveSpeedBuff>();

        public float CurrentNormalizedX => normalizedX;
        public float MoveSpeedColumnsPerSecond => moveSpeedColumnsPerSecond;
        public float MoveSpeedBonusPercent => GetActiveMoveSpeedBonusPercent();
        public int ColumnCount => buildingGridUI != null ? Mathf.Max(1, buildingGridUI.Columns) : BuildingGridUI.DefaultColumns;
        public RectTransform RectTransform
        {
            get
            {
                CacheRectTransforms();
                return rectTransform;
            }
        }

        private void Awake()
        {
            CacheRectTransforms();
        }

        private void OnDestroy()
        {
            if (floorManager != null)
            {
                floorManager.CurrentFloorChanged -= HandleCurrentFloorChanged;
            }
        }

        private void Update()
        {
            if (RemoveExpiredMoveSpeedBuffs())
            {
                RecalculateMoveSpeed();
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isConfigured)
            {
                return;
            }

            ApplyPosition();
        }

        public void Configure(BuildingGridUI gridUI, InfiniteFloorManager manager, int startColumn, float columnsPerSecond)
        {
            CacheRectTransforms();

            if (floorManager != null)
            {
                floorManager.CurrentFloorChanged -= HandleCurrentFloorChanged;
            }

            buildingGridUI = gridUI;
            floorManager = manager;
            baseMoveSpeedColumnsPerSecond = Mathf.Max(0f, columnsPerSecond);
            moveSpeedBuffs.Clear();
            RecalculateMoveSpeed();

            int columnCount = buildingGridUI != null ? buildingGridUI.Columns : BuildingGridUI.DefaultColumns;
            int clampedStartColumn = Mathf.Clamp(startColumn, 0, Mathf.Max(0, columnCount - 1));
            normalizedX = (clampedStartColumn + 0.5f) / Mathf.Max(1, columnCount);
            normalizedX = ClampNormalizedX(normalizedX);
            isConfigured = true;

            if (floorManager != null)
            {
                floorManager.CurrentFloorChanged += HandleCurrentFloorChanged;
            }

            ApplyPosition();
        }

        public void SetCharacterRuntime(PlayerCharacterRuntime runtime)
        {
            characterRuntime = runtime;
        }

        public void Move(float horizontalInput, float deltaTime)
        {
            if (!isConfigured || buildingGridUI == null || movementLocked)
            {
                return;
            }

            float input = Mathf.Clamp(horizontalInput, -1f, 1f);
            if (Mathf.Approximately(input, 0f))
            {
                return;
            }

            float previousNormalizedX = normalizedX;
            normalizedX += input * moveSpeedColumnsPerSecond * deltaTime / Mathf.Max(1, buildingGridUI.Columns);
            normalizedX = ClampNormalizedX(normalizedX);
            ApplyPosition();

            float movedColumns = Mathf.Abs(normalizedX - previousNormalizedX) * Mathf.Max(1, buildingGridUI.Columns);
            characterRuntime?.AddMoveDistanceColumns(movedColumns);
        }

        public void SetMovementLocked(bool isLocked)
        {
            movementLocked = isLocked;
        }

        public float AddTimedMoveSpeedPercentBonus(float percent, float durationSeconds)
        {
            float clampedPercent = Mathf.Max(0f, percent);
            float clampedDuration = Mathf.Max(0f, durationSeconds);
            if (clampedPercent <= 0f || clampedDuration <= 0f)
            {
                return moveSpeedColumnsPerSecond;
            }

            moveSpeedBuffs.Add(new MoveSpeedBuff(clampedPercent, Time.time + clampedDuration));
            RecalculateMoveSpeed();

            return moveSpeedColumnsPerSecond;
        }

        public void SetManualAnchoredPosition(Vector2 anchoredPosition)
        {
            CacheRectTransforms();

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
        }

        public Vector2 GetAnchoredPositionForFloorIndex(int pageFloorIndex)
        {
            if (buildingGridUI == null)
            {
                return RectTransform.anchoredPosition;
            }

            CacheRectTransforms();

            Rect parentRect = parentRectTransform != null ? parentRectTransform.rect : buildingGridUI.GetComponent<RectTransform>().rect;
            int row = Mathf.Clamp(pageFloorIndex, 0, Mathf.Max(0, buildingGridUI.Rows - 1));
            float x = Mathf.Lerp(parentRect.xMin, parentRect.xMax, normalizedX);
            float y = GetFloorContactY(parentRect, row);

            return new Vector2(x, y);
        }

        public void WarpToColumn(int column)
        {
            if (!isConfigured || buildingGridUI == null)
            {
                return;
            }

            int columnCount = Mathf.Max(1, buildingGridUI.Columns);
            int clampedColumn = Mathf.Clamp(column, 0, columnCount - 1);
            normalizedX = (clampedColumn + 0.5f) / columnCount;
            normalizedX = ClampNormalizedX(normalizedX);
            ApplyPosition();
        }

        public void SnapCenterToColumn(int column)
        {
            if (!isConfigured || buildingGridUI == null)
            {
                return;
            }

            int columnCount = Mathf.Max(1, buildingGridUI.Columns);
            int clampedColumn = Mathf.Clamp(column, 0, columnCount - 1);
            normalizedX = (clampedColumn + 0.5f) / columnCount;
            ApplyPosition();
        }

        public bool IsWithinColumnRange(int column, float toleranceColumns)
        {
            if (!isConfigured || buildingGridUI == null)
            {
                return false;
            }

            int columnCount = Mathf.Max(1, buildingGridUI.Columns);
            int clampedColumn = Mathf.Clamp(column, 0, columnCount - 1);
            float targetNormalizedX = (clampedColumn + 0.5f) / columnCount;
            float tolerance = Mathf.Max(0f, toleranceColumns) / columnCount;

            return Mathf.Abs(normalizedX - targetNormalizedX) <= tolerance;
        }

        public bool IsAtHorizontalLimit(int direction, float toleranceColumns = 0.02f)
        {
            if (!isConfigured || buildingGridUI == null || direction == 0)
            {
                return false;
            }

            int columnCount = Mathf.Max(1, buildingGridUI.Columns);
            float limit = direction < 0 ? GetMinimumColumnCenterNormalizedX(columnCount) : GetMaximumColumnCenterNormalizedX(columnCount);
            float tolerance = Mathf.Max(0f, toleranceColumns) / columnCount;

            return direction < 0
                ? normalizedX <= limit + tolerance
                : normalizedX >= limit - tolerance;
        }

        private void HandleCurrentFloorChanged(int currentAbsoluteFloor)
        {
            ApplyPosition();
        }

        private void ApplyPosition()
        {
            if (buildingGridUI == null)
            {
                return;
            }

            CacheRectTransforms();

            int row = floorManager != null ? floorManager.CurrentPageFloorIndex : 0;
            Vector2 position = GetAnchoredPositionForFloorIndex(row);

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
        }

        private float GetFloorContactY(Rect parentRect, int row)
        {
            float rowHeight = parentRect.height / Mathf.Max(1, buildingGridUI.Rows);
            float floorLineY = parentRect.yMin + rowHeight * row;
            float halfPlayerHeight = rectTransform.rect.height * 0.5f;

            return floorLineY + halfPlayerHeight;
        }

        private float ClampNormalizedX(float value)
        {
            CacheRectTransforms();

            int columnCount = buildingGridUI != null ? Mathf.Max(1, buildingGridUI.Columns) : BuildingGridUI.DefaultColumns;
            float minNormalizedX = GetMinimumColumnCenterNormalizedX(columnCount);
            float maxNormalizedX = GetMaximumColumnCenterNormalizedX(columnCount);

            if (parentRectTransform == null || parentRectTransform.rect.width <= 0f)
            {
                return Mathf.Clamp(value, minNormalizedX, maxNormalizedX);
            }

            float halfWidth = rectTransform.rect.width * 0.5f;
            float horizontalMargin = halfWidth / parentRectTransform.rect.width;
            horizontalMargin = Mathf.Clamp(horizontalMargin, 0f, 0.5f);
            minNormalizedX = Mathf.Max(minNormalizedX, horizontalMargin);
            maxNormalizedX = Mathf.Min(maxNormalizedX, 1f - horizontalMargin);
            if (minNormalizedX > maxNormalizedX)
            {
                float center = (minNormalizedX + maxNormalizedX) * 0.5f;
                minNormalizedX = center;
                maxNormalizedX = center;
            }

            return Mathf.Clamp(value, minNormalizedX, maxNormalizedX);
        }

        private float GetMinimumColumnCenterNormalizedX(int columnCount)
        {
            return 0.5f / Mathf.Max(1, columnCount);
        }

        private float GetMaximumColumnCenterNormalizedX(int columnCount)
        {
            return 1f - GetMinimumColumnCenterNormalizedX(columnCount);
        }

        private void RecalculateMoveSpeed()
        {
            float multiplier = 1f + GetActiveMoveSpeedBonusPercent() * 0.01f;
            moveSpeedColumnsPerSecond = Mathf.Max(0f, baseMoveSpeedColumnsPerSecond * multiplier);
        }

        private float GetActiveMoveSpeedBonusPercent()
        {
            float totalBonusPercent = 0f;
            for (int i = 0; i < moveSpeedBuffs.Count; i++)
            {
                totalBonusPercent += Mathf.Max(0f, moveSpeedBuffs[i].BonusPercent);
            }

            return totalBonusPercent;
        }

        private bool RemoveExpiredMoveSpeedBuffs()
        {
            bool removed = false;
            float now = Time.time;
            for (int i = moveSpeedBuffs.Count - 1; i >= 0; i--)
            {
                if (now >= moveSpeedBuffs[i].ExpiresAt)
                {
                    moveSpeedBuffs.RemoveAt(i);
                    removed = true;
                }
            }

            return removed;
        }

        private readonly struct MoveSpeedBuff
        {
            public MoveSpeedBuff(float bonusPercent, float expiresAt)
            {
                BonusPercent = bonusPercent;
                ExpiresAt = expiresAt;
            }

            public float BonusPercent { get; }
            public float ExpiresAt { get; }
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
    }
}
