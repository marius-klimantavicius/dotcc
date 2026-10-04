# Variadic arguments through shared runtime spans

C/C++ `...` is emitted as a final
`params System.ReadOnlySpan<Edg.Runtime.VaArg>` parameter. Calls keep their source
function names and ordinary promoted scalar arguments; no `__edg_varcall`
per-call helpers are generated. `VaArg`, `VaPack` and `VaList` are authored shared
sources in [backend/runtime](../backend/runtime), copied into generated projects
as needed. Their namespace is `Edg.Runtime`; they are never generated inside the
translated owner and contain no project-specific type references.

## Calling convention

For example, a translated `int log_message(int level, ...)` has this final
parameter shape:

```csharp
int log_message(int level, params ReadOnlySpan<VaArg> __edg_varargs)
```

Direct calls can use `log_message(2, 17, 2.5, (void*)message)`. EDG applies C/C++
default argument promotions before emission. `VaArg` has implicit conversions
from `int`, `uint`, `long`, `ulong`, `double` and `void*`. Other values use
`VaArg.From<T>(value, expectedSize, alignment)` with the EDG layout, including
records, wide integers and extended floating-point representations. Enums keep
the required physical representation. Each conversion snapshots its value at
that argument's evaluation point, before later arguments can mutate its source.

Managed function pointers take an ordinary `ReadOnlySpan<VaArg>` final parameter:
their calls supply an explicit collection expression, such as `[17, 2.5]` or
`[]`. They cannot carry the C# `params` modifier. Instance-mode callbacks retain
their typed owner argument. This is an internal managed calling convention,
not a native platform varargs ABI or a P/Invoke signature.

## Callee storage and explicit va_list

Each variadic callee builds one argument pack, then exposes its unmanaged cursor
to the existing lowered `va_start`, `va_arg`, `va_copy` and `va_end` operations:

```csharp
int capacity = VaPack.GetStackBytes(__edg_varargs);
byte* workspace = stackalloc byte[capacity];
using var pack = new VaPack(__edg_varargs, workspace, capacity);
VaList argumentList = pack.Value;
VaList* arguments = &argumentList;
```

`VaList` remains a 24-byte unmanaged structure containing `Next`, `End` and
`Begin`. An explicit source `va_list` parameter uses this existing cursor ABI,
normally `VaList*` after array parameter adjustment; it does **not** become a
span. This preserves forwarding that advances the original cursor and
`va_copy` that creates an independent cursor. Forwarded lists and cursor copies
borrow the owning callee's activation and must not outlive it. `va_end` clears
only its cursor, not the pack. Copy `pack.Value` into a local before taking its
address, and do not copy an owning `VaPack`.

The allocation requirement includes alignment padding. At most **64 KiB** uses
caller-supplied stable stack storage; larger packs use `NativeMemory.Alloc` and
are freed on normal return or exception through `using`. `GetStackBytes`
returns zero for that native fallback. The payload limit is **64 MiB**, with
power-of-two alignments up to 4096 bytes. Layout and bounds are checked rather
than relying on CLR aggregate alignment. A workspace pointer must remain stable
for the whole pack lifetime; do not pass a pointer into an unpinned managed array.

Values up to **16 bytes** are held inline in `VaArg`. Larger values own an
ordinary **unpinned byte-array snapshot**, then copy into the callee's pack.
This costs one managed allocation and an extra copy per large argument, but
preserves by-value evaluation when a later argument mutates the source or
throws. No pointer into that array escapes the copy, and abandoning argument
evaluation cannot leak native memory. Small scalar calls need no managed
allocation or boxing; this does not promise that every possible span-producing
caller is allocation-free.

## Provider migration

Regenerate translated code and update external partial implementations and
managed callback signatures together. For a source `...` function, replace the
old final `VaList*` transport parameter with the generated `params
ReadOnlySpan<VaArg>` contract. A provider can use the prologue above when it needs
to call an existing cursor-based service. Providers implementing explicit
`va_list` APIs keep their cursor signatures. Known libc adapters already perform
the span-to-pack bridge. Shared versus project-specific placement still follows
[partial external contract rules](partial-externals.md).

## Verification and limits

- [Runtime tests](../backend/tests/varargs_span_runtime_verify.py) passed strict
  build, JIT and Linux x64 NativeAOT: 25 checks covering snapshots after mutation
  and GC, exact bits, alignment, bounds, native storage, cursor copies and
  exceptions. The scalar test measured zero managed bytes over 10,000 calls.
- [Translation tests](../backend/tests/varargs_verify.py) passed C and C++ native
  GCC/Clang/EDG-C oracles and static/instance JIT and NativeAOT runs, including
  direct/indirect calls, large aggregates, 128-bit and binary80 values,
  forwarding, empty calls and unwinding.
- SQLite core matched native output under JIT and NativeAOT; the translated
  SQLite library consumer also passed both. Full EDG source emission succeeded,
  which does not establish a runnable translated EDG compiler.

Evidence is retained under `artifacts/campaign/persistent-continuation/` in
`span-varargs-runtime-002`, `varargs-span-tests-001`,
`varargs-span-partials-002`, `varargs-span-sqlite-core-001`,
`sqlite-varargs-span-review-20261004-001` and
`edg-self-varargs-span-20261004-001`. These runs establish the tested Linux x64
profile, not execution on every OS/architecture or complete C++/libc coverage.
The runtime uses BCL memory APIs and no dynamic code generation.
