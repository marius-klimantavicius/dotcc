# Independent floating foundation review — isolated round 1

Status: **ACCEPTED ONLY AS THE BOUNDED ISOLATED BINARY32/BINARY64 NUMERIC
FOUNDATION** on the final held bytes identified below. FLT-01 is closed by an
actual decoder repair and matching JIT/NativeAOT execution. FLT-02 is closed
through root's explicit narrow pinned-native-defect exception, independent
integer/rational proof, correct managed assertions and matching execution.
No known unresolved critical/high source finding remains in this reviewed scope.

All four final receipts have identical 125-file maps matching current bytes;
Release is warning/error-free, JIT and Linux-x64 NativeAOT each execute 131/131
groups with zero failures/skips, and all three actual artifact fingerprints
match. Eleven authored native proposals yield nine expectation matches and two
explicit exclusions. Six earlier disproved native hypotheses remain excluded.

This accepts no main frontend floating support, blanket EDG floating parity,
long double, other formats or full-plan milestone completion. All integration
and remaining M0–M6 work stays required. The sections below retain the initial
provisional observations and subsequent closure history; the final isolated
acceptance section governs current status.

## Scope and authority

Reviewed the main campaign `AGENTS.md`, `PLAN.md`, `PROGRESS.md`, `SUPPORT.md`
and `docs/FEATURE-INVENTORY.md`; the isolated early handoff; actual authored
`FloatingValues.cs`, `FloatingLiterals.cs`, `FloatingValueTests.cs`, the 19
floating registrations in `Program.cs`, and all 11 `m1-floating-*.cpp` fixtures.
The isolated worktree is
`/home/marius/.t3/worktrees/dotcc/t3-95580a51/edgcpp/manual-csharp`, with stated
branch `t3/floating-literal-foundation` and base
`6c3848ca5f50c5e06f0e8a6fbfc07bfbfd5ef685` (accepted 112-group Unicode checkpoint).
No Git command was run to independently verify these stated branch/base values.
Later main string/control-flow work is outside this isolated foundation.

The reviewer performed source/data/receipt reads and SHA-256 inspection only.
No source edits, builds, test execution, native execution, Git operations or
subagent dispatch occurred. This report is the sole reviewer-written file.
Material observations were sent to parent/root through T3 for supervisor relay.

This foundation does not implement frontend floating literals, C++ floating
types, usual arithmetic conversions, overload ranking, storage or constexpr
evaluation. Its integration and long-double/other-format implementation remain
required by the full plan. The numeric API's ability to represent infinities
and NaNs is not a rule accepting their production in a C++ constant expression.

## Findings

### FLT-01 — high: decimal e/E can start a valid unsupported UDL suffix

**Closed in the final held version; original source-confirmed finding follows.**
`FloatingLiterals.cs:83–115` in the original version
commits an `e`/`E` after a decimal floating significand to exponent scanning and
returns `Malformed/MissingExponent` when no exponent digit follows. This occurs
before suffix classification and validation at lines 120–141.

For example, `1.0e`, `1.0e_tag` and `.1E_tag` have a decimal floating base plus
an identifier ud-suffix. The foundation's advertised lexical contract requires
`ValidUnsupported`, suffix `UserDefined`, no numeric value, and suffix offsets
3, 3 and 2 respectively. The active implementation instead returns malformed.
This observation concerns lexical classification; it does not assert that an
unresolved literal operator is semantically valid in a translation unit.

Pinned `lexical.c:12461–12482` explicitly records the exponent marker as a
possible ud-suffix and restores that position when exponent digits are absent
and a decimal point was seen. `lexical.c:12824–12830` preserves that suffix for
UDL handling. The current malformed/unsupported test groups omit this boundary.

