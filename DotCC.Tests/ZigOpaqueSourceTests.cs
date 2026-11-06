using System;
using System.IO;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigOpaqueSourceTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "dotcc-opaque-" + Guid.NewGuid().ToString("N"));
    public ZigOpaqueSourceTests()
    {
        Directory.CreateDirectory(Path.Combine(_directory, "lib", "std"));
        File.WriteAllText(Path.Combine(_directory, "lib", "std", "std.zig"), "");
    }
    public void Dispose() => Directory.Delete(_directory, true);
    private ZigCompilationOptions Options(string path) => new("root", [new("root", path, [])],
        zigLibDir: Path.Combine(_directory, "lib"), stdlibTypes: ZigStdlibTypeModel.Source,
        functionPointerAbi: ZigFunctionPointerAbi.Managed);

    [Theory]
    [InlineData("return @intCast(@sizeOf(Handle));")]
    [InlineData("return @intCast(@alignOf(Handle));")]
    [InlineData("return @intCast(@bitSizeOf(Handle));")]
    [InlineData("const value: Handle = undefined; _ = value; return 42;")]
    [InlineData("const value: ?Handle = null; _ = value; return 42;")]
    [InlineData("return @intCast(@sizeOf([0]Handle));")]
    [InlineData("return @intCast(@sizeOf(?Handle));")]
    [InlineData("const pointer: *Handle = @ptrFromInt(8); _ = pointer.*; return 42;")]
    [InlineData("const pointer: [*]Handle = @ptrFromInt(8); _ = pointer[0]; return 42;")]
    [InlineData("const pointer: *Handle = @ptrFromInt(8); _ = pointer + 1; return 42;")]
    [InlineData("const value = Handle{}; _ = value; return 42;")]
    public void Incomplete_values_and_layout_queries_fail_explicitly(string body)
    {
        var path = Path.Combine(_directory, "main.zig");
        File.WriteAllText(path, "const Handle = opaque {}; pub fn main() u8 { " + body + " }");
        var error = Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: Options(path)));
        error.Message.ShouldContain("opaque");
    }

    [Theory]
    [InlineData("const a: *First = @ptrFromInt(8); const b: *Second = a; _ = b;")]
    [InlineData("const a: *First = @ptrFromInt(8); const b: *Second = @as(*Second, a); _ = b;")]
    [InlineData("const a: *First = @ptrFromInt(8); const b: *u8 = a; _ = b;")]
    [InlineData("const a: *anyopaque = @ptrFromInt(8); const b: *First = a; _ = b;")]
    public void Erasing_target_pointer_spelling_does_not_erase_source_nominal_checks(string body)
    {
        var path = Path.Combine(_directory, "main.zig");
        File.WriteAllText(path, "const First = opaque {}; const Second = opaque {}; pub fn main() u8 { " + body + " return 42; }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: Options(path)))
            .Message.ShouldContain("incompatible opaque pointer types");
    }

    [Fact]
    public void Object_contracts_keep_nominal_pointees_without_creating_aggregate_layouts()
    {
        var a = new CType.Opaque("owner__Handle");
        var b = new CType.Opaque("other__Handle");
        ObjectAggregateMetadata.Describe(new CType.Pointer(a))
            .ShouldNotBe(ObjectAggregateMetadata.Describe(new CType.Pointer(b)));
        new CType.Pointer(a).SizeOf.ShouldBe(IntPtr.Size);
        Should.Throw<IrUnsupportedException>(() => a.SizeOf).Message.ShouldContain("no value or layout");
        Should.Throw<IrUnsupportedException>(() => IrBuilder.LayoutType(a)).Message.ShouldContain("no value or layout");
        var path = Path.Combine(_directory, "main.zig");
        File.WriteAllText(path, "const Handle = opaque {}; const Box = struct { pointer: *Handle }; pub fn main() u8 { const box = Box{ .pointer = @ptrFromInt(8) }; return if (@intFromPtr(box.pointer) == 8) 42 else 1; }");
        var emitted = Compiler.EmitObject(path, zigOptions: Options(path));
        emitted.ShouldNotContain("struct Handle");
        emitted.ShouldContain("void*");
    }
}
