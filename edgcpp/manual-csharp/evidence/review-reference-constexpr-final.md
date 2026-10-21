# Independent final review — bounded reference/object constant storage

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21.
**Accepted within the implemented and executed scalar-storage bounds.** No
known unresolved critical/high finding remains. This is not full constexpr,
reference/lifetime, M3/M4 or campaign completion. The full PLAN continues.
Isolated Unicode implementation/validation receives no acceptance here.

The reviewer read actual produced source/tests, the authoring handoff, upstream
conditional branches and executed receipts. No reviewer implementation edits,
builds/tests, native/compiler execution or Git operations were performed.

## Independently audited execution

- `reference-constexpr-build-round2.json`: Release succeeds, zero warnings/errors.
- `reference-constexpr-tests-round2.json`: direct dotnet-hosted DLL,
  **102/102 groups, zero failures/skips** (95 baseline plus seven storage groups).
- `reference-constexpr-nativeaot-publish-round2.json` and
  `reference-constexpr-nativeaot-tests-round2.json`: Linux-x64 publication/run
  succeed; **102/102 groups, zero failures/skips**.
- All four **96-file maps are identical and match held disk bytes**. The maps
  include all source/tests/fixtures and three explicit oracle/receipt tools.
  `reference-constexpr-held-snapshot.json` has the same map. All three output
  sizes/SHA256s in `reference-constexpr-built-artifacts.json` match actual bytes.
  All four upstream SHA256s in `source-reference-constexpr.json` match.
- Twenty exact `native-reference-constexpr-fixture-*-round1.json` receipts
  match current input hashes and expectations: **11 accept0, nine reject2**.
  Catastrophe/tool failure is not counted as rejection. The included combined
  type witness also accepts0, with matching witness/compiler/additional-input
  hashes and the explicit strict-C++20, no-code-gen, instantiate-used profile.

Round1 is retained: clean build,101/102 groups; the sole failure was the obsolete
expectation that alternative-spelling `1 and 2` was Unsupported. Comparing maps
independently shows **only tests/EdgSharp.Tests/Program.cs changed** before round2.
The corrected test inspects AndAnd/OrOr bool nodes and the actual true result.
There was no production repair between the executed rounds.

## Canonical finding closure

| ID | Disposition | Actual fix and executed proof |
| --- | --- | --- |
| RCE-01 | Disproved review hypothesis | Ordinary const-reference late-body use accepts natively. No erroneous declaration-time eligibility gate was added. Historical expected-reject mismatch remains excluded. |
| RCE-02, high | Closed | Potentially constant scalar initializers replay original typed recipes when loaded; historical optional failure does not forbid a later visible body. Late-body positive, required constexpr negative, original selected-long entity, mutable-read and cycle assertions execute. |
| RCE-03, high | Closed within tested parameter transport | Unknown reference-parameter locations travel through reference calls without an invented value/identity. Definition-context transport succeeds, required reads fail, and invocation-time known storage stays distinct. |
| RCE-04, high | Closed | Scalar location formation does not initialize/read the object. Deferred loads use an in-progress guard. Optional/required initializing-address positives, uninitialized snapshot facts and self-load negatives execute. |
| RCE-05, medium | Closed | Unknown lifetime, nullable identity/liveness/initialization, no object Symbol/owner, and separate UnknownReference parameter provenance replace fabricated automatic-object facts. Direct graph assertions execute. |

The initially assumed scalar late-body rejection and unknown persistent-reference
acceptance were also disproved. Their exploratory mismatches are retained and
excluded. Corrected exact fixture receipts establish scalar late-body accept0
and required unknown-reference initializer reject2. Unknown transport therefore
does not imply persistent unknown reference constants or complete P2280 support.

## Produced semantics and source correspondence

ConstantStorage separates scalar values from locations. Bindings transport
locations; typed lvalue-to-rvalue nodes perform loads. Mutable static addresses
can be retained while their runtime values remain unreadable. Public results
are immutable evaluation-local observations; interpreter cells never escape.

All arguments finish before callee parameters are installed. Value parameters
own fresh callee cells, while reference parameters/results preserve caller
locations. Recursive same-Symbol controls inspect separate invocation identities
and values7/10/12/13. Caller argument temporaries survive a callee through their
full expression; local/by-value/return-created objects expire before later
caller loads. Aliases preserve the original cell/owner without re-extension.
Discarded glvalue calls execute without loading their result. Early returns,
nested failure, limits and interpreter reuse clear storage/work/call state and
reset per-evaluation counters. Retained/disposed-session queries and live-helper
WeakReference collection controls execute alongside prior session ownership tests.

The combined source actually connects expanded RET tokens, char16/long template
instances, frozen earlier const-reference overloads, later void overload/caller
hiding, local aliasing and required constexpr values. Managed assertions inspect
selected candidates/replay/storage/origins. The native witness includes those
unchanged bytes and distinguishes char16/long result types, const-reference
return types and the later mutable-argument void overload.

Actual correspondence includes interpret.c address/reference/load dispatch,
live sets, process_call_work's two-phase argument setup and return-before-unwind;
exprutil.c potential-variable/var_constant_value and il.c dynamic initializer
folding explain replay and recursion protection. Logical &&/|| check both source
operands but schedule a value read of the RHS only when required, following
interpret.c31659ff and expr.c35086ff. Temporary-address caching, classes, pointer,
this/array/unknown-object and dialect/C++26 branches are not fully ported.

## Remaining bounds and commit documentation

Assignment, branch/loop statements, dependent/template reference declarations,
classes, source-level pointers and constexpr void remain explicitly Unsupported.
This bounded scalar storage model does not establish complete initialization,
cleanup/destruction, dangling-use analysis, diagnostic parity, full P2280 or
all constant-required contexts. The prior truthful extra lifetime warnings remain
an explicit adaptation. Remaining M1–M6 requirements stay open.

At report creation, SUPPORT/REFERENCES still describe the prior95/73 checkpoint.
Before committing this new checkpoint, reconcile them and the feature/source
inventory to102/96, replace superseded blanket reference-constexpr Unsupported
wording with these executed bounds, and retain all wider gaps. This report gives
the bounded semantic decision now; that documentation update requires no new
implementation cycle or campaign stop.
