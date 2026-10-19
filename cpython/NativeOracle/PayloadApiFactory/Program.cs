using System.Buffers.Binary;
using System.Security.Cryptography;
using DotCC;

if (args.Length != 1) throw new ArgumentException("output source directory");
var folder = Path.GetFullPath(args[0]);
Directory.CreateDirectory(folder);
// This program calls the exact frozen compiler library, through its existing
// test-friend API. Expected bytes are specified independently of its output.
uint[] singleBits = [0x00000000, 0x80000000, 0x00000001, 0x007fffff,
    0x7f800000, 0xff800000, 0x7fc00042, 0xffc000a5, 0x7fffffff];
ulong[] doubleBits = [0, 0x8000000000000000, 1, 0x000fffffffffffff,
    0x7ff0000000000000, 0xfff0000000000000, 0x7ff8000000000042,
    0xfff80000000000a5, 0x7fffffffffffffff];
var bytes32 = new byte[singleBits.Length * 4];
var bytes64 = new byte[doubleBits.Length * 8];
for (int i = 0; i < singleBits.Length; i++) BinaryPrimitives.WriteUInt32LittleEndian(bytes32.AsSpan(i * 4), singleBits[i]);
for (int i = 0; i < doubleBits.Length; i++) BinaryPrimitives.WriteUInt64LittleEndian(bytes64.AsSpan(i * 8), doubleBits[i]);
var pool = new LiteralPool();
string payload32 = pool.AddPayload("uint", bytes32), payload64 = pool.AddPayload("ulong", bytes64);
var declarations = new Dictionary<string, string>();
pool.AddDeclarations(declarations);
var output = LiteralPool.CreateOutput(declarations, "PayloadApiLiterals", enabled: true);
string ViewName(string name) => "__DotccPrimitiveView_" + Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(name)));
string view32 = ViewName("actual frozen uint to float NaN payload"), view64 = ViewName("actual frozen ulong to double NaN payload");
string source = """
using DotCcLiterals = global::FrozenPayload.PayloadApi.PayloadApiLiterals;
namespace FrozenPayload;
public sealed unsafe class PayloadApi : global::System.IDisposable
{
    // CONTROL_FIELDS
    public PayloadApi()
    {
        // CONTROL_BODY
        global::System.Span<float> singles = stackalloc float[9];
        VIEW32.Values.CopyTo(singles);
        global::System.Span<double> doubles = stackalloc double[9];
        VIEW64.Values.CopyTo(doubles);
    }
    public void Dispose() { }
    PAYLOADS
    internal static class VIEW32
    {
        public static global::System.ReadOnlySpan<float> Values => global::System.Runtime.InteropServices.MemoryMarshal.Cast<uint, float>(PAYLOAD32);
    }
    internal static class VIEW64
    {
        public static global::System.ReadOnlySpan<double> Values => global::System.Runtime.InteropServices.MemoryMarshal.Cast<ulong, double>(PAYLOAD64);
    }
}
// CONTROL_FOREIGN
""".Replace("VIEW32", view32).Replace("VIEW64", view64).Replace("PAYLOADS", output.Source)
    .Replace("PAYLOAD32", payload32).Replace("PAYLOAD64", payload64);
File.WriteAllText(Path.Combine(folder, "PayloadApi.cs"), source);
File.WriteAllText(Path.Combine(folder, "PayloadApi.csproj"), """
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings></PropertyGroup></Project>
""");
File.WriteAllText(Path.Combine(folder, "Dotcc.SourceFiles.txt"), "PayloadApi.cs\n");
File.WriteAllText(Path.Combine(folder, "expected.json"), System.Text.Json.JsonSerializer.Serialize(new {
    single = Convert.ToHexString(bytes32).ToLowerInvariant(), wide = Convert.ToHexString(bytes64).ToLowerInvariant(),
    view32, view64, payload32, payload64,
    scope = "explicit expected constant-property/RVA/typed-view bytes; independent of runtime union overwrite"
}));
Console.WriteLine("Frozen LiteralPool API emitted explicit signed-zero/subnormal/infinity/NaN payload backings and typed views");
