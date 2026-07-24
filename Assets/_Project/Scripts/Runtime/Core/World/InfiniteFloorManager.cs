using System;
using UnityEngine;
using UnityEngine.Events;

namespace LootUp.Core.World
{
    public sealed class InfiniteFloorManager : MonoBehaviour
    {
        [SerializeField]
        private int startAbsoluteFloor = 1;

        [SerializeField]
        private int floorsPerPage = BuildingGridUI.DefaultRows;

        [SerializeField]
        private BuildingGridUI buildingGridUI;

        [SerializeField]
        private UnityEvent<int> absoluteFloorChanged;

        [SerializeField]
        private int currentAbsoluteFloor;

        [SerializeField]
        private int currentPageIndex;

        [SerializeField]
        private int currentPageFloorIndex;

        [SerializeField]
        private int runHighestFloor;

        private FloorPageGenerator pageGenerator;
        private FloorAddress currentAddress;

        public event Action<int> CurrentFloorChanged;

        public int CurrentAbsoluteFloor => currentAbsoluteFloor;
        public int CurrentPageIndex => currentPageIndex;
        public int CurrentPageFloorIndex => currentPageFloorIndex;
        public int RunHighestFloor => runHighestFloor;
        public int StartAbsoluteFloor => Mathf.Max(1, startAbsoluteFloor);

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            SetCurrentFloor(startAbsoluteFloor);
        }

        public void SetCurrentFloor(int absoluteFloor)
        {
            EnsureGenerator();

            FloorAddress nextAddress = pageGenerator.GetAddress(absoluteFloor);
            bool pageChanged = nextAddress.PageIndex != currentAddress.PageIndex;

            currentAddress = nextAddress;
            SyncCurrentAddressFields();
            runHighestFloor = Mathf.Max(runHighestFloor, currentAddress.AbsoluteFloor);

            if (buildingGridUI != null && pageChanged)
            {
                buildingGridUI.SetPage(pageGenerator.GeneratePage(currentAddress.PageIndex));
            }

            CurrentFloorChanged?.Invoke(currentAddress.AbsoluteFloor);
            absoluteFloorChanged?.Invoke(currentAddress.AbsoluteFloor);
        }

        public void MoveToNextFloor()
        {
            SetCurrentFloor(CurrentAbsoluteFloor + 1);
        }

        [ContextMenu("Debug/Move To Next Floor")]
        private void DebugMoveToNextFloor()
        {
            MoveToNextFloor();
        }

        [ContextMenu("Debug/Move To Next Page")]
        private void DebugMoveToNextPage()
        {
            SetCurrentFloor((CurrentPageIndex + 1) * floorsPerPage + 1);
        }

        public FloorPageData GetCurrentPageData()
        {
            EnsureGenerator();
            return pageGenerator.GeneratePage(CurrentPageIndex);
        }

        public FloorPageData GetNextPageData()
        {
            EnsureGenerator();
            return pageGenerator.GeneratePage(CurrentPageIndex + 1);
        }

        private void Initialize()
        {
            floorsPerPage = Mathf.Max(1, floorsPerPage);
            pageGenerator = new FloorPageGenerator(floorsPerPage);
            currentAddress = pageGenerator.GetAddress(startAbsoluteFloor);
            SyncCurrentAddressFields();
            runHighestFloor = currentAddress.AbsoluteFloor;

            if (buildingGridUI == null)
            {
                buildingGridUI = FindFirstObjectByType<BuildingGridUI>();
            }

            if (buildingGridUI != null)
            {
                buildingGridUI.SetPage(pageGenerator.GeneratePage(currentAddress.PageIndex));
            }
        }

        private void EnsureGenerator()
        {
            if (pageGenerator == null)
            {
                Initialize();
            }
        }

        private void SyncCurrentAddressFields()
        {
            currentAbsoluteFloor = currentAddress.AbsoluteFloor;
            currentPageIndex = currentAddress.PageIndex;
            currentPageFloorIndex = currentAddress.PageFloorIndex;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            startAbsoluteFloor = Mathf.Max(1, startAbsoluteFloor);
            floorsPerPage = Mathf.Max(1, floorsPerPage);
        }
#endif
    }
}
