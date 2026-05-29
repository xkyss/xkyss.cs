# Issue 046: Activity Workspace Scope Model

## What to build

Define the descriptor and state model for Activity Workspace scoping before changing rendering behavior.

## Acceptance criteria

- [ ] Contribution descriptors can represent `OwnerPluginId` separately from `ActivityScopeId`.
- [ ] Activity scope can be inferred for single-Activity plugins.
- [ ] Multi-Activity plugins fail or report a contribution error when scope cannot be inferred.
- [ ] Global contribution scope is explicit, not the default.
- [ ] Tests cover scope inference and ambiguous multi-Activity failures.

## Blocked by

- ADR 0015
