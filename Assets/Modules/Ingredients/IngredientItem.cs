using UnityEngine;
using OverEnchanted.Interaction;
using OverEnchanted.Player;

namespace OverEnchanted.Ingredients
{
    [RequireComponent(typeof(Collider))]
    public class IngredientItem : MonoBehaviour
    {
        [Header("Ingredient Details")]
        [SerializeField] private string ingredientName = "Ingredient";
        [SerializeField] private Sprite icon;

        [Header("Physics Settings")]
        [SerializeField] private float mass = 0.5f;
        [SerializeField] private float drag = 2.0f;
        [SerializeField] private float angularDrag = 3.0f;

        public string IngredientName => ingredientName;
        public Sprite Icon => icon;

        private Rigidbody rb;

        private void Awake()
        {
            // Ensure InteractableObject is present so ingredients on floor or surfaces can be highlighted and picked back up
            if (GetComponent<InteractableObject>() == null)
            {
                gameObject.AddComponent<InteractableObject>();
            }

            rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = mass;
                rb.linearDamping = drag;
                rb.angularDamping = angularDrag;

                // Freeze X and Z rotation so ingredients stay upright on the floor without rolling endlessly
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

                // If instantiated directly under a socket/parent (e.g. HoldPoint), start as kinematic with collisions disabled
                if (transform.parent != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    rb.detectCollisions = false;
                }
            }
        }

        /// <summary>
        /// Interaction called when player attempts to grab/drop while targeting this ingredient.
        /// Allows the player to pick this item back up if their inventory is empty.
        /// </summary>
        public bool OnGrabDrop(PlayerInventory inventory)
        {
            if (inventory == null) return false;

            // If player is empty-handed, pick up this ingredient off the floor/surface
            if (!inventory.HasItem)
            {
                return inventory.PickUpItem(gameObject);
            }

            return false;
        }
    }
}
