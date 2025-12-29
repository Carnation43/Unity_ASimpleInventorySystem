## Core Architecture Refactor
- **InventoryManager.cs**: [Modified] Replaced native C# events with `InventoryEventChannel` to decouple the UI, and added `GetItemCount` method for crafting system queries.
- **InventoryController.cs**: [New] Independent controller separated from the original `MenuController`, specifically responsible for focus management, panel switching (Upgrade/Crafting), and input response within the inventory page.
- **MenuController.cs**: [Refactored] Transformed into a generic "Page Manager" that manages all sub-pages (Inventory, Recipe Book, Side Menu) via the `IMenuPage` interface, and added support for Rest Mode.
- **SelectionStateManager.cs** (formerly `MenuStateManager.cs`): [Refactored] Renamed and implemented the `IResettableUI` interface to generically track and reset currently selected UI objects.
- **BaseGridView.cs**: [New] Generic base class extracting grid generation, object pooling, and refresh animation logic from the original `InventoryView` for reuse by Inventory and Recipe Book.
- **BaseTabsManager.cs**: [New] Generic base class extracting tab navigation, cooldown, and visual feedback logic from the original `TabsManager`.
- **BaseTab.cs**: [New] Generic base class extracting common logic for individual tab buttons.
- **IGridView.cs** / **ISlotUI.cs**: [New] Defined generic interfaces for grid views and slot UIs to decouple the navigation system.

## Inventory System Updates
- **InventoryView.cs**: [Refactored] Changed to inherit from `BaseGridView`, removed significant redundant code, retaining only radial menu masking and focus handling logic after item consumption.
- **InventorySlotUI.cs**: [Modified] Implemented the `ISlotUI` interface to adapt to the new generic grid system.
- **TooltipViewController.cs**: [Modified] Implemented `IResettableUI` interface for state reset and switched to `InventoryEventChannel` for monitoring data changes.
- **TooltipView.cs** / **TooltipAnimator.cs**: [Modified] Implemented `IResettableUI` interface, added support for details button animation, and optimized display state management.
- **TooltipPosition.cs**: [Modified] Removed pivot easing animation and changed to instant switching to resolve jitter issues during position following.
- **TabsManager.cs** / **Tab.cs**: [Refactored] Changed to inherit from their corresponding Base classes to adapt to the new architecture.
- **ItemActionManager.cs**: [Modified] Switched to `ItemActionEventChannel` for receiving commands and integrated VFX (`CharacterPanelVfxChannel`) and audio channels.
- **InventoryAnimator.cs**: [Modified] Implemented `IResettableUI` and added `FadeOnly` mode to support more flexible panel animations.
- **DetailsContent.cs**: [Modified] Implemented `IResettableUI`, removed singleton registration (lifecycle now managed by controller), and fixed typewriter effect reset logic.

## Recipe Book System
- **RecipeBookManager.cs**: [New] Handles backend logic for recipe data, including recipe acquisition, unlock status management (consuming Inspiration points), and category filtering.
- **RecipeBookController.cs**: [New] Core controller for the Recipe Book page, coordinating data, view, and input, implementing complex "Hold to Unlock" interaction logic.
- **RecipeBookView.cs**: [New] Grid view for the Recipe Book, inheriting from `BaseGridView` and reusing generic grid display logic.
- **RecipeDetailsView.cs**: [New] Manages the right-side details panel in the Recipe Book, displaying recipe information, required ingredients list, and current owned/craftable quantities.
- **RecipeSlotUI.cs**: [New] UI controller for individual recipe slots, containing visual toggling for locked/unlocked states, long-press progress animation, and "New" indicators.
- **RecipeBookTabsManager.cs** / **RecipeTab.cs**: [New] Inherit from Base classes, implementing category tab logic specific to the Recipe Book.
- **RecipeStatus.cs**: [New] Runtime wrapper class for `Recipe` assets, used to store unlock status and "New" tags.
- **Recipe.cs**: [Modified] Added `filterCategory` (classification) and `inspirationCost` (unlock cost) fields, and added editor data validation logic.
- **RecipeIngredientSlotUI.cs**: [New] Used in the details page to display the required amount and stock amount for a single crafting ingredient.
- **InspirationView.cs**: [New] Specifically used to display Inspiration points and handle particle flight effects during unlocking.

