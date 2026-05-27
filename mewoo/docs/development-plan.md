# Mewoo Development Plan

This document is the local issue tracker for Mewoo V1 and V2. Each item is a tracer-bullet slice that should be independently demoable or verifiable.

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

### Milestone 4: Quick Launcher Interaction Refinement

Goal: make the first real plugin feel operable through visible click targets and direct item execution.

Includes slice 014.

### V1 Baseline

Tag: `v1-baseline`

Baseline commit: V1 shell and Quick Launcher tracer bullet are usable enough to start the second-stage runtime plugin work.

### Milestone 5: Runtime Plugin Manifest Foundation

Goal: introduce the V2 runtime plugin shape without changing the V1 compiled plugin path.

Includes slices 015-016.

### Milestone 6: Runtime Plugin Loading

Goal: load trusted local plugin assemblies into Mewoo through the existing lifecycle and contribution model.

Includes slices 017-020.

### Milestone 7: Runtime Plugin Operations

Goal: make runtime plugins diagnosable, unloadable, and locally packageable.

Includes slices 021-023.

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

**Status:** Done

**Blocked by:** Slice 003, Slice 008

**User stories covered:** As a user, I keep theme, always-on-top, Sidebar, and Panel preferences across restarts.

### What to build

Implement JSON-backed shell state storage behind abstractions. Persist the V1 shell state fields owned by Mewoo.

### Acceptance criteria

- [x] Shell state stores active Activity ID.
- [x] Shell state stores Sidebar collapsed state and width.
- [x] Shell state stores Panel visibility and height.
- [x] Shell state stores active/open MainArea view identities where available.
- [x] Shell state stores theme ID.
- [x] Shell state stores always-on-top state.
- [x] State is saved and restored across app restarts.

### Implementation notes

Shell state is stored through `IStateStorage` and the JSON-backed `JsonFileStateStorage`.

`Mewoo.App` writes state under the user's local application data directory at `Mewoo/State`.

`WorkbenchStateSnapshot` captures active Activity, Sidebar and Panel layout, open MainArea views, active MainArea view, theme ID, and always-on-top state.

Workbench saves state after layout/theme/topmost/MainArea changes and restores it after compiled plugins are activated.

## Slice 010: Implement Quick Launcher JSON Model and List UI

**Type:** AFK

**Status:** Done

**Blocked by:** Slice 005, Slice 006

**User stories covered:** As a user, I can view grouped URL, file, executable, and script launch items.

### What to build

Implement the Quick Launcher plugin data model, JSON configuration loading, ActivityBar contribution, Sidebar search/groups/recent UI, and MainArea list UI.

### Acceptance criteria

- [x] Quick Launcher registers its ActivityBar item.
- [x] Quick Launcher loads groups and items from JSON.
- [x] Launcher item kinds include URL, file, executable, and script.
- [x] Sidebar shows search, groups, and recent sections.
- [x] MainArea shows a searchable list of launcher items.
- [x] StatusBar can show item count or selected item.

### Implementation notes

Quick Launcher now loads JSON configuration from the user's local application data directory at `Mewoo/QuickLauncher/launcher.json`.

If the configuration file does not exist, Quick Launcher creates a default configuration with Development and Trading groups.

The MainArea renders grouped launcher items as a list. Search input is visible in Sidebar and MainArea; interactive filtering is deferred to a later refinement.

## Slice 011: Implement Quick Launcher Actions and Status Feedback

**Type:** AFK

**Status:** Done

**Blocked by:** Slice 010

**User stories covered:** As a user, I can run the selected launch item and see the result.

### What to build

Implement Quick Launcher commands and action execution for URL, file, executable, and script targets. Update StatusBar with selected item and last run result.

### Acceptance criteria

- [x] `quickLauncher.open` opens or focuses the launcher MainArea tab.
- [x] `quickLauncher.focusSearch` focuses the launcher search field.
- [x] `quickLauncher.runSelected` runs the selected item.
- [x] `quickLauncher.reload` reloads JSON configuration.
- [x] `quickLauncher.openConfig` opens the configuration target.
- [x] URL targets open in the default browser.
- [x] File, executable, and script targets invoke the expected local target.
- [x] StatusBar updates after selection and execution.

### Implementation notes

Quick Launcher now maintains a selected launcher item and updates the StatusBar when an item is selected or a command runs.

`quickLauncher.runSelected` runs the selected item, or falls back to the first configured item when nothing has been selected.

URL, file, executable, and script targets are launched through `QuickLauncherLaunchService`. PowerShell scripts are invoked through `powershell -NoProfile -ExecutionPolicy Bypass -File`.

`quickLauncher.reload` reloads the JSON configuration and refreshes the launcher MainArea. `quickLauncher.openConfig` opens the JSON configuration file through the shell.

## Slice 012: Implement Error Views, Logs Panel, and Plugin Failure Feedback

**Type:** AFK

**Status:** Done

**Blocked by:** Slice 007, Slice 011

