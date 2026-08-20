using UnityEngine;
using OverEnchanted.Interaction;

namespace OverEnchanted.Player
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerInteractor))]
    public class PlayerController : MonoBehaviour
    {
        public PlayerInput Input { get; private set; }
        public PlayerMovement Movement { get; private set; }
        public PlayerInteractor Interactor { get; private set; }

        private void Awake()
        {
            Input = GetComponent<PlayerInput>();
            Movement = GetComponent<PlayerMovement>();
            Interactor = GetComponent<PlayerInteractor>();
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