## Rest System & Side Menu
- **RestSystemController.cs**: [New] Manages the flow of entering/exiting the resting state, including locking player input, triggering cutscenes, saving the game, and opening the side menu.
- **SideMenuController.cs**: [New] Logic controller for the side menu in the resting interface, responsible for button navigation and page request forwarding.
- **SideMenuView.cs**: [New] Handles the visual presentation (fade in/out) of the side menu and full-screen black fade transitions.
- **SideMenuButton.cs**: [New] Side menu button component, including selection breathing animation, confirmation feedback, and red dot notification features.
- **SideMenuType.cs**: [New] Defined an enumeration for side menu button functions (e.g., Inventory, Recipe, Depart).

## Player Gameplay & FSM
- **PlayerCharacter.cs**: [New] Master control script for the player character, responsible for integrating the state machine, physics motor, and data settings, and distributing input events.
- **PlayerStateMachine.cs** / **PlayerState.cs**: [New] Built a Finite State Machine (FSM) architecture, defining the base class for states and switching logic.
- **PlayerState_Grounded.cs**: [New] Implemented ground state logic, including idle, walking, running, and state transitions.
- **PlayerState_InAir.cs**: [New] Implemented in-air state logic, containing Coyote Time, Jump Buffer, and air mobility control.
- **PlayerState_Resting.cs**: [New] Implemented resting state logic, disabling movement input and triggering active animations.
- **PlayerMotor.cs**: [New] Physics executor, encapsulating Rigidbody2D movement, jumping, gravity adjustment, and ground detection logic.
- **PlayerSettings.cs**: [New] Data configuration script, centrally managing all movement-related parameters (speed, jump force, game feel tuning parameters).
- **PlayerTriggerZone.cs**: [New] Generic trigger script used to detect player entry/exit of zones (like resting points) and trigger UnityEvents.

## Input & Audio Systems
- **UserInput.cs**: [Modified] Added mouse position tracking, added `OnRun` and `OnCancel` actions, and introduced `IsRadialMenuHeldDown` static lock to prevent operation conflicts.
- **InputEventChannel.cs**: [Modified] Added `IsInputLocked` state property for external query, and extended new events like Cancel and Run.
- **GridNavigationHandler.cs**: [Refactored] Generic version of the original `InventoryNavigationHandler`, supporting grid navigation in any `IGridView`, and added input interruption protection.
- **InventoryEventChannel.cs**: [New] Event channel specifically designed to decouple InventoryManager and UI.
- **ItemActionEventChannel.cs**: [New] Defined data structures and event channels for item action requests.
- **AudioManager.cs**: [Modified] Integrated `AudioMixer`, supporting dynamic effects like "muffling/lowering volume" on background music when opening menus.

## Visuals & Rendering
- **CinematicManager.cs**: [New] Master control for cutscenes, coordinating black bars, camera zoom, and black screen fades.
- **CinematicBars.cs**: [New] Implemented the sliding in/out effect of top and bottom black bars for cinematic mode.
- **CinemachineZoomController.cs**: [New] Controls zoom and composition panning (Pan) for the Cinemachine virtual camera, used for close-up shots during resting.
- **BackgroundController.cs**: [New] Implemented parallax scrolling and infinite looping logic for 2D backgrounds.
- **BlurManager.cs** / **RecipePanelLogic.cs**: [New] Implemented a high-performance UI background Gaussian blur system, including downsampling and texture caching mechanisms.
- **BaseIconAnimationController.cs**: [Modified] Added state reset logic on `OnDisable`, fixing bugs where icon animations persisted after closing the menu.
- **ItemParticleController.cs**: [Modified] Added callback function after reaching the target and optimized suction logic at the end of particle flight to prevent orbiting.
- **TypewriterEffect.cs** / **SequenceController.cs**: [Modified] Implemented `IResettableUI` interface, enhanced null checking, and fixed coroutine errors in specific situations.