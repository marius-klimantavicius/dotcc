# Independent scalar-reference review baseline

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21.
Prior bounded M1 acceptance is committed as `3b667ce73`. This is read-only
review of the isolated held reference patch, not implementation acceptance.
No reviewer implementation edits, builds/tests, native/compiler executions or
Git operations were performed. The reviewer now owns only new
`evidence/review-references-*.md` reports.

The complete isolated handoff, References/Types/OverloadResolution and parser/
expression/graph integration plus all15 authored test groups were read.
All20 handoff implementation/test/fixture fingerprints match the frozen tree;
all7 consulted EDG SHA256 values match actual upstream files. Main integration
is still in progress and must preserve the newer character/diagnostic/PP/input
APIs; isolated full-file replacement would not establish that preservation.

Actual EDG branches independently inspected include reference-relatedness and
compatibility in `types.c:11218+`, reference canonicalization in `il.c:13951+`,
strict `compare_reference_matches`/`compare_argument_tiebreakers` in
`overload.c:7979+`, and C++23-gated `make_xvalue_if_move_eligible` in
`expr.c:38208+` with return initialization at52651+. The strict profile's
reference-kind tie break precedes immediate cv; scalar identity versus added
immediate reference cv is not a pointer qualification-conversion subsequence.
GNU/Clang alternative order and class/pointer/deep-qualification branches are
not qualified by this scalar adaptation.

## Preliminary produced-code observations

- Separate canonical reference identity retains qualified referred type, with
  all four alias collapsing combinations and ignored top-level alias cv.
- Related scalar binding checks cv/category before allowing a temporary path;
  unrelated scalar conversions retain exact/promotion/conversion rank.
- Named lvalue/rvalue reference expressions are lvalues of the referent type;
  reference-return calls retain declared reference type while exposing lvalue/
  xvalue expression category. Direct binding precedes scalar value loading.
- Materialized/converted temporaries retain initializer and binding context.
  Named-reference chains retain KnownTemporary identity and original lifetime
  owner, without creating or extending a second temporary. Invalid binding is
  distinguished from valid binding with a lifetime hazard. Unknown is not safe.
- Reference and function type interning use a recursive scope/dependency walk,
  including function return/parameter types nested beneath references. Tests
  keep the session/type table alive while dropping valid, invalid, unsupported
  and replay analyses; those authored tests still require execution.
- Optional const-scalar folding is separated from required reference evaluation;
  actual reference constexpr/object interpretation stays explicitly Unsupported.
  No reference binding/materialization fabricates an integer ConstantValue.

The earlier missing warning for a named local reference to a temporary was
raised during M1 spare-time review and is repaired in the isolated held patch.
The new tests assert original KnownTemporary/lifetime owner and no local warning
for a global owner. Exact native warning behavior is still unexecuted; warning
parity must remain qualified until observed.

## Required integrated evidence

Inspect actual main-tree hunks, execute all prior79 groups plus integrated
reference assertions in managed and NativeAOT, and compare exact native fixture/
selection/category witnesses with matching held hashes. Preserve failed-native
expectations and do not count catastrophic/tool exits as language rejection.

New character integration needs finite direct controls beyond the old base:
char32_t→unsigned-int promotion over int conversion for reference candidates;
exact char32_t reference preference; char16_t reference-return/alias type and
lvalue category; char8 literal materialization retaining its actual character
payload/type. Existing M1 templates/constexpr and changed-stream gates must
remain passing after the semantic merge.

Full declarators/qualifiers, pointer/class/array/function references, dependent
reference templates/forwarding, general overload conversions, reference object
constexpr execution and complete lifetime/cleanup semantics remain required
PLAN/inventory work. No whole M3 or campaign completion is implied by a later
bounded scalar-reference checkpoint.
