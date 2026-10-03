# Selected formatter language and stream integration

The driver currently chooses one source language for a semantic merge. The
selected musl formatter can participate in either C11 or C++17 with a small
source adaptation; adding per-source language support is unnecessary for
these two units.

## Retained C backend comparison

The unchanged selected sources parse in C11. Original EDG with its C backend
reports eight errors in `vfprintf.c` and two in the buffer adapter/header when
parsing the same units as C++17. These are ordinary language differences, not
EDG defects:

- explicit conversions are needed from void pointers;
- a 16-character string plus terminator cannot initialize `char[16]` in C++;
- the bounded scratch assertion must spell `static_assert` in C++;
- the infinity/NaN string pointer must be const;
- private public entry declarations need C linkage for consistent symbol names.

The compatibility patch keeps the exact 16-byte hexadecimal table by listing
its characters explicitly, adds the conversions and a C/C++ assertion macro,
and gives the two public private-runtime entry points `extern "C"` declarations.
The existing source's mutable string cursor is retained; its string-pointer
cast is read-only on the relevant formatting paths. No parser, rounding or
digit-generation algorithm changes.

All initial errors, the patch, source snapshots, generated C/IL and native
executables are retained under
[`artifacts/campaign/musl-printf-edg-language-20261002-231437/`](../artifacts/campaign/musl-printf-edg-language-20261002-231437/).
`compatible-results.json` records all four successful C/C++ frontend runs;
`numerical-results.json` records **109 numerical cases passing for each
language after lowering through original EDG's C backend**. The small native
classification bridge is preserved beside the tests. These are synthetic
formatter tests, not real-project acceptance or C# formatter evidence.

The compatibility patch has now been applied to the canonical selected files
after isolation of the broad regression snapshot. Provenance hashes and the
upstream adaptation patch were regenerated; replay and 109 numerical checks
pass under both GCC and Clang in
`artifacts/campaign/musl-printf-cpp-promoted-20261002-233035/`.

## FILE formatting session

Locking each `Fwrite` call is insufficient: a single format may produce many
literal, padding, integer and floating chunks. Other writers could interleave
between them. Hold the FILE's existing `Entry.Gate` monitor across the whole
format operation.

The preferred allocation-free managed bridge is `LibcFiles.WithFileLock`,
which invokes the translated formatter callback itself under a C# `lock`.
C# lowers that lock to Monitor.Enter with try/finally release. The exact private
C import contract is preserved in `backend/tests/libc-format-include/format_file.h`:

```c
typedef int (*__edg_format_body)(void *context, const char *format,
                                 va_list arguments);
int __edg_with_file_lock(FILE *, __edg_format_body, void *context,
                         const char *format, va_list arguments);
```

Use the adjusted `va_list` parameter, not `va_list *`. In the current EDG
array-of-tag profile, adjustment produces the builtin tag pointer mapped to
`Edg.Runtime.VaList*`. A pointer to the va_list array would instead introduce
a separate generated array aggregate. The dedicated import must validate the
FILE identity, callback signature and builtin tag identity/layout before
emitting the managed call, rather than accepting arbitrary pointer-sized values.

The runtime overloads in the new `LibcFilesFormat.cs` are:

```csharp
int WithFileLock(void* file,
    delegate* managed<void*, sbyte*, VaList*, int> formatter,
    void* context, sbyte* format, VaList* arguments);
int WithFileLock<TOwner>(void* file,
    delegate* managed<TOwner, void*, sbyte*, VaList*, int> formatter,
    TOwner owner, void* context, sbyte* format, VaList* arguments)
    where TOwner : class;
```

The generated static wrapper selects the first overload; instance mode selects
the second and passes its explicit owner. The method resolves the FILE in that
owner's dictionary, validates writable/update-direction state, and invokes the
callback while holding its existing monitor. Neither a managed closure,
GCHandle nor per-call session object is required. The existing LibcFiles declaration
has now been made partial. Driver/runtime-copy and validated internal-import
integration are now active.

The source callback receives an unmanaged sink in its context and calls
`__edg_musl_vformat`. Its sink writer forwards to `Fwrite(bytes,1,count,FILE)`;
short counts mark the current sink operation as failed. Reentrant monitor
entry allows existing byte buffering/error behavior to be reused for each
chunk while preserving the whole-operation lock. The snprintf sink directly
calls the core and needs no FILE lock.

Do not place unmatched lock/unlock calls around the C core: a managed callback
exception could bypass C cleanup. Do not store a mutable static callback owner.
The sink's current-operation error flag starts clear and observes short writes;
a preexisting sticky FILE error remains sticky but does not falsely mark a new
successful formatting call as failed. Preserve the core's first error/errno;
lock release must not replace it.

The existing LibcFiles global dictionary lock is released before waiting for a
stream monitor; Dispose and fflush-all snapshot entries first. Keep that order
when adding sessions. Generated owner leases must continue excluding owner
shutdown from active format calls. Nested formatting on the same stream and
thread is safe through reentrant monitor behavior, though callers still bear
normal cross-stream lock-order responsibility.

