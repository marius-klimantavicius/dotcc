# Campaign validation

## Current async transport qualification — 2026-09-23

The default product now uses completion-driven C# sockets and fd callbacks.
The managed facade supports async connection, file operations and disposal;
synchronous facade methods wait on the same async implementation. No C source
was added or changed, and generated C# is not patched. The authored four-byte
`t_socket` stores a registry token separate from Libc descriptors.

Linux x64 qualification passed:

- Clean default translation from outside the repository processes all 53 pinned
  upstream units and promotes raw and processed outputs to their required paths.
  Host/build input hashes are included in `artifacts/translation/result.json`.
  The solution builds without warnings or errors, and semantic postprocessing is
  idempotent, including unchanged imported host files.
- **88/88 Samba cases**: sample and lifecycle suites, each in raw/processed ×
  JIT/NativeAOT. Coverage includes signing, encryption, ordinary rejection cases,
  Unicode CRUD, large transfers, independent connections, cancellation before
  submission and disposal with pending operations. The lifecycle harness observes
  zero I/O completions or service notifications during a settled idle interval.
  See `artifacts/async-transport/matrix.json` and the individual oracle receipts.
- Fresh native Samba controls pass **11/11**. All **45 crypto/ABI values** match
  native controls in both raw/processed JIT and NativeAOT consumers. The native
  control uses the separate integer-descriptor configuration.
- Socket loopback checks pass in raw/processed JIT and NativeAOT, exercising
  IPv4/IPv6, concurrent contexts, short reads/writes, EOF, prepared DNS, token
  ownership and close/drain. Each run transfers 1 MiB + 137 bytes in each
  direction per context. Recorded elapsed times are 21–108 ms, allocations
  65–70 MiB and thread-pool sizes 9–11 for the complete harness. Dividing the
  roughly 6 MiB transferred by total elapsed time gives 56–286 MiB/s; this
  includes setup and is not an isolated transport benchmark. Per-socket
  send/receive buffers peak at 256 KiB each;
  all registered contexts, sockets and buffered bytes drain to zero.
  Logs are under `artifacts/async-host/`.
- Whole-assembly-rooted NativeAOT publication/execution passes for both products.
  The audit evaluates imported authored C# and build inputs as well as generated
  files; it finds no unexpected native dependencies. Generic OS runtime imports
  and dormant loader helpers remain explicitly inventoried in
  `artifacts/product-audit/result.json`.
- Both checked-heap finalizer cases pass for raw and processed JIT facade
  binaries (**4 executions**). Compiler unit tests pass **2,441/2,441**;
  functional tests pass **619**, with **1,081 existing skips** and no failures;
  postprocessor tests pass **101/101**.

Unchanged upstream synchronous C tests use an explicit **legacy profile**, with
separate generated directories and receipts; they do not qualify the async host.
Ten upstream programs build in raw/processed JIT/NativeAOT. The suite's unchanged
rerun reports **35 passed, 60 skipped, 1 known native baseline failure and 4
blocked**. Its initial run also had one processed-JIT connection startup timeout;
the failing evidence is preserved under
`artifacts/upstream-tests-first-async-qualification/`. A coarse upstream
zero-timeout comparison is a suspected cause, not an established diagnosis. No
source or test expectations were changed to make the rerun pass.

The current aggregate receipt is `artifacts/async-transport/qualification.json`;
the older `artifacts/qualification/result.json` describes a historical campaign.
Windows execution remains unverified. Kerberos and DFS remain on hold. Direct
translated synchronous/manual-poll APIs require the separate legacy profile;
the default product requires fd callbacks. This completes the Linux async
transport milestone, not every remaining gate in the overall libsmb2 plan.

The sections below preserve earlier campaign history; their poll-based behavior
and previously injected fault checks are not current async acceptance evidence.

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

## Translated upstream test programs

The new [upstream campaign](upstream-tests.md) builds ten original C test/utility
programs for native, raw/processed JIT, and raw/processed NativeAOT (50 binaries).
The live Samba matrix passes six original shell equivalents plus the original AES
vector program in each variant: **35 case passes**, with 295 process invocations.
It preserves the full upstream server-side-copy sequence through 20 MiB.

The pinned standalone NTLM vector fails its native baseline because it omits a
server identity required by the current private decoder. Its original assertions
remain unchanged: one native baseline failure and four managed blocked cases are
reported separately. Twelve remaining shell cases per variant have explicit
prerequisite skips; this campaign's `complete` field remains false.

The shared POSIX header fixes required by the original programs pass the full
2,327-test compiler unit suite, and all 53 library units regenerate successfully.
The complete `upstream-tests.sh` entrypoint also passes from `/tmp`, including
fresh native/translated builds, all five execution variants, and server cleanup.
The default `test.sh` now includes `upstream-tests.sh`; the combined expanded
qualification command has not been rerun in this milestone. Its earlier authored
suite results above and the new upstream receipt are separate execution evidence.
