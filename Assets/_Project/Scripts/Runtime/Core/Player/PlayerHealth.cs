using PH.Core.Game;
using PH.Core.UI;
using PH.Core.World;
using UnityEngine;

namespace PH.Core.Player
{
    public enum PlayerMaxLifeItemResult
    {
        None,
        Healed,
        IncreasedMaxLife,
        ScoreBonus
    }

    [DisallowMultipleComponent]
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField]
        private int maxLife = 3;

        [SerializeField]
        private int currentLife = 3;

        [SerializeField]
        private float invincibleSecondsAfterHit = 1f;

        [SerializeField]
        private float respawnMovementLockSeconds = 0.5f;

        [SerializeField]
        private float blinkIntervalSeconds = 0.09f;

        [SerializeField]
        private int respawnColumn;

        private TopHUDController topHUDController;
        private GameStateController gameStateController;
        private ElevatorController elevatorController;
        private PlayerMotor playerMotor;
        private PlayerController playerController;
        private CanvasRenderer[] canvasRenderers;
        private float invincibleUntilTime;
        private float movementLockUntilTime;
        private float nextBlinkTime;
        private bool blinkVisible = true;
        private bool isDepleted;
        private int maxLifeBonusFromItems;

        public int MaxLife => maxLife;
        public int CurrentLife => currentLife;
        public int MaxLifeBonusFromItems => maxLifeBonusFromItems;
        public bool IsInvincible => Time.time < invincibleUntilTime;
        public bool IsDepleted => isDepleted;

        private void Awake()
        {
            playerMotor = GetComponent<PlayerMotor>();
            playerController = GetComponent<PlayerController>();
            canvasRenderers = GetComponentsInChildren<CanvasRenderer>(true);
        }

        private void Update()
        {
            UpdateBlink();

            if (playerMotor == null || Time.time < movementLockUntilTime)
            {
                return;
            }

            if (gameStateController != null && gameStateController.IsGameOver)
            {
                return;
            }

            playerMotor.SetMovementLocked(false);
            playerController?.SetControlEnabled(true);
            movementLockUntilTime = 0f;
        }

        public void Configure(int life, TopHUDController hudController, GameStateController stateController, ElevatorController currentElevatorController, int startColumn)
        {
            maxLife = Mathf.Max(1, life);
            currentLife = maxLife;
            respawnColumn = Mathf.Max(0, startColumn);
            topHUDController = hudController;
            gameStateController = stateController;
            elevatorController = currentElevatorController;
            playerMotor = GetComponent<PlayerMotor>();
            playerController = GetComponent<PlayerController>();
            canvasRenderers = GetComponentsInChildren<CanvasRenderer>(true);
            invincibleUntilTime = 0f;
            movementLockUntilTime = 0f;
            nextBlinkTime = 0f;
            blinkVisible = true;
            SetVisible(true);
            isDepleted = false;
            maxLifeBonusFromItems = 0;

            SyncHUD();
        }

        public bool TakeDamage(int amount)
        {
            if (gameStateController != null && gameStateController.IsGameOver)
            {
                return false;
            }

            if (isDepleted || IsInvincible)
            {
                return false;
            }

            int damage = Mathf.Max(0, amount);
            if (damage <= 0)
            {
                return false;
            }

            currentLife = Mathf.Max(0, currentLife - damage);
            ConsumeItemMaxLifeBonus(damage);
            SyncHUD();

            if (currentLife <= 0)
            {
                isDepleted = true;
                gameStateController?.RequestGameOver(GameOverReason.LifeDepleted);
                return true;
            }

            invincibleUntilTime = Time.time + Mathf.Max(0f, invincibleSecondsAfterHit);
            MoveToCurrentFloorStartPosition();
            return true;
        }

        public int Heal(int amount)
        {
            int healAmount = Mathf.Max(0, amount);
            if (healAmount <= 0 || isDepleted)
            {
                return 0;
            }

            int before = currentLife;
            currentLife = Mathf.Min(maxLife, currentLife + healAmount);
            SyncHUD();

            return currentLife - before;
        }

        public PlayerMaxLifeItemResult ApplyMaxLifeItem(int amount, int maxBonusFromItems)
        {
            int value = Mathf.Max(1, amount);
            int bonusLimit = Mathf.Max(0, maxBonusFromItems);
            if (isDepleted)
            {
                return PlayerMaxLifeItemResult.None;
            }

            if (currentLife < maxLife)
            {
                Heal(value);
                return PlayerMaxLifeItemResult.Healed;
            }

            if (maxLifeBonusFromItems < bonusLimit)
            {
                int increase = Mathf.Min(value, bonusLimit - maxLifeBonusFromItems);
                maxLife += increase;
                maxLifeBonusFromItems += increase;
                currentLife = maxLife;
                SyncHUD();
                return PlayerMaxLifeItemResult.IncreasedMaxLife;
            }

            return PlayerMaxLifeItemResult.ScoreBonus;
        }

        private void ConsumeItemMaxLifeBonus(int damage)
        {
            if (damage <= 0 || maxLifeBonusFromItems <= 0)
            {
                return;
            }

            // (추가) 날개하트로 얻은 추가 슬롯은 피해를 대신 받은 뒤 즉시 제거한다.
            int consumedBonus = Mathf.Min(damage, maxLifeBonusFromItems);
            maxLifeBonusFromItems -= consumedBonus;
            maxLife = Mathf.Max(1, maxLife - consumedBonus);
            currentLife = Mathf.Min(currentLife, maxLife);
        }

        public void Revive(int reviveLife)
        {
            currentLife = Mathf.Clamp(reviveLife, 1, maxLife);
            isDepleted = false;
            invincibleUntilTime = Time.time + Mathf.Max(0f, invincibleSecondsAfterHit);
            MoveToCurrentFloorStartPosition();
            SyncHUD();
        }

        [ContextMenu("Debug/Take 1 Damage")]
        private void DebugTakeOneDamage()
        {
            TakeDamage(1);
        }

        private void SyncHUD()
        {
            topHUDController?.SetHearts(maxLife, currentLife);
        }

        private void MoveToCurrentFloorStartPosition()
        {
            playerMotor ??= GetComponent<PlayerMotor>();
            playerController ??= GetComponent<PlayerController>();
            int currentFloor = elevatorController != null ? elevatorController.CurrentAbsoluteFloor : 1;
            int targetColumn = currentFloor <= 1 || elevatorController == null
                ? respawnColumn
                : elevatorController.CurrentFloorStartColumn;
            playerMotor?.SetMovementLocked(true);
            playerController?.SetControlEnabled(false);
            playerMotor?.WarpToColumn(targetColumn);
            movementLockUntilTime = Time.time + Mathf.Max(0f, respawnMovementLockSeconds);
            nextBlinkTime = 0f;
            blinkVisible = true;
        }

        private void UpdateBlink()
        {
            if (!IsInvincible)
            {
                if (!blinkVisible)
                {
                    blinkVisible = true;
                    SetVisible(true);
                }

                return;
            }

            if (Time.time < nextBlinkTime)
            {
                return;
            }

            blinkVisible = !blinkVisible;
            SetVisible(blinkVisible);
            nextBlinkTime = Time.time + Mathf.Max(0.01f, blinkIntervalSeconds);
        }

        private void SetVisible(bool isVisible)
        {
            if (canvasRenderers == null || canvasRenderers.Length == 0)
            {
                canvasRenderers = GetComponentsInChildren<CanvasRenderer>(true);
            }

            float alpha = isVisible ? 1f : 0.2f;
            for (int i = 0; i < canvasRenderers.Length; i++)
            {
                if (canvasRenderers[i] != null)
                {
                    canvasRenderers[i].SetAlpha(alpha);
                }
            }
        }
    }
}
