# Translate Valkey to C# with dotcc

Created 2026-09-24. Campaign directory: `<repo>/valkey/`.
Status: **Planning and source review complete; translation implementation has
not started.** The downloaded release and checksum are recorded in
[source.md](source.md). All implementation gates below remain open.

## Objective and inherited conventions

Translate Valkey's actual C server into a reusable unsafe C# library using dotcc,
with an owning managed server API and a runnable consumer. Keep command dispatch,
RESP parsing/replies, data structures, expiry/eviction, transactions, scripting
and persistence formats translated from upstream. A native Valkey wrapper, a
client that connects to a native server, or a separately written C# key-value
engine does not satisfy this objective. Native Valkey runs only as a test oracle.

This plan follows the reviewed [SQLite](../../sqlite/docs/PLAN.md),
[picotls](../../picotls/docs/PLAN.md), [MsQuic](../../msquic/docs/PLAN.md),
[libsmb2](../../libsmb2/docs/PLAN.md) and [Blink](../../blink/docs/PLAN.md) plans:

- Preserve pinned upstream sources and record the effective source closure.
- Fix shared compiler/runtime defects through reduced regressions and full-source
  retries. Parsing or C# compilation alone does not establish execution success.
- Supply explicit BCL host services and preserve upstream algorithms.
- Deliver raw and semantically post-processed output, qualified under JIT and
  NativeAOT, plus a separate consumer referencing the final generated project.
- Keep authored sources editable at their original `src/` locations, following
  Blink's current delivery convention. Regeneration must not overwrite them.
- Record actual results, limitations and unrun platforms. Historical passes and
  campaign-specific authorizations from other plans do not transfer to Valkey.

Use .NET 10/C# 14, dotcc's headers/libc and existing generic compiler facilities.
Normal BCL implementation dependencies are acceptable. Do not add an
application-owned native Valkey, Lua, allocator, socket or filesystem backend.
Keep Roslyn and C compilation out of the delivered application's runtime.

Commit locally after each significant milestone and each coherent tested
compiler/runtime repair; do not wait for the entire campaign. Include evidence
and update this plan in milestone commits. Preserve unrelated work, stay on the
current branch unless otherwise requested, and do not push. This planning request
does not itself execute the implementation campaign or authorize sub-agents.

## Reviewed baseline and reproducible inputs

Use proposed release **Valkey 9.1.2**, commit
`7f1dffedff6de73058b2c2a389422b6ecd56c8fb`. The immutable archive was downloaded
and hashed during planning; [source.md](source.md) records its URL, SHA-256,
local paths and concrete source findings. P0 must create machine-readable
manifests and validate the build profile. Do not silently upgrade the pin.

Valkey is a multi-translation-unit server, not an amalgamation. Derive an
explicit list from the pinned server/dependency Makefiles, including transitive
headers, generated command tables and the static Lua engine. Classify each input
as translated C, generated C/header, managed host boundary, native test-only tool,
or deferred feature. Account for portable fpconv/fast-float, hashes, checksums,
compression and histogram helpers. Audit actual libvalkey dependencies instead
of including the whole CLI build or assuming the server needs none of them.

Lua requires the bundled `deps/lua/` revision, extensions and
`src/modules/lua/` integration. The repository's existing Lua translation is
regression evidence and reusable compiler support, not a substitute for Valkey's
patched Lua source. GoogleTest C++ drivers and Tcl tests are separate test inputs;
do not send them to dotcc as C. See the immutable build/test links in source.md.

Keep `ref/` unchanged. Run upstream generators and native builds in staged copies
under `build/`. Hash generated command definitions, formatting/release headers,
generator inputs, options and interpreter identity; do not depend on timestamps
or the checkout's incidental Git metadata for product identity. Preserve bundled
notices and record any test-only downloads independently.

## Workspace and required delivery paths

The following is the **implementation layout**; executable scripts, C# projects
and the solution are milestone deliverables, not working artifacts of this plan.
Do not create an empty solution or a successful no-op translation placeholder.

