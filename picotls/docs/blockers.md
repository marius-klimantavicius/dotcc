# P1 compiler boundary findings

The unchanged pinned core was attempted on Linux x64 with a freshly built Release
dotcc, C17, `PTLS_HAVE_LOG=0` and `PICOTLS_USE_DTRACE=0`. No compiler, upstream C,
or generated C# was repaired during P0/P1. A subsequently authorized P2 repair
fixes chained token pasting in dotcc; upstream sources remain unchanged.

From `picotls/`, run `python3 scripts/probe-compiler.py --build-dotcc`.
The script intentionally exits **1** while any boundary is blocked, captures all
commands and exit codes in `artifacts/compiler/results.json`, and retains each
unit's preprocessing and object-emission diagnostics. It runs the reduced cases
with the native compiler too. Do not interpret dotcc's exit zero alone as success.

| Input | Preprocessing | Separate object emission |
| --- | --- | --- |
| `lib/hpke.c` | Pass, no residual `##` | Exit 2 at `picotls.h:845` |
| `lib/picotls.c` | Missing `pthread.h`; no residual `##` | Exit 2 at `picotls.h:845` |
| `lib/pembase64.c` | Pass, no residual `##` | Exit 2 at `picotls.h:845` |

## B1: chained token pasting in callback typedefs — fixed

`PTLS_CALLBACK_TYPE0(uint64_t, get_time)` expands `st_ptls_##name##_t` and
`ptls_##name##_t`. Dotcc previously left an unexpanded `##` in its token stream, then reported
`unexpected '##'` at the callback invocation. This is shared by all three core
files in the original P1 results.

The small standalone reproducer is
[`chained-paste.c`](../tests/compiler-blockers/chained-paste.c). Its native build
and execution succeed with exit 0, and the native preprocessor forms
`st_clock_t` and `clock_t`. Dotcc object emission originally failed with exit 2. The reducer
retains a callback self-pointer and invocation so the expected semantics are
explicit.

The fix in `DotCC.Lib/MacroExpander.cs` consumes every paste in a chain before
macro rescan, using raw parameter operands and retaining empty/multi-token
boundaries. Eleven unit regressions failed before the fix and pass afterward.
The runnable `macro-chained-paste` fixture covers the self-pointer callback,
final-name rescan and an empty middle operand; its output also matches native C.
The reduced picotls case now preprocesses and emits an object successfully.
All three unchanged real units get past line 758, exposing B4 below.

## B2: an unconditional include is missing, but not fatal

`lib/picotls.c` unconditionally includes `<pthread.h>` in its non-Windows branch,
even when `PTLS_HAVE_LOG=0` removes its mutex uses. Dotcc has no supplied header
for it and prints `#include 'pthread.h' not resolvable`, while `-E` returns 0 and
continues. The standalone [`pthread-include.c`](../tests/compiler-blockers/pthread-include.c)
also returns 0 from both preprocessing and object emission with that diagnostic;
the native compiler accepts it and its program returns 0.

This is both a source-closure gap and an error-reporting gap. A future fix should
be generic and should distinguish unused declarations from actual required
pthread functionality. P1 does not add an empty campaign header or impersonate
Windows to hide the diagnostic. The probe treats diagnostic output as blocked.

## B3: aligned allocation has no supplied declaration or runtime

The active `ptls_buffer_reserve_aligned` implementation calls `posix_memalign`
at `lib/picotls.c:613`. Native symbol inventory finds that dependency, and dotcc's
`stdlib.h` and runtime contain no matching implementation. The standalone
[`posix-memalign.c`](../tests/compiler-blockers/posix-memalign.c) allocates 1,024
bytes at 64-byte alignment, verifies the alignment and frees the result; native
compilation and execution both return 0. Dotcc preprocesses and emits an object
without a diagnostic, but `--emit=build` fails with C# error `CS0103`: the name
`posix_memalign` does not exist in the current context. The probe retains this
build command/output too. A generic libc implementation and declaration belong
in P2, with alignment, invalid arguments, allocation failure and `free` ownership
tests. An unselected aligned backend does not remove this upstream API body.

## B4: format attribute on a function-pointer member — open

After B1's repair, all three units stop at `picotls.h:845`, at
`__attribute__((format(printf, 4, 5)))` following the variadic callback declarator
inside `ptls_log_event_t`. Dotcc reports unexpected `ID`, expecting `;`.
The declaration remains present when logging is disabled. The reduced input is
[`callback-format-attribute.c`](../tests/compiler-blockers/callback-format-attribute.c).
Its native compile/run and dotcc preprocessing pass; dotcc object emission
reproduces the same parse error with exit 2. The compiler probe records all four
commands and results.
No attribute/parser repair is included in the chained-paste change.

## Remaining uncertainty

These are the first observed blockers, not an exhaustive compiler defect list.
Parsing has not reached the core implementation, and linking, C# compilation,
translated layout metadata, and translated callback behavior remain unverified.
The native-versus-C# layout probes use explicitly hand-authored boundary mirrors;
they establish a feasible layout contract, not proof of dotcc emission.

The native dependency inventory also exposes a review item: several dotcc libc
declarations accept `int` byte counts where picotls passes `size_t`. This is not
yet a reduced real-core emission failure. P2/P3 must assess conversions and bound
sizes rather than silently allowing truncation.

## Initial linking choice

Use dotcc's **source linking** for the initial product: pass `lib/hpke.c`,
`lib/picotls.c`, and `lib/pembase64.c` as separate input paths to one managed-library
invocation. The frontend creates a separate preprocessor/parser for each input
and calls `IrBuilder.AddUnit` per filename. This retains translation-unit
boundaries and lets the existing linker handle external symbols, static names
and header inline definitions. Do not concatenate the C files or reuse a single
preprocessed header state.

The target invocation, once P2 repairs the recorded blockers, is:

```sh
dotnet ../DotCC/bin/Release/net10.0/dotcc.dll -std=c17 \
  -DPTLS_HAVE_LOG=0 -DPICOTLS_USE_DTRACE=0 -I "$source_root/include" \
  "$source_root/lib/hpke.c" "$source_root/lib/picotls.c" \
  "$source_root/lib/pembase64.c" --emit=managedlib \
  --class-name Picotls --namespace Managed.Security \
  --split=size --split-size=102400 -o generated/TranslatedPicotls
```

Here `source_root=$(scripts/fetch.sh)`. Per-unit `--emit=obj` is used now to expose
the first compiler boundary independently; it is not a claim that fragments link.
Object caching can follow once actual-source static/inline semantics pass. No
native crypto import flags are selected.