Required closure: author-owned repair preserving the entire identifier suffix;
direct status/kind/offset assertions for the examples above; malformed controls
`1.0e+`, `1.0e-`, `1.0e'1` and invalid suffix tails; and retention of required
hexadecimal-exponent rules. Supervisor-owned exact native controls must
distinguish literal-operator lookup from missing-exponent rejection. Finally,
review the repaired held bytes and matching executed JIT/NativeAOT assertions.
No repair is implemented by this reviewer.

## Numerical source inspection

No additional concrete numeric implementation defect has been identified so
far. This is an inspection result, not broad algorithm acceptance.

- The only constructible formats are immutable binary32/binary64 descriptors;
  values retain exact `ulong` payloads. Classification distinguishes zero,
  subnormal, normal, infinity and NaN. Numerical equality deliberately differs
  from representation equality, including signed zero and NaN payloads.
- `RoundRational` computes the floor binary exponent from bounded integer bit
  lengths plus an exact comparison, chooses normal/subnormal spacing, and uses
  one quotient/remainder rounding decision. Comparing the remainder with
  `denominator - remainder` avoids doubling a remainder. Ties use quotient
  parity; carry renormalizes before the final overflow check. Subnormal spacing
  is selected before rounding, including a carry to minimum normal.
- Exact zero preserves its supplied sign without Tiny. A nonzero value lost to
  zero carries Tiny/Underflow/Inexact. Exact subnormal carries Tiny alone;
  tininess is explicitly detected after rounding. These flags must not be
  mechanically treated as errors by later C++ evaluation.
- Add/subtract align exact significands; multiply retains the full product;
  divide uses an exact rational. Same-format operands are mandatory and mixed
  formats throw, leaving C++ type/rank selection to future integration.
  Nearest-even cancellation yields positive zero except two negative zeros;
  multiplication/division use the sign XOR rule.
- Nonfinite operation branches distinguish opposite infinities, zero-times-
  infinity, zero/zero, infinity/infinity and finite division by signed zero.
  NaNs are quieted and signaling inputs flag InvalidOperation; a same-format
  representation copy deliberately preserves a signaling NaN. Cross-format
  payload conversion is an explicit deterministic policy, not proven EDG
  payload parity. Comparison is unordered for either NaN and equal for ±zero;
  signaling/constant-context policy remains with evaluation.
- Integer conversion truncates before signed/unsigned representability checks,
  permitting -0.75 to unsigned zero and -128.75 to signed-eight-bit -128.
  Nonfinite/out-of-range results have no fabricated integer. Negative endpoints,
  unsigned upper bounds and width limits were inspected explicitly.
- Token, digit and exponent counts are checked before substantial work. The
  saturating exponent cannot be canceled by the bounded significand scale;
  power/shift budgets precede `BigInteger.Pow` and large scaling. Relevant
  exponent arithmetic is bounded or widened to `long`; input traversal is
  iterative. Numeric code retains no mutable AST/TU/session cache or host
  floating parser/arithmetic. UDL validation reuses the accepted Unicode module
  under the token bound. New numeric decisions use ASCII/integer operations,
  independent of the process culture.

The authored 19-group suite is materially more than toy examples: fixed
independent expected bits cover decimal/hex halfway directions, sticky tails,
carry, overflow thresholds, half-minimum subnormal, normal crossover, a direct
decimal conversion that discriminates double rounding, signed zeros, integer
boundaries, nonfinite flags, grammar/error offsets, bounded hostile inputs and
concurrent reuse. The 512 finite widening round trips and small integer round
trips are useful invariants, but do not independently prove arithmetic or
decoder correctness. All groups remain unexecuted from this review's evidence
at this provisional stage.

## EDG correspondence inspected

Read actual upstream branches, rather than relying on routine names alone:

- `literals.c:915–1140`, `conv_float_literal`: suffix/type selection,
  imaginary/extensions/dialect branches, separator removal, decimal versus hex
  conversion, range diagnostic versus hexadecimal inexact warning.
- `lexical.c:12380–12505,12798–12891`, `scan_number`: fractional/exponent scan,
  exponent-marker/UDL ambiguity, suffix validity and literal-operator boundary.
