# Early template replay and constant-evaluation gate

The held M4 implementation passes the combined source gate and 48 managed/AOT
groups. Independent acceptance is recorded in `evidence/review-m4-final.md`.
This is a difficult-path feasibility checkpoint, not full template/constexpr or
C++ support. All remaining inventory rows stay active after the commit.

## Representation, phases and ownership

`TemplateParameterType` has owner-scoped identity. Dependent unary/binary/call/
conversion nodes preserve deferred semantics separately from ErrorExpression.
Known expression type and known value are distinct: a bool result or concrete
conversion target can still be value-dependent. Queries inspect these retained
facts; they do not silently instantiate new bodies.

Each function template retains its type parameters, source parameter scope,
exact body token interval, immediately analyzed prototype body, copied ordinary
definition binding set, and canonical type-argument-keyed instance records.
Prototype analysis checks nondependent names/calls and unsupported body syntax
even for unused templates. Per pinned EDG CWG2518/P2593R1 handling, a false
static_assert is deferred in the template prototype; malformed/nonconstant
nondependent assertion conditions still diagnose.

By-value deduction uses unqualified actual scalar type identity without integral
promotions; repeated occurrences must agree. Matching/substitution alone does
not instantiate a losing candidate body. The selected instance is published
before body rescanning, retains explicit in-progress/complete/failed state and
reuses its concrete declaration on later calls. A fresh cursor rescans retained
tokens with concrete parameter types and fresh local scopes. Ordinary lookup
uses copied definition-visible entities, so later overloads and caller locals
cannot replace them. Dependent calls requiring unsupported ADL are diagnosed.

Dependent signatures are template-owned and never retained by the session's
scope-free concrete function-type intern table. Repeated analysis under a live
session has direct identity/isolation, count and non-inlined weak-reference
collection tests. When pointers/references/functions/classes gain nested
dependent types, ownership/dependency detection must recurse into components
before session interning; current direct-kind checking is bounded to current
scalar forms.

Replay mutations share the root analysis journal: template links, instance
registration/state/declarations/replay counts, symbols/overload membership,
scopes, definition links and dependent-value flags have explicit undo paths.
Root checkpoints can restore those states. Child replay states explicitly reject
independent Checkpoint/Restore because cross-state lineage is not implemented;
this bound must not be advertised as arbitrary nested speculative support.

## Handwritten algorithm correspondence

Exact consulted source hashes are in `evidence/source-m4.json`. These mappings
are bounded routine branches, not whole-file port claims.

| Authored operation | Upstream routines / relevant branches | Adaptation and exclusions |
| --- | --- | --- |
| Body token retention and prototype analysis | `templates.c:cache_function_template_body` (27868+), `function_prototype_instantiation` (6310–6480) | Managed immutable TokenBuffer plus immediate parser/semantic rescan; no skipped-body success; currently named direct type parameters/free functions |
| Deduction, completion and concrete signatures | `overload.c:adjust_deduction_pair` (4267+), `deduce_from_one_pair` (4445+), `deduce_one_parameter` (4669+), `function_template_call_argument_deduction` (4818+); `templates.c:wrapup_function_template_argument_deduction` (3069+) | Direct by-value scalar parameter pairs; explicit bindings/substitution, consistency/undeduced failures; references/pointers/packs/defaults remain open |
| Ordering and selection | `overload.c` ordinary/template tie (8943+), template ordering (9284+); `templates.c:compare_function_templates` (3706–3949) | Standard conversion quality first; ordinary preference on equal quality; bidirectional deduction of direct patterns, not a concreteness score. General partial ordering/constraints remain open |
| Selected body instantiation | `templates.c:instantiate_template_function_full` (7321+, rescan at 7580+) | Type-keyed managed instance state and fresh-cursor body rescan; no losing body instantiation or native fallback |
| Definition-context lookup | `lookup.c:is_acceptable_symbol` declaration-sequence branch (2314+) | Frozen source-visible ordinary binding membership and definition lexical scope; general dependent member/ADL rules are not claimed |
| Constant evaluation | `interpret.c` explicit work-kind/phase model (overview 58+, work 1150+, phases 1396+), `interpret_expr` (34030), `interpret_constexpr_call` (34124) | Authored explicit expression/statement/call/block work stack with per-call locals and separate outcomes; only integer straight-line subset |
| Static assertion phases | `decls.c` static_assert branch (15551–15574) | Contextual bool conversion, required evaluation and prototype false-assert deferral; message literals/general contexts remain open |

## Constant execution and required contexts

