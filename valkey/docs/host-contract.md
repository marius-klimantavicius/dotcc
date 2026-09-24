# Host boundary audit

Reviewed 2026-09-24 against Valkey 9.1.2, commit
`7f1dffedff6de73058b2c2a389422b6ecd56c8fb`. This is source-level feasibility
work; none of the boundaries below is claimed qualified in a translated server.
Paths and line numbers refer to that immutable tree under `ref/`.

## Concrete startup sequence

The CLI entry is `src/server.c:7459`, `main`. An embedding entry must preserve this
ordering while separating process setup from per-instance state:

1. Set the allocator error handler, seed random/hash state and initialize CRC64.
2. `initServerConfig` (`server.c:2265`), `ACLInit`,
   `moduleInitModulesSystem` and `connTypeInitialize`.
3. Apply configuration and profile guards before initialization can perform I/O.
4. `initServer` (`server.c:2882`), module initialization/loading and ACL loading,
   then `initListeners` (`server.c:3103`).
5. For `LUA_ENABLED && STATIC_LUA`, load the static Lua engine before data loading.
6. `InitServerLast` (`server.c:3188`) creates background workers and initializes
   I/O threads, then `aofLoadManifestFromDisk`, `loadDataFromDisk` and
   `aofOpenIfNeededOnServerStart`.
7. Publish readiness only after loading/opening persistence succeeds. Dispatch
   through `aeMain` (`ae.c:540`) or equivalent calls to upstream
   `aeProcessEvents` preserving before/after-sleep hooks and timer policy.

Calling CLI `main` directly is not a reusable host seam: it changes signal
handlers and process umask, supports daemonization, changes process title and
CPU affinity, and terminates through `exit`. `initServer` itself also installs
signals and calls `ThreadsManager_init`/`makeThreadKillable`; extracting only
`main` is insufficient. Generic libc `exit`, `_exit`, and `_Exit` currently call
`Environment.Exit`, so expected configuration, startup and persistence errors
must be contained at an explicit owning boundary before any embedded execution.
Do not replace a nonreturning failure with a returning no-op.

## Earliest concrete boundary blockers

| Boundary | Source evidence | Required implementation |
| --- | --- | --- |
| Wakeup pipe | `module.c:13063` unconditionally calls `anetPipe`; `DotCC.Libc/PosixFsLib.cs:417` currently fails `pipe` with EPERM. | Real per-owner readable/writable wakeup descriptors or a typed adaptation of the module notification transport; preserve nonblocking behavior and errors. |
| Readiness | `ae.c:52` selects OS backend; fallback `ae_select.c` requires fd sets/select. `DotCC.Libc/UnistdLib.cs` fd-set operations are no-ops and select throws. | Explicit working backend with create/add/delete/resize/poll/free and wakeup. Falling back to select does not establish support. |
| Static Lua symbols | `module.c:13540` resolves static symbols with `dlopen(NULL)` and `dlsym`. | Resolve the actual translated `ValkeyModule_OnLoad_lua` and `ValkeyModule_OnUnload_lua`; preserve normal module registration/unregistration. |
| Paths | `config.c:2851` changes cwd; libc `chdir` calls `Directory.SetCurrentDirectory`. | Per-owner path context for all relative file operations, including temporary files and manifests, without process cwd changes. |
| Shutdown | `db.c:1486` and `server.c:1548,1551` exit after successful upstream shutdown. | Owner stop/unwind with upstream persistence/error decisions retained and no process exit. |
| Full disposal | `finishShutdown` closes listeners/unloads modules, and CLI return deletes the event loop, but no complete embedded destructor is present. | Drain workers and callbacks; free clients, data/config/registries, file handles and owner allocations with tested failure rollback. |

Existing libc BCL sockets and `poll` (`DotCC.Libc/PosixLib.cs:249`) are useful
building blocks, not evidence that an `ae` backend works. The public upstream
`aeSetCustomPollProc` hook (`ae.c:561`) can preserve dispatch, but replacing only
poll does not replace backend creation and event registration: `aeCreateEventLoop`
still calls `aeApiCreate`, and additions still call `aeApiAddEvent`. A custom poll
also must implement the timer/don't-wait behavior that its branch bypasses.

