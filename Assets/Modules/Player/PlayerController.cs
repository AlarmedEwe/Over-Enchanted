using UnityEngine;

namespace OverEnchanted.Player
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerController : MonoBehaviour
    {
        public PlayerInput Input { get; private set; }
        public PlayerMovement Movement { get; private set; }

        private void Awake()
        {
            Input = GetComponent<PlayerInput>();
            Movement = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            if (Input != null && Movement != null)
            {
                Movement.Move(Input.MoveInput, Input.IsSprinting);
            }
        }
    }
}
