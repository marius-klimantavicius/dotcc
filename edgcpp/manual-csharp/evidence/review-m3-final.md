# Independent review — bounded M3 scalar free-function overloads

Date: 2026-10-21. Reviewer: `/root/manual_edg_supervisor/review_m3`.

**Decision: approve this held snapshot as the bounded by-value integer/scalar
free-function overload checkpoint. No critical/high correctness finding remains
open within that scope. Prior N1's direct rollback-test requirement is closed.**

This is a checkpoint in the continuing full `PLAN.md` campaign. It does not
complete M3 (classes, references and other branches remain), approve M4, or
complete the handwritten C++ frontend. Proceed to the early M4 implementation
and independent review gate automatically; the broader roadmap remains active.

## Review performed

Independently read the actual produced `OverloadResolution.cs`, scope/binding
and call-graph changes, parser declaration/redeclaration and hiding paths,
conversion changes, rollback adaptation, new tests and six M3 fixtures. Reviewed
the retained native inspector inputs and all new execution receipts. The
unchanged session/token/type ownership contracts were checked against the new
binding and candidate types. The independent EDG routine inspection and required
adversarial cases are recorded in `review-m3-baseline.md`.

Reviewer performed only source/evidence reads and hash checks, and changed only
the assigned M3 review reports. Supervisor serialized every build, test, native
EDG invocation, and NativeAOT operation. No implementation edits, native/compiler
execution, builds/tests, or Git mutations were performed by the reviewer.

No production native fallback, generated semantic input, compiler process,
dynamic-emission dependency, fixture-name/value dispatch, or test-only semantic
path was found. The new production file carries the applicable upstream license
attribution and explicitly names its limited EDG branches.

## Algorithm and graph conclusions

- `NameBinding` owns an ordered list of canonical function entities. Compatible
  declarations merge with an existing entity by normalized parameter signature;
  distinct overloads remain distinct. Same-parameter conflicting result types
  diagnose an error without entering an additional visible candidate. Return
  type never ranks an overload. Alias and top-level parameter-const identity
  regressions inspect the actual declaration/entity/definition relationships.
- `FindLocalBinding`/`LookupBinding` preserve nearest-scope hiding. The legacy
  single-entity query returns null for an overload and cannot fall through to
  an outer binding. Parameters, locals and nested locals hide the whole global
  overload set. Rollback removes overload insertions in reverse order and
  preserves a preexisting binding's identity.
- A multi-function reference copies the visible entities into
  `OverloadSetExpression`; a call copies candidate records into its own graph.
  Later declarations cannot grow those arrays. Tests inspect both early/late
  selected identities and a two-candidate expression before a third overload.
  Canonical entity declaration/definition information may grow, as intended.
- Each viable candidate has one `StandardConversionSequence` per argument,
  with original source type, normalized target, lvalue transformation and rank.
  Integer kind equality is exact; implemented integral promotion is better than
  an ordinary conversion. Width, signedness, constant magnitude and destination
  rank cannot break equal ordinary-conversion matches. Int-to-bool is ordinary
  conversion. Rejected candidates retain explicit arity/conversion reasons.
- `BetterCandidate` requires no worse rank on every argument and a strict
  improvement on at least one; selection applies it against every other viable
  candidate. For the total order of these three scalar ranks this preserves
  EDG's per-argument best-set intersection and strict-better final condition.
  No score summation or declaration-order preference is present.
- Selected arguments receive explicit conversion graph nodes after selection.
  Exact lvalues expose lvalue-to-rvalue conversion; promoted lvalues expose that
  transformation followed by promotion. Source const remains queryable in the
  descriptor; destination/function parameter types are normalized. Previously
  accepted unqualified scalar call-result behavior remains intact.
- Public bindings, callee snapshots, candidates, conversion lists and arguments
  use read-only collections; constructor arrays are copied. Nodes retain
  reference identity and session/source ownership. Queries trigger no additional
  analysis. Existing separate/repeated-session and NativeAOT checks still pass.

## Adversarial tests and executed evidence

All four receipts below contain the **same 23-file source/test/project/fixture
SHA-256 map**, and every entry matches the held current files:

