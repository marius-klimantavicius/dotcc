# Scalar references implementation handoff (HOLD, unexecuted)

Workspace: `/home/marius/.t3/worktrees/dotcc/t3-546893e6`.
Branch: `t3/scalar-reference-semantics`.
Base: `abb239b200817d3fa9cf32888162cce9ef010271`.
This isolated worker does not edit `/home/marius/p/dotcc`, execute builds/tests/
native compilers, or mutate Git. All validation and integration belong to the
supervisor. This is bounded M3.2/M3.3 work, not campaign completion.

## Implemented integration interfaces

- `ReferenceType : SemanticType` has `ReferredType` and `ReferenceKind`
  (`LValue`/`RValue`). The declared type is canonical separately from the type
  of an expression naming that entity. References have no top-level cv;
  alias-applied cv is ignored and alias reference composition collapses.
- `TypeTable.Reference` interns only concrete, scope-free types. A recursive
  scope/dependency predicate guards both reference and function caches, including
  references nested inside function signatures. Dependent template reference
  declarators remain explicitly Unsupported; failed analyses must not leak their
  scopes through compound interning.
- `ValueCategory.XValue` is added. `ReferenceExpression` exposes referred-to type
  and LValue category for both kinds of named reference. `CallExpression`
  preserves the selected function's declared return type separately from its
  referred-to expression type and LValue/XValue category.
- `ReferenceBindingExpression` represents initialization/argument/return
  binding without a load for direct binding. It retains the `ReferenceType`,
  source/converted referent, binding descriptor, optional materialized scalar
  temporary and bounded lifetime facts. It does not fabricate constant values.
  `VariableDeclaration.ReferenceBinding` provides the declaration's binding.
  `Temporary` means a new temporary at this binding; `KnownTemporary` follows
  named references to an existing temporary without creating/extending one.
- `StandardConversionSequence` retains scalar rank plus reference kind,
  source category, direct/temporary binding and cv facts. Candidate comparison
  remains per-argument with qualification and rvalue-reference tiebreaks;
  by-value versus reference is not itself a ranking preference.
- Parser additions scan reference declarator operators after a base type in
  declarations, aliases, parameters and function returns. Return initialization
  uses binding before ordinary lvalue-to-rvalue conversion.
- Reference constexpr execution is explicitly Unsupported until an actual
  object/referent evaluator is implemented. Potentially dangling references
  remain distinct from invalid binding; accepted local-reference returns must
  not be turned into invented language errors.

All code here is handwritten. No generated frontend implementation, mechanical
translation, compiler invocation, native runtime fallback or fixture answers were
used. Read-only source searches, SHA-256, Git status/diff and `git diff --check`
were the only verification-like commands. `git diff --check` had no output.
No build, test, native compiler, Git mutation, commit or push was executed.

## Exact owned files

New production: `src/EdgSharp/References.cs`.

Modified production (small additive patches): `src/EdgSharp/Types.cs`,
`src/EdgSharp/Parser.cs`, `src/EdgSharp/Expressions.cs`,
`src/EdgSharp/OverloadResolution.cs`, `src/EdgSharp/SemanticGraph.cs`.

New tests: `tests/EdgSharp.Tests/ReferenceTests.cs`.
Modified test registration: `tests/EdgSharp.Tests/Program.cs`.
New evidence: this file only; no campaign ledger was edited.

New fixtures, all under `fixtures/`:

- `m3-reference-positive.cpp`
- `m3-reference-ranking.cpp`
- `m3-reference-invalid.cpp`
- `m3-reference-rvalue-invalid.cpp`
- `m3-reference-temporary-invalid.cpp`
- `m3-reference-void-invalid.cpp`
- `m3-reference-redeclaration-invalid.cpp`
- `m3-reference-ambiguous.cpp`
- `m3-reference-crossed-ambiguous.cpp`
- `m3-reference-lifetime.cpp`
- `m3-reference-dependent-unsupported.cpp`
- `m3-reference-native-witness.cpp`

