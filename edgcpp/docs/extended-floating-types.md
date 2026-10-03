# Extended floating representation

The backend emits GNU `__float128` and C/C++ extended binary128 as a distinct
16-byte software value, and `_Float16`/`__fp16`/extended binary16 as `System.Half`.
The portable target still uses x87 binary80 for `long double`; equal storage
sizes do not conflate these types or their C++ exception identities.

`Float128.cs` reuses the repository's MIT-licensed dotcc implementation. The
source and adapted hashes are recorded in `runtime/float128-source.json`. The
adaptations change namespace, permit a separate conversion partial, and specify
16-byte layout. C-specific conversions in `Float128Conversions.cs` round directly
to binary16/binary32/binary80 instead of passing through double. Unsigned and
128-bit integer widening uses bounded UInt128 arithmetic; narrowing truncates
toward zero for the defined C range. Existing binary80 conversion now supports
direct half narrowing as well. NaN payload preservation is not promised.

Native GNU arithmetic generates 2,048 deterministic boundary/random rows for
addition, subtraction, multiplication, division and narrowing. The synthetic
fixture also round-trips all 65,536 half encodings (excluding NaN payload
identity), checks exact binary128 literals, 128-bit integers, binary80 roundtrip,
compound arithmetic, classification and distinct C++ thrown types. The original
EDG C/C++ output and all eight translated static/instance JIT/NativeAOT modes
pass ten checks in `artifacts/campaign/extended-float-translated-001/results.json`.
A separate managed probe includes a float midpoint plus one binary128 ulp,
which detects accidental double rounding.

These results cover representation and basic language operations, not complete
quadmath or floating-environment semantics. Arithmetic uses dotcc's exact
BigInteger core and round-to-nearest ties-to-even; allocation/performance
optimization and all directed rounding modes remain explicit completion work.
Bfloat16, complex arithmetic and complete extended-format libc APIs remain
separate gates. Only Linux x64 execution is currently established.

The integration regression after the source-nonlocal-jump filters passes33
checks in `artifacts/campaign/setjmp-float-regression-001/results.json`: existing
floating predicates,64 basic-language traces, array temporaries and initialization/
allocation under JIT/NativeAOT. Integer builtin count/parity/ffs/sign-redundancy/
byte-swap mappings separately pass five checks with10,000 deterministic cases
in `artifacts/campaign/integer-intrinsics-translated-001/results.json`; each
operand is evaluated once and width is validated before emission.
