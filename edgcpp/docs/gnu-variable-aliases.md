# GNU variable aliases in the C# backend

A resolved GNU variable alias is a typed view of the target object, including
when the public type differs from the raw storage type. The backend follows
`a_variable::aliased_variable`, resolves chains, allocates the canonical target
once, and maps each alias to that same global or TLS offset. Named C# ref
accessors retain the alias's own type and source name. Only the target receives
storage initialization; aliases do not acquire an extra constructor or
destructor. This supports the aligned raw-storage pattern used by libc++ streams.

Alias types must fit the target extent. Target allocation honors the strongest
required alignment among its views. Alias and target must both have global
storage or both have thread storage. GNU `__thread` uses `DM_THREAD` in EDG IL,
whereas standard `thread_local` / `_Thread_local` uses `is_thread_local`; the
backend recognizes both, following `c_gen_be.c`'s declaration emission.

Unresolved weak references, missing storage definitions, cycles, mismatched TLS,
oversized views, separate alias initializers and aliases to native exception
metadata produce explicit backend diagnostics. This does not implement native
linker interposition, late dynamic symbol lookup, or routine aliases.

The focused tests use one source TU. The unchanged stock C backend and C#
frontend invocation both accept one-TU alias tests. A two-TU probe accepted an
alias-bearing TU second, but rejected it first; adding a third TU also failed
with the alias TU last. Full musl provider merging remains subject to this
configuration constraint. An experimental runtime-sources-first invocation
order did not solve it and was not adopted. No frontend semantics were modified. The C thread test includes the unchanged
shared C11 runtime shim into its test TU and uses the existing dotcc runtime
bindings, so actual translated thread and TLS behavior is exercised without
claiming full provider merging works for aliases.

Verification: `backend/tests/variable_alias_verify.py --compiler <snapshot>
--output-dir <new-directory> --nativeaot` compares native and stock EDG C output
with static and instance C# in JIT and NativeAOT. Fixtures cover initialized
scalar alias chains; address identity using volatile pointer values; GNU and
standard TLS with an actual worker thread; 32-byte aligned raw storage viewed
through a nontrivial C++ class; placement construction, member writes and explicit
destruction; and final destruction counters detecting duplicate lifetime work.
An unresolved weakref checks the backend rejection path. These tests do not yet
exercise independent simultaneous managed owner instances or non-Linux execution.

## Explicit assembly symbol views

An external variable declaration with an explicit `__asm__("symbol")` can bind
to a source-defined object in this closed translated program. The backend indexes
actual emitted symbol identities: explicit assembly labels take precedence;
otherwise it uses the retained lowered mangled name or C-linkage name. It never
uses a display name to guess C++ linkage, and named-register union entries do not
participate. Referenced hardware-register globals are explicitly rejected before
storage allocation: the C backend emits their actual register binding, which
managed pinned storage cannot reproduce. An asm declaration requires exactly one definition and shares that
definition's storage through the same typed-view checks described above.

Provider pruning follows the additional asm-symbol edge, retaining the target's
initializer and dependencies when the only reference is an asm declaration.
The target can have a different source spelling or live in a C++ namespace.
Unresolved symbols, multiple definitions, oversized views and mismatched TLS
produce explicit diagnostics. This represents closed-program linker identity;
it does not implement dynamic interposition or arbitrary assembly statements.

`backend/tests/variable_asm_verify.py` verifies separate native object compilation
at GCC `-O2` without LTO, three-TU stock C output, and a full musl-provider merge
with static/instance JIT and NativeAOT. The test marks the storage-definition TU
as prunable and verifies that an asm-only initialized target survives while an
unrelated variable is removed. It also covers explicit renamed symbols and an
actual C++ mangled definition. Native typed views and raw definitions are in
separate TUs, reflecting the library convention; placing incompatible named
views together in one native optimized TU can introduce optimizer assumptions
and is not claimed equivalent. GNU alias-attribute frontend constraints remain
separate and are not solved by this feature.
