# Native interpreter embedding probe

`probe.c` exercises the unchanged pinned interpreter through its actual
`ExecuteInstruction` entry. It is an authored embedding/test adapter, not a CPU
implementation. `scripts/probe-core.sh` links the native configured Blink archive,
runs the cases with a 15-second process deadline, records the 83 actually extracted
upstream translation units and hashes, and attempts dotcc on that closure.
`--native-only` limits the script to the independently useful native oracle.

Each upstream translation unit is checked against `config/source-inventory.json`
and copied byte-for-byte to `generated/core-profile/source/`; the copied hash is
checked again. Passing upstream `blink/*.c` directly to dotcc adds that directory
to its include map and can replace bare system `signal.h`/`string.h` with Blink's
unrelated headers. Staging removes that accidental collision without editing any
upstream source. The selected managed host profile is copied into a unique
attempt directory, with header and compiler assembly hashes recorded before
translation. The profile is included last to follow dotcc's current include
overlay precedence; declared host operations remain unresolved until implemented.
`config/core-config.h` owns core feature selection and matches the native oracle's
JIT/x87/thread/VFS/metal/MMX/BCD/ROM/BMI2 exclusions; sockets remain enabled and
`HAVE_FORK` remains absent. The host profile's own `config.h` is excluded from the
snapshot so it cannot override core selection through include-map precedence.

The adapter allocates `System`/`Machine` through upstream constructors, initializes
upstream page tables as the loader does, reserves code/data guest pages, copies
the test instructions through `CopyToUser`, and reads results with `CopyFromUser`.
Guest virtual addresses are translated by upstream memory code. Code bytes are
ordinary data interpreted by Blink. This probe does not invoke the ELF loader.

The existing frontend `TerminateSignal` hook is supplied by this adapter, as the
upstream CLI and TUI each supply their own implementation. For the fixed cases,
the hook captures signal/code and returns. Upstream `HaltMachine` then performs
the synchronous `siglongjmp` to `m->onhalt`. No upstream file or instruction
algorithm is patched. This hook is qualified only for these fixed fault paths,
without guest signal handlers, guest syscalls, or asynchronous host signals.

| Case | Actual guest operations | Expected result |
| --- | --- | --- |
| Arithmetic | `mov eax,40; add eax,2; mov [rbx],rax; inc qword [rbx]` | Four completed steps; RAX 42, guest memory 43 |
| Undefined | Arithmetic sequence followed by `ud2` | Four completed steps; upstream halt -3; Linux SIGILL 4/code 1; IP restored to faulting instruction |
| Budget | `jmp` to itself | Seven completed steps; returns to caller with original IP |
| Unmapped | `mov rax,[0]` | Zero completed steps; upstream halt -4; Linux SIGSEGV 11/code 1; original IP |

All four cases run twice with full `FreeMachine` cleanup between each case. The
native adapter checks completed-step count, halt result, signal/code, RAX, memory,
and final IP. Both repetitions passed on Linux x64 on 2026-09-14. Its ABI output
was `Machine=22432 System=3016 ax=24 ip=0 flags=12 onhalt=1264`, in bytes.
This is a measured native layout; it is not evidence of an emitted C# layout.

Current receipts are `artifacts/core/native.txt`, `native-link.map`, `closure.json`,
and `translation-status.json`. Every new translation result is also preserved in
`artifacts/core/attempts/`, so retrying does not erase a previous blocker.
The checked-in `observed-native-closure.json`
preserves the observed source/configuration/archive/adapter/binary identities.
Native linker extraction includes additional reachable native system services;
these are dependencies to port, not permission to expose those services from a
managed product. The native archive was configured with JIT and linear mapping
disabled by `scripts/native-oracle.sh`.

The first actual dotcc attempt stopped at `blink/util.h:22`:

```c
int GetOpt(int, char *const[], const char *);
```

The parser rejected the unnamed array parameter. A native-checked reduced source
and the original diagnostic are retained under `artifacts/core/reduced/`; the
coordinator's generic array-parameter repair cleared that failure. After staging
the verified translation units, the next actual parse blocker was
`static inline struct Dll *dll_last(struct Dll *list)` in `blink/dll.h:21`.
Its native-checked reduction is also retained in `artifacts/core/reduced/`, and
the coordinator's generic declaration-specifier repair cleared it. The retained
`pthread_t` Machine slot then required an explicit, native-checked 8-byte host
identity type even with guest threads disabled; the input worker supplied that
declaration without adding thread operations. The next observed semantic blocker
was `unsupported GNU attribute: __const__`, from `pureconst` in `blink/builtin.h`.
`config/core-overrides.json` now removes this optimization/purity hint using a
required exact macro-body match; it changes no instruction algorithm or ABI.
Packed, aligned, and calling-convention attributes are preserved. The override
profile is snapshotted before each attempt, and any emitted override report is
preserved with the attempt's diagnostics. Translation remains bounded and open;
the current observed result is recorded in `translation-status.json`.
The input worker owns the
authored host headers and their independent native ABI qualification.
No managed core execution, NativeAOT core execution, or complete host closure is
claimed. Native shim-layout comparisons qualify those C storage records alone;
complete `Machine`/`System` emitted layouts must also be measured and compared,
with any host-record differences identified separately from guest CPU/ABI state.

