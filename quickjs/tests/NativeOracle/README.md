# Native QuickJS controls

`run.py` takes an already acquired upstream directory and an artifact directory:

```bash
python3 quickjs/tests/NativeOracle/run.py \
  --source quickjs/ref/quickjs-2026-06-04 \
  --output quickjs/artifacts/native-baseline
```

The recipe owns acquisition. Version, source units, and guarded adaptations come
from campaign metadata. The runner builds stock upstream and the embedded profile
with switch dispatch and upstream Atomics enabled, linking only the five core
units plus test code.
It preserves stdout/stderr, compiler identity, commands, source hashes, case counts,
and failure statuses in `native-report.json`. Each process has a 180-second hard
timeout. `TZ=UTC`; compiler assertions remain enabled. Both native controls use
`-funsigned-char` to match DotCC's documented plain-char model; the ABI probe
checks promotion of `(char)0xff` to 255.

The embedding runs the exact JSON/module/callback/Promise workflow twice with
fresh runtimes. It supplies only test print/console and `std.gc` (real `JS_RunGC`),
loads bundled source modules, drains jobs with a bound, and rejects unhandled
Promise rejections. `--fail` must return 1 with the deliberate assertion message.
The ABI probe uses the actual upstream `quickjs.h` types and real `JS_Call`
aggregate callback returns for integers, signed zero, NaN, short/heap BigInts,
and strings containing NUL. Pointer payloads are omitted from comparison logs;
the native probe checks their exact identity internally.

Bundled test assertions stay intact. Reporting wraps only the frozen top-level
calls in staged test copies. `test_finalization_registry` is explicitly excluded
because it requires the out-of-scope OS timer loop. The cyclic import fixture
succeeds under this pinned release despite its negative-test metadata; the test
runs its own `assert(f(1), 4)`.

The shared Atomics fixtures exercise all eight integer/BigInt typed-array views,
load/store, arithmetic/bitwise read-modify-write, successful and unsuccessful
compare-exchange, wrapping, coercion, validation, lock-free widths and pause.
The default runtime must reject blocking waits. Explicit test-only
`JS_SetCanBlock` enables bounded Int32/BigInt64 mismatch and timeout checks.
Two independent pthread runtimes then share test-owned backing storage: each
increments the same atomic counter, one really waits, and the other notifies it.
Both views must finalize before the harness frees backing storage. Stock and
switch-dispatch fixture transcripts must match exactly.

Native results establish controls only. They do not qualify translated execution,
managed allocator lifetime accounting, managed callbacks, or safe managed stack
limits. The runner replaces only its own `embedded-source` staging directory when
reusing an output directory.
