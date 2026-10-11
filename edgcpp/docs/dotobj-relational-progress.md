# Relational program implementation — first executable slice

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

## Remaining work

This is a new executable foundation, not a complete C++ implementation. Lifetime
and exception facts/execution are the next required integration. The native
source exposes concrete EDG data for lifetime parent/order, dynamic initialization,
selected construction/destruction, conditional and partial-construction cleanup,
try/handler/throw regions and function exception specifications. Store these as
ordinary named relations and reuse existing runtime algorithms where applicable.
Do not infer C++ destruction rules in the reader or create another frontend graph
snapshot layer. Add the actual required facts as each supported construct is
implemented; missing export/emission remains an explicit diagnostic.

Also incomplete: source template formals/dependent type variants, all declarations
and macros, complex initialization, allocations/statics/TLS, ABI-sensitive C++
features and full library/runtime closure. Current C# reader holds its small
input units in memory; bounded whole-project loading is not yet implemented.
The next real-project checkpoint must be a bounded DuckDB subset, not another
blind full-corpus export or a compression project.
