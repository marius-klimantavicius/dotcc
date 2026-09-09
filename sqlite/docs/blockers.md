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
