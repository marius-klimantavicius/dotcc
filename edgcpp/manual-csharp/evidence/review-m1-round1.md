# Independent M1 in-flight code review, round 1

Date: 2026-10-21. Reviewer: `/root/manual_edg_supervisor/review_m3`.
This is an in-flight source review, **not implementation acceptance**. No builds,
tests, native compiler processes or Git mutations were run by the reviewer.
The implementation is still changing. Source-level repairs below require
direct assertions and matching executed evidence before they close.

Canonical finding IDs for this review series follow. They identify defects or
required concrete regression cases, not claims that the complete M1 surface is
implemented. The baseline remains `evidence/review-m1-baseline.md`.

| ID | Severity / original defect | Current source observation / closure requirement |
| --- | --- | --- |
| M1-01 | Medium: a half-open source-map range included the next segment when its start equaled the requested end. | `SourceFile.OriginForRange` now stops at that boundary for nonempty ranges. Assert adjacent token boundaries and spliced logical-input segments, including zero-width EOF queries. |
| M1-02 | Medium: a clipped logical-input range retained the containing segment's presumed column rather than its actual physical subrange location. | The logical-input path now derives presumed coordinates from the clipped first physical contributor. Assert `ab\\\nc` normalized to `abc`, query the `b` subrange: only the physical `b` contributor and its exact line/column. |
| M1-03 | High for explicit include resolution: `Directory("/root.cpp")` was empty and searched `header.h` instead of `/header.h`. | It now returns `/`. Assert an in-memory `/root.cpp` quoted include resolves `/header.h`, without relying on other search roots. |
| M1-04 | High: every variadic invocation eagerly prescanned its pack even if unused or only stringized. A malformed macro invocation inside such an argument produced an erroneous diagnostic. | `nonemptyPack` now prescans only when the replacement contains `__VA_OPT__`; ordinary substitutions still request prescan lazily. Directly assert the native controls below and no diagnostic from suppressed `BAD(1)`. |
| M1-05 | High for retained graph fidelity: argument contextualization replaced a prior expansion chain, losing nested prescan invocations. | `TokenOrigin.Expanded` now retains `[this]` in immutable `RelatedOrigins`; `ExpansionHistory` traverses this graph. Assert `G→7`, `ID(x)→x`, `F(x)→ID(x)`, `F(G)` retains F, ID and G with physical spelling and invocation sites after session disposal. |
| M1-06 | High: `# __VA_OPT__(...)` was reduced to an ordinary `#` followed by a nonparameter, incorrectly rejected. | A dedicated stringized-VA_OPT branch now exists. Assert empty/nonempty outputs, substitution/pasting inside its body and contributors. Native strict C++20 confirms the simple controls below. |
| M1-07 | High for range-origin fidelity: `OriginForRange` collects spelling ranges from all covered segments but retains only the first origin in `RelatedOrigins`, losing later macro/include histories for parser covering ranges. | Open at this inspection. For `A→1`, `B→2`, `return A+B;`, query the binary expression range's origin and require both A and B histories. Cover tokens from distinct include activations too. Retain all covered origins; aggregate diagnostic/include notes must traverse them without inventing physical extents. |
| M1-08 | High for conditional semantics: physical-file tokenization emits unsupported prefixed-literal diagnostics before inactive groups or unused macro replacement text are discarded. | Open at this inspection. Native accepts the exact inactive/unused literal control. Defer language-literal semantics until active output; correct raw-string recognition and its phase-transform exemption remain required lexical work. |
| M1-09 | High: `EquivalentMacro` accepts parameter alpha-renaming, contrary to EDG's explicit parameter spelling comparison. A benign redefinition also replaces original definition metadata. | Open at this inspection. `F(x)→x` followed by `F(y)→y` must reject in the pinned strict profile. EDG `macro.c:9990–10001` compares parameter names; its benign branch discards the new definition. Assert compatible repeats preserve canonical definition provenance. |
| M1-10 | High: definition validation does not reject `##` at a VA_OPT operand's own start/end, allowing an invalid unused definition. | Open at this inspection. Native rejects both `__VA_OPT__(## a)` and `__VA_OPT__(a ##)` at definition time, exit 2. Validate every operand's own token boundaries before invocation. |
| M1-11 | High: `#define X+1` is accepted without required object-macro whitespace. | Open at this inspection. Exact native strict probe rejects, exit 2. Preserve valid function-like adjacency and empty object replacements while diagnosing this malformed definition. |
| M1-12 | High: `#line` filenames are sliced without escape processing. | Open at this inspection. Native decodes `"a\\x62.cpp"` to `ab.cpp`, and slash/quote escapes are decoded then reescaped by `__FILE__`. A wide filename is rejected. EDG `preproc.c:2490–2497` explicitly distinguishes escaped #line names from unescaped #include names; `extract_header_name:1208+` performs conversion. |
| M1-13 | High: physical token-start line grouping splits a preprocessing directive at a newline contained inside a block comment. | Open at this inspection. Native `#define X /*` + physical newline + `*/ 7`, then `value X`, produces `value 7`. Track actual preprocessing logical-line boundaries independently from physical token line numbers; raw multiline pp-tokens must eventually obey the same distinction. |
| M1-14 | High: signed preprocessing left shift goes through arithmetic-overflow rejection, incorrectly rejecting a valid sign-bit result. | Open at this inspection. Native `(1LL << 63)` equals INTMAX_MIN and `(-1LL << 1)` equals -2, both exit 0. EDG `folding.c:do_shift` truncates shifted integers with `check_overflow=FALSE`; validate counts separately and normalize target-width result bits. |
| M1-15 | High: alternative operator spellings are classified before macro eligibility, preventing the pinned EDG profile's accepted `and` macro definition/expansion. | Open at this inspection. Native `#define and 1` then `token and` produces `token 1`. Retain preprocessing identifiers through macro operations; classify surviving identifiers as alternative operators for controlling expressions and final language tokens. Also assert `defined(and)`, undefinition and pasted-name eligibility. |
| M1-16 | High for profile acceptance: `#warning` produces only a warning and success in the implementation despite rejection in pinned strict C++20. | Open at this inspection. Native exact control rejects with status 2 as an unrecognized directive. Match the selected profile's rejection; do not count a later-standard/extension path as current success. |
| M1-17 | High: definition-leading whitespace leaks into replacement rescan/stringization instead of using the invocation's whitespace. Empty replacement whitespace is also lost. | Native-confirmed. `E(a+A)` and `E(a+F())` stringify as `"a+x"`, a multitoken body as `"a+x+y"`, and explicitly spaced `E(a+ A)` as `"a+ x"`. Extended controls show `E(a EMPTY+b)`→`"a +b"`, `E(a+EMPTY b)`→`"a+b"`, `E(a+EMPTY+b)`→`"a++b"`. Propagate invocation spacing through replacement insertion/removal, preserving authoritative token boundaries. |
| M1-18 | High: a comment delimiter formed by phase-2 splicing is missed by physical-text comment tracking, so raw-looking text inside it wrongly receives a splice exemption. | Native exact source accepts. Preserve phase ordering and comment context after delimiter-forming splices; assert the resulting declarations and contributors. |
| M1-19 | Qualification / local scanner invariant, **not a confirmed native unused-definition rejection**: a missing raw terminator is reported by `RawLiteralEnd` as EOF, while `ScanRaw` treats any final quote as closure. | The original expected-reject control lacked an EOF newline and native rejected only that EOF policy. A newline-terminated unused definition unexpectedly accepts; the stronger unused-raw rejection claim is disproved. Retain explicit closure state locally, but qualify deferred unused-macro compatibility and inspect actual active-token behavior before asserting broader native semantics. |
| M1-20 | High, discovered by the broader upstream matrix: flattening VAOPT content before parameter substitution creates false adjacency to outside `##`; argument/pasted `##` data also needs separation from declaration operators. | Source repair now recursively substitutes/pastes the operand before surrounding paste, marks completed substitutions and marks declaration paste operators separately. Full 32-label native/managed matrix and direct `##` data controls must pass before closure. |

