using UnityEngine;

public class FloorDebugTester : MonoBehaviour
{
    public BuildingGridUI buildingGridUI;

    [Range(0, 9)]
    public int currentFloorIndex = 0;

    private int maxReachedFloorIndex = 0;

    private void OnValidate() // 인스펙터 값이 바뀔 때마다 호출
    {
        if (buildingGridUI == null) return;

        // 현재까지 도달한 최고 층 갱신
        maxReachedFloorIndex = Mathf.Max(maxReachedFloorIndex, currentFloorIndex);

        buildingGridUI.ApplyFloorStates(currentFloorIndex, maxReachedFloorIndex);
    }

    private void Start()
    {
        if (buildingGridUI != null)
        {
            buildingGridUI.ApplyFloorStates(currentFloorIndex, maxReachedFloorIndex);
        }
    }
}