- `floating.c:552–595,2490–2586`: half-tail comparison, parity and exact
  powers-of-five/two plus half-ULP decimal adjustment. The new direct rational
  quotient/remainder approach is an implementation adaptation; it does not port
  EDG's approximate-first algorithm or whole source files.
- `float_pt.c:2031–2139,2217–2320,2712–3006`: halfway/sticky/carry,
  hidden-bit/subnormal scaling, range handling, decimal per-format internal
  conversion versus `USE_HOST_FP_CONVERSION_ROUTINES`, and GNU/MS differences.
- `float_pt.c:3687–4085` and the corresponding declarations/mantissa structures
  in `float_pt.h`: signed/unsigned conversion, operation/rounding dependence,
  separate negation for IEEE zero, and unordered comparison.
- `float_type.h:197–236,524–654,1426–1491`: explicit precision/hidden-bit
  definitions, biased exponents, split/make representation and return statuses.
- `folding.c:630–701,4301–4521`: truncate/signedness handling, same-kind operands,
  arithmetic errors, configurable IEEE exception handling on division, unordered
  predicates and `depends_on_fp_mode`. These are future evaluation obligations.
- `targ_def.h:844–984,2311–2358` and `floating.h` format/precision/hidden-bit
  definitions: source configuration alternatives are separate from the running
  oracle's witnessed target. No host ABI was inferred from defaults.

Reviewer-observed SHA-256 values (main `edgcpp/upstream/src/`):

```text
de5276f87f073a67bb4b7c6a4cd152a66eab0a155fda84f0d991e3d296213d95  literals.c
e987ffcb7645931564f3a6f262d089c21524e78ce2c60d5e0e0868a5b4b98875  lexical.c
7f627e2aa3f3c1ee75ee58529eeceb95f1db71caee06ded0cb5615a1bd31b45a  floating.c
cb448268eb224ba4dfa21894d7fa66c4c94c39c94247f13623bb317483160b59  floating.h
ac39be169ec49270b762dc03ed6e41087709fde15698434b2c124f5a383c376a  float_pt.c
3ae9427a552c5c0957175f958420d780334466b22672db60ea8b131195e885ba  float_pt.h
44b9d368c0f7d3b0d88debe60c95d693154df76837b7786426d5ac4cc2232bfb  float_type.h
91fef22e8578541a8f7211ce401b7f15a3acd55aa42bed2815c254237cf045ff  folding.c
e39ed09cdd9d150092231dce1d2b39032ffd442efe57ced0ad7636e12cf1958b  targ_def.h
b1bdf56f2fa73f1f3fdb62f5bcf296c55a39f4350ad9d80467c6253a2365330e  target.h
```

The author routine/SHA/omission inventory and nonself owned-file manifest were
still promised, not present in the early handoff. Reconciliation against that
inventory remains required before acceptance.

## Existing native evidence actually read

Read the exact three main inputs under `tools/oracle-inputs/m1-floating-contracts`
and `evidence/native-floating-contract-*-round1.json`, including commands,
diagnostics, input hashes and status expectations. Their recorded input paths
are in held Unicode worktree `t3-96dc97a5`; the main copies independently hash
identically. Current executable SHA-256 matches all three receipts:
`e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3`.

The actual invocation includes `--clear_flag use_predefined_macro_file
--c++20 --strict --no_code_gen --instantiate used --unicode_source_kind UTF-8`.
The independent `profile.cpp`/`native-profile.json` establishes the baseline
C++20/LP64/signed-char facts under the same executable; that older profile
receipt does not itself include the later instantiation/Unicode flags.

- `format-values.cpp`, SHA
  `fcd4a64bbb902dcd2704fb8c567e0f987c49c290740d8f053f7931683286b246`:
  accept 0. Actual assertions establish float/double/long-double sizes 4/8/16,
  the listed binary32 ties in both directions, binary64 integer halfways in
  both directions, positive minimum float subnormal, long-double precision
  discriminated at 2^63/2^64 and a 2^16383 value. Hex inexact warnings are
  retained. These do not establish full object encoding/alignment or every
  arithmetic branch.
