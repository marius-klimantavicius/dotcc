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
| libc_numeric_verify | verify_libc_numeric(root, tests, out, nativeaot, run, translate=True, library_source=None) | 9 |
| libc_musl_patch_verify | verify_musl_patches(root, out, components=...) | one per component |

With NativeAOT disabled the first four return five checks: one native plus
C/C++ × static/instance JIT. `translate=False` returns just the native check.
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

The patch verifier currently defaults to wide, multibyte, wide_numeric and
stdlib_algorithms. An immutable milestone snapshot can pass an explicit list
matching the components it actually contains. It checks original/adapted
hashes, replayed patches, and adapter hashes separately. Provider component
registration and broad verifier registration are independent of focused proof;
only report the modules actually invoked by a saved broad run.
