using LootUp.Core.Player;
using LootUp.Core.World;
using UnityEngine;
using UnityEngine.UI;

namespace LootUp.Core.Items
{
    public sealed class ItemViewFactory
    {
        private readonly BuildingGridUI buildingGridUI;
        private readonly InfiniteFloorManager floorManager;
        private readonly PlayerMotor playerMotor;
        private readonly RunItemEventRecorder eventRecorder;
        private readonly RectTransform itemLayer;
        private readonly RectTransform artifactLayer;
        private readonly IItemIconProvider iconProvider;
        private readonly Vector2 itemSize;
        private readonly int passCountFontSize;

        public ItemViewFactory(
            BuildingGridUI buildingGridUI,
            InfiniteFloorManager floorManager,
            PlayerMotor playerMotor,
            RunItemEventRecorder eventRecorder,
            RectTransform itemLayer,
            RectTransform artifactLayer,
            IItemIconProvider iconProvider,
            Vector2 itemSize,
            int passCountFontSize)
        {
            this.buildingGridUI = buildingGridUI;
            this.floorManager = floorManager;
            this.playerMotor = playerMotor;
            this.eventRecorder = eventRecorder;
            this.itemLayer = itemLayer;
            this.artifactLayer = artifactLayer;
            this.iconProvider = iconProvider;
            this.itemSize = itemSize;
            this.passCountFontSize = passCountFontSize;
        }

        // (추가) 계획된 아이템을 런타임 UI View로 생성한다.
        public ItemInstance Create(ItemSpawnPlan plan)
        {
            ItemDefinition definition = plan.Definition;
            FloorAddress address = plan.Address;
            GameObject itemObject = new GameObject(
                $"Item_{definition.ItemId}_{address.AbsoluteFloor}_{plan.Column}",
                typeof(RectTransform),
                typeof(Image),
                typeof(ItemInstance));
            RectTransform targetLayer =
                definition.CollectionItemType == CollectionItemType.Artifact
                    ? artifactLayer
                    : itemLayer;
            targetLayer ??= itemLayer;
            itemObject.layer = targetLayer.gameObject.layer;
            itemObject.transform.SetParent(targetLayer, false);

            RectTransform itemRect = itemObject.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemRect.pivot = new Vector2(0.5f, 0.5f);
            itemRect.localScale = Vector3.one;
            itemRect.sizeDelta = itemSize;
            itemRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemSize.x);
            itemRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemSize.y);
            itemRect.anchoredPosition =
                GetItemAnchoredPosition(plan.Column, address.PageFloorIndex);

            Image image = itemObject.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = false;

            CreateIconImage(itemObject.transform, definition);
            CreateProgressText(itemObject.transform);

            ItemInstance item = itemObject.GetComponent<ItemInstance>();
            item.Configure(
                definition,
                floorManager,
                playerMotor,
                eventRecorder,
                address.AbsoluteFloor,
                address.PageIndex,
                address.PageFloorIndex,
                plan.Column,
                new Color(1f, 1f, 1f, 0f),
                plan.RuntimePassCount,
                plan.ScoreBonusPercent);
            return item;
        }

        private void CreateIconImage(Transform parent, ItemDefinition definition)
        {
            GameObject shapeObject = new GameObject(
                "IconImage",
                typeof(RectTransform),
                typeof(Image),
                typeof(Outline));
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
            GameObject textObject = new GameObject(
                "ProgressText",
                typeof(RectTransform),
                typeof(Text),
                typeof(Outline),
                typeof(Shadow));
            textObject.layer = parent.gameObject.layer;
            textObject.transform.SetParent(parent, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObject.GetComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontSize = passCountFontSize;
            text.fontStyle = FontStyle.Bold;
            text.raycastTarget = false;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Outline outline = textObject.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
            outline.effectDistance = new Vector2(3f, -3f);
            outline.useGraphicAlpha = true;

            Shadow shadow = textObject.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(2f, -2f);
            shadow.useGraphicAlpha = true;
        }

        private Vector2 GetItemAnchoredPosition(int column, int row)
        {
            RectTransform cellRect = buildingGridUI.GetCellRectTransform(column, row);
            if (cellRect == null)
            {
                Rect layerRect = itemLayer.rect;
                float normalizedX =
                    (column + 0.5f) / Mathf.Max(1, buildingGridUI.Columns);
                float x = Mathf.Lerp(layerRect.xMin, layerRect.xMax, normalizedX);
                float rowHeight =
                    layerRect.height / Mathf.Max(1, buildingGridUI.Rows);
                float floorLineY = layerRect.yMin
                    + rowHeight
                    * Mathf.Clamp(row, 0, Mathf.Max(0, buildingGridUI.Rows - 1));

                return new Vector2(x, floorLineY + itemSize.y * 0.5f);
            }

            Vector3[] corners = new Vector3[4];
            cellRect.GetWorldCorners(corners);

            Vector3 bottomCenterWorld = Vector3.Lerp(corners[0], corners[3], 0.5f);
            Vector2 bottomCenterLocal = itemLayer.InverseTransformPoint(bottomCenterWorld);
            return new Vector2(
                bottomCenterLocal.x,
                bottomCenterLocal.y + itemSize.y * 0.5f);
        }
    }
}
