# Reproducing the Pinta campaign

The implementation is still being qualified. Check [VALIDATION.md](VALIDATION.md)
for actual pass/fail evidence before treating these recipes as a usable release.
The current target is Linux x64; the user explicitly deferred Windows qualification.
Run commands from the dotcc repository root; campaign scripts resolve their own
paths. Required tools are .NET 10 SDK, Python 3, Bash, a Linux x64 C compiler,
and the .NET NativeAOT prerequisites for later publication. Initial source/package
fetches require network access; existing pinned sources support offline verification.

Fetch the pinned source and verify all tracked input hashes:

```sh
python3 pinta/scripts/fetch.py
python3 pinta/scripts/fetch.py --offline
```

Run native builds/tests serially. Pristine and corrected stages remain separate;
the corrected stage applies the hash-checked patches in `config/patches.json`.
Nonzero exits retain their logs and receipts, including original native failures.

```sh
python3 pinta/scripts/native.py --stage pristine --profile release
python3 pinta/scripts/native.py --stage corrected --profile release --cases
python3 pinta/scripts/native.py --stage corrected --profile debug --cases
python3 pinta/scripts/native-boundaries.py --stage corrected
```

Emit the complete interpreter and run the semantic postprocessor, then compile
both raw and optimized forms. Never bypass a compiler failure by editing emitted C#.

```sh
pinta/scripts/translate.sh
pinta/scripts/build.sh
pinta/scripts/dependency-audit.sh
```

`--pristine` selects the untouched-source comparison for translation/build/audit;
`--profile debug` selects diagnostic code. The default corrected release outputs
are `generated/TranslatedPinta.Raw` and `generated/TranslatedPinta`. All selected
sources, including core diagnostics, remain in the library. No source-language
compiler, old wrapper, external debugger or native Pinta library supplies execution.

`src/ManagedApi` is the owning BCL facade; `samples/ManagedConsumer` is a separate
project that consumes it. Its project references the generated library normally.
The [host contract](host-contract.md) describes arena ownership, module handles,
UTF-16 lengths, copied output and callback lifetimes. Run its qualification matrix
after generation/build (execution outcomes are recorded separately):

```sh
pinta/scripts/test.sh --form all --mode all
pinta/scripts/translate.sh --profile debug
pinta/scripts/test.sh --profile debug --form all --mode all
```

Use `--mode jit` for a shorter local iteration; it does not establish NativeAOT
qualification. Release receipts are under `artifacts/managed/<rid>/`; diagnostic
receipts are under `artifacts/managed/debug/<rid>/`. Diagnostic is a Pinta source
profile (`PINTA_DEBUG=1`); both host builds use .NET Release configuration.

A separate project references `src/ManagedApi/ManagedApi.csproj`. For example,
with `receipt.pint` copied from the authored fixture directory:

```csharp
using Managed.Interpreters;

using var engine = new PintaEngine(new Dictionary<string, byte[]>
{
    ["receipt"] = File.ReadAllBytes("receipt.pint")
});
var module = engine.LoadModule("receipt");
engine.SetString(module, "customer", "Ada");
engine.SetInteger(module, "quantity", 3);
engine.SetString(module, "unitPrice", "12.50");
engine.Execute(module);
Console.Write(engine.GetOutputString());
```

The output is `Customer: Ada\nTotal: 37.5\n`. The engine executes once; create
another engine for another execution. The full checked-in consumer demonstrates
callback registration, explicit collection, imports and ownership checks.

The separate upstream assertion runner translates the selected core together with
the original test bodies and the portable harness, then compares isolated case
output with the saved native reference:

```sh
pinta/scripts/test-upstream.sh --form all --mode all
pinta/scripts/test-upstream.sh --profile debug --form all --mode all
```

This broader runner has its own compiler/runtime gates; consumer success does not
imply it passes. Case failures, native crashes, and absent reference logs remain
failures in its receipts under `artifacts/upstream-tests/`.

`python3 pinta/scripts/benchmark.py --iterations 1000` measures the fixed receipt
pipeline against native and all four release forms. Add `--profile debug` to
measure the diagnostic source profile separately. See [BENCHMARK.md](BENCHMARK.md)
for allocation domains, measurement limits and recorded results.

Keep builds and tests serial across the campaign. The final qualification must
also regenerate existing translations after shared compiler changes. Linux
cross-builds do not qualify Windows execution. See [CROSS-REGRESSIONS.md](CROSS-REGRESSIONS.md)
for the existing campaign command inventory.
