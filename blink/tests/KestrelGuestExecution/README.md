# Actual translated Kestrel execution

The current optimized-JIT gate **passes** at
`../../artifacts/kestrel-guest-execution/attempt-nfmi2iou/receipt.json`
(SHA-256 `49502a64368a261c6803cb308561891e5da34c6e256f9c1ab9d4787d3a60f64d`).
The refreshed intrinsic/signal product executes the unchanged native Kestrel ELF:
all five HTTP comparisons, normal exit zero and all resource cleanup pass at
97,443,472 instructions with ten guest Machines. The original 100M/60s bounds
remain. The complete trace contains no `tkill` or `rt_sigreturn`; this pass does
not qualify activation-signal delivery. The separate worker matrix and normal
GuestSignals fixture cover the remaining lifecycle and signal requirements.
Historical failed attempts below retain their original evidence.


The original first gate ran only optimized JIT, using exact public threaded delivery
`attempt-i4a5mfa8`, genuine static Kestrel ELF from `attempt-o5jvvf7t`, and the
passing controlled native profile `attempt-zphi57zq`. Their receipt identities
identify the preserved first execution. The native guest/profile hashes remain
pinned in `run.py`; each public delivery receipt now requires its explicit
reviewed SHA-256 on the command line. The first execution produced a complete failed diagnostic:
the instruction budget was exhausted in guest GC before readiness.

```sh
python3 blink/tests/KestrelGuestExecution/run.py \
  --delivery-receipt blink/artifacts/translation/attempt-5vi_voys/receipt.json \
  --delivery-sha256 20d1eda5d5915c7ee38eb37f0c3e8ef5203a542a0c4d8d5aa20df92118b2c43a \
  --native-receipt blink/artifacts/kestrel-guest-musl/attempt-o5jvvf7t/receipt.json \
  --profile-receipt blink/artifacts/kestrel-native-profile/attempt-zphi57zq/receipt.json
```

The runner resolves and freezes the unchanged public generated library and its
original authored Host/adapters plus the original C# `ThreadedGuestExecution`
owner into a private build. The owner retains a 64 MiB default; this harness now
explicitly requests the reviewed 128 MiB profile. It never translates again or
repairs generated C#.
The direct InstanceIo owner accepts the 9.37 MB ELF under its existing 16 MiB
image limit; no worker/controller framing is involved. Coupled guest AS/DATA and
host backing ceilings are now 128 MiB, with 16 total guest workers and a 100-million instruction
budget, 60-second execution deadline and five-second join bound. Host build and
execution have separate finite process-group limits and isolated temporary paths.

A corrected public delivery must supply its own receipt path and expected
SHA-256. The runner still verifies the complete generated/raw/authored source,
compiler, staged profile and object closure; a new identity does not relax those
checks. The next corrected-delivery retry retains the original 100-million
instruction and 60-second limits. The 128 MiB ceiling is not a claim about physical
usage; its reservation evidence is below. Historical diagnostic snapshots remain intact.

Exactly six guest environment variables match the controlled native witness:
`LANG=C`, the prior three GC settings, `DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false`
and `DOTNET_EnableDiagnostics=0`. The last two are supported immutable-config and
diagnostics settings; normal Kestrel transport remains unchanged.

Real readiness is required before publishing the guest listener and making the
same five requests. Status/reason, all headers except a validated real RFC1123
Date value, and full body must match native responses. The fragmented request
uses the same seven consecutive writes. Ordinary HTTP stop must yield exit zero,
exact READY/STOPPED output and all guest Machines/workers/backing released.

Failures remain failed qualification. Result artifacts retain guest output,
actual execution termination/stop reason and per-thread syscall arguments/return
values (up to 16,384 rows per thread, with explicit truncation/count indicators).
The runner separately records whether a diagnostic result was produced, without
counting that as a successful guest. Source/tool/SDK/runtime/package/binary
identities are checked before and after execution. Raw JIT and both NativeAOT
host modes remain separate worker-matrix gates.

## First observed execution

`../../artifacts/kestrel-guest-execution/attempt-md2uqfje/receipt.json` has SHA-256
`95e9dcd1133dcc11d74c3fb5b1c359c6876dd6757dec041fc92a2a5f188d8e44`;
its result has SHA-256
`94c3073bf7d1119ed95c1cb261e46d316104a017cf2a58a3f8480559c97b9735`.
The private managed build passed in 14.60 seconds with zero errors and ten
CS8632 nullable-context warnings from unchanged authored bridges.

