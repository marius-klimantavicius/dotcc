# Managed embedded PostgreSQL API

The authored API and canonical consumer now compile against the complete
translated backend and original frontend. The first raw JIT run completes
original bootstrap but fails post-bootstrap initialization when
`pg_import_system_collations` requests the unsupported `locale -a` host service.
No required consumer SQL assertion or successful SQL lifetime has completed.
The supervisor retains the host repair, full matrix and acceptance through P6.

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
PL/pgSQL loading and synchronization. The staged prefix contains copied upstream
data, translated-entrypoint descriptors and the static translated PL/pgSQL
manifest. Those descriptors are paths for the original executable-discovery
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
Startup binds original stdio, registers the actual translated PL/pgSQL exports,
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
selects the canonical inventory location. NativeAOT
publication uses the same consumer with normal trimming and `-p:PublishAot=true`.
All raw/processed JIT/NativeAOT SQL rows and the root solution remain pending until
the supervisor supplies and qualifies complete products.

## Related-library delivery controls

Current continuation evidence is under `artifacts/product-owner-r2/`.
`framework-companions.log` passes 13 isolated framework tests: transactional
publication of both backend and frontend forms, final-companion rollback of all
four prior trees and the current receipt, preflight rejection without mutation,
main-only authored hooks, semantic-copy isolation, and warn/strict/off companion
provenance behavior, including missing companion hash evidence. The runner
fixtures supply no SQL responses.

`product-delivery-tests.log` passes four checks against the actual PGlite recipe
and configuration hooks. Raw/processed host copies remain separate, original
authored bytes survive simulated semantic rewrites, original source links are
restored, and all four consumer command rows select matching backend/frontend
projects and the backend form's bootstrap prefix. Recipe/form state is restored
after a failed build.

`msbuild-graph-evaluation.json` records six successful real MSBuild evaluations
from `/tmp`, covering the default, explicit raw and private-frontend selections
for both API and consumer. It checks original compile links, the canonical case
file's build/publish copy settings, and all four required solution projects.
These evaluations do not compile the API or establish that the complete static
project graph builds. Actual SQL and the complete product matrix remain open.

`ApiContracts.csproj` compiles the actual options, initialization-observation
gate and process-transport contracts without an engine dependency. JIT and
normally trimmed Linux x64 NativeAOT each pass 32 assertions: default/equivalent
backend paths, mismatched overrides, missing descriptors, command-count false
positives, missing/failed Bootstrap or SingleUser operations, and exact recorded
arguments/status/failures, and ordered preservation of simultaneous backend and
diagnostic errors. Real file/directory collisions exercise record and spool-copy
failures during exception unwinding. Diagnostic JSON uses explicit writes without
reflection serialization. Logs are `api-contracts-{build,run,aot-publish,aot-run}.log`.
The descriptor and observation fixtures supply no database or SQL behavior;
all 36 consumer assertions remain required against the complete product.

## First complete-backend execution

`artifacts/product-owner-r3/` records the actual `194223-e06ea2a0` backend and
fresh frontend. The canonical consumer's private build passes with zero errors
and 14 generated/runtime warnings. Actual initdb invokes the original backend
for Version and resource checks, processes the original 973,545-byte BKI stream
successfully, then exits 1 during original SingleUser post-bootstrap SQL.
`bootstrap-005.stderr` identifies the first failure: `locale -a` is unsupported
while executing `SELECT pg_import_system_collations('pg_catalog');`. All original
invocation records and BKI/SQL/stdout/stderr spools remain in the private work
directory named in `owner-report.json`. No SQL input is skipped or replaced.

`EntryValidation.csproj` references the actual API and both translated projects.
Its private incremental build and JIT run pass 23 assertions over null/mismatched
inputs, missing-cluster Open, nonempty-directory Create, exact preservation of
caller data, real exclusive-file lease release and repeated rejected entries in
one process. It supplies no cluster or SQL substitute. These negative entries
do not establish clean shutdown/reopen of a successful SQL owner. The required
36-assertion, three-lifetime workload and all four product rows remain open.

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
