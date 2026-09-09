# SQLite blocker ledger

## B001 — callback returning a callback (parser, verification in progress)

Full retry: `scripts/translate.sh`, initial compiler revision `681e94a`.
Preprocessing succeeds. Parsing stops at unmodified `sqlite3.c:1819:10`:
`unexpected '(' ... expected ')' or 'ID'` on
`void (*(*xDlSym)(sqlite3_vfs*,void*,const char *zSymbol))(void);`.

Regression: `DotCC.FunctionalTests/Fixtures/fnptr-returning-fnptr-field`.
Native GCC C17 output is `7`; the original dotcc fails at the corresponding
nested declarator before emission. Evidence: `artifacts/fnptr-before.log`.
The first structural fix passed full-amalgamation retry through this declaration;
SQLite next stops at B002. Executable regression then exposed missing `static`
handling for function-returning-callback signatures, which is also being fixed.

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

## B002 — callback output parameters (parser/IR, verification in progress)

The full retry after B001 fails at `sqlite3.c:7861:31`, `unexpected '*'`, on
`void (**pxFunc)(sqlite3_context*,int,sqlite3_value**)` inside xFindFunction.
Regression `fnptr-output-parameter` prints `1 42` in GCC and fails in original
dotcc. The fix preserves pointer-to-callback storage as `Pointer(Func)`, including
real dereference when calling through it; `&function` remains canonical `Func`.
Evidence: `artifacts/fnptr-output-before.log`, `artifacts/offsetof/sqlite-retry.stderr`.

## B003 — JSON arrow stringification (preprocessor, open)

The actual preprocessed SQLite registers the JSON operator as `"-> >"`, because
macro stringification unconditionally inserts a space between adjacent tokens.
Strict GCC gives `NAME(->>)` as `"->>"`; dotcc gives `"-> >"`.
Reduced native/old-compiler evidence is in `artifacts/parse-probes/11_stringify_json_arrow`.
Worker is adding regression coverage and fixing raw argument/whitespace handling.
