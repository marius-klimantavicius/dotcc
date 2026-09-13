# Nested generated product

The final postprocessed project is
`msquic/generated/TranslatedMsQuic/TranslatedMsQuic.csproj`.
Raw output is retained at `msquic/generated/raw/TranslatedMsQuic`.
Both use
`--nest-types --runtime=c --class-name MsQuic --namespace Managed.Transport`.
No generated global using directives are needed by a consumer.

The product link also uses `--deduplicate-inline`. Unambiguous shared helpers
receive their original names automatically. Selected helpers from
`config/inline-exports.txt` are exported under their original names using
`--export-inline`: initially `CxPlatEwma` and address get/set/compare/wildcard helpers. Edit that file to select
additional inline APIs. Non-equivalent definitions produce a compiler diagnostic;
functions with distinct state or C address identities retain separate bodies.
`QuicAddrSetToLoopback` now shares one exported body: the compiler recognizes
equivalent reads of the immutable `in6addr_loopback` initializer across units.
Managed calls such as `MsQuic.QuicAddrGetPort(&address)` need no function-pointer
field. The chosen selectors and deduplication setting are recorded in the product
receipt and frozen closure.

Status constants are exposed directly on `MsQuic`; full macro expansion and
typed constant detection now discover `QUIC_STATUS_*` automatically. Regeneration
retains `--emit-define 'QUIC_STATUS_*'` to require that these API values remain
exportable; the selector is recorded in the host, object and product receipts.
The native/C# comparison without selectors, covering all 37 values through both
direct emission and object linking, is in
`artifacts/automatic-macro-constants/results.json`. The earlier explicit-selection
comparison remains in `artifacts/macro-exports/results.json`.

Generated ABI types, globals and runtime helpers are nested in
`Managed.Transport.MsQuic`. For example, a low-level consumer can name
`MsQuic.MSQUIC_HOST_TABLE` or import nested types with
`using static Managed.Transport.MsQuic;`. The owning API remains in
`Managed.Transport.Api`; its default project reference points to the final
postprocessed directory.

See [inline regeneration results](inline-options.md) for the measured source
reduction and validation of the selected managed methods.

## Mutable promoted members and partial structs

Promoted aggregate members now return a managed reference annotated with
`UnscopedRef`, so this consumer code updates the original settings storage:

```csharp
using static Managed.Transport.MsQuic;

QUIC_SETTINGS settings = default;
settings.IsSet.PeerUnidiStreamCount = 1;
settings.PeerUnidiStreamCount = 10;
```

Scalar and bitfield projections keep their value properties. Const, volatile,
and atomic aggregates retain their existing projection behavior. The reference
adds no storage and preserves the native layout; references into managed owners
remain valid across compacting garbage collection.