No upstream/older-campaign/header/literal/Unicode/preprocessing/filesystem files,
project manifests, Source/CompilationSession/ConstantEvaluation/Templates/
ParseState files, PLAN/AGENTS/support/progress/correspondence ledgers or main
checkout files were modified. The new code uses the existing template replay,
diagnostic bag and target integer conversion APIs.

## EDG source correspondence and actual consulted hashes

Paths below are relative to `edgcpp/upstream/src/`. These are exact consulted
files, not whole-file port claims. Native executable/hash/command receipts must
be added by the supervisor when it executes the controls.

| Source | SHA-256 |
| --- | --- |
| `types.c` | `89e6393ab4109ef3924eb25ca8fe04503e87db8fcd053be559bac52713ba11fc` |
| `il.c` | `535768a37a2bd0f90dd18c41bd867398074289a305c3ff88589da67153514cd0` |
| `declarator.c` | `26680a8059cb338336b31d68ab40b25e9d1660bb5ba6192aea16ae15a9651552` |
| `overload.c` | `62c8b3971d169872985b6855871488690c971455a0ef07773e6f78f335a1c67e` |
| `expr.c` | `056f8e7416d6f948846abbe17c3401e9eb4ca8ed1ce0ae7673cd573b314df3c2` |
| `exprutil.c` | `1b01609700d2a72f64f94e218a3f210e7e9503394bc3cf16168e8a1a50e50812` |
| `decl_inits.c` | `8d66e24839804eb5e636c960fde1598553b646179a199611ee1d38eca8277bc7` |

| Upstream routines/consulted region | Preserved invariants and authored mapping | Omitted branches |
| --- | --- | --- |
| `types.c:is_reference_that_can_bind_to_rvalue` (1325+), `are_reference_related`/`are_reference_compatible` (11218–11271) | Related integer kinds bind without a value conversion if cv/category permit; const lvalue references and rvalue references can bind scalar rvalues; cv cannot be dropped by manufacturing a copied temporary | Volatile/restrict/nullable qualifiers, function/array/pointer similarity, base classes and compatibility modes |
| `il.c:make_reference_type`/`make_rvalue_reference_type` (13951–14017), `make_reference_to_reference` (14370–14480) | Separate canonical lvalue/rvalue reference identity over the qualified referent; alias composition collapses to lvalue if either component is lvalue; alias-applied cv does not qualify the referred object | Native based-type lists/layout, restrict/near/far/tracking refs and compatibility diagnostics; ignored alias-cv warnings are not implemented |
| `declarator.c:pointer_declarator` (5690–6220) | Scan reference operators in the existing interleaved parser; reject void referents, explicit reference-to-reference operators and cv after the operator; aliases may collapse | Pointers, arrays, nested/parenthesized/function/member declarators, attributes, extended calling conventions; template reference declarators explicitly Unsupported |
| `overload.c:determine_arg_match_level` (3013–3520, 3737–3830) | Determine reference-relatedness, cv and category viability before ranking; unrelated scalar conversion can create a temporary, including for `int&&` from a `long` lvalue; parameter reference identity survives matching | User conversions/classes/bitfields/decay, variadics/defaults, extensions and general deduction |
| `overload.c:compare_standard_conversions` (7660–7762), `compare_reference_matches` (7979–8027), `compare_argument_tiebreakers` (8030–8380) | Exact/promotion/conversion rank first; strict Core1374 reference-kind ordering then cv subset when underlying integer kinds match; immediate reference cv is not a qualification-conversion subsequence; by-value/reference rank ties remain ties | Pointer/deep qualification subsequences, class-base, UDC, list, function-reference and implicit-object branches; GNU/Clang alternative order |
| `overload.c:direct_reference_binding_possible` (25070–25370), `prep_reference_initializer_operand` (25576–25690, 25950–26331), `prep_initializer_operand` (28590–28660) | Route reference initialization before glvalue-to-prvalue loading; compatible glvalues keep original referent; scalar prvalues materialize and unrelated types copy-convert into a temporary; context distinguishes reference-variable from argument/return full-expression lifetime | Class/array/list binding, UDC and constructors, complete cleanup/lifetime engine, managed constexpr reference execution |
| `overload.c:check_for_returning_reference_to_local_entity` (25419–25438), `exprutil.c:examine_expr_for_auto_object`/`is_glvalue_for_auto_object` (25185–25279) | Bounded known automatic/temporary referents can warn on return; valid binding stays accepted. Named local references retain their original known temporary/lifetime owner; global temporary lifetime owners do not receive that warning | General addressing-path traversal, member/subobject/indirect/call-return lifetime propagation and full warning parity remain unqualified until native runs |
| `expr.c:scan_identifier` (40247+), `make_param_ref_operand` (40115–40174), `scan_function_call` (8040+); `exprutil.c:add_reference_indirection` (17286–17366), call-result construction (19715–19760), `conv_rvalue_reference_result_to_xvalue` (10898–10917) | Named references of either kind are lvalues of the referent type. Reference-return calls retain declared return identity and expose lvalue/xvalue referent categories; scalar value uses explicitly load either glvalue kind | Member/overloaded operators, dependent reference expressions, general casts, class results and reference constexpr folding |
| `expr.c:scan_return_expression` (52651–52969), `make_xvalue_if_move_eligible` (38208–38256) | Return binds directly before ordinary scalar conversion. The supported C++20 profile does not apply C++23's blanket move-eligible-id xvalue rewrite | Deduced returns, classes/copy elision, coroutines, implicit moves through UDC and all C++23 profile support |
| `decl_inits.c:check_for_missing_initializer_full` (10690–10755), default-reference init (8365–8372) | A reference variable in the implemented non-extern declaration grammar requires an initializer | Extern/storage/linkage, fields/special members/default construction and other initialization forms |

