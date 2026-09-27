# Managed consumer

`ManagedConsumer.csproj` references the owning API and selects the generated
product using `QuickJsProject` (processed by default).

The workload takes `{"items":[2,3,5]}`, calls managed `hostDouble` exactly three
times, imports an in-memory sum module, and drains Promise jobs to produce
exactly `{"values":[4,6,10],"sum":20}`. It checks cleanup and repeats using a
fresh runtime. The ordinary trimmed NativeAOT consumer is distinct from the
whole-library-rooted ABI harness.

```bash
dotnet run --project quickjs/samples/ManagedConsumer -c Release
```

This authored sample requires a successfully translated product. Its existence
does not establish execution; see [validation](../../docs/validation.md).
