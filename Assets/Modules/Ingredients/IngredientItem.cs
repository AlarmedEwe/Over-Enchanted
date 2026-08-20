using UnityEngine;

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
    }
}
