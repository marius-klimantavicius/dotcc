# WAT execution regression

After the shared Release compiler and functional-test assemblies are built and
frozen, with `wat2wasm` and Node on `PATH`:

```sh
python3 blink/scripts/test-wat-regression.py
```

This wrapper runs only `DotCC.FunctionalTests.WatOracleTests` using
`dotnet test --no-build` and `DOTCC_RUN_WAT=1`. It never builds shared tools,
installs dependencies, or runs the broad functional suite. The current source
contains 146 ordinary `InlineData` cases across return-value and stdout methods.
Those cases include normal allocation and IEEE floating NaN/infinity formatting;
there is no custom fault-injection or expected WebAssembly-trap selection.

The existing tests emit WAT from authored C, assemble/type-check it with WABT's
`wat2wasm`, and execute it through Node's WebAssembly engine. The stdout cases
use the existing minimal `fd_write` shim. Results are compared to authored
expected values/bytes, not to a fresh native C oracle. This is WAT execution
evidence, not managed JIT/AOT or complete WASI qualification.

The wrapper records the exact source metadata rows, source hash, per-method
counts, discovered test names and complete TRX results. The observed count is
derived from source/discovery, not silently fixed to a historical result. Each
TRX case must match the discovered multiset, have a distinct test ID, and pass;
missing cases, unexpected methods, skips and failed outcomes reject the gate.
A change to the theory metadata shape requires review.

The receipt binds the already-built functional-test/CLI binaries and matching
shared compiler dependencies, test source/project and local build policy,
resolved `dotnet`, `wat2wasm` and Node paths, executable hashes and reported tool
versions. It snapshots the wrapper and WAT test source. An isolated `TMPDIR`
contains transient test files; the unchanged tests delete their temporary
C/WAT/wasm files after each case. The wrapper does not claim retained per-case
module artifacts or an independent source-to-test-assembly build proof.

Attempts live under `blink/artifacts/wat-regression/`. Closed command logs and
TRX are hashed; log, source, tool and binary identities are checked again at the
end. Timeout/interruption terminates the owned command process group and
escalates after a grace period; no injected timeout tests are added. Missing
tools fail before running the oracle rather than satisfying the gate via skips.

## Qualification status

Source-only wrapper; execution is held for the serialized validation slot.
Earlier ad-hoc WAT results are historical, not qualification of this wrapper.
