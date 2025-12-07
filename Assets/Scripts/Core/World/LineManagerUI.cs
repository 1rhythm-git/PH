using UnityEngine;
using UnityEngine.UI;

public class LineManagerUI : MonoBehaviour
{
    public int columns = 8;
    public RectTransform linePanel; // LinePanel
    public Image linePrefab;        // LineColumn 이미지

    private Image[] columnLines;

    private void Awake()
    {
        GenerateLines();
    }

    private void GenerateLines()
    {
        if (linePanel == null || linePrefab == null)
        {
            Debug.LogError("LineManagerUI: linePanel 또는 linePrefab이 비어 있습니다.");
            return;
        }

        // 기존 자식 삭제
        for (int i = linePanel.childCount - 1; i >= 0; i--)
        {
            Destroy(linePanel.GetChild(i).gameObject);
        }

        columnLines = new Image[columns];

        float panelWidth = linePanel.rect.width;
        float cellWidth = panelWidth / columns;

        for (int x = 0; x < columns; x++)
        {
            Image line = Instantiate(linePrefab, linePanel);
            line.name = $"Line_{x}";

            RectTransform rt = line.rectTransform;
            float posX = (cellWidth * x) + cellWidth * 0.5f - (panelWidth * 0.5f);
            rt.anchoredPosition = new Vector2(posX, 0f);

            Color c = line.color;
            c.a = 0f; // 처음엔 안 보이게
            line.color = c;

            columnLines[x] = line;
        }
    }

    // 적이 있는 열만 켜거나 끌 때 사용할 함수
    public void SetLineVisible(int columnIndex, bool visible, float alpha = 1f)
    {
        if (columnIndex < 0 || columnIndex >= columns) return;
        if (columnLines == null || columnLines[columnIndex] == null) return;

        Color c = columnLines[columnIndex].color;
        c.a = visible ? alpha : 0f;
        columnLines[columnIndex].color = c;
    }
}
