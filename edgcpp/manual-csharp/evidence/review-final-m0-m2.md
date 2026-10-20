# Final independent review — bounded M0 / lexical foundation / M2

Date: 2026-10-20. Reviewer: independent implementation reviewer assigned by the
supervisor under the manual-csharp campaign instructions.

**Decision: approve the reviewed stable snapshot for the M0 baseline, the
implemented lexical foundation of partial M1, and the documented first
integrated M2 semantic slice. No identified critical/high finding remains open
for this bounded checkpoint.**

This decision does **not** approve complete M1, C++20/EDG compatibility, M3,
M4, or M6 product/platform qualification. The full manual-port roadmap remains
active. `SUPPORT.md` and `docs/CORRESPONDENCE.md` accurately delimit the actual
integer/function/lexical subset and its intentional adaptations. Supervisor
progress headings and issue statuses should now be reconciled from pending
review to this bounded decision; they are currently conservative stale status.

## What was independently reviewed

Reviewed all eight authored production C# files, both projects and standalone
solution, the deterministic test harness, all five managed fixtures, oracle and
receipt tools, support/design/correspondence/attribution records, original EDG
branches cited in the earlier reports, all seven findings and their actual
fixes, and executed validation receipts. Earlier design-only and incremental
reports did not approve the milestone.

Reviewer performed read-only inspection and hash comparison. All build/test,
native EDG and NativeAOT execution was serialized and performed by the
supervisor, as required. Reviewer changed only `evidence/review-*.md`; no
implementation edits, builds/tests/native executions or Git mutations occurred.

The implementation contains real interleaved declaration insertion, source-order
lookup, immediate expression binding/conversion and structured graph creation.
No fixture-name/value dispatch, native fallback, compiler subprocess, generated
EDG assembly/source input or dynamic-emission dependency was found in production
or the default test path. The Python tools only record/check explicitly selected
commands or source provenance; they do not generate frontend implementation.
Production source has applicable Apache-2.0 WITH LLVM-exception SPDX attribution,
and the retained notice/license identifies the EDG provenance.

## Snapshot and executed evidence verified

All five final receipts carry the **same 16-file source/test/fixture SHA-256
map**, and every entry matches the held current files:

| Receipt | Supervisor-executed result |
| --- | --- |
| `managed-build.json` | Release solution build, exit 0, 0 warnings, 0 errors |
| `managed-tests.json` | 25 named groups, 25 passed, 0 failed, 0 skipped |
| `nativeaot-publish.json` | Linux x64 NativeAOT publish, exit 0 |
| `nativeaot-tests.json` | Published executable: 25 groups, 25 passed, 0 failed, 0 skipped |
| `managed-positive-inspect.json` | Positive source succeeds without unsupported features; four declarations; prototype/definition `long (int)` |

The reviewed `managed-tests.json` file itself has SHA-256
`14f0f03d8d53c85bbb83120d5a6f8403969e5d7109b08607ccdd4b8dfbc455f5`.
Key files in its source map are:

| File | SHA-256 |
| --- | --- |
| `CompilationSession.cs` | `d8e13fba703a9a910984d94c0909c4387813e1c5406d8e2ff14e72264c747d12` |
| `Parser.cs` | `6a338117dc941bdcc69faccd6a0a8a7b6fe1dfbe2f152d7d3be49dbd3aa1f69d` |
| `Expressions.cs` | `b934a16daf65854034457d537e385116ef84d33999d61ac081afb45a606d45f5` |
| `ParseState.cs` | `cd4af101f86db24d6f977f4817b02f4ad296c46188cdfebe98508011a53a0041` |
| `tests/EdgSharp.Tests/Program.cs` | `f3a1bbd2161effd0d32c76a234ebcda22f6e9cce423fc019240505376d39d7df` |

Additionally verified:

- All **17 native receipts** have matching input/additional-input and native
  executable hashes, successful expected-status matches, and explicit strict
  C++20/profile arguments. Native controls for invalid source exit 2 with actual
  language diagnostics; they are not missing-tool failures counted as rejection.
- Native executable SHA-256:
  `e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3`.
- The **22 consulted upstream source hashes** in `source-baseline.json` match
  current working files; the imported base is not mislabeled a pristine tree.
- All **three retained artifact** size/hash entries in `built-artifacts.json`
  match disk, including the published AOT executable.
- Native `m2-semantic-witness.cpp` includes the exact hashed managed positive
  fixture and proves `Count=long`, widen/entry call result long, and widen's type
  `long (*)(int)`. Managed graph assertions separately establish identity,
  resolved references, conversion structure and scope/body relationships.
- Native profile witnesses establish the tested LP64 widths, signed plain char
  and C++20 mode. Native review-valid and review-boundaries witnesses discriminate
  the const-call, alias-void and valid-but-unsupported forms from invalid source.

