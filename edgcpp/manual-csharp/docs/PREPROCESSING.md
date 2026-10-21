# Integrated preprocessing correspondence

This bounded M1 checkpoint has 60/60 managed and Linux x64 NativeAOT groups,
zero failures/skips and a Release build with zero warnings/errors. Four round-3
receipts have identical 47-file maps matching held bytes. Independent bounded
acceptance is `evidence/review-m1-final.md`. Full lexical/directive compatibility remains required in
`FEATURE-INVENTORY.md`; this is not complete M1 or full-plan acceptance.

The exact consulted source/document hashes are in
`evidence/source-m1-preprocessing-round2.json`. These mappings identify algorithm
contracts, not claims that entire native files have been ported.

| EDG source contract | Managed ownership/adaptation | Required acceptance |
| --- | --- | --- |
| `lexical.c` logical input and source-position maps; `macro.c` macro text maps | Immutable physical sources and spelling contributors, separate presumed positions/include activations/macro frames; changed output has a real presentation buffer | Direct/clipped/spliced/boundary maps, reordered/stringized/pasted contributors, cross-file declaration ranges, retained graph after disposal |
| `macro.c:proc_define`, parameter scanning and adjacency | Handwritten PP definitions, raw PP identifiers before language keyword classification | Splice versus comment adjacency, malformed definitions, unused PP-numbers not eagerly language-validated |
| `macro.c:arg_get_token`, strict argument prescan/substitution and rescan | Input/replacement frames and persistent suppression; explicit raw/expanded arguments | Nested same-macro prescan, caller-tail alias/function invocation, self/mutual inertness, argument comma balancing, discarded args |
| `macro.c` `#`, `##`, variadic and `__VA_OPT__` branches | Contributor-preserving synthesized tokens; token paste is explicit retokenization | Whitespace stringization, valid/invalid paste, empty arguments/placemarkers, expanded-empty variadic controls |
| `preproc.c` conditional stack/defined/if expression | Per-include activation stack and PP-specific target-width evaluator | Header conditional boundaries, skipped nesting, defined suppression, short-circuit/dead invalid arithmetic, target signed/unsigned values |
| `folding.c:do_shift`, `const_ints.c` fixed-width representation | Explicit intmax/uintmax 64-bit values, separate shift count validation and truncation | Signed sign-bit construction and negative left shifts match pinned C++20; no CLR-width dependence |
| `preproc.c` include/line; `lexical.c` input-file search | Configured provider and ordered physical search roots; presumed remapping separate from physical identity | Guarded reentry, search precedence, missing/depth cases, repeated activation remaps, physical include search after `#line` |

`SourceRange.Cover` remains one-buffer only. Parser/template spans cover actual
ordered presentation text, never a fabricated physical range between unrelated
files or reordered macro arguments. Tokens remain authoritative: inserting
presentation whitespace cannot merge `+ +`, `< <`, identifiers or PP-numbers.
All retained frames and contributor sources must survive session disposal.

Pinned EDG also allows an alternative operator spelling to be a macro name:
`#define and 1` causes `and` to expand to `1`. Preserve macro eligibility before
classifying unexpanded alternative spellings for controlling expressions and
the language token stream, including tokens created by paste. The initial
assumed-negative operator-name receipt returned 0 and receives no pass credit.
Strict-C++20 #warning is rejected by this profile as an unknown directive.

The explicitly selected native-only suite is
`tools/test-oracle-preprocessing.py`. The final run has six accept and two exact
language-status-2 rejection contracts, with macro/multiline/include-tail text
assertions. Its current observed profile is strict C++20, predefined macro file
disabled and external template instantiation `used`. Included-file EOF stops
function-macro lookahead; caller-tail rescan inside one input is still required.
Spliced `__LINE__` uses the final physical line; a multiline argument's builtin
uses the closing-parenthesis presumed line. Physical spelling locations are
retained separately from those expansion-time builtin values.

The selected upstream `macro_vaopt_expand.sft.cpp` is also executed as an exact
strict-C++20 preprocessing-only native control. Its original `fp` kind and two
`//options`/`TEST_NUMBER` variants are preserved in
`evidence/upstream-m1-selection.json`. Neither original frontend case receives
pass credit from that narrower execution; C++11 is unverified. The full managed
token differential passes with exact copied source bytes and native output;
separate graph tests assert origins and retained owners.
The complete matrix has 32 invocation labels; earlier “26-case” dispatch
shorthand was not an accurate invocation count. Operand substitution must occur
before surrounding paste; replacement-list operators must stay distinguishable
from `##` passed as ordinary argument data.

Microsoft-traditional `choose_raw_or_expanded_arg` behavior is not the strict
branch and is not claimed by this assignment. Platform filesystem/encoding,
full literals, extension directives/pragmas/predefs and general lexical/cache
coverage require their own inventory closure and cannot be inferred from these
controlled contracts.

The subsequent character/builtin/host-input extension expands these historical
bounds; its exact current contracts and 79-group execution are recorded in
`LITERALS-AND-INPUT.md`. The 60-group snapshot above remains the earlier accepted
checkpoint, not a statement that those subsequently implemented branches are
still absent. Full Unicode/literal/dialect/header coverage remains outstanding.

The pinned #line implementation has a nonordinary-prefix anomaly: L-prefixed
filenames reject, while u8/u/U/R forms accept but retain prefix fragments in
__FILE__. Exact per-prefix receipts are retained. The combined earlier negative
expectation was disproved (`native-m1-line-other-prefixed-filenames-contract-round1.json`)
and receives no pass credit. Ordinary quoted filename escape decoding is the
current required path; remaining prefix compatibility stays explicit in M1.8
and the broader dialect/configuration audit.

Final evidence: `m1-build-round3.json`, `m1-tests-round3.json`,
`m1-nativeaot-publish-round3.json`, `m1-nativeaot-tests-round3.json`;
`m1-built-artifacts.json` fingerprints three outputs. Explicit adapter/profile,
used-instantiation and PP controls pass in `oracle-m1-*-final.json`.
`native-m1-difficult-fixture-round1.json` and its included semantic witness use
the exact managed root/header bytes. Managed graph assertions prove nested
int/long instances/reuse, one replay each, earlier mark(long) despite later
mark(int)/caller hiding, losing template body uninstantiated, constant values,
and BODY/include origins surviving disposal.

Canonical review findings are M1-01..21. M1-19 is explicitly a local active-raw
closure invariant and a native-accepted unused-raw Unsupported compatibility
bound; the original no-newline native error only establishes EOF policy. Three
disproved expected-negative M1 probes (nonordinary #line, operator macro name,
newline malformed unused raw) are retained and excluded from pass counts.
