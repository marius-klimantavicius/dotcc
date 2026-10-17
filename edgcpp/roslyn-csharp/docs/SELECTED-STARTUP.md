# Explicit selected startup and teardown

The [receipt](../acceptance/duckdb/selected-startup-report.json) records 20 passing
expected outcomes: 17 successes and three intended unsupported rejections.
Native, JIT and NativeAOT agree on actual captured global constructors, both
advertised fixture calls and reverse registered destruction. This finite fixture
proof does not execute DuckDB's real startup or complete the Vector accessor.
Receipt SHA-256: `185eb658d45891002163cdd71438477dfce23de22ab1912c3b545cc1a9085bc6`.

`roots.managedStartup` selects exact captured definitions with the explicit
`edg7-itanium-explicit-startup-atexit-v1` policy and ordered input hashes matching
the manifest-order target contract. No cross-TU order is inferred from C++.
All actual supported backend startup candidates must be covered. Zero/constant
storage exists first; a library host calls `InitializeSelectedProgram()` and
`ShutdownSelectedProgram()` explicitly. Application startup is never placed in
CLR type initialization or ProcessExit. Advertised public roots guard readiness;
actual internal calls use private bodies, including a shared advertised `note`
called during both startup and destruction.

The genuine input constructs two namespace objects. A native-only earlier atexit
observer reports after their actual C++ destructors; it never calls D1 manually.
Native/JIT/AOT observe startup events `[101,102]`, fixture results 13 and 14, and
final events `[101,102,501,502,302,301]`, live zero and two destructions. Generated
physical item size/alignment 4/4, field offset zero and stride four agree. Repeated
Initialize/Shutdown cause no application effects. Legitimate calls before startup
and after shutdown reject with unchanged application storage. Post-shutdown
inspection reads independent scalar/event storage, never ended object members.

Only actual validated `__cxa_atexit` occurrences register the selected managed D1,
object and module addresses. Successful append precedes return zero; nonzero
failure publishes nothing and its captured consumer controls subsequent behavior.
No failed registration causes synthetic destruction. Explicit shutdown pops each
entry before D1 and drains reverse successful-registration order once. One stable
generated 8/8 `__dso_handle` slot supports only the exact address-only registration
uses; native contents, escapes, conflicting definitions, general __cxa_finalize
and module unload are unsupported. No UCO or native application body is supplied.

The initial lifecycle is instance-owned on one managed thread. Concurrent/reentrant
transitions and registration during draining are unsupported. Source bodies need
a separate closed supported-body proof; nullable noexcept is never promoted or
added to cleanup-marker permission. Unexpected managed faults latch failure and
propagate directly without inventing C++ recovery, cleanup or termination.

Six focused checks preserve four prior source/ABI outputs and two unsupported
boundaries; the closed 77-phase composition campaign is not repeated. The first
compiler had a nullable guard error. A later clean compiler used wrongly positioned
runner arguments (oversized timeout and wrong outer expected exit); that attempt
is preserved with its real limits. A separate correctly configured 600-second/
8-GiB build succeeds. Reports do not relabel the faulty harness attempt.

With its explicit real startup root, core293 now advances beyond ambient rejection
but fails selected EH dependency admission, without publication. Descriptor
presence alone does not prove executing cleanup. Normal-EH dependency accounting
is separate unverified follow-on work, excluded from the proven source2 baseline.
Actual FileOpenFlags C1/D1/Slot execution, startup exceptions, TLS/priorities and
full real guard/string/JSON/library closure remain open. SQLite and earlier C++
proofs retain their original frozen source/runtime attribution.
