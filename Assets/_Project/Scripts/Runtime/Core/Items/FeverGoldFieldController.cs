using System.Collections;
using System.Collections.Generic;
using LootUp.Core.Audio;
using LootUp.Core.Characters;
using LootUp.Core.Player;
using LootUp.Core.UI;
using LootUp.Core.World;
using UnityEngine;
using UnityEngine.UI;

namespace LootUp.Core.Items
{
    public sealed class FeverGoldFieldController : MonoBehaviour
    {
        private const string FeverLayerName = "FeverGoldLayer";
        private const string GoldIconResourcePath = "Items/Icons/score_gold_bar";

        [SerializeField]
        private int scorePerGoldBar = 100;

        [SerializeField]
        private Vector2 goldBarSize = new Vector2(76.8f, 76.8f);

        private readonly List<FeverGoldInstance> activeGoldBars = new List<FeverGoldInstance>();
        private readonly Stack<FeverGoldInstance> pooledGoldBars = new Stack<FeverGoldInstance>();
        private readonly HashSet<long> consumedCellKeys = new HashSet<long>();
        private PlayerCharacterRuntime characterRuntime;
        private BuildingGridUI buildingGridUI;
        private InfiniteFloorManager floorManager;
        private ItemSpawner itemSpawner;
        private PlayerMotor playerMotor;
        private TopHUDController topHUDController;
        private RectTransform feverLayer;
        private Sprite goldBarSprite;
        private Coroutine presentationCoroutine;
        private int displayedPageIndex = int.MinValue;

