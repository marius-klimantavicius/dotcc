# SQLite blocker ledger

## B001 — callback returning a callback (parser, open)

Full retry: `scripts/translate.sh`, initial compiler revision `681e94a`.
Preprocessing succeeds. Parsing stops at unmodified `sqlite3.c:1819:10`:
`unexpected '(' ... expected ')' or 'ID'` on
`void (*(*xDlSym)(sqlite3_vfs*,void*,const char *zSymbol))(void);`.

Regression: `DotCC.FunctionalTests/Fixtures/fnptr-returning-fnptr-field`.
Native GCC C17 output is `7`; the original dotcc fails at the corresponding
nested declarator before emission. Evidence: `artifacts/fnptr-before.log`.
Structural fix and full-amalgamation retry are in progress.

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