- `underflow.cpp`, SHA
  `a4f693231fa7b8a125ce17177c9fe727c27aaab55cee92ebf99510bf99f664ac`:
  accept 0 for `0x1p-150f == 0.0f`, with the exact inexact-value warning.
  Underflow must not be blanket-rejected by future integration.
- `overflow.cpp`, SHA
  `218e4ebe1b4a8e762a7c44a45af12d7b547ecd28b2ee8628f7043f55cfe6be9b`:
  language rejection 2 for `float value=1e1000f`, diagnosed out of range.
  A decoded infinity plus Overflow is not frontend source acceptance.

No other native fixture acceptance is inferred. The author fixtures explicitly
remain proposals, including builtin signed-zero bit-cast availability.

## Initial qualification controls and acceptance gate (historical)

1. Close FLT-01 with produced-code review and actual matching executions.
2. Obtain explicit author OWNED-FILES HOLD and the promised nonself manifest;
   reconcile all allowed files/Program additions, upstream inventory and hashes.
3. Audit supervisor-owned warning-free Release, complete managed and Linux-x64
   NativeAOT runs against identical current source/data/tool maps and actual
   artifact hashes. The expected current inventory is 112 base + 19 additions
   = 131, subject to a final held registration audit. No main 120/136-group
   receipt qualifies this different isolated checkout.
4. Read exact native receipts for the authored positive/negative fixtures and
   additional discriminating boundaries; preserve mismatched expectations as
   excluded evidence. In particular inspect single rounding versus EDG staged
   conversion: `0x0.fffffeffffffp-126f` is just below the max-subnormal/min-normal
   midpoint and should yield bits `0x007fffff` in this mathematical nearest-even
   foundation. The analogous binary64 below-midpoint and decimal
   `1.0000000596046447753906250000000000000001f` also need explicit outcomes.
   `float_pt.c:2751–2776` rounds before denormalization, and its host decimal
   branch can introduce an intermediate representation. These are source-backed
   parity questions, not claimed native failures. Do not silently weaken the
   exact expected bits or declare an EDG anomaly without executed evidence.
5. Keep value flags separate from future C++ required/optional constant-context
   acceptance, diagnostic severity, IEEE mode and signaling-NaN policy. Full
   frontend integration and additional formats remain mandatory later work.

No foundation or campaign acceptance is granted by this provisional report.
If this delegated review round ends before the final held evidence, a later
independent round must be a fresh `delegate_task` with the full brief, FLT-01,
responses and unresolved qualification controls.

## First held-handoff audit during this active round

The author subsequently published READY / OWNED-FILES HOLD. Read the final
routine/SHA/omission inventory, integration and exception/resource contracts,
test/fixture inventories and nonself manifest. Independently matched all 15
owned-file entries and all 13 upstream/config entries with current bytes, with
zero mismatches. The handoff SHA at this inspection is
`ea3476b7cceaa956694ea1228195cc59c40166300fc660780071547a23ed7c2a`.
This identifies that first handoff; it is not a later repair's hold authority.

Compared the isolated files with all 111 entries in the accepted Unicode held
snapshot: none missing; only `tests/EdgSharp.Tests/Program.cs` differs. Its
current source contains 131 registrations, including exactly 19 floating
registrations. This supports the promised absence of central parser/type/
evaluator/string/control-flow edits without using Git. It is not evidence that
the new registrations have run.

Read the additional actual source/configuration branches in `defines_linux.h`,
`config/base.cmakedef`, `config/cpfe.cmakedef` and
`backend/target/portable-lp64-binary80-v2.def`. The last file describes a separate
backend target and is not treated as runtime manual-frontend configuration.
The first held value source now explicitly restricts `FloatingResult.IsExact`
to finite values; that refinement was inspected directly.

