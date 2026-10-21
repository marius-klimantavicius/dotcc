# Independent review — bounded early M4 difficult-path gate

Date: 2026-10-21. Reviewer: `/root/manual_edg_supervisor/review_m3`, continued
independent reviewer under the manual frontend campaign instructions.

**Decision: approve the held snapshot for the implemented early M4 gate. It
contains a real coupled path through template definition analysis, deduction,
retained-token rescanning, concrete instances, frozen ordinary lookup, overload
participation and required constexpr execution. No identified critical/high
finding remains open within the documented bounds.**

This approval does not complete the full template/constexpr surface, full M3/M4,
other milestones, or the campaign. The full `PLAN.md` and feature inventory stay
active. Continue the remaining assignments automatically; a partial checkpoint
is not a final campaign handoff.

## Review scope and independence

Reviewed the actual new `Templates.cs` and `ConstantEvaluation.cs`, changed
parser/expression/type/graph/session/rollback/token paths, all 13 new M4 test
groups, nine fixtures, native inspector/control sources, oracle adapter and
controls, execution receipts and retained artifact hashes. Independently read
the relevant actual EDG implementation branches; baseline references and
intermediate findings are in `review-m4-baseline.md` and rounds 1–2.

Reviewer changed only assigned M4 evidence reports. Supervisor performed every
build/test, native EDG invocation, NativeAOT operation and Git mutation; reviewer
performed only source/evidence reads and hash comparisons. No production native
fallback, generated semantic input/source, compiler subprocess, fixture-specific
semantic dispatch, dynamic code generation, or alternate AOT semantic path was
found. New production code has applicable upstream-derived attribution.

## The implemented difficult path is coupled

`m4-combined-gate.cpp` is parsed as actual source. It defines a constexpr long
helper, a dependent identity template with a local, and a dependent combine
template whose local initializer calls identity and whose return calls the
earlier helper with a nondependent argument. A later int helper overload and a
caller-local object of the same name cannot affect the earlier template lookup.
The source then requires three actual constexpr results, including int and long
template calls and reuse of an existing instance.

Managed assertions establish all of the following together:

- The template pattern contains dependent operator/conversion/call facts and
  source-owned cached body tokens with braces and stable replay EOF.
- Concrete int/long combine and identity instances are distinct and each body
  is rescanned exactly once; repeated int calls reuse the same canonical
  instance and selected symbol. Actual concrete scopes, parameters and bodies
  come from fresh-cursor rescanning with substituted types.
- The pattern's earlier ordinary helper has one visible candidate. Every
  concrete combine body selects that same long helper identity despite the
  final global binding having two overloads and the caller containing a local
  object with that name.
- Required evaluation executes nested calls and local initialization, produces
  three successful bool results, and records nontrivial work/depth statistics.
  It does not substitute optional parser folds for call execution.

The native semantic witness includes the exact hashed combined fixture. Its
static assertions independently prove short/int/long identity result types,
repeated-pattern versus generic direct-pattern selection, and ordinary preference
on equal conversions versus template preference with better conversions. The
separate unordered-pattern control rejects with a real ambiguity diagnostic.
Native facts establish their chosen values/types; managed assertions and code
review separately establish graph and cache identities.

## Algorithm and ownership conclusions

Definition-time prototype analysis scans every retained body, including unused
templates, and diagnoses nondependent unresolved names, invalid calls and illegal
template-parameter redeclarations. Type dependency and value dependency are
separate; a known bool result can still contain an unknown template value.
False static assertions in template prototypes defer under the pinned EDG
CWG2518/P2593R1 branch; nonconstant/malformed conditions do not gain that exception.

Deduction uses unqualified actual by-value integer types without arithmetic
promotion. Repeated parameter identities must agree; nondeduced fixed parameters
convert only after substitution. Short remains short/exact, and short plus int
does not satisfy repeated T. Candidate substitution is separate from body
instantiation: a losing template gets no body instance/error, while the selected
invalid dependent return produces a failed, diagnosed instance.

Scalar pairwise comparison still requires no worse conversion on every argument.
Ordinary preference applies only when conversions tie. Direct by-value patterns
receive the implemented bidirectional deduction ordering; crossed or unordered
patterns remain ambiguous. General template ordering is not claimed.

