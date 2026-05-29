# Mewoo Development Plan

This document is the local issue index for Mewoo. Detailed per-slice issue documents live in `docs/issues/`.

## Baselines

- V1 baseline tag: `v1-baseline`
- Planned V2 baseline tag: `v2-baseline`
- Planned V3 baseline tag: `v3-baseline`
- V4 baseline tag: `v4-baseline`
- V5 baseline tag: `v5-baseline` (version `0.5.0`, release tag `v0.5.0`)
- Current development version: `0.6.0-dev`

## Milestones

### Milestone 1: Runnable Shell Skeleton

Goal: start the app, show the custom shell, and prove that plugins can open MainArea content through commands.

- [x] [Issue 001: Lock MewUI Baseline and Create Runnable App Skeleton](issues/001-lock-mewui-baseline-and-create-runnable-app-skeleton.md)
- [x] [Issue 002: Implement Borderless Window and Custom TitleBar Minimum Loop](issues/002-borderless-window-and-custom-titlebar.md)
- [x] [Issue 003: Implement Fixed Workbench Layout Skeleton](issues/003-fixed-workbench-layout-skeleton.md)
- [x] [Issue 004: Implement Plugin Registration and Contribution Descriptors](issues/004-plugin-registration-and-contribution-descriptors.md)
- [x] [Issue 005: Render Plugin-Contributed Activity, Sidebar, and MainArea View](issues/005-render-plugin-contributed-workbench-views.md)
- [x] [Issue 006: Implement Command Execution and MainArea Open Service](issues/006-command-execution-and-mainarea-open-service.md)

### Milestone 2: Usable Plugin Host

Goal: complete plugin lifecycle, theme switching, shell persistence, and the Quick Launcher plugin.

- [x] [Issue 007: Implement Plugin Lifecycle and Contribution Revocation](issues/007-plugin-lifecycle-and-contribution-revocation.md)
- [x] [Issue 008: Implement Theme Tokens and Dark/Light Switching](issues/008-theme-tokens-and-dark-light-switching.md)
- [x] [Issue 009: Implement Shell State Persistence](issues/009-shell-state-persistence.md)
- [x] [Issue 010: Implement Quick Launcher JSON Model and List UI](issues/010-quick-launcher-json-model-and-list-ui.md)
- [x] [Issue 011: Implement Quick Launcher Actions and Status Feedback](issues/011-quick-launcher-actions-and-status-feedback.md)
- [x] [Issue 012: Implement Error Views, Logs Panel, and Plugin Failure Feedback](issues/012-error-views-logs-panel-and-plugin-failure-feedback.md)

### Milestone 3: Human UI Review

Goal: review the running UI against the ASCII visual design and tune the first usable experience.

- [x] [Issue 013: Human UI Review and Visual Tuning](issues/013-human-ui-review-and-visual-tuning.md)

### Milestone 4: Quick Launcher Interaction Refinement

Goal: make the first real plugin feel operable through visible click targets and direct item execution.

- [x] [Issue 014: Make Quick Launcher Groups and Items Clickable](issues/014-quick-launcher-clickable-groups-and-items.md)

### Milestone 5: Runtime Plugin Manifest Foundation

Goal: introduce the V2 runtime plugin shape without changing the V1 compiled plugin path.

- [x] [Issue 015: Add Runtime Plugin Manifest Discovery](issues/015-runtime-plugin-manifest-discovery.md)
- [x] [Issue 016: Document and Validate Runtime Manifest Schema](issues/016-runtime-manifest-schema-validation.md)

### Milestone 6: Runtime Plugin Loading

Goal: load trusted local plugin assemblies into Mewoo through the existing lifecycle and contribution model.

- [x] [Issue 017: Implement Runtime Plugin Load Context](issues/017-runtime-plugin-load-context.md)
- [x] [Issue 018: Instantiate Runtime Plugins from Entry Point](issues/018-runtime-plugin-entrypoint-instantiation.md)
- [x] [Issue 019: Register and Activate Runtime Plugins](issues/019-register-and-activate-runtime-plugins.md)
- [x] [Issue 020: Runtime Plugin Unload and Reference Release](issues/020-runtime-plugin-unload-and-reference-release.md)

### Milestone 7: Runtime Plugin Operations

Goal: make runtime plugins diagnosable, unloadable, and locally packageable.

- [x] [Issue 021: Runtime Plugin Diagnostics UI](issues/021-runtime-plugin-diagnostics-ui.md)
- [x] [Issue 022: Runtime Plugin Compatibility and Disabled State](issues/022-runtime-plugin-compatibility-and-disabled-state.md)
- [x] [Issue 023: Local Runtime Plugin Packaging Helper](issues/023-local-runtime-plugin-packaging-helper.md)

