# Independent early M4 review baseline

Date: 2026-10-21. Reviewer: `/root/manual_edg_supervisor/review_m3` (continued
independent reviewer). **Source/design inspection only; no M4 approval.**
M3 checkpoint `8b32f7c60a18184476248e759c085b5a86e4588c` is accepted within its
scalar scope. N1's direct rollback tests are closed. Full `PLAN.md` stays active.

## Independently inspected EDG contracts

`templates.c`, `overload.c`, and `interpret.c` retain the exact hashes recorded
and independently checked at the M3 gate in `source-next-gates.json`. Also read
the lookup declaration-sequence and lexical reusable-cache branches already
covered by the initial source ledger. No whole source file is being claimed.

- `templates.c:cache_function_template_body` (27868) retains actual body tokens
  and diagnoses missing closing delimiters. `function_prototype_instantiation`
  (6310–6462) explicitly detects definition-time errors and records nondependent
  calls. Its body path pushes template context, rescans reusable tokens, scans
  the function body, then restores context. A balanced token scan alone is not
  this semantic check, including for unused templates.
- `templates.c:instantiate_template_function_full` (7321; rescan/body dispatch
  around 7584) distinguishes prior/prototype/body instantiation, activates template
  declaration/argument context, rescans retained tokens and performs real body
  analysis. Record dependent types and concrete instances explicitly; do not
  manufacture concrete graph facts by replacing source text or cloning a graph
  whose earlier name binding was never established.
- `overload.c:adjust_deduction_pair` (4267; by-value branch around 4380) applies
  array/function transformations and ignores top-level cv for by-value deduction.
  It does not apply integral promotion/usual arithmetic conversions to deduce T.
  `deduce_from_one_pair` (4445) consistently binds a parameter across pairs;
  `deduce_one_parameter` (4669) distinguishes nondeduced parameters;
  `function_template_call_argument_deduction` (4818) produces a substituted
  routine type or failure. `templates.c:wrapup_function_template_argument_deduction`
  (3069) completes and substitutes bindings separately from body instantiation.
- Ordinary lookup respects declaration sequence (`lookup.c` around 2314).
  Nondependent lookup must use the template definition's visible candidates,
  unaffected by later declarations or the caller's local scope. Any supported
  dependent-call policy must distinguish definition lookup from instantiation
  and state the unimplemented ADL/class/namespace branches explicitly.
- `interpret.c` documents an explicit work stack/driver, interpreter state,
  local storage maps, call frames, failure diagnostics and cost/depth limits
  (intro, `an_interpreter_state` around 1790). `push_call_work` (6139),
  `process_call_work` (ending near 23288), `do_constexpr_call` (23291), and
  `interpret_constexpr_call` (34124) distinguish argument evaluation, mapped
  parameters, body execution, results, cleanup, invalid operations and failure.
  A bounded integer interpreter can adapt storage to managed identity-keyed
  maps and target-width integers, but optional parser folding is not execution
  of a constexpr call or proof of a required constant expression.

## Required adversarial review gates

1. Analyze unused template definitions, retaining dependent representation
   while rejecting nondependent unresolved names/errors. Unsupported statements
   cannot disappear successfully inside an unused cached body.
2. Deduce from actual unqualified by-value types, preserve char/short identity,
   reject repeated-T conflicting int/long types, and then rank conversions for
   substituted signatures. Concrete nondeduced parameters may convert after
   deduction. Non-template preference applies only after equal conversions;
   a template with better conversions still wins.
3. Candidate deduction/substitution and selected body instantiation are distinct.
   A losing template body must not introduce instantiation errors. Equivalent
   template redeclarations or currently unsupported partial ordering must not
   silently become unrelated candidates with first-declaration wins.
4. Canonical instances key on the template entity and all canonical argument
   identities; repeated calls reuse one instance, distinct types retain distinct
   instances, and sessions/analyses remain isolated. Failed/recursive instances
   cannot become success-shaped cached definitions.
5. Freeze definition-context ordinary lookup and nondependent candidate sets.
   Test later better overloads, caller-local hiding, nested template calls, local
   aliases/declarations, recursion, and continuation after a failed instantiation.
   Verify replay cursor/scope/current function/diagnostic and retained graph
   ownership explicitly, including source ranges from the original token buffer.
6. Execute constexpr bodies through explicit work/call/local state; test nested
   calls, shadowed locals, early return, parameter binding, non-constexpr calls,
   missing return, uninitialized reads, unsigned wrapping, signed overflow,
   division by zero/minimum divided by -1, and deterministic depth/cost failure.
   Rejected constant evaluation must produce a required-context diagnostic,
   while ordinary valid runtime code need not be constant-evaluable.
7. Require an actual combined source integration fixture coupling declaration,
   deduction, cached-token body rescanning, nested instances, frozen lookup,
   overload selection and an instantiated constexpr call in static_assert.
   Disconnected unit assertions alone do not qualify the difficult-path gate.
   Native discriminating semantic witnesses and negative controls must match the
   hashed source; managed tests inspect the actual graphs/instances/results.
8. Independently match the final held source/test/fixture snapshot against
   supervisor-executed Release/managed/NativeAOT/native receipts and artifacts.

These gates do not imply full templates/constexpr support. References, classes,
ADL, non-type/template-template parameters, packs, constraints/SFINAE, partial
specialization/ordering, arbitrary constexpr control flow/lifetimes/storage and
all other unimplemented PLAN requirements remain continuing work.
