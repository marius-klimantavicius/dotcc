# Campaign validation

Implementation is in progress. P0 establishes inputs and reference behavior;
the complete translated and post-processed project now builds, while SMB
interoperability and P1–P6 completion remain separate gates.

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

The first full translation pipeline attempt used a frozen pre-repair compiler
and stopped at the known `aes128ccm.c` attribute/include blockers. It emitted no final
product. The current translation receipt now records the later successful attempt.
This historical attempt was superseded by the successful full pipeline below.

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

The subsequent object-emission census exposed flexible-array initialization and
missing networking services. Those repairs are recorded below; frontend success
alone did not establish a buildable product.

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
- `python3 libsmb2/scripts/test-host-services.py`: **seven native/JIT/NativeAOT
  fixtures pass in all three forms (21 executions)**: Linux endian/layout,
  IPv4/IPv6 nonblocking TCP with poll, linger, resolver/entropy/vector I/O,
  host identity/PRNG, protocol lookup/errno, and byte-preserving asprintf. See
  `artifacts/host-services/results.json` for exact compiler hashes and commands.

These host-service checks are separate from the SMB engine. The subsequent
complete 53-unit generation succeeded, followed by crypto/ABI and Samba checks;
current qualification results are recorded below.

## Complete product generation

- Default `./libsmb2/scripts/translate.sh`: **PASS**. All 53 unchanged upstream
  units emit and link into 12 C# source files. The raw and processed projects
  build with zero errors. Two unused-field warnings describe externally filled
  vector I/O layout fields.
- Semantic postprocessing rewrites 3,569 `Cond.B` calls, simplifies 216 boolean
  comparisons, and removes 97 standalone empty blocks across 11 source files.
- Final product: `generated/TranslatedLibsmb2/TranslatedLibsmb2.csproj`; separate
  raw comparison: `generated/TranslatedLibsmb2.Raw/TranslatedLibsmb2.csproj`.
  The successful receipt records source/config/tool/output hashes.
- Shared repairs were committed separately: linger (`8a9e923`), protocol lookup
  and errno (`3edabb2`), byte-preserving formatted allocation (`e0fc5c1`), explicit
  GNU lvalue rejection (`ae7321b`), and BCL host identity/PRNG (`c553125`).

Generated crypto/ABI checks, consumer builds, Samba execution, rooted NativeAOT,
postprocessor idempotence and clean-regeneration checks remain in progress. A
passing generation receipt alone does not establish the protocol acceptance gates.

## Managed consumer and implemented Linux qualification (historical scope)

`./libsmb2/scripts/test.sh` passed against the final default translation invoked
from `/tmp`, including the final shared compiler correction (`5d63fec`). Its
receipt at `artifacts/qualification/result.json` records generated and authored
source hashes, checks them for changes, and explicitly sets `plan_complete:false`.

- `ManagedConsumer.slnx` builds and the sample's help command runs.
- Seven host fixtures pass native/JIT/NativeAOT (21 executions).
- Forty-five crypto/layout/callback values match native in both raw and processed
  JIT/NativeAOT consumers, including a rejected CCM tag and high-bit status.
- Both complete generated assemblies publish and run with explicit whole-assembly
  NativeAOT roots; the import audit records generic OS runtime helpers separately.
- Semantic postprocessing is idempotent on a private copy of the final output.
- The native Samba matrix passes 11/11 with server signing mandatory.
- The sample passes 11/11 in each raw/processed × JIT/NativeAOT combination (44).
- The lifecycle harness passes the same 11-case matrix in all four combinations
  (44): Unicode CRUD, stat/fstat/statvfs, >1 MiB transfers with short-operation
  loops, cancellation before submission, empty/disposed operations, independent
  connections, pending-operation disposal, and a real TCP reset during a read.
- Four isolated checked-allocation/finalizer regressions pass for both raw and
  processed JIT facade binaries (8). These explicitly use private reflection
  and do not claim NativeAOT coverage.

The original read-error facade failed the real reset case; its preserved failing
log and build log are in `artifacts/lifecycle-baseline-red/`. The corrected facade
uses upstream asynchronous requests and retains operation state through callback
completion or context destruction.

Repository regressions after shared repairs: **2,325 unit tests, 583 functional
tests and 101 postprocessor tests pass**; the functional suite records 1,077
explicit optional external-oracle skips. The postprocessor CLI smoke also passes.
These builds use the NuGet LALR.CC dependency path. Fresh SQLite and picotls
campaigns passed. Scoped MsQuic regression also passed: 60 ABI records across
native/JIT/NativeAOT, complete raw/processed rooted JIT/AOT builds, and 167 existing
packet checks per variant/runtime. Exact scope and hashes are recorded in
`artifacts/compiler-baseline/cross-campaign.json`; this does not claim every
historical cross-campaign peer/performance test was rerun.

A subsequent isolated probe confirmed an upstream compound-request cancellation
leak: abandoning a `Stat` request retains its shared callback allocation. This is
not covered by the successful read/close cleanup cases above. Further investigation
is paused under the updated user scope; no correction has been applied to the
pinned source or generated product. Full failure-path acceptance remains open.

## Updated user scope

The coordinator and both subagents were restarted for read-only scope review.
[The explicit upstream test mapping](test-scope.md) governs fault injection.
The custom TCP-reset scenario and synthetic failed-close/pending-read cases were
removed from the default suite; ordinary API, cancellation, ownership, known-answer
and invalid-input checks remain. Earlier results for removed cases above are
historical and do not broaden current scope. No upstream correction was integrated.

The updated full `test.sh` suite **passes**. Sample and lifecycle matrices each
pass 11/11 in all four raw/processed × JIT/NativeAOT combinations. The two normal
finalizer cases pass against both raw and processed JIT binaries (4 checks);
the receipt lists the two excluded synthetic cases explicitly. Host, crypto/ABI,
rooted-build, native baseline and idempotence checks also pass.

Shell/Python syntax and help handling for the test and verification entry points
pass. `build.sh`, `test.sh --help` and `verify.sh --help` also work from `/tmp`.
The `verify.sh` orchestration itself was not rerun as a single command; its full
regeneration, current qualification and repository suites have separate recorded
executions. The qualification receipt still explicitly sets `plan_complete:false`.
