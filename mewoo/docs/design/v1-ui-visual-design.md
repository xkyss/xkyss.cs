# Mewoo V1 UI Visual Design

This document visualizes the Mewoo V1 workbench shell using ASCII diagrams. It complements the ADRs and focuses on visible layout, region ownership, and expected UI states.

## Design Intent

Mewoo uses a VSCode-like desktop workbench:

- Custom borderless title bar.
- Icon-only ActivityBar.
- Activity-specific Sidebar.
- Tabbed MainArea.
- Optional bottom Panel.
- Persistent StatusBar.

Mewoo is not primarily an editor. The central area is called `MainArea`.

## Default Window

Suggested default window size:

```text
Width:  1200px
Height: 760px
```

Region dimensions:

```text
TitleBar:     34px
ActivityBar:  48px
Sidebar:     260px, min 180px, max 420px
MainAreaTabs: 35px
Panel:       260px height, min 120px, max 50% window height
StatusBar:    24px
```

## Full Shell Layout

```text
+--------------------------------------------------------------------------------------------------+
| AppIcon Mewoo 0.1.0   File  View  Help             Active Main Title        Theme  Pin   _  □  X |
+--------------------------------------------------------------------------------------------------+
|        |                           |                                                               |
|        | Sidebar                   | MainArea                                                      |
|        |                           |                                                               |
|        | +-----------------------+ | +-----------------------------------------------------------+ |
|        | | ViewContainer Title   | | | Tab 1        Tab 2        Tab 3                         | |
|        | +-----------------------+ | +-----------------------------------------------------------+ |
|        | | Sidebar View          | | |                                                           | |
|        | |                       | | | Active Main View                                          | |
|Activity| | Activity-specific     | | |                                                           | |
|  Bar   | | navigation, filters,  | | | Plugin-owned UI, hosted by the Workbench                   | |
|        | | lists, search, etc.   | | |                                                           | |
|        | |                       | | |                                                           | |
|        | |                       | | |                                                           | |
|        | +-----------------------+ | +-----------------------------------------------------------+ |
|        |                           |                                                               |
+--------+---------------------------+---------------------------------------------------------------+
| Status left: plugin/status text                                             Status right: theme  |
+--------------------------------------------------------------------------------------------------+
```

## Workbench With Panel Visible

Panel is hidden by default. When visible, it occupies the bottom of the workbench above the StatusBar.

```text
+--------------------------------------------------------------------------------------------------+
| AppIcon Mewoo 0.1.0   File  View  Help             Active Main Title        Theme  Pin   _  □  X |
+--------------------------------------------------------------------------------------------------+
|        |                           |                                                               |
|        | Sidebar                   | MainArea                                                      |
|Activity|                           | +-----------------------------------------------------------+ |
|  Bar   |                           | | Tabs                                                      | |
|        |                           | +-----------------------------------------------------------+ |
|        |                           | | Active Main View                                          | |
|        |                           | |                                                           | |
+--------+---------------------------+---------------------------------------------------------------+
| Panel title                                 Problems   Output   Logs                 Collapse  X |
|--------------------------------------------------------------------------------------------------|
| Panel content: shell logs, plugin output, diagnostics, or task details                           |
+--------------------------------------------------------------------------------------------------+
| Status left: plugin/status text                                             Status right: theme  |
+--------------------------------------------------------------------------------------------------+
```

## Sidebar Collapsed

When the Sidebar is collapsed, ActivityBar remains visible and MainArea expands.

```text
+--------------------------------------------------------------------------------------------------+
| AppIcon Mewoo 0.1.0   File  View  Help             Active Main Title        Theme  Pin   _  □  X |
+--------------------------------------------------------------------------------------------------+
|        |                                                                                         |
|        | MainArea                                                                                |
|        |                                                                                         |
|Activity| +-------------------------------------------------------------------------------------+ |
|  Bar   | | Tab 1        Tab 2        Tab 3                                                     | |
|        | +-------------------------------------------------------------------------------------+ |
|        | |                                                                                     | |
|        | | Active Main View                                                                    | |
|        | |                                                                                     | |
|        | |                                                                                     | |
+--------+-----------------------------------------------------------------------------------------+
| Status left: plugin/status text                                             Status right: theme  |
+--------------------------------------------------------------------------------------------------+
```

