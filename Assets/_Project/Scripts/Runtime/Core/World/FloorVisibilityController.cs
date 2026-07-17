using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.World
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class FloorVisibilityController : MonoBehaviour
    {
        [SerializeField]
        private BuildingGridUI buildingGridUI;

        [SerializeField]
        private InfiniteFloorManager floorManager;

        [SerializeField]
        private Color currentFloorColor = new Color(0f, 0f, 0f, 0f);

        [SerializeField]
        private Color pastFloorColor = new Color(0f, 0f, 0f, 0.34f);

        [SerializeField]
        private Color futureFloorColor = new Color(0f, 0f, 0f, 0.92f);

        [SerializeField]
        private bool clearCurrentFloorCellBackground = true;

        [SerializeField]
        private Color currentFloorCellColor = new Color(0f, 0f, 0f, 0f);

        [SerializeField]
        private bool blockRaycasts;

        private RectTransform rectTransform;
        private Image[] floorOverlays;

        private void Awake()
        {
            EnsureReferences();
            BuildVisibilityRows();
        }

        private void OnEnable()
        {
            EnsureReferences();

            if (floorManager != null)
            {
                floorManager.CurrentFloorChanged += RefreshVisibility;
            }
        }

        private void Start()
        {
            RefreshVisibility();
        }

        private void OnDisable()
        {
            if (floorManager != null)
            {
                floorManager.CurrentFloorChanged -= RefreshVisibility;
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled || floorOverlays == null)
            {
                return;
            }

            ApplyOverlayLayout();
        }

        public void RefreshVisibility()
        {
            if (floorManager == null)
            {
                return;
            }

            RefreshVisibility(floorManager.CurrentAbsoluteFloor);
        }

        [ContextMenu("Debug/Refresh Visibility")]
        private void DebugRefreshVisibility()
        {
            RefreshVisibility();
        }

        public void RefreshVisibility(int currentAbsoluteFloor)
        {
            EnsureReferences();

            if (buildingGridUI == null)
            {
                return;
            }

            if (floorOverlays == null || floorOverlays.Length != buildingGridUI.Rows)
            {
                BuildVisibilityRows();
            }

            FloorPageData pageData = buildingGridUI.CurrentPageData;
            if (pageData == null)
            {
                return;
            }

            for (int row = 0; row < floorOverlays.Length; row++)
            {
                Image overlay = floorOverlays[row];
                if (overlay == null)
                {
                    continue;
                }

                int rowAbsoluteFloor = pageData.GetAddressByRow(row).AbsoluteFloor;

                if (rowAbsoluteFloor == currentAbsoluteFloor)
                {
                    overlay.color = currentFloorColor;
                    ApplyRowCellBackground(row, true);
                }
                else if (rowAbsoluteFloor < currentAbsoluteFloor)
                {
                    overlay.color = pastFloorColor;
                    ApplyRowCellBackground(row, false);
                }
                else
                {
                    overlay.color = futureFloorColor;
                    ApplyRowCellBackground(row, false);
                }

                overlay.raycastTarget = blockRaycasts;
            }
        }

        private void ApplyRowCellBackground(int row, bool isCurrentFloor)
        {
            if (buildingGridUI == null || !clearCurrentFloorCellBackground)
            {
                return;
            }

            Color color = isCurrentFloor ? currentFloorCellColor : buildingGridUI.CellColor;
            buildingGridUI.SetRowCellBackgroundColor(row, color);
        }

        private void BuildVisibilityRows()
        {
            EnsureReferences();

            int rowCount = buildingGridUI != null ? buildingGridUI.Rows : BuildingGridUI.DefaultRows;
            rowCount = Mathf.Max(1, rowCount);

            ClearChildren();
            floorOverlays = new Image[rowCount];

            for (int row = 0; row < rowCount; row++)
            {
                GameObject overlayObject = new GameObject($"VisibilityRow_{row}", typeof(RectTransform), typeof(Image));
                overlayObject.layer = gameObject.layer;
                overlayObject.transform.SetParent(transform, false);

                Image image = overlayObject.GetComponent<Image>();
                image.color = futureFloorColor;
                image.raycastTarget = blockRaycasts;

                floorOverlays[row] = image;
            }

            ApplyOverlayLayout();
        }

        private void ApplyOverlayLayout()
        {
            if (floorOverlays == null || floorOverlays.Length == 0)
            {
                return;
            }

            float rowHeight = 1f / floorOverlays.Length;

            for (int row = 0; row < floorOverlays.Length; row++)
            {
                Image overlay = floorOverlays[row];
                if (overlay == null)
                {
                    continue;
                }

                RectTransform overlayRect = overlay.rectTransform;
                overlayRect.anchorMin = new Vector2(0f, row * rowHeight);
                overlayRect.anchorMax = new Vector2(1f, (row + 1) * rowHeight);
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;
                overlayRect.pivot = new Vector2(0.5f, 0.5f);
            }
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);

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
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (buildingGridUI == null)
            {
                buildingGridUI = FindFirstObjectByType<BuildingGridUI>();
            }

            if (floorManager == null)
            {
                floorManager = FindFirstObjectByType<InfiniteFloorManager>();
            }
        }

    }
}
