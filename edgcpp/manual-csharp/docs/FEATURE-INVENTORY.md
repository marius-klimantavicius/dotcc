# Full-plan feature and qualification inventory

This is the active completion ledger for `PLAN.md`, not a smaller replacement
scope. Milestones and commits are checkpoints. Every open/partial row remains
required work; diagnosing a construct unsupported is not implementing that row.
The first profile is C++20/LP64, not the final compatibility boundary. Claims
about additional dialects/configurations require their own explicit evidence.

Status meanings: **accepted slice** has named code, executed checks and independent
review for only the stated bounds; **active** is being implemented/reviewed;
**partial** has substantial remaining branches; **open** has no accepted complete
implementation; **unverified** lacks executed qualification. No percentage is
computed from these coarse rows. Each row must split into routine/branch tasks
as it becomes active, preserving unsupported branches until they are implemented.

Each acceptance needs source-text positive/adversarial/negative tests, explicit
target/dialect input, actual graph assertions, native oracle/witness evidence,
upstream routine/conditional-branch correspondence and independent review.
Native acceptance alone never proves graph facts. Unavailable platform checks
remain unverified or evidenced external blockers, never successful skips.

`UPSTREAM-SOURCE-INVENTORY.json` indexes every live `.c`/`.cpp`/`.h` under
`upstream/src/` with SHA-256, including ignored/generated tables and IFC files.
It is provenance metadata, not generated frontend
code or a whole-file port claim. Routine mappings remain in CORRESPONDENCE and
per-subsystem records. The upstream documentation chapters listed below are
source guides; implementing a few mapped routines does not cover the chapter.

## M0: baseline, contracts and verification infrastructure

| ID | Required task / current bounds | State | Concrete acceptance / remaining evidence |
| --- | --- | --- | --- |
| M0.1 | Standalone net10/C#14 library and deterministic harness | accepted slice | Existing independent solution builds; future changes keep default tests independent of native compiler/generated frontend |
| M0.2 | Source baseline, attribution and correspondence | partial | Retain source/executable hashes and Apache/LLVM notices; map every added routine's invariants/conditional branches, including omitted branches; full upstream coverage audit remains open |
| M0.3 | Independent ownership and retained query graph | accepted slice | Separate sessions and repeated analyses isolate symbols/cursors/diagnostics; disposed sessions preserve prior facts; extend tests for every new cache/state owner |
| M0.4 | Explicit diagnostic and result contracts | partial | Invalid/unsupported distinctions and diagnosed partial graphs tested for current subset; complete error recovery and source provenance remain open |
| M0.5 | Native oracle profile and semantic witness protocol | accepted slice | Strict C++20 LP64 witnesses and exact-byte fixtures retained; each future subsystem adds discriminating fact witnesses and invalid controls |
| M0.6 | Independent code/evidence review and serialized commits | active | No unresolved critical/high finding accepted; reviews must inspect produced code and executed matching-hash receipts; workers never commit/build concurrently |

## M1: source, lexer and integrated preprocessing

Source guides: `lex_pp.rst`, `includes.rst`; `lexical.c`, `preproc.c`, `macro.c`,
`literals.c`, source/position structures in `il_def.h`.

| ID | Required task / current bounds | State | Concrete acceptance / remaining evidence |
| --- | --- | --- | --- |
| M1.1 | Owned buffers, physical positions, retained token intervals | accepted slice | Exact ASCII token spelling/kind/UTF-16 range tests, CR/LF/CRLF, stable EOF; preserve these through macros/includes/replay |
| M1.2 | Encoding and translation phases | partial | 79-group snapshot covers real-context raw phase restoration and strict opt-in UTF-8/BOM UTF-16 input decoding; explicit source/execution encoding metadata, Unicode/UCN identifier rules and other encodings remain required |
| M1.3 | Full literal lexing and values | partial | Bounded target character types/ASCII/simple/octal/hex/multichar values and diagnostics pass; Unicode/UCN, full string/raw/prefixed value semantics, concatenation, floating and user-defined literals remain required |
| M1.4 | Object-like macros, suppression and rescanning | partial | ASCII strict-profile nesting/inertness/caller-tail/provenance and isolated state pass; Unicode macro identifiers and broader configured dialect/corpus qualification remain |
| M1.5 | Function-like/variadic macros and operators | partial | Lazy raw/expanded args, #/##/placemarkers/VA_OPT and full 32-label upstream matrix pass; Unicode/complete dialect/configured branch qualification remains, including explicit native unused malformed-raw laziness bound |
| M1.6 | Include processing and search configuration | partial | Configured in-memory/structured opt-in Linux-x64-glibc provider search, separate probe/read caches/errors, guards/once, limits, strict decoding and retained activation origins pass; other hosts/encodings and real headers remain unqualified |
| M1.7 | Conditional directives and PP arithmetic | partial | Conditional nesting/defined/alternative operators/short circuit, numeric intmax/uintmax64 shifts, witnessed character/bool32 promotions and expansion-driven __has_include header context pass; remaining special operators/dialects still required |
| M1.8 | Other directives and predefined macros | partial | define/undef/ordinary line/error/once and line/file/cplusplus/date/time/hosted pass, with immutable configured UTC environment; broader predefs/pragma/extensions remain; nonordinary #line anomaly has exact receipts/explicit bound |
| M1.9 | Lexical/token-cache state restoration | partial | Cursor and owner-mediated checkpoint foundation tested; nested rescan/suppression/error positions and every production speculative mutation need direct rollback tests |
| M1.10 | Preprocessing differential suite and provenance API | partial | 79-group snapshot expands exact upstream/native matrix with character→template→overload→constexpr graph, ABI/error/encoding/search/probe/disposal/concurrency and bounded special-file controls; remaining lexical/header/dialect branches required; native frontend remains oracle only |

