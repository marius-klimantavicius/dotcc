# Managed Zydis

DotCC translation of the complete Zydis 4.1.1 library: decoding, encoding,
Intel/AT&T/MASM formatting, tokenization, disassembly, instruction/register
metadata, segment and address utilities. The upstream algorithms and generated
instruction tables are translated from C. The product has no native Zydis backend.

Campaign acceptance is in progress. See [the plan](docs/PLAN.md) and
[current evidence](docs/IMPLEMENTATION.md) for passed and pending gates.

Requires .NET 10 SDK, Python 3.11+ and Bash. Initial execution qualification is
Linux x64. Native differential tests additionally require a C compiler; NativeAOT
requires its normal native toolchain. Source acquisition and NuGet restore have
separate controls. Inputs, revisions and licenses are recorded under `config`.

```bash
bash zydis/scripts/fetch.sh
bash zydis/scripts/translate.sh --fetch never
dotnet build zydis/ManagedConsumer.slnx -c Release -m:1
dotnet run --project zydis/samples/ManagedConsumer -c Release
bash zydis/scripts/verify.sh --fetch never --form all --mode all --rid linux-x64
```

`translate.sh` builds raw and processed libraries and a separate qualification
companion through the shared transactional publisher. Fixed test workloads and
upstream command-line programs are confined to the test companion. Generated
output, acquired sources and execution evidence are ignored; scripts reproduce
them. `build.sh` rebuilds existing output; it does not translate sources.

Applications reference `generated/TranslatedZydis/TranslatedZydis.csproj` normally.
The sample uses the `TranslatedProject` MSBuild property to select the raw library
when testing it. Authored API sources stay linked from `src/Api`, so rebuilding
picks up edits without regeneration. Both upstream MIT license texts accompany
the product. The release tag is 4.1.1; its original API version constant reports
4.1.0.0 and is preserved.

```csharp
using Managed.Zydis;
using Core = Managed.Zydis.Zydis;

var instruction = InstructionCodec.Decode(new byte[] { 0x48, 0x83, 0xC0, 0x2A });
Console.WriteLine(instruction.Format()); // add rax, 0x2A
var bytes = instruction.ToEncoderRequest()
    .WithMnemonic(Core.ZydisMnemonic_.ZYDIS_MNEMONIC_SUB)
    .WithImmediate(1, 0x33).Encode();
Console.WriteLine(Convert.ToHexString(bytes));
```

Decoded results own their bytes, operands and copied flag metadata. Calls borrow
input spans synchronously. Encoder requests are immutable snapshots; `Value`
returns the actual emitted request struct for advanced settings. The complete
generated low-level surface is available in `Managed.Zydis.Zydis`; its unsafe
pointer contracts follow upstream. Independent owning calls use local state.
This translation makes no isolation guarantee for untrusted unsafe callers.

The consumer also accepts `disassemble`, `file`, `roundtrip` and
`rewrite-immediate`; run it with `--help` for arguments. See its
[usage](samples/ManagedConsumer/README.md).

Default provenance policy is `--hashes warn`; `--hashes off` disables historical
integrity checks while retaining structural closure and behavioral failures.
`--fetch never` reuses usable local trees and cached archives. Strict hashes are
an optional audit, not the ordinary execution prerequisite. Scripts resolve
paths from their own locations and use ordinary copies, without symlinks or
administrator requirements.