### Milestone 8: V2 Hardening Baseline

Goal: prove V2 with a real external plugin workflow, improve runtime diagnostics operations, and prepare a `v2-baseline` tag before V3.

- [x] [Issue 024: Create an External Runtime Plugin Smoke Sample](issues/024-external-runtime-plugin-smoke-sample.md)
- [x] [Issue 025: Add Runtime Diagnostics Operations](issues/025-runtime-diagnostics-operations.md)
- [x] [Issue 026: Verify Collectible Load Context Unload](issues/026-collectible-load-context-unload-verification.md)
- [x] [Issue 027: Document Runtime Plugin Author Workflow](issues/027-runtime-plugin-authoring-guide.md)
- [x] [Issue 028: Runtime Plugin Failure UX Pass](issues/028-runtime-plugin-failure-ux-pass.md)
- [x] [Issue 029: End-to-End Runtime Plugin Road Test](issues/029-runtime-plugin-road-test.md)
- [x] [Issue 030: Tag V2 Baseline](issues/030-v2-baseline.md)

### Milestone 9: V3 Local Plugin Package Management

Goal: move from manually copied runtime plugin folders to installable local plugin packages with update, uninstall, and clearer package metadata.

- [x] [Issue 031: Define V3 Plugin Package Format](issues/031-v3-plugin-package-format.md)
- [x] [Issue 032: Local Plugin Installer Service](issues/032-v3-local-plugin-installer-service.md)
- [x] [Issue 033: Plugin Uninstall and Update Operations](issues/033-v3-plugin-uninstall-and-update.md)
- [x] [Issue 034: Plugin Catalog Metadata UI](issues/034-v3-plugin-catalog-metadata-ui.md)

### Milestone 10: V3 Trust and Isolation Exploration

Goal: make plugin trust explicit and decide whether V3 isolation should remain in-process, become out-of-process, or use a hybrid model.

- [x] [Issue 035: Plugin Trust and Permission Declarations](issues/035-v3-plugin-trust-and-permission-declarations.md)
- [x] [Issue 036: Out-of-Process Plugin Host Spike](issues/036-v3-out-of-process-plugin-spike.md)

### Milestone 11: V4 Plugin Manager Product Surface

Goal: introduce a first-class local Plugin Manager experience separate from Runtime Diagnostics.

- [x] [Issue 037: Define Plugin Manager Product Surface](issues/037-v4-plugin-manager-product-surface.md)
- [x] [Issue 038: Plugin Manager Catalog Model](issues/038-v4-plugin-manager-catalog-model.md)
- [x] [Issue 039: Plugin Manager Activity and List UI](issues/039-v4-plugin-manager-activity-and-list-ui.md)
- [x] [Issue 040: Plugin Details View](issues/040-v4-plugin-details-view.md)

### Milestone 12: V4 Local Package Management Flows

Goal: make install, update, broken install cleanup, and operation feedback usable through Plugin Manager.

- [x] [Issue 041: Install from File Flow](issues/041-v4-install-from-file-flow.md)
- [x] [Issue 042: Update from File Flow](issues/042-v4-update-from-file-flow.md)
- [x] [Issue 043: Broken Install Removal](issues/043-v4-broken-install-removal.md)

### Milestone 13: V4 Author Workflow and Baseline

Goal: align authoring tools with Plugin Manager and freeze a stable V4 baseline.

- [x] [Issue 044: Author Workflow and V4 Road Test](issues/044-v4-author-workflow-and-road-test.md)
- [x] [Issue 045: Tag V4 Baseline](issues/045-v4-baseline.md)

### Milestone 14: V5 Activity Workspace Scoping

Goal: make Activity switching change the full Activity Workspace instead of only the Sidebar.

- [x] [Issue 046: Activity Workspace Scope Model](issues/046-v5-activity-workspace-scope-model.md)
- [x] [Issue 047: MainArea Per-Activity Tabs](issues/047-v5-mainarea-per-activity-tabs.md)
- [x] [Issue 048: Panel and Logs Activity Scope](issues/048-v5-panel-and-logs-activity-scope.md)
- [x] [Issue 049: StatusBar Activity and Global Scope](issues/049-v5-statusbar-activity-and-global-scope.md)
- [x] [Issue 050: Activity Workspace Road Test](issues/050-v5-activity-workspace-road-test.md)
