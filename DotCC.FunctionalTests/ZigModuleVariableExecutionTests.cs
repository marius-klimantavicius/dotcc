using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigModuleVariableExecutionTests
{
    [Theory]
    [InlineData(false, "mutation.zig")]
    [InlineData(true, "mutation.zig")]
    [InlineData(false, "initializer.zig")]
    [InlineData(true, "initializer.zig")]
    [InlineData(false, "callback.zig")]
    [InlineData(true, "callback.zig")]
    [InlineData(false, "threadlocal.zig")]
    [InlineData(true, "threadlocal.zig")]
    public void Imported_mutable_storage_keeps_owner_identity_and_state(bool objects, string entry)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-module-variable-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(Fixtures(), entry);
            var options = Options(path, directory);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.csobj");
                File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
                emitted = Compiler.LinkObjects([fragment], emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp([path], emit: EmitMode.Csproj, zigOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, [], TestContext.Current.CancellationToken);
            result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();
            result.stderr.ShouldBeEmpty();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("pub var value: u32 = value;", "return @intCast(module.value);", "initializer dependency loop")]
    [InlineData("pub threadlocal var value: [2]u32 = .{ 1, 2 };", "return @intCast(module.value[0]);", "only a scalar threadlocal")]
    [InlineData("pub extern var value: u32;", "return @intCast(module.value);", "extern")]
    [InlineData("pub var value: *const u32 = &seed; const seed: u32 = 17;", "module.value.* = 3; return 42;", "cannot assign to constant")]
    public void Unsupported_or_readonly_storage_fails_explicitly(string source, string body, string diagnostic)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-module-variable-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(Path.Combine(directory, "module.zig"), source);
            File.WriteAllText(path, "const module = @import(\"module.zig\"); pub fn main() u8 { " + body + " }");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path], zigOptions: Options(path, directory)))
                .Message.ShouldContain(diagnostic);
        }
        finally { Directory.Delete(directory, true); }
    }

    private static ZigCompilationOptions Options(string path, string directory)
    {
        var library = Path.Combine(directory, "lib");
        Directory.CreateDirectory(Path.Combine(library, "std"));
        File.WriteAllText(Path.Combine(library, "std", "std.zig"), "");
        return new ZigCompilationOptions("root", [new("root", path, [])], zigLibDir: library,
            stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed);
    }

    private static string Fixtures([CallerFilePath] string file = "")
        => Path.Combine(Path.GetDirectoryName(file)!, "Fixtures", "ZigModuleVariables");
}
