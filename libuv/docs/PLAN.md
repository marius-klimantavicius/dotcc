# Translate libuv with DotCC

Status: implementation active; P0/P1 in progress; no product qualification claimed.
Campaign: `libuv/`. Updated: 2026-09-29.

This plan adopts [translation baseline v2](../../docs/plans/translation-baseline.md),
the [project template](../../docs/plans/translation-template.md), and the
[shared framework](../../docs/campaigns.md). Common execution, provenance,
compiler-repair, and publication methodology stays in the baseline. On 2026-09-29 the user explicitly authorized implementation and coordinator
agents supervising implementation sub-agents until the whole plan is implemented.
Implementation and delegation are active. Milestone commits follow the baseline
and coordinator review; pushing is not authorized. See [coordination](coordination.md).

## Outcome and scope

Translate libuv's actual implementation, including **all platform-specific
implementations**, into `TranslatedLibuv`. Preserve upstream loop scheduling,
timers, queues, handle/request lifecycles, thread pool, and platform I/O code.
Call the same OS, libc, CRT, and platform-provider entry points as upstream,
using `NativeLibrary` and unmanaged function pointers or `[LibraryImport]`.
Select the implementation at runtime. BCL implementations of sockets, files,
processes, timers, DNS, watchers, or thread pools are not the default host model.

The default managed product contains every translated backend. Building on Linux
must not silently produce a Linux-only library. Windows and Unix variants of
common source files must coexist where their types or implementation differ.
Translation-time preprocessing may capture separate source variants, but the
consumer's build OS, MSBuild RID, or C# conditional compilation must not choose
which backend source is delivered. NativeAOT still produces a native executable
for a particular RID; this is separate from runtime backend selection in the
managed product.

The first useful consumer resolves a local endpoint, accepts TCP connections,
echoes framed payloads, writes and reads a temporary file asynchronously, queues
work, sends a cross-thread async notification, and uses a timer to bound the
run. It checks payloads, completion and close callbacks, then closes the loop
successfully. Both client and server I/O go through translated libuv. This is
an early gate, not the scope limit.

| Scope | Features/APIs/workloads |
| --- | --- |
| Required for completion | Complete product source/conditional-platform closure; public `uv_*` API coverage; runtime dispatch; native-call fidelity; loop/timer/async/prepare/check/idle; TCP/UDP/poll; pipes and handle-passing IPC; filesystem and events; TTY; DNS; processes; signals; threads/synchronization/TLS; clocks, entropy, system information and dynamic loading. Preserve upstream platform limitations and fallbacks. |
| Excluded from the product | Native libuv as an implementation dependency; a handwritten replacement event loop; BCL I/O emulation as an unrecorded fallback; upstream test/benchmark executables embedded in the library. |
| Later extensions | Additional execution targets beyond the initial qualification matrix, native ABI exports for arbitrary existing C consumers, performance optimization after correctness. Platform source translation itself is not deferred. |
| Departures from baseline | Native OS bindings replace the usual BCL host services. All backends ship in one default managed product. Qualification distinguishes full source coverage from execution coverage on platforms lacking a usable .NET runtime/toolchain. Narrow boundary compromises require recorded evidence, as specified below. |

## Decisions and feasibility work

