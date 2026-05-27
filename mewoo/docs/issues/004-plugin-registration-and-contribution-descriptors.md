# Issue 004: Implement Plugin Registration and Contribution Descriptors

## What to build

Implement the fluent contribution registry backed by descriptors. Add ID validation, duplicate contribution detection, and automatic owner plugin assignment.

## Acceptance criteria

- [x] Plugins can register ActivityBar item descriptors.
- [x] Plugins can register ViewContainer and SidebarView descriptors.
- [x] Plugins can register MainView descriptors with lazy view factories.
- [x] Plugins can register Command descriptors.
- [x] Plugins can register StatusBarItem descriptors.
- [x] Duplicate contribution IDs fail registration.
- [x] Invalid non-namespaced IDs fail registration.
- [x] Each contribution is owned by the registering plugin.

## Blocked by

- Issue 001

## Status

Done

