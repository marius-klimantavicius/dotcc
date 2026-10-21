# Floating foundation implementation contract and handoff

Status: **READY / OWNED-FILES HOLD after actual FLT-01 repair and root-authorized
FLT-02 qualification**. Independent review remains active and unaccepted. No
worker build, test, native execution, acceptance or frontend integration claim.
The final nonself map below supersedes every earlier held map.

## Early ownership / interface contract

- Worktree: `/home/marius/.t3/worktrees/dotcc/t3-95580a51`.
- Branch: `t3/floating-literal-foundation` (read directly from worktree HEAD).
- Base: `6c3848ca5f50c5e06f0e8a6fbfc07bfbfd5ef685` (read directly from its ref).
- Owned new files: `src/EdgSharp/FloatingValues.cs`,
  `src/EdgSharp/FloatingLiterals.cs`, `tests/EdgSharp.Tests/FloatingValueTests.cs`,
  `fixtures/m1-floating-*.cpp`, this handoff. Only additive test registrations
  in `tests/EdgSharp.Tests/Program.cs` are additionally permitted.
- No central type/graph/parser/expression/evaluator/storage/token/string/Unicode/
  preprocessor/provider edits. No builds/tests/native execution, Git mutations,
  commits, main-checkout edits or delegated work. Supervisor owns all validation
  and subsequent additive integration.
- Internal immutable `TargetFloatFormat` descriptors explicitly describe IEEE
  binary32 and binary64. `TargetFloatValue` carries a format plus exact `ulong`
  payload; classification, sign and payload remain inspectable without CLR
  floating arithmetic. No long-double mapping is inferred from the host.
- Internal `FloatingLiterals.Decode(string, FloatingLiteralProfile, ...)`
  returns an immutable typed result, with separate lexical status (decoded,
  malformed, valid unsupported, resource limit), requested suffix/type, error
  offset and optional value/conversion flags. Source/diagnostic ownership stays
  with the caller; offsets refer to the supplied token spelling. Unary signs
  belong to expressions; signed zero is created/preserved by value operations.
- Internal value operations accept explicit formats and return immutable
  outcomes (value plus inexact/underflow/overflow/invalid/divide-by-zero/resource
  flags). Exact integer/rational intermediates implement nearest ties to even;
  integer conversion is target-width-aware and truncates toward zero. No native
  process, CLR floating parser or host long-double semantics is used.
- Limits are explicit and immutable. Oversized token/digit/exponent work is
  bounded before large powers/shifts. Calls share no mutable TU/source caches.
- Primary integration proposal: consume decode outcomes at the actual numeric
  token path, select target-qualified float type, preserve payload in a distinct
  floating constant variant, and invoke floating operations after actual C++
  conversions. These are proposed hooks only; this worker will not modify them.

Full M0–M6 remains required. This independently reviewable foundation does not
establish language support; parser/type/conversion/overload/storage/evaluator
integration and later floating branches remain active campaign work.

## Actual authored behavior / algorithm adaptation

`FloatingValues.cs` implements logical IEEE binary32/binary64 bit encodings in
an immutable value object. `ulong Bits` is the scalar payload; `BigInteger` is
used only for exact intermediate magnitudes, ratios and returned integer values.
Sign, normal/subnormal/zero/infinity/NaN classification, quiet/signaling state
and NaN payload are explicit. Logical bits do not select a host byte order,
target alignment or C++ semantic type identity.

`RoundRational(format, negative, n, d, binaryExponent, limits)` evaluates
`(-1)^negative * n/d * 2^binaryExponent`. Bounded bit lengths determine the
binary exponent by one exact comparison. It selects normal spacing or the
minimum subnormal spacing, performs exact DivRem, compares the remainder to
half the denominator, and increments for above-half or odd halfway results.
Carry renormalizes the exponent and can overflow to infinity. This retains
EDG's explicit normalization, half-ULP, sticky-tail/even and range decisions.
Direct rounding at subnormal spacing deliberately avoids double rounding at
the normal/subnormal transition.

Decimal scanning trims leading/trailing zeros and records radix-point scale.
Its exact ratio factors powers of ten into powers of five and two, following
the exact-integer portion of `floating.c:dec2bin`. The authored adaptation
replaces FAST_DEC2BIN's initial approximation and subsequent error/epsilon
adjustment with one exact quotient/remainder operation. It does not use CLR
floating parsing, host float arithmetic, strtod/strtold, runtime compilers,
generated C# input, SoftFloat packages or native processes. Hexadecimal scanning
retains all bounded significant digits rather than EDG's four-word temporary
mantissa plus sticky bit; intended half/tie/even decisions are made exactly.
Root authorized a narrow exception for the proved pinned-EDG subnormal packing
defect below; this is not a blanket hexadecimal/native parity claim.

Values implement integer-to-float rounding, float-to-integer truncation and
destination signed/unsigned range checking, explicit format conversion, unary
sign inversion, contextual bool, same-format equality/order, add, subtract,
multiply and divide. Arithmetic combines exact decomposed significands and
exponents (or ratios for division) and rounds once to the selected target.
No operation passes through existing integer constant folding. Binary operands
must already have the same format; mismatches throw an internal contract error
instead of inventing C++ promotions or overload ranking.

NaN representation equality includes payload/sign; numeric equality/order uses
`Compare`, including equal signed zeros and unordered NaNs. This module chooses
a deterministic numeric NaN policy: first NaN operand payload/sign, quieting on
arithmetic or cross-format conversion, InvalidOperation for a signaling input;
generated invalid results use positive canonical quiet NaN. Payloads are aligned
by fraction width for cross-format conversion, and low bits can be discarded.
Same-format copies and `Negate` preserve every payload/signaling bit. Quiet
comparison returns Unordered without raising signaling exception state. These
policies are explicit numerical choices, not qualified parity for every EDG
host/SoftFloat/exception mode or builtin NaN construction path.