## M2: declarations, fundamental graph and statements

Source guides: `decls.rst`, `symbol_tbl.rst`, `expr.rst`, `statements.rst`, `il.rst`.

| ID | Required task / current bounds | State | Concrete acceptance / remaining evidence |
| --- | --- | --- | --- |
| M2.1 | Integer/void/const types and canonical entities | accepted slice | Current aliases, scalar widths/ranks, prototype-definition identity and source declarations pass; preserve identity while expanding types |
| M2.2 | Complete fundamental type/qualifier/declarator forms | partial | Floating/extended types, volatile/restrict/atomic dialect rules, pointers/references/arrays/function/member types, compound/multiple declarators and invalid forms |
| M2.3 | Scope/lookup source order | partial | Current lexical nearest binding/shadowing; extend namespaces, using directives, qualified/elaborated lookup, linkage, tags, active/inactive/other symbol distinctions and declaration sequence |
| M2.4 | Function declarations and parameter forms | partial | Defaults, variadics, storage/linkage/qualifiers, block-scope declaration semantics, `main` special rules, matching redeclarations and source ranges |
| M2.5 | Expressions, categories and conversion graph | partial | Current integer arithmetic/comparison/bitwise/unary/calls; assignment, logical/shift/conditional/comma, casts, pointers/member access, decay, sizeof/alignof/typeid and every new category/conversion |
| M2.6 | Statements/control flow | partial | Current blocks/returns/empty/declaration/expression; if/switch/loops/break/continue/labels/goto, scoped conditions, invalid transfers and structured bodies |
| M2.7 | Initializers and declaration semantics | partial | Current copy-initialized scalar; direct/list/aggregate/designated initialization, deduction, constexpr/storage/linkage/tentative dialect rules and illegal combinations |
| M2.8 | Source-text graph semantic qualification | accepted slice | Current bounded M2 fixture and negative controls pass under managed/AOT plus native witnesses; broader forms remain rows above |

## M3: overloads, references and classes

Source guides: `class_decl.rst`, `expr.rst`, `decls.rst`; `overload.c`,
`class_decl.c`, `class_decl.h`, `decl_inits.c`, `lookup.c`, `types.c`.

| ID | Required task / current bounds | State | Concrete acceptance / remaining evidence |
| --- | --- | --- | --- |
| M3.1 | Ordered scalar free-function overload sets | accepted slice | Separate binding/entity identity, frozen candidates and scalar comparison pass 35-group suite/native witnesses; full M3 branches remain below |
| M3.2 | Complete standard conversion sequences | partial | 95-group integration covers scalar reference viability/rank/kind/cv and ambiguity with exact native witnesses; pointer/member/floating/null/deep qualification/class/UDC branches remain required; no summed heuristics |
| M3.3 | Lvalue/rvalue references and cv binding | partial | Scalar const/lvalue/rvalue/temporary/alias-collapse/category/owner/invalid controls pass managed+AOT; volatile, compound/class/dependent reference forms and full lifetime semantics remain required |
| M3.4 | Records/classes/member lookup | open | Class scopes, fields, member functions, access/static members, complete/incomplete types, qualified/hidden members, identity and graph assertions |
| M3.5 | Constructors and initialization overloads | open | Direct/copy/list/aggregate candidate phases, explicit/deleted/defaulted functions, initializer-list preference and invalid narrowing controls |
| M3.6 | Operators, conversions and advanced candidates | open | User-defined conversions, operators, ADL, surrogate/builtin candidates, deleted/inaccessible resolution and scoped overload rules |
| M3.7 | Special members and object model | open | Copy/move ctor/assignment, destructor synthesis/deletion/triviality, bases/fields and actual operation/lifetime facts |
| M3.8 | Inheritance, virtual functions and RTTI | open | Multiple/virtual bases, lookup/ambiguity/access, override/final/pure rules, casts/typeid and target layout/vtable facts promised by API |
| M3.9 | Class/overload regression and header qualification | open | Adversarial branch tests, exact selected callees/conversions and invalid controls, then controlled/real record-heavy headers |

