# Selected explicit class-template COMDAT providers

The [final receipt](../acceptance/duckdb/template-provider-report.json) passes
all 22 expected outcomes: 17 successes and five intended rejections, with no
unexpected outcomes or budget violations. It is 160,887 bytes, SHA-256
`8855f8290c41a0ee5313d9995675e0353fe35c6be2d5c2aaf492a12fd3996597`.
Evidence pins product `template-provider-product-source-1` (273 files), checker
source 1 (eight files), four genuine fixture sources and both immutable inputs.
Runtime, IR and NoThrow behavior are unchanged.

Selected manifests separately opt in with
`link.definitionSelection = "captured-template-comdat-manifest-order"`.
The existing `strong-only` and `captured-inline-comdat-manifest-order` meanings
remain unchanged. The new branch requires a usable defined, unsuppressed COMDAT
body and exact captured explicit class/member instantiation provenance: concrete,
explicit, nondependent, nonprototype, nonspecialized member and class instances;
member-function/class template kinds; recorded explicit-instantiation flags; and
reciprocal instance Result/OriginalDefinition identities for the actual function
and containing complete record. Deleted/do-not-instantiate, weak/alias/ifunc,
unsupported entry/adjustment and existing signature/use restrictions still reject.
No template pattern, source name or inline spelling substitutes for these facts.

A unique eligible provider binds through exact captured linkage and public ABI
comparison. Duplicate selection remains a separate manifest-order representative
operation with the existing ABI/specification/address/storage/ambient checks;
neither operation validates C++ ODR or body equivalence. Constant storage still
needs its independent opt-in, and owner-only cleanup tables retain their existing
metadata classification rather than becoming shared application state.

The independent extern-template consumer calls the genuine provider's out-of-class
C1/D1. Actual capture records both provider bodies as non-inline, COMDAT,
defined and unsuppressed; consumer entries are declarations. Native, JIT and AOT
each match two complete observations: result 30, values
`[7,1,11,2,19,1,2,2,0]`, events `[107,111,530,311,319]`, constructed/destroyed
2/2 and live/overflow zero. JIT/AOT entire stdout is identical. Actual fixture
scopes execute construction/destruction; the checker makes no manual lifetime
calls. Both emitted TU carriers physically measure size/alignment 4/4, value
offset 0, sequential-prefix offset 4 and stride 4, with aligned field addresses
and independent stores. Those raw CLR probes make no C++ lifetime claim and
leave all application observations unchanged.

Fresh publication under the new policy preserves source/native-ABI bytes for
the existing valid duplicate-inline fixture and promoted constant-storage fixture;
the prior suppressed-provider fixture also stays identical under its old policy.
The duplicate-inline native/JIT/AOT proof keeps its original closed receipt and
Runtime attribution through verified source/Runtime identity. No duplicate
explicit-instantiation definitions were manufactured. Four policy controls
reject: the new fixture under old inline-only and strong-only policies, genuine
strong duplicates under the new policy, and this opt-in in retain-all mode.

Actual three-input core293/core300/string-support selection advances through the
`_M_create` provider, then rejects ROC1214 at `StringUtil::Lower`'s retained
`indirect_variable` destructor dependency. It remains partial and unpublished
(21 bodies/979 operations); dependency-accounting follow-on work is separate
from this proof and grants no cleanup execution permission. Implicit, specialized
and free-function template cases are not newly covered. Full string execution,
real startup/guard/JSON/allocator/EH closure, whole DuckDB and broader T5 remain open.
