using UnityEngine;
using OverEnchanted.Player;
using OverEnchanted.Ingredients;

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
        private PlayerInventory playerInventory;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            playerInventory = GetComponent<PlayerInventory>();
        }

        private void OnEnable()
        {
            if (playerInput != null)
            {
                playerInput.OnInteractPressed += HandleInteract;
                playerInput.OnGrabDropPressed += HandleGrabDrop;
            }
        }

        private void OnDisable()
        {
            if (playerInput != null)
            {
                playerInput.OnInteractPressed -= HandleInteract;
                playerInput.OnGrabDropPressed -= HandleGrabDrop;
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

                // Fallback: If object has Table, IngredientBox, or IngredientItem component directly without InteractableObject on root
                if (interactable == null)
                {
                    Table table = hit.collider.GetComponentInParent<Table>();
                    IngredientBox box = hit.collider.GetComponentInParent<IngredientBox>();
                    IngredientItem item = hit.collider.GetComponentInParent<IngredientItem>();

                    if (table != null) interactable = table.GetComponent<InteractableObject>() ?? table.gameObject.AddComponent<InteractableObject>();
                    else if (box != null) interactable = box.GetComponent<InteractableObject>() ?? box.gameObject.AddComponent<InteractableObject>();
                    else if (item != null) interactable = item.GetComponent<InteractableObject>() ?? item.gameObject.AddComponent<InteractableObject>();
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

        private int lastHandleGrabDropFrame = -1;

        private void HandleGrabDrop()
        {
            if (Time.frameCount == lastHandleGrabDropFrame) return;
            lastHandleGrabDropFrame = Time.frameCount;

            // Perform raycast check on the exact input frame
            PerformRaycastCheck();

            // 1. If targeting an interactable object, delegate interaction to it and return
            // Under NO circumstance should a held item be dropped on the floor when facing an interactable object
            if (CurrentTarget != null)
            {
                CurrentTarget.OnGrabDrop(playerInventory);
                return;
            }

            // 2. Only drop held item on the floor if NOT targeting any interactable object
            if (playerInventory != null && playerInventory.HasItem)
            {
                playerInventory.DropItem();
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