Managed adaptation: binding-expression type/category describe its referent,
where EDG's stored reference-to operation can be a prvalue with reference type.
The explicit `Binding.ReferenceType` preserves that distinct declared identity.
All reference binding/materialization nodes deliberately have null
`ConstantValue`; their source/initializer can retain already-known scalar folds.

The recursive ownership predicate follows every current compound type
(reference and function, including both return and all parameter positions)
until it finds a scoped template parameter or dependent type. Such compounds
never enter session-lived caches, even during failed/unsupported analyses.
Concrete compounds retain canonical session identity. This is a managed lifetime
adaptation; it does not replace token replay or dependent signature non-interning.
Every future compound type must extend the predicate before it is interned.

## Authored test inventory — NOT EXECUTED

15 named groups are registered, giving 75 expected groups against this base's
60-group harness. Main's newer character/header suite will have its own total.
This is an authored inventory, not a pass count or independent acceptance.

| Group | Source-text/identity assertions |
| --- | --- |
| `ReferenceIdentity` | Distinct `int&`/`const int&`/`int&&` function entities; compatible aliases/prototype/definition; ignored cv on reference alias; all four scalar alias-collapse combinations; incompatible return redeclaration does not replace original binding |
| `ReferenceBindings` | Mutable/const lvalue bindings, actual selected symbols/referent types, no direct loads/folds, parameter/local/nested-scope identity |
| `ReferenceTemporaries` | Prvalue and converted temporary materialization, target cv/rank/load facts, reference-variable versus argument lifetime owner, converted xvalues, no second temporary for direct xvalues, no fake self-reference referent or const scalar value |
| `ReferenceInvalid` | Missing initializer, void referent/expression, const drop, mutable ref from prvalue/xvalue/wrong scalar type, related rvalue ref from lvalue/named rvalue ref, explicit repeated operators/cv, bad reference call/return/redeclaration |
| `ReferenceOverloads` | Selected canonical Symbol identities for mutable/const/prvalue/xvalue/named-rvalue arguments; viable/rejected candidates and direct binding facts |
| `ReferenceRanking` | Rank before reference kind/cv, promotion vs conversion across every existing narrow integer kind, cv subset and const xvalue viability, unrelated equal-rank ambiguity, converted lvalue rvalue-ref match |
| `ReferenceAmbiguity` | By-value vs each reference flavor, crossed cv/rvalue-reference and crossed promotion/conversion matches |
| `ReferenceCandidateOrder` | All six declaration permutations of the three reference overloads preserve selected results |
| `ReferenceLookup` | Frozen early candidate arrays with later overloads/definition links, local hiding, retained definition context, actual existing by-value-template replay calling reference overloads, by-value deduction from a reference expression |
| `ReferenceReturns` | Direct return binding without a load, lvalue/const/xvalue call results and declared return identity, xvalue value-use load, named rvalue references remain lvalues |
| `ReferenceLifetimeHazards` | Locals/by-value parameters/direct temporaries and named local temporary references warn while analysis succeeds; original temporary identity/lifetime survives aliases; global temporary reference does not acquire the local warning |
| `ReferenceRollback` | Real parsed reference graphs/candidates in owner-journaled child replay; restore overload membership/definitions/scopes; retained earlier graph/candidate snapshots remain separate after a second replay |
| `ReferenceOwnership` | Non-inlined WeakReference helpers with session and TypeTable kept alive; valid/invalid/unsupported/ref-callee-template analyses, recursive dependent-ref/function cache guard, repeated-analysis concrete type identity and independent-session isolation |
| `ReferenceBoundaries` | Template/dependent reference declarators, constexpr reference object operations, pointer/parenthesized/array forms explicitly Unsupported |
| `ReferenceFixtures` | Actual positive/ranking fixtures inspect selected types/categories; invalid/ambiguous fixtures reject; valid template-ref fixture stays Unsupported; lifetime fixture accepts with warnings |

