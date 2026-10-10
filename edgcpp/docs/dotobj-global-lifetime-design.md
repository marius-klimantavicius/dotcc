# Owned file-scope destruction and initializer lifetimes

Status: design and native/stock-C oracles; exporter and managed implementation pending.

DuckDB's 38-unit subsystem sample with native077 contains eight failures at
`file-scope destruction obligations`. This is an exporter gate, not a frontend
failure. For example, `third_party/parquet/parquet_types.cpp:44` declares the
first of several constant global `std::map` registries. Their constructors,
argument temporaries, and eventual destructors need distinct owned lifetimes.

## Stock EDG behavior

The anchors below refer to the embedded source tree inspected during v39.

- `upstream/src/lower_init.c:7881`, `record_needed_destruction`, registers a
  destructor after the object's initialization succeeds. For the configured
  IA-64 ABI it emits `__cxa_atexit(complete_destructor, object, &__dso_handle)`.
  Thread storage uses `__cxa_thread_atexit`; it is a separate lifetime domain.
- At `lower_init.c:7967`, array cleanup selects a fabricated callback from
  `make_destruction_routine` and supplies a null registration object. That
  callback owns the actual array destruction operation and global address.
- `lower_init.c:7827`, `make_destruction_routine`, creates a generated routine,
  calls `add_destructor_call` with the original initialization descriptor and
  position, and completes the generated routine. This is the correct place to
  capture the callback's origin and complete typed body.
- `lower_init.c:8088` removes the original initialization from its destruction
  list because its destruction has transferred to the static cleanup mechanism.
  A post-lowering lifetime walk alone therefore loses essential information.
- `lower_init.c:7908` can omit a destructor that stock lowering proves has no
  effect. Record this explicit disposition and original selected destructor;
  do not silently discard the source obligation or infer emptiness ourselves.
- `lower_init.c:11014` selects static registration only after full construction.
  Partial static aggregate construction still uses automatic exception cleanup.
  An array whose second constructor throws must destroy only its completed
  elements and must not acquire a successful static registration.
- `lower_init.c:8120`, `push_init_expr_lifetime`, can create a function-region
  copy of a file-scope initializer's lifetime. Capture the original-to-copy
  relation before `*init_expr_lifetime` is overwritten. The existing hook in
  `lower_expr::lower_object_lifetime_expr` does not cover this distinct path.
- `lower_init.c:18901`, `b_lower_file_scope_dynamic_inits`, splits initializers
  by priority/needed flags/TLS criteria and creates their generated routines.
  One original global lifetime cannot simply be assigned to one invented
  source function.

The C backend emits those lowered calls and callback bodies. The retained
stock-C output verifies the lowering rather than relying on function names as
semantic evidence.

## Original provenance

Extend `observe_file_initializers` to snapshot the complete original static
lifetime graph and destruction entries before mutation. Each entry retains:

- Original lifetime identity, parent relation, child ordinal, lifetime kind,
  parent destruction sublist relation, and source location.
- Original initialization identity, associated declaration/storage, original
  source type, selected destructor, destruction ordinal, and source location.
- The exact association with an initializer and its full-expression lifetimes.
  Persistent static obligations and automatic initializer temporaries or
  partial subobjects remain separate semantic categories.

Original source ownership must not be replaced by a generated routine. Preserve
an explicit adoption relation from an initializer's original lifetime to the
generated initializer that executes it, plus original/copy correspondence.
Existing `source_lifetimes.function_entity_id` and destruction ownership refer
to `source_functions`, so they cannot represent these origins as they stand.
The implementation must introduce an explicit initializer owner alternative or
separate static-origin tables and verified execution-owner adoption. It must
not populate fake source-function rows for generated initializer/callback code.

## Proposed persistent relations

These describe required contracts; names/normalization should be finalized
with the shared schema implementation before native wire emission.

