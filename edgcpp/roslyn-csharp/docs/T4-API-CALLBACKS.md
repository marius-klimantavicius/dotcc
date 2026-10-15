The bounded API/callback slice passes native, generated JIT and NativeAOT. See
[the harness](../tests/SqliteApiCallbacks/README.md) and
[the frozen receipt](../acceptance/sqlite/api-callbacks-report.json).
All five measured phases pass; generated observations match the exact native
object, all tracked resources release, and no callback error is recorded.
Generated process maps contain the generated ABI adapter and system/runtime
libraries, with no native SQLite application library.

The finite observations cover blob destructor lifetimes across reset, rebind,
clear and finalize (release counts 0/1/2/3, owner IDs 101/102/103); four preupdate
events with defined old/new values and unregister behavior; update/delete/insert
changesets with undefined values distinct from SQL NULL; successful apply then
OMIT conflicts 1/2/3; and two incremental backup steps ending with exact rows
and integrity. NativeAOT explicitly selects `/usr/lib/llvm-18/bin/clang` and
publishes in 71.92 seconds, sampled process-tree peak 3.339 GB. Short execution
RSS samples can miss startup; waited kernel RSS is retained separately.

Public inventory and actual emitted signatures validate 51 APIs and four callback
shapes. Surface validation is distinct from executed behavior. The first build
rejected working-tree versus frozen-validator path identity and is preserved;
the accepted fresh snapshot uses metadata from its own exact validator.

RBU staged updates, snapshot/carray/normalize and column-metadata APIs,
shared-cache/unlock-notify, tokenizer interfaces, controlled allocation/I/O
failures, wider ownership/error/provider contracts and the rest of the enabled
feature matrix remain open. This closes a bounded slice, not the whole T4 gate.
