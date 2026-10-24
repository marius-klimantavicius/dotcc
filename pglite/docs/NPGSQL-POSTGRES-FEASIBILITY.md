# Npgsql integration and upstream PostgreSQL feasibility

Investigated 2026-10-24. The original investigation below is source-level
feasibility research; its findings are not runtime compatibility claims.

The user subsequently authorized an embedded Npgsql implementation, with
multiple simultaneous sessions, streaming COPY and running-query cancellation
expressly excludable. That separate campaign is tracked in
[NPGSQL-PLAN.md](NPGSQL-PLAN.md). It selects a copied managed source fork rather
than a submodule, socket proxy or replacement query engine. Implementation and
qualification evidence must be assessed separately from this original research.
Full upstream PostgreSQL translation remains research only and is not authorized
by the Npgsql implementation request.

The implemented copy is Npgsql v10.0.1 at
`12d792ac2163596385152dcea5c68ba99769731f`, under
[`src/Npgsql`](../src/Npgsql/Npgsql.csproj), with the distinct assembly identity
`Managed.PGlite.Npgsql`. It executes through the actual translated engine.
[Qualification](NPGSQL-QUALIFICATION.md) and the
[validation receipt](../../docs/validation/pglite-npgsql-20261024.json) record the
completed Linux x64 raw/processed JIT/NativeAOT matrix and explicit exclusions.
These later execution results do not change the research status of the original
upstream PostgreSQL assessment below.

## Sources inspected

- Npgsql commit `54053259ef2b1bc531f8c4c937c33a9eaa57b0a6`, resolved from upstream
  HEAD for inspection. Choose a maintained release explicitly before a fork;
  this research pin is not a release recommendation.
- Official PostgreSQL `REL_18_6`, commit
  `724edf9bde9d356724ad384a2e196edc3c9f80f7`.
- Our PGlite PostgreSQL fork, commit
  `29b6424aa312ec3556247db069939006dd27f878`.
- Current authored API, runtime services, backend manifest and existing
  protocol acceptance cases in this repository.

Inspected snapshots, hashes and the upstream/fork file comparison are retained
under `pglite/artifacts/feasibility-20261024/`. The archive comparison covers
regular files, not missing submodule contents or a configured build.

## 1. Adapt Npgsql to the embedded translated engine

**Feasible in principle, with a bounded embedded connection profile.** Reusing
Npgsql's commands, readers, PostgreSQL type codecs and transaction machinery is
more attractive than recreating that surface. Our engine already accepts real
extended-protocol messages. `ApiProtocolEngine/CoreProtocolCases.cs` exercises
binary parameter/result formats and suspended portals against translated code.
This does not yet qualify Npgsql's full type matrix.

The proposed path is:

`Npgsql API → embedded connector/transport → dedicated owner thread → translated backend`

Retain a small, reviewable fork and introduce an embedded data-source/connector
mode. Preserve upstream notices and record the copied revision. Decide package
and assembly identity explicitly: coexistence with stock Npgsql and compatibility
with its EF Core provider are separate requirements and need separate tests.

### Changes required

