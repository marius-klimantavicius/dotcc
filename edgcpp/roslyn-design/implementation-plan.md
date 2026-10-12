# Implementation and acceptance plan

Status: proposed, awaiting approval. No backend, schema DDL, build integration,
managed project, or tests have been implemented by phases 1–2.

## Roles, ownership, and repository layout

Phases 1–2 use GPT-6-Astra with high reasoning effort. The current primary agent
integrates three independent analysis agents covering lowering/integration,
semantic coverage, and C# API/provenance. They investigate pristine upstream
sources and review this design against each other.

After approval, use GPT-6.1-Sol with **high** reasoning effort as implementation
coordinator/supervisor and GPT-6.1-Sol with **xhigh** reasoning effort for
implementation agents. The supervisor owns plan adherence, schema decisions,
build scheduling, acceptance evidence, integration and commits. With four active
agent slots, keep the primary, supervisor, and two implementation workers active;
the primary supervises handoff and user communication. Backend and C# work run
in parallel once the common contract is fixed. Workers do not independently
change the shared schema, run competing compiler builds, or commit.

Proposed new files are isolated under `edgcpp/roslyn/`:

| Area | Ownership / purpose |
|---|---|
| `backend/roslyn_gen_be.c` | Native exporter; EDG-compatible C++ compilation despite `.c` suffix |
| `backend/` supporting headers | Typed adapters/statement bindings if splitting the new backend improves reviewability |
| `schema/roslynobj.sql`, schema kind catalog | Supervisor-owned versioned schema and stable discriminants |
| `build/` source scripts/configuration | Prepare pristine pinned upstream, configure the new backend target, link SQLite, invoke output wrapper |
| `managed/RoslynObj/` | Independent C# model plus read-only SQLite loader; no translator dependency |
| `managed/RoslynObj.Tests/` | API and integration tests |
| `tests/` | Export fixtures, upstream-test selection manifest, projection checks, size benchmarks |
| `docs/` | Implementation ledger, usage and measured limitations |

Generated compiler builds/databases/logs live in a new ignored artifact directory;
source-controlled `build/` scripts must not be accidentally ignored by a broad
build-output pattern. Do not reuse custom backend preparation scripts or object
artifacts from the excluded work. Use the pristine revision identified in the
design, with an independently maintained allowed build/configuration overlay.
Record its commit and hashes of core sources in build evidence. Never overwrite
the user's embedded upstream tree or revert its existing modifications.

The managed project initially follows the repository's `.NET 10` / C# 14 target,
but its public model remains its own API. Use `Microsoft.Data.Sqlite` or a
similarly thin SQLite provider; pin the actual chosen package version during M0
after checking available tooling. This does not introduce Microsoft.CodeAnalysis.
SQLite native code is an external dependency or separate C target; EDG's core
C++ compile flags must not accidentally apply to its amalgamation. The future
translator will be a separate project that references `RoslynObj`; it is not
created in these phases.

## Milestones and dependency order

| Milestone | Deliverable | Acceptance gate and commit |
|---|---|---|
| P: design | These six documents, upstream pin, coverage inventory, chosen configuration and boundaries | Cross-review consistency; commit design only; ask user for approval |
| M0: integration feasibility | Isolated stock-source build with new backend entrypoint; full macro/capability profile; separate SQLite dependency; basic object header publication | Core EDG diff remains empty; verify `back_end` timing/live IL; successfully compile representative dialect inputs; verify output wrapper; commit |
| M1: frozen format/API foundation | Complete field-to-relation/field-to-property matrix; DDL, stable enum catalog, exact scalar adapters; managed identities, availability and loader skeleton | Map every active executable union/auxiliary field and each retained metadata family; settle numeric losslessness and profile-specific semantics; hand-author a small valid object fixture; commit |
| M2: complete basic vertical slice | Native and managed handling of declarations, scopes, types, constants, expressions, control flow, selected calls and source coordinates | Real EDG-produced object navigated through API and direct SQL; overload/types/conversions/exact constants/jumps and declaration ordering demonstrated; commit |
| M3: C++ object semantics | Initialization, lifetime, exceptions, class layout, inheritance/virtual relations, templates/instances, lambdas, allocation and destruction | Export and query normal/exceptional cleanup obligations, partial construction, virtual bases/covariance, instantiations/origins, new/delete and captures; commit |
| M4: full configured surface/provenance | Every remaining enabled kind/profile, coroutine protocols, GNU/MS and other upstream extensions, optional template retention profile, macros/source history | Exhaustive adapter coverage for accepted modes; selected upstream suites pass export/read checks; unsupported exporter kinds are zero in completion matrix; commit |
| M5: scale and packaging | Large-TU measurements, lazy/batched API, multi-object isolation, deterministic logical export, user documentation | Data normalization demonstrated, no loss of required data for size, bounded read behavior, independent TU identities, full supported profile matrix and remaining upstream retention limits published; commit |