`FloatingLiterals.Decode` scans complete decimal/hex numeric spelling, including
required hexadecimal binary exponents, optional decimal exponents, f/F and l/L,
digit separators in each digit sequence and malformed positions. Leading unary
signs, whitespace and spellings such as `nan`/`inf` are not floating literals.
Long double, f16/bf16/f32/f64/f128/f32x/f64x, w/q and recognized imaginary forms
have explicit unsupported outcomes. Even suffixes with binary32/64 storage
need separate extended C++ type identity/dialect qualification, so f32/f64 are
not silently treated as ordinary float/double. Unknown identifier suffixes,
including reserved identifiers such as ff, are potentially user-defined, not a
second builtin suffix; actual literal-operator lookup is still required.
Accepted Unicode/UCN identifier validation is reused under the spelling limit,
without editing Unicode code or constructing/caching source owners.

Following FLT-01, the decoder implements EDG's decimal-point exponent fallback:
if e/E has no following exponent digits and a decimal point already identifies
the floating numeric core, it restores the marker as the start of the whole
putative UDL suffix. `1.0e`/`1.0e_tag` return ValidUnsupported/UserDefined at
SuffixOffset3; `.1E_tag` does so at SuffixOffset2. Identifier-tail validation
still runs. Missing digits after a sign, misplaced apostrophes and missing hex
exponents remain malformed. The no-decimal-point integer/UDL branch belongs to
integer-token decoding, outside this floating-core API. Modern delimited UCN
syntax retains the accepted validator's explicit unqualified/Unsupported outcome.

## Exact internal API and proposed integration

All new production types are internal. Public-looking Equals overrides implement
the normal object/IEquatable contract on an internal immutable type.

```csharp
// Immutable singleton format descriptors; private constructor.
TargetFloatFormat.Binary32 / Binary64
TargetFloatValue.FromBits(TargetFloatFormat format, ulong bits)
TargetFloatValue.Zero/Infinity(TargetFloatFormat format, bool negative = false)
TargetFloatValue.NaN(TargetFloatFormat format, ulong payload = 0,
    bool signaling = false, bool negative = false)
TargetFloatValue.Negate() / QuietNaN()

FloatingLiteralResult FloatingLiterals.Decode(string spelling,
    FloatingLiteralProfile profile, FloatingLimits? limits = null)

FloatingResult FloatingValues.RoundRational(TargetFloatFormat format,
    bool negative, BigInteger numerator, BigInteger denominator,
    int binaryExponent = 0, FloatingLimits? limits = null)
FloatingResult FloatingValues.FromInteger(TargetFloatFormat format,
    BigInteger integer, FloatingLimits? limits = null)
FloatingIntegerResult FloatingValues.ToInteger(TargetFloatValue value,
    int width, bool signed, FloatingLimits? limits = null)
FloatingResult FloatingValues.Convert(TargetFloatValue value,
    TargetFloatFormat destination, FloatingLimits? limits = null)
FloatingResult FloatingValues.Add/Subtract/Multiply/Divide(
    TargetFloatValue left, TargetFloatValue right, FloatingLimits? limits = null)
FloatingOrder FloatingValues.Compare(TargetFloatValue left, TargetFloatValue right)
bool FloatingValues.NumericallyEqual(TargetFloatValue left, TargetFloatValue right)
bool FloatingValues.ToBoolean(TargetFloatValue value)
```

`FloatingResult` has optional Value and FloatingFlags; `FloatingIntegerResult`
has optional BigInteger Value and flags. `FloatingLiteralResult` contains
Status, Suffix, IsHexadecimal, SuffixOffset, ErrorOffset, Issue, Numeric and
derived Value/IsExact. No source/diagnostic/session/AST references are retained.
ErrorOffset is an offset in the supplied UTF-16 spelling; -1 denotes no issue.
SuffixOffset=-1 means scanning failed before suffix classification.
Malformed/unsupported results have no value. Resource results have no value and
ResourceLimit. Decoded numerical overflow/underflow still has a value plus flags.
IsExact describes finite numerical values only; NaN/infinity payload transport
does not claim a mathematical exactness result even when its flags are empty.

The actual current `Expressions.cs:ScanOperand` handles malformed floating
spelling through `Lexer.IsValidFloatingSpelling`, then diagnoses valid floats as
unsupported. No new decode call has been inserted. The current semantic graph
and evaluator remain integer-value-only. No language support is claimed here.

Exact proposed numeric-boundary call for the primary-owned operand path:

```csharp
FloatingLiteralResult decoded = FloatingLiterals.Decode(
    token.Spelling, FloatingLiteralProfile.Binary32Binary64, floatingLimits);
// Caller maps decoded status/flags to its DiagnosticBag and token-origin range.
// Only a newly implemented FloatingType/FloatingLiteralExpression can own Value.
```

Primary-owned required integration (not applied in this worktree):

1. Add explicit target floating configuration/type identity and canonical cv
   Float/Double types in Types; retain LongDouble unsupported until implemented.
2. Add a distinct floating constant/value variant in SemanticGraph/Expressions;
   do not encode Bits or rational intermediates into BigInteger ConstantValue.
3. Replace/bypass the old blanket lexical suffix validator at the correct active
   numeric-token phase. Preserve inactive/unused PP timing, UDL disambiguation,
   source origins and malformed Unicode checks; do not eagerly decode every PP
   number. The old validator currently cannot express these typed distinctions.
4. Implement EDG scalar floating/integer/bool conversions, usual arithmetic,
   overload ranks and explicit conversion nodes before calling these operations.
