# Managed embedded PostgreSQL API

The authored owning API and canonical consumer execute the complete translated
backend and original in-process initializer, including required PL/pgSQL and
Snowball modules. Private raw JIT and normally trimmed Linux x64 NativeAOT each
pass all 36 SQL/ownership assertions across three clean same-process lifetimes
and two persistence reopens against receipt `20261022-202228-8f7e10f1`.

Final paired publication `20261022-211008-fe98b7c0` supplies both canonical
backend/frontend forms and their bootstrap assets and notices. All four
canonical raw/processed × JIT/normally trimmed NativeAOT rows pass all 36
assertions, three lifetimes and two reopens each. The ordinary four-project
solution also builds with its default processed backend/frontend references.
All 1,900 published-output hashes and 57 authored-source hashes remain unchanged.
The supervisor combines this product qualification with the passing boundary,
import and provenance ledgers in `artifacts/final-qualification.json`; selected
acceptance through P6 is complete.

`src/Api/PGlite.csproj` builds the separate `Managed.PGlite` owning assembly. It
references the complete backend through `TranslatedProject` and the original
initdb frontend through `InitdbTranslatedProject`. Defaults are
`generated/TranslatedPGlite/TranslatedPGlite.csproj` and
`generated/TranslatedInitdb/TranslatedInitdb.csproj`. Raw/processed selection uses
those ordinary MSBuild properties. The generated backend contains `PgliteOwner`,
the backend initialization adapter and process transport. This API compiles only
`InitializationFrontend.cs` and its internal stable-argument helper alongside its
own sources, avoiding a backend-to-frontend dependency cycle.

```csharp
using Managed.PGlite;

var options = new PgliteOptions("/private/example/data",
    "/staged/translated-bootstrap-prefix", "/private/example/diagnostics");
using (var database = PgliteDatabase.Create(options))
{
    database.Query("CREATE TABLE items(id integer PRIMARY KEY, note text)")
        .EnsureSuccess();
    database.Query("INSERT INTO items VALUES (1, 'Vilnius Žąsis')")
        .EnsureSuccess();
    var result = database.Query("SELECT id, note, NULL::text FROM items")
        .EnsureSuccess();
    Console.WriteLine(result.ResultSets[0].Rows[0][1]);
}
using (var reopened = PgliteDatabase.Open(options))
    Console.WriteLine(reopened.Query("SELECT count(*) FROM items")
        .EnsureSuccess().ResultSets[0].Rows[0][0]);
```

Create requires a new or empty data directory. It invokes original translated
initdb, preserving its original BKI processing, bootstrap, post-bootstrap SQL,
PL/pgSQL and Snowball loading, and synchronization. The staged prefix contains
copied upstream data, translated-entrypoint descriptors and both static
translated-module manifests. Those descriptors are paths for the original executable-discovery
checks; all actual backend invocations call the original translated entrypoint
in process. No shell or native database executable is launched. Initializer
stdout/stderr and actual backend command spools remain in the session diagnostics
directory. `backend-invocations.json` records each actual backend operation,
arguments, exit status or failure. A successful initialization requires observed
successful Bootstrap and SingleUser operations, as well as the resulting cluster
files; command counts alone cannot satisfy this check. An unsuccessful
initialization throws and leaves diagnostics and any partial data for inspection.
If preserving diagnostics also fails while a backend exception is unwinding,
both causes are retained, with the original backend failure first. A diagnostic
failure without a backend exception propagates directly.

Open creates a fresh generated `Managed.PGlite.Translated.PGlite` state context.
Startup binds original stdio, calls the shared
`InitializationBackend.RegisterModules(bootstrapPrefix)` for both original
translated module catalogs in that fresh context. The PL/pgSQL catalog and the
Snowball catalog, including the original Snowball algorithms, are shared with
initializer invocations. Startup then
installs managed function-pointer callbacks and invokes original single-user
main. It requires the fork's 99 yield, opens an owned postmaster-liveness pipe,
attaches the original protocol port and verifies `SELECT 1` before returning a
ready owner. The selected managed poll implementation supports the bound file
descriptor at the dummy port's fd 1; the native oracle's epoll/socketpair setup
is a different host prerequisite.

