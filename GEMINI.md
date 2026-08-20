# Over Enchanted - Unity Project Guidelines & Best Practices

This document outlines mandatory guidelines for AI assistants working on the *Over Enchanted* Unity codebase.

---

## 1. Composition Over Inheritance (Mandatory Unity Practice)
- **Do NOT default to class inheritance** for game mechanics or object behaviors.
- **Always prefer Component-Based Design**:
  - Keep visual/interaction behaviors (e.g. `InteractableObject` for raycast highlights and rendering) as **standalone components**.
  - Keep gameplay logic behaviors (e.g. `Table`, `IngredientBox`, `Stove`, `Trash`) as **separate, dedicated `MonoBehaviour` components**.
  - Attach **both components** to game object prefabs.
- **Why**: Inheriting from visual components leads to broken material instantiation, lifecycle bugs (`Awake`/`OnEnable` overrides), and unnecessary tight coupling.

---

## 2. Architecture & Single Responsibility (SRP)
- **Player Input (`PlayerInput`)**: Handles direct input reading and fires events (including per-tap `OnDashPressed`).
- **Player Movement (`PlayerMovement`)**: Handles camera-relative movement and tap-activated Dash impulse bursts (`dashSpeed = 16f`, `dashDuration = 0.18s`).
- **Player Interactor (`PlayerInteractor`)**: Spherecasts for targets and dispatches input events to the target's components.
- **Player Inventory (`PlayerInventory`)**: Manages held item state, hand sockets, and inventory transfers.
- **Interactables (`InteractableObject`)**: Manages raycast highlighting and delegates interactions to specific gameplay components (`Table`, `IngredientBox`, `TrashCan`, `IngredientItem`).
- **Gameplay Objects (`Table`, `IngredientItem`, `TrashCan`, etc.)**: Focus exclusively on their specific game mechanics (e.g. placing items, dispensing ingredients, trashing items, picking items up off floor/tables).

---

## 3. Floor Drop vs. Object Interaction Rules
- **Floor Drop Condition**: `PlayerInventory.DropItem()` (dropping items on the floor) MUST ONLY occur when `PlayerInteractor.CurrentTarget == null` (i.e. facing empty floor / no interactable object).
- **Targeting an Interactable Object**: When `CurrentTarget != null`, pressing grab/drop MUST NEVER drop the held item on the floor. Instead, delegate the action to `CurrentTarget.OnGrabDrop(playerInventory)` and return immediately. Other interactable objects have their own rules for grab/drop input.

---

## 4. Physics, Scale & Mesh Deformation Prevention
- **Scale Deformation Prevention (`ItemScaleTracker`)**: Items maintain their original prefab scale (`OriginalScale`) captured via `ItemScaleTracker`. When parenting items to sockets or containers (e.g. `HoldPoint`, `Table.ItemSlot`), scale compensation (`originalScale / parent.lossyScale`) is applied so the world scale (`lossyScale`) remains identical to the prefab scale. When unparenting (`SetParent(null)`), `localScale` is restored to `originalScale`.
- **Item in Hand**:
  - `Rigidbody`: Kinematic (`isKinematic = true`), `detectCollisions = false`, velocities zeroed.
  - `Collider`: Disabled (`col.enabled = false`).
  - `Transform`: Parented to `PlayerInventory.HoldPoint` with scale compensation.
- **Item on Table / Counter**:
  - `Rigidbody`: Disabled physics simulation (`isKinematic = true`, `useGravity = false`, `detectCollisions = false`). No active physics needed on static surfaces.
  - `Collider`: Enabled (`col.enabled = true`) for raycasting and interaction.
  - `Transform`: Parented to `Table.ItemSlot` with scale compensation.
- **Item Dropped on Floor**:
  - `Rigidbody`: Active (`isKinematic = false`, `useGravity = true`, `detectCollisions = true`). Auto-created by `PlayerInventory.DropItem()` if missing.
  - `Collider`: Enabled (`col.enabled = true`).
  - `Transform`: Unparented (`transform.SetParent(null)`), `localScale = originalScale`.

---

## 5. Testability & Multi-Environment Support
- Write NUnit EditMode tests for all inventory and interaction state transitions under `Assets/Modules/Tests/EditMode/`.
- Ensure component code supports both runtime and EditMode execution (e.g. `Application.isPlaying ? Destroy(obj) : DestroyImmediate(obj)`).
- Maintain clear namespace boundaries (`OverEnchanted.Player`, `OverEnchanted.Interaction`, `OverEnchanted.Ingredients`, `OverEnchanted.Tests`).