**FLT-01 remains open on this first held snapshot.** The final handoff documents
the exponent-marker UDL branch as omitted and proposes future caller
disambiguation. The current Decode API already classifies other unknown
identifier suffixes as valid unsupported UDLs and advertises malformed versus
valid-unsupported lexical outcomes. Documentation of this one omitted branch
does not make `1.0e_tag` malformed or meet the requested decoder contract.
This objection was relayed to root. No numeric or foundation acceptance is
granted; matching new execution receipts are still outstanding.

## Relayed native boundary evidence, correction and root policy

Independently read the subsequent nine exact native receipts and input copies:
`native-floating-contract-{subnormal-direct-float,subnormal-direct-double,
subnormal-edg-float,subnormal-edg-double,subnormal-classification,
subnormal-scaled,subnormal-half,decimal-direct,udl-exponent}-round1.json`.
All nine main input-copy hashes match the receipts. Commands use the explicit
strict C++20/LP64/used/UTF-8 profile and the same executable SHA recorded above.

The four initial subnormal equality expectations **all failed** with status 2,
`matched=false`. In particular, the two `subnormal-edg-*` min-normal hypotheses
did not succeed. The supervisor immediately corrected the mistaken initial
success summary; this report does not use it. Classification and scaled
expected-accept inputs also failed, for **six excluded exploratory mismatches**
total. These are disproved positive expectations, not successful negative
language tests and not evidence of any replacement numeric result by themselves.

Actual positive proof is `native-floating-contract-subnormal-half-round1.json`,
accept 0 with the inexact warnings retained. Its exact input SHA is
`d63138a496fac4b37f9d966c9d3b5bd1eb2949da39217ac2592f22bdfeec6bf8`:

```cpp
static_assert(0x0.fffffeffffffp-126f == 0x1p-127f);
static_assert(0x0.fffffffffffff7ffffp-1022 == 0x1p-1023);
```

### FLT-02 — high qualification issue: proved pinned hex carry/reset defect

The initially authored exact decoder differs from these actual pinned native
results. Source inspection through **storage**, not merely staged rounding,
explains the discrepancy. `check_and_denormalize_hex_fp_value` at
`float_pt.c:2282–2285` rounds with `mant_dig - implicit_bits`.
`round_hex_fp_value:2132–2134` can retain the shifted mantissa and increment
the exponent on carry. `check_and_denormalize_hex_fp_value:2290` then
unconditionally resets that exponent to `min_exp - 1`.
The carry leaves top mantissa word `0x80000000` and zero lower words.
`store_hex_fp_value:2513` packs `0x80000000 >> 9` with a zero exponent field,
giving `0x00400000` (2^-127). Lines 2525–2528 analogously pack binary64
`0x0008000000000000` (2^-1023). This is the carry/reset numerical defect proved
by the positive witness; merely replacing direct rounding with conventional
two-stage nearest rounding would not explain that result.

Independent exact nearest-value proof for the **same** input spellings:

- Float subnormal unit `u = 2^-149`. The 12-digit hex integer
  `N = 0xfffffeffffff = 2^48 - 2^24 - 1`; therefore
  `x/u = N/2^25 = (2^23 - 1) + 1/2 - 2^-25`.
  Distances to max-subnormal `M = 2^23 - 1` and min-normal `M+1` are
  `1/2 - 2^-25` and `1/2 + 2^-25` units. The unique nearest payload is
  **0x007fffff**, not either half-min-normal or min-normal.
- Double subnormal unit `u = 2^-1074`. The 18-digit hex integer
  `N = 0xfffffffffffff7ffff = 2^72 - 2^19 - 1`; hence
  `x/u = N/2^20 = (2^52 - 1) + 1/2 - 2^-20`.
  The corresponding distances are `1/2 - 2^-20` and `1/2 + 2^-20`, proving
  unique nearest payload **0x000fffffffffffff**.

