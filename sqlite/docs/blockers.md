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

## B014 — combined engine include context (parser, active)

`scripts/emit-engine.sh` compiles `src/engine.c`, including unchanged sqlite3.c
and the memory VFS. It currently fails at reported8614:27, unexpected `(`,
expected `*` or identifier. The amalgamation alone emits; reduction is ongoing.
Evidence: `artifacts/engine-emission.log`.

## B015 — nested switch labels (C# emitter, active)

The amalgamation alone also emits managed-library source and its project with
the actual offsetof analyzer. The first Roslyn build reports twelve syntax
diagnostics: ten `switch expected` and two missing braces, in two functions.
Reduction is ongoing; compilation and execution are not yet established.
Evidence: `artifacts/sqliteonly-build.log`, `generated/SqliteOnly/Program.cs`.
