# Independent final review: bounded integrated M1 preprocessing gate

Date: 2026-10-21. Reviewer: `/root/manual_edg_supervisor/review_m3`, requested
GPT-6.1 Sol xhigh independent review. **Accept the bounded implemented/tested
preprocessing, provenance and changed-stream M4 integration gate. No high finding
remains open within those bounds. This does not complete M1 or the full PLAN.**
The supervisor must continue the remaining roadmap automatically after this
checkpoint; milestone approval is not campaign completion.

The reviewer inspected actual EDG source, produced C# implementation, assertions,
fixtures, tool adapters and executed evidence. The reviewer ran only read-only
inspection/hash audits and wrote review reports: no implementation edits,
builds/tests/native processes or Git mutations. Execution was supervisor-owned.

## Final held evidence independently audited

All four `m1-{build,tests,nativeaot-publish,nativeaot-tests}-round3.json` receipts
have **identical 47-file maps matching every current held implementation/test/
fixture byte**. Release build exits 0 with zero warnings/errors. Managed and
published Linux x64 NativeAOT each pass **60/60 named groups**, zero failed or
skipped. This is the prior 48 groups plus 12 actual preprocessing groups,
including the full upstream differential and difficult integration graph.

Read and verified all three SHA-256/size entries in `m1-built-artifacts.json`:

| Output | Bytes | SHA-256 |
| --- | ---: | --- |
| EdgSharp Release DLL | 183808 | `8e74176c95d416cb63c9e2204b99fe95e582d8d0c0c09789be59f1347e436481` |
| Managed tests DLL | 111616 | `67d2b7fa271f58d9cfaccc8aae6153f38f58dd88fc33f7aea365c478b1d7c035` |
| Linux x64 NativeAOT tests | 2842040 | `2a686f4ecfbb01bf58ea5fa125e4bcadf33a155571457c1b2bdd97dcc83da021` |

All 17 consulted implementation/document hashes in
`source-m1-preprocessing-round2.json` match the actual upstream files. Reviewed
native fixture/witness and final PP-control receipts have matching primary and
additional input hashes and pinned compiler hash
`e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3`.
Exact accepted fixture receipts cover integrated root/header, difficult
root/header, lazy arguments, multiline builtins, rescan whitespace and upstream
VAOPT; two included semantic witnesses establish concrete types/values.

Final test-only tool receipts also pass: eight adapter status/profile/config
unit controls, four used-instantiation controls and eight PP contracts (six
accept, two language-status-2 rejects). Tool source hashes match current tools;
child native input hashes match. Rejection accepts only EDG RC_ERROR=2, not an
arbitrary failing process. Explicit `--instantiate used` prevents default-none
template-body acceptance from masquerading as semantic evidence. Exact PP text
comparisons preserve stringized contents and token boundaries. Production has
no dependency on these tools, compiler processes, native fallback, generated EDG
input or reflection-based code generation.

## Produced algorithms and retained graph reviewed

`Source.cs` keeps physical buffers, logical input, presentation text and
synthesized spelling distinct. Every range denotes one real buffer;
`SourceRange.Cover` rejects cross-buffer construction. Origins separately retain
spelling contributors, macro definition/invocation/argument history, include
activation and presumed location. Immutable related origins preserve histories
that cannot fit one parent chain. Range aggregation includes every covered
origin; note queries traverse expansion/include graphs. Zero-width splice
anchors have an explicit forward bias. Public collections copy/retain ownership
and remain queryable after disposal.

`Preprocessing.cs`, `PreprocessingDirectives.cs` and `MacroExpansion.cs` separate
logical character phases, raw preprocessing tokens, directive/include activation,
isolated argument prescan and replacement rescan with caller-tail visibility.
Frozen suppression sets retain inertness; only actual replacement-list paste
operators execute paste. VAOPT operands substitute/paste before surrounding
paste, retaining placemarkers and completed-substitution identity. Keywords and
alternative operator spellings remain macro-eligible identifiers until expansion.
Presentation serializes authoritative tokens without accidental retokenization.

Include search uses configured provider/canonical paths and ordered physical
including-directory/quote/system roots; no implicit host search is performed.
Guarded reentry is permitted. Conditional stacks and #line state belong to each
activation; #line does not alter physical-relative search. Pinned EDG's include
entry/EOF macro-lookahead barriers and closing-parenthesis/spliced-line builtin
locations are deliberately matched while physical spelling remains separate.

The PP evaluator is separate from language constexpr execution. It scans dead
branches syntactically, short-circuits evaluation and uses explicit 64-bit
intmax/uintmax arithmetic. Shift count validation is separate from target-width
truncation, following `folding.c:do_shift`; signed sign-bit construction passes.
Character PP operands and unimplemented special operators are Unsupported.

Actual correspondence includes `lexical.c` logical input/source maps,
`macro.c:macro_invocation` 5760+ and inertness/raw-prescan branches around
6010/6165/6560, `stringized_arg` 3323+, stringized VAOPT 4224+,
`proc_define` parameter/redefinition checking 9990+, `preproc.c:scan_if_expr`,
include/line/conditional stack paths and `folding.c:do_shift` 3862+.
Microsoft-traditional `choose_raw_or_expanded_arg`, PCC modes and unrelated
conditional branches are not claimed ported by citing these contracts.

## Canonical finding disposition

Canonical IDs below supersede informal descriptions in earlier messages.
`review-m1-round1.md` and `review-m1-round2.md` preserve discovery history and
provisional observations; source repairs alone were never acceptance.

