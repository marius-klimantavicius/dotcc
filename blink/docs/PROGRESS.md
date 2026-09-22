# Blink implementation progress

Campaign started 2026-09-14 on branch `sqlite`. The approved plan is [PLAN.md](PLAN.md).

## Current gate

**Resumed by the user on 2026-09-23.** The saved 25-file source checkpoint was
compared with its base, current HEAD, index and worktree before recovery; no
intervening changes affected those paths. The separate nine staged historical
files and unstaged old blocker additions are preserved, with an exact index/
diff backup in `artifacts/resume-reconciliation-10kx1jn6`. They include obsolete
Host directory copies and regress historical receipts; they are not active
product inputs and are not included in campaign commits.

Transient wake qualification now passes native smoke and raw/optimized
JIT/NativeAOT: 32 translated scenarios plus one direct BCL reference in each
mode (`host-io-cancellation/attempt-7bwtbsow`, receipt SHA-256
`d777809043e1b3e7cdf6061092b4b561adf1973754e7827038a9b85dac365564`).
The recovered helper's competing CTS disposal and blocked-test duplicate-wake
races were fixed; existing binding overloads are retained. This does not yet
qualify epoll/sleep wake or actual guest signal delivery. Fresh six-callback
source staging/zero-fuzz reproduction passes at
`threaded-signal-staging/attempt-o_47yol3`; owner integration now compiles in the refreshed public product. Actual signal
delivery still needs the bounded GuestSignals qualification below. The historical restart checklist
is retained in [RESTART-P5-KESTREL.md](RESTART-P5-KESTREL.md).

The six unsigned endian load/store intrinsics are committed in `0377af7`;
47 unit and 12 functional tests pass and actual compiler/postprocessor binaries
were rebuilt. The pinned-header differential passes 336 native rows and 1,344
managed comparisons across all four forms (`endian-intrinsics/attempt-u1ulwk20`,
receipt SHA-256 `859cb515c67c0cceda30e1af49c78dc63d1d27da077d34a66324db1c4cc0eb27`).
Reviewed profile selection and per-object typed-report checks are integrated.
Public delivery `translation/attempt-5vi_voys` passes (receipt SHA-256
`20d1eda5d5915c7ee38eb37f0c3e8ef5203a542a0c4d8d5aa20df92118b2c43a`):
108 freshly emitted objects, zero reuse; six matches on 55 producers and explicit
absence on 53. Final output retains the direct original source references,
literal pooling and inline deduplication. Actual Kestrel retry
`kestrel-guest-execution/attempt-nfmi2iou` passes all five native HTTP semantics,
normal exit and cleanup at 97,443,472 instructions (receipt SHA-256
`49502a64368a261c6803cb308561891e5da34c6e256f9c1ab9d4787d3a60f64d`).
Its complete trace contains no tkill or rt_sigreturn, so this success does not
qualify the recovered signal path. A normal static GuestSignals fixture is being
prepared to prove sender metadata, masked pending delivery, recursive handler
accounting and cleanup. The first actual two-instance run
`kestrel-worker-instances/attempt-3cr6q7v7` fails in raw JIT: A health succeeds;
B returns EOF before any response. Its receipt SHA-256 is
`6bfcab73dbe749713fdbae88a8bf76157510fad61f431ccd9f6f86e3b728b6e7`.
The fixture failed to retain worker termination details on this path; a diagnostic
capture correction precedes the retry, with production sources and limits frozen.
CPU refresh and actual worker/sample gates remain pending. No measured speedup is claimed; no P6 starts.

The user's subsequent source-boundary direction is now implemented in source:
`SignalActor`, `KillOtherThreads`, `SysExitGroup` and `SysExit` retain their original
C bodies and select typed authored managed methods through a separate pinned
boundary profile. The two exit targets use an explicit nonreturning compiler
contract; the SysExit adapter preserves the upstream IsOrphan decision. Narrow
thread-launch and signal-frame/metadata adaptations remain documented. The
compiler registry is also being generalized to own the fully qualified managed
method and argument adaptation. Combined compiler qualification passes 55 focused unit and 14 functional cases.
The fresh two-producer preflight passes at
`managed-boundary-preflight/attempt-oq3bl4lg` (receipt SHA-256
`6496838240f572f90ea8bd4a99d0958bcc0154c4f46be56b7194e778a918ba55`), including actual
Machine/System pointer signatures and both nonreturning exit contracts. Full
regeneration and runtime qualification remain pending; the earlier successful product receipts
remain evidence only for their exact prior sources. The valid GuestSignals native Linux witness also passes
(`guest-signals/attempt-y8mbkxpd`, SHA-256
`78c0dc100f4ac103e990d8f6ebfc6d68526815b9f2dab529c4019b5526306467`).
Fresh public delivery `translation/attempt-8roztrys` now passes (SHA-256
`348c75d3f80072a765618ee425a80dbf25fc4f624430aeccb0117fccd208fd7a`):
108 producer objects, including two exact preflight reuses; endian selection
55/53 and ownership-boundary selection 63/45. Raw/postprocessed/direct-source
builds and publication pass; the independent semantic audit verifies 226 files.
GuestSignals passes all four managed forms at `attempt-wzgc590u` (SHA-256
`ec24a1d0d30133108696e1492fbd5760400550425a4d7712a3fc1efcd2cd9c09`):
actual sender metadata, pending/unmask, two signal returns, per-thread handler
accounting, normal exit and complete cleanup. All eleven commands pass and
1,246 recorded identities independently recheck. This finite fixture does not
claim pre-bind notification or blocked guest IO interruption. The Kestrel worker
matrix retry `attempt-5czkvcnt` failed in raw JIT (SHA-256
`942470fcb630e00f78f1dce5508bf435c8a30f5e07eb987ee9b8e30ac109527c`).
Both owners reached exactly 100M instructions with actual stop reason Budget,
no guest signal/halt or managed error, and complete cleanup. A health passed;
B returned empty EOF. B's later controller reason was changed to stopped during
fixture disposal; the retained child detail independently records Budget.
All 1,561 checked identities match. Other forms were not attempted.

Read-only diagnosis found an observed missing clock contract. In the unchanged
native ELF, `Heartbeat__TimerLoop` (0x55bf80) reaches
`SystemNative_GetLowResolutionTimestamp` (0x40c1e0) and `minipal_lowres_ticks`
(0x4ddcd0), which calls clock_gettime(6) and reads the timespec without checking
failure. The prior complete standalone trace records 3,910 clock-6 EINVAL returns
on the inferred heartbeat thread and no blocking futex waits there. Clock 6 is
CLOCK_MONOTONIC_COARSE, absent from the selected private host clock capability.
The read-only disassembly/trace receipt is
`kestrel-clock-diagnosis/attempt-cdb6r_pz` (SHA-256
`9ea3c3954b0010ad4afefc95be1862af68897479b7f052b328f2ea0aee04b22b`).
The minimal same-origin monotonic coarse clock and matching resolution now
pass native and all four managed host-boundary forms at
`host-coarse-clock/attempt-ny75u0ux` (SHA-256
`814a144c582b76521d9b429def663464a66afc1a11c66ac436eeddd3400e3841`):
15 successful commands, 64 live calls plus 13 controlled-provider rows per mode,
and 260 independently checked identities. Fresh product generation is next;
unchanged upstream XlatClock dispatch and actual Kestrel behavior remain unqualified
for this correction. The 100M/60s
limits stay unchanged. This defect is established; its contribution to budget
exhaustion must still be verified by the corrected Kestrel run. The separate
sysinfo fallback (1 GiB total, zero free) remains a documented audit finding,
not an established cause or a new implementation task. CPU/final sample refresh
remain held until the corrected product is stable.

P5 is now explicitly authorized: build and execute a real ASP.NET Core/Kestrel
NativeAOT HTTP guest through translated Blink, distinct from NativeAOT compilation of the
emulator host. Guest creation first uses the installed SDK for ordinary Linux
publishing; musl/container tooling is considered only if needed. If producing
the musl guest is very difficult or impossible, stop and ask for user direction
through the coordinator; no prolonged build workarounds. No P6 phase starts.

User guest update: HTTP must be served by Kestrel, replacing the authored
raw-socket HTTP implementation as the required workload. Existing raw-socket
results remain baseline evidence. Kestrel build/runtime/integration gates are
open. The new Kestrel guest worker owns a separate reproducible fixture/native
witness; the coordinator audits host gaps and integration. Fresh raw-socket
sample verification passed, but cannot close the updated Kestrel P5 gate.

Both ordinary glibc and static musl raw-socket NativeAOT guest builds and native HTTP
references pass. The static musl build succeeded in one standard pinned Podman
attempt, so the difficult-build stop condition did not trigger. The genuine
static-musl .NET guest now passes raw/optimized JIT/NativeAOT: actual readiness,
exact native health/stop responses, exit0, four joined guest workers and complete
resource release (`dotnet-threaded-guest-execution/attempt-febf3tyc`). The public
translation command now delivers the reviewed threaded profile directly at
`generated/TranslatedBlink` (`translation/attempt-i4a5mfa8`).

Actual worker/controller qualification now passes all four modes at
`worker-instances/attempt-kd2trw_m`: 16 workers and 40 exact HTTP comparisons,
simultaneous private instances, normal/cooperative shutdown, restart and idle
deadline cleanup. Native traffic reference `worker-native-traffic/attempt-znwlxua8`
also passed. A historical final-sample task was rejected by automated review
(B036). After the user requested fresh verification, the actual solution and
JIT/NativeAOT samples passed at `managed-consumer-delivery/attempt-8k2fus34`
(commit f06256d). These are raw-socket baseline results; Kestrel remains pending.
P6 has not started.

The genuine static-musl Kestrel guest builds and passes its five native HTTP
cases. Its upstream SIMD comparison-mask defect is corrected and passes 2,056
managed CPU comparisons. A bounded 128 MiB address-space/backing profile now
reaches actual Kestrel READY and passes health, large and fragmented requests.
The next required contract is cross-thread activation: the guest's signal 35
is queued upstream, but the internal wake callback rejects nonzero signals,
causing a guest abort. Reviewed causes also include absent sender PID metadata
and nested handler dispatch outside owner accounting. Source implementation is
integrated after recovery, with transient Host wake and fresh product compilation
qualified. The standalone Kestrel five-request run now passes without invoking
activation; actual guest signal delivery remains separately unqualified. The worker
matrix first failed with an empty second-instance response, pending termination
diagnostics; the actual sample remains unqualified for Kestrel. No P6 work starts.

The full P5 checklist is in PLAN.md. The C fixture and NativeAOT publication of
the emulator host alone do not satisfy the real NativeAOT guest requirement.
Prior P4 results below retain their exact scope.

The current product is a 108-producer translated library with no `CoreProbe`,
test `main` or C execution driver. Original adapters live in `src/Host`, headers
in `src/Host/include`, and the BCL implementation in `src/Managed.Emulation.Host`.
The generated project links original sources/projects directly; immutable
raw/profile snapshots are archival inputs only. Shared narrow-literal pooling,
semantic postprocessing and the actual final direct-source build pass with
unchanged authored hashes (`translation/attempt-i4a5mfa8`).

The prior P4 authored `Managed.Emulation.Execution` C# API owns initialization,
loading, the instruction loop, stop and teardown through upstream exports. The
real sample passes JIT/NativeAOT health and normal stop. All four service forms
pass six exact native HTTP cases each (`guest-service/attempt-18nn8vfz`). The
ordinary stop/deadline matrix passes eight native controls and all 44 managed
cases. P4 is complete for the finite selected profile. Its earlier stop boundary
was subsequently lifted for the explicitly authorized P5 work above; P6 remains
held.

User test-scope update: custom fault-injection and invalid/malformed-ELF tests
are excluded from work and completion gates. Such cases may run only when
already present in pinned upstream tests, with revision/path provenance.
Valid ELF loading, ordinary functional/lifecycle tests and runtime error handling
remain in scope. Earlier custom-test blockers below are historical and no longer
block completion; excluded checks are not reported as passed.

P0–P3 have passed for the selected Linux x64 profile. `blink/scripts/translate.sh`
produces the final postprocessed `blink/generated/TranslatedBlink/` project and
immutable raw comparison. Corrected canonical core execution passes all four
managed forms. The final CPU set passes 504 normal cases per form (2,016 total),
with 46 custom fault cases excluded. Actual guest-memory lifecycle algorithms,
valid ELF loading and explicit TLS startup also pass native and all four forms.

The owning normal-core sample and fresh clean public delivery at revision
497ce69 pass with the earlier INC/CMPXCHG8B repair (109 fresh objects, zero reuse).
The subsequent NEG repair has fresh delivery, core and CPU qualification; the
clean receipt is not relabeled as a run of that later revision.
P4 is complete for the selected Linux x64 profile. P5/P6 remain open for
subprocess/multiple-instance service integration, Windows execution and remaining
publication/performance/regression qualification. Historical custom fault results are excluded, not passed.

## P3 completion evidence

| Owner | Completed work | Evidence |
| --- | --- | --- |
| CPU worker | Finite flags/widths/shifts/division, scalar FP, packed SSE2, addressing/cross-page access, REP, CMOV, CLFLUSH, RDTSC invariants and balanced stack set; four targeted NEG repair regressions | `cpu-conformance-managed/attempt-disfjyq2`: 504 cases in each raw/optimized JIT/NativeAOT form, 2,016 matches, 46 exclusions. First 546 descriptors preserved. No clock-value equality claim. |
| Inputs worker | Actual guest page tables: growth, cross-page copy, protection metadata/permitted access, unmap/remap, cleanup and two lifecycle cycles | `guest-memory/attempt-gbyg6j74`: native/all-four exact transcript, vss cleanup and stable bounded retained pool, then final disposal. |
| Coordinator and reviewers | Canonical NEG integration, source/producer identity review and retained valid ELF/TLS evidence | `core-execution/attempt-273a6hks`, `elf-loading/attempt-qclmm2fz`, `tls-loading/attempt-m6fpvl1m`; only four ALU NEG bodies changed since the memory/loader runs, with the other 108 producer records and Host/header/compiler inputs identical. |

This closes the finite selected-profile P3 gate. It is not exhaustive ISA
certification, a general libc/dynamic TLS ABI, forbidden-access enforcement
qualification, or post-NEG reexecution of unchanged memory/loader tests.
All P3 assignments finished and released the build slot. The next section
records the separately authorized P4 work.

## Prior phase: P4 completed at the requested boundary

The user explicitly authorized P4 after the selected P3 milestone. This phase
stops at P4 completion or a concrete blocker after independently permitted P4
work is exhausted; P5/P6 do not start automatically. P0–P3 are not reopened.

The concrete map and source/receipt distinctions are in
[P4-HOST-SERVICES.md](P4-HOST-SERVICES.md). Initial work traces the current binding manifest, actual translated syscall
paths, host implementations and exact execution receipts. Historical pending
labels are not treated as present defects without checking current code.

| Owner | P4 task | State |
| --- | --- | --- |
| Inputs worker | Ordinary owning execution, zero-fd poll, sleep and inherited-I/O stop/deadlines | Eight native controls and 44 managed cases passed; assignment complete |
| Guest worker | Separate C# execution owner and actual pinned service | All 24 HTTP cases passed across four forms; assignment complete |
| Coordinator | Product integration, exact checklist, review, serial validation and commits | Product, sample, service and stop gates passed; evidence reviewed and phase closed |

The user explicitly requested another P4 attempt on 2026-09-20. The earlier
generic automated rejection remains historical evidence, not a permanent scope
rule invented by the campaign. Ordinary local execution/service implementation
is authorized; any actual current tool rejection must be recorded exactly and
must not be concealed, renamed or routed around. Actual service startup is now qualified in all four forms. Custom fault-injection and malformed ELF stay excluded.

## Ownership

Work resumed after the user's instruction that libsmb2 has stopped and Blink
should continue. All implementation and milestone commits use `sqlite` in the
main checkout. Historical recovery/detached worktrees remain evidence only.

- Coordinator: current shared-toolchain build/identity freeze, normal-only probe
  scope alignment, integration, runtime revalidation, durable status and commits.
- Inputs worker: P5 host boundary qualification and read-only musl stack/GC review.
- Guest worker: P5 dispatcher integration and read-only guest thread ownership review.

Shared compiler edits and heavy test suites remain serialized. Workers own
disjoint authored files and do not commit duplicate recovery-branch history.

## Milestones

| Milestone | State | Evidence / remaining work |
| --- | --- | --- |
| P0 | Passed | Immutable sources verified offline; native Blink and 25 assembly cases pass; six HTTP cases pass on Linux and Blink; exact native archive/import/global audit and initial translation failures recorded. |
| P1 | Passed | Actual bounded instructions, synchronous faults/unwind and exit/exit_group match native under raw/optimized JIT/NativeAOT; profile ABI matches a separate native probe. |
| P2 | Passed, delivery refinements passed | 108 product producers, upstream exports without test/C execution frontends, shared literal pool, direct original-source references and immutable raw comparison. Separate C# sample passes JIT/AOT. |
| P3 | Passed — selected profile | 504 normal CPU cases per form (2,016 matches), valid ELF/fixed TLS and actual guest-memory lifecycle pass. Bounded coverage and retained producer evidence are documented above. |
| P4 | Passed — selected profile | Native/all-four contracts, 24 actual HTTP comparisons, eight native completion controls and 44 actual owning stop/deadline cases. Phase stopped; no later phase started. |
| P5 | Active | Genuine glibc/static-musl NativeAOT guest and native HTTP pass; actual translated startup and first required memory-barrier boundary are recorded below. NativeAOT readiness, threaded runtime, worker/API, restart and two-instance lifecycle remain open. |
| P6 | Partial | Native 25 and corrected clean public delivery pass; additional scoped regressions/performance remain unrun. Windows and full performance/dependency gates remain open. |

## Observed environment

.NET SDK 10.0.111 is available. Baseline build disables automatic sibling LALR.CC substitution using `-p:UseLocalLalrCc=false`. Test TMPDIR is isolated to `blink/artifacts/tmp`.

## Remaining delivery gates

The current shared Release compiler is frozen and qualified by the receipts
above. P2 delivery, the separate C# execution sample and P4's actual service and
stop/deadline gates have passed for the selected Linux x64 profile. P5's
subprocess worker, restart and two-instance HTTP lifecycle remain unqualified.
Prepared P6 regression/performance work remains unrun, and actual Windows
execution still requires that platform. P5 is explicitly active; P6 remains held.
Earlier automated-review stops are historical evidence.

## Observed validation (initial campaign baseline)

- Release solution build passed with `UseLocalLalrCc=false`.
- Unit tests: 2218 passed, zero failed.
- Functional tests: 490 passed, 1009 skipped, zero failed. Skips are preserved, not counted as executed passes.
- Native Blink interpreter: 25 selected upstream assembly cases agree with direct Linux execution.
- Static musl service: six exact-wire HTTP cases, including 128 KiB response and fragmented requests, pass direct Linux and Blink; clean exit.
- Full source/checksum verification passes offline. Host distribution toolchain packages are not yet archived.
- First actual translation blocker (comma-separated bit-fields) reduced and repaired generically; five focused cases pass. Full post-fix regression run is active. Next decoder blocker is multidimensional array parameters.

Receipts are under `artifacts/`; durable inputs and interpretations are in SOURCES.md, GUEST.md, HOST-CONTRACT.md and BLOCKERS.md.

## P0 exit evidence

Native link audit observes 147 archive members, 89 extracted by the CLI, 185 dynamic imports and 151 writable state candidates. `DEPENDENCIES.md` separates this native CLI closure from the future managed embedding; it does not infer isolation from compile flags. The disabled-thread profile makes `g_machine` and `g_siginfo` ordinary process globals. P0 is complete; P1 is active with a real native bounded interpreter seam under investigation and managed decoder/compiler blockers still open.

## Significant progress: generic bit-field declaration fix

Post-fix Release build passed; 2218 unit tests passed; 491 functional tests passed with 1011 explicitly skipped. The new native-checked fixture verifies adjacent byte-width fields, anonymous padding, signed values, compound initialization and actual storage bytes. P0 baseline committed as `5f6ced0`.

## P1 decoder progress

The actual upstream decoder now emits, compiles and executes in raw/optimized JIT/NativeAOT on Linux x64. All six native/staged/raw/optimized output files have identical SHA-256, checking 11 instruction encodings and actual bit-field/operand/decoded-record layout. `scripts/probe-decoder.sh` reproduces this and records compiler/config/source/generated hashes. This is decoding, not instruction execution, so P1 remains open. Native bounded core probe separately exercises actual ExecuteInstruction with guest page tables, normal arithmetic/memory, invalid instruction, unmapped access and budget exhaustion; managed core port remains blocked.

## Significant progress: array declarators and tagged function returns

Generic named/unnamed array parameter support now retains inner row dimensions and discards only the outer bound; `sizeof(*parameter)`, pointer arithmetic, indexing and writes match native C. Function specifiers before struct/union/enum return tags preserve inline flags and qualifiers. Three new reduced native-checked fixtures pass. Full Release build passes, with 2218 unit tests and 494 functional tests passed; 1017 functional rows explicitly skipped. Actual upstream decoder/core retries clear both defects.

Decoder qualification rerun after the final parser rebuild: raw/optimized JIT/NativeAOT again match the untouched native and staged-native output exactly. Current compiler/config/generated identities are captured in the refreshed decoder receipt. The core profile now has its own explicit exclusions; its host header profile remains declarations-only and cannot satisfy runtime gates.

## Significant progress: native embedding and explicit host storage

Native actual-ExecuteInstruction harness passes eight rows (four cases repeated with create/destroy), with an observed 83-source archive extraction. The core has an independent feature profile and hash-verified staged inputs; pureconst is removed only as a required-match optimization hint, with ABI attributes retained. Host declarations isolate 53 operations, with 87 native layout/offset checks, 252 constant checks and seven unresolved-symbol checks passed. Host declarations remain unimplemented and cannot enter runtime gates. Native host storage baseline committed as `be2d56e`; emitted storage comparison is in progress.

The first long full-core retry was invalidated by editing the running shell script, which caused a duplicated invocation. The duplicate was terminated and this run is recorded as interrupted, not as a clean compiler timeout or pass. Subsequent diagnosis uses immutable per-attempt scripts/profile snapshots and bounded translation-unit isolation.

## P4 feasibility work while core translation proceeds