```text
valkey/
  README.md
  ManagedConsumer.slnx                root solution for final library/API/sample
  ref/                               verified unmodified archives and sources
    valkey-<commit>/                  upstream src/, deps/, tests/, utils/
  docs/
    PLAN.md                          milestones and acceptance criteria
    source.md                        revisions, URLs, digests, licenses, tools
    configuration.md                 source closure, ABI, defines, feature matrix
    host-contract.md                 I/O, clocks, allocation, workers, errors
    persistence.md                   RDB/AOF guarantees and fork limitations
    api.md                           instance/lifetime/cancellation contract
    blockers.md                      reduced failures, fixes and full retries
    validation.md                    commands, case results, exclusions, receipts
    usage.md                         clean generation/build/run instructions
  config/                            source manifests, defines and typed overrides
  scripts/
    common.sh                        paths and SQLite-style Python 3 resolution
    fetch.sh                         checksum-verified input preparation
    translate.sh                     complete default translation entry point
    translate.py                     pipeline driver, if Python is used
    build.sh                         build existing final output
    test.sh                          focused campaign tests and consumer checks
    oracle.sh                        native reference build/run
    verify.sh                        regeneration and full acceptance matrix
  src/
    Host/                            authored BCL services and boundary declarations
    Managed/                         owning ValkeyServer API
    Server/                          managed executable for clients/upstream tests
  samples/ManagedConsumer/            separate runnable owning-API example
  tests/                             ABI, RESP, commands, scripting, persistence
  generated/
    TranslatedValkey/                 final post-processed C# and .csproj
    TranslatedValkey.Raw/             isolated raw comparison library
  build/                             staging, native and JIT/AOT outputs
  artifacts/                         logs, manifests, diffs and validation receipts
```

All campaign-specific files belong under `valkey/`; shared compiler/libc fixes
and generic regressions remain in their existing repository projects. Ignore
`ref/`, `generated/`, `build/` and `artifacts/`. Commit authored sources, scripts,
configurations, tests and documentation. Store no disposable credentials in Git.

`valkey/ManagedConsumer.slnx` must include
`generated/TranslatedValkey/TranslatedValkey.csproj`, authored Host/API/server
projects as needed, and `samples/ManagedConsumer/ManagedConsumer.csproj`.
Use ordinary relative project references. The managed facade references the
generated core; avoid a dependency cycle between Host, facade and generated code.
Bridge files that require generated types can be compiled into the generated
project through relative links to their canonical files:

```xml
<Compile Include="../../src/Host/ValkeyIoBridge.cs" Link="Host/ValkeyIoBridge.cs" />
```

Reference separate host projects in place where their types are independent of
the generated core. Do not copy authored code into the active generated output.
Editing it through the solution and rebuilding must use those edits immediately.
Post-processing applies only to generated code. Test harnesses, fake command
handlers and native-oracle entry points must not enter the product assembly.

## Python resolution and input acquisition

Every campaign shell entry point that uses Python must source `scripts/common.sh`
and invoke `"$PYTHON_CMD"`, including upstream generators. Resolve Python exactly
as [sqlite/scripts/common.sh](../../sqlite/scripts/common.sh) does:

```bash
PYTHON_CMD=python3
if ! "$PYTHON_CMD" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then PYTHON_CMD=python; fi
if ! "$PYTHON_CMD" -c 'import sys; raise SystemExit(sys.version_info.major != 3)' >/dev/null 2>&1; then
  echo "Python 3 is required" >&2
  exit 1
fi
```

Resolve `VALKEY_ROOT` and `DOTCC_ROOT` from `BASH_SOURCE[0]`, with quoted paths.
A Python-backed `translate.sh` sources this helper and uses
`exec "$PYTHON_CMD" "$VALKEY_ROOT/scripts/translate.py" "$@"`. Python subprocesses
use `sys.executable`; native Make invocations receive `PYTHON="$PYTHON_CMD"`.
Do not hard-code `python3` in wrappers or rely on a Python 2 `python` executable.
If a generator needs a newer Python 3 minor version, diagnose that prerequisite
separately rather than changing the interpreter-selection order.

