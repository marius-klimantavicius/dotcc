Executed on 2026-10-29 with .NET SDK 10.0.112 / runtime 10.0.12 on Linux x64.

All four private source-identical controls passed:

- **raw/jit**: 3,277 getters + 92 zero-argument factories; every measured delta exactly 0 bytes; allocation-counter positive control 288 bytes. Build 19.353 s; execution 9.681 s.
- **raw/nativeaot**: 3,277 getters + 92 zero-argument factories; every measured delta exactly 0 bytes; allocation-counter positive control 288 bytes. Build 56.905 s; execution 4.02 s.
- **processed/jit**: 3,277 getters + 92 zero-argument factories; every measured delta exactly 0 bytes; allocation-counter positive control 288 bytes. Build 17.643 s; execution 9.836 s.
- **processed/nativeaot**: 3,277 getters + 92 zero-argument factories; every measured delta exactly 0 bytes; allocation-counter positive control 288 bytes. Build 57.802 s; execution 3.87 s.

All four complete warmup passes also recorded 0 bytes across all 3,369 cases.
JIT runs used default tiering. NativeAOT runs reported dynamic code unsupported;
actual ILC response files contain `--root:FactoryAllocation.raw` /
`--root:FactoryAllocation.processed` and `--parallelism:1`.

Command used from the repository root:

```sh
python3 libghostty/tests/FactoryAllocation/run.py \
  --work libghostty/build/factory-allocation/canonical-20261029-run1
```

Evidence is preserved in
[receipt.json](../../build/factory-allocation/canonical-20261029-run1/receipt.json),
with exact commands, logs, per-source SHA-256 values, canonical project/manifest
identities, accepted PE identities and the four executable/result hashes.
[review.json](../../build/factory-allocation/canonical-20261029-run1/review.json)
records the final hash/entrypoint checks and actual ILC root arguments.
Per-case outputs and call inventories are in each form's `outputs/*-result.json`
and `cases.json`. Every getter hash and factory checksum matched its accepted PE
report, including all returned bytes.

Canonical source fingerprints:

- raw: `FE619269D5E4DC978F57D4E72DA5B61755A0D24618BC84E72CBCE0E3BF3D231F` (57 files, 26,267,155 bytes).
- processed: `A8DF7260314C0AF9BF354645F7870BE022523C330751B3897A1781D320CE2E01` (57 files, 26,444,812 bytes).

Every canonical source, project and manifest remained byte-identical. The
canonical source sets contain zero diagnostic entrypoints. Only the private
executables include the authored partial diagnostic driver.

Validator negative controls rejected a nonzero individual allocation even with
a zero summary, wrong getter/factory bytes, missing cases, wrong runtime and
missing full-set warmup; the original result was accepted. See
[validator-checks.log](../../build/factory-allocation/canonical-20261029-run1/validator-checks.log).

The one argument-taking factory
`__DotccAggregateInit_000B66A9C2E585BCF91DD13909745B1897C7950506A9499E3EAF97A97356D669`
is explicitly excluded from dynamic allocation measurement; its accepted
IL/callgraph remains the evidence here, and actual consumer coverage is a
separate obligation. These results measure warmed managed allocations in a
private executable compiling exact canonical sources. They do not measure
native allocations, cold initialization, terminal semantics or an existing
canonical product executable. See [README.md](README.md) for the full method
and exclusions.

Owned additions: `Driver.cs`, `run.py`, `README.md`, and this `RESULTS.md`, all
under `libghostty/tests/FactoryAllocation/`. No staging or commits were made.
No build/execution failures occurred in this diagnostic run; all build/run logs
and the separate preparation-only receipt are retained.