Implemented a private memory-backed filesystem host module with typed error results, immutable cloned images, bounded writable storage/descriptors, path lookup, shared-cursor dup/close, sparse zero-fill, short writes, seek, append and cleanup. A separate consumer passes four assertion groups under Linux JIT and NativeAOT, including two-instance isolation and resource failure atomicity. The C callback/guest descriptor/socket integration is not implemented, so P4 remains pending.

Host storage ABI qualification also now passes all 174 emitted measurements in raw/optimized JIT/NativeAOT against native authored storage, including actual field-address offsets, aggregate placement and array strides. This is storage compatibility, not implementation of the 53 declared host operations.

## Significant progress: safe unmanaged jumps and C declarations

Ordinary C jump buffers now store a numeric identity instead of a CLR reference in unmanaged memory. Lowered handlers capture a freshly armed identity and evaluate buffer expressions once. Allocated, nested, rearmed, zero-value and compacting-GC cases pass; the independent jump consumer agrees with native output under raw/optimized JIT/NativeAOT, with managed-pointer warnings treated as errors. Signal-aware mask saving remains separate work.

Function-form parameters now adjust to function pointers. Global array typedefs retain full storage, including multidimensional arrays; thread-local arrays have per-thread pinned managed roots. Reduced direct/object-linked tests cover two simultaneous threads and compacting GC. Full Release regression after all three compiler repairs passes 2222 unit tests and 500 functional tests, with 1025 functional rows explicitly skipped and zero failures.

## Significant progress: isolated TCP host

The managed TCP module now uses real sockets with per-instance virtual bindings
and explicit physical loopback publication. Independent instances reuse guest
port 8080, exchange exact fragmented/128 KiB traffic through separate clients,
and retain separate endpoint metadata and descriptor limits. Actual socket
backpressure, cancellation, close during receive and disposal of pending accept
operations pass bounded tests. Four assertion groups pass Linux JIT/NativeAOT;
both host suites now record source, assembly, executable and output hashes.
This remains a typed host module without guest callbacks or a unified guest
descriptor table, so P4 and P5 remain open.

## Significant progress: signal-aware unwind and measured host declarations

A separate synchronous signal-jump adapter now captures/restores a virtual
host-delivery mask when requested. It owns explicit 336-byte storage, keeps
Blink's guest Linux mask separate and changes no host OS signal state. A real
native POSIX oracle, native virtual adapter and raw/optimized JIT/NativeAOT agree
for saved/unsaved masks, nesting, rearming, repeated jumps, zero-to-one returns
and evaluated-once buffers, including compacting GC. Host storage now passes
99 native comparisons and 198 emitted outputs in all four modes.

Actual syscall translation exposed missing flock, timer and resource declarations.
Their authored headers have measured native layouts/constants and unresolved
host-prefixed operations. Timer14/resource28 output rows also agree in all four
emitted modes; native inventory checks308 constants and9 isolated symbols.
These declarations implement no file locks, interval timers or resource services.

Individual actual upstream memory.c now emits after erasing only an exact-match
GNU flatten optimization hint. machine.c and syscall.c reach a valid negated
assignment setjmp condition that needs generic recognition. Full-core translation
remains open. The new actual Machine/System ABI probe reached C# compilation and
exposed the upstream System type colliding with the BCL namespace; planned nested
library output is being checked before selecting a structural repair.

## Significant progress: actual core ABI and final compiler repairs

The planned nested managed library now preserves Blink's `System` type while
qualified BCL references prevent namespace collisions. Assignment-guard setjmp
lowering preserves the buffer expression's evaluation order, loop exits and
late/repeated jumps. Full Release build passes without warnings; 2225 unit tests
and 505 functional tests pass, with 1027 explicitly skipped functional rows and
zero failures.

After the final shared rebuild, decoder qualification again matches native in
all four generated/runtime modes. A separate consumer of the actual upstream
core type library also passes 236 exact ABI/register outputs under raw/optimized
JIT/NativeAOT with the entire library rooted for AOT. It checks real offsets,
strides, register aliases/vector bytes, 16-byte alignment, callbacks, ELF and Linux
records. The explicit signal profile changes Machine 22432→22576 bytes; System
remains 3016 bytes. This completes P1's storage comparison item, not its actual
instruction execution gate.

Actual machine.c, memory.c, instruction.c, sse2.c and syscall.c now emit as
isolated objects after measured host declarations and generic fixes. Full
83-source nested-library emission is running with a frozen profile and 1800-second
bound. The memory worker is implementing explicit bounded host allocation and
capabilities to replace arbitrary native address probing and temporary-file
fallback; upstream guest memory algorithms remain unchanged.

## Significant progress: one instance I/O descriptor table

`InstanceIo` now owns files, sockets and standard streams through one bounded
descriptor namespace. Dup shares file/input cursors and socket lifetime; only
the last socket close cancels its operations. Standard input is cloned/bounded,
and stdout/stderr share a bounded capture budget with exact short writes and
owned snapshots. Failed allocation cannot create or truncate a file. Disposal
releases input/files, drains socket/accept operations, and retains only bounded
captured logs for the caller. Four independent consumer assertion groups pass
Linux JIT/NativeAOT without build warnings. Guest C callback marshalling remains
pending, so this is host implementation progress and not guest-service startup.

The full frozen core translation returned a real bare-negated setjmp diagnostic
in debug.c after roughly six minutes. It was not a timeout. Continued isolated
translation records progress beyond that point while the generic guard repair
and bounded host memory adapter are qualified.

## Significant progress: bounded non-linear host memory

The host memory adapter owns real aligned anonymous allocations with a bounded
payload/record budget, zero filling, exact-owner frees and explicit unsupported
operation errors. Hash-checked staging replaces only map.c's native page/address
discovery with 4096-byte/47-positive-bit logical capabilities and selects anonymous
mapping without its temporary-file fallback. The guest memory algorithms remain
upstream. Native staged InitMap and raw/optimized JIT/NativeAOT allocator tests
agree; two independent worker-thread owners survive compacting GC.

The same adapter also passes the actual native core arithmetic, budget and fault
probe through NewSystem/page allocation. Upstream caches two mappings totaling
270384 charged bytes after FreeSystem; final release is valid only when the
complete worker state is discarded. The adapter does not yet bound all generic
malloc allocations or implement mapped files/mprotect; teardown and managed core
execution remain open. Full managed source closure now explicitly includes
hash-pinned upstream pte32.c, which the native archive extraction omitted.

Additional retained mutex-attribute storage is measured at 4-byte size/alignment.
The host ABI baseline now passes 101 native comparisons and 202 emitted outputs
in all four Linux modes; the times header adds six matching outputs.

## Significant progress: conditional jumps and nested string initializers

Bare negated setjmp guards now preserve the protected block and recovery path,
including later jumps. Known-dimension nested character arrays use target-typed
string row initialization without changing pointer-array behavior. Native
reductions and focused tests pass, followed by a warning-free Release build,
2228 unit tests and 507 functional tests; 1031 functional rows are explicitly
skipped. The actual disarg.c translation now emits. Omitted outer dimensions
with retained inner array dimensions are the next measured parser blocker in
disspec.c; a native reduction is prepared, with implementation still pending.

## Significant progress: diagnostic ownership and explicit byte order

Hash-checked debug.c staging replaces its native signal-handler pointer probe
with reads limited to mappings owned by the current worker. It preserves the
existing diagnostic sentinel and upstream load/store implementations. Native
untouched/staged and raw/optimized JIT/AOT tests pass for all modes, invalid and
foreign pointers, boundary crossings, forced GC and unchanged host signal masks.
General malloc storage remains conservatively unavailable to this diagnostic.

The native oracle exposed missing endian declarations: undefined macros compared
as zero and selected byte swapping. An explicit little-endian storage profile
now rejects big-endian targets without impersonating a native CPU or OS.
Independent byte-pattern read/store checks pass. Decoder, bounded memory/native
core, and all 236 actual core ABI observations were freshly requalified in all
four managed modes with this profile. Full-core staging snapshots both precise
host boundaries and its one-shot worker allocation owner.

## Significant progress: real clock and entropy callbacks

An authored bridge now binds per-instance BCL clock/secure entropy providers on
an explicit C worker thread. Translated C calls are exercised against a native
probe in raw/optimized JIT/AOT, with deterministic injection, short requests,
provider failures and unbound errors checked by the consumer. All pass. The
bridge contains managed exceptions and holds managed ownership outside C storage.
Full-core capability selection and guest pointer/syscall integration remain open.

Current ownership: coordinator integrates host callbacks and commits; guest
worker owns inferred outer-array dimensions and subsequent core closure
translation; inputs worker audits advertised CPUID against excluded handlers.
P0 is passed. P1 remains active; P2–P6 remain open.

## Significant progress: private file/vector C callbacks

The common instance descriptor table is now reached by real translated C
open/close/dup/seek/read/write/readv/writev calls. Bounded 64 KiB owned transfers
preserve legal short I/O; paths remain inside the private namespace. Native C
comparisons and raw/optimized JIT/AOT pass, including 128 KiB exact bytes, sparse
files, shared cursors, optional argument evaluation, captured streams and two
concurrent owners. Full-core binding, guest buffer validation, metadata and
complete socket/readiness callbacks remain open.

## Significant progress: real TCP through translated C

Authored socket callbacks now reach the same private descriptor table and real
BCL TCP implementation. Native C and raw/optimized JIT/AOT agree on fragmented
requests, exact 128 KiB responses, EOF and sockaddr boundaries. Two simultaneous
translated C servers use guest port 8080 with different published endpoints.
External destinations are denied; disposal releases a worker blocked in C accept.
Full x86 guest syscall binding, readiness and service startup remain pending.

## Significant progress: inferred outer array extents

The generic grammar/lowering now retains inner dimensions for omitted-outer
array initializers at global, static-global, local and static-local scope. It
infers rows from grouped initializers or scalar counts and preserves string row
semantics. Native reductions and 128 focused array tests pass; the full suite
passes 2231 unit and 508 functional tests, with 1033 explicit skips. Actual
disspec.c now emits, and the core object scan has advanced through instruction.c.

Small translation units that never encounter an override definition are retried
only when their actual preprocessing trace proves that absence; the failed
required-match trace remains preserved. Mismatching or partly selected overrides
still fail. The next observed closure gap is measured winsize/ioctl host storage,
which the guest worker is qualifying before continuing the frozen object scan.

## Significant progress: CPUID matches selected exclusions

Exact-hash staging adds existing exclusion guards to three incorrect x87/MMX
feature advertisements. Eight native configurations permit only those changes;
16 actual CPUID queries agree in raw/optimized JIT/AOT. Seven actual native
instruction probes verify excluded x87/MMX/BMI2/ADX faults and retained SSE2/
FXSAVE-XMM behavior. The retained-feature inventory distinguishes selected code
from qualification. This resolves the demonstrated B004 mismatch; broad managed
instruction/timing/entropy/syscall qualification remains open.

## Significant progress: terminal ABI and continued core closure

Native-measured terminal size storage and five ioctl constants now qualify in
11 exact rows under raw/optimized JIT/AOT. Calls are isolated as blink_host_ioctl
and remain unimplemented; the declaration never reports false success. Actual
ioctl.c emits. Core snapshots also include the qualified CPUID adaptation.
The next actual source gap is log.c's vsnprintf dependency: its generic header
and runtime lacked the required va_list formatter. A reduced generic repair is
in progress while the independent source scan continues.

## Significant progress: actual native guest exit trap

The embedding harness now enables upstream System.trapexit and executes real
Linux exit and exit_group instruction sequences. Both return through the
existing halt jump with exited/status recorded, before host exit or machine
free. Native runs repeat successfully with statuses 37 and 42 alongside the
existing arithmetic, budget and fault cases. Upstream restores the syscall IP
to its instruction start (0x40000a), confirmed by throw.c and the native probe.
No syscall algorithm or process-exit implementation was patched. Managed
instruction execution remains open; the frozen source profiles will incorporate
this extended authored harness on their next generation.

## Significant progress: private metadata and readiness

The private filesystem now exposes stable inode/type/size/mode/timestamp metadata
through real stat/fstat/lstat and restricted fstatat C callbacks. Native 144-byte
host layout checks, native filesystem invariants and four managed modes pass;
existing private file/descriptor regressions also pass. Execute permission for
image files is explicit opt-in. Unsupported directory-descriptor traversal and
metadata operations remain explicit, with no generic host stat fallback.

Readiness callbacks use the same descriptor table and actual BCL socket state.
Native and raw/optimized JIT/AOT pass invalid/negative fd, timeout, pending accept,
FIN/half-close/HUP, close and disposal cases. Infinite polls, including an empty
fd set, join the owner's disposal drain. A barrier proves two private port-8080
listeners coexist before real clients connect. Priority/band events are unsupported;
reset/OOB and full x86 guest syscall integration remain open.

## Significant progress: real va_list formatting

The shared stdio declaration/runtime now implements vsnprintf over a borrowed
va_list cursor, preserving byte counts, truncation, zero-capacity destinations,
va_copy/current-cursor use, dynamic width/precision and %n. Native and all four
managed modes agree, and actual log.c emits. Wide strings/characters and
long-double cursor conversions reject explicitly instead of claiming support.
The related snprintf zero-size path no longer writes a terminator.

Full regression passes with a warning-free build, 2234 unit tests and 509
functional tests; 1035 functional rows are explicitly skipped. The next shared
fixes are `_Noreturn static` declaration ordering and unresolved authored callback
function-pointer ownership. The latter currently prevents the termination-guard
probe's C# build; the failed generated snapshot remains preserved.

## Significant progress: private process identity

Read-only C process/credential queries now have explicit per-worker identity
bindings. Default PID1/parent0 and uid/gid0 match the single private process and
filesystem owner; no host identity is queried or changed. Native C invariants
and raw/optimized JIT/AOT pass, including exact injected IDs, errno preservation,
unbound errors, concurrent workers and compacting GC. Full-core startup must
bind this before NewSystem; credential/process mutation remains unsupported.

## Significant progress: directory-relative opens and descriptor flags

Private openat now resolves real directory descriptions, and metadata fstatat
uses the same lookup. Per-descriptor close-on-exec state is distinct from shared
file status; dup clears it, explicit CLOEXEC duplication sets it, and changing
append status affects the shared open description. Unsupported nonblocking and
locking requests fail atomically. A C variadic adapter evaluates all arguments
and consumes only the integer arguments required by supported commands.

Native C/raw/optimized JIT/AOT and refreshed metadata matrices pass, together
with the older file/descriptor regressions. A native real-exec oracle verifies
CLOEXEC behavior; the managed owner exposes the equivalent explicit transition
without claiming guest exec startup. Writable payload and fd counts are bounded,
but zero-length node/path metadata still need a separate allocation quota.
Measured PATH_MAX/PIPE_BUF declarations also let actual overlays.c emit; they
do not themselves implement path policy or atomic pipe writes.

## Significant progress: declaration ordering and callback ownership

Generic parsing now accepts the actual `_Noreturn static` function declaration.
Unresolved function addresses now resolve in the translated owner's lexical
scope, matching direct calls while retaining runtime fallback and authored
shadowing. Native reductions and direct/object-linked consumers pass; the full
suite passes 2235 units and 514 functional rows, with 1037 explicit skips and
a warning-free build. Actual memorymalloc.c emits beyond its declaration.

The termination guard now passes native/direct/indirect and all four managed
modes. Unexpected C exit/abort paths produce typed worker failures instead of
terminating the controller; normal guest exit remains the upstream trap. The
last known source parser gap is strace.c's ordinary static-local array declaration
with multiple declarators; upstream removes its TLS marker in this profile.

## Significant progress: static-local array declarations

Generic lowering now gives every static-local array in a declaration list its
own rooted pinned storage, preserving dimensions and initializers. Native and
direct/object-linked tests pass across compacting GC, and actual strace.c emits.
The full repository suite passes 2235 units and 517 functional tests, with 1039
explicit skips. All 83 native-selected core translation units now have at least
one historical successful isolated emission; a single frozen profile, complete
link and actual managed instruction execution remain the next gates.

## Significant progress: private file-backed host mappings

The authored host mmap adapter can copy existing private file pages into bounded
owned aligned storage through positional reads. Cursor state is preserved, a
partial EOF page is zero filled, mappings survive descriptor/instance disposal,
and failures roll back all allocations. Unsupported protection/shared/fixed and
whole absent file pages return explicit errors; native SIGBUS is independently
observed rather than simulated by readable zero pages. Native, all four managed
modes, ASan/leak checks, and anonymous/diagnostic regressions pass. Full loader
execution remains open; startup must bind I/O, begin memory, then enable files.

## Significant progress: real public signal sets and virtual mask calls

The actual core's isolated sigemptyset/fillset/addset/delset/ismember and
sigprocmask calls now have authored C implementations over the existing virtual
worker mask. Native comparisons and all four managed modes pass for public bits,
reserved/unmaskable signals, old-mask queries and atomic errors. The real host
thread mask remains unchanged across forced GC. Guest signal delivery and
handler/kill/suspend operations remain separate open contracts.

## Significant progress: filesystem capacity ABI declarations

The measured statvfs record and flags now let actual statfs.c translate without
pretending to implement capacity reporting. Native and emitted layout/constant
checks pass in all four managed modes (36 rows). Calls retain isolated names
until a private filesystem capacity policy exists.

## Significant progress: injected time-of-day and clock resolution

Actual gettimeofday and clock_getres callbacks now share the bound instance
TimeProvider. Native and raw/optimized JIT/AOT checks pass, including normalized
pre-epoch time, private worker values, provider failures and unchanged outputs
on errors. Reported resolution is the software output quantum, not a claim of
physical clock accuracy. Selected-core native import evidence and lexical binding
hints are reproducible. Sleep/timer behavior remains open: unchanged upstream
wait code requires a real interruption/remaining-time contract.

## Significant progress: combined frozen core binding and object pipeline

The explicit binding manifest snapshots authored C/C# host adapters and the Host
project. Every verified upstream translation unit receives the same boundary
preamble, including utility sources that do not include builtin.h. Existing
map/debug/CPUID adaptations retain their exact source-hash chain. Additional
upstream sources and authored wrappers participate in one manifest.

Canonical per-translation-unit C snapshots preserve the absolute paths used by
static symbol names and __FILE__. Cache identity includes selected source, all
headers/included fragments/overrides, pinned upstream inventory, compiler DLLs
and runtime descriptors, options and helper scripts. A copied profile with only
a C# bridge change reused two objects with their original producing receipts;
changed C/header/parser inputs invalidate reuse. This avoids re-emission for
independent consumer changes without mixing unverified C inputs.

Actual address, pte32, prog and machine sources pass the combined preamble. The
frozen attempt-7cn5o6ww full set (90 sources) is now emitting with two workers.
Linkage, initializer/import audit and actual managed execution remain open.

## Significant progress: bounded namespace metadata

VFS nodes and canonical UTF-8 path names now have separate owner limits, including
empty files, image nodes, implicit parent directories and root. Repeated empty
create/close can no longer bypass payload limits. Image overflow rejects setup;
new-file quota errors preserve namespace/descriptors, and existing opens consume
no extra charge. Focused JIT/NativeAOT and InstanceIo regressions pass. Logical
namespace bounds do not claim exact CLR allocation or total worker memory bounds.
The active core build keeps its earlier immutable Host snapshot.

## Significant progress: nonterminal descriptor policy and termios storage

The private descriptor model's terminal operations now return ENOTTY for real
instance descriptors and EBADF for invalid/closed descriptors; payload pointers
are never dereferenced. The service's TIOCGWINSZ stdout probe is covered. Normal
C argument evaluation remains intact through the variadic ioctl adapter. Pure
termios speed helpers match native full-record bytes across 131104 setter cases.
Native/staged-native and raw/optimized JIT/AOT matrices pass. These bindings are
ready for the next combined profile; the current core build stays frozen.

## Significant progress: additional private process metadata

Read-only credential triples, supplementary groups, process/session IDs,
hostnames and selected sysconf queries now use the bound immutable identity.
Native and all four managed modes pass, including exact separate worker names
and IDs, compacting GC, invalid queries and unchanged failed outputs. Measured
selector values are explicit; reported page size matches the mapping adapter,
while ticks are a declared guest ABI value. Process mutations and CPU-accounting
operations remain isolated and unimplemented.

## Significant progress: private cwd and canonical paths

Each InstanceIo now owns its current directory. Relative opens/stat and AT_FDCWD
use it consistently; explicit cwd overrides and real directory descriptions keep
their intended bases. getcwd/chdir/fchdir/realpath callbacks share the existing
VFS component walk and never consult host paths or working directory. Native
common invariants, raw/optimized JIT/AOT, and existing file/I/O regressions pass.
Checks include two different worker cwd values, root escape rejection, unchanged
errors, and bounded malloc/free-compatible result strings surviving GC/disposal.

## Significant progress: explicit UTC calendar and instance time

C time() now uses the bound realtime provider; gmtime_r/localtime_r use the
generic pure UTC converter with an explicit UTC-only policy and declared
years1–9999 range. Actual native calendar cases and raw/optimized JIT/AOT pass,
including injected negative epoch times and unchanged errors. This supplies
log.c's calendar dependency without consulting the host timezone. An initial
copied-runner path mistake is preserved as failed harness evidence; the passing
receipt selects and executes the intended calendar fixture.

## Significant progress: immutable private environment variables

Selected commandv/fspath/demangle getenv calls now have an immutable instance
variable owner with bounded entries/name bytes/total UTF-8 data and stable
owned native value pointers. Missing, empty and equals-name lookups preserve
errno; no host process environment is read. Native and all four managed modes
pass, including GC, repeated pointers, independent workers and explicit teardown.
All upstream cached pointers must be discarded before disposing that owner.
No selected setenv/unsetenv calls exist, so mutation remains isolated.

## Qualification correction: detailed identity fixture

The first IdentityDetails runner accidentally copied the older basic identity
fixture. Its bridge compiled, but the reported detailed coverage was premature.
The corrected runner now executes the intended native and managed assertions;
all four managed modes pass at attempt-fwi4bpie and snapshot source hashes match
the intended files. It also exposed missing ENAMETOOLONG in the shared header,
which the measured host errno profile now supplies. Earlier evidence is retained
with its actual narrower scope, not counted as detailed identity qualification.

## Significant progress: complete measured host errno names

The host profile now exposes 134 native-measured public errno names while
preserving generic thread-local errno storage. All 59 existing names match;
75 missing names are added, and preexisting mismatches fail preprocessing.
Native and raw/optimized JIT/AOT values and thread isolation pass. This fixes
preprocessor selection as well as unresolved names: upstream error translation
can now include cases for errors the private callbacks already return. Fresh
core preprocessing is required; old missing-branch objects are not reused.

