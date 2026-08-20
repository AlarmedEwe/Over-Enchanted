using UnityEngine;

namespace OverEnchanted.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Camera Reference")]
        [Tooltip("Fixed camera reference. If null, Camera.main will be used automatically.")]
        [SerializeField] private Camera targetCamera;

        private CharacterController characterController;
        private float verticalVelocity;

        public float CurrentSpeed => new Vector3(characterController.velocity.x, 0f, characterController.velocity.z).magnitude;
        public bool IsGrounded => characterController.isGrounded;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        /// <summary>
        /// Moves and rotates the player character relative to the fixed camera's orientation.
        /// </summary>
        /// <param name="inputDirection">Vector2 direction from Input System (WASD / Left Stick)</param>
        /// <param name="isSprinting">Whether the player is sprinting</param>
        public void Move(Vector2 inputDirection, bool isSprinting)
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    Debug.LogWarning("PlayerMovement: Camera target is missing. Assign a Camera or set one as MainCamera.");
                    return;
                }
            }

            // 1. Get Camera's forward and right vectors projected onto the horizontal XZ plane
            Vector3 cameraForward = targetCamera.transform.forward;
            Vector3 cameraRight = targetCamera.transform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // 2. Calculate movement direction relative to camera view
            Vector3 moveDirection = (cameraForward * inputDirection.y + cameraRight * inputDirection.x).normalized;

            // 3. Calculate speed & horizontal velocity
            float speed = isSprinting ? sprintSpeed : walkSpeed;
            Vector3 horizontalVelocity = moveDirection * speed;

            // 4. Gravity calculation
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f; // Keep character grounded on slopes/flat ground
            }

            verticalVelocity += gravity * Time.deltaTime;

            // Combine horizontal & vertical velocities
            Vector3 finalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;

            // Apply movement via CharacterController
            characterController.Move(finalVelocity * Time.deltaTime);

            // 5. Rotate towards movement direction smoothly
            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        public void SetCamera(Camera newCamera)
        {
            targetCamera = newCamera;
        }
    }
}
