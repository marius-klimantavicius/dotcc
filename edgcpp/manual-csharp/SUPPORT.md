# Supported behavior

Status: the bounded M0 baseline, partial M1 lexical foundation and first
integrated M2 slice, scalar overload checkpoint and early M4 difficult-path gate
and the bounded integrated preprocessing checkpoint are independently reviewed
and accepted (`evidence/review-m1-final.md`). Release build and all 60 named groups pass in managed
execution and a published Linux x64 NativeAOT
executable. This inventory does not establish C++20/EDG completeness or broader
platform/product qualification. See `evidence/review-final-m0-m2.md` and
`evidence/review-m3-final.md` and `evidence/review-m4-final.md`; preprocessing
correspondence and exact evidence are in `docs/PREPROCESSING.md`. The full required backlog is
`docs/FEATURE-INVENTORY.md`.

The initial profile is strict C++20 with fixed LP64 integer/pointer widths
and signed plain char. Target configuration is separate from semantic types,
but additional target models, floating formats, alignment and object layout are
not implemented or claimed. Controlled configured in-memory includes/macros are
qualified for the bounds below; platform filesystem and real headers are unqualified.

| Area | Implemented bounds | Partial, unsupported or unverified |
| --- | --- | --- |
| Source/tokens | Retained physical sources, UTF-16 half-open ranges; phase-2 splices/comments/raw token boundaries; ASCII identifiers; immutable token intervals/cursors; changed streams use a real presentation buffer and immutable spelling/expansion/include/presumed-location DAG | No encoding/Unicode identifier/UCN semantics. Arbitrary span mapping uses retained contributors, never a fabricated cross-file physical range; zero-width splice anchors are forward biased |
| Lexical diagnostics | Active numeric grammar and invalid quotes/comments/raw termination; inactive/unused language checks deferred; active float/string/character/UDL values explicitly unsupported | Full literal encoding/escape/count/concatenation/value repertoire remains required. Native accepts a malformed unused raw replacement that this bounded path diagnoses Unsupported; nonordinary #line compatibility is explicitly partial |
| Preprocessing | Object/function/variadic macros, lazy argument prescan, persistent inertness, caller-tail rescan, #/##/placemarkers/VA_OPT; full retained 32-label upstream macro matrix; configured include search/guards/reentry/once; conditional stack/defined/64-bit PP integer evaluation; #line, #error, strict #warning rejection; __LINE__/__FILE__/__cplusplus | In-memory provider only; no host include fallback. PP character operands, Unicode/UCN, standard date/time/hosted predefs, __has_include/other special operators, general pragmas/extensions and complete lexical compatibility remain required. Include entry/EOF are lookahead barriers in the pinned profile |
| Types/declarations | Canonical bool, char, signed/unsigned char/short/int/long/long long; const scalars; void function results; simple `using`/`typedef` aliases and scalar variables | No pointers/references/arrays, compound/function typedef declarators, multiple declarators, direct/list initialization, storage/linkage specifiers; const void unsupported |
| Functions/scopes | Source-ordered scalar free-function overload bindings, compatible declaration/definition identity, named/unnamed by-value parameters; exact/promotion/conversion candidate comparison; nearest lexical binding/hiding, parameter/body rules and nested shadowing | No reference/pointer/class/user-defined-conversion/ADL overload branches, defaults/variadics/block-scope functions, namespaces/classes or general active/inactive/other symbol rules. Global `main` special semantics are explicitly unsupported |
| Expressions/statements | Direct/parenthesized ordinary calls; explicit scalar conversion nodes; returns; empty/declaration/expression statements; integer `+ - * / %`, comparisons, bitwise operators and unary `+ - ! ~`; implemented alternative operator spellings map to the same token kinds | Assignment, logical binary operations, shifts, comma/conditional, casts, member access, function decay, labels/control flow and other missing forms are explicitly unsupported |
| Constants | Radix/suffix/rank/width rules; optional folds plus separate explicit constexpr integer work/call/block/local execution; static_assert and constexpr-variable required evaluation; explicit failure/dependency/resource outcomes | Integer straight-line evaluation only. No assignment/control flow/void/object/lifetime interpreter, complete constant-expression classification or extended integers. `ConstantValue` alone remains only a folded fact |
| Function templates | Named direct type parameters, immediate dependent prototype graph and nondependent checks, actual retained-token replay, by-value scalar deduction without promotions, canonical instances/reuse, frozen ordinary lookup, ordinary-template ties and direct-pattern ordering | No template forward/redeclarations, explicit arguments/specialization, compound/ref/pointer/class/member/ADL templates, packs/defaults/constraints or general partial ordering |
| State/replay | Independent session-owned state, retained graphs after disposal; real nested template replay and owner-journaled instance mutations; dependent signatures scoped to template lifetime with live-session collection/isolation tests | Root-owner checkpoints restore replay mutations; child replay Checkpoint/Restore is explicitly prohibited. General speculation and resource/cancellation/IDE lifecycle qualification remain outstanding |

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

Remaining M3 reference/class/advanced overload semantics, general M4 templates/
constexpr, M5 realistic headers/language breadth, and M6 product/platform/
performance qualification remain outstanding. There is no production native
fallback, generated EDG assembly/input, compiler process or code emitter.
Ordinary managed tests run without the separately invoked native oracle.
