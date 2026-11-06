using System;
using System.IO;
using DotCC.Frontends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigFileNamespaceDemandTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "dotcc-file-namespace-" + Guid.NewGuid().ToString("N"));
    public ZigFileNamespaceDemandTests()
    {
        Directory.CreateDirectory(Path.Combine(_directory, "lib", "std"));
        File.WriteAllText(Path.Combine(_directory, "lib", "std", "std.zig"), "");
        File.WriteAllText(Path.Combine(_directory, "dep.zig"), """
            value: @compileError("instance layout demanded"),
            pub const answer = 42;
            pub fn add(value: u8) u8 { return value + answer; }
            """);
    }
    public void Dispose() => Directory.Delete(_directory, true);
    private IrBuilder Build(string body, string extra = "")
    {
        var path = Path.Combine(_directory, "root.zig");
        File.WriteAllText(path, "const dep = @import(\"dep.zig\"); pub fn main() u8 { " + body + " } " + extra);
        var options = new ZigCompilationOptions("root", [new("root", path, [])],
            zigLibDir: Path.Combine(_directory, "lib"), stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed);
        return new ZigFrontend().BuildIr(new FrontendRequest([path], ZigOptions: options));
    }

    [Fact]
    public void Static_file_members_do_not_demand_instance_fields()
    {
        var ir = Build("return dep.add(0);");
        var main = System.Linq.Enumerable.Single(ir.Functions, f => f.Sym.Name == "main");
        ir.EvalComptimeValue(new Call("main", []) { CalleeSym = main.Sym, Type = CType.UChar })
            .ShouldBeOfType<IrBuilder.CtInt>().Value.ShouldBe(42);
        ir.StructFields.ContainsKey("dep__dep").ShouldBeFalse();
    }

    [Theory]
    [InlineData("return @intCast(@sizeOf(dep));")]
    [InlineData("return @intCast(@alignOf(dep));")]
    [InlineData("return @intCast(@sizeOf([2]dep));")]
    [InlineData("const value: dep = undefined; _ = value; return 42;")]
    [InlineData("const value: ?dep = null; _ = value; return 42;")]
    [InlineData("const value = dep{}; _ = value; return 42;")]
    [InlineData("const pointer: *dep = @ptrFromInt(8); _ = pointer.value; return 42;")]
    [InlineData("const pointer: *dep = @ptrFromInt(8); _ = pointer.*; return 42;")]
    [InlineData("const pointer: [*]dep = @ptrFromInt(8); _ = pointer[0]; return 42;")]
    [InlineData("const pointer: [*]dep = @ptrFromInt(8); _ = pointer + 1; return 42;")]
    [InlineData("return @intCast(@offsetOf(dep, \"value\"));")]
    public void Demanded_file_values_keep_the_original_field_diagnostic(string body)
        => Should.Throw<CompileException>(() => Build(body)).Message.ShouldContain("instance layout demanded");

    [Fact]
    public void By_value_parameter_keeps_the_original_field_diagnostic()
        => Should.Throw<CompileException>(() => Build("return 42;", "pub fn read(value: dep) u8 { _ = value; return 0; }"))
            .Message.ShouldContain("instance layout demanded");
}
