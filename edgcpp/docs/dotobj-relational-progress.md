# Relational program implementation — executable slices

The broad v66 graph campaign remains stopped. The replacement contract is
[dotobj-relational-restart.md](dotobj-relational-restart.md).

## Verified result

Native compiler built with the all-extensions configuration and direct SQLite
support. The two-unit C++ test exports a shared record/member function, concrete
function-template instances, named fields, structured loops/continue, cross-unit
calls and an integer-initialized shared global. The original sources are moved
away before the C# linker/generator runs. Native C++, generated C# and Linux x64
NativeAOT all return success. Unsupported nested destruction and unreferenced
dynamic global initialization fail before an object is published.

Review artifacts: `artifacts/review/dotobj-relational-v3/`.

- `original-source/`: input C++ and header.
- `provider.dotobj`: 81,920 bytes; 52 program nodes, 48 child edges, 17 types.
- `main.dotobj`: 81,920 bytes; 74 program nodes, 71 child edges, 19 types.
- Each object: 14 ordinary relational tables, no JSON/BLOB semantic payloads.
- `generated/Translated.cs`: source-like C# including Point fields, named
  functions, for/while/continue and ordinary arithmetic/calls.
- Build/run logs and SQL row summaries retained alongside objects.

Reproduce with `object-linker/relational/tests/verify.py --out <new-directory>
--linker artifacts/relational-tool/dotobj-relational.dll --nativeaot`.

These sizes describe this small test only, not a prediction for DuckDB.

## Verified construction, cleanup and exception slice

Review artifacts: `artifacts/review/dotobj-relational-lifetime-v4/`.
Two source units define `Guard`, `Failure` and a two-member `Pair`. Native C++,
source-hidden generated C# and Linux x64 NativeAOT all pass. A visible effect
precedes local construction; normal return produces trace `931234768` and a
throwing second member produces `931267889`. The failure path destroys only the
completed first member, enters the ordered reference handler, mutates the
exception payload, rethrows, cleans up the outer local, and finally destroys the
exception object after the outer handler.

The object schema now has 21 ordinary tables. The provider is 122,880 bytes,
with 110 nodes, 37 types, 11 initialization rows, six destruction actions, six
constructor/destructor member entries, two throw/rethrow rows and two handlers.
The main object is 114,688 bytes with 61 nodes and one handler. SQL summaries,
per-table page/payload sizes, compiler build log and tool hashes are retained.
These are small-fixture measurements, not predictions for a real project.

EDG supplies lifetime parents, boundaries, ordered destruction lists, selected
constructors/destructors, ordered arguments, initialization identities, conditional
and partial-construction flags, try handlers and finalized exception policies.
The same initialization row is referenced from execution nodes and cleanup rows.
The generator emits activation flags after successful construction and uses the
recorded cleanup order; it does not derive lifetime rules from source declarations.
The existing `object-linker/runtime/CppExceptions.cs` is reused unchanged.
Source primitive identities distinguish equal-width C++ builtins for typed catches.
Compiler-generated RTTI declarations marked `lowering_generated` are excluded
from file-scope source enumeration.

The original cross-unit slice also passes native/C#/NativeAOT with the final
tools at `artifacts/review/dotobj-relational-regression-v4/`. Dynamic global
initialization, virtual dispatch and reversed subscripting reject before object
publication. The current reader also links and executes the preserved 14-table
v3 objects; compatibility output is `artifacts/review/dotobj-relational-v3-compat/`.

Reproduce with `object-linker/relational/tests/verify_lifetime.py --out <new-directory>
--linker artifacts/relational-tool/dotobj-relational.dll --nativeaot`.

## First bounded real-project probe

After the lifecycle milestone, one retained DuckDB source unit,
`src/common/types/hugeint.cpp`, was compiled with the current direct relational
exporter and the retained libc++/DuckDB include and define recipe. Evidence is
`artifacts/review/dotobj-relational-duckdb-hugeint-v1/`: source hash, invocation,
diagnostics and GNU time output. No bulk job was started.

The exporter rejected `record base classes` before publishing a `.dotobj`.
The measured early rejection took 0.08 seconds wall time and 30,892 KiB maximum
RSS. This is not a full-TU performance measurement or a DuckDB export/generation
pass. The next concrete type work is EDG-selected base relationships and layout;
the declaration must not be replaced with a raw graph placeholder. Whole-project
bounded loading remains required before increasing linker input scale.

## Verified nonvirtual base layout and execution

Review artifacts: `artifacts/review/dotobj-relational-bases-v2/`. The source-hidden
two-unit fixture passes native C++, generated C# and Linux x64 NativeAOT. `Derived`
contains `Left` at offset 0 and `Right` at offset 4. Selected conversions preserve
null pointers, adjust the second base by +4 or -4, and allow cross-unit mutation.
Normal construction/destruction gives trace `123476`; a throwing second base
destroys only the completed first base and gives `1269` after the handler.

The schema has 23 ordinary tables. `base_subobjects` records EDG layout and direct
declaration order; `class_conversions` records selected base identity, signed
adjustment and null/representation facts. Constructor/destructor entries reference
the selected base. The generator uses named pointer projections into the recorded
layout, avoiding full-size base fields that could overwrite reused tail padding.
Provider: 139,264 bytes, 158 nodes, 47 types. Main: 126,976 bytes, 67 nodes, 37 types.
SQL inspection, table sizes and tool hashes are retained. The original cross-unit
and lifetime slices pass native/C#/NativeAOT at `dotobj-relational-regression-v5`
and `dotobj-relational-lifetime-regression-v5` respectively.