The former mutable `HashSet` suppression-sharing concern is now structurally
prevented by immutable `FrozenSet<string>` snapshots. The previous expander's
visible writers made fresh sets; this review did not establish a distinct
runtime alias-mutation defect before the change. Persistent suppression still
requires the self/mutual/tail/argument-boundary semantic tests.

## Native controls inspected

`tools/oracle-inputs/m1-lazy-arguments.cpp` defines `BAD(x,y)`, an unused
variadic argument, a stringize-only pack and a stringized VA_OPT body.
`evidence/native-m1-lazy-arguments-contract-round1.json` records explicit
strict C++20 / instantiate-used / preprocess / no-line-commands, exit 0:

```text
unused 7
stringized "BAD(1)"
vaopt "" "a"
```

Source hash is
`b52c38eb8b186d79febf8a933844782850d07580f5c690237fe2e8864c074063`.
The pinned native compiler hash is
`e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3`.
This is native semantic evidence, not managed execution evidence.

The separate multiline-origin receipt establishes `direct 6`, `wrapped 9`,
`nested 7`, `logical 82`. Function argument rescan uses the invocation's closing
parenthesis line in this pinned profile. The current source now builds the
function invocation context using the closing parenthesis presumed line;
direct managed assertions must establish the exact behavior without replacing
the argument's physical spelling location. Explicit include EOF markers are
also present to preserve the native include-tail boundary recorded in the
baseline; same-buffer replacement-tail lookahead must continue working.

