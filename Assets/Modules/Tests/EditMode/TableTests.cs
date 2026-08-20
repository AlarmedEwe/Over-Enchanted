using NUnit.Framework;
using UnityEngine;
using OverEnchanted.Player;
using OverEnchanted.Interaction;

namespace OverEnchanted.Tests
{
    public class TableTests
    {
        private GameObject playerObject;
        private PlayerInput playerInput;
        private PlayerInventory playerInventory;
        private PlayerInteractor interactor;

        private GameObject tableObject;
        private Table table;
        private GameObject testItem;

        [SetUp]
        public void SetUp()
        {
            playerObject = new GameObject("Player");
            playerInput = playerObject.AddComponent<PlayerInput>();
            playerInventory = playerObject.AddComponent<PlayerInventory>();
            interactor = playerObject.AddComponent<PlayerInteractor>();

            tableObject = new GameObject("Table");
            tableObject.AddComponent<BoxCollider>();
            InteractableObject interactable = tableObject.AddComponent<InteractableObject>();
            table = tableObject.AddComponent<Table>();
            table.EnsureItemSlot();

            testItem = new GameObject("TestItem");
            testItem.AddComponent<BoxCollider>();
            testItem.AddComponent<Rigidbody>();
        }

        [TearDown]
        public void TearDown()
        {
            if (playerObject != null) Object.DestroyImmediate(playerObject);
            if (tableObject != null) Object.DestroyImmediate(tableObject);
            if (testItem != null) Object.DestroyImmediate(testItem);
        }

        [Test]
        public void PlaceItem_WhenTableIsEmpty_ParentsItemAndRemovesRigidbody()
        {
            // Act
            bool result = table.PlaceItem(testItem);

            // Assert
            Assert.IsTrue(result, "PlaceItem should return true when table is empty.");
            Assert.IsTrue(table.HasItem, "Table should hold item.");
            Assert.AreEqual(testItem, table.CurrentItem, "CurrentItem should match placed item.");
            Assert.AreEqual(table.ItemSlot, testItem.transform.parent, "Item should be parented to Table ItemSlot.");
            
            Collider col = testItem.GetComponent<Collider>();
            Assert.IsNotNull(col, "Item must retain its Collider on table.");
            Assert.IsTrue(col.enabled, "Collider must be enabled while item is on table.");

            Rigidbody rb = testItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Assert.IsTrue(rb.isKinematic, "Rigidbody must be kinematic while item is sitting on table.");
            }
        }

        [Test]
        public void PlaceItem_OnScaledTable_CompensatesScaleToPreventMeshDeformation()
        {
            // Arrange: Custom item scale (like Tomato 0.8, 0.8, 0.8) and non-uniformly scaled table
            testItem.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            tableObject.transform.localScale = new Vector3(2.5f, 0.4f, 1.8f);

            // Act
            table.PlaceItem(testItem);

            // Assert: World lossyScale of item must match its exact prefab original scale (0.8, 0.8, 0.8)
            Vector3 lossyScale = testItem.transform.lossyScale;
            Assert.AreEqual(0.8f, lossyScale.x, 0.01f, "Item lossyScale.x must match original prefab scale.");
            Assert.AreEqual(0.8f, lossyScale.y, 0.01f, "Item lossyScale.y must match original prefab scale.");
            Assert.AreEqual(0.8f, lossyScale.z, 0.01f, "Item lossyScale.z must match original prefab scale.");
        }

        [Test]
        public void HandleGrabDrop_WhenHoldingItemAndFacingTable_PlacesItemOnTableNotFloor()
        {
            // Arrange: Player picks up testItem
            playerInventory.PickUpItem(testItem);
            Assert.IsTrue(playerInventory.HasItem, "Player should hold item initially.");

            // Set interactor target to table
            var property = typeof(PlayerInteractor).GetProperty("CurrentTarget");
            property.SetValue(interactor, tableObject.GetComponent<InteractableObject>());

            // Act: Trigger HandleGrabDrop on interactor
            var handleMethod = typeof(PlayerInteractor).GetMethod("HandleGrabDrop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            handleMethod.Invoke(interactor, null);

            // Assert
            Assert.IsFalse(playerInventory.HasItem, "Player inventory should be empty after placing item on table.");
            Assert.IsTrue(table.HasItem, "Table should hold the item after grab/drop interaction.");
            Assert.AreEqual(table.ItemSlot, testItem.transform.parent, "Item should be on top of table, not unparented/dropped to floor.");
            Assert.IsNull(testItem.GetComponent<Rigidbody>(), "Item on table must not have Rigidbody.");
        }

        [Test]
        public void HandleGrabDrop_WhenEmptyHandedAndFacingTableWithItem_PicksUpItemFromTable()
        {
            // Arrange: Place item on table
            table.PlaceItem(testItem);
            Assert.IsFalse(playerInventory.HasItem, "Player is empty handed initially.");
            Assert.IsTrue(table.HasItem, "Table holds item initially.");

            // Set interactor target to table
            var property = typeof(PlayerInteractor).GetProperty("CurrentTarget");
            property.SetValue(interactor, tableObject.GetComponent<InteractableObject>());

            // Act
            var handleMethod = typeof(PlayerInteractor).GetMethod("HandleGrabDrop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            handleMethod.Invoke(interactor, null);

            // Assert
            Assert.IsTrue(playerInventory.HasItem, "Player should pick up item from table.");
            Assert.IsFalse(table.HasItem, "Table should no longer hold item.");
            Assert.AreEqual(playerInventory.HoldPoint, testItem.transform.parent, "Item should be parented to Player HoldPoint.");
        }

        [Test]
        public void DropItem_AfterPickingUpFromTable_RestoresRigidbodyAndGravity()
        {
            // Arrange: Place item on table (rigidbody removed), then pick up into player inventory
            table.PlaceItem(testItem);
            table.OnGrabDrop(playerInventory);
            Assert.IsTrue(playerInventory.HasItem);

            // Act: Drop item from player inventory onto floor
            GameObject dropped = playerInventory.DropItem();

            // Assert
            Assert.IsNotNull(dropped);
            Rigidbody rb = dropped.GetComponent<Rigidbody>();
            Assert.IsNotNull(rb, "Dropped item must have Rigidbody recreated when dropped to floor.");
            Assert.IsTrue(rb.useGravity, "Rigidbody gravity must be enabled on floor drop.");
            Assert.IsFalse(rb.isKinematic, "Rigidbody must not be kinematic on floor drop.");
        }
    }
}
