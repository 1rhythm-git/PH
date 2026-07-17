using UnityEngine;

namespace PH.Core.UI
{
    [DisallowMultipleComponent]
    public sealed class TopSafeAreaController : MonoBehaviour
    {
        private const string DefaultContentRootName = "TopHUDRuntimeRoot";

        [SerializeField]
        private RectTransform contentRoot;

        [SerializeField]
        private string contentRootName = DefaultContentRootName;

        [SerializeField]
        private bool useSafeArea = true;

        [SerializeField]
        [Min(0f)]
        private float additionalTopPadding = 12f;

        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;
        private float lastAdditionalTopPadding = -1f;

        private void OnEnable()
        {
            ApplySafeArea(true);
        }

        private void Update()
        {
            ApplySafeArea(false);
        }

        private void OnDisable()
        {
            if (contentRoot != null)
            {
                contentRoot.offsetMin = Vector2.zero;
                contentRoot.offsetMax = Vector2.zero;
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplySafeArea(true);
        }

        private void ApplySafeArea(bool force)
        {
            if (!TryResolveContentRoot())
            {
                return;
            }

            Rect safeArea = useSafeArea ? Screen.safeArea : new Rect(0f, 0f, Screen.width, Screen.height);
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            float clampedAdditionalPadding = Mathf.Max(0f, additionalTopPadding);

            if (!force && safeArea == lastSafeArea && screenSize == lastScreenSize &&
                Mathf.Approximately(clampedAdditionalPadding, lastAdditionalTopPadding))
            {
                return;
            }

            lastSafeArea = safeArea;
            lastScreenSize = screenSize;
            lastAdditionalTopPadding = clampedAdditionalPadding;

            // (추가) 화면 상단과 Safe Area 상단의 차이를 TopUI 로컬 좌표로 변환한다.
            float safeTopInset = CalculateSafeTopInset(safeArea);
            float totalTopPadding = safeTopInset + clampedAdditionalPadding;

            contentRoot.offsetMin = Vector2.zero;
            contentRoot.offsetMax = new Vector2(0f, -totalTopPadding);
        }

        private bool TryResolveContentRoot()
        {
            if (contentRoot != null)
            {
                return true;
            }

            string targetName = string.IsNullOrWhiteSpace(contentRootName)
                ? DefaultContentRootName
                : contentRootName;
            contentRoot = transform.Find(targetName) as RectTransform;
            return contentRoot != null;
        }

        private float CalculateSafeTopInset(Rect safeArea)
        {
            if (!useSafeArea || Screen.height <= 0)
            {
                return 0f;
            }

            RectTransform parentRect = contentRoot.parent as RectTransform;
            Canvas canvas = GetComponentInParent<Canvas>();
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            float centerX = Screen.width * 0.5f;

            if (parentRect != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, new Vector2(centerX, Screen.height), uiCamera, out Vector2 screenTop) &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, new Vector2(centerX, safeArea.yMax), uiCamera, out Vector2 safeTop))
            {
                return Mathf.Max(0f, screenTop.y - safeTop.y);
            }

            RectTransform canvasRect = canvas != null ? canvas.transform as RectTransform : null;
            float referenceHeight = canvasRect != null ? canvasRect.rect.height : Screen.height;
            float topInsetPixels = Mathf.Max(0f, Screen.height - safeArea.yMax);
            return topInsetPixels * referenceHeight / Screen.height;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            additionalTopPadding = Mathf.Max(0f, additionalTopPadding);
            ApplySafeArea(true);
        }
#endif
    }
}
