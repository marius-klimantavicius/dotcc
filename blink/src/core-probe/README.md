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
