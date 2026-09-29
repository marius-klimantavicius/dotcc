# Upstream test inventory

`config/test-inventory.json` inventories every file below the pinned `test/`
directory and all textual `TEST_IMPL`, `HELPER_IMPL`, `TEST_DECLARE` and
`HELPER_DECLARE` occurrences (including filesystem macro variants) and registrations with their source preprocessor guards. There are
530 textual test/helper definitions across all platforms. Declarations are separate
records and must not be counted as additional executed cases. Textual guards are
provenance, not a completed per-backend applicability decision.

`native-upstream` builds the upstream `uv_run_tests_a` runner with its original
assertions and timeouts, obtains its runtime `--list`, then runs the whole native
suite. It copies fixtures into a private ordinary directory before execution.
Results preserve each TAP pass/failure/skip and the original transcript; incomplete
case counts or any native failure fail this oracle gate. Native skips retain
the upstream reason and do not establish managed support.

No cases have been excluded from translated qualification. Every translated
upstream case is currently unrun. Native evidence does not change that status.