5. Extend real constant evaluation, storage/loads/parameters/returns, failure and
   required-constant policy to carry a floating payload variant. Extend replay,
   templates and reference transport without changing canonical ownership.
6. Map strict/GNU/Microsoft range/inexact and IEEE-exception rules from actual
   EDG configuration, then author source-to-graph positive/negative tests and
   native semantic witnesses. Current value tests prove only isolated APIs once
   the supervisor executes them.

No new shared interface is needed to compile the foundation: the only reused
internal semantic helper is existing `UnicodeIdentifiers.Analyze`, unchanged.
No central-file patch is required or authorized for this checkpoint.

## Range / failure / resource policy

- Fixed round-to-nearest, ties-to-even only. Explicit alternate rounding modes,
  excess precision, dynamic rounding/traps and FP contraction are future work.
- Tiny is detected after rounding. Exact subnormal: Tiny alone. Inexact rounded
  subnormal/zero: Tiny|Underflow|Inexact. A halfway result rounded up to minimum
  normal has Inexact without Underflow. Exact zero has no Tiny, including signed
  zero and zero with an enormous but bounded exponent.
- Finite overflow rounds to signed infinity with Overflow|Inexact. Lexical
  Status stays Decoded. This is a numerical outcome, not source acceptance:
  supervisor's strict-profile `1e1000f` probe rejects, and frontend policy must
  diagnose it. No guessed blanket invalidness replaces these distinctions.
- Finite nonzero / signed zero returns signed infinity with DivideByZero;
  0/0, infinity/infinity, opposite infinities in addition and infinity*zero
  return quiet NaN with InvalidOperation. The evaluator must apply its actual
  required-constant/IEEE exception policy; these values are not automatically
  legal constant expressions. Nonfinite integer conversion has no integer result.
- Integer conversion truncates first, then checks the requested signed/unsigned
  target width. Out-of-range returns no value plus InvalidOperation and
  IntegerOutOfRange; no wrap/saturation result is supplied. GNU saturation and
  error-but-result branches in folding.c remain explicitly unimplemented.
- Defaults: token16,384 UTF-16 units, significand4,096 digits (including zeros),
  exponent4,096 digits (including leading zeros), intermediate32,768 bits.
  Constructor hard ceilings: token65,536, significand16,384, exponent16,384,
  intermediate65,536 bits; minimum intermediate64. Exceeding a policy is an
  explicit ResourceLimit, not a CLR overflow, invalid C++ or successful skip.
- Grammar scans iteratively within these limits. Exponent magnitude saturates
  at1,000,000, greater than any cancellation possible from a bounded mantissa;
  the rest of exponent syntax is still checked. Known far decimal orders yield
  target overflow/underflow before powers. Zero is recognized before scaling.
- Significand accumulation, powers of five, rational scaling and arithmetic
  shifts/products are conservatively bounded before allocation. A conservative
  resource result can precede a representable result under a very small custom
  budget. No promise of maximal acceptance at each bit-budget boundary is made.
- Argument-contract violations (nonpositive denominator, negative magnitude,
  invalid width/limits, incompatible operand formats or oversized raw32 payload)
  throw internal Argument exceptions. Source-sized failure paths return typed
  outcomes. No recursive input traversal or mutable per-TU/static power cache.
- Every call owns temporary BigIntegers only. Format/profile/default-limit
  singletons are immutable and retain no TU or source. The existing Unicode
  declarative tables remain unchanged; no additional static source cache exists.

## Pinned EDG provenance and partial source coverage

Imported source revision from `upstream-origin.json`:
`89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`; actual consulted bytes below are
identified separately. Base/checkout is the accepted commit named above.
All derived source/test/fixture files retain Apache-2.0 WITH LLVM-exception.