## ActivityBar

ActivityBar is icon-only. Text appears through tooltip or accessible name.

```text
+--------+
|        |
|  [L]   |  selected: Quick Launcher
|        |
|  [K]   |  Trading K-line Analyzer
|        |
|  [ ]   |  future plugin activity
|        |
|--------|
|  [G]   |  global/settings activity, if added later
+--------+
```

Visual rules:

- Width is fixed at `48px`.
- Active item has a clear left accent or filled active background.
- Hover state uses subtle contrast, not layout movement.
- ActivityBar selection is `ActivityId`, not `PluginId`.
- One plugin may contribute multiple ActivityBar items.

## Sidebar

Each ActivityBar item selects a `ViewContainer`.

```text
+---------------------------+
| ViewContainer Title   ... |
+---------------------------+
| Search / filter           |
+---------------------------+
| SidebarView A             |
|                           |
| - item                    |
| - item                    |
| - item                    |
+---------------------------+
| SidebarView B             |
|                           |
| - recent                  |
| - recent                  |
+---------------------------+
```

V1 may render one SidebarView per ViewContainer, but the API supports multiple views.

Sidebar behavior:

- Width defaults to `260px`.
- Resizable between `180px` and `420px`.
- Collapsible from View menu, command, or shell action.
- Content is plugin-owned, but container chrome is shell-owned.

## MainArea

MainArea owns tab layout, activation, closing, and view hosting.

```text
+----------------------------------------------------------------+
| Tab: Launcher    Tab: BTC 1h    Tab: Notes                  +  |
+----------------------------------------------------------------+
|                                                                |
| Active Main View                                               |
|                                                                |
| The Workbench hosts plugin-provided IMewooView.NativeView.      |
|                                                                |
+----------------------------------------------------------------+
```

MainArea behavior:

- Supports multiple tabs.
- Does not support split groups in V1.
- Plugins open content through `IWorkbenchService`.
- Shell controls deduplication, activation, close behavior, and future restore.
- Unsupported native views are shown as error views instead of crashing the shell.

## Custom TitleBar

TitleBar is shell-owned in V1.

```text
+--------------------------------------------------------------------------------------------------+
| AppIcon Mewoo 0.1.0   File  View  Help             Active Main Title        Theme  Pin   _  □  X |
+--------------------------------------------------------------------------------------------------+
  |------ identity ------| |--- menus ---|             |--- title ---|       | actions | controls |
```

TitleBar regions:

```text
+----------------------+----------------------+------------------------+----------------+-----------+
| App identity         | Function menus       | Active title           | Shell actions  | Window    |
| icon/name/version    | File/View/Help       | from title service     | theme/pin/etc. | controls  |
+----------------------+----------------------+------------------------+----------------+-----------+
```

Title priority:

```text
active MainArea tab title
-> active Activity title
-> app name
```

Required borderless-window behavior:

- Drag TitleBar to move the window.
- Double-click TitleBar to maximize or restore.
- Minimize, maximize/restore, and close buttons.
- Resizable window edges.
- Correct maximize behavior around screen work area.
- Always-on-top toggle.
- Window-state-aware maximize/restore icon.

V1 constraints:

- TitleBar height is fixed at `34px`.
- Menus are shell-owned.
- Right-side actions are shell-owned.
- Plugin status belongs in StatusBar, not TitleBar.

## StatusBar

StatusBar is persistent and shell-owned, with plugin contribution slots.

```text
+--------------------------------------------------------------------------------------------------+
| QuickLauncher: 12 items   Selected: GitHub                         Dark   Main ready   Ln/Col n/a |
+--------------------------------------------------------------------------------------------------+
  |-------- left contributions --------|                              |--- right contributions ----|
```

StatusBar behavior:

- Height is fixed at `24px`.
- Left side favors plugin workflow state.
- Right side favors shell state and compact global indicators.
- Plugins update status items through shell-provided handles.
- Status items should be short and stable; detailed plugin UI belongs in MainArea or Panel.

## Panel

Panel is optional and hidden by default.

```text
+--------------------------------------------------------------------------------------------------+
| Panel title                                 Problems   Output   Logs                 Collapse  X |
|--------------------------------------------------------------------------------------------------|
| Content                                                                                          |
|                                                                                                  |
+--------------------------------------------------------------------------------------------------+
```

