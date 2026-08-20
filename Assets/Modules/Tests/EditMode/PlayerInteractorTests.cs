using NUnit.Framework;
using UnityEngine;
using OverEnchanted.Player;
using OverEnchanted.Interaction;

namespace OverEnchanted.Tests
{
    public class PlayerInteractorTests
    {
        private GameObject playerObject;
        private PlayerInput playerInput;
        private PlayerInventory playerInventory;
        private PlayerInteractor interactor;

        private GameObject boxObject;
        private InteractableObject interactable;
        private IngredientBox box;
        private GameObject prefab;

        [SetUp]
        public void SetUp()
        {
            playerObject = new GameObject("Player");
            playerInput = playerObject.AddComponent<PlayerInput>();
            playerInventory = playerObject.AddComponent<PlayerInventory>();
            interactor = playerObject.AddComponent<PlayerInteractor>();

            prefab = new GameObject("TestPrefab");
            prefab.AddComponent<BoxCollider>();
            prefab.AddComponent<Rigidbody>();

            boxObject = new GameObject("Box");
            interactable = boxObject.AddComponent<InteractableObject>();
            box = boxObject.AddComponent<IngredientBox>();

            var field = typeof(IngredientBox).GetField("ingredientPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(box, prefab);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(boxObject);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void HandleGrabDrop_WhenFacingIngredientBoxAndEmptyHanded_PickUpsItemAndDoesNotDrop()
        {
            // Set target on interactor
            var property = typeof(PlayerInteractor).GetProperty("CurrentTarget");
            property.SetValue(interactor, interactable);

            // Invoke private HandleGrabDrop via reflection
            var handleMethod = typeof(PlayerInteractor).GetMethod("HandleGrabDrop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            handleMethod.Invoke(interactor, null);

            // Assert
            Assert.IsTrue(playerInventory.HasItem, "Player should hold the item after interacting with box.");
            Assert.IsNotNull(playerInventory.CurrentItem, "Held item must exist.");
            Assert.AreEqual(playerInventory.HoldPoint, playerInventory.CurrentItem.transform.parent, "Item must remain in player's hand!");

            Object.DestroyImmediate(playerInventory.CurrentItem);
        }

        [Test]
        public void HandleGrabDrop_WhenFacingIngredientBoxAndAlreadyHoldingItem_DropsItem()
        {
            // Arrange
            GameObject currentHeld = new GameObject("HeldItem");
            playerInventory.PickUpItem(currentHeld);

            var property = typeof(PlayerInteractor).GetProperty("CurrentTarget");
            property.SetValue(interactor, interactable);

            // Act
            var handleMethod = typeof(PlayerInteractor).GetMethod("HandleGrabDrop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            handleMethod.Invoke(interactor, null);

            // Assert
            Assert.IsFalse(playerInventory.HasItem, "Player should drop current held item when interacting with box while full.");
            Assert.IsNull(currentHeld.transform.parent, "Dropped item should be unparented.");

            Object.DestroyImmediate(currentHeld);
        }
    }
}
