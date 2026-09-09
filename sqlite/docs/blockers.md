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

## B007 — self-referential object/function macro rescan (preprocessor, active)

After B006, `vfsList` (reported26735/raw26892) leaves `GLOBAL(sqlite3_vfs*,vfsList)`
in actual preprocessed C. The object alias's macro hide state is lost before the
function-like GLOBAL expansion. This defect is also visible in the pristine
preprocessed baseline. Worker is reducing/fixing shared rescan semantics.