The actual guest did **not** reach READY or execute any HTTP comparison. It
stopped at exactly 100,000,000 instructions with stop reason Budget. Its main
thread executed 99,997,904 instructions and stopped at `0x44946d`; two children
executed 1,650 and 446 instructions. Main/child syscall traces contain 2,391/8/3
rows without truncation. Guest stdout/stderr and host diagnostics were empty;
there was no CLR or notification exception. All three Machines/workers joined,
backing storage was released and InstanceIo disposed. Before release, accounting
reported 12,857,686 bytes and 50 mappings. The host process exited one and its
group drained without signals. Qualification remains failed.

Read-only `addr2line` against the matching native producer's `KestrelService.dbg`
(SHA-256 `170bb1bad50c7f4585f4ebc0d0528274ca4cfd42480ac5246a1f0c621a8dd6ae`)
identifies the stop IP as `WKS::gc_heap::relocate_in_loh_compact()` in gc.cpp.
Disassembly shows an object descriptor/pointer traversal loop. The final main
syscalls were successful anonymous mappings of 4,096 and 16,384 bytes. This
narrows the observation to guest GC user code; one terminal instruction pointer
does not establish an infinite loop, corrupted GC data or a specific translator
error. No budget extension or host repair is credited by this result.

An independent audit verified 1,007 recorded input/binary/artifact identities
immediately after execution. The attempt retains immutable private source
snapshots, including this harness as it stood before the result was documented.
Later Host changes require a new coherent delivery or explicit use of that
private baseline; the original receipt must not be applied to changed sources.

## One larger-workload budget assessment

`budget-assessment.py` makes a separate immutable baseline copy and derives only
the authored test's instruction budget (100 million to one billion) and deadline
(60 to 120 seconds). It records the exact two-line diff and source hashes; guest,
generated library, Host and C# execution owner bytes stay unchanged. The runner
pins the original attempt's private manifests rather than reading changed live
Host sources. Its sole execution has an outer 140-second bound.

The approved assessment `attempt-budget-f4rifsku/receipt.json` has SHA-256
`c6bcda3589e3c29ef4d50a2f03e308efe0a660b1616456a36f531ac0bd88cc2d`;
result SHA-256 is
`62cc04eceea058fd4dc5581e2661688164ca151419d36148bc115709350005a2`.
It progressed beyond the previously observed GC loop, then ended before READY
with an actual guest `OutOfMemoryException` in `Hashtable.rehash(Int32)`, reached
through `UriParser`/`Uri` static initialization and Kestrel's host startup. The
guest raised SIGABRT (6) after 160,772,447 instructions in 13.54 seconds. Neither
the larger instruction budget nor its deadline was exhausted. There was no HTTP
comparison and qualification remains failed.

The three guest workers/Machines joined, backing memory was released and IO was
disposed; no CLR/notification exception occurred. Retained accounting before
release was 17,577,862 bytes/68 mappings. All traces (2,451/8/3 rows) are complete,
the owned process group drained without signals, and 321 immutable input, prepared
source, binary and artifact identities were independently verified. The private
build had the same ten existing nullable warnings and zero errors.

This disproves an inference that the first terminal GC IP alone established an
infinite loop. It does not explain the guest OOM: the equivalent native profile
passed, recorded retained backing was below 64 MiB, and no new mmap ENOMEM was
observed. No additional budget extension, memory-limit change or semantic repair
is credited by this assessment.

## Bounded allocation-register observation

`register-assessment.py` derives a diagnostic-only partial copy of the authored
owner in the historical private tree. It observes 16 GPR scalars at exact ELF
instruction addresses, including Hashtable expansion, bucket-array allocation
and terminal cleanup. It preserves errno and records observation failures without
changing guest control flow. Each group retains its first 16 and last 128 rows,
plus total/omitted counts. No guest memory is read or patched, and no generated
source, guest ELF, environment or execution budget changes.

Attempt `attempt-registers-ndumh6v1` receipt SHA-256 is
`a364e73be5f9f8d84fc9cd189029e4f4b8195fa6c3ea996cdbc3e032ac944b05`;
result SHA-256 is
`dbade26e44f707c9efa6d5f4582115d654be85a9b1963a49220a9c2e9b3219bc`.
It records 17 groups and 496 rows with zero omissions or observation errors.
Every one of 46 Hashtable insertion entries triggers expansion. The final URI
table grows through 36,353, 75,431, 156,437, 324,449 and 672,827 buckets while
entry count is only 11 through 15. The final positive length 672,827 computes an
allocation of 16,147,872 bytes; the slow allocator returns null, followed by the
same guest OOM/SIGABRT before readiness. This identifies excessive table growth,
not an initially negative array length. The instruction count is 159,751,951;
execution took 20.69 seconds, all guest resources released, and 325 identities
were independently checked.