Query encodes a protocol-v3 Simple Query frame. The read callback copies available
request bytes into the borrowed core buffer. The write callback copies borrowed
response bytes before returning, checking LP64 lengths and the configured response
limit. PostgreSQL's original parser, planner, executor, transaction machinery and
storage produce every result. A 100 yield dispatches original
`PostgresMainLongJmp` recovery through `PgliteOwner`; original ready-for-query and
flush exports then complete the response. The host requires complete input
consumption and a valid final ReadyForQuery frame.

SQL errors are values in `PgliteQueryResult.Errors`, with the original SQLSTATE,
message and error fields. `EnsureSuccess()` optionally throws `PgliteSqlException`
while preserving the usable recovered session and full response. Transaction
status distinguishes idle, in-transaction and failed-transaction states; a failed
transaction still requires SQL ROLLBACK. Returned column metadata, rows, nullable
text fields, errors and notices are managed copies. They remain valid after
subsequent queries, disposal and reopen. No core pointer is exposed or borrowed
past the callback. Multiple commands produce distinct result sets. Unsupported
protocol messages fail explicitly; this API currently targets text Simple Query,
without COPY or extended/binary query protocols.

An owner stays on its creating managed thread and rejects concurrent owners,
foreign-thread entry and callback reentry. A sibling `.dotcc-owner` file is held
exclusively through initialization and the SQL lifetime. Arguments, transport
storage and callback targets remain rooted through real shutdown and cleanup.
The session explicitly selects UTF-8 client encoding. NUL SQL and invalid UTF-16
are rejected before backend entry. Host/core/protocol
failures quarantine the lifetime and retain its roots and exclusion lease; they
never pretend to be a clean close. The initial failure remains attached to the
quarantine error.

Dispose disables embedding, supplies the exact Terminate frame and runs the
original backend loop without yield interception. `PgliteOwner` requires actual
`proc_exit(0)` completion, then drains original fork cleanup callbacks and retires
the generated context. Only afterward does the API release argv, copied buffers
and directory exclusion, and restore the original process environment. Successful
disposal is idempotent. The caller's data,
bootstrap prefix and diagnostics persist. Every reopen gets a new generated
context with original global initializers; no PostgreSQL globals are manually
reset.

The canonical `samples/ManagedConsumer` initializes a private cluster and reads
the unchanged `tests/native/cases.json` inventory. It checks all 21 exact native
cases across three clean same-process lifetimes, then checks ordinary SQL-error
recovery, caller-text rejection, thread affinity and owner exclusion in each
lifetime: 36 assertions total. GC collections before SQL calls exercise retained
callback/argument roots; a response is inspected after close and reopen. It uses
ordinary project references and `JsonDocument`, with no reflection or dynamic
root additions. Bootstrap assets are provided by the campaign recipe.

```bash
dotnet build pglite/ManagedConsumer.slnx -c Release
dotnet run --project pglite/samples/ManagedConsumer -c Release -- \
  --bootstrap-prefix /absolute/staged/bootstrap-prefix --work /private/run-parent
```

For a private backend/frontend, pass `-p:TranslatedProject=/absolute/backend.csproj`
and `-p:InitdbTranslatedProject=/absolute/frontend.csproj` to build/run/publish.
The optional `--backend-executable` must normalize to the selected bootstrap
prefix's `bin/postgres`. Original initdb discovers that exact sibling path from
`bin/initdb`; another override is rejected before initialization. `--cases`
selects the canonical inventory location. Both `plpgsql` and `dict_snowball`
translated-module descriptors are required during options normalization, before
any owner lease or generated context is created. NativeAOT
publication uses the same consumer with normal trimming and `-p:PublishAot=true`.
Its authored `NativeAotNotices.targets` copies exact `LICENSE.TXT` and
`THIRD-PARTY-NOTICES.TXT` files from the SDK's resolved target NativeAOT runtime
pack into `notices/` beside the published executable. `DOTNET-NATIVEAOT.txt`
records .NET attribution, the selected package/version/RID and both SHA-256
hashes. Missing or ambiguous resolved-pack evidence fails publication.

## Product execution evidence