Root explicitly authorized a **narrow intentional adaptation**: retain correct
exact-rational target semantics for this proved pinned hexadecimal carry/reset
defect; do not reproduce the factor-of-two exponent error, mark the valid
literal unsupported, or alter shared decimal/conversion/arithmetic rounding.
This is not blanket floating parity and does not authorize ignoring other
differences. FLT-02's policy question is resolved by that explicit decision;
qualification closure still requires exact direct managed assertions, author
provenance/handoff reconciliation, fresh HOLD and matching executions.
No source change is required to the mathematically correct `RoundRational`
algorithm merely to imitate this defect.

Two other new receipts are genuine accept-0 proofs:

- `decimal-direct`, input SHA
  `ffcf5706219777496270cc99a33c74f68b6b0d9ca9a5001a57125f5f240984a4`,
  is identical to the authored direct-rounding fixture. Its assertions
  distinguish direct decimal-to-float from conversion of the rounded double.
  No universal decimal or host-configuration claim follows from this one case.
- `udl-exponent`, input SHA
  `ec47a3289201d7fb4d7c057dfb4afbeffe9d0c71e231e51677722e80b7debd74`,
  defines real cooked literal operators e/e_tag/E_tag and checks results
  7/9/11 for `1.0e`, `1.0e_tag`, `.1E_tag`. Reserved-suffix warnings are
  retained. This independently confirms FLT-01's lexical interpretation.

The author repair now visible at `FloatingLiterals.cs:116–125` preserves the
decimal exponent-marker position when the decimal point already supplies a
floating core, no exponent digits follow and no sign intervenes. It then
reuses the whole-suffix identifier validator. Paired tests check status, suffix,
issue and offsets, and retain signed/misplaced-separator/hex malformed controls.
This source repair is consistent with FLT-01, but is not execution closure.

Independently inspected the supervisor's new
`evidence/floating-subnormal-qualification.md`. Its numeric values, exact
integer fractions/distances, six excluded mismatches, source/compiler hashes
and authorized policy agree with the reviewed sources and receipts. Requested
one precise source-description correction through root: conversion at
`float_pt.c:2754–2758` shifts left to remove the explicit leading one and make
it implicit; denormalization at 2271–2273 subsequently restores it explicitly.
The carry's exponent increment is at 2134, so the relevant retained-shift range
is 2132–2134. No direct edit to the supervisor-owned document was made.
The author must also assert the exact witnessed binary64 `...f7ffff` spelling,
not rely solely on a different below-midpoint tail. Fresh HOLD and executed
closure are still awaited during this same active review round.

## Fresh repaired HOLD and initial actual execution audit

Supervisor relayed author terminal/HOLD after FLT-01 and FLT-02, recorded in
isolated `evidence/floating-isolated-held-snapshot.json`. Independently matched
the fresh handoff SHA
`36f43e26121a0401c06210654e82a14a25d8333be381304faa442d996529f234`,
all 15 owned hashes, all 13 handoff source/config hashes and all 111 baseline
entries (only permitted Program registrations differ). The handoff now states
the new HOLD unambiguously; the original `ea3476...` hold is superseded.

Repaired held code includes both exact native-probe spellings with correct
expected max-subnormal payloads and Tiny/Underflow/Inexact flags. The test also
constructs the independent exact 48/72-bit input integers, verifies distances
and midpoint equations, then calls `RoundRational` for N, N+1 and N+2 with
fixed expected below/midpoint/above payloads and flags. This does not duplicate
the rounder's normalization/DivRem implementation. The value algorithm remains
unchanged at SHA `95c4815b5e66a6b41dcce7f7477386f18ed2acf6f081f946b6e289d06e472b6b`.
The decoder has the actual FLT-01 repair and narrow-exception source comment;
the handoff explicitly retains root policy, positive witnesses and exclusions.
The requested main qualification-note hidden-bit correction is now present.

