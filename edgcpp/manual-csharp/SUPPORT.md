# Supported behavior

Status: the bounded M0 baseline, partial M1 lexical foundation and first
integrated M2 slice, scalar overload checkpoint and early M4 difficult-path gate
and the bounded integrated preprocessing checkpoint are independently reviewed
and accepted (`evidence/review-m1-final.md`). The subsequent bounded character,
builtin and filesystem-input extension is independently accepted within its
executed bounds (`evidence/review-m1-literals-final.md`);
The following bounded scalar-reference integration is independently accepted
(`evidence/review-references-final.md`). The following scalar-reference constant-storage
extension is accepted in `evidence/review-reference-constexpr-final.md`. Release
build and all 102 named groups pass in managed
execution and a published Linux x64 NativeAOT
executable. This inventory does not establish C++20/EDG completeness or broader
platform/product qualification. See `evidence/review-final-m0-m2.md` and
`evidence/review-m3-final.md` and `evidence/review-m4-final.md`; preprocessing
correspondence and exact evidence are in `docs/PREPROCESSING.md` and
`docs/LITERALS-AND-INPUT.md`, `docs/REFERENCES.md` and `docs/CONSTANT-STORAGE.md`. The full required backlog is
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
| Types/declarations | Canonical integer/character kinds; const scalars; scalar lvalue/rvalue references, alias collapse and explicit binding/materialization facts; void function results; simple aliases/variables | No pointers/arrays/class or dependent references, complete cv/compound declarators, multiple declarators, direct/list initialization, storage/linkage specifiers; const void unsupported |
| Functions/scopes | Source-ordered scalar free-function overloads, compatible entity identity, by-value/scalar-reference parameters/results; pairwise rank/kind/cv comparison and nearest lexical hiding | Pointer/class/UDC/ADL overload branches, defaults/variadics/block-scope functions, namespaces/classes and general symbol-state rules remain required. Global `main` special semantics unsupported |
| Expressions/statements | Direct/parenthesized ordinary calls; explicit scalar conversion nodes; returns; empty/declaration/expression statements; integer `+ - * / %`, comparisons, bitwise operators and unary `+ - ! ~`; contextual-bool `&&`/`||` with real short circuit; implemented alternative operator spellings map to the same token kinds | Assignment, shifts, comma/conditional, casts, member access, function decay, labels/control flow and other missing forms are explicitly unsupported |
| Constants | Radix/suffix/rank/width rules; optional folds plus separate constexpr integer work/call/block/local execution; static_assert/constexpr-variable required evaluation; explicit failure/dependency/resource outcomes; scalar locations, reference transport, explicit loads, temporary/frame cleanup and original-initializer replay | General object/class/pointer lifetime execution, assignment/control flow/void/general interpreter/classification and extended integers remain required |
| Function templates | Named direct type parameters, immediate dependent prototype graph and nondependent checks, actual retained-token replay, by-value scalar deduction without promotions, canonical instances/reuse, frozen ordinary lookup, ordinary-template ties and direct-pattern ordering | No template forward/redeclarations, explicit arguments/specialization, compound/ref/pointer/class/member/ADL templates, packs/defaults/constraints or general partial ordering |
| State/replay | Independent session state/retained graphs; nested template replay and owner-journaled mutations; recursively scoped reference/function signatures avoid session retention, with valid/invalid/unsupported live-session collection tests | Every future compound must extend recursive ownership guards. Root-owner restores replay; child replay Checkpoint/Restore prohibited. General speculation/resource/cancellation/IDE qualification remains outstanding |

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

Remaining M3 complete reference/class/advanced overload semantics, general M4 templates/
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

Reference calls expose declared return identity separately from referent type and
LValue/XValue category; named rvalue references remain lvalues. Known temporary
identity/owner facts are bounded, not a full lifetime proof. Additional managed
warnings for named temporary hazards are intentional and are not native warning
parity; valid binding remains successful. Complete reference/template and general object-lifetime behavior remains required;
the accepted scalar constant-storage bounds are documented separately.
