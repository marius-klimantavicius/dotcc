# Selected libc component verification hooks

The coordinator owns broad verifier registration and compiler build scheduling.
These independent entry points all return a list of passed checks. `root` is
the edgcpp root, `tests` is `root / 'backend/tests'`, and `out` is a unique
campaign output directory. The supplied `run(command)` must preserve stdout,
stderr and exact arguments and raise on nonzero status. No routine cleans
intermediates. The native oracle requires Linux x64 GCC for the currently
accepted binary80 profile; NativeAOT acceptance currently uses linux-x64.

| Module | Callable | Normal check count with NativeAOT |
| --- | --- | --- |
| libc_multibyte_verify | verify_libc_multibyte(root, tests, out, nativeaot, run, translate=True) | 9 |
| libc_wide_numeric_verify | verify_libc_wide_numeric(root, tests, out, nativeaot, run, translate=True, library_source=None) | 9 |
| libc_stdlib_algorithms_verify | verify_libc_stdlib_algorithms(root, tests, out, nativeaot, run, translate=True) | 9 |
| libc_scanf_verify | verify_libc_scanf(root, tests, out, nativeaot, run, translate=True, library_source=None) | 9 |
| libc_scanf_failure_verify | verify_libc_scanf_failure(root, tests, out, nativeaot, run, library_source=None) | 8 |
| libc_numeric_verify | verify_libc_numeric(root, tests, out, nativeaot, run, translate=True, library_source=None) | 9 |
| libc_musl_patch_verify | verify_musl_patches(root, out, components=...) | one per component |

With NativeAOT disabled the native-oracle matrices return five checks: one native plus
C/C++ × static/instance JIT. `translate=False` returns just the native check.
The allocation-failure matrix has no native aggregate check and returns four/eight generated checks.
The standalone CLIs accept `--output-dir`, `--nativeaot` and `--native-only`.
Both numeric verifiers also accept `--source-dir`. Narrow numeric expects the
strto directory; wide numeric expects its parent containing strto and
wide_numeric. Defaults select canonical runtime sources. Do not pass the same
output directory to multiple runs.

All verifier translations explicitly select `--libc dotcc` and pass their
snapshot's implementation TUs, so automatic `--libc musl` provider packaging
cannot hide a missing candidate dependency. A separate provider packaging
matrix should continue checking automatic source selection. The wide numeric
fixture additionally selects iswspace, wcschr and wcslen from the accepted wide
component. It verifies 12,277 rows against the narrow native oracle, plus the
separate full narrow 2,575-row regression when shared scanner inputs change.

Multibyte native verification compares original and adapted algorithms; its
managed host also checks each hidden state API across owners, threads and
ReleaseThread. Stdlib verification compares original and adapted algorithms,
then tests integer aggregates, function addresses, callback context/reentrancy,
owner separation and C++ exceptions crossing translated sort frames.

The patch verifier currently defaults to wide, multibyte, wide_numeric,
stdlib_algorithms and scanf. An immutable milestone snapshot can pass an explicit list
matching the components it actually contains. It checks original/adapted
hashes, replayed patches, and adapter hashes separately. Provider component
registration and broad verifier registration are independent of focused proof;
only report the modules actually invoked by a saved broad run.

## Wide FILE and position ABI

`libc_wide_files_translated_verify.verify_libc_wide_files(root, tests, out,
nativeaot, run)` supplies one native and eight C/C++ static/instance JIT/AOT
checks. `libc_fpos_abi_verify.verify_libc_fpos_abi(root, out)` supplies sixteen
strict malformed/obsolete position ABI rejection checks. Reuse existing
`verify_libc_files`, `verify_musl_printf` and `verify_libc_scanf` hooks for the
byte-stream regression; no duplicate suite wrapper is required.

## Wide scanf

`libc_wscanf_verify.verify_libc_wscanf(root, tests, out, nativeaot, run,
translate=True, library_source=None)` supplies native plus eight generated
configurations for 271 records and FILE/locale/varargs/address contracts.
`libc_wscanf_failure_verify.verify_libc_wscanf_failure(root, tests, out,
nativeaot, run, library_source=None)` supplies eight injected allocator-failure
checks. `libc_wscanf_abi_verify.verify_libc_wscanf_abi(root, tests, out)` supplies
eight strict private-import ABI negatives. The optional library source is a
parent directory containing `wscanf/` and `strto/`. The selected C component is
exactly `wscanf/vfwscanf.c` plus `wscanf/wscan_wrappers.c`; it depends on accepted
numeric parsers, wide classification and multibyte services.

