# Mewoo Development Plan

This document is the local issue tracker for Mewoo V1. Each item is a tracer-bullet slice that should be independently demoable or verifiable.

## Milestones

### Milestone 1: Runnable Shell Skeleton

Goal: start the app, show the custom shell, and prove that plugins can open MainArea content through commands.

Includes slices 001-006.

### Milestone 2: Usable Plugin Host

Goal: complete plugin lifecycle, theme switching, shell persistence, and the Quick Launcher plugin.

Includes slices 007-012.

### Milestone 3: Human UI Review

Goal: review screenshots/running UI against the ASCII visual design and tune the first usable experience.

Includes slice 013.

## Slice 001: Lock MewUI Baseline and Create Runnable App Skeleton

**Type:** AFK

**Status:** Done

**Blocked by:** None - can start immediately

**User stories covered:** As a developer, I can run an empty Mewoo desktop window.

### What to build

Create the initial solution/project skeleton and a runnable empty `Mewoo.App`. Establish how Mewoo references MewUI, and record any relevant differences between MewUI v0.15.2 and the local source being used.

### Acceptance criteria

- [x] The repository contains the agreed project structure for App, Workbench, Core, Abstractions, and Quick Launcher.
- [x] `Mewoo.App` starts and opens a desktop window.
- [x] The chosen MewUI reference strategy is documented.
- [x] Any v0.15.2/local API differences discovered during setup are recorded.

### Implementation notes

Mewoo targets .NET 10 and references the `Aprillz.MewUI` NuGet package at version `0.15.2`.

The local MewUI source at `D:\Code\github\MewUI` was used as implementation reference, especially Gallery startup and `NativeCustomWindow`.

The generated solution uses `.slnx`, matching the .NET 10 SDK behavior in this workspace.

## Slice 002: Implement Borderless Window and Custom TitleBar Minimum Loop

**Type:** AFK

**Status:** Done

**Blocked by:** Slice 001

**User stories covered:** As a user, I see a Mewoo custom title bar and can move, minimize, maximize, restore, and close the window.

### What to build

Implement the V1 borderless main window and shell-owned custom TitleBar with app identity, menus, active title placeholder, shell action buttons, and window controls.

### Acceptance criteria

- [x] The main window uses borderless chrome.
- [x] TitleBar shows app icon, app name, version, File/View/Help placeholders, center title, theme action, always-on-top action, and window controls.
- [x] Dragging the TitleBar moves the window.
- [x] Double-clicking the TitleBar toggles maximize/restore.
- [x] Minimize, maximize/restore, and close buttons work.
- [x] Always-on-top toggles the window state.

### Implementation notes

`MewooNativeWindow` is based on MewUI's native custom window sample and owns the shell TitleBar chrome.

File/View/Help and Theme are currently shell-owned placeholders. Theme behavior is completed later in Slice 008.

## Slice 003: Implement Fixed Workbench Layout Skeleton

**Type:** AFK

**Status:** Done

**Blocked by:** Slice 002

**User stories covered:** As a user, I see a VSCode-like workbench with ActivityBar, Sidebar, MainArea, Panel support, and StatusBar.

### What to build

Build the V1 workbench layout shell with fixed regions, default dimensions, Sidebar and Panel resize/collapse support, MainArea tab strip placeholder, and StatusBar.

### Acceptance criteria

- [x] ActivityBar is fixed at 48px.
- [x] Sidebar defaults to 260px and can be resized within the documented bounds.
- [x] MainArea contains a tab strip and active view host region.
- [x] Panel exists, is hidden by default, and can be shown/collapsed.
- [x] StatusBar is fixed at 24px.
- [x] Sidebar collapsed state expands MainArea correctly.

### Implementation notes

`WorkbenchState` owns Sidebar collapsed/width and Panel visible/height state.

Sidebar and Panel resize grips use MewUI mouse capture and update state-driven dimensions. State persistence is intentionally deferred to Slice 009.

## Slice 004: Implement Plugin Registration and Contribution Descriptors

**Type:** AFK

**Status:** Done

**Blocked by:** Slice 001

**User stories covered:** As a plugin author, I can register Activity, ViewContainer, MainView, Command, StatusBarItem, and theme token override contributions.

### What to build

Implement the fluent contribution registry backed by descriptors. Add ID format validation, duplicate ID detection, and automatic owner plugin assignment.

