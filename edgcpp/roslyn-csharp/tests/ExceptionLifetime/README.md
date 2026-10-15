This checker exercises only the existing genuine
[exception-cleanup.cpp](../Fixtures/T5/exception-cleanup.cpp) through generated
scalar fixture/lifetime observation functions. The bounded normal milestone
`f5c0f255a` is accepted; exceptional generated execution remains a separate gate.
No new C++ fixture or native application fallback is introduced here.

The coordinator-owned independent native oracle is
[tests/Lifetimes/native.c](../Lifetimes/native.c), compiled against the original
fixture. Its exact two-run JSON is
`/mnt/storage/roslynobj-csharp-20261015/t5/native-exception-cleanup-run-1/stdout.txt`.
It observes result 11, values `[3,0,8,0,0]` and all 14 lifetime events, with
live/overflow zero on both runs. The checker compares that complete observation,
without predicting a different cleanup order from the C++ source. EDG's retained
execution and current stock contracts remain authoritative.

[Program.cs](Program.cs) calls the generated fixture twice, checks event bounds,
live/overflow and prior-run live zero, and copies all five values and every
event. It then requires exact native JSON agreement. All JsonArray additions
explicitly select JsonNode for NativeAOT. The native-compatible observation is
the first output line. The second line records independent exception-ownership
diagnostics, which the native application does not expose.

After each invocation, exactly one payload owner must have been created,
committed, selected, marked caught and released; no live owner, pending release
or protocol rejection may remain. Every exceptional-cleanup assertion must
observe prior handler selection. The recorded sequence must satisfy selection
before first cleanup before release. Counts are process-wide diagnostics for
this serial checker, not a registry for resolving exception ownership. The
native event comparison still establishes the actual application cleanup;
the diagnostic assertions alone are not an event-order proof.

The new [ManagedCppException runtime](../../src/RoslynObj.CSharp.Runtime/ManagedCppException.cs)
owns a naturally aligned 4-byte payload explicitly. Its opaque tokens are
supplied from captured type/handler identities by the emitter. It performs no
name lookup, type-width-only match, source recovery, reader access, native C++
unwinding or application destruction through GC/finalizers. Preparation-finally
releases an uncommitted owner; a committed owner remains eligible only for its
selected handler. This positive C++ fixture does not activate constructor,
allocation or parameter-copy failure and does not establish those C++ variants.

The agreed emitter integration uses a function's false CLR filter to observe
the carrier without catching it, and the typed try's true filter to select its
destination before CLR cleanup. Encoded unwind sites call RequireSelectedHandler
before following captured region chains. CaughtObjectAddress is checked against
the selected handler token before the explicit retained parameter copy;
MarkCaught follows that copy, and ReleaseCaught runs at the actual epilogue or
eligible no-object semantic action. No ambient current-owner/TLS registry or
native provider resolves the owner. See
[T5-EH-CONTRACT.md](../../docs/T5-EH-CONTRACT.md).

Coordinator commands after attributable exceptional emission, using a generated
project that references the new immutable runtime snapshot:

```sh
dotnet build edgcpp/roslyn-csharp/tests/ExceptionLifetime/Checks.csproj \
  -c Release --disable-build-servers -p:UseSharedCompilation=false \
  -p:GeneratedProject="$GENERATED_PROJECT" -o "$JIT_OUTPUT"
dotnet "$JIT_OUTPUT/Checks.dll" "$NATIVE_OBSERVATIONS_JSON"

dotnet publish edgcpp/roslyn-csharp/tests/ExceptionLifetime/Checks.csproj \
  -c Release -r linux-x64 -p:PublishAot=true --disable-build-servers \
  -p:UseSharedCompilation=false -p:CppCompilerAndLinker=/usr/lib/llvm-18/bin/clang \
  -p:GeneratedProject="$GENERATED_PROJECT" -o "$AOT_OUTPUT"
"$AOT_OUTPUT/Checks" "$NATIVE_OBSERVATIONS_JSON"
```

Use fresh output directories and the frozen native observation. The coordinator
schedules every build and execution. This first slice excludes nested throw,
rethrow, multiple matching handlers, class/pointer exception payloads, partial
construction/arrays, termination, cross-thread owner transfer and exceptions
crossing a native call boundary.
