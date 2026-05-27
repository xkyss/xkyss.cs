# Issue 006: Implement Command Execution and MainArea Open Service

## What to build

Implement the command registry, command execution by ID, `CanExecute`, duplicate command rejection, and `IWorkbenchService.OpenMainViewAsync`.

## Acceptance criteria

- [x] Commands execute by ID.
- [x] Commands support asynchronous `ValueTask` handlers.
- [x] `CanExecute` prevents unavailable commands from running.
- [x] Duplicate command IDs are rejected.
- [x] A command can open a MainArea view through the Workbench service.
- [x] MainArea open behavior handles activation and deduplication for non-multiple views.

## Blocked by

- Issue 004
- Issue 005

## Status

Done

