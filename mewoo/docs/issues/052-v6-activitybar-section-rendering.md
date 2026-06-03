# Issue 052: Render Primary and System ActivityBar Sections

## What to build

Render ActivityBar Activities in two vertical sections: `Primary` at the top and `System` pinned to the bottom.

## Acceptance criteria

- [ ] Primary Activities render in the existing top ActivityBar flow.
- [ ] System Activities render in a bottom ActivityBar section.
- [ ] The bottom system section stays pinned when the ActivityBar has extra vertical space.
- [ ] Activity selection styling, hover tooltips, and click behavior are consistent across both sections.
- [ ] Selecting a system-section Activity switches the full Activity Workspace.
- [ ] Tests or focused UI-verifiable code paths cover section ordering and selection.

## Notes

- V6 does not support user sorting or hiding system-section Activities.
- System Activities still use normal Activity Workspace state.

## Blocked by

- Issue 051
