# Translated upstream tests

`tests/UpstreamRunner` is a C# orchestration executable for original upstream C
test programs. It replaces the shell orchestration while preserving the C test
bodies, arguments, and pass/fail observations. Each C program runs in a fresh
process against the translated library, or against native libsmb2 for comparison.
The runner itself uses only the .NET BCL.

The pinned `tests/` directory contains 34 files: 13 C sources, 18 shell cases,
`functions.sh`, `Makefile.am`, and `README`. Nine C programs are built by default
upstream; a DCE/RPC program is optional, two sources contain standalone vectors,
and `ld_sockerr.c` is a native interposition helper. The shell cases also depend
on `utils/smb2-cp.c`, which must be translated separately.

## Invocation and manifest

The campaign entrypoint builds the original programs, creates disposable Samba
directories, runs native plus raw/processed JIT and NativeAOT, and removes the
server and temporary credentials:

```sh
./libsmb2/scripts/translate.sh --profile legacy
./libsmb2/scripts/upstream-tests.sh
```

These unchanged C programs retain manual polling and synchronous APIs, so they
explicitly link the isolated legacy transport objects. Their receipt must be
the framework receipt referenced by `artifacts/campaign/current-legacy.json`
with profile `legacy`; product async
objects cannot silently substitute for them. This baseline establishes protocol
regressions separately from the callback-only product's managed async tests.

Use `--jit-only` for a development run, or `--no-build` to execute the existing
program manifest. Reuse checks the source/configuration, library translation,
build script, executable and wrapper hashes. Regenerate the legacy library first
if its successful translation receipt or object fragments are absent.
The top-level receipt is `artifacts/upstream-tests/result.json`; detailed case
results are under `artifacts/upstream-tests/cases/result.json`.
The default `scripts/test.sh` qualification also executes this campaign.

The lower-level C# runner also runs from any directory using absolute paths:

```sh
dotnet run --project /path/to/dotcc/libsmb2/tests/UpstreamRunner -c Release -- --manifest /path/to/manifest.json
```

The JSON contract is:

```json
{
  "sourceRoot": "/absolute/path/to/pinned/libsmb2-source",
  "artifactRoot": "/absolute/path/to/run-artifacts",
  "testUrl": "smb://WORKGROUP;testuser@127.0.0.1:445/share/isolated-test-directory",
  "timeoutSeconds": 120,
  "environment": { "NTLM_USER_FILE": "/absolute/path/to/temporary/credentials" },
  "variants": [
    {
      "name": "native",
      "programs": {
        "prog_mkdir": ["/absolute/path/to/prog_mkdir"],
        "prog_rmdir": ["/absolute/path/to/prog_rmdir"],
        "smb2-cp": ["/absolute/path/to/smb2-cp"],
        "prog_cat": ["/absolute/path/to/prog_cat"],
        "prog_cat_cancel": ["/absolute/path/to/prog_cat_cancel"],
        "prog_setsd": ["/absolute/path/to/prog_setsd"],
        "prog_ssc": ["/absolute/path/to/prog_ssc"],
        "aes128ccm-test": ["/absolute/path/to/aes128ccm-test"],
        "ntlmssp_generate_blob": ["/absolute/path/to/ntlmssp_generate_blob"]
      }
    },
    {
      "name": "raw-jit",
      "testUrl": "smb://WORKGROUP;testuser@127.0.0.1:445/share/raw-jit",
      "environment": {},
      "programs": {
        "prog_mkdir": ["dotnet", "/absolute/path/to/raw/prog_mkdir.dll"]
      }
    }
  ]
}
```

Program command arrays contain the executable followed by fixed arguments; the
runner appends the original test arguments without shell interpretation. Supported
variant names conventionally are `native`, `raw-jit`, `processed-jit`, `raw-aot`,
and `processed-aot`. Each variant may override `testUrl` and environment variables.
The caller must create empty, disposable share directories before invocation and
remove them afterward. Remote fixture names intentionally follow upstream; the
runner does not run safely against unrelated existing data. Credentials are passed
through the inherited/manifest environment and are not copied into receipts.

The builder may additionally supply a top-level `baselineBlockedPrograms` object,
keyed by executable program name. Each value records `reason`, `nativeExitCode`,
`nativeStdoutSha256`, `nativeStderrSha256`, and `logs` from an actual failed native
baseline. The runner preserves that object as `baselineEvidence`, records the
native case as `baseline-failed`, and records corresponding managed cases as
`blocked` without executing them. This never converts the failing test to a pass.

