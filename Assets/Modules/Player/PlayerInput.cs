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

        public Vector2 MoveInput { get; private set; }
        public bool IsSprinting { get; private set; }

        private UnityEngine.InputSystem.PlayerInput playerInputComponent;
        private InputAction moveAction;
        private InputAction sprintAction;
        private InputAction interactAction;

        public delegate void InputVectorHandler(Vector2 input);
        public delegate void InputButtonHandler();

        public event InputVectorHandler OnMoveInputChanged;
        public event InputButtonHandler OnInteractPressed;

        private void Awake()
        {
            playerInputComponent = GetComponent<UnityEngine.InputSystem.PlayerInput>();

            if (playerInputComponent != null && playerInputComponent.actions != null)
            {
                moveAction = playerInputComponent.actions.FindAction(moveActionName);
                sprintAction = playerInputComponent.actions.FindAction(sprintActionName);
                interactAction = playerInputComponent.actions.FindAction(interactActionName);
            }
        }

        private void OnEnable()
        {
            if (moveAction != null) moveAction.Enable();
            if (sprintAction != null) sprintAction.Enable();
            if (interactAction != null)
            {
                interactAction.Enable();
                interactAction.performed += OnInteractPerformed;
            }
        }

        private void OnDisable()
        {
            if (moveAction != null) moveAction.Disable();
            if (sprintAction != null) sprintAction.Disable();
            if (interactAction != null)
            {
                interactAction.performed -= OnInteractPerformed;
                interactAction.Disable();
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

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            OnInteractPressed?.Invoke();
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
        }

        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                OnInteractPressed?.Invoke();
            }
        }
    }
}