| Actual source / routines or conditional branches inspected | Adaptation / omitted coverage |
| --- | --- |
| literals.c:915–1140 `conv_float_literal`; `remove_digit_separators` | Unsuffixed/f/F vs L kinds, separator handling, decimal/hex dispatch and inexact/range distinction adapted. GNU imaginary both suffix orders, w/q, extended f16/bf16/f32/f64/f128/f32x/f64x recognized unsupported. Microsoft `.1.234`, pcc missing-exponent repair, constructed complex constants and diagnostic severity branches omitted, not silently accepted. |
| lexical.c:12038–12735 `scan_number`, `skip_digit_separator`, float_accum1/2, end_float_accum;12461–12482 decimal-point exponent fallback;12824–12830 suffix boundary preservation | Numeric-core decimal/hex exponent and separator validation, including restoring e/E as whole UDL suffix after a decimal point. PP-number streaming/context timing, actual UDL lookup and no-point integer reinterpretation, GNU imaginary precedence/version gates, fixed-point parsing and Microsoft/Clang/pcc dialect quirks are not ported. |
| floating.c:131–285 `split_string`;529–646 tail-to-half/should-round/shift;1337–1430 powers5;2339–2586 `adjust_down`, `adjust_up`, `dec2bin` | Leading/trailing zero/exponent scale, exact powers2/5, half-ULP/even/sticky decisions adapted to bounded exact rationals. No FAST_DEC2BIN, host/emulated preliminary approximation, formatting/bin2dec, Java formatting or mutable power caches ported. Infinity/nan string readers are deliberately excluded from C++ literal grammar. |
| float_type.h format setup,580–675 NORMALIZE_AND_ROUND/MAKE_FN,1367–1495 CONVERT_FN/READ_FN | Explicit hidden-bit binary32/64 packing, normal/subnormal/zero/infinity and signed representation adapted. _Float16/bfloat16,80/128, host unions/endian byte layout, half precision special normalization and string formatting not implemented. |
| float_pt.h `a_mantissa`, round/conversion/arithmetic declarations | Contracts examined; managed payload/exact-rational temporaries replace native union/array pointer ownership. No claim of porting every declaration. |
| float_pt.c:1500–1700 `make_fp_nan`/`make_fp_infinity`/classification | Quiet/signaling exponent/fraction distinction retained. Native payload32 limitation, endian pointer edits, Microsoft signaling quirks and default payload choices not replicated; explicit policy documented above. |
| float_pt.c:1843–1876 `fp_change_kind`;1942–2029 mantissa initialization/shifts/masks;2031–2139 `round_hex_fp_value`;2220–2325 `check_and_denormalize_hex_fp_value`;2461–2580 `store_hex_fp_value`;2582–3006 hex mantissa/conversion and `fp_string_to_float` | Same-kind bit copy, intended normalize/ties/even/carry, range/subnormal and typed result policy adapted. Root explicitly excepted the proved second-round carry/exponent-reset packing defect; correct target values remain decoded. Long-double-is-double, explicit80/128, fixed-point rounding, USE_HOST_FP_CONVERSION_ROUTINES strtod/strtold, GNU errno suppression, Microsoft underflow, host-FP and SoftFloat implementations are not used. |
| float_pt.c:3687–4085 integer conversion, zero, fp_add/subtract/negate/multiply/divide/compare | Exact target numerical equivalents authored. Native a_host_fp_value fetch/store and SoftFloat macros replaced; depends_on_fp_mode and profile exception choices remain for the real evaluator. Non-IEEE target branch, ceil and complex arithmetic omitted. |
| folding.c:561–935 conv_integer_to_float/conv_float_value_to_int_value/conv_float_to_integer/conv_float_to_float;3091 do_fnegate;4301–4518 do_fadd/subtract/multiply/divide/compare;4688 do_fgnu_min_max | Numeric conversion/result/error separation and unordered comparison branch followed. This module does not port C++ conversion/type/fold dispatch, constant classification, GNU saturation/min/max, imaginary/complex, strict exception policy, builtin math/pow or constexpr legality. |
| floating.h FPT_FLOAT/DOUBLE defaults, long-double binary64/extended80/binary128, FP_USE_EMULATION and bigint bounds | Explicit32/64 descriptors; no host-configuration inference, no other formats or formatting APIs. |
| targ_def.h:834–984 TARG_FLT/DBL/LDBL precision/exponents;2239–2555 size/alignment/host-IEEE configuration | Inspected width/precision/exponent/configuration distinctions. Logical bit representation is not an ABI-layout qualification. |
| defines_linux.h x86_64 defaults, FP_LONG_DOUBLE_IS_80BIT_EXTENDED vs aarch64/riscv binary128, configured target fields | Source evidence for long-double differences preserved. Active oracle facts are separately witnessed below. |
| config/base.cmakedef,config/cpfe.cmakedef; backend/target/portable-lp64-binary80-v2.def | Broad feature enables/import and separate backend target facts inspected, not assumed to configure the manual frontend or selected native executable. |

Consulted upstream/config SHA-256 (source files, not execution receipts):

```text
de5276f87f073a67bb4b7c6a4cd152a66eab0a155fda84f0d991e3d296213d95  upstream/src/literals.c
7f627e2aa3f3c1ee75ee58529eeceb95f1db71caee06ded0cb5615a1bd31b45a  upstream/src/floating.c
ac39be169ec49270b762dc03ed6e41087709fde15698434b2c124f5a383c376a  upstream/src/float_pt.c
3ae9427a552c5c0957175f958420d780334466b22672db60ea8b131195e885ba  upstream/src/float_pt.h
44b9d368c0f7d3b0d88debe60c95d693154df76837b7786426d5ac4cc2232bfb  upstream/src/float_type.h
cb448268eb224ba4dfa21894d7fa66c4c94c39c94247f13623bb317483160b59  upstream/src/floating.h
91fef22e8578541a8f7211ce401b7f15a3acd55aa42bed2815c254237cf045ff  upstream/src/folding.c
e39ed09cdd9d150092231dce1d2b39032ffd442efe57ced0ad7636e12cf1958b  upstream/src/targ_def.h
64f9c35401c9ab612443d85edd74574881d074a9036cb863cf7fe12c7a3b1948  upstream/src/defines_linux.h
e987ffcb7645931564f3a6f262d089c21524e78ce2c60d5e0e0868a5b4b98875  upstream/src/lexical.c
e3944295b342c75e2f7527668e689fa90a2e64b39a1ca5478ec63cfe9229cbf9  config/base.cmakedef
e4ca5b2fdd46f826ebde5e2d10b5cdcbd8938a0e6df998fa43348424332af29b  config/cpfe.cmakedef
c0a05e546107298a6a979fb02935237f6975cbc809e76e4a0a0fbe66b963e793  backend/target/portable-lp64-binary80-v2.def
```

Paths above are relative to edgcpp. Reused accepted manual identifier helper:
`src/EdgSharp/UnicodeIdentifiers.cs` SHA-256
`5efa6c24dcb94cf14db83d85d91da6f8d832d81290b5627969ce50ef4a76b84b`;
no change to that helper or its declarative data.

## Supervisor-executed target witnesses (external to this worktree)

The supervisor supplied and this worker read (read-only) these main-checkout
receipts and exact source copies. They are not executions by this worker or
passes of this worker's authored fixtures/managed groups.