| ID | Issue / final disposition |
| --- | --- |
| M1-01 | Half-open adjacent-map exclusion: closed by repaired overlap and exact spliced subrange/adjacent token tests. |
| M1-02 | Clipped logical presumed position: closed; sole `b` contributor/column asserted, plus forward splice boundary, empty and EOF controls. |
| M1-03 | Absolute root include directory: closed by `/root.cpp`→`/header.h` direct test. |
| M1-04 | Eager unused/stringized variadic prescan: closed by lazy requests and exact BAD(1) suppression outputs. |
| M1-05 | Lost argument/prescan origin history: closed by RelatedOrigins and retained F/ID/G assertions after disposal. |
| M1-06 | Stringized VAOPT support: closed for bounded path by empty/nonempty and complete matrix results. |
| M1-07 | Composite-range macro/include history loss: closed by all-covered-origin retention, A/B expression graph and repeated-header aggregate notes. |
| M1-08 | Inactive/unused literal diagnostic timing: closed for tested prefixed/raw/PP-number controls; full literal repertoire remains open. |
| M1-09 | Renamed macro parameters and replaced benign origin: closed by exact spelling comparison, rejection and first-definition-origin assertions. |
| M1-10 | VAOPT operand paste edges: closed by definition-time validation and both rejected edge controls. |
| M1-11 | Missing object-macro whitespace: closed by validation and native/managed rejection. |
| M1-12 | #line filename escapes / builtin reencoding: closed for ordinary quoted path, decoded hex/control assertions and explicit prefix qualification. |
| M1-13 | Comment/raw multiline directive grouping: closed by comment normalization and prior-token-end grouping; exact raw-token-plus-tail output asserted. |
| M1-14 | Signed PP shifts treated as arithmetic overflow: closed by width normalization and sign-bit/negative-shift controls. |
| M1-15 | Alternative operator macro eligibility: closed by delayed classification; macro expansion and unexpanded operator controls pass. |
| M1-16 | Strict20 #warning acceptance: closed by language rejection matching native. |
| M1-17 | Rescan/empty replacement whitespace: closed by invocation spacing propagation and the exact extended stringization matrix. |
| M1-18 | Splice-created comment context/raw exemption: closed by logical comment state and retained accepted source/declaration test. |
| M1-19 | **Qualified, not full parity:** active raw closure has explicit success state/tests. Native-accepted malformed unused raw replacement is diagnosed Unsupported, with direct test. Deferred native raw recognition remains open M1 literal compatibility. |
| M1-20 | Nested VAOPT substitution/paste ordering and ## data: closed by revised algorithm, full 32-label differential and HH/raw-argument ## stringization controls. |
| M1-21 | Builtin conversion dropped RelatedOrigins: closed by retaining source origin and a discriminating pasted __LINE__ graph with A/B/E/CAT histories. |

M1-21 does not need an extra edit to repeat every contributor assertion: the
builtin constructor copies the exact spelling-range collection and independently
tested paste construction retains operands/operators. Its new test exercises
the previously lost nonlinear expansion histories, which a simple wrapper would
not discriminate. Positive PP text helpers now require IsSuccess, including
concurrent/state cases; diagnostics cannot coexist with a passing prefix-only
text comparison.

## Source integration and evidence limits

The unchanged complete upstream fixture is byte-identical to
`upstream/tests/tests/imported/clang/cpp/Preprocessor/macro_vaopt_expand.sft.cpp`,
SHA-256 `fd3b0f88387c48b7eedcdb0cf2f4ca23123dbd0c7af183fc996a909ed92b453b`.
All **32 invocation labels** are compared, including the difficult final labels;
the earlier informal “26-case” shorthand was inaccurate. Type/options/comments
and original TEST_NUMBER profile metadata are retained. Only the explicit
strict-C++20 preprocessing control was executed; neither original frontend fp
variant, C++11, FileCheck harness, compile/link nor runtime receives pass credit.

`m1-difficult.cpp`/`.h` combine includes/macros with nested int/long template
instances, local values, BODY token replay, frozen mark(long) lookup despite a
later mark(int) and caller-local mark, repeated instance reuse, and an ordinary
function winning a tie without instantiating an invalid losing template body.
Managed graph tests assert selected identities/candidate counts, one body replay,
instance counts, constexpr outcomes and retained BODY/include origins after
session disposal. The native included witness independently proves int/long id
types, long combine types and values run=7/combine(5L)=8/ordinary=1. Native
acceptance alone is not graph proof; the managed assertions provide that proof.
Prior M3/M4 and N1 rollback groups remain passing in the same managed/AOT run.

Exploratory failures are preserved and excluded: combined nonordinary #line
expected-reject, alternative-operator-name expected-reject and newline malformed
unused-raw expected-reject all returned native acceptance. The original
no-newline malformed raw reject diagnoses EOF newline policy, not missing raw
termination. Historical whitespace round1 bytes have a separate retained-source
mapping; current expanded matrix uses round2. These do not inflate pass counts.

Full encoding/Unicode/UCN, literal values/escapes/concatenation, deferred raw macro
recognition, nonordinary #line prefix compatibility, character PP operands,
special PP operators, standard date/time/hosted predefs, general pragmas/dialects,
platform filesystem/real headers, broader cache/speculation/recovery and full
M2–M6 language/product requirements remain active in the feature inventory.
This approval establishes the bounded gate above and authorizes its checkpoint;
the supervisor should commit owned verified work and immediately continue the
remaining PLAN without a user-prompt or partial-campaign stopping point.
