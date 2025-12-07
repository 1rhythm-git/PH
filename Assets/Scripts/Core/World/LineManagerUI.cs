using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LineManagerUI : MonoBehaviour
{
    [Header("References")]
    public RectTransform linePanel;      // LinePanel
    public Image linePrefab;             // LineColumn (세로 라인 템플릿)
    public BuildingGridUI buildingGridUI;

    private Image[] columnLines;

    private IEnumerator Start()
    {
        if (linePanel == null)
            linePanel = GetComponent<RectTransform>();

        if (buildingGridUI == null)
            buildingGridUI = Object.FindFirstObjectByType<BuildingGridUI>();

        // 레이아웃이 모두 끝난 뒤 셀 위치를 읽기 위해 한 프레임 대기
        yield return new WaitForEndOfFrame();

        GenerateLinesFromGrid();
    }

    private void GenerateLinesFromGrid()
    {
        if (linePanel == null || linePrefab == null || buildingGridUI == null)
        {
            Debug.LogError("LineManagerUI: linePanel / linePrefab / buildingGridUI 중 하나가 비어 있습니다.");
            return;
        }

        int columns = buildingGridUI.columns;
        int rows = buildingGridUI.rows;

        // 이전 라인 제거 (템플릿은 유지)
        if (columnLines != null)
        {
            for (int i = 0; i < columnLines.Length; i++)
            {
                if (columnLines[i] != null)
                    Destroy(columnLines[i].gameObject);
            }
        }

        // ★ 칸 8개 → 경계 9개 (0~8)
        columnLines = new Image[columns + 1];

        if (linePrefab.gameObject.activeSelf)
            linePrefab.gameObject.SetActive(false);

        // 세로 길이 계산용 : 맨 아래 (0층,0열) 셀 vs 맨 위 (마지막층,0열) 셀
        RectTransform bottomCell = buildingGridUI.GetCellRectTransform(0, 0);
        RectTransform topCell = buildingGridUI.GetCellRectTransform(0, rows - 1);

        if (bottomCell == null || topCell == null)
        {
            Debug.LogError("LineManagerUI: 기준 셀을 찾을 수 없습니다.");
            return;
        }

        var corners = new Vector3[4];

        // 아래/위 Y 좌표 (라인 높이를 셀 전체 영역에 딱 맞게 하기 위함)
        bottomCell.GetWorldCorners(corners); // 0=BL,1=TL,2=TR,3=BR
        Vector3 bottomWorld = corners[0];    // bottom-left

        topCell.GetWorldCorners(corners);
        Vector3 topWorld = corners[1];       // top-left

        Vector2 bottomLocal = linePanel.InverseTransformPoint(bottomWorld);
        Vector2 topLocal = linePanel.InverseTransformPoint(topWorld);

        float minY = bottomLocal.y;
        float maxY = topLocal.y;
        float height = maxY - minY;
        float centerY = (minY + maxY) * 0.5f;

        // ★ 0~columns까지 경계 생성
        for (int i = 0; i <= columns; i++)
        {
            float worldX;

            if (i == 0)
            {
                // 가장 왼쪽 벽: (0,0) 셀의 left edge
                RectTransform cell = buildingGridUI.GetCellRectTransform(0, 0);
                cell.GetWorldCorners(corners);
                worldX = corners[0].x; // bottom-left.x
            }
            else if (i == columns)
            {
                // 가장 오른쪽 벽: (columns-1,0) 셀의 right edge
                RectTransform cell = buildingGridUI.GetCellRectTransform(columns - 1, 0);
                cell.GetWorldCorners(corners);
                worldX = corners[2].x; // top-right.x
            }
            else
            {
                // 중간 경계: (i-1,0) 셀의 right edge
                RectTransform cell = buildingGridUI.GetCellRectTransform(i - 1, 0);
                cell.GetWorldCorners(corners);
                worldX = corners[2].x; // top-right.x
            }

            Vector2 localPos = linePanel.InverseTransformPoint(new Vector3(worldX, 0f, 0f));

            Image line = Instantiate(linePrefab, linePanel);
            line.gameObject.SetActive(true);
            line.name = $"Line_{i}";

            RectTransform rt = line.rectTransform;

            // 패널 중앙 기준 로컬 좌표로 위치/길이 설정
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = new Vector2(localPos.x, centerY);
            // x = 두께, y = 전체 높이
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);

            columnLines[i] = line;
        }
    }

    // 적이 있는 열만 켜거나 끄는 함수
    public void SetLineVisible(int columnIndex, bool visible, float alpha = 1f)
    {
        if (columnLines == null) return;
        if (columnIndex < 0 || columnIndex >= columnLines.Length) return;
        if (columnLines[columnIndex] == null) return;

        Color c = columnLines[columnIndex].color;
        c.a = visible ? alpha : 0f;
        columnLines[columnIndex].color = c;
    }
}