**Fetching is enabled by default.** The ordinary `translate.sh` invokes the
verified fetch/preparation path, acquiring missing pinned inputs and validating
cached inputs. `--no-fetch` is an explicit opt-in for limited-network environments
where the reference sources have already been downloaded. It must never become
the default, an environment-derived default, or a silent fallback after a fetch
failure. Apply and forward the same option through `verify.sh`, `fetch.sh` and
any other entry point that can acquire reference inputs.

| Invocation | Required behavior |
| --- | --- |
| `./valkey/scripts/translate.sh` | Fetch/prepare pinned sources as needed, verify integrity, then run the full pipeline. A network/integrity failure is a nonzero result. |
| `./valkey/scripts/translate.sh --no-fetch` | Make no reference-source download, clone, pull, tag lookup or submodule update. Verify and reuse the exact local inputs, then run the same generation, translation, post-processing and build stages. |
| `./valkey/scripts/verify.sh --no-fetch` | Propagate the mode to all reference/test-input preparation; report missing prerequisites without attempting to obtain them. |

In `--no-fetch` mode a verified local archive may be extracted, or an already
extracted tree may be used after comparison with a trusted file manifest or the
checksum-verified archive. Support the extracted-tree-only case when that
verification manifest was retained from a prior verified fetch. Verify required
headers, bundled dependencies and generator inputs, not just `src/server.c` or
the version string. Reject missing, wrong-version, modified or incomplete inputs
with exact paths and the corrective fetch command; never overwrite modified
references silently. Do not manufacture a trusted manifest from an unverified
tree merely because downloading is unavailable.

The option controls reference acquisition; it does not imply `--no-restore`,
`--no-build`, skipped post-processing, or reuse of stale generated files. NuGet,
.NET SDK/NativeAOT packs, compilers and test tools have separate prerequisites.
Document how to pre-provision those caches for a completely disconnected run,
and record fetch mode, source identity and validation method in every receipt.

P0 script checks must cover Python 3 preference, Python 3 under `python` only,
rejection when neither works, arguments/paths with spaces, default acquisition,
successful `--no-fetch` with verified cached inputs, archive-free verified tree
reuse, and failure for absent/tampered inputs. Exercise `--no-fetch` with source
network operations blocked and assert none are attempted. Shell syntax checks
alone do not prove the mode reaches nested generators or dependency preparation.

## Required first product profile

P0–P9 target a useful **standalone embedded server**. This bounded initial profile
does not claim full Valkey deployment parity. P0 must freeze a command/configuration
inventory with required, translated-but-unqualified and deferred entries.

| Area | Required by initial completion |
| --- | --- |
| Protocol | RESP2/RESP3 negotiation, binary-safe requests/replies, pipelining, fragmented/coalesced traffic, errors and push replies; ordinary Valkey clients connect over TCP. |
| Data model | Strings, lists, sets, hashes, sorted sets, streams/consumer groups, bit operations, HyperLogLog and geo commands; encoding transitions and pin-supported hash-field expiry. |
| Keyspace | Multiple databases, expiry, TTL updates, rename/copy/delete, iteration, limits and upstream eviction policies. Use invariants for randomized/approximate behavior. |
| Execution | Upstream command table and arity checks, MULTI/EXEC/DISCARD/WATCH, blocking operations, Pub/Sub and client tracking/invalidation. |
| Scripting | Actual bundled Lua, static engine registration, EVAL/EVALSHA, script cache and FUNCTION/FCALL; bundled JSON/MessagePack/bit helpers required by the pin. |
| Access | AUTH/ACL user, key, command and channel checks; loopback default, explicit endpoints and credentials, useful command/server diagnostics. |
| Persistence | Foreground RDB save/load plus AOF configured at startup, append/replay and declared fsync modes; same-pin native file interoperability. Explicit restrictions on background operations below. |
| Hosting | Authored C# create/start/readiness/stop/dispose API, per-instance configuration/files/endpoints and multiple independent instances in one .NET process. |
| Execution services | One serialized command executor per instance, real BCL TCP/files/time/entropy, supported background workers and cooperative stop. Optional I/O-thread parallelism initially disabled. |
| Platforms | Linux x64 first; raw/processed × JIT/NativeAOT. Windows x64 is a subsequent qualification target; other architectures require separate execution evidence. |