Read-only source inspection found a concrete candidate in the pinned upstream
`blink/ssefloat.c`: `OpCmppsd` assigns comparison results to the floating member
of its union. For an ordered scalar comparison, this converts integer -1 to
float -1 (bits `bf800000`) instead of producing the required all-ones comparison
mask. Generated C# faithfully preserves this upstream behavior. The actual guest
uses CMPORDSS then ANDPS during Hashtable load-threshold conversion. A separate
bounded GPR/XMM observation is prepared to identify the first incorrect value;
no source correction is credited by the allocation observation alone.

The pinned [.NET 10.0.12 Hashtable source](https://raw.githubusercontent.com/dotnet/runtime/v10.0.12/src/libraries/System.Private.CoreLib/src/System/Collections/Hashtable.cs)
computes the load threshold from bucket length times the adjusted load factor,
and expands when count reaches that threshold. For default load factor 0.72,
lengths three and seven should yield thresholds two and five respectively.

## Exact first incorrect comparison-mask value

`float-register-assessment.py` uses the same historical baseline and bounds,
adding only diagnostic GPR/flags and XMM0/XMM1 register observations at the
constructor/rehash threshold instructions. It reads Machine register storage,
not arbitrary guest memory. The generated library and guest remain unchanged.

Attempt `attempt-float-registers-smhynls8` receipt SHA-256 is
`b10f27addb1cfaa4b66f89699f42b222ce028f9aa6c30da8d907215afd3754bc`;
result SHA-256 is
`a26835a15d0370ef9aea10e39c88238f14bacc6b38168eddd9db45e019dff069`.
All 458 rows in 20 groups were retained without observation error or omission;
325 immutable identities were independently checked.

The first constructor observes the correct load factor `3f3851ec` (0.72f),
length three, and product `400a3d71` (approximately 2.16f). CMPORDSS at `0x6b16a1`
then produces **`bf800000`**, observed at `0x6b16a6`, instead of all-ones
`ffffffff`. ANDPS produces zero, CVTTSS2SI produces integer zero, and the store
at `0x6b16bc` receives EDI zero instead of the expected threshold two.

The first rehash likewise computes the correct length-seven product `40a147ae`
(approximately 5.04f). CMPORDSS at `0x6b19a9` produces `bf800000`; ANDPS reduces
the product to `00800000` (a tiny positive float), integer conversion produces
zero, and the threshold store at `0x6b19c4` receives EAX zero instead of five.
All 51 observed constructor/rehash comparison masks show this defect. This
connects the pinned upstream union-member assignment to the actual zero load
threshold and repeated expansion; the preceding conversions and products are
correct for these observed operands.

The diagnostic still fails guest qualification: no READY or HTTP case, the same
guest OOM/SIGABRT after 160,695,525 instructions in 24.00 seconds, and all guest
workers/Machines/backing/IO released. No production fix or broader floating-point
coverage is claimed by these observations. Earlier receipts and snapshots remain
unchanged. No further observation runs were performed.

## Corrected product: ordinary Gate-thread reservation exceeds 64 MiB AS

The corrected public delivery `attempt-tf_6yqk6` (receipt SHA-256
`92763476d1719166912ecbf2d5ca31cabb1feb512cfcef278a288319980524a5`)
contains the repaired SSE comparison masks and qualified async socket Host.
Its first unchanged 100-million/60-second Kestrel run, `attempt-7lswgkg3`, has
receipt SHA-256 `e7e2075e1b6b6088724093069e592dd7d8a7fbb23bd35c720094b2995c4e4af7`
and result SHA-256 `5fe079f63162191852f93819771e74bc82605eb02b8e074151b8a7bf418ce544`.
The private build passed in 19.02 seconds with zero errors and the ten existing
nullable warnings. The actual guest failed before READY or any HTTP comparison,
writing `Process terminated. Failed to create the thread pool Gate thread.`
after its sole new mmap ENOMEM. It later exhausted 100 million instructions
while constructing the failure stack trace. All six Machines/workers joined,
backing and IO released, and the process group drained without signals. The
1,000 recorded input, binary and artifact identities were independently checked.

`reservation-evidence.py` reads only the preserved receipt/result, exact ELF,
native trace, immutable generated/owner source and pinned resource/loader/memory
sources. Every input has a required SHA-256. It emits all mapping events, merged
active intervals, ELF program headers and the following arithmetic as JSON:

```sh
python3 blink/tests/KestrelGuestExecution/reservation-evidence.py > /tmp/kestrel-reservations.json
```

| Recorded reservation component | 4 KiB pages |
| --- | ---: |
| Active mmap intervals after successful munmaps | 11,436 |
| Heap pages not already covered by MAP_FIXED | 4 |
| ELF PT_LOAD union, including BSS | 2,887 |
| Loader's fixed 8 MiB main stack | 2,048 |
| Reconstructed total | 16,375 |
| Original RLIMIT_AS ceiling | 16,384 |

Only nine pages (36,864 bytes) remain, while the failed PROT_NONE anonymous
Gate-stack reservation requests 67 pages (274,432 bytes): 58 pages (237,568 bytes)
beyond the limit. `GuestResources.c` initializes both AS and DATA from the owner
ceiling. Pinned `SysMmapImpl` checks RSS and then `size / 4096 + vss > GetMaxVss`
before `ReserveVirtual`; the generated code preserves these checks. PROT_NONE
reservations count toward virtual size independently of touched backing. The
native trace explicitly reports unlimited AS, succeeds at the identical stack
reservation, then clones the `.NET TP Gate` thread.

This is a reconstruction, not a direct vss/rss snapshot. Per-thread traces have
no global ordering; worker 262146 omits its last 3,664 of 20,048 calls. Its
recorded calls contain no mapping changes, and the sole other child mapping is
a disjoint 16 KiB interval. The arithmetic establishes insufficient VSS admission
for the recorded active set, without claiming which early ENOMEM branch fired
or excluding simultaneous backing pressure. The 6,826,350 retained bytes were
measured after upstream Machine cleanup and do not establish peak usage.

The next reviewed profile requests a 128 MiB ceiling through the authored owner,
coupling AS/DATA and backing as before. It retains 100 million instructions,
60 seconds, 16 guest workers, the unchanged ELF, all six native environment
entries and the 16 MiB GC cap. The default owner profile remains 64 MiB. This
profile's first execution is recorded below; full qualification remains failed.

## 128 MiB profile: READY and three responses, then activation-signal rejection

Attempt `attempt-uq52p1wf` has receipt SHA-256
`20727ac9628d261882dfd21e9efe3aee271c1b80cb7b820c93bb0876960cb04d`
and result SHA-256
`b12eddb8735c5b1a19b805776d09f28b54249bea01a8063d17f8b1dc0e57de4d`.
The private build passed in 12.28 seconds, with zero errors and the same ten
nullable warnings. The unchanged guest reached `READY 8080` and passed the
health, 3,592-byte large and seven-write fragmented request comparisons against
the native response semantics. Each retained response has 140 bytes, including
a syntactically valid Date. There was no mmap ENOMEM in this run.

The guest still failed. Worker 262146's complete trace records, at index 13,785,
`tkill(262150, 35)` returning `-95` (EOPNOTSUPP) from syscall IP `0x4ebb42`.
The pinned debug ELF identifies this instruction as musl `pthread_kill`.
The worker then invokes `tkill(262146, 6)` at index 13,788 and terminates with
SIGABRT. The immutable authored owner's `IHostGuestThreads.Signal` explicitly
returns 95 for every nonzero cross-thread notification. Native and translated
startup both install signal 35; native's handler address `0x463b20` resolves to
`ActivationHandler` in NativeAOT `Runtime/unix/PalUnix.cpp`. The saved native
witness installs this handler but does not itself issue a tkill35 call.

The harness was awaiting the missing-route response when its 60-second deadline
expired; it never attempted normal HTTP stop. Actual instructions total
89,780,692, below the 100-million bound. The final owner stop reason is Deadline,
while the execution result preserves StopReason None and the earlier worker
SIGABRT. These distinct outcomes do not establish ordinary guest shutdown.
Stdout contains only READY; stderr is empty. All nine Machines/workers joined,
backing released and IO disposed; the outer process group drained without
signals. Every per-thread trace is complete, and all 1,005 input, binary and
artifact identities were independently verified. The all-mode worker matrix
remains held, and no further limit increase or retry is credited.
