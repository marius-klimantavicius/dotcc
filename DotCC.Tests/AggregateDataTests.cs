using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class AggregateDataTests
{
    [Fact]
    public void Aggregate_data_is_disabled_by_default() => new CSharpOutputOptions().AggregateData.ShouldBeFalse();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_off_preserves_default_output_and_enabled_emission_is_deterministic(bool objects)
    {
        WithSource(Rows(128), path =>
        {
            var options = new CSharpOutputOptions(Runtime: objects ? RuntimeProfile.All : RuntimeProfile.C);
            string Emit(CSharpOutputOptions settings) => objects
                ? Compiler.EmitObject(path, outputOptions: settings)
                : Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, className: "Api", outputOptions: settings);
            var defaultOutput = Emit(options);
            Emit(options with { AggregateData = false }).ShouldBe(defaultOutput);
            defaultOutput.ShouldNotContain("// aggregate-data:");
            var enabled = Emit(options with { AggregateData = true });
            enabled.ShouldContain("// aggregate-data: 1024 bytes");
            Emit(options with { AggregateData = true }).ShouldBe(enabled);
        });
    }

    [Theory]
    [InlineData(127, false)]
    [InlineData(128, true)]
    [InlineData(129, true)]
    public void Threshold_uses_complete_target_storage_bytes(int count, bool blob)
    {
        WithSource(Rows(count), path =>
        {
            var emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib,
                outputOptions: new(AggregateData: true));
            emitted.Contains("// aggregate-data:", StringComparison.Ordinal).ShouldBe(blob);
            if (blob)
            {
                emitted.ShouldContain($"// aggregate-data: {count * 8} bytes");
                emitted.ShouldContain("global::System.BitConverter.IsLittleEndian");
                emitted.ShouldContain("sizeof(Row) != 8");
                emitted.ShouldContain("new global::System.Span<byte>");
            }
        });
    }

    [Theory]
    [InlineData("int *owner;", "&root")]
    [InlineData("int *owner;", "0")]
    [InlineData("int *owner;", "")]
    [InlineData("int (*apply)(int);", "plus")]
    [InlineData("unsigned __int128 value;", "((unsigned __int128)1 << 100)")]
    [InlineData("volatile int value;", "7")]
    [InlineData("_Atomic int value;", "7")]
    public void Unsupported_layouts_and_relocations_keep_the_complete_typed_initializer(string field, string value)
    {
        WithSource("int root=7; int plus(int n) { return n+3; } struct Row { int tag; " + field
            + " }; static struct Row rows[256]={{11" + (value.Length == 0 ? "" : "," + value) + "}};", path =>
        {
            var emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib,
                outputOptions: new(AggregateData: true));
            emitted.ShouldNotContain("// aggregate-data:");
        });
    }

    [Fact]
    public void Link_time_enablement_requires_regenerating_typed_objects()
    {
        WithSource(Rows(128), path =>
        {
            string obj = path + ".o";
            try
            {
                File.WriteAllText(obj, Compiler.EmitObject(path));
                Should.Throw<CompileException>(() => Compiler.LinkObjects([obj], emit: EmitMode.ManagedLib,
                    outputOptions: new(AggregateData: true))).Message.ShouldContain("EmitObject");
                File.WriteAllText(obj, Compiler.EmitObject(path, outputOptions: new(AggregateData: true)));
                Compiler.LinkObjects([obj], emit: EmitMode.ManagedLib).ShouldContain("// aggregate-data: 1024 bytes");
            }
            finally { if (File.Exists(obj)) File.Delete(obj); }
        });
    }

    private static string Rows(int count) => "struct Row { int signed_value; unsigned bits; }; static struct Row rows["
        + count + "] = {{-17,0x80000000u},[" + (count - 1) + "]={23,0xffffffffu}};";

    private static void WithSource(string source, Action<string> action)
    {
        string path = Path.Combine(Path.GetTempPath(), "dotcc-aggregate-data-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try { action(path); }
        finally { File.Delete(path); }
    }
}