Virtual base execution, base copy/zero initialization without an explicit extent,
and derived-exception matching remain explicit unsupported-generation diagnostics.
Metadata for virtual bases is retained. The base milestone does not claim those
execution paths.

The same bounded DuckDB `hugeint.cpp` probe now passes the base metadata boundary
and rejects `enum metadata` before publication. Evidence:
`artifacts/review/dotobj-relational-duckdb-hugeint-v2/` (0.07 seconds, 32,144 KiB
maximum RSS for this early rejection, not a complete TU measurement).

Reproduce with `object-linker/relational/tests/verify_bases.py --out <new-directory>
--linker artifacts/relational-tool/dotobj-relational.dll --nativeaot`.

## Verified enum declarations and execution

`artifacts/review/dotobj-relational-enums-v2/` passes native C++, source-hidden C#
and Linux x64 NativeAOT. The two units retain named scoped/unscoped enums in
function signatures and record fields, signed values and the exact unsigned
64-bit maximum. A thrown `Status` matches its enum handler rather than an earlier
handler for its underlying `unsigned short`. Generated C# retains `Status.ready`,
`Legacy.negative` and `Wide.high` where EDG supplied the selected declaration.

`enums` and `enum_values` bring the schema to 25 ordinary tables. EDG supplies
the underlying builtin type, scoped/explicit-base flags, ordered values and
source coordinates. Nodes reference selected enumerators by identity; folded
numeric values do not acquire guessed names. Provider: 143,360 bytes, 37 nodes,
19 types. Main: 151,552 bytes, 85 nodes, 24 types. Per-table storage and tool hashes
are retained. Enum storage forms unsupported by C# reject explicitly. The earlier
base, lifetime and original slices pass native/C#/NativeAOT under the same tools
at their `*-regression-v6` directories.

The bounded DuckDB `hugeint.cpp` probe advances to `template argument (EDG kind 3)`
at `artifacts/review/dotobj-relational-duckdb-hugeint-v3/`. This is an early rejection,
not a successful TU export. The next work is compact pack membership and template
argument identity; dependent recipes must remain metadata rather than C# analysis.

Reproduce with `object-linker/relational/tests/verify_enums.py --out <new-directory>
--linker artifacts/relational-tool/dotobj-relational.dll --nativeaot`.

## Verified bounded function-body loading

`artifacts/review/dotobj-relational-body-loading-v1/` contains a generated five-unit,
41-function acceptance case. Native C++, source-hidden C# and Linux x64 NativeAOT
pass. The objects contain 1,531 function nodes; the reader peaks at 251 loaded
function nodes / 502 body relation rows, with one active function body maximum
and zero remaining after generation. The exception prepass and generation each
load/release bodies, for 82 function loads. Generated C# streams to disk. The
reader retains one body lease at a time rather than all bodies in all input units.

This is a body-retention bound, not a whole-project memory bound or an RSS/GC
measurement. Type/symbol/field/parameter and generated-name/exception catalogs,
plus global expressions, still remain in memory. The acceptance case reports 131
types, 202 symbols and no global nodes. Whole-project catalog loading remains work.

Base, lifetime and enum slices pass native/C#/NativeAOT with the body-loading reader
at `dotobj-relational-{bases,lifetime,enums}-body-regression-v1`. The preserved
14-table objects also link and execute at `dotobj-relational-v3-body-compat/`.
Reproduce the retention check with `tests/verify_body_loading.py` using the usual
`--out`, `--linker` and optional `--nativeaot` arguments.

## Verified template packs and dependent source declarations

`artifacts/review/dotobj-relational-packs-v5/` passes SQL integrity, native C++,
source-hidden C# and Linux x64 NativeAOT. The two units exercise ordered type and
integer packs, an empty pack, and `Wrapped<Box, int>` with a selected template
argument. Source formals and dependent `Box<T>`/`Wrapped` prototype fields remain
in the object, while the generator follows recorded executable references and
emits concrete types. It never instantiates a recipe. EDG's empty function-pack
placeholder is represented by an `empty_parameter_packs` source declaration,
not a runtime static. Actual static storage remains explicitly unsupported.

The schema has 29 ordinary tables. Pack members reference their container ordinal;
template formals preserve selected type/template identities, coordinates, pack
flags and supported scalar/type/template defaults. Dependence is explicit on
types and instances. `template_type_parameters` records the actual EDG parameter,
member or unknown-dependent form. More complex dependent expressions/defaults
remain unsupported rather than becoming opaque payloads. Each object is 176,128
bytes; provider has 38 nodes/30 types, main has 33 nodes/16 types. Storage summaries
and tool hashes are retained.

`floating_types` also preserves source builtin identity and explicit target
precision/exponent/format properties. `dotobj-relational-floating-metadata-v1/`
verifies float, double, long double (x87 binary80) and __float128 (binary128),
including source-hidden folded-sizeof execution. This does not claim extended
floating arithmetic emission. Native/C#/NativeAOT regressions pass for the original,
base, lifetime and enum slices under `*-template-regression-v1`; body retention
remains 251 peak nodes of 1,531 at `dotobj-relational-body-loading-type-closure-v1`.

The bounded DuckDB probe at `dotobj-relational-duckdb-hugeint-v6/` reaches
`constructor return initialization`. No full DuckDB object is published yet.
Explicit returned-object initialization and caller-provided return storage are
the next active work; the full-corpus campaign remains stopped.

Reproduce with `tests/verify_packs.py` and `tests/verify_floating_metadata.py` using
the usual `--out` and `--linker` arguments (`--nativeaot` is supported for packs).

## Verified returned-object construction

