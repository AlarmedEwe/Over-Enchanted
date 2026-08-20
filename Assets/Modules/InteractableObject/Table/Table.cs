using UnityEngine;
using OverEnchanted.Player;

namespace OverEnchanted.Interaction
{
    public class Table : MonoBehaviour
    {
        [Header("Table Settings")]
        [Tooltip("Transform position where item is placed on top of the table. Created automatically if empty.")]
        [SerializeField] private Transform itemSlot;

        public GameObject CurrentItem { get; private set; }
        public bool HasItem => CurrentItem != null;
        public Transform ItemSlot => itemSlot;

        private void Awake()
        {
            EnsureItemSlot();
        }

        public void EnsureItemSlot()
        {
            if (itemSlot == null)
            {
                Transform existingSlot = transform.Find("ItemSlot");
                if (existingSlot != null)
                {
                    itemSlot = existingSlot;
                }
                else
                {
                    GameObject slotObj = new GameObject("ItemSlot");
                    slotObj.transform.SetParent(transform, false);

                    float height = 0.55f;
                    Collider col = GetComponent<Collider>();
                    if (col != null)
                    {
                        height = col.bounds.extents.y + 0.05f;
                    }

                    slotObj.transform.localPosition = new Vector3(0f, height, 0f);
                    slotObj.transform.localRotation = Quaternion.identity;
                    itemSlot = slotObj.transform;
                }
            }
        }

        public bool OnGrabDrop(PlayerInventory inventory)
        {
            if (inventory == null) return false;

            // 1. Player is holding an item -> place it on the table if empty
            if (inventory.HasItem)
            {
                if (!HasItem)
                {
                    GameObject item = inventory.RemoveItem();
                    if (item != null)
                    {
                        return PlaceItem(item);
                    }
                }
                // If table is already occupied, mark as handled so item is not dropped to floor
                return true;
            }

            // 2. Player is empty-handed -> pick up item from table if present
            if (!inventory.HasItem && HasItem)
            {
                GameObject item = RemoveItem();
                if (item != null)
                {
                    bool pickedUp = inventory.PickUpItem(item);
                    if (!pickedUp)
                    {
                        // Restore item to table if player failed to pick it up
                        PlaceItem(item);
                        return false;
                    }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Places an item on top of the table.
        /// Enables colliders on the item and removes any Rigidbody components (no physics needed on table).
        /// </summary>
        public bool PlaceItem(GameObject item)
        {
            if (HasItem || item == null) return false;

            EnsureItemSlot();

            CurrentItem = item;

            Transform parentTarget = itemSlot != null ? itemSlot : transform;
            ItemScaleTracker tracker = CurrentItem.GetComponent<ItemScaleTracker>();
            if (tracker == null)
            {
                tracker = CurrentItem.AddComponent<ItemScaleTracker>();
            }
            tracker.SetParentPreservingScale(parentTarget);

            // 1. Ensure colliders are enabled on the item
            Collider[] colliders = CurrentItem.GetComponentsInChildren<Collider>(true);
            if (colliders == null || colliders.Length == 0)
            {
                CurrentItem.AddComponent<BoxCollider>();
            }
            else
            {
                foreach (var col in colliders)
                {
                    col.enabled = true;
                }
            }

            // 2. Disable physics simulation on Rigidbody while item is sitting on the table
            Rigidbody[] rigidbodies = CurrentItem.GetComponentsInChildren<Rigidbody>(true);
            foreach (var rb in rigidbodies)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.detectCollisions = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            return true;
        }

        /// <summary>
        /// Removes and returns the item currently sitting on the table.
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
    }
}
