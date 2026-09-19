# Campaign validation

Implementation is in progress. P0 establishes inputs and reference behavior;
no managed SMB product or P1–P6 completion is claimed yet.

## P0 reference baseline

- `dotnet build DotCC/DotCC.csproj -c Release --nologo`: passed before repairs.
- Existing unit baseline: **2,257 passed**, zero skipped/failed, recorded by the
  coordinator under `artifacts/compiler-baseline/` before production fixes.
- Initial full parse/token probe: native 53/53 syntax checks; dotcc 36/53 clean
  token streams and 30/53 clean parses. [Detailed initial report](parse-probe.md).
- `./libsmb2/scripts/oracle.sh`: **11/11 native/Samba cases pass**. Five signed
  dialects (2.0.2 through 3.1.1), three encrypted dialects (3.0, 3.0.2, 3.1.1),
  and rejection of wrong credentials, a missing share, and SMB2 access to an
  encryption-required share. Each positive case writes/reads and compares 65,537
  bytes, checks EOF, flush/metadata/truncate, rename, enumeration, and deletion.
- The native library builds all configured units. Server image identity, package
  inventory, commands, per-case outputs and binary hashes are recorded in
  `artifacts/native-oracle/results.json` and adjacent logs.

The first full translation pipeline attempt uses a frozen pre-repair compiler
and stops at the known `aes128ccm.c` attribute/include blockers. It emits no final
product. Evidence: `artifacts/translation/result.json` and the per-unit logs.
The pipeline's later build/postprocessing/promotion stages remain unqualified
until the compiler and required host services support the complete source closure.

The new frontend/header regressions were demonstrated failing against the baseline
before their repairs. The coordinator records current regression and full-source
retry results separately; the initial probe report is intentionally historical.
Subsequent compiler fixes require a new complete translation and runtime campaign.
