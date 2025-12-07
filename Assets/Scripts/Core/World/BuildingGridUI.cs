using UnityEngine;
using UnityEngine.UI;

public class BuildingGridUI : MonoBehaviour
{
    [Header("Grid Settings")]
    public int columns = 8;   // 가로 칸 수
    public int rows = 10;     // 세로 칸 수 (보이는 층 수)

    [Header("References")]
    public RectTransform gridPanel;   // GridPanel (없으면 자기 자신)
    public GameObject cellPrefab;     // CellUI 프리팹

    [Header("Floor Colors")]
    public Color unreachedColor = new Color(0f, 0f, 0f, 0.9f);   // 아직 도달 X (암전)
    public Color currentColor = Color.white;                    // 현재 층 (밝게)
    public Color passedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // 이미 지난 층 (어둡게)

    private Image[,] cellImages;
    private GridLayoutGroup gridLayout;
    private bool isInitialized = false;

    private void Awake()
    {
        // gridPanel 비어 있으면 자기 RectTransform 사용
        if (gridPanel == null)
            gridPanel = GetComponent<RectTransform>();

        gridLayout = gridPanel.GetComponent<GridLayoutGroup>();
    }

    private void Start()
    {
        RegenerateGrid();
    }

    // RectTransform 크기가 바뀔 때마다 호출됨 (Canvas 리사이즈 등)
    private void OnRectTransformDimensionsChange()
    {
        if (isInitialized)
        {
            UpdateCellSize();
        }
    }

    // === 그리드 전체 다시 만들기 ===
    public void RegenerateGrid()
    {
        if (gridPanel == null || cellPrefab == null || gridLayout == null)
        {
            Debug.LogError("BuildingGridUI: gridPanel, cellPrefab, gridLayout 중 하나가 비어 있습니다.");
            return;
        }

        // 기존 자식 제거
        for (int i = gridPanel.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(gridPanel.GetChild(i).gameObject);
        }

        cellImages = new Image[columns, rows];

        // 먼저 Cell Size를 패널 크기 기준으로 계산
        UpdateCellSize();

        // 셀 생성
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                GameObject cellObj = Instantiate(cellPrefab, gridPanel);
                cellObj.name = $"Cell_{x}_{y}";

                Image img = cellObj.GetComponent<Image>();
                cellImages[x, y] = img;
            }
        }

        isInitialized = true;
        ApplyFloorStates(0, 0); // 초기 상태 적용 (옵션)
    }

    // === GridPanel 크기에 맞게 Cell Size 조정 ===
    private void UpdateCellSize()
    {
        if (gridLayout == null || gridPanel == null) return;

        Vector2 size = gridPanel.rect.size;

        // GridLayoutGroup의 패딩을 고려해서 내부 실제 영역 계산
        var padding = gridLayout.padding;
        float innerWidth = size.x - padding.left - padding.right;
        float innerHeight = size.y - padding.top - padding.bottom;

        float cellW = innerWidth / Mathf.Max(columns, 1);
        float cellH = innerHeight / Mathf.Max(rows, 1);

        gridLayout.cellSize = new Vector2(cellW, cellH);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
    }

    // === 특정 층 색 바꾸기 ===
    public void SetFloorColor(int floorIndex, Color color)
    {
        if (cellImages == null) return;
        if (floorIndex < 0 || floorIndex >= rows) return;

        for (int x = 0; x < columns; x++)
        {
            if (cellImages[x, floorIndex] != null)
            {
                cellImages[x, floorIndex].color = color;
            }
        }
    }

    // === 현재/지난/미도달 층 상태 적용 ===
    public void ApplyFloorStates(int currentFloorIndex, int maxReachedFloorIndex)
    {
        if (cellImages == null) return;

        currentFloorIndex = Mathf.Clamp(currentFloorIndex, 0, rows - 1);

        for (int y = 0; y < rows; y++)
        {
            if (y < currentFloorIndex)
            {
                SetFloorColor(y, passedColor);     // 이미 지난 층
            }
            else if (y == currentFloorIndex)
            {
                SetFloorColor(y, currentColor);    // 현재 층
            }
            else
            {
                SetFloorColor(y, unreachedColor);  // 아직 도달 X
            }
        }
    }
}