Existing `UnsupportedBoundaries` no longer calls uninitialized `int &r;`
Unsupported. It is now an invalid supported reference declaration, covered by
`ReferenceInvalid`. No existing scalar/template test was run by this worker.

## Proposed serialized supervisor native controls — NOT EXECUTED

Use the campaign's established strict C++20/LP64 used-instantiation profile,
including `--clear_flag use_predefined_macro_file --c++20 --strict --no_code_gen
--instantiate used`, through the supervisor's oracle adapter. Record exact
compiler/source hashes/statuses; expected language rejection is 2, never a
catastrophic/tool failure. Native execution remains oracle-only.

- Accept: exact `m3-reference-positive.cpp` and `m3-reference-ranking.cpp`.
  Acceptance alone does not prove selected identities.
- Accept with warnings: exact `m3-reference-lifetime.cpp`. Inspect actual
  warnings separately for direct local/parameter/temporary, named local const
  reference chains and named rvalue-reference temporaries. The global
  `static_temporary_reference` is the discriminating nonlocal control. No
  warning expectation here is an executed/native-verified fact.
- Reject: the seven `*-invalid.cpp`/`*-ambiguous.cpp` files named above.
  They distinguish const drop, related rvalue-to-lvalue rejection, mutable ref
  from prvalue, void reference, conflicting return reference, by-value/reference
  ambiguity and crossed cv ambiguity. Additional exact individual source
  controls are retained in `ReferenceInvalid`/`ReferenceAmbiguity` tests.
- Native accept, managed Unsupported: exact
  `m3-reference-dependent-unsupported.cpp`; no fixture rewrite bypasses its real
  `T&` signature and actual call.
- Accept native-only `m3-reference-native-witness.cpp`. It includes both exact
  positive fixtures unchanged. Distinct overload return types discriminate
  selection, `decltype` discriminates reference types/call value categories and
  named-rvalue-reference lvalue behavior, and function-pointer type checks
  discriminate the reference parameter/return signature. Further assertions
  discriminate alias collapsing, promotion, cv, reference-kind and unrelated
  lvalue-to-rvalue-reference temporary selection. Its final constexpr section
  proves native reference values **only as a future interpreter contract**;
  managed reference constexpr remains Unsupported and receives no value credit.

## Integration notes for newer main APIs

Main was inspected read-only. It adds distinct `WChar`, `Char8`, `Char16`,
`Char32` IntegerKinds, target character state, character literal expression
handling, and special integral promotions. None is present in this base and
none was copied/reverted. Reference matching compares `IntegerKind` identity
and calls the existing `IntegerConversionRank`/`Promote`/`Convert` APIs; it
does not enumerate int/long destinations or alias character types.

