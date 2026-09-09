# SQLite blocker ledger

## B001 — callback returning a callback (parser/emitter, fixed 0071282)

Full retry: `scripts/translate.sh`, initial compiler revision `681e94a`.
Preprocessing succeeds. Parsing stops at unmodified `sqlite3.c:1819:10`:
`unexpected '(' ... expected ')' or 'ID'` on
`void (*(*xDlSym)(sqlite3_vfs*,void*,const char *zSymbol))(void);`.

Regression: `DotCC.FunctionalTests/Fixtures/fnptr-returning-fnptr-field`.
Native GCC C17 output is `7`; the original dotcc fails at the corresponding
nested declarator before emission. Evidence: `artifacts/fnptr-before.log`.
The first structural fix passed full-amalgamation retry through this declaration;
SQLite next stops at B002. Executable regression also exposed missing `static` handling and indirect-call
argument coercion; both fixed and executable regressions now pass.

## Upcoming reduced probes (not full retry results yet)

Worker verified the following valid C17 fragments with strict GCC and reproduced
old-dotcc parser failures in `artifacts/parse-probes/`. Each will be promoted to a
blocker when reached by a full retry; source hints are evidence of active usage.

- `void (**pxFunc)(...)` callback output parameters (`sqlite3.c:7864`).
- Tagged nested struct definitions with pointer members (`sqlite3_index_info:7988`).
- Static tagged aggregate definition + variable (`sqlite3StatType:24341`).
- Pointer-to-function-pointer member/local/casts (autoext:139841,139889).
- Static const function-pointer arrays (`sqlite3BuiltinExtensions:182695`).
- Local tagged struct definition with variable (`EncName:142886`).

These are generic parser issues; no upstream C modifications are planned.

## B002 — callback output parameters (parser/IR, fixed 0071282)

The full retry after B001 fails at `sqlite3.c:7861:31`, `unexpected '*'`, on
`void (**pxFunc)(sqlite3_context*,int,sqlite3_value**)` inside xFindFunction.
Regression `fnptr-output-parameter` prints `1 42` in GCC and fails in original
dotcc. The fix preserves pointer-to-callback storage as `Pointer(Func)`, including
real dereference when calling through it; `&function` remains canonical `Func`.
Evidence: `artifacts/fnptr-output-before.log`, `artifacts/offsetof/sqlite-retry.stderr`.

## B003 — JSON arrow stringification (preprocessor, fixed c679ee4)

The actual preprocessed SQLite registers the JSON operator as `"-> >"`, because
macro stringification unconditionally inserts a space between adjacent tokens.
Strict GCC gives `NAME(->>)` as `"->>"`; dotcc gives `"-> >"`.
Reduced native/old-compiler evidence is in `artifacts/parse-probes/11_stringify_json_arrow`.
Regression suite and actual SQLite preprocessing now preserve both operators;
raw STR(VALUE), two-level expansion, escaped literals, comments, variadic commas,
and empty argument/replacement whitespace are covered.

## B004 — nested aggregate pointer/array members (parser, fixed 0071282)

Full retries advanced from reported line7990 (`*aConstraint`) to line18863
(`aCol[FLEXARRAY]`). Native-verified functional fixtures
`nested-tagged-pointer-member` and `nested-tagged-array-member` both print `42 17`.
Pointer and sized-array syntax now parses and runs. A related typedef lexer bug
mistook a callback field for the enclosing typedef name; `typedef-nested-callback`
prints `42 8` and now passes. Upstream source remains unchanged.

## B005 — flexible nested array and storage (parser/layout, fixed becd09e)

After sized-array fix, full SQLite stops at reported `18863:19`, closing `]`;
raw source `sqlite3.c:18967` is `struct sColMap {...} aCol[FLEXARRAY]` and the
C17 profile expands FLEXARRAY empty. Existing generic flexible arrays also model
`[]` as `[1]`, which cannot satisfy native sizeof/alignment. Parser and generator now implement zero-storage flexible tails with explicit
header size, alignment anchors and generator-backed tail pointer accessors.
Native and translated regression headers agree at size4/8 instead of the prior
8/16/24. No FLEXARRAY macro override was introduced.

