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
features and full library/runtime closure. Current C# reader holds its small
input units in memory; bounded whole-project loading is not yet implemented.
The next real-project checkpoint must be a bounded DuckDB subset, not another
blind full-corpus export or a compression project.
