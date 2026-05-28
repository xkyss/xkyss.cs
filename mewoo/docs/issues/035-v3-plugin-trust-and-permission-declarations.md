# Issue 035: Plugin Trust and Permission Declarations

## What to build

Introduce explicit trust and permission metadata for local plugin packages before adding stronger isolation.

## Acceptance criteria

- [x] Manifest supports declared permissions such as filesystem, process launch, network, and native interop.
- [x] Missing permission declarations are handled conservatively in diagnostics.
- [x] Runtime Diagnostics shows trust and permission metadata.
- [x] Package install surfaces a local-code trust warning.
- [x] Tests cover permission parsing and diagnostics mapping.

## Status

Done.

## Verification

- `dotnet test Mewoo.slnx --no-restore --verbosity minimal`
- `dotnet build Mewoo.slnx --no-restore --verbosity minimal`

## Blocked by

- Issue 034
