# Multiple-inheritance dispatch checker

Build `Checks.csproj` with
`-p:GeneratedProject=/absolute/path/to/Generated.csproj`, then pass the frozen
separate native observations JSON as the sole runtime argument. The coordinator
owns builds, JIT/NativeAOT execution and receipts. The source names here match
the actual production CLI publication, not a reconstructed source ABI.

Two generated fixture invocations must exactly match the native `runs` object:
result 109, all nine values, all 18 events, overflow/live 0 and prior-live 0.
The checker reads the oracle JSON instead of replacing it with a predicted
event array. Physical audit fields are appended after that full comparison.

The physical gate verifies complete Left/Right/Derived sizes 16/16/32,
complete alignment 8, occupied view sizes 12, base offsets 0/16, ids 8/24 and
bonus offset 28. The occupied views borrow aligned complete owners; no
standalone CLR alignment or by-value ABI is claimed for those views. Actual
generated constructors create two complete objects of each type, and actual
destructors release all corresponding live units in a finally block.

All five loaded canonical Cdecl UCO entries are nonzero, distinct, stable
across objects and **actually executed**:

| Loaded entry | Actual receiver | Native comparison |
| --- | --- | --- |
| Complete Left slot 0 | Complete Left pointer | Base Left result/event |
| Complete Right slot 0 | Complete Right pointer | Base Right result/event |
| Derived primary slot 0 | Complete Derived pointer | Derived Left result/event |
| Derived primary slot 1 | Complete Derived pointer | Derived Right result/event |
| Derived secondary slot 0 | Actual Right-subobject pointer at +16 | Same Derived Right result/event, distinct entry |

The final call passes the raw secondary-subobject address. The generated thunk
body performs its retained -16 adjustment; the checker does not pre-adjust the
receiver. Both right-method paths must yield the same native result and event
while preserving both base fields and the derived bonus. Generated table
address points at byte offsets 16/48 and signed offset-to-top constants 0/-16
are checked in actual memory. No direct managed-method substitute is called.

Exactly two borrowed providers are selected in this input: class metadata and
VMI metadata. The earlier SI descriptor stays outside this input's diagnostics.
For each selected export the checker verifies pinned path/hash, default ELF
version metadata, audited RVA and 88-byte provider extent, independent
GetExport identity and actual `/proc/self/maps` read-only/non-executable owner
range. The provider reference remains held for process lifetime. Generated
Left and Right typeinfo vptrs must each equal the class export base plus 16;
the anonymous generated Derived typeinfo vptr must equal the VMI export base
plus 16. Captured raw metadata flags/count/base-info fields are also preserved.
This inspects generated initialization bytes without executing an RTTI query.

The provider's 88-byte ELF extent does not complete an EDG declaration type.
`CXXABI_1.3` is pinned default-export metadata, not a versioned runtime lookup.
No native RTTI table method, native application body, virtual inheritance,
virtual destructor, covariant return, member pointer or native unwind is
admitted. Output adds `physical` and `providers` to `runs`; process-specific
addresses are audit values, not cross-process equality requirements. JSON is
constructed explicitly for NativeAOT.