Initially defer cluster/Sentinel, replication/failover, background RDB/AOF rewrite,
TLS, RDMA, native loadable modules, alternative scripting engines, vector commands,
jemalloc-specific defragmentation, daemonization/systemd and OS crash/profiling
facilities. Keep static Lua's required module/engine registration functional even
though arbitrary native `MODULE LOAD` is disabled. Do not claim unsupported
configuration or commands succeeded; reject them with documented errors.

Select native `MALLOC=libc`, static Lua, no TLS/RDMA/systemd, one I/O execution
thread, no automatic RDB saves and no automatic AOF rewrite for matching profile
comparisons. These are native recipe choices, not proof that dotcc has equivalent
preprocessor switches. Derive managed definitions from the actual build and
inspect their effect; never impersonate GCC or an OS to select missing facilities.

Newly discovered features may be classified explicitly in P0. Do not silently
remove a required row to pass a gate. Later extensions below provide the path
toward broader parity without treating a SET/GET smoke test as completion.

## Translation and host contracts

### C semantics, layout and source closure

Use dotcc's LP64 C model on an initial little-endian 64-bit host and a matching
native layout oracle. Record dialect and extension needs from the pinned inputs
(native Make probes GNU C11); do not assume a C17 flag alone resolves extensions.
Check real emitted storage for packed objects, bitfields, unions, flexible arrays,
SDS headers, listpacks, stream IDs, callback tables and pointer/integer tagging.
Measure sizes, alignment, offsets and serialized bytes. Windows LLP64 is not a
matching C-layout oracle merely because it is a 64-bit build.

Preserve translation-unit static/inline identity, global initialization order,
integer widths and overflow, byte order, atomics, floating conversion and callback
calling conventions. Qualify portable implementations of CPU-specific helpers;
do not replace upstream encodings or number parsing with approximate behavior.
Existing [instance translation](../../docs/instance-translation.md),
[function overrides](../../docs/function-overrides.md), source/object linking,
layout constants and split output are tools to validate, not evidence of success.

### Instance ownership and server lifecycle

Use `--instance-methods` consistently for all objects and managed-library linking
to own Valkey globals, function-local statics and C TLS per server. Audit shared
libc state, Lua/module registries, allocator accounting, clocks/random state and
cached pointers too; moving only `server` is insufficient. Do not substitute a
process-global lock or hidden native/managed subprocess for independent instances.

The C# owner invokes exported upstream initialization, event processing and
cleanup operations. Keep command processing and scheduling policy translated.
Use typed managed boundary overrides with explicit instance context when needed.
Each synchronous entry binds `__DotCcEnter()`; no binding may span `await`.
Deferred callbacks retain their originating owner through `__DotCcRetain()` until
drained. In instance mode callbacks include the owner argument; do not reuse a
static-mode function pointer signature. Root handles and pin/copy retained data,
never store movable C# references in C memory or retain stack-backed buffers.

Define startup rollback, readiness after listening/loading, cooperative stop,
client draining, persistence policy and disposal order. Drain I/O, timers,
background jobs and callbacks before freeing translated state. Keep CLI signals,
`exit`, process cwd/environment changes and global console changes out of the
embedding path. A protocol SHUTDOWN stops its owning server only. The managed
server executable may map process signals to that same owning API.

Unsafe translated code is not a hostile-code sandbox. Resource/accounting limits
and ACL behavior are functional contracts; hard termination and OS containment
would need a separately selected and qualified process mode.

### Networking, timers, memory and workers

