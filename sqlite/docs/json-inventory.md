# SQLite 3.51.3 JSON inventory

Inventory comes from the pinned `sqlite3RegisterJsonFunctions` registration table
and `sqlite3JsonTableFunctions`, not a moving release's function list.

The SQL probe covers `json`, `jsonb`, `json_array`, `jsonb_array`,
`json_array_length` (1/2 arguments), `json_error_position`, `json_extract`,
`jsonb_extract`, `->`, `->>`, `json_insert`, `jsonb_insert`, `json_object`,
`jsonb_object`, `json_patch`, `jsonb_patch`, `json_pretty` (1/2 arguments),
`json_quote`, `json_remove`, `jsonb_remove`, `json_replace`, `jsonb_replace`,
`json_set`, `jsonb_set`, `json_type` (1/2 arguments), `json_valid` (1/2 arguments),
`json_group_array`, `jsonb_group_array`, `json_group_object`, `jsonb_group_object`,
`json_each`, `json_tree`, `jsonb_each`, and `jsonb_tree`. Debug-only `json_parse` is excluded from the release
profile. The 3.51.3 update adds the JSONB-valued table variants, covered by the host WAL SQL contract.

Coverage includes JSON5, malformed input, Unicode escapes/surrogates, JSON null,
negative array indexes, extracted SQL versus JSON values, explicit JSON null /
missing path / SQL NULL / text `"null"` distinctions, JSONB validity flags,
stored JSONB updates, constructors, changes, aggregates and table traversal.
The native transcript preserves types, lengths and exact text/blob bytes;
JSONB bytes are compared only within this pinned version. The 39-case core
corpus also records extended result codes and exact error text for constraint,
FTS, malformed JSON/JSONB and invalid-path failures. Its translated transcript
matches the native mmap-disabled profile exactly. Separate API and VFS checks
also pass, including all 128 allocation-failure recovery cases. All 37 adapted upstream
JSONB cases pass through both engines; see `upstream-tests.md`.
