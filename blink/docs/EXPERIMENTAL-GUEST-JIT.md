# Experimental native guest JIT

This uncommitted, opt-in x64 backend emits native machine code from a strict
allowlist of Blink-decoded instructions. Hybrid interpretation is intended:
unsupported instructions, unavailable leases and denied host capabilities return
to the established interpreter. The interpreter remains the default.

```csharp
await using var machine = new BlinkMachine(new MachineOptions {
    GuestJit = new GuestJitOptions { Enabled = true },
    InstructionLimit = 100_000_000,
    ExecutionDeadline = TimeSpan.FromSeconds(30)
});
```

A run can override `MachineOptions.GuestJit` through `ExecutionOptions.GuestJit`,
including disabling it. Options and statistics pass through SeparateProcess;
the consumer and worker must come from the same source closure. NativeAOT **host**
and NativeAOT **guest** are independent axes. This backend requires neither
Reflection.Emit nor a native Blink library.

## Initial private backing and worker safety

The first worker transition restricts native code to initially enrolled private,
non-writable ELF executable pages. Enrollment happens once after `LoadProgram`
finishes and before the first guest instruction or child publication. Reserved code pages become eligible only after ordinary guest fetch commits
them; enrollment does not change guest RSS or reservation accounting. The initial
main ELF executable range is used; later libraries and anonymous code heaps are
not enrolled. Shared executable backing, including memfd aliases, interprets.

The protocol protects content, identity and lifetime through native return:

* Every non-fetch `LookupAddress2` revokes overlapping enrolled code **before** a
  data pointer can escape. This includes benign reads and unchecked interpreter,
  stack, REP, syscall-buffer and cleanup writers. Only the exact execute lookup
  used by instruction fetch is exempt. The JIT decodes protected bytes with
  Blink's decoder; its former debugger helper uses data lookup and is unsuitable.
* Protect, remove, fixed replacement, reservation/publication, advice discard,
  reload and whole-space cleanup revoke overlapping initial code before their
  first effect and invalidate cached data identities. Initial code eligibility
  is permanently removed. Live mapping writers retain `System.mmap_lock` through
  all permission, identity and lifetime effects; loader and final cleanup run
  only while the execution owner is quiescent.
* The gate serializes native calls and cache retirement within an owner. Other
  workers can interpret. No gate is carried into subsequent helper work or a
  blocking syscall. Data-capable calls first try the mapping mutex without
  waiting, then acquire the native gate, and hold both through descriptor
  resolution and native return. A busy mapping mutex selects interpretation.
  Cached register-only blocks with a live initial-code lease and unexpired
  negative decisions need only the native gate; that path never acquires the
  mapping mutex or compiles a data block. The lifecycle statistics lock is
  acquired only after the native snapshot releases its gate. Final backing/cache
  release follows worker joins.

This is a narrow, audited mapping class, not a claim that current read-only bits,
TLB entries or generation checks alone make arbitrary code immutable. Before
child creation, the original single-worker byte-validated path can also execute
private anonymous code. After publication, revoked/noninitial code interprets.
Recursive signal actors, tracing and exceptional machine modes interpret.

The authored memory adaptation also corrects anonymous fault CAS-loss recycling
to resolve the host page from its registry identity. It fixes `VirtualCopy` output lookup to require
user write permission before each copy. `CopyToUserWrite` bookkeeping alone did
not enforce that permission. The source-derived uname buffer case was not run as
an invalid-access experiment. Fresh producer receipts and approved ordinary
mapping/permission controls must accompany this change; generated C# is never
hand edited. The writer inventory and lock argument are retained in the attempt.

## Coverage, checked loads and exact boundaries

Register/immediate MOV, ADD, SUB, CMP, AND, OR, XOR, TEST, INC/DEC, Jcc, JMP and NOP
retain their 8/16/32/64-bit behavior, high bytes, zero extension and Blink flags.
New pure forms include 32/64-bit LEA with 64-bit base/index/scale/displacement or
RIP addressing, register MOVZX/MOVSX/MOVSXD, ordinary padding and ENDBR64. Blink's
instrumentation NOP encodings still interpret. Address-size and FS/GS prefixes
interpret; guest segment bases are never host segment bases.

