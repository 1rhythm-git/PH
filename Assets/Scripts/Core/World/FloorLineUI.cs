using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FloorLineUI : MonoBehaviour
{
    [Header("References")]
    public RectTransform linePanel;        // FloorLinePanel
    public Image linePrefab;               // FloorLine (가로 라인 템플릿)
    public BuildingGridUI buildingGridUI;

    private Image[] floorLines;

    private IEnumerator Start()
    {
        if (linePanel == null)
            linePanel = GetComponent<RectTransform>();

        // 기존: buildingGridUI = FindObjectOfType<BuildingGridUI>();
        buildingGridUI = Object.FindFirstObjectByType<BuildingGridUI>();

        // 레이아웃 완료 후 셀 위치를 읽기 위해 한 프레임 대기
        yield return new WaitForEndOfFrame();

        GenerateLinesFromGrid();
    }

    private void GenerateLinesFromGrid()
    {
        if (linePanel == null || linePrefab == null || buildingGridUI == null)
        {
            Debug.LogError("FloorLineUI: linePanel / linePrefab / buildingGridUI 중 하나가 비어 있습니다.");
            return;
        }

        int columns = buildingGridUI.columns;
        int rows = buildingGridUI.rows;

        // 이전 라인 제거
        if (floorLines != null)
        {
            for (int i = 0; i < floorLines.Length; i++)
            {
                if (floorLines[i] != null)
                    Destroy(floorLines[i].gameObject);
            }
        }

        // ★ 층 10개 → 경계 11개 (0~10)
        floorLines = new Image[rows + 1];

        if (linePrefab.gameObject.activeSelf)
            linePrefab.gameObject.SetActive(false);

        var corners = new Vector3[4];

        // 좌/우 X 범위 계산용 : (0,0) 셀과 (마지막열,0) 셀
        RectTransform leftCell = buildingGridUI.GetCellRectTransform(0, 0);
        RectTransform rightCell = buildingGridUI.GetCellRectTransform(columns - 1, 0);

        if (leftCell == null || rightCell == null)
        {
            Debug.LogError("FloorLineUI: 기준 셀을 찾을 수 없습니다.");
            return;
        }

        leftCell.GetWorldCorners(corners);
        Vector3 leftWorld = corners[0];   // bottom-left

        rightCell.GetWorldCorners(corners);
        Vector3 rightWorld = corners[2];   // top-right

        Vector2 leftLocal = linePanel.InverseTransformPoint(leftWorld);
        Vector2 rightLocal = linePanel.InverseTransformPoint(rightWorld);

        float minX = leftLocal.x;
        float maxX = rightLocal.x;
        float width = maxX - minX;
        float centerX = (minX + maxX) * 0.5f;

        // 맨 아래/위 Y 기준 : (0,0) 셀과 (0,rows-1) 셀
        RectTransform bottomCell = buildingGridUI.GetCellRectTransform(0, 0);
        RectTransform topCell = buildingGridUI.GetCellRectTransform(0, rows - 1);

        bottomCell.GetWorldCorners(corners);
        Vector3 bottomWorld = corners[0];  // bottom-left

        topCell.GetWorldCorners(corners);
        Vector3 topWorld = corners[1];     // top-left

        Vector2 bottomLocal = linePanel.InverseTransformPoint(bottomWorld);
        Vector2 topLocal = linePanel.InverseTransformPoint(topWorld);

        float minY = bottomLocal.y;
        float maxY = topLocal.y;
        float stepY = (maxY - minY) / rows;  // 층 간격

        for (int i = 0; i <= rows; i++)
        {
            Image line = Instantiate(linePrefab, linePanel);
            line.gameObject.SetActive(true);
            line.name = $"FloorLine_{i}";

            RectTransform rt = line.rectTransform;

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            float posY = minY + stepY * i;

            rt.anchoredPosition = new Vector2(centerX, posY);
            // x = 전체 너비, y = 두께
            rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);

            floorLines[i] = line;
        }
    }
}
