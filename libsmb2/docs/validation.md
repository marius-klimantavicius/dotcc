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

## Frontend closure

- `81b3769`: relative include resolution using physical source identity and Linux
  network/endian headers. Forty-four focused unit tests plus native/translated
  endian and ABI functional coverage pass.
- `9cac4d1`: standalone anonymous enums, GNU unused attributes, and scoped GNU
  statement expressions through typed IR and C# emission. Thirty-four focused
  unit tests and four functional checks, including source/object linking, pass.
- Fresh complete probe: **53/53 clean preprocessing/lexing, 53/53 clean parsing,
  53/53 native syntax controls**, with no unresolved-include diagnostics.

The object-emission census is the next independent gate. Its newly exposed
flexible-array initializer issue and missing runtime networking services are being
reduced and repaired; frontend success does not establish a buildable product.

## Host services and object lowering

- `8f1d8ab` repairs typedef-array global storage and sized static flexible-array
  initializers with native-matched focused and emitted-runtime regressions.
- `ccbfbda` implements actual nonblocking socket I/O, IPv6, descriptor flags and
  poll readiness/timeouts, including Linux `sockaddr_storage` layout. Focused
  socket, ABI, resolver and aggregate unit checks passed 38/38 at this checkpoint.
- DNS, owned resolver results, secure entropy, and single-call vector I/O now have
  shared BCL runtime implementations. Their translated C regression was first
  shown failing on the absent runtime symbols before implementation. Native C and
  managed JIT results match; direct tests check IPv4/IPv6 address/port layout,
  ownership, errors, buffer guards, datagram preservation and scatter order.
- `python3 libsmb2/scripts/test-host-services.py`: **three native/JIT/NativeAOT
  fixtures pass in all three forms**: Linux endian/layout, IPv4/IPv6 nonblocking
  TCP with poll, and resolver/entropy/vector I/O. See
  `artifacts/host-services/results.json` for exact compiler hashes and commands.

These service checks do not validate the SMB engine. The complete 53-unit source
closure is being regenerated through the actual product pipeline to expose the
next link/C# compilation failures. P1 crypto/complete ABI gates and all managed
SMB interoperability gates remain open.
