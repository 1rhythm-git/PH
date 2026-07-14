using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.World
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class FloorLineUI : MonoBehaviour
    {
        [SerializeField]
        private BuildingGridUI buildingGridUI;

        [SerializeField]
        private int lineCount = BuildingGridUI.DefaultRows + 1;

        [SerializeField]
        private float lineThickness = 4f;

        [SerializeField]
        private Color lineColor = new Color(1f, 1f, 1f, 0.42f);

        [SerializeField]
        private bool usePixelSnapping = true;

        private RectTransform rectTransform;
        private RectTransform[] lines;
        private Canvas parentCanvas;

        private void Awake()
        {
            BuildLines();
        }

        private void LateUpdate()
        {
            ApplyLineLayout();
        }

        public void Rebuild()
        {
            BuildLines();
        }

        private void BuildLines()
        {
            EnsureComponents();

            if (buildingGridUI == null)
            {
                buildingGridUI = FindFirstObjectByType<BuildingGridUI>();
            }

            int targetLineCount = buildingGridUI != null ? buildingGridUI.Rows + 1 : lineCount;
            lineCount = Mathf.Max(2, targetLineCount);

            ClearChildren();
            lines = new RectTransform[lineCount];

            for (int i = 0; i < lineCount; i++)
            {
                GameObject lineObject = new GameObject($"FloorLine_{i}", typeof(RectTransform), typeof(Image));
                lineObject.layer = gameObject.layer;
                lineObject.transform.SetParent(transform, false);

                Image image = lineObject.GetComponent<Image>();
                image.color = lineColor;
                image.raycastTarget = false;

                lines[i] = lineObject.GetComponent<RectTransform>();
            }

            ApplyLineLayout();
        }

        private void ApplyLineLayout()
        {
            EnsureComponents();

            if (lines == null || lines.Length == 0)
            {
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                RectTransform line = lines[i];
                if (line == null)
                {
                    continue;
                }

                Rect parentRect = rectTransform.rect;
                float normalizedY = (float)i / (lines.Length - 1);
                float localY = Mathf.Lerp(parentRect.yMin, parentRect.yMax, normalizedY);

                if (usePixelSnapping)
                {
                    localY = SnapToCanvasPixel(localY);
                }

                line.anchorMin = new Vector2(0f, 0.5f);
                line.anchorMax = new Vector2(1f, 0.5f);
                line.anchoredPosition = new Vector2(0f, localY);
                line.sizeDelta = new Vector2(0f, lineThickness);
                line.pivot = new Vector2(0.5f, 0.5f);
            }
        }

        private float SnapToCanvasPixel(float localPosition)
        {
            float scaleFactor = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
            if (scaleFactor <= 0f)
            {
                return localPosition;
            }

            return Mathf.Round(localPosition * scaleFactor) / scaleFactor;
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

        private void EnsureComponents()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            lineCount = Mathf.Max(2, lineCount);
            lineThickness = Mathf.Max(1f, lineThickness);
        }
#endif
    }
}
