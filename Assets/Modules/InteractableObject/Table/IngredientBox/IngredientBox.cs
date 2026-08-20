using UnityEngine;
using OverEnchanted.Player;

namespace OverEnchanted.Interaction
{
    public class IngredientBox : MonoBehaviour
    {
        [Header("Box Settings")]
        [Tooltip("The ingredient prefab that this box dispenses.")]
        [SerializeField] private GameObject ingredientPrefab;

        public GameObject IngredientPrefab => ingredientPrefab;

        /// <summary>
        /// Attempts to dispense an ingredient into the player's inventory.
        /// Only succeeds if the player is empty-handed.
        /// </summary>
        public bool TryDispenseIngredient(PlayerInventory inventory)
        {
            if (inventory == null) return false;

            if (inventory.HasItem)
            {
                Debug.Log($"[IngredientBox] Player is already holding an item: {inventory.CurrentItem.name}");
                return false;
            }

            if (ingredientPrefab == null)
            {
                Debug.LogWarning($"[IngredientBox] No ingredient prefab assigned to {gameObject.name}!");
                return false;
            }

            // Instantiate prefab directly under player's HoldPoint socket
            Transform targetParent = inventory.HoldPoint != null ? inventory.HoldPoint : inventory.transform;
            GameObject newIngredient = Instantiate(ingredientPrefab, targetParent);
            newIngredient.transform.localPosition = Vector3.zero;
            newIngredient.transform.localRotation = Quaternion.identity;

            // Immediately neutralize physics and colliders before any frame passes
            foreach (var col in newIngredient.GetComponentsInChildren<Collider>(true))
            {
                col.enabled = false;
            }

            foreach (var rb in newIngredient.GetComponentsInChildren<Rigidbody>(true))
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.detectCollisions = false;
            }

            bool success = inventory.PickUpItem(newIngredient);

            if (success)
            {
                Debug.Log($"[IngredientBox] Dispensed {ingredientPrefab.name} to player.");
            }
            else
            {
                Destroy(newIngredient);
            }

            return success;
        }
    }
}
