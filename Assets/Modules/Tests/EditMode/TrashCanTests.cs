using NUnit.Framework;
using UnityEngine;
using OverEnchanted.Player;
using OverEnchanted.Interaction;

namespace OverEnchanted.Tests
{
    public class TrashCanTests
    {
        private GameObject playerObject;
        private PlayerInventory inventory;
        private GameObject trashObject;
        private TrashCan trashCan;

        [SetUp]
        public void SetUp()
        {
            playerObject = new GameObject("Player");
            inventory = playerObject.AddComponent<PlayerInventory>();

            trashObject = new GameObject("TrashCan");
            trashCan = trashObject.AddComponent<TrashCan>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(trashObject);
        }

        [Test]
        public void OnGrabDrop_WhenPlayerIsHoldingItem_DestroysItemAndClearsHand()
        {
            // Arrange
            GameObject item = new GameObject("TrashableIngredient");
            item.AddComponent<BoxCollider>();
            item.AddComponent<Rigidbody>();
            inventory.PickUpItem(item);

            Assert.IsTrue(inventory.HasItem, "Player should start holding the item.");

            // Act
            InteractableObject interactable = trashObject.GetComponent<InteractableObject>();
            bool result = interactable.OnGrabDrop(inventory);

            // Assert
            Assert.IsTrue(result, "OnGrabDrop should return true when trashing a held item.");
            Assert.IsFalse(inventory.HasItem, "Player hand should be empty after trashing.");
            Assert.IsNull(inventory.CurrentItem, "CurrentItem should be null after trashing.");
        }

        [Test]
        public void OnGrabDrop_WhenPlayerIsEmptyHanded_ReturnsFalse()
        {
            // Arrange
            Assert.IsFalse(inventory.HasItem, "Player hand should start empty.");

            // Act
            InteractableObject interactable = trashObject.GetComponent<InteractableObject>();
            bool result = interactable.OnGrabDrop(inventory);

            // Assert
            Assert.IsFalse(result, "OnGrabDrop should return false when player holds no item.");
        }
    }
}