### Acceptance criteria

- [x] Plugins can register ActivityBar item descriptors.
- [x] Plugins can register ViewContainer and SidebarView descriptors.
- [x] Plugins can register MainView descriptors with lazy view factories.
- [x] Plugins can register Command descriptors.
- [x] Plugins can register StatusBarItem descriptors.
- [x] Duplicate contribution IDs fail registration.
- [x] Invalid non-namespaced IDs fail registration.
- [x] Each contribution is owned by the registering plugin.

## Slice 005: Render a Plugin-Contributed Activity, Sidebar, and MainArea View

**Type:** AFK

**Status:** Done

**Blocked by:** Slice 003, Slice 004

**User stories covered:** As a user, I can click an ActivityBar item and see the corresponding Sidebar and MainArea tab.

### What to build

Wire plugin contribution descriptors into the Workbench so one plugin can contribute an ActivityBar item, a Sidebar ViewContainer, and a MainArea view hosted through `IMewooView`.

### Acceptance criteria

- [x] A plugin-contributed ActivityBar item renders in the ActivityBar.
- [x] Selecting the ActivityBar item shows its ViewContainer in the Sidebar.
- [x] A plugin-contributed MainArea view can be hosted.
- [x] Unsupported native views render an error view instead of crashing the shell.
- [x] Workbench selection is based on ActivityId, not PluginId.

### Implementation notes

Quick Launcher currently validates this slice as the first real plugin contribution path.

## Slice 006: Implement Command Execution and MainArea Open Service

**Type:** AFK

**Status:** Done

**Blocked by:** Slice 004, Slice 005

**User stories covered:** As a plugin author, I can register a command that opens a MainArea view through the shell.

### What to build

Implement the command registry, command execution by ID, `CanExecute`, duplicate command rejection, and `IWorkbenchService.OpenMainViewAsync`.

### Acceptance criteria

- [x] Commands execute by ID.
- [x] Commands support asynchronous `ValueTask` handlers.
- [x] `CanExecute` prevents unavailable commands from running.
- [x] Duplicate command IDs are rejected.
- [x] A command can open a MainArea view through the Workbench service.
- [x] MainArea open behavior handles activation and deduplication for non-multiple views.

### Implementation notes

`quickLauncher.open` is executed on load to validate command execution and `IWorkbenchService.OpenMainViewAsync`.

Command palette and keybinding UI are intentionally outside Milestone 1.

## Slice 007: Implement Plugin Lifecycle and Contribution Revocation

**Type:** AFK

**Status:** Done

**Blocked by:** Slice 004, Slice 006

**User stories covered:** As the shell, I can activate, deactivate, unload, and dispose plugins without orphaned UI.

### What to build

Implement the V1 plugin lifecycle states and shell orchestration for registration, activation, deactivation, unload, dispose, and failure handling.

### Acceptance criteria

- [x] Plugins move through Created, Registered, Activated, Deactivated, Unloaded, and Disposed states.
- [x] Any lifecycle failure moves the plugin to Failed.
- [x] Register failure makes the plugin invisible.
- [x] Activate failure hides plugin contributions and records the error.
- [x] Deactivated plugin contributions are hidden by default.
- [x] Unloading a plugin closes its owned MainArea tabs.
- [x] Cleanup failures are recorded without blocking shell cleanup.

### Implementation notes

`MewooPluginHost` now keeps registered contributions separate from visible activated contributions.

Commands are registered only while plugin contributions are visible and are revoked when a plugin is deactivated or unloaded.

Workbench renders `VisibleContributions`, listens for contribution changes, and removes MainArea tabs whose descriptors are no longer visible.

Quick Launcher validates the activation path during app startup.

## Slice 008: Implement Theme Tokens and Dark/Light Switching

**Type:** AFK

**Status:** Done

**Blocked by:** Slice 003

**User stories covered:** As a user, I can switch between Dark and Light themes and see the Workbench update.

### What to build

Implement the V1 theme model, built-in Dark and Light themes, token-to-MewUI style mapping, and TitleBar theme action wiring.

### Acceptance criteria

- [x] Theme model includes strongly typed base tokens and extension values.
- [x] Dark and Light themes are available.
- [x] Theme tokens apply to TitleBar, ActivityBar, Sidebar, MainArea, Panel, and StatusBar.
- [x] TitleBar theme action switches themes.
- [x] Plugin token overrides can be represented without allowing plugins to register full themes.