P1 remains open. Required follow-up work includes emitted ABI comparison, actual
managed interpreter execution, qualified synchronous unwind and signal-mask
semantics, page-lock cleanup on every halt path, managed host ownership, and
cancellation of instructions that can block or do substantial internal work.
An instruction-count budget alone does not bound `REP` or blocking syscalls.

## Bounded source isolation

`probe-core.sh --stage-only` prepares a new immutable profile without starting
whole-closure translation. It also snapshots the authored adapter and qualified
`HostSignals.c`/header. Full managed translation includes that signal-jump seam;
its virtual delivery mask remains distinct from guest Linux signal state and
does not implement asynchronous host signal delivery.

Use the printed profile path with:

```sh
python3 blink/scripts/isolate-core.py --profile blink/generated/core-profile/attempt-NAME --start memory.c --only --timeout 120
```

Each object attempt records exact source/profile/compiler identities, command,
duration, diagnostics, and an independent output path. A timeout means that the
bounded invocation did not finish. Typical header-heavy successful objects take
17–20 seconds on the observed host, so the original 20-second isolation bound
can expire from throughput alone. Whole-closure timeouts are not proof that the
compiler cannot process the core.

The first sequential isolation scan emitted 19 unchanged upstream units before
`debug.c` exposed file-scope array-typedef storage. Generic compiler repairs now
let that unit emit, including real rooted per-thread array storage where needed.
`syscall.c` exposed function-form parameter adjustment, which is now repaired
with a native-checked compiler fixture. Its subsequent concrete missing host
records drove the fcntl/timer/resource profile additions and independent ABI
probes documented in `docs/MANAGED-HOST.md`.

A directed `memory.c` invocation emitted successfully in 17.7 seconds
(`artifacts/core/isolate-ls30l3ur/result.json`). Its `flattencalls` macro uses the
`__flatten__` inlining hint; `core-overrides.json` now removes that hint with the
same exact required-match rule as `pureconst`. No ABI-bearing attribute is removed.
At this checkpoint, `machine.c` and `syscall.c` both reach the generic setjmp
recognizer's unsupported `if (!(rc = sigsetjmp(...)))` shape. Object emission
alone does not qualify linking, generated C# compilation, or guest execution;
the actual managed core gate remains open.

Full emission now uses the planned managed library shape: `--emit=managedlib
--nest-types --class-name Blink --namespace Managed.Emulation --runtime=c`.
The default whole-closure bound is 1800 seconds and remains configurable through
`CORE_TRANSLATION_TIMEOUT`. The generated public upstream type remains
`Managed.Emulation.Blink.System`; compiler/runtime framework references use
`global::System` so that nested C name cannot capture them. Direct and object-linked
regressions exercise the same boundary with C/all runtimes and split output.
Default global-namespace output containing a C type named `System` is still
unqualified: that shape requires a separate collision-safe naming design.

The generic negated-assignment setjmp repair subsequently let `machine.c` emit
in 24.6 seconds (`artifacts/core/isolate-87btc0ub/result.json`). After adding the
native-qualified `tms` record, `syscall.c` advanced to three static assertions
requiring `PROT_READ`, `PROT_WRITE`, and `PROT_EXEC` from the missing `sys/mman.h`
(`artifacts/core/isolate-26kz7f86/result.json`). This is an incomplete host header
surface, not a reason to weaken those assertions.

The native-measured memory-protection header cleared those assertions, and
`syscall.c` emitted in 28.1 seconds
(`artifacts/core/isolate-gmmf9m7u/result.json`). `instruction.c` and `sse2.c` also
emitted in independent bounded invocations. The complete 83-unit library is now
attempted under one immutable profile; per-unit successes do not substitute for
that combined emission, linking, C# build, or actual managed execution.

The first combined nested-library attempt (`attempt-m53uo6gu`) completed with a
real compiler diagnostic rather than a timeout: `debug.c:127` uses the valid
bare negated guard `if (!setjmp(g_busted))`. Continuing source isolation after
that unit found a retained mutex-attribute type in `demangle.c` (now qualified)
and then the generic nested string-array initialization failure in `disarg.c`.
The native-checked reduction is `artifacts/core/reduced/nested-string-array/`.

`config/core-managed-additions.json` explicitly records unchanged upstream
translation units required by the honest managed profile beyond the native
archive extraction. The initial addition is `pte32.c`: the managed `CAN_64BIT=0`
path needs its page-table helpers. Each additional source is checked against
both this manifest and `source-inventory.json`, then copied and hashed into the
unique profile snapshot. It is not retroactively labelled part of the native
83-unit archive closure. The isolation script can target these additional units
as well as the native-selected ones.

