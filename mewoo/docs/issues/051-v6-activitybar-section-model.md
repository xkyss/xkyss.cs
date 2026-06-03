# Issue 051: ActivityBar Section Model and System Permission

## What to build

Add an ActivityBar section model so Activity contributions can declare whether they belong to the default `Primary` section or the bottom `System` section.

## Acceptance criteria

- [ ] Activity descriptors include an ActivityBar section with `Primary` as the default.
- [ ] Contribution builder API can declare `ActivityBarSection.System` for built-in plugins.
- [ ] Compiled built-in plugins can contribute system-section Activities.
- [ ] Runtime/local installed plugins cannot contribute system-section Activities.
- [ ] Runtime/local plugins that declare `System` fail registration with a clear diagnostic instead of silently falling back to `Primary`.
- [ ] Tests cover default `Primary`, built-in `System`, and runtime/local rejection.

## Notes

- `System` is UI placement and authorization, not a special Activity kind.
- Do not add manifest permissions or marketplace authorization in V6.

## Blocked by

- Issue 050
