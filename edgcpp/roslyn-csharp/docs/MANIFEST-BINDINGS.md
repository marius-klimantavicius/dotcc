# Production native binding integration

The production CLI now consumes explicit native function provider bindings for
its supported single-TU emission subset. This connects accepted callback emission
to the manifest path; it does not implement SQLite's providers or close T2.

Each required native function binding must match the exact captured linkage key,
inventory signature descriptor and captured calling convention, and provide the
existing ownership/error/thread/callback contracts. `libraryPath` names a pinned
provider file; `librarySha256` is its SHA256. Relative paths resolve against the
manifest directory. The translator verifies the absolute provider file/hash before
emission. `symbol` is the explicit host export, allowing reviewed ABI aliases
without changing captured identities. The emitter separately validates its
supported selected scalar/pointer representation. This is native boundary
admission, not C++ type/overload/ODR revalidation.

Required external data and recorded compiler intrinsics remain unsupported by
this binding path. Missing bindings, signature mismatch, incomplete contracts and
provider hash mismatch fail ROC1301 without publishing output. Generated projects
and their program/completion/import reports retain the provider dependency
contract. Library file/hash verification does not itself prove runtime ABI
behavior or prevent external mutation after translation; executable acceptance is
separate. Inventory's `declared-unverified` status is retained honestly.

[The immutable integration receipt](../acceptance/manifest-bindings/report.json)
records nine successful phases and four expected configuration rejections, with
no unexpected failure. Production `translate` emits the existing genuine callback
fixture, then the existing checker executes result751 and ordered events
21,11,21,31,21,11,41 in JIT and NativeAOT. Its dependency manifest contains only
Generated and Runtime. The native provider contains the previously reviewed
observation/callback forwarding functions, no application bodies. This specific
ABI is execution-exercised; other supplied providers are not automatically so.

The authoritative attempt is
`/mnt/storage/roslynobj-csharp-20261015/t2/manifest-bindings/final-reproduction`.
It uses a sparse isolated snapshot at `fe292742f` plus exactly Contracts.cs,
ProgramManifest.cs, Translation.cs and the integration driver, separating it from
concurrent unintegrated planner/varargs changes. Source/build/input/provider and
completion file hashes are recorded. The earlier current-tree attempt is retained
separately. No full SQLite inventory rerun was needed because dependency traversal
is unchanged.

Reproduce with an unused artifact directory:

```sh
python3 edgcpp/roslyn-csharp/tests/run-manifest-bindings.py \
  --object /mnt/storage/roslynobj-csharp-20261015/t2/export/callbacks.roslynobj \
  --artifacts /mnt/storage/roslynobj-csharp-20261015/t2/NEW-BINDING-ATTEMPT
```

The driver freezes/hashes implementation sources and uses the real public CLI;
no reader bypass or synthetic object is involved. Its explicit LD_LIBRARY_PATH
is limited to finding the existing checker's observation adapter. Generated
application imports use the pinned absolute path. No native SQLite is loaded.