## M4: early coupled template and constant-evaluation gate

Source guides: `templates.rst`, `expr.rst`; `templates.c`, `overload.c`,
`lookup.c`, `interpret.c`, `lexical.c` reusable cache routines.

| ID | Required task / current bounds | State | Concrete acceptance / remaining evidence |
| --- | --- | --- | --- |
| M4.1 | N1 direct checkpoint branch regressions | accepted slice | Nested/outer definition links/current function, null/original identities, stale/cross-state paths and overload membership restoration pass managed/AOT; each new replay state needs branch checks |
| M4.2 | Function template declarations and dependent entities | partial | Real template parameter/type/operand variants, definition-time nondependent analysis including unused-template errors; unsupported recipe forms never success placeholders |
| M4.3 | Retained token body caches and rescanning | partial | Preserve exact body tokens/source, scope activation/restoration and nested replay EOF; instantiate by rescanning with substitutions, no generated/native frontend |
| M4.4 | By-value deduction and instance identity | partial | int/long distinct instances, repeated parameter conflicting deduction failure, canonical argument-keyed caching, no arithmetic promotions during deduction |
| M4.5 | Definition/instantiation lookup contexts | partial | Earlier helper overload set frozen for nondependent calls despite later declarations and caller locals; dependent lookup/ADL exercised as supported forms arrive |
| M4.6 | Template overload participation | partial | Viability/deduction failure and template-vs-nontemplate tie, ambiguity controls and later template partial ordering; inspect actual selected instance |
| M4.7 | Real constexpr execution | partial | Explicit evaluator work/call/local state, target arithmetic/invalid operations, call/recursion/cost limits and separate evaluation outcomes; optional folding is not execution |
| M4.8 | Constant-required contexts | partial | Instantiated constexpr call used in static_assert/constant-required declaration, true/false/nonconstant/invalid controls and actual computed graph fact |
| M4.9 | Early difficult-path integrated acceptance | accepted slice | Combine declaration/cache/deduction/rescan/overload/definition-context lookup/constexpr in header-free source, distinct instances and failure controls; independent review+native witness |
| M4.10 | General templates, specialization and constraints | open | Class/variable/alias/member templates, packs, defaults, explicit instantiation/specialization, partial ordering/specialization, SFINAE/constraints/concepts; tracked separately from simple deduction |

## M5: language breadth, lifetimes, targets and realistic headers

Source guides: `class_decl.rst`, `templates.rst`, `exceptions.rst`, `modules.rst`,
`pragma.rst`, `ext_intf.rst`; corresponding sources remain in upstream index.

| ID | Required task / current bounds | State | Concrete acceptance / remaining evidence |
| --- | --- | --- | --- |
| M5.1 | Object lifetime and cleanup semantics | open | Construction/destruction order, temporaries/materialization, lifetime extension, new/delete, storage duration and explicit cleanup graph under normal/exception paths |
| M5.2 | Exceptions and noexcept | open | Throw/rethrow/catch matching, cleanup propagation, exception specifications/noexcept and invalid controls; consumer graph retains source semantics |
| M5.3 | Lambdas/local classes/captures | open | Capture lookup/lifetimes, generic/mutable/constexpr lambdas, call operators and conversion rules; source and selected entities retained |
| M5.4 | Templates/constraints/constexpr breadth | open | Complete M4.10 branches, dependent semantic recipes, constant evaluation of objects/control flow/all supported standard contexts, diagnostics/limits |
| M5.5 | C++ initialization/deduction breadth | open | auto/decltype/CTAD, structured bindings, aggregate/list/designated forms, deduction guides and invalid controls by pinned dialect |
| M5.6 | Target layout and numerical models | partial | Existing fixed LP64 integer widths only; alignment/layout/bitfields/endian, pointer/member representation, floating formats/rounding/conversions and alternate target fixtures |
| M5.7 | Attributes, pragmas and vendor extensions | open | Inventory each pinned extension/configuration branch; handwritten semantic handling with target/dialect fixture, no silent ignore counted support |
| M5.8 | Coroutines | open | Promise/await/yield/return lookup, transformation facts/lifetimes and invalid controls as required by pinned C++ surface |
| M5.9 | Modules/header units/PCH state | open | Module visibility/ownership/import/serialization boundaries, cycles/invalid imports and profile-specific tests; retained caches must not share session mutation |
| M5.10 | Controlled include fixtures | open | Standard-style integer/type/class/template headers with exact include+macro profile and native acceptance/fact comparisons; preserve actual source forms |
| M5.11 | Selected real library headers/projects | open | Exact acquired versions/configuration, encountered gap ledger, source/semantic differential suite and retained graph facts; no fixture rewriting to bypass semantics |
| M5.12 | Pinned dialect/configuration coverage audit | open | Enumerate modern/earlier C++ mode gates and configured vendor branches against upstream; no full compatibility claim from C++20 subset. C/CLI/driver/native-emitter surfaces are recorded separately below, not silently credited |
| M5.13 | Multi-TU program semantics and libraries | open | Explicit TU/entity/linkage/ODR/template ownership model, selected multi-TU fixtures and diagnostics; no accidental same-session state leakage |