| Relation | Required information |
| --- | --- |
| `source_static_lifetimes` | Original lifetime id, parent id, kind, sibling ordinal, parent obligation, optional original initializer association, source location, `before-lowering` proof |
| `source_static_destructions` | Id, original lifetime id, ordinal, storage/declaration id, source type id, selected source destructor id, process/thread domain, source location, `before-lowering` proof |
| `initializer_lifetime_executions` | Original initializer/lifetime id, generated initializer entity id, copied lifetime correspondence, observed adoption proof |
| `static_destruction_registrations` | Exact node id, source static obligation id, optional initializer id, owner entity id, selected callback id, object-expression node id, process/thread domain, observed registration proof |
| `generated_static_destructors` | Callback entity id, source static obligation id, storage id, original element type/count/size when applicable, observed creation/completion proof |
| `static_destruction_dispositions` | Source obligation id, disposition `registered` or `stock-proven-no-effect`, observed reason/proof; every reached obligation must be accounted for |

Source node/expression references, declarations, callbacks, fields, and types
remain ordinary renamable ids. Do not encode synthesized native names or raw
addresses into the persistent identity.

The registration can be a dedicated typed expression node replacing the exact
observed runtime call. Preserve its integer result contract and argument
evaluation. The process/module owner is a translated-program runtime token;
it need not become an unresolved `__dso_handle` platform import. Do not rewrite
arbitrary user calls merely because their names resemble runtime functions.

For a scalar, verify that the callback is the complete-object ABI entry for
the exact original selected destructor and that the object expression refers
to the captured storage/path. For an array, verify the generated callback's
owned origin, synthetic `void*` formal, typed body, array count/stride, and
element destructor. Existing array-runtime provenance should be captured
inside the generated callback through the original destruction context.

## Native observation points

1. Snapshot source lifetimes/obligations in `observe_file_initializers` before
   lowering mutates the static destruction lists.
2. Associate the exact current initializer with its generated owner at
   `observe_initializer_begin`; retain all child source boundaries.
3. Observe the original/copy lifetime pair in `push_init_expr_lifetime`.
4. Observe generated cleanup creation before `add_destructor_call`, enter an
   original-destruction capture context for its lowering, then observe its
   complete body/formal before the generated scope is released.
5. In `record_needed_destruction`, observe the final call node, callback,
   object argument, domain, original descriptor, and position immediately
   before insertion. Record no-effect elimination on its distinct branch.
6. Export exact source coverage and verify it before removing existing gates.

## Runtime behavior

Maintain a registry per translated program/state owner and drain registrations
in reverse successful registration order. Registration occurs at the actual
lowered call, including conditional/local-static cases when supported; never
schedule every destructor unconditionally at startup. Keep global storage alive
until callbacks finish. Integrate normal executable shutdown and explicit
library-owner disposal; process-exit handling alone cannot serve independent
instances. The existing initializer exception boundary retains termination
semantics. Thread registration requires its own thread-owner lifecycle and
must remain explicitly gated until that implementation exists.

Use statically generated typed callback adapters or managed function pointers
with explicit state, compatible with NativeAOT. No reflection-based invocation
or platform-specific `__cxa_atexit` dependency is necessary.

## Verification already retained

`object-linker/tests/duckdb_global_lifetime_oracle.py` ran 27 checks with native
C++ and stock EDG C output at O0/O2. Results and generated C are retained in
`/mnt/storage/dotcc-edg-campaign-20261003/duckdb-global-lifetime-design-001`.

| Fixture | Expected output | Contract |
| --- | --- | --- |
| `duckdb_global_lifetime_scalar.cpp` | `120657` | Constant initialization still registers destruction; later objects destroy in reverse registration order |
| `duckdb_global_lifetime_array.cpp` | `12065` | One generated array callback destroys elements in reverse order |
| `duckdb_global_lifetime_temporary.cpp` | `13204` | Startup temporary dies before main; successful global registration survives until shutdown |

The stock scalar output registers its constant object without calling a
constructor. The array output passes a null registration object and its
generated callback invokes `__cxa_vec_dtor` with count 2 and stride 4. The
temporary output registers the global, then destroys the temporary before
returning from startup. These fixtures have not passed dotobj export or managed
execution yet.

Further acceptance requires source-hidden verification/linking and JIT/AOT
execution, multi-TU ordering compatible with the selected startup order,
partial-array construction failure, conditional registration and disposal
exactly once. Resealed negative objects should reject missing source ownership,
wrong storage/callback/complete-destructor entry, duplicate registration,
missing callback origin, and treating a temporary as a persistent static
obligation. Empty-destructor elimination must retain an explicit verified
source disposition.
