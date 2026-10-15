# Bounded integer rethrow checker

This checker calls the generated
[integer-rethrow.cpp](../Fixtures/T5/integer-rethrow.cpp) fixture twice and compares
its complete five-value/event JSON with a separate native oracle. Generated
execution now passes production CLI and JIT/NativeAOT; the
[final receipt](../../acceptance/lifetimes/rethrow-report.json) records all 31
expected phase outcomes and the verified source/input freeze. The
oracle is `t5/native-integer-rethrow-run-1/stdout.txt` under the artifact root;
it reports result 7, values `[7,99,7,0,0]`, all nine events, live/overflow zero,
twice. No native application body supplies generated execution.

The inner catch changes its local copied parameter to 99, then rethrows the
original integer payload. The outer catch observes 7. Event comparison also
checks destruction before the inner handler, during rethrow and after the
outer handler. Prior-live slot 4 must remain zero on each invocation.

Runtime observations are separate from native application observations. Each
fixture invocation must allocate, commit and free one primary payload, select
and enter two handlers, create one rethrow propagation entry and retire two
handling references. Live owners/references, pending releases and protocol
rejections must finish at zero. Opaque diagnostic primary-owner identities at
creation, rethrow and final free must match and advance between invocations;
they never resolve ownership. The old GetDiagnostics positional API is unchanged.

Every encoded cleanup assertion must observe prior destination selection.
The latest sequence fields describe only the last propagation, not independent
histories of both handler entries; cleanup assertions are not destructor counts.
The first stdout line preserves native-compatible JSON. The second reports
ownership counters and identities. JsonArray additions explicitly use JsonNode
for NativeAOT. This serial checker makes no concurrent diagnostic-snapshot claim.

Coordinator commands after attributable emission, with fresh output directories:

```sh
dotnet build edgcpp/roslyn-csharp/tests/RethrowLifetime/Checks.csproj \
  -c Release --disable-build-servers -p:UseSharedCompilation=false \
  -p:GeneratedProject="$GENERATED_PROJECT" -o "$JIT_OUTPUT"
dotnet "$JIT_OUTPUT/Checks.dll" "$NATIVE_OBSERVATIONS_JSON"

dotnet publish edgcpp/roslyn-csharp/tests/RethrowLifetime/Checks.csproj \
  -c Release -r linux-x64 -p:PublishAot=true --disable-build-servers \
  -p:UseSharedCompilation=false -p:CppCompilerAndLinker=/usr/lib/llvm-18/bin/clang \
  -p:GeneratedProject="$GENERATED_PROJECT" -o "$AOT_OUTPUT"
"$AOT_OUTPUT/Checks" "$NATIVE_OBSERVATIONS_JSON"
```

The generated project references the frozen updated runtime, not the reader or
native C++ application. The finite adapter permits one rethrow from the primary
caught entry. Repeated/rethrow-of-rethrow, new exceptions from a handler, class
or pointer payloads, termination, cross-thread ownership and native unwinding
remain outside this evidence. See [the retained contract](../../docs/T5-RETHROW.md).