M2 and M3 are reviewable milestones, not claims that the requested full surface
is complete. Phases 3–4 finish only after M4/M5 acceptance; rare enabled extensions
cannot be silently postponed under a claim of “all EDG support.” There are no
calendar estimates until a working isolated build establishes cost.

### M0: prove the approved boundary

1. Create a clean staging checkout and the new backend target with a consistent
   macro set across all its EDG translation units. Apply the flags and separate
   compatibility profiles in [lowering-and-integration.md](lowering-and-integration.md).
2. Confirm baseline no optional IL lowering, no allocation folding, full semantic
   and macro retention available in that profile, no unwanted needed-entity
   pruning, and preserved instantiation state. Capture resolved macro values,
   rather than trusting intended CMake definitions.
3. Use tiny valid inputs to observe live file/function IL and available template,
   layout, lifetime and macro fields at `back_end`. Do not invoke semantic routines
   to manufacture absent records. Do not read freed front-end memory.
4. Resolve compatibility-profile dependencies before claiming coverage: ordinary
   independent templates, legacy exported templates/definition discovery, legacy
   assignment-to-this with folded allocation, GNU versus Microsoft ABI settings,
   optional managed/UPC/vector features supported by upstream. Do not infer
   CUDA/GPU support from EDG's use by other compilers: the pinned IL inventory
   does not establish such nodes. Macro-tree
   availability may differ for modes involving EDG secondary translation units.
5. Verify available exact numeric helpers and representations for integer extremes,
   extended floating formats, signed zero, infinities and NaNs. Define relational
   scalar extraction without host-width truncation or display-only conversion.
6. Keep all SQLite resources within backend-owned lifetime. Use a fresh connection
   and identity map per invocation. Prove success publication and ordinary write
   error cleanup. Stock `-o` is not directly available to this custom resident-IL
   backend: expose `ROSLYNOBJ_OUTPUT`/derived filename plus an external wrapper.

Any requirement that cannot be met without modifying core EDG is reported with
the upstream evidence and proposed contract consequence. This gate authorizes
configuration experiments and the new backend, not edits to parsing, lowering,
layout, lifetime or other semantic code.

### M1: freeze what the two implementations share

Create a coverage matrix with one row per active union arm and relevant common
field: upstream owner/discriminator/field, semantic meaning, scalar or typed
relation, managed property, availability condition, source citation, test case.
Mark omitted fields with a concrete reason (parser-only/cache/redundant list
plumbing/inactive configuration), not “unused.” The design's exhaustive kind
ledger is the starting point, not a substitute for this mechanical field audit.

Freeze keys, cardinalities, units, null meanings, stable kinds, containment versus
reference roles, source coordinate conventions, literal precision, synthesized
flags, and backward compatibility. Keep a single DDL and kind catalog; generate
or embed copies for C/C# mechanically. SQL strings and managed enum numbers must
not drift independently. Supporting code generation is a build concern, not
additional semantic processing.

The native worker then owns extraction/storage; the managed worker owns loading
and API. Both use the same valid small object fixture and approved SQL contract.
Schema changes require supervisor review and synchronized updates before either
worker can call the milestone complete.

### M2–M4: execute in parallel by semantic family

For each milestone the native worker projects the EDG families into the shared
schema while the managed worker implements their immutable records, visitors,
typed queries and batched loading. Integrate each family using real backend
output; fixture-only reader tests are insufficient. The supervisor reviews any
new conditional field/extension against the coverage matrix and ensures no
raw-payload fallback, EDG modification, or independent semantic resolver appears.

Source-shaped and semantic-expansion traversal have explicit policies. Lifetime
queries expose EDG records, not a computed cleanup plan. Template-origin queries
must follow retained IL relationships; parser-only symbols cannot be recovered
through the already freed front-end region. Native offsets remain target facts.