Retain upstream `ae` scheduling, connection callbacks and RESP buffer handling.
Adapt readiness and nonblocking socket operations through a BCL host boundary:
accept/read/write, partial I/O, would-block/EOF, backpressure, close and wakeup.
Use BCL async completions to wake the owning executor; completions must not enter
the same translated state concurrently. Do not busy-spin or block every idle
client on a dedicated thread. Define buffer ownership until operations finish.

Supply monotonic time separately from wall time, secure entropy, native-width
errno/status mapping and bounded conversions. Keep expiry/eviction decisions
upstream. Reuse dotcc allocator/libc semantics where they fit, preserving
`zmalloc` accounting, realloc behavior, alignment and owner/free pairing. Do not
pretend host GC size is Valkey's allocation accounting.

Inventory `bio`, lazy freeing, fsync and pthread/atomic use even when I/O threads
are disabled. Either implement the required workers with correct synchronization
and owner propagation or select an upstream-supported synchronous configuration
with equivalent documented behavior. A missing worker cannot report success.
Map host exceptions at typed boundaries and test ordinary errors and cleanup.

### Persistence and fork

Retain translated RDB/AOF/rio/checksum/compression code. Host services implement
real file operations, short writes, flush/fsync semantics, rename, truncate,
directory operations and per-instance paths. Do not change process cwd. Specify
file versus directory durability and any BCL/platform limitation; a flush test
does not establish power-loss durability or cross-filesystem atomic replacement.

The initial profile uses explicit foreground SAVE, RDB load and startup-enabled
AOF append/replay. Qualify fresh and existing multipart AOF manifests, RDB AOF
preambles, transactions, script/function effects and graceful flush. The source
has a foreground initial AOF creation path; prove that the configured startup
path uses it. Runtime `CONFIG SET appendonly yes`, BGSAVE, BGREWRITEAOF, scheduled
saves and automatic rewrite remain disabled/rejected until their fork-dependent
state machines have a qualified design. Test the rejection and configuration
guards; disabling only an exposed command leaves internal triggers reachable.

Never map `fork()` to success, run child branches against a concurrently mutable
dataset, or invoke a native Valkey child. Background snapshots require a real
consistency/lifetime design and a later gate. Preserve failure reporting; missing
required initial persistence semantics block P6 rather than silently disabling
all persistence. Test native-to-managed and managed-to-native RDB/AOF loading,
comparing logical contents, types and TTL semantics rather than nondeterministic
file bytes where ordering/metadata legitimately differ.

### Scripting and allowed adaptations

Translate bundled Lua and its static module together with server scripting and
function engines. Verify protected calls/nonlocal exits, C callbacks, allocator
ownership, time limits, GC and script abort behavior. Retain upstream sandboxing
and deterministic server integration. Lua package/native module loading must not
escape through dotcc's generic dynamic-library facilities. Existing Lua tests
must be rerun against the actual embedded engine.

Prefer existing configuration/extension points and typed function overrides for
host operations. If an embedding seam cannot be expressed that way, record the
smallest reviewed, hash-checked adaptation in a staged input copy. Compare the
same staged boundary natively and keep untouched native Valkey as an independent
behavior oracle. Do not patch algorithms, strip required command handlers or
hand-edit generated C# to hide compiler defects. Record every override's intended
matches and ensure profile changes force object re-emission.

## Full translation and post-processing contract

`valkey/scripts/translate.sh` works from the repository root, `valkey/` or an
unrelated working directory. Its default invocation must perform all stages:

1. Resolve Python/prerequisites, fetch and verify pinned inputs (or explicitly
   validate local inputs with `--no-fetch`), build dotcc and `DotCC.PostProcess`,
   and record tool identities and package/sibling LALR.CC selection.
2. Stage configuration and generated upstream headers, validate the source
   manifest, then translate every configured C unit with consistent instance
   conventions and typed overrides. Link the reusable managed library; unresolved
   imports or missing handlers fail, without opportunistic native-library binding.
