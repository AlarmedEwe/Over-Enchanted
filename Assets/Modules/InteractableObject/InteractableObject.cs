using UnityEngine;
using OverEnchanted.Player;

namespace OverEnchanted.Interaction
{
    public class InteractableObject : MonoBehaviour
    {
        [Header("Highlight Settings")]
        [Tooltip("Factor to multiply original material color when highlighted.")]
        [SerializeField] private float brightnessFactor = 1.45f;

        [Tooltip("Optional custom highlight color tint offset.")]
        [SerializeField] private Color highlightTint = new Color(0.15f, 0.15f, 0.15f, 0f);

        [Header("References")]
        [SerializeField] private Renderer objectRenderer;

        private Material targetMaterial;
        private Color originalColor;
        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorID = Shader.PropertyToID("_Color");
        private int activeColorID;
        private bool isHighlighted;

        private void Awake()
        {
            if (objectRenderer == null)
            {
                objectRenderer = GetComponentInChildren<Renderer>();
            }

            if (objectRenderer != null)
            {
                // Instantiate material so we don't modify asset file on disk
                targetMaterial = objectRenderer.material;

                if (targetMaterial.HasProperty(BaseColorID))
                {
                    activeColorID = BaseColorID;
                }
                else if (targetMaterial.HasProperty(ColorID))
                {
                    activeColorID = ColorID;
                }

                if (activeColorID != 0)
                {
                    originalColor = targetMaterial.GetColor(activeColorID);
                }
            }
        }

        public virtual void SetHighlight(bool highlight)
        {
            if (isHighlighted == highlight || targetMaterial == null || activeColorID == 0) return;

            isHighlighted = highlight;

            if (isHighlighted)
            {
                // Multiply original color by brightness factor and add tint offset
                Color highlightedColor = (originalColor * brightnessFactor) + highlightTint;
                highlightedColor.a = originalColor.a;
                targetMaterial.SetColor(activeColorID, highlightedColor);
            }
            else
            {
                targetMaterial.SetColor(activeColorID, originalColor);
            }
        }

        public virtual void Interact()
        {
            Debug.Log($"[InteractableObject] Interacted with: {gameObject.name}");
        }

        /// <summary>
        /// Action triggered when player presses the Grab/Drop button (Space / Gamepad South).
        /// Automatically delegates to specific functional components like IngredientBox attached to this object.
        /// </summary>
        public virtual bool OnGrabDrop(PlayerInventory inventory)
        {
            IngredientBox ingredientBox = GetComponent<IngredientBox>();
            if (ingredientBox != null)
            {
                return ingredientBox.TryDispenseIngredient(inventory);
            }

            return false;
        }

        /// <summary>
        /// Action triggered when player presses the Work/Interact button (E / Gamepad North).
        /// </summary>
        public virtual bool OnWorkInteract(PlayerInventory inventory)
        {
            return false;
        }

        private void OnDisable()
        {
            if (isHighlighted)
            {
                SetHighlight(false);
            }
        }
    }
}
