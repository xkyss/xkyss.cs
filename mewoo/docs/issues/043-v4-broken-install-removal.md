# Issue 043: Broken Install Removal

## What to build

Allow Plugin Manager to show and remove broken installed plugin directories.

## Acceptance criteria

- [ ] Broken entries appear for missing manifest, invalid manifest, invalid id, and missing assembly cases.
- [ ] Broken entries show a user-facing error summary.
- [ ] User can remove a broken installed plugin directory from Plugin Manager.
- [ ] Removal reports success or failure in recent operations.
- [ ] Tests cover broken install discovery and removal failure.

## Blocked by

- Issue 038