## M6: API product and qualification

| ID | Required task / current bounds | State | Concrete acceptance / remaining evidence |
| --- | --- | --- | --- |
| M6.1 | Stable typed query API and examples | partial | Current read-only graph API; document identity/canonicalization/queries that instantiate vs inspect, complete unsupported-result contracts and runnable examples for every promised subsystem |
| M6.2 | Session lifecycle and concurrency | partial | Existing independent/concurrent sessions; repeated real-header/template analyses with live session and discarded old results, WeakReference/retained-memory checks against dependent-signature scope leaks; cancellation safe points and stress/race checks |
| M6.3 | Recovery and diagnostic quality | partial | Existing bounded recovery and invalid/Unsupported distinctions; managed extra named-reference hazard warnings are deliberate non-parity. Full recovery, diagnostic compatibility/suppression/speculation, cascade limits, source notes and malformed termination remain required |
| M6.4 | Explicit resource limits | open | Depth/token/include/expansion/template/constexpr work and memory budgets, cancellation/failure outcomes, bounded adversarial fixtures and cleanup; no process crash counted diagnosis |
| M6.5 | NativeAOT and platform qualification | partial | Current95-group suite passes managed and Linux-x64 NativeAOT including glibc host input and scalar references; broader frontend/OS/architecture/ABI qualification remains required. Unavailable host stays unverified/evidenced blocker |
| M6.6 | Performance/resource measurements | open | Reproducible startup/throughput/allocations/retained-memory on representative headers/projects with baseline and profile; optimize only under semantic parity tests |
| M6.7 | Regression/differential corpus qualification | open | Preserve upstream fp/fn/cp/lp/rp case kinds, per-case //options and TEST_NUMBER; parser acceptance qualifies only frontend fp/fn stages, never compile/link/runtime stages. Explicit expected language exit/status+diagnostics, semantic witnesses, inventory/pass/fail/skip and malformed/adversarial provenance |
| M6.8 | Upstream update procedure | open | Rehash/diff routine+configuration baseline, identify impacted invariants and fixtures, run actual update rehearsal with regression receipts and reviewed correspondence changes |
| M6.9 | Full-plan completion audit | open | Every required row and split branch implemented+validated, no required unsupported/unverified row or unresolved critical/high finding; root reviews explicit coverage before completion |

## Recorded upstream surfaces and product boundary

`PLAN.md` makes native emitters, compiler CLI compatibility, runtime libraries
and prelinking non-initial deliverables; they are not implementations of the
managed parser/API. The upstream index retains those modules for semantic
reference and makes no port claim. Upstream C dialects/C++CLI/driver modes and
future-standard conditional features must be explicitly classified during the
pinned-surface audit before any EDG-equivalence claim. This note does not remove
any stated plan requirement or convert an unsupported branch into completion.

Current primary dispatch: the integrated M1 preprocessing snapshot passes final
60-group managed/AOT validation and independent `review-m1-final.md` acceptance.
Current remaining-M1 character/predefined/probe/host-input extension has 79/79
managed/AOT execution and independent bounded acceptance in
`review-m1-literals-final.md`. Next is
pinned Unicode17 XID/UCN canonical identity and explicit source/execution encoding,
then complete string/floating/UDL semantics. Scalar references are now actually
integrated with95/95 managed/AOT execution and independent bounded acceptance in
`review-references-final.md`. Unicode remains separately authored and unintegrated; no Unicode pass
is inferred. Remaining M1/M2/M3/general
M4 and M5/M6 rows are burned
down without stopping at checkpoint commits. Dependencies/order may change based
on real-header failures, but rows remain visible until validated.
