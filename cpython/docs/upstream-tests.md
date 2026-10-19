# Upstream and native test inventory

The initial native control is executable against the exact-pin statically built
CPython, with isolated paths. It checks the useful JSON/callback/import/file
workflow, exact bytes, returned values/output, caught error/traceback, real workers
and TLS values, cyclic GC, two initialization/finalization cycles, and expected
nonzero exits for deliberate assertion and harness errors. It does not qualify
translated CPython. See validation.md for fresh receipts.

`config/stdlib.json` is a dependency/delivery inventory, not a qualification case
list. B supplies `tests/Upstream/inventory.seed.json` and `run_inventory.py` with
in-interpreter serial discovery and explicit required/excluded case accounting.
The seed currently has no frozen required IDs. Missing `_testcapi`,
`_testinternalcapi` and other required helpers remain blockers, not exclusions.
A separate test profile must preserve the selected upstream assertions without
installing test-only modules in the product. Exact discovery, per-case selection,
resource exclusions and translated case execution remain unpassed P0/P4 gates.

B's `tests/fixtures` and authored consumer are independent foundations. Their
contracts must execute through the actual translated exports, and their fresh
native reports belong to B's ledgers. Recipe verification retains every required
suite name and raises a clear blocker for unintegrated translated runners; it
cannot pass with only the native control and consumer declaration.

B's completed round-2 seed now supplies **18 mandatory source-declared IDs** as
an unfrozen floor, plus `tests/Upstream/helper-contract.json` identifying 65
source paths and the real helper/import dependencies. This is not a classified
complete inventory. `unicodedata` proved necessary for product named-Unicode-
escape parsing and is now in A's ordinary product/native manifests; its new
native behavior case passes. `_socket`, `_posixsubprocess`, `_testcapi`,
`_testlimitedcapi` and `_testinternalcapi` still require the separate real test
profile. No missing dependency became an exclusion, no translated case executed,
and P0/P4 remain open. Rediscovery/classification after helper preparation is
still required; B's earlier blocked discovery counts are historical observations.