Panel behavior:

- Appears above StatusBar.
- Resizable vertically.
- Default height is `260px`.
- Minimum height is `120px`.
- Maximum height is 50% of window height.
- Suitable for logs, output, diagnostics, task progress, and future plugin panels.

## Quick Launcher Plugin UI

Quick Launcher validates the V1 shell.

```text
+--------------------------------------------------------------------------------------------------+
| AppIcon Mewoo 0.1.0   File  View  Help                  Launcher              Theme  Pin  _ □ X |
+--------------------------------------------------------------------------------------------------+
| [L]*   | Launcher                  | Launcher                                                    |
| [K]    | +-----------------------+ | +---------------------------------------------------------+ |
|        | | Search shortcuts      | | | Launcher                                             | |
|        | +-----------------------+ | +---------------------------------------------------------+ |
|        | | Groups                | | | Search websites, apps, scripts...                    | |
|        | | - Development         | | +---------------------------------------------------------+ |
|        | | - Trading             | | | Development                                             | |
|        | | - Writing             | | |  GitHub                     https://github.com          | |
|        | |                       | | |  Terminal                   wt.exe                      | |
|        | | Recent                | | |  Build Script                scripts/build.ps1           | |
|        | | - GitHub              | | |                                                         | |
|        | | - Terminal            | | | Trading                                                 | |
|        | |                       | | |  Binance                    https://...                 | |
+--------+---------------------------+-------------------------------------------------------------+
| QuickLauncher: 5 items   Selected: GitHub                                      Dark   Ready      |
+--------------------------------------------------------------------------------------------------+
```

Quick Launcher V1:

- ActivityBar launcher icon.
- Sidebar search, groups, and recent list.
- MainArea launcher tab with search and launcher item list.
- StatusBar item count, selected item, or last run result.
- JSON-backed configuration.
- Supports URL, file, executable, and script targets.

## Trading K-line Analyzer Future Shape

The trading plugin is not the first tracer bullet, but the shell should allow this shape later.

```text
+--------------------------------------------------------------------------------------------------+
| AppIcon Mewoo 0.1.0   File  View  Help                  BTCUSDT 1h            Theme  Pin  _ □ X |
+--------------------------------------------------------------------------------------------------+
| [L]    | Markets                   | BTCUSDT 1h                                                   |
| [K]*   | +-----------------------+ | +---------------------------------------------------------+ |
|        | | Symbol search         | | | BTCUSDT 1h   ETHUSDT 4h                               | |
|        | +-----------------------+ | +---------------------------------------------------------+ |
|        | | Watchlist             | | | K-line chart area                                      | |
|        | | - BTCUSDT             | | |                                                         | |
|        | | - ETHUSDT             | | | Candle-by-candle analysis                              | |
|        | | - SOLUSDT             | | |                                                         | |
|        | |                       | | | Markers, notes, strategy overlays                      | |
+--------+---------------------------+-------------------------------------------------------------+
| K-line: BTCUSDT 1h   Candle 241/500                                      Dark   Data ready       |
+--------------------------------------------------------------------------------------------------+
```

## Empty and Error States

No active MainArea tab:

```text
+----------------------------------------------------------+
|                                                          |
|                    No view is open                       |
|                                                          |
|              Select an activity or run a command         |
|                                                          |
+----------------------------------------------------------+
```

Plugin view error:

```text
+----------------------------------------------------------+
| View failed to load                                      |
|                                                          |
| Plugin: quickLauncher                                    |
| View: quickLauncher.home                                 |
|                                                          |
| [Open Logs] [Reload Plugin]                              |
+----------------------------------------------------------+
```

Plugin activation failure:

```text
+---------------------------+
| Launcher unavailable      |
| Activation failed         |
| [Open Logs]               |
+---------------------------+
```

## Visual Hierarchy

Primary hierarchy:

```text
TitleBar
ActivityBar + Sidebar
MainArea
Panel
StatusBar
```

Guidelines:

- TitleBar is dense and functional.
- ActivityBar is compact and icon-only.
- Sidebar is navigational and activity-specific.
- MainArea is the dominant work surface.
- Panel is secondary and hidden unless needed.
- StatusBar is compact and non-disruptive.

