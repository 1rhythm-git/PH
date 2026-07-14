using System.Collections.Generic;
using PH.Core.Player;
using PH.Core.World;
using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.Items
{
    public sealed class ItemSpawner : MonoBehaviour
    {
        [SerializeField]
        private TextAsset itemTableCsv;

        [SerializeField]
        private TextAsset itemIconTableCsv;

        [SerializeField]
        private BuildingGridUI buildingGridUI;

        [SerializeField]
        private InfiniteFloorManager floorManager;

        [SerializeField]
        private PlayerSpawner playerSpawner;

        [SerializeField]
        private RunItemEventRecorder eventRecorder;

        [SerializeField]
        private RectTransform itemLayer;

        [SerializeField]
        private bool spawnOnStart = true;

        [SerializeField]
        private int maxItemsPerPage = 6;

        [SerializeField]
        private int randomSeed = 30415;

        [SerializeField]
        private string forcedTestItemId;

        [SerializeField]
        private Vector2 itemSize = new Vector2(64f, 64f);

        [SerializeField]
        private int passCountFontSize = 32;

        [SerializeField]
        private Color defaultItemColor = new Color(1f, 0.08f, 0.06f, 0.95f);

        private readonly List<ItemInstance> spawnedItems = new List<ItemInstance>();
        private ItemTable itemTable;
        private ItemIconTable itemIconTable;
        private IItemIconProvider iconProvider;
        private PlayerMotor playerMotor;
        private int lastSpawnedPageIndex = int.MinValue;

        public ItemIconTable IconTable => itemIconTable;

        private void Awake()
        {
            EnsureReferences();
            ReloadTable();
            ReloadIconTable();
            EnsureItemLayer();
            EnsureIconProvider();
        }

        private void OnEnable()
        {
            if (floorManager != null)
            {
                floorManager.CurrentFloorChanged += HandleCurrentFloorChanged;
            }
        }

        private void Start()
        {
            TryResolvePlayerMotor();

            if (spawnOnStart)
            {
                SpawnCurrentPageItems();
            }
        }

        private void OnDisable()
        {
            if (floorManager != null)
            {
                floorManager.CurrentFloorChanged -= HandleCurrentFloorChanged;
            }
        }

        [ContextMenu("Debug/Reload Item Table")]
        public void ReloadTable()
        {
            itemTable = ItemTable.Load(itemTableCsv);
        }

        [ContextMenu("Debug/Reload Item Icon Table")]
        public void ReloadIconTable()
        {
            itemIconTable = ItemIconTable.Load(itemIconTableCsv);
        }

        [ContextMenu("Debug/Spawn Current Page Items")]
        public void SpawnCurrentPageItems()
        {
            EnsureReferences();
            EnsureItemLayer();
            EnsureIconProvider();
            TryResolvePlayerMotor();

            if (buildingGridUI == null || floorManager == null || itemLayer == null || itemTable == null)
            {
                return;
            }

            ClearSpawnedItems();

            FloorPageData pageData = buildingGridUI.CurrentPageData;
            if (pageData == null)
            {
                return;
            }

            System.Random random = new System.Random(randomSeed + floorManager.CurrentPageIndex);
            HashSet<int> occupied = new HashSet<int>();
            int spawnedCount = 0;

            for (int guard = 0; guard < buildingGridUI.Rows * buildingGridUI.Columns && spawnedCount < maxItemsPerPage; guard++)
            {
                int row = random.Next(0, buildingGridUI.Rows);
                int column = random.Next(0, buildingGridUI.Columns);
                int key = row * buildingGridUI.Columns + column;

                if (occupied.Contains(key) || IsExcludedCell(column, row))
                {
                    continue;
                }

                FloorAddress address = pageData.GetAddressByRow(row);
                ItemDefinition definition = PickItem(address.AbsoluteFloor, random);
                if (definition == null)
                {
                    continue;
                }

                CreateItem(definition, address, column);
                occupied.Add(key);
                spawnedCount++;
            }

            lastSpawnedPageIndex = floorManager.CurrentPageIndex;
        }

        [ContextMenu("Debug/Clear Items")]
        public void ClearSpawnedItems()
        {
            for (int i = spawnedItems.Count - 1; i >= 0; i--)
            {
                ItemInstance item = spawnedItems[i];
                if (item == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(item.gameObject);
                }
                else
                {
                    DestroyImmediate(item.gameObject);
                }
            }

            spawnedItems.Clear();
        }

        private void HandleCurrentFloorChanged(int currentAbsoluteFloor)
        {
            if (floorManager == null)
            {
                return;
            }

            if (floorManager.CurrentPageIndex != lastSpawnedPageIndex)
            {
                SpawnCurrentPageItems();
            }
        }

        private ItemDefinition PickItem(int absoluteFloor, System.Random random)
        {
            if (!string.IsNullOrWhiteSpace(forcedTestItemId) && itemTable.TryGet(forcedTestItemId, out ItemDefinition forcedItem))
            {
                return forcedItem.CanSpawnAtFloor(absoluteFloor) ? forcedItem : null;
            }

            List<ItemDefinition> candidates = itemTable.GetSpawnCandidates(absoluteFloor);
            int totalWeight = 0;

            for (int i = 0; i < candidates.Count; i++)
            {
                totalWeight += Mathf.Max(0, candidates[i].SpawnWeight);
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int roll = random.Next(0, totalWeight);
            int cursor = 0;

            for (int i = 0; i < candidates.Count; i++)
            {
                cursor += Mathf.Max(0, candidates[i].SpawnWeight);
                if (roll < cursor)
                {
                    return candidates[i];
                }
            }

            return candidates[candidates.Count - 1];
        }

        private void CreateItem(ItemDefinition definition, FloorAddress address, int column)
        {
            GameObject itemObject = new GameObject($"Item_{definition.ItemId}_{address.AbsoluteFloor}_{column}", typeof(RectTransform), typeof(Image), typeof(ItemInstance));
            itemObject.layer = itemLayer.gameObject.layer;
            itemObject.transform.SetParent(itemLayer, false);

            RectTransform itemRect = itemObject.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemRect.pivot = new Vector2(0.5f, 0.5f);
            itemRect.localScale = Vector3.one;
            itemRect.sizeDelta = itemSize;
            itemRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemSize.x);
            itemRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemSize.y);
            itemRect.anchoredPosition = GetItemAnchoredPosition(column, address.PageFloorIndex);

            Image image = itemObject.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = false;

            CreateIconImage(itemObject.transform, definition);
            CreateProgressText(itemObject.transform);

            ItemInstance item = itemObject.GetComponent<ItemInstance>();
            item.Configure(definition, floorManager, playerMotor, eventRecorder, address.AbsoluteFloor, address.PageIndex, address.PageFloorIndex, column, new Color(1f, 1f, 1f, 0f));
            spawnedItems.Add(item);
        }

        private void CreateIconImage(Transform parent, ItemDefinition definition)
        {
            GameObject shapeObject = new GameObject("IconImage", typeof(RectTransform), typeof(Image), typeof(Outline));
            shapeObject.layer = parent.gameObject.layer;
            shapeObject.transform.SetParent(parent, false);

            RectTransform shapeRect = shapeObject.GetComponent<RectTransform>();
            shapeRect.anchorMin = Vector2.zero;
            shapeRect.anchorMax = Vector2.one;
            shapeRect.offsetMin = Vector2.zero;
            shapeRect.offsetMax = Vector2.zero;

            Image image = shapeObject.GetComponent<Image>();
            ItemIconData iconData = iconProvider.GetIcon(definition);
            image.sprite = iconData.Sprite;
            image.color = iconData.Color;
            image.preserveAspect = true;
            image.raycastTarget = false;

            Outline outline = shapeObject.GetComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.65f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;
        }

        private void CreateProgressText(Transform parent)
        {
            GameObject textObject = new GameObject("ProgressText", typeof(RectTransform), typeof(Text));
            textObject.layer = parent.gameObject.layer;
            textObject.transform.SetParent(parent, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObject.GetComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.fontSize = passCountFontSize;
            text.fontStyle = FontStyle.Bold;
            text.raycastTarget = false;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private Vector2 GetItemAnchoredPosition(int column, int row)
        {
            RectTransform cellRect = buildingGridUI.GetCellRectTransform(column, row);
            if (cellRect == null)
            {
                Rect layerRect = itemLayer.rect;
                float normalizedX = (column + 0.5f) / Mathf.Max(1, buildingGridUI.Columns);
                float x = Mathf.Lerp(layerRect.xMin, layerRect.xMax, normalizedX);
                float rowHeight = layerRect.height / Mathf.Max(1, buildingGridUI.Rows);
                float floorLineY = layerRect.yMin + rowHeight * Mathf.Clamp(row, 0, Mathf.Max(0, buildingGridUI.Rows - 1));

                return new Vector2(x, floorLineY + itemSize.y * 0.5f);
            }

            Vector3[] corners = new Vector3[4];
            cellRect.GetWorldCorners(corners);

            Vector3 bottomCenterWorld = Vector3.Lerp(corners[0], corners[3], 0.5f);
            Vector2 bottomCenterLocal = itemLayer.InverseTransformPoint(bottomCenterWorld);

            return new Vector2(bottomCenterLocal.x, bottomCenterLocal.y + itemSize.y * 0.5f);
        }

        private bool IsExcludedCell(int column, int row)
        {
            if (floorManager == null || buildingGridUI == null)
            {
                return true;
            }

            int maxColumn = Mathf.Max(0, buildingGridUI.Columns - 1);

            return column <= 0 || column >= maxColumn;
        }

        private void EnsureReferences()
        {
            if (buildingGridUI == null)
            {
                buildingGridUI = FindFirstObjectByType<BuildingGridUI>();
            }

            if (floorManager == null)
            {
                floorManager = FindFirstObjectByType<InfiniteFloorManager>();
            }

            if (playerSpawner == null)
            {
                playerSpawner = FindFirstObjectByType<PlayerSpawner>();
            }

            if (eventRecorder == null)
            {
                eventRecorder = FindFirstObjectByType<RunItemEventRecorder>();
            }
        }

        private void EnsureIconProvider()
        {
            if (itemIconTable == null)
            {
                ReloadIconTable();
            }

            if (iconProvider == null)
            {
                iconProvider = new FallbackItemIconProvider(defaultItemColor);
            }
        }

        private void EnsureItemLayer()
        {
            if (itemLayer != null || buildingGridUI == null)
            {
                return;
            }

            GameObject layerObject = new GameObject("ItemLayer", typeof(RectTransform));
            layerObject.layer = buildingGridUI.gameObject.layer;
            layerObject.transform.SetParent(buildingGridUI.transform.parent, false);
            itemLayer = layerObject.GetComponent<RectTransform>();
            itemLayer.anchorMin = Vector2.zero;
            itemLayer.anchorMax = Vector2.one;
            itemLayer.offsetMin = Vector2.zero;
            itemLayer.offsetMax = Vector2.zero;
            itemLayer.pivot = new Vector2(0.5f, 0.5f);
            itemLayer.SetSiblingIndex(buildingGridUI.transform.GetSiblingIndex() + 1);
        }

        private void TryResolvePlayerMotor()
        {
            if (playerMotor != null)
            {
                return;
            }

            if (playerSpawner != null && playerSpawner.SpawnedPlayer != null)
            {
                playerMotor = playerSpawner.SpawnedPlayer.GetComponent<PlayerMotor>();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            maxItemsPerPage = Mathf.Max(0, maxItemsPerPage);
            passCountFontSize = Mathf.Max(1, passCountFontSize);
            itemSize.x = Mathf.Max(1f, itemSize.x);
            itemSize.y = Mathf.Max(1f, itemSize.y);
        }
#endif
    }
}
