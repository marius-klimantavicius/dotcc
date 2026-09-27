# Validation harnesses

| Directory | Responsibility |
| --- | --- |
| `NativeOracle/` | Real native embedding; stock/switch dispatch comparisons and deliberate failure control |
| `Abi/` | Native actual-header layouts, aggregate callback returns and byte/string probes |
| `Behavior/` | Managed ABI, core features, errors, limits, ownership, callback lifetimes and runtime independence |
| `Upstream/` | Frozen selected entrypoint manifest, including explicit OS timer exclusion |
| `fixtures/` | Fixed JS feature assertions, consumed by both native and translated harnesses |
| `Reproduction/` | Disposable edited-source, offline policy, full-core emission and transactional publication checks |

Native controls run through `tests/NativeOracle/run.py` and preserve all commands,
source hashes, compiler identity and stdout/stderr. Native qualification uses
unsigned plain char to match the translation ABI. See
[the native runner](NativeOracle/README.md) and
[upstream inventory](../docs/upstream-tests.md).

Managed suites use the owning facade and actual generated types:

```bash
python3 quickjs/scripts/test.py --suite abi --suite behavior --suite lifecycle \
  --suite upstream --form all --mode all --rid linux-x64 --fetch never
```

The entrypoint delegates to the shared campaign framework. Its suite adapter uses
shared product checks, source acquisition, managed builds/publishing, restore
policy and bounded process handling. Each matrix executable must first prove that
the deliberately failing assertion exits with 1 and the expected message. ABI
AOT publication roots the whole translated library; the other harness publication
uses normal trimming. Intermediate files are separated by product/mode/rooting
under `build/managed-harness/`; `--restore none` requires that cell's assets from
a previous successful restore. Shared receipts and case reports reside under
`artifacts/campaign/<run-id>/`.

The friend assembly registers test-only `print`, collection and identity thunks
through actual `JS_NewCFunction2`. `std.gc` directly invokes real `JS_RunGC`,
without violating the owning API's prohibition on general callback reentry.
Original upstream assertion bodies remain intact in staged harness inputs.

Authored managed harnesses are not qualification evidence until built and executed
against translated products. See [validation status](../docs/validation.md).

Atomics coverage is shared with the native control:

| Fixture/case | Required observations |
| --- | --- |
| `atomics.js` (5 cases) | Six integer and two BigInt typed-array views; all RMW operations, successful/failed CAS, signed/unsigned wrapping, coercion exactly once, invalid inputs, lock-free widths, pause, and default nonblocking wait rejection |
| `atomics-wait.js` (1 case) | Explicit test-only blocking opt-in; Int32/BigInt64 not-equal, zero and bounded timeouts, no stale waiter, engine recovery |
| `atomics-cross-runtime-int32` / `atomics-cross-runtime-bigint64` | Independent real runtimes share test-owned backing; 4,000 concurrent atomic increments, notify wakes exactly one waiter with result `ok`, no remaining waiter, two view finalizers, all runtime allocation accounts return to zero |

The ordinary owning facade exposes JavaScript Atomics and SharedArrayBuffer while
retaining upstream `can_block=false`. Only the friend test harness calls the raw
engine blocking setter and supplies externally owned shared backing; this adds no
public worker or shared-memory API. Backing remains alive until both runtimes and
their active calls are disposed. All waits are bounded and the suite process also
has a deadline.

The pinned bundled tests contain no Atomics test entrypoints. Their
`test_worker.js` exercises shared buffers through the excluded `os.Worker` host;
the pin's `test262.conf` declares Atomics and Atomics.pause, but explicitly skips
Atomics.waitAsync. The authored native-matched tests above cover the embedded
engine capability without claiming an OS worker host or a Test262 suite run.

Run the disposable reproduction probe with a passing translation receipt:

```bash
python3 quickjs/tests/Reproduction/run.py \
  --receipt quickjs/artifacts/campaign/<run-id>/receipt.json \
  --output quickjs/artifacts/reproduction-local
```