`artifacts/review/dotobj-relational-returns-v6/` passes native C++, source-hidden
C# and Linux x64 NativeAOT. A returned `Result` stores its own address; EDG-selected
caller-provided storage is forwarded through `forward_result` into `make_result`,
so the address matches the final caller object. Trace `1234` verifies construction,
callee-local cleanup and caller destruction. A throwing result constructor gives
`1235`: only the completed callee local is destroyed before the handler. A separate
`Scalar` list-return exercises EDG-selected value-return storage.

Function types record `return_storage`; return nodes reference selected
initializations and result types. `class-result` initializations bind the selected
call to the supplied destination. C# uses an explicit result pointer for
caller-provided storage and a local value for EDG value returns. Selected implicit
member copy sources are recorded in `constructor_initializers.copy_source_symbol_id`;
the consumer does not choose a copy parameter by position. NRVO candidate metadata
is retained; optional NRVO optimization is not applied. Required frontend elision
is already reflected in EDG's selected initialization.

Provider: 176,128 bytes, 69 nodes, 46 types. Main: 184,320 bytes, 109 nodes, 45 types.
Storage summaries and tool hashes are retained. Lifetime, bases and packs regress
under native/C#/NativeAOT at their `*-return-regression-v1` directories. The test
also verifies rejection when potentially throwing local cleanup would require
unexported return-result unwinding facts. Shared comma/conditional result storage,
unbound caller-result expressions, nontrivial implied-copy constructor arguments
and return-result lifetime transfer remain explicit unsupported cases.

The real DuckDB probe at `dotobj-relational-duckdb-hugeint-v7/` advances to
`noninteger constant value (EDG kind 4)`. Exact floating literal values are the
next exporter/consumer work. This is still an early rejection without a published
DuckDB object. Reproduce returns with `tests/verify_returns.py` and the usual
`--out`, `--linker`, `--nativeaot` arguments.

## Verified exact floating values

`artifacts/review/dotobj-relational-floats-v1/` passes native C++, SQL integrity,
source-hidden C# and Linux x64 NativeAOT. Cross-unit execution checks negative
zero through reciprocal sign, double rounding, a binary80 increment at 2^-63,
a binary128 increment at 2^-112, and floating template arguments/defaults.
EDG supplies exact literal bits through its target floating API. The 30-table
schema stores each literal as a typed `floating_values` scalar row; expressions
and template arguments/defaults reference it. Display text is separate.

C# emits readable round-trip finite float/double literals, with bit constructors
for negative zero and nonfinite values. Extended values use the existing Float80
and Float128 software runtime. This verifies the exercised arithmetic, not full
floating-environment or dynamic-rounding-mode behavior. Half formats remain
unsupported. Function-local floating values share the existing body lease rather
than becoming an unbounded literal catalog.

Provider: 184,320 bytes, 20 nodes, 16 types. Main: 192,512 bytes, 100 nodes, 27 types.
Storage summaries and tool hashes are retained. Return-object and body-loading
regressions pass native/C#/NativeAOT at `*-float-regression-v1`; peak retained
body nodes remain 251 of 1,531. Reproduce with
`tests/verify_floats.py` and the usual `--out`, `--linker`, `--nativeaot` arguments.

The real DuckDB probe at `dotobj-relational-duckdb-hugeint-v8/` advances to
`missing type (EDG kind 23)`. It still rejects before publishing a complete
DuckDB object; this is not a full-unit time or memory measurement. The next type
is standard `std::nullptr_t`; explicit export and consumption are next. The
full-corpus campaign remains stopped.

## Verified nullptr source type and value normalization

`artifacts/review/dotobj-relational-nullptr-v2/` passes native C++, SQL integrity,
source-hidden C# and Linux x64 NativeAOT. `std::nullptr_t` retains a distinct SQL
type and EDG layout, with ordinary pointer storage in C#. Nodes explicitly record
EDG's selected `nullptr-zero` result normalization; the consumer preserves operand
evaluation once and emits the recorded null result. Lvalue storage stays intact.
The fixture writes a nonzero byte into nullptr storage and then converts the
value to an object pointer, checks a side-effecting nullptr-returning call,
selected nullptr/pointer overloads, cross-unit fields/parameters/returns, and
nullptr template arguments/defaults. Source typedef/qualifier layers remain.

Provider: 184,320 bytes, 37 nodes, 20 types. Main: 192,512 bytes, 130 nodes, 31 types.
Storage summaries and tool hashes are retained. Floating and bounded-body
regressions pass native/C#/NativeAOT at `*-nullptr-regression-v1`; the body bound
remains 251 peak nodes of 1,531. C++/CLI null types, pointer-to-member null values,
and nullptr exception matching remain explicitly unsupported. Reproduce with
`tests/verify_nullptr.py` and the usual arguments.

The real DuckDB probe at `dotobj-relational-duckdb-hugeint-v9/` advances to
`missing expression (EDG kind 10)`, an EDG condition declaration. No full DuckDB
object has been published. Explicit condition initialization and lifetime scope
are next; broad export remains stopped.

## Verified condition declarations and iteration lifetimes

`artifacts/review/dotobj-relational-conditions-v2/` passes original C++ under
Clang 21, SQL integrity, source-hidden C# and Linux x64 NativeAOT. Nine source
condition nodes retain selected initialization, exact test-expression edges,
optional C++17 initializer statements and EDG lifetime ownership. A `for`
initializer's lifetime is separate from each condition iteration. Explicit
`class-rvalue-adjust` nodes preserve the same class result and destination.

The fixture covers scalar and record selection initializers, false conditions,
nontrivial loop conditions, nested continue targets, break, body exceptions,
constructor failure, and a record in the outer for initializer. Rewritten for
continues execute the increment before condition destruction, as EDG selected.
GCC's original-source reference fails that check with early destruction, matching
[reported GCC bug 86769](https://gcc.gnu.org/pipermail/gcc-bugs/2018-August/631919.html).
Clang agrees with EDG's executable lowering (`lower_il.c` increment insertion then
condition cleanup); the test uses Clang and retains compiler provenance. EDG
semantics were not changed to match GCC.

