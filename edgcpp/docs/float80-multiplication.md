# Allocation-free binary80 multiplication

The finite significand product fits exactly in UInt128. Multiplication now keeps
that product in fixed-width storage and rounds once to the selected binary80
quantum, preserving normal/subnormal transitions, signed zero and overflow.
The packing helper handles shifts at and beyond128 explicitly; C# masked shift
counts must not turn deep underflow into a different operation. A rounding carry
renormalizes the significand before the final overflow check.

Special NaN/infinity/zero behavior is unchanged. Arbitrary finite noncanonical
encodings retain the previous numerical interpretation. No floating environment
or NaN-payload preservation was added. Addition, division and conversions still
have their existing BigInteger paths; this is a targeted optimization.

Two standalone JIT/NativeAOT checks compare14738 native x87 results and100000
independent BigInteger products, including arbitrary input encodings. One million
warmed products allocate zero managed bytes in either mode. Nine further checks
compare original/adapted native logarithms and all generated C/C++ static/owner
JIT/NativeAOT modes, exercising multiplication inside composed binary80 arithmetic.
See evidence/milestone-float80-multiply.json. Runtime source hashes match the
standalone candidate exactly; all intermediate binaries and failures are retained.