## Significant progress: private executable/access checks

access/faccessat now use the existing resolver, current directory, directory
descriptors and real private metadata. Virtual UID0 execute checks require an
execute bit on regular files; immutable image write checks return EROFS, while
directory access reflects the writable private layer. Native common tests,
actual UID0 behavior in an isolated user namespace and all four managed modes
pass. No generic host access or second path resolver is used.

## Significant progress: explicit private process refusal policy

The bound private process namespace now returns ENOSYS for creation/exec, EPERM
for identity/group/session mutation, and ECHILD for valid waitpid requests.
Callbacks preserve failed outputs and never invoke host process APIs. Native
fork/wait baseline and the separately specified managed denial policy pass all
four managed modes at attempt-t2f68dzt, including actual C function pointers,
argument evaluation, worker isolation and GC. Process execution support remains
open. An authored helper rename avoided an observed generic Path namespace
collision; that compiler defect remains recorded rather than claimed fixed.

## Significant progress: shared header identity and core consumer preparation

Core staging now uses one content-addressed header tree for every TU and stable
separate source paths. The prior 90-object run is retained as failed evidence:
per-TU physical headers changed anonymous signal-info identities. Corrected actual
two-object and 17-object links pass, and a consumer-only snapshot demonstrably
reuses objects with their original provenance. All 92 objects in the corrected
full profile emit, but full linkage now reports a distinct linger_linux aggregate
conflict between address.c and describesignal.c; investigation continues.

The separate ManagedCore library and owning JIT/AOT consumer runner are prepared
and snapshot-checked. It compares actual instruction/fault/exit rows with native,
uses a separate explicit-profile ABI oracle, binds private owners and verifies
memory accounting. Preparation is validated; complete managed core execution is
still unqualified and P1 remains open. Current failed linkage receipt:
objects/92682452e55497bb04f491b8938eaa6a6304c8c1a3073fc2558a6f34486f1576.

## Significant progress: real relative sleep and owned interruption

The new HostSleep boundary waits against a monotonic deadline and supports
explicit EINTR wakeups with normalized remaining time, without native signals
or retained C pointers. Native real delay/SIGALRM oracle and raw/optimized JIT/AOT
pass at host-sleep/attempt-avrdf_c1. Tests include long.MaxValue-second waits,
active disposal, invalid inputs, actual C function pointers, one-wait bounds,
two simultaneous independent owners and compacting GC. Guest signal delivery
and the stop protocol remain separate: upstream CheckInterrupt must see guest
state before a wake, and disposing a still-retrying guest owner is not a stop.

## Significant progress: bounded ancillary record walking

The missing CMSG_NXTHDR helper now performs pure bounded traversal of explicit
host control records. Native libc and authored algorithms agree across137655
valid-buffer cases; raw/optimized JIT/AOT and native ASan/UBSan pass. Overflow,
logical bounds and actual C function-pointer checks are included. Receipt:
host-ancillary/attempt-edgk8yzj. This supplies a retained ancillary.c dependency
without claiming ancillary socket transport, Unix sockets or descriptor passing.

## Significant progress: bounded file resizing and positioned writes

Private pwrite/ftruncate/truncate now share existing nodes, cursors and quotas;
growth is zero-filled, shrink releases quota and failed growth is atomic.
Positioned writes preserve shared cursor state, including the explicitly tested
Linux append behavior. fsync/fdatasync validate ephemeral in-memory files whose
writes are already committed; no persistent storage is promised. Native and all
four managed modes pass at host-file-updates/attempt-z_73a10u, together with
HostFiles/InstanceIo regressions, actual C function pointers and two-worker GC.

## Significant progress: genuine TLS storage and object-global repairs

Two-worker signal tests exposed shared explicit TLS arrays; the reduced linked
case then exposed line-wise deletion of repeated getter braces and attributes.
Both are fixed generically, preserving actual array declarations and complete
generated members. Native/direct/object-link GC regressions pass, and the full
repository suite passes a warning-free build,2238 units and520 functional cases
with1041 explicit skips. Compiler outputs are frozen for the next core profile.

## Significant progress: private signal registration and queries

A bounded per-worker disposition table now stores real default/ignore/simple/
siginfo records, action masks and reviewed metadata flags. Begin/End lifecycle,
old-action/query semantics and errors are explicit; native OS dispositions and
masks remain unchanged. Native/staged and raw/optimized JIT/AOT pass through both
direct emission and separate object linking, including actual callback pointers,
independent workers and GC (host-signal-actions/attempt-1qbw0fxz). Asynchronous
signal delivery and unqualified action flags remain outside this module.

## Significant progress: atomic private descriptor replacement

Real dup2/dup3 now reuse the existing descriptor table and description reference
counts, preserving shared cursor/socket lifetimes and independent CLOEXEC flags.
Native and raw/optimized JIT/AOT pass at host-descriptors/attempt-u1mzbw4k, with
full-table replacement, failed-target atomicity, C function pointers, two-worker
GC, last-socket port release and captured-stream redirection. Existing file/I/O
regressions also pass. No operating-system descriptor duplication is used.

## Significant progress: private exit callbacks and real cleanup order

atexit now registers real callbacks in a bounded per-worker table; explicit Run
invokes them in LIFO order, including newly registered callbacks, and detects
nested runs, slot/invocation exhaustion and context replacement. Escaping native
longjmp or CLR exceptions preserve an incomplete state requiring owner discard.
Native/staged and direct/object-linked raw/optimized JIT/AOT pass, including the
ABCA native callback trace, two workers and GC (host-exit-callbacks/attempt-_nkm9dcr).
Actual upstream cleanup callbacks must run after guest execution stops and before
IO, signal/memory owners or cached environment pointers are discarded. Immediate
exit/abort remains a separate policy. This does not qualify full core execution.

## Significant progress: corrected binding profile and normal cleanup integration

Socket tag macros no longer rename unrelated guest fields, and the embedding
driver sees the same binding preamble as upstream TUs. Opposite-order native and
all4 object-linked checks pass; actual address/describesignal/driver objects have
no aggregate signature conflicts and link. The owning driver now begins private
signal/exit contexts, runs genuine cleanup callbacks before owner teardown, and
the C# consumer binds its sleep owner. Consumer preparation passes against the
frozen snapshot; this is not complete interpreter execution evidence.

Current full95-source profile: generated/core-profile/attempt-wl47nc50.
Assembly receipt: artifacts/core/objects/7a3706a7340160c6c02d5b5b2f9111d2337b0b557fa75d08c65c820ec38db2d8/receipt.json.
All validated host bindings through exit callbacks and descriptor replacement
are included. The raw C# build will run only after complete successful linkage.

## Significant progress: actual private socket option queries

getsockopt now reads real type/reuse/buffer/no-delay values from the existing
private BCL sockets, with bounded short-output handling and unchanged failures.
Native and raw/optimized JIT/AOT pass at host-socket-queries/attempt-mheqynnb,
including actual option changes, callback pointers and two independent sockets
across GC. Unsupported levels/options remain explicit errors; no unqualified
SO_ERROR or ancillary option support is inferred.

## Significant progress: real private filesystem capacity

statvfs/fstatvfs now report actual immutable-image bytes, writable-byte limits
and usage, and node quotas/counts. Fragment size1 exposes exact byte accounting;
preferred transfer size4096, private fsid and capability flags are explicit.
Other path/descriptor/allocation limits remain independent. Native ABI/common
invariants and all4 pass at host-capacity/attempt-7y2zni_0, including growth,
shrink, failed quotas, output atomicity, cwd/duplicates, independent owners and
GC, plus existing HostFiles/InstanceIo regressions. No host disk is consulted.

## Significant progress: actual worker scheduling hint

The retained sched_yield call now invokes BCL Thread.Yield on the bound worker,
without promising a context switch or enabling guest thread/priority features.
Native and all4 pass at host-yield/attempt-rybwxwrz, including function pointers,
independent worker errno and GC. Existing upstream feature selection is unchanged.

## Significant progress: complete95-object link with the correct container

Every frozen object emitted and the exact complete set now links as BlinkCore.
The requested Blink containing class had collided with the preserved upstream
function Blink; no compiler or upstream algorithm repair was needed. Authored
bridge declarations select the full container through BLINK_FULL_CORE, with
unchanged default fixture declarations and method bodies. The first raw C# build
uses an explicit new consumer snapshot; successful linkage is not P1 execution.

## Significant progress: bounded private directory streams

Directory streams now snapshot real private node names/inodes/types and retain
internal descriptor leases. Failed acquisition preserves the fd; closing or
replacing a borrowed fd cannot cause a later stream close to close its reused
number. C tokens and records contain no CLR references, with explicit stream,
registration, entry and name-byte bounds. The supplied272-byte dirent profile
is measured separately from native Linux280-byte storage; components beyond255
bytes reject the entire acquisition before ownership transfer, without truncation.
Native/common and all4 pass at host-directories/attempt-wkfwdrzj, including an
upstream-style guest-record copy loop, snapshot/rewind, quotas, two-owner GC and
existing file/I/O regressions. Full guest Getdents execution is still separate.

## Significant progress: reproducible full consumer build

The consumer-only snapshot tool records each reviewed C# replacement and proves
that all C emission identities remain unchanged. Baseline snapshot
generated/core-profile/attempt-7cuxtttf retained its original Host project and
reused all95 objects; assembly ccc91fea6174948a084d8070b81d0844b68bfd5f7ba9d43366b58211e140f456
linked successfully with BlinkCore and explicit link options in its identity.
The first raw C# build, core-execution/attempt-68vow5of, then exposed seven
declaration errors: nested C tag/function collisions (B023) and repeated
per-object static-local identities (B024). The latter can also silently merge
identical storage declarations. Reduced generic repairs are being qualified
separately; this build is failure evidence, not interpreted guest execution.

The next consumer overlay, generated/core-profile/attempt-vhtbyjga, adds the
separately qualified capacity, socket-query, scheduling and directory bridges
with the current Host project. It again proves all95 C inputs unchanged. Future
fresh profiles include these four sources through the binding manifest. The
consumer binds its directory owner and releases its snapshots/descriptor leases
after translated main returns, before disposing I/O. This integration still
awaits the complete C# build and actual guest execution.

## Significant progress: actual native ELF loader seam

The unchanged LoadProgram path loads the pinned service twice with matching
entry bytes/program headers and actual guest stack argv/env/auxv/random data.
Receipt: loader-seam/attempt-ujcb9q9w. It extracts88 upstream sources, adding
argv.c, biosrom.c, endswith.c, loader.c and tainted.c to the83-source interpreter
oracle closure. Absolute paths require the owner's SetOverlays initialization;
the initial omission and SIGSEGV are retained at attempt-bly5yapz. No managed
profile changed, guest instructions ran, or malformed-ELF gate passed here.

## Significant progress: generic naming and object-storage repairs

B018/B023/B024 are repaired with native-checked reduced regressions. The full
repository run passes2239 unit and535 functional tests with1047 explicit skips;
build succeeds with17 analyzer warnings in unchanged existing test files.
Log: artifacts/repository-tag-static-path.log. Twelve affected upstream objects
re-emit with distinct local-static storage; a transparent mixed-producer replay
clears all seven original declaration errors and exposes355 later diagnostics
at core-execution/attempt-0imxwoxa. That replay cannot count as fresh canonical
emission or P1 execution. B025–B029 now record the concrete next repair families.

## Recovery resumed 2026-09-19

The interrupted work was saved in the user's partial_blink.patch, and recovery
starts from3f12b2e. The newer fixed-address global/TLS storage implementation is
preserved: the patch's old globals hunk and stale MsQuic receipt hashes are not
restored. User patch files remain untouched. Historical qualification receipts
do not count as validation of recovered source against this compiler.

Ownership: guest worker recovers B025–B028 and their reduced regressions; inputs
worker recovers libc string/math primitives and qualification; coordinator owns
host message/fd-set boundaries, source closure, integration and commits. Builds
and repository suites remain coordinated. The new object metadata contract
rejects old core objects, so the next complete core link requires fresh emission.
P0 remains complete; P1–P6 remain open until their actual runtime gates pass.

Fresh recovered HostFdSets qualification passes native and raw/optimized JIT/AOT
at host-fd-sets/attempt-0ko66tt9, including two TLS records and cached addresses
through compacting GC. The newer fixed-address compiler storage resolves the
earlier B031 failure without reshaping the authored record. Fresh private TCP
message/peer qualification passes at host-messages/attempt-o2m20b_6, including
actual short transfers, two-owner GC, cancellation and InstanceIo regressions.
Both boundaries are included in the next fresh binding profile. They do not yet
prove execution through the complete translated guest syscall paths.

## Significant progress: recovered generic repairs pass full regression

The recovered B025–B028 typing/literal/constant-branch changes and real generic
string/default-rounding primitives pass2257 unit and556 functional tests, with
1057 explicit platform skips and zero failures. Release build succeeds with17
existing analyzer warnings. Log: artifacts/recovery-isolated-repository.log.

A separate session began changing shared compiler files during recovery. To keep
qualification coherent while preserving those edits, subsequent campaign work
is isolated at /home/marius/p/dotcc-blink-campaign on blink-campaign-recovery,
based on11ea352. The recovered generic source is revalidated there. Reviewed
commits must be integrated without overwriting the other session's work.
All three original user patch files remain untouched in the original checkout.

The next fresh closure includes12 additional pinned upstream sources (including
the five actual ELF-loader dependencies), bringing managed additions to13.
Seven missing Linux LP64 tokens now have strict native and profile value/type
checks in tests/ProfileConstants/probe.c. These constants do not advertise the
corresponding unsupported operations. HAVE_REALPATH selects the already qualified
private HostPaths implementation rather than an unbound upstream fallback.
Full fresh object emission and actual managed guest execution remain next.

## Significant progress: exact private file timestamps

HostFileTimes implements futimens/utimensat over the actual private nodes with
exact nanoseconds, normalized negative epochs, NOW/OMIT/null modes, dirfd-relative
lookup, atomic pair validation, and read-only image protection. Explicit times
are bounded to years1–9999; NOW uses the clock provider's100ns quantum. The native
oracle confirms both-OMIT path bypass and descriptor-validation asymmetry.
Native and all four generated/runtime forms pass at
host-file-times/attempt-ddqcd8yp, along with copied HostFiles/InstanceIo tests.
The existing HostFileMetadata native/all4 matrix also passes at
host-file-metadata/attempt-a5vfkobj after timestamp-output normalization.
The binding is included in the next frozen full-core profile; no guest runtime
claim follows from this isolated host contract qualification.

## Significant progress: private resource and priority policy

HostResources reports the live InstanceIo descriptor ceiling and the actual
HostMemory owner budget, with immutable limits and explicit errors for unsupported
selectors/changes. AS/DATA report the private mapping budget, not process-wide
malloc or operating-system accounting. Priority is the immutable private
identity's zero priority; foreign targets and mutations fail explicitly.
Native ABI/error checks, the real C memory-owner getter lifecycle, and all four
generated/runtime forms pass at host-resources/attempt-de3l8ktb, including two
owners, actual descriptor/mapping exhaustion and compacting GC.
The unchanged upstream System.rlim initializes to infinity and bypasses host
callbacks for AS/DATA/NOFILE; connecting the owning limits at guest initialization
remains required before claiming guest resource-limit enforcement.

## Significant progress: explicit asynchronous-signal profile policy

The single-worker profile now exposes its disarmed interval-timer invariant,
allows idempotent zero disarm/query, and checks private process existence through
kill(pid,0). Attempts to arm timers, deliver asynchronous signals, or suspend
for signals fail explicitly without touching controller timers/processes/masks.
The private alarm error convention is documented as an extension, not POSIX
conformance. Native common checks and raw/optimized JIT/NativeAOT pass at
host-signal-policy/attempt-jp9pdo9u, including output atomicity, two identities,
function pointers, and cached TLS addresses across compacting GC.

Recovery commits f4eef67/e8a0c46/1323fc8 were integrated into the original branch
as7453d02, preserving the other session's committed a513309 changes and all user
patch files. A fresh combined repository suite is running there. The isolated
108-object profile remains frozen on its original compiler for reproducible
diagnosis; a513309 advances global-storage object format to2, so qualification
against that newer compiler requires a fresh emission, not old-object replay.

## Significant progress: private namespace, permissions and advisory locks

The shared filesystem foundation now supports atomic mkdir/unlink/rmdir/rename
with dirfd-relative paths, subtree/cwd rebasing, readonly image protection and
open-descriptor node lifetime. Detached bytes/nodes remain charged until the
final description closes. Permission bits and creation modes are real metadata;
per-owner umask affects new files/directories, chmod affects executable checks,
and chown validates the fixed private UID/GID policy. Unsupported mode bits and
mutations fail explicitly. Removing/replacing cwd is a documented EBUSY policy.

Private SH/EX/UN advisory locks follow node/open-description identity, duplicate
lifetime and native nonblocking conversion behavior. Contended blocking waits
fail explicitly before conversion; no controller file locks are used. Final
descriptor close immediately releases lock state before detached quota release.

Native plus raw/optimized JIT/NativeAOT qualification passes at
host-namespace/attempt-qu5h_znj, host-permissions/attempt-pskdga_g and
host-locks/attempt-5qgfx1tz. Namespace/permissions include existing HostFiles and
InstanceIo regressions. Both worker snapshots share exact VFS hash
feee5791dbd9d8c0a7ff8c321cfdb11041494482d35a571b53071f30bab80946.

The main workspace's combined compiler recovery now passes2257 unit and570
functional tests with1057 explicit skips and zero failures; log
artifacts/recovery-integrated-repository.log. The isolated108-source profile
attempt-7uqp6soy emitted and linked every object; its first raw consumer build
is running. Linkage alone still does not qualify P1 guest execution.

HostResources now also qualifies the actual childless-process accounting
invariant: RUSAGE_CHILDREN is zero. Self CPU usage and times explicitly fail
without touching outputs; neither controller CPU usage nor wall time is reported
as guest CPU usage. Native rusage/tms layouts and all4 owner/error/GC checks pass
at host-resources/attempt-z6tz96_q.

Fresh108-source consumer attempt-8z_7urui fails with27 diagnostics, all remaining
host/closure names. The recovered B025–B028 generic repairs clear their actual
full-core diagnostics. The frozen profile predates the newly qualified signal,
namespace, permission, lock and accounting bridges; the next profile includes
them. Anonymous pipes and link/symlink/FIFO/socketpair contracts remain open.
No managed guest instruction execution has yet passed.

## Significant progress: seed actual guest resource records

GuestResources seeds fresh upstream System AS/DATA/NOFILE records from the live
HostMemory budget and InstanceIo descriptor capacity before NewMachine. It refuses
nonfresh systems and cannot reset a guest-lowered maximum. Native actual NewSystem
and limit consumers plus raw/optimized JIT/NativeAOT pointer/owner tests pass at
guest-resources/attempt-7s2drlx5; independently compared System size3016, resource
record offset2704 and16-byte records agree.
The authored core driver now invokes this adapter before machine creation. Its
full-core wrapper includes the same binding preamble as upstream objects. Actual
guest get/setrlimit execution remains a later gate. DATA has no independent
upstream allocation enforcement, and generic malloc is outside HostMemory
accounting; the README records those limits and the unchanged upstream setter's
missing cur<=newmax validation.

## Significant progress: explicit ordinary-node and IPv4 profile boundaries

The selected filesystem continues to represent regular files/directories only.
Hard/symbolic link and named-FIFO creation validates private paths, owners and
existing entries, then fails explicitly without mutation; readlink performs an
actual lookup and distinguishes absent/bad paths from existing non-links. The
IPv4-only network refuses socketpair, with no descriptor allocation. These
unsupported-feature contracts are documented, not claimed as implementations of
general links, named pipes or UNIX-domain IPC. Capability macros select the
qualified mkfifo/mkfifoat boundary functions rather than missing fallback names;
they do not promise successful FIFO creation.
Native common and all four managed forms pass at
host-namespace-policy/attempt-n660fhgd, including exact metadata/quota/output
preservation, root escapes, invalid UTF8, owner disposal and two-owner GC.
An independent worker review found no blocking issue. The native negative
socketpair probe changed its output array on failure; the stronger private
output-preservation contract is deliberately checked separately.

## Significant progress: bounded private anonymous pipes

Anonymous pipe endpoints now occupy the real instance descriptor table, with
64KiB rings,1MiB aggregate storage and128 active-transfer bounds by default.
Writes through4096 bytes are atomic; EOF, EPIPE, nonblocking/short transfers,
dup/final-close state, FIFO metadata and readiness are real backend behavior.
In-flight operations retain their original ends across descriptor reuse, and
disposal cancels/drains blocked I/O and long readiness deadlines. Pipe pair
allocation and failure outputs are atomic. No OS pipe or native handle is used.
Native/common, raw/optimized JIT/NativeAOT and copied HostFiles/InstanceIo
regressions pass at host-pipes/attempt-73tfaaeh. The upstream pipe()+fcntl
fallback is supported without advertising HAVE_PIPE2. The final bridge and
Host snapshot are included in the next109-source full profile.

## Significant progress: independent CPU reference corpus

Twelve fixed-byte instruction cases match actual x86-64 execution against the
pinned native Blink interpreter at cpu-conformance/attempt-usdzucbq. The corpus
covers arithmetic flags, masked/edge shift counts, signed division and overflow
fault, retained SSE2 lanes, decode/data page boundaries and absent-page faults.
Both witnesses start from the same explicit inputs and preserve complete mapped
data plus raw register/flag/fault evidence; comparisons use only defined flags
and architectural fault state. The hardware witness executes the actual bytes
in disposable native test processes. This is native reference preparation;
managed conformance and P3 completion remain open.

## Validation infrastructure limit: malformed ELF corpus

An automated safety check stopped the worker preparing malformed-ELF reference
inputs. That work was not retried or counted as qualification; its incomplete
draft files/failed receipts remain preserved outside committed deliverables.
The worker switched to read-only integration review of ordinary valid pinned
ELF loading. Malformed-input coverage remains open; other implementation and
qualification work continues.

## Significant progress: owned-page software protection

