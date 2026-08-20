using UnityEngine;

namespace OverEnchanted.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Dash Settings")]
        [Tooltip("Burst movement speed during a dash.")]
        [SerializeField] private float dashSpeed = 16f;

        [Tooltip("Duration in seconds of a single dash burst.")]
        [SerializeField] private float dashDuration = 0.18f;

        [Tooltip("Cooldown in seconds between dash triggers to prevent single-frame spam.")]
        [SerializeField] private float dashCooldown = 0.25f;

        [Header("Physics Interaction")]
        [Tooltip("Force applied when player bumps into rigidbodies on the ground.")]
        [SerializeField] private float pushPower = 2.0f;

        [Header("Camera Reference")]
        [Tooltip("Fixed camera reference. If null, Camera.main will be used automatically.")]
        [SerializeField] private Camera targetCamera;

        private CharacterController characterController;
        private float verticalVelocity;

        private bool isDashing;
        private float dashTimer;
        private float dashCooldownTimer;
        private Vector3 dashDirection;

        public float CurrentSpeed => characterController != null ? new Vector3(characterController.velocity.x, 0f, characterController.velocity.z).magnitude : 0f;
        public bool IsGrounded => characterController != null && characterController.isGrounded;
        public bool IsDashing => isDashing;

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
        /// Triggers a high-speed dash burst in the current input direction (or transform.forward if neutral).
        /// Must be tapped repeatedly for long distance traversal.
        /// </summary>
        public bool TriggerDash(Vector2 inputDirection)
        {
            if (dashCooldownTimer > 0f) return false;

            Vector3 dir = CalculateCameraRelativeDirection(inputDirection);
            if (dir.sqrMagnitude < 0.001f)
            {
                dir = transform.forward;
            }

            dashDirection = dir.normalized;
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            if (dashDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dashDirection, Vector3.up);
            }

            return true;
        }

        /// <summary>
        /// Moves and rotates the player character relative to the fixed camera's orientation.
        /// </summary>
        /// <param name="inputDirection">Vector2 direction from Input System (WASD / Left Stick)</param>
        /// <param name="isSprinting">Compatibility parameter</param>
        public void Move(Vector2 inputDirection, bool isSprinting = false)
        {
            // Cooldown timer update
            if (dashCooldownTimer > 0f)
            {
                dashCooldownTimer -= Time.deltaTime;
            }

            Vector3 horizontalVelocity;

            if (isDashing)
            {
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0f)
                {
                    isDashing = false;
                }

                horizontalVelocity = dashDirection * dashSpeed;
            }
            else
            {
                Vector3 moveDirection = CalculateCameraRelativeDirection(inputDirection);
                horizontalVelocity = moveDirection * walkSpeed;

                // Rotate towards movement direction smoothly
                if (moveDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }

            // Gravity calculation
            if (characterController != null)
            {
                if (characterController.isGrounded && verticalVelocity < 0f)
                {
                    verticalVelocity = -2f; // Keep character grounded on slopes/flat ground
                }

                verticalVelocity += gravity * Time.deltaTime;

                // Combine horizontal & vertical velocities
                Vector3 finalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;

                // Apply movement via CharacterController
                characterController.Move(finalVelocity * Time.deltaTime);
            }
        }

        private Vector3 CalculateCameraRelativeDirection(Vector2 inputDirection)
        {
            if (inputDirection.sqrMagnitude < 0.001f) return Vector3.zero;

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                return new Vector3(inputDirection.x, 0f, inputDirection.y).normalized;
            }

            Vector3 cameraForward = targetCamera.transform.forward;
            Vector3 cameraRight = targetCamera.transform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            return (cameraForward * inputDirection.y + cameraRight * inputDirection.x).normalized;
        }

        public void SetCamera(Camera newCamera)
        {
            targetCamera = newCamera;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;

            // Do not push static objects or kinematic rigidbodies
            if (body == null || body.isKinematic) return;

            // Do not push objects below player feet
            if (hit.moveDirection.y < -0.3f) return;

            // Calculate push direction on horizontal plane
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);

            // Apply push force
            body.AddForce(pushDir * pushPower, ForceMode.Impulse);
        }
    }
}