Main root: `/home/marius/p/dotcc/edgcpp/manual-csharp`.
Native compiler recorded by all three receipts:
`/home/marius/p/dotcc/edgcpp/build/bin/cpfe`, SHA-256
`e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3`.
Arguments: `--clear_flag use_predefined_macro_file --c++20 --strict
--no_code_gen --instantiate used --unicode_source_kind UTF-8 <source>`.
Receipt original source/cwd is isolated `t3-96dc97a5`; retained main source-copy
hashes below match those recorded source hashes. This does not claim a rehash or
re-execution of the compiler binary by this worker.

- `evidence/native-floating-contract-format-values-round1.json`: exit0, matched;
  receipt SHA `6228865d106a5fd0f3a83100f8a102f9c9ae1a246b0f99d548b8503c6d163f7c`.
  Source `tools/oracle-inputs/m1-floating-contracts/format-values.cpp` SHA
  `fcd4a64bbb902dcd2704fb8c567e0f987c49c290740d8f053f7931683286b246`.
  sizeof float4/double8/longdouble16; float integer/hex halfway-even controls;
  double integer halfway controls; positive float minimum subnormal;
  long-double64 precision witnesses around2^63 and2^64 and exponent16383.
  Native hex inexact warnings are retained, not hidden.
- `evidence/native-floating-contract-underflow-round1.json`: exit0, matched;
  receipt SHA `cde33d1ce3d7d9cdbba19f78301a3a9bfef22b60ef02f5422d1ec36fb496c1fb`.
  Source `underflow.cpp` SHA
  `a4f693231fa7b8a125ce17177c9fe727c27aaab55cee92ebf99510bf99f664ac`.
  Half minimum float subnormal `0x1p-150f` rounds zero, with native inexact warning.
- `evidence/native-floating-contract-overflow-round1.json`: exit2, matched;
  receipt SHA `d86e6f0f2894c2276d63df21a31076d7622848adf4b31e446d6c5f4d1d4ea9c5`.
  Source `overflow.cpp` SHA
  `218e4ebe1b4a8e762a7c44a45af12d7b547ecd28b2ee8628f7043f55cfe6be9b`.
  `float value=1e1000f;` rejects as an out-of-range floating constant.

These give active-profile target evidence separate from host and source-default
assumptions. They do not implement long double; `L` remains unsupported by code.
They also do not qualify every subnormal/tie/decimal/native diagnostic branch.

## Authored group inventory / registrations (unexecuted)

Exactly19 additive Program registrations, after CharacterPreprocessing and before
Unicode groups. Existing112 + new19 = proposed131 groups, zero executed here.
Full names are in Program.cs; method inventory in FloatingValueTests.cs:

1. FloatingRepresentations —32/64 descriptors, exact payload/class/sign/NaNs.
2. FloatingGrammar — decimal/hex/core/suffix, separator positions and offsets.
3. FloatingDecimalRounding — float vs double, below/half/above and even neighbors.
4. FloatingHexRounding — sticky tails, tie parity and exponent carry.
5. FloatingDirectRounding — decimal double-rounding discriminator and decimal/hex equivalence.
6. FloatingSubnormals — signed half/minsub, odd/even and transition to normal,
   including float/binary64 just-below-half controls that would expose double rounding.
7. FloatingOverflow — maxfinite, below/at carry threshold, range and zero.
8. FloatingSignedZero — zero sign, unary/binary and format-conversion transport.
9. FloatingIntegerConversions — precision rounding, signed/unsigned truncation/bounds.
10. FloatingFormatConversions — widening/narrowing, under/overflow, NaN bits and same-format copy.
11. FloatingAddition — ties, cancellation, signed result, subnormal/normal and overflow.
12. FloatingMultiplicationDivision — exact/inexact products, rational division and tiny ties.
13. FloatingNonfiniteOperations — infinity, zero divisor, invalid combinations and NaN policy.
14. FloatingOrdering — complete ordered boundary matrix, signed zero and unordered NaNs.
15. FloatingMalformed — suffix/exponent/separator/sign/whitespace/culture/Unicode controls.
16. FloatingUnsupported — extended/long-double/imaginary/UDL versus malformed,
    exact decimal exponent-marker suffix offsets and dialect gates.
17. FloatingResourceLimits — huge token/digits/exponents, bounded powers/shifts and failed conversions.
18. FloatingRecoveryConcurrency — repeated malformed/unsupported/resource failures then independent successful calls.
19. FloatingWideningRoundTrips —512 deterministic finite payload32->64->32 identities and4097 exact integer round trips.

No tests mirror a host parser; expected payloads and mathematical boundary
witnesses are authored. Raw-pattern round trips supplement explicit known-value
controls and do not by themselves establish correct rounding. No test bypasses
the numeric APIs with fixture answers.

## Authored native fixture proposals (all unexecuted here)

Common invocation is the exact supervisor profile above; preserve each fixture's
bytes and record an actual status0/2 expectation match. All remain unexecuted by
this worker; decimal-direct has an exact-byte supervisor accept noted below.
The subnormal fixture now retains explicitly qualified known native mismatches.
No cp/lp/rp compile/link/runtime upstream case is counted as a frontend parser pass.