Checked MOV32/64 and MOVZX byte/word loads use at most four invocation-owned,
unmanaged descriptors. Current committed private non-executable mappings,
including later heap and worker stacks, qualify under the live mapping mutex.
A bounded 64-entry, four-way descriptor cache reuses positive identities only
within the current mapping epoch. Every mapping callback invalidates it; wrap
clears all tags before recycling an epoch. Generation equality is useful only
while the live mutex excludes writers. Fresh resolution walks authenticated page
tables without a TLB or fault helper and checks owner extent, private identity,
read/write permission and file availability. Reserved pages wait for ordinary
interpreter commitment. Shared backing, EOF-unavailable pages and cross-page
operations interpret. Data contents may change concurrently. No unchecked
retained TLB pointer, managed callback or host-fault translation is used.

Native guards check the address, page, extent, read permission and authenticated
identity before issuing the instruction. A miss returns at its original IP with
no effect or issuance. The ordinary checkpoint then executes that instruction
once through `ExecuteOne`. Descriptor construction neither faults nor commits
lazy guest pages. Checked MOV32/64 stores and register64 PUSH/POP use the same private nonexec
leases, with exact write permission and guest-RSP semantics. Checked memory
CMP/TEST cover scalar 8/16/32/64-bit operands, register/immediate comparison,
reverse CMP, SIB and RIP addressing while retaining exact Blink flags.
Unprefixed direct CALL rel32 and RET near64 use checked private guest stack
memory and return through the dispatcher after updating guest RSP/IP. CALL stores
the guest fall-through address; RET reads the guest target. Neither emits a host
CALL or adjusts host RSP. Indirect/far/prefixed calls and returns, writing memory
ALU, atomics, REP, SIMD and x87 remain interpreted in this slice.

Blocks contain at most 32 static instructions in one page. Calls execute at most
32 dynamic instructions; simple internal loops return at a complete iteration
boundary within that cap. Forward conditional fall-through can stay in the same
bounded trace. Taken early branches return to the dispatcher even when their
target is a later trace label; only the final branch can loop internally. This
keeps each committed operation prefix, including memory-work accounting, exact. Every instruction polls stop/attention, atomically
issues against the shared budget, commits exact state and atomically completes.
There are no counter credits or suppressed GC transitions. Known exhausted
budgets retain the interpreter's rejected-issuance path. Private frame fields
materialize on every exit; guest state is current at every boundary.

## Entry-value address plans

The independently authored C# backend now retains a bounded value/address/effect
IR for admitted blocks. It propagates MOV, constants, LEA, stack deltas and
pure scalar integer expressions from entry registers, including width wrapping,
32-bit zero extension and partial/high-byte aliases. Descriptor preparation uses
the reaching value at each memory instruction. A prologue that copies RDI to
RBX therefore prepares the object page used by the later RBX load.

Loads, POP and RET produce unknown memory values. Preparation never reads guest
memory, forwards a stored value, commits a reserved page or assumes a loaded
pointer. Loop-written registers conservatively become unknown at an internal
backedge header; invariant overwrites may recover recipes. This can reduce stack
loop coverage and is a measured performance consideration. Native guards still
recompute every actual address and authorize each operation independently.

Plans retain before/after register definitions, flag inputs and explicit
poll/guard/atomic-issue/memory/commit/atomic-completion effects. Exit descriptions
distinguish pre-instruction stop/miss, rejected issuance and completed exits.
Guest state remains materialized at every instruction; the maps do not authorize
lazy register/flag writes. No QEMU code, extraction, linkage or dependency is used.

Known full-width affine recipes evaluate directly from at most two entry
registers and a displacement. Generic recipes retain width/alias/truncation
semantics; unknown load-derived values remain unknown.

A plan has at most 32 instructions/memory recipes, 512 value nodes, 80
definitions and 512 preparation slots. Its conservative maximum retained charge
is 20,280 bytes; evaluation uses at most 4,096 stack bytes and no managed
allocation. The owner charges actual retained lengths, including exit/effect
metadata. The emitter writes through bounded stack spans (16,872 bytes of buffer
payload), with one independent result array and no pooled owner references.
Layout checks run once per compiled emitter/producer type identity.