**User stories covered:** As a user, I can understand plugin/view failures and open logs.

### What to build

Add visible error states for plugin activation failures and unsupported view hosting. Add a basic logs panel that can be opened from error UI.

### Acceptance criteria

- [x] Unsupported native views show an error view in MainArea.
- [x] Plugin activation failures show a Sidebar or Activity-level unavailable state.
- [x] Error views include Open Logs action.
- [x] Logs panel can show shell/plugin lifecycle errors.
- [x] Plugin failures do not crash the shell.

### Implementation notes

Mewoo now has a lightweight `IMewooLogger` abstraction and an in-memory logger implementation.

Plugin lifecycle transitions and failures are logged by `MewooPluginHost`. App-level UI exceptions are also logged before being marked handled.

Workbench Panel is now a Logs panel. Error views include an Open Logs action that reveals the Panel.

Unsupported plugin native views and plugin activation failures are surfaced as visible error UI instead of crashing the shell.

## Slice 013: Human UI Review and Visual Tuning

**Type:** HITL

**Blocked by:** Slice 012

**User stories covered:** As the project owner, I confirm the first usable Mewoo UI matches the intended visual design.

### What to build

Run the app, compare it against the ASCII UI visual design, capture feedback, and tune spacing, sizing, colors, hover states, active states, and region behavior.

### Acceptance criteria

- [ ] The running UI is reviewed against `docs/design/v1-ui-visual-design.md`.
- [x] TitleBar behavior and density are approved or adjusted.
- [x] ActivityBar, Sidebar, MainArea, Panel, and StatusBar sizing are approved or adjusted.
- [ ] Dark and Light themes are visually checked.
- [ ] Quick Launcher first-use flow is checked.
- [ ] Follow-up issues are created locally for anything intentionally deferred.

### Implementation notes

First feedback pass removed the TitleBar app/version text, changed shell actions to icon buttons, reduced ActivityBar density, replaced the selected border with a left accent line, moved Panel under the MainArea column, added Panel tabs, introduced closable MainArea tabs, and made Sidebar drag-left collapse directly.

## Slice 014: Make Quick Launcher Groups and Items Clickable

**Type:** AFK

**Status:** Done

**Blocked by:** Slice 013 first feedback pass

**User stories covered:** As a user, I can click launcher groups and items instead of treating the launcher as a static list.

### What to build

Turn Quick Launcher group and item rows into explicit click targets. Keep single-click selection for safe preview/status feedback, and support double-click run for fast launch.

### Acceptance criteria

- [x] Sidebar group rows can be clicked.
- [x] Clicking a Sidebar group opens the Launcher MainArea and selects that group's first item when available.
- [x] Sidebar recent rows can be clicked.
- [x] MainArea launcher rows can be clicked.
- [x] Double-clicking a launcher item runs it.
- [x] StatusBar updates after group/item selection and run attempts.

### Implementation notes

Launcher rows remain safe on single click by updating selection/status only. Double click runs the selected target through the existing `QuickLauncherLaunchService`, so command execution and direct row execution share the same status/error behavior.

## Slice 015: Add Runtime Plugin Manifest Discovery

**Type:** AFK

**Status:** Done

**Blocked by:** V1 baseline

**User stories covered:** As the shell, I can discover runtime plugin manifests from the local plugin directory before implementing runtime assembly loading.

### What to build

Define the V2 runtime plugin manifest contract and a catalog that scans local plugin directories for `mewoo.plugin.json`. Do not load external assemblies yet.

### Acceptance criteria

- [x] A runtime plugin manifest model exists.
- [x] Manifest fields include plugin id, display name, version, assembly path, and entry point type.
- [x] Manifest assembly paths must be relative and stay inside the plugin directory.
- [x] A runtime plugin catalog scans the local plugin root for manifest files.
- [x] Disabled runtime plugins are skipped.
- [x] Invalid manifests are logged without crashing app startup.
- [x] App startup discovers runtime plugin manifests without affecting compiled V1 plugins.

### Implementation notes

The local runtime plugin root is `%LocalAppData%\Mewoo\Plugins`.

Runtime discovery currently stops at manifest validation and logging. Assembly loading, dependency handling, unload boundaries, trust model, and plugin activation through runtime descriptors are deferred to later V2 slices.

## Slice 016: Document and Validate Runtime Manifest Schema

**Type:** AFK

**Status:** Done

**Blocked by:** Slice 015

**User stories covered:** As a plugin author, I can create a manifest that Mewoo can validate before loading code.

### What to build

Turn the initial manifest discovery into a tighter schema contract with examples and validation coverage.

### Acceptance criteria

- [x] Manifest documentation includes a complete example.
- [x] Required field validation is covered.
- [x] Relative assembly path validation is covered.
- [x] Disabled plugin behavior is covered.
- [x] Manifest id format validation is covered.
- [x] Manifest id and future plugin instance id matching rule is documented.

### Implementation notes

