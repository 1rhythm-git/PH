using PH.Core.World;
using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.Player
{
    public sealed class PlayerSpawner : MonoBehaviour
    {
        [SerializeField]
        private bool spawnOnStart = true;

        [SerializeField]
        private BuildingGridUI buildingGridUI;

        [SerializeField]
        private InfiniteFloorManager floorManager;

        [SerializeField]
        private RectTransform playerLayer;

        [SerializeField]
        private RectTransform touchArea;

        [SerializeField]
        private GameObject playerPrefab;

        [SerializeField]
        private int startColumn;

        [SerializeField]
        private float moveSpeedColumnsPerSecond = 4f;

        [SerializeField]
        private Vector2 playerSize = new Vector2(72f, 72f);

        [SerializeField]
        private Color playerColor = new Color(0.95f, 0.78f, 0.22f, 1f);

        [SerializeField]
        private Color playerOutlineColor = new Color(0.05f, 0.05f, 0.06f, 0.85f);

        private GameObject spawnedPlayer;

        public GameObject SpawnedPlayer => spawnedPlayer;

        private void Awake()
        {
            EnsureReferences();
        }

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnPlayer();
            }
        }

        [ContextMenu("Debug/Spawn Player")]
        public void SpawnPlayer()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Player는 Play 모드에서만 런타임 생성합니다.", this);
                return;
            }

            EnsureReferences();

            if (spawnedPlayer != null)
            {
                Destroy(spawnedPlayer);
            }

            if (playerLayer == null || buildingGridUI == null || floorManager == null)
            {
                Debug.LogWarning("PlayerSpawner 참조가 부족해 Player를 생성할 수 없습니다.", this);
                return;
            }

            spawnedPlayer = playerPrefab != null
                ? Instantiate(playerPrefab, playerLayer)
                : CreateDefaultPlayer();

            spawnedPlayer.name = "Player";
            spawnedPlayer.transform.SetParent(playerLayer, false);

            RectTransform playerRect = spawnedPlayer.GetComponent<RectTransform>();
            playerRect.sizeDelta = playerSize;

            PlayerMotor motor = spawnedPlayer.GetComponent<PlayerMotor>();
            if (motor == null)
            {
                motor = spawnedPlayer.AddComponent<PlayerMotor>();
            }

            PlayerController controller = spawnedPlayer.GetComponent<PlayerController>();
            if (controller == null)
            {
                controller = spawnedPlayer.AddComponent<PlayerController>();
            }

            controller.Configure(motor, touchArea);
            motor.Configure(buildingGridUI, floorManager, startColumn, moveSpeedColumnsPerSecond);
        }

        private GameObject CreateDefaultPlayer()
        {
            GameObject playerObject = new GameObject("Player", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(PlayerMotor), typeof(PlayerController));
            playerObject.layer = playerLayer.gameObject.layer;

            Image image = playerObject.GetComponent<Image>();
            image.color = playerColor;
            image.raycastTarget = false;

            Outline outline = playerObject.GetComponent<Outline>();
            outline.effectColor = playerOutlineColor;
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            return playerObject;
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

            if (touchArea == null && buildingGridUI != null)
            {
                touchArea = buildingGridUI.transform.parent as RectTransform;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            startColumn = Mathf.Max(0, startColumn);
            moveSpeedColumnsPerSecond = Mathf.Max(0f, moveSpeedColumnsPerSecond);
            playerSize.x = Mathf.Max(1f, playerSize.x);
            playerSize.y = Mathf.Max(1f, playerSize.y);
        }
#endif
    }
}
