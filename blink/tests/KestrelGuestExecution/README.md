# Actual translated Kestrel startup diagnostic

This first gate runs only optimized JIT, using exact public threaded delivery
`attempt-i4a5mfa8`, genuine static Kestrel ELF from `attempt-o5jvvf7t`, and the
passing controlled native profile `attempt-zphi57zq`. Their receipt identities
are pinned in `run.py`. The first execution produced a complete failed diagnostic:
the instruction budget was exhausted in guest GC before readiness.

```sh
python3 blink/tests/KestrelGuestExecution/run.py \
  --delivery-receipt blink/artifacts/translation/attempt-i4a5mfa8/receipt.json \
  --native-receipt blink/artifacts/kestrel-guest-musl/attempt-o5jvvf7t/receipt.json \
  --profile-receipt blink/artifacts/kestrel-native-profile/attempt-zphi57zq/receipt.json
```

The runner resolves and freezes the unchanged public generated library and its
original authored Host/adapters plus the original C# `ThreadedGuestExecution`
owner into a private build. It never translates again or repairs generated C#.
The direct InstanceIo owner accepts the 9.37 MB ELF under its existing 16 MiB
image limit; no worker/controller framing is involved. Existing backing/worker
limits remain 64 MiB and 16 total guest workers, with a 100-million instruction
budget, 60-second execution deadline and five-second join bound. Host build and
execution have separate finite process-group limits and isolated temporary paths.

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
host modes remain separate future gates.

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
