using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;
using DotCC;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class ConstantPayloadTests
{
    private delegate ReadOnlySpan<byte> PayloadGetter();

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Constant_payloads_use_RVA_and_allocate_nothing_on_repeated_access_and_copy(bool link, bool pool)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-payload-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "payload.c");
            var expected = new[] { 1, 2, 3, 4, 257, 70000 }.Select(length =>
                Enumerable.Range(0, length).Select(index => (byte)(index * 73 + 255)).ToArray()).ToArray();
            File.WriteAllText(path, string.Join("\n", expected.Select((bytes, index) =>
                "unsigned char a" + index + "[] = { " + string.Join(",", bytes) + " };")) +
                "\nunsigned char twin[] = {255,72,145,218};\n" +
                "int distinct(void) { a3[0] = 12; return a3 != twin && twin[0] == 255 && a3[0] == 12; }\n");
            if (link)
            {
                var obj = path + ".o";
                File.WriteAllText(obj, Compiler.EmitObject(path));
                path = obj;
            }
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, LiteralPool: pool, NestTypes: true);
            var files = link
                ? Compiler.LinkObjectFiles([path], emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Payload", outputOptions: options, split: SourceSplit.Size, splitSize: 4000)
                : Compiler.EmitCSharpFiles([path], emit: EmitMode.ManagedLib, className: "Api", namespaceName: "Payload", outputOptions: options, split: SourceSplit.Size, splitSize: 4000);
            var source = string.Join("\n", files.Values);
            source.ShouldNotContain("AsSpan(new byte[]");
            source.ShouldContain("static global::System.ReadOnlySpan<byte> B");
            source.ShouldContain("Globals.a3 = Libc.GlobalArrayZeroed<byte>(4)");
            source.ShouldContain("Globals.twin = Libc.GlobalArrayZeroed<byte>(4)");
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Select(p => MetadataReference.CreateFromFile(p));
            var compilation = CSharpCompilation.Create("Payload" + Guid.NewGuid().ToString("N"),
                files.Select(file => CSharpSyntaxTree.ParseText(file.Value, new CSharpParseOptions(LanguageVersion.Preview), path: file.Key)), references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release));
            using var stream = new MemoryStream();
            var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            var image = stream.ToArray();
            var context = new AssemblyLoadContext("payload-" + Guid.NewGuid(), isCollectible: true);
            try
            {
                var assembly = context.LoadFromStream(new MemoryStream(image));
                assembly.GetType("Payload.Api")!.GetMethod("distinct")!.Invoke(null, null).ShouldBe(1);
                var getters = assembly.GetTypes().SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    .Where(method => method.Name.StartsWith("get_B", StringComparison.Ordinal) && method.ReturnType == typeof(ReadOnlySpan<byte>)).ToArray();
                var chunks = expected.SelectMany(bytes => bytes.Chunk(LiteralPool.PayloadElements))
                    .DistinctBy(bytes => Convert.ToHexString(bytes)).ToArray();
                getters.Length.ShouldBe(chunks.Length); // Equal bounded backings pool; C destinations stay separate.
                using var pe = new PEReader(new MemoryStream(image));
                var metadata = pe.GetMetadataReader();
                foreach (var method in getters)
                {
                    var getter = method.CreateDelegate<PayloadGetter>();
                    var bytes = getter().ToArray();
                    chunks.ShouldContain(value => value.SequenceEqual(bytes));
                    var instructions = Instructions(method.GetMethodBody()!.GetILAsByteArray()!).ToArray();
                    instructions.ShouldNotContain(instruction => instruction.Code == OpCodes.Newarr);
                    // Tie the getter to actual data in this PE; opcode-byte searches
                    // and source syntax do not establish an RVA-backed property.
                    var fields = instructions.Where(instruction => instruction.Code == OpCodes.Ldtoken || instruction.Code == OpCodes.Ldsflda)
                        .Where(instruction => (instruction.Token & unchecked((int)0xff000000)) == 0x04000000)
                        .Select(instruction => metadata.GetFieldDefinition(MetadataTokens.FieldDefinitionHandle(instruction.Token & 0x00ffffff))).ToArray();
                    fields.ShouldContain(field => field.GetRelativeVirtualAddress() != 0
                        && pe.GetSectionData(field.GetRelativeVirtualAddress()).GetContent(0, bytes.Length).SequenceEqual(bytes));
                    var destination = new byte[bytes.Length];
                    for (int index = 0; index < 200; index++) getter().CopyTo(destination);
                    long before = GC.GetAllocatedBytesForCurrentThread();
                    for (int index = 0; index < 10000; index++) getter().CopyTo(destination);
                    (GC.GetAllocatedBytesForCurrentThread() - before).ShouldBe(0);
                    destination.ShouldBe(bytes);
                }
            }
            finally { context.Unload(); }
        }
        finally { Directory.Delete(directory, true); }
    }

    private static IEnumerable<(OpCode Code, int Token)> Instructions(byte[] il)
    {
        var codes = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(field => field.FieldType == typeof(OpCode)).Select(field => (OpCode)field.GetValue(null)!)
            .ToDictionary(code => unchecked((ushort)code.Value));
        for (int position = 0; position < il.Length;)
        {
            ushort value = il[position++];
            if (value == 0xfe) value = (ushort)(0xfe00 | il[position++]);
            var code = codes[value];
            int size = code.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, position),
                _ => 4
            };
            yield return (code, size == 4 ? BitConverter.ToInt32(il, position) : 0);
            position += size;
        }
    }
}
