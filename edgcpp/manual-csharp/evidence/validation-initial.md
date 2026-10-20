# Initial integrated managed checkpoint — 2026-10-20

Supervisor executed, serially, from `edgcpp/manual-csharp/`:

1. `dotnet build EdgSharp.slnx -c Release --nologo`: exit 0; build succeeded,
   0 warnings, 0 errors, SDK 10.0.112.
2. `dotnet run --project tests/EdgSharp.Tests/EdgSharp.Tests.csproj -c Release
   --no-build`: exit 0; 25 named test groups, 25 passed, 0 failed, 0 skipped.
3. Five exact-byte `fixtures/m2-*.cpp` native comparisons through
   `tools/oracle.py`: all expectations matched; retained as `native-m2-*.json`.
4. `dotnet run --project tests/EdgSharp.Tests/EdgSharp.Tests.csproj -c Release
   --no-build -- --analyze fixtures/m2-positive.cpp`: exit 0. Output:

```text
success=True; unsupported=False; declarations=4
declaration=Count; type=long; range=131:19
declaration=widen; type=long (int); range=151:23
declaration=widen; type=long (int); range=175:133
declaration=entry; type=long (); range=309:61
```

This is an initial checkpoint, not final acceptance. Subsequent worker audit
identified missing explicit promotion nodes before some common-type conversions;
additional preprocessing diagnostic boundaries are being checked. Those fixes
will receive fresh integrated tests and independent review. NativeAOT execution
is deferred to that stable snapshot. No native compiler is required by the
default managed test execution above.