Provider: 233,472 bytes, 407 nodes, 25 types. Main: 184,320 bytes, 4 nodes, 18 types.
Storage summaries and hashes are retained. Original, lifetime and return-object
regressions pass native/C#/NativeAOT at `*-condition-regression-v1`. Reproduce with
`tests/verify_conditions.py`, usual arguments and optional `--native-cxx` (defaults
to available Clang). Switch emission and more complex condition temporary
lifetimes remain work.

The real bounded probe at `dotobj-relational-duckdb-hugeint-v10/` advances to
`virtual call dispatch projection`. No complete DuckDB object is published yet.
Explicit EDG-selected virtual dispatch tables/calls and lifecycle writes are the
next investigation, without reconstructing override selection in C#.

## Verified EDG-selected virtual dispatch

`artifacts/review/dotobj-relational-virtual-v6/` passes native C++, SQL integrity,
source-hidden C# and Linux x64 NativeAOT. The 34-table schema adds selected virtual
calls, table/address-point identities, finalized entries with source signatures
and exact thunk adjustments, and ordered constructor/destructor vptr writes.
Targeted stock lowering hooks record those decisions; the consumer never selects
overrides or rebuilds a hierarchy. Source signatures are retained before lowering
changes them. No EDG semantic state is changed by these hooks.

Pinned BCL slot arrays and typed managed function pointers implement dispatch.
Named helpers capture receiver and target before arguments; translated method
bodies remain source-readable. The fixture checks construction/destruction trace
1221 through an external virtual-call helper, a pure slot without a method body,
multiple inheritance with an EDG-selected -16 receiver adjustment, and exactly-once
receiver/argument side effects. The pure slot is represented but is not invoked.

Provider: 225,280 bytes, 173 nodes, 85 types, six tables/address points and eleven
entries. Main: 221,184 bytes, 100 nodes, 82 types. Storage summaries and tool hashes
are retained. Condition, return-object and bounded-body regressions pass native/
C#/NativeAOT at `*-virtual-regression-v1`; peak body nodes remain 251 of 1,531.
Calls and vptr writes share function-body leases; table/entry catalogs remain
resident and are reported. Constructor tables/VTT, virtual destructors, dynamic
virtual-base and covariant adjustments, and variadic slots remain explicit
unsupported generation paths. The existing separate value-initialization boundary
was also retained in the failed `virtual-v3` artifact.

Reproduce with `tests/verify_virtual.py` and the usual arguments. The real bounded
DuckDB probe at `dotobj-relational-duckdb-hugeint-v11/` advances to
`noninteger constant value (EDG kind 14)`, a dependent template value. No complete
DuckDB object is published yet. Source-shaped dependent-value projection is next.

## Verified dependent source value recipes

`artifacts/review/dotobj-relational-dependent-values-v2/` passes native C++, SQL
integrity, source-hidden C# and Linux x64 NativeAOT. `template_arguments.expression_id`
and `template_formals.default_expression_id` reference ordinary expression graphs;
`template_value_references` preserves EDG parameter coordinates and qualified member
references. The 35-table schema distinguishes recipe ownership with `nodes.domain`.
Recipe nodes always have NULL function ownership and remain browsable in SQL; the
consumer does not instantiate them or load them into executable body/global data.

The fixture covers `Gate<(N > 1)>`, `Gate<T::enabled>`, integer default `N + 1`, and
floating default `V + 0.5`. Concrete EDG instantiations execute across two units.
Provider: 208,896 bytes, 38 nodes, 46 types, including 13 recipe nodes. Main:
217,088 bytes, 84 nodes, 50 types, including 17 recipe nodes. Both link with zero
loaded global expression nodes. Recipe caches are scoped to a capture, avoiding
stale EDG function-region pointers. Unsupported dependent operations retain
contextual diagnostics instead of text payloads or managed semantic analysis.

Virtual, floating and bounded-body regressions pass native/C#/NativeAOT under
`*-dependent-regression-v1`; body retention remains 251 of 1,531 nodes. Preserved
14-table v3 objects also link and execute with the new reader at
`dotobj-relational-v3-dependent-compat`. Storage summaries and hashes are retained.
Reproduce with `tests/verify_dependent_values.py` and the usual arguments.

The actual DuckDB probe at `dotobj-relational-duckdb-hugeint-v13/` advances past the
`enable_if` dependent argument and reports `field owner`. This may be declaration
capture reentrancy and is being distinguished from a lowered ABI field before any
fix. No complete DuckDB object is published; the broad campaign remains stopped.

## Verified record definition completion

`artifacts/review/dotobj-relational-completion-v1/` passes native C++, SQL integrity,
source-hidden C# and Linux x64 NativeAOT. A function referencing `Later*` precedes
its record definition; later construction and member access use the same type ID.
The exporter now completes records initially seen incomplete using EDG's completed
source definition, updating layout/completeness and emitting fields/bases once.
The existing source prelower boundary completes pending records before synthetic
ABI fields appear. Already-complete records are not refreshed from lowered lists.
No schema expansion, observation history or consumer semantic inference is added.

Provider: 208,896 bytes, 23 nodes, 22 types; its one `Later` row has size/alignment
4/4 and one source field. Main: 208,896 bytes. Dependent-value and virtual-dispatch
regressions pass native/C#/NativeAOT at `*-completion-regression-v1`. Storage and
tool hashes are retained. Reproduce with `tests/verify_record_completion.py`.

