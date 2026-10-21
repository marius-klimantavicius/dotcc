# Independent M1 source/preprocessor architecture baseline

Date: 2026-10-21. Reviewer: `/root/manual_edg_supervisor/review_m3`, continued
independent reviewer. **Architecture/source inspection only, not approval of an
implementation or completion of M1.** Actual produced code, retained graph
origins, adversarial tests and matching executed evidence remain mandatory.

Reviewer owns only new `evidence/review-m1-*.md` reports. No production changes,
builds/tests/native processes or Git mutations are performed by the reviewer.
M4's accepted gate must continue passing through preprocessing; the complete
remaining `PLAN.md` and feature inventory stay active.

## Actual EDG source inspected

There is no `lex_pp.c`: `lex_pp.rst` describes implementation in `lexical.c`,
`macro.c` and `preproc.c`. Source hashes at this inspection:

| Source | SHA-256 |
| --- | --- |
| `macro.c` | `3cd96e32ba6676fb66b17900922abdc9a33caab5aade78f2046dba0490d996a3` |
| `preproc.c` | `e0927cba0b5ba139152d69063a6ec73114e103be957effd90eec4616438a65af` |
| `lexical.c` | `e987ffcb7645931564f3a6f262d089c21524e78ce2c60d5e0e0868a5b4b98875` |
| `lex_pp.rst` | `fdeea9d1bba1c5f3f89d5fde9596070aa0d5c9aa22927a62756049e29eceb06b` |
| `includes.rst` | `124607c25e2f8ed6d29bec97ca466586279d3df9639d6f770049949dd8ab9dea` |

Independently read these actual branches:

- `lexical.c:read_logical_source_line` (9383+) performs character-level phase
  transformations and retains mapping back to original physical sequence and
  columns. Its EOF behavior can pop include input frames and continue the caller.
  `lex_pp.rst` distinguishes these intra-token transformations from inter-token
  comment/macro source modifications, including nested modifications and inert
  macro/end-of-token markers.
- `macro.c:macro_invocation` (5760+) detects own-expansion ancestry before
  function-argument collection and emits persistent inert-macro markers
  (around 6010/6165). Function-like lookahead and raw argument collection
  (around 6560+) operate with expansion disabled; only parentheses shield comma
  separators. Required argument prescans are isolated, then replacement text
  rescans in the surrounding stream. Macro source maps distinguish replacement
  definition and argument spelling (structures near file start and argument
  fields around 350+).
- `macro.c:stringized_arg` (3323+) uses raw argument spelling, canonical trivia,
  and quote/backslash handling. Paste/placemarker and variadic substitution
  branches are separate. Variadic emptiness is checked in raw and expanded forms
  (around 6930/7059); C++20 permits omitted final variadic arguments (7320+).
  `choose_raw_or_expanded_arg` (3013) is specifically a Microsoft-traditional
  idiosyncrasy and must not be mislabeled the standard strict algorithm.
- `preproc.c:scan_if_expr` (470+) activates macro expansion and a separate
  preprocessing expression context. `expr.c:scan_pp_expression` (53098+) uses
  preprocessing expression semantics and restores its prior expression state.
  Conditional-stack/else/elif/nesting branches must preserve skipped-group state.
- `preproc.c:proc_include` (1390+) accepts raw or macro-produced header names;
  `lexical.c:resolve_header` (7763+) chooses the appropriate ordered search path.
  `preproc.c:proc_line` (2387+) macro-expands its operands, distinguishes standard
  from cpp-output extensions, validates line numbers and records file/line state.

PCC/Microsoft traditional behavior, GNU extensions, PCH/modules/header units and
other conditional branches are not established by citing these routines.

## Source/provenance contract to inspect in produced code

1. Physical source buffers, preprocessed presentation and synthesized spelling
   buffers are distinct kinds. `SourceRange` always denotes a real contiguous
   extent in exactly one retained buffer. `Cover` must not invent a cross-file
   physical extent or collapse non-monotonic/reordered contributors.
2. Preprocessing tokens are authoritative. A presentation buffer serializes
   their spelling/boundaries for parser spans; it must not accidentally retokenize
   adjacent `+ +`, `< <`, identifiers or pp-numbers. Only an explicit paste
   operation may create a new token. Language keyword classification follows
   macro-eligible preprocessing identifiers (`#define private public`, for example).
3. Preserve spelling contributors, macro definition/invocation/argument chains,
   include activation chains and presumed logical location separately. Stringize,
   paste and spliced tokens may have multiple physical contributors. Their
   definition operators and raw/expanded argument relations remain inspectable.
4. `AnalysisResult.Source` remains the primary physical source. Changed
   `Tokens.Source`/template body caches may instead own an explicitly marked
   presentation buffer. Parser spans cross include/expansion output in that real
   ordered buffer, while origin queries recover each physical source/activation.
