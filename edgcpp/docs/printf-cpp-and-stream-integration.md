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
