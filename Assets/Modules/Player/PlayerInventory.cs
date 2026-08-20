using UnityEngine;
using OverEnchanted.Interaction;

namespace OverEnchanted.Player
{
    public class PlayerInventory : MonoBehaviour
    {
        [Header("Hold Socket")]
        [Tooltip("Transform position where held item is parented. Created automatically if empty.")]
        [SerializeField] private Transform holdPoint;

        public GameObject CurrentItem { get; private set; }
        public bool HasItem => CurrentItem != null;
        public Transform HoldPoint => holdPoint;

        private void Awake()
        {
            if (holdPoint == null)
            {
                // Auto-create a HoldPoint in front of the player character
                GameObject socket = new GameObject("HoldPoint");
                socket.transform.SetParent(transform);
                socket.transform.localPosition = new Vector3(0f, 0.5f, 0.6f); // In front of chest/hands
                socket.transform.localRotation = Quaternion.identity;
                holdPoint = socket.transform;
            }
        }

        /// <summary>
        /// Equips an item instance into the player's hands. Returns true if successfully picked up.
        /// </summary>
        public bool PickUpItem(GameObject itemInstance)
        {
            if (HasItem || itemInstance == null) return false;

            // 1. Immediately disable Rigidbody physics & velocity BEFORE parenting to prevent physical kick
            Rigidbody[] rigidbodies = itemInstance.GetComponentsInChildren<Rigidbody>(true);
            foreach (var rb in rigidbodies)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.detectCollisions = false;
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
#else
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
#endif
            }

            // 2. Disable all colliders while held
            Collider[] colliders = itemInstance.GetComponentsInChildren<Collider>(true);
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // 3. Parent to holdPoint socket using scale tracker to preserve original mesh scale
            CurrentItem = itemInstance;
            ItemScaleTracker scaleTracker = CurrentItem.GetComponent<ItemScaleTracker>();
            if (scaleTracker == null)
            {
                scaleTracker = CurrentItem.AddComponent<ItemScaleTracker>();
            }
            scaleTracker.SetParentPreservingScale(holdPoint);

            return true;
        }

        /// <summary>
        /// Removes and returns the currently held item without applying floor placement or gravity physics.
        /// Useful for placing items onto tables, counters, or containers.
        /// </summary>
        public GameObject RemoveItem()
        {
            if (!HasItem) return null;

            GameObject item = CurrentItem;
            CurrentItem = null;

            ItemScaleTracker tracker = item.GetComponent<ItemScaleTracker>();
            if (tracker != null)
            {
                tracker.UnparentPreservingScale();
            }
            else
            {
                item.transform.SetParent(null);
            }

            return item;
        }

        /// <summary>
        /// Drops the currently held item and returns its GameObject reference.
        /// Activates physics, colliders, and gravity on the dropped item.
        /// </summary>
        public GameObject DropItem()
        {
            if (!HasItem) return null;

            GameObject droppedItem = CurrentItem;
            CurrentItem = null;

            Vector3 dropPosition = transform.position + transform.forward * 0.95f + Vector3.up * 0.3f;
            ItemScaleTracker tracker = droppedItem.GetComponent<ItemScaleTracker>();
            if (tracker != null)
            {
                tracker.UnparentPreservingScale();
            }
            else
            {
                droppedItem.transform.SetParent(null);
            }
            droppedItem.transform.position = dropPosition;

            // 1. Re-enable all colliders (add BoxCollider if completely missing)
            Collider[] colliders = droppedItem.GetComponentsInChildren<Collider>(true);
            if (colliders == null || colliders.Length == 0)
            {
                droppedItem.AddComponent<BoxCollider>();
            }
            else
            {
                foreach (var col in colliders)
                {
                    col.enabled = true;
                }
            }

            // 2. Re-enable Rigidbody physics, collision detection, and gravity
            Rigidbody[] rigidbodies = droppedItem.GetComponentsInChildren<Rigidbody>(true);
            if (rigidbodies == null || rigidbodies.Length == 0)
            {
                Rigidbody newRb = droppedItem.AddComponent<Rigidbody>();
                rigidbodies = new Rigidbody[] { newRb };
            }

            foreach (var rb in rigidbodies)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.detectCollisions = true;
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
#else
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
#endif
            }

            return droppedItem;
        }
    }
}