| ID | Decision | Choice and reason | Status; resolving evidence/gate |
| --- | --- | --- | --- |
| D1 | Source and dependencies | Start with libuv `v1.52.1`, commit `1cfa32ff59c076ffb6ed735bbc8c18361558661f`. Use the same pin for native controls and translated tests. Track OS SDK/sysroot inputs and any platform dependency, including z/OS's zoslib, separately. | Decided starting pin, verified against upstream; P0 records acquired provenance and notices. No claim that this is the newest release. |
| D2 | Source/configuration closure | Union of CMake, autotools and source/header conditionals. GNU C11 plus the selected platform's necessary extensions. Default `native-runtime` profile contains all backend variants; diagnostic single-backend probes cannot publish the default product. | Decided policy; P0 freezes machine-readable inventory, headers, ordered defines/includes, and branch coverage. |
| D3 | ABI and encodings | Explicit backend ABI descriptions, not the translation machine's headers. LP64 Unix and LLP64 Windows first; inventory ILP32, endianness and other architecture branches without dropping them. Preserve native scalar, aggregate, callback and string contracts. | Open implementation mechanism; actual emitted/native probes close P1. Windows requires resolving DotCC's current LP64 assumption. |
| D4 | Host and dependencies | Direct native binding of external services. Reuse DotCC runtime facilities only after a per-symbol audit establishes their semantics and permits them. No native `uv_*` imports. | Decided; symbol inventory and native-call audit at P1/P2, execution at P3/P4. |
| D5 | State and lifecycle | Preserve upstream process-wide globals, default loop and shared worker pool. Multiple explicit loops are supported with upstream threading rules. No invented independent-library-instance guarantee. | Decided; P1 callback/TLS/atomic probes, P3/P4 lifecycle checks. |
| D6 | Translation/delivery | Object emission per source/configuration closure, isolated generated symbol/type domains per backend, then one aggregate product. Namespace `Managed.Libuv.Translated`, backend classes such as `LinuxGlibcX64` and `WindowsX64`. Owning API in `Managed.Libuv`. Use shared `link_flags` defaults: 256 KB splitting, literal pooling and inline deduplication. | Provisional assembly mechanism; P1 proves coexistence and P2 full publication. No splitting-default exception. |
| D7 | Native controls | Compile native libuv and native harnesses with matching platform, ABI, feature settings and environment. Translate applicable upstream tests and preserve their assertions. Add a small independent socket/child-process peer for interoperability. | Decided; P0 verifies native harness behavior before comparisons. |
| D8 | Execution matrix | Required initial targets: Linux glibc x64/arm64, Linux musl x64, Windows x64/arm64, macOS x64/arm64. Raw/processed × JIT/NativeAOT on each. Every other backend remains required for source translation; execution status is reported separately. | Provisional target availability; P0 records actual runners/toolchains. Unavailable required rows remain unrun/blocked, never passes. |
| D9 | Test scope | Full applicable upstream case inventory, ordinary errors, cleanup/concurrency, focused import/capability injection, and bounded wakeup/cancellation/lifetime stress. No custom broad fuzzing or performance threshold. | Decided; P4 records case-level applicability and outcomes. |

Planned metadata: `config/sources.json`, `config/platforms.json`,
`config/source-manifest.json`, `config/native-imports.json`,
`config/translation.json`, and `config/test-inventory.json`. These files now exist as initial inventories and policy; their explicit unresolved
coverage/binding statuses remain implementation deliverables. Revisions, archives, SDK
provenance, optional checksums and adaptation fingerprints belong there, not in
scripts. Hash policy defaults to `warn`, supports `off`, and reserves `strict`
for explicit reproduction. Exact adaptations require unique local guards in
every mode. Preserve usable edited source trees and record their actual state.
Carry upstream licenses/notices and review SDK-header redistribution separately.

## Source closure and platform inventory