Read actual isolated `floating-isolated-build-round1.json` and
`floating-isolated-tests-round1.json`, not just the supervisor's counts:

- Release command is `dotnet build EdgSharp.slnx -c Release --nologo`, with
  isolated cwd, exit 0, 0 warnings/errors and empty stderr.
- JIT command is `dotnet tests/EdgSharp.Tests/bin/Release/net10.0/EdgSharp.Tests.dll`,
  with the same isolated cwd, exit 0, all 131 actual PASS lines matching the
  current ordered registrations, and summary 131 passed / 0 failed / 0 skipped.
  All 19 floating groups, including FLT-01 suffix taxonomy and FLT-02 exact
  subnormal/midpoint assertions, execute successfully.
- Independently hashed every entry of both identical 125-file maps against
  current bytes: zero mismatches. All 15 owned entries match the fresh held map;
  the handoff still matches its separately recorded hold hash.

FLT-01 and FLT-02 now have repaired/qualified held source plus actual JIT
coverage. Final closure and acceptance still await matching NativeAOT, actual
artifact fingerprints and the supervisor's separate 11-fixture native audit.
No reviewer test/build/native execution occurred.

## Final isolated acceptance and receipt/artifact audit

Final authority is the unchanged repaired handoff SHA
`36f43e26121a0401c06210654e82a14a25d8333be381304faa442d996529f234`
and isolated `floating-isolated-held-snapshot.json`, author terminal/HOLD with
all 16 owned paths frozen. The reviewer independently rechecked this after
all four executions. Its 15 nonself owned hashes agree with every execution
map; no held source changed. The existing accepted Unicode baseline remains
intact apart from the additive Program registrations. No main string/control-
flow implementation was imported or credited to the 112+19 isolated inventory.

Read all four actual round-1 receipts, including commands, isolated cwd,
stdout/stderr, exits, summaries and complete file maps:

- `floating-isolated-build-round1.json`: Release exit 0, 0 warnings/errors.
- `floating-isolated-tests-round1.json`: direct-dotnet JIT exit 0,
  131/131 actual ordered registration names pass, zero failures/skips.
- `floating-isolated-nativeaot-publish-round1.json`: explicit
  `linux-x64 -p:PublishAot=true`, output
  `tests/EdgSharp.Tests/bin/aot-floating-isolated`, exit 0 and empty stderr.
- `floating-isolated-nativeaot-tests-round1.json`: runs that exact published
  executable, exit 0, the same 131 ordered names pass, zero failures/skips.

All four source/data/tool maps are identical: 125 entries each, every entry
independently SHA-256 checked against current bytes with zero mismatches.
The maps retain all three Unicode data/license/provenance inputs and all three
oracle/receipt tool inputs. Main copies of all four receipts, the final hold
and the artifact receipt are byte-identical to their isolated originals.

Independently read and hashed all three actual output files listed in
`floating-isolated-built-artifacts.json`; sizes and SHA-256 match exactly:

```text
1416704  1212b13c915c691cc831a17c7284338564de25e64350eec340290d10b609a7d0  src/EdgSharp/bin/Release/net10.0/EdgSharp.dll
 269824  af913538047f3ffa82deff83c978b90be4a35a5404b6d630098cfa4120be56a0  tests/EdgSharp.Tests/bin/Release/net10.0/EdgSharp.Tests.dll
5639960  40f84c049321c8bb97a7542d7b8859e5fb05c236c5035bd035dabb38eaf97f41  tests/EdgSharp.Tests/bin/aot-floating-isolated/EdgSharp.Tests
```

Read all 11 `native-floating-isolated-<name>-round1.json` receipts and their
actual held fixture inputs. Verified each input hash, same explicit pinned
profile/compiler SHA, expected and actual normal/error status, additional-input
list and main/isolated receipt-copy identity. Actual results are:

