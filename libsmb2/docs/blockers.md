# Failure ledger

The original full-source probe is preserved in [parse-probe.md](parse-probe.md).
All compiler failures below were reduced and regression-tested without rewriting
upstream SMB code or repairing generated C# by hand.

| Observed failure | Repair and evidence |
| --- | --- |
| Relative includes failed across physically included sources; Linux networking headers were missing. | Physical include identity and shared network/endian headers, `81b3769`; 44 focused tests and native/managed ABI fixture. |
| Anonymous standalone enums, GNU unused attributes and statement expressions blocked parsing/lowering. | Scoped structural lowering, `9cac4d1`; 34 focused tests and four functional checks. Unsupported statement-expression lvalue use explicitly rejected in `ae7321b`. Complete frontend retry: 53/53 clean. |
| Typedef-array global storage and initialized flexible tails blocked object generation. | Correct underlying array types and separately sized/aligned object storage, `8f1d8ab`; native-matched storage and separate-object tests. |
| Missing sockaddr_storage/linger, readiness, nonblocking and IPv6 behavior. | Shared BCL descriptor/socket implementation, `ccbfbda` and `8a9e923`; native/JIT/AOT transport fixtures, plus actual abortive-close reset test. |
| Raw linked library exposed missing DNS, vector I/O, entropy, host identity, errno, protocol lookup and asprintf. | Real shared host implementations and named Linux ABI declarations, `e7da7ea`, `3edabb2`, `e0fc5c1`, `c553125`; seven fixtures pass native/JIT/AOT. |
| Existing nested C type named System broke a newly embedded framework exception reference. | Globally qualified framework name, `5d63fec`; existing four failing functional cases now pass, followed by the complete functional suite. |
| Original facade returned from failed synchronous read with its context still active and queued requests potentially retaining the caller buffer. | Actual loopback TCP reset regression demonstrated failure after authenticated file open. The facade now submits upstream async requests and drains callbacks or destroys the context before its operation scope releases pinned/stack storage. Baseline evidence: `artifacts/lifecycle-baseline-red/`. |
| Original finalizer path destroyed the context but leaked idle file allocations. | The facade tracks local handle ownership, drains pending callbacks before releasing idle handles, and handles close's transfer of ownership separately. An isolated allocation-counter regression established the original leak. |

The final generation retry invoked `scripts/translate.sh` from `/tmp` and passed
all 53 source emissions, linking, raw compilation, semantic postprocessing and
processed compilation. Its receipt is `artifacts/translation/result.json`.

Remaining acceptance work includes the full adversarial protocol corpus,
multiple outstanding requests on one context, broader allocation-failure and
credit-pressure cases, performance characterization, and additional platforms.
Passing the implemented Linux suites does not close those plan gates.

Fault-injection work now follows the explicit upstream-only
[test scope](test-scope.md). Additional compound-metadata cleanup investigation
is paused and preserved under ignored build artifacts. No C correction was
integrated; the downloaded snapshot and translated product remain unchanged.
