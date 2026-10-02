# Printf core selection and adaptation audit

Status: selected-library prototype with synthetic inputs, 2026-10-02.
Prefer the pinned musl formatting core over extending dotcc's current builder.
The small sink adaptation has native numerical evidence; C# translation,
portable-va_list integration and full floating/locale contracts remain gates.

The reviewed core is now preserved in
[`backend/runtime/musl/printf`](../backend/runtime/musl/printf/), with its
unchanged upstream file, exact adaptation patch, license and hash manifest.
The private buffer adapter is also present. Neither source is yet wired into
the generated C# provider.

## Evidence and patch cost

The inspected source is musl **1.2.6**, `src/stdio/vfprintf.c`, SHA-256
`642d9649d96702ca689f9b0cb47e78e8585a04e2de4478d987e5718691909b2e`.
Release provenance and license requirements are in
[musl-port-audit.md](musl-port-audit.md).

All prototype files, patches, source notices, commands' logs and native
executables are retained in
[`artifacts/campaign/musl-printf-sink-20261002-221948`](../artifacts/campaign/musl-printf-sink-20261002-221948/).
This work compiles a selected libc implementation against synthetic tests;
it does not translate or accept a whole musl project.

| Part | Observed cost |
| --- | --- |
| Original source | 703 lines |
| Parser/integer/floating core prefix | 663 lines, with only the first include replaced for the sink prototype |
| Native FILE wrapper replacement | Patch deletes 41 lines and adds 15, including the changed include |
| Prototype sink bridge | 20 lines; callback, context and an independent error flag |
| Fixed scratch adaptation | Two original lines replaced by five lines, retaining the original logical extent |
| Final vendored adaptation | 55 deleted / 35 added lines; also namespaces the sink type, helpers, entry and fixed positional limit |

The native wrapper contains all direct `f->flags`, buffer, locking and FILE
write-function accesses. The preceding algorithm core uses `ferror` and
`__fwritex` as its output interface. Replacing those two operations with a
private sink leaves flags, field widths, precision, positional argument scans,
integer conversion, `%n` stores and floating digit generation in upstream code.

The unbounded/VLA prototype and the bounded-scratch prototype both pass the
native synthetic corpus. The bounded version passes under GCC and Clang:
**109 comparisons/assertions per compiler**. This is native adaptation evidence,
not a claim that the new C# VaList ABI or translated formatter already passes.
The final vendored files and their new snprintf buffer sink independently pass
the same 109 cases under each compiler, with source hashes and patch replay
verified. Evidence is retained in
[`artifacts/campaign/musl-printf-vendored-20261002-223606`](../artifacts/campaign/musl-printf-vendored-20261002-223606/).
The repeatable verifier is `backend/tests/musl_printf_native_verify.py`; it
requires a new `--output-dir` and accepts explicit `--gcc`/`--clang` paths.

Two deliberately retained failures refined the oracle:

- Musl and glibc choose different valid hexadecimal representations for some
  subnormals. Full-precision `%a` is therefore checked by exact numeric round
  trip there, rather than asserting glibc's byte spelling. Explicit precision
  needs musl-specific golden vectors where exponent normalization differs.
- Host glibc's fortified `wctomb` interface assumes its larger `MB_LEN_MAX`,
  while musl's formatter has a four-byte temporary. The native bridge build
  disables host fortification for this isolated compatibility experiment and
  restricts the wide test to UTF-8. This is not a production workaround; the
  translated provider must use its own consistent four-byte UTF-8 ABI.

## Why dotcc's builder is not a complete reuse path

`DotCC.Libc/VarargLib.cs` stores integer/pointer bits in eight bytes and a
separate double value; its VaList is a ref struct over that representation.
`VsnprintfLib.cs` explicitly rejects long-double and wide conversions.
`PrintfBuilder` also routes substantial formatting through managed strings.
It remains a useful source of .NET techniques and tests, but adapting it would
require new numerical/argument semantics rather than a narrow ABI bridge.

The EDG runtime's new `VaList` instead holds an unmanaged 24-byte cursor over
an aligned borrowed pack. That representation can retain binary80, Int128 and
POD values. Musl's `va_arg` operations can consume the appropriate promoted
types through backend lowering without converting to the legacy dotcc slots.
Standard printf has no Int128 length modifier in this LP64 profile: carrying
Int128 in general varargs does not make `%j` accept it when intmax_t is 64-bit.

## Exact dependencies of the selected core