Actual DuckDB `hugeint-v14` identified the missing field as `error_condition::__val_`,
with exact membership in EDG's owner field list. With completion fixed, bounded
`dotobj-relational-duckdb-hugeint-v15/` advances to expression operation EDG kind 83.
No complete DuckDB object is published. Probe timing remains early-rejection data;
the full campaign stays stopped while this next concrete frontier is investigated.

## Verified compound assignment computation and identity

`artifacts/review/dotobj-relational-compounds-v1/` passes native C++, SQL integrity,
source-hidden C# and Linux x64 NativeAOT. Schema version 1013 adds nullable
`nodes.operation_type_id`, the result of EDG's stock
`compound_assignment_operation_type` helper. Stored/result types and value
categories remain separate. The five bitwise/shift compound operators join the
existing arithmetic compounds. Generated helpers compute using the recorded type,
convert back to storage, preserve the recorded operand order and return the
original destination address. The consumer does not recompute C++ promotions.

Coverage includes narrow integer truncation, bool storage, signed/unsigned right
shift, mixed signedness, RHS-before-LHS effects exactly once, reference/address
aliasing of the result, and skipped conditional/short-circuit effects. Provider:
249,856 bytes, 317 nodes, 13 types. Main: 208,896 bytes, 4 nodes, 4 types. Helpers
retain scalar operator/type keys; per-body node maps are released. Complex lvalue
address forms and address-taken globals remain explicit unsupported paths.

Condition/lifetime and body-loading regressions pass native/C#/NativeAOT at
`*-compound-regression-v1`; peak body nodes remain 251 of 1,531. Preserved old
14-table objects still link/run at `dotobj-relational-v3-compound-compat`.
Reproduce with `tests/verify_compound_assignments.py`; storage/hashes are retained.
Actual bounded DuckDB `hugeint-v16` advances from `|=` to `va_start` (EDG operation
112). No complete object is published; this is still an early-rejection probe.
Next work projects EDG's explicit va_list role and builtin operations into the
existing portable cursor runtime, preserving source identity and nonexecuted
last-parameter anchors.

## Verified portable varargs and function values

`artifacts/review/dotobj-relational-varargs-v1/` passes native C++, SQL integrity,
source-hidden C# and Linux x64 NativeAOT. Schema version 1014 adds only
`types.semantic_role` for EDG's builtin va_list typedef/storage identities. Source
names, type chains and original layout/fields remain available in SQL. Five
builtin operations carry typed operands; `va_start`'s ignored source parameter
anchor uses a nonexecuted `last-parameter` edge. These additions directly support
cursor emission and source browsing, without frontend bookkeeping.

The consumer reuses unchanged shared VaArg/VaPack/VaList runtime sources. Variadic
methods receive promoted argument snapshots via ReadOnlySpan; each activation owns
a pack with bounded stack storage/native fallback and using disposal. Explicit
cursor forwarding advances the original; va_copy borrows an independent cursor.
Function pointer values use typed method addresses and indirect variadic calls
pass explicit spans. Generated source retains routine/parameter/local names.

Coverage includes promoted short/float/bool values, pointers, once-only argument
effects, forwarding, copying, empty packs, a selected function pointer and exception
cleanup. Provider: 221,184 bytes, 160 nodes, 32 types. Main: 217,088 bytes, 104 nodes,
27 types. Virtual-dispatch and returned-object regressions pass native/C#/NativeAOT
at `*-varargs-regression-v1`; old 14-table objects still link/run at
`dotobj-relational-v3-varargs-compat`. Storage and tool hashes are retained.
Reproduce with `tests/verify_varargs.py`. Aggregate variadic payloads and variadic
virtual dispatch remain explicit unsupported generation paths.

Actual bounded DuckDB `hugeint-v17` advances beyond va_start and rejects an array
aggregate initializer (constant kind 12/type kind 9). Its reported line/column
matches libc++'s 200-character decimal lookup table; file-confirming diagnostics
are next. No complete DuckDB object is published, and measurements still describe
early rejection. The next slice targets explicit array initialization destinations,
values/repeats and zero actions needed by the generator, not generic compiler data.

## Verified scalar array initialization and storage

`artifacts/review/dotobj-relational-arrays-v2/` passes native C++, SQL integrity,
source-hidden C# and Linux x64 NativeAOT. Schema 1015 has 37 tables; the two new
relations carry generator-needed zero actions and ordered selected array element
indices/repeats/value references. Nested initializers remain ordinary graph nodes.
The zero flag comes from EDG's selected partial-initialization flag; source static
zero arrays receive an explicit empty zero action using EDG's storage-duration
predicate. The consumer does not infer zeroing from omitted element rows.

Named fixed layouts retain EDG size/stride. Globals have aligned pinned GC backing;
fixed local arrays have aligned function-owned native storage, disposed after
source cleanup. Writes still execute at each source declaration, including loop
iterations. Array action rows share body leases and retention metrics. Globals
and their initializer data remain resident, as previously disclosed.

The fixture covers a character lookup table, partial/nested/empty local arrays,
repeated loop initialization, shared mutable globals and stable cross-unit pointers.
A destructor reads a local array on both normal return and exception unwinding.
An early extern reference exposed a cached-declaration bug: final source capture
now completes the same global symbol ID and records its initializer once. The
fixture verifies initialized and default-zero globals after such early references.
Provider: 237,568 bytes, 262 nodes, 34 types. Main: 229,376 bytes, 178 nodes, 19 types.