Additional supervisor-executed contracts read/reported during this round:
inactive/unused literals accept; renamed macro parameters, either VA_OPT paste
edge and missing object-macro whitespace reject with language status 2. Escaped
#line filename output proves decoding, wide #line filename rejects, and a
multiline block comment does not end the macro definition. The include-entry
control leaves root `F` plus header `(7)` unexpanded, agreeing with the retained
include-return barrier. Exact receipts are `native-m1-*-contract-round1.json`;
their final current-input hash audit remains pending with implementation review.

A read-only audit of the 25 current `native-m1-*-contract-round1.json` receipts
found every primary/additional input hash matches the actual retained input.
Twenty-four have `matched=true`. The one historical
`line-other-prefixed-filenames` expected-reject mismatch is preserved and
excluded: native accepts u8/u/U/R filename prefixes with unusual retained prefix
fragments; only the L-prefixed rejection is established. Do not replace these
observations with a blanket prefix rule. Both signed-shift receipts explicitly
produce `pass` with status 0. This audit runs no compiler or semantic tests.

Alternative C++ operator spellings were mapped by the preprocessing lexer's
spelling switch to operator token kinds, so there was no operator-to-zero
controlling-expression bug. **That observation did not establish correct macro
eligibility.** A later exact native `and` macro control proves the early mapping
itself is incorrect for the pinned profile (M1-15); move it after macro expansion.
The native alternative-operator controlling expression also passes and must
continue passing. The earlier assumed-reject unused operator-name receipt
returned acceptance and is preserved/excluded as another exploratory mismatch.
The exact native #warning receipt rejects with language status 2 (M1-16).

The extended rescan-whitespace matrix is in the round2 receipt. Its original
round1 receipt remains historical, with exact old input bytes retained under
`tools/oracle-inputs/history/m1-rescan-whitespace-round1.cpp` and a mapping in
`evidence/retained-m1-rescan-whitespace-round1.json`; the old receipt must not be
asserted to hash the expanded current source.

Two additional concrete raw-scanner cases were forwarded for native control.
M1-18 is confirmed by native acceptance of the spliced-comment source. M1-19's
original no-newline control rejects, but the actual diagnostic is solely the
missing final newline. **Correction:** newline-terminated
`#define UNUSED R"(foo"` accepts in this pinned native profile; therefore an
unconditional unused-definition lexical-reject requirement is not established.
The failed expected-reject receipt is preserved/excluded. The source's ambiguous
closure result remains a local scanner invariant to review, while unused-macro
recognition/laziness requires the actual observed profile rather than a guessed
lexical rule. None of this approves the complete phase/literal surface.

## Acceptance still pending

No M1 build has been supplied for this review. The directive reader, conditional
expression evaluator, includes and session integration remain under construction.
Review must cover the final produced code, actual regression assertions, held
source/test hashes, managed and NativeAOT execution, native token comparisons,
diagnostic/origin ownership and the combined preprocessing→template→constexpr
gate. Encoding/literal/predefined/directive inventory gaps cannot count as
success or disappear behind a bounded checkpoint. Full PLAN remains active.
