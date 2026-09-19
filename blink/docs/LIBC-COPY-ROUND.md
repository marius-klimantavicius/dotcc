# Generic string copies and default rounding

The first actual core C# link exposed missing `stpcpy`, `strdup`, `strndup`,
`rint`, `rintf`, and `isunordered` definitions. They were absent from both the
supplied public headers and the embedded runtime. The implementations now live
in generic `DotCC.Libc/StringCopyLib.cs` and `MathRoundLib.cs`; they do not replace
Blink instruction algorithms or invoke private host services.

`stpcpy` copies the terminator and returns its destination address. `strdup`
and `strndup` allocate independent native storage compatible with existing
`free`, including the checked debug heap. `strndup` reads at most its `size_t`
bound and allocates the actual prefix plus one terminator, so a huge bound with
a short source remains valid. The allocation does not narrow to `int`.
Terminator-size overflow or allocation failure returns NULL and sets ENOMEM;
success preserves errno. These are raw C pointer APIs: readable source storage
and writable destination storage are caller obligations. They do not provide a
new quota policy for Blink's generic malloc allocations.

`rint`/`rintf` implement the runtime's default nearest rounding with ties to even,
including negative zero, infinity and NaN classification. They do **not** model
a mutable floating-point environment or floating-point exception flags. This is
a restricted runtime contract, not full `fenv` conformance. Native `rint` changes
with `fesetround`; the separate native rounding-contract audit records that
behavior rather than treating all modes as equivalent. NaN payload preservation
and signaling-NaN exception behavior are not promised. `isunordered` returns
nonzero when either evaluated operand is NaN, without an ordering comparison.

Pinned upstream `cvt.c` uses `rint` for its nearest MXCSR branch and separate
`floor`/`ceil`/`trunc` branches for the other modes. Some upstream float conversion
handlers call `rintf` directly. The implementation preserves that upstream code;
qualifying every guest conversion and rounding-mode combination remains an
instruction-conformance task. No `fesetround`, `fegetround` or `fesetenv` use was
found in the pinned upstream Blink C/header sources.

The `libc-copy-round` native C fixture checks terminator cursors, buffer canaries,
independent copies, un-terminated bounded input, huge bounds on short strings,
free/errno behavior, C function pointers, rounding bit patterns and single
operand evaluation. Unit tests additionally cover checked-heap allocations and
unrepresentable/impossible allocation requests that fail before copying.
`blink/tests/LibcPrimitives/run.py` translates a copied fixture and adds an
explicit authored GC hook; it preserves raw generated C# and postprocesses only
a copy. It compares raw/optimized JIT/NativeAOT output with native output, with
CS8500 forbidden. Separate functional consumers cover direct/object linking and
flat/nested type output.

Selective recovery from `partial_blink.patch` was reviewed against HEAD
`3f12b2e`, including its fixed-address global storage changes. These tests use
public C functions and owned native buffers, without depending on generated
global-container field names. The older `attempt-xicjix3s` receipt is historical. Fresh recovered-source
qualification passed in
`blink/artifacts/libc-primitives/attempt-amjdro8m/receipt.json`: native and
raw/optimized JIT/NativeAOT agree under forced GC, with CS8500 forbidden.
The rebuilt focused suites passed 104 unit tests and five functional tests; two
external-platform oracle rows were unavailable and skipped. The local native
fixture was compiled and executed independently. The recovery receipt under
`blink/artifacts/libc-primitives/recovery-3f12b2e/` records the selected patch and
confirms all three original user patch files remain unchanged.
Full repository regression is coordinated separately.

The read-only missing-definition inventory is
`blink/artifacts/core-missing-definitions/inventory.json`. It distinguishes
portable upstream source files, missing generic libc, host capability bindings,
JIT branch references, measured constants and unresolved private host contracts.
Its original diagnostic input is a mixed-producer diagnostic replay, not a
canonical full-core build or successful managed execution claim.

Fresh isolated full repository regression now passes: 2257 unit tests and556
functional tests, with1057 explicit platform skips, zero failures and a successful
Release build (17 existing analyzer warnings). The log is
`artifacts/recovery-isolated-repository.log`. Full translated-core execution
remains a separate gate.