Diagnostic note: existing line-continuation splicing shifts reported coordinates
from physical source lines. Raw locations are recorded where known. Fixing source
mapping is a remaining M1 correctness task.

## Offset generator integration

Commit `4db77b5` adds shared layout, incremental generator, constant designators,
standalone/project/GeneratorDriver/object integration and native-checked fixtures.
Full suite caught eager recursive CType.Slice formatting for unused Zig metadata;
fixed by nonrecursive type-kind diagnostics. Unknown requested layouts still fail
clearly. See `sqlite/generators/README.md` for remaining FAM/ABI/AOT work.

## B006 — tagged definition with variable (parser, fixed c32dd74)

After B005, SQLite reaches `sqlite3StatType` (reported24191/raw24341), a tagged
struct definition used as a type specifier with a variable declarator. Parser
worker added a native-verified fixture and structural Type productions. The
functional suite passes 242 tests, and the obsolete FAM emitted-string assertion
was updated and verified against the correct zero-storage representation.
Full SQLite retry reaches B007 without changing upstream source.

## B007 — self-referential object/function macro rescan (preprocessor, fixed fba0378)

After B006, `vfsList` (reported26735/raw26892) leaves `GLOBAL(sqlite3_vfs*,vfsList)`
in actual preprocessed C. The object alias's macro hide state was lost before the
function-like GLOBAL expansion. Tokens now retain their disabled-macro sets
through object/function boundaries. Function rescan intersects invocation and
closing-token sets so external arguments still expand correctly. Native-verified
tests cover self references, aliases, cycles, nested arguments, and the boundary
case `#define A F` / `#define F(x) x` / `A(A)` producing `F`.
Full preprocessing has zero stray GLOBAL calls and preserves JSON `->`/`->>`;
the complete parse retry advances to B008.

## B008 — register locals and for initializers (parser/IR, fixed ba0b0e2)

The next full parse stopped at reported35733/raw35947 in `sqlite3_strnicmp`:
`register unsigned char *a,*b;`. Added a real storage-class keyword/production,
lowered to automatic locals. The reduced fixture also caught for-initializer
dispatch bypassing storage-class declaration lowering; it now uses the common
declaration dispatcher. Native and translated output is `43`. Full retry reaches
B009; SQLite C remains unchanged.

## B009 — array-first mixed declaration (parser, fixed 6ae3c5f)

After B008, full SQLite stops at reported55596/raw55953:
`PgHdr *a[N_SORT_BUCKET], *p;`. The grammar accepted arrays in later
declarators but not an array before the first comma. Native/red reduced cases
cover pointer-array and multidimensional heads, scalar/pointer tails, and pointer
typedefs. Timed current retry: 0.80 seconds, 123632 KiB peak RSS; evidence under
`artifacts/parse-probes/register-sqlite-retry.*`.
The fix joins an array head into the shared declarator list while preserving
the first type and each later declarator's pointer levels. Native and translated
fixture output is `42 20 17 7 2 2`. Full retry advances to B010.

## B010 — comma in conditional middle operand (parser, fixed b8c11e1)

Full retry next stops inside the `putVarint32` replacement list (raw definition
21695, first affected call78591). C's conditional middle operand is a full
expression, permitting commas; the grammar accepted only an assignment-level
expression. The middle operand now uses `Expr`, preserving right association
of nested conditionals. Native and translated fixture output is
`30 7 16 14 20 1 42`, including untaken-arm side effects and a SQLite-style macro.
Full retry advances to B011 (0.99 seconds, 123952 KiB before source-map changes).

## B011 — callback storage in members, locals, and casts (parser, fixed 3d7d65e)

