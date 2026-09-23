# Translate Blink to C# with dotcc

Created 2026-09-14. Campaign directory: `<repo>/blink/`.
The plan was prepared on branch `sqlite`; it does not require a branch change.

## Objective and inherited conventions

User test-scope update: exclude custom fault-injection and invalid/malformed-ELF
tests from campaign work and completion gates. Such cases may run only when
already present in the pinned upstream test suites; record their upstream
revision and test path. Do not create or extend custom malformed-input corpora,
forced allocation/resource/host failures, or injected network delays/disconnects.
Valid ELF loading, ordinary functional/lifecycle tests and required runtime
error handling remain in scope. Preserve historical results; excluded or unrun
checks are not passes and do not block completion. This scope applies to every
milestone below.

Translate Blink's actual x86-64 interpreter and reusable Linux userspace
emulation code into a reusable unsafe C# library. Build a small managed service
runner around it: start an instance from an executable and files, communicate
with its service, capture output, stop it, and release its resources. This is the
first implementation of the user's "fake EC2" idea.

The product exposes the translated upstream functions, types and state needed
by an authored C# execution API. That API consumes the translated library and
owns initialization, ELF loading, instruction execution, stop/deadline handling
and cleanup. Keep upstream changes minimal; do not add a C execution framework
or translate a campaign test harness into the delivered product.

Follow the established [SQLite](../../sqlite/docs/PLAN.md),
[picotls](../../picotls/docs/PLAN.md), and
[MsQuic](../../msquic/docs/PLAN.md) campaign conventions:

- Pin inputs and preserve downloaded sources; keep a source/configuration ledger.
- Translate upstream algorithms. Fix compiler/libc defects generically, with
  reduced native-checked regressions and a retry of the actual upstream input.
- Put platform services behind an explicit host contract. Native Blink, QEMU,
  KVM, WSL, or a container runtime must not execute guest instructions for the
  product; native tools are independent test oracles only.
- Target .NET 10/C# 14, BCL host services, and raw/optimized generated code under
  JIT and NativeAOT. No native JIT or runtime C# compilation is required.
- Deliver an owning managed API and a separate consumer; parsing or compilation
  alone does not complete a runtime milestone.
- Preserve case-level failures and unrun platform rows. Do not copy historical
  completion claims, commit instructions, or delegation authorizations from the
  older campaigns into this new one.

The required guest is a Linux x86-64 service process, not an EC2 machine image.
Booting a Linux kernel, implementing AWS APIs, and accepting arbitrary Docker/AMI
images are separate work. Linux x64 and Windows x64 are the intended host targets;
Windows runs the same Linux guest ELF through the translated interpreter, without
Cygwin or WSL. Other host architectures are follow-ups.

## Reviewed upstream baseline

