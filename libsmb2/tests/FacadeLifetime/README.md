These JIT-only ownership regressions use private reflection to construct a
connection with disconnected synthetic file handles, and the embedded runtime's
checked native heap to count live allocations. They require no SMB server and add
no public test hooks. Each case runs in a separate process.

The cases check deterministic finalizer cleanup, actual GC finalization, disposal
when the first of several closes fails, and a pending read whose event pump fails.
The final case also checks terminal connection disposal and buffer stability.
Real network failure and concurrency coverage lives in `ManagedLifecycle`.

Run after building the facade/sample:

```sh
python3 libsmb2/scripts/facade-lifetime.py --assemblies libsmb2/build/ManagedConsumer-jit-processed --label final
```

Pass the actual output directory if the build uses a different location. The
receipt records binary hashes, per-case outcomes, and captured logs under
`artifacts/facade-lifetime/<label>`.