5. `#line` state belongs to an include activation, not a mutable shared
   SourceFile. Repeated activation of the same physical buffer can have distinct
   logical mappings. `__LINE__`/`__FILE__` use expansion-site presumed locations;
   include-relative search still uses the physical including-file directory.
6. Diagnostic primary logical location, physical expansion anchor, spelling
   contributors and macro/include notes must agree without confusing a synthetic
   presentation position for a physical source location. Origins, buffers,
   diagnostics and retained template tokens remain queryable after disposal.

## Concrete semantic probes forwarded

- Self/mutual recursion and tokens disabled inside replacement ancestry retain
  inertness after frame pop and substitution. Mere disabled-name stack lookup
  without persistent token inertness is insufficient.
- Object alias to function and argument-to-function tail cases must rescan with
  caller tokens visible: `#define F(x) x`, `#define A F`, then `A(7)`; and
  `#define ID(x) x`, then `ID(F)(7)`. Invocation can span ordinary newlines.
  Include-return tail interaction needs a native control: a header ending in F
  followed by `(7)` in root code must not be assumed equivalent to per-file
  completed-list expansion without checking.
- Function-macro definition adjacency uses logical trivia: a phase-2 splice
  between F and `(` preserves function form; a comment between them introduces
  whitespace and makes the definition object-like. Directives originate only
  at logical input beginning of line, never from macro-generated `#` tokens.
- Raw argument splitting occurs before expansion and only tracks parentheses;
  braces/brackets/angle syntax do not shield commas. Argument prescan must not
  consume the caller tail past its own boundary. `#`/`##` use raw arguments;
  ordinary substitutions use prescanned arguments.
- Stringize comment/whitespace normalization, quote/backslash escaping, empty
  arguments and paste validation/placemarkers have exact token/origin assertions.
  `#define EMPTY` plus `#define V(...) __VA_OPT__(1)`, then `V(EMPTY)`, must use
  expanded emptiness. Omitted/empty/multiple variadic arguments are distinct cases.
- `defined` suppresses operand expansion; other unknown #if identifiers become
  zero; C++ true/false and intmax/uintmax target arithmetic are explicit.
  Short-circuit `&&`, `||` and `?:` must not evaluate dead division-by-zero
  operands. Inactive groups must not enter declarations or execute active-only
  directives. Malformed directive nesting remains diagnosed.
- Includes use only the configured provider/search roots. Quotes search physical
  directory, quote roots, then system roots; angle names search system roots.
  Missing files, canonical identity and depth/work limits are deterministic.
  A repeated active file ID is not itself illegal: guarded or macro-dependent
  finite self-inclusion can be valid. Do not reject it before its conditionals
  can skip contents. Header-name spelling is not ordinary C++ string unescaping.

Final acceptance must include native preprocessing token comparisons with the
same explicit include/macro profile, actual managed provenance/ownership graph
assertions, independent sessions, replayed M4 tokens with origins, and a combined
preprocessing-to-template-to-constexpr source gate. Architecture agreement and
native acceptance alone establish none of those implementation facts. All open
M1 lexical/encoding/literal/predefined/directive rows remain active until actually
implemented and qualified; unsupported source cannot become skipped success.

## Native contract results inspected after the initial probes

Read the actual six `native-m1-*-contract-round1.json` receipts and stdout:

- Same-buffer `F(F(7))` produces 7; `F(F)(7)` remains `F(7)`; A(8) and
  ID(F)(9) expand to 8 and 9. Expanded-empty VA_OPT omits its payload. Empty
  paste operands produce right/left/empty results; stringize canonicalizes
  spacing; `+ +` and `< <` remain distinct tokens; spliced definition adjacency
  is function-like while comment-separated adjacency is object-like.
- **Include-return lookahead is bounded:** the header-final F plus root `(7)`
  remains `F\n(7)` in this pinned EDG profile. Do not require cross-include EOF
  invocation merely because replacement-frame caller-tail rescanning works.
- **Spliced-line presumed location differs from physical spelling:** both
  __LINE__ occurrences in the tested spliced logical line expand to 28, its
  final physical line. Keep each actual physical spelling contributor while
  matching this observed presumed expansion location. The earlier baseline
  called for a probe and did not establish a contrary result.
- Guarded/macro-state finite re-entry is accepted. Renaming a file with #line
  does not change physical-relative include resolution; the later assertion
  witnesses __LINE__ 502. A header closing its includer's #if, or leaving an
  open #if to be closed by its includer, receives actual native language errors.

These are supervisor-executed native facts, not managed implementation passes.
The exact source/include hashes and profile still need the final evidence audit.
