using UnityEngine;
using OverEnchanted.Player;

namespace OverEnchanted.Interaction
{
    public class TrashCan : MonoBehaviour
    {
        private void Awake()
        {
            // Ensure InteractableObject is present for raycasting and interaction highlighting
            if (GetComponent<InteractableObject>() == null)
            {
                gameObject.AddComponent<InteractableObject>();
            }
        }

        /// <summary>
        /// Interaction called when player attempts to drop an item over the trash can.
        /// Destroys the held item completely and returns true if an item was trashed.
        /// </summary>
        public bool OnGrabDrop(PlayerInventory inventory)
        {
            if (inventory == null || !inventory.HasItem) return false;

            GameObject itemToDestroy = inventory.DropItem();
            if (itemToDestroy != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(itemToDestroy);
                }
                else
                {
                    DestroyImmediate(itemToDestroy);
                }
                return true;
            }

            return false;
        }
    }
}