Varargs and bounded-body regressions pass native/C#/NativeAOT at
`*-arrays-regression-v1`; peak body nodes remain 251 of 1,531. Old 14-table objects
still link/run at `dotobj-relational-v3-arrays-compat`. Reproduce with
`tests/verify_arrays.py`; storage summaries and tool hashes are retained. Dynamic
array components, record-element construction/destruction, runtime counts, flattened
multidimensional repeats and local-static guards remain unsupported. Nonzero
relocated initializer constants and more general static ordering are also work.

Actual bounded DuckDB `hugeint-v19` advances beyond the array initializer and
rejects unsigned __int128 (integer kind 12). No complete object is published and
probe measurements remain early-rejection data. The next change uses existing
scalar type/literal columns and EDG's exact decimal formatter, adding no tables.

## Verified exact 128-bit scalar integers

`artifacts/review/dotobj-relational-wide-v1/` passes native C++, SQL integrity,
source-hidden C# and Linux x64 NativeAOT. Existing type/scalar columns now carry
`i128`/`u128`, source builtin names and EDG layout. All integer formatting uses
EDG's stock decimal formatter instead of a host64 conversion. No tables or payload
formats were added. Literals, template arguments/defaults and existing enum values
retain exact decimal scalars; C# wide literals use readable values or high/low
constructors with the exact decimal value shown alongside.

Coverage includes unsigned maximum and signed minimum, signed division/remainder
and shift, unsigned compound operations, narrowing, cross-unit calls, template
values/defaults, aligned static arrays, variadic wide values, pointer round trips
and long-double conversion. Provider: 217,088 bytes, 55 nodes, 17 types. Main:
233,472 bytes, 191 nodes, 25 types. Enum and dependent-template regressions pass
native/C#/NativeAOT at `*-wide-regression-v1`. Storage/tool hashes are retained;
reproduce with `tests/verify_wide_integers.py`. Wide enum execution remains an
explicit unsupported target representation.

Actual bounded DuckDB `hugeint-v20` advances to a non-array aggregate initializer.
No complete DuckDB object is published. Next work must identify its precise source
record and project the selected field/base destinations needed for generation;
the consumer must not reconstruct aggregate member selection.

## Verified selected record and base initializer actions

`artifacts/review/dotobj-relational-record-initializers-v2/` passes native C++, SQL
integrity, source-hidden C# and Linux x64 NativeAOT. Schema 1016 has 39 tables.
Two new relations carry selected field/base destinations and exact zero byte
extents. These are concrete generator actions, not general EDG metadata. Native
capture follows stock aggregate member selection and initialization lowering:
empty bases have no zero write; base extents exclude reusable tail padding.
The consumer never reconstructs member selection from declaration order.

Coverage includes the actual empty inherited hash temporary shape, partial and
designated fields, nested record/array initialization, constructor base actions,
record value returns and a cross-unit initialized record global. Global record
backing is pinned and aligned so selected base projections have stable addresses.
The hash check also exposed an if-test path bypassing temporary expansion; the
consumer now uses its existing selected initialization expansion there. Provider:
237,568 bytes, 174 nodes, 59 types. Main: 233,472 bytes, 75 nodes, 62 types.

Array, return-storage and bounded-body regressions pass native/C#/NativeAOT at
`*-records-regression-v1`; body retention remains one active function. Reproduce
with `tests/verify_record_initializers.py`; SQL storage and tool hashes are retained.
Dynamic record components, partial-construction cleanup, virtual layout actions,
overlapping nonempty fields and pointer-to-member zero models reject explicitly.

Actual bounded DuckDB `hugeint-v22` passes the inherited hash initializer and
reaches a constant address to the newline string in `exception.hpp:146`. It still
publishes no complete object. The 0.54 s / 162,304 KiB measurement is an early
rejection, not full-TU cost. Next work is exact literal storage and selected address
references, preserving EDG identity and target bytes without consumer pooling or
opaque semantic payloads. The broad campaign remains stopped.

## Verified exact string objects and selected addresses

`artifacts/review/dotobj-relational-literals-v4/` passes native C++, SQL integrity,
source-hidden C# and Linux x64 NativeAOT. Schema 1017 has 42 tables. Literal objects,
ordinary byte-offset/value rows, and selected array copy/zero actions represent
target character storage, including embedded zeros and wide encodings. Expression
nodes reference selected objects and byte offsets. No content pooling or UTF
conversion is performed by the consumer. Character builtin names retain wchar_t,
char8_t, char16_t and char32_t identity alongside their target scalar storage.

Two narrow stock-lowering callbacks observe selected promoted literal variables
and release source-function identity maps before region reclamation. Final linker
names come from EDG's selected variable. Callback-only transient correspondence
is not dumped unless the source program references it. Release builds use explicit
captured ownership or stable promoted identity, not a checking-only region field.
Copy/zero action rows exist only for actual initialization contexts: three in the
provider fixture, none for main's literal reads. Exact bytes are streamed during
C# generation into aligned pinned backing; only object metadata remains resident.

Coverage includes embedded zeros, all four wider/UTF character spellings, constexpr
literal references, a selected nonzero address offset, and local/global padded
arrays. SQL checks retained inline-literal linker identity. Pointer equality across
GCC's constexpr and runtime string evaluations was not used as an oracle for EDG's
pooling decisions. Provider: 249,856 bytes, 114 nodes, 41 types, 11 literal objects
and 82 bytes of literal content. Main: 278,528 bytes, 259 nodes, 40 types, two literal
objects and 24 bytes of content. Storage summaries/tool hashes are retained.

Array and bounded-body regressions pass native/C#/NativeAOT at
`*-literals-regression-v1`; peak body nodes remain 251 of 1,531. Original 14-table
objects still link/run at `dotobj-relational-v3-literals-compat`. Reproduce with
`tests/verify_literals.py`. General relocated variable/routine address constants
remain unsupported; callback-known promoted literal variables are handled.

