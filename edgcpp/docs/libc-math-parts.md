# Selected floating remainder and decomposition

The selected musl provider includes `fmod`, `fmodf`, `fmodl`, `modf`, `modff`
and `modfl`. Their original musl 1.2.6 algorithms and private math header are
retained with exact hashes, license and patches in `runtime/musl/math_parts`.
The private configuration fixes the published little-endian binary80 target;
it does not select behavior from the compiler host's operating system.

The remainder adapters additionally set the owner/thread errno to EDOM for
non-NaN inputs with an infinite dividend or zero divisor. NaN operands retain
errno. This follows the current portable header's MATH_ERRNO contract and the
[POSIX remainder error ordering](https://man7.org/linux/man-pages/man3/fmod.3p.html).
Floating exception flags and dynamically selected rounding modes remain part
of the unfinished fenv work; this component does not claim to provide them.

Nine focused checks at `artifacts/campaign/libc-math-parts-translated-002`
compare original/adapted native results with C/C++ static/instance JIT and
NativeAOT: binary32/64/80 bit-pattern matrices, signed zero, subnormal and
extreme exponent cases, canonicalized NaN classification, decomposition output
pointers, domain errno and owner/TLS isolation under forced GC. The native
oracle renames only unused musl-private trig declarations to avoid glibc's
unrelated private symbol declarations; the tested algorithms are unchanged.

Eight automatic-packaging checks pass at `musl-provider-math-parts-001`, and
seven source/header patch replays at `libc-math-parts-patch-001` form one
component check. The selected provider contains 135 source units. Linux-x64
execution is verified; complete math/fenv coverage and other platforms remain
open. The binary80 runtime still has the separately documented BigInteger
performance costs.
