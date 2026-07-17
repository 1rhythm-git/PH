using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.World
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class BuildingGridUI : MonoBehaviour
    {
        public const int DefaultColumns = 8;
        public const int DefaultRows = 10;

        [SerializeField]
        private int columns = DefaultColumns;

        [SerializeField]
        private int rows = DefaultRows;

        [SerializeField]
        private Color cellColor = new Color(0.12f, 0.13f, 0.15f, 0.82f);

        [SerializeField]
        private Color cellBorderColor = new Color(1f, 1f, 1f, 0.16f);

        [SerializeField]
        private bool showFloorLabels;

        [SerializeField]
        private Font labelFont;

        [SerializeField]
        private int labelFontSize = 22;

        private GridCell[,] cells;
        private RectTransform rectTransform;
        private FloorPageData currentPageData;

        public int Columns => columns;
        public int Rows => rows;
        public Color CellColor => cellColor;
        public FloorPageData CurrentPageData => currentPageData;

        private void Awake()
        {
            BuildGrid();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled || cells == null)
            {
                return;
            }

            ApplyCellLayout();
        }

        public void SetPage(FloorPageData pageData)
        {
            currentPageData = pageData;

            if (cells == null)
            {
                BuildGrid();
                return;
            }

            RefreshCellData();
        }

        public GridCell GetCell(int column, int row)
        {
            if (cells == null)
            {
                BuildGrid();
            }

            if (column < 0 || column >= columns || row < 0 || row >= rows)
            {
                return null;
            }

            return cells[column, row];
        }

        public RectTransform GetCellRectTransform(int column, int row)
        {
            GridCell cell = GetCell(column, row);
            return cell != null ? cell.RectTransform : null;
        }

        public Vector2 GetGridLocalPositionByNormalizedPoint(float normalizedX, float normalizedY)
        {
            EnsureRectTransform();

            Rect rect = rectTransform.rect;
            float x = Mathf.Lerp(rect.xMin, rect.xMax, Mathf.Clamp01(normalizedX));
            float y = Mathf.Lerp(rect.yMin, rect.yMax, Mathf.Clamp01(normalizedY));

            return new Vector2(x, y);
        }

        public void SetRowCellBackgroundColor(int row, Color color)
        {
            if (cells == null)
            {
                BuildGrid();
            }

            if (row < 0 || row >= rows)
            {
                return;
            }

            for (int column = 0; column < columns; column++)
            {
                cells[column, row]?.SetBackgroundColor(color);
            }
        }

        private void BuildGrid()
        {
            EnsureRectTransform();

            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);

            if (currentPageData == null)
            {
                currentPageData = new FloorPageData(0, rows);
            }

            ClearChildren();

            cells = new GridCell[columns, rows];

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    GridCell cell = CreateCell(column, row);
                    cells[column, row] = cell;
                }
            }

            ApplyCellLayout();
            RefreshCellData();
        }

        private GridCell CreateCell(int column, int row)
        {
            GameObject cellObject = new GameObject($"Cell_{column}_{row}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(GridCell));
            cellObject.layer = gameObject.layer;
            cellObject.transform.SetParent(transform, false);

            Image image = cellObject.GetComponent<Image>();
            image.color = cellColor;
            image.raycastTarget = false;

            Outline outline = cellObject.GetComponent<Outline>();
            outline.effectColor = cellBorderColor;
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;

            if (showFloorLabels)
            {
                CreateFloorLabel(cellObject.transform, column);
            }

            return cellObject.GetComponent<GridCell>();
        }

        private void CreateFloorLabel(Transform parent, int column)
        {
            GameObject labelObject = new GameObject("FloorLabel", typeof(RectTransform), typeof(Text));
            labelObject.layer = gameObject.layer;
            labelObject.transform.SetParent(parent, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(4f, 2f);
            labelRect.offsetMax = new Vector2(-4f, -2f);

            Text text = labelObject.GetComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 1f, 1f, 0.72f);
            text.fontSize = labelFontSize;
            text.raycastTarget = false;
            text.text = string.Empty;

            if (labelFont == null)
            {
                labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            if (labelFont != null)
            {
                text.font = labelFont;
            }

            labelObject.SetActive(showFloorLabels && column == 0);
        }

        private void ApplyCellLayout()
        {
            if (cells == null)
            {
                return;
            }

            float cellWidth = 1f / columns;
            float cellHeight = 1f / rows;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    GridCell cell = cells[column, row];
                    if (cell == null || cell.RectTransform == null)
                    {
                        continue;
                    }

                    RectTransform cellRect = cell.RectTransform;
                    cellRect.anchorMin = new Vector2(column * cellWidth, row * cellHeight);
                    cellRect.anchorMax = new Vector2((column + 1) * cellWidth, (row + 1) * cellHeight);
                    cellRect.offsetMin = Vector2.zero;
                    cellRect.offsetMax = Vector2.zero;
                    cellRect.pivot = new Vector2(0.5f, 0.5f);
                }
            }
        }

        private void RefreshCellData()
        {
            if (cells == null || currentPageData == null)
            {
                return;
            }

            for (int row = 0; row < rows; row++)
            {
                FloorAddress address = currentPageData.GetAddressByRow(row);

                for (int column = 0; column < columns; column++)
                {
                    bool showLabel = showFloorLabels && column == 0;
                    cells[column, row].Configure(column, row, address, showLabel, cellColor);
                }
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

        private void EnsureRectTransform()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);
            labelFontSize = Mathf.Max(1, labelFontSize);
        }
#endif
    }
}
