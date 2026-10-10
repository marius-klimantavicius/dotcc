# Virtual array deletion provenance

Profile50 preserves the stock IA64 array-delete protocol as owned semantic
nodes. It records the exact selected original destructor/deallocator, element
layout, padding and callback selection. `callback_kind` distinguishes a direct
destructor address from `virtual-cookie-guarded`; the latter has a required
`array_deletion_virtual_callbacks` row with callback node, vtable index, cookie
size and stock observation proof. All symbols and expression edges remain IDs.

Stock `lower_init.c::lower_array_delete` saves the array pointer once and calls
its vector-delete helper only for a nonnull array. It reads the count cookie
before the first element's vptr, passing a null destructor for zero elements.
Otherwise it selects the complete destructor through the owned vptr/base path.
The verifier checks the same saved binding in the outer guard, helper, cookie
and vptr paths; exact cookie width/displacement and zero arm; exact vtable slot;
selected destructor identity and actual ABI; and the unchanged helper ABI.
Pointer casts cannot disguise arithmetic truncation or an integer round trip.

Stock can form the intermediate virtual-function address before finalizing the
destructor receiver parameter. ICU gender.cpp exposes this original member view
`void()` even though the selected destructor's finalized ABI is `void(Element*)`.
Only this exact original no-parameter member signature, with matching original
receiver/exception policy and complete destructor ABI, is admitted inside the
observed callback protocol. General function-pointer checking is unchanged.
Original complete destructors need no generated-entry row; generated entries
must have an exact complete-entry origin. Source records and execution types
remain separately preserved.

Native121/managed140 pass default169 and late-definition195 focused checks,
including native/stock O0/O2 behavior, source-hidden linking/generation and
resealed protocol/guard/cookie/vptr/signature rejection cases. Existing direct
vector117 and array/VTT42 regression checks pass. The new virtual suites do not
claim managed execution: production vector-new/delete providers are a later
runtime closure requirement. No fabricated test implementations were added.

All17 measured ICU array-delete exporter gates now package, verify and generate.
Together with five prior managed rejects and one cleared record-copy followup,
23 actual DuckDB units plus two translated RTTI providers link to5,943 function
definitions. Review: `artifacts/review/dotobj-duckdb-v50/combined23`. The complete
DuckDB project has not yet generated, compiled or run. Exact paths/tool hashes
and acceptance receipts are in
`evidence/dotobj-duckdb-virtual-array-provenance-20261010.json`.
