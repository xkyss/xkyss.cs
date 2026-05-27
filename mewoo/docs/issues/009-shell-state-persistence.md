# Issue 009: Implement Shell State Persistence

## What to build

Implement JSON-backed shell state storage behind abstractions. Persist the V1 shell state fields owned by Mewoo.

## Acceptance criteria

- [x] Shell state stores active Activity ID.
- [x] Shell state stores Sidebar collapsed state and width.
- [x] Shell state stores Panel visibility and height.
- [x] Shell state stores active/open MainArea view identities where available.
- [x] Shell state stores theme ID.
- [x] Shell state stores always-on-top state.
- [x] State is saved and restored across app restarts.

## Blocked by

- Issue 003
- Issue 008

## Status

Done

