using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using DotCC;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class TypedStorageA25Tests
{
    private delegate ReadOnlySpan<T> Getter<T>() where T : unmanaged;

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Typed_rows_payloads_TLS_and_owners_retain_final_storage(bool link, bool instance)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-a25-storage-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string input = Path.Combine(AppContext.BaseDirectory, "Fixtures", "d10-typed-storage-a25", "main.c");
            if (link)
            {
                string obj = Path.Combine(directory, "storage.o");
                File.WriteAllText(obj, Compiler.EmitObject(input, outputOptions: new(InstanceMethods: instance)));
                input = obj;
            }
            var options = new CSharpOutputOptions(Runtime: RuntimeProfile.C, LiteralPool: true, NestTypes: true, InstanceMethods: instance);
            var files = link
                ? Compiler.LinkObjectFiles([input], emit: EmitMode.ManagedLib, className: "Api", namespaceName: "A25",
                    outputOptions: options, split: SourceSplit.Size, splitSize: 4096)
                : Compiler.EmitCSharpFiles([input], emit: EmitMode.ManagedLib, className: "Api", namespaceName: "A25",
                    outputOptions: options, split: SourceSplit.Size, splitSize: 4096);
            string source = string.Join("\n", files.Values);
            source.ShouldNotContain("AsSpan(new ");
            source.ShouldContain("ReadOnlySpan<float> Values");
            source.ShouldContain("ReadOnlySpan<double> Values");
            source.ShouldContain("ReadOnlySpan<CBool> Values");
            source.ShouldContain("rows_destination[0] = new()");
            source.ShouldContain("nested = new()");
            source.ShouldContain("selected = new()");
            source.ShouldNotContain("bitrows_destination[0] = new()");
            source.ShouldContain("fixed (WithArray* containing_destination");
            source.ShouldContain("__dotcc_tls_initialize_thread_record");
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path));
            var compilation = CSharpCompilation.Create("A25Storage" + Guid.NewGuid().ToString("N"),
                files.Select(file => CSharpSyntaxTree.ParseText(file.Value, new CSharpParseOptions(LanguageVersion.Preview), path: file.Key)),
                references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true,
                    optimizationLevel: OptimizationLevel.Release));
            using var stream = new MemoryStream();
            var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            var context = new AssemblyLoadContext("a25-storage-" + Guid.NewGuid(), isCollectible: false);
            byte[] image = stream.ToArray();
            var assembly = context.LoadFromStream(new MemoryStream(image));
            Type api = assembly.GetType("A25.Api")!;
            object? first = instance ? Activator.CreateInstance(api) : null;
            object? second = instance ? Activator.CreateInstance(api) : null;
            try
            {
                object? Invoke(string name, object? owner = null) => api.GetMethod(name)!.Invoke(owner, null);
                Invoke("a25_check", first).ShouldBe(1);
                Invoke("a25_mutable", first).ShouldBe(1);
                Invoke("a25_static_step", first).ShouldBe(31);
                Invoke("a25_static_step", first).ShouldBe(32);
                if (instance)
                {
                    Invoke("a25_static_step", second).ShouldBe(31);
                    Invoke("a25_rows_address", first).ShouldNotBe(Invoke("a25_rows_address", second));
                }
                object? address = Invoke("a25_rows_address", first);
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
                Invoke("a25_check", first).ShouldBe(1);
                Invoke("a25_rows_address", first).ShouldBe(address);
                Invoke("a25_thread_step", first).ShouldBe(24);
                Invoke("a25_thread_step", first).ShouldBe(26);
                if (instance) Invoke("a25_thread_step", second).ShouldBe(24);
                var outcomes = new int[2, 2];
                var addresses = new object?[2];
                var errors = new Exception?[2];
                using var barrier = new Barrier(3);
                var threads = Enumerable.Range(0, 2).Select(index => new Thread(() =>
                {
                    try
                    {
                        outcomes[index, 0] = (int)Invoke("a25_thread_step", first)!;
                        outcomes[index, 1] = (int)Invoke("a25_thread_step", first)!;
                        addresses[index] = Invoke("a25_thread_address", first);
                    }
                    catch (Exception error) { errors[index] = error; }
                    finally { barrier.SignalAndWait(TimeSpan.FromSeconds(15)); }
                })).ToArray();
                foreach (var thread in threads) thread.Start();
                barrier.SignalAndWait(TimeSpan.FromSeconds(15)).ShouldBeTrue();
                foreach (var thread in threads) thread.Join(TimeSpan.FromSeconds(15)).ShouldBeTrue();
                errors.ShouldAllBe(error => error == null);
                for (int i = 0; i < 2; i++) { outcomes[i, 0].ShouldBe(24); outcomes[i, 1].ShouldBe(26); }
                addresses[0].ShouldNotBe(addresses[1]);
                Invoke("a25_thread_step", first).ShouldBe(28);
                InspectSpans(assembly, image);
            }
            finally { (second as IDisposable)?.Dispose(); (first as IDisposable)?.Dispose(); }
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void Integer_backed_float_views_preserve_noncanonical_NaNs_and_signed_zero_bits()
    {
        // Deliberate noncanonical payloads are supplied as owning data bytes,
        // independently of floating arithmetic/decimal formatting in the folder.
        var pool = new LiteralPool();
        byte[] single = Convert.FromHexString("000000804200C07F010000000000807F");
        byte[] real = Convert.FromHexString("0000000000000080420000000000F87F0100000000000000");
        string s = pool.AddPayload("uint", single), d = pool.AddPayload("ulong", real);
        pool.AddPayload("uint", single).ShouldBe(s);
        var declarations = new Dictionary<string, string>();
        pool.AddDeclarations(declarations);
        var output = LiteralPool.CreateOutput(declarations, "Data", enabled: false);
        string source = "using DotCcLiterals = Data;\n" + output.Source + output.Rewrite("""
            public static class Views {
                public static System.ReadOnlySpan<float> Singles => System.Runtime.InteropServices.MemoryMarshal.Cast<uint,float>(SINGLE);
                public static System.ReadOnlySpan<double> Doubles => System.Runtime.InteropServices.MemoryMarshal.Cast<ulong,double>(DOUBLE);
            }
            """.Replace("SINGLE", s).Replace("DOUBLE", d));
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("A25Bits" + Guid.NewGuid().ToString("N"),
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview))], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        result.Success.ShouldBeTrue(string.Join("\n", result.Diagnostics));
        byte[] image = stream.ToArray();
        var context = new AssemblyLoadContext("a25-bits-" + Guid.NewGuid(), isCollectible: true);
        try
        {
            var assembly = context.LoadFromStream(new MemoryStream(image));
            CheckGetter<float>(assembly.GetType("Views")!.GetMethod("get_Singles")!, single);
            CheckGetter<double>(assembly.GetType("Views")!.GetMethod("get_Doubles")!, real);
            InspectSpans(assembly, image);
        }
        finally { context.Unload(); }
    }

    private static void InspectSpans(Assembly assembly, byte[] image)
    {
        using var pe = new PEReader(new MemoryStream(image));
        var metadata = pe.GetMetadataReader();
        var methods = assembly.GetTypes().SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Where(method => method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(ReadOnlySpan<>)
                && (method.Name.StartsWith("get_B", StringComparison.Ordinal) || method.Name.StartsWith("get_P", StringComparison.Ordinal)
                    || method.Name == "get_Values")).ToArray();
        methods.ShouldNotBeEmpty();
        foreach (var method in methods)
        {
            Type element = method.ReturnType.GenericTypeArguments[0];
            byte[] bytes = (byte[])typeof(TypedStorageA25Tests).GetMethod(nameof(ReadAndMeasure), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(element).Invoke(null, [method])!;
            int width = element.IsEnum ? Marshal.SizeOf(Enum.GetUnderlyingType(element))
                : element == typeof(char) ? 2 : element.Name == "CBool" ? 1 : Marshal.SizeOf(element);
            (bytes.Length / width).ShouldBeLessThanOrEqualTo(LiteralPool.PayloadElements);
            var instructions = Instructions(method.GetMethodBody()!.GetILAsByteArray()!).ToArray();
            instructions.ShouldNotContain(instruction => instruction.Code == OpCodes.Newarr
                || instruction.Code.Name!.StartsWith("conv.", StringComparison.Ordinal));
            if (method.Name.StartsWith("get_B", StringComparison.Ordinal) || method.Name.StartsWith("get_P", StringComparison.Ordinal))
            {
                var fields = instructions.Where(instruction => instruction.Code == OpCodes.Ldtoken || instruction.Code == OpCodes.Ldsflda)
                    .Where(instruction => (instruction.Token & unchecked((int)0xff000000)) == 0x04000000)
                    .Select(instruction => metadata.GetFieldDefinition(MetadataTokens.FieldDefinitionHandle(instruction.Token & 0x00ffffff))).ToArray();
                fields.ShouldContain(field => field.GetRelativeVirtualAddress() != 0
                    && pe.GetSectionData(field.GetRelativeVirtualAddress()).GetContent(0, bytes.Length).SequenceEqual(bytes));
            }
            else
            {
                var calls = instructions.Where(instruction => instruction.Code == OpCodes.Call)
                    .Select(instruction => method.Module.ResolveMethod(instruction.Token)).ToArray();
                calls.ShouldContain(call => call!.DeclaringType == typeof(MemoryMarshal) && call.Name == "Cast");
            }
        }
    }

    private static byte[] ReadAndMeasure<T>(MethodInfo method) where T : unmanaged
    {
        var getter = method.CreateDelegate<Getter<T>>();
        byte[] expected = MemoryMarshal.AsBytes(getter()).ToArray();
        CheckGetter<T>(method, expected);
        return expected;
    }

    private static void CheckGetter<T>(MethodInfo method, byte[] expected) where T : unmanaged
    {
        var getter = method.CreateDelegate<Getter<T>>();
        MemoryMarshal.AsBytes(getter()).ToArray().ShouldBe(expected);
        var destination = new byte[expected.Length];
        for (int i = 0; i < 200; i++) MemoryMarshal.AsBytes(getter()).CopyTo(destination);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10000; i++) MemoryMarshal.AsBytes(getter()).CopyTo(destination);
        (GC.GetAllocatedBytesForCurrentThread() - before).ShouldBe(0);
        destination.ShouldBe(expected);
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
