# Scalar free-function overload correspondence

Implemented/validated bounded M3 checkpoint; final independent acceptance is
recorded separately in `evidence/review-m3-*.md`. This is not full overload or
class support and does not complete M3 or the campaign.

`NameBinding` owns source-ordered entity references for one scope/name. Distinct
function signatures have distinct canonical Symbols; compatible redeclarations
share their Symbol and retain separate declaration nodes. Ordinary nearest-scope
lookup stops at the first binding, including a nonfunction hiding all outer
functions. `FindLocal`/`Lookup` single-symbol convenience queries return a symbol
only for a sole entity; they do not choose the first overload. Consumers inspect
the explicit binding for overloaded names.

An `OverloadSetExpression` captures a copied candidate entity array at the source
reference point. `CallExpression` retains candidates, rejection reasons,
per-argument standard conversions and the selected canonical function. Later
declarations/definitions may update the same entity's definition link but cannot
add candidates to an earlier call. Views are read-only and captured arrays copied.
Scope rollback removes appended overload membership and restores earlier binding
identity/order. Direct tests cover that state and N1 definition/current-function
restoration before upcoming template replay.

## Algorithm mapping

Source SHA-256 is in `evidence/source-m3.json`; no whole EDG source file is ported.

| Authored operation | EDG routines/branches | Preserved invariant / managed adaptation |
| --- | --- | --- |
| Ordered function entity collection | `overload.c:add_function_to_candidate_functions_list` (1782–1816), declaration/symbol insertion routines | Preserve candidate identities/source visibility using copied ordered arrays instead of native linked-list prepend; no candidate-order ranking |
| `MatchCandidate` | `determine_arg_match_level` (3013+, including void rejection, lvalue transforms, unqualified exact 3283–3332, promotion 3473–3487) | Reject arity/unsupported argument types; integer exact, integral promotion or ordinary conversion descriptors; lvalue-to-rvalue/top cv do not worsen rank |
| Scalar rank comparison | `compare_standard_conversions` (7660+, exact/promotion/conversion branch 7735–7762) | Exact beats promotion beats conversion; equal scalar conversions remain indistinguishable, with no destination width/signedness/bool/return-type preference |
| `SelectBestCandidate`/`BetterCandidate` | `select_best_candidate_functions` (10199+), `match_is_better_on_at_least_one_arg` (9712–9775) | Direct pairwise comparison requires no worse argument and at least one strictly better argument against every rival; equivalent to EDG best-set intersection for this totally ordered scalar-rank subset; never sum ranks |

Classes/references/pointers/member functions/user-defined conversions/templates/
ADL/defaults/variadics and extension tiebreak branches remain required later work.
The direct pairwise adaptation must be revisited when conversion relations gain
those additional rules; current equivalence is explicitly limited to scalar ranks.

## Executed evidence

`m3-build-initial.json`, `m3-tests-initial.json`,
`m3-nativeaot-publish-initial.json`, `m3-nativeaot-tests-initial.json` identify the
same 23-file implementation/test/fixture snapshot. Release has zero warnings/
errors; both harness runs pass 35 named groups, zero failures/skips. New groups
cover N1 plus nine M3 paths, including equal/crossed ranks, dominated third
candidates through declaration permutations, bool-versus-long ambiguity,
unsigned-short promotion, alias/const signature merging, viability, source-order
snapshots, hiding and overload membership rollback.

Six exact-byte M3 fixtures match native expectations. The included-fixture
native semantic witness discriminates selected overloads through distinct int/
long return types and `decltype` static assertions for exact and short/bool/
unsigned-short promotion. A separate native-only staged declaration witness
proves earlier lookup has long result and later lookup int. The managed source
ordering test independently inspects selected Symbol/candidate identity; mere
acceptance of `m3-order.cpp` does not prove native selected-callee identity.

Three M3 build artifact hashes are retained. Prior `f27031ffc` artifact bytes
were copied to ignored checkpoint storage before ordinary build paths changed,
with relocation metadata in `evidence/retained-f27031ffc.json`.
