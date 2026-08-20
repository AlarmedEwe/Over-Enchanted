using NUnit.Framework;
using UnityEngine;
using OverEnchanted.Player;
using OverEnchanted.Ingredients;

namespace OverEnchanted.Tests
{
    public class PlayerInventoryTests
    {
        private GameObject playerObject;
        private PlayerInventory inventory;

        [SetUp]
        public void SetUp()
        {
            playerObject = new GameObject("Player");
            inventory = playerObject.AddComponent<PlayerInventory>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void PickUpItem_WhenEmpty_EquipsItemAndDisablesPhysics()
        {
            // Arrange
            GameObject item = new GameObject("TestItem");
            item.AddComponent<BoxCollider>();
            Rigidbody rb = item.AddComponent<Rigidbody>();
            rb.isKinematic = false;

            // Act
            bool result = inventory.PickUpItem(item);

            // Assert
            Assert.IsTrue(result, "PickUpItem should return true when player is empty-handed.");
            Assert.IsTrue(inventory.HasItem, "HasItem should be true after picking up.");
            Assert.AreEqual(item, inventory.CurrentItem, "CurrentItem should match picked up item.");
            Assert.AreEqual(inventory.HoldPoint, item.transform.parent, "Item should be parented to HoldPoint.");
            Assert.IsTrue(rb.isKinematic, "Rigidbody should be kinematic while held.");
            Assert.IsFalse(rb.detectCollisions, "detectCollisions should be false while held.");

            Object.DestroyImmediate(item);
        }

        [Test]
        public void PickUpItem_WhenAlreadyHoldingItem_ReturnsFalse()
        {
            // Arrange
            GameObject item1 = new GameObject("Item1");
            GameObject item2 = new GameObject("Item2");

            inventory.PickUpItem(item1);

            // Act
            bool result = inventory.PickUpItem(item2);

            // Assert
            Assert.IsFalse(result, "PickUpItem should return false if player already holds an item.");
            Assert.AreEqual(item1, inventory.CurrentItem, "CurrentItem should remain item1.");

            Object.DestroyImmediate(item1);
            Object.DestroyImmediate(item2);
        }

        [Test]
        public void DropItem_WhenHoldingItem_UnparentsAndReenablesPhysics()
        {
            // Arrange
            GameObject item = new GameObject("TestItem");
            item.AddComponent<BoxCollider>();
            Rigidbody rb = item.AddComponent<Rigidbody>();

            inventory.PickUpItem(item);

            // Act
            GameObject dropped = inventory.DropItem();

            // Assert
            Assert.IsNotNull(dropped, "Dropped item should not be null.");
            Assert.IsFalse(inventory.HasItem, "HasItem should be false after dropping.");
            Assert.IsNull(dropped.transform.parent, "Dropped item should have no parent.");
            Assert.IsFalse(rb.isKinematic, "Rigidbody should not be kinematic after drop.");
            Assert.IsTrue(rb.useGravity, "Gravity should be enabled after drop.");
            Assert.IsTrue(rb.detectCollisions, "detectCollisions should be true after drop.");

            Object.DestroyImmediate(item);
        }

        [Test]
        public void DropItem_WhenEmpty_ReturnsNull()
        {
            // Act
            GameObject dropped = inventory.DropItem();

            // Assert
            Assert.IsNull(dropped, "DropItem should return null when inventory is empty.");
        }

        [Test]
        public void PickUpIngredientItemFromFloor_WhenEmptyHanded_EquipsItem()
        {
            // Arrange
            GameObject floorItem = new GameObject("FloorTomato");
            floorItem.AddComponent<BoxCollider>();
            floorItem.AddComponent<Rigidbody>();
            IngredientItem ingredient = floorItem.AddComponent<IngredientItem>();

            // Act
            bool result = ingredient.OnGrabDrop(inventory);

            // Assert
            Assert.IsTrue(result, "OnGrabDrop on floor ingredient should return true when player is empty handed.");
            Assert.IsTrue(inventory.HasItem, "Player should hold the floor ingredient.");
            Assert.AreEqual(floorItem, inventory.CurrentItem, "Held item should match floor ingredient.");

            Object.DestroyImmediate(floorItem);
        }
    }
}