Required synthetic integration tests are: two writers formatting many chunks
through one FILE without record interleaving; reentrant same-stream output;
short-write/error propagation with prior sticky errors; C++ and managed
callback exceptions proving monitor release; independent owners with identical
FILE argument shapes; static/instance callback addresses; and NativeAOT.
An injected blocking stream can coordinate a competing writer deterministically
so the test verifies whole-operation locking, not merely a lucky schedule.

The isolated helper prototype now passes JIT and NativeAOT on Linux x64:
[`artifacts/campaign/libc-file-format-lock-20261002-231958/results.json`](../artifacts/campaign/libc-file-format-lock-20261002-231958/results.json).
At the time of that run, only its copied LibcFiles declaration was made partial
inside the artifact project; the canonical declaration has since been promoted. Tests deterministically observe a rival writer blocked between the
formatter's two chunks, then verify `ABX` ordering, reentrant calls, owner
validation, foreign exception cleanup and zero managed allocation per session.
The repeatable verifier is `backend/tests/libc_files_format_verify.py`.
This initially proved the managed helper. The later 25-case translated
formatter acceptance verifies the real C import/callback integration too.

## Public wrappers

`backend/runtime/musl/printf/format_wrappers.c` now supplies printf, fprintf,
sprintf, snprintf and their four va_list variants. The FILE path constructs
the private sink and calls the validated lock import; the buffer path calls
the selected core directly. All eight wrappers pass fixed synthetic buffer
and FILE expectations after original EDG C11 and C++17 lowering. Evidence is
`artifacts/campaign/musl-printf-public-wrappers-20261002-233355/results.json`.
Its native bridge is only a test adapter; managed provider integration and
translated JIT/NativeAOT formatting subsequently pass in
`artifacts/campaign/musl-printf-translated-20261002-235819/results.json`.


## Wide formatter integration

The `wprintf` component contributes `vfwprintf.c`, `wformat_wrappers.c`, and
unchanged musl `wcsnlen.c`. It implements wprintf/fwprintf/swprintf and their
three va_list forms, plus wcsnlen. Original musl 1.2.6 files, release hash,
zero-fuzz replay patches, COPYRIGHT and authored-adapter hashes accompany it.
The C++ adaptations preserve the parser/state tables, nine positional arguments,
width/precision handling and `%n`; they replace native FILE access with an
opaque wide sink. The positional prepass still occurs before FILE orientation.

Numeric conversion uses the already accepted narrow musl formatter. ASCII
numeric chunks widen through a fixed 64-wchar stack buffer; literal spans are
batched into the wide sink. There are no temporary pinned arrays or numeric
heap strings. The memory sink validates encoding with the selected wcrtomb
algorithm and stores exact scalars directly, including a terminated valid prefix
if a later scalar cannot be encoded. Its locale profile is C/POSIX/C.UTF-8;
this is not support for arbitrary stateful encodings or directed fenv rounding.

A whole-format managed monitor encloses FILE output. Its typed callback and
VaList ABI are validated before emission; ordinary routine-address adapters
preserve owner capture. The helper temporarily clears an existing FILE error,
tracks new failures, and restores the prior error even across CLR exceptions.
Captured stream encoding remains in force. Byte/wide mixing follows the existing
explicit rejection contract. Managed runtime probes cover old/new errors,
exceptional release, another thread reusing the stream, and zero steady-state
allocation after initialization.

The pinned wide core has a `%p` delegation defect: it constructs a j-prefixed
pointer conversion that the pinned narrow parser rejects. Feeding the resulting
negative count into later count arithmetic triggers signed overflow. The bounded
source patch delegates `%p` without that unsupported prefix, passes an actual
pointer, and returns any negative delegated result before count arithmetic.
The original file is unchanged in originals/. Pointer records use an independent
accepted narrow-%p oracle; every other record is compared against the original
wide parser. This is a musl adaptation, not an EDG change.

Final evidence is retained under `artifacts/campaign/`:

* `libc-wide-printf-translated-20261003-043000/results.json`: native plus all eight
  generated modes, 174 complete result/errno/%n/output/guard records per mode,
  all six APIs, FILE output, stream projection and routine addresses.
* The same directory's `asan-results.json`: ASan/UBSan for all records and the
  six-API FILE fixture. The removed positional/nonpositional mixed `%n` fixture
  was undefined; its original failure is preserved in the earlier 040000 run.
* `libc-wide-printf-runtime-20261003-035100/results.json`: JIT/NativeAOT lock/error
  and allocation checks; `libc-wide-printf-abi-20261003-040000/results.json`:
  eight rejected malformed private declarations.
* `libc-wide-printf-provenance-20261003-043100/results.json`: both original hashes,
  exact patch replay and authored adapter hashes.

These results are Linux execution evidence. Multi-platform execution, arbitrary
locale data and full floating-environment behavior remain separate gates.