Concrete high-value acceptance examples:

| Family | Questions the exporter/reader must answer |
|---|---|
| Names/declarations | Which declarations and definition belong to this EDG routine? Which lexical scope differs from semantic ownership? |
| Calls/conversions | Which overload or conversion was selected? Which receiver, converted/default arguments and virtual/indirect target expression remain? |
| Types/layout | Is this a typedef/qualified/reference wrapper or class instance? What are its EDG field/bitfield offsets, repeated/virtual bases, overrides and adjustments? |
| Templates | Which actual specialization/arguments and originating template/member are retained? Which pattern is dependent and lacks an executable definition? |
| Lifetime | Which initialization owns this temporary and destructor? What destruction ordering, full-expression lifetime, exit checkpoint and failure cleanup did EDG record? |
| Initialization | What are the selected constructor, base/member/delegating steps, array prefix obligations, return-slot/elision and initializer-list backing lifetime? |
| Range-for | Which range/begin/end objects, resolved comparisons/increments, scopes, initializer and source body are present? |
| Allocation | Which scalar/array/aligned/placement operator and constructor were selected? Which deallocation is used on initialization failure? |
| Coroutines | Which promise and await-ready/suspend/resume operations and parameter copies are retained? Is native frame layout correctly unclaimed? |
| Exceptions/extensions | Which ordered handlers, rethrow/throw initialization, noexcept fact, Microsoft filter/finally/SEH or GNU control construct is represented? |
| Literals | Do integers beyond 64 bits, wide/embedded-NUL strings, exact floats and aggregates/repeats retain their values and distinct address identities? |
| Macros/positions | Which nested definition version/invocation and original/expansion positions contributed? Are canonical args, `#line`, repeated includes, tabs and Unicode reported honestly? |
| Multiple objects | Can two TUs with identical names coexist and be queried independently without cross-unit resolution or accidental identity merging? |

Compiler accepted inline assembly is retained with its semantic operands and
constraints; these phases do not promise it can later execute as C#. Likewise,
capturing an accepted target extension is distinct from implementing it on .NET.

### M5: quantify compactness and usability

Select representative upstream units first, then larger independent C++ inputs
available in the repository without consulting excluded campaign findings.
Document each input revision, compilation profile and command. Measure frontend
and exporter separately, plus database table/index size, atom sharing, entity
counts, reader opening/RSS and bounded traversal. Use repeated names/literals,
header templates, macro-heavy files and large aggregate/string data to expose
duplication and pathological normalized storage.

Two identical compilations need not produce byte-identical files because artifact
identity is intentionally new. Compare deterministic logical ordering and payloads
after excluding declared run-specific metadata. No pointer-address-derived IDs.
Test a collection of many independent objects without constructing a whole-program
symbol table. Demonstrate cache eviction and batched enumeration do not omit data.

## Validation policy

EDG is trusted. Tests verify extraction and reader behavior, using successful
upstream inputs and inspectable EDG facts as the reference. They are not a new
C++ conformance engine and do not second-guess EDG's overload, lifetime or layout
decisions. The C generator guides adapter coverage and may provide reference
observations where its lowered stage applies; textual equality to generated C
is not the unlowered object contract.

Use real fixture assertions, node-family coverage, selected upstream positive
tests, SQL inspection, managed query tests and scale measurements. Ordinary
foreign-key enforcement and checking complete writer output are storage
correctness, not semantic revalidation. Do not introduce handcrafted corrupted
database campaigns, malformed semantic mutations, fuzzing corrupt IL, or tests
whose purpose is to reject EDG decisions. Existing upstream negative tests can
still test stock frontend behavior where relevant; they do not become successful
serialized executable units.

No new runtime-execution translator harness is part of these phases. A readable
future C# translator needs its own lowering, runtime and execution acceptance
plan; storing a coroutine or cleanup graph does not prove that future compiler.

## Milestone evidence and commits

For each milestone record exact source revision/configuration, commands, passing
cases, measured counts/size where relevant, and remaining required kinds/profiles.
Stage only owned files; never include existing dirty EDG or unrelated changes.
The coordinator serializes Git mutations and commits every significant verified
milestone. Do not create or update a PR unless it becomes part of the user's
requested workflow. Final phase-3/4 acceptance must list optional upstream data
that remain unavailable separately from actual implementation gaps.