All translated structs, unions, and generated aggregate helpers are `partial`.
The enclosing `MsQuic` class is also partial. When copying generated source into
a project, another declaration in the same namespace and containing class can
override `Equals`, `GetHashCode`, or `ToString`. Both declarations must compile
into the same assembly; see [the compiler guide](../../docs/cli.md#extending-copied-translated-structs).

Compiler commit `8372ce2` passed 2,195 unit tests and 459 functional tests
(1,005 optional cases skipped), including separate-assembly consumers, custom
partial overrides, nested member mutation, copying, and compacting GC. A fresh
MsQuic regeneration passed all 89 native/JIT/NativeAOT ABI observations and the
raw/postprocessed JIT/NativeAOT product checks, including the settings assignment
above. All 32 public-consumer cases passed; the targeted optimized JIT platform
check also passed. Evidence is retained under `artifacts/promoted-refs/`.

SQLite was regenerated and postprocessed with the same compiler: 24,231 `Cond.B`
calls rewritten, 2,208 empty blocks removed, and 41 files updated. Its managed
consumer passed under JIT and NativeAOT, covering SQL and WAL workloads, JSONB,
FTS5, callbacks, preupdate hooks, encodings, integrity, GC, and cleanup. These
targeted checks do not relabel the historical full transport or SQLite campaigns.

## Stable event payload names

`config/dotcc-overrides.json` selects the anonymous types of nine
`QUIC_CONNECTION_EVENT` fields through `fieldTypeNames`. For example, `CONNECTED`
uses `QUIC_CONNECTION_EVENT_CONNECTED_DATA`, which can be named directly:

```csharp
QUIC_CONNECTION_EVENT connectionEvent = default;
ref QUIC_CONNECTION_EVENT_CONNECTED_DATA data = ref connectionEvent.CONNECTED;
data.SessionResumed = 1;
```

The other configured fields use the same `QUIC_CONNECTION_EVENT_<FIELD>_DATA`
pattern. The profile also names `QUIC_LISTENER_EVENT.NEW_CONNECTION` as
`QUIC_LISTENER_EVENT_NEW_CONNECTION_DATA` and `QUIC_LISTENER_EVENT.STOP_COMPLETE` as
`QUIC_LISTENER_EVENT_STOP_COMPLETE_DATA`.
Seven `QUIC_STREAM_EVENT` payloads (`PEER_RECEIVE_ABORTED`, `PEER_SEND_ABORTED`,
`RECEIVE`, `SEND_COMPLETE`, `SEND_SHUTDOWN_COMPLETE`, `SHUTDOWN_COMPLETE`, and
`START_COMPLETE`) use the `QUIC_STREAM_EVENT_<FIELD>_DATA` pattern.
These are partial structs with the original ABI layout. The compiler
profile is applied during source/object emission; the host, ABI, cached-object,
and product receipts record its hash. Build and closure checks reject objects
from a different profile. Regenerate after changing the mapping.

The initial eleven-name profile with compiler commit `b879ef8` passed 2,216 unit tests and 463 functional tests
(1,005 optional cases skipped). All eleven then-configured types appeared exactly once
in both raw and postprocessed output and remain exposed through ref properties.
The regenerated product passed all 89 ABI observations and the raw/postprocessed
JIT/NativeAOT consumers, which explicitly name all eleven types. All 32 separate
public-consumer cases also passed. Receipts and source hashes are retained under
`artifacts/field-type-names/`.

SQLite was regenerated and postprocessed with this compiler (24,231 `Cond.B`
rewrites, 2,208 empty blocks removed, 41 files updated). Its managed consumer
passed under JIT and NativeAOT, including SQL/WAL, JSONB, FTS5, callbacks,
preupdate hooks, encodings, integrity, GC, and cleanup. These checks retain their
targeted scope and do not relabel historical full campaigns.

The expanded eighteen-name profile in commit `b150670` adds `_DATA` to the two
listener payload names and names seven stream payloads. Regeneration with the
same compiler passed all 89 ABI observations, raw/postprocessed JIT/NativeAOT
product consumers, and all 32 public transport-consumer cases. Each configured
type appears exactly once in both output variants and is exposed by ref; the
old listener type declarations are gone. Evidence is retained under
`artifacts/field-type-names/stream-events/`.

## Regeneration

```sh
msquic/scripts/translate.sh
# Reuse already-built tools instead:
msquic/scripts/translate.sh --no-build-tools
```

The script archives the preceding closure, verifies the pinned upstream source,
refreshes all 47 product objects and native/JIT/NativeAOT ABI controls, links the
raw nested library, copies its manifest-owned files to the final directory, runs
the postprocessor in place, and builds both variants and their rooted NativeAOT
consumers. All build and validation scripts resolve optimized output to the final
directory; raw output remains a separate comparison input.

`freeze-product.py --without-sqlite` records these ABI/build gates with exact
compiler, staged-source, object, generated-source and output-layout identities.
It explicitly leaves shared SQLite campaign flags false. Historical SQLite and
full transport receipts are not relabeled as executions of a new compiler/layout.
The existing `--sqlite-receipt` mode still requires its complete fresh regression
evidence before crediting those gates.

## Original nesting validation

These checks describe the initial nested-output regeneration, before the later
macro-export compiler update. The current regeneration refreshes the ABI and
rooted product-build gates; the additional runtime campaigns below remain
evidence for the original nested build.

- All 60 host/core and 29 public ABI observations match native under JIT and
  NativeAOT.
- Raw and postprocessed libraries pass rooted JIT/NativeAOT consumer checks,
  including direct use of the nested `MsQuic.MSQUIC_HOST_TABLE` type.
- Postprocessing rewrites 18,761 `Cond.B` calls, removes 3,147 standalone empty
  blocks and updates 29 source files.
- All 32 separate public-consumer cases pass across both variants, JIT/NativeAOT
  and IPv4/IPv6, including streaming, FIN, resumption and authentication failures.
- Each variant passes 162 packet-crypto checks under JIT and NativeAOT.
- Platform, UDP, TLS adapter and owning stream/lifetime controls pass under JIT
  for both variants. Targeted receipts do not claim a full runtime matrix.
- The owning API builds with its default project references with zero warnings
  and errors.

Those receipts are indexed in `artifacts/nested-layout/results.json`.
The preceding closure is preserved at
`artifacts/closure-7439fb1112858639/`; its former optimized directory, including
build outputs, is retained at `artifacts/nested-layout/legacy-optimized/`.

The first public-consumer attempt retained a server exit by SIGINT during an
authentication-negative case. Its sample removed the cancellation handler before
printing its terminal error, allowing the parent cancellation request to terminate
the process in that interval. The executable now keeps the handler until process
exit and tolerates cancellation after its token source has been disposed. Owner
drain, actual TLS rejection and zero application-data assertions are unchanged.
The original receipt and logs remain at
`artifacts/public-consumer/nested-before-signal-lifetime-results.json` and its
recorded run directory; the complete corrected run passes all 32 cases.
