# Runtime framework Path qualification

B018 was first observed when the authored HostProcessPolicy fixture declared a
C function named `Path`. The generated owner's imported member shadowed bare
`System.IO.Path` references inside the embedded runtime, producing CS0119.
The campaign fixture was renamed `SelectExecutable` to continue qualification;
that fixture remains unchanged by this generic repair.

The central runtime embedding code in `Compiler.Resources.cs` deliberately
strips file-scope namespace/using directives and splices the shared Libc source.
It does not semantically resolve bare framework identifiers before doing so.
The generated shell imports the C owner's static members, which makes a valid
C function name visible inside sibling or nested runtime helpers. Adding a
file-scope alias would not reliably fix the embedded source because aliases
are stripped by that existing mechanism.

The repair qualifies the five affected call sites directly in the runtime's
single source of truth with `global::System.IO.Path`: temporary filename and
temporary file creation in `FileLib.cs`, native-library search path composition
in `NativeImports.cs`, and directory entry/canonical path construction in
`PosixFsLib.cs`. This changes name resolution only. It does not alter path,
filesystem, native loading, error, or allocation behavior, and does not imply
that these generic OS-facing APIs are the private Blink filesystem boundary.

The separate `runtime-path-shadow` C fixture retains a real `Path(int)`
definition while testing tmpfile write/seek/read/close, realpath allocation/free,
and opendir/readdir/closedir. Native cc passes. The pre-fix generated-library
failure is retained at `artifacts/runtime-path-shadow/repro-sb9j947z/`, where
CS0119 identifies the fixture's `DotCcLib.Path(int)` in runtime expressions.

The managed functional test covers direct and object-linked emission with both
flat and nested layouts. It invokes the C probe and `Path` function, exercises
`tmpnam`, and checks the native-loader path-composition branch using a randomly
named nonexistent library without invoking foreign code. Existing Libc file,
POSIX filesystem, and import-mode unit tests provide regression coverage for
the affected runtime files. Full repository validation remains centralized.

Focused qualification passed 36 related unit tests and 5 functional tests;
two unavailable external-platform oracle rows were skipped. Final logs are
`artifacts/runtime-path-shadow/focused-unit.log`,
`focused-functional-final.log`, and `native.log`.

The first functional fixture run also exposed the existing generic runtime's
`void*` return signatures for opendir/readdir versus their typed C declarations
(CS0266). The authored Path regression uses explicit, native-valid C pointer
casts to isolate framework name lookup; that separate typed-return boundary
was not changed or claimed repaired. The initial diagnostics remain in
`focused-functional.log`. Compiler/test assemblies were unchanged for the
final rerun; the revised fixture was copied from its tracked source to the
test fixture output, as the normal build copy step does.