| Owned fixture under fixtures/ | Proposed purpose / expectation and uncertainty |
| --- | --- |
| m1-floating-format-witness.cpp | Proposed accept0: scalar sizeof, actual literal decltype through template specialization, float/double integer ties and minsub/normal ranges. Extended long-double type identity is probed only, not implemented. |
| m1-floating-rounding.cpp | Proposed accept0: decimal below/at/above ties, hex even neighbors/carry, decimal/hex agreement and separators. Native inexact warnings may occur. |
| m1-floating-direct-rounding.cpp | Direct decimal->float versus rounded-double->float discriminator. Supervisor decimal-direct receipt now accepts0 with this exact source SHA ffcf5706..., establishing this decimal distinction. No worker execution or full decimal parity claim. |
| m1-floating-subnormal.cpp | Target-correctness witness including the exact18-digit native double probe and float below-half probe. Their nearest-value assertions intentionally differ from the proved pinned EDG half-minnormal defect under root's narrow exception. This is not an expected language-negative/parser pass. Other boundary controls remain individually unqualified until actual native/managed evidence, with no blanket parity extrapolation. |
| m1-floating-conversion-arithmetic.cpp | Proposed accept0: integer and float conversions, truncate-before-width-check, usual finite operations and target ties. Native intermediate/excess-precision behavior remains to be observed. |
| m1-floating-long-double-abi-probe.cpp | Native-only proposed accept0: longdouble16 storage/precision64/range and minsub. Supervisor has separately witnessed storage/precision/high exponent, but this exact fixture and its minimum-subnormal assertion remain unexecuted. |
| m1-floating-signed-zero-bits-probe.cpp | Header-free __builtin_bit_cast payload probe; strict builtin availability/constexpr support and expected accept0 are UNVERIFIED. Failure on builtin support is not numeric failure or a passing parser case. |
| m1-floating-overflow-invalid.cpp | Proposed reject2 for strict out-of-range float; matches a separately executed design probe in intent, not an exact-byte receipt for this fixture. |
| m1-floating-exponent-invalid.cpp | Proposed reject2 for missing binary exponent digits. |
| m1-floating-separator-invalid.cpp | Proposed reject2 for apostrophe immediately after exponent sign. |
| m1-floating-udl-unresolved.cpp | Proposed reject2 from missing literal operator. Lexically valid UDL remains Unsupported in foundation, not diagnosed as malformed suffix by numeric decoding. |

Native infinity/NaN payload and operation exceptions need separate explicit
builtin/profile witnesses; numeric API tests currently cover the documented
target policy only. No native source claim has been inferred from CLR behavior.

## Provisional review response FLT-01

Independent reviewer raised HIGH FLT-01 after the initial HOLD. Supervisor
explicitly issued a narrow source repair release. This response changes actual
decoding, not just docs or a caller-preselection deferral. No finding is claimed
independently closed or accepted before supervisor execution/review recheck.

Source rechecked: lexical.c12461–12482 restores possible_start_of_ud_suffix when
decimal_point_seen;12824–12830 excludes the whole putative suffix from conversion.
The decoder now saves the exponent-marker position and restores it when no
digits follow unsigned e/E after a decimal point. Whole-suffix Unicode/identifier
validation then runs through the existing UserDefined path. Concrete direct
test expectations:

| Spelling | Status / suffix / issue | SuffixOffset / ErrorOffset |
| --- | --- | --- |
| 1.0e | ValidUnsupported / UserDefined / UserDefinedSuffix | 3 / 3 |
| 1.0e_tag | ValidUnsupported / UserDefined / UserDefinedSuffix | 3 / 3 |
| .1E_tag | ValidUnsupported / UserDefined / UserDefinedSuffix | 2 / 2 |

Additional direct suffix controls include `1.E`, `.1e` and a Unicode UCN tail.
Paired negative assertions retain `1.0e+`/`1.0e-` as Malformed/MissingExponent,
`1.0e'1` as Malformed/InvalidSeparator, signed uppercase/dotted forms, and hex
missing/signed/identifier exponent failures. No numeric Value is produced on
unsupported/malformed paths. These source-path outcomes are statically
inspected assertions, not executed results.

The requested float subnormal regression already asserts
`0x0.fffffeffffffp-126f -> 0x007fffff` with Tiny|Underflow|Inexact. Added binary64
controls `0x0.fffffffffffff7p-1022` and `0x0.fffffffffffff7ffffffp-1022` assert
`0x000fffffffffffff` with the same flags; exact-half and just-above-half assert
minimum normal `0x0010000000000000` with Inexact only. Existing native subnormal
fixture now contains both below-half precisions as proposals. No numeric
algorithm defect is asserted or arithmetic implementation changed by this
review response; `FloatingValues.cs` retains its original held hash.

Only FloatingLiterals.cs, FloatingValueTests.cs, m1-floating-subnormal.cpp and
this handoff changed under this release. Program registrations remain19, total
inventory131; other production/fixture hashes are unchanged. Native suffix and
rounding controls, builds/tests/Git and independent finding closure stay with
the supervisor/reviewer. All are unexecuted by this worker.

## FLT-02: root-authorized narrow pinned-EDG defect exception

Root expressly directed: retain correctly rounded exact-rational target
semantics for the proved hexadecimal subnormal carry/exponent-reset defect.
Do not emulate the factor-two error, diagnose these valid literals Unsupported,
or alter shared RoundRational/decimal/conversion/arithmetic to inherit it.
Other numerical differences require separate source/witness investigation.
FLT-01 is independently repaired actual UDL decoding, not part of this exception.

Supervisor owns the unified source/witness/proof record, read here without edits:
`/home/marius/p/dotcc/edgcpp/manual-csharp/evidence/floating-subnormal-qualification.md`.
Observed SHA at this inspection:
`9c44e5c6825225a051272ea41478a215ed03e10d77817a9473eed085b6f1d094`.
This handoff retains the outcome/policy without taking ownership of that report.

Exact pinned observations (not the earlier retracted minnormal summary):

| Exact literal | Supervisor positive native equality / payload | Correct nearest payload asserted here |
| --- | --- | --- |
| `0x0.fffffeffffffp-126f` | `0x1p-127f`, binary32 `0x00400000` | maximum subnormal `0x007fffff` |
| `0x0.fffffffffffff7ffffp-1022` | `0x1p-1023`, binary64 `0x0008000000000000` | maximum subnormal `0x000fffffffffffff` |