Apply the small diff against the shared files, **never replace whole files**:

1. `Types.cs`: append `TypeKind.Reference`, make TypeTable partial, and replace
   only Function's shallow dependency test with the recursive predicate.
   Preserve main's IntegerKinds, target/predefined additions and promotion body.
2. `Expressions.cs`: insert the reference read check beside ordinary identifier
   construction, route selected reference arguments through binding, retain the
   declared reference return through existing `types.Unqualified`, and load
   either glvalue category for value use. Preserve main's character decoder,
   literal branches and newer diagnostic fixes. The call's binding Source/Referent
   can therefore be a character expression of any canonical integer kind.
3. `SemanticGraph.cs`: append XValue; adapt only ReferenceExpression and
   CallExpression plus the declaration binding query. Preserve main's new
   CharacterLiteralExpression and associated decoded payloads.
4. `Parser.cs`: additions are reference scanning after existing ScanType and
   reference dispatch in initializer/return/parameter parsing. Preserve main's
   expanded integer keyword/specifier handling. `optionalReferenceEvaluation`
   is saved/restored alongside the existing constant-required flag so ordinary
   runtime `const int copy=reference;` remains valid without fake folding.
5. `OverloadResolution.cs`: descriptor additions and per-argument comparison;
   preserve any newer diagnostic/type work. No candidate source ordering,
   entity redeclaration or template deduction algorithm is replaced.
6. `Program.cs`: add the 15 registrations and remove only the old `int &r;`
   Unsupported expectation. Main independently removes character-literal
   Unsupported expectations and adds M1 groups; preserve all of those edits.

No actual merge/integration was attempted, so there is no claim of a conflict-free
merge or successful integrated build. These are the concrete overlapping hunk
areas for supervisor integration. No source execution is authorized to this
worker until the supervisor releases it.

## Remaining required branches and validation

- Volatile/restrict and full cv combinations; complete declarators, pointer/
  array/function/member/class reference semantics and initializer forms.
- Dependent reference declarators, deduction/forwarding references,
  substitution/reference collapsing in templates, SFINAE and general templates.
  The existing by-value template path can call ordinary reference overloads;
  that does not implement template reference signatures.
- Full standard conversion subsequences, pointer/deep qualification, class/
  base/UDC/list/implicit-object ranking and every other M3 branch remain required.
- Reference object/address constexpr execution, constant-expression
  classification and folding; full temporary cleanup/lifetime propagation,
  dangling-use analysis, classes, special members and M5 object model remain
  required. Known origin is informational; Unknown never means safe.
- Native warning parity (especially named temporary chains), every native
  selection/category witness, managed builds/tests, NativeAOT, independent final
  review and integration with main's newer M1 APIs are all unexecuted/unqualified.
- Cancellation/resource bounds and broader repeated-header/memory stress are
  future M6 work. The authored weak-reference regression is not executed proof.

HOLD: source work is complete for this bounded handoff. The worker stops all file
edits until explicit supervisor release; the full campaign remains active under
the supervisor/root.

## Main integration additions (unexecuted until supervisor receipts)

The primary implementation worker manually merged the reference additions into
main after accepted commit `3b667ce73`, preserving target character types and
promotions, character payloads/nonsealed literal base, template ErrorCount
policies, preprocessing/structured filesystem integration and all79 existing
test registrations. The15 reference groups plus one authored character/reference
integration group produce95 expected groups, not an executed pass claim.

`m3-reference-character.cpp` and its separate native-only witness combine
target character references, exact versus promoted bindings, real u8 literal
materialization, alias/return/call categories, and by-value template replay
using definition-time reference candidates despite a later better overload and
caller hiding. Tests inspect retained scopes on failed/unsupported template
analyses and collect dropped scopes while the session remains alive.

