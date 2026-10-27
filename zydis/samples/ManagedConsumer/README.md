The default run executes an asserting decode/format/encode workflow through an
ordinary project reference. It covers 16/32/64-bit decoding, Intel and AT&T text,
metadata and operands, new and decoded encoder requests, immutable mutation,
roundtrips, address calculation, absolute encoding, NOP filling, ordinary errors,
ownership after input mutation/GC, and concurrent calls.

After the parent produces the translated library:

```bash
dotnet run --project zydis/samples/ManagedConsumer -c Release
dotnet run --project zydis/samples/ManagedConsumer -c Release -- disassemble 64 "48 83 C0 2A C3" 0x1000 att
dotnet run --project zydis/samples/ManagedConsumer -c Release -- roundtrip 32 "B8 78 56 34 12 C3"
dotnet run --project zydis/samples/ManagedConsumer -c Release -- rewrite-immediate 64 "48 83 C0 2A" 1 0x33
dotnet run --project zydis/samples/ManagedConsumer -c Release -- file 64 code.bin 0x1000 intel
```

Select another generated project with the `TranslatedProject` MSBuild property,
preferably an absolute path. There is no package, native Zydis import, or runtime
source generation. The library itself contains no demo workload.

The owning API is in namespace `Managed.Zydis`:

```csharp
using Managed.Zydis;
using Core = Managed.Zydis.Zydis;

var instruction = InstructionCodec.Decode(new byte[] { 0x48, 0x83, 0xC0, 0x2A });
Console.WriteLine(instruction.Format(Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_ATT));
var request = instruction.ToEncoderRequest().WithImmediate(1, 0x33);
byte[] encoded = request.Encode();

// Advanced encoder features retain the actual upstream request and inline arrays.
var raw = request.Value;
raw.mnemonic = Core.ZydisMnemonic_.ZYDIS_MNEMONIC_SUB;
encoded = new EncoderRequest(raw).Encode();
```

`DecodeBuffer` fails at the first invalid/truncated instruction and includes its
byte offset in `ZydisException`. `TryDecode` and `TryEncode` return the original
upstream status without an exception; programming errors such as invalid operand
indices still throw argument exceptions. Encoder destinations are unspecified on
failure and report zero bytes written.

Snapshots retain no caller buffers or translated pointers. Their byte and operand
spans are read-only views of owned arrays. `Metadata` is a copy of the actual
emitted instruction, with its two flag pointers cleared; `CpuFlags` and `FpuFlags`
hold copied values. All calls create local state, so snapshots and requests can
be shared across concurrent calls. Caller inputs must remain stable during a call.
The generated `Managed.Zydis.Zydis` low-level surface remains public for callers
requiring upstream settings, formatter hooks, or other low-level APIs.