Sixteen bounded worker hints retain only scalar owner/IP/slot/generation/type
information. A data hint selects mmap-before-gate directly, then authenticates
under the gate before allocation or descriptor use. Stale hints use the ordinary
validated lookup. Register-only gate entry still requires an immutable enrolled
code lease. Positive and negative tables are separately bounded. Completed useful
work per charged byte and age guide positive retention; positive admission can
reclaim cold negative storage. Structural negatives persist only for the same
unchanged enrolled code identity. Expired mutable/revoked negatives decode again,
including when a relevant suffix changes behind an unchanged first 15 bytes.

The current implementation and actual qualification/performance result are
recorded in `artifacts/jit-implementation/ir-20261027-r1/`. The r16 measurements
below remain historical controls; new allocation or native-coverage results do
not establish service acceleration.

## Admission, accounting and ABI

Hotness observes executed branch/backedge and bounded continuation entries.
Default admission waits for sixteen visits and an internal loop or four static
instructions. A separate bounded compilation budget prevents a hotness threshold
from authorizing unlimited compilation. Four-way entries, two-way history,
positive/negative replacement and same-IP worker-local negative cooldown contain
fallback overhead. A known negative entry can defer observation for at most 256
interpreted instructions while IP stays within its 64-byte area; leaving that
area restores boundary observation immediately. Interpreter issue, stop and
signal checks remain per instruction. Four consecutive short continuation or memory-miss
exits below four completed instructions trigger a worker-local cooldown while
retaining the positive entry. Such dynamic short exits and early data misses use
at most 256 candidate observations of cooldown; structural negative decisions
retain their separate longer bound. `ShortNativeExits` records actual short work.
Threshold one bypasses this profitability response and remains a diagnostic override, not a service performance setting.

A bounded same-owner continuation experiment reuses the existing read leases
across at most eight authenticated cached leaves and at most 32 total dynamic
instructions. It uses materialized IR exit descriptions to check successor IPs;
RET targets still require normal cached-code authentication. A gate-only call
cannot continue into a data entry. It neither compiles a successor nor acquires
the mapping mutex while holding the gate. Each target prepares descriptors from
its actual current entry state. Stop, attention, guards and atomic counters remain
per instruction. Memory work is summed from each leaf's actual prefix, so an
arbitrary sequence never indexes one old linear prefix. `NativeDispatches` and
`Continuations` distinguish this from the number of native leaf calls.

The owner cache defaults to 8 MiB, configurable from 64 KiB to 64 MiB. Rounded
native allocations, tables, entries, enrollment metadata and conservative maximum
worker JIT metadata/poll storage are globally charged. Workers do not receive
independent 8 MiB caches. Statistics retain peak charge, compilation/fragment and
collision counts, first-child/native counts, revocations and fallback decisions.
Native memory instructions are counted from each block's exact committed dynamic
prefix, including repeated internal backedges; a guard miss excludes its missed
instruction. The bounded prefix metadata is charged with each cache entry.
`CacheBytes` becomes zero after successful teardown. Counts describe work, not a
CPU speedup forecast.

ABI v4 uses a 96-byte unmanaged frame and 48-byte descriptors. The native function
remains a leaf on SysV and Win64: no host CALL, stack adjustment, nonvolatile
register use or managed reference. Only volatile integer scratch registers are
used. Offset 60, formerly padding, carries the remaining invocation limit; native
entry clamps it to the compiled cap and every exit reports its exact work. The rooted owner and pinned counters live through the ordinary synchronous
interop transition; exceptions never cross an emitted frame.

Linux publishes separately allocated RW storage only after changing it to RX.
Windows retains VirtualAlloc/VirtualProtect/FlushInstructionCache/VirtualFree and
its conservative dynamic-code, CFG/XFG and shadow-stack policy checks. Guest
backing stays RW/NX. No mitigation is disabled. Windows native execution still
requires `AllowUnverifiedWindows=true` and remains **UNVERIFIED** without a real
Windows runner. PowerShell syntax or Linux tests do not qualify it. Linux
asynchronous stack walking through generated ranges also remains unqualified.

## Reproduction and evidence