The initial executor serializes translated command/event work per owner. BCL
completions may enqueue readiness and wake that executor; they cannot enter the
same server concurrently. Socket boundaries retain upstream partial reads,
partial writes, would-block, EOF, close and errno behavior. Buffers stay pinned or
copied for the complete lifetime of a host operation.

## Static Lua and callback identity

`src/modules/lua/engine_lua.c:562,568` exposes both suffixed entrypoints.
`moduleLoadStatic` (`module.c:13589`) feeds the resolved load callback into
`moduleInitPostOnLoadResolved` (`module.c:13396`); this performs module context
creation, API registration, ACL command-bit recomputation and module events.
Preserve this path. Static unload also resolves through `moduleLoadStaticSymbol`
(`module.c:13635`). Resolve only the explicitly supported static module/symbol
pairs, returning failure for unknown names; arbitrary native module loading is
outside the initial profile. A staged, hash-checked direct-symbol adaptation is
appropriate if an instance-aware typed override cannot preserve these pointer
signatures.

Bundled Lua uses protected nonlocal exits (`deps/lua/src/ldo.c` and `luaconf.h`).
Its module API uses function-pointer tables and pointer/integer conversions.
Translate the pinned Lua engine and extensions with the same instance calling
convention as the server; standalone Lua historical results do not qualify this
combination. The module API and Lua callback pointers must carry their owner
through initialization, execution, error unwinding and teardown.

## Ownership inventory and workers

`--instance-methods` must cover every object. In addition to `server` and shared
reply objects, ownership includes:

- `module.c` registries, timer tree, temporary clients, callback queues and mutexes;
  `scripting_engine.c:64` engine manager; Lua API pointers and engine contexts.
- `connection.c:75,87,102` function-local connection-type caches.
- `dict.c`/`hashtable.c` hash seeds and resize policy; `mt19937-64.c` random state.
- `zmalloc.c:99` thread-local index, accounting arrays/counters and OOM handler.
- `bio.c` worker records, queues and job counters, plus thread-local error state.

`InitServerLast` always calls `bioInit`, even with one I/O thread. The workers
handle fsync, close and lazy-free jobs; selecting one I/O thread does not remove
this boundary. Existing `DotCC.Libc/PthreadLib.cs:91` provides an owner-aware
`pthread_create<T>` which retains/binds runtime context and releases it at thread
exit. Reuse and qualify that contract with real bio work; do not replace worker
creation with successful no-op results. Upstream bio workers run until cancellation;
its crash-oriented `bioKillThreads` is not by itself proof of cooperative disposal.
A reviewed stop/drain/join adaptation remains required.

Every synchronous managed entry binds `__DotCcEnter`; no binding spans an await.
Deferred work retains the owner until it can no longer invoke callbacks. Dispose
only after event dispatch, workers and deferred callbacks are quiescent; runtime
owner disposal must not be used to free live callback state. Native-width layout,
atomics, packed/tagged storage and per-owner TLS still require P1 executable
probes under JIT and NativeAOT.

## Stop and rollback contract

Use `prepareForShutdown` (`server.c:4723`) and `finishShutdown`
(`server.c:4806`) for upstream save/flush decisions, with the owning API expressing
SAVE/NOSAVE policy. A failed save keeps the server running unless FORCE applies.
Successful stop must stop only its own event loop, close its own clients, drain
and join its own workers, and release its own files and state. `SHUTDOWN` reaches
the same lifecycle. Startup failures unwind only successfully initialized stages.
No readiness task can complete successfully after a load/listen failure.

The first executable host proof must exercise two simultaneous instances, stop
while idle and while clients/jobs are active, ordinary startup/file failures,
Lua errors and restart. Source review alone leaves P1 open.