| Products | Execution | Result |
| --- | --- | --- |
| Private coherent928, `202228-8f7e10f1` | Raw JIT | Passed: 36 assertions, three lifetimes, two reopens. |
| Same private products | Raw normally trimmed NativeAOT | Passed: 36 assertions, three lifetimes, two reopens. |
| Canonical publication, `211008-fe98b7c0` | Raw JIT | Passed: 36 assertions, three lifetimes, two reopens. |
| Same canonical publication | Raw normally trimmed NativeAOT | Passed: 36 assertions, three lifetimes, two reopens; observer correction below. |
| Same canonical publication | Processed JIT | Passed: 36 assertions, three lifetimes, two reopens. |
| Same canonical publication | Processed normally trimmed NativeAOT | Passed: 36 assertions, three lifetimes, two reopens; observer correction below. |
| Same canonical publication | Ordinary solution, default references | Passed; exact API/sample/backend/frontend graph, no duplicate SQL run. |

Private execution commands, actual logs, session diagnostics and exact project
identities are recorded in `artifacts/product-owner-r3/raw928/owner-report.json`.
That NativeAOT publish has 14 C# warnings, zero errors and no native/trimming
warnings; its run exits zero. These passes qualify that private source receipt.

The final canonical matrix uses the actual campaign recipe, shared consumer
hooks and `Context` runner, invoked from `/tmp` with `--fetch never` and warn
provenance. Each row has a separate private whole-graph `--artifacts-path`, fresh
data and the selected form's backend, original frontend and bootstrap prefix.
All four actual compile/publish and execution commands, the solution build and
two default-reference evaluations pass with exit zero. Each build/publish has
14 C# warnings and zero errors; neither NativeAOT publish reports native/trimming
warnings. Commands, source/publication hashes before and after execution, and
the original observer results are retained in
`artifacts/product-owner-r3/canonical-final/20261022-211359-8ba5a1f9/receipt.json`.
The passed translation receipt is
`artifacts/campaign/20261022-211008-fe98b7c0/receipt.json`. Historical private
passes remain separate from these newly executed canonical rows.

The original private observer incorrectly expected CoreLib under `lib/net10.0`;
both actual NativeAOT responses reference
`runtimes/linux-x64/native/System.Private.CoreLib.dll`. The completed original
Context receipt remains **failed** with those two observer errors and is
preserved byte-for-byte as `receipt.initial-observer-failure.json`. Its eleven
actual command records all remain passed; it has not been rewritten as a
successful attempt.

The separate `qualification-revalidation.json` beside that receipt passes a
read-only check of the retained four actual 36/3/2 executions, ordinary default
solution graph, exact canonical case inventories, compiled-pack notices and
all published/authored hashes. It records the corrected observer SHA-256,
original failed receipt SHA-256 and complete error history. No build, publish
or SQL execution is repeated for this correction. `owner-report.json` in the
same directory summarizes the verified results and evidence paths.

## Related-library delivery controls

Earlier isolated delivery evidence is retained under `artifacts/product-owner-r2/`.
`framework-companions.log` passes 13 isolated framework tests: transactional
publication of both backend and frontend forms, final-companion rollback of all
four prior trees and the current receipt, preflight rejection without mutation,
main-only authored hooks, semantic-copy isolation, and warn/strict/off companion
provenance behavior, including missing companion hash evidence. The runner
fixtures supply no SQL responses.

The actual PGlite delivery checks now pass five tests against the recipe
and configuration hooks. Raw/processed host copies remain separate, original
authored bytes survive simulated semantic rewrites, original source links are
restored, and all four consumer command rows select matching backend/frontend
projects and the backend form's bootstrap prefix. Recipe/form state is restored
after a failed build. A complete manifest still cannot publish the four-unit
`native-diagnostic` subset; that profile remains probe-only.

`msbuild-graph-evaluation.json` records six successful real MSBuild evaluations
from `/tmp`, covering the default, explicit raw and private-frontend selections
for both API and consumer. It checks original compile links, the canonical case
file's build/publish copy settings, and all four required solution projects.
These evaluations are graph controls; the actual API/consumer builds, SQL
passes and complete canonical product qualification are recorded above.

