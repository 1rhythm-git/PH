using PH.Core.World;
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
        private float normalizedX;
        private float moveSpeedColumnsPerSecond;
        private bool isConfigured;
        private bool movementLocked;

        public float CurrentNormalizedX => normalizedX;
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
            moveSpeedColumnsPerSecond = Mathf.Max(0f, columnsPerSecond);

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

            normalizedX += input * moveSpeedColumnsPerSecond * deltaTime / Mathf.Max(1, buildingGridUI.Columns);
            normalizedX = ClampNormalizedX(normalizedX);
            ApplyPosition();
        }

        public void SetMovementLocked(bool isLocked)
        {
            movementLocked = isLocked;
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

            if (parentRectTransform == null || parentRectTransform.rect.width <= 0f)
            {
                return Mathf.Clamp01(value);
            }

            float halfWidth = rectTransform.rect.width * 0.5f;
            float horizontalMargin = halfWidth / parentRectTransform.rect.width;
            horizontalMargin = Mathf.Clamp(horizontalMargin, 0f, 0.5f);

            return Mathf.Clamp(value, horizontalMargin, 1f - horizontalMargin);
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
