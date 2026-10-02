# Math provider and GNU builtin routing

The additional provider slice implements `abs`, `labs`, `llabs`, binary80
`fabsl`, `sqrtl`, `truncl`, `floorl`, `ceill`, `roundl`, and all three
`copysign` widths. Existing binary32/binary64 sqrt, fabs, trunc, floor and ceil
bindings remain in the initial provider. This is bounded math coverage, not a
complete libm or floating environment.

## Implementation choices

`LibcMath.cs` uses BCL integer primitives and exact representation operations.
Its binary80 square root never narrows to binary64. After normalization, an
input with 64-bit significand S and unbiased exponent e produces the integer
radicand `S << (63 + (e & 1))`. Its square root fits a 64-bit significand before
rounding. Bounded Newton iteration in UInt128 obtains the floor root q, with
remainder r. Nearest rounding increments exactly when `r > q`: `(q+1/2)^2`
has fractional part 1/4, so no integer radicand ties. The result exponent is
`floor(e/2) + 16383`, followed by carry normalization. All nonzero canonical
finite binary80 inputs have normal square-root results. No BigInteger, scratch
allocation, unmanaged allocation or host binary80 operation is required.

Negative nonzero inputs produce NaN and set owner/thread errno to EDOM=33.
Successful calls leave errno unchanged. Signed zero, positive infinity and NaN
payloads are preserved; arithmetic quiets signaling NaNs. Absolute value and
copysign directly edit sign bits, preserving the remaining payload. Integral
rounding edits significand bits and implements the stated C rounding direction;
`roundl` breaks half ties away from zero. The unrepresentable result of integer
abs at the minimum value is undefined by C; unchecked generated arithmetic is
retained, rather than introducing a BCL OverflowException.

`LibcMath` is created lazily per generated owner and receives the existing
owner's errno setter. It owns no native resource, TLS slot or pinned storage.
It is safe to use the stateless methods without a LibcMath instance. All actual
resources and errno lifetime remain with LibcContext and generated lifecycle
leases.

The selected header advertises `math_errhandling=MATH_ERRNO` and the target
currently uses nearest-even arithmetic. This slice does **not** implement FE
status flags, directed rounding, trap modes or every noncanonical x87 encoding.
No public `fesetround`, `rint`, `nearbyint` or general `scalbn` binding is added.
In particular, these functions must not be advertised as IEC 60559 environment
completion merely because nearest numerical results agree.

## Why builtins need explicit routing

The original EDG C backend's `dump_intrinsic_call` only rewrites selected
special cases such as constant evaluation and launder; GNU math calls can
remain as compiler builtin names for a native C compiler. Our runtime does not
have that native compiler step. `dot_gen_imports.h` therefore recognizes exact
`bfk_*` identities from EDG's builtin metadata, after verifying the routine has
`sfk_none`. It explicitly maps bound sqrt/fabs/abs/copysign/trunc/floor/ceil
families and roundl to the corresponding libc binding. Arbitrary user names
starting `__builtin_` are not stripped or guessed.

The normal return/parameter ABI checks still run for aliases. Definitions in
translated input continue to win over provider imports. Alias and ordinary
function addresses use one canonical routine adapter per libc symbol, including
instance owner arguments and lifecycle leases. This matches runtime address comparison in original EDG-generated C.

EDG may fold a direct source expression such as `abs == __builtin_abs` to
false in C++ before either backend receives it. We retain that result. The
original C backend emits that same constant, while comparison through local
function-pointer variables remains a runtime comparison and yields true in
the native reference. The preserved probe is
`artifacts/campaign/libc-math-alias-fold-20261003-002640`; it contains the
original C, IL and executable. The generated acceptance test therefore uses
local pointers for runtime identity, without changing EDG or rewriting its
folded expressions. The initial overly broad source-comparison expectation
and failure remain in `libc-math-translated-20261003-002445`.

## Source audit

The comparison sources are retained in the workspace:

- `upstream/src/c_gen_be.c`, `builtin_kinds.h`, `il_def.h` and `folding.c`: native
  emission and exact builtin identity contracts.
- `../DotCC.Libc/MathLib.cs`: BCL float/double implementation precedent. Its
  long-double-to-double conversion is not sufficient for our binary80 target.
- Pinned musl 1.2.6 `src/math/sqrtl.c`, `fabsl.c`, `truncl.c`, `floorl.c`,
  `ceill.c`, `roundl.c` and `copysignl.c`, under the retained source acquisition
  in `artifacts/campaign/musl-feasibility-20261002-214747/musl-1.2.6`.
  Acquisition hashes/license are in [musl-port-audit.md](musl-port-audit.md).

These are original managed algorithms, not a copied musl sqrt table port.
Musl's fenv-sensitive final rounding and native exception-raising expressions
cannot be ported by calling Math.Sqrt(double). The bounded integer algorithm
provides a simpler exact nearest-mode binary80 implementation; a future full
floating-environment layer must handle its flags and directed rounding too.

## Evidence

`backend/tests/libc_math_verify.py --nativeaot --output-dir <new-directory>`
builds the synthetic native binary80 oracle, retains its vectors and commands,
and runs the same exact-bit corpus in managed JIT and Linux-x64 NativeAOT.
The first proof is
[`libc-math-20261003-001930/results.json`](../artifacts/campaign/libc-math-20261003-001930/results.json):
30,344 canonical inputs, seven results each, plus errno, NaN payload and
zero-allocation checks. This includes exponent boundaries, extrema, subnormals,
both signs, infinities, quiet/signaling NaNs and 30,000 deterministic random
encodings. Native and generated acceptance of C/C++ imports, aliases, address
identity and instance/static ownership is a separate verifier:
`backend/tests/libc_math_translated_verify.py`.
It passes the native oracle and all eight C/C++ × static/instance × JIT/NativeAOT
combinations, including split output:
[`libc-math-translated-20261003-002740/results.json`](../artifacts/campaign/libc-math-translated-20261003-002740/results.json).

Five explicit ABI/unavailable-symbol rejection probes pass in
[`libc-math-abi-20261003-002525/results.json`](../artifacts/campaign/libc-math-abi-20261003-002525/results.json):
wrong integer result, wrong binary80 argument, mixed copysign width, unknown
builtin-like name and still-unimplemented public rounding-mode function.
They retain diagnostics and verify rejection before C# output; the repeatable
entry point is `backend/tests/libc_math_abi_verify.py`.

Linux-x64 test results do not establish execution on Windows, macOS or ARM64.
The implementation uses portable BCL UInt128/bit primitives and no host ABI
imports, but those runtime targets still require acceptance runs.

## Next groups

1. Add exact decomposition/remainder/minimum/maximum operations with explicit
   signed-zero, NaN and errno contracts. Bind only after synthetic native
   vectors and generated C/C++ JIT/AOT acceptance.
2. Implement a coherent floating environment across emitted arithmetic,
   conversions and helpers before exposing mode mutation or flag APIs.
3. Port selected pinned musl transcendental algorithms and their constants,
   respecting binary80 range and precision; avoid substituting binary64 math
   for long double. Investigate numerical accuracy and error behavior per
   function family, with domain, overflow, underflow and difficult-rounding
   corpora. Remaining declarations continue to fail closed when called.
