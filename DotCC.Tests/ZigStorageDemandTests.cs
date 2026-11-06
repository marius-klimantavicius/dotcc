using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigStorageDemandTests
{
    [Fact]
    public void Demanded_invalid_generic_type_const_keeps_its_source_error()
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-type-const-demand-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "root.zig");
            File.WriteAllText(path, """
                fn Box(comptime T: type) type {
                    return struct { value: T, pub const Bit = @Int(.unsigned, @typeInfo(T).int.bits); };
                }
                pub fn main() u8 { return @typeInfo(Box(*u8).Bit).int.bits; }
                """);
            var options = new ZigCompilationOptions("root", [new("root", path, [])],
                zigLibDir: EmptyStdlib(directory), stdlibTypes: ZigStdlibTypeModel.Source);
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path], zigOptions: options))
                .Message.ShouldContain("active `std.builtin.Type` field is `pointer`");
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Imported_unsupported_storage_is_validated_when_its_holder_is_emitted(bool demanded)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-storage-demand-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "dep.zig"), """
                pub const Tail = extern struct { value: u8, tail: [0]u8 };
                pub const Holder = extern struct { entry: Tail };
                pub fn answer() u8 { return 42; }
                """);
            var path = Path.Combine(directory, "root.zig");
            File.WriteAllText(path, "const dep = @import(\"dep.zig\"); pub fn main() u8 { return dep.answer(); }"
                + (demanded ? " pub fn read(value: dep.Holder) u8 { return value.entry.value; }" : ""));
            var options = new ZigCompilationOptions("root", [new("root", path, [])],
                zigLibDir: EmptyStdlib(directory), stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed);
            string Emit() => Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
            if (demanded)
                Should.Throw<DotCC.Layout.OffsetLayoutException>(() => Emit()).Message.ShouldContain("zero-length field");
            else
                Emit().ShouldNotContain("struct dep__Holder");
        }
        finally { Directory.Delete(directory, true); }
    }

    private static string EmptyStdlib(string directory)
    {
        var library = Path.Combine(directory, "lib");
        Directory.CreateDirectory(Path.Combine(library, "std"));
        File.WriteAllText(Path.Combine(library, "std", "std.zig"), "");
        return library;
    }
}
