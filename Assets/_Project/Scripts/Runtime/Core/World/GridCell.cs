using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.World
{
    public sealed class GridCell : MonoBehaviour
    {
        [SerializeField]
        private int column;

        [SerializeField]
        private int row;

        [SerializeField]
        private int absoluteFloor;

        [SerializeField]
        private int pageIndex;

        [SerializeField]
        private int pageFloorIndex;

        [SerializeField]
        private Image backgroundImage;

        [SerializeField]
        private Text floorLabel;

        public int Column => column;
        public int Row => row;
        public int AbsoluteFloor => absoluteFloor;
        public int PageIndex => pageIndex;
        public int PageFloorIndex => pageFloorIndex;
        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            CacheComponents();
        }

        public void Configure(int columnIndex, int rowIndex, FloorAddress floorAddress, bool showFloorLabel, Color backgroundColor)
        {
            CacheComponents();

            column = columnIndex;
            row = rowIndex;
            absoluteFloor = floorAddress.AbsoluteFloor;
            pageIndex = floorAddress.PageIndex;
            pageFloorIndex = floorAddress.PageFloorIndex;
            name = $"Cell_{column}_{row}";

            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }

            if (floorLabel != null)
            {
                floorLabel.gameObject.SetActive(showFloorLabel);
                floorLabel.text = showFloorLabel ? absoluteFloor.ToString() : string.Empty;
            }
        }

        public void SetBackgroundColor(Color color)
        {
            CacheComponents();

            if (backgroundImage != null)
            {
                backgroundImage.color = color;
            }
        }

        private void CacheComponents()
        {
            if (RectTransform == null)
            {
                RectTransform = transform as RectTransform;
            }

            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
            }

            if (floorLabel == null)
            {
                floorLabel = GetComponentInChildren<Text>(true);
            }
        }
    }
}