3. Emit split C# and `TranslatedValkey.csproj`, wire original authored sources,
   and compile the raw library in isolated staging. Preserve a separate raw
   comparison project. A raw-only diagnostic must not replace final output.
4. Run the existing [semantic postprocessor](../../docs/postprocess.md) over
   staged generated sources and build the resulting product. Preserve authored
   source hashes even when they provide semantic context to the tool; never
   rewrite linked `src/` files. Optimization must not repair invalid raw emission.
5. Promote only validated output to `generated/TranslatedValkey/`, check final
   relative references, remove obsolete campaign-owned generated files, and
   write a receipt with input/config/tool/output hashes and completed stages.
   On failure exit nonzero and retain diagnostics plus the previous valid output,
   explicitly marked stale for the attempted inputs. Do not publish partial output.

`build.sh` builds existing output; `test.sh` executes checks; `verify.sh` performs
regeneration and acceptance. No extra manual source copying, emission or
post-processing step may be required. Translation does not need a native server
or live peer. A repeated run must not accumulate duplicate/stale generated files.

Planned user commands after implementation and prerequisite installation:

```sh
./valkey/scripts/translate.sh
# Explicit reuse of already downloaded, verifiable ref inputs:
./valkey/scripts/translate.sh --no-fetch
dotnet build valkey/ManagedConsumer.slnx -c Release
dotnet run --project valkey/samples/ManagedConsumer/ManagedConsumer.csproj -c Release
dotnet publish valkey/samples/ManagedConsumer/ManagedConsumer.csproj -c Release -r linux-x64 -p:PublishAot=true
./valkey/scripts/verify.sh --no-fetch
```

## Failure-driven implementation workflow

For each observed preprocessing, parser, IR, link, C# build or runtime failure:

1. Save the command, identities, stage, source location and first root diagnostic
   under `artifacts/`; summarize in `docs/blockers.md`.
2. Reduce to valid minimal C and demonstrate a failing regression before fixing
   shared code. Use `DotCC.Tests` for focused semantics and
   `DotCC.FunctionalTests/Fixtures` for C → generated C# → compile → execute.
   Verify expected behavior with a matching-ABI native compiler.
3. Repair shared compiler/runtime structure. Fully rebuild generated parser
   tables after grammar changes. Do not classify a hypothesized gap as a finding
   until reproduced, and do not use string-only checks as execution evidence.
4. Run focused and affected suites serially, retry the full selected Valkey
   source closure, and record the new first failure or passing stage. Add Valkey
   integration coverage for semantic defects, then commit the coherent change.

Record repository baseline failures separately. Run relevant compiler/runtime
suites, existing Lua/chibi checks and affected Zig/WAT checks after shared semantic
changes. Revalidate affected SQLite/picotls/MsQuic/libsmb2/Blink consumers with the
new compiler; respect each campaign's own test scope and preserved exclusions.
Use existing upstream error cases and ordinary API/lifecycle/invalid-input checks;
custom fault-injection campaigns are not a prerequisite for this initial plan.

## Milestones and exit gates

### P0 — Freeze the profile, scripts and native baseline

- [ ] Turn the reviewed pin into source/file manifests, license inventory and
      verified fetch recipes; create the required workspace structure.
- [ ] Implement common Python resolution and explicit `--no-fetch` semantics,
      including the script checks above and receipts distinguishing both modes.
- [ ] Freeze command/configuration/source/dependency inventories, generated
      inputs, ABI and host imports. Record required/deferred capability guards.
- [ ] Build the matching native server/CLI in `build/`, run baseline selected
      upstream tests, and record tool versions and existing dotcc test results.
- [ ] Attempt the complete managed source closure and record actual first blockers.

**Gate:** reproducible verified inputs, working native control, tested acquisition
behavior and an evidence-based blocker ledger. Commit the baseline milestone.

### P1 — Prove ABI, ownership and host feasibility

- [ ] Check actual layouts, tagged/packed storage, canonical callbacks and
      cross-unit/instance initialization against native probes.
- [ ] Exercise two independent translated owners, Lua callback context, allocator
      accounting, worker owner propagation and quiescent disposal under JIT/AOT.
