# CoreCLR memory diagnostics

`diagnose.py` copies the frozen r8 public consumer from
`blink/artifacts/coreclr-runtime/guest/attempt-wfoso6ej/private`. It runs the
ordinary public workload against the exact retained .NET 10.0.12 closure at
`blink/artifacts/coreclr-runtime/native/attempt-h40bq9ta/closure.json`.
It never modifies shared product sources or generated delivery files.

The private owner replaces opcode 261 in that **instance's** dispatch table,
checks its original `OpSyscall` pointer, observes syscall entry/return, calls
the original implementation directly, and restores the table after worker
quiescence. It preserves translated errno and existing fault handling. This
avoids decoding every guest instruction merely to find syscalls. Generated
source bytes and imported guest images retain their captured hashes.

Each observation records C-owner bytes/mappings/limit, guest VSS/RSS, effective
VSS/RSS limits, raw `RLIMIT_AS`, public `MemoryFilesUsage()`, and private combined
accounting. Regular-file page/lease counts distinguish imported shared backing.
Snapshots use the existing individual owner locks; counters from different
owners are consecutive observations, not one atomic cross-owner snapshot.
Guest stack entries are raw words with mapped module offsets, not an unwound
native stack. The diagnostic host runs with the JIT; reflection-based trace
serialization does not establish NativeAOT compatibility.

Run the unchanged-limit baseline and summarize its retained trace:

```sh
python3 blink/tests/GuestCoreClrMemory/diagnose.py
python3 blink/tests/GuestCoreClrMemory/analyze.py /absolute/path/to/attempt
```

Builds use `Scripts/production_support.py` locking, one MSBuild worker, and
disabled build servers/shared compilation. A busy build lock fails without
queuing; `--resume /absolute/path/to/prepared-attempt` retries the same frozen
inputs. Guest execution remains bounded by 600 seconds and two billion
instructions. Failed guest reports remain failures; these are diagnostic
artifacts, not fresh qualification receipts.

## Measured unchanged-limit failure

Baseline receipt:
`blink/artifacts/coreclr-runtime/memory-diagnostic/attempt-0t0j05qr/receipt.json`.
Its `analysis.json` retains source trace/report hashes and resolved module
offsets. The guest reproduced `Out of memory.`, SIGABRT, and clean resource
release at 148,102,332 instructions.

The causal stack allocation is syscall sequence **6893**, main guest thread 1,
instruction **147,216,408**:

```text
mmap(NULL, 0x801000, PROT_NONE,
     MAP_PRIVATE|MAP_ANONYMOUS|MAP_STACK, -1, 0) = -ENOMEM (12)
```

At entry and return:

* VSS: **64,542 pages = 264,364,032 bytes**; AS limit: **65,536 pages = 256 MiB**.
* Request: **2,049 pages = 8,392,704 bytes**, an 8 MiB stack plus a 4 KiB guard.
* The full request would require 66,591 pages, exceeding the limit by 1,055 pages.
* C owner: **58,618,627 bytes**, **12,962 mappings**; backing limit: **256 MiB**.
* Memory-file accounting: **19,437,184 bytes**, nine files, 4,469 leases;
  sublimit: **64 MiB**.
* Combined accounting: **78,055,811 bytes**, also the peak sampled at syscalls.
* Regular shared cache: eight files, **3,791 pages**, 3,791 leases.

The accounting constants imply 4,464 unique file pages (18,284,544 bytes of
native payload): 15,527,936 bytes in regular-file cache pages and 2,756,608 bytes
in memfd pages. These payload figures are derived from the recorded counters
and the captured 1,024-byte node / 4,224-byte page / 128-byte lease charges.

The frozen r8 generated `BlinkCore.00005.cs:706` rejects
`size / 4096 + system->vss > GetMaxVss(system)` before `ReserveVirtual`.
`BlinkCore.00002.cs:4520` obtains that maximum from `RLIMIT_AS`. The RSS precheck
was not exhausted (4,984 pages versus 65,536). No backing allocation was reached
for this request.

The exact retained libc caller return address resolves to ELF address
**0x9d617**, inside exported `pthread_create`. Its instruction at **0x9d612**
calls `__mmap`; the requested flags and following failure branch match the
trace. The same binary converts ENOMEM to pthread EAGAIN at **0x9d745–0x9d758**.
CoreCLR later writes `Out of memory.` at syscall sequence 6996 and raises
SIGABRT at sequence 7018. Earlier ENOMEMs are recoverable 64/128 MiB glibc arena
reservations; the stack request is the only other ENOMEM size in this trace.
No failed `MAP_FIXED` request occurred, so overlap charging is not this trigger.

## Private AS-only control

Only after a completed baseline has been captured, an explicit private control
can use 4 GiB for AS while retaining the 256 MiB C-owner and 64 MiB memory-file
limits:

```sh
python3 blink/tests/GuestCoreClrMemory/diagnose.py \
  --diagnostic-virtual-mib 4096 \
  --baseline-evidence /absolute/path/to/baseline/analysis.json
```

This changes only the copied host AS query boundary and initial guest AS
records. It does not introduce a product option or modify default policies.
The receipt pins the baseline evidence, control boundaries, unchanged
translated bytes, and exact imported images. Control findings are diagnostic
evidence; public Console and file compatibility remain separate work.

The completed control is retained in
`blink/artifacts/coreclr-runtime/memory-diagnostic/attempt-4kxakhbr`.
Its guest report passes the complete workload: exact stdout, exported file
contents, empty stderr, exit 0, 259,455,038 instructions, and released resources
in 343.33 seconds. There were zero ENOMEM syscall returns. Peak combined
accounting sampled at syscalls was **82,614,285 bytes** (C owner 62,370,061;
memory files 20,244,224), with both backing limits unchanged. The equivalent
late `0x801000` stack request succeeds at instruction 147,166,430.
`comparison.json` pins both attempts and the successful control report.
The retained control's enclosing diagnostic receipt has `guest_passed=true`
and `fresh_qualification=false`; that earlier harness revision left its outer
`passed` flag false. Current harness revisions set `passed` from the actual
guest report while still marking all runs as diagnostic, not qualification.

The retained partial attempts `attempt-sn3bq2di` (slow uncached observer),
`attempt-jfp54az6` (build error), and `attempt-xs2nkhjg` (incomplete AS control,
stopped after snapshots showed the original limit) are not causal controls.
