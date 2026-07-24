using LootUp.Core.Player;
using LootUp.Core.World;
using UnityEngine;

namespace LootUp.Core.Items
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class FeverGoldInstance : MonoBehaviour
    {
        private readonly Vector3[] worldCorners = new Vector3[4];
        private FeverGoldFieldController owner;
        private InfiniteFloorManager floorManager;
        private PlayerMotor playerMotor;
        private RectTransform rectTransform;
        private int absoluteFloor;
        private int columnIndex;
        private bool isPlayerInside;
        private bool collected;

        public int AbsoluteFloor => absoluteFloor;
        public int ColumnIndex => columnIndex;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (collected || owner == null || floorManager == null || playerMotor == null)
            {
                return;
            }

            if (floorManager.CurrentAbsoluteFloor != absoluteFloor)
            {
                isPlayerInside = false;
                return;
            }

            bool inside = IsPlayerInside();
            if (inside && !isPlayerInside)
            {
                collected = true;
                owner.Collect(this);
                return;
            }

            isPlayerInside = inside;
        }

        public void Configure(
            FeverGoldFieldController fieldOwner,
            InfiniteFloorManager manager,
            PlayerMotor motor,
            int targetAbsoluteFloor,
            int targetColumn)
        {
            owner = fieldOwner;
            floorManager = manager;
            playerMotor = motor;
            absoluteFloor = targetAbsoluteFloor;
            columnIndex = targetColumn;
            isPlayerInside = false;
            collected = false;
            gameObject.SetActive(true);
        }

        private bool IsPlayerInside()
        {
            Rect playerWorldRect = GetWorldRect(playerMotor.RectTransform);
            Rect goldWorldRect = GetWorldRect(rectTransform);
            return goldWorldRect.Overlaps(playerWorldRect);
        }

        private Rect GetWorldRect(RectTransform target)
        {
            target.GetWorldCorners(worldCorners);
            Vector3 bottomLeft = worldCorners[0];
            Vector3 topRight = worldCorners[2];
            return new Rect(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
        }
    }
}