- [ ] Prototype BCL event wakeup, nonblocking TCP, real file/flush operations and
      stop while waiting; identify any missing shared runtime contracts.
- [ ] Resolve startup/shutdown, protected Lua calls and foreground persistence
      seams before committing to a facade or claiming an embeddable server.

**Gate:** concrete feasibility evidence and explicit blockers for every required
boundary. Prototypes do not substitute for whole-server execution. Commit results.

### P2 — Translate, link and post-process the selected server

- [ ] Repair real compiler/libc failures with regressions and full-source retries.
- [ ] Translate the complete selected closure, including dependencies and static
      Lua registration; audit unresolved imports and command-handler reachability.
- [ ] Implement the full `scripts/translate.sh` pipeline and final/raw project
      paths, receipts, failure-safe promotion and authored-source references.
- [ ] Build raw/processed libraries with JIT and whole-assembly-rooted NativeAOT
      checks so trimming cannot hide uncompiled required handlers.

**Gate:** complete libraries build in all four forms; default and `--no-fetch`
generation differ only in acquisition behavior. No runtime success is implied.
Commit the translation milestone and each independent repair along the way.

### P3 — Run the actual server and RESP lifecycle

- [ ] Start/load/listen through the owning API, signal readiness, and serve real
      TCP clients with upstream parsing/dispatch/replies in all four forms.
- [ ] Cover RESP2/3, pipelining, partial/large binary traffic, multiple clients,
      backpressure, timers, ordinary disconnects and protocol errors.
- [ ] Implement stop, SHUTDOWN, cancellation and disposal without host process
      side effects; verify restart and simultaneous independent instances.
- [ ] Qualify required background workers and callback/buffer lifetimes under GC.

**Gate:** independent clients exchange real commands and two servers complete
their full lifecycle without shared state or resource leakage. Commit evidence.

### P4 — Qualify the standalone command profile

- [ ] Run the required data-type command families, encoding transitions and
      numeric/binary boundaries against the pinned native server.
- [ ] Cover expiry/hash-field expiry, eviction/limits, transactions/WATCH, scans,
      blocking operations, streams, Pub/Sub and tracking/invalidation.
- [ ] Exercise AUTH/ACL permissions and negative authorization cases, command
      metadata and guards for unsupported options/commands.
- [ ] Run applicable pinned upstream Tcl tests against the managed launcher and
      maintain case-level results, ported assertions and justified exclusions.

**Gate:** the frozen required command inventory passes, with explicit comparison
rules for time, randomized ordering and approximate results. Commit the milestone.

### P5 — Qualify embedded scripting and functions

- [ ] Execute the pinned Lua suite plus server EVAL/EVALSHA and FUNCTION/FCALL
      tests through the translated engine, including bundled extension helpers.
- [ ] Verify script cache/load/flush, replies/errors, ACL checks, atomic effects,
      protected-call unwinding, time limits/kill where upstream permits, and GC.
- [ ] Test independent engine state and ordinary teardown/restart; retain the
      same upstream restrictions on stopping scripts that already performed writes.

**Gate:** native differentials and upstream scripting cases pass under
raw/processed JIT/NativeAOT; a standalone Lua smoke test is insufficient. Commit.

### P6 — Qualify foreground RDB and startup AOF persistence

- [ ] Implement/verify required file, flush, rename and worker contracts with
      explicit durability capabilities and per-instance path ownership.
- [ ] Run RDB save/reload and AOF fresh/existing startup, append/fsync/replay,
      multipart manifests, transactions and script/function persistence cases.
- [ ] Exchange same-pin files in both native/managed directions and verify data,
      types and TTL semantics after graceful stop and restart.
- [ ] Verify background/runtime-enable configuration guards, ordinary file errors
      and applicable upstream recovery tests; record unsupported crash guarantees.

**Gate:** persistent state survives the qualified lifecycle and agrees with
native Valkey; no fake fork or native persistence helper. Commit the milestone.

