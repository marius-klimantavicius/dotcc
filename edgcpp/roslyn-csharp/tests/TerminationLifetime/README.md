# Isolated noexcept termination child

This child invokes the generated
[noexcept-termination.cpp](../Fixtures/T5/noexcept-termination.cpp) using an
existing exact 272-byte shared file. Args are `Checks MAP_PATH MODE`, with mode
0 for normal completion and 1 for the sole integer throw. The parent supplies
a fresh zeroed file for each process and reads it only after that process exits.
No custom terminate/signal handler, native observer or native application body
supplies generated execution.

The child maps the file read/write and pins its view pointer for the complete
fixture call with AcquirePointer/ReleasePointer. Slots are count 0, overflow 1,
live objects 2, source result marker 3 and 64 events at 4 through 67. Mode 0
requires return 1, marker 1, no overflow and zero final live objects, then prints
minimal JSON and exits 0. The parent checks the complete mapped event sequence
against the independent native normal run. Explicit JsonObject construction is
NativeAOT-safe; there is no reflective serialization.

Mode 1 must not return. The approved runtime first selects a distinct noexcept
destination, executes retained leaf/boundary cleanup under the selected stock
TRUE unwind policy, then calls Environment.FailFast with exact public marker:

```text
RoslynObj.CSharp.ManagedCppException:default-noexcept-termination:unwind-true:failfast-v1
```

The parent must prove actual SIGABRT (`-6` in its subprocess result), the exact
marker and complete selected-policy map contents in JIT and NativeAOT. It also
disables child core files. Source outer catch/destructor must remain unexecuted.
The process still owns its payload at termination: no caught transition,
handler-selected counter or synthetic release is invented. A normal return,
CLR unhandled-exception exit, wrong signal or wrong marker is not this proof.

The native observation protocol is recorded separately in
`t5/noexcept-native-observations-1.json` under the artifact root. The GCC normal
trace agrees with ordinary cleanup; its raising prefix of five events differs
from the seven-event selected EDG TRUE policy. Preserve that native result;
do not select unwind FALSE or change retained cleanup to imitate it. This child
does not assert that native and generated raising traces are identical.

Coordinator build/publish commands use `-p:GeneratedProject=<frozen project>`
and fresh outputs, with the existing clang-18 NativeAOT configuration. The child
then runs as `dotnet Checks.dll MAP_PATH 0|1` or `Checks MAP_PATH 0|1`; the parent
owns each subprocess and all acceptance checks. Production CLI, JIT and
NativeAOT now pass; the [receipt](../../acceptance/lifetimes/termination-report.json)
records all 38 expected outcomes and exact map/signal/marker evidence. See
[the contract](../../docs/T5-TERMINATION.md).