The complete parser now reaches physical `sqlite3.c:139841:10` (byte5075023),
the auto-extension table's `void (**aExt)(void)` member. The earlier callback
output-parameter fix does not cover these other declarator contexts. Reduced
native/red fixture covers member/local storage, abstract two-star casts,
callback indexing, and void/old-style signatures. Native output is `42 17 2`.
Source positions are now physical after commit50308f9; see `source-mapping.md`
for the remaining included-filename and macro-backtrace boundaries.
The fix preserves callback-storage pointer levels in members, local declarations,
and abstract casts. Twelve related callback fixtures pass; full retry advances
to the built-in extension callback array B012 (1.95 seconds, 221640 KiB RSS).

## B012 — direct callback arrays and nulls (parser/emitter, fixed 04555ea)

The next full parse stops at physical182695, `sqlite3BuiltinExtensions`, a static
array of const callback pointers. New declarator productions retain signature,
qualifiers, array bounds, and the existing pinned global-array representation.
The native/red regression then exposes emission defects for uninitialized
callback arrays, partial zero initialization, and callback/ordinary-pointer
comparison with integer-zero null constants. These now use pointer-sized backing
storage, typed initializer coercion, and the other operand's pointer context.
The fixture prints `42 17 1 1 9 2`; full retry advances to B013.

## B013 — scoped local callback typedef (parser/IR, fixed ccf6ab9)

Physical183277 in `sqlite3_config` declares a local callback typedef, then retrieves
one through `va_arg`. Existing typedef support handles only file scope. The new
native/red fixture covers callback alias shadow/restoration, scalar alias sizes,
no type-name leakage to a later function, and actual variadic callback retrieval.
Native and translated output is `59 16 42 17`. The fix uses the existing scoped
SymbolTable and scoped lexer names. Variadic callback arguments preserve their
typed function-pointer signature before conversion to pointer bits; tests invoke
both a direct function designator and a callback variable retrieved with va_arg.

## Inline aggregate array initialization (IR/emitter, fixed 037a1ab)

While reducing `sqlite3Stat` at physical24341, its two array members initialized
with `{{0,},{0,}}` exposed the lowerer's assumption that nested braces always
initialize structs. Typed inline-array IR now retains dense values and zero fill;
generated factories write actual inline storage and work in global/local/static
and compound-expression contexts without heap/delegate allocation. Factories use
full typed-shape hashes for deterministic cross-object identity. Tests also found
and fixed global array-member address projection. Native/translated primitive,
multidimensional, nested-aggregate, pointer and callback arrays pass, including
separate-object callback factory linking. See `aggregate-initializers.md`.

## Inline character-array strings (IR, fixed 65cd92d)

The complete typed-IR retry next reached the `aXformType` date-name table at
physical25380, whose `char zName[7]` member has string initializers. Inline arrays
now decode compatible byte/UTF-16/UTF-32 literals, omit terminators for exact-fit
arrays, zero-fill short strings, accept optional braces and nested rows, and
reject actual character overflow. The full configured amalgamation now emits
106,296 C# lines (3,641,990 bytes) in 3.09 seconds, peak RSS 771,092 KiB.

## B014 — completed include expansion (preprocessor, fixed 840ab96)

`scripts/emit-engine.sh` compiles `src/engine.c`, including unchanged sqlite3.c
and the memory VFS. It failed at reported8614:27, unexpected `(`, because tokens
already expanded inside the included file were rescanned by the outer expander
using the header's final macro definitions. This retrospectively expanded the
earlier sqlite3_mutex_alloc prototype. Completed include tokens now preserve
their expansion state. Native/red tests cover late definitions, redefine/undef,
nested includes, and a header-tail function name followed by parent parentheses.
Focused tests pass and the combined engine emits (3.34 seconds, 911,224 KiB RSS);
full checkpoint validation passes. Evidence: `artifacts/include-order/`.

