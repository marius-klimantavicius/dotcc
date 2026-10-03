# Assertion facade

The portable `assert.h` supplies a macro whose expansion observes `NDEBUG` on
every inclusion, evaluates an enabled expression once, and ignores a disabled
expression entirely. C++ uses contextual conversion to bool without invoking
an overloaded `operator||`. C11 `static_assert` aliases `_Static_assert`.
This header is an authored compiler facade, audited against pinned musl 1.2.6
`include/assert.h` and `src/exit/assert.c`; no upstream source was modified.

`__assert_fail` has a strictly validated `void(char*,char*,int,char*)` import.
The owner `LibcFiles` writes expression, file, line and function to the current
addressable `stderr`, flushes it, and invokes existing `LibcProcess.Abort`.
Internal diagnostic bytes may be written even when stderr is wide-oriented;
normal public byte/wide FILE mixing rules remain intact. Formatting uses stack
scratch and source bytes, with no per-diagnostic managed temporary allocation.
Failure to write a diagnostic still aborts. `Environment.FailFast` cannot be
caught by translated C++ or host C# and does not unwind caller finally blocks.
Signal handlers for SIGABRT remain an explicit separate provider requirement.

Evidence retained under `artifacts/campaign/`:

- `libc-assert-candidate-20261003-005900/native-proof/results.json`: native C/C++
  header tests, including repeated inclusion, disabled undefined expression,
  once-only side effects and contextual bool.
- `libc-assert-candidate-20261003-005900/runtime-proof-004/results.json`: six
  JIT/NativeAOT runtime checks, including redirected/wide stderr, line bounds,
  zero temporary allocations, fatal exception behavior and failing output.
- `libc-assert-translated-20261003-012300/results.json`: eight translated
  C/C++ static/instance JIT/NativeAOT configurations with fatal subprocesses.

Earlier failed harness/build attempts are retained alongside those results.
NativeAOT execution evidence is Linux x64; runtime uses BCL cross-platform APIs.
