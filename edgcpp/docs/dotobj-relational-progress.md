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

## Remaining work

This is a new executable foundation, not a complete C++ implementation. Remaining
lifecycle emission includes arrays, virtual-base/delegating construction, function-try
blocks, nontrivial expression temporaries, overlap/unordered cleanup, nontrivial
by-value catches, pointer/base-conversion catches, and value/partial aggregate
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
