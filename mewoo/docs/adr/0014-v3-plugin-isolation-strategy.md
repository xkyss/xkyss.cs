# ADR 0014: V3 Plugin Isolation Strategy

## Status

Accepted

## Context

V3 adds local plugin packages, install/update/uninstall operations, catalog metadata, and explicit trust/permission declarations. The remaining decision is whether V3 should move plugins out of the Mewoo process.

V2 runtime plugins are trusted local code loaded in-process through collectible load contexts. This supports the current workbench contribution model, including `IMewooView.NativeView`.

## Decision

Mewoo will use a hybrid plugin isolation strategy after V3.

Trusted UI plugins remain in-process for now. Non-UI plugins may later run out-of-process behind a JSON-line IPC contract for lifecycle, commands, status, logs, and diagnostics.

V3 permission declarations are informational metadata until an isolation boundary is implemented.

## Consequences

- Mewoo can finish V3 without rewriting the view model.
- Package trust remains explicit in diagnostics and install results.
- Out-of-process work can start with commands and background/status plugins.
- `IMewooView.NativeView` is identified as the main blocker for fully isolated UI plugins.
- A future declarative or remote-rendered view protocol is required before UI plugins can move safely out-of-process.
