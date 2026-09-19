# SQLite regression scope for the Blink campaign

The owning managed consumer is a separate ordinary query/callback/GC/cleanup
gate. It regenerates its library and tests raw/optimized JIT and NativeAOT:

```sh
DOTCC_COMPILER="$PWD/DotCC/bin/Release/net10.0/dotcc.dll" \
  python3 blink/scripts/test-sqlite-regression.py
```

The C corpus wrapper selects exactly `core`, `api`, `vtable`, `upstream`, and
`fts5`. It runs each native oracle and compares raw/optimized JIT/NativeAOT
transcripts, giving five native suites and 20 managed suite/mode results:

```sh
python3 blink/scripts/test-sqlite-corpora.py --cache "$PWD/sqlite/ref"
```

The pinned source archives must already be available in that cache. Existing
fetch verification, native recipes and compiler-emission recipes remain unchanged.
This wrapper fixes `DOTCC_COMPILER` to the current repository Release compiler
and never rebuilds it or the postprocessor. All tool identities and tracked
SQLite inputs must remain unchanged throughout the run. Schedule the two wrappers
serially: they replace SQLite generated output, and the existing C emission
recipe chooses shared `sqlite/artifacts/tmp` despite the wrapper's own temporary
directory. Native and managed consumer builds are expected; generic tool builds
are not part of these commands.

Two whole suites are excluded, with source hashes and reasons in each receipt:

- `allocation`: 128 custom injected allocation failures.
- `vfs`: custom forced read/write/truncate/sync/delete/open failures.

There is no option in this wrapper to re-enable those suites. Their previous
receipts remain historical evidence, not newly executed or passed cases.
Ordinary SQL syntax, constraint, supported/unsupported API and authentication
outcomes are distinct from injected implementation failures and are not removed
merely because an operation returns an error. The `upstream` suite adapts named
cases in pinned `test/jsonb01.test`, including its existing error case; the
receipt records the archive, test-source and adapter identities.

Each receipt preserves selected/excluded suites, expected and actual ordered
suite/mode pairs, source/transcript hashes, separate raw snapshots, tool hashes,
executed commands, stdout/stderr hashes and any failures. Success requires the
entire exact selected matrix, current runner identity and unchanged inputs.
The five-suite wrapper passed all five fresh native transcripts and all 20
managed suite/mode results at
`blink/artifacts/sqlite-corpora/attempt-f3xzr2_h/receipt.json`, SHA256
`07f2f661c3a087d396e3ab4334670a6bc3c15899a64e7c2145de64f670d1e9c4`.
The selected/excluded inventory, exact expected/actual matrix, raw snapshots,
runner, compiler/postprocessor and tracked SQLite input checks passed. No shared
tool rebuild or SQLite authored source edit occurred. The earlier seven-suite
receipt remains historical and is not relabeled as this scoped execution.
