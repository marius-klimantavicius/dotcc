# Selected musl power-of-two scaling

The scalbn/scalbln/ldexp families now use nine musl1.2.6 source units at float,
double and binary80 widths. Public scalbn wrappers set ERANGE for finite
nonzero inputs that overflow or round to zero. A nonzero subnormal result sets
ERANGE only if reverse scaling proves discarded bits; exact subnormal results
preserve errno. This reverse operation is bounded: INT_MIN always produces zero
for a finite nonzero input, so the nonzero branch cannot negate INT_MIN.

Original scalbln clamps long exponents to int range before calling scalbn;
ldexp delegates to scalbn. Algorithms and all ten original files are retained,
with exact patches and an authored portable ABI header. The implementation uses
the current nearest-even arithmetic profile. Dynamic fenv rounding and flags
remain open, as does binary80 arithmetic allocation performance.

Nine component checks compare exact original/adapted native bits with all C/C++
static/two-owner JIT/NativeAOT modes. Cases cover extreme exponents, full-width
long bounds, random input bits, ties, normal/subnormal boundaries, signed zeros,
NaNs, infinities, errno, GC and owner/TLS isolation. One check replays every
source patch. Eight shared packaging checks additionally verify184 source units,
all nine GNU aliases and runtime function-pointer identity, plus synchronous
signal integration. Evidence: milestone-scale-signals.json. Linux-x64 execution
is verified; Windows/macOS and other architectures remain required gates.