The host memory contract now tracks NONE/READ/WRITE/RW per 4096-byte page and
charges/frees the metadata with its owned mapping. Range validation precedes
all protection changes; unowned/cross-mapping ranges fail without mutation.
Backing allocations remain ordinary RW data; the translated guest page tables
provide guest access checks. Executable host mappings remain unsupported.
This supports the actual loader's read-only segment backing and temporary
write/restore sequence without claiming OS hardware mprotect enforcement.
Native and raw/optimized JIT/NativeAOT pass at host-memory/attempt-voyktkm1,
host-file-mapping/attempt-mmr6w46m and host-diagnostic/attempt-lazd_317;
ASan passes at host-file-mapping/asan-o2y4be1f. These include quota, invalid
ranges, private file snapshots, GC/two-owner checks and affected I/O regressions.
The previous full-core profile remains frozen with its older HostMemory copy;
valid managed ELF loading is being qualified in a separately recorded derived link.

## P1/P2 milestone: actual complete translated execution

The uniform format2 profile attempt-gjk_ktle emits and links all109 source
objects at core/objects/e7c33225f0f91a793410b9483f597d230368f6a4ded5f8628e0e599f1fffe43d.
Core consumer attempt-ny3j_02m passes raw/optimized JIT and NativeAOT, with
whole-library trim roots. Actual arithmetic/memory state, bounded looping,
undefined-instruction and unmapped-address faults, exit37 and exit_group42
match the native interpreter exactly over two complete create/run/destroy cycles.
The ABI matches the independent configured native storage probe (Machine22576,
System3016); the untouched native signal jump buffer intentionally gives a
different total Machine size. The final process-owned memory accounting is
2 retained mappings/270384 bytes before final owner disposal; this does not
claim reusable independent owners within one process.

Raw and optimized metadata-only audits finish with5594/5584 method definitions,
4524 roots, zero traversed native imports and no unresolved tokens/errors.
The supplied generic runtime still declares17 native methods; declarations
are recorded separately from selected direct calls.155 indirect call sites,
virtual/delegate dispatch and framework implementation boundaries are explicit
limitations, not an isolation proof. No native emulator is referenced. The
recovery compiler regressions already passed2257 unit/570 functional tests
with1057 explicit platform skips after format2 integration.
P1 and P2 pass their stated execution/build gates; CPU/ELF coverage, service
startup, worker lifecycle, hostile-input qualification and Windows remain open.

The integrated CoreExecution audit gate was rerun at
core-execution/attempt-ymbkz80n: native/configured ABI, both direct boundary
inventories and all four executions pass. The runner snapshots and hashes its
auditor sources/binary, refuses incomplete/direct-native inventories and direct
process-exit/start/native-loader calls, and preserves the inventory limitations.
This fresh receipt qualifies the audit's placement before managed execution.

## Significant progress: managed CPU and valid ELF qualification

Twelve CPU cases now pass in the actual translated core in all four forms,
48 comparisons against fresh real hardware and native Blink:
cpu-conformance-managed/attempt-yvylj8k6. The separately provenanced link retains
108 original objects and replaces only the authored frontend. Full outputs,
flags masks, fault state and complete mapped test memory are preserved.
COVERAGE.md explicitly records missing floating-point, advertised-feature and
broader integer/SIMD coverage; this is not complete P3 qualification.

The unchanged pinned service ELF loads through the actual upstream translated
loader in all four forms at elf-loading/attempt-e1zyczdz. Each execution creates
two fresh machines and compares26074 file bytes,394960 zero BSS bytes,318 segment
permission checks, entry/PHDR, RW/NX stack, argv/env/auxv and cleanup against the
native witness. The derived link retains 108 baseline objects, replaces only
qualified HostMemory and adds the authored loader adapter, preserving the exact
canonical header paths and per-object producer evidence. No guest instructions
are executed by this loader-only check. Malformed inputs and service startup
remain separate open gates.

## Implementation infrastructure limit: service worker task

An automated safety check stopped the service embedding/worker implementation
task, reporting possible cybersecurity risk without a more specific reason.
The task was not retried. Partial src/Embedding/Embedding.c and
src/Managed.Emulation.Worker files are preserved outside committed deliverables;
none was compiled or executed. Actual translated service startup, the owning
worker and HTTP lifecycle gates remain unqualified. Independent CPU coverage
and generic controller/protocol validation continue; the worker switched to a
read-only review of that controller. The earlier malformed-ELF limit also remains.

## Significant progress: owning controller and bounded protocol

The independent managed controller now owns an explicit worker process,
configuration snapshot, source-generated bounded JSON protocol, readiness/result
tasks, cancellation, stop grace, deadline, output limits and asynchronous disposal.
Configuration is deep-copied and capped during serialization before spawning.
Redirected pipes are independently cancelled after forced stop or250ms after
root exit, so inherited handles cannot hang controller completion. Captured
stderr prefixes survive cancellation and output-limit termination.

Read-only review found and drove fixes for inherited-pipe drain hangs, mutable
configuration, oversized startup serialization and lost diagnostics. Actual
subprocess fixtures pass JIT and NativeAOT at
instance-lifecycle/attempt-p4b8juxr with zero warnings: graceful/forced stop,
deadline, crash, oversized incoming/outgoing frames, two owners, input mutation,
inherited pipe handles and exact retained diagnostic prefixes. These are
controller/protocol tests only. The blocked translated worker was not compiled
or executed, and P5 remains open. Detached descendant cleanup is explicitly
outside the current controller guarantee; the actual worker profile must not
create descendants.

## Significant progress: execution-time binary identity and publication inventory

The complete-core matrix was rerun at core-execution/attempt-38g8255n with
JIT consumer/dependency and NativeAOT binary hashes checked before/after every
execution. All four forms and both direct IL audits pass. The corresponding
Linux ELF publication inventory passes at publication-audit/attempt-ojn9i06e:
raw and optimized each declare libm.so.6, libc.so.6 and ld-linux-x86-64.so.2,
with254 undefined dynamic symbols and separate RX code/RW data segments.
No dynamic dependency is a native emulator, and no LOAD segment is both writable
and executable. This is a static publication inventory only: statically linked
code, later dynamic loading, framework internals and later executable mappings
remain outside its proof. P6 is still open.

## Significant progress: complete-core CPUID and floating-point diagnostics

The expanded corpus at cpu-conformance-managed/attempt-s62j5n5q completes
31 inputs in all four forms:124 comparisons,104 matching and20 failing. Every
managed row agrees with native Blink; the five repeated failures are actual
upstream FP defects, preserved in FP-FINDINGS.md and blocker B032. Observation
mode retains passed=false and is not a conformance waiver. The19 passing
instruction cases and seven CPUID leaves per form remain individually recorded.

The actual CPUID inventory decodes41 feature locations per form, preserving
hardware observations separately from the virtual CPU identity. Unqualified
optional advertisements are explicit reduction candidates; no feature-policy
change has yet been applied. Historical focused HostCpu evidence is retained
without presenting it as complete-core instruction qualification. The next CPU
task is a reviewed staged-upstream correction and stronger architectural
fault-state comparisons; immutable ref and generic compiler remain unchanged.

## Significant progress: freshly regenerated SQLite owning consumer

P6 cross-campaign regression now includes a new SQLite3.53.4 translation using
the unchanged campaign compiler. The original SQLite preparation script checks
its pinned source/header hashes; no historical generated assembly was reused.
Raw/optimized JIT/NativeAOT all pass the existing owning managed consumer at
sqlite-regression/attempt-1nqhp4wq: tables/bound inserts/joins/window queries,
WAL/reopen, JSONB, FTS5, optional math/percentile/metadata, managed callbacks,
function identity, forced GC and cleanup. The four transcripts agree exactly;
execution binaries, compiler, postprocessor and authored inputs are hashed.
This is the owning-consumer regression, not a claim that every dependent
campaign/corpus or Windows execution has passed. P6 remains open.

## Significant progress: freshly regenerated picotls campaign

The existing complete picotls Linux x64 campaign now passes with the unchanged
compiler after checksum-verified native preparation and fresh translation of
all nine selected source files. Raw/optimized JIT/NativeAOT pass at
picotls-regression/attempt-ymi5de8u, including ABI/provider/upstream/TLS suites
and 224 independent native-picotls/SslStream peer executions. The dependency
audit reports zero violations and zero missing entries. Exact tools, authored
inputs, generated files, binaries and nested receipts are retained; 11 tool
inputs and 110 tracked picotls files remained unchanged. This closes the picotls
regression row for Linux x64 only. MsQuic and other campaigns, Windows execution
and the remaining Blink runtime gates are still open.

## Significant progress: qualified staged scalar FP correction

A reviewed, hash-checked adaptation of cvt.c, ssefloat.c and the SIMD arm of
throw.c now passes 491 hardware/native cases and all 1964 raw/optimized
JIT/NativeAOT comparisons at cpu-conformance-managed/attempt-kenqm2yg. Scalar
CVT/CVTT rounding and masked/unmasked exceptions, COMIS/UCOMIS flags and sticky
status, and Linux SIMD signal-code priority now match the measured architectural
behavior. Fault comparisons retain destination registers, MXCSR, defined flags,
RIP and signal state. The original 31 inputs are preserved with 460 added cases.

The immutable reference and compiler are unchanged. The original native Blink
results retain 397 failures; no normalization converts them to passes. Staging
checks exact original/replacement/diff hashes and all three files reject builds
without DISABLE_JIT. The derived qualified link retains 105 original objects and
replaces the frontend plus three upstream TUs. This closes B032 only for the
reviewed scalar scope; the next full canonical profile will explicitly include
these changes and the separately qualified HostMemory revision. P3 remains open.

## Significant progress: Lua/chibi and WAT execution regressions

The unchanged compiler freshly translates the existing CI-selected Lua and
chibi sources in private scratch copies. Lua's complete `_U=true` upstream
user-test runner reports `final OK !!!`; chibi passes 1225 tests in 18 subgroups
and its full transcript matches the committed native baseline after only the
existing timing/ANSI normalization. Receipt:
language-regressions/attempt-vqeg8yjo. Native chibi bootstrap regenerates the four
required FFI stubs. Compiler, authored sources, generated outputs and executed
managed binaries are hashed. These are the existing Linux x64 JIT conformance
recipes, not AOT/platform or unselected internal C-API coverage.

The opt-in WAT execution oracle also passes 146 cases using installed wat2wasm
and Node: wat-zig-regression/attempt-4uflu_5h. The compiled compiler DLL matches
the campaign CLI's library hash. All 205 selected Zig oracle rows are explicitly
skipped because zig is unavailable, so they remain unqualified. Tool identities,
command, TRX case records and skips are retained. No generic compiler changes
were needed, and P6 remains open.

## Significant progress: complete Zig oracle and campaign runbook

The initial missing-tool skips are resolved using the repository CI's existing
Zig0.16.0 Linux x64 archive pin. The downloaded SHA256 is
70e49664a74374b48b51e6f3fdfbf437f6395d42509050588bd49abe52ba3d00;
the tool remains under ignored campaign ref/build paths. A first actual run
passed203 tests and retained two standard-library-configuration skips. With
the bundled library path explicitly configured, a fresh run passes all205
Zig differential cases, zero skips, at zig-regression/attempt-lmenyhyn. The
runner verifies the test compiler matches the campaign CLI, records binary and
archive identities, and rejects skipped cases as incomplete. Earlier setup and
skip receipts remain preserved. No generic compiler or library changes occurred.

The campaign README now gives the actual core reproduction path, focused test
entry points, ownership constraints and current unqualified product gates.
B030 was also freshly revalidated: a minimal native directory-call program
passes, while typed opendir/readdir assignments still fail managed compilation.
It remains a separate generic issue, outside the qualified private Blink bridge.

## Significant progress: fresh complete profile with scalar and memory corrections

Profile attempt-ngy_l10p emits and links all 109 sources afresh, with zero cached
objects reused. It explicitly includes the reviewed scalar correction and
qualified software-protection HostMemory/FileMapping sources; the original
profiles remain immutable. The staging recipe snapshots the reviewed module,
checks its exact diff/source hashes and JIT exclusion, and applies the existing
host-binding prefix afterward.

CoreExecution attempt-yufxocze passes native behavior/configured ABI and all four
raw/optimized JIT/NativeAOT forms. The independent CoreAbi attempt-mlv4tdgf
passes 236 measured rows in all four forms. Raw/optimized direct IL inventories
cover 5597/5587 methods with zero traversed native imports or errors; their
155 indirect and 594 virtual call sites remain explicit limits. Publication
audit attempt-tvcys8m0 passes against exact execution-time AOT hashes. The full
identity chain is core/canonical-scalar-integration.json. This is a new complete
profile checkpoint, not a change to the still-open service/platform gates.
Optional CPUID advertisements are unchanged here and are the next profile task.

## Significant progress: fresh MsQuic ABI, product and public consumer

MsQuic regenerates its 47 selected source units with the unchanged compiler.
Fresh host/core/TLS qualification matches 60 native observations under JIT/AOT;
public ABI matches 29 observations, and raw/optimized whole-library-rooted product
gates pass. The actual public ProjectReference consumer then passes all 32 cases
in raw/optimized JIT/NativeAOT: IPv4/IPv6 full and resumed connections, 65,537 bytes
each direction with FIN, wrong trust/name/ALPN rejection without application
data, actual source identity and clean owning disposal. Receipt:
msquic-regression/attempt-m_bkw5xt; public run-gv10e8sc.

The existing freeze recipe refreshes only config/product-closure.json after its
real ABI/build gates. Its status remains limited to that scope; the public
consumer receipt is separate, and full transport/recovery campaigns are not
claimed. The previous closure's 261 bound files were verified and archived.
Compiler/postprocessor and all 399 authored MsQuic/picotls inputs stayed unchanged.
Verified generated inputs and dependency receipts were copied into the main
checkout; replaced evidence is preserved under blink/artifacts/attempt-4e8gbkdi
there. This does not claim a separate execution in that checkout.

## Significant progress: canonical CPUID advertisement policy

The private profile now omits 13 unqualified optional feature bits and clears
thermal/power leaf6. This changes advertisement only; unadvertised instruction
handlers are not promised to reject. Focused qualification passes 16 queries
under eight native configurations and all four managed forms. The guest service
build preflight confirms baseline x86-64 flags without these optional features.

Profile attempt-7i4_ajz4 retains 108 identity-verified objects from the qualified
scalar/memory profile and freshly emits cpuid.c. CoreExecution attempt-n9ligxbc
passes native/configured ABI and raw/optimized JIT/NativeAOT; exact AOT publication
audit attempt-803iuwqv passes. The direct IL audits cover 5597/5587 methods with
zero traversed native imports; unresolved indirect/framework limits remain.

The CPU corpus passes 495 cases in each form, 1980 comparisons total, at
cpu-conformance-managed/attempt-jwuzr1go. It retains 108 canonical core objects
and replaces only the authored frontend. Native attempt-22ltifm3 agrees; the
original 397 upstream scalar mismatches remain preserved. An old-profile policy
mismatch is correctly rejected before emission. Identity chain:
core/canonical-policy-integration.json. Baseline RDTSC/FXSR/CX8/SSE2/system
instruction coverage and P3–P6 remain incomplete.

## Significant progress: all seven fresh SQLite C corpora

Fresh native and raw/optimized JIT/NativeAOT pass all seven existing SQLite C
corpora: core, API, VFS, virtual tables, allocation, upstream JSONB and FTS5.
Each corpus is emitted afresh from the pinned amalgamation using the unchanged
campaign compiler; every native/managed transcript exactly matches its committed
expected output. All 28 managed runs pass. Receipt:
sqlite-corpora/attempt-k83q6spa (SHA256
853b1307a9882b3f251ae57da2471af66bb81281e7d016df9f68f25a9b67130f).
The compiler/postprocessor and tracked SQLite sources are unchanged before/after;
executed binaries are hashed before/after. No IL/AOT or CS8500 warnings appeared;
other existing generated C# warnings are retained. Raw snapshots and command logs
are preserved. Reproduce with scripts/test-sqlite-corpora.py --cache <verified-
SQLite-archive-directory>, containing both pinned amalgamation and source archives.

Together with the fresh owning SQLite consumer, picotls, MsQuic, Lua/chibi, WAT
and complete Zig oracle runs above, this completes P6's dependent-campaign
regression checklist item on Linux x64. It does not complete P6: actual Windows,
service/fault, clean delivery and broader performance gates remain open. P3's
selected instruction-comparison item is also checked based on the qualified
495-case corpus; broader architectural coverage and the full P3 gate remain open.

## Significant progress: bounded real interpreter throughput

CoreThroughput retains 108 canonical objects and replaces only the authored
frontend with an eleven-byte guest integer loop. Native and all four managed
forms pass 75 timed samples (75 million interpreted instructions), with exact
register/flags/memory/canary/code/XMM/MXCSR checks after every batch. Setup, state
reset, checks and output are outside the dispatch timing interval. There is a
fixed 40,000-instruction warmup followed by three fresh processes of five
one-million-instruction samples per mode; no tuning from observed scores.

On this Ryzen7950X host, CPU31, .NET10.0.11, median million instructions/second
are native84.203, rawJIT5.500, rawAOT42.580, optimizedJIT14.676 and optimizedAOT
47.759. These describe one fixed hot loop, not service throughput or general
performance equivalence. Campaign builds were quiet; uncontrolled external
load, scheduling, frequency and tiering effects remain explicit. Per-process
wall time includes lifecycle costs and is not a startup-latency measurement.

Receipt core-throughput/attempt-h4zo2vxu has SHA256
ae0e7eaf1596221ebf89286d11983a55aa67546b0fe14f4a336cb583cf469763.
All 711 frozen artifacts and runtime identities match before/after. Independent
review identified and repaired incomplete-mode/provenance acceptance; nine
negative gate cases pass at core-throughput-controls/attempt-10wi772e. Earlier
preparations remain unqualified and preserved. Documentation now covers current
source/configuration/host/usage/validation and explicit unsupported boundaries;
P6's documentation item is checked, while its broad performance item stays open.

Clean-checkout reproduction is active from committed717ba66 in a new detached
worktree. Only the verified source archive was copied; no compiler, object or
native build output is reused. Installed host tools and NuGet cache remain
allowed and recorded. This independent task began after timing finished.

## Current phase complete: clean reproduction and primary-branch consolidation

The detached clean checkout at committed717ba66 passes the documented core
reproduction from an empty source/output tree. Only the pinned Blink archive
was copied. Its fresh Release compiler build passes with zero errors and
17 existing xUnit analyzer warnings; offline source verification and all25
native instruction cases pass. All109 selected sources emit/link afresh with
zero object reuse. CoreExecution attempt-xo_zqqig passes raw/optimized JIT and
NativeAOT, configured native ABI and direct IL audits. Exact-binary publication
audit attempt-ciqxzgs4 passes. The final checkout is clean and the original
campaign compiler/postprocessor stayed unchanged.

Top receipt: clean-reproduction/attempt-6pfwe3d4/receipt.json, SHA256
caabce37b242743a4586006c260d092f90aa10cd921b2fb8340ff37e51e091e6.
The original detached path is /home/marius/p/dotcc-blink-clean-mukw8i3k.
Commands, tool/package identities, prepared-state recovery and independent
verification are retained. The exact preparation payload was recovered only
after matching its previously recorded launch hash; recovery provenance is
explicit. Installed SDK/native tools and NuGet cache were shared, so this is
not a hermetic build. Timeout handling bounds direct children only; no timeout
occurred. Indirect/framework/runtime dependency limits still keep the bundled
P6 delivery audit item open.

Pending reviewed authored files and final documentation are now in the main
`sqlite` checkout. All campaign receipts were copied there; clean-tree artifact
copies are nested under this attempt's checkout-artifacts directory to avoid
replacing earlier native receipts. Historical unqualified partials were copied
byte-for-byte into artifacts/attempt-50_uy7vz/unqualified-partials, with a hash
manifest, while their original paths remain intact. The three user patch files
are hash-verified unchanged and unrelated libsmb2 work was untouched.

No new recovery-branch commit was created for this phase. Its worktree is kept
for evidence, not ongoing duplicate history. All worker jobs completed; the
coordinator and agents now stop as requested. P0–P2 are complete; this checkpoint
does not claim completion of the remaining campaign milestones.

## Resume: stable generated delivery on the current shared compiler

The user resumed Blink after libsmb2 stopped, on main sqlite at b671720. The
coordinator and two workers are active with disjoint delivery/sample ownership.
The Release build passes at
resume-current-compiler/attempt-9_82zbv2. Offline source verification and the
normal-only native core probe also pass. Prior shared regression and Blink runtime
receipts are preserved; no old object/runtime pass is relabeled as current.

## Current translation delivery and normal usage sample

The final delivery invocation passes at translation/attempt-rv0jhxuv, receipt
SHA256 8ce646a6bfc761d0b8a833450cd04ae5d7806861f761c93a965cd524d06d6037.
The first invocation attempt-g84jz4hn emitted all 109 objects afresh with zero
reuse; later invocations reused only matching current-compiler objects with
producer provenance. Native 25 pinned upstream cases, raw build, semantic
postprocessing and final build pass. The stable project is
`generated/TranslatedBlink/TranslatedBlink.csproj`; all 101 raw/final source and
project files match recorded manifests. Earlier successful runner versions are
archived alongside their receipts. Publication rollback and signal cleanup were
reviewed; no custom injected-failure tests were added.

`ManagedConsumer.slnx` consumes that actual final project. Its owning normal-core
sample passes Release build, JIT and whole-library-rooted Linux NativeAOT with
identical native/profile transcripts and ownership footer (2 retained mappings,
270450 charged bytes). Qualification attempt-0quh0jc1 has SHA256
508cfdaafeb285fa5183e0e5f4da89ea61a354796b76b6cf797cee2f05dddbff.
An initial NativeAOT publish failed NETSDK1047; the identical command passed on
retry without source changes. Both attempts are preserved; the cause is unknown.
The sample owns host binding/disposal and one translated driver call, not the
still-unqualified service worker, readiness, capture or HTTP API. P5 remains open.

Full current core/ABI/valid-ELF checks are underway. The clean-delivery runner is
reviewed but awaits a committed delivery revision before execution. No Windows
execution or current dependent-campaign revalidation is claimed.

## Current complete-core, ABI and valid ELF refresh

Delivery source is committed directly on sqlite as f38de7f. Current complete
core execution passes all four raw/optimized JIT/NativeAOT forms at
core-execution/attempt-8vbjtywv (SHA256
a28a474e7f4ad3bddc827a6ea683e4428462f5012e01926fc08f8129230c8c70).
Exact-binary publication audit passes at publication-audit/attempt-b0wppzl8
(SHA256 774f8a98273e39d4badea87fc8ac626153478c94a2536e76a414dbf1efbc03e2).
All 236 actual upstream ABI/register rows pass in all four forms at
core-abi/attempt-ezjynh7t (SHA256
6b583ae4f4b7d267cbe2b0954695de9eb56e450120b293cd3d5094f3836f0110).
Valid pinned ELF loading also passes all four forms at
elf-loading/attempt-r3eglgkk. This covers file bytes/BSS, permissions and initial
stack/argv/env/auxv over two loads and cleanup, without guest instructions.
The pinned ELF has no PT_TLS, so this is not TLS setup qualification.