The current managed snapshot also integrates the qualified `HostMemory`
boundary. It verifies and records `stage-map.py`'s exact two host-capability
replacements in `map.c`, preserving its original and staged hashes separately.
`HAVE_MAP_ANONYMOUS` is selected only together with `NOLINEAR`, the campaign mmap
header, and the owning adapter sources. The native archive oracle remains
unchanged. `managed-driver.c` owns a 64 MiB memory budget, runs the original
repeated-case probe, and disposes its remaining mappings only when the entire
worker is being discarded. Its entry point rejects a second invocation; no
upstream call or slab-cache reuse is allowed after disposal.

The nested string-array repair reuses the compiler's existing target-typed
array-field initializer for standalone character arrays. The native-checked
fixture includes partial rows, exact-size literals, UTF-16/UTF-32, local/static
storage, and ordinary pointer arrays. The unchanged `disarg.c` then emitted in
19.5 seconds (`artifacts/core/isolate-443uvfbh/result.json`).

The generic omitted-outer-extent array fix lets unchanged `disspec.c` emit its
nested name table; the reduced fixture checks brace-grouped, fully flat,
character, pointer, static and local arrays against native C. The complete
repository suite passes 2,231 unit and 508 functional tests (1,033 skips).

New profile snapshots also apply `HostCpu/stage-cpuid.py`, preserving the
original source hash and separately recording the staged result. Its only
changes guard three advertised MMX/x87 bits with the existing feature switches;
qualification lives in `tests/HostCpu`, including native flag combinations and
actual representative instruction probes. This does not replace instruction
execution. The terminal-size declaration addition lets actual `ioctl.c` emit
(`artifacts/core/isolate-3dwb1gji/result.json`); complete managed linkage and the
repeated instruction harness remain the open P1 gate.

The authored harness enables upstream `System.trapexit` for every case and now
executes Linux syscall 60 (`exit`, status 37) and 231 (`exit_group`, status 42).
Native execution records `exited=true` and the exact status, then unwinds with
`kMachineExitTrap=-10` before `FreeMachine` or a host exit call. `HaltMachine`
restores the syscall instruction IP, so both exit cases stop at `0x40000a` after
two completed instructions. Both pass twice along with the original cases; no
managed execution claim follows until the emitted core is linked and run.

`assemble-core.py --profile <frozen-attempt> --jobs 2` builds a resumable object
set and links its `.cs` fragments directly into the planned nested managed
library. New profiles contain their own `closure.json`. Each emitted object has an independent conservative C-input identity: exact
selected source, all profile headers and included C fragments, macro overrides,
compiler options and dependencies, helper/isolation script hashes, and the
pinned upstream include inventory. Inputs are copied into verified canonical
content-addressed directories before compilation. This preserves absolute-path
static-symbol mangling and `__FILE__` values across profile copies. Reuse retains
the original producing profile, receipt and object hash. Independent translation
units and consumer C# files do not invalidate an otherwise identical C object;
the complete selected profile, managed bridges and Host project remain hashed
in the enclosing link/consumer receipt.
Compiler identity includes every DLL, deps.json and runtimeconfig.json in the
compiler output directory, including the parser dependencies. The identity
helper itself is snapshotted and hashed.
`--sources <filenames...>` performs bounded emission only. A smoke run emitted
`startswith.c` and `prog.c`, then reused both with no new compiler invocation
(`artifacts/core/objects/28925a559ca04fdae970c6a358405e28a7d945cdae2ed766557d88df0b457b7e/receipt.json`).
The separate initial two-worker check emitted real `memorymalloc.c` and
`statfs.c` after their declaration fixes. These are object-emission checks;
complete C# linkage, host callback selection and actual instruction execution
remain separately required.

The generic static-local array-list repair lets unchanged `strace.c` emit its
paired scratch buffers in 22.15 seconds (`artifacts/core/isolate-zhqr_q7j`).
Each declarator uses the existing rooted global-array backing under its own
mangled local name. A native fixture checks zeroing, repeated calls, nested
arrays and struct/pointer initializers; separate direct/object-linked managed
consumers retain the returned pointers through forced GC. The complete suite
passes 2,235 unit and 517 functional tests (1,039 skips). Blink itself removes
its `_Thread_local` spelling under the selected `DISABLE_THREADS` profile;
this repair does not claim support for block-scope TLS.

The explicit binding staging helper runs after the hash-checked map, diagnostic
and CPUID source adaptations. Its preamble applies to all upstream translation
units, including managed-only `pte32.c`, and leaves authored C wrappers separate.
The frozen manifest selects qualified C/managed bridges and isolates deferred
host operations as unresolved names. Initial binding-preamble checks emitted
`address.c`, `pte32.c`, `prog.c` and `machine.c`; this does not establish that all
remaining declarations or callbacks link. The driver enables private file
mappings after memory ownership begins, and its owning C# consumer must bind
private IO, environment and identity before entering the driver.
