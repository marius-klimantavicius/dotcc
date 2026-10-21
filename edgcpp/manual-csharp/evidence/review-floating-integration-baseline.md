# Floating integration independent baseline

Status: review in progress; **no acceptance**. Main starting checkpoint is
`764d67a16` (136 accepted groups). The numerical foundation in isolated
`t3-95580a51` has its own independent review and is not accepted by this report.
This reviewer reads source/evidence only and runs no build, test, native
frontend or Git operation. Report ownership is `review-floating-integration-*`.

Read local/parent AGENTS, PLAN, actual main `FloatingSemantics.cs`, Types,
References, conversion/overload and storage integration under active editing,
the integration handoff and isolated round-one numerical review. In-flight
partial paths are not alleged as held-snapshot defects.

## Required main gate

- Preserve source-independent canonical float/double identities, const forms,
  structural compound-type comparison and recursive scope-owning cache guards.
  Long double requires its distinct target representation; no double alias.
- Carry an actual typed floating payload through literal nodes, conversions,
  cells, materialization, arguments/results, references, array elements,
  assignments and public observations. Existing integer constant APIs stay
  integer-only. Representation equality is not C++ numeric equality; signed
  zero and unordered comparisons need deliberate handling.
- Float-to-double is a promotion; other supported mixed arithmetic conversions
  have conversion rank. Preserve pairwise candidate comparison, crossed
  ambiguity, frozen lookup, selected entity identity and exact repeated-T
  deduction without arithmetic promotion.
- Convert integer to target format explicitly. Float-to-integer truncates
  toward zero **before** checking destination representability; never wrap an
  out-of-range result. Compound assignment uses the common operation type and
  converts its result to the destination, e.g. integer `3 *= 1.5f` becomes 4.
- Keep numerical range/rounding flags separate from source validity and
  optional versus required constant evaluation. Inexact/underflow alone do not
  justify rejection. Overflowing source literals and invalid required
  operations need source diagnostics, not successful nonfinite placeholders.
  Ordinary const floating globals have no const-integral import privilege.
- Preserve stable actual-object cells, actual-cv readonly flags,
  BeganInEvaluation, single LHS evaluation/RHS-before-LHS, same-cell sequencing,
  nonobject one-past transport versus indirection, alias lifetime, aggregate
  scope cleanup, failure cleanup and fresh evaluation resource counters.
- Preserve authoritative PP tokens, canonical Unicode identity, raw/effective
  spelling and origins, retained replay, journaled links and iterative
  dependency/effect walks. FLT-01 exponent-marker UDL taxonomy must survive
  Lexer and active PP-number routing as well as the isolated decoder.

Acceptance requires an actual difficult source path combining changed-stream
macros/includes, floating template deduction, frozen overload lookup,
references and mutation/control flow in a required constant context, with
typed graph/storage and retained-origin assertions plus discriminating native
witnesses and negative controls. Read actual final code/tests and matching
held Release/JIT/NativeAOT/native receipts, source/data/tool maps and output
hashes. Isolated numerical passes alone cannot establish main integration.

## Actual EDG branches consulted

- `literals.c:915–1140`, `conv_float_literal`: suffix/target-kind selection,
  decimal versus hex conversion, range error and separate hex-inexact warning;
  GNU imaginary, extended kinds and dialect branches remain separately scoped.
- `exprutil.c:11738–11898`, `promoted_float_kind` and
  `determine_arithmetic_conversions_full`: highest supported floating kind,
  explicit operation/result type distinction and conversion from integer.
- `overload.c:3013` onward, `determine_arg_match_level`: reference versus value,
  transformations and argument-match descriptors before comparison.
- `folding.c:600–779`: target-kind integer conversion, truncation and signedness
  followed by destination-range validation; optional folding failure differs
  from mandatory source failure. `4301–4521`: same-kind operations, numerical
  errors, IEEE exception configuration, unordered comparisons and fp-mode facts.
- `interpret.c:27494–27560`: typed integer/floating casts and target-range
  rejection. `30381–30460`: compound floating operation type before destination
  conversion, including integer destination with floating RHS.

The direct exact-rational nearest-even foundation is an explicit managed
algorithm adaptation, not a claim to port EDG's approximate-first decimal
conversion or every floating-mode branch. Source hashes will be reconciled
against the final supervisor ledger. The isolated review tracks the possible
EDG staged-rounding discrepancy at subnormal crossover; main documentation
must preserve executed outcomes and any deliberate exact-rounding adaptation.

The prior finite scalar sequencing detector remains a documented stricter
C++20 adaptation (`evidence/sequencing-qualification.md`), with no assertion
that native accepted probes prove the detector's rejection behavior. Do not
silently widen that claim during floating integration.

Full PLAN, remaining formats and unimplemented language branches remain active.