| Receipt | Executed result |
| --- | --- |
| `m3-build-initial.json` | Release build succeeds, zero warnings/errors |
| `m3-tests-initial.json` | 35 named groups pass, zero failures/skips |
| `m3-nativeaot-publish-initial.json` | Linux x64 NativeAOT publish succeeds |
| `m3-nativeaot-tests-initial.json` | Published executable passes all 35 groups, zero failures/skips |

The 35 groups include the previous 25, direct N1 coverage, and nine new M3
groups. Assertions inspect graph facts, not just acceptance. Reviewed cases
include exact versus conversion; short/bool/unsigned-char/unsigned-short
promotion; const source and parenthesized calls; boolean conversion value;
equal conversions across width/signedness; int-to-bool versus int-to-long
ambiguity; crossed two-argument and unequal-summed three-argument ambiguity;
dominated third candidates; six permutations of three declarations with a unique
winner; reversed equal-match declarations; wrong arity and void arguments;
unresolved arguments; source-order visibility; nonfunction hiding; overload
rollback; conflicting declarations; and immutable graph views.

`m3-tests-initial.json` SHA-256:
`a68e6ba9ee30004827d5267a08ec315c044a6415a432402a5e9665d4c42790d8`.
Key reviewed snapshot entries:

| File | SHA-256 |
| --- | --- |
| `OverloadResolution.cs` | `666817233a85d2b2709f09ff1fe2d028f13e1a88a6f83a58bc0c52e15e67deb9` |
| `SemanticGraph.cs` | `f7b4607f9d35a8fb14d3fcbf7c83b4efa0cb9a7fd1ea9a8511aa77e3c06c3f26` |
| `Parser.cs` | `0512b0e84acac2fe010c2dd040f700cb8eff88b6154ec54fcfdb020d7d1d92d8` |
| `Expressions.cs` | `322acddda1a46ac10ae9ce97be4aeb90494047ebb14f49d276accf838ec3fecf` |
| `ParseState.cs` | `f918a8bbd39fecb7950b25974829e172a8179b693422a3bd74ad3243736b3185` |
| `tests/EdgSharp.Tests/Program.cs` | `5dbe25f72ee573e5f62ccdc2d408061bd856c7ce77539f88a7c03c182fc1ce07` |

All eight `native-m3-*.json` receipts match their current source/compiler hashes,
strict C++20 arguments and expected status. Positive/order fixtures accept;
equal/crossed ambiguity, no viable function and local hiding controls reject
with actual language diagnostics at exit 2. The semantic witness includes the
exact hashed managed positive fixture and uses distinct int/long result types
to prove exact and short/bool/unsigned-short promotion selections. The standalone
source-order witness proves long before, int after a later declaration. These
native inspectors establish their selected type facts; managed assertions and
source review separately establish graph identities and conversion structure.

The four next-gate upstream file hashes still match `source-next-gates.json`.
All three size/SHA entries in `m3-built-artifacts.json` match current DLL/AOT
artifacts, including the separately retained `bin/aot-m3` executable.

## N1 closure and remaining boundaries

`DefinitionCheckpoint` changes a preexisting function definition twice and an
initially null definition twice, changes `CurrentFunction`, restores nested and
outer checkpoints, repeats a restore, rejects an abandoned nested checkpoint,
and restores an initially null current function. It asserts the actual original
links/identities. Managed and AOT execution both pass. This closes N1 before
production speculation/replay. New M4 cache/instance/context state will require
its own ownership and restoration review; this test is not exhaustive future
transaction coverage.

The supervisor should reconcile conservative old no-overload and N1
source-inspected-only wording in `SUPPORT.md`, `PROGRESS.md`, and
`docs/CORRESPONDENCE.md` with this checkpoint. Those stale restrictions are not a
claim of broader implementation and do not block this code acceptance.

References, floating/pointer/class conversions, member lookup/functions,
constructors/initialization, namespaces/ADL, block-scope functions, defaults,
ellipses and general overload contexts remain outstanding. Templates, dependent
lookup, real cached-token instantiation, constexpr execution and required
constant evaluation are not implemented or approved by these tests. Full M1,
M3, M4, M5 and M6 requirements continue under the full plan.
