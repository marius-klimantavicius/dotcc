# Generated program assembly candidates

Each application pair is exported as two separate genuine objects. Only the
public ProgramAssembly selection may join declarations to definitions. The
native oracle links independently compiled application TUs and an oracle-only
main; generated workloads must never depend on those native application bodies.

Both pairs expose same-spelling internal functions and data with different
behavior, one external data definition read and written through both TUs, and
function-address identity across declaration/definition boundaries.

| Pair | Predicted result (unverified) | Ordered observations |
|---|---:|---|
| `c-unit-a.c`, `c-unit-b.c` | 1150 | 3,8,7,12,18,9,1,1,1,1 |
| `cpp-unit-a.cpp`, `cpp-unit-b.cpp` | 1212 | 4,16,7,10,6,1,1,16 |

The C pair additionally passes an opaque data pointer from an incomplete TU view
to a complete TU view and uses canonical cross-TU function/data addresses in
constant initializers. The C++ pair includes two already-selected overloaded
external functions; their recorded final names are the identities, never a
source lookup or remangling exercise.

These candidates are limited to two open units, fixed scalar/pointer function
boundaries and supported static storage. No generated execution, native aggregate
ABI, C++ ODR revalidation or bounded-scale claim follows from adding the sources.
