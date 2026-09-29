# Compiler and direct-native feasibility

These are diagnostic gates, not a published libuv backend. Source pin remains
`1cfa32ff59c076ffb6ed735bbc8c18361558661f` (libuv v1.52.1).

## Executed gates

- Fixed-signature native calls: translated C invokes exact glibc `pipe`, `read`,
  `write`, `close`, and `__errno_location` through authored `LibraryImport` methods.
  The probe transfers binary bytes over actual native descriptors and checks
  invalid-close `EBADF` both through native TLS and captured P/Invoke error state.
- Native callback: `pthread_create` and `pthread_join` use an unmanaged Cdecl thunk
  that invokes the translated C worker. The test keeps its context in unmanaged
  storage, blocks the native callback while the host forces garbage collection,
  and releases storage only after a successful join. Exceptions cannot escape the
  reverse interop boundary. This proves one explicit adapter, not automatic
  translation of arbitrary libuv callback signatures or signal safety.
- The native boundary probe passes raw and processed source under JIT and
  NativeAOT on Linux glibc x64. Run
  `python3 libuv/tests/probes/run-native-boundary.py --aot --postprocess`.
  `libuv/artifacts/native-boundary/result.json` records commands and compiler DLL
  hashes. Per-symbol production errno preservation remains required: these checks
  do not authorize arbitrary intervening runtime transitions before reading errno.
- Actual `src/version.c`, `src/strtok.c`, and `src/strscpy.c` materialized from native
  preprocessing and AST dependency closure match the original native source in
  raw/processed JIT/NativeAOT. The native snapshot is also recompiled and compared.
  Run `python3 libuv/tests/probes/run-common-closure.py --aot --postprocess`.
  Two emitted domains of the **same ABI** coexist in one managed assembly, with
  distinct nested enum types. This is not Windows/Unix ABI coexistence evidence.

- The real `src/timer.c` closure builds and passes native differential behavior
  for invalid starts, inactive repeat, deadline ordering, equal-deadline insertion
  order, repeating timers, callback cancellation, active-handle counts, and next
  timeout. Native original and native closure agree. All **116** measured sizes,
  alignments, and direct-field offsets for loop/timer/handle/async/signal/I/O/queue/
  heap records and pthread mutex/rwlock storage agree under raw/processed JIT/AOT.
  Run `python3 libuv/tests/probes/run-timer-closure.py --aot --postprocess`;
  `libuv/artifacts/timer-closure/result.json` is the receipt. Loop storage is
  manually initialized for this timer-only gate; this does not validate the
  untranslated OS event loop. The initial fixed-buffer offset discrepancy was a
  harness temporary-address error; the corrected probe measures its first
  element's actual storage address.

## Native declaration materialization

`tests/probes/materialize-native-closure.py` invokes the actual native Clang
preprocessor and AST frontend. Roots are every function definition and global in
that product source; referenced declarations and native typedef/enum dependencies
are retained. Product-body ranges are copied from the preprocessed byte stream.
Unused SDK declarations are dependency-pruned, not replaced with guessed headers.
Physical preprocessor line markers identify root provenance independently of
Clang's omitted repeated JSON file fields. Receipts record exact source ranges,
origins, tool commands, and input/output hashes. GCC line-marker syntax inside
ranges is canonicalized to standard `#line` metadata; C body tokens are retained.
Unknown types, unresolved dependencies, nonrepresentable ranges and unsupported
ABI types fail explicitly. Primitive mode rejects named aggregates; `--records`
enables experimental aggregate dependency capture requiring native layout
comparison. Neither mode approves an ABI simply because emitted C# compiles.

The initial native-header survey tried all 35 Linux/glibc product units with actual
host headers and retained compiler attributes. It emitted no objects: native
headers exposed unsupported GNU declaration syntax and unrelated native types.
The subsequent closure approach separates unused SDK material from required
libuv storage while preserving original selected declarations.

## Shared compiler repairs

The first compiler checkpoint supports authored `malloc`/`free` function overrides
without losing allocator effects to malloc-to-stack promotion, including direct
and function-pointer calls and object linking. It also corrects `malloc(sizeof)`
argument coercion to the declared `size_t` ABI. Caller-supplied allocator families
still own their pairing contracts.

Native function declarations now accept chained trailing GNU attributes with
`extern`; optimizer promises `nothrow`, `leaf`, `const`, and `pure` remain
conservatively effectful. Layout and calling-convention annotations are not
silently discarded. The checkpoint passed 83 focused unit cases and five
functional cases, including actual generated execution for allocator accounting
and direct/indirect annotated calls. Logs are in
`libuv/artifacts/compiler-probes/`.

The timer source additionally required the native `__builtin_offsetof` spelling
and the ordinary `visibility("default")` declaration annotation. These structural
additions passed 37 focused attribute/offsetof unit cases and the real timer's
four execution modes above. Other visibility contracts remain explicit failures.


## Open work

Full event-loop translation, remaining aggregate ABI coverage, direct
native external-data/TLS binding, comprehensive callback adapters, foreign ABI
models and SDKs, and all-platform product assembly remain incomplete. Signal and
post-fork managed-execution boundaries have not been qualified. The existing
consumer deliberately fails until a usable complete translated product exists.