### Implementation notes

`Mewoo.Abstractions` defines strongly typed base theme tokens and built-in Dark/Light theme descriptors.

`Mewoo.Workbench` owns `MewooThemeController`, maps the current Mewoo theme to MewUI `ThemeVariant`, and exposes a TitleBar Theme action.

Plugin token overrides are represented by contribution descriptors. Full plugin-provided themes remain out of scope for V1.

## Slice 009: Implement Shell State Persistence

**Type:** AFK

**Blocked by:** Slice 003, Slice 008

**User stories covered:** As a user, I keep theme, always-on-top, Sidebar, and Panel preferences across restarts.

### What to build

Implement JSON-backed shell state storage behind abstractions. Persist the V1 shell state fields owned by Mewoo.

### Acceptance criteria

- [ ] Shell state stores active Activity ID.
- [ ] Shell state stores Sidebar collapsed state and width.
- [ ] Shell state stores Panel visibility and height.
- [ ] Shell state stores active/open MainArea view identities where available.
- [ ] Shell state stores theme ID.
- [ ] Shell state stores always-on-top state.
- [ ] State is saved and restored across app restarts.

## Slice 010: Implement Quick Launcher JSON Model and List UI

**Type:** AFK

**Blocked by:** Slice 005, Slice 006

**User stories covered:** As a user, I can view grouped URL, file, executable, and script launch items.

### What to build

Implement the Quick Launcher plugin data model, JSON configuration loading, ActivityBar contribution, Sidebar search/groups/recent UI, and MainArea list UI.

### Acceptance criteria

- [ ] Quick Launcher registers its ActivityBar item.
- [ ] Quick Launcher loads groups and items from JSON.
- [ ] Launcher item kinds include URL, file, executable, and script.
- [ ] Sidebar shows search, groups, and recent sections.
- [ ] MainArea shows a searchable list of launcher items.
- [ ] StatusBar can show item count or selected item.

## Slice 011: Implement Quick Launcher Actions and Status Feedback

**Type:** AFK

**Blocked by:** Slice 010

**User stories covered:** As a user, I can run the selected launch item and see the result.

### What to build

Implement Quick Launcher commands and action execution for URL, file, executable, and script targets. Update StatusBar with selected item and last run result.

### Acceptance criteria

- [ ] `quickLauncher.open` opens or focuses the launcher MainArea tab.
- [ ] `quickLauncher.focusSearch` focuses the launcher search field.
- [ ] `quickLauncher.runSelected` runs the selected item.
- [ ] `quickLauncher.reload` reloads JSON configuration.
- [ ] `quickLauncher.openConfig` opens the configuration target.
- [ ] URL targets open in the default browser.
- [ ] File, executable, and script targets invoke the expected local target.
- [ ] StatusBar updates after selection and execution.

## Slice 012: Implement Error Views, Logs Panel, and Plugin Failure Feedback

**Type:** AFK

**Blocked by:** Slice 007, Slice 011

**User stories covered:** As a user, I can understand plugin/view failures and open logs.

### What to build

Add visible error states for plugin activation failures and unsupported view hosting. Add a basic logs panel that can be opened from error UI.

### Acceptance criteria

- [ ] Unsupported native views show an error view in MainArea.
- [ ] Plugin activation failures show a Sidebar or Activity-level unavailable state.
- [ ] Error views include Open Logs action.
- [ ] Logs panel can show shell/plugin lifecycle errors.
- [ ] Plugin failures do not crash the shell.

## Slice 013: Human UI Review and Visual Tuning

**Type:** HITL

**Blocked by:** Slice 012

**User stories covered:** As the project owner, I confirm the first usable Mewoo UI matches the intended visual design.

### What to build

Run the app, compare it against the ASCII UI visual design, capture feedback, and tune spacing, sizing, colors, hover states, active states, and region behavior.

### Acceptance criteria

- [ ] The running UI is reviewed against `docs/design/v1-ui-visual-design.md`.
- [ ] TitleBar behavior and density are approved or adjusted.
- [ ] ActivityBar, Sidebar, MainArea, Panel, and StatusBar sizing are approved or adjusted.
- [ ] Dark and Light themes are visually checked.
- [ ] Quick Launcher first-use flow is checked.
- [ ] Follow-up issues are created locally for anything intentionally deferred.

