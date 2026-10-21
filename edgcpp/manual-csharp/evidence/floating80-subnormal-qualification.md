# Separate binary80 hexadecimal boundary exception

Root authorizes a narrow exact-nearest numerical exception for the positively
witnessed below-boundary long-double literal. This is separate from the32/64
half-minnormal defect. Correct binary80 rounding is preserved; neither zero nor
an invalid raw encoding is fabricated to imitate the pinned native result.
Actual managed fixed-bit/flag/rational-boundary execution remains required.

## Executed native fact and bounds

`native-floating80-contract-below-to-zero-round1.json` accepts0 with an inexact
warning. Source `tools/oracle-inputs/m1-floating80-contracts/below-to-zero.cpp`
defines constexpr auto x from `0x0.fffffffffffffffeffffp-16382L` and proves
`x == 0.0L`. Exact source SHA256 is 51cf40a73f5073477a8e32753dbd4cca5eb8c2244479dd78c6a4befd2c4d1492.
The receipt pins compiler e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3,
strict C++20/LP64, used instantiation, no-codegen, predefined file disabled and
explicit two-argument UTF8 source interpretation. This observes frontend constant
comparison behavior; it does not observe80 raw bytes or runtime machine state.

The nearby above-half-minimum literal
`0x1.0000000000000001p-16446L == 0x1p-16445L` accepts0 independently. The exact
size/alignment/minimum-subnormal/half-minimum witness in
`native-floating-contract-binary80-layout-round1.json` accepts0 and qualifies
16-byte size/alignment16 and the listed minimum/half-minimum values, not
precision64 by itself. Separate native-floating-contract-format-values-round1
and native-floating-isolated-long-double-abi-probe-round1 receipts discriminate
precision64. None proves byte order or raw payload layout. No unavailable bit-cast is treated as a pass.

Seven exploratory expected-accept/native2 hypotheses are retained and excluded:
below-to-maxsub, below-to-minnormal, below-to-half, midpoint-to-minnormal,
below-unordered, below-ordered-positive and midpoint-unordered. False assertions
are not invalid-language tests. In particular the midpoint's exact native numeric
value remains separately unproved; this exception does not silently qualify it.

## Independent exact-nearest proof

In units q=2^-16445, the below literal is exactly
N/2^17, N=2^80-2^16-1. This equals
(2^63-1)+1/2-2^-17. The midpoint between largest subnormal and smallest normal
is (2^63-1)+1/2. The literal lies2^-17 unit below it and1/2-2^-17 unit above
largest subnormal. Nearest is uniquely largest subnormal; no tie choice exists.
Correct canonical payload has SignExponent0 and Significand0x7fffffffffffffff.
Smallest normal is SignExponent1, Significand0x8000000000000000. Neither has
the native comparison's zero value. Fixed expected bits/flags and N/N+1/N+2
rational boundary tests must execute under both managed runtimes.

## Source inference, explicitly not observed bytes

Independent review traced float_pt.c conv_mantissa_to_floating_point2751ff
normal-precision rounding followed by check_and_denormalize_hex_fp_value2217ff.
Binary80 takes implicit_bits=0, unlike32/64. Staged rounding across this boundary
and the caller's exponent reset are source-predicted to create exponent0/J1
(a pseudo-denormal raw pattern). The raw pattern is an inference from source
operations, never an executed byte observation. If that predicted pattern reaches SPLIT_FN, GET_FRACTION459–490 copies J but
excludes it from saw_nz, and SPLIT_FN540–548 classifies it fpt_invalid before
the ordinary-zero branch557–559. It does NOT classify that pattern zero.
The executed fp_compare4033–4085 instead fetches host values via1407–1495;
no trace establishes that SPLIT_FN causes the observed zero comparison or
even receives the inferred pattern on that execution route. The source
counterpart is conditional; the exact causal path of the observed numerical
non-parity remains unproved. These evidence levels do not alter the narrow
root-authorized nearest-value exception.

Pinned float_pt.c SHA256 ac39be169ec49270b762dc03ed6e41087709fde15698434b2c124f5a383c376a.
Pinned float_type.h SHA256 44b9d368c0f7d3b0d88debe60c95d693154df76837b7786426d5ac4cc2232bfb.

The scoped exception covers this positively witnessed below-boundary literal
only. Midpoint/raw pseudo-encoding/NaN/layout branches and any other numerical
difference need independent qualification. No shared arithmetic, decimal or
conversion algorithm is changed to mimic this defect. FP80-01 foreign-format
budget taxonomy is a separate actual implementation repair. Foundation/module
and eventual long-double frontend acceptance remain separate full gates.