The design reference is `docs/design/v2-runtime-plugin-design.md`.

Manifest schema validation is covered by `Mewoo.Core.Tests`. The reader wraps JSON deserialization failures as manifest validation failures so catalog discovery can log invalid manifests consistently.

## Slice 017: Implement Runtime Plugin Load Context

**Type:** AFK

**Status:** Ready

**Blocked by:** Slice 016

**User stories covered:** As the shell, I can load plugin assemblies without putting all private dependencies into the default context.

### What to build

Implement a collectible per-plugin load context backed by `AssemblyDependencyResolver`.

### Acceptance criteria

- [ ] Runtime plugin assemblies load from the manifest assembly path.
- [ ] Plugin-private dependencies resolve from the plugin directory.
- [ ] `Mewoo.Abstractions` resolves from the default context.
- [ ] MewUI assemblies used by plugin views resolve from the default context.
- [ ] Failed assembly loads are logged and do not crash startup.

## Slice 018: Instantiate Runtime Plugins from Entry Point

**Type:** AFK

**Status:** Ready

**Blocked by:** Slice 017

**User stories covered:** As the shell, I can turn a valid manifest into an `IMewooPlugin` instance.

### What to build

Resolve the manifest `entryPoint`, instantiate it, validate it implements `IMewooPlugin`, and verify manifest id matches `plugin.Id`.

### Acceptance criteria

- [ ] Missing entry point type fails the plugin only.
- [ ] Entry point not implementing `IMewooPlugin` fails the plugin only.
- [ ] Entry point constructor failure is logged.
- [ ] Manifest id mismatch is rejected.
- [ ] Valid runtime plugin instances can be handed to `MewooPluginHost`.

## Slice 019: Register and Activate Runtime Plugins

**Type:** AFK

**Status:** Ready

**Blocked by:** Slice 018

**User stories covered:** As a user, runtime plugins appear in the shell through the same ActivityBar, Sidebar, MainArea, command, and status bar contribution points as compiled plugins.

### What to build

Wire runtime plugin instances into the existing `MewooPluginHost` registration and activation flow.

### Acceptance criteria

- [ ] Runtime plugin contributions render through existing Workbench code.
- [ ] Runtime commands register through the existing command registry.
- [ ] Runtime plugin activation failures are logged and surfaced like compiled plugin failures.
- [ ] Compiled V1 plugins still work unchanged.
- [ ] Runtime plugin discovery order is deterministic.

## Slice 020: Runtime Plugin Unload and Reference Release

**Type:** AFK

**Status:** Ready

**Blocked by:** Slice 019

**User stories covered:** As the shell, I can unload a runtime plugin and remove its UI contributions without leaving orphaned state.

### What to build

Add runtime plugin unload orchestration around the existing lifecycle and release the plugin load context.

### Acceptance criteria

- [ ] Deactivation removes visible contributions.
- [ ] Plugin-owned MainArea tabs are closed during unload.
- [ ] Plugin lifecycle cleanup is called before load-context unload.
- [ ] Loader-held references are released.
- [ ] Load-context unload success or residual risk is logged.

## Slice 021: Runtime Plugin Diagnostics UI

**Type:** HITL

**Status:** Ready

**Blocked by:** Slice 019

**User stories covered:** As a user, I can see discovered runtime plugins and understand why a plugin failed.

### What to build

Add a shell-owned diagnostics view for runtime plugins.

### Acceptance criteria

- [ ] Runtime plugin list shows discovered, disabled, loaded, activated, and failed states.
- [ ] Manifest path and assembly path are visible.
- [ ] Failure messages link to Logs panel details.
- [ ] Diagnostics view is reachable from shell UI.

## Slice 022: Runtime Plugin Compatibility and Disabled State

**Type:** AFK

**Status:** Ready

**Blocked by:** Slice 019

**User stories covered:** As a user, incompatible or disabled plugins do not break startup.

### What to build

Apply `minimumMewooVersion` and disabled-state rules consistently before loading plugin code.

### Acceptance criteria

- [ ] Plugins requiring a newer Mewoo build are not loaded.
- [ ] Compatibility failures are logged and visible in diagnostics.
- [ ] Disabled plugins are discovered but not loaded.
- [ ] Disabled-state behavior is stable across restarts.

## Slice 023: Local Runtime Plugin Packaging Helper

**Type:** AFK

**Status:** Ready

**Blocked by:** Slice 019

**User stories covered:** As a plugin author, I can produce a local plugin folder that Mewoo can discover.

### What to build

Add a simple packaging convention or command that publishes a plugin project into `%LocalAppData%\Mewoo\Plugins\<pluginId>`.

### Acceptance criteria

- [ ] Packaging output includes `mewoo.plugin.json`.
- [ ] Packaging output includes the plugin assembly.
- [ ] Packaging output includes plugin-private dependencies.
- [ ] Packaging does not copy host-shared assemblies unnecessarily.
- [ ] A packaged sample runtime plugin can be discovered by Mewoo.

