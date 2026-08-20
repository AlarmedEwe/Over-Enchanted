using UnityEngine;
using OverEnchanted.Player;

namespace OverEnchanted.Interaction
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [Tooltip("Maximum distance in front of the player to check for interactive objects.")]
        [SerializeField] private float rayDistance = 1.8f;

        [Tooltip("Height offset from player pivot position for the ray origin.")]
        [SerializeField] private float rayHeightOffset = 0.5f;

        [Tooltip("Radius for SphereCast cone detection.")]
        [SerializeField] private float sphereRadius = 0.25f;

        [Tooltip("Layer mask filter for interactable objects.")]
        [SerializeField] private LayerMask interactableLayer = ~0;

        [Header("Debug")]
        [SerializeField] private bool showGizmos = true;

        public InteractableObject CurrentTarget { get; private set; }

        private PlayerInput playerInput;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
        }

        private void OnEnable()
        {
            if (playerInput != null)
            {
                playerInput.OnInteractPressed += HandleInteract;
            }
        }

        private void OnDisable()
        {
            if (playerInput != null)
            {
                playerInput.OnInteractPressed -= HandleInteract;
            }

            ClearCurrentTarget();
        }

        private void Update()
        {
            PerformRaycastCheck();
        }

        private void PerformRaycastCheck()
        {
            Vector3 origin = transform.position + Vector3.up * rayHeightOffset;
            Vector3 direction = transform.forward;

            InteractableObject newTarget = null;

            // SphereCast gives a forgiving detection cone in front of the player
            if (Physics.SphereCast(origin, sphereRadius, direction, out RaycastHit hit, rayDistance, interactableLayer))
            {
                InteractableObject interactable = hit.collider.GetComponentInParent<InteractableObject>();
                if (interactable == null)
                {
                    interactable = hit.collider.GetComponent<InteractableObject>();
                }

                if (interactable != null)
                {
                    newTarget = interactable;
                }
            }

            SetCurrentTarget(newTarget);
        }

        private void SetCurrentTarget(InteractableObject newTarget)
        {
            if (CurrentTarget == newTarget) return;

            if (CurrentTarget != null)
            {
                CurrentTarget.SetHighlight(false);
            }

            CurrentTarget = newTarget;

            if (CurrentTarget != null)
            {
                CurrentTarget.SetHighlight(true);
            }
        }

        private void ClearCurrentTarget()
        {
            if (CurrentTarget != null)
            {
                CurrentTarget.SetHighlight(false);
                CurrentTarget = null;
            }
        }

        private void HandleInteract()
        {
            if (CurrentTarget != null)
            {
                CurrentTarget.Interact();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;

            Gizmos.color = CurrentTarget != null ? Color.green : Color.red;
            Vector3 origin = transform.position + Vector3.up * rayHeightOffset;
            Vector3 targetPos = origin + transform.forward * rayDistance;

            Gizmos.DrawLine(origin, targetPos);
            Gizmos.DrawWireSphere(targetPos, sphereRadius);
        }
    }
}
