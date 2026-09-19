# Blink implementation progress

Campaign started 2026-09-14 on branch `sqlite`. The approved plan is [PLAN.md](PLAN.md).

## Current gate

P0–P2 have passed their stated gates on Linux x64. The complete selected interpreter executes bounded instructions, faults and guest exits in raw/optimized JIT/NativeAOT. P3–P6 remain open; no managed service runner or Windows execution is claimed.

## Ownership

- Coordinator: source/host inventory, dotcc baseline and translation probes, integration, validation, and milestone/significant-progress commits.
- Inputs worker: bounded host memory, ordinary/signal-aware nonlocal jumps and host storage ABI qualification (after completing source/native baseline).
- Guest worker: actual interpreter closure translation, measured host header contracts and reduced compiler blockers (after completing native guest fixture).

Workers share one worktree with disjoint authored-file ownership. Shared compiler edits and repository suites are serialized by the coordinator. Generated/ref/build/artifacts content is disposable and ignored; reproducible scripts and durable summaries are committed.

## Milestones

| Milestone | State | Evidence / remaining work |
| --- | --- | --- |
| P0 | Passed | Immutable sources verified offline; native Blink and 25 assembly cases pass; six HTTP cases pass on Linux and Blink; exact native archive/import/global audit and initial translation failures recorded. |
| P1 | Passed | Actual bounded instructions, synchronous faults/unwind and exit/exit_group match native under raw/optimized JIT/NativeAOT; profile ABI matches a separate native probe. |
| P2 | Passed | All109 selected sources emit/link; raw/optimized libraries and whole-library-rooted AOT execute; direct IL/import/initializer inventories complete with zero traversed native imports. Indirect/framework limits remain explicit for P4/P6. |
| P3 | Pending | CPU/memory/ELF behavior corpus. |
| P4 | Pending | Real host contracts and service startup. |
| P5 | Pending | Worker/controller lifecycle and two-instance HTTP qualification. |
| P6 | Pending | Faults, Linux/Windows runtime matrix, reproduction and regression qualification. |

## Observed environment

.NET SDK 10.0.111 is available. Baseline build disables automatic sibling LALR.CC substitution using `-p:UseLocalLalrCc=false`. Test TMPDIR is isolated to `blink/artifacts/tmp`.

## Next actions

Qualify the independent CPU corpus and unchanged pinned valid ELF through derived
links of the actual translated core. Extend the owning execution adapter to
service startup and bounded worker lifecycle. Keep malformed-input and Windows
execution gates open. Current workers own CpuConformance and ElfLoading;
the coordinator owns audit integration, milestone commits and reviewed changes
back to the original checkout. See the final progress entries for current evidence.

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
