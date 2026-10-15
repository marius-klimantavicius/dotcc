# Bounded normal C++ lifetime observations

The checker invokes only generated scalar `fixture/lifetime_*` functions. It
calls the genuine normal-lifetime fixture twice, copies every retained event and
all nine scalar observations, checks live/overflow/prior-run leak invariants, and
compares the complete JSON with the separate native oracle's stdout. It adds no
native application dependency or predicted constructor/destructor event order.
The native oracle is coordinator-owned `tests/Lifetimes/native.c` compiled against
the original fixture in a separate process. Its run does not define EDG's order.

Actual selected receiver/result/copy roles, explicit calls and temporary/storage
occurrences must be established from the genuine public-object trace before
emitter admission. This checker does not infer lexical cleanup or replay metadata
destructor lists. Exception execution and native C++ unwinding are separate.

Coordinator commands after genuine normal-path emission:

```sh
dotnet build edgcpp/roslyn-csharp/tests/NormalLifetime/Checks.csproj \
  -c Release --disable-build-servers -p:UseSharedCompilation=false \
  -p:GeneratedProject="$GENERATED_PROJECT" -o "$JIT_OUTPUT"
dotnet "$JIT_OUTPUT/Checks.dll" "$NATIVE_OBSERVATIONS_JSON"

dotnet publish edgcpp/roslyn-csharp/tests/NormalLifetime/Checks.csproj \
  -c Release -r linux-x64 -p:PublishAot=true --disable-build-servers \
  -p:UseSharedCompilation=false -p:CppCompilerAndLinker=/usr/lib/llvm-18/bin/clang \
  -p:GeneratedProject="$GENERATED_PROJECT" -o "$AOT_OUTPUT"
"$AOT_OUTPUT/Checks" "$NATIVE_OBSERVATIONS_JSON"
```

Use fresh output directories and the exact immutable native observation file.
All JsonArray additions select the JsonNode overload. No generated execution or
normal-lifetime acceptance is established merely by preparing this harness.
