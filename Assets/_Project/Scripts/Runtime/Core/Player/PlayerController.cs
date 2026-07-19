using PH.Core.Characters;
using PH.Core.Feedback;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PH.Core.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private PlayerMotor motor;

        [SerializeField]
        private RectTransform touchArea;

        [SerializeField]
        private int facingDirection = 1;

        [SerializeField]
        private bool moveOnStart;

        [SerializeField]
        private bool controlEnabled = true;

        [SerializeField]
        private float pivotCooldownSeconds;

        private InputAction leftAction;
        private InputAction rightAction;
        private Camera uiCamera;
        private PlayerCharacterRuntime characterRuntime;
        private int moveDirection;
        private float lastPivotTime = -999f;

        public int FacingDirection => facingDirection;
        public int MoveDirection => moveDirection;
        public bool IsMoving => moveDirection != 0;

        private void Awake()
        {
            if (motor == null)
            {
                motor = GetComponent<PlayerMotor>();
            }

            CreateInputActions();
        }

        private void OnEnable()
        {
            leftAction?.Enable();
            rightAction?.Enable();
        }

        private void OnDisable()
        {
            leftAction?.Disable();
            rightAction?.Disable();
        }

        private void OnDestroy()
        {
            leftAction?.Dispose();
            rightAction?.Dispose();
        }

        private void Update()
        {
            if (!controlEnabled)
            {
                motor.Move(0f, Time.deltaTime);
                return;
            }

            HandleTouchInput();
            HandleKeyboardInput();

            motor.Move(moveDirection, Time.deltaTime);
            StopAtHorizontalLimit();
        }

        public void Configure(PlayerMotor playerMotor, RectTransform inputTouchArea)
        {
            Configure(playerMotor, inputTouchArea, null, pivotCooldownSeconds);
        }

        public void Configure(PlayerMotor playerMotor, RectTransform inputTouchArea, PlayerCharacterRuntime runtime, float pivotCooldown)
        {
            motor = playerMotor;
            touchArea = inputTouchArea;
            characterRuntime = runtime;
            pivotCooldownSeconds = Mathf.Max(0f, pivotCooldown);
            facingDirection = NormalizeDirection(facingDirection);
            moveDirection = moveOnStart ? facingDirection : 0;
        }

        public void SetExternalHorizontalInput(float input)
        {
            int direction = input < 0f ? -1 : 1;
            StartMoving(direction);
        }

        public void StopAndFace(int direction)
        {
            facingDirection = NormalizeDirection(direction);
            moveDirection = 0;
        }

        public void SetControlEnabled(bool isEnabled)
        {
            controlEnabled = isEnabled;
            if (!controlEnabled)
            {
                moveDirection = 0;
            }
        }

        private void HandleTouchInput()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame)
            {
                return;
            }

            Vector2 screenPosition = pointer.position.ReadValue();
            if (!IsInsideTouchArea(screenPosition))
            {
                return;
            }

            int nextDirection = moveDirection == 0 ? facingDirection : -facingDirection;
            StartMoving(nextDirection);
        }

        private void HandleKeyboardInput()
        {
            if (leftAction != null && leftAction.WasPressedThisFrame())
            {
                StartMoving(-1);
            }

            if (rightAction != null && rightAction.WasPressedThisFrame())
            {
                StartMoving(1);
            }
        }

        private void StartMoving(int direction)
        {
            int normalizedDirection = NormalizeDirection(direction);
            bool isPivot = moveDirection != 0 && normalizedDirection != moveDirection;

            if (isPivot && Time.time - lastPivotTime < pivotCooldownSeconds)
            {
                return;
            }

            facingDirection = normalizedDirection;
            moveDirection = facingDirection;

            if (isPivot)
            {
                lastPivotTime = Time.time;
                characterRuntime?.AddPivotCharge();
                HapticFeedback.Play(HapticFeedbackPattern.Pivot);
            }
        }

        private void StopAtHorizontalLimit()
        {
            if (motor == null || moveDirection == 0)
            {
                return;
            }

            if (!motor.IsAtHorizontalLimit(moveDirection))
            {
                return;
            }

            StopAndFace(-moveDirection);
        }

        private bool IsInsideTouchArea(Vector2 screenPosition)
        {
            if (touchArea == null)
            {
                return true;
            }

            return RectTransformUtility.RectangleContainsScreenPoint(touchArea, screenPosition, uiCamera);
        }

        private void CreateInputActions()
        {
            leftAction = new InputAction("MoveLeft", InputActionType.Button);
            leftAction.AddBinding("<Keyboard>/leftArrow");
            leftAction.AddBinding("<Keyboard>/a");

            rightAction = new InputAction("MoveRight", InputActionType.Button);
            rightAction.AddBinding("<Keyboard>/rightArrow");
            rightAction.AddBinding("<Keyboard>/d");
        }

        private int NormalizeDirection(int direction)
        {
            return direction < 0 ? -1 : 1;
        }
    }
}
