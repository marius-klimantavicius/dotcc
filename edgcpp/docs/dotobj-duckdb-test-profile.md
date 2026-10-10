The DuckDB project manifest now prepares an explicit portable test profile when
`--include-tests` is selected. Existing manifests can be adapted reproducibly with
`object-linker/duckdb_test_profile.py --manifest INPUT --output-dir OUTPUT`.
All test units, assertions, ordinary exception handlers and language standards
are retained. The report records original and staged file hashes and keeps the
original source tree unchanged.

Catch's supported `CATCH_CONFIG_NO_POSIX_SIGNALS` option disables OS crash-signal
interception. Its GCC-only `__builtin_constant_p` diagnostic probe is disabled
for the private EDG target because an unevaluated lambda is invalid in C++17;
the actual assertion expression is still evaluated exactly where Catch placed
it. This does not disable tests or C++ exception handling.

The two SQLite-runner translation units receive the previously reviewed ANSI
terminal adaptation from `duckdb/edg/portable.py`. The preparation calls only
that adaptation, not the broader preparation pipeline or its unrelated pending
PCG experiment. It does not advertise a host OS or CPU. Redirected runner streams
remain the verified terminal profile; interactive terminal capabilities are a
separate runtime task.

The shared portable headers now declare `fork`, `kill`, `SIGKILL` and `bzero`.
`SIGKILL` is a portable profile value that a provider must translate; it is not
a promise of a host ABI match. The POSIX signal header extends the synchronous C
signal header, leaving plain C users' header independent of POSIX types. These
are import declarations, not runtime implementations. Process tests require
real fork/process/signal support before executable acceptance; no successful
fork, signal delivery or translated test execution is fabricated here.

Seven formerly rejected test units export and verify, and jointly generate
5,662 definitions with two RTTI providers in the initial native138/managed161
probe. The final canonical header/profile preparation is independently exported
with native139. The remaining `unittest.cpp` now passes parsing and reaches a
real exporter gate for parent ownership of static destruction. Receipts are
retained under `dotobj-test-profile-*` in the campaign storage. Full-project
linking, compilation and execution remain separate milestones.
