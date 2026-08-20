using UnityEngine;
using UnityEngine.InputSystem;

namespace OverEnchanted.Player
{
    [RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
    public class PlayerInput : MonoBehaviour
    {
        [Header("Action Names")]
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string sprintActionName = "Sprint";
        [SerializeField] private string interactActionName = "Interact";
        [Tooltip("Action mapping for picking up / dropping items (maps to Unity 'Jump' action: Space / South Button)")]
        [SerializeField] private string grabDropActionName = "Jump";

        public Vector2 MoveInput { get; private set; }
        public bool IsSprinting { get; private set; }

        private UnityEngine.InputSystem.PlayerInput playerInputComponent;
        private InputAction moveAction;
        private InputAction sprintAction;
        private InputAction interactAction;
        private InputAction grabDropAction;

        public delegate void InputVectorHandler(Vector2 input);
        public delegate void InputButtonHandler();

        public event InputVectorHandler OnMoveInputChanged;
        public event InputButtonHandler OnInteractPressed;
        public event InputButtonHandler OnGrabDropPressed;
        public event InputButtonHandler OnDashPressed;
        public event InputButtonHandler OnJumpPressed; // Compatibility alias

        private void Awake()
        {
            playerInputComponent = GetComponent<UnityEngine.InputSystem.PlayerInput>();

            if (playerInputComponent != null && playerInputComponent.actions != null)
            {
                moveAction = playerInputComponent.actions.FindAction(moveActionName);
                sprintAction = playerInputComponent.actions.FindAction(sprintActionName);
                interactAction = playerInputComponent.actions.FindAction(interactActionName);
                grabDropAction = playerInputComponent.actions.FindAction(grabDropActionName);
            }
        }

        private void OnEnable()
        {
            if (moveAction != null) moveAction.Enable();

            bool isSendingMessages = playerInputComponent != null &&
                (playerInputComponent.notificationBehavior == PlayerNotifications.SendMessages ||
                 playerInputComponent.notificationBehavior == PlayerNotifications.BroadcastMessages);

            if (!isSendingMessages)
            {
                if (sprintAction != null)
                {
                    sprintAction.Enable();
                    sprintAction.performed += OnSprintPerformed;
                }
                if (interactAction != null)
                {
                    interactAction.Enable();
                    interactAction.performed += OnInteractPerformed;
                }
                if (grabDropAction != null)
                {
                    grabDropAction.Enable();
                    grabDropAction.performed += OnGrabDropPerformed;
                }
            }
        }

        private void OnDisable()
        {
            if (moveAction != null) moveAction.Disable();

            if (sprintAction != null)
            {
                sprintAction.performed -= OnSprintPerformed;
                sprintAction.Disable();
            }
            if (interactAction != null)
            {
                interactAction.performed -= OnInteractPerformed;
                interactAction.Disable();
            }
            if (grabDropAction != null)
            {
                grabDropAction.performed -= OnGrabDropPerformed;
                grabDropAction.Disable();
            }
        }

        private void Update()
        {
            ReadInputs();
        }

        private void ReadInputs()
        {
            if (moveAction != null)
            {
                Vector2 newMove = moveAction.ReadValue<Vector2>();
                if (newMove != MoveInput)
                {
                    MoveInput = newMove;
                    OnMoveInputChanged?.Invoke(MoveInput);
                }
            }

            if (sprintAction != null)
            {
                IsSprinting = sprintAction.IsPressed();
            }
        }

        private float lastInteractTime = -10f;
        private float lastGrabDropTime = -10f;
        private float lastDashTime = -10f;
        private const float INPUT_COOLDOWN = 0.25f; // 250ms debounce cooldown between interaction triggers

        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                TriggerDash();
            }
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                TriggerInteract();
            }
        }

        private void OnGrabDropPerformed(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                TriggerGrabDrop();
            }
        }

        // SendMessages / BroadcastMessages Unity Callbacks compatibility
        public void OnMove(InputValue value)
        {
            MoveInput = value.Get<Vector2>();
            OnMoveInputChanged?.Invoke(MoveInput);
        }

        public void OnSprint(InputValue value)
        {
            IsSprinting = value.isPressed;
            if (value.isPressed)
            {
                TriggerDash();
            }
        }

        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                TriggerInteract();
            }
        }

        public void OnJump(InputValue value)
        {
            if (value.isPressed)
            {
                TriggerGrabDrop();
            }
        }

        private void TriggerDash()
        {
            if (Time.time - lastDashTime < 0.1f) return;
            lastDashTime = Time.time;
            OnDashPressed?.Invoke();
        }

        private void TriggerInteract()
        {
            if (Time.time - lastInteractTime < INPUT_COOLDOWN) return;
            lastInteractTime = Time.time;
            OnInteractPressed?.Invoke();
        }

        private void TriggerGrabDrop()
        {
            if (Time.time - lastGrabDropTime < INPUT_COOLDOWN) return;
            lastGrabDropTime = Time.time;
            OnGrabDropPressed?.Invoke();
            OnJumpPressed?.Invoke();
        }
    }
}
