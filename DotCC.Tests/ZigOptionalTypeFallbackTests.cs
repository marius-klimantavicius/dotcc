using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigOptionalTypeFallbackTests
{
    [Theory]
    [InlineData("const O: ?type = undefined; const T = O orelse u8;", "optional type initializer")]
    [InlineData("const O: ?type = 42; const T = O orelse u8;", "optional type initializer")]
    [InlineData("const O: ?type = if (1) u8 else null; const T = O orelse u8;", "optional type condition must be bool")]
    [InlineData("var flag: bool = true; const O: ?type = if (flag) u8 else null; const T = O orelse u8;", "optional type condition must be comptime-known")]
    [InlineData("const O: ?type = if (missing_flag) u8 else null; const T = O orelse u8;", "missing_flag")]
    [InlineData("const O: ?u8 = 3; const T = O orelse u8;", "type 'T' not resolved")]
    [InlineData("const O: ?type = u21; const T = O;", "type 'T' not resolved")]
    [InlineData("const O: ?type = null; const T = O orelse @compileError(\"selected fallback\");", "selected fallback")]
    [InlineData("const O: ?type = if (true) @compileError(\"selected initializer\") else null; const T = O orelse u8;", "selected initializer")]
    [InlineData("const O: ?type = O; const T = O orelse u8;", "dependency loop")]
    public void Invalid_optional_types_fail_explicitly_in_source_and_object(string declarations, string diagnostic)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-optional-type-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            var library = Path.Combine(directory, "lib");
            Directory.CreateDirectory(Path.Combine(library, "std"));
            File.WriteAllText(Path.Combine(library, "std", "std.zig"), "");
            File.WriteAllText(path, declarations + " pub fn main() u8 { const value: T = undefined; _ = value; return 42; }");
            var options = new ZigCompilationOptions("root", [new("root", path, [])], zigLibDir: library,
                stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed);
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path], zigOptions: options)).Message.ShouldContain(diagnostic);
            Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: options)).Message.ShouldContain(diagnostic);
        }
        finally { Directory.Delete(directory, true); }
    }
}
