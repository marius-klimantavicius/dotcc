# Narrow pinned-EDG hexadecimal subnormal exception

This is an intentional target-semantics adaptation, authorized by root after
independent source review and exact positive native witnesses. It does not
qualify general floating parity or frontend integration. Correctly rounded
exact-rational binary32/binary64 behavior is retained. No host floating arithmetic
is semantic authority, and no valid literal is marked Unsupported to imitate
the native defect. Passing managed assertions still require held execution.

## Exact native observation

`native-floating-contract-subnormal-half-round1.json` accepts0 with inexact
warnings under the pinned strict C++20/LP64, used-instantiation, no-codegen,
predefined-file-disabled, explicit two-argument UTF8-source profile. Exact source
is `tools/oracle-inputs/m1-floating-contracts/subnormal-half.cpp`; receipt records
its bytes and compiler SHA e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3.

| Literal | Native value proved by accepted equality | Correct nearest payload |
| --- | --- | --- |
| `0x0.fffffeffffffp-126f` | `0x1p-127f`, binary32 `0x00400000` | largest subnormal `0x007fffff` |
| `0x0.fffffffffffff7ffffp-1022` | `0x1p-1023`, binary64 `0x0008000000000000` | largest subnormal `0x000fffffffffffff` |

The six expected-accept explorations named subnormal-direct-float,
subnormal-direct-double, subnormal-edg-float, subnormal-edg-double,
subnormal-classification and subnormal-scaled all returned2 with failed
assertions, matched=false. They are retained historical expectation mismatches,
not language-negative passes or parity evidence. An initial transmitted summary
incorrectly called the minnormal guesses accepts; it was immediately retracted
before any fix or acceptance. Actual receipts and the later half-value positive
control govern. Decimal direct-rounding and cooked exponent-like UDL controls
separately accept0 and are not explained by this hexadecimal defect.

## Integer/rational nearest proof

Measure each positive exact literal in units q of its format's smallest
subnormal, q=2^-149 or q=2^-1074. The midpoint between largest subnormal and
smallest normal is respectively 8388607.5 or 4503599627370495.5 units.

For binary32 the literal is exactly 281474959933439/33554432 units. It is
1/33554432 unit below the midpoint and 16777215/33554432 unit above the largest
subnormal. For binary64 it is exactly 4722366482869644689407/1048576 units,
1/1048576 unit below midpoint and 524287/1048576 unit above largest subnormal.
Each distance to largest subnormal is strictly below1/2; distance to minimum
normal is strictly above1/2. Therefore nearest is uniquely largest subnormal;
no tie policy ambiguity exists. Direct target rounding and Inexact/Tiny facts
must survive; a factor-of-two change is not a permitted rounding alternative.

## Source/configuration correspondence

Pinned `float_pt.c` conv_mantissa_to_floating_point2751–2776 first rounds to
format precision, shifts LEFT to remove the explicit leading one and make it
implicit (2754–2758), and then calls
check_and_denormalize_hex_fp_value. That routine2282 rounds the shifted subnormal
mantissa after its2271–2273 shift RIGHT restores the explicit leading bit;
round_hex_fp_value2132–2134 may retain a right shift and increment the
exponent on carry. The caller2290 unconditionally replaces that exponent with
min_exp-1. store_hex_fp_value2461ff packs the shifted mantissa with the reset
denormal exponent, producing the witnessed half-minnormal payloads. This narrow
source-backed defect is distinct from the intended nearest-rounding algorithm.
The source SHA256 is ac39be169ec49270b762dc03ed6e41087709fde15698434b2c124f5a383c376a.
The actual native fixture/configuration evidence is required alongside source
branches; this note does not infer all USE_HOST_FP_CONVERSION_ROUTINES or target
configuration behavior from one executable. Decimal direct-rounding has its
own passing discriminator; shared rational rounding, conversion and arithmetic
remain correct rather than inheriting this hexadecimal packing error.

FLT-01 exponent-like decimal UDL disambiguation is a separate actual decoder
repair, never covered by this exception. Other numerical differences require
independent investigation. Foundation and integrated frontend acceptance remain
separate held-code/execution/review gates.
