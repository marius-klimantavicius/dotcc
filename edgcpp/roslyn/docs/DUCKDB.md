# DuckDB core library RoslynObj export

The requested core-library scope is complete: all 164 configured compilation
entries publish independent RoslynObj files and pass managed reader admission.
There are no core frontend/backend failures or unmapped semantic flags. The
selected files total 104,942,180,352 bytes (97.735 GiB).

The source is a clean official current-main checkout at
`a809d3cd519df6db70223c562c4de80da311868b`, version `v2.1.0-dev1`, not a stable
release. The dedicated directory is `/mnt/storage/roslynobj-duckdb-20261014`,
referenced by the ignored workspace path `edgcpp/artifacts/roslynobj/duckdb`.

## Exact core scope

Core ownership follows configured object outputs starting with `src/`, rather
than source filenames alone. This selects entries 227–390: 163 C++17 invocations
and one GNU C17 invocation, across 145 core targets and 164 direct input paths.
It includes generated `duckdb_static_extension_loader.c` and
`static_extension_autoregister.cpp` compiled into the main library. Separate test,
shell, tools, bundled-extension implementation and third-party targets do not
count toward this result. Headers and other included dependencies remain part of
the actual core translation units.

There are 139 unity invocations with 1,627 distinct recorded included source
paths. Direct inputs and these included paths form a union of 1,791 paths,
including generated unity paths; this is not a count of independent translation
units. Coverage comes from configured commands and actual generated unity include
lists, without claiming optional platform/build variants.

The fresh Release/Ninja configuration was originally used to enumerate the whole
project with the default bundled core_functions/parquet/json/icu, shell and tests.
The user's final instruction restricted completion to the main core-library
commands above. These existing commands were selected without reconfiguring or
changing their source, defines, include paths or target choices. The actual GCC
13.3 system includes, host predefines and x86-64-v3 feature deltas are recorded.
Host exception/RTTI choices and GNU C dialect are preserved. Neither DuckDB nor
EDG core source was rewritten, and no excluded dotobj findings or recipes were used.

## Artifacts and proof

Committed [core manifest](duckdb-core-manifest.json),
[summary](duckdb-core-summary.json) and
[reader reconciliation](duckdb-core-reader-admission.json) freeze the accepted
result. [Native](duckdb-core-native-ready.json) and
[managed](duckdb-core-managed-ready.json) inventories record the evidence hashes.

The local artifact directory contains:

- `export/selected-core-final.json`: the 164 selected paths, hashes, versions and
  exact attempt links.
- `export/core-final-summary.json`: scope, totals, versions and configuration.
- `export/core-per-entry-final-summary.json`: original commands, actual EDG
  commands, logs, output sizes and timing/resource records.
- `export/core-source-coverage.json`: direct and unity source/target coverage.
- `export/reader-admission/journal.jsonl`: incremental reader admission records
  keyed by immutable path/hash and actual artifact identity.

The core set uses 163 current format 2.0 objects and one accepted format 1.0
object, entry 228. Exact admission is documented in [FORMAT.md](FORMAT.md).
The exporter checks foreign keys before atomic publication. Every selected core
object was opened and disposed by the managed reader; final reconciliation uses
these existing proofs without reopening unchanged objects or scanning their
whole graphs. Native header/publication checks and reader admission are recorded
separately.

A bounded property/visitor sample on the actual core boolean-operator input
(entry 316) passes 2,193 assertions over four roots, 404 operations and 40 calls.
The earlier C allocator sample belongs to a separate dependency target and is not
counted as core evidence. [The frozen sampler evidence](duckdb-project-sampler-ready.json)
retains both original sample scopes and cache/query/RSS limits. Name searches are
paged but may still inspect many SQL rows; sampled navigation is not C++ lookup,
whole-TU validation or execution equivalence.

The [owned project driver](../build/project_export.py) prepares exact configured
invocations, records source/recipe hashes and actual host predefines, freezes a
content-addressed compiler, and supports graceful stop/resume and selection of
completed attempts. It refuses to overwrite an existing prepared manifest. Each
object remains an independent translation unit; neither exporter nor reader links
or merges the project.

## Exporter corrections and exploratory history

The concrete fixes were committed in `98f337453`; the reproducible driver and
bounded consumer were committed in `de5e9cc2c`.

A real constant's correspondence traversal reentered it before its exact value
had been registered. Registering content and the value mapping before attaching
metadata fixes this false cycle diagnosis without changing the payload or cycle
guard. A separate valid EDG prototype base placeholder has a null `derived_class`
pointer; format 2.0 preserves the nullable owner while retaining base type,
identity and constructor-initializer associations. See [the contract comparison](nullable-base-major2-contract.json),
[managed fix proof](duckdb-major2-managed-ready.json) and
[native fix proof](duckdb-major2-native-ready.json).

A representative trace also established exhaustion of the default 8 MiB stack;
corrected project runs use an explicit 64 MiB process stack. GNU C invocations use
the supported `--no_strict_gnu` option to match the configured GNU dialect. Actual
runtime limits and timings are retained per attempt; missing older pilot limit
metadata stays unavailable. Stock frontend/backend CPU times are distinct from
combined invocation wall time, and summed invocation wall time is not concurrent
campaign elapsed time.

The original 650-entry exploratory campaign and all earlier results remain under
`export/project` and `export/major2`. Their commands, provisional architecture/GNU
C attempts, failures and interruptions are preserved. The user then excluded
tests and restricted the task to core library source. Noncore queued work stopped
and active commands drained safely. Noncore visibility-option differences and
Catch test diagnostics are outside the accepted core scope, not core failures.
No remaining test triage, dialect escalation or optional-source campaign was run.