Clean delivery is running from committed f38de7f in a fresh detached checkout;
only its pinned source archive is copied. Origin compiler binaries remain frozen.
The CPU worker is replacing custom trap completion instrumentation and selecting
449 normal cases, recording 46 historical custom fault cases as excluded. The
coordinator is narrowing HostMemory to ordinary lifecycle cases. Neither revised
harness has been executed yet; they wait for clean-delivery's heavy build slot.

## Active normal-scope qualification queue

The clean runner owns the heavy build slot at clean-delivery/attempt-0gfaz9m3,
with detached checkout `/home/marius/p/dotcc-blink-delivery-2nz9u2sm`. It targets
f38de7f; fresh compiler and native baseline pass and source emission is active.
`inputs` owns the frozen CpuConformance normal-only witness/selection changes;
its combined native/four-mode execution is queued next. The coordinator owns
HostMemory normal lifecycle and HostIo ordinary callback changes; their serial
refresh follows CPU. No new result for these modified harnesses is claimed yet.
The sample/clean worker may prepare additive tests/TlsLoading while monitoring
clean execution. Its bounded valid static TLS fixture must qualify startup copy/
zeroing, actual ARCH_SET_FS and FS-relative access, independently of any service
worker. It must wait for the same build slot before executing. Shared compiler
and canonical Blink profile/host sources stay frozen; user patches are preserved.

## Clean public delivery workflow passed

Clean delivery at committed f38de7f passes at clean-delivery/attempt-0gfaz9m3,
SHA256 4aaeca06fb629779914d24969e60de2fedcff1d456193f3494f2185107f1143b.
The detached checkout `/home/marius/p/dotcc-blink-delivery-2nz9u2sm` starts with
no compiler/generated/native outputs and copies only the pinned Blink archive.
Its fresh compiler build, all 109 freshly emitted sources (zero reuse), native
25, raw build, semantic postprocessing, final project build, solution build and
actual JIT/NativeAOT sample all pass. Both sample transcripts match the normal
native reference/configured ABI, with footer 2 mappings and 270450 charged bytes.
The standard AOT publish succeeds on its first clean attempt. Child delivery
attempt-8bxczpb2 has SHA256
f516f3a6e2c9d6fdd7e913c379a79ec4264462ba0c2084d2b5db0b1bc7ed0a08.
The final checkout is clean; original and fresh compiler identities remain
unchanged. Independent coordinator verification checks receipt links, raw/final
manifests and recorded execution binary equality. Installed tools and NuGet cache
are shared, so this remains a clean-source/output reproduction, not hermetic.

The added P6 public-workflow item is complete for the normal-core sample. P5's
service API/worker and broad platform/dependency/performance gates remain open.
The CPU normal matrix now owns the validation slot; standalone normal memory and
I/O follow. Additive valid TLS fixture source preparation continues without
builds. Neither historical excluded cases nor Windows passes are inferred.

## Normal-only CPU matrix passed

The revised independent hardware witness returns normally after capturing
registers/flags/XMM/MXCSR, replacing the authored INT3 completion sentinel. The
unchanged corpus retains stable IDs; 449 normal cases are selected and all 46
custom fault cases are explicitly excluded, not passed. Native and managed
runners verify identical selections, exact coverage and implementation hashes.

Native attempt-7j03qmvz passes all 449 selected comparisons against hardware or
explicit virtual CPUID policy, SHA256
b33297b3c0ba375772763b274f8236dd042bdc8fa6e292ebe0206776dbb85c73.
Managed attempt-3r1msqyr passes all 1796 comparisons (449 in each raw/optimized
JIT/NativeAOT form), with native agreement and SHA256
4692e7760c37899215cec4025fcd116fd0eeeb22ddd3c47a320067d6a0e75ff0.
The reviewed staged scalar floating-point correction remains required; 359
original-native mismatches are retained separately, not relabeled as original
upstream passes. Eleven CPUID rows verify the selected virtual policy. Independent
coordinator checks confirm complete unique mode/case pairs and current source
identities. This bounded corpus does not close the remaining instruction-family,
TLS, guest-memory or Windows gates listed in the coverage inventory.

Normal HostMemory then HostIo now own the serial validation slot. The additive
valid TLS fixture is authored/reviewed but awaits execution; its source presence
is not a pass. The shared compiler and canonical profile remain unchanged.

## Current normal host-memory and file-I/O refresh

Normal HostMemory passes staged native and raw/optimized JIT/NativeAOT at
host-memory/attempt-16k81wpo, SHA256
0e5dd6bb725edc507ff58989ad82edeba00a6d3776c9a2a32af68f19c17d5bbb.
Cases check aligned/zeroed backing, valid cross-page copies, protection metadata,
additional live allocations, unmap, slabs, disposal/reinitialization and distinct
host-thread allocations retained through compacting GC. Forced exhaustion,
invalid arguments, foreign mutation, unsupported modes and out-of-lifetime calls
are historical exclusions. This qualifies the authored boundary and staged
InitMap, not every guest page-table algorithm or a hardened sandbox.

Normal HostIo passes native and all four managed forms at
host-io/attempt-f61wdrei, SHA256
f48c848dbedea45a5edf045aa6cd37406e197e7bc3eca92260009a07cc806531.
Cases cover descriptor duplication/shared cursors/close, vector and sparse I/O,
128 KiB transfer loops, captured streams and private concurrent owners. Ordinary
missing/existing-file errors remain; invalid-descriptor and unbound-owner calls
are excluded. Actual guest service startup is not part of this standalone gate.
Both receipts identify the unchanged current compiler and postprocessor.

Valid TLS now owns the build slot. Native Linux, native Blink CLI and the native
loader/execution adapter already pass the fixed valid fixture; four managed forms
are still pending. Shared regression runners are being audited read-only for
current scope and compiler-rebuild hazards before further execution.

## Valid static TLS startup passed

The additive fixed valid TLS fixture passes Linux hardware and native Blink CLI
internal assertions, plus exact native-adapter versus raw/optimized JIT/NativeAOT
state comparisons at tls-loading/attempt-4arxvilg. Receipt SHA256:
459a13a7df40990e692f0e23ba115758da144f28231850c513839ea7aea6c7d9.
The fixture hash is eac738a2b874ebf6bf0dc51b3b4138c3ee5914abbf1f85b4c31ab4f5b4a1519b.
The adapter observes the loaded PT_TLS header (8 file bytes, 16 memory bytes,
8-byte alignment), writable/nonexecutable runtime storage, FS base 0x401010,
initial value 0x1122334455667788, updated tail 9, sum 0x1122334455667791,
exactly 25 completed instructions and normal trap -10/status 0.

Startup explicitly copies/zeros the private TLS block and calls ARCH_SET_FS;
the loader is not credited with allocating runtime TLS. This fixed positive-
offset FS layout is not a general libc TCB, dynamic TLS, guest-thread or service
startup qualification. The link records 107 retained exact objects and reviewed
HostMemory/TLS-driver replacements. Original native archive/profile and staged
managed source derivation remain distinct. All source/profile/compiler and
execution-binary hashes are stable; coordinator and second-worker reviews pass.
Together with valid pinned-image loading, this closes P3's bounded ELF/TLS item;
CPU-family and guest-memory coverage still keep the whole milestone open.

SQLite's ordinary owning consumer is now being regenerated and tested on the
current frozen compiler. The corpus wrapper needs an explicit normal-suite
selection before reuse: allocation and VFS suites contain custom injected
failures. Their exclusion is recorded as scope, not a pass. The remaining shared
campaign gates are still pending current execution.

## Current SQLite regression passed; new integer findings

SQLite's owning consumer passes freshly regenerated raw/optimized JIT/NativeAOT
at sqlite-regression/attempt-71n3t9tf (SHA256
a01d0de82fc7e47d12e331df27e6a0648f25b6d09b92cdb7fbf278c789204dd5).
The revised corpus wrapper passes five native suites and all 20 managed forms at
sqlite-corpora/attempt-f3xzr2_h (SHA256
07f2f661c3a087d396e3ab4334670a6bc3c15899a64e7c2145de64f670d1e9c4).
Selected core/api/vtable/upstream/FTS5 transcripts and source/tool identities are
exact; allocation and VFS mixed suites are explicitly excluded because they
contain custom injected failures. Ordinary SQL/API errors remain covered.
SQLite authored files and the shared compiler were unchanged.

Seventeen appended normal CPU cases exposed three inherited native mismatches
before managed execution. Preserved native attempt-cjel3vz8 has SHA256
75f0c3a7bccc23e6b014116d8430a0a3a849637f82ed0fb5228fcf7048150812.
INC32/64 lose AF on nibble wrap (the pinned helpers compare with unused y=0),
and CMPXCHG8B nonmatch preserves upper register bits instead of zeroextending
EAX/EDX. Defined hardware comparisons remain unchanged. The 31-byte FXSR
roundtrip case passes natively, including changed/restored MXCSR and cleared
saved-image bytes. Managed expanded execution did not start after the native
failure; previous 449-case receipts remain bounded historical evidence for those
exact cases, not a pass for the expansion.

The worker is preparing separately pinned integer source adaptations and native/
managed proof flags, adding INC8/16 coverage for all repaired widths. Coordinator
profile integration is pending validation; immutable upstream and generated C#
remain untouched. Picotls's scoped regression owns the build slot meanwhile;
its native preparation and raw copied/ABI/55-peer JIT/AOT checks pass, with
optimized forms pending. No shared compiler change is needed for these inherited
Blink algorithm defects, so current dependent-campaign evidence remains relevant.

## Current scoped picotls regression passed

Picotls normal-subset attempt-fx7grn1c passes (SHA256
8d2a80a7a1bfb6c57f09427b94f4043b83430b6e1e2e9ef4e90babe4109a360e):
pinned native upstream oracle, fresh current-compiler translation, copied-source
and 92-field actual ABI checks in all four forms, and 55 normal/authentication
peer cases per form (220 total). Ordinary wrong-credential/name/ALPN outcomes
remain part of runtime error handling. An exact AST-verified derivation removes
only the forced unoffered-ALPN server call; the original peer script is unchanged.
Mixed ProviderVectors/TlsTests/UpstreamVectors targets are excluded explicitly
because they include custom injections/crafted records/additional truncations.
No old full-campaign PASS or dependency audit is consumed as current evidence.

Exact ordered suite/peer inventories and final tool/authored identities pass
independent review. Copied/ABI executions record binary before/after identities;
the unchanged peer runner lacks that per-execution detail, so its archived final
binary hashes are not promoted into stronger proof. Resume only permits the
preparation-only state; failed matrix logs are preserved. The shared compiler
and picotls authored files remain unchanged.

The integer source correction/native qualification is next; no build remains
active from picotls. MsQuic's existing wrapper is being audited read-only for
scope and closure-cache compatibility before any new execution.

## Reviewed integer correction: native qualification passed

UpstreamInteger stages four INC auxiliary-carry corrections and CMPXCHG8B
nonmatch zeroextension from pinned original sources with an exact checked-in
patch. Expanded normal CPU coverage now selects 468 of 514 cases; the original
495 and 512 descriptor prefixes remain pinned and 46 custom fault cases stay
excluded. Native attempt-lc9j96ag passes all 468 comparisons (SHA256
0a58ffb7f3a083709795ac1b86a93bff1fb44420f35690614b9227aefdbc2e41),
retaining 364 original-native differing rows as evidence. All four INC widths,
CMPXCHG8B success/nonmatch and the FXSR restoration case are covered.

Canonical profile integration is authored, but current published managed
products predate these corrections. Next: regenerate delivery, qualify core and
publication, then run the expanded 1,872 managed comparisons and refresh the
valid ELF/TLS and clean consumer delivery. B033 remains open until managed
qualification; shared compiler binaries and dependent-campaign results remain
unchanged.

## Corrected canonical delivery and core runtime passed

Delivery attempt-w78n0i5u passes the complete 109-source closure with 107
identity-verified reused objects and fresh alu/machine emissions (receipt SHA256
46bddf20dd127cc7a72ca88da13863edcc17b0cde1dcf08b4cfa74347f7d71c9).
All 101 immutable raw files and 101 final files, plus command log hashes, were
independently rechecked. Profile attempt-7byl9v1e retains the exact reviewed
integer boundary, and stable TranslatedBlink now contains the correction.

Core execution attempt-yzck8kpr passes all four actual runtime forms; publication
inventory attempt-e0l80fe1 passes for those exact AOT binaries. These inventory
checks retain their direct/import-only limits and do not establish service or
sandbox qualification. Expanded CPU execution now owns the build slot. Next
after it passes: valid ELF/TLS refresh and a clean public consumer delivery from
a committed correction revision. No generic compiler or libsmb2 source changed.

## Expanded normal CPU all four runtime forms passed

CPU attempt-sgren8zu passes 468 cases per runtime form, 1,872 total, with all
defined hardware comparisons unchanged (receipt SHA256
e470675d116c505f377eff74671f709bac6c0b28a366646e5f6e1c94ffdf23d7).
Independent review rechecked exact counts/matched results and implementation/
staging identities. The derived CPU frontend retains canonical alu/machine
objects and integer boundary d7d7816ae9ae74f9fda094cd178c831cc009cbb3563f6c5fcc241ef69fbc0729.
The 46 custom fault inputs remain excluded; original native differences remain
preserved. B033 is closed for the reviewed INC AF/CMPXCHG8B width correction.
New valid shifts/division and actual FXSR XMM/MXCSR restoration also pass.
This does not close broader CPU family, service or Windows requirements.

The worker now refreshes valid ELF and explicit TLS against the same corrected
assembly; clean public consumer delivery follows. MsQuic and language wrappers
are reviewed and committed but remain runtime-pending, with unchanged shared
compiler binaries and no libsmb2/user-patch changes.

## Corrected-profile valid ELF and TLS refresh passed

Valid ELF attempt-qclmm2fz passes native and all four managed forms (SHA256
3c4bacbbdc886e045263a6be34f8db7cf4cabd3a436fb6b487d8824f24a93f6b),
using corrected assembly 03789a247c2d723303c55875538c4b46e1bb0d8aa298e5061c001ea3f8bf7511.
Explicit valid TLS attempt-m6fpvl1m also passes Linux/native assertions and the
native-adapter/all-four-managed exact state matrix (SHA256
c6f37c66870e00ae8c30178be850eff8755a2202b8fb942eb441a4780ef4cfdf).
PT_TLS 8/16/8, preserved program header, private RW/NX storage, FS=0x401010,
initial value/update/sum, 25 instructions and exit0 remain exact. Fixed fixture
limits remain: no general libc TCB, dynamic TLS or guest-thread qualification.
No malformed image or injected fault case ran. Next is a fresh clean checkout
public delivery/sample run, followed by the scoped dependent regressions.

## User priority: finish P3 before additional P6 work

The in-flight clean delivery at 497ce69 will finish; no additional throughput or
dependent-regression run starts ahead of unresolved P3. Prepared P6 wrappers
and past passing receipts are preserved. P3 already has 468 normal cases per
managed form, corrected integer semantics, valid pinned ELF and explicit TLS
startup, but those results do not substitute for the remaining checks.

Inputs owns a new actual upstream GuestMemory lifecycle harness: allocate guest
page tables; reserve initially zero mappings and grow the live mapping set; copy
valid buffers across page boundaries; change RW to read-only/NX and back with
metadata checks and only permitted accesses; unmap/remap and verify zero/refill;
then verify cleanup and repeated valid ownership. This must retain and execute
upstream algorithms in native and all four managed forms. Standalone HostMemory
callback tests are supporting evidence, not a substitute.

The CPU worker owns a bounded gap review and explicit case list for remaining
defined flags/shifts, packed SSE2 lane/move/shuffle behavior, valid addressing/
stack/REP effects and baseline advertised instruction contracts. It preserves
existing descriptor prefixes/exclusions and uses invariants for nondeterministic
timestamps. The coordinator reviews the finite list against the selected profile
before source freeze; this is not a request for exhaustive ISA coverage. No new
custom fault or malformed-image case is permitted. Runtime work stays serial.

## Corrected clean public delivery passed; P3 owns the active queue

Clean attempt-ru82jd14 passes at commit 497ce69 (SHA256
813f4071e96f83eb60aedd7e3d5bf8ab8133d59bae9db31a8c82a6e7b6fa5a95).
Detached checkout /home/marius/p/dotcc-blink-delivery-m263q57j is preserved. A
fresh compiler, pinned native 25, 109 fresh source objects with zero reuse,
immutable raw/final manifests, solution build, and actual JIT/rooted-AOT sample
all pass. Child delivery attempt-1bf9hpx3 has SHA256
85d73d467c174d95f0920ec36503203ce4428402c381252ce6e86b587f91fe66.
The checkout is clean; original compiler identities and execution binary pairs
remain unchanged. This qualifies the corrected normal sample, not the blocked
service-worker or Windows target.

The build slot is now released exclusively for the reviewed P3 CPU and actual
guest-memory qualification work. Both workers are authoring their bounded cases;
native and all-four-managed runs follow source review. Additional P6 regression
and throughput scripts remain prepared/committed but execution is deferred.

## P3 actual guest-memory lifecycle passed

GuestMemory attempt-gbyg6j74 passes native and all four managed forms (SHA256
3e6289d234e4ad96bf9e26162671142648c759c5deef05ead92a57fec251cb02).
The probe executes retained upstream page-table/mapping algorithms and replaces
only the authored frontend: 108 canonical objects, including HostMemory, remain
unchanged. Independent review verifies all command logs, execution binaries and
retained objects against the receipt.

Both cycles match ten deterministic native rows: initially vss2/rss6, grown
vss5/rss10, protection metadata and allowed data preservation, remapped vss5/
rss10 with zero/refill and neighbors preserved, then vss0/rss5 (page tables remain
until FreeMachine frees the orphaned System). Managed retained host-pool count
and bytes stay identical across both cycles and within the live owner budget.
Final disposal is invoked once; no post-disposal translated call or unobserved
post-disposal zero claim is made. No forbidden access/fault is injected.

P3's memory checklist item is now passed. The final finite CPU32 expansion
(native500 then managed2,000) is the remaining active P3 gate. P6 extras remain
held; the already-qualified clean delivery is preserved.

## P3 native expansion exposed NEG AF defect

Native500 attempt-hmylb6uo completes with 499 matches and one NEG8 minimum-value
AF mismatch (receipt SHA256
ed8badd8f2400f05e13f3fbee247a8d11c3b49b5b711c3e3e10c0e0c7fe5ad70).
Managed500 did not run. B034 records the inherited formula error shared by all
four width helpers. The CPU worker owns the exact staged correction and four
focused width/nonzero-nibble regressions (504 selected); inputs independently
reviews it read-only. The coordinator will regenerate the single changed ALU
producer and qualify integration after corrected native passes, then run the
managed expansion. All other P3 cases/evidence remain intact, and no unrelated
P6 suites or custom fault cases are added.

## P3 NEG correction passed native504

Reviewed stage/patch plus four focused width/AF-set regressions pass all 504
selected native cases at attempt-ovjovt6b (SHA256
798f5c3eb0a01dbcfe5135331ab746931895ee1bc1e56e511538af5ff0fdec33).
The original546 descriptors remain unchanged; 46 custom fault inputs remain
excluded and 368 original-native differences are preserved. RDTSC invariants
pass independently in hardware/original/staged native captures.

The coordinator now regenerates the single changed canonical ALU producer,
qualifies its core integration, then releases the final 2,016 managed CPU
comparisons. Guest-memory and loader results remain preserved; no shared
compiler change or unrelated P6 execution is needed for this repair.

## P3 NEG canonical integration passed; final CPU matrix active

Delivery attempt-xxngak_0 passes with 108 verified reused objects and one fresh
ALU producer (SHA256
38ca60466b15424d33daa14e60d7aab8f69d974ef47d3cbb018da41717790a93).
Independent comparison confirms alu.c is the only changed canonical object;
all 101 raw and 101 final file hashes verify. Profile attempt-5yrh5owk carries
the reviewed NEG staging boundary.

Core integration attempt-273a6hks passes all four managed forms (SHA256
d9ae7c6c2d068998d7911b5009933827cdbdf87b77494161566df4707ddf2cba),
and exact publication inventory attempt-og66g378 passes (SHA256
1775711976bd51d8ba2326af9954e94dd49b5412f623691797ebf1d6f0d8af3d).
The final CPU worker now runs 504 normal cases per managed form (2,016 total)
against this qualified canonical profile. B034 stays open until those actual
comparisons pass. Existing guest-memory/loader objects and evidence remain
unchanged; broader P6 work is still held.

## P3 selected-profile milestone passed

Final CPU attempt-disfjyq2 passes all 2,016 comparisons (504 in each of four
forms); receipt SHA256
`e3b4a964d69e0bced3d2896ea093f66c535008709fbd196318bc0fe7b99aa72e`.
Fresh native child attempt-27pjwx09 passes 504 and preserves 368 original-native
differences; SHA256
`0e9557adcbebe0bca31ea6109ce9fa3889279abb4c18f88876d4ae8440ccdffa`.
All RDTSC invariant lists are empty. All 2,016 ownership footers report two
mappings and 270,450 charged bytes. Coordinator verification rehashed all
implementation, compiler, raw source, output binary, 108 retained producer and
command-log identities and checked the exact receipt links and comparison counts.
B034 is closed for its reviewed four-width NEG correction and finite regressions.

Independent integration review compared canonical assemblies 03789a and 14c483:
only alu.c changed; all other 108 producer records and Host/header/compiler
inputs match. The source and normalized emitted-object differences contain
exactly the four NEG AF/CF body corrections, with no ABI/layout/global changes.
This retains the prior successful GuestMemory/ELF/TLS evidence without claiming
those tests were reexecuted after NEG. Together these satisfy the selected P3
CPU/ELF/memory gate. Broader ISA, general TLS, service integration and Windows
claims remain outside this pass. All workers are idle; no new suite started.

## P4 actual guest-I/O native reference passed

