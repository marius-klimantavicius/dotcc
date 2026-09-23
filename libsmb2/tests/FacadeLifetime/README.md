These JIT-only ownership regressions use private reflection to construct a
connection with disconnected synthetic file handles, and the embedded runtime's
checked native heap to count live allocations. They require no SMB server and add
no public test hooks. Each case runs in a separate process.

The default cases check deterministic finalizer cleanup and actual GC
finalization. Synthetic failed-close and pending-read cases are excluded under
the updated [upstream test scope](../../docs/test-scope.md). Normal I/O and
concurrency coverage lives in `ManagedLifecycle`.

Cleanup runs on the async facade's serialized executor. The tests wait for an
executor barrier after finalization before inspecting native allocations; they
do not scan the heap concurrently with background cleanup.

Run after building the facade/sample:

```sh
python3 libsmb2/scripts/facade-lifetime.py --assemblies libsmb2/build/ManagedConsumer-jit-processed --label final
```

Pass the actual output directory if the build uses a different location. The
receipt records binary hashes, per-case outcomes, and captured logs under
`artifacts/facade-lifetime/<label>`.
