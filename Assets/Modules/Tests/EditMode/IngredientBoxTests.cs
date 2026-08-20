using NUnit.Framework;
using UnityEngine;
using OverEnchanted.Player;
using OverEnchanted.Interaction;

namespace OverEnchanted.Tests
{
    public class IngredientBoxTests
    {
        private GameObject playerObject;
        private PlayerInventory inventory;
        private GameObject boxObject;
        private IngredientBox box;
        private GameObject prefab;

        [SetUp]
        public void SetUp()
        {
            playerObject = new GameObject("Player");
            inventory = playerObject.AddComponent<PlayerInventory>();

            prefab = new GameObject("PrefabIngredient");
            prefab.AddComponent<BoxCollider>();
            prefab.AddComponent<Rigidbody>();

            boxObject = new GameObject("IngredientBox");
            box = boxObject.AddComponent<IngredientBox>();

            // Assign private ingredientPrefab field using reflection
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
        public void TryDispenseIngredient_WhenEmptyHanded_DispensesAndEquipsItem()
        {
            // Act
            bool result = box.TryDispenseIngredient(inventory);

            // Assert
            Assert.IsTrue(result, "TryDispenseIngredient should return true when player is empty handed.");
            Assert.IsTrue(inventory.HasItem, "Player should now hold an item.");
            Assert.IsNotNull(inventory.CurrentItem, "CurrentItem should be non-null.");
            Assert.IsTrue(inventory.CurrentItem.name.StartsWith("PrefabIngredient"), "Item name should match prefab.");

            // Cleanup instance
            Object.DestroyImmediate(inventory.CurrentItem);
        }

        [Test]
        public void TryDispenseIngredient_WhenPlayerAlreadyHoldsItem_FailsAndDoesNotDispense()
        {
            // Arrange
            GameObject existingItem = new GameObject("ExistingItem");
            inventory.PickUpItem(existingItem);

            // Act
            bool result = box.TryDispenseIngredient(inventory);

            // Assert
            Assert.IsFalse(result, "TryDispenseIngredient should fail when player already has an item.");
            Assert.AreEqual(existingItem, inventory.CurrentItem, "Player's held item should remain unchanged.");

            Object.DestroyImmediate(existingItem);
        }
    }
}
