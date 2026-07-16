using PH.Core.Player;
using PH.Core.Game;
using PH.Core.World;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PH.Core.Enemies
{
    public sealed class TestEnemySpawner : MonoBehaviour, IGameplayPausable
    {
        private const string EnemyLayerName = "EnemyLayer";
        private const string EnemyGuideLineLayerName = "EnemyGuideLineLayer";

        [SerializeField]
        private BuildingGridUI buildingGridUI;

        [SerializeField]
        private InfiniteFloorManager floorManager;

        [SerializeField]
        private PlayerSpawner playerSpawner;

        [SerializeField]
        private RectTransform enemyLayer;

        [SerializeField]
        private RectTransform guideLineLayer;

        [SerializeField]
        private RectTransform playerLayer;

        [SerializeField]
        private bool spawnOnStart = true;

        [SerializeField]
        private int baseEnemiesPerPage = 4;

        [SerializeField]
        private int maxEnemiesPerPage = 7;

        [SerializeField]
        private int floorsPerEnemyIncrease = 10;

        [SerializeField]
        private int enemyLineRandomSeed = 81473;

        [SerializeField]
        private bool randomizeSeedOnStart = true;

        [SerializeField]
        private bool randomizeLinesPerPage = true;

        [SerializeField]
        private int damage = 1;

        [SerializeField]
        private float hitCooldownSeconds = 0.65f;

        [SerializeField]
        private float minVerticalSpeed = 130f;

        [SerializeField]
        private float maxVerticalSpeed = 360f;

        [SerializeField]
        private float speedStepPerLine = 22f;

        [SerializeField]
        private float speedStepPerDifficulty = 18f;

        [SerializeField]
        private float guideLineThickness = 4f;

        [SerializeField]
        private Color guideLineColor = new Color(1f, 1f, 1f, 0.68f);

        [SerializeField]
        private Vector2 enemySize = new Vector2(48f, 76f);

        [SerializeField]
        private Sprite enemySprite;

        [SerializeField]
        private Color enemySpriteTint = Color.white;

        [SerializeField]
        private bool preserveEnemySpriteAspect = true;

        [SerializeField]
        private Vector2 enemyVisualScale = new Vector2(1.45f, 1.45f);

        [SerializeField]
        private Vector2 enemyVisualOffset;

        [SerializeField]
        private Color enemyColor = new Color(1f, 0.12f, 0.1f, 0.94f);

        [SerializeField]
        private Color enemyOutlineColor = new Color(1f, 1f, 1f, 0.52f);

        private readonly System.Collections.Generic.List<TestEnemyHazard> spawnedEnemies = new System.Collections.Generic.List<TestEnemyHazard>();
        private readonly List<int> availableLineIndices = new List<int>();
        private int lastPageIndex = -1;
        private int runtimeEnemyLineRandomSeed;
        private bool hasRuntimeEnemyLineRandomSeed;

        private void Awake()
        {
            EnsureReferences();
        }

        private void Start()
        {
            if (floorManager != null)
            {
                lastPageIndex = floorManager.CurrentPageIndex;
            }

            InitializeRuntimeSeed();

            if (spawnOnStart)
            {
                SpawnTestEnemy();
            }
        }

        private void OnEnable()
        {
            EnsureReferences();

            if (floorManager != null)
            {
                floorManager.CurrentFloorChanged += HandleCurrentFloorChanged;
            }
        }

        private void OnDisable()
        {
            if (floorManager != null)
            {
                floorManager.CurrentFloorChanged -= HandleCurrentFloorChanged;
            }
        }

        [ContextMenu("Debug/Spawn Test Enemy")]
        public void SpawnTestEnemy()
        {
            EnsureReferences();

            if (enemyLayer == null || buildingGridUI == null)
            {
                Debug.LogWarning("TestEnemySpawner 참조가 부족해 테스트 Enemy를 생성할 수 없습니다.", this);
                return;
            }

            ClearSpawnedEnemies();
            lastPageIndex = floorManager != null ? floorManager.CurrentPageIndex : lastPageIndex;

            int difficultyStep = GetDifficultyStep();
            int enemyCount = GetEnemyCountForDifficulty(difficultyStep);
            BuildLineSelection(enemyCount);

            for (int i = 0; i < availableLineIndices.Count; i++)
            {
                SpawnEnemyAtLine(availableLineIndices[i], i, difficultyStep);
            }
        }

        public void SetGameplayPaused(bool isPaused)
        {
            enabled = !isPaused;
        }

        private void SpawnEnemyAtLine(int lineIndex, int sequenceIndex, int difficultyStep)
        {
            GameObject enemyObject = new GameObject($"TestEnemy_Line_{lineIndex}", typeof(RectTransform), typeof(TestEnemyHazard));
            enemyObject.layer = enemyLayer.gameObject.layer;
            enemyObject.transform.SetParent(enemyLayer, false);

            RectTransform enemyRect = enemyObject.GetComponent<RectTransform>();
            enemyRect.sizeDelta = enemySize;

            CreateEnemyVisual(enemyObject.transform, enemyObject.layer);

            float difficultySpeedBonus = Mathf.Max(0f, speedStepPerDifficulty) * Mathf.Max(0, difficultyStep);
            float lineMinSpeed = Mathf.Max(0f, minVerticalSpeed + difficultySpeedBonus + speedStepPerLine * sequenceIndex);
            float lineMaxSpeed = Mathf.Max(lineMinSpeed, maxVerticalSpeed + difficultySpeedBonus + speedStepPerLine * sequenceIndex);
            TestEnemyHazard enemy = enemyObject.GetComponent<TestEnemyHazard>();
            enemy.Configure(buildingGridUI, playerSpawner, lineIndex, damage, hitCooldownSeconds, lineMinSpeed, lineMaxSpeed, guideLineThickness, guideLineColor, guideLineLayer);
            spawnedEnemies.Add(enemy);
        }

        private void CreateEnemyVisual(Transform parent, int layer)
        {
            GameObject visualObject = new GameObject("Visual", typeof(RectTransform), typeof(Image), typeof(Outline));
            visualObject.layer = layer;
            visualObject.transform.SetParent(parent, false);

            RectTransform visualRect = visualObject.GetComponent<RectTransform>();
            visualRect.anchorMin = new Vector2(0.5f, 0.5f);
            visualRect.anchorMax = new Vector2(0.5f, 0.5f);
            visualRect.pivot = new Vector2(0.5f, 0.5f);
            visualRect.anchoredPosition = enemyVisualOffset;
            visualRect.sizeDelta = new Vector2(enemySize.x * Mathf.Max(0.01f, enemyVisualScale.x), enemySize.y * Mathf.Max(0.01f, enemyVisualScale.y));

            Image enemyImage = visualObject.GetComponent<Image>();
            enemyImage.sprite = enemySprite;
            enemyImage.color = enemySprite != null ? enemySpriteTint : enemyColor;
            enemyImage.preserveAspect = preserveEnemySpriteAspect;
            enemyImage.raycastTarget = false;

            Outline outline = visualObject.GetComponent<Outline>();
            outline.effectColor = enemyOutlineColor;
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;
        }

        private void ClearSpawnedEnemies()
        {
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                TestEnemyHazard enemy = spawnedEnemies[i];
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }

            spawnedEnemies.Clear();
        }

        private int GetDifficultyStep()
        {
            int currentFloor = floorManager != null ? floorManager.CurrentAbsoluteFloor : 1;
            int interval = Mathf.Max(1, floorsPerEnemyIncrease);

            return Mathf.Max(0, (Mathf.Max(1, currentFloor) - 1) / interval);
        }

        private int GetEnemyCountForDifficulty(int difficultyStep)
        {
            int internalLineCount = GetInternalLineCount();
            int minCount = Mathf.Clamp(baseEnemiesPerPage, 1, internalLineCount);
            int maxCount = Mathf.Clamp(maxEnemiesPerPage, minCount, internalLineCount);

            return Mathf.Clamp(minCount + Mathf.Max(0, difficultyStep), minCount, maxCount);
        }

        private int GetInternalLineCount()
        {
            int columns = buildingGridUI != null ? Mathf.Max(1, buildingGridUI.Columns) : BuildingGridUI.DefaultColumns;

            return Mathf.Max(1, columns - 1);
        }

        private void BuildLineSelection(int enemyCount)
        {
            availableLineIndices.Clear();

            int internalLineCount = GetInternalLineCount();
            for (int i = 0; i < internalLineCount; i++)
            {
                availableLineIndices.Add(i + 1);
            }

            if (randomizeLinesPerPage)
            {
                int pageIndex = floorManager != null ? floorManager.CurrentPageIndex : 0;
                System.Random random = new System.Random(unchecked(GetRuntimeEnemyLineRandomSeed() + pageIndex * 73856093));

                for (int i = availableLineIndices.Count - 1; i > 0; i--)
                {
                    int swapIndex = random.Next(0, i + 1);
                    int temp = availableLineIndices[i];
                    availableLineIndices[i] = availableLineIndices[swapIndex];
                    availableLineIndices[swapIndex] = temp;
                }
            }

            int clampedEnemyCount = Mathf.Clamp(enemyCount, 1, availableLineIndices.Count);
            if (availableLineIndices.Count > clampedEnemyCount)
            {
                availableLineIndices.RemoveRange(clampedEnemyCount, availableLineIndices.Count - clampedEnemyCount);
            }

            availableLineIndices.Sort();
        }

        private int GetRuntimeEnemyLineRandomSeed()
        {
            if (!hasRuntimeEnemyLineRandomSeed)
            {
                InitializeRuntimeSeed();
            }

            return runtimeEnemyLineRandomSeed;
        }

        private void InitializeRuntimeSeed()
        {
            runtimeEnemyLineRandomSeed = randomizeSeedOnStart
                ? unchecked(enemyLineRandomSeed ^ Random.Range(int.MinValue, int.MaxValue) ^ GetInstanceID() ^ System.Environment.TickCount)
                : enemyLineRandomSeed;
            hasRuntimeEnemyLineRandomSeed = true;
        }

        private void HandleCurrentFloorChanged(int currentAbsoluteFloor)
        {
            if (!spawnOnStart || floorManager == null)
            {
                return;
            }

            int currentPageIndex = floorManager.CurrentPageIndex;
            if (lastPageIndex < 0)
            {
                lastPageIndex = currentPageIndex;
                return;
            }

            if (currentPageIndex == lastPageIndex)
            {
                return;
            }

            SpawnTestEnemy();
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

            if (enemyLayer == null)
            {
                enemyLayer = FindEnemyLayer();
            }

            if (guideLineLayer == null)
            {
                guideLineLayer = FindGuideLineLayer();
            }

            if (playerLayer == null)
            {
                playerLayer = FindLayer("PlayerLayer");
            }

            ApplyForegroundLayerOrder();
        }

        private RectTransform FindEnemyLayer()
        {
            Transform existing = GameObject.Find(EnemyLayerName)?.transform;
            if (existing is RectTransform existingRect)
            {
                return existingRect;
            }

            RectTransform parent = ResolveMiddleUIRoot();
            if (parent == null)
            {
                return null;
            }

            GameObject layerObject = new GameObject(EnemyLayerName, typeof(RectTransform));
            layerObject.layer = parent.gameObject.layer;
            layerObject.transform.SetParent(parent, false);

            RectTransform layerRect = layerObject.GetComponent<RectTransform>();
            layerRect.anchorMin = Vector2.zero;
            layerRect.anchorMax = Vector2.one;
            layerRect.offsetMin = Vector2.zero;
            layerRect.offsetMax = Vector2.zero;
            layerRect.pivot = new Vector2(0.5f, 0.5f);

            return layerRect;
        }

        private RectTransform FindGuideLineLayer()
        {
            Transform existing = GameObject.Find(EnemyGuideLineLayerName)?.transform;
            if (existing is RectTransform existingRect)
            {
                return existingRect;
            }

            RectTransform parent = ResolveMiddleUIRoot();
            if (parent == null)
            {
                return null;
            }

            GameObject layerObject = new GameObject(EnemyGuideLineLayerName, typeof(RectTransform));
            layerObject.layer = parent.gameObject.layer;
            layerObject.transform.SetParent(parent, false);

            RectTransform layerRect = layerObject.GetComponent<RectTransform>();
            layerRect.anchorMin = Vector2.zero;
            layerRect.anchorMax = Vector2.one;
            layerRect.offsetMin = Vector2.zero;
            layerRect.offsetMax = Vector2.zero;
            layerRect.pivot = new Vector2(0.5f, 0.5f);

            return layerRect;
        }

        private RectTransform FindLayer(string layerName)
        {
            Transform existing = GameObject.Find(layerName)?.transform;
            return existing as RectTransform;
        }

        private void ApplyForegroundLayerOrder()
        {
            guideLineLayer?.SetAsLastSibling();
            enemyLayer?.SetAsLastSibling();
            playerLayer?.SetAsLastSibling();
        }

        private RectTransform ResolveMiddleUIRoot()
        {
            if (buildingGridUI != null && buildingGridUI.transform.parent is RectTransform gridParent)
            {
                return gridParent;
            }

            GameObject middleUI = GameObject.Find("MiddleUI");
            return middleUI != null ? middleUI.transform as RectTransform : null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            baseEnemiesPerPage = Mathf.Max(1, baseEnemiesPerPage);
            maxEnemiesPerPage = Mathf.Max(baseEnemiesPerPage, maxEnemiesPerPage);
            floorsPerEnemyIncrease = Mathf.Max(1, floorsPerEnemyIncrease);
            damage = Mathf.Max(1, damage);
            hitCooldownSeconds = Mathf.Max(0f, hitCooldownSeconds);
            minVerticalSpeed = Mathf.Max(0f, minVerticalSpeed);
            maxVerticalSpeed = Mathf.Max(minVerticalSpeed, maxVerticalSpeed);
            speedStepPerLine = Mathf.Max(0f, speedStepPerLine);
            speedStepPerDifficulty = Mathf.Max(0f, speedStepPerDifficulty);
            guideLineThickness = Mathf.Max(1f, guideLineThickness);
            enemySize.x = Mathf.Max(1f, enemySize.x);
            enemySize.y = Mathf.Max(1f, enemySize.y);
            enemyVisualScale.x = Mathf.Max(0.01f, enemyVisualScale.x);
            enemyVisualScale.y = Mathf.Max(0.01f, enemyVisualScale.y);
        }
#endif
    }
}
