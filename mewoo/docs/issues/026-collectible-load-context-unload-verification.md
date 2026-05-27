# Issue 026: Verify Collectible Load Context Unload

## What to build

Add best-effort unload verification using `WeakReference`, forced GC, and diagnostic logging so Mewoo can tell whether a runtime plugin load context was actually released.

## Acceptance criteria

- [ ] Runtime unload creates a weak reference to the load context before release.
- [ ] Runtime unload forces a bounded GC verification loop.
- [ ] Successful load-context collection is logged.
- [ ] Remaining live load-context risk is logged.
- [ ] Tests cover the success path where no plugin references remain.

## Blocked by

- Slice 020

