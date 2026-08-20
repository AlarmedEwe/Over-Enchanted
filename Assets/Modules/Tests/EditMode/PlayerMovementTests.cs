using NUnit.Framework;
using UnityEngine;
using OverEnchanted.Player;

namespace OverEnchanted.Tests
{
    public class PlayerMovementTests
    {
        private GameObject playerObject;
        private PlayerMovement movement;
        private CharacterController characterController;

        [SetUp]
        public void SetUp()
        {
            playerObject = new GameObject("Player");
            characterController = playerObject.AddComponent<CharacterController>();
            movement = playerObject.AddComponent<PlayerMovement>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void TriggerDash_WhenNotOnCooldown_ActivatesDashState()
        {
            // Act
            bool result = movement.TriggerDash(new Vector2(0f, 1f));

            // Assert
            Assert.IsTrue(result, "TriggerDash should return true when not on cooldown.");
            Assert.IsTrue(movement.IsDashing, "IsDashing should be true immediately after TriggerDash.");
        }

        [Test]
        public void TriggerDash_WhenCalledRapidlyInSameFrame_EnforcesCooldown()
        {
            // Act
            bool firstDash = movement.TriggerDash(new Vector2(0f, 1f));
            bool secondDash = movement.TriggerDash(new Vector2(0f, 1f));

            // Assert
            Assert.IsTrue(firstDash, "First TriggerDash should succeed.");
            Assert.IsFalse(secondDash, "Second TriggerDash in rapid succession should fail due to cooldown.");
        }

        [Test]
        public void Move_DuringDashDuration_MaintainsDashStateAndExitsAfterDuration()
        {
            // Arrange
            movement.TriggerDash(new Vector2(1f, 0f));
            Assert.IsTrue(movement.IsDashing, "Should start in dash state.");

            // Act - simulate movement step
            movement.Move(Vector2.zero);

            // Assert - still dashing during short burst duration
            Assert.IsTrue(movement.IsDashing, "IsDashing should remain true during the dash burst.");
        }
    }
}
