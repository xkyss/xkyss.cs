# V3 Out-of-Process Plugin Host Spike

## Purpose

Evaluate whether V3 should isolate selected plugins outside the Mewoo process.

The spike keeps the scope intentionally narrow: lifecycle activation, shutdown, commands, status, and diagnostics for non-UI plugins.

## Prototype

The prototype is `MewooOutOfProcessPluginProcess` in `Mewoo.Core`.

It starts a child process with redirected stdin/stdout and exchanges one JSON message per line:

```json
{ "type": "activate", "pluginId": "xkyss.outOfProcessProbe" }
{ "type": "activated", "pluginId": "xkyss.outOfProcessProbe", "capabilities": ["commands", "status", "diagnostics"] }
{ "type": "shutdown" }
{ "type": "shutdownComplete" }
```

`Mewoo.TestPlugins.OutOfProcessProbeHost` is a minimal non-UI process used by tests to prove activation and shutdown.

## Minimum IPC Contract

Lifecycle:

- `activate`: host asks the plugin process to initialize a plugin id.
- `activated`: plugin process confirms readiness and advertised capabilities.
- `shutdown`: host asks the plugin process to stop cleanly.
- `shutdownComplete`: plugin process confirms it has stopped its plugin work.

Commands:

- `registerCommands`: plugin advertises command ids, titles, categories, and enablement metadata.
- `executeCommand`: host invokes a command by id with JSON-serializable arguments.
- `commandResult`: plugin returns success, failure message, and optional JSON payload.

Status:

- `registerStatusItems`: plugin advertises status bar item ids and initial text.
- `updateStatusItem`: plugin pushes text updates for registered status items.

Diagnostics:

- `log`: plugin emits structured log entries with level, source, message, and optional exception text.
- `health`: host asks for process health and last known plugin state.

## Boundary Findings

The current `IMewooView.NativeView` model cannot cross a process boundary. It carries an in-process native UI object from MewUI, so a child process cannot safely return it to the host.

Out-of-process plugins can support commands, status, logging, and diagnostics first. UI plugins need either:

- a declarative view protocol that the host renders in-process;
- a remote surface protocol owned by MewUI;
- or a hybrid rule where UI plugins remain in-process and worker-style plugins can run out-of-process.

## Isolation Comparison

Signed in-process packages:

- Lowest implementation cost.
- Keeps existing `IMewooPlugin` and `IMewooView.NativeView` behavior.
- Can verify package provenance before install.
- Does not protect Mewoo from plugin crashes, hangs, filesystem access, process launch, native interop, or dependency side effects.

Out-of-process plugins:

- Stronger crash and unload isolation.
- Enables per-plugin process lifetime, diagnostics, and eventual OS-level constraints.
- Requires an IPC contract, version negotiation, timeout handling, process supervision, and contribution mirroring.
- Does not solve UI plugin rendering with the current native view API.

Hybrid:

- Keeps in-process plugins for trusted UI contributions.
- Allows declared non-UI plugins to run out-of-process for commands, background work, status, and diagnostics.
- Uses V3 trust and permission declarations to decide whether a package is eligible for out-of-process execution.
- Adds complexity, but can be delivered incrementally without rewriting the workbench view model.

## Recommendation

Use a hybrid strategy after V3.

V3 should keep trusted in-process local packages as the default and treat trust/permission declarations as visible metadata. The next isolation milestone should add an out-of-process lane for non-UI plugins only, with commands, status, logs, and diagnostics crossing IPC. UI isolation should wait until Mewoo has a declarative or remote-rendered view protocol.
