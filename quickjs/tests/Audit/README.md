# Delivery dependency audit

`QuickJs.Audit (ROLE ASSEMBLY_PATH)+` reads PE metadata with BCL
`System.Reflection.Metadata`/`PEReader`. It does not load the inspected assembly.
Roles are product, facade, consumer and harness. Each has an exact assembly name
and approved project references; other assembly references must belong to the
running .NET framework. P/Invoke modules are limited to the shared runtime's
`libc`, `kernel32.dll` and `psapi.dll`. Compiler-emitted native import binding
tables are rejected independently of ordinary P/Invoke metadata.

`scripts/audit.py` is a shared campaign suite adapter. Run it after consumer and
ABI suites in the same framework invocation, or use `verify`. Missing artifacts
fail the audit. The adapter verifies source ownership, original-path host links,
notices, absence of project-authored native/dynamic loaders, compiler per-symbol
native fallback bindings, actual assembly references/imports, dependency manifests,
and NativeAOT ELF dependencies. Generic unused loader helpers from the shared
runtime are distinguished from emitted required-symbol binding tables.

The consumer and rooted ABI harness each retain separate intermediate directories
per product form and execution mode. For NativeAOT, the audit queries MSBuild's
`TargetPath` with the recorded publisher properties to inspect the exact retained
input IL and dependency manifest, then runs `readelf` against the actual executed
native output. Rooted AOT publishes retain a hashed NativeAOT method map; the
audit requires nonempty machine-code entries for the engine evaluator/interpreter,
closures, regexp, Unicode, buffers, BigInt and exported upstream ownership/callback inlines, and reports the full
translated method count and code size. It never selects assemblies by searching for the latest filename.

Reports attach to the current framework receipt as `dependency_audit`; assembly
inspection, SDK property queries and ELF inspection preserve their raw logs.
Source or metadata checks alone do not establish JavaScript behavior.
