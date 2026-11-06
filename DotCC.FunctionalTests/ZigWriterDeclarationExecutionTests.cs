using System;
using System.IO;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigWriterDeclarationExecutionTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "dotcc-writer-declaration-" + Guid.NewGuid().ToString("N"));
    public ZigWriterDeclarationExecutionTests() => Directory.CreateDirectory(_directory);
    public void Dispose() => Directory.Delete(_directory, true);
    private static string Fixture(string name, [CallerFilePath] string caller = "")
        => Path.Combine(Path.GetDirectoryName(caller)!, "Fixtures", "zig-writer-declaration", name);
    private string Source(string name, string source)
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, source);
        return path;
    }
    private string EmptyLibrary()
    {
        Directory.CreateDirectory(Path.Combine(_directory, "lib", "std"));
        File.WriteAllText(Path.Combine(_directory, "lib", "std", "std.zig"), "");
        return Path.Combine(_directory, "lib");
    }
    private string Emit(string path, string library, string root, bool objects)
    {
        var options = new ZigCompilationOptions("root", [new("root", path, [])], outputMode: ZigSourceOutputMode.Lib,
            optimizeMode: ZigSourceOptimizeMode.Fast, zigLibDir: library, stdlibTypes: ZigStdlibTypeModel.Source,
            functionPointerAbi: ZigFunctionPointerAbi.Managed, managedRoots: [root]);
        if (!objects) return Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
        var fragment = Path.Combine(_directory, "writer.csobj");
        File.WriteAllText(fragment, Compiler.EmitObject(path, zigOptions: options));
        return Compiler.LinkObjects([fragment], emit: EmitMode.ManagedLib);
    }
    private static void Run(string emitted, string host)
    {
        var result = FixtureRunner.CompileAndRunCapturingStreams(emitted + host, [], TestContext.Current.CancellationToken);
        result.exit.ShouldBe(42);
        result.stdout.ShouldBeEmpty();
        result.stderr.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Actual_Writer_formats_reflected_narrow_integer_fields(bool objects)
    {
        var library = Environment.GetEnvironmentVariable("DOTCC_TARGET_ZIG_LIB");
        if (string.IsNullOrEmpty(library)) Assert.Skip("Set DOTCC_TARGET_ZIG_LIB to the pinned source standard library.");
        Run(Emit(Fixture("writer.zig"), library, "formatAnswer", objects),
            "\npublic static class WriterHost { public static int Main() => DotCcLib.formatAnswer(); }");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Void_optional_preserves_presence_effects_and_physical_extent(bool objects)
        => Run(Emit(Fixture("optional-void.zig"), EmptyLibrary(), "answer", objects), """

            public static unsafe class OptionalHost {
                public static int Main() {
                    if (sizeof(ZigOptionalVoid) != 1) return 90;
                    ZigOptionalVoid* slots = stackalloc ZigOptionalVoid[3];
                    if ((byte*)&slots[2] - (byte*)&slots[0] != 2) return 91;
                    return DotCcLib.answer();
                }
            }
            """);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Aligned_optional_preserves_source_discriminant_offsets_stride_and_effects(bool objects)
        => Run(Emit(Fixture("aligned-optional.zig"), EmptyLibrary(), "answer", objects),
            "\npublic static class AlignedHost { public static int Main() => DotCcLib.answer(); }");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Optional_error_preserves_the_source_null_niche_and_tuple_extent(bool objects)
        => Run(Emit(Fixture("optional-error.zig"), EmptyLibrary(), "answer", objects),
            "\npublic static unsafe class ErrorHost { public static int Main() { if (sizeof(ZigOptionalError) != 2) return 90; return DotCcLib.answer(); } }");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Callback_error_union_keeps_its_inline_array_payload(bool objects)
        => Run(Emit(Fixture("array-error.zig"), EmptyLibrary(), "answer", objects),
            "\npublic static class ArrayHost { public static int Main() => DotCcLib.answer(); }");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Callback_pointee_demands_its_real_file_declaration(bool objects)
    {
        Source("dep.zig", "value: u8, next: ?*@This(),");
        var path = Source("root.zig", """
            const dep = @import("dep.zig");
            fn read(pointer: *dep) u8 { _ = pointer; return 42; }
            comptime { @export(&answer, .{ .name = "answer" }); }
            pub fn answer() callconv(.c) u8 {
                const callback: *const fn (*dep) u8 = read;
                return callback(@ptrFromInt(8));
            }
            """);
        var emitted = Emit(path, EmptyLibrary(), "answer", objects);
        emitted.ShouldContain("public byte value;");
        emitted.ShouldContain(" next;");
        Run(emitted, "\npublic static class CallbackHost { public static int Main() => DotCcLib.answer(); }");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Unused_file_namespace_fields_remain_lazy(bool objects)
    {
        Source("dep.zig", "value: @compileError(\"instance layout demanded\"), pub fn answer() u8 { return 42; }");
        var path = Source("root.zig", """
            const dep = @import("dep.zig");
            comptime { @export(&answer, .{ .name = "answer" }); }
            pub fn answer() callconv(.c) u8 { return dep.answer(); }
            """);
        Run(Emit(path, EmptyLibrary(), "answer", objects),
            "\npublic static class NamespaceHost { public static int Main() => DotCcLib.answer(); }");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Demanded_invalid_file_layout_keeps_its_source_error(bool objects)
    {
        Source("dep.zig", "value: @compileError(\"instance layout demanded\"),");
        var path = Source("root.zig", """
            const dep = @import("dep.zig");
            comptime { @export(&answer, .{ .name = "answer" }); }
            pub fn answer() callconv(.c) u8 { return @intCast(@sizeOf(dep)); }
            """);
        Should.Throw<CompileException>(() => Emit(path, EmptyLibrary(), "answer", objects))
            .Message.ShouldContain("instance layout demanded");
    }
}
