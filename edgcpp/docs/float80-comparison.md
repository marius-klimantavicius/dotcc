# Allocation-free binary80 comparisons

The fixed portable profile stores long double as `Float80`. Comparisons now
use exact integer exponent/significand comparisons, with no managed allocation.
The previous path aligned both operands into `BigInteger` values, potentially
with over 32,000 bits even though comparison needs only their leading exponent
and at most 64 significand bits.

For a nonzero finite operand, count leading zero bits in its significand.
Subtract that count from its effective exponent and left-normalize the
significand by the same count. Compare the resulting exponent first, then the
normalized significand if exponents match. The left shift loses no set bits.
Apply the sign after comparing magnitudes; signed zero, infinities and NaNs
retain their existing treatment. This also preserves the runtime's prior exact
numerical interpretation of finite noncanonical encodings and pseudo-denormals.
`<=` now performs one classification/comparison instead of evaluating `<` and
then `==`.

Accepted evidence is indexed in `evidence/float80-comparison.json`:

- 18,400 native binary80 vectors match all six relational operators under JIT
  and Linux-x64 NativeAOT.
- An independent test-only BigInteger reference checks arbitrary finite
  encodings, nearby equivalent representations, zero and extreme exponents.
- One million warmed calls evaluating all six operators allocate zero bytes
  on both managed execution paths.
- Existing binary80 math vectors and translated musl remainder/decomposition
  pass their JIT/NativeAOT regressions, including owner/TLS errno and bit checks.

This change concerns comparison only. Other arithmetic still uses BigInteger
where previously implemented; directed rounding, floating exception flags and
other platforms remain separate completion gates.
