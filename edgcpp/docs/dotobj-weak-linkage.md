# Weak routine linkage

Profile v54 preserves the observed GNU weak flag separately from each routine's
external identity, declaration/body status, source signature and execution ABI.
A weak definition has `definition='weak'`; a weak declaration remains a
`declaration`. The reader requires external linkage and consistent strength.
GNU weak aliases, ordinary aliases, indirect functions and weak storage remain
separate unsupported features; none are inferred from a routine's weak flag.
Stock C emits the original weak attribute (`il_to_str.c`) and also uses weak
attributes for selected COMDAT cases (`c_gen_be.c`). The exporter keeps explicit
weak binding distinct from its existing coalescible `linkonce` definitions.

The managed linker selects a strong definition over weak definitions regardless
of input order. Equal weak definitions follow supplied input order, after the
existing preference for primary objects over selectable providers. Signature,
source type, exception policy and duplicate strong definition checks still
apply. Existing selectable-provider strong-definition preference is retained.

A missing weak routine has a typed null function address. Address expressions
and static relocations use that null; no runtime partial or fabricated body is
created. A call through such an unresolved address is not made safe: the source
program must check the address before calling, as with native weak references.
A reachable strong external reference to the same identity still requires its
ordinary external implementation.

A weak-only reference does not extract a selectable provider. If another strong
reference activates that provider, an available weak target in that object is
retained. Tests compare both cases to a native/stock-C static archive, as well as
strong overrides in both orders, weak fallback and equal-weak input order.
The focused suite tests native/stock C at O0/O2, source-hidden object linking,
managed execution and NativeAOT; malformed strength/linkage metadata is rejected.

Profile v54 also admits C compound-literal initialization through the existing
exact pooled-record initialization proof. This stock lowering was already
captured; a C++-only reader guard incorrectly excluded it. Complete source record,
compiler-owned storage, typed initialization and nonescaping storage checks stay
in place. A C fixture and the actual musl vfscanf provider now verify and emit.
