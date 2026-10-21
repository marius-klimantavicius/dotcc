# String values, arrays and constant addresses

The bounded integrated gate passes120/120 groups under JIT and Linux-x64
NativeAOT with zero skips and warning-free Release. Four round4 receipts share
118 current files including Unicode data3 and tools3; three output artifacts
and nine consulted upstream files are fingerprinted. Independent final review
is `../evidence/review-strings-final.md`. This does not complete M1 or full PLAN.

| EDG branch | Handwritten contract |
| --- | --- |
| literals.c conversion/concatenation2204–2747 and single wide rescan1743ff | Typed ordinary/u8/u/U/L units, raw spelling, embedded zeros, one final terminator; deferred ordinary diagnostics and converted-byte widening without a second escape pass |
| lexical.c adjacent literals15980ff | Concatenation follows authoritative post-PP token adjacency; retained parts keep encoding/raw/origin facts; raw CRLF spelling survives while decoded units normalize to LF |
| il.c string_literal_type12779ff | Real const-element array lvalue and bound including NUL; reuse only through80, structural comparison and recursive owner/large-array cache exclusion beyond that |
| decl_inits.c string initialization101ff | Supported copy initialization infers omitted bounds, pads larger arrays and rejects terminator truncation; both declaration/symbol type links restore through owner journal |
| expr.c sizeof14915ff/15035ff and subscript2640ff | Unevaluated sizeof suppresses loads/decay/unneeded body instantiation, retains dependent size nodes; subscript preserves original order/range and unpromoted integer index |
| interpret.c string7081ff, decay27927ff, subscript31730ff and reference27321ff | Stable array element cells, explicit base/index pointer values, object locations versus nonobject one-past addresses, and separate reference-to/indirection/load gates |

Literal expressions expose immutable CodeUnits/Parts, never fabricated integer
ConstantValue. Array parameters adjust to pointers retaining element cv while
ParameterDeclaredType preserves the written type. Array returns reject. Scalar
pointer offsets/null-zero/bool conversions, pointer argument matching and
reference element binding are implemented within the current target profile.
Other valid pointer operators diagnose Unsupported. Type construction/sizeof is
independent from storage budgets; invalid negative bounds remain InvalidSource,
unrepresentable positive bounds ResourceLimit. Preallocation checks widen budget
arithmetic before allocation; int.MaxValue regression returns ResourceLimit
with zero materialized arrays. Scoped and large-array compound types are not
retained indirectly by session pointer/reference/function caches.

Constant arrays own one stable cell per actual element. Literal cells are static/
readonly; copied local arrays have destination qualification and frame/scope
lifetime. Const access does not change the underlying object's readonly fact.
Readonly and constant usability are separate: ordinary global const arrays and
pointers cannot be loaded solely due to cv; constexpr and evaluation-local
objects have their distinct eligibility. Unknown readonly observations are null.
Fresh initializer-result units are separate from current named global storage,
whose unavailable scalar Values stay null. Pointer/address transport does not
implicitly load a pointee or re-extend lifetime.

One-past glvalues have an explicit nonobject address, not a fake element cell.
Direct discard/reference-to transport and bare constexpr reference initialization
can succeed; named-reference and reference-return-call ref_indirect or actual
loads fail. NonObjectReferenceValue/Bindings record those addresses separately
from real-object reference observations. Dead local pointer boolean conversion
only tests non-null and matches native acceptance; persistent escaped pointer
results fail. The earlier proposed blanket rejection is retained as disproved.

Three exact integrated native fixtures match2 accept0/1 reject2; their type
witness includes unchanged source/header bytes.32 finite design receipts contain
27 matches and5 retained exclusions: two disproved expectations and three pinned
native -6 internal catastrophes for invalid-numeric-byte ordinary+UTF8 result
concatenation. That specific branch diagnoses Unsupported (UNS041), not invalid
C++ or verified native parity. Direct u8 numeric units and wide numeric-byte
fallback have separate positive controls. Every receipt uses explicit two-arg
UTF8 source kind, strict C++20 LP64 and used instantiation; only normal0/2 count.

STR01–17 are reconciled by final independent review; STR15 was an earlier
in-flight reading, absent from both held builds, and its added query strengthens
evidence without an evaluator patch. Initial build naming collision and119/120
conversion-wrapper assertion are retained; final120 gate tests real code.
The coupled source checks Unicode macros/includes, typed concatenation, pointer
deduction/replay, frozen overloads, element-reference identity/cleanup, origins
after disposal, no-storage sizeof and forbidden mutable-global reads.

General pointer/compound/dependent declarators, complete conversions/operators,
UDLs/floating/legacy/dialect literal breadth, classes and all other inventory
requirements remain. Accepted isolated scalar control flow still needs its
combined main integration; floating groundwork needs actual frontend integration.