| Dependency | Required implementation/contract |
| --- | --- |
| `va_copy`, `va_arg`, `va_end` | New aligned pack ABI; preserve argument promotions and independent cursors across the positional prepass. |
| Sink write/error | Count attempted output independently of truncation; propagate write failure; hold a FILE lock for the entire formatting operation. |
| `memset`, `strnlen` | Existing audited bindings; preserve size_t widths. |
| `isdigit` | Unsigned-character-safe ASCII digit classification used by the parser. |
| `errno`, `strerror` | Owner/thread errno and provider-owned stable message storage, including GNU `%m`; report EINVAL/EOVERFLOW. |
| `signbit`, `isfinite` | Exact double/long-double classification, including signed zero and NaNs. |
| `frexpl`, `scalbn` | Exact exponent decomposition/scaling with binary80 support and the selected floating-environment policy. |
| `wctomb` | Owner/thread locale contract, 32-bit wchar_t and bounded multibyte output; report EILSEQ without splitting a character at a precision limit. |
| Target headers | Consistent float.h, limits.h, stdint.h, inttypes.h, wchar.h and stdarg.h; no host Linux layout leakage. |

The inspected core uses musl's `NL_ARGMAX=9` and `MB_LEN_MAX=4` profile.
The adaptation makes the positional limit private (`EDG_NL_ARGMAX=9`) so
a host header cannot silently enlarge the arrays or change that contract.
Numeric locale/grouping expectations need an explicit provider contract;
silently substituting host CultureInfo formatting is not that contract.

`frexpl` can be implemented directly from Float80 sign/exponent/significand
bits, including normalization of subnormals. `Math.ScaleB` is a candidate for
double scaling in the default rounding mode, but does not by itself prove all
C fenv behavior. Native tests exercise non-nearest rounding; current software
Float80 operators still need equivalent rounding/environment integration before
those results can be claimed for translated C#.

## Bounded scratch storage

Musl's `fmt_fp` uses a VLA sized from the effective floating type. The backend
currently rejects general variable array bounds, so the isolated prototype
changes the physical array to a target-derived constant bound:

```c
enum {
    capacity = 1 + (LDBL_MANT_DIG-29+7)/8 + 1
             + (LDBL_MAX_EXP+LDBL_MANT_DIG+28+8)/9
};
_Static_assert(LDBL_MANT_DIG >= DBL_MANT_DIG &&
               LDBL_MAX_EXP >= DBL_MAX_EXP, "scratch bound");
uint32_t big[capacity];
```

The original logical `bufsize` is retained where upstream's `sizeof(VLA)`
previously selected its logical end. For binary80 this allocates **1,838
uint32_t slots / 7,352 bytes**, compared with the 128-slot logical double
scratch. This is a measured storage tradeoff, not a hardcoded magic size.

The current backend uses stack storage for frames within 65,536 bytes and
scoped NativeMemory with finally cleanup above that threshold. The bounded
binary80 scratch fits that existing budget. No pinned managed array is needed.
Once general VLA support or a typed scratch helper is available, restore
dynamic physical sizing to avoid carrying long-double scratch for ordinary
double formatting. Keep that optimization separate from digit correctness.

## Integration shape

Keep a private unmanaged format sink distinct from public opaque FILE. Its
writer callback, context pointer and current-operation error flag serve two
consumers:

1. A snprintf sink copies only available bytes, always accounts for the full
   formatted count, terminates when capacity is nonzero and never dereferences
   a null destination at zero capacity. Its algorithm is small and portable C.
2. A FILE sink forwards bytes to `LibcFiles` while one generated managed wrapper
   holds a formatting session. That wrapper must release the lock in finally,
   including managed allocation failures or foreign exceptions. Do not use
   unmatched C begin/end lock calls that can leak a lock on managed unwinding.

Use the existing explicit-owner callback ABI when the sink points to a
translated routine. A mutable static callback owner would break reentry and
independent program instances. The core's per-operation error flag avoids
confusing a preexisting sticky FILE error with a new failure; FILE errors remain
sticky after formatting.

The public wrapper and snprintf sink replace musl's internal FILE construction;
there is no need to implement musl's native FILE layout, futex locks or buffer
function table merely to obtain its formatter.

## Corpus before declaring the formatter complete

The preserved 109-case native corpus includes:

- zero/one/truncated capacities, embedded NUL, high bytes and `%%`;
- signs, alternate form, zero/left padding, width and precision, promoted
  narrow values and LP64 signed/unsigned boundaries;
- positional arguments with positional dynamic width/precision;
- every supported `%n` destination width;
- double and binary80 signed zero, subnormals, minimum/maximum exponents,
  decimal ties, infinities and NaNs;
- four native rounding modes, wide UTF-8 output, count overflow and a failing
  output sink.

Expand this to generated C# JIT/NativeAOT with the real pack ABI, owner reentry,
FILE error/locking behavior, errno/locale isolation, precision boundaries,
binary80 hexadecimal golden vectors and callback exceptions. Test Windows,
Linux and macOS execution separately. Preserve the upstream revision and patch
manifest, and record unsupported extensions rather than silently accepting
incorrect output.
