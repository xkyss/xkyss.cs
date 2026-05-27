# Issue 026: Verify Collectible Load Context Unload

## What to build

Add best-effort unload verification using `WeakReference`, forced GC, and diagnostic logging so Mewoo can tell whether a runtime plugin load context was actually released.

## Acceptance criteria

- [x] Runtime unload creates a weak reference to the load context before release.
- [x] Runtime unload forces a bounded GC verification loop.
- [x] Successful load-context collection is logged.
- [x] Remaining live load-context risk is logged.
- [x] Tests cover the success path where no plugin references remain.

## Blocked by

- Slice 020