GuestIo now executes the real two-byte SYSCALL dispatcher for open/read/write,
dup/lseek/readv/writev/poll/close, with 128 KiB exact content, cross-page iovec
and payload marshalling, shared cursors, reopen and two complete lifecycles.
Native receipt `guest-io/attempt-q_w37qqy` SHA256
`e7c24b3d04e5a64e73e8f560cfdbf46ad8ad4243a9a6f142c221433ea17aefef`
passes its ten exact state rows. Initial compile-only fixture const diagnostic
is preserved in attempt-2zh_09gz; no warning suppression or compiler change.
The managed matrix is now active using the exact canonical 14c483 profile
and 108 retained objects. No mutable cancellation bridge edits are mixed into
that snapshot. The independent cancellation change remains source-only.

## P4 guest filesystem/descriptor subgate passed

GuestIo full attempt-tnq5itfa passes native plus all four managed forms with
exact ten-row output; SHA256
`15795abe9f6ead804b60eac9c3f996807f3d4f244c10da8515b583146f3c825c`.
Every 128 KiB file byte is checked, including vector overwrites and reopen;
readiness is 5, duplicate cursors are shared, upstream fds return to zero and
only the three private standard descriptors remain. Two cycles preserve the
bounded retained memory pool. Coordinator rehashed closed logs, all executed
binaries, current templates and 108 retained canonical objects. The original
compile-only failure remains recorded. P4's ordinary filesystem/dup/short-I/O/
cleanup checklist is now checked; no service or cancellation inference follows.

Next serial validation is the independent I/O token propagation. Inputs prepares
normal actual guest clock/entropy/thread-ID/private signal-state qualification;
no signal delivery, timer, worker or service loop is introduced.

## P4 I/O callback cancellation implemented and qualified

Four authored bridges now propagate an explicitly bound cancellation token
through asynchronous byte/vector/message I/O, accept/connect and readiness.
The original one-argument BindHostIo signature remains as a forwarding overload;
unbind clears owner and token. Existing positive partial writes remain successes.

All four forms pass 22 translated-C scenarios plus a direct BCL pipe-read
reference in host-io-cancellation/attempt-blvv0ho4 (SHA256
`24085e30571d06b3fb41bdc3020cb960bcf708a5926c8323c0000b18a5065f6a`).
Native ordinary ABI/vector/poll smoke passes separately. Exact source, closed
logs, execution binaries and tool identities were reverified independently.
No shared compiler/generated-source repair was made. The final generated
product now needs its authored Host snapshot republished and affected core
execution checked; this is integration of the concrete P4 bridge change, not
reopening P0–P3.

This remains ECANCELED at the callback boundary. Guest Poll maps it to POLLERR;
sleep and execution-loop stop still need their own integration. P4's full
stop/service-startup conditions remain unchecked. Inputs prepares actual guest
environment/state tests; the consumer worker reviews a bounded TCP syscall
fixture with no ELF, HTTP, service or worker loop.

## P4 cancellation bindings published into the canonical product

Delivery attempt-16eer8t5 passes with 109 verified reused C objects and the four
updated Host bridges; SHA256
`259b8c3f3e5c8e9b6687b231defbed68d50ac4aa8926dd6b9844e07cc6f808c7`.
The stable TranslatedBlink project and immutable raw/final manifests were
independently rehashed. All 109 object hashes equal the preceding 14c483 set.
New assembly 734288 (profile attempt-kviofky_) has receipt SHA256
`779514d915d56d58a531e390a1d0a143d67bbde14d496e473429d321b689bdf4`.

Affected normal-core attempt-g8r4tp0k passes raw/optimized JIT/NativeAOT, ABI and
direct boundary checks; SHA256
`45e49ad508405e15142389d94244f61e9098b69e86a1f405eb98ff859582e8eb`.
No C source/compiler change, new P3 corpus or P6 publication/performance suite
was introduced. Earlier exact publication/clean-run receipts keep their original
binary/revision identities. GuestEnvironment native qualification is released
against this new snapshot; GuestTcp remains in source preparation.

## P4 native guest environment/state reference passed

GuestEnvironment executes actual clock_gettime/getres, getrandom, set_tid_address,
rt_sigaction and rt_sigprocmask with valid mapped records across two lifecycles.
Native attempt-aeqsjnzi passes; SHA256
`e746ff4a1a90cb15c02d8b423d3552ea20ca661ddcc6a74555e8c8782a66f925`.
Twenty raw rows retain actual timestamps/random bytes/tid/flags; ten invariant
rows check normalized clocks/execution wall brackets, monotonic nondecrease,
32-byte random return/canaries, virtual tid/pointer state, signal IGN/query/DFL
and exact mask restoration, cleanup and no pending/delivered signal. Actual
host disposition is queried so an ignored upstream registration error cannot
produce a false pass. No entropy-quality or cross-process timestamp equality
claim is made. No source fix was required. The managed matrix now uses the
new 734288 canonical Host snapshot; bounded GuestTcp remains source-only.

## P4 guest environment and signal-state subgate passed

GuestEnvironment attempt-emy2577e passes native and all four managed forms against
canonical 734288, retaining 108 objects and replacing only the authored frontend.
Receipt SHA256: `45d7a0080dfac1664a95c9bb269f80a3318192483396213ecb4c25ee619f7706`.
Each execution retains ten raw observations and compares ten invariant rows
across two lifecycles. Clocks satisfy normalization, wall brackets, monotonic
nondecrease and provider resolution; random bytes are retained without equality
or quality claims. Actual guest thread-ID/ctid storage, signal disposition
registration/query/restoration, private mask state and cleanup pass without
signal delivery. Sources, 108 retained producers, logs and execution binaries
were independently rehashed. No implementation repair was required.

GuestTcp has passed its native reference and is next for managed qualification.
Standard streams and the observed terminal-query path are the last bounded
contract gap under design review. Owning execution stop/deadlines and actual
service startup remain separate unqualified P4 conditions.

## P4 finite actual guest TCP contract passed

GuestTcp attempt-8r2oj_2k passes native and all four managed forms with an exact
five-line state transcript; receipt SHA256
`ad31914a360345f527ae55fcff7dcb669b8e8b86b706971953585e447549a03d`.
Actual SYSCALL dispatch covers blocking IPv4 socket/bind/listen/accept, endpoint
queries, setsockopt/getsockopt, ready/nonready poll(0), recvfrom/sendto, orderly
shutdown/EOF and close. The independent native/BCL peers send 257 exact request
bytes and validate 263 exact response bytes. Cross-page payload/address buffers,
canaries, private guest versus published physical endpoint observations, empty
guest fd/page state and private host descriptors=3/pending socket operations=0
pass. All 108 retained canonical 734288 producer identities, source snapshots,
logs and binaries were independently verified. No implementation fix was needed.

This is one finite exchange, with no guest ELF, HTTP, service or execution worker.
It does not qualify blocking guest poll cancellation. The final independent P4
contract fixture covers AddStdFd, exact standard-stream capture and the observed
ordinary terminal query. Actual service startup and owning execution stop remain
unqualified; no P5/P6 task has begun.

## P4 native inherited-stream reference passed

GuestStreams attempt-wl8t5nv8 passes its native reference with actual AddStdFd
registration and SYSCALL read/writev/write/ioctl across two lifecycles; SHA256
`5825dd864319ef666939d6bf2cebfb6bae68a8462c89391b1dc1270f7c2fa6cb`.
Frozen input is 34 bytes, stdout 46 bytes and stderr 14 bytes. Exact captures
and a separate bounded diagnostic report pass, including ordinary TIOCGWINSZ
ENOTTY, unchanged valid buffers, six inherited fd records, two guest metadata
cleanups and three surviving underlying standard descriptors. This deliberately
does not close fd0..2. Managed qualification is released against canonical734288.

## P4 contract work finished; full gate remains blocked

GuestStreams attempt-21z_o_3s passes native and raw/optimized JIT/NativeAOT;
receipt SHA256 `0f62de4b0ffed1af3c25a4a9b09863cd8ba098ad790912fd173d455be3c81104`.
Each form consumes 34 exact input bytes, captures 46 stdout and 14 stderr bytes,
and produces the same separate diagnostic report for two lifecycles. Actual
AddStdFd registration, cross-page read/writev, stderr write, ordinary terminal
ENOTTY with unchanged buffer, guest metadata cleanup and surviving standard
descriptors pass. The coordinator independently verified 384 identity entries
and all capture/report bytes. All 108 retained producers use canonical 734288.
No source repair or generated-source edit was required.

P4's selected host-service contracts, ordinary filesystem/lifecycle tests and
individual audit of the pinned service's 18 observed syscall names are qualified.
Callback cancellation passes 22 translated scenarios in all four forms and is
published into the final generated project; it is not actual guest execution
stop. Upstream Poll converts callback ECANCELED to POLLERR, and nanosleep retries
without an observed guest interrupt. The owning execution/deadline integration
and actual translated guest service startup therefore remain unqualified.

The prior automated service-worker task rejection reported a possible
cybersecurity risk without a more specific reason. That action was not retried,
recovered or renamed. Independent normal contracts are now exhausted for this
bounded P4 phase; no fixture is substituted for the rejected service runner.
P4 remains incomplete at this concrete blocker. Worker assignments are finished
and no P5/P6 work is started. All significant progress was committed directly on
`sqlite`; user patches, libsmb2 work and historical artifacts remain preserved.

## P4 resumed by explicit user direction — 2026-09-20

The user requested "Try to continue P4". Existing native/all-four contract
receipts remain qualified. Inputs owns a persistent host stop/deadline owner,
cancellable sleep and exact staged syscall safe points; the guest worker owns
the minimal same-thread execution driver and actual pinned ServiceFixture
startup. The coordinator owns canonical profile integration, source review,
serialized builds/tests and milestone commits. No P5 controller/protocol or
two-instance/restart expansion is included.

Source review found that Poll with zero descriptors never calls CheckInterrupt.
The proposed adaptation adds an explicit outer-loop stop check as well as the
normal CheckInterrupt boundary. Both return EINTR normally so syscall lock and
scratch cleanup can complete; the owning loop checks the persistent host reason
before and after each interpreted instruction. No guest signal is fabricated.
Implementation/qualification is pending; this design is not a gate pass.

## P4 C# ownership and literal-pool source integration prepared

The product link now excludes the C core-probe frontend entirely. The native
oracle remains a test input, and CoreExecution derives a separate test-only
frontend link from the pure product objects. Actual ownership/loading/execution
will be a separate C# consumer over existing public upstream functions/types and
the public numeric jump-buffer exception transport. No production C execution
wrapper is selected. A narrow C# TerminateSignal callback supplies the frontend
ABI and forwards only the event to its bound consumer.

Reviewed HostExecutionStop and cancellable HostSleep preserve existing binding
APIs, latch one stop reason and use persistent cancellation. The pinned staged
CheckInterrupt/Poll adaptation returns normally through syscall cleanup. The
product and canonical links now request --literal-pool explicitly; the flag is
part of the hashed link identity. This fixes a delivery omission rather than
changing postprocessing semantics. Source/manifest syntax checks pass; fresh
generation and actual runtime qualification are next, with no new pass claimed.

## Direct-source delivery correction prepared

At the user's request, the active TranslatedBlink project links authored bridge
files and the original Host project under `src`; the solution also references
that original project. Private immutable raw/profile copies remain archival
qualification inputs only. The postprocessor receives a private frozen project
for semantic context; only generated transformations are retained. The final
project is rebuilt at its real path against original authored sources, whose
hashes must remain unchanged. Failures restore the prior generated directory.

The preceding generation attempt-0b2hbt7s was explicitly interrupted with SIGTERM
during assembly to apply this correction before publication. Its failed/interrupted
receipt and completed immutable object producers are preserved for reuse; no
pass or published product is claimed for it. Revised generation is next.

### P4 direct-source publication retry

The consolidated-layout delivery `translation/attempt-re4ic3ef` reused all 108 verified C objects and passed raw build, semantic postprocessing and the postprocessed build. Publication stopped before replacing the stable output: restoring the private bridge snapshot also inherited its read-only directory mode, preventing removal of that temporary context. The pipeline now makes the private restored directory writable; the immutable raw snapshot and original authored sources remain unchanged. This attempt is a preserved failure, not a delivered-product pass.

### P4 product delivery refinement — actual build passed

At `1d602e4`, `translation/attempt-4yjaed1_/receipt.json` passed (SHA-256 `bb4bfb42e64035f75cca237fff60fd6d6ca4c0b20c13f4d963e32c2cef589d54`). All 108 selected product objects were verified and reused. Raw build, semantic postprocessing, restored-authored-context build, and actual published direct-source build passed. The product has 34 original authored Compile/Link items plus the original Host ProjectReference, no active Host/Bridges copies, no CoreProbe/test main/C execution driver, and one 13,569-byte pinned translated narrow-literal pool; runtime literal storage remains separate. All authored and final manifests were independently rechecked.

The real `ManagedConsumer.slnx` solution also builds with zero warnings/errors using the separate authored C# execution owner (`managed-consumer/attempt-3c0qzs2p`). This is compilation evidence only. The next gate is actual pinned service startup through that owner, followed by the ordinary stop/deadline matrix and the sample runtime checks. P4 remains open.

### P4 first actual translated service execution and consumer delivery

The separate authored `Managed.Emulation.Execution` API now owns initialization, static ELF loading, the instruction loop, budget/stop checks and same-thread cleanup using exported upstream functions/types. The minimal managed TerminateSignal binding and reviewed syscall stop checks are boundary adaptations; no C execution driver enters the product. Caller-owned IO/stop state remains alive until Run returns.

The first raw-JIT service gate passed all six existing pinned native HTTP cases, including the 131,160-byte large response, real READY/STOPPED captures, normal exit status zero, and final memory-owner release. Receipt: `guest-service/attempt-pu9_o0z7/receipt.json`, SHA-256 `9875d127a520eec4d86184fcea03c38960535d4a7ef484ee7393056535cee6e1`. This is explicitly one mode, not a four-mode matrix pass. Its lifecycle runner was subsequently tightened before the full matrix, preserving that earlier receipt.

The actual delivered ManagedConsumer solution passed compilation, JIT and rooted Linux x64 NativeAOT health/normal-stop execution, exact output and empty stderr (`managed-consumer/attempt-3c0qzs2p/receipt.json`, SHA-256 `6a13ccf38f409879ae170305aac97f18c1346bc0cc0c56a944a0e49bb26ae70a`). Execution binaries were hashed before/after each run, and original authored/final source hashes were rechecked. P2 architecture/layout refinements are satisfied; this does not complete the P5 subprocess/multiple-instance API.

Next: complete all four actual service forms, then qualify the 11 ordinary completion/stop/deadline cases in each managed form against eight native completion witnesses. Stop fixtures are authored and reviewed but have no runtime pass yet. P4 remains open; no P5/P6 phase has started.

### P4 actual service startup — all four forms passed

`guest-service/attempt-18nn8vfz/receipt.json` passes (SHA-256 `a9283cb6bdc87aa83a64fa1f4c19ea2bb02b6640aef60684b8c88e4502b60c4e`): all 24 exact native request/response comparisons in raw/optimized JIT/NativeAOT, each with 1,725,554 completed instructions, guest exit zero/halt -10, no signal/stop, exact captured READY/STOPPED and final memory-owner release. Direct IL/publication inventories passed within their documented indirect/framework limits. Independently rechecked 635 frozen inputs, execution binaries and every request/response hash.

Earlier `guest-service/attempt-bzu3jva5` remains a failed fixture-report attempt (receipt SHA-256 `ae89e8e1d4c715514e3642f30afd3d1faa33008e81edd7b2e31d554cb3a73d4e`): NativeAOT disabled reflection-based JSON. Explicit closed-schema writers fix both qualification harnesses, without product/compiler changes.

Stop validation `guest-execution-stop/attempt-geaqvgvp` stopped before managed builds when the pinned native profile rejected guest pipe creation (receipt SHA-256 `3812ae97af000f8492061eb41e260e2cdcaefcc7974f7edc5f432372b7c59579`). Source review confirmed pipe/pipe2 are excluded by the selected dispatcher. The reviewed replacement uses ordinary guest read on a real inherited stdin pipe; normal controls preload 0x5a, cancellation keeps the writer open and observes an actual pending read. All 11 case IDs/44 managed runs remain selected; no profile semantics were changed. This final stop/deadline gate remains pending.

### P4 completion — selected Linux x64 profile

Final stop receipt `guest-execution-stop/attempt-kl7r74np/receipt.json` passes (SHA-256 `aacfeae66a3d1eb23cee6194147d2451667e06f3bfd0dc74cee223bd5579443f`). Eight native completion controls and all 44 managed cases pass: four controls, requested CPU/zero-descriptor-poll/sleep/inherited-read stop, two actual wait deadlines and exact 128 budget in each mode. First reason, absence of fabricated guest exit/signal, normal syscall cleanup, owner release, pending-operation drain and final zero descriptors/pipe bytes are checked. Independently rehashed 1,038 frozen inputs, 68 command/log/binary records and 44 observations.

The affected test-only core derivation also passes native/all-four execution, ABI and direct-boundary checks (`core-execution/attempt-_qybvbye`, SHA-256 `916f582095a05edbd3d72f2a4ead4e421c5b7aee929fcaeef6a6c0a1ba3a4dd2`). It retains 108 canonical product producers and adds the probe solely in its private test link. No product test frontend was restored.

All P4 checklist and actual service-startup gates are complete for the selected Linux x64 profile. Existing source/indirect-call and isolation limits remain explicit. The coordinator and workers stop at the user-requested boundary; no P5/P6 phase, new cases, Windows claim or later campaign is started. User patches and historical evidence remain preserved on the main `sqlite` checkout.

### P5 authorized — actual NativeAOT guest preparation

At `66d0060` the user explicitly requested the next phase and imposed a bounded
musl-build condition. Installed SDK10.0.111 and Podman/Docker are present; no
`musl-gcc` is on PATH. Two disjoint worker tasks are active: genuine C# service
source/ordinary NativeAOT publish and read-only actual profile/container audit.
No guest build or translated-execution pass is claimed yet. The existing P4
C fixture and host NativeAOT receipts are not NativeAOT guest evidence.

### P5 real NativeAOT guest — native build and reference passed

The ordinary Linux build succeeds without musl/container work. Genuine C# HTTP
fixture `tests/DotNetService` uses pinned SDK10.0.111/runtime10.0.11, invariant
globalization and normal runtime threads/GC. Final receipt
`dotnet-guest/attempt-5130ydrk/receipt.json` SHA-256
`0c5841150df27f83e0aa49d4225b84dde1cc041caff1f0de16ea14d1ce1bd2ac` records
exact native health/stop replies, READY/STOPPED, exit zero and empty stderr.
The 1,553,192-byte ELF SHA-256 is
`87985a99c01e395468de19e0fae5f8c907dbcf50ad08cf4fb07613aeda124be4`.
Source/config, binary and all19 artifact hashes were independently verified.

The actual ELF needs ld-linux/libc/libm and TLS24/288/8. Native strace confirms
real Finalizer/Sockets/SigHandler threads plus futex/epoll/pipe2/signal and
filesystem probes. A static musl build would not remove the thread requirement.
Current profile disables guest threads/futex and seeds a64MiB guest address-space
limit; the observed GC virtual reservation is much larger. These are explicit
compatibility gaps, not compiler failures or translated-execution passes.
Next is an actual bounded compatibility probe with a private dependency closure
and a reviewed opt-in C# interpreter-image allowance. No native fallback or
success stub is used. Details: [P5-NATIVEAOT-GUEST.md](P5-NATIVEAOT-GUEST.md).

### P5 actual dynamic NativeAOT guest — first compatibility result

The real guest executes through the translated dynamic loader, then fails before
readiness after 18,791 completed instructions. The private image contains only
the pinned guest and individually hashed ld-linux/libc/libm files. C# ownership,
instruction budget, deadline and joined cleanup remain in use. The owner now
supports an explicit interpreter opt-in with static-only default, plus immutable
exception-boundary scalar register diagnostics; no generated source was edited.

Initial receipt `dotnet-guest-execution/attempt-twmyfw31/receipt.json` SHA-256
`7ac6b0e12d77ba9ca2ca3f3a8421f6bed0871c99a4eeabb59eaf87c7a1987a5d` and
follow-up `attempt-046ir0z5/receipt.json` SHA-256
`eee2ab47b268fa9fd83582660eb32dc28c2c71a2a3e46e6d3124dcfe50b9faa7` preserve
the contained mmap panic/host exit request 250. The latter captures actual mmap
arguments `(0, 2170256, 1, 2050, 3, 0)` at IP `0x110000025d2c`, matching glibc's
initial libc image span. That span includes 11 whole pages beyond the file;
HostMemory currently rejects those pages instead of making them readable.
The exception errno follows diagnostic output and does not identify the original
allocation errno. No HTTP cases or translated NativeAOT pass are claimed.
Independently checked 123 frozen inputs and 117 prepared files for the latter.

Next bounded task: one ordinary static-musl build using the separately pinned
official SDK image, to remove this dynamic-library dependency shape. The inputs
worker owns its isolated runner/profile; no pull/build has happened yet. Stop
for user direction if that build is very difficult or impossible. The genuine
threaded-runtime requirements remain regardless of libc. The exact syscall and
thread-ownership audit is [P5-SYSCALLS.md](P5-SYSCALLS.md). P5 stays active; no
P6 work has begun and the old raw baseline is not new-compiler qualification.

### P5 static-musl NativeAOT guest — ordinary build passed

The one approved standard attempt passes in
`dotnet-guest-musl/attempt-8za50rji/receipt.json`, SHA-256
`8876eaf0cda1c0cc9ec81e163a9293745caa436215dec90790ff6f57acce8505`.
Pinned official Alpine SDK 10.0.401/runtime 10.0.12 produced a genuine static
x86-64 ET_EXEC with no interpreter or dynamic dependencies from the unchanged
C# service source. Pull took 64.65 seconds, publish 17.39 seconds; no retries,
installs or workarounds were needed. Exact native health/stop HTTP and exit zero
pass, with the named container removed and recorded identities rechecked.
Guest ELF SHA-256:
`b8fc2c2ba465ded0349c46ecd332dc361ebd0d8c7265b17938adc7e79eac82b3`.

The user-directed difficult-build stop condition did not occur. Next is the
same bounded translated diagnostic using only this static guest in its private
image, preserving default static-only execution. The guest worker owns that
fixture refinement; the inputs worker records the exact musl trace contracts.
No translated NativeAOT pass, threaded profile or P5 milestone completion is
claimed yet. No P6 work is started.

### P5 static guest startup — first required runtime boundary identified

