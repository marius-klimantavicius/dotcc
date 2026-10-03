# GNU checked overflow builtins

SQLite's signed64 add/subtract/multiply helpers require GNU overflow builtins.
The adapter supports generic and signed/unsigned typed add/subtract/multiply
forms, evaluating each argument once and storing the truncated result even when
overflow is reported. Generic operands retain individual signedness rather than
undergoing usual arithmetic conversions.

Same-width32/64 add/subtract use direct modular arithmetic and sign/carry tests.
Inputs through64bits use exact Int128 arithmetic (UInt128 for unsigned products).
128bit operands use signed magnitudes, carry detection and product-high detection
without allocating BigInteger or using managed exceptions. Volatile destinations
retain the existing scalar barrier-store contract. Predicate-only overflow_p
forms are not part of this project-required slice.

Ten checks in `evidence/checked-overflow-builtins.json` compare40 digests with
GCC, Clang and original EDG C;32 mixed width/sign combinations exercise1,600
input pairs each for all three operations. Typed families, aliasing, evaluation
counts, canaries and volatile outputs are included. C17/C++20 static/instance
JIT/NativeAOT all match on Linux x64.

The first SQLite managed library now builds strictly after retaining an
intentional C signed-to-unsigned OR conversion. CS0675 is suppressed in generated
projects, matching dotcc's generated-code warning policy; its sign bits are not
changed. The interim diagnostic build omits optional SQL math explicitly and
does not establish full-profile project acceptance.
