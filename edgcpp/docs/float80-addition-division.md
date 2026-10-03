# Fixed-width binary80 addition and division

Finite addition normalizes each significand to64 bits, orders magnitudes, then
aligns into UInt128 with three low guard bits and a sticky bit. Close operands
retain exact cancellation; distant discarded bits still distinguish halfway
rounding. The existing exact packing helper performs one nearest-even rounding.
Subtraction uses this same addition after flipping the second sign. Both zero
signs and the established numerical interpretation of finite noncanonical
encodings are retained.

Division determines the exact leading quotient exponent first. Values below
half the smallest subnormal round to signed zero immediately. In all remaining
finite cases the scaled numerator fits128 bits and a left-scaled denominator
fits65 bits. UInt128 quotient/remainder arithmetic therefore provides exact
tie detection without BigInteger. The shared packer handles a rounding carry
and final overflow. Special NaN/infinity/zero behavior is unchanged.

Each operation matches14738 native x87 vectors and100000 independent BigInteger
reference cases under JIT and NativeAOT. One million warmed operations allocate
zero bytes for addition and division. Nine composed logarithm checks cover
native/original results and all generated C/C++ static/two-owner JIT/NativeAOT
modes against the final merged runtime. Evidence is indexed in
`evidence/milestone-float80-add-divide.json`.

The four basic Float80 operators and comparisons now avoid heap allocation.
Conversions and other library helpers still have BigInteger paths; no complete
performance or dynamic fenv claim follows. Platform execution remains Linux-x64.
