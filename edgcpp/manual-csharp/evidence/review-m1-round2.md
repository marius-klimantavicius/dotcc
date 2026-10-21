# Independent M1 early execution and assertion review

Date: 2026-10-21. Reviewer: `/root/manual_edg_supervisor/review_m3`.
**Not final acceptance.** Production source was inspected read-only; the
supervisor executed all builds/tests/native processes and released edits after
the initial held run. Reviewer writes only these review reports.

## Executed early snapshot

Read `m1-build-round1.json` and `m1-tests-round1.json` in full. Both record the
same 44-file source/test/fixture hash map. Release build exits 0 with zero
warnings/errors. Managed execution passes 56/56 groups, zero failed/skipped:
the prior 48 groups plus eight actual preprocessing groups. Those assertions
are in `tests/EdgSharp.Tests/PreprocessingTests.cs`; this is real source/semantic
execution, not a native fallback or a parser-cache-only gate. This does not
establish the later edited snapshot, NativeAOT, full M1 or campaign completion.

## Source repairs and direct assertions inspected

Canonical M1 IDs are defined in `review-m1-round1.md`; qualifications and
disproved hypotheses there remain part of this history.

- M1-01/02: the exact spliced `b` subrange asserts its sole contributor and
  column, exercising both half-open exclusion and clipped logical mapping.
  Empty/EOF/adjacent zero-width map controls are still required.
- M1-03: an in-memory `/root.cpp` resolves `/header.h` without other roots.
- M1-04/06: unused/stringized malformed arguments and stringized VA_OPT
  empty/nonempty results pass; the extended upstream VAOPT matrix is pending.
- M1-05: F/ID/G histories, physical definition kinds, expansion anchor and
  spelling line remain queryable after the helper disposes its session.
- M1-07: a parsed binary range retains A and B expansion histories. Source now
  retains every covered origin and traverses the include-origin graph for notes;
  direct multiple-include-activation/range assertions remain required.
- M1-08: unused prefixed and inactive prefixed/raw tokens no longer emit the
  premature unsupported diagnostic. Active literal semantics are still bounded.
- M1-09/10/11: renamed parameters, invalid VAOPT paste edges and missing object
  name whitespace are rejected. Compatible redefinition retains first origin.
- M1-12: hexadecimal filename decoding has a direct assertion. Source also
  reencodes control characters for `__FILE__`; full slash/quote/control and
  repeated-activation logical-location assertions are pending. Nonordinary
  filename behavior is explicitly bounded, including native prefix anomalies.
- M1-13: multiline comments and spliced-line presumed locations pass. **Residual
  source defect:** raw multiline token content still changes `LogicalLine` for
  following tokens. The macro below is split before `+ 7` by `Tokenize`'s
  token-start-line grouping. This must close before claiming that raw pp-token
  replay within directives works.
- M1-14/15/16/17/18: sign-bit/negative shifts, alternative operators and macro
  eligibility, strict20 warning rejection, rescan whitespace matrix and the
  spliced-comment/raw context have direct passing groups. Macro suppression
  tests also cover mutual recursion, F(F(7)), F(F)(7), alias/function tail and
  include-entry/return barriers.
- M1-19 remains **a local closure invariant / compatibility qualification**.
  The test rejecting a no-newline malformed unused raw token is not native
  proof of unused-definition lexical rejection. Native's matching original
  reject diagnoses EOF newline policy only; newline-terminated unused form
  accepts. This cannot be counted as broad raw-macro semantic parity.

Residual M1-13 exact source (newline inside the raw token):

```cpp
#define X R"(a
b)" + 7
value X
```

Expected ordered output contains `value`, the one raw string token, `+`, `7`.
Group by newline in actual inter-token input trivia, ignoring newlines internal
to pp-tokens. Keep physical source positions separately.

## Finite final-gate requirements forwarded

1. Empty source, EOF and adjacent-boundary origin-map assertions.
2. The requested 26-case upstream VAOPT comparison with actual managed output
   and native expected token spellings, preserving explicit case metadata.
3. Paste/stringize assertions for operand/operator spelling contributors,
   expansion invocation/history and retained origin ownership.
4. Distinct activations of the same physical header under different logical
   `#line` state, no header-state leakage into the root, and aggregate range
   include notes retaining both activations.
5. Independent/concurrent preprocessing sessions with conflicting macros and
   include providers. Existing concurrency coverage is macro-free.
6. A changed include/macro presentation stream exercising the accepted M4
   difficult path: frozen definition lookup despite later overload/caller local
   hiding, deduction/instance reuse and ordinary/template selection, constexpr
   required context and cached body origins retained after disposal. The early
   `m1-integrated.cpp` identity function is useful but does not prove that path.
7. Matching held final build/JIT/NativeAOT receipts, native exact fixtures and
   discriminating semantic witnesses, followed by independent actual code and
   evidence review.

The supervisor reports work on these additions. They are pending at this
inspection. Source/literal/encoding/predefined/directive gaps stay explicit in
the feature inventory; this finite gate cannot replace the full active PLAN.

## Subsequent matrix source inspection

Canonical M1-20 records the matrix-discovered substitution/paste ordering defect.
Read the actual revised code: a VAOPT operand is substituted and pasted before
being inserted next to outer paste operators, completed operand tokens cannot
be rebound as parameters, and `PasteOperator` distinguishes a replacement-list
operator from raw argument data or an already-pasted `##` token. This is a real
algorithm change, not a fixture-specific branch. Its final executed assertions
and snapshot still require audit.

The full retained upstream fixture has **32 labeled invocations**, including
fractional/underscore labels, rather than the informal "26-case" shorthand.
Read-only comparison verified identical bytes to
`upstream/tests/tests/imported/clang/cpp/Preprocessor/macro_vaopt_expand.sft.cpp`,
SHA-256 `fd3b0f88387c48b7eedcdb0cf2f4ca23123dbd0c7af183fc996a909ed92b453b`.
The type/options metadata and comments are retained. The native receipt runs
strict C++20 preprocessing; alternative metadata profiles are not claimed run.