The native witnesses establish their selected facts only. They do not prove
full diagnostic parity, graph equality, preprocessing support, layout or headers.

## Findings resolved in produced code and executed regressions

| Finding | Final resolution and evidence |
| --- | --- |
| R1, assignment value category | Assignment is explicitly unsupported, including nested `(x=1)=2`. No accepted assignment uses the PRValue-only binary node. Regression passes under managed and AOT execution. |
| R2, stale checkpoint ancestry | Registered checkpoint reference identity invalidates descendants before mutation. Equal-count divergent scopes, diagnostic-only stale branches, nested/repeated and cross-state cases are asserted and pass. |
| R3, scalar call const | Declared scalar return qualification is retained; the call PRValue uses canonical unqualified type. Signature/definition parameter cv remains distinct. Graph tests and native decltype witness agree. |
| R4, global main | Global function declarations/uses and global object main explicitly diagnose unsupported. All reported forms have regressions; local object main remains supported. Native invalid controls reject signatures/call/global variable. This is a scope exclusion, not implemented main semantics. |
| R5, unsupported conversion/operator taxonomy | Function decay is explicitly unsupported; implemented alternative operator spellings normalize to corresponding token kinds; logical/spaceship forms and `_custom` suffixes diagnose unsupported. Valid native controls and passing managed/AOT regressions distinguish these branches. |
| R6, directive-position taxonomy | Analyze centrally diagnoses Hash/HashHash tokens regardless of parser position. Body, declaration-interior and parameter-interior regressions pass; native accepts the source forms. Lex remains a documented tokenization-only API. |
| R7, alias-bound void parameter | A sole unnamed resolved void type contributes no parameter and merges with the empty-list function declaration. Graph tests inspect shared symbol and zero arity; named/mixed void negative controls remain errors; native accepts the valid alias form. |

Additional reviewed fixes preserve complete unnamed parameter type ranges,
explicitly diagnose direct initialization/labels/member-pointer continuations,
and expose short/bool promotion followed by common-type conversion. The latter
is documented as a richer managed query representation: EDG computes promotions
then can cast directly to the common type. No literal EDG cast-node parity or
prior arithmetic value defect is claimed.

## Correctness and ownership conclusions within the subset

- Session state and canonical tables have explicit ownership; separate sessions
  do not share mutable graph/diagnostic/cursor state. Repeated analyses retain
  independent symbols/sources while immutable canonical types can be shared
  within one session. Disposal preserves completed queries.
- Public graph/token collections use safe read-only wrappers; parameter,
  argument and body arrays are copied. Semantic nodes use reference identity,
  not mutable-record structural equality. Canonical prototype/definition symbols
  survive later declarations; parameter/body restrictions and nested shadowing
  follow the reviewed EDG branches.
- Token cursors have stable EOF, source-preserving retained intervals and
  cursor-owned checkpoints. Semantic rollback is separately owner-mediated;
  lexical replay is not advertised as template instantiation.
- LP64 type identity distinguishes long from long long despite equal widths.
  Promotions, signedness/rank/representability, literal candidate ordering and
  width conversions are explicit. BigInteger is bounded by those rules;
  unsigned arithmetic wraps and undefined signed/division folds withhold values.
- Negative/malformed/unsupported tests start from source text, inspect failures,
  exercise recovery and verify following declarations remain visible. Successful
  tests inspect actual selected symbols/types/conversions/bodies, not just
  acceptance or emitted text.

## Explicit deferrals and one nonblocking coverage limitation

M1 remains **partial**: no macro expansion, includes, conditionals, rescan
suppression, logical source remapping or complete literal validation. Lex scans
number-shaped tokens without complete numeric validation and quoted boundaries
without full literal semantics; SUPPORT states this explicitly. There is no
claim of assignment, global main semantics, pointers/references/classes/overloads,
templates/constexpr, complete constant-expression classification, headers,
lifetime/layout, exceptions, modules or broader compatibility.

**N1 — Medium validation limitation, deferred before production speculation:**
the checkpoint test exercises scope/symbol/declaration mutation, cursor,
diagnostics and pending replay, but does not directly exercise `SetDefinition`
or `CurrentFunction` restoration. Those two branches were source-inspected.
The current parser has no production speculative-checkpoint path, so this is not
a correctness blocker for the accepted semantic slice. Before activating such
paths, add a test that changes a preexisting definition link/current function,
restores nested/outer checkpoints and verifies identities and original links.
Do not cite the current test label as exhaustive coverage of future mutable
parser state or arbitrary direct graph-property mutations.

Cancellation, recursion/resource bounds, throughput/memory, all target models,
platform qualification beyond this Linux x64 NativeAOT smoke and the difficult
M4 gate remain outstanding. These are explicit roadmap gaps, not established
passes. No additional open critical/high objection is deferred as a scope gap
in this approval.