Review found the isolated lifetime-origin traversal recursively re-walked every
named-reference chain. Main now reuses earlier completed immutable binding
origin/temporary facts in constant work per binding, retaining exact temporary
identity and lifetime owner. A self-reference has no completed initializer and
remains Unknown. The regression constructs512-link automatic/static/temporary
alias chains; valid lifetime hazards remain warnings. The isolated fingerprints
below remain historical handoff metadata, not current integrated hashes.

Supervisor first integrated execution: Release compilation succeeds with one
nullable-flow warning in the authored test, and all95 managed groups pass with
zero failures/skips. The15 explicit native fixture expectations match (8 accept,
7 reject), including the separate `m3-reference-frozen.cpp` late-void lookup
discriminator under used-instantiation policy; the valid dependent-reference
native control is still managed Unsupported. The single test nullable-flow
warning is corrected using the typed assertion helper. Zero-warning rebuild,
NativeAOT and final independent acceptance remain pending their exact receipts.

## Held implementation/test/fixture SHA-256

The following fingerprints describe isolated files, not an executed checkpoint.
This Markdown is excluded from its own map.

```text
87ceed243413dfbebe203f6f24fc29458087abfbf82896da31bf936f230aa19e  src/EdgSharp/References.cs
598da83f6edd78c2026d9f397dc44f15cb405d04c575a9aa64af5ef7d011fa27  src/EdgSharp/Types.cs
245124cf2bd697a441cbc07f5e0b4607935c6bb755a4e816515a6a47bb643a8f  src/EdgSharp/Parser.cs
e9a8d7647f78d26028ee838b4992036ea281198128f69dea6b0d805b961dcd4b  src/EdgSharp/Expressions.cs
ab9a2ad3eba36b3158437e141dcb1a8c113dbe40d185589798ecad200554708d  src/EdgSharp/OverloadResolution.cs
778ed335e953efc2005c417261fd1cd8574c20fee0d37b14c35e466d20cd5e77  src/EdgSharp/SemanticGraph.cs
2a9ed7fa4f62f3cc9c98f1513bba5b177001e61e73b08ca874f84556e6ca5d70  tests/EdgSharp.Tests/Program.cs
aef54e03a48b1b4f6be031b73193e720d50310c7204b8331670d438cf6d93519  tests/EdgSharp.Tests/ReferenceTests.cs
15f6e9902aefbcfe24000318af5ba4a2881c6d5f5d27dc6ebdd799733e5a4600  fixtures/m3-reference-ambiguous.cpp
dfec05290a145a5e3b6818ee9f270e0dae4fd786d4cfe19cb9e80d56702cf7d1  fixtures/m3-reference-crossed-ambiguous.cpp
aee868eeb02f6202804d3562d865c364a41e19a47fb44c785ebea489c2ea6622  fixtures/m3-reference-dependent-unsupported.cpp
1a146b74313fe22059521ab23df9736ec8f5d17e21423d3d8526a5e341d91ecd  fixtures/m3-reference-invalid.cpp
de628cda44d4e0f465dabdfe1825fab469725433dfb5efbbf979e56cd41d8a17  fixtures/m3-reference-lifetime.cpp
426f6795025368fc9c44e6b7bbe11ea8f016dc477253c566ddbfda2c6afe0b09  fixtures/m3-reference-native-witness.cpp
8407f10b497d8bc82c72248dc2fe62682f3f2135a32c0bb27f7e2d7c3469eb32  fixtures/m3-reference-positive.cpp
ea41534f9dd779f1ce2688003f4141332011ad4aa9b451a8b808ffcaf89d6444  fixtures/m3-reference-ranking.cpp
07d55e108c8c3f872ce3eba36ee95c82ced265e6ba4fe7ca92090c2611877bd6  fixtures/m3-reference-redeclaration-invalid.cpp
96764eb2e5772ecc3c81b2018759739b8f4ff35a066b630df80225e278687678  fixtures/m3-reference-rvalue-invalid.cpp
bb19e9179fe4b3888827d5891cb1e1401cb84e78c918971726fa7d743b613b12  fixtures/m3-reference-temporary-invalid.cpp
7990e6b9ab7818a5db18de14e92499ced81e04e69c26424765dc9a1892e927a8  fixtures/m3-reference-void-invalid.cpp
```