## B015 — nested switch labels (C# emitter, fixed 4678f4e)

The first managed-library Roslyn build reported twelve syntax diagnostics:
ten `switch expected` and two missing braces, in VDBE/JSON functions. C permits
case labels nested within blocks, conditionals and loops. The backend now lowers
these switches to same-scope dispatch labels and explicit control-flow edges,
hoisting local storage while leaving initializer evaluation at its original site.
Native/red runtime tests cover shared locals, skipped initializers, outer-loop
continue, nested switches and Duff entry. Syntax errors are cleared; full
checkpoint validation passes. Evidence: `artifacts/sqliteonly-build.log`,
`artifacts/include-order/switch-baseline.log`.

## B016 — sizeof type-specifier and typedef folding (layout, fixed 304eff4)

A static comparison of all 30 emitted offsetof contracts to native finds 27
matches and three related WhereInfo mismatches. Its size and flexible-tail offset
are 736 rather than 864 because WhereMaskSet.ix has 32 elements rather than 64.
The token folder chooses the final `int` in `unsigned long long int`, then caches
that wrong size through SQLite's Bitmask typedef chain. It also ignores typedef
block scope and qualified pointer widths. Native/red fixture
`sizeof-type-specifier-order` records `64 260 272 264`, `8 8 2 2 4 8`,
and `4 8 12 8 4`. Complete types now defer to the existing scoped typed binder;
the unsafe token-level alias cache is removed. All 30 compiler contracts now
match native, and full checkpoint validation passes.
Evidence: `artifacts/sizeof-order/`, `artifacts/layout-metadata-before.log`.

## B017/B018 — opaque types and tentative globals (fixed a6ff6dc/7fdf9aa)

After B014/B015, the actual combined engine's C# build reports 225 CS0246
diagnostics for opaque pointer types (sqlite3_stmt, sqlite3_pcache, Fts5Context,
sqlite3_mutex, sqlite3_blob, Fts5Tokenizer, SQLiteThread, CCurHint), plus three
CS0102 duplicate tentative globals (sqlite3_temp_directory,
sqlite3_data_directory, sqlite3WhereTrace). The compiler now emits identity-only types behind opaque pointers, rejects
incomplete object storage, and preserves dotcc runtime-owned calendar/locale
aggregates. Tentative scalar/pointer globals share canonical symbols/storage;
real duplicate definitions and incompatible declarations diagnose. Native/red
regressions and full suites pass. Opaque FTS header types do not enable FTS.

## B019 — file-scope designated aggregate initializer (fixed dd1636b)

The combined virtual-table harness stops at tests/vtable_native.c:116 on
`static sqlite3_module numbers_module = { .iVersion = 1, ... };`.
Local and compound designated initializers existed, but file-scope forms did not.
The native/red fixture checks static const and external globals with callback,
integer, pointer and omitted fields. The new productions reuse existing typed
member initialization and the B018 canonical global storage registry.
Evidence: `artifacts/global-designated/`, `artifacts/translated-vtable-emission.log`.

## B020 — initialized arrays in mixed declarations (fixed 529a68c)

The allocation harness stops at tests/allocation_native.c:56 on
`int phase, i, rc, final_rc, nomem_results[2] = {0, 0};`. Array heads/tails
previously supported declarations only. New productions preserve the initializer
and reuse typed array lowering, dimensions and zero fill. The native/red fixture
checks scalar/pointer tails, pointer arrays, initialized multidimensional heads
and omitted values. Evidence: `artifacts/mixed-array-initializer/`.

## B021 — excessive switch flattening (C# compile performance, fixed 905c8b3)

