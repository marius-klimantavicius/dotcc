# Supported behavior

Status: the bounded M0 baseline, partial M1 lexical foundation and first
integrated M2 slice plus scalar free-function overload checkpoint are independently
reviewed and accepted. Release build and all 35 named groups pass in managed
execution and a published Linux x64 NativeAOT
executable. This inventory does not establish C++20/EDG completeness or broader
platform/product qualification. See `evidence/review-final-m0-m2.md` and
`evidence/review-m3-final.md`; the full required backlog is
`docs/FEATURE-INVENTORY.md`.

The initial profile is header-free C++20 with fixed LP64 integer/pointer widths
and signed plain char. Target configuration is separate from semantic types,
but additional target models, floating formats, alignment and object layout are
not implemented or claimed. Macro environments and real headers are unqualified.

| Area | Implemented bounds | Partial, unsupported or unverified |
| --- | --- | --- |
| Source/tokens | Immutable source strings; physical UTF-16 offset/column and half-open ranges; CR/LF/CRLF; comments; ASCII identifiers/keywords; maximal punctuation/digraph handling; stable EOF; immutable token intervals, independent cursors and lexical checkpoints | No encoding/Unicode identifier/UCN semantics, macro/include origins, logical line remapping or phase-2 line splicing |
| Lexical diagnostics | Invalid source characters, unterminated comments/quotes, explicit unsupported line splicing/prefixed forms | Numeric spellings are scanned as pp-number-like tokens. `Lex` alone does not validate integer digits/suffixes; semantic literal conversion does. Quoted scanning only handles escape-aware boundaries/termination: no escape repertoire, character-count, concatenation or string/character/floating value semantics |
| Preprocessing | Directives encountered during analysis are diagnosed unsupported | No object/function macros, expansion suppression/rescanning, includes or conditionals; M1 remains partial |
| Types/declarations | Canonical bool, char, signed/unsigned char/short/int/long/long long; const scalars; void function results; simple `using`/`typedef` aliases and scalar variables | No pointers/references/arrays, compound/function typedef declarators, multiple declarators, direct/list initialization, storage/linkage specifiers; const void unsupported |
| Functions/scopes | Source-ordered scalar free-function overload bindings, compatible declaration/definition identity, named/unnamed by-value parameters; exact/promotion/conversion candidate comparison; nearest lexical binding/hiding, parameter/body rules and nested shadowing | No reference/pointer/class/user-defined-conversion/ADL overload branches, defaults/variadics/block-scope functions, namespaces/classes or general active/inactive/other symbol rules. Global `main` special semantics are explicitly unsupported |
| Expressions/statements | Direct/parenthesized ordinary calls; explicit scalar conversion nodes; returns; empty/declaration/expression statements; integer `+ - * / %`, comparisons, bitwise operators and unary `+ - ! ~`; implemented alternative operator spellings map to the same token kinds | Assignment, logical binary operations, shifts, comma/conditional, casts, member access, function decay, labels/control flow and other missing forms are explicitly unsupported |
| Constants | Radix/suffix candidate ordering, rank/representability-aware usual arithmetic conversions; target-width unsigned wrapping; signed overflow/division-by-zero results withhold optional folded values | `ConstantValue` is a selected folded fact. No constexpr execution, constant-required context, complete constant-expression classification or extended integers |
| State/replay | Independent session-owned mutable state; serialized analysis within a session; retained completed graphs after disposal; source-order symbol identity; token retention; owner-mediated semantic checkpoint restoration | No production speculative parser path or template token replay/instantiation yet. Checkpoint tests establish a foundation, not M4 acceptance; resource/cancellation/IDE lifecycle qualification is outstanding |

For short/bool arithmetic promoted to a wider common type, the managed graph
exposes conceptual lvalue conversion, integral promotion and final integral
conversion separately. This is an intentional query representation adaptation;
EDG can cast directly to the selected common type after computing promotions.

Checkpoint regression tests directly exercise scopes, symbols, declaration
lists, cursor, diagnostics, pending replay, nested definition-link and
current-function restoration, and ordered overload membership rollback. These
cover owner-mediated current state; every new speculative/replay mutation still
needs its own restoration contract and direct branch checks.

On invalid or unsupported source, `IsSuccess` is false and the graph contains
only diagnosed partial facts, including explicit error nodes where recovery
continues. `ContainsUnsupportedFeatures` distinguishes unimplemented branches
from invalid supported input. Recovery does not qualify skipped constructs.

Remaining M3 reference/class/advanced overload semantics, M4 template deduction/token-cache instantiation and
constexpr gate, M5 realistic headers/language breadth, and M6 product/platform/
performance qualification remain outstanding. There is no production native
fallback, generated EDG assembly/input, compiler process or code emitter.
Ordinary managed tests run without the separately invoked native oracle.