| Fixture name | Result and qualification |
| --- | --- |
| conversion-arithmetic | accept 0, matched; the authored finite conversions/operations |
| direct-rounding | accept 0, matched; direct decimal versus rounded-double discriminator |
| format-witness | accept 0, matched; exact listed size/type/precision assertions |
| long-double-abi-probe | accept 0, matched; native-only size/precision/range/minsub facts, no managed long-double implementation |
| rounding | accept 0, matched; authored decimal/hex ties/carry/separators, four inexact warnings retained |
| exponent-invalid | language reject 2, matched; extra numeric text diagnosed for `0x1p+` |
| overflow-invalid | language reject 2, matched; out-of-range floating constant |
| separator-invalid | language reject 2, matched; misplaced digit separator |
| udl-unresolved | language reject 2, matched; literal operator not found, not malformed suffix |
| signed-zero-bits-probe | expected accept, actual 2, **excluded**; 17 unavailable/unsupported `__builtin_bit_cast` name/grammar errors, not a numerical assertion failure |
| subnormal | expected accept, actual 2, **excluded**; five failed target-correctness assertions on lines 8,9,12,13,14, with inexact warnings retained |

The five subnormal failures are the two midpoint controls and three below-
midpoint tails at the same normal/subnormal carry/reset path inspected above.
The exact two independently witnessed half-value inputs anchor that defect;
no claim that an arbitrary other numerical difference is explained by it is
authorized. The three additional crossover controls remain correct direct
managed assertions, and no excluded fixture is counted as a successful native
negative or a skipped managed test. Native signed-zero bit-cast qualification
remains an explicit oracle capability gap; managed sign/payload/operation tests
execute in both runtimes and are not presented as native bit-cast parity.

The complete floating evidence inspected comprises 12 design-contract receipts
and 11 authored-fixture receipts: **15 expectation matches (10 accept-0,
5 reject-2), 8 exclusions (six disproved design expectations and the two
fixture proposals above)**. The historical independent LP64 profile receipt is
separate. No catastrophic status, unexecuted proposal or unsupported builtin
is counted as a numeric/language pass.

**FLT-01 closed:** repaired decimal exponent-marker/UDL fallback follows the
specific EDG lexical branch; exact kind/status/offset and malformed complements
pass JIT and NativeAOT on matching held bytes; the native cooked-UDL positive
independently proves the lexical interpretation.

**FLT-02 closed with the explicit narrow adaptation:** positive native halves,
actual carry/reset/packing source, exact nearest-distance proof, author source
comment/handoff, supervisor qualification note and both-runtime direct
assertions establish the reviewed distinction. Correct shared rational,
decimal, conversion and arithmetic algorithms remain intact. This is an
authorized deviation from the proved pinned hexadecimal defect, not a native
parity claim. The corrected main qualification note was independently read;
its current SHA is
`89f501819998a0102b801cafe73c2cde85da830f3f47e15e2658fd8741611a4d`.
A minor old line-range shorthand `2128–2132` remains in the frozen author
handoff; the actual exponent increment is at 2134 as correctly recorded in
the reviewed main qualification note and this report. This does not change
the source proof, policy, executable behavior or acceptance.

**Bounded isolated acceptance granted.** No unresolved critical/high finding
is known in these reviewed bytes. Numeric NaN payload policy, fixed nearest-
even rounding and specified resource limits are explicitly bounded; alternate
formats/modes, full native diagnostic/exception parity and broader corpus/
platform qualification remain required. Actual source-to-graph floating type,
literal, conversion, overload, array/storage/reference, template and evaluator
integration must receive separate combined held-snapshot execution and
independent review. Long double and all other full-plan gaps remain required.
No production floating-language support or campaign completion is claimed.

The reviewer ran no compiler, tests, native tool, Git operation or subagent and
changed only this report. This active review round now ends; a subsequent
independent review round requires a fresh `delegate_task` with this brief,
findings, closure evidence and unresolved qualification bounds.