After declaration fixes, the full C# compiler did not finish within 5:14.68.
A bounded read-only trace attributed 99.97% of the busy thread's sampled CPU to
Roslyn definite-assignment analysis. The VDBE method contained 1,234 labels,
1,643 goto edges and 319 hoisted locals. Retaining structured regions without
entry labels reduces those to 167, 590 and 48. The actual full compile now ends
in 7.32 seconds with 170 semantic diagnostics. A native-verified stress case,
compiler cancellation bound and full suites pass. See `switch-lowering.md`.

## B022 — switch prelude storage (fixed 8c45a24)

SQLite's yy_reduce declares yylhsminor before its first case. The IR discarded
all prelude statements, losing declarations as well as skipped initializers.
An unlabeled prelude section now retains storage and named-goto reachability.
Fixed arrays allocate before dispatch while initializer effects remain at their
original declaration sites; variable arrays bypassed by case entry diagnose.
Native regressions cover aggregate and multidimensional arrays and skipped side
effects. All yylhsminor diagnostics disappear in the actual retry.

## B023 — runtime DateTime name collision (fixed ea32962)

SQLite defines a DateTime aggregate in the same generated source as dotcc's
calendar/POSIX runtime. Runtime references now qualify global::System.DateTime.
The native/red fixture retains the C type and checks epoch formatting alongside
it. Actual engine diagnostics for calendar members and constructors disappear.

## B024 — included-file diagnostic provenance (fixed cb266d8)

Physical numeric positions survived macro expansion, but parser reductions lost
filenames. Token origins now survive rewrites and identity action AST creation;
IR diagnostics prefer that origin. Five regressions and the actual amalgamation
confirm nested headers, semantic errors, macro invocation and parent restoration.
See `source-mapping.md`.

## B025 — conditional pointer types and conversions (fixed 0b4d96d)

The actual engine reported 37 CS0029 and 15 CS1503 errors in pointer/null
conditionals. Binding now applies array decay, compatible pointee qualifiers,
void-pointer common types and proven null constants. Emission establishes that
common type before an outer C# cast/call can supply its own target type. Native
tests cover `_Generic`, sizeof, callback pointers, context conversions and skipped
side effects; incompatible operands diagnose.

## B026 — global pointer-member addresses (fixed 9003712)

Addresses of sqlite3Config.xLog and pLogArg incorrectly used pointer types as
Unsafe.AsPointer generic arguments (CS0306). The emitter now projects member
addresses from the containing global aggregate. A nested native callback-table
regression checks storage identity, pointer mutation, invocation and null values.

## B027 — wide compound shift counts (fixed d738f6f)

Two SQLite ulong shifts use long counts, rejected by C#. Counts now receive an
explicit int cast independently of the target type. Native tests retain exactly
one evaluation of indexed targets, function calls and comma-operator effects.

## B028/B029 — shared handlers and constant loops (fixed 1687ce7)

The old switch renderer moved JSON's to_double handler outside its locals' scope,
causing seven missing-name errors. Named-label switches now use the scoped
dispatcher, preserving storage and skipped initializers. Literal/enum loop truth
is emitted as C# constants, resolving two false missing-return errors. Other
expressions keep their runtime truth evaluation. Native tests cover shared
handlers, fallthrough, outer continue and condition side effects.

## B030 — default variadic promotions (fixed 6a1700d)

The final engine error was an ambiguous ushort-to-VaArg int/uint conversion.
Known C variadic arguments now explicitly promote small integers to int and
float to double. A native test checks signs, unsigned bounds and single evaluation.
The initial broad fallback affected Zig saturation helper overloads; the existing
full unit suite caught it. Promotion is restricted to known C argument tails,
and all Zig/frontend and repository tests now pass.

The full unchanged SQLite plus memory VFS now builds as a managed C# library
with zero errors. Fifty warnings remain across pointer identity/inline arrays,
unreachable code, empty statements, self-assignment, byte-range comparisons and
unused labels; storage and runtime validation are separate requirements.
Evidence: `artifacts/pointer-conditionals/engine-scope-build.log` and
`artifacts/variadic-promotions/test-before.log`.