1. **Connection opening and transport.** The inspected
   [connector](https://github.com/npgsql/npgsql/blob/54053259ef2b1bc531f8c4c937c33a9eaa57b0a6/src/Npgsql/Internal/NpgsqlConnector.cs)
   creates a socket/NetworkStream and performs startup/authentication. Its buffers
   use `Stream`, but timeout paths also access the socket. A custom stream alone
   is insufficient; embedded opening, timeouts, cancellation and cleanup need
   explicit paths. The inspected data-source builder offers no public arbitrary
   stream factory.
2. **Startup/session metadata.** The current raw PGlite API starts after engine
   initialization and accepts ordinary tagged frontend frames, not an untagged
   startup packet. Bootstrap the connector from the real engine's parameters and
   readiness state. The fork contains `pgl_sendConnData`, which emits connection
   data, but it is not a qualified public managed startup transport. Audit its
   protocol/version/key representation before reuse. Do not fabricate a working
   cancellation key or claim socket authentication for a trusted local owner.
3. **Execution and framing.** Marshal core work to its owner thread, preserve
   arbitrary write fragmentation and extended-message boundaries, and return the
   actual backend bytes to Npgsql. Do not translate Npgsql commands back into
   SQL strings or decode/re-encode typed results through our high-level API.
   Preserve Npgsql's catalog/type bootstrap queries.
4. **Session ownership.** Start with one exclusively leased connection. The
   current engine permits one owner per process. Serializing individual commands
   from several apparent sessions would incorrectly share transactions, prepared
   statements, temporary tables and session settings. Pooling and multiplexing
   require explicit lifecycle semantics; disabling them is the simplest first
   profile. Connection disposal must release the lease and clean session state
   without accidentally destroying a caller-owned database.
5. **Cancellation and waits.** The current worker can cancel queued work but
   cannot interrupt running SQL. Npgsql's ordinary cancellation opens another
   connection. An embedded implementation needs safe-point cancellation and
   wake-up semantics, or must explicitly reject unsupported cancellation modes.
   Closing a stream must not be presented as successful query cancellation.

### Streaming COPY is the critical protocol boundary

Npgsql's
[binary importer](https://github.com/npgsql/npgsql/blob/54053259ef2b1bc531f8c4c937c33a9eaa57b0a6/src/Npgsql/NpgsqlBinaryImporter.cs)
sends COPY, waits for CopyInResponse, then accepts data from the caller. Our
`ExecProtocolRaw` requires COPY FROM and its data/completion frames together;
the engine cannot currently suspend inside COPY, and output callbacks are
delivered after translated entry returns. A naive adapter deadlocks or gives
the engine premature EOF.

An expanded transport could keep the backend on its owner thread while its
input callback waits for actual bytes and its output callback publishes response
chunks. The frontend must be able to read and write independently of completion
of that owner operation. Queue draining, backpressure, cancellation and teardown
need design and tests. This is plausible because callbacks already cross the
boundary; it has not been implemented or proven. Simply queueing CopyData as
another worker operation cannot work while COPY blocks the owner.

Ordinary queries could initially use buffered exchanges. That would retain the
existing response-size/memory limits, even if Npgsql exposes a sequential reader.
Do not advertise bounded-memory streaming on that basis. For intended full
Npgsql usage, establish duplex transport early rather than accumulating API
workarounds. See upstream [COPY semantics](https://www.npgsql.org/doc/copy.html).

A loopback TCP/Unix-socket facade is an alternative that could retain stock
Npgsql. It still needs startup handling, a session contract and duplex COPY;
it does not turn one embedded session into a multi-session server.

### First implementation gate, if pursued

Use one connection and prove real startup/type discovery, SELECT with binary
parameters/results, prepared statements, transactions, SQL-error recovery and
clean close/reopen. Then add large/fragmented requests, early reader disposal,
COPY handshake/data/abort, queued/running cancellation and supported async/AOT
cases. Reuse applicable Npgsql tests and compare with same-version native
PostgreSQL. Dapper/EF compatibility should follow actual provider tests.

## 2. Translate upstream PostgreSQL itself

**A credible research direction, substantially larger than Npgsql integration.**
The comparison found only **24 changed common files, 25 added files, and no
removed regular files** between the pinned upstream and fork archives. Many
changes are build or embedding adjustments. This supports substantial reuse;
it does not measure the amount of unqualified server behavior.

Our backend manifest already includes `postmaster.c`, `launch_backend.c`,
autovacuum, checkpointer, WAL writer and dynamic shared-memory code. They were
compiled under embedding defines and overrides. Their presence in generated
C# does not establish successful server execution. The fork changes checkpoint
execution, protocol entry, process-local shared-memory handling and other
behavior used by the embedded profile.

### Concrete runtime gaps

| Boundary | Current evidence and required server work |
| --- | --- |
| Process lifecycle | `ProcessSignalLib.cs` returns EPERM for fork/exec/waitpid. Ordinary PostgreSQL starts per-session/background processes. Fresh-process startup, inherited descriptors, child status and crash propagation need real implementations. |
| Shared memory | Fork `pgl_shm*` uses process-local allocations. Upstream requires actual cross-process mappings, attachment accounting and, on EXEC_BACKEND paths, consistent virtual addresses for shared pointers. |
| Mapping safety | `MemoryMappingLib.cs` has real Linux mmap but rejects MAP_FIXED. Address reservation/reattachment must coexist with the CLR; overwriting runtime mappings is not a solution. |
| Semaphores/atomics | The semaphore adapter allocates native storage privately and looks it up in an owner-local dictionary. Passing a process-shared flag does not make that allocation shared. Use actual shared storage and validate cross-process synchronization and memory ordering. |
| Signals/latches | Embedded safe-point delivery is not qualification of interprocess signals, child death, latch wakeups, cancellation or blocking I/O interruption. |
| Server behavior | Authentication, concurrent sessions, locks/deadlocks, MVCC, autovacuum, checkpoints, WAL recovery, process crashes and parallel workers require native differential validation. Existing SQL execution is valuable groundwork. |
| Dependencies | LLVM JIT, TLS/ICU/compression choices, dynamic extensions and external tools need separately declared profiles. Native extension loading cannot silently substitute an untranslated database engine. |

### Most promising process design

Investigate PostgreSQL's existing
[EXEC_BACKEND path](https://github.com/postgres/postgres/blob/724edf9bde9d356724ad384a2e196edc3c9f80f7/src/backend/postmaster/launch_backend.c).
It already reconstructs backend state after starting a fresh process. On Unix
it still calls fork then exec, so enabling the define alone does not solve our
runtime gap. A narrow platform launcher using spawn/fresh managed executables
would need to preserve its descriptor/state transfer contract.

This is preferable as an initial hypothesis to cloning a running CLR with fork
or replacing all PostgreSQL processes with managed threads. NativeAOT is a
candidate deployment profile, not a guarantee of fork safety. A threaded port
would also require extensive global/TLS/context and crash-isolation work.

The EXEC_BACKEND path reattaches shared memory at the postmaster's address.
On Linux upstream uses System V shared memory for this mode; see
[sysv_shmem.c](https://github.com/postgres/postgres/blob/724edf9bde9d356724ad384a2e196edc3c9f80f7/src/backend/port/sysv_shmem.c).
The source itself flags address-layout challenges. An implementation must prove
safe reservation/reattachment with normal address randomization, or adopt an
explicitly different shared-pointer design. Do not solve this by changing the
machine's global address-randomization setting.

### Investigation gates before a full campaign

1. Translate unmodified upstream single-user mode to separate removed embedding
   hooks from multi-process requirements. This would not qualify a server.
2. Prove a translated parent plus freshly started translated child sharing a
   real mapped C structure, semaphore/atomic, inherited socket and wakeup path.
   Prove pointer/address validity across repeated starts and child failure.
3. Run the real translated postmaster with one session and required background
   processes, then two concurrent sessions with lock/MVCC/error tests.
4. Expand native differential coverage to crash recovery, WAL durability,
   autovacuum, checkpoints and the chosen replication/parallel features.

For this project, native-equivalent unsuccessful outcomes count toward parity
when the observable contract matches. Tests should separately report functional
success, parity and unresolved evidence, following the MsQuic investigation.

The original recommendation was embedded Npgsql integration first, then the
small upstream process/shared-memory feasibility gates. No integration code,
PostgreSQL translation probe or full-server implementation was performed as part
of this research. The subsequently authorized Npgsql campaign is linked above;
it does not authorize those full-server investigations as implementation work.
