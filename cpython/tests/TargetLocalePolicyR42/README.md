# Target locale policy controls

`test_policy.py` exercises the whole-target policy and every original ordered
193/286 unit's common/per-unit override guards. `source_review.py` binds both
profiles, complete selected headers/Host/runtime dependencies, original sources,
builtins and asset plan. `run_native.py` builds a new complete same-pin native
archive with the actual selected feature and calls the unchanged Host. Native
DSOs remain control-only. `run_headers.py` compares actual supplied-header
layouts and real V22 object emission before/after without generating a product.

```bash
python3 -B -m unittest discover -s cpython/tests/TargetLocalePolicyR42 -v
python3 -B cpython/tests/TargetLocalePolicyR42/source_review.py \
  --policy cpython/config/pyconfig-policy.json \
  --report cpython/build/owner-b-r42-20261018/live-source-review.json
```

Native/header drivers require the coordinator's direct SAFE handoff and an
unused output directory. The completed R42 run, original failed fixture
attempts, source/archive pins, process PIDs, exact commands/environment and all
raw reports remain under `cpython/build/owner-b-r42-20261018/`. Drivers use the
existing process helper and individual production locks. They never modify
upstream source or regenerate a product.

The native probe runs six original cases with original method/decorator/source
hash checks and 158 boundary rows per mode; both actual `utf8_mode` values remain
observable. The original 104 marker statuses are retained separately. Header
controls do not prove the changed real CPython algorithms on wchar16: fresh
A32 product generation and B41 actual early-four/full tests are the next gate.
The two mode-1 witnesses stay required pending coordinator review of fresh
outcomes. No new waiver, source cut or test reclassification is introduced.

The baseline `_Static_assert((wchar_t)-1 > 0)` limitation is preserved in
`header-controls-object-options/embedded-gil-baseline-object.log`. The fixture
keeps native-header signedness, target width/LP64 checks and real layout
emissions; B41 already supplies the independent runtime unsigned16 proof.
