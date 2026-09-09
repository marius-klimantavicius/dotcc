# Public upstream test adaptation

The pinned public SQLite 3.50.4 `test/jsonb01.test` contains setup, a table of 18
JSON paths with exact expected JSON text, two removal queries per path, and one
malformed-JSON error probe. `scripts/generate-upstream-jsonb.py` adapts all 37 SQL
assertions plus setup into a C API harness without a Tcl dependency.

The adapter reads the unchanged source, checks its expected table shape, preserves
SQL and expected JSON bytes, and binds `$path` using `sqlite3_bind_text`, equivalent
to the upstream Tcl binding. It also checks SQL column types, byte lengths, step/
finalize result codes, and the exact malformed-JSON message. Unexpected source
shape aborts generation; cases are never silently skipped. Generated C and the
manifest with source hash/case IDs live under `generated/`.

Run `scripts/test-upstream-native.sh` from `sqlite/`. All 37 assertions pass using
the pinned native amalgamation and campaign memory VFS; its transcript is saved in
`tests/upstream-jsonb.expected`. Translated execution of this identical generated
harness is still pending. Other upstream test files have not yet been executed;
this is not a claim of passing the entire public SQLite suite or TH3.
