# Issue 052: Render Primary and System ActivityBar Sections

Status: Done

## What to build

Render ActivityBar Activities in two vertical sections: `Primary` at the top and `System` pinned to the bottom.

## Acceptance criteria

- [x] Primary Activities render in the existing top ActivityBar flow.
- [x] System Activities render in a bottom ActivityBar section.
- [x] The bottom system section stays pinned when the ActivityBar has extra vertical space.
- [x] Activity selection styling, hover tooltips, and click behavior are consistent across both sections.
- [x] Selecting a system-section Activity switches the full Activity Workspace.
- [x] Tests or focused UI-verifiable code paths cover section ordering and selection.

## Verification

- `dotnet test tests\Mewoo.Core.Tests\Mewoo.Core.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~WorkbenchActivityBarModelTests"`

## Notes

- V6 does not support user sorting or hiding system-section Activities.
- System Activities still use normal Activity Workspace state.

## Blocked by

- Issue 051
