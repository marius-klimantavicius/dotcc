using System;
using System.IO;
using System.Runtime.Loader;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Bit_operation_intrinsic_preserves_width_effects_and_pointer_calls(bool objectLink)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-bit-intrinsic-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "probe.c");
            File.WriteAllText(path, """
                int count_bits(unsigned long long value) { return -1; }
                int probe(void) {
                    int (*callback)(unsigned long long) = count_bits;
                    unsigned long long value = 0x8000000000000000ULL;
                    if (count_bits(0) != 0) return 1;
                    if (count_bits(~0ULL) != 64) return 2;
                    if (callback(value++) != 1) return 3;
                    if (value != 0x8000000000000001ULL) return 4;
                    if (count_bits(value++) != 2) return 5;
                    if (value != 0x8000000000000002ULL) return 6;
                    if (callback(0xaaaaaaaaaaaaaaaaULL) != 32) return 7;
                    return 0;
                }
                """);
            var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: new[]
            {
                new FunctionOverride("count_bits", new("int", new[] { "unsigned long long" }),
                    new("intrinsic", "popcount.u64"), RequireMatch: true)
            });
            string generated;
            if (objectLink)
            {
                var obj = Path.Combine(directory, "probe.o");
                File.WriteAllText(obj, Compiler.EmitObject(path, preprocessing: options));
                generated = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.ManagedLib, className: "Api");
            }
            else generated = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib,
                className: "Api", preprocessing: options);
            generated.ShouldContain("global::System.Numerics.BitOperations.PopCount(");
            var image = Compile("BitIntrinsic_" + Guid.NewGuid().ToString("N"), generated, RuntimeReferences());
            var context = new AssemblyLoadContext("bit-intrinsic-" + Guid.NewGuid(), isCollectible: false);
            var assembly = context.LoadFromStream(new MemoryStream(image));
            assembly.GetType("Api", true)!.GetMethod("probe")!.Invoke(null, null).ShouldBe(0);
        }
        finally { Directory.Delete(directory, true); }
    }
}
