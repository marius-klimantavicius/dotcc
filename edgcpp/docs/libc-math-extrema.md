# Selected musl extrema and positive difference

The source provider translates musl 1.2.6 `fmin`, `fmax`, and `fdim` at float,
double and long-double widths. All nine original files, their hashes and exact
adaptation patches are retained under `backend/runtime/musl/math_extrema`.
The six min/max files are unchanged: they select the numeric operand when only
one argument is NaN and preserve musl's explicit signed-zero policy.

The three positive-difference files retain the original arithmetic and add
`ERANGE` when finite operands produce an infinite subtraction result. This is
required by the portable header's advertised `MATH_ERRNO` profile; an infinite
input or a NaN does not change errno. Return values remain those of the original
source. Directed rounding and floating exception flags remain unfinished.

Synthetic verification compares original and adapted native sources with
translated C and C++ in static and two-owner configurations, under JIT and
Linux-x64 NativeAOT. Coverage includes 600 random bit-pattern pairs at each
width, a binary80 boundary matrix, NaN classification, exact finite bits,
signed zeros, overflow errno, subnormal difference, forced GC and owner/TLS
errno isolation. A warmed generated binary80 min/max loop allocates zero bytes.
Positive-difference arithmetic still uses the existing Float80 arithmetic path;
this component does not claim it is allocation-free.

The automatic provider also verifies all source hashes and invokes the new
functions without explicit component inputs. Evidence is indexed in
`evidence/milestone-math-extrema.json`. Cross-platform execution beyond
Linux-x64 and complete libm/fenv support remain completion gates.
