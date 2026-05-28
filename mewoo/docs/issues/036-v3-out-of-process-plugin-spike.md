# Issue 036: Out-of-Process Plugin Host Spike

## What to build

Spike a minimal out-of-process plugin host boundary to evaluate whether V3 should isolate selected plugins outside the Mewoo process.

## Acceptance criteria

- [x] Document the minimum IPC contract needed for lifecycle, commands, status, and diagnostics.
- [x] Prototype activation and shutdown of a non-UI plugin process.
- [x] Record what cannot cross the process boundary with the current `IMewooView.NativeView` model.
- [x] Compare out-of-process isolation against signed in-process packages.
- [x] Produce an ADR recommending whether V3 continues with in-process trust, out-of-process isolation, or a hybrid.

## Status

Done.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~MewooOutOfProcessPluginProcessTests"`

## Blocked by

- Issue 035
