# Scoped MsQuic regression

`test-msquic-regression.py` reuses the already-built repository compiler and
postprocessor, regenerates the selected 47-TU MsQuic product, and checks its
native/managed ABI and public consumer against the current picotls translation.
It does not rebuild shared compiler tools. Preparation and final closure
publication are separate invocations:

```sh
python3 blink/scripts/test-msquic-regression.py --cache-root /path/to/msquic-cache
# Review the printed attempt's preparation receipt and closure changes first.
python3 blink/scripts/test-msquic-regression.py --finish blink/artifacts/msquic-regression/attempt-EXAMPLE
```

The default cache root is this checkout's `msquic` directory. A separate cache
must contain the checksum-pinned source archive under `ref/`. When neither a
matching local historical archive nor matching current live closure evidence
exists, it must also supply the complete immutable
`artifacts/closure-<16-character-closure-hash>/` tree. The wrapper verifies every
archived member, the exact archived closure, and its explicitly required stage,
evidence, compiler and generated-source hashes. It copies a missing archive
only into the archive location; it never restores historical generated objects,
compiler binaries or receipts over current live output paths. Existing
mismatched destination archive bytes cause rejection. This archive is historical
preservation, not current test evidence or an object cache for the fresh run.
If no historical archive exists but every current live closure-bound file
matches, the existing `--archive-current` recipe preserves those files first.
It is never used to bless mismatched live evidence.

Preparation runs the existing fast translation with `--no-build-tools`, then the
host contract, upstream public ABI, and raw/optimized rooted product consumer.
The host contract includes native/JIT/AOT binding, TLS-layout and core-layout
probes. The public ABI includes native/JIT/AOT observations; the rooted product
consumer runs in all four modes. It does not claim implemented transport merely
because ABI and build checks succeed.

The sole excluded host assertion is:

```c
CHECK(CxPlatInitialize() == QUIC_STATUS_NOT_SUPPORTED);
```

The original fixture supplies a synthetic initialization callback returning that
status. Its invocation is custom host-failure injection. The wrapper removes
exactly this assertion from a derived C probe, leaving the rest byte-for-byte
unchanged. The callback definition remains as unused fixture code. The 99
missing-slot table checks, invalid registration parameters, loaded-state
rejections and copied-table ownership checks remain normal boundary validation.
The generated abort callbacks are unexpected-call canaries; a passing run never
executes them. No allocation-failure loops, packet corruption, or malformed
traffic tests are selected.

The wrapper also derives copies of `test-host-contract.py` and
`freeze-product.py` by changing exactly their binding-source path expression.
They live directly under `msquic/generated/`, preserving each script's original
root calculation and existing include/project paths. The original C/script
bytes, derived hashes, exact substitutions and unified diffs are retained in
the attempt. No tracked MsQuic source or generated C# is repaired. Drift in any
original or derived input prevents qualification.

After preparation is reviewed, `--finish` runs the derived closure freezer with
`--without-sqlite`. **This intentionally updates the tracked
`msquic/config/product-closure.json`.** It binds the actual derived binding-source
hash through the executed host-contract receipt; the wrapper records this hash
and the before/after closure bytes explicitly. It never substitutes the original
binding hash for the executed one. The separate compiler-metadata helper may be
built to inspect the frozen compiler; the compiler itself is not rebuilt.
Historical SQLite injection suites are neither executed nor relabeled.

The unchanged public consumer then runs exactly 32 cases: raw/optimized ×
JIT/NativeAOT × IPv4/IPv6 × roundtrip-resumption, wrong trust, wrong name, and
wrong ALPN. Authentication and configuration rejections are ordinary runtime
error handling and remain included. The public runner verifies process results,
application bytes/FIN/resumption, cleanup and binary/input hashes; this wrapper
also requires the exact ordered case set. It does not claim the separate full
selected-API campaign, dependency isolation, or historical transport results as
fresh evidence.

Each attempt preserves its wrapper, commands, closed log hashes, source/tool identities and
receipt under `blink/artifacts/msquic-regression/`. Commands receive an isolated
`TMPDIR`. Timeout or interruption triggers termination of the owned command
process group and escalation after a grace interval; no injected timeout tests
are added, and unkillable kernel processes are outside this guarantee. Failed
attempts are retained. `--finish` requires the exact prepared wrapper, tool,
authored-source and subordinate-receipt identities and cannot restart a finish
that already began. Retry with a new preparation attempt instead of overwriting
failed matrix evidence. The broad final disk inventory is labeled archival;
only the named fresh gates establish qualification.

## Qualification status

The scoped wrapper changes are source-only and await the serialized execution
slot. The earlier full MsQuic receipt remains historical and does not qualify
this derived binding selection. Historical runtime was roughly 22 minutes,
dominated by the complete host-contract object/ABI gate; this is an estimate,
not a time bound or a fresh measurement.
