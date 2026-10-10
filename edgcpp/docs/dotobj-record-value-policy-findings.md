# DuckDB record-value and deferred ABI findings

Three v39 failures had different causes. None required changing EDG semantics.

## Discarded places in string_util.cpp

Native084 packaged `string_util.cpp`, but managed090 rejected `Split` with
`Record value operation lacks its required construction, transport or assignment permission`.
Object: `dotobj-duckdb-frontier-084/ad9e4b547df1e1f11f8f6e3d-001/unit.dotobj`
under `/mnt/storage/dotcc-edg-campaign-20261003`.

Node 862 is a value-producing sequence. Its first child 830 is a dereference
place of source record 160, `std::__dotgen_v1::basic_istream`, which correctly
has no bitwise construction/assignment permission. Its second child is the
actual conversion result. The first object is discarded, not copied.
`c_gen_be.c:6460` emits the comma operator and checks that operand one's result
is unused. The managed verifier incorrectly imposed transport permission on
that discarded place, and the emitter's fallback would have read the struct.

The v40 fix admits existing record places only in discarded sequence prefixes,
expression statements, and explicit void conversions. Emission evaluates the
place's address/effects, once, without loading/copying its record value. Actual
return, assignment, argument, and materialization contexts retain their checks.
Reducer: `duckdb_discarded_record_place.cpp`.

## Parameter transport differs from return transport

Native084 rejected `exception.cpp` while visiting the libc++ pair piecewise
constructor at the configured `__utility/pair.h:279`. The same error reproduces
with a standalone `void consume(std::tuple<int&&>)`.

The successful reference-only tuple reducer exported these facts for `tuple`,
its `__tuple_impl`, and `__tuple_leaf`: bitwise construction allowed, assignment
not allowed, return storage required. They were not stale or inconsistent.

`il.c:10752`, `set_arg_transfer_method_flag`, makes a parameter indirect when
bitwise construction is disallowed or (outside cfront mode) its destructor is
deleted/nontrivial. `il.c:27735`, `type_returned_by_cctor`, additionally requires
return storage when a copy/move constructor is deleted. A deleted copy and
trivial move can therefore allow a direct parameter but require an indirect
result. Our parameter checks incorrectly reused the return restriction.

The v40 snapshot adds `requiresParameterStorage` independently of
`requiresReturnStorage`, using the exact stock parameter predicate. Argument
edges and physical parameter signatures use the argument policy; result
signatures retain the return policy. Generic temporary transfer is unchanged.
Reducer: `duckdb_record_argument_transport.cpp` explicitly exercises both
direct parameter and indirect result for the same original class.

Stock EDG successfully lowered the real `exception.cpp` after preprocessing
with native084's exact target macros for this investigation. Generated C at
`duckdb-record-value-policy-design-001/exception.c:9727` and `:15634` shows the
piecewise constructor's tuple parameters passed by value. The preprocessing
step is an investigation oracle, not a production pipeline change. Original
commands, unsuccessful stock target-setup attempts, source, logs, and tuple
wire snapshots are preserved in that artifact directory.

## Incomplete callback result types in dictionary/common.cpp

Object `dotobj-duckdb-subsystems-080/c44bdfa433e360b627c96857-001/unit.dotobj`
has routine types 204/269 returning incomplete `unique_ptr<AnalyzeState>` and
`unique_ptr<CompressionState>` (types 203/268, source records 141/184). Source
routine types 346/446 retain their original signatures. No entity uses either
routine as its own callable signature, and no executable node uses those
function-pointer types. They occur in `CompressionFunction` fields
`init_analyze` and `init_compression`, physical pointer types 205/270.

`il.c:27788`, `set_routine_calling_method_flag`, explicitly permits incomplete
return information and schedules a later fixup. Observing routine lowering
does not prove that an unused incomplete result already has a callable ABI.
The managed global routine check had mistaken those field declarations for
completed callable signatures.

The v40 representation retains the full routine/pointer and source-type rows,
with `deferredRoutineAbis(routineTypeId, sourceRoutineTypeId, reason,
captureProof)` marking the observed incomplete value signature. Such pointers
can occupy storage, but indirect calls remain rejected in that object. No
source declaration, type, or original callback signature is dropped.

The linker selects a completed counterpart by the exact original semantic
routine signature. It normalizes execution types for record-layout comparison
and emitted callback types, and rejects competing completed ABI shapes. If no
counterpart exists, the field is emitted as opaque `void*` storage with its
full original signature still in the object/source metadata. Actual function
definitions and calls continue to require complete ABI proofs.

The paired `duckdb_deferred_routine_abi_first.cpp` / `second.cpp` reducer checks
an incomplete result in one TU and a completed nontrivial-copy result using a
result buffer in the other. Its runner includes standalone storage-only output,
both link input orders, source-hidden linking, JIT/AOT execution, and rejection
when deferred provenance is removed. v40 acceptance runs are pending the
coordinator's combined build.