Each template owns its immutable definition-binding snapshot and argument-keyed
instance collection. Function/entity/source identities remain explicit. The
session interns only concrete signatures; dependent signatures are retained by
their template owner. A non-inlined WeakReference test proves a dropped template
TU can collect while its session remains alive. Repeated analyses retain
independent templates, parameters and instances while sharing only canonical
concrete immutable types.

The integer interpreter uses explicit work/value/call stacks and per-call maps
keyed by symbol identity. It executes supported blocks, locals, expressions and
returns; distinguishes failure/dependency/undefined operations/resource limits;
and resets both stacks and counters per invocation. Tests cover early return,
shadowed locals, nested failure followed by reuse, uninitialized/self reads,
mutable globals, non-constexpr/undefined calls, unsigned wrapping, signed
overflow, min/-1 division and remainder, and call/work/instantiation limits.
Discarded ordinary nonvolatile references do not become value reads.

Replay states share their root's mutation journal. New cache membership, instance
state/declaration/replay counts, definition links, scopes, declarations, dependent
value flags and assertions have owner-mediated mutations. Root rollback tests
restore actual identities and remove a replay-created instance/body. Child
Checkpoint/Restore is explicitly prohibited; this is a bounded safe contract,
not support for arbitrary cross-cursor speculative transactions.

## Canonical findings and closure ledger

The following IDs are authoritative for this checkpoint. Historical reports
remain unchanged; the alias column resolves the provisional R3 collision.
Every listed code closure is present in the held source. Relevant production
regression groups passed in both managed and NativeAOT execution; oracle adapter
controls were separately executed as described below.

| Canonical ID | Severity / historical alias | Final closure |
| --- | --- | --- |
| M4-01 | High; round 1 R1 | Dependent/scope-owning signatures bypass the session intern list. WeakReference collection and repeated-template isolation pass. |
| M4-02 | High; round 1 R2 | Lexical template-parameter checks cover initial parameters and nested locals; the function name is checked after the template scope is popped. Unused invalid definitions diagnose. |
| M4-03 | High risk addressed; supervisor provisional R3 | Separate type/value dependency and explicit dependent-node interpretation preserve concrete bool types with dependent values. Const dependent locals and known-type arity errors have controls. |
| M4-04 | High; round 2 R3 | Discarded nonvolatile lvalues/function designators are not read or called. Required constexpr control succeeds; actual uninitialized reads still fail. |
| M4-05 | High; round 2 R4 | `constexpr using` and `constexpr typedef` diagnose illegal combinations instead of silently creating aliases. |
| M4-06 | High contract issue; round 2 R5 | Static assertions use explicit bool conversion. Valid excluded function decay is diagnosed unsupported rather than only invalid-source NotConstant. |
| M4-07 | High contract issue; descriptive follow-up | Unnamed template type parameters and pre-name packs diagnose unsupported. They no longer fall into only an identifier syntax error. |
| M4-08 | High; descriptive follow-up | A sole function-template name is an indefinite overload designator. Standalone use cannot become a successful selected ordinary-function expression. |
| M4-09 | Unsupported-bound consistency | Ordinary and template constexpr-void forms explicitly diagnose the current integer-evaluator bound. Full void execution remains required later work. |
| M4-10 | High; required-initializer follow-up | Const/constexpr initializer parsing activates required constant context; selected constexpr template bodies can instantiate during prototype analysis. Both unused-outer initializer controls pass, including following static_assert. |
| M4-11 | Preventive medium state bound | Replay child checkpoints/restores are explicitly rejected; root rollback journals replay/cache/instance mutations and restores them in executed tests. |
| M4-12 | Medium helper contract | Each Evaluate resets steps/depth and clears work/value/call state on success or failure. Same-interpreter reuse after early return and nested failure passes with fresh statistics. |
| M4-13 | High validation gap; oracle-profile discovery | Oracle commands explicitly use `--instantiate used`; only normal 0/language-error 2 match. Seven adapter unit controls and four explicit selected/unused/losing native integration controls pass. |

M4-09, -11 and -12 were consistency/preventive requirements, not claims that
the then-current ordinary production path already exhibited every hypothetical
failure. Native source checks established the language facts before the final
code decisions. No extra open high objection is hidden as an unsupported gap.

## Held snapshot and executed evidence verified

All four receipts contain the **same 34-file source/project/test/fixture SHA-256
map**, and every entry matches the held current files:

| Receipt | Supervisor-executed result |
| --- | --- |
| `m4-build-round1.json` | Release build succeeds, zero warnings/errors |
| `m4-tests-round1.json` | 48 named groups pass, zero failures/skips |
| `m4-nativeaot-publish-round1.json` | Linux x64 NativeAOT publish succeeds |
| `m4-nativeaot-tests-round1.json` | Published executable passes all 48 groups, zero failures/skips |

The inventory is the previous 35 groups plus 13 M4 groups, with multiple
source/graph assertions in each group. `m4-tests-round1.json` SHA-256 is
`ef85f31c6693e60ead1f330d4cdc6b35ebac216d05aa3f7306ebfb605af7854a`.

| Key file | SHA-256 |
| --- | --- |
| `Templates.cs` | `0d3d38e8971b44ab57ff461a44425bf1b07589422e57a62925eb7f49dc4d6bd3` |
| `ConstantEvaluation.cs` | `684bf076aec0e508b700576f3a876d9261bf95729f1c8c5c202e3912fa8e63e7` |
| `Types.cs` | `6544b3c28819fd57b0771f505e41a0c7122e0c28cf694a82151fd0d524693c61` |
| `Parser.cs` | `c7a1c56964cdefb6f0907e369d9e1d65c17c783d31910c90081da9b3a57a81f0` |
| `Expressions.cs` | `374447a979a203d2ac10155253e0cc5433a3aa528424a2f42ffa516af937f6e9` |
| `tests/EdgSharp.Tests/Program.cs` | `c7345f9e58d7a6d6d6358f8a6446b0c603dd1d87a5e9e47575ca916ac4f8d798` |

All three artifact size/hash entries match `m4-built-artifacts.json`, including
the separately retained `bin/aot-m4` executable. All nine exact fixture receipts
`native-m4-*-fixture-used.json` match their current fixture and compiler hashes,
explicit used profile, expected status and actual language diagnostics. The
semantic witness/additional included fixture and unordered-pattern receipts also
match. The 12 implementation hashes in `source-m4.json` match disk.

The adapter's seven-unit receipt fingerprints the current `oracle.py` and
`test-oracle.py`; four `oracle-integration-*.json` receipts match current
inputs/compiler and discriminate selected false/invalid bodies from unused false
assertions and losing templates. Test-only native invocation remains explicit
and absent from the default managed harness.

Two exploratory expectation mismatches are deliberately **excluded** from
acceptance: `native-m4-selected-false-assert-contract.json` exposed the old
default-none profile, and `native-m4-nonconstexpr-definition-used-contract.json`
disproved an expectation of an immediate diagnostic on an unused constexpr
definition calling a non-constexpr function. They are retained failures of
those expectations, not passing negative controls. Required non-constexpr-call
rejection is separately tested. Earlier default-none controls are not evidence
for selected nonconstexpr template-body checking.

## EDG correspondence and explicit remaining work

Reviewed the bounded branches of `templates.c` caching/prototype/concrete body
rescanning and deduction wrapup; `overload.c` adjustment/pair deduction/call
deduction and comparison; `lookup.c` declaration ordering; `lexical.c` retained
token rescan contracts; `interpret.c` work/call/local/cleanup/failure paths; and
`decls.c:static_assert_declaration`, including CWG2518/P2593R1. The intermediate
baseline gives routine names/line references and conditional omissions. Oracle
status follows `host_envir.c:exit_compilation` and `host_envir.h`; external
instantiation follows `ext_intf.rst:1649+` (document SHA-256
`1617b34a82eb53c671ed2b894df8cb793a641c6c75abebc056338d727f104d98`).

The supported forms are header-free direct named type-parameter free function
template definitions, by-value integer/fixed-integer parameter patterns, bounded
direct-pattern ordering, frozen definition-visible ordinary calls, and the
straight-line integer constexpr interpreter/required contexts described above.
The source and feature ledgers must be reconciled from pending M4 status to
these exact accepted bounds.

Template declarations/redeclarations without supported definitions, explicit
arguments/instantiations/specializations, unnamed parameters/packs/defaults,
non-type/template-template parameters, references/pointers/classes, ADL, general
dependent lookup, general partial ordering/specialization, SFINAE/constraints,
constexpr void/message literals, control flow, objects/lifetimes/storage and
broader constant contexts remain outstanding. Preprocessing and real headers,
other targets/platforms and full resource/product/cancellation qualification
remain required. Do not count compile/link/run native-only testcase modes as
frontend-only passes or discard upstream testcase metadata when scaling.
