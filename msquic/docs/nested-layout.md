# Nested generated product

The final postprocessed project is
`msquic/generated/TranslatedMsQuic/TranslatedMsQuic.csproj`.
Raw output is retained at `msquic/generated/raw/TranslatedMsQuic`.
Both contain 30 manifest-owned C# source files and use
`--nest-types --runtime=c --class-name MsQuic --namespace Managed.Transport`.
No generated global using directives are needed by a consumer.

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
