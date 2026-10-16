# Single-inheritance dispatch checker

This checker targets the actual production CLI publication of
`Fixtures/T5/single-inheritance.cpp`. Build `Checks.csproj` with
`-p:GeneratedProject=/absolute/path/to/Generated.csproj`; pass the separate
native `{"runs":[...]}` observations JSON as its only runtime argument.
The coordinator owns compilation, JIT/NativeAOT execution and receipts.

First, it invokes the generated fixture twice and compares both complete
nine-value/ordered-event observations with the native oracle, including live,
overflow and prior-live invariants. The existing native result is 28, values
`2,5,11,3,5,23,3,0,0`, and ten events per invocation. The JSON input, rather than
a replacement source-derived oracle, supplies those comparisons.

Physical checks use the published `Base__2`, `Derived__2` and occupied-base
`Record` representations. They verify complete sizes 16/16, view size 12,
complete alignment 8, base field offset 0, id offset 8 and reused-tail bonus
offset 12. The 12-byte view is borrowed inside its aligned owner; this checker
does not claim it has standalone CLR alignment 8 or a by-value/native ABI.
Actual generated constructors initialize two complete objects of each type.
Their vptrs must agree with the distinct generated application table address
points, and their loaded method slots must be stable across objects.

The checker executes **both actual loaded slot addresses**, using separate
`delegate* unmanaged[Cdecl]<Base__2*,int,int>` and
`delegate* unmanaged[Cdecl]<Derived__2*,int,int>` calls with matching complete
receivers. It compares results 5/23 and method events 602/705 with the native
observations, then invokes the actual generated destructors and verifies
balanced lifetimes. These are generated scalar method wrappers, not native C++
application implementations or a native-unwinding boundary.

Exactly two borrowed RTTI data diagnostics must match the pinned provider
path/hash, audited RVAs, 88-byte ELF symbol sizes and `r--p` owner mappings.
Independent file hashing, exact-provider GetExport and `/proc/self/maps`
inspection check the reported addresses. Generated typeinfo memory must contain
each export **base plus the captured 16-byte offset exactly once**. The native
provider's RTTI vtable slots are never called, read as application dispatch,
copied, written or freed. `CXXABI_1.3` identifies the pinned ELF's default-export
metadata; GetExport is not a runtime versioned symbol lookup.

Output retains the full `runs` array and adds explicit `physical` and `providers`
objects. Addresses are process-specific audit fields, not cross-run equality
requirements. All JSON construction is explicit and NativeAOT safe.
This is a bounded ordinary single-inheritance proof only; multiple/virtual
inheritance, nonzero thunks, virtual destructors, RTTI queries, member pointers,
native application fallback and native exception unwinding remain outside it.
