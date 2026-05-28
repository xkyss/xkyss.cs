# Issue 035: Plugin Trust and Permission Declarations

## What to build

Introduce explicit trust and permission metadata for local plugin packages before adding stronger isolation.

## Acceptance criteria

- [ ] Manifest supports declared permissions such as filesystem, process launch, network, and native interop.
- [ ] Missing permission declarations are handled conservatively in diagnostics.
- [ ] Runtime Diagnostics shows trust and permission metadata.
- [ ] Package install surfaces a local-code trust warning.
- [ ] Tests cover permission parsing and diagnostics mapping.

## Blocked by

- Issue 034

