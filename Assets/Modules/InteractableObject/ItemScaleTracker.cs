using UnityEngine;

namespace OverEnchanted.Interaction
{
    [DisallowMultipleComponent]
    public class ItemScaleTracker : MonoBehaviour
    {
        [SerializeField] private Vector3 originalScale = Vector3.one;
        private bool isInitialized = false;

        public Vector3 OriginalScale
        {
            get
            {
                EnsureInitialized();
                return originalScale;
            }
            set => originalScale = value;
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (isInitialized) return;
            
            // If transform is unparented, localScale is the true original prefab scale.
            // If transform is parented, lossyScale represents world scale before socket transformation.
            if (transform.parent == null)
            {
                originalScale = transform.localScale;
            }
            else
            {
                originalScale = transform.lossyScale;
            }

            // Ensure scale values are non-zero
            if (originalScale.x == 0) originalScale.x = 1f;
            if (originalScale.y == 0) originalScale.y = 1f;
            if (originalScale.z == 0) originalScale.z = 1f;

            isInitialized = true;
        }

        /// <summary>
        /// Parents this item to targetParent while compensating for parent scale to prevent mesh deformation.
        /// </summary>
        public void SetParentPreservingScale(Transform targetParent)
        {
            EnsureInitialized();

            transform.SetParent(targetParent, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (targetParent != null)
            {
                Vector3 pScale = targetParent.lossyScale;
                if (pScale.x != 0 && pScale.y != 0 && pScale.z != 0)
                {
                    transform.localScale = new Vector3(
                        originalScale.x / pScale.x,
                        originalScale.y / pScale.y,
                        originalScale.z / pScale.z
                    );
                }
                else
                {
                    transform.localScale = originalScale;
                }
            }
            else
            {
                transform.localScale = originalScale;
            }
        }

        /// <summary>
        /// Unparents this item to root while restoring its exact original prefab scale.
        /// </summary>
        public void UnparentPreservingScale()
        {
            EnsureInitialized();
            transform.SetParent(null);
            transform.localScale = originalScale;
        }
    }
}
