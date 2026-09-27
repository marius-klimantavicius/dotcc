# Managed embedding qualification

`QuickJs.Behavior SUITE CAMPAIGN_ROOT [UPSTREAM_SOURCE]` runs one of `abi`,
`behavior`, `lifecycle`, `upstream`, or the expected-failure `fail` control.
Use `scripts/test.py` through the campaign framework to build/run the selected
raw/processed and JIT/AOT cells with bounded subprocess deadlines.

Every native callback thunk uses the actual emitted `JSValue` aggregate. The
friend assembly grants test-only access for ABI probes and real collection;
ordinary consumer usage remains through `QuickJsRuntime`/`QuickJsContext`.

A passing case emits `PASS <id>`. JavaScript fixtures and selected upstream
entrypoints emit `CASE PASS <id>`. The final `SUMMARY` is emitted only after the
selected suite returns; any assertion, exception or disposal leak exits nonzero.
The upstream adapter compares the complete ordered case-ID list to the frozen
manifest. The sole selected-file exclusion is the documented OS timer fixture.
