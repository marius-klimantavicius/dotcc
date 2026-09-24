# Real managed-host execution harness

This BCL-only executable references `src/Managed.Valkey`, which references the
actual generated `generated/TranslatedValkey` project. It has no substitute core,
native-server implementation, or fallback success path. Generate the product and
build the consumer solution before running it.

```sh
valkey/scripts/validate-managed.sh
valkey/scripts/validate-managed.sh --native-compare --persistence-exchange --upstream-protocol
valkey/scripts/validate-managed.sh --native-compare --persistence-exchange --upstream-protocol --aot
```

The validator consumes existing generated inputs; it does not download sources.
`--native-compare` requires the passing same-pin native control produced by
`scripts/oracle.py`. `--upstream-protocol` copies the pinned reference tree to
writable build staging and runs its Tcl `unit/protocol` suite using `--host` and
`--port` against the managed process. The reference tree stays unchanged. The Tcl bootstrap sees an executable barrier
that always fails if invoked; attempting to launch a native server fails the gate.
Six explicitly named `needs:debug` tests are excluded because DEBUG is outside
the managed profile; the exact names and reasons appear in the receipt. With
`--aot`, the same protocol suite and exclusions also run against the NativeAOT
`--serve` host; separate logs live in `aot-upstream/` and the receipt records
`checks.native_aot_upstream_protocol`.
`--variant raw|processed` selects the real generated project (processed by default)
through the public API project reference. `--no-build` explicitly reuses a
previously compiled **real** consumer and verifies its translated-library hash
against the requested variant's compiled output. Each run
creates a fresh directory under `artifacts/managed-validation/` with commands'
logs, JIT/AOT reports, persistence files, binary hashes and a final receipt.

NativeAOT publishing happens only after the same invocation's JIT checks pass.
The AOT project roots the complete translated assembly, so trimming cannot hide
unreachable compilation errors. A native binary must actually run and report
NativeAOT execution to pass; a successful cross-build alone does not count.

The managed cases cover:

- Two simultaneous owners with distinct authentication, key spaces, script caches,
  function registries, data directories, and independent shutdown. Occupied-port
  startup runs partial initialization, then faults readiness, completion and a
  queued persistence snapshot; cleanup must finish without quarantine while
  both existing owners remain responsive.
- Real TCP RESP2/RESP3, fragmented 256 KiB binary data, pipelining, collections,
  transactions, expiration metadata, Lua callbacks, cjson/bit, protected script
  errors, and callback survival after GC.
- Admitted numeric, bit, hash, list, set, sorted-set, stream consumer-group, HLL,
  geo, scan, WATCH conflict and ACL command/key-permission transitions.
- Explicit rejection of fork-dependent commands and configuration, replication,
  native module loading and Lua debugging, including MULTI/EXEC and Lua dispatch.
  Checks require real RESP errors and unchanged configuration/state rather than
  depending on one error wording or which admission layer rejects the request.
- Failed foreground SAVE retaining a live server, recovery and retry, followed by
  a fresh owner's RDB reload of binary data, types, TTL and registered functions.
- Fresh startup AOF, a nonempty multipart manifest, synchronous append, two
  existing-AOF replays and persisted functions.
- Actual lazy-free worker completion observed through upstream counters, peer
  responsiveness and immediate shutdown with queued lazy-free work. EverySecond
  AOF writes must advance the primary stream offset and publish background fsync
  completion through that offset before all 256 values are checked after restart.
  Persistence snapshots also reject pre-cancelled and stopped-owner requests.
- RESP SHUTDOWN with successful completion and an unaffected peer, repeated startup
  on the same port, concurrent/idempotent and pre-cancelled stop requests, completed
  owner cleanup without quarantine, exclusive reopening of persistence files,
  unchanged process working directory, and an unaffected peer.


`--persistence-exchange` adds four real-process gates: native→managed and
managed→native for RDB and multipart AOF. Each producer stops before its files are
hashed and copied to a separate consumer directory. Both implementations verify
binary payloads, strings, lists, hashes, sets, sorted sets, streams, exact absolute
expiry, transaction/script effects and persisted functions through RESP.

The pinned native checksum/framing checkers validate produced files before and
after replay without `--fix`; SHA256 inventories verify they do not change bytes.
This pin's `valkey-check-aof` recognizes only legacy REDIS base-file magic, while
its server emits VALKEY magic. The validator therefore checks the manifest's
file/sequence/type structure, checks the RDB base with `valkey-check-rdb`, and
checks incremental files individually with `valkey-check-aof`. This tooling
limitation is recorded in each integrity receipt; actual consumer replay remains
mandatory. With `--aot`, all four exchange gates repeat using the native managed
consumer after JIT succeeds. Native preflight alone never passes a managed gate.

This is initial integration coverage, **not completion of P3–P8**. It does not
claim exhaustive command/upstream scripting coverage, all AOF recovery/corruption cases, crash durability, heap leak freedom
from public lifecycle checks alone, or Windows execution.

A serve mode is available for additional external test clients:

```sh
dotnet run --project valkey/tests/ManagedHostTests -c Release -- \
  --serve --port 16379 --directory /tmp/managed-valkey-test
```

It binds loopback and prints `MANAGED_VALKEY_READY <port>` only after the owning
API reports readiness. Send `STOP SAVE` or `STOP NOSAVE` on standard input to stop;
closing stdin requests NOSAVE. Startup, execution, or cleanup failure returns a
nonzero process exit code. Readiness text alone is never a completed test result.

Integration checks require a new or empty data directory; serve mode may reopen
existing persistence. Direct execution accepts `--suite all|rdb|aof`, optional `--port`/`--peer-port`,
`--directory`, `--report`, and `--native-port` for an already running disposable
native control. Omitted ports are reserved from loopback ephemeral ports and
released immediately before startup; an external bind race fails the run visibly.

Exchange subprocesses use `--exchange export|import --suite rdb|aof --expiry
<absolute Unix milliseconds>` with `--directory` and `--report`. Export requires
an empty directory; import opens the other implementation's persisted files and
validates them before shutdown. The Python validator supplies these arguments
and records each producing and consuming backend explicitly.