### P7 — Deliver the managed API and consumer solution

- [ ] Create `valkey/ManagedConsumer.slnx` with the final generated library,
      original authored projects and separate sample; validate its references.
- [ ] Expose typed configuration, start/readiness/status, endpoint discovery,
      logs, stop and asynchronous disposal with explicit ownership/cancellation.
- [ ] Make the sample start its own translated server, connect a separate .NET
      client, exercise data structures, transaction, script and persistence, then
      stop/restart and dispose it. Use generated constants where available.
- [ ] Build/run the sample under JIT and NativeAOT; verify edits under `src/`
      survive translation and are picked up by ordinary solution rebuilds.

**Gate:** documented commands produce a working consumer of the delivered product,
without native Valkey or a translated campaign test driver. Commit the delivery.

### P8 — Qualify upstream tests, portability and dependencies

- [ ] Expand case-level upstream coverage for the required profile, with Tcl
      server tests and applicable C/C++ assertions handled by suitable runners.
- [ ] Audit published imports, static Lua registration, trimming roots and all
      application-owned native dependencies; no fallback to native engine code.
- [ ] Complete Linux x64 raw/processed × JIT/NativeAOT execution and run Windows
      x64 separately when available. A cross-build is not a platform execution.
- [ ] Measure startup, representative command/pipeline latency/throughput, memory,
      allocations, persistence and output size against equivalent native settings.

**Gate:** required Linux profile passes with explicit platform/feature limits;
Windows remains open until actually executed. Make no unmeasured performance
parity claim. Commit the qualification ledger.

### P9 — Reproduce the final delivery and shared regressions

- [ ] Reproduce from a clean checkout with default fetching; repeat with verified
      pre-populated `ref/` and `--no-fetch`, including extracted-tree-only reuse.
- [ ] Verify identical input identities and equivalent output behavior, stale-file
      cleanup, failed-stage receipts and post-processing idempotence.
- [ ] Rebuild the actual root solution/sample from final paths, audit authored
      source links, and run affected shared and existing-campaign regressions.
- [ ] Publish usage, API, configuration, persistence and validation documentation;
      list unrun/deferred cases separately from passes.

**Gate:** a new consumer can regenerate, build and run the selected profile using
the documented entry points. Commit completion evidence and report the exact
qualified scope; unchecked gates remain incomplete.

Dependencies: P0 → P1 → P2 → P3 → P4/P5/P6 → P7 → P8 → P9. Boundary feasibility
work can inform earlier decisions, but later gates require the actual translated
server, not native-only runs or independent host prototypes.

## Later profile extensions

These are separate planned increments, not implicit claims of initial completion:

- **Background persistence:** design consistent snapshots, AOF rewrite handoff,
  writes during snapshotting, cancellation and completion without CLR `fork`.
  Retain translated serialization and validate native interoperability before
  enabling BGSAVE/BGREWRITEAOF or scheduled persistence.
- **Replication and failover:** full/partial sync, backlog, offsets, acknowledgments,
  expiration/script propagation and reconnects against native primary/replica
  peers. Full sync depends on the snapshot design; replication cannot be enabled
  merely by translating `replication.c`.
- **Cluster and Sentinel:** routing/MOVED/ASK, slots/migration, cluster bus,
  failover and a separate Sentinel executable/profile with multi-node tests.
- **TLS:** integrate an explicit managed connection backend, preferably existing
  translated picotls and its provider if its contract fits. No native OpenSSL
  backend; qualify certificates, ACL authentication and transport backpressure.
- **Further parity:** optional I/O concurrency, vector commands, additional
  platforms and explicitly registered managed modules need separate capability,
  ownership and execution gates. Native shared-library module ABI compatibility
  is not implied by generated C structs and callback pointers.

Initial completion means a translated, reusable standalone server with the stated
command, Lua, persistence and lifecycle profile, reproducible default fetching
and opt-in `--no-fetch`, a working root consumer solution, and recorded execution
evidence. It does not mean all native Valkey operational modes are supported.