Static diagnostic `dotnet-guest-execution/attempt-_772un12` executes 139,230
instructions then exits -1 through the normal guest exit trap, without HTTP
readiness. Memory-owner release and thread join complete. A repeat with bounded
C# syscall observations reproduces the identical count, status and IP:
`attempt-w6n64zmg/receipt.json` SHA-256
`ac264c5e2ee12364fba52a8cb099eed4578744a945d8363d1e457b754783d838`.
All 21 observations fit the trace bound. QUERY membarrier returns ENOSYS, the
runtime allocates a fallback page, mlock returns ENOSYS, then cleanup and
exit_group(-1) follow. This is a failed guest run, not a service pass.

Pinned runtime source `95017c711e6afc1085133d440e42b4bd78155701` confirms this
initialization chain and the required private-expedited query/registration.
Next implementation is an explicitly single-guest-thread membarrier boundary:
query mask 24, owner-scoped registration and an actual BCL memory fence. The
staged syscall must reject builds enabling guest threads/fork until a coherent
multi-thread barrier replaces that contract. No mlock success or host pinning
will be fabricated. Inputs owns the host/bridge and normal qualification;
guest owns the narrow staged dispatcher/header; coordinator owns profile and
C# owner integration. P5 remains active and P6 remains held.

### P5 private membarrier boundary qualified

Normal native Linux and raw/optimized JIT/NativeAOT qualification pass in
`host-membarrier/attempt-a4b_894o/receipt.json`, SHA-256
`48a786c8bc411ad91afd27b94054e1da443f010e94971b511038632d729b6205`.
QUERY advertises only mask 24 after probing the actual BCL
`Interlocked.MemoryBarrierProcessWide()` fence; registration is owner scoped,
and two normal fences preserve the expected values and errno. This is a real
process-wide fence, not a fabricated mlock success. Guest thread lifecycle
remains unqualified; staging rejects HAVE_THREADS/HAVE_FORK and requires the
selected single-thread interpreter profile. The exact dispatcher composition
source receipt is `upstream-guest-runtime/attempt-lauasdlc/receipt.json`,
SHA-256 `57599f4df64d1b065008baf670f247aff08ba57481e1be86b9f38a8a761ba456`.

The manifest, profile composer and C# owner now include this boundary. Product
regeneration is next, with current compiler identities and unchanged authored
source references, followed by the actual static NativeAOT diagnostic. Boundary
qualification does not claim runtime startup or HTTP readiness. P5 remains
active; no P6 work has begun.

### P5 current product and next actual runtime requirement

Fresh delivery `translation/attempt-wd_77mip/receipt.json`, SHA-256
`a9af0c02fa4dc7c4bcc576968fa26f41250a187f877bb943ea429dee039032ac`,
passes all108 fresh producers with zero reuse, native25, raw/postprocessed and
final direct-source project builds. All35 original bridge links and original
Host project remain active; authored hashes are unchanged. Current warning
suppression and shared literal pooling are present.

The unchanged static .NET guest now passes actual membarrier QUERY24 and
registration0, then reaches `clone(0x7d0f00, stack, ptid, ctid, tls)`. The selected
profile returns ENOSYS; the runtime unmaps its stack and exits -1 after151,283
instructions, without readiness or HTTP. Receipt
`dotnet-guest-execution/attempt-xyn4u0iw/receipt.json`, SHA-256
`3c02bc464ff04ad6fa9ac6d95e2e1fbb270063ba22cbdabc0e90613010e2502f`,
records40 untruncated syscall observations, ordinary exit trap, no managed
exception, joined execution and final memory release. Diagnostic build has zero
warnings/errors. This is progress past the former barrier failure, not a guest
pass.

A separate native-only control of the exact static ELF passes with supported
GC settings `DOTNET_GCHeapHardLimit=1000000`, `DOTNET_GCRegionRange=2000000`,
`DOTNET_GCRegionSize=100000` (hexadecimal16/32/1MiB). Receipt
`dotnet-guest-gc-profile/attempt-lp8kf_q3/receipt.json`, SHA-256
`f145da71d5032421fdd40bd368d248cad4918065b0b07ee545e32772fab7bb55`,
preserves exact HTTP/exit0 and real3 helper threads; observed PROT_NONE reserve
is33,558,528bytes. No whole-process memory or translated execution claim follows.

Next: genuine managed guest-thread ownership and shared memory/lifetime. Generic
BCL pthread mutex/condition primitives are reusable, but the guest Machine/System
ABI, C# child loops, stop/join, TLS and futex ownership need explicit qualification.
Guest worker owns source-only staged thread/ABI design; coordinator owns C#
execution integration; inputs worker finished the native GC witness. The stable
single-thread product remains intact until a new profile qualifies. P5 remains
active, P6 held.

### P5 shared memory-barrier registration qualified

`HostProcessMemoryBarrier` now shares explicit registration across attached
execution threads, retains one actual BCL capability probe and fences the host
process on each supported operation. The existing single-thread implementation
retains its behavior behind the same typed interface. New normal native and
all-four translated two-worker qualification passes in
`host-process-membarrier/attempt-04hsv8u9/receipt.json`, SHA-256
`14996bc97e7800a89a4f1aa97881f7d7575be551d1009ada83ccf099057bfb94`.
It verifies registration on the creator, fences from two other attached threads,
explicit ordered value42→52, join and attachment disposal. This qualifies the
shared boundary, not guest clone/futex or the threaded product. Guest thread
staging, synchronized shared HostMemory and process-scoped signal/exit state
are being implemented in disjoint worker-owned files; no threaded profile has
been enabled or claimed to execute yet.

### P5 threaded host ownership prerequisites qualified

Shared HostMemory now has generation-token contexts, attachment accounting,
creator-only final disposal and a real pthread registry lock. The original
single-thread branch remains selected by default. Native legacy/shared and all
four managed forms pass two-worker/two-cycle private-file, mapping, protection
metadata, accounting and detach/destroy checks:
`host-shared-memory/attempt-if0_xhrl/receipt.json`, SHA-256
`82ace64abe504f9216e77483081de36a7c27ab00116926c4c6f1f73e9fc14bc8`.
Three earlier fixture header/inclusion failures are preserved; no compiler or
generated repair was used. Actual native pthread context charge136bytes differs
from managed100bytes; semantic comparisons retain that explicit ABI distinction.

Threaded-only process signal dispositions and exit-callback registries also
pass native plus all four managed forms. Two actual workers observe shared
actions and ordered callback registration; callback reentry produces B,C,A
exactly once after joins. Receipt `host-process-state/attempt-o9f8h7kl/receipt.json`,
SHA-256 `24e8ae53f5ae1a9b472c27d13797b5234f6fc6e0d2d63cb2203a6f5c40b5210d`.
Locks do not cross arbitrary callback invocation. Old TLS behavior remains in
the disabled-thread branch.

The separate, hash-pinned thread stage and header overlay are source-reviewed
and layout/type checks are exercised by these fixtures. Whole Machine/System
threaded layouts, clone/futex execution and C# worker lifecycle remain pending.
The coordinator's new C# threaded owner is under review and not yet compiled;
it is not part of the stable delivery. P5 remains active with no service pass.

### P5 threaded profile emission exposed and repaired block-static TLS

The first separate threaded profile (`generated/threaded-core/attempt-first`)
retains108 upstream/host producers and no C execution frontend. Its first frozen
assembly emitted101 objects and rejected7 valid `_Thread_local static` declarations
in assert/debug/errno/signal/log/page-format/trace sources. Receipt
`core/objects/184ebabf76ba382b877d9e132d8506d66e32494f86e99671d147feb9e90c54f0/receipt.json`
remains a failed initial attempt; no generated source was repaired.

The compiler now accepts adjacent TLS/static specifiers in either order and
lowers block-static scalars and uninitialized arrays into existing per-thread
pinned storage with lexical/object-link names. Nonzero scalar initialization,
initialized TLS arrays and unsupported block storage remain explicit errors.
Focused TLS plus compiler lowering tests pass232/232; four actual two-thread,
compacting-GC functional cases pass direct/object linking for file/block storage.
Logs are retained under `artifacts/compiler-block-tls`. An initial unit assertion
about unrelated undeclared-name diagnostics was corrected to a valid lexical
shadowing check; its failed log is retained.

Next is fresh threaded production with the new compiler, followed by actual
layout and normal clone/futex/TLS ELF qualification. The C# threaded owner and
separate prototype delivery helper are still unqualified source work. Stable
single-thread delivery and existing P4 receipts remain unchanged. P5 service
execution has not passed; P6 remains held.

### P5 complete threaded source link and normal native thread witness

With the TLS repair, all108 threaded producers freshly emit and link with zero
reuse/failures. Assembly `core/objects/ab4bf1703cddc0d53f908e86297d8aa8efbc2d6dea46728867bcbfa1ce57fbdf/receipt.json`
has SHA-256 `449a0bd09cfda81d504d84bb6ff02b8137b0b6a12ed8d7f5472394bdbb6c74b5`.
The first raw C# build (`threaded-delivery/attempt-jz5_6z8e`) then failed on two
discarded atomic boolean results emitted as bare casts and one unbound private
`raise` reference. That failed build is preserved; no product publication occurred.
The compiler statement renderer now discards those values while preserving the
real atomic operation. Atomic/lowering tests pass229/229 and the new executable
byte/word compare-exchange/flag fixture passes; two opt-in external oracle cases
were skipped. Logs remain under `artifacts/compiler-atomic-discard`.

Separately, the valid static assembly thread fixture passes on Linux and a fresh
pinned Blink built with threads enabled, preserving the old native baseline.
It verifies distinct TIDs, isolated FS/TLS, shared bytes, actual private futex
handshakes, non-private clear-TID waiting and ordinary child/group exits. Both
executions produce the exact50-byte transcript with empty stderr and exit0.
Receipt `guest-threads/attempt-b_de41tq/receipt.json` has SHA-256
`32b9cc19b5b9ca72574ee0a9a8c34852d3c155c44935357e7022268096427c82`.
The release handshake took the ordinary EAGAIN race; no claim that both handshake
waits blocked is made. These are native witnesses, not managed thread execution.

The private raise binding follows the existing unsupported asynchronous-signal
policy; it must never invoke a native host signal. Fresh threaded generation
with the atomic fix, final C# owner compilation, actual ABI layout and four-form
guest thread qualification remain the next gates before the NativeAOT service.

### P5 separate threaded library and C# owner build

The corrected threaded profile builds raw, postprocessed and final direct-source
projects. The current-machine export is a narrow authored C# accessor for the
actual generated TLS field; execution and lifetime remain in the separate C#
owner. Its accessor-only derivative reuses all108 verified C objects and links
successfully. Delivery `threaded-delivery/attempt-fxbxjsio/receipt.json` has SHA-256
`0c1422e85054902e4caff93e8e6e9ff07096b90fdbdc65c262fb0136a493f5de`.
The experimental project is `generated/ThreadedBlink`; stable delivery is retained.

`src/Managed.Emulation.ThreadedExecution` builds with zero warnings/errors against
that project. Owner receipt `threaded-core/attempt-thread-accessor/owner-build.json`
has SHA-256 `c74371f7f9d7b3d39b3282d7a81a4b80f6278c123b5d56b35a71ea26c5f21b63`.
It owns per-Machine C# threads, shared memory attachment, group stop, actual joins,
child-TID clearing and cleanup. A reviewed execution-outcome latch prevents a
later teardown deadline from relabeling successful guest exit. It retains live
backing on incomplete teardown and requires process discard. Positive pthread
signal notification remains explicitly unsupported. These are build results;
actual ABI and managed clone/TLS/futex qualification run next. The .NET guest has
not yet passed translated service startup, and P6 remains held.

### P5 threaded ABI layout qualified in all four forms

The actual threaded profile now matches a GCC native oracle using the explicit
managed storage policy across169 size/alignment/offset/width/stride rows,
include-order checks and bidirectional C/C# field sentinels with a compacting GC.
Native and raw/optimized JIT/NativeAOT all pass. Receipt
`threaded-layout/attempt-8zznigqf/receipt.json` has SHA-256
`fcc4d2f11ce27d6535ee500cb7db356f6c72c61dc7452cc5c6a2f687b25c1765`.
The test retains108 exact core objects and adds only three layout TUs. Native-only
compiler intrinsic declarations are pinned to their runtime source definitions;
no measured upstream structure or pthread/signal storage definition was changed.

Three earlier runner failures remain recorded: requiring absent root global.json,
missing GCC declarations for dotcc intrinsic types, and a missing private Host
reference assembly before postprocessing. The final attempt records24 successful
commands,768 frozen inputs and147 snapshot files, all reverified with execution
binary identities. This closes the internal threaded ABI gate, not guest-thread
execution. The first managed clone/TLS/futex launch built but stopped before
execution because its strict process-group check found a retained SDK build
process; a fresh run with build-server reuse explicitly disabled is next.

### P5 normal threaded guest executes in all four managed forms

The exact native-qualified ELF now passes raw/optimized JIT/NativeAOT through the
separate C# owner. Receipt `guest-threads-managed/attempt-xh97uv94/receipt.json`
has SHA-256 `1043a108e58bbed55ac8080bc10867b62dd8c7c4910b266aaa8e0eb79c584096`.
Each fresh process creates distinct main/child TIDs, verifies private FS/TLS and
shared bytes, completes actual futex handshakes and child-TID clearing, emits the
exact50-byte native transcript, and exits both threads normally. All Machines
release, all workers join, shared backing releases, and the owner is quiescent.
Descriptors drain from3 to0 after disposal; pending operations are0. The retained
270550 bytes/2 mappings are measured before backing release, not a post-release
zero claim. JIT observed148 instructions and AOT146 due to ordinary scheduling;
counts remain recorded without normalization.

All11 commands finish without cleanup signals;947 pinned inputs and1174 final
source/tree/log/binary checks pass. The initial build-only descendant failure is
preserved; explicit disabled SDK build-server reuse resolves that runner issue.
This closes the finite ordinary clone/TLS/futex owner gate, not general threading
or .NET service compatibility. The pinned .NET raw-JIT diagnostic follows with
unchanged guest/configuration, and will record the next actual runtime boundary.

### P5 real threaded NativeAOT guest reaches stack discovery

The unchanged static-musl .NET service now executes beyond clone: membarrier
query/register succeed, clone returns child TID262144, and the configured
33558528-byte GC reservation succeeds. It does not reach readiness or HTTP.
Receipt `dotnet-threaded-guest-execution/attempt-2fpjx0g2/receipt.json` has SHA-256
`419e2d6a7da05a65ef9692e6217f5384c477c5192238c7fb40cbd2a21f8e047b`.
The main/child execute19998387/1613 instructions, stopping at the unchanged20M
budget with no execution or notification exception. All workers join, Machines
release, shared backing releases, and the owner is quiescent.

The main observes440030 syscalls; its first4096 retained rows are explicitly
truncated. Repeated pagewise mremap(old page,4096,8192,flags0) returns ENOMEM.
Pinned Blink's SysMremap unconditionally returns ENOMEM, unlike the native musl
stack-discovery witness that terminates on EFAULT at an unmapped source page.
Next work is truthful source-range validation under the actual guest mmap lock,
with general remapping support remaining explicit. No hardcoded guest address,
fabricated success, larger budget or native fallback is used. A preparation-only
Host snapshot-copy mismatch is preserved separately; the actual run's716 frozen
inputs,104 prepared files,11 binaries and33 artifacts were reverified.

### P5 stack-discovery boundary corrected; actual epoll startup reached

The reviewed mremap source-range check uses real guest PAGE_V reservations under
mmap_lock and restores no-fault state before absent-source EFAULT. General mapped
requests retain upstream's unsupported ENOMEM; no mapping mutation or broader
remap support is claimed. Optional `stage-threaded-core.py --mremap-validation`
reproduces the exact predecessor chain. Actual assembly reuses107 producers and
recompiles only syscall.c; all delivery builds pass in `threaded-delivery/attempt-wwrpenct`
(receipt SHA-256 `77c8f2c18fef1df9289c8e1a0e10add0d34fee88724d5de1d97ea2400251f030`).

The same real .NET guest now finishes2048 stack-discovery probes:2047 ENOMEM then
EFAULT at actual missing page0x4fffff7ff000. It reaches epoll_create1(EPOLL_CLOEXEC),
gets ENOSYS, writes the genuine SocketAsyncEngine initializer error and aborts
with guest SIGABRT6 before readiness. Receipt `dotnet-threaded-guest-execution/attempt-n9cjykot/receipt.json`
has SHA-256 `270edb094faf8b72fc9858b0bf0f86bdd2c781d198cca19a4853c7998c878a37`.
Both complete traces are retained (main2159/child7), with4388440 instructions and
no budget stop, CLR execution exception or notification failure. All workers join,
Machines and backing release, and the owner is quiescent. The same ELF/environment
and limits remain in force. Independent verification covers723 frozen inputs,
104 prepared files,11 binaries and33 artifacts.

Next ownership: inputs implements genuine private empty-epoll descriptor lifetime
and waits in Host; guest implements the bridge/header and normal qualification;
coordinator integrates the threaded profile. Native service evidence contains
create1 plus an unfinished empty wait, with no ctl or delivered events. No event
registrations/readiness will be fabricated; unsupported registration semantics
remain explicit. The actual .NET service and P5 are still unpassed; P6 is held.


### P5 empty epoll boundary qualifies before actual guest retry

A real private epoll open-description now owns duplicate-descriptor lifetime and
empty waits. Native common create/CLOEXEC/dup/close and zero/finite waits pass;
raw/optimized JIT/NativeAOT also pass normal caller cancellation, final-close
interruption and owner drain. Virtual signal masks and errno restore on the
calling worker; event bytes/canaries remain untouched. Linux packed12 and private
aligned16 callback layouts are measured separately. Registrations return explicit
EOPNOTSUPP; no event or readiness is fabricated.

Receipt `host-epoll/attempt-ixes6bgg/receipt.json` has SHA-256
`3688001f5bcff43194a790bb50cc344ef8dc0349978ff388380131e98e2bda5d`.
All21 commands pass, with current source/tool identities and unchanged execution
closures independently checked. Earlier attempt-bkcnbn9o passed all runtime rows
but failed the final runner closure check because AOT publication added a RID
subdirectory below the JIT output. Its failed receipt is retained; the corrected
runner executes a verified private JIT copy and keeps the final exact-tree check.

Next, inputs owns fresh ordinary base staging and the optional threaded
`--mremap-validation --empty-epoll` derivation, all108 producer emissions and
experimental delivery build. The selected host capability is HAVE_EPOLL_PWAIT1;
upstream guest pwait2 still uses its existing millisecond conversion. This focused
boundary result does not qualify registered events or actual guest service
startup. The unchanged actual .NET guest diagnostic follows successful delivery;
P5 remains open and P6 is held.


### P5 fresh threaded epoll delivery builds

The fresh base `core-profile/attempt-sbwhbkys` and reviewed threaded derivation
`threaded-core/attempt-empty-epoll` pass. All108 producers emit afresh with zero
reuse and successful link; assembly receipt `core/objects/36b04436f1e87762aac89c315944e7a8ec3ed19a95c93c4acacbbe05f2dab598/receipt.json`
has SHA-256 `4781efd31b0ced30666e773bf89bbfafc8c9b6e619b13114638409e10ff2e393`.
Delivery `threaded-delivery/attempt-jttvocmh/receipt.json` has SHA-256
`ed8698dda0797a9de502513913efca30ebfaf655395aedf0bdc150413e4a7655`:
raw build, semantic postprocess, restored-authored build and final direct-source
build all pass. The experimental project links37 original adapters and the
original Host project, without active copied Host/Bridges. Stable single-thread
delivery is preserved. No compiler rebuild was needed.

The launch receipt has SHA-256
`dfaa1fd69ad14a52886e8a381b800c09fa0784a0d1dd734f8c4017e50269b8ee`;
`threaded-core/attempt-empty-epoll/final-review.json` independently verifies1723
identities/maps, including objects, source/compiler and raw/final/log closures.
Coordinator rechecked all607 frozen delivery inputs. These are build gates; guest
now owns the unchanged actual .NET raw-JIT diagnostic on this exact delivery.


### P5 actual .NET readiness and accept reached

The unchanged real static-musl .NET guest reaches `READY 8080` and accepts the
published HTTP connection on the epoll-enabled threaded profile. Its next
required operation is `setsockopt(fd8,SOL_SOCKET,SO_SNDTIMEO_OLD,*,16)`, which
returns ENOPROTOOPT. The guest reports `SocketException: Protocol not available`
and exits the group with status1. The external client sees a connection reset;
no response case passes. This is observed guest behavior, not injected network
failure or a passing service gate.

Receipt `dotnet-threaded-guest-execution/attempt-4nsdxnrz/receipt.json` has SHA-256
`cf15e01a693614a61d1927783171b7336230c07dc6bdec0df297eee1bf9226b3`;
result SHA-256 `c1ab0945c167e86e64279477ca46a3130a4e3505392b2674fed4aee3872cf560`.
The four complete traces contain2201/7/6/3 observations with no truncation.
The actual child epoll_pwait uses maxevents1024 and timeout-1; it returns EINTR
during group teardown. All four workers join and Machines/backing release;
owner is quiescent after1249598 instructions, with no execution/notification
exception. Execution latched StopReasonNone at guest exit; the outer diagnostic
requests stop afterward while handling its HTTP error. Independent verification
covers883 frozen/prepared/binary/artifact identities.

Next ownership: inputs reviews truthful Host send/receive timeout state and
async-operation enforcement; guest designs normal native/all-four callback
qualification and bridge marshalling. No success-only socket option is accepted.
Do not use forced send-buffer saturation or injected peer delays/disconnects to
claim expiry coverage. Preserve ordinary successful I/O, timed empty waits,
caller cancellation and owner drain as distinct contracts. P6 remains held.


### P5 actual socket timeout semantics qualify

Private SO_SNDTIMEO/SO_RCVTIMEO now marshal signed LP64 timeval16 values, retain
per-socket/dup-shared settings, and inherit them on accept. Zero is unbounded;
positive values round up to milliseconds with an explicit int.MaxValue-ms
profile ceiling. Getsockopt reports the effective value. Physical nonblocking
Send/Receive/Accept attempts use monotonic managed deadlines and real WouldBlock
results: positive bytes return immediately, no-progress timeout returns EAGAIN,
and caller/owner cancellation remains125. Existing integer options remain4-byte.
A configured timed connect is explicitly unsupported; no in-progress connect
state machine or fake option success is claimed. Poll/epoll semantics are separate.

