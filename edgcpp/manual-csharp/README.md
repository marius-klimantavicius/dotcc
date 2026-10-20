# EdgSharp handwritten frontend

Experimental in-process C# port of selected EDG parsing and semantic algorithms.
The full roadmap is in [PLAN.md](PLAN.md); exact current limits and acceptance
state are in [SUPPORT.md](SUPPORT.md) and [PROGRESS.md](PROGRESS.md).

```csharp
using EdgSharp;

using var session = new CompilationSession(new AnalysisOptions
{
    Language = LanguageProfile.Cpp20,
    Target = TargetInfo.Lp64
});
AnalysisResult result = session.Analyze(
    "using I = int; int twice(I x) { return x + x; }", "example.cpp");
if (result.IsSuccess)
{
    var function = (FunctionDeclaration)result.Declarations[1];
    Console.WriteLine(function.Type.ReturnType); // int
    Console.WriteLine(function.Body!.Statements.Count); // 1
}
```

Results retain source, diagnostics, scopes, declarations and typed structured
bodies. Symbols identify entities; declarations retain individual source sites.
Queries inspect existing facts and do not trigger instantiation. Graph queries
remain valid after session disposal. Independent sessions own independent
mutable frontend state. Invalid and unsupported results expose diagnosed partial
facts; callers must check `IsSuccess` before treating the analysis as accepted.

From this directory, the standalone managed checks are:

```sh
dotnet build EdgSharp.slnx -c Release
dotnet run --project tests/EdgSharp.Tests/EdgSharp.Tests.csproj -c Release --no-build
```

The reviewed Linux x64 NativeAOT smoke uses the same 25 named test groups:

```sh
dotnet publish tests/EdgSharp.Tests/EdgSharp.Tests.csproj -c Release \
  -r linux-x64 -p:PublishAot=true -o tests/EdgSharp.Tests/bin/aot-smoke
tests/EdgSharp.Tests/bin/aot-smoke/EdgSharp.Tests
```

The test project is a deterministic executable harness. Its native oracle is a
separately invoked tool; ordinary library use and the managed suite do not run
EDG or another compiler. For an explicit native baseline:

```sh
python3 tools/oracle.py tools/oracle-inputs/profile.cpp --expect accept \
  --receipt evidence/native-profile.json
```

Source correspondence and omissions are documented in
[docs/CORRESPONDENCE.md](docs/CORRESPONDENCE.md). EDG-derived source retains
`Apache-2.0 WITH LLVM-exception` attribution; see [docs/NOTICE.md](docs/NOTICE.md).