The double spelling above has EXACTLY18 fractional hexadecimal digits, not just
the shorter/longer analogues. `FloatingSubnormals` explicitly calls Decode on
that exact spelling and the exact float spelling, asserting the correct payload
and Tiny|Underflow|Inexact. It also asserts their exact-rational results directly
and the exact midpoint/just-above controls through RoundRational. These are
authored assertions pending supervisor execution, not worker-reported passes.

Independent integer-distance proof retained as executable assertions:

- Float: input is `Nf*2^-174`, `Nf=0xfffffeffffff = 2^48-2^24-1`.
  In the same integer units, upper minimum-normal is `Uf=2^48` and lower
  maximum-subnormal is `Lf=(2^23-1)*2^25 = 2^48-2^25`.
  `Nf-Lf=2^24-1`, `Uf-Nf=2^24+1`, and `2*Nf=Lf+Uf-2`.
  It is exactly1 integer unit below midpoint, strictly closer to maximum-subnormal.
- Double: input is `Nd*2^-1094`,
  `Nd=0xfffffffffffff7ffff = 2^72-2^19-1`.
  `Ud=2^72`, `Ld=(2^52-1)*2^20 = 2^72-2^20`.
  `Nd-Ld=2^19-1`, `Ud-Nd=2^19+1`, and `2*Nd=Ld+Ud-2`.
  It is exactly1 integer unit below midpoint, again uniquely nearer the lower.
- Tests construct Nf from the exact48-bit integer literal and Nd from explicit
  high8+low64-bit integers, then assert the distances/equations, the Decode
  payloads, and RoundRational at N/N+1/N+2. No host FP parser or native semantic
  result supplies the correct answer. At midpoint N+1, ties-to-even selects
  minimum-normal; below midpoint, there is no tie ambiguity.

Source/configuration trace (same float_pt.c SHA ac39be16... recorded above):

1. `conv_mantissa_to_floating_point`2751–2776 rounds to format precision,
   removes the explicit hidden bit, decrements exponent, then calls the
   subnormal checker. Binary32/64 use an implicit bit, with precisions24/53.
2. `check_and_denormalize_hex_fp_value` restores/shifts the implicit mantissa
   bit. At2282 its second call rounds to `mant_dig-implicit_bits` (23/52).
3. `round_hex_fp_value`2128–2132 carries into mantissa0=`0x80000000`, retaining
   a right shift and incrementing the exponent. The checker then unconditionally
   sets exponent=`min_exp-1` at2290, discarding that increment.
4. `store_hex_fp_value`2461ff packs the shifted mantissa with the reset subnormal
   exponent: float `(mp0>>9)` gives `0x00400000`; double `(mp0>>12)` in the
   upper word gives `0x0008000000000000`. These are exactly the positively
   witnessed half-minnormal values, not either prior maxsub/minnormal guess.
5. This is the hexadecimal path (`fp_hex_string_to_float`/implicit-bit target).
   The demonstrated carry/reset defect is not a reason to change decimal
   `fp_string_to_float`, the shared target rounder, conversions or arithmetic.
   No universal host/SoftFloat/USE_HOST_FP_CONVERSION_ROUTINES claim follows.

Additional supervisor receipts read, main paths relative to the same root as
the earlier witnesses, same recorded compiler/profile/arguments:

- `evidence/native-floating-contract-subnormal-half-round1.json`: actual0,
  expected0, matched=true, native inexact warnings. Receipt SHA
  `b5d77cd1f99c218f094518f9fe6762a9427b3186ba965c83bad58688bf645bd5`;
  exact source `tools/oracle-inputs/m1-floating-contracts/subnormal-half.cpp`
  SHA `d63138a496fac4b37f9d966c9d3b5bd1eb2949da39217ac2592f22bdfeec6bf8`.
- `evidence/native-floating-contract-decimal-direct-round1.json`: actual0,
  expected0, matched=true. Receipt SHA
  `49c4cdaefd8c4861e0e0824a74b2911fa64cf1c05cebf1b926596c7b1e15046d`;
  source SHA `ffcf5706219777496270cc99a33c74f68b6b0d9ca9a5001a57125f5f240984a4`,
  identical to owned `fixtures/m1-floating-direct-rounding.cpp`. This records
  exact direct-decimal rounding distinct from passing through rounded double.
- `evidence/native-floating-contract-udl-exponent-round1.json`: actual0,
  expected0, matched=true with reserved-suffix warnings. Receipt SHA
  `bda8cbe045b1474f97f1e5dccf189ec30530993cb728bce7ed64c6d56cec09be`;
  source SHA `ec47a3289201d7fb4d7c057dfb4afbeffe9d0c71e231e51677722e80b7debd74`.
  Real cooked literal operators e/e_tag/E_tag prove `1.0e==7`, `1.0e_tag==9`,
  `.1E_tag==11` and support FLT-01 lexical taxonomy; this module still does not
  perform literal-operator lookup or expose their semantic result.

All six `evidence/native-floating-contract-<name>-round1.json` explorations below
have actual2, expected0, matched=false and are explicitly EXCLUDED from pass and
language-negative counts: subnormal-direct-float, subnormal-direct-double,
subnormal-edg-float, subnormal-edg-double, subnormal-classification,
subnormal-scaled. The initial minnormal-success summary was immediately
retracted and is superseded by the actual receipts and accepted half witness.
Their failed assertions are historical disproved expectations, not successful
invalid-language controls or permission to claim blanket numerical parity.

Only root-authorized decoder comments, numeric tests, the existing subnormal
fixture and this handoff changed during this qualification. Shared
FloatingValues.cs, decimal/conversion/arithmetic algorithms, Program, central
files and main checkout remain unchanged. Supervisor-held execution and
independent review still must establish actual passing assertions.

