# Supported behavior

Status: the bounded M0 baseline, partial M1 lexical foundation and first
integrated M2 slice, scalar overload checkpoint and early M4 difficult-path gate
and the bounded integrated preprocessing checkpoint are independently reviewed
and accepted (`evidence/review-m1-final.md`). The subsequent bounded character,
builtin and filesystem-input extension is independently accepted within its
executed bounds (`evidence/review-m1-literals-final.md`);
Release build and all 79 named groups pass in managed
execution and a published Linux x64 NativeAOT
executable. This inventory does not establish C++20/EDG completeness or broader
platform/product qualification. See `evidence/review-final-m0-m2.md` and
`evidence/review-m3-final.md` and `evidence/review-m4-final.md`; preprocessing
correspondence and exact evidence are in `docs/PREPROCESSING.md` and
`docs/LITERALS-AND-INPUT.md`. The full required backlog is
`docs/FEATURE-INVENTORY.md`.

The initial profile is strict C++20 with fixed LP64 integer/pointer widths
and signed plain char. Target configuration is separate from semantic types,
but additional target models, floating formats, alignment and object layout are
not implemented or claimed. Controlled configured in-memory includes/macros are
qualified for the bounds below. The opt-in filesystem provider is exercised on
Linux x86-64 LP64 little endian with GNU libc major2 >=2.33; other hosts/ABIs and
real-header compatibility remain unsupported or unverified.

| Area | Implemented bounds | Partial, unsupported or unverified |
| --- | --- | --- |
| Source/tokens | Retained physical sources, UTF-16 half-open ranges; phase-2 splice maps with real-context raw restoration; ASCII identifiers; immutable token intervals/cursors; changed streams use a real presentation buffer and immutable spelling/expansion/include/presumed-location DAG | Unicode identifier/UCN semantics remain required. Optional provider strictly decodes UTF-8 and BOM-marked UTF-16; other encodings explicitly unsupported. Arbitrary spans retain contributors without fabricated cross-file ranges; zero-width splice anchors are forward biased |
| Lexical diagnostics/literals | Active numeric grammar and invalid quotes/comments/raw termination; inactive/unused language checks deferred; target-aware ordinary/prefixed character graph values, ASCII/simple/octal/hex escapes, strict range/count checks, first-character sign-aware multichar packing and warning severity | Unicode/UCN character values, complete string/floating/UDL values and encoding/concatenation remain required. Native accepts a malformed unused raw replacement that this bounded path diagnoses Unsupported; nonordinary #line compatibility is explicitly partial |
| Preprocessing | Object/function/variadic macros, lazy prescan/inertness/caller-tail rescan, #/##/placemarkers/VA_OPT and full32-label upstream matrix; configured includes/guards/once; numeric intmax/uintmax64 plus witnessed character/bool32 PP behavior; #line/error/strict-warning; __LINE__/__FILE__/__cplusplus/date/time/hosted and __has_include | Default/in-memory path has no host fallback. Optional structured provider separates probe/read/failure caches and activation. Unicode/UCN, other special operators, general pragmas/extensions and complete lexical compatibility remain required. Include entry/EOF are lookahead barriers |
| Types/declarations | Canonical bool, char, signed/unsigned char/short/int/long/long long, distinct wchar_t/char8_t/char16_t/char32_t; const scalars; void function results; simple aliases/scalar variables | No pointers/references/arrays, compound/function typedef declarators, multiple declarators, direct/list initialization, storage/linkage specifiers; const void unsupported. Isolated reference implementation is not yet integrated |
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

Date/time defaults capture UTC once per session; an explicit timestamp override
is normalized to UTC, retained in effective Options/results and stable across
analyses. English month names and space-padded days are deliberate. Optional
filesystem host interop is isolated from frontend semantics: same-descriptor
nonblocking open/regular-file validation, strict decoding and typed errors. Its
lexical allowed roots are not a symlink security boundary, and byte bounds do
not guarantee wall-clock I/O latency/cancellation. Retained queries perform no I/O.
