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

        private InputAction leftAction;
        private InputAction rightAction;
        private Camera uiCamera;
        private int moveDirection;

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
        }

        public void Configure(PlayerMotor playerMotor, RectTransform inputTouchArea)
        {
            motor = playerMotor;
            touchArea = inputTouchArea;
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
            facingDirection = NormalizeDirection(direction);
            moveDirection = facingDirection;
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