The interpreter executes typed graph operations, nested calls, local storage,
blocks and returns; it does not substitute optional BigInteger folding for call
execution. Arguments are evaluated in caller frames, each callee gets separate
locals, early return skips later statements, and discarded nonvolatile plain/
parenthesized lvalues/function designators do not trigger storage reads. Actual
lvalue-to-rvalue operations still read and detect uninitialized state.

Results distinguish Success, Dependent, NotConstant, InvalidExpression,
UninitializedRead, UndefinedBehavior and ResourceLimit, with step/depth facts.
Target-width unsigned operations wrap; signed overflow, min/-1 division and
invalid reads fail required evaluation. Per-evaluation work/depth counters and
stacks reset after success/failure. Configured positive step/call/instantiation
limits have direct failure controls; broader parser/PP/resource bounds remain M6.

Constexpr scalar initializers and static_assert use actual required evaluation.
Prototype nondependent const/constexpr initialization can force the selected
constexpr template body when needed, without forcing a nonconstexpr optional
initializer body. Mutable globals do not become constants from a folded
initializer. Constexpr void/object/control-flow/assignment/lifetime evaluation is
outside this checkpoint and explicitly diagnosed where encountered.

## Executed integrated evidence

Four `m4-*-round1.json` build/test receipts identify the same 34-file held
implementation/test/fixture snapshot. Release has zero warnings/errors; managed
and Linux x64 NativeAOT each pass 48 named groups with zero failures/skips. The
13 M4 groups include real combined source, prototype errors, deduction/ordering,
dependency, constexpr execution/failure/limits, live-session collection/isolation
and replay rollback. Three output hashes are retained; M3 outputs were relocated
before standard build paths changed (`retained-8b32f7c60.json`).

The exact `m4-combined-gate.cpp` fixture couples nested identity/combine caches,
int/long deduction/instance reuse, local storage, earlier mark(long) despite a
later better overload and caller local, and required constexpr results 7/8/7.
Nine exact fixtures match native used-profile status/diagnostics. A native
inspector includes the hashed combined fixture and proves short/int/long result
types, direct-pattern ordering and ordinary/template preferences through distinct
return types; an unordered-pattern control rejects ambiguity.

All acceptance commands explicitly use native `--instantiate used` plus strict
C++20 and the witnessed LP64 target. Native default `none` omits external
selected bodies. Historical selected-false default-none expectation mismatch and
the disproved exploratory nonconstexpr-definition rejection expectation are
retained as failed probes, excluded from pass counts. Seven adapter unit tests
and four explicit native integration controls guard profile/status handling;
compiler catastrophe/command failure/signal never counts as a language negative.

## Canonical review finding IDs

Historical reports/messages used overlapping provisional R numbers. The final
review and this table use the canonical IDs below; aliases do not create extra
independent findings or erase history.

| Canonical ID | Subject / historical alias | Implemented resolution and executed control |
| --- | --- | --- |
| M4-01 | Session retention; round1 R1 | Template-owned dependent signatures; live-session collection/isolation regressions |
| M4-02 | Template-parameter redeclaration; R2 | Initial signature, body/nested scopes and function-name checks |
| M4-03 | Type/value dependency; supervisor provisional R3 | Separate dependency facts and dependent evaluator outcome even for concrete bool/conversion target |
| M4-04 | Discarded lvalues; reviewer round2 R3 | Explicit no-read discarded plain/parenthesized lvalues/designators, paired actual-read failure |
| M4-05 | Ignored constexpr alias; round2 R4 | Illegal alias/typedef specifier diagnostic |
| M4-06 | Contextual bool/function decay; round2 R5 | Explicit bool conversion and unsupported decay boundary |
| M4-07 | Unnamed/pack taxonomy | Unsupported guards before missing-name expectations |
| M4-08 | Sole template designator | Unresolved overload representation; no selected-instance fiction |
| M4-09 | Constexpr void boundary | Uniform explicit integer-evaluator bound |
| M4-10 | Required initializer instantiation | Selected constexpr body available in unused outer required/potential-constant contexts |
| M4-11 | Replay checkpoint lineage bound | Child branching rejected; root journal restores replay/instance mutations |
| M4-12 | Per-Evaluate state/budgets | Same interpreter success/early-return/failure/success reset controls |
| M4-13 | Native instantiation profile | Explicit used-mode command and selected/unused integration rejection controls |

Remaining required work includes template forward/redeclarations, explicit
arguments/specializations, compound/dependent class/member/ADL forms,
packs/defaults/constraints/general ordering, and full constexpr objects/control
flow/lifetimes. The next active assignment is integrated preprocessing with
explicit physical/expansion/logical source provenance; it does not remove these
remaining M4/M5 requirements from the feature inventory.
