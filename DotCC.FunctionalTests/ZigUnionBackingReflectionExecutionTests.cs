using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigUnionBackingReflectionExecutionTests
{
    [Theory]
    [InlineData("plain.zig", false)]
    [InlineData("plain.zig", true)]
    [InlineData("packed.zig", false)]
    [InlineData("packed.zig", true)]
    public void Union_backing_integer_matches_source_layout_and_declared_type(string fixture, bool objects)
    {
        var path = Path.Combine(FixtureDirectory(), fixture);
        var options = Options(path);
        var directory = Path.Combine(Path.GetTempPath(), "union-backing-reflection-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted
                + "\npublic static class UnionBackingReflectionHost { public static int Main() => DotCcLib.answer(); }", [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("mismatched.zig", "packed backing width 16 does not match field width 8", false)]
    [InlineData("mismatched.zig", "packed backing width 16 does not match field width 8", true)]
    [InlineData("null-unwrap.zig", "unwrap the null backing_integer", false)]
    [InlineData("null-unwrap.zig", "unwrap the null backing_integer", true)]
    public void Invalid_union_backing_queries_reject(string fixture, string diagnostic, bool objects)
    {
        var path = Path.Combine(FixtureDirectory(), fixture);
        var options = Options(path);
        var error = Should.Throw<CompileException>(() =>
        {
            if (objects) Compiler.EmitObject(path, zigOptions: options);
            else Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
        });
        error.Message.ShouldContain(diagnostic);
    }

    [Theory]
    [InlineData("sign")]
    [InlineData("offset")]
    [InlineData("width")]
    [InlineData("physical")]
    public void Explicit_union_object_proof_keeps_strict_sign_overlay_and_layout_checks(string corruption)
    {
        var parts = SignedUnionContract().Split('|');
        ZigStorageObjectMetadata.ReadContract(string.Join('|', parts)).IsPackedUnion.ShouldBeTrue();
        if (corruption == "sign") parts[5] = "0";
        else if (corruption == "physical") parts[7] = "2";
        else
        {
            var fields = parts[11].Split(';');
            var field = fields[1].Split(',');
            field[corruption == "offset" ? 3 : 4] = corruption == "offset" ? "1" : "7";
            fields[1] = string.Join(',', field);
            parts[11] = string.Join(';', fields);
        }
        Should.Throw<CompileException>(() => ZigStorageObjectMetadata.ReadContract(string.Join('|', parts)))
            .Message.ShouldContain("invalid Zig storage semantic descriptor");
    }

    [Fact]
    public void Existing_inferred_packed_union_object_proofs_remain_readable()
    {
        var parts = SignedUnionContract().Split('|');
        parts[4] = Convert.ToBase64String(Encoding.UTF8.GetBytes("packed-union"));
        parts[5] = "0";
        var proof = ZigStorageObjectMetadata.ReadContract(string.Join('|', parts));
        proof.IsPackedUnion.ShouldBeTrue();
        proof.Bits.ShouldBe(8);
    }

    private static string SignedUnionContract()
    {
        var path = Path.Combine(FixtureDirectory(), "packed.zig");
        return Compiler.EmitObject(path, zigOptions: Options(path)).Split('\n')
            .Where(line => line.StartsWith("//!!dotcc-obj zig-storage:", StringComparison.Ordinal))
            .Select(line => Encoding.UTF8.GetString(Convert.FromBase64String(line.Split(' ')[3])))
            .Single(contract => contract.StartsWith("zig-storage-v1|", StringComparison.Ordinal)
                && Encoding.UTF8.GetString(Convert.FromBase64String(contract.Split('|')[4])) == "packed-union-explicit-i8");
    }

    private static ZigCompilationOptions Options(string path)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        return new ZigCompilationOptions("root", [new("root", path, [])], zigLibDir: library,
            outputMode: ZigSourceOutputMode.Lib, optimizeMode: ZigSourceOptimizeMode.Debug,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed,
            managedRoots: ["answer"], isTest: true);
    }
    private static string FixtureDirectory([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigUnionBackingReflection");
}
