# SQLite 3.50.4 JSON inventory

Inventory comes from the pinned `sqlite3RegisterJsonFunctions` registration table
and `sqlite3JsonTableFunctions`, not a moving release's function list.

The SQL probe covers `json`, `jsonb`, `json_array`, `jsonb_array`,
`json_array_length` (1/2 arguments), `json_error_position`, `json_extract`,
`jsonb_extract`, `->`, `->>`, `json_insert`, `jsonb_insert`, `json_object`,
`jsonb_object`, `json_patch`, `jsonb_patch`, `json_pretty` (1/2 arguments),
`json_quote`, `json_remove`, `jsonb_remove`, `json_replace`, `jsonb_replace`,
`json_set`, `jsonb_set`, `json_type` (1/2 arguments), `json_valid` (1/2 arguments),
`json_group_array`, `jsonb_group_array`, `json_group_object`, `jsonb_group_object`,
`json_each`, and `json_tree`. Debug-only `json_parse` is excluded from the release
profile. This version has no `jsonb_each`/`jsonb_tree` table variants.

Coverage includes JSON5, malformed input, Unicode escapes/surrogates, JSON null,
negative array indexes, extracted SQL versus JSON values, JSONB validity flags,
stored JSONB updates, constructors, changes, aggregates and table traversal.
The native transcript preserves types, lengths and exact text/blob bytes;
JSONB bytes are compared only within this pinned version. Comprehensive randomized,
failure and translated execution coverage remains pending.