## File inventory / merge rules / remaining work

New files: FloatingValues.cs, FloatingLiterals.cs, FloatingValueTests.cs, eleven
fixtures above and this handoff (15 new files). One existing file receives only
the19 Program test registrations. Merge the new files additively; apply those
registration lines as an additive hunk to the supervisor's current Program.cs.
Do not replace Program.cs or any central file from this older base.

No edits to Types.cs, SemanticGraph.cs, Parser.cs, Expressions.cs, ConstantEvaluation,
storage/arrays, Tokens.cs, Unicode/string/PP/provider files, campaign ledgers,
main checkout or Git metadata. Source correspondence and support claims here
are handed to the supervisor rather than changing shared ledgers independently.

Still required after supervisor validation/review of this checkpoint:

- M1.3: actual numeric-token/UDL/profile/diagnostic integration, longdouble80 and
  remaining extended/imaginary/fixed-point/dialect branches, full literal surface.
- M2.2/M2.5/M3.2: floating canonical types/declarations/conversion graph, usual
  arithmetic, overload ranking, narrowing and all expression/type rules.
- M4.7/M4.8: real payload-aware constexpr/storage/reference/call/return execution,
  template replay and required-constant legality; no integer-fold substitution.
- M5.6/M5.12: remaining target formats/ABIs/layout/endian, rounding/exception
  modes, native branch qualification, extended types and pinned dialect audit.
- M6: serialized Release/JIT/NativeAOT, actual matching snapshots, independent
  produced-code review, graph/native differential corpus, product resource/
  cancellation/performance/platform/lifecycle qualification. New code is not yet
  built, tested, independently reviewed, accepted or committed.
- Every other open/partial PLAN M0–M6 row remains required. Main strings and
  separately accepted scalar control flow must be preserved during integration.

The next campaign step after this held foundation is actual primary-owned
floating parser/type/conversion/evaluator integration plus the next owned
extended-format work. This handoff is a review checkpoint, not full-plan
completion or a request to stop the continuous campaign.

## Held nonself owned-file fingerprints

SHA-256 paths relative to this worktree's edgcpp/manual-csharp directory.
The15 entries cover every owned source/test/fixture and the existing Program
registration file. This handoff is excluded from its own map. Its final hash
is reported separately to the supervisor after this last edit.

```text
95c4815b5e66a6b41dcce7f7477386f18ed2acf6f081f946b6e289d06e472b6b  src/EdgSharp/FloatingValues.cs
0e185844696a443cf8302122712aa0235853e4a07eca6514b2eeb5735bd35d8a  src/EdgSharp/FloatingLiterals.cs
d647f501d24831182c2fff9da17562dc6a28e9f4589c105af24ab99d41cc8219  tests/EdgSharp.Tests/FloatingValueTests.cs
9fa1bfc6424c85b1cf0aa1216c32ecc1a73cc3b1bcc482c71e19121d041f71e6  tests/EdgSharp.Tests/Program.cs
558e071fda1268f74f0d206b5c2d687f3a5b5731033e237abff39a3fdfc183ef  fixtures/m1-floating-conversion-arithmetic.cpp
ffcf5706219777496270cc99a33c74f68b6b0d9ca9a5001a57125f5f240984a4  fixtures/m1-floating-direct-rounding.cpp
b39138845b4fec5f46ca5eeb8b7ad95de77abab04d4586f1ecfdd0d7e13567e7  fixtures/m1-floating-exponent-invalid.cpp
8d24314681345ccda88130c50f4dc6cc0da1e508a037f27de5fe05fe51fb8903  fixtures/m1-floating-format-witness.cpp
9b5bb2f6adb37297b0bbf09972010a6c1e50d12791f2989bc36e98125223d2d2  fixtures/m1-floating-long-double-abi-probe.cpp
08e846df0be5f32fe81e32d92c015ffa80398a49ba3719b9d056607f263cd217  fixtures/m1-floating-overflow-invalid.cpp
4f465cf71fe6e111319ec1325a70f910d2ceb8060446d9b8ea1579b5a7f01340  fixtures/m1-floating-rounding.cpp
8c2b1c20460ccfe909a73d6ef1f3f48c40754e9cfa71f0fdc714dae68c9ed1a5  fixtures/m1-floating-separator-invalid.cpp
ec792ded8c9c80d3f87bf2a1e80e69dc6be7afb22d184a748a3cada3d10444ac  fixtures/m1-floating-signed-zero-bits-probe.cpp
e1b15d6ef1814d67b56769d731992d5367de8a606c56f9635dc6585f1614114f  fixtures/m1-floating-subnormal.cpp
5beb97b331a08c53e496ba84e36781daa2dc04a7fa9ce34561f5954a5e567c09  fixtures/m1-floating-udl-unresolved.cpp
```

Final author actions, including FLT-01 repair and FLT-02 source-defect qualification,
were static source inspection, inventory counting (131
registered cases,19 new), reads of supervisor receipts, authored patches and
SHA-256 reads. No build, test harness, native compiler, generated compiler,
runtime semantic oracle or Git mutation was executed by this worker. No new
file was placed in the main checkout. All16 owned paths, including this handoff,
are now held; no further edits until explicit supervisor release. Source is
ready for validation/review, not accepted or campaign-complete.

**READY / OWNED-FILES HOLD after actual FLT-01 repair and root-authorized FLT-02
qualification. All16 owned paths are frozen. Independent review remains active/
unaccepted. No worker execution/Git or acceptance claim. Full M0–M6 and the next
floating/parser/evaluator assignments remain required under the continuous
campaign.**