Inspected upstream `master` at commit
**`f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580`** (2025-12-10).
Use this immutable revision as the proposed campaign baseline. Before execution,
record the archive SHA-256, toolchains, source manifest, and license inventory;
no archive checksum or successful build is claimed by this planning review.
[Pinned source](https://github.com/jart/blink/tree/f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580).

Concrete findings that constrain the implementation:

| Source | Finding and consequence |
| --- | --- |
| `configure`, `config.h.in` | JIT, fork, guest threads, sockets, VFS, and CPU extensions have separate configuration paths. Select an explicit profile; do not use `--disable-all` and accidentally remove networking. |
| `blink/machine.c` | `ExecuteInstruction` and `JitlessDispatch` provide the interpreter path. `Actor` is an unbounded loop; `Blink` uses `sigsetjmp`. Bounded execution and halt handling need a deliberate embedding contract. |
| `blink/blink.c` | CLI startup installs signal handlers, uses process globals, and calls process exit functions. Reusing `main` is not an in-process library API. |
| `blink/syscall.c`, `signal.c` | Linux ABI handling is intertwined with host POSIX calls, threads, signals, and exit behavior. Inventory the reachable call graph before declaring the host layer complete. |
| `blink/memory.c`, `map.c` | Disabling linear memory selects explicit guest translation, but the implementation still uses host allocation/mapping facilities. `-m` alone does not remove the platform porting work. |
| `blink/linux.h`, `elf.h`, `endian.h` | Guest ABI structures and byte encodings must remain Linux x86-64 regardless of the managed host OS. |
| `blink/blink.mk` | The native archive collects many source files; linker extraction hides unused code. Build an explicit managed source closure, separating CLI/TUI, device, JIT, and test-only code. |
| `test/asm`, `test/func`, `test/blink` | Existing instruction, functional, and implementation tests provide starting corpora. Assembly is guest test input assembled by a native toolchain, not C for dotcc to translate. |

Source references: [execution](https://github.com/jart/blink/blob/f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580/blink/machine.c),
[syscalls](https://github.com/jart/blink/blob/f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580/blink/syscall.c),
[memory](https://github.com/jart/blink/blob/f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580/blink/memory.c),
[build](https://github.com/jart/blink/blob/f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580/blink/blink.mk),
[functional tests](https://github.com/jart/blink/blob/f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580/test/func/README.md).

These are source-review findings, not observed dotcc compilation failures.

## Required first product profile

| Area | Required behavior |
| --- | --- |
| Guest executable | Pinned little-endian ELF64 Linux x86-64 executables; start with a static musl service and fixed-address ELF, then qualify the selected service's actual ELF form. Static PIE is a separate explicit loader case. |
| CPU | Upstream interpreter for the selected baseline integer and SSE/SSE2 workload. Inventory every advertised CPUID feature and require matching execution tests. Disable unqualified optional extensions, including x87 initially; do not claim general x86-64 software compatibility. |
| Memory | Explicit guest virtual-address translation, Linux page semantics, permissions, zero-filled anonymous mappings, `brk`, `mmap`, `munmap`, and required `mprotect` behavior. No direct guest-address-to-host-pointer fast path. |
| Process model | One guest process, initially one guest thread, per worker process. Explicit argv, environment, working directory, virtual identity, exit status, and fault result. |
| Files | Private per-instance filesystem namespace and descriptor table; immutable image files plus a bounded writable layer. No implicit host-root fallback. |
| Networking | TCP/IPv4 service path: socket/bind/listen/accept/connect/read/write/shutdown and required readiness operations. Private endpoint names/ports plus explicitly published host endpoints. |
| Host services | BCL allocation, files or memory-backed files, sockets, synchronization, clocks, and randomness behind the host table; deterministic substitutes for tests. |
| Lifecycle | Start, readiness, status, captured stdout/stderr, bounded stop, forced worker termination, and cleanup. Starting one instance must not alter another's files or endpoints. |
| Limits | Guest memory, descriptors, output volume, execution budget, and wall-clock deadline. Blocked I/O must respond to cancellation or worker termination. |
| Hosts and builds | Linux x64 and Windows x64; raw/optimized × JIT/NativeAOT. Report each actual execution target independently. |

For the initial P0–P4 profile, defer dynamic linking/general distro userspace,
guest `fork`/`exec`/thread creation,
full signal ABI coverage, UDP/IPv6, Unix sockets, advanced Linux facilities,
the `blinkenlights` TUI, real-mode devices/BIOS, kernel boot, native JIT, live
snapshots/migration, full cloud APIs, and Wasm. A service that requires a deferred
feature needs a documented profile extension rather than a success stub. P5 must
extend and qualify the profile for the selected .NET NativeAOT service's actual
requirements, including runtime threads, synchronization or loader support if
required; these dependencies cannot be deferred while claiming the P5 gate.

Select and pin a small service fixture in P0; a single-threaded static musl HTTP
server is the default planning assumption. Record its source, compiler flags,
executable hash, requests, expected responses, and complete observed syscall
inventory. The required P5 workload is an ASP.NET Core service using Kestrel,
compiled to a Linux x86-64 NativeAOT ELF, using musl if needed. The C and
raw-socket .NET fixtures remain baselines; their success does not establish
Kestrel compatibility.

## Translation and embedding boundaries

### Preserve instruction and guest ABI semantics

Retain upstream decoder, instruction handlers, flags, register state, guest
address translation, ELF parsing, and guest argument/result marshalling wherever
they are platform independent. The host implements the operations these require;
it must not become a handwritten replacement CPU or a second Linux interpreter.

Use dotcc's supplied headers/libc where semantics match. Define a managed host
header/profile for the remaining services rather than impersonating GCC, Linux,
or Windows to select unavailable native mechanisms. Guest Linux constants remain
upstream constants; they are not host `errno`, socket, signal, or structure values.

Compare actual emitted sizes, alignment, unions, offsets, register storage,
callback tables, and guest byte encodings with native probes. A Linux LP64 native
oracle can check the managed C model; a Windows LLP64 build is not automatically
the same ABI. Keep guest ELF/ABI records distinct from both host models.

### Bound execution, faults, and process-global state

Export the required upstream loader, machine, execution and cleanup surface
through the generated managed library. Implement initialize/load/run-budget/
request-stop/destroy orchestration in authored C# consuming those exports.
Keep instruction decoding, CPU algorithms and guest ABI semantics translated
from upstream C; the C# layer controls their lifecycle and execution loop.
Supply required frontend callbacks from C# where the translated ABI permits.
Any unavoidable ABI or nonlocal-unwind shim must be minimal, documented and
limited to that boundary; it must not take ownership of execution in C.
Audit instruction completion, attention flags, fault unwinding, pending signals,
page locks, and cleanup before replacing the CLI loop. One x86 instruction can
perform substantial work (`REP`, blocking syscalls); instruction counting alone
does not establish a hard time bound.

Model guest faults and termination as instance results. A guest `exit`,
`exit_group`, `kill`, or fault must never reach `Environment.Exit`, terminate the
controller, or install process-wide host signal handlers. Preserve required
guest signal-mask/handler behavior without depending on asynchronous CLR fault
recovery. Unsupported signal operations return documented guest errors.

Treat `sigsetjmp`/`siglongjmp` and guest signal delivery as separate contracts:
ordinary synchronous nonlocal control flow may reuse or extend generic dotcc
support, but host signal-mask capture cannot be faked by renaming the functions.
Prove the reachable halt/unwind paths and retain cleanup on every exit.

Inventory mutable globals, thread-local `g_machine`, flags, maps, file tables,
logging, and initialization. First release permits one active emulation context
per worker process, with repeated create/run/destroy qualified. Multiple workers
provide multiple instances. Multiple concurrent emulators inside one CLR process
remain deferred until all shared state is made instance-local and tested.

### Files, sockets, and isolation

Define a typed host table with explicit instance context, stable callback
addresses, checked buffer lengths, Linux error conversion, and ownership rules.
No movable C# reference can be stored directly in translated C memory. Contain
managed exceptions at callback boundaries and preserve pending-operation buffers
until completion or cancellation has drained.

Initially use a memory-backed directory/file model and BCL-managed socket handles.
Preserve required open flags, seek/EOF/short I/O, descriptor duplication/close,
read-only files, path lookup, and readiness semantics. Unknown syscalls return
`ENOSYS`; prohibited supported operations return the documented policy error.
Do not globally replace libc behavior needed by other translated libraries.

Virtual network bindings must permit two guests to use the same guest port.
Map explicitly published endpoints to distinct host loopback ports. Keep guest
addresses virtual when returning socket metadata. Test partial I/O, backpressure,
normal connection closure, cancellation, and closure while awaiting I/O.
Fault-injection cases are limited to existing pinned upstream tests.
Real external access is opt-in through the instance host policy.

Emulation and unsafe C# translation are not a security boundary by themselves.
The initial claim is isolated behavior for controlled test services. Use one
worker process per instance for lifecycle containment, but do not call an ordinary
same-user worker a hardened sandbox. Hostile-code isolation requires separately
qualified OS restrictions on Linux and Windows and is a follow-up security gate.

### Allowed adaptations

Keep `ref/` immutable. Prefer configuration, existing extension points, managed
host headers, generic macro overrides, and authored C adapters. Where process
exit/signal/global-state coupling makes a narrow source adaptation necessary,
record it as a reviewed, hash-checked patch applied only to staged inputs. Compile
that same staged input natively for a host-contract oracle, and retain untouched
native Blink for an independent behavior comparison. Do not patch instruction
algorithms or generated C# to conceal compiler defects. Upstream bugs need their
own minimized evidence and explicitly tracked correction.

## Workspace and generation

```text
blink/
  ManagedConsumer.slnx   final translation and runnable usage sample solution
  ManagedConsumer/      separate sample application using the owning managed API
  docs/         PLAN, source, configuration, host-contract, blockers, validation, usage
  config/       immutable source manifest, feature profile, host headers/overrides
  scripts/      fetch, native-oracle, probe, translate, build, test, dependency-audit
  src/          minimal host/ABI adapters, BCL host, C# execution API, worker/controller
    Host/       consolidated authored C# bridges and C ABI adapters
      include/  adapter headers
      scripts/  reviewed staging and inventory helpers
      docs/     adapter-specific documentation
    Managed.Emulation.Host/       managed host implementation project
  tests/        ABI, CPU, ELF, memory, host services, lifecycle, service fixtures
  ref/          unchanged upstream/test/toolchain inputs (ignored)
  generated/    staged C and raw/optimized C# with manifests (ignored)
  build/        native oracle, managed builds, NativeAOT publishes (ignored)
  artifacts/    diagnostics, traces, comparisons, performance/validation receipts (ignored)
```

Required delivery paths are `blink/scripts/translate.sh` and
`blink/generated/TranslatedBlink/`. The script performs translation and the
existing semantic post-processing end to end, placing the final post-processed
C# sources and `TranslatedBlink.csproj` in that output directory. Preserve the
immutable raw translation separately so post-processing cannot overwrite it.
Record source/configuration/compiler and output identities for reproducible runs.

Authored C# stays in `blink/src/`; do not copy Host, Bridges or other authored
sources into `blink/generated/TranslatedBlink/`. The generated project compiles
the required original bridge files through parent-relative `Compile Include`
paths, with `Link` metadata for IDE grouping, and references the original host
project through a parent-relative `ProjectReference`. For example, from the
final generated project:

```xml
<Compile Include="../../src/Host/HostIoBridge.cs" Link="Bridges/HostIoBridge.cs" />
<ProjectReference Include="../../src/Managed.Emulation.Host/Managed.Emulation.Host.csproj" />
```

Apply this rule to every authored source compiled into TranslatedBlink and to
all authored project dependencies. `ManagedConsumer.slnx` includes the original
projects under `src/`, so editing any non-generated file through the solution
edits its canonical source directly. Rebuilding picks up those edits without
translation or manual copying; regeneration must preserve them. Only generated
sources are post-processed; linked authored sources must remain untouched.
Private input snapshots for reproducibility may remain in test/profile/artifact
storage, but they must not become active source inputs of the delivered solution.
Record the actual referenced source identities in validation receipts.

Keep all host bridge/C adapter sources together in `blink/src/Host/` and all
their headers in `blink/src/Host/include/`; do not recreate per-service HostXXX
directories. The separate managed host implementation project remains
`blink/src/Managed.Emulation.Host/`. All live manifests, staging helpers and
test recipes must reference this layout; historical frozen inputs stay intact.

The delivered translation must exclude `CoreProbe`, the authored test `main`,
fixed test workloads and campaign C execution drivers. Link these only into
separate test consumers. Expose the needed upstream surface through generator
configuration or generic export support, preserving upstream names where
possible; never hand-edit generated C#. The authored C# API references the
generated project and is separate from its translated sources.

`blink/ManagedConsumer.slnx` is the showcase solution for the final translation.
Include the generated project, required host/API/worker projects, and a separate
runnable usage sample under `blink/ManagedConsumer/`. The sample consumes the
final generated project through project references and the owning API, showing
service startup, readiness, a real HTTP request, captured output and cleanup.
All sample execution goes through the authored C# API, not a translated test
entry point or a campaign-owned C run loop.
Document translation, solution build and sample run commands from a clean checkout.

Shared compiler/runtime fixes stay in their existing projects. Scripts resolve
paths from their own location, isolate temporary/build directories, and support
offline reruns after checksum-verified fetches. Record any upstream test toolchain
download separately; native `make check` must not silently fetch floating tools.

### Semantic intrinsics and Blink function overrides

The 2026-09-23 continuation includes extending dotcc's semantic function override
support beyond `load.i32.le` and applying qualified BCL-backed replacements to
Blink. Use the existing typed `functionOverrides` mechanism, preserving original
function signatures, identity, direct/exported/function-pointer calls and
single evaluation of arguments. Keep generic intrinsic support in dotcc and
Blink-specific selection/provenance in the campaign profile; never rewrite
generated C# manually. Replacements are explicit reviewed optimizations, not
permission to replace the interpreter or guest kernel semantics.

| Candidate in pinned Blink | Proposed implementation | Qualification needed |
| --- | --- | --- |
| `endian.h`: `Get16`, `Get32`, `Get64` | New unsigned little-endian load intrinsics using `BinaryPrimitives.ReadUInt16/32/64LittleEndian` | Exact width, unaligned ordinary byte access, host-endian independence and reads after writes |
| `endian.h`: `Put16`, `Put32`, `Put64` | New unsigned little-endian store intrinsics using `BinaryPrimitives.WriteUInt16/32/64LittleEndian` | Exact byte layout, truncation at the C-call boundary, unaligned writes and adjacent bytes unchanged |
| `bitscan.c`/`bitscan.h`: `bsf`, `bsr`, `popcount` | Assess `BitOperations.TrailingZeroCount`, `LeadingZeroCount`/`Log2`, and `PopCount` through intrinsics or typed managed helpers | Preserve the selected C implementation's zero-input behavior and widths; inspect preprocessing/builtin lowering first to avoid redundant overrides |
| Byte-swap helpers | Assess `BinaryPrimitives.ReverseEndianness` | Existing GNU builtin lowering already uses this BCL operation; add overrides only for actual uncovered calls |

- [x] Implement and document the six unsigned endian load/store targets above,
      with signature validation and focused native differential tests. Preserve
      the existing signed `load.i32.le` contract; additional signed/big-endian
      targets may be added where a concrete caller justifies them.
- [x] Select the actual pinned Blink endian functions through reproducible
      profiles. Respect per-translation-unit matching and physical declaration
      paths; assert intended coverage with override reports. A global
      `requireMatch` on unrelated producers must not make valid units fail.
- [x] Record which secondary bit-operation candidates are useful, already
      optimized, or deferred. Add any selected replacement only after checking
      its actual semantics, including zero input, integer width and side effects.
- [x] Qualify direct and pointer calls, argument side effects, signed boundaries
      where applicable, and ordinary unaligned buffers under raw/optimized
      JIT/NativeAOT. No custom fault injection or invalid-ELF cases are added.
      Memory intrinsics must not bypass guest address translation, protection,
      atomic/volatile behavior or syscall contracts.
- [ ] Reemit affected C objects: object-only relinking cannot apply function
      overrides. Include compiler/profile/target identities and selection reports
      in receipts, retain literal pooling and inline deduplication, then rerun
      affected CPU and Kestrel/worker/sample gates against the final delivery.
      Describe emitted BCL calls and observed correctness; do not claim measured
      speedups without measurements. This work is part of the authorized P5
      continuation and does not start the separate P6 performance campaign.

### Managed boundaries through function overrides

Prefer semantic `functionOverrides` targeting authored `managedMethod` entries
over source patches that replace entire upstream functions. This applies to
`UpstreamGuestThreads`, starting with `SignalActor`'s handoff to the owning C#
instruction loop, and other thread/lifecycle boundaries with compatible
whole-function contracts. The original upstream declarations and callers remain;
the profile selects the replacement, and the authored implementation stays in
`src` with direct project/source references.

- [x] Inventory `UpstreamGuestThreads` patch hunks and convert suitable complete
      function substitutions to typed managed overrides. Remove superseded
      patch hunks, callback plumbing and staging assumptions from the active
      pipeline while retaining historical inputs/receipts.
- [x] Preserve function identity, pointer signatures, per-thread ownership,
      nested instruction accounting, unwind behavior and cleanup. Resolve
      generated record types and managed bridge signatures explicitly; extend
      generic override support only for a demonstrated missing contract.
- [x] Keep upstream signal selection, frame construction and return algorithms.
      For internal statement changes, sender metadata or actual upstream bug
      repairs that cannot be expressed as a whole-function override without
      duplicating those algorithms, document the reason for each remaining
      narrow patch. Do not assume an override can call an original body that
      it has replaced; any delegation mechanism must be explicit and qualified.
- [ ] Record selected overrides and remaining patches in the profile/audit
      documentation, regenerate affected objects, and qualify ordinary
      signal/thread behavior plus Kestrel lifecycle and final consumer against
      the resulting product. Prior patch-based passes are baseline evidence,
      not automatic qualification of the override-based implementation.

Required output: `blink/generated/TranslatedBlink/TranslatedBlink.csproj`, class
`BlinkCore`, namespace `Managed.Emulation`, using `--emit=managedlib --nest-types
--runtime=c --literal-pool --deduplicate-inline --split=size --split-size=102400`. Verify actual CLI options when
implementation starts. Run the existing semantic postprocessor after normal
emission, retain an immutable raw snapshot, and never hand-edit generated output.
Enable `--deduplicate-inline` at link time in canonical assembly, delivery and
core probe/replay links, including the derived CoreExecution test link. Include
the option in recorded link identities; preserve per-object identity checks.
Deduplicate only implementations the compiler proves equivalent, retaining
function-address and state distinctions. This is separate from semantic
postprocessing. Rebuild and qualify the next deduplicated delivery before
claiming runtime results; existing receipts describe their original options.
The full container is `BlinkCore` because actual upstream code defines a function
called `Blink`, which cannot be a same-named C# class member. Focused fixtures
may still use `Blink`; authored bridges select the full container explicitly.
The owning facade is provisionally `BlinkInstance`; its naming is not upstream ABI.

## Failure-driven workflow

Commit at every completed milestone and whenever other significant progress is
made.

1. Run the pinned selected closure through preprocessing, translation, linking,
   C# compilation, and execution as far as possible; preserve the first failure.
2. Classify compiler, libc, host adapter, upstream defect, or unsupported profile.
   Reduce compiler/runtime defects to valid minimal C with a native expectation.
3. Add a meaningful failing regression, fix the shared structural cause, run
   focused checks, and retry the full selected Blink closure.
4. Preserve separate native upstream, native staged-host, raw C#, and optimized
   C# results. Undefined flags and nondeterministic values require explicit
   comparison masks/invariants, not broad normalization.
5. Run affected repository suites serially after shared changes. Record exact
   compiler/source/config/generated hashes so stale assemblies cannot pass a gate.

## Milestones and exit gates

### P0 — Freeze the useful guest and reproducible inputs

- [x] Pin Blink, licenses, guest service/toolchain, and test corpora with hashes.
- [x] Inventory source closure, host calls, globals, CPU features, and guest syscalls.
- [x] Run native Blink with JIT and linear memory disabled against the pinned
      static service and selected upstream guest tests on Linux x64.
- [x] Record current dotcc regression baseline and initial translation diagnostics.

**Gate:** a real native service baseline, reproducible inputs, and a concrete
source/host/profile manifest. Do not mark a source inspection as a passed probe.

### P1 — Prove the interpreter and embedding seams

- [x] Translate a decoder/instruction subset with a real guest memory harness.
- [x] Compare actual emitted ABI/register storage to matching native probes.
- [x] Demonstrate budgeted stepping and nonlocal unwind without host exit.
- [x] Map every remaining platform dependency and planned adaptation explicitly.

**Gate:** actual guest instructions execute and halt safely through the proposed
embedding boundary under JIT and NativeAOT; unresolved dependencies are recorded.

### P2 — Translate the complete selected core

- [x] Repair actual compiler/header/runtime blockers using reduced regressions.
- [x] Emit all required decoder, CPU, memory, loader, and syscall-marshalling code.
- [x] Build raw/optimized libraries and whole-library-rooted AOT consumers.
- [x] Audit imports and initializers; placeholders cannot enter runtime gates.
- [x] Deliver `blink/scripts/translate.sh` to run translation and semantic
      post-processing, producing the final sources and project in
      `blink/generated/TranslatedBlink/` with a separate immutable raw snapshot.
- [x] Align the delivered library with the C# consumer architecture: export the
      needed upstream functions/types/state and exclude campaign test frontends
      and C execution drivers. Requalify the sample through the authored C# API.
- [x] Reference authored bridge/source files and `Managed.Emulation.Host`
      directly from `src/` through parent-relative includes/project references;
      remove copied authored Host/Bridges from final generated output. Ensure
      regeneration and post-processing never overwrite authored sources.

The refined delivery now builds 108 product producers without the authored C
probe, uses the shared literal pool, and links the consolidated original `src/`
adapters and Host project. Raw, postprocessed and final direct-source builds
passed with unchanged authored source hashes. The separate C# execution API and
sample pass actual service health and normal stop in JIT and Linux NativeAOT. Existing core
translation and P3 evidence retain their recorded source scope. The full service
API required by P5 remains open.

**Gate:** complete selected source closure builds with matching actual layouts,
no native emulator dependency, and passing affected compiler regressions;
`blink/scripts/translate.sh` reproduces the final post-processed output directory
with the required exported upstream surface and no campaign test frontend.

### P3 — Qualify CPU, guest memory, and ELF loading

- [x] Run selected upstream instruction cases and generated edge cases against
      native Blink and real x86-64 Linux execution where guest behavior is defined.
- [x] Cover flags, shifts, signed division, SIMD lanes, floating-point edge cases,
      instruction/page boundaries and advertised CPUID bits; fault-triggering
      cases are limited to existing pinned upstream tests. The finite selected
      profile passes 504 normal cases per form (2,016 comparisons); this is
      bounded family coverage, not exhaustive ISA certification.
- [x] Cover valid ELF headers/segments, BSS zeroing,
      executable permissions, stack/argv/env/auxv, and required TLS setup.
      Valid pinned-image loading and a fixed explicit TLS startup fixture pass
      all four forms; this does not claim a general libc/dynamic TLS ABI.
- [x] Exercise memory growth, map/unmap/protect, valid cross-page reads/writes,
      and cleanup. Actual guest page-table algorithms pass native and all four
      managed forms through two normal lifecycles; protection metadata and
      permitted accesses are checked, with no injected forbidden access. Guest
      code pages remain data interpreted by Blink.

**Gate:** the selected guest CPU/ELF/memory profile passes exact state/output or
explicit architectural-invariant comparisons in all four generated/runtime forms.

### P4 — Complete the per-instance host services

- [x] Implement the required virtual filesystem, descriptors, clocks/randomness,
      guest status/signal handling, TCP sockets, and readiness contracts.
      Selected blocking-profile contracts pass through actual guest SYSCALL
      fixtures in native and all four forms, including inherited standard streams
      and ordinary terminal ENOTTY. Async signal delivery and optional socket
      features are outside this selected profile; owning stop remains below.
- [x] Test ordinary filesystem access, descriptor duplication/close, short I/O,
      and cleanup. Fault-injection cases, including invalid guest buffers and
      forced resource/host failures, are limited to existing pinned upstream tests.
      GuestIo passes real guest SYSCALL marshalling and both descriptor-table
      lifecycles in native and all four forms, including 128 KiB transfers and
      valid cross-page vectors. TCP waiting/stop contracts remain separate.
- [x] Bind stop/deadline behavior to execution and outstanding I/O; prove no
      guest operation exits the controller or reaches an unintended host service.
      Implement the owning execution loop and lifecycle in the C# API consumer
      over exported upstream functions, with only necessary boundary adaptations.
      Callback token propagation and 22 normal lifecycle scenarios pass all four
      forms. The actual C# owner passes 11 normal completion/request/deadline/
      budget cases in each form, including zero-descriptor poll, nanosleep and
      a pending read on an inherited pipe; eight native completion witnesses
      pass. Cleanup, first stop reason and controller return are verified.
- [x] Audit required service startup syscalls; qualify additions individually.
      All 18 names observed in the pinned native service trace have bounded
      individual guest-dispatch evidence (GuestIo/Environment/Tcp/Streams,
      retained TLS and core exit). This is not a managed service-startup trace.

**Gate:** contract tests and the actual guest service startup pass; unsupported
operations fail explicitly without false success or a native fallback.

P4 completed on 2026-09-20 for the finite selected Linux x64 profile. Actual
service startup and six exact native HTTP cases pass all four managed forms;
the owning execution/poll/sleep/inherited-I/O stop matrix also passes all four.
Direct inventories and source review retain their stated indirect/framework
limits, without a hostile-code sandbox claim. The phase stops here as requested;
P5/P6 remain held. See [the exact P4 ledger](P4-HOST-SERVICES.md).

### P5 — Execute a .NET NativeAOT service and deliver the instance runner

The required guest is a real ASP.NET Core/Kestrel service published as native
machine code and executed by translated Blink. HTTP serving must use Kestrel,
not an authored raw-socket HTTP implementation. This is distinct from publishing the C# emulator
host with NativeAOT. Docker or Podman may build a musl-targeted guest and provide
an independent native reference run; the delivered emulator must execute the
guest instructions itself. Phase execution was explicitly authorized on
2026-09-20. Begin with the ordinary native guest build; use musl/container
tooling only if needed. If musl NativeAOT compilation proves very difficult or
impossible, stop and report for user direction rather than pursuing prolonged
workarounds.

- [x] Add a reproducible ASP.NET Core/Kestrel NativeAOT HTTP service fixture with source, pinned
      SDK/toolchain and container image identity when used, publish settings,
      executable hash and exact build/run commands. Provide its ELF at a
      documented path for ManagedConsumer; retain existing C/raw-socket fixtures.
      Raw-socket native glibc and static-musl builds and exact HTTP references pass; see
      `tests/DotNetService/README.md` and `tests/DotNetService/MUSL.md`. Actual
      Kestrel publication/native reference passes at `kestrel-guest-musl/attempt-o5jvvf7t`;
      see `tests/KestrelService/README.md` for the pinned ELF path and receipt.
      Its translated execution remains a separate unqualified gate.
- [ ] Inspect the actual Kestrel guest ELF/dependencies and native service behavior;
      inventory required startup/runtime instructions, syscalls, TLS, threads,
      synchronization, signals and filesystem inputs. Record required profile
      extensions explicitly and implement them through translated upstream
      algorithms and the managed host boundary, without success stubs.
      The raw-socket baseline's finite observed surface, four-mode results and explicit
      unsupported/tolerated operations are recorded in
      `docs/P5-NATIVEAOT-RUNTIME.md`; broader runtime compatibility is not claimed.
- [ ] Execute the Kestrel NativeAOT guest through translated Blink in raw/optimized
      JIT/NativeAOT host forms on Linux x64. Verify readiness, real HTTP requests,
      normal shutdown, ordinary cancellation and resource cleanup against the
      native reference. A guest build or a container/native-only run is not a
      translated-execution pass. Preserve custom fault/invalid-ELF exclusions.
- [ ] Deliver `blink/ManagedConsumer.slnx` and its runnable usage sample, consuming
      `blink/generated/TranslatedBlink/TranslatedBlink.csproj` and the owning API.
      Reference original authored projects/files under `src/`, including Host;
      solution edits must persist there and ordinary builds must use those edits.
      Document selection of the NativeAOT guest ELF and its inputs, and adapt
      readiness/request/shutdown behavior to that fixture's explicit contract.
- [x] Provide an owning API for image/argv/env/limits, start/readiness, logs,
      endpoint publication, status/exit reason, stop, and asynchronous disposal.
      This is authored C# consuming the translated upstream exports, not an
      authored C execution wrapper translated into the product.
- [x] Implement one managed worker per instance and a bounded control protocol.
      Distinguish guest exit, guest fault, budget exhaustion, and worker failure.
- [ ] Run two simultaneous Kestrel instances using the same guest port, distinct files,
      and distinct published endpoints. Verify restart and resource cleanup.
- [ ] Send real HTTP requests to Kestrel from a separate BCL client and verify status,
      headers/body as specified by the fixture, including large/fragmented traffic.
      Define deterministic comparison rules for any variable protocol fields;
      do not substitute raw-socket response bytes for Kestrel evidence.

**Gate:** a separate application starts, talks to, stops, and restarts actual
ASP.NET Core/Kestrel NativeAOT service instances through translated Blink without cross-instance
interference; the showcase solution builds and its sample demonstrates the
documented usage. Neither the earlier C nor raw-socket .NET service satisfies
the Kestrel guest gate. Their completed checks remain valid baseline evidence.

Observed raw-socket baseline worker gate: `worker-instances/attempt-kd2trw_m` passes 16 actual
workers and 40 exact native HTTP comparisons across all four modes. Simultaneous
instances load distinct private executable paths, publish distinct host ports,
and complete normal exit, cooperative stop, fresh-process restart and idle
Deadline cleanup. The auxiliary marker files are retained but not read by the
guest. The raw-socket showcase solution/sample subsequently passed at
`managed-consumer-delivery/attempt-8k2fus34` (commit `f06256d`), resolving that
historical sample qualification block. It remains baseline evidence until the
Kestrel guest is qualified in the final sample. P5 is not complete.

### P6 — Qualify upstream tests, platforms, and delivery

- [ ] Run applicable pinned upstream tests and preserve case-level results.
      Include fault-injection or invalid-ELF cases only where they already exist
      upstream; record provenance and exclusions without adding custom cases.
- [ ] Execute raw/optimized × JIT/NativeAOT on Linux x64 and Windows x64.
      A Windows cross-build does not count as a Windows execution result.
- [ ] Measure startup/readiness, request latency, CPU throughput, memory,
      allocations, shutdown, and output size against equivalent native Blink
      interpreter settings; set no unsupported performance-equivalence promise.
- [ ] Reproduce generation from a clean checkout; audit published imports,
      executable mappings, stale files, trim roots, and all runtime dependencies.
- [x] From a clean checkout, run `blink/scripts/translate.sh`, build
      `blink/ManagedConsumer.slnx` and execute its usage sample using the final
      post-processed sources; document the exact commands and observed results.
      Passed for the corrected normal-core sample at 497ce69; this does not close P5 service
      API/worker requirements or the broader dependency/platform audit.
- [ ] Verify the delivered solution resolves all authored files/projects to
      `src/`, picks up their edits on ordinary rebuild, and preserves those
      files through translation/post-processing; exclude copied authored inputs
      from the active generated project.
- [ ] Regenerate SQLite with the final compiler and rerun its JIT/AOT corpus;
      rerun picotls/MsQuic after relevant shared fixes, plus Lua/chibi and affected
      Zig/WAT checks. Record observed failures rather than relabeling old evidence.
      Earlier Linux results are historical; the shared compiler changed during
      libsmb2, and revalidation must obey the current test-scope exclusions.
- [x] Publish source/configuration/host-contract/usage/validation documentation,
      including unqualified guest features, hosts, and security boundaries.

**Gate:** all mandatory profile cases pass with reproducible receipts. Linux-only
completion may be reported as such, but the two-platform target remains open
until Windows actually runs. A passing HTTP smoke test is not full qualification.

Dependencies: P0 → P1 → P2 → P3/P4 → P5 → P6. Host feasibility can inform P1,
but later runtime gates require the real translated core.

## Completion and follow-ups

Completion means the pinned translated Blink interpreter runs the documented
service workload through the managed host and worker API, with isolation and
limits tested to the stated scope, correct faults/cleanup, and the required
platform/runtime matrices recorded. No claim of arbitrary Linux compatibility,
kernel virtualization, or hostile-code sandboxing follows from this milestone.

Follow-ups beyond the selected P5 .NET NativeAOT workload: general dynamic ELF
userspace, additional guest threading/runtime compatibility, expanded
signals/syscalls, IPv6/UDP, deterministic virtual
networks, checkpoints with external-resource reconstruction, hardened OS worker
containment, multiple in-process instances, and a Wasm backend sharing the
controller's lifecycle/host concepts. None is an implicit dependency of P0–P6.
