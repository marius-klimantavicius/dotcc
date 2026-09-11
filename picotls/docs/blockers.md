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

## B2: pthread header/runtime dependency — fixed

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

The generic `pthread.h` and BCL process-private implementation landed in
`76362a9`. It supplies real threads, mutexes, conditions, once and TLS destructors;
unsupported scheduling/process-sharing/cancellation facilities remain explicit
limits in `docs/C-SUPPORT.md`. Fourteen direct tests, translated/native fixtures
and Linux x64 NativeAOT execution passed. Inclusion now preprocesses cleanly.
The separate generic missing-include error-reporting concern remains a review
item; this repaired dependency no longer relies on a missing include.

## B3: aligned allocation declaration/runtime — fixed

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

BCL-backed `posix_memalign` landed in `66f81aa`, with LP64 alignment/size
arguments, invalid/overflow errors, unchanged output and errno on failure,
portable `free`/`realloc` ownership and debug heap integration. Selected unit,
translated/native fixture and Linux x64 NativeAOT tests pass. Host glibc changes
errno on impossible-size ENOMEM, so that native oracle excludes only this errno
comparison; runtime unit tests still require errno preservation.

## B4: format attribute on a function-pointer member — fixed

After B1's repair, all three units stop at `picotls.h:845`, at
`__attribute__((format(printf, 4, 5)))` following the variadic callback declarator
inside `ptls_log_event_t`. Dotcc reports unexpected `ID`, expecting `;`.
The declaration remains present when logging is disabled. The reduced input is
[`callback-format-attribute.c`](../tests/compiler-blockers/callback-format-attribute.c).
Its native compile/run and dotcc preprocessing pass; dotcc object emission
reproduces the same parse error with exit 2. The compiler probe records all four
commands and results.
A subsequent generic grammar repair accepts the GNU diagnostic `format`
annotation on function-pointer members, leading function declarations/definitions,
and trailing function prototypes. Both `__attribute__`/`__attribute` and
`format`/`__format__` spellings work. Unknown GNU attributes remain errors;
ABI-affecting annotations are never silently removed. Format-string diagnostics
are not implemented by this repair. The syntax and diagnostic-only contract are
based on [GCC's attribute documentation](https://gcc.gnu.org/onlinedocs/gcc/Common-Attributes.html).
Twelve attribute unit tests pass. The unchanged three core units now get past
line 845 and stop at `picotls.h:1583`'s GNU thread-storage spelling.

## B5: GNU `__thread` storage spelling — fixed

The unchanged header declares `extern PTLS_THREADLOCAL ...` at line 1583, and
its non-Windows macro expands to `__thread`. This is the GNU spelling for
thread storage, not an OS service. Dotcc already supports C11 `_Thread_local`
with zero-initialized file-scope storage. The reduced case is
[`gnu-thread-local.c`](../tests/compiler-blockers/gnu-thread-local.c).
The lexer now maps the GNU spelling onto the existing thread-storage token,
retaining the same zero-initialization, file-scope and `[ThreadStatic]` contract.
Nine thread-local unit tests and the `gnu-thread-local` functional fixture pass;
independent workers keep separate state and the main thread remains unaffected.
All three unchanged core units advance to the extern volatile callback below.

## B6: extern qualified function-pointer declarations — fixed

At `picotls.h:1959` the core declares
`extern void (*volatile ptls_clear_memory)(void *, size_t);` followed by the
similarly qualified `ptls_mem_equal` callback. Definitions of qualified callback
variables already have a generic declarator path; the extern declaration does
not. Preserve its declaration-only storage and volatile read/write semantics.

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

The generic extern path now uses the same qualified declarator as definitions,
registers declaration-only storage, and supports callback arrays too. The runnable
multi-unit regression exposed a second issue: volatile callback reads took a bare
address of a C# static field. Volatile pointer globals now use the existing `nint`
backing storage, keeping `Volatile.Read`/`Write` semantics and stable storage.
Twelve extern-callback/volatile unit tests and a multi-unit functional/native
fixture pass. The actual core advances to B7.

## B7: inline specifier before qualified primitive return — fixed

`PTLS_LOG_DEFINE_GETSNI` at `picotls.h:2012` expands a
`static inline const char *` function. The parser accepts `inline` before a
plain typedef but cannot combine its specifier list with a following qualified
primitive type. All three units stop at the `char` token. The fix must retain
both the inline function marker and const qualification of the returned bytes.

Keyword runs now retain interspersed `const`/`volatile` qualifiers before
resolving their base type, so both function markers and qualified pointees remain
intact. Const-write rejection and volatile accesses are tested. The unchanged
header now emits successfully into both `hpke.c` and `pembase64.c` objects; their
remaining const-discard warnings expose old libc prototype mismatches.

## B8: inline enum definitions in declarations — fixed

After B7, core parsing reached the nested tagged enum member at
`picotls.c:187`; a later local anonymous enum at line 4428 uses the same missing
type form. The generic type grammar now handles tagged and anonymous inline enum
definitions, reusing enum registration and existing scope/type rules. Unit and
native/translated functional tests cover explicit/implicit values and member/local
storage. Main-core parsing advances to B9.

## B9: const pointers on later declarators — fixed

At `picotls.c:672`, `const uint8_t *p = bignum, *const end = p + size;`
requires a const qualifier on the second declarator's pointer. Tail declarators
currently count only stars, losing the ability to attach pointer qualifiers.
The tail binder now rebuilds pointer types level by level and the grammar
accepts `*const` tails, retaining const on the correct level. Pointer/pointee
write rejection and mutable outer pointers around inner const pointers are
covered. Twenty-four selected tests and the native/translated fixture pass.
The actual core advances to B11.

## B10: bitfield tail-byte reuse — fixed

Actual-header metadata and an emitted/native runnable reducer disagree:
`ptls_aead_algorithm_t.align_bits` is offset 81 natively and 84 in dotcc, although
both sizes are 104. A one-bit unsigned field followed by a byte exposes the
full-word backing storage's unused tail. The reduced
[`bitfield-tail-padding.c`](../tests/compiler-blockers/bitfield-tail-padding.c)
reports offset/address 9 natively and 12 in emitted code. Shared layout and
aggregate emission must agree while bit setters preserve adjacent ordinary bytes.
`probe-translated-abi.sh --metadata-only` retains the failing evidence.

## B11: static local designated aggregate initialization — fixed

`PTLS_LOG_CONN` at `picotls.c:988` expands a function-local
`static struct st_ptls_log_point_t logpoint = { .name = ... };` even with logging
off. Global and ordinary local designated aggregates parse, but the static-local
statement form only accepts positional initializers. The shared repair must hoist
once-initialized storage with a unique local-static name and preserve zero-filled
unspecified members.

B10 landed in `3ffc7bd`: generic layout trims the bitfield run to its occupied
bytes before an ordinary member, while overlapped backing units use explicit
field offsets and byte-limited setters. All 64 actual-header metadata tuples
match the 92-entry native oracle; native/emitted/AOT reducers preserve neighboring
bytes under positive, negative and overflowing bitfield writes. This is not yet
full product runtime ABI execution.

B11 now reuses the aggregate initializer lowering for the static-local designated
form, with one program-lifetime backing field per declaration. The unit test and
native/translated fixture validate repeated calls, independent same-named statics
and zero-filled unspecified fields. Main-core parsing advanced to B12.

## B12: braced initialization across multiple declarators — fixed

At `picotls.c:4430`, `ptls_iovec_t pubkey = {0}, ecdh_secret = {0};`
uses two aggregate-initialized declarators. Single-declaration aggregate
initializers exist; the comma-list parser/binder needs to preserve type-aware
initialization for each list item. No upstream-source split is used as a workaround.

The comma-list grammar now retains positional/designated (and C23 empty)
initializer shapes until each declarator's own type is known. Local, global and
static-local aggregate lists share the existing nested/partial zero-fill binder;
qualified pointer heads and typedef pointers keep the right per-declarator type.
Validation: 21 targeted units, six functional fixtures and native gcc parity for
`multi-brace-declaration/` pass; logs are in `artifacts/multi-brace/`. The unchanged
core advances to B13; hpke and pembase64 continue to emit successfully.

## B13: braced designated member initializer — open

At `picotls.c:4440`, assignment uses
`(struct st_ptls_client_hello_t){.unknown_extensions = {{UINT16_MAX}}}`.
The designated-member production currently only accepts an expression after `=`,
so the nested array/struct braces fail during parsing. The repair must bind those
braces against the actual member type, preserving zero-fill for omitted elements.
