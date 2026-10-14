# Whole-project DuckDB RoslynObj export

The user authorized attempting RoslynObj generation for the whole configured
DuckDB project on 2026-10-14. Generation is in progress; no whole-project success
or faithful equivalence for all host options is claimed yet. This task follows the
accepted phases 3–4 checkpoint `fc22240f6`.

## Pinned source and configured scope

The clean official current-main checkout is commit
`a809d3cd519df6db70223c562c4de80da311868b`, version `v2.1.0-dev1`, not a stable
release. Its dedicated root is `/mnt/storage/roslynobj-duckdb-20261014`, referenced
by the ignored workspace path `edgcpp/artifacts/roslynobj/duckdb`.

The fresh Release/Ninja configuration enables the main library, shell and C++
unit tests with default unity and bundled core_functions/parquet/json/icu.
Configuration creates the compile database without building the host library.
There are 650 distinct configured command entries for 640 source paths and 243
targets: 575 effective C++17, 65 GNU C99 and 10 host-default GNU C17 invocations.
Each actual unity translation unit receives one object; source-path duplicates
with different commands remain distinct. This scope excludes optional external
extensions and other platform/build variants.

The actual GCC 13.3 include paths, per-command defines and target predefines are
recorded. All 650 commands select x86-64-v3. Host exception/RTTI choices and C/C++
dialects are preserved rather than inherited from earlier benchmark recipes.
Neither DuckDB nor EDG core source is rewritten. No excluded dotobj campaign
results, recipes or findings are used.

## Concrete exporter corrections

A genuine C++ entry first exposed a constant registration-order bug. Attaching its
source correspondence could reenter the same constant through a parent scope,
record instance and template arguments before its exact content/value had been
registered. The exporter mistook this metadata reentry for an inline content
cycle. It now registers exact content and the value mapping before attaching
correspondence; content, union traversal and the cycle guard remain unchanged.

A second failure exposed an actual null EDG `derived_class` pointer on retained
prototype constructor-initializer base placeholders. Format 2.0 preserves that
absence in a nullable column/property, keeping the base type, identity and
initializer link. It does not derive an owner from traversal ancestry. The reader
admits exactly the published 1.0 and current 2.0 contracts; [FORMAT.md](FORMAT.md)
and [the exact comparison](nullable-base-major2-contract.json) document the change.

Focused current native outputs for configured entries 0 and 316 now publish and
pass reader admission. Canonical prototype scope/initializer/base navigation
passes 15 assertions; four genuine old/new objects pass admission, and a genuine
earlier development object is rejected. Existing constant/template fixtures pass
173 assertions with their preserved recipes. The managed build is clean and the
current reader self-check passes 73 assertions. This is focused regression
acceptance, not another full profile or whole-field campaign. The native profile
was rebuilt; the other 13 accepted profile builds remain historical 1.0 evidence.

A separate crash trace showed exhaustion of the default 8 MiB process stack:
the stack pointer reached the mapping boundary during a deep exporter traversal.
An explicit 64 MiB process stack lets the same entry 18 publish a 697,355,264-byte
object. This is a recorded runtime limit adjustment, without changing EDG IL or
exported semantics. GNU C header diagnostics were separately traced to EDG's
strict GNU mode when selecting C99/C17; the supported `--no_strict_gnu` option
restores the host GNU dialect. A corrected C entry 164 publishes.

## Campaign provenance and remaining limits

The initial baseline uses an immutable copied 1.0 producer with the constant-order
fix. Completed baseline results, commands and diagnostics are retained. Corrected
2.0 attempts use a separate directory and compiler keyed by its hash. Final
selection will cover every configured entry and retain links to all earlier
attempts; successful unchanged 1.0 objects can be admitted without re-export.

The first 26 baseline invocations lacked ten actual target predefines. Their
original recipes remain immutable; final selection must mark those attempts as
configuration deltas and require corrected selected outputs. All 75 C invocations require the
GNU dialect correction, including any baseline successes.

There is no exact stock EDG mapping for the default/inline visibility options in
189 configured extension invocations. These are attempted but remain explicit
configuration deltas, even if they produce objects. Visibility is not guessed
from source or replaced with a blanket pragma. The final report will separate
faithfully mapped invocations, configuration deltas, frontend diagnostics,
backend publication failures, crashes and reader admission results.

Four export workers share the native worker's sole scheduling ownership. Pilot
RSS/output sizes and available disk govern this bound. Per-entry commands, source
hashes, producer hashes, exit codes, wall time/RSS, diagnostic logs and output
sizes are recorded. Failed attempts are not successful objects. The exporter
checks foreign keys before publication; project admission does not repeat a full
large-object semantic/database scan for every successful TU.

The managed lane opens selected successful objects and samples actual retained
symbol/type/function/operation properties. Its pending generic sampler caps name
matches and routine roots; no whole-program graph is constructed. FindSymbols
and GetReferences return lazy paged lists, but SQL searches may still inspect
many rows and Count uses a SQL count. Selected navigation is not C++ lookup,
whole-TU validation or a constant-time query guarantee.