Run `python3 blink/tests/GuestJit/run.py --output /absolute/new-attempt --aot
--emitter-contract --bench` on Linux; `--form raw` selects raw translation. The
runner builds ordinary ELF witnesses, freezes inputs, then builds and executes
serial qualification. It records an available producer pointer as provenance;
source-snapshot execution alone is not full producer qualification.

Adaptation changes additionally require a fresh strict producer, followed by
applicable raw/processed ordinary permission/mapping tests. This attempt used
`DOTCC_BLINK_FRESH_OBJECTS=1 bash blink/scripts/translate.sh --profile threaded
--offline --hashes strict --tools build --form all --jobs 1 --timeout 600`.
Windows assets accept the same frozen source and ELF fixtures and exercise both
public modes, both host runtimes and descriptor contracts on a real runner.

Current work and retained failures are indexed under
`artifacts/jit-implementation/ir-20261027-r1/`; the prior r16 handoff remains under
`artifacts/jit-implementation/sol-20261026-r2/`. The prior implementation index
and prior research report were absent on entry; historical numbers in retained
documents are context, not reverified receipts. The current Astra opportunity
report is `artifacts/jit-opportunities/astra-20261027-r2/report.md`.

Service acceleration requires actual native work after child publication and in
health/echo phases, paired serial CPU/wall measurements on identical guest
closures, at least 5% median CPU improvement with consistent paired direction,
and no material fallback regression. See the attempt's acceptance and final
report for the actual pass/fail result. Synthetic speedup or nonzero coverage
alone does not satisfy that gate.

The final r16 Linux attempt **failed service acceleration acceptance**. Five
alternating pairs on the identical NativeAOT guest measured warm-host whole-service
CPU overhead of 2.43% on the CoreCLR host and 1.85% on the NativeAOT host; wall
overhead was 1.86% on both. Only one of five CPU pairs improved on each host.
Approximately 6.3–6.6 million instructions executed natively after child creation,
with positive health/echo deltas, but this did not amortize entry, lease,
admission and compilation costs. The two selected full CoreCLR-guest controls
completed through hybrid execution with zero native instructions after their
first child; their anonymous JIT code remains interpreted. These are functional
compatibility results, not CoreCLR service acceleration.

Raw and processed final sources passed 729 public/state rows and 24,813 native
contract assertions per Linux host/runtime combination. This is a selected
experimental qualification, not full product qualification. Keep the backend
opt-in; use the durable attempt report for phase measurements, retained failures,
source/producer hashes, platform limits and exact reproduction commands.

Large reconstructible snapshots, binaries and traces use caller-owned scratch.
The entire `/mnt/storage/blink-jit-ir-20261027/` tree is disposable once its
README is marked COMPLETE. Compact reports, hashes, receipts and scoped rollback
source are retained in the real repository evidence directory. No required
product or test asset depends on that scratch tree after completion.

The IR/entry/continuation attempt also **fails service acceleration acceptance**.
Five alternating matched pairs measured warm whole-service CPU +0.84% on CoreCLR
(3/5 favorable) and -3.43% on NativeAOT (5/5 favorable), below the required 5%
reduction. Wall medians improved 2.54% and 3.52%. Compilation allocation fell to
about 12.4 MB per warm owner; selected predictable prologue misses became zero.
Those improvements do not establish the required CPU result. The declared
second-experiment stop condition applies: no additional ISA or private-library
enrollment was added. Keep this backend opt-in.

The final combined Linux raw/processed × CoreCLR/NativeAOT host matrix passed
936 public/state rows, 52,877 native contract assertions and 1,344 IR assertions
per cell, covering both public modes. All eight NativeAOT-guest public service
cells passed requests and cleanup. These are selected experimental checks;
Windows execution and Linux asynchronous stackwalking remain unqualified.
Use the current report for the retained failures, phase/fallback results,
component-measurement limits and remaining deterministic scheduling gaps.

All changes remain locally discardable. The current attempt retains exact initial
sources (including the pre-existing r16/prototype), final hashes, scoped diffs and
a guarded rollback manifest. Restore only listed matching bytes and remove only
explicitly added matching files after stopping owners; rebuild derived consumers.
Generated C and producer adaptations were unchanged in this managed milestone.
Never use a broad reset or clean. No commit, push, PR or branch change is part of
this work. The current `reproduce.py` rebuilds in new disposable caller scratch.