The starting inventory comes from the pinned [CMake build](https://github.com/libuv/libuv/blob/v1.52.1/CMakeLists.txt),
[autotools source list](https://github.com/libuv/libuv/blob/v1.52.1/Makefile.am),
and [platform header selection](https://github.com/libuv/libuv/blob/v1.52.1/include/uv/unix.h).
Reconcile differences explicitly: for example, the two build descriptions select
different process-title sources for z/OS. A host-only CMake configure is not the
source manifest.

| Platform family | Backend material to inventory and translate |
| --- | --- |
| Common | Root `src/*.c`, shared headers, queues, heap and inline code, instantiated under each ABI/configuration that changes semantics. |
| Windows | All `src/win/` product units and `include/uv/win.h`; MSVC/MinGW conditionals and CRT distinctions. |
| Unix common | Shared `src/unix/` units, instantiated for every applicable platform. |
| Linux / Android | Linux backend, procfs, entropy paths; glibc/musl/bionic and architecture/feature differences. |
| Apple | Darwin, kqueue, FSEvents and process-title paths; SDK conditionals including restricted Apple targets. |
| BSDs | FreeBSD, DragonFly, NetBSD and OpenBSD variants; kqueue, interfaces, clocks and entropy. |
| Solaris / illumos | SunOS backend and version-dependent branches. |
| AIX / IBM i | AIX, shared AIX code and IBM i/PASE variants. |
| z/OS | OS390 backend, syscall helpers, process-title variants and external dependency interfaces. |
| Other Unix environments | Haiku, QNX, GNU Hurd, Cygwin and MSYS; their poll, clock, information and unsupported-feature implementations. |

The table is a starting inventory, not a substitute for exhaustively inspecting
the selected revision. P0 assigns every product file, inline body and material
platform conditional to at least one emitted variant, or to an explicit boundary
exception. Architecture, compiler, libc, OS-version and Apple SDK variants must
also be accounted for. Translate upstream `no-*` implementations where selected;
do not replace real backends with those implementations. Tests and build tools
receive separate classifications.

Do not promise executable support for every upstream OS merely because its C#
builds. `config/platforms.json` records source coverage, ABI readiness, runtime
availability, native prerequisites and execution qualification independently.
For platforms with no usable .NET port, keep translated code and binding
descriptions in the product, mark execution unavailable, and retain the open
porting requirement. This never counts as native behavior validation. 32-bit and
big-endian paths likewise need explicit coverage and feasibility findings; the
initial 64-bit execution matrix does not justify deleting them.

## Runtime selection and ABI design

1. Prepare separate, reproducible preprocessing environments for each backend
   variant, independent of the build machine. Capture its SDK/sysroot, macros,
   constants, ordered includes and layouts. Keep platform-dependent common code
   with that backend. Never define all OS macros simultaneously.
2. Emit isolated backend type/function/global domains. Link translation units
   within a compatible domain; do not coalesce same-named Windows/Unix types,
   static initialization, function pointers or platform-dependent common bodies.
   Preserve traceability to original paths and symbols. Sharing truly identical
   code is optional and follows proof of equivalent layout and state semantics.
3. Compile all domains into the same default raw/processed managed product.
   Avoid host-OS/RID-based `Compile` exclusions, conditional project references,
   mutually exclusive NuGet backend packages, or manually edited emitted C#.
4. An authored dispatcher selects the actual OS, process architecture, libc/CRT
   ABI and required capabilities at first use, then freezes that identity for
   the process. OS detection alone cannot distinguish glibc/musl/bionic or POSIX
   environments hosted on Windows. Probe ABI identity without executing a
   foreign backend; require a validated explicit identity for ambiguous cases.
   Unknown combinations fail before allocating a loop or opening resources.
5. Initialize and bind only the chosen domain. Avoid cross-platform module
   initializers and eager import tables. Foreign DLLs/frameworks must not be
   loaded merely by loading the assembly. Optional symbols stay optional and
   retain upstream feature probes, failure handling and fallback order.
6. Expose a stable managed owning API using opaque backend-tagged loop, handle
   and request storage. Dispatch at API boundaries; internal backend calls stay
   direct translated calls. Raw APIs retain backend-specific C layouts. Reject
   mixed-backend pointers/handles; never reinterpret all `uv_*` structs through
   one assumed layout. Size/type queries come from the selected implementation.

The same built IL library must run unchanged on the required hosts, with only
their runtime/native prerequisites varying. Record its identity when copying it
between runners. NativeAOT uses ordinary per-RID publishing from this same
source/product, with statically rooted callback thunks and no runtime code
generation. Do not use direct native link references that require foreign OS
libraries on every target. Validate full-library rooting and a normally trimmed
consumer separately. NativeAOT is not a universal machine-code executable.

P1 must compare actual emitted storage to native `sizeof`, alignment and offset
measurements: `uv_loop_t`, every handle/request family, `uv_buf_t`, sockaddr
variants, `stat`, `dirent`, `addrinfo`, pthread primitives, `timespec`, `epoll_event`,
`kevent`, `OVERLAPPED`, `WSABUF` and structures used by NT/Winsock calls. Include
unions, private fields, nested alignment, packing, bitfields, pointer/integer
casts, `long`, `size_t`, `ssize_t`, `off_t`, `time_t`, `wchar_t`, and byte order.
Generated constants must match the selected headers, not Linux fallback values.

Prefer a structural target-ABI facility in the compiler when a native ABI cannot
currently be represented. Typed external layouts or narrow adapters are viable
only where they preserve all accessed members and operations. A Windows import
wrapper cannot repair an already incorrectly laid-out translated `long` field.
Record compiler extensions as hypotheses until reduced probes reproduce them.

## Native bindings, ownership and compromises

The import manifest records, per backend and symbol: upstream call sites;
library/framework/CRT identity; export spelling or ordinal; signature and calling
convention; required/optional status; availability probe; error channel; ownership;
and any approved adaptation. Audit transitive calls, including runtime helpers,
so a nominal native socket path cannot use a BCL-backed descriptor registry later.

Use `[LibraryImport]` for fixed known entry points with supported explicit
marshalling, and `NativeLibrary` plus correctly typed `delegate* unmanaged` for
dynamic/optional symbols and upstream dispatch tables. Keep native libraries
loaded while any function pointer, callback or resource can refer to them.
Preserve Winsock extension discovery rather than assuming every function is a
DLL export. Variadic functions (`fcntl`, `ioctl`, `syscall`, formatting), imported
data, header macros, compiler intrinsics and ABI-sensitive aggregate calls need
verified fixed-signature bindings, translated inline logic, or a documented
small bridge. They cannot be guessed from a C prototype alone.

| Boundary | Translated responsibility | Native service and ownership/validation contract |
| --- | --- | --- |
| Loop and readiness | Scheduling, watchers, timer heap, pending queues, completions and fallback decisions. | Call the selected backend's epoll/kqueue/IOCP/event-port/poll and wakeup entry points. Preserve flags, timeout rules, EINTR and native descriptor/handle identity. Cover Linux io_uring paths when selected by the pinned source, with its own enablement/fallback behavior. |
| Streams, UDP, IPC | libuv buffering, request queues, connect/accept/shutdown, datagram batching and handle passing. | Native sockets/Winsock/named pipes and completion structures. Retain buffers and `OVERLAPPED` storage through real completion; exactly-once request completion and close callbacks. |
| Files and filesystem events | libuv requests, translated worker scheduling and OS-specific event processing. | Native filesystem/syscall/CRT, inotify/kqueue/FSEvents/Windows event calls. Preserve offset, short-I/O, path encoding and real OS errors. No `FileStream` or `FileSystemWatcher` backend. |
| DNS and entropy | libuv dispatch, IDNA handling, request lifetime and result conversion. | The actual resolver and random-provider calls; native allocation/free pairing. No `Dns` or managed entropy-provider substitution without a recorded exception. |
| Threads, synchronization and atomics | Upstream worker pool, work queues, cancellation protocol, locks and TLS use. | pthread/Windows primitives; native thread entry thunks, correct TLS/destructors and memory ordering. Compiler atomic intrinsics may lower to equivalent managed intrinsics with an explicit semantic audit. No .NET ThreadPool replacement. |
| Processes and signals | Parent-side spawn setup, process watchers, exit collection, signal bookkeeping and loop delivery. | Native process, wait, signal and descriptor operations; resolve the post-fork and async-signal boundary described below before enabling these paths. |
| Clocks, TTY, system information, loader | Upstream conversions, fallback policy, terminal handling and exported behavior. | Same native providers. Preserve loader flags, native `dlerror` lifetime and error state; the generic runtime loader wrapper is not assumed equivalent. |
| Memory, callbacks and errors | Upstream allocation hooks, ownership, error translation and callback order. | Matching native allocator families; stable unmanaged storage or pins with proven lifetime; explicit native errno/WSA/Win32 error capture; ABI-correct callback thunks. Never free native resolver/CRT storage with an unrelated allocator. |

Capture errors at the call boundary before another runtime/native transition can
overwrite them. Preserve POSIX functions that return an error directly rather
than setting errno. Model access to native errno/TLS and external data such as
`environ` explicitly; do not reuse DotCC's emulated errno implicitly. Preserve
Unix byte paths and Windows UTF-16 conversions, including upstream invalid-input
behavior. Root callback contexts through final completion; contain managed
exceptions and surface them after returning to a safe managed boundary.

BCL use is allowed for the managed API's ordinary objects, strings, bookkeeping,
runtime/platform detection and interop machinery. It must not take over libuv's
algorithms or OS policy. Consumer conveniences such as `Task` completion can wrap
real libuv callbacks, while retaining the raw callback contract and ownership.

### Boundary exceptions and early feasibility probes

The user's fidelity goal permits necessary compromises. Each exception goes in
`docs/compromises.md`: original source/symbol and native behavior, observed
obstacle, selected replacement, behavioral differences, affected targets/modes,
evidence, and possible removal. Prefer (1) exact imports, (2) translated inline
logic and equivalent compiler intrinsics, (3) a minimal native ABI/runtime bridge,
then (4) a narrowly bounded managed substitute only if needed and demonstrated.
Do not ask for permission for routine choices already covered by this policy.
An exception that would replace a backend or materially reduce the all-platform
scope needs a scope decision; it cannot be hidden as an implementation detail.

Two likely exceptions deserve immediate investigation:

- Unix signal handlers cannot be assumed safe to execute as managed reverse
  P/Invoke callbacks in asynchronous signal context. Probe a minimal native
  handler/notification bridge preserving upstream's signal mask, wakeup and
  delivery semantics; keep translated registration and loop dispatch where
  possible. Document interactions with runtime-installed signal handlers.
- After a multithreaded managed process forks, do not assume the child can safely
  resume managed code before exec. Probe a minimal native fork-to-exec path,
  including descriptor setup and error reporting, preserving the upstream call
  sequence. Use an upstream `posix_spawn` path where that backend already chooses
  it. Substituting `posix_spawn` elsewhere is a behavioral deviation to measure,
  not an automatic equivalence. `System.Diagnostics.Process` is not the default
  resolution. Public fork-reinitialization behavior needs its own finding.

These risks follow the pinned [Unix process implementation](https://github.com/libuv/libuv/blob/v1.52.1/src/unix/process.c)
and [signal implementation](https://github.com/libuv/libuv/blob/v1.52.1/src/unix/signal.c);
no experiment has yet established their solution. Native helpers must be small,
source-audited and separately packaged for their targets. They must not contain
a native libuv event loop or become a wrapper around the native library. Any
upstream body retained natively rather than translated is explicitly counted as
a translation exception, even if behavior matches perfectly.

| Risk | Smallest useful experiment | Success/failure and dependent gate |
| --- | --- | --- |
| Existing BCL routing overrides intended imports | Translate an actual loop init/run/close slice with explicit imports and audit the emitted call graph. | Native descriptors and calls throughout; no silent synthetic-header runtime binding. P1. |
| Incompatible domains/ABI | Emit Linux and Windows loop/async slices together, probe their actual storage, then add Darwin. | No type/global collisions or foreign-library loads; correct layouts and callbacks. P1. |
| Runtime dispatch and optional imports | Run the same IL on Linux/Windows/macOS; inject a missing optional export and a missing required export. | Correct backend, upstream fallback only for the optional symbol, clear failure for required binding. P1/P3. |
| Native callback/thread/TLS correctness | Actual `uv_async`, queued work and native-thread callback, retaining contexts across forced GC. | Correct calling convention, loop-thread delivery, memory ordering, stable identity and cleanup in JIT/AOT. P1/P3. |
| Signal and fork boundaries | Actual signal delivery and spawn fixtures through candidate bridges, with native controls. | Defined runtime-safe entry, matching process setup/results and no managed child continuation before exec. P1/P3. |
| Platform-only SDK/extensions | Representative Windows headers, FSEvents, AIX/IBM i, z/OS helpers and architecture intrinsics. | Validated declarations/lowering or bounded exceptions; missing SDK/runtime evidence remains explicit. P1/P2. |
| Native-call fidelity despite behavioral equality | Trace focused native and translated workloads at import and OS boundaries. | Selected APIs, flags and fallback decisions match after documented normalization. P3/P4. |

## State, embedding and shutdown

Keep each loop on its owner thread unless upstream explicitly allows an operation
from another thread. Cross-thread scheduling uses translated `uv_async_send` and
the selected native wakeup mechanism. `uv_run` reentrancy, `uv_stop`, ref/unref,
default-loop access, `uv_walk`, `uv_loop_fork`, allocator replacement and global
shutdown retain their upstream contracts. A global lock is not a concurrency
implementation, and `uv_stop` does not itself close resources or cancel work.

The owning API keeps request/handle storage alive through cancellation and close
callbacks. Shutdown stops accepting new work, requests applicable cancellation,
closes handles, runs the loop to drain completions, verifies no live requests,
then closes and frees loop storage. Preserve `UV_EBUSY` for a busy loop and the
upstream distinction between cancellation requested and work actually cancelled.
Finalizers cannot call arbitrary thread-affine libuv APIs; deterministic disposal
must have a tested owner-thread protocol. Invoke global library shutdown only
after all loops/workers are quiescent under its actual one-time contract.

## Framework integration and deliverables

- Recipe: `scripts/campaign.py`, exporting `Recipe("libuv", ...)`, default profile
  `native-runtime`. The recipe aggregates every backend; specialist diagnostic
  profiles have separate output paths and cannot satisfy P2.
- Product: `generated/TranslatedLibuv/TranslatedLibuv.csproj`; raw product:
  `generated/TranslatedLibuv.Raw/TranslatedLibuv.csproj`.
- Authored interop/dispatch: `src/Managed.Libuv.Native/`; owning API:
  `src/Managed.Libuv/`; necessary native boundary helpers: `src/native/` with
  an audited manifest. Generated projects reference authored files/projects at
  their original locations. Avoid circular project dependencies.
- Delivery: `ManagedConsumer.slnx`, `README.md`, and
  `samples/ManagedConsumer/ManagedConsumer.csproj`, using `TranslatedProject`
  to select the generated raw/processed reference. Sample runs the useful
  workflow, not just `--help`.
- Tests: ABI/native-oracle, upstream translated runner, behavior, native-call
  audit, dispatch, independent peer, and consumer harnesses under `tests/`.
- Evidence: `docs/blockers.md`, `docs/compromises.md`, `docs/upstream-tests.md`,
  `docs/validation.md`, plus framework receipts and specialist attachments under
  `artifacts/campaign/<run-id>/`. References belong under ignored `ref/`.
- Prerequisites: repository .NET 10 SDK/toolchain, Python 3.11+, Bash; matching
  native compilers, CMake/autotools and platform SDKs for oracles/probes. NativeAOT
  and any bridge require target-native prerequisites. Ordinary managed rebuilds
  use already prepared inputs and must not require every foreign SDK installed.

Use the existing recipe hooks for source preparation and backend aggregation. If
the framework cannot represent multiple incompatible ABI domains in one product,
add the smallest generic extension supported by a libuv reproducer; do not write
a second campaign runner. Reuse shared fetching, tool snapshots, process logging,
raw/processed staging and transactional publication. `translate.sh` must complete
the entire pipeline and preserve the last valid output on failure.

All orchestration works under an ordinary Windows account without administrator
privileges, Developer Mode or host symlinks. SDK access and native test tools are
separate prerequisites. Link/privilege-dependent test cases report capability
limits individually. Native-call tracing that needs extra privileges uses a
declared test runner or a binding-level alternative with its coverage recorded.

## Milestones and evidence

P0/P1 are active. Pinned source acquisition, recipe/layout, complete textual file
and test inventories, platform source-set assignment, a passing native first
workflow and native-only Linux ABI measurements exist. Conditional/emitted closure,
foreign SDKs and translated execution remain incomplete. See [validation](validation.md).

| Phase | Concrete exit gate | Status / evidence deliverable |
| --- | --- | --- |
| P0 — Scope/native baseline | Freeze all platform/source/API inventories and metadata; establish recipe/layout and available runners; build/assert native first workflow; run first real Linux, Windows and Darwin translation probes; record repository baseline. | In progress; inventories, native workflow/ABI receipts and [blocker ledger](blockers.md) exist; conditional closure/foreign probes remain open. |
| P1 — Boundary feasibility | Prove backend coexistence, ABI strategy, native binding override, lazy runtime selection, callbacks/TLS/atomics, fork/signal policy and representative uncommon-platform feasibility. | In progress; native layout and native-boundary/dispatch-foundation probes exist; full emitted ABI and every required boundary remain open. See [validation](validation.md). |
| P2 — Complete translation | Every inventoried platform body is emitted and compiled into default raw/processed products, or has a bounded recorded exception. No unresolved required symbols, success stubs, host-only source pruning or manual output repair. | Planned; per-backend source/branch/API/import coverage and canonical publication receipt. |
| P3a — Loop/network/file execution | Useful consumer passes through real translated loop, TCP, files, DNS, work, timer and async paths on the first host, then Windows and macOS. Validate call fidelity and clean disposal. | Planned; native differential and consumer receipts. |
| P3b — Remaining host features | Processes, signals, UDP, IPC, events, TTY, system information, loader and all remaining public API families execute; native bridges, if required, are audited. | Planned; feature-to-suite map and compromise evidence. |
| P4 — Behavioral qualification | Applicable upstream tests, errors, resource lifetimes, fallback and independent-peer checks pass across the full required matrix; same IL runs on each required host. | Planned; case counts, matrix results, call traces and exclusions. |
| P5 — Consumer delivery | Root solution builds; separate owning consumer runs under JIT and NativeAOT via ordinary project references; full-library rooting and normal trimming pass. Usage, ownership and native requirements documented. | Planned; consumer/build/package audit. |
| P6 — Reproduction/acceptance | Regenerate with final tools; rerun required matrix and affected shared regressions; verify all-platform inclusion and absence of native libuv/BCL emulation; reproduce from clean campaign outputs. | Planned; final validation report and current framework/specialist receipts. |

Dependencies: P0 → P1 → P2 → P3a/P3b → P4 → P5 → P6. Small actual-code P3
probes are part of P1; API design and inventory work can proceed independently
of unrelated blocked backends. Partial success never closes P2/P4/P6.

For implementation, follow the baseline's focused milestone commit policy after
required validation, subject to the then-current user instructions. The
coordinator owns milestone commits after validation; workers do not stage/commit. The blocker ledger records classification,
reproducer, native expectation, regression, fix and full-source retry; compiler
repairs belong in shared compiler/runtime code with meaningful tests. Preserve
unrelated existing failures separately.

## Validation selection

Suite names below are intended recipe declarations. Required execution targets
are `linux-x64`, `linux-arm64`, `linux-musl-x64`, `win-x64`, `win-arm64`,
`osx-x64`, and `osx-arm64`. Each runs raw/processed × JIT/NativeAOT for its
applicable runtime suites. Full source/build coverage covers all backend domains
even when runtime qualification is unavailable. Cross-compilation is not an
execution result.

| Behavior/gate | Suite and assertions | Required scope / prerequisites |
| --- | --- | --- |
| Complete source/API/import closure | `source-coverage`: reconcile build/header conditionals to emitted methods, map every public API and external call, reject native libuv and unapproved BCL service paths. | All backends, both forms; P2/P6. Count boundary exceptions separately from translated bodies. |
| ABI and callback contract | `abi`: native vs actual emitted layouts/constants, error capture, callback identity, retained buffers and allocator pairing. | Full execution matrix; matching native SDK/oracle. Other ABI rows remain explicitly unrun until measured. |
| Runtime selection | `dispatch`: identical IL payload across hosts, correct backend identity, no foreign library loads, unsupported-host diagnostics; required/optional import tests. | Required hosts in both JIT forms; corresponding AOT dispatch on each RID. |
| First useful workflow | `consumer`: TCP payloads, file bytes, resolver result invariants, work/timer/async completion and successful cleanup. | Full matrix; local networking and temporary directory, no public DNS dependency. |
| Public features and upstream behavior | `upstream`: translate pinned tests/helpers, retain assertions, timeouts and subprocess isolation; cover each applicable API family and OS case. | Full matrix; case-level inventory and declared native/TTY/link prerequisites. Native oracle failure is classified, not copied as an expectation. |
| Errors, concurrency and shutdown | `lifecycle`: partial/short I/O, EOF, cancellation states, busy loop close, callback drain, repeated loops, independent loops, async coalescing, outstanding work and retained storage across GC. | Full matrix; preserve legal scheduling nondeterminism. |
| Process/signal/native exceptions | `process-signal`: executable/argument/env/cwd/stdio setup, failed spawn, exit/signal results, registration and shutdown; compare native and bridge paths. | Applicable required targets/forms/modes; explicit unsupported upstream operations. |
| Fidelity at native boundary | `native-calls`: selected symbol, library, flags, errors, fallback path and pointer/handle usage vs native libuv; inspect emitted binding graph and focused OS traces. | Required target backends; chosen workloads for poll/wakeup, socket, file, thread, process, clock and entropy boundaries. |
| Interoperability | `interop`: native peer exchanges TCP/UDP bytes and IPC handles where supported; child helper validates stdio/env/exit; filesystem output readable natively. | Full matrix; native peer/helpers separate from the product. |
| Focused failure and bounded stress | `capabilities`: absent optional native feature/export exercises actual fallback, required binding failure stays loud; bounded concurrent wakeup/work/cancel/close checks. | Full matrix; controlled harness hooks. Production selection cannot be switched to a foreign ABI by test override. |
| Delivery and trimming | `delivery`: root solution, ordinary references, full-library-rooted AOT and normal trimmed consumer; regeneration preserves authored edits. | All required RIDs; AOT prerequisites. |
| Shared regressions | `shared-regressions`: focused DotCC compiler/functional/postprocessor checks and affected translation campaigns. | Triggered by actual ABI/import/callback/runtime/framework changes; select downstream campaigns by affected behavior. |

Default suites: `consumer`. Verification suites: `source-coverage`, `abi`,
`dispatch`, `consumer`, `upstream`, `lifecycle`, `process-signal`, `native-calls`,
`interop`, `capabilities`, `delivery`. Run required shared regressions separately
when changes trigger them, with receipts linked from final validation.

`config/test-inventory.json` records every upstream test/helper and applicable
platform/feature, whether translated, and its native/managed result.
`docs/upstream-tests.md` explains exclusions and capability skips. Missing
privileges, terminal, SDK, runtime or runner are specific unrun/blocked results;
required behavior remains incomplete. Do not import other campaigns' test
exclusions. Broad custom fuzzing and extended soak campaigns are optional later
work, outside default verification. Benchmarking is informational with equivalent
workloads; there is no performance acceptance threshold.

Compare deterministic bytes, status codes, sizes, offsets and contractual callback
ordering exactly. Compare equivalent sets for resolver/system-information results;
use monotonicity and bounded tolerances for time; assert allowed ordering and
coalescing for concurrent callbacks. Do not assert one notification per
`uv_async_send`. Native-call fidelity means the same upstream decision path and
external entry points under matched conditions, not identical whole-process
syscall traces: the managed runtime has its own activity. Normalize addresses,
PIDs, descriptor numbers and scheduling only with explicit rules, preserving
original logs. Equal end results alone do not prove call fidelity.

## Reproduction commands and acceptance

Layout, fetch, inventory and native-control commands are now runnable. Normal
translation intentionally rejects incomplete all-platform publication; consumer
execution remains unimplemented. Required acceptance commands remain:

```bash
bash Scripts/campaign.sh layout libuv --write
bash Scripts/campaign.sh layout libuv --check
bash libuv/scripts/fetch.sh --fetch missing
bash libuv/scripts/translate.sh --profile native-runtime --fetch never
dotnet build libuv/ManagedConsumer.slnx -c Release
bash libuv/scripts/test.sh --suite consumer --form all --mode all --rid linux-x64
bash libuv/scripts/verify.sh --fetch never --form all --mode all --rid linux-x64
```

Repeat verification on each required target with its RID, using the same published
managed product for the JIT cross-host dispatch check. Backend-inventory emission
must not vary with `--rid`; that option selects execution/publish targets. Record
native compiler/SDK/runtime identities, source/configuration inputs, test case
counts, native-call evidence, compromise list and receipt paths in
`docs/validation.md`. A single host's `verify` closes only that host's rows.

Check `--fetch never` with usable local inputs and `warn`/`off` provenance drift;
NuGet restore is controlled separately with `--restore`. Repeat from an unrelated
working directory and clean campaign outputs, preserving reference trees,
authored files and unrelated workspace changes. Test transactional failure and
regeneration behavior when new aggregation support is introduced.

Acceptance requires all-platform source coverage (with each bounded exception
visible), all required public behavior and matrix gates, the owning consumer,
native-call fidelity, and reproducible delivery with current evidence. Report
source completeness, native boundary exceptions, qualified targets, and unrun
platforms separately. No required blocked/skipped row becomes a pass, and no
unavailable OS becomes a claim of universal runtime support.

Current evidence: [validation](validation.md) records acquired source, inventories,
recipe/layout, native useful workflow, native ABI measurements and translation
feasibility probes. No complete implementation milestone or managed execution
target is qualified. Remaining optional work:
expanded runtime-port qualification, broad fuzzing/soak testing, benchmarks and
native ABI exports.

## Planning references

- [Pinned release commit](https://github.com/libuv/libuv/commit/1cfa32ff59c076ffb6ed735bbc8c18361558661f)
  and [upstream platform policy](https://github.com/libuv/libuv/blob/v1.52.1/SUPPORTED_PLATFORMS.md).
- [DotCC CLI and data model](../../docs/cli.md),
  [function overrides](../../docs/function-overrides.md), and
  [external type contracts](../../docs/external-types.md). The documented LP64
  model, system-header runtime routing and eager per-table import binding are
  feasibility inputs, not proof that this native-host campaign already works.
- [P/Invoke source generation](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke-source-generation)
  and [NativeAOT deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
  for binding and delivery constraints; confirm selected SDK behavior with probes.
