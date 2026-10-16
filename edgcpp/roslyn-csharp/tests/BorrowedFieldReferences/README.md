# Borrowed field-reference checker

Both independent production publications and preliminary JIT comparisons passed.
This checker is bound to their actual published `Translated.Owner`,
`Translated.Member` and `Translated.Program.view(Owner*)` names. Its physical
and NativeAOT acceptance remains open until the coordinator runs the frozen
checker against each separately compiled publication.

Build this source separately against each independent `GeneratedProject`, with
separate checker output directories/binaries. The same native JSON bytes cannot
identify which source was compiled: each invocation therefore requires both
its native oracle and its own frozen capture provenance:

```text
Checks NATIVE_OBSERVATIONS_JSON CAPTURE_PROVENANCE_JSON
```

For the original capture these are
`native-borrowed-field-reference-run-1/stdout.txt` and
`borrowed-field-reference-capture-provenance-1.json`; for the mutable companion
use `native-borrowed-field-reference-mutable-run-1/stdout.txt` and
`borrowed-field-reference-mutable-capture-provenance-1.json`. Paths are under
`/mnt/storage/roslynobj-csharp-20261015/t5/`.

The checker verifies native report content against the supplied provenance,
accepts only the exact original/companion source/input/provenance hashes, and
reports that attribution independently. The coordinator receipt must correlate
each checker binary/GeneratedProject/publication completion with its actual
input hash. Supplying a provenance argument is not independent proof of what
was linked, and it never changes application behavior or selects a method.
Output order is the native-compatible JSON first,
`borrowedFieldReferencePhysical` second, then `suppliedCaptureProvenance` third.

Each fixture runs twice. Every result, all nine values and every event must
match its complete native `runs` object and repeat exactly. Both native inputs
currently report result 36, values `[5,7,11,13,1,1,1,1,0]`, events
`[100,205,307,400,511,613,704]`, zero overflow and protocol live. The entire
observation is compared; matching only a sum or alias boolean is insufficient.
Explicit JsonNode arrays and output avoid reflective DTO serialization and
are AOT-safe.

Zero live/unused slots are protocol only. The const reference borrows storage
in an existing caller-owned object; this checker proves no ownership transfer,
lifetime extension, destruction or cleanup. It introduces no reader, runtime,
provider, native application fallback or native callback.

Independent typed physical checks inspect complete Member size 8, fields at
0/4 and sequential-prefix alignment 4; Owner size 16, canary/payload at 0/8
and sequential-prefix alignment 8. Actual stack-array strides are 8/16 and
element addresses must have their corresponding natural alignment. Both owners
are fully initialized with distinct fields and full-width signed canaries.
The actual translated getter is invoked on each. Its two returned pointers
must be distinct and equal the corresponding payload fields at offset 8,
expose their initial values, then reflect fresh mutations of each owner while
both 64-bit canaries remain intact. These are direct typed accesses; no
reflection or source-derived getter replaces the published method.

The physical JSON records `memberSize`, `firstOffset`, `secondOffset`,
`memberSequentialPrefixOffset`, `memberArrayStride`, `ownerSize`, `canaryOffset`,
`payloadOffset`, `ownerSequentialPrefixOffset`, `ownerArrayStride`,
`firstReturnedOffset`, `secondReturnedOffset`, `memberAddressesAligned`,
`ownerAddressesAligned`, `exactFieldPointers`, `returnedPointersDistinct`,
`ownerMutationVisible` and `canariesPreserved`. A failed assertion prevents a
passing physical result; writing this checker is not execution evidence.

The original direct const field-place return and companion explicit
qualification adjustment remain separately attributed. Neither proves general
reference-return/native ABI support, no-throw cache permission, ownership or
DuckDB class/string/base/link closure. Previous checkers and both frozen
fixtures remain unchanged.