## Implemented cases and observations

| Upstream case | Managed orchestration |
| --- | --- |
| `test_0200_mkdir.sh` | Create and remove `testdir`; require successful exits. |
| `test_0210_cp_basic.sh` | Upload/download `HappyPenguins!` plus newline; compare bytes; require nonexistent source copy to fail. |
| `test_0300_cat_basic.sh` | Upload original `prog_cat.c`; run cat and additionally verify its stdout begins with the complete fixture. Record trailing diagnostics separately. |
| `test_0310_cancel_pdu.sh` | Upload the same fixture; run original OPEN-PDU cancellation test; additionally check cancellation was reached, no forbidden callback ran, and the complete fixture was read. |
| `test_0500_setsd_basic.sh` | Upload original `prog_setsd.c`; execute original security-descriptor test. |
| `test_0600_ssc_basic.sh` | Preserve all upstream copy modes, lengths from zero through 20 MiB, chunk limits, invalid control code, server rejection, self-overlap acceptance/rejection, parser diagnostics, and subsequent copy. |
| `aes128ccm-test.c` | Execute both upstream vectors and decryption assertions; additionally compare printed expected/encrypted bytes. |
| `ntlmssp_generate_blob.c` | Execute original deterministic blob assertion. |

The pinned standalone NTLM source currently fails its unchanged native baseline:
it creates a context without setting the server, while the current challenge
decoder calls `strlen(smb2->server)`. The observed native process terminates with
SIGSEGV. The builder's prerequisite-header wrapper permits compiling this old
source but does not change its body or initialize missing context fields. The
baseline evidence therefore blocks executing this vector in managed variants;
its assertion is not weakened and the source is not repaired as part of this
translation qualification. Consult the receipt for its recorded exit and logs.

When a passing `native` vector variant is present, each passing managed vector
must also produce exactly identical binary stdout and stderr. Cat checks are
explicit additions: the upstream mains return success for some failure paths,
so exit status alone does not demonstrate a successful read. The runner records
these adaptations rather than claiming byte-for-byte shell execution.
The native cat program also prints a socket-close diagnostic after the file
payload on this Samba fixture; that suffix is recorded rather than interpreted
as additional file bytes or a translation difference.

The runner inventories all 18 shell cases for every variant. Missing executables
receive explicit skips. Remaining skips include allocator interposition needed by
`prog_ls`, native Valgrind/trace-based cases, ten-process credit stress, the special
scrambla unanswered-CREATE server, and optional untranslated libdcerpc. Native
instrumentation is not represented as equivalent managed evidence. Custom fault
injection is excluded; the cancellation case is explicitly present upstream.

## Receipts

`artifactRoot/result.json` includes every case status, skip/failure reason,
checks, adaptations, source hashes, and per-invocation exit code, timeout state,
duration, binary output paths, and output hashes. Output files are stored beneath
the variant/case directories. `statusCounts` separates `passed`, `failed`,
`skipped`, `baseline-failed`, and `blocked` cases. The overall `passed` field means
at least one case passed and no supported executed case failed; documented native
baseline failures and their blocked managed counterparts remain separate from
translation failures. `complete` additionally requires every case to pass.
Exit status is 0 for such a passing partial/full run, 1 for case failure or an
entirely skipped run, and 2 for invalid configuration/runner failure.

Runner implementation alone is not execution evidence. Consult its generated
receipt for the exact variants and original programs that were actually run.

## Validated Linux x64 baseline

The first complete execution matrix passed **35 cases**: six original shell
equivalents plus the standalone AES program, each run natively and with
raw/processed JIT and NativeAOT. Each variant executed 59 original-program
invocations. The server-side-copy sequence includes every upstream length through
20 MiB and its original server rejection and self-overlap checks.

Ten original test/utility programs build in all five variants (50 binaries).
The standalone NTLM vector's native baseline failure and four blocked managed
executions are recorded separately. The remaining 12 shell cases are skipped
per variant (60 skip receipts), with explicit dependencies; `complete` is false.
Building `prog_open_timeout` does not establish its special-server execution.

Shared POSIX metadata typedef/include-order and standard-descriptor header gaps
found by these programs were fixed in dotcc. All 53 library units regenerated,
and the full compiler unit suite passed 2,327 tests. No upstream library or test
source bytes were edited.
The full `upstream-tests.sh` build/server/runner/cleanup entrypoint also passes
when invoked from `/tmp`; qualification is not limited to separately run stages.