Actual bounded DuckDB `hugeint-v24` advances to the selected default field
initializer `ValidityArray::validity_mask = nullptr` in `validity_mask.hpp:483`.
No complete object is published. Its 0.84 s / 219,136 KiB is early-rejection cost,
not a full-TU measurement. The next bounded change can reuse existing selected
constructor initialization rows for this scalar case; richer defaults requiring
remapping/destruction must follow stock EDG decisions. Broad campaign remains stopped.

## Verified scalar default member initialization

`artifacts/review/dotobj-relational-default-members-v1/` passes native C++, SQL
integrity, source-hidden C# and Linux x64 NativeAOT. No schema or managed changes
were needed. When EDG explicitly selects a field initializer, the exporter reuses
its scalar constant through existing constructor initialization rows. It does not
infer a default from an absent explicit initializer. Constructor-dependent
expressions, variable/master bindings and lifetime/destruction actions remain
explicitly unsupported until stock copying/remapping decisions are represented.

Coverage includes typed pointer zero, booleans, signed and floating constants,
enums, explicit constructor overrides, inherited construction, and instantiated
template members across two units. Provider: 253,952 bytes, 123 nodes, 49 types.
Main: 249,856 bytes, 73 nodes, 51 types. Reproduce with
`tests/verify_default_members.py`; storage summaries and tested tool/header hashes
are retained. The lifetime/EH regression passes native/C#/NativeAOT at `*-defaults-regression-v1`.

Actual bounded DuckDB `hugeint-v26` identifies a runtime-count scalar array new
at `string.hpp:290`: selected operator new[], no explicit initialization, placement,
alignment, destructor or exception cleanup. It still publishes no complete object.
The next step projects the exact allocation size/count and selected call decisions
from stock lowering; it must not replace C++ allocation failure behavior with a
CLR exception or override a supplied allocator. The broad campaign remains stopped.

## Verified selected plain allocation expressions

`artifacts/review/dotobj-relational-allocations-v2/` passes native C++, SQL integrity,
source-hidden C# and Linux x64 NativeAOT. Schema 1018 has 43 tables. One allocation
relation references the source count, EDG-selected count temporary, ordinary
check/size expression graphs, selected failure/allocator symbols and result
adjustment. Five narrow hooks observe stock decisions; no whole lowered function
graph is collected. Synthetic expressions retain the owning source new node.
Runtime array type metadata does not demand a fixed generated array layout.

Expression-local helpers bind the selected count once, test the exact predicate,
call the selected failure routine, and pass the exact size expression to the
selected allocator. Fixed-count allocation retains no unused count/check actions.
C# preserves stock C-like integer comparison/logical results as explicit 1/0 values.
No consumer sizeof/cookie/overflow rules or blanket host allocator replacements
were introduced. Initialization, placement/alignment, class/cookie handling and
rollback remain explicit unsupported boundaries.

The fixture supplies arena allocator and length-failure definitions, exercising
count effects once, zero/invalid/fixed counts, conditional evaluation and allocator
exceptions. It does not establish standard allocation-runtime completeness. Provider:
245,760 bytes, 56 nodes, 19 types, one allocation. Main: 286,720 bytes, 276 nodes,
22 types, six allocations. Conditions and bounded-body regressions pass
native/C#/NativeAOT at `*-allocations-regression-v1`; peak body nodes remain 251
of 1,531. Storage summaries/tool hashes are retained; reproduce with
`tests/verify_allocations.py`.

Actual `hugeint-v27` advances to `delete[] ptr` at `string.hpp:299`, with selected
operator delete[], a scalar character pointee and no destructor. No complete
DuckDB object is published; 0.86 s / 221,184 KiB remains early-rejection cost.
Next delete work can use ordinary selected call nodes/edges, preserving stock
argument conversions and null behavior without another table. Broad campaign
remains stopped.

## Selected plain deallocation calls (2026-10-11)

Verified `dotobj-relational-deallocations-v1` passes native C++, source-hidden generated
C#, and Linux NativeAOT. The schema remains 1018 / 43 tables: source `delete` nodes
retain their pointer operand for navigation and an `execution` edge to the exact
selected routine call. Stock `modify_delete_call_args` supplies pointer casts and
any sized/aligned scalar arguments before the call is captured. Synthesized call
nodes point back to their source node; no generic lowered body snapshot is retained.

The supported stock plain branch delegates null handling to the selected routine.
C# executes that supplied call graph without adding a null guard; the source pointer
is evaluated once through its execution argument. The fixture supplies a null-safe
arena deallocator and checks side effects, skipped branches and cross-TU release.
Class/destructor/cookie/destroying-delete paths remain explicit unsupported cases.
No external runtime binding is claimed. Provider: 253,952 bytes / 85 nodes / 23 types;
main: 290,816 bytes / 353 nodes / 27 types. Per-table storage summaries and tested
tool hashes are retained in the artifact directory.
Allocation regression also passes native/C#/NativeAOT at
`dotobj-relational-allocations-deallocations-regression-v1`.

Actual `hugeint-v28` gets past `delete[] ptr` and rejects statement kind 14
(range-based for). No complete DuckDB object is published: 0.96 s / 250,624 KiB
is early-rejection cost. Next work is the selected range iteration graph and its
source/lifetime relationships. Broad campaign remains stopped.

## Resolved range iteration and lifetimes (2026-10-11)

Verified `dotobj-relational-ranges-v3` passes native C++, source-hidden generated
C#, and Linux NativeAOT. No schema/table additions: source `range-for` nodes retain
EDG's selected range/begin/end variable initialization, comparison and increment
expressions. The root owns the actual outer range scope lifetime; a source-linked
synthetic body owns the iterator scope lifetime and contains the selected iterator
initialization followed by the source body. This follows the actions documented in
stock `lower_range_based_for_statement` without exporting its generic lowered body.

