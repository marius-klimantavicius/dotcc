# Translate Blink to C# with dotcc

Created 2026-09-14. Campaign directory: `<repo>/blink/`.
The plan was prepared on branch `sqlite`; it does not require a branch change.

## Objective and inherited conventions

Translate Blink's actual x86-64 interpreter and reusable Linux userspace
emulation code into a reusable unsafe C# library. Build a small managed service
runner around it: start an instance from an executable and files, communicate
with its service, capture output, stop it, and release its resources. This is the
first implementation of the user's "fake EC2" idea.

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

Defer dynamic linking/general distro userspace, guest `fork`/`exec`/thread creation,
full signal ABI coverage, UDP/IPv6, Unix sockets, advanced Linux facilities,
the `blinkenlights` TUI, real-mode devices/BIOS, kernel boot, native JIT, live
snapshots/migration, full cloud APIs, and Wasm. A service that requires a deferred
feature needs a documented follow-up profile rather than a success stub.

Select and pin a small service fixture in P0; a single-threaded static musl HTTP
server is the default planning assumption. Record its source, compiler flags,
executable hash, requests, expected responses, and complete observed syscall
inventory. The user's eventual service is an additional compatibility workload;
its language/runtime is not known yet. No .NET, Node, Go, or general glibc service
compatibility is inferred from a small HTTP fixture.

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

Add a small campaign-owned C embedding entry around the actual loader and
instruction path, exposing initialize/load/run-budget/request-stop/destroy.
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
disconnects, occupied endpoints, cancellation, and closure while awaiting I/O.
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
  docs/         PLAN, source, configuration, host-contract, blockers, validation, usage
  config/       immutable source manifest, feature profile, host headers/overrides
  scripts/      fetch, native-oracle, probe, translate, build, test, dependency-audit
  src/          embedding C adapter, BCL host, owning API, worker/controller
  tests/        ABI, CPU, ELF, memory, host services, lifecycle, service fixtures
  ref/          unchanged upstream/test/toolchain inputs (ignored)
  generated/    staged C and raw/optimized C# with manifests (ignored)
  build/        native oracle, managed builds, NativeAOT publishes (ignored)
  artifacts/    diagnostics, traces, comparisons, performance/validation receipts (ignored)
```

Shared compiler/runtime fixes stay in their existing projects. Scripts resolve
paths from their own location, isolate temporary/build directories, and support
offline reruns after checksum-verified fetches. Record any upstream test toolchain
download separately; native `make check` must not silently fetch floating tools.

Proposed output: `generated/TranslatedBlink/TranslatedBlink.csproj`, class
`Blink`, namespace `Managed.Emulation`, using `--emit=managedlib --nest-types
--runtime=c --split=size --split-size=102400`. Verify actual CLI options when
implementation starts. Run the existing semantic postprocessor after normal
emission, retain an immutable raw snapshot, and never hand-edit generated output.
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

- [ ] Translate a decoder/instruction subset with a real guest memory harness.
- [x] Compare actual emitted ABI/register storage to matching native probes.
- [ ] Demonstrate budgeted stepping, faults, and nonlocal unwind without host exit.
- [ ] Map every remaining platform dependency and planned adaptation explicitly.

**Gate:** actual guest instructions execute and halt safely through the proposed
embedding boundary under JIT and NativeAOT; unresolved dependencies are recorded.

### P2 — Translate the complete selected core

- [ ] Repair actual compiler/header/runtime blockers using reduced regressions.
- [ ] Emit all required decoder, CPU, memory, loader, and syscall-marshalling code.
- [ ] Build raw/optimized libraries and whole-library-rooted AOT consumers.
- [ ] Audit imports and initializers; placeholders cannot enter runtime gates.

**Gate:** complete selected source closure builds with matching actual layouts,
no native emulator dependency, and passing affected compiler regressions.

### P3 — Qualify CPU, guest memory, and ELF loading

- [ ] Run selected upstream instruction cases and generated edge cases against
      native Blink and real x86-64 Linux execution where guest behavior is defined.
- [ ] Cover flags, shifts, signed division, SIMD lanes, floating-point edge cases,
      instruction/page boundaries, invalid instructions, and advertised CPUID bits.
- [ ] Cover malformed ELF headers/segments, overlap/overflow, BSS zeroing,
      executable permissions, stack/argv/env/auxv, and required TLS setup.
- [ ] Exercise memory growth, map/unmap/protect, cross-page reads/writes, invalid
      guest pointers, allocation failure, and cleanup. Guest code pages are data
      interpreted by Blink; no host executable allocation is needed.

**Gate:** the selected guest CPU/ELF/memory profile passes exact state/output or
explicit architectural-invariant comparisons in all four generated/runtime forms.

### P4 — Complete the per-instance host services

- [ ] Implement the required virtual filesystem, descriptors, clocks/randomness,
      guest status/signal handling, TCP sockets, and readiness contracts.
- [ ] Test invalid guest buffers, path escapes, absent files, denied access,
      duplicate descriptors, short I/O, allocation exhaustion, and host failures.
- [ ] Bind stop/deadline behavior to execution and outstanding I/O; prove no
      guest operation exits the controller or reaches an unintended host service.
- [ ] Audit required service startup syscalls; qualify additions individually.

**Gate:** contract tests and the actual guest service startup pass; unsupported
operations fail explicitly without false success or a native fallback.

### P5 — Deliver the first fake-instance service runner

- [ ] Provide an owning API for image/argv/env/limits, start/readiness, logs,
      endpoint publication, status/exit reason, stop, and asynchronous disposal.
- [ ] Implement one managed worker per instance and a bounded control protocol.
      Distinguish guest exit, guest fault, budget exhaustion, and worker failure.
- [ ] Run two simultaneous instances using the same guest port, distinct files,
      and distinct published endpoints. Verify restart and resource cleanup.
- [ ] Send real HTTP requests from a separate BCL client and verify exact status,
      headers/body as specified by the fixture, including large/fragmented traffic.

**Gate:** a separate application starts, talks to, stops, and restarts actual
translated-emulator service instances without cross-instance interference.

### P6 — Qualify faults, platforms, and delivery

- [ ] Exercise bounded seeded malformed ELF/instruction/syscall inputs and
      injected delays/disconnects/resource failures; preserve minimized failures.
- [ ] Execute raw/optimized × JIT/NativeAOT on Linux x64 and Windows x64.
      A Windows cross-build does not count as a Windows execution result.
- [ ] Measure startup/readiness, request latency, CPU throughput, memory,
      allocations, shutdown, and output size against equivalent native Blink
      interpreter settings; set no unsupported performance-equivalence promise.
- [ ] Reproduce generation from a clean checkout; audit published imports,
      executable mappings, stale files, trim roots, and all runtime dependencies.
- [ ] Regenerate SQLite with the final compiler and rerun its JIT/AOT corpus;
      rerun picotls/MsQuic after relevant shared fixes, plus Lua/chibi and affected
      Zig/WAT checks. Record observed failures rather than relabeling old evidence.
- [ ] Publish source/configuration/host-contract/usage/validation documentation,
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

Follow-ups: the user's actual service/runtime, dynamic ELF userspace, guest
threads/futexes, expanded signals/syscalls, IPv6/UDP, deterministic virtual
networks, checkpoints with external-resource reconstruction, hardened OS worker
containment, multiple in-process instances, and a Wasm backend sharing the
controller's lifecycle/host concepts. None is an implicit dependency of P0–P6.