Native and raw/optimized JIT/NativeAOT pass ordinary empty accept and all four
receive-route timeouts, exact5s option round-trips, dup sharing, accept inheritance,
normal transfers with active options and after zero reset, and private caller
cancellation/owner drain. Send-expiry under forced backpressure is not tested or
claimed. Receipt `host-socket-timeouts/attempt-w1lhum8k/receipt.json` has SHA-256
`836b760fe3abfacbc58c6a031e0e65ae2c358e0482fde4d6af28cbdc9325720d`;
all17 commands pass and361 identity checks verify frozen inputs/output closures.
Coordinator independently rechecked live sources/tools and execution maps.
The preceding native-only attempt-7kpmkwuj records native_passed separately.

Inputs now owns a fresh Host/bridge profile and experimental delivery. No C or
header input changed, so all108 C producers are expected to be reused with exact
identity checks. The actual .NET guest must then be rerun; focused timeout
qualification alone does not establish its HTTP or normal shutdown gate.


### P5 genuine NativeAOT service passes raw JIT

The qualified timeout implementation is integrated in fresh threaded profile
`threaded-core/attempt-socket-timeouts`, with all108 C producers reused only
after individual source/object/emission identity checks. Delivery receipt
`threaded-delivery/attempt-6zot2nmd/receipt.json` has SHA-256
`5367a49b019f896f36f023e8503411ab92cac2e89fe1cb151d5c973926f67985`.
All five commands pass; independent final review verifies1836 identities and
coordinator rechecks609 frozen delivery inputs. Original authored project links
remain active; the stable single-thread delivery is preserved.

The unchanged .NET guest now passes actual translated execution in raw JIT.
Receipt `dotnet-threaded-guest-execution/attempt-v27zcpxg/receipt.json` has SHA-256
`61ed427f7395988490f374c1acc3fb5f0c653b2b02ce73b3b1f5658684efce3c`;
result SHA-256 `bdf46aec03b07bc2ffda1f41e8a65b6c4fc00e4770f2ef438f6aea97d717b8a9`.
Actual READY and published TCP requests produce the exact native86-byte health
and91-byte stop responses, stdout READY/STOPPED, empty stderr and group exit0.
Both accepted sockets' send/receive timeout options succeed. All four workers
join, all Machines/backing release and the owner is quiescent; no execution,
notification or diagnostic error occurs. StopReasonNone and1213246 completed
instructions are observed. Complete traces retain2206/7/6/3 calls without
truncation. All889 frozen/prepared/binary/artifact identities verify.

Next ownership: guest extends the exact same workload to all four host forms
using immutable raw/final delivery bytes and fresh processes; inputs designs
the actual bounded worker around the existing controller/protocol and C# owner.
Coordinator owns product/sample integration. Other host modes, ordinary stop
qualification of this workload, simultaneous instances/restart and the showcase
remain pending. Raw JIT success alone does not complete P5. P6 stays held.


### P5 actual NativeAOT HTTP guest passes all four host modes

Receipt `dotnet-threaded-guest-execution/attempt-febf3tyc/receipt.json` has SHA-256
`087d1f9af88019af1762135d35d11f4c4f9d0dca4ad86f25a952267df742144d`.
Its exact four-mode gate passes raw/optimized JIT/NativeAOT in fresh processes,
using immutable raw and final delivered C# bytes without a second postprocess.
All11 commands exit0. Each run produces exact native health86/stop91-byte
responses, READY/STOPPED stdout, empty stderr, group exit0 and StopReasonNone.
All four guest workers join and all Machines/backing release; every owner is
quiescent without diagnostic, execution or notification errors.

Observed instruction totals are1213688/1213688/1214130/1213246 respectively;
ordinary scheduling variation remains recorded. Complete main traces contain
2207/2207/2208/2206 calls and children7/6/3 each, with no truncation.
Independent verification checks1227 identities/maps/content assertions,
including829 frozen inputs,203 prepared files,32 binaries and91 artifacts.

This closes the four-mode actual HTTP/normal-shutdown subgate. P5 still requires
ordinary controller stop, actual worker/API delivery, two simultaneous instances,
restart and the sample/traffic gate. Inputs owns the worker and normal integration
fixture; coordinator promotes the reviewed threaded profile through the public
translation pipeline and integrates the showcase. Guest owns an exact native
and translated runtime inventory from these receipts. No P6 work begins.


### P5 threaded profile promoted through the public delivery pipeline

`bash blink/scripts/translate.sh --offline` now defaults to the reviewed threaded
profile and publishes it at `generated/TranslatedBlink`; the explicit
`--profile single-thread` option retains the older library profile. The pipeline
calls the same reviewed threaded derivation with mremap validation and real empty
epoll support, with exact source/configuration provenance. After postprocessing,
both private Host and bridge context are restored before rebuilding, and the
active product references original src files/projects only.

Public delivery receipt `translation/attempt-i4a5mfa8/receipt.json` has SHA-256
`387b147a1d95d998f1a73f980fcda227a06d244a8b9eaad8f74456fee8e7c741`.
All14 commands pass, including the pipeline's native prerequisite,108 verified
reused C objects, link, raw build, semantic postprocess, restored-authored build
and final direct-source build. Coordinator rechecked336 authored/raw/final/object
identities. Previous output is preserved in the delivery attempt's backup.

The original ThreadedExecution project now references the final product path;
its Release build passes with zero warnings/errors and disabled build servers.
The same C# execution implementation remains unchanged. The sample/solution now
point toward the controller, actual worker and this original execution project;
those consumer changes await their own runtime qualification and are not credited
from this library build. P5 remains active; P6 remains held.


### P5 actual runtime inventory recorded

`P5-NATIVEAOT-RUNTIME.md` records the pinned guest producer/ELF, native controlled
GC witness and all four translated service traces. It distinguishes successful
required operations from tolerated refusals, unfinished native observations and
unsupported facilities. Private epoll and thread ABI layouts remain distinct
from Linux guest layouts; no command8 membarrier, epoll registrations, arbitrary
signal delivery or send-expiry coverage is inferred from this workload. The
final P3 finite504/2016 CPU evidence keeps its separate producer lineage.
The P5 observed-runtime inventory checklist item is complete. Remaining service
lifecycle and final worker/sample gates are still pending.


### P5 actual worker/API milestone; final sample gate blocked

The public controller now starts the real authored C# worker over the threaded
translated product. Image admission is bounded at 2 MiB within the existing 3 MiB
frame; pre-launch serialization/validation remains intact. Each worker owns one
execution, reports actual readiness/guest output/endpoints/outcome, and reserves
raw standard streams for control frames. Options, limits and unsupported settings
are explicit in `src/Managed.Emulation.Worker/README.md`.

Native traffic reference `worker-native-traffic/attempt-znwlxua8/receipt.json`
has SHA-256 `4dc8d1122825b03fb75dad720665e212eaad97ccb8447ee882b90b8d2c5b2755`.
It verifies the unchanged guest with a 3,573-byte health request, the same bytes
in seven immediate writes, a 61-byte missing-path request and normal HTTP stop.
The 404 response is 101 bytes; larger/fragmented health responses retain the exact
86-byte native oracle. Write boundaries do not imply TCP packet boundaries.

`worker-instances/attempt-kd2trw_m/receipt.json` has SHA-256
`f17019198ff8f02aa3d522944e04c94d05de9ad10509e6cdb9fa742f56a79483`.
All 13 commands exit0 with no cleanup signals. Each raw/optimized JIT/NativeAOT
form runs four real processes and ten exact native HTTP comparisons: simultaneous
instances, HTTP exit, controller stop, fresh-process restart and an ordinary idle
accept deadline. Every actual final report proves joined/quiescent execution,
released Machines/backing, disposed/drained private IO and no notification error.
The parent result alone is never accepted as cleanup evidence. Auxiliary private
marker hashes are recorded but guest reads of them are not claimed; distinct
private executable paths are actually loaded. Worker control-read drainage is
reported honestly and remains separate from guest IO cleanup.

Independent worker review checked 1,382 identities; coordinator additionally checked
1,724 source/log/artifact/binary hash references and normal command cleanup. Only
two READMEs changed after execution; their exact prior bytes are preserved in
private source snapshots and `documentation-update.json` records the doc delta.
All executable inputs and the main receipt remain unchanged.

The prepared `ManagedConsumer` solution/sample now uses this public controller
and actual worker, with original authored project references. Its own final build,
JIT and NativeAOT qualification is unrun after current automated rejection B036.
Earlier C sample receipts are not reused as evidence for this replacement.
The phase stops here with that explicit pending P5 gate; no P6 work begins.


### Kestrel scope: baseline sample verified, new guest feasibility in progress

User-directed fresh baseline verification passes at
`managed-consumer-delivery/attempt-8k2fus34/receipt.json`, SHA-256
`6c0647e95d8fe1df01b3a907e8343b3bd138fe5d913f0db142c7737590398f0b`.
The actual solution builds, JIT sample executes, actual worker and sample publish
with NativeAOT, and native sample executes: six successful commands and four
actual raw-socket guest workers, with exact output and normal process cleanup.
The verification agent independently rechecked121 source/log identities. The
solution build has zero warnings/errors; existing AOT annotation warnings are
retained in logs. Commit f06256d records the result and historical B036 closure.
This is baseline evidence only; no Kestrel gate is credited.

`tests/KestrelService` is being prepared separately using the official ASP.NET
NativeAOT/CreateSlimBuilder path and genuine Kestrel HTTP/1 loopback transport.
The known successful pinned Alpine NativeAOT toolchain is reused for one bounded
standard static-musl publish and native HTTP witness. The user-directed difficult
musl-build stop condition remains in force. No compiler or shared host inputs
changed during baseline sample verification.

Read-only current boundary audit finds empty-only epoll waits with registration
explicitly unsupported, and socket O_NONBLOCK rejected (pipes already support
it). Physical sockets being internally nonblocking does not supply guest-facing
nonblocking semantics. The new native trace will establish required extensions
and actual image/memory/thread limits before implementation. Existing 2MiB
image/64MiB backing/16-worker profile is not assumed adequate for Kestrel.


### Genuine Kestrel NativeAOT guest build and native HTTP milestone

The first standard pinned Alpine publish succeeds in23.91 seconds with no
warnings/errors or workarounds. Native five-case HTTP, actual READY/STOPPED,
exit0, empty stderr and normal group/container cleanup pass. Receipt
`kestrel-guest-musl/attempt-o5jvvf7t/receipt.json` has SHA-256
`7bc07c1e8d01dd3d326fdbb436473ff0b2b8dcaf2910aea6fffebdaa7b119865`.
ELF `publish/KestrelService` is9,371,272 bytes, SHA-256
`ef6f1433794a42fe32b0fed4851bf88dd0631cd6a836550c6effca79d9e9a3ac`,
static x64 ET_EXEC with no interpreter/shared dependencies and TLS24/296/8.
All14 commands pass; independent source/tool/package/log/ELF review passes.
The user-directed difficult-musl-build condition did not occur.

Native observed14 TIDs and actual edge-triggered EPOLLIN/EPOLLOUT registrations,
nonblocking sockets, accept4, TCP_NODELAY and shutdown SO_LINGER. Default hosting
also observes filesystem watches/diagnostics; their necessity for this fixed
HTTP fixture is separate from recording them. No native result is relabeled as
managed execution. Next work is exact translated startup plus real readiness
and nonblocking contracts, with measured image/profile limits. P6 remains held.


### Selected Kestrel configuration and first managed diagnostic

The unchanged Kestrel ELF passes the same native five-case traffic with supported
configuration reload and diagnostic IPC disabled. Exact six-entry environment
and provenance are in `kestrel-native-profile/attempt-zphi57zq/receipt.json`, SHA
`e1e3c2ecf4c929f6f13d0f4937757cdc0dc82ee2b55d2c76d1fd88c4ec7db01a`.
All semantics except actual validated Date match the first native witness;
12 TIDs and genuine nonblocking edge-triggered socket operations remain.
The first default-configuration trace remains intact.

Guest worker owns `tests/KestrelGuestExecution`, preparing exact public-delivery
optimized JIT startup/HTTP with this native profile, a100-million-instruction
budget and60-second wall bound. Existing64MiB backing/16-worker limits are kept
for the initial observed outcome. Verification worker owns source-only normal
socket/epoll test preparation, with current host frozen until this baseline run
finishes. Coordinator owns bridge/integration and prepared measured image
admission16MiB/control-frame24MiB; these source changes are not yet qualified.
Required edge semantics are explicitly the observed drain-to-EAGAIN contract;
arbitrary general Linux EPOLLET equivalence is not presumed from polling.


### First translated Kestrel startup: bounded GC progress and unresolved guest OOM

The exact original public-delivery optimized-JIT diagnostic builds and runs,
but does not reach READY. `kestrel-guest-execution/attempt-md2uqfje/receipt.json`
SHA `95e9dcd1133dcc11d74c3fb5b1c359c6876dd6757dec041fc92a2a5f188d8e44`
reaches100M instructions in about8.6 seconds at a guest LOH GC relocation loop.
No HTTP pass is claimed. Complete syscall traces and normal release of all three
guest workers/Machines, backing and IO remain recorded.

One explicitly reviewed larger-workload assessment uses the immutable original
private library/Host/owner/image snapshots. Only the authored test's instruction
bound changes100M to1B and wall bound60 to120 seconds; exact derivation hashes
and diff are retained. `attempt-budget-f4rifsku/receipt.json` has SHA
`c6bcda3589e3c29ef4d50a2f03e308efe0a660b1616456a36f531ac0bd88cc2d`.
It progresses beyond GC, then the guest raises OutOfMemoryException in
Hashtable.rehash during UriParser/Uri initialization and Kestrel startup,
ending via guest SIGABRT at160,772,447 instructions in13.54 seconds. Neither
budget nor wall deadline ends that run. Retained backing before release is
17,577,862 bytes across68 maps; no new mmap ENOMEM appears. Native execution of
the same ELF/GC profile passes, so this does not establish host quota exhaustion.
All resources release and no host CLR/notification error occurs;321 identities
were independently checked. The first instruction bound was insufficient, but
the actual guest OOM still requires allocation/state investigation.

Meanwhile the confirmed asynchronous socket/epoll host contract is implemented
with finite drain-to-EAGAIN edge generations, actual readiness, alias lifetime,
peek/nonblocking behavior and real zero-second linger. Native and raw-JIT
focused checks have passed; remaining modes are running. No unrelated P6 work
or guest fallback is used to bypass the unresolved Kestrel startup result.


### Required asynchronous socket boundary qualified

Commits3d89c32 andb1396c3 integrate the Host implementation and translated
bindings. `host-async-sockets/attempt-n7spb0qb/receipt.json` SHA-256
`5557d1877e7fba91399618d5767c00b91b78fabe290d7c3eb9e9479dc80e405c`
passes native behavior/private ABI and raw/optimized JIT/NativeAOT. All21 commands
and six executions pass; both the worker and coordinator independently verify
211 source/tool/log/binary identities.

The finite contract uses actual socket readiness and epochs advanced only by
observed EAGAIN, shared open-description identity, fair bounded event collection,
nonblocking reads/accept/writes, MSG_PEEK, real zero-second linger and drained
cancellation/disposal. Tests cover new data/connections after drain, initial
actual writability, opaque64-bit event data, alias/final-close/fd reuse, and
ordinary listener shutdown. Positive-duration linger and nonblocking connect
remain explicit unsupported operations. Arbitrary Linux EPOLLET, one-shot/MOD,
forced backpressure and fault injection are not claimed. Upstream retains guest
12-byte to private16-byte epoll ABI conversion; the bridge copies actual events.

The Kestrel guest's earlier startup OOM is independent of this later transport
boundary. Investigation continues on immutable original snapshots: selected
allocation arguments are being observed, and a read-only audit identified the
old upstream sysinfo fallback reporting1GiB total and zero free RAM. That is a
candidate pressure-accounting discrepancy, not yet a proven root cause or fix.


### Kestrel Hashtable failure localized to upstream SIMD mask storage

The bounded allocation observation at `attempt-registers-ndumh6v1` records
46 insertions and 46 expansions. The final request is 672,827 buckets,
16,147,872 bytes. A second immutable-baseline observation at
`kestrel-guest-execution/attempt-float-registers-smhynls8/receipt.json`
(SHA-256 `b10f27addb1cfaa4b66f89699f42b222ce028f9aa6c30da8d907215afd3754bc`)
identifies the first wrong value: the constructor computes `.72 * 3 = 2.16`
correctly, but CMPORDSS produces `bf800000` instead of `ffffffff`. ANDPS and
CVTTSS2SI consequently produce a zero growth threshold. Rehash with seven
buckets likewise computes 5.04 correctly before the mask corrupts its threshold.
All 51 observed comparison masks are wrong. The diagnostic records 458 bounded
rows, no observation omissions/errors, the same guest OOM, and complete teardown.
The coordinator independently rechecked all 151 pinned input hashes.

The immutable pinned upstream `OpCmppsd` assigns the integer comparison result
through the floating member of its union in all eight scalar/packed stores.
Generated C# faithfully preserves that erroneous numeric conversion. This is a
specific upstream instruction defect, not evidence for increasing guest quotas.
Commit 3e35284 preserves both diagnostic recipes and exact observations.

The verification worker owns a reviewed staging correction to these eight
stores and ten normal instruction regressions, preserving the first 550 CPU
case descriptors and 46 exclusions. The coordinator owns canonical regeneration
and integration; the guest worker next retries actual Kestrel against that
coherent delivery. The separate sysinfo fallback reporting zero free RAM remains
a known input mismatch, not the cause established by this observation. P5 is
still open and P6 remains held.


The correction passes 514 normal native/hardware comparisons, including all ten
new regressions (`cpu-conformance/attempt-havagw22`, receipt SHA-256
`a400af6ae56ad7e0ba4e6a7b5a157caea94b3ab2d418c7857281ba3cb300c267`),
and is committed as 9d2dc31. Managed regression and Kestrel retry remain pending.
The first regeneration attempt, `translation/attempt-gs58ug95`, stopped before
translation because IDE `.idea` files had been added under the reference tree.
The fetch check now preserves narrowly recognized XML/gitignore IDE metadata;
every archived file remains checked byte-for-byte, and other extra files or
symlinks still fail. The normal offline archive verification passes with those
settings untouched.


### Corrected public delivery and the next Kestrel startup limit

`translation/attempt-tf_6yqk6/receipt.json` (SHA-256
`92763476d1719166912ecbf2d5ca31cabb1feb512cfcef278a288319980524a5`)
passes all 14 production steps with 108 fresh objects, unchanged original
adapter/Host sources, raw comparison, semantic postprocessing and final build.
The coordinator checked all command logs, 89 authored source hashes and 18 final
files. The delivered library has one `IsJitDisabled` definition and zero
`Libc.L` calls, confirming the requested inline deduplication and literal pool.

The first corrected Kestrel retry at `kestrel-guest-execution/attempt-7lswgkg3`
(SHA-256 `e7e2075e1b6b6088724093069e592dd7d8a7fbb23bd35c720094b2995c4e4af7`)
progresses into thread-pool and epoll initialization. It does not reach READY:
the guest reports failure to create the thread-pool Gate thread, and the trace
records one failed anonymous PROT_NONE mmap of 274,432 bytes. The old Hashtable
OOM is absent. Execution reaches the 100M instruction bound; all six Machines,
workers, backing and IO release. One busy worker's 20,048 syscall observations
exceed the stored 16,384 rows; that truncation is explicit, not complete evidence.
The guest worker is distinguishing virtual reservation limits from actual
backing limits before any profile change; post-cleanup retained bytes are not a
peak-memory measurement.

A separate legacy CoreExecution check at `attempt-qzdd97ja` passes arithmetic
and instruction-budget rows, then its exit probe reaches an unbound guest-thread
callback. That test frontend assumes the single-threaded profile; the runner now
rejects the incompatible threaded profile before building. No replacement owner
or success callback is introduced. CPU regression is being adapted to derive
its own normal frontend directly from the verified public delivery, with explicit
compiled-product provenance, rather than calling this failed check a core pass.


### Kestrel readiness reached; cross-thread activation remains open

The corrected CPU matrix passes all 2,056 comparisons (514 in each raw/optimized
JIT/NativeAOT mode), with native agreement and 46 exclusions unchanged.
`cpu-conformance-managed/attempt-jt41ulk6/receipt.json` has SHA-256
`c62ea727bde462c938a91a67c6c3f4ae0ef69bff6c3bb28be1fe0bb06e10a7b2`.
It adds only the CPU frontend to the 108 verified public objects; it does not
claim the incompatible legacy CoreExecution test passed. Commit 7451cce records
the qualification and direct-delivery provenance checks.

The recorded reservation reconstruction accounts for 16,375 pages under the
16,384-page address-space limit; the Gate thread needs another 67 pages.
`tests/KestrelGuestExecution/reservation-evidence.py` pins the preserved inputs
and emits the intervals, arithmetic and limitations. It is not a direct peak
backing measurement or proof of which of two early ENOMEM checks fired.
The authored C# owner now accepts a selected 64 or 128 MiB limit (default 64),
and the worker forwards its validated option. Existing initialization couples
this backing ceiling with guest RLIMIT_AS/DATA. Kestrel selects 128 MiB; the
native ELF, six guest variables, 16 MiB GC limit, 100M instructions, 60 seconds
and 16-worker bound stay unchanged. No C/generated source changes are needed.

`kestrel-guest-execution/attempt-uq52p1wf/receipt.json` has SHA-256
`20727ac9628d261882dfd21e9efe3aee271c1b80cb7b820c93bb0876960cb04d`.
It reaches real READY and three matching native-semantic 140-byte HTTP responses:
health, large and fragmented. There is no mmap ENOMEM. The missing-route request
does not complete, and the stop request is not attempted. The guest worker
262146 requests signal 35 for worker 262150; the call returns EOPNOTSUPP, then
that worker raises SIGABRT. The client later reaches its deadline. These distinct
outcomes are retained; this is not reported as simply insufficient wall time.
All nine Machines/workers, backing and IO release, stderr is empty, and the
worker independently checks 1,005 source/artifact/execution identities.

Upstream SysTkill queues the guest signal before issuing an internal
pthread_kill(SIGSYS) wake. The C# owner currently rejects all nonzero wake signals.
The native control installs the runtime ActivationHandler for signal 35, but
its recorded five requests do not issue that activation signal. The next work
is truthful private wake and upstream handler execution under the actual guest
path, not native-host signals, a success stub, or a longer timeout. The guest
worker owns exact target/runtime analysis; the verification worker owns managed
wake/token review; the coordinator owns integration and serial validation.
The prepared four-mode worker matrix and final sample stay held until this
required service path works.