        public void Configure(
            PlayerCharacterRuntime runtime,
            BuildingGridUI grid,
            InfiniteFloorManager manager,
            ItemSpawner regularItemSpawner,
            PlayerMotor motor,
            TopHUDController hudController)
        {
            Unsubscribe();

            characterRuntime = runtime;
            buildingGridUI = grid;
            floorManager = manager;
            itemSpawner = regularItemSpawner;
            playerMotor = motor;
            topHUDController = hudController;

            EnsureFeverLayer();
            Subscribe();

            if (characterRuntime != null && characterRuntime.IsFeverActive)
            {
                HandleFeverStarted(characterRuntime.FeverDurationSeconds);
            }
            else
            {
                ReleaseAllGoldBars();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
            ReleaseAllGoldBars();
        }

        private void Subscribe()
        {
            if (characterRuntime != null)
            {
                characterRuntime.FeverStarted += HandleFeverStarted;
                characterRuntime.FeverEnded += HandleFeverEnded;
            }

            if (floorManager != null)
            {
                floorManager.CurrentFloorChanged += HandleCurrentFloorChanged;
            }

            if (itemSpawner != null)
            {
                itemSpawner.CurrentPageItemsSpawned += HandleCurrentPageItemsSpawned;
                itemSpawner.CurrentPageItemOccupancyChanged += HandleCurrentPageItemOccupancyChanged;
            }
        }

        private void Unsubscribe()
        {
            if (characterRuntime != null)
            {
                characterRuntime.FeverStarted -= HandleFeverStarted;
                characterRuntime.FeverEnded -= HandleFeverEnded;
            }

            if (floorManager != null)
            {
                floorManager.CurrentFloorChanged -= HandleCurrentFloorChanged;
            }

            if (itemSpawner != null)
            {
                itemSpawner.CurrentPageItemsSpawned -= HandleCurrentPageItemsSpawned;
                itemSpawner.CurrentPageItemOccupancyChanged -= HandleCurrentPageItemOccupancyChanged;
            }
        }

        private void HandleFeverStarted(float durationSeconds)
        {
            consumedCellKeys.Clear();
            displayedPageIndex = floorManager != null ? floorManager.CurrentPageIndex : int.MinValue;
            RebuildCurrentPage();
            PlayFeverStartPresentation();
        }

        private void HandleFeverEnded()
        {
            displayedPageIndex = int.MinValue;
            ReleaseAllGoldBars();
        }

        private void HandleCurrentFloorChanged(int absoluteFloor)
        {
            if (characterRuntime == null || !characterRuntime.IsFeverActive || floorManager == null)
            {
                return;
            }

            if (displayedPageIndex != floorManager.CurrentPageIndex)
            {
                displayedPageIndex = floorManager.CurrentPageIndex;
                RebuildCurrentPage();
            }

            feverLayer?.SetAsLastSibling();
        }

        private void HandleCurrentPageItemsSpawned(int pageIndex)
        {
            if (characterRuntime == null
                || !characterRuntime.IsFeverActive
                || floorManager == null
                || pageIndex != floorManager.CurrentPageIndex)
            {
                return;
            }

            displayedPageIndex = pageIndex;
            RebuildCurrentPage();
        }

        private void HandleCurrentPageItemOccupancyChanged()
        {
            if (characterRuntime == null || !characterRuntime.IsFeverActive)
            {
                return;
            }

            RebuildCurrentPage();
        }

        private void RebuildCurrentPage()
        {
            ReleaseAllGoldBars();
            EnsureFeverLayer();

            if (feverLayer == null
                || buildingGridUI == null
                || floorManager == null
                || playerMotor == null
                || buildingGridUI.CurrentPageData == null)
            {
                return;
            }

            feverLayer.SetAsLastSibling();
            FloorPageData pageData = buildingGridUI.CurrentPageData;

            for (int row = 0; row < buildingGridUI.Rows; row++)
            {
                FloorAddress address = pageData.GetAddressByRow(row);
                for (int column = 0; column < buildingGridUI.Columns; column++)
                {
                    if ((itemSpawner != null && itemSpawner.IsCurrentPageCellOccupied(column, row))
                        || consumedCellKeys.Contains(GetCellKey(address.AbsoluteFloor, column)))
                    {
                        continue;
                    }

                    SpawnGoldBar(address, column);
                }
            }
        }

        private void SpawnGoldBar(FloorAddress address, int column)
        {
            FeverGoldInstance goldBar = pooledGoldBars.Count > 0
                ? pooledGoldBars.Pop()
                : CreateGoldBar();

            RectTransform goldRect = goldBar.GetComponent<RectTransform>();
            goldRect.SetParent(feverLayer, false);
            goldRect.sizeDelta = goldBarSize;
            goldRect.anchoredPosition = GetGoldAnchoredPosition(column, address.PageFloorIndex);
            goldRect.SetAsLastSibling();

            goldBar.Configure(this, floorManager, playerMotor, address.AbsoluteFloor, column);
            activeGoldBars.Add(goldBar);
        }

        private FeverGoldInstance CreateGoldBar()
        {
            GameObject goldObject = new GameObject(
                "FeverGold",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Outline),
                typeof(FeverGoldInstance));
            goldObject.layer = feverLayer.gameObject.layer;
            goldObject.transform.SetParent(feverLayer, false);

            RectTransform goldRect = goldObject.GetComponent<RectTransform>();
            goldRect.anchorMin = new Vector2(0.5f, 0.5f);
            goldRect.anchorMax = new Vector2(0.5f, 0.5f);
            goldRect.pivot = new Vector2(0.5f, 0.5f);
            goldRect.localScale = Vector3.one;
            goldRect.sizeDelta = goldBarSize;

            Image image = goldObject.GetComponent<Image>();
            image.sprite = goldBarSprite != null ? goldBarSprite : goldBarSprite = Resources.Load<Sprite>(GoldIconResourcePath);
            image.color = image.sprite != null ? Color.white : new Color(1f, 0.78f, 0.12f, 1f);
            image.preserveAspect = true;
            image.raycastTarget = false;

            Outline outline = goldObject.GetComponent<Outline>();
            outline.effectColor = new Color(1f, 0.92f, 0.45f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            return goldObject.GetComponent<FeverGoldInstance>();
        }

        public void Collect(FeverGoldInstance goldBar)
        {
            if (goldBar == null || characterRuntime == null || !characterRuntime.IsFeverActive)
            {
                return;
            }

            consumedCellKeys.Add(GetCellKey(goldBar.AbsoluteFloor, goldBar.ColumnIndex));
            topHUDController?.AddScore(Mathf.Max(0, scorePerGoldBar));
            topHUDController?.SetItemStatus($"+{Mathf.Max(0, scorePerGoldBar)} FEVER SCORE");
            GameSfxPlayer.Play(GameSfxId.ItemGain);

            PlayerItemPickupFeedback pickupFeedback = playerMotor != null
                ? playerMotor.GetComponent<PlayerItemPickupFeedback>()
                : null;
            pickupFeedback?.Show($"+{Mathf.Max(0, scorePerGoldBar)} SCORE", new Color(1f, 0.86f, 0.18f, 1f), 1f);

            ReleaseGoldBar(goldBar);
        }

        private void ReleaseAllGoldBars()
        {
            for (int i = activeGoldBars.Count - 1; i >= 0; i--)
            {
                FeverGoldInstance goldBar = activeGoldBars[i];
                if (goldBar == null)
                {
                    continue;
                }

                goldBar.gameObject.SetActive(false);
                pooledGoldBars.Push(goldBar);
            }

            activeGoldBars.Clear();
        }

        private void ReleaseGoldBar(FeverGoldInstance goldBar)
        {
            if (!activeGoldBars.Remove(goldBar))
            {
                return;
            }

            goldBar.gameObject.SetActive(false);
            pooledGoldBars.Push(goldBar);
        }

        private Vector2 GetGoldAnchoredPosition(int column, int row)
        {
            RectTransform cellRect = buildingGridUI.GetCellRectTransform(column, row);
            if (cellRect == null)
            {
                return Vector2.zero;
            }

            Vector3[] corners = new Vector3[4];
            cellRect.GetWorldCorners(corners);
            Vector3 bottomCenterWorld = Vector3.Lerp(corners[0], corners[3], 0.5f);
            Vector2 bottomCenterLocal = feverLayer.InverseTransformPoint(bottomCenterWorld);
            return new Vector2(bottomCenterLocal.x, bottomCenterLocal.y + goldBarSize.y * 0.5f);
        }

        private void EnsureFeverLayer()
        {
            if (feverLayer != null || buildingGridUI == null || buildingGridUI.transform.parent == null)
            {
                return;
            }

            Transform existing = buildingGridUI.transform.parent.Find(FeverLayerName);
            if (existing != null)
            {
                feverLayer = existing as RectTransform;
            }
            else
            {
                GameObject layerObject = new GameObject(FeverLayerName, typeof(RectTransform));
                layerObject.layer = buildingGridUI.gameObject.layer;
                layerObject.transform.SetParent(buildingGridUI.transform.parent, false);
                feverLayer = layerObject.GetComponent<RectTransform>();
                feverLayer.anchorMin = Vector2.zero;
                feverLayer.anchorMax = Vector2.one;
                feverLayer.offsetMin = Vector2.zero;
                feverLayer.offsetMax = Vector2.zero;
                feverLayer.pivot = new Vector2(0.5f, 0.5f);
            }

            feverLayer.SetAsLastSibling();
        }

        private void PlayFeverStartPresentation()
        {
            if (presentationCoroutine != null)
            {
                StopCoroutine(presentationCoroutine);
            }

            presentationCoroutine = StartCoroutine(PlayFeverStartPresentationRoutine());
        }

        private IEnumerator PlayFeverStartPresentationRoutine()
        {
            GameObject flashObject = new GameObject("FeverFlash", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            flashObject.layer = feverLayer.gameObject.layer;
            flashObject.transform.SetParent(feverLayer, false);

            RectTransform flashRect = flashObject.GetComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.offsetMin = Vector2.zero;
            flashRect.offsetMax = Vector2.zero;

            Image flashImage = flashObject.GetComponent<Image>();
            flashImage.color = new Color(1f, 0.78f, 0.12f, 0.18f);
            flashImage.raycastTarget = false;

            GameObject textObject = new GameObject("FeverStartText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
            textObject.layer = feverLayer.gameObject.layer;
            textObject.transform.SetParent(feverLayer, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.42f);
            textRect.anchorMax = new Vector2(0.9f, 0.58f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObject.GetComponent<Text>();
            text.text = "FEVER TIME!";
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 72;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(1f, 0.9f, 0.28f, 1f);
            text.raycastTarget = false;

            Outline outline = textObject.GetComponent<Outline>();
            outline.effectColor = new Color(0.12f, 0.05f, 0f, 0.95f);
            outline.effectDistance = new Vector2(4f, -4f);
            outline.useGraphicAlpha = true;

            float elapsed = 0f;
            const float duration = 0.8f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float alpha = 1f - normalized;
                text.color = new Color(1f, 0.9f, 0.28f, alpha);
                flashImage.color = new Color(1f, 0.78f, 0.12f, 0.18f * alpha);
                textRect.localScale = Vector3.one * Mathf.Lerp(0.82f, 1.08f, normalized);
                yield return null;
            }

            Destroy(flashObject);
            Destroy(textObject);
            presentationCoroutine = null;
        }

        private static long GetCellKey(int absoluteFloor, int column)
        {
            return ((long)absoluteFloor << 32) | (uint)column;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            scorePerGoldBar = Mathf.Max(0, scorePerGoldBar);
            goldBarSize.x = Mathf.Max(1f, goldBarSize.x);
            goldBarSize.y = Mathf.Max(1f, goldBarSize.y);
        }
#endif
    }
}
