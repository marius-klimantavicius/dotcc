# Complete constructor/destructor callable exception contracts

The native118 DuckDB pair parquet_reader.cpp/parquet_types.cpp disagrees on
FileMetaData's generated complete constructor (C1): the imported lowered entry
has `unwind`, the defined entry has `terminate`, and both original selected
constructors explicitly have `noexcept`. Existing `abi_routine_origins` identifies
the exact primary C2 and `source_routine_types` retains its resolved policy.

This is intentional stock lowering. `lower_init.c:5738-5753` copies the original
exception specification to the generated complete entry only when a definition
has virtual-base initializations/destructions needing exception handling.
`define_default_version_of_routine` at4897/4981 moves that virtual-base work into
the complete entry; the forwarding body later calls the primary entry. The C
backend emits those exact lowered bodies. Imported complete entries have no
owned initializer body, and consequently no local exception specification.

The managed correction uses two distinct existing facts:

* Raw routine exception policy continues to describe the emitted body boundary.
  Neither native types nor their IDs nor body emission are rewritten.
* For a verified complete constructor or destructor origin only, callable policy
  is derived from its exact original constructor/destructor signature. Link resolution uses this
  contract together with all physical parameter/result types. Source signature
  comparison still verifies receiver, original parameters, ABI roles and policy.

No function-pointer type key is globally aliased and no arbitrary wrapper policy
is ignored. Deleting/delegation/subobject/default-argument entries remain on their
existing raw-policy path. A complete entry with a terminate execution boundary
but a throwing original contract is rejected. An unresolved complete constructor/destructor
extern receives the callable noexcept wrapper; reports expose both policies.
Existing origin and source rows prove this relation, so no duplicate schema is
introduced.

The focused pair uses a virtual base initialized outside the primary constructor.
The caller catches exceptions and returns7 if the noexcept boundary is lost.
Native and stock C O0/O2 terminate for the intentional throwing path. The old
managed133 linker rejects the pair after25 successful oracle/metadata checks.
The updated acceptance runner also tests JIT/NativeAOT termination, both object
orders, unresolved extern enforcement and resealed source/origin negatives.

## Accepted validation

Native120 / managed135 passed all45 focused checks, including JIT and NativeAOT
normal execution and termination when the virtual-base initializer throws,
source-hidden verification, both input orders, callable noexcept for unresolved
externs and resealed conflicting-boundary/origin negatives. The fixture supplies
real free-backed sized and unsized deletion for the shared RTTI closure.

Receipt: `/mnt/storage/dotcc-edg-campaign-20261003/dotobj-v49-constructor-120-135-002/results.json`.
The older118/133 run remains retained as the exact pre-fix failure reproduction.
The targeted31-object inventory found this discrepancy only in complete
constructors for `FileMetaData` and `FileCryptoMetaData`; full combined DuckDB
linking is a separate acceptance requirement and is not claimed by this fixture.

## Complete destructor extension

After resolving the constructor mismatch, the real combined120 objects expose
`RowGroupD1` with the same absent-import/copy-to-definition distinction. The
stock complete destructor owns virtual-base destruction after its D2 call
(`lower_init.c:5326-5357`). That epilogue is subject to the original destructor's
exception contract just as virtual-base initialization is subject to the original
constructor contract. The managed rule therefore also admits the exact
`complete` + original `destructor` origin. The deleting entry D0 remains excluded:
it has additional deallocation work after its complete-destructor call.

`duckdb_destructor_contract_verify.py` verifies a separate paired-TU case with
throwing virtual-base destruction and a caller catch that must never receive the
exception. Native120 / managed136 completed 48 checks. This combines the38
retained native/stock/JIT/NativeAOT and metadata checks with corrected exact
boundary, deleting-entry exclusion and origin negatives; it does not count the
two superseded mutation checks from the earlier failed receipt.

Combined receipt:
`/mnt/storage/dotcc-edg-campaign-20261003/dotobj-v49-destructor-tail-120-136-002/results.json`.
The receipt records source object, linker and prior execution receipt hashes.
The source artifacts and JIT/AOT outputs remain in
`/mnt/storage/dotcc-edg-campaign-20261003/dotobj-v49-destructor-120-136-001/`.
