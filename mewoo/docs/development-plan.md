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

**Blocked by:** None - can start immediately

**User stories covered:** As a developer, I can run an empty Mewoo desktop window.

### What to build

Create the initial solution/project skeleton and a runnable empty `Mewoo.App`. Establish how Mewoo references MewUI, and record any relevant differences between MewUI v0.15.2 and the local source being used.

### Acceptance criteria

- [ ] The repository contains the agreed project structure for App, Workbench, Core, Abstractions, and Quick Launcher.
- [ ] `Mewoo.App` starts and opens a desktop window.
- [ ] The chosen MewUI reference strategy is documented.
- [ ] Any v0.15.2/local API differences discovered during setup are recorded.

## Slice 002: Implement Borderless Window and Custom TitleBar Minimum Loop

**Type:** AFK

**Blocked by:** Slice 001

**User stories covered:** As a user, I see a Mewoo custom title bar and can move, minimize, maximize, restore, and close the window.

### What to build

Implement the V1 borderless main window and shell-owned custom TitleBar with app identity, menus, active title placeholder, shell action buttons, and window controls.

### Acceptance criteria

- [ ] The main window uses borderless chrome.
- [ ] TitleBar shows app icon, app name, version, File/View/Help placeholders, center title, theme action, always-on-top action, and window controls.
- [ ] Dragging the TitleBar moves the window.
- [ ] Double-clicking the TitleBar toggles maximize/restore.
- [ ] Minimize, maximize/restore, and close buttons work.
- [ ] Always-on-top toggles the window state.

## Slice 003: Implement Fixed Workbench Layout Skeleton

**Type:** AFK

**Blocked by:** Slice 002

**User stories covered:** As a user, I see a VSCode-like workbench with ActivityBar, Sidebar, MainArea, Panel support, and StatusBar.

### What to build

Build the V1 workbench layout shell with fixed regions, default dimensions, Sidebar and Panel resize/collapse support, MainArea tab strip placeholder, and StatusBar.

### Acceptance criteria

- [ ] ActivityBar is fixed at 48px.
- [ ] Sidebar defaults to 260px and can be resized within the documented bounds.
- [ ] MainArea contains a tab strip and active view host region.
- [ ] Panel exists, is hidden by default, and can be shown/collapsed.
- [ ] StatusBar is fixed at 24px.
- [ ] Sidebar collapsed state expands MainArea correctly.

## Slice 004: Implement Plugin Registration and Contribution Descriptors

**Type:** AFK

**Blocked by:** Slice 001

**User stories covered:** As a plugin author, I can register Activity, ViewContainer, MainView, Command, StatusBarItem, and theme token override contributions.

### What to build

Implement the fluent contribution registry backed by descriptors. Add ID format validation, duplicate ID detection, and automatic owner plugin assignment.

### Acceptance criteria

- [ ] Plugins can register ActivityBar item descriptors.
- [ ] Plugins can register ViewContainer and SidebarView descriptors.
- [ ] Plugins can register MainView descriptors with lazy view factories.
- [ ] Plugins can register Command descriptors.
- [ ] Plugins can register StatusBarItem descriptors.
- [ ] Duplicate contribution IDs fail registration.
- [ ] Invalid non-namespaced IDs fail registration.
- [ ] Each contribution is owned by the registering plugin.

## Slice 005: Render a Plugin-Contributed Activity, Sidebar, and MainArea View

**Type:** AFK

**Blocked by:** Slice 003, Slice 004

**User stories covered:** As a user, I can click an ActivityBar item and see the corresponding Sidebar and MainArea tab.

### What to build

Wire plugin contribution descriptors into the Workbench so one plugin can contribute an ActivityBar item, a Sidebar ViewContainer, and a MainArea view hosted through `IMewooView`.

### Acceptance criteria

- [ ] A plugin-contributed ActivityBar item renders in the ActivityBar.
- [ ] Selecting the ActivityBar item shows its ViewContainer in the Sidebar.
- [ ] A plugin-contributed MainArea view can be hosted.
- [ ] Unsupported native views render an error view instead of crashing the shell.
- [ ] Workbench selection is based on ActivityId, not PluginId.

## Slice 006: Implement Command Execution and MainArea Open Service

**Type:** AFK

**Blocked by:** Slice 004, Slice 005

**User stories covered:** As a plugin author, I can register a command that opens a MainArea view through the shell.

### What to build

Implement the command registry, command execution by ID, `CanExecute`, duplicate command rejection, and `IWorkbenchService.OpenMainViewAsync`.

### Acceptance criteria

- [ ] Commands execute by ID.
- [ ] Commands support asynchronous `ValueTask` handlers.
- [ ] `CanExecute` prevents unavailable commands from running.
- [ ] Duplicate command IDs are rejected.
- [ ] A command can open a MainArea view through the Workbench service.
- [ ] MainArea open behavior handles activation and deduplication for non-multiple views.

## Slice 007: Implement Plugin Lifecycle and Contribution Revocation

**Type:** AFK

**Blocked by:** Slice 004, Slice 006

**User stories covered:** As the shell, I can activate, deactivate, unload, and dispose plugins without orphaned UI.

### What to build

Implement the V1 plugin lifecycle states and shell orchestration for registration, activation, deactivation, unload, dispose, and failure handling.

### Acceptance criteria

- [ ] Plugins move through Created, Registered, Activated, Deactivated, Unloaded, and Disposed states.
- [ ] Any lifecycle failure moves the plugin to Failed.
- [ ] Register failure makes the plugin invisible.
- [ ] Activate failure hides plugin contributions and records the error.
- [ ] Deactivated plugin contributions are hidden by default.
- [ ] Unloading a plugin closes its owned MainArea tabs.
- [ ] Cleanup failures are recorded without blocking shell cleanup.

## Slice 008: Implement Theme Tokens and Dark/Light Switching

**Type:** AFK

**Blocked by:** Slice 003

**User stories covered:** As a user, I can switch between Dark and Light themes and see the Workbench update.

### What to build

Implement the V1 theme model, built-in Dark and Light themes, token-to-MewUI style mapping, and TitleBar theme action wiring.

### Acceptance criteria

- [ ] Theme model includes strongly typed base tokens and extension values.
- [ ] Dark and Light themes are available.
- [ ] Theme tokens apply to TitleBar, ActivityBar, Sidebar, MainArea, Panel, and StatusBar.
- [ ] TitleBar theme action switches themes.
- [ ] Plugin token overrides can be represented without allowing plugins to register full themes.

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

