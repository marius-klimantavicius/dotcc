This diagnostic reads the actual generated library's optimized assembly. It
does not translate sources or build the compiler/product. It records aggregate
helper parameter counts and IL sizes, checks every immutable byte/ushort/uint/
ulong payload getter for direct PE RVA backing and absence of `newarr`, and
executes warmed getter reads/copies while measuring managed allocations. It also
executes every zero-argument `Create()` returning a closed unmanaged value up to
1 MiB through a strongly typed generic delegate. Reflection and delegate setup
occur once outside measurement. Every result byte feeds a no-inline checksum
that is written to a volatile sink. Values of at least 4096 bytes use 1000
iterations; smaller values use 10000. Ordinary reusable IL locals hold values;
there is no per-iteration array, boxing or stack allocation.

Build the audit only when the supervisor releases the project lock:

```sh
dotnet build libghostty/tests/AggregateFactoryAudit/AggregateFactoryAudit.csproj \
  -c Release --artifacts-path libghostty/build/aggregate-factory-audit
```

Run the resulting `GhosttyVt.AggregateFactoryAudit.dll` with these five arguments:

```text
<actual Release TranslatedGhosttyVt.dll>
<canonical generated source directory>
libghostty/artifacts/aggregate-factories-before.json
<ignored audit output.json>
2
```

The measured loop excludes type initialization, delegate creation and allocation
of its reusable copy destination. The report identifies those exclusions and
contains exact getter hashes, IL byte sizes, matching RVA field counts and
allocation deltas. This establishes PE storage and executed JIT getter behavior;
it also records factory allocations and recursively audits generated factory,
copy helper, view and getter IL for `newarr`, ordinary reference constructors,
known allocation services (including `GlobalArrayFrom`) and unresolved/indirect
calls. Value-type constructors and guarded exception constructors are listed
separately. Unsupported returns and errors are explicit report entries and fail
the audit. Factories with arguments receive IL/callgraph inspection but no
invented argument values or runtime claim.

This does not claim NativeAOT or terminal qualification. Use a separate invocation
for each product form. Compare only an assembly built from the reported canonical
sources; the caller must coordinate that build and its receipt.

The auditor's two allocation measurement methods use `NoInlining` and
`AggressiveOptimization`. The standalone product audit additionally warms the
complete getter set through a full first pass, retaining its measured runtime
allocation total in the JSON. Acceptance uses the next complete pass with the
same strict zero-byte condition. Emitted methods and default process tiering
remain unchanged. Early one-pass reports observed one 6,192-byte allocation at
varying getters (4,264 bytes after optimizing the measurement loop), despite
RVA/no-allocation IL. Disabling tiering or completing the full-set warmup removed
it. Those failed reports are retained; no allocation threshold is relaxed and
no emitted source is changed for the audit.
