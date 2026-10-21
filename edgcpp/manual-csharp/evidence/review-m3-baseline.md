# Independent M3 review baseline

Date: 2026-10-21. Reviewer: `/root/manual_edg_supervisor/review_m3`.
This is a read-only design/source inspection record, **not milestone approval**.
The implementation and new serialized execution receipts must still be reviewed.

## Scope and authority

Read the parent and manual-csharp `AGENTS.md`, local `PLAN.md`, `PROGRESS.md`,
`SUPPORT.md`, `docs/CORRESPONDENCE.md`, and prior final M0/M2 review. The local
campaign explicitly authorizes handwritten C# semantics; the older exporter
semantic-authority restrictions do not prohibit that separate campaign.
Reviewer may change only new M3/M4 review evidence. Builds, tests, native EDG,
implementation changes and Git operations belong to the supervisor/worker.

The current bounded assignment is ordered, by-value integer/scalar free-function
overload binding and resolution. References, classes, member functions, default
arguments, ellipses, user-defined conversions and ADL remain outside approval.
An overload binding is distinct from canonical function entity symbols.

## Independent EDG inspection

Inspected actual `upstream/src/overload.c` and `overload.h` at the hashes retained
in `source-next-gates.json` (respectively
`62c8b3971d169872985b6855871488690c971455a0ef07773e6f78f335a1c67e` and
`e70767da9150b2f117f60624800b7dab762237ca085ed65aae3a2fb2da46f6bf`).

- `overload.h:268` defines exact, promotion and ordinary standard-conversion
  match levels in that order; `an_arg_match_summary` stores the descriptor per
  argument and separately remembers lvalue-to-rvalue conversion.
- `add_function_to_candidate_functions_list` (`overload.c:1782`) preserves a
  function entity, its containing overload symbol, and its argument matches as
  separate facts. Canonical declaration identity must survive binding growth.
- `determine_arg_match_level` (`3011`) simulates required transformations before
  ranking. A void argument has no match. Pointer/reference/class paths do not
  become supported merely because this routine is cited.
- `compare_standard_conversions` (`7660`) prefers exact to nontrivial, then
  promotion to ordinary conversion. Its integer-only bounded branch provides
  no closer-width, signedness, constant-value, or destination-rank preference
  between ordinary integral conversions. Integral-to-bool is an ordinary
  conversion; the later bool tiebreaker is specifically pointer/member-pointer
  to bool, outside this assignment.
- `select_best_candidate_functions` (`10199`) intersects best sets per argument;
  `match_is_better_on_at_least_one_arg` (`9712`) requires a survivor to be strictly
  better on an argument against each other candidate (or a permitted function
  tiebreaker). Within this integer/non-template subset, equivalent pairwise
  dominance must give the same result. Summed ranks are not equivalent.

## Required produced-code and execution checks

1. Exact versus conversion; short/bool/unsigned-short to int promotion versus
   wider ordinary conversion; equal ordinary conversions remain ambiguous.
2. Crossed multi-argument vectors remain ambiguous even when one summed score
   would win. Reverse declaration order and include a dominated third candidate.
3. Local variable/parameter/alias hiding stops lookup before the global overload
   set. It must not merge candidates through a nonfunction binding.
4. A call before a later overload keeps precisely its earlier visible candidate
   identities; its retained graph must not grow candidates retrospectively.
   A subsequent call sees the later declaration. Rejected later redeclarations
   cannot poison the canonical visible entities silently.
5. Compatible prototype/definition pairs (including top-level parameter const
   and alias use) share one canonical entity inside a growing binding; distinct
   signatures retain distinct entities and definition links. Same signature
   with a different result remains invalid.
6. Wrong arity, void arguments, unresolved/non-callable names, equal matches,
   redefinitions and conflicts are invalid; valid excluded forms are explicitly
   unsupported. Error recovery must not turn an unresolved overload into a
   success-shaped selected call.
7. Public candidates/conversion descriptors/argument graphs are immutable or
   safe read-only views with explicit source ownership. Separate/repeated
   sessions preserve graph independence. No native fallback, generated semantic
   input, dynamic emission, fixture-specific dispatch, or AOT-only branch.
8. Verify new Release and NativeAOT results, native positive/negative/witness
   receipts, and source/fixture hashes against the reviewed current files.

## M4 preconditions and lookup risks

Prior N1 is mandatory before production speculation/replay: direct tests must
change a preexisting `SetDefinition` link and `CurrentFunction`, restore nested
and outer checkpoints, and assert the original identities/links. The old helper
test label did not establish those branches.

Initial source lookup references include `templates.c` cache/rescan routines,
`function_prototype_instantiation`, `instantiate_template_function_full`, and
`wrapup_function_template_argument_deduction`, plus `lexical.c` reusable/cached
token rescanning. A retained token interval alone does not establish a real
template rescan/deduction/instantiation path or constexpr execution.

Replay must not look up nondependent names through a final mutable global
dictionary or the caller's local scope. Retain the definition-context visibility
and candidate identities/order needed by the bounded forms; a later better
overload must not replace a nondependent call selected at definition. Dependent
calls need their own explicit support boundary. No M4 behavior is approved here.
