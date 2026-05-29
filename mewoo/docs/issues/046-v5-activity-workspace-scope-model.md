# Issue 046: Activity Workspace Scope Model

## What to build

Define the descriptor and state model for Activity Workspace scoping before changing rendering behavior.

## Acceptance criteria

- [x] Contribution descriptors can represent `OwnerPluginId` separately from `ActivityScopeId`.
- [x] Activity scope can be inferred for single-Activity plugins.
- [x] Multi-Activity plugins fail or report a contribution error when scope cannot be inferred.
- [x] Global contribution scope is explicit, not the default.
- [x] Tests cover scope inference and ambiguous multi-Activity failures.

## Status

Done.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooContributionRegistryScopeTests"`
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`
- `dotnet test Mewoo.slnx --no-restore --verbosity minimal`

## Blocked by

- ADR 0015