`ApiContracts.csproj` compiles the actual options, initialization-observation
gate and process-transport contracts without an engine dependency. JIT and
normally trimmed Linux x64 NativeAOT each pass 34 assertions: default/equivalent
backend paths, mismatched overrides, missing descriptors, command-count false
positives, missing/failed Bootstrap or SingleUser operations, and exact recorded
arguments/status/failures, and ordered preservation of simultaneous backend and
diagnostic errors. Real file/directory collisions exercise record and spool-copy
failures during exception unwinding, plus exact rejection of a missing Snowball
descriptor before owner acquisition. Diagnostic JSON uses explicit writes without
reflection serialization. Logs are `api-contracts-{build,run,aot-publish,aot-run}.log`.
The descriptor and observation fixtures supply no database or SQL behavior;
all 36 consumer assertions remain required against the complete product.
The updated 34-assertion logs are under
`artifacts/product-owner-r3/snowball-options/`; the earlier 32-assertion logs
remain under `artifacts/product-owner-r2/`.

## Historical complete-backend retries

The failures below retain the first actual backend errors and their diagnostics.
The complete 928-unit module closure subsequently resolved both initialization
failures; they are not current SQL blockers.

`artifacts/product-owner-r3/` records the actual `194223-e06ea2a0` backend and
fresh frontend. The canonical consumer's private build passes with zero errors
and 14 generated/runtime warnings. Actual initdb invokes the original backend
for Version and resource checks, processes the original 973,545-byte BKI stream
successfully, then exits 1 during original SingleUser post-bootstrap SQL.
`bootstrap-005.stderr` identifies the first failure: `locale -a` is unsupported
while executing `SELECT pg_import_system_collations('pg_catalog');`. All original
invocation records and BKI/SQL/stdout/stderr spools remain in the private work
directory named in `owner-report.json`. No SQL input is skipped or replaced.

The resource-only `supervisor-runtime2008` relink passes the same actual API and
consumer graph build with zero errors and 14 warnings. Its fresh raw JIT retry
passes the locale-import step, selects `Europe/Vilnius` and logs the actual EEST
zone. Original bootstrap again succeeds. Post-bootstrap SingleUser then exits 1
when `CREATE FUNCTION dsnowball_init` loads the absent `$libdir/dict_snowball`.
`artifacts/product-owner-r3/raw2008/owner-report.json` records the exact command,
observations, stderr and private spools. At that checkpoint the required original
Snowball sources, static export catalog and per-owner registration were missing.
The later coherent928 product includes all 51 original Snowball sources and
registers both required catalogs without replacing bootstrap SQL or algorithms.

`EntryValidation.csproj` references the actual API and both translated projects.
Its private incremental build and JIT run pass 23 assertions over null/mismatched
inputs, missing-cluster Open, nonempty-directory Create, exact preservation of
caller data, real exclusive-file lease release and repeated rejected entries in
one process. It supplies no cluster or SQL substitute. These negative entries
are separate from the successful 36-assertion, three-lifetime SQL workloads and
complete canonical matrix recorded above.

## Current codec evidence

Private outputs are under `build/product-owner/` and `artifacts/product-owner/`.
`ProtocolTests.csproj` compiles the actual API codec directly without an engine
substitute or backend dependency. It reads the retained readonly native-control
wire transcripts at
`build/campaign/20261022-163944-c778dbf6/native-control/`. JIT and normally trimmed
Linux x64 NativeAOT both pass 1,833 assertions over all 21 transcripts, covering
exact UTF-8/NULL rows, command completion, SQLSTATE and transaction state; every
truncated prefix; malformed lengths, strings, row shape and error fields; invalid
UTF-8; trailing data; and a deliberately wrong expected row. The actual API
transport is also compiled into this control: short reads, EOF, lengths above
2^32, copied fragmented native responses, retained requests across GC, response
reuse, null/zero buffers, sticky callback failures and response limits pass.
Logs are
`protocol-{build,native-transcripts,aot-publish,aot-run}.log`. The forwarding-only
boundary also emits successfully with immutable supervisor compiler 1812, recorded
in `boundary-emission.log`. No managed SQL execution is claimed by these controls.

```bash
dotnet run --project pglite/tests/Product/ProtocolTests.csproj -c Release -- \
  /absolute/pglite/build/campaign/20261022-163944-c778dbf6/native-control
```