C# preserves optional C++20 initialization scope, initialization order and iterator
cleanup before increment, including continue, break, return and exception paths.
Tests cover scalar arrays, reference mutation, a custom cursor with a different
sentinel type, class-valued iteration variables, nested loops, constructor failure,
and distinct outer/per-iteration destruction. The initial fixture's nonconstant
aggregate returns reached an existing unsupported initializer; explicit constructors
isolate range behavior. That aggregate boundary remains unchanged.
main: 311,296 bytes / 429 nodes / 63 types
provider: 266,240 bytes / 136 nodes / 69 types.
Per-table storage/tool hashes are retained. The condition-loop regression also passes
native/C#/NativeAOT at `dotobj-relational-conditions-ranges-regression-v1`.

Actual `hugeint-v29` gets past ranges and rejects the selected class-valued default
member initializer `ColumnIndex::type = LogicalType::INVALID` (constructor action
with destructor). No full DuckDB object is published: 1.11 s / 274,432 KiB is still
early-rejection cost. Next work must capture the selected per-constructor action
without reconstructing its remapping or cleanup. Broad campaign remains stopped.

## Copied class default initialization and rollback (2026-10-11)

Verified `dotobj-relational-class-defaults-v2` passes native C++, source-hidden
C#, and Linux NativeAOT. Schema remains 1018 / 43 tables. Source-selected class
constructors with scalar constant arguments use the existing initialization rows.
A narrow callback at the end of stock
`copy_non_static_data_member_initializers_if_necessary` associates the exact copied
initializer with that row, records its finalized flags/lifetime, and writes the
stock constructor-scope destruction order once. The incomplete pre-copy root list
is deferred. Only per-constructor identity maps survive to this callback and are
then released; no expression cloning or cleanup-order reconstruction is performed.

Tests cover normal defaults, explicit overrides, the same field default in two
constructors, and failure after earlier members completed. Default arguments that
refer to another member remain unsupported and reject before object publication.
Constructor-dependent remapping, expression temporary/overlap cleanup and partial
aggregate copying remain explicit boundaries; this is not general default-member
initializer support. EDG's ctor-dependent flag alone is not sufficient to identify
all nonconstant member expressions, so the narrow argument-shape check also applies.

Review also found a constructor-exit cleanup bug: a noexcept(false) body-local
destructor can throw after the generated completion flag is set. Member cleanup
now also considers that cleanup region's existing unwinding flag, so already
constructed members unwind in the supplied order. Both fallthrough and explicit
return exits pass, including NativeAOT (`dotobj-relational-constructor-exit-v1` and
the combined fixture). Successful exits still preserve member lifetime.
main: 258,048 bytes / 160 nodes / 49 types
provider: 245,760 bytes / 60 nodes / 57 types.
Per-table storage and tested tool hashes are retained. Original lifetime/EH and
scalar-default regressions pass native/C#/NativeAOT at `*-class-defaults-regression-v1`.

Actual `hugeint-v30` gets past `ColumnIndex::type` and rejects the switch in
`TryCastStringBool`, `cast_operators.hpp:483`. No complete DuckDB object is published;
1.15 s / 293,888 KiB remains early-rejection cost. Next work
is selected switch labels/control-flow using ordinary source nodes and edges.
Broad campaign remains stopped.

## Retention purpose audit (2026-10-11)

The resolved program graph includes requested source browsing facts beyond current
executable generation. `templates`, `template_instances`, `template_arguments`,
`template_formals`, `template_type_parameters`, `template_value_references`, and
`empty_parameter_packs` currently have no managed linker/generator readers. Their
purpose is ordinary SQL navigation from declarations to instantiations/selected
arguments, pack boundaries, formal defaults and dependent references; `browse.sql`
and dependent-value fixtures exercise that purpose. They must not be described as
necessary runtime inputs merely because they are retained.

`nodes.domain` is consumed to exclude template recipes from execution/body loading;
`types.is_dependent` prevents realization of unresolved types. `source_node_id` and
`is_synthetic` preserve source-to-selected-action navigation (allocation/deletion/
range initialization); C# execution follows explicit selected action edges instead.
Node source locations feed diagnostics; declaration locations also serve browsing.
This is a bounded purpose audit, not a claim that every retained scalar has been
proven minimal. Each added relation must have a concrete linker/generator or
requested navigation purpose. No metadata removal or new collection follows merely
from the absence of a current managed reader.

## Remaining work

This is a new executable foundation, not a complete C++ implementation. Remaining
lifecycle emission includes nontrivial/dynamic arrays, virtual-base/delegating construction, function-try
blocks, nontrivial expression temporaries, overlap/unordered cleanup, nontrivial
by-value catches, pointer/base-conversion catches, and dynamic/partial aggregate
initialization. Those unsupported execution paths reject explicitly. Finalized
`noexcept` policy is emitted; this slice does not separately exercise termination
or destructor failure during unwinding. More complex constrained expression
evaluation still needs expansion beyond the supported assignment positions.
Do not infer C++ rules in the reader or add another frontend snapshot layer.

Also incomplete: source template formals/dependent type variants, all declarations
and macros, complex initialization, allocations/statics/TLS, ABI-sensitive C++
features and full library/runtime closure. Function bodies now load one at a time,
but type/symbol catalogs and global expressions are still materialized; bounded
whole-project loading is not yet implemented. The next real-project checkpoint
must be a bounded DuckDB subset, not another blind full-corpus export or a
compression project.
