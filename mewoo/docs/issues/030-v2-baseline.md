# Issue 030: Tag V2 Baseline

## What to build

Finalize docs, ensure tests pass, and create the `v2-baseline` tag.

## Acceptance criteria

- [x] `dotnet test Mewoo.slnx` passes.
- [x] V2 hardening docs are up to date.
- [x] Development plan marks completed V2 slices.
- [x] Working tree is clean.
- [x] Annotated tag `v2-baseline` is created.

## Blocked by

- Issue 029

## Verification Notes

- Restored with an isolated NuGet scratch directory to avoid stale user-temp locks.
- `dotnet test Mewoo.slnx --no-restore --verbosity minimal` passed with 25 tests.
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal` passed with one NU1900 vulnerability-data warning from offline NuGet access.
