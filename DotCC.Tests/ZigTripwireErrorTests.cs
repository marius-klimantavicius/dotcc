using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigTripwireErrorTests
{
    private static string Compile(string source)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-tripwire-reject-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(directory, "lib", "std"));
        File.WriteAllText(Path.Combine(directory, "lib", "std", "std.zig"), "");
        var path = Path.Combine(directory, "root.zig");
        File.WriteAllText(path, source);
        try
        {
            var options = new ZigCompilationOptions("root", [new("root", path, [])], outputMode: ZigSourceOutputMode.Lib,
                optimizeMode: ZigSourceOptimizeMode.Fast, zigLibDir: Path.Combine(directory, "lib"),
                stdlibTypes: ZigStdlibTypeModel.Source, functionPointerAbi: ZigFunctionPointerAbi.Managed,
                managedRoots: ["answer"]);
            return Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib, zigOptions: options);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("error{Allowed}")]
    [InlineData("error{Allowed}!u8")]
    [InlineData("seed")]
    public void Reflected_finite_error_sets_reject_foreign_source_errors(string argument)
    {
        var source = """
            fn module(comptime E: anytype) type {
                return struct {
                    pub const Error = result: {
                        const T = if (@TypeOf(E) == type) E else @TypeOf(E);
                        break :result switch (@typeInfo(T)) {
                            .error_set => E,
                            .error_union => |info| info.error_set,
                            .@"fn" => |info| @typeInfo(info.return_type.?).error_union.error_set,
                            else => @compileError("not an error seed"),
                        };
                    };
                };
            }
            fn seed() error{Allowed}!u8 { return 42; }
            const A = module(error{Other});
            fn keepFirst(_: A.Error) void {}
            const B = module(ARGUMENT);
            fn bad() B.Error!u8 { return error.Foreign; }
            comptime { @export(&answer, .{ .name = "answer" }); }
            pub fn answer() callconv(.c) u8 {
                keepFirst(error.Other);
                return bad() catch 42;
            }
            """.Replace("ARGUMENT", argument, StringComparison.Ordinal);
        Should.Throw<CompileException>(() => Compile(source)).Message.ShouldContain("error 'Foreign' is not a member");
    }

    [Fact]
    public void Nonvoid_catch_blocks_still_require_a_result()
        => Should.Throw<CompileException>(() => Compile("""
            fn attempt() error{Denied}!u8 { return error.Denied; }
            comptime { @export(&answer, .{ .name = "answer" }); }
            pub fn answer() callconv(.c) u8 { const n = attempt() catch {}; return n; }
            """)).Message.ShouldContain("not fall through");

    [Fact]
    public void Concatenation_cannot_fold_a_runtime_global_effect_into_a_literal()
        => Should.Throw<CompileException>(() => Compile("""
            var effects: u8 = 0;
            fn part() []const u8 { effects += 1; return "suffix"; }
            comptime { @export(&answer, .{ .name = "answer" }); }
            pub fn answer() callconv(.c) u8 { const bytes = "prefix" ++ part(); return @intCast(bytes.len); }
            """)).Message.ShouldContain("zig `a ++ b`");

    [Fact]
    public void Demanded_void_reference_capture_still_rejects_missing_storage()
        => Should.Throw<CompileException>(() => Compile("""
            const Command = union(enum) { absent: void, byte: u8 };
            fn inspect(pointer: *void) bool { return @intFromPtr(pointer) != 0; }
            fn visit(command: *Command) u8 {
                switch (command.*) {
                    .absent => |*payload| return if (inspect(payload)) 42 else 1,
                    .byte => return 2,
                }
            }
            comptime { @export(&answer, .{ .name = "answer" }); }
            pub fn answer() callconv(.c) u8 { var c: Command = .{ .absent = {} }; return visit(&c); }
            """)).Message.ShouldContain("runtime use of by-reference void union capture");
}