## C compatibility headers

`libc_c_compat_verify.verify_libc_c_compat(root, tests, out, nativeaot, run)`
supplies one native portable-header and eight generated C/C++ static/instance
checks. Pinned header adaptation provenance is the `c_headers` source component;
it contributes no translation units to the musl source package.

## C11 atomics

`c11_implicit_atomic_verify.verify_c11_implicit_atomics(root, tests, out,
nativeaot, run)` supplies one original EDG-C oracle plus C static/instance
JIT/AOT: scalar/pointer/double/binary80/uint128/aggregate accesses, NaN and
negative-zero CAS, concurrent increments, adjacent-byte guards, independent
owners and zero steady-state integer allocation.

The separately gated public-header matrix is
`libc_stdatomic_verify.verify_libc_stdatomic(root, tests, out, nativeaot, run,
include=None)`; optional include supplies the candidate header directory.
It has two native oracles, four C runtime configurations and two C++17 include
compatibility checks. The public matrix passed after contained GNU statement-expression emission
was added; the canonical stdatomic header is now the default input.

Assertion facade: `libc_assert_verify.verify_libc_assert(root, tests, out, nativeaot, run)` returns four JIT checks or eight with NativeAOT. Each configuration checks enabled/disabled/reincluded assertions plus fatal subprocesses for ordinary and redirected wide-oriented owner stderr. Child subprocess outputs are retained; core dump generation is disabled for intentional aborts. Registered in the full suite using `suite_output("libc_assert")`.

C11 threads: `libc_c11_threads_verify.verify_libc_c11_threads(root, tests, out, nativeaot, run)` returns two native checks plus four/eight generated checks. It explicitly supplies `runtime/libc/threads/threads.c`. `libc_c11_threads_abi_verify.verify_libc_c11_threads_abi(root, out)` returns thirteen rejected ABI checks. `libc_c11_threads_runtime_verify.verify_libc_c11_threads_runtime(root, tests, out, nativeaot, run)` adds three/six standalone C11/shared-C++ normal and first-pass fatal checks. Runtime helpers must be copied together: `LibcxxThreads.cs` and `LibcC11Threads.cs`. Main/host exit completion is outside this worker matrix.

Process termination: `libc_process_exit_verify.verify_libc_process_exit(root, tests,
out, nativeaot, run)` supplies ten checks without AOT or eighteen with AOT;
`libc_process_exit_abi_verify.verify_libc_process_exit_abi(root, out)` supplies
nine malformed-ABI rejections. Both are registered with isolated suite outputs.
Each process case retains stdout/stderr/file bytes and command; managed hosts
explicitly exercise returning Run and separate thread/owner cleanup.

Wide scanf registration correction: the wide scanner and allocator-failure
verifiers detect automatic provider registration and omit duplicate explicit
translation units. Copied wscanf/strto candidate bytes must match the registered
provider; differing candidates fail before generated acceptance and require a
separate provider snapshot. The native-only path can still inspect candidates.
The two-file correction is independent of compiler/runtime changes; registration,
candidate rejection and unregistered paths have retained harness checks in
`libc-wscanf-harness-registration-20261003-030000/results.json`.

Initial process threads: `libc_initial_thread_verify.verify_libc_initial_thread(root,
tests, out, nativeaot, run)` returns six checks without AOT or ten with AOT;
the native C++ check records host interop behavior separately from the exact C
oracle and translated C++ cleanup policy. `libc_initial_thread_runtime_verify.verify_libc_initial_thread_runtime(root,
tests, out, nativeaot, run)` returns four/eight standalone subprocess checks.
The thread runtime now requires `LibcInitialThread.cs` along with the two existing
shared files. Existing C11/C++ standalone verifiers copy all three dependencies.

Wide printf: `libc_wprintf_verify.verify_libc_wprintf(root, tests, out, nativeaot,
run, translate=True, library_source=None)` returns native plus four/eight
generated checks. It detects component registration to avoid duplicate TUs and
rejects differing source candidates once registered. The private ABI hook
`libc_wprintf_abi_verify.verify_libc_wprintf_abi(root, tests, out)` returns eight
rejections; `libc_wprintf_runtime_verify.verify_libc_wprintf_runtime(root, tests,
out, nativeaot, run)` returns one/two standalone checks. Runtime dependency:
LibcFilesWideFormat with LibcFiles/VaList. Component source list is exactly
vfwprintf.c, wformat_wrappers.c and wcsnlen.c; private numeric dependency is the
accepted printf component. Add wprintf to the patch verifier component list.
