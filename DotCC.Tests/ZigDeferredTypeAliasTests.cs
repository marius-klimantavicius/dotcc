using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigDeferredTypeAliasTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "zig-deferred-type-alias-" + Guid.NewGuid().ToString("N"));
    public ZigDeferredTypeAliasTests() => Directory.CreateDirectory(_directory);
    public void Dispose() => Directory.Delete(_directory, true);
    private string Source(string name, string source)
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, source);
        return path;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Premature_type_probe_does_not_consume_a_forward_type_call(bool objects)
    {
        Source("api.zig", """
            pub const Set = Make(Value);
            pub const Early = enum(u8) { selected = @sizeOf(Set) };
            pub const Value = struct { value: u8 };
            fn Make(comptime T: type) type { return struct { value: T }; }
            """);
        var root = Source("main.zig", "const api = @import(\"api.zig\"); pub fn main() u8 { return @intCast(@sizeOf(api.Set)); }");
        var emitted = objects ? Compiler.EmitObject(root) : Compiler.EmitCSharp(new[] { root });
        emitted.ShouldNotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("pub const Set = Make(Set); fn Make(comptime T: type) type { return T; }", "dependency loop")]
    [InlineData("pub const Set = Make(Missing); fn Make(comptime T: type) type { return T; }", "Missing")]
    [InlineData("pub const Set = Make(); pub const Early = enum(u8) { selected = @sizeOf(Set) }; fn Make() type { return Identity(struct { value: Missing }); } fn Identity(comptime T: type) type { return T; }", "Missing")]
    public void Invalid_and_recursive_type_aliases_fail_in_source_and_object_modes(string api, string diagnostic)
    {
        Source("api.zig", api);
        var root = Source("main.zig", "const api = @import(\"api.zig\"); pub fn main() u8 { return @intCast(@sizeOf(api.Set)); }");
        Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { root })).Message.ShouldContain(diagnostic);
        Should.Throw<CompileException>(() => Compiler.EmitObject(root)).Message.ShouldContain(diagnostic);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Anonymous_container_methods_keep_unused_bad_signatures_and_bodies_lazy(bool demand)
    {
        Source("api.zig", """
            fn Identity(comptime T: type) type { return T; }
            pub const Context = Identity(struct {
                pub fn answer() u8 { return 42; }
                pub fn bad_signature(_: Missing) u8 { return 0; }
                pub fn bad_body() u8 { @compileError("R41_DEMANDED_BODY"); }
            });
            """);
        var root = Source("main.zig", "const api = @import(\"api.zig\"); pub fn main() u8 { return api.Context."
            + (demand ? "bad_body" : "answer") + "(); }");
        if (demand)
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { root })).Message.ShouldContain("R41_DEMANDED_BODY");
            Should.Throw<CompileException>(() => Compiler.EmitObject(root)).Message.ShouldContain("R41_DEMANDED_BODY");
        }
        else
        {
            Compiler.EmitCSharp(new[] { root }).ShouldNotContain("R41_DEMANDED_BODY");
            Compiler.EmitObject(root).ShouldNotContain("R41_DEMANDED_BODY");
        }
    }

    [Fact]
    public void Demanded_bad_anonymous_method_signature_reports_the_original_type()
    {
        Source("api.zig", "fn Identity(comptime T: type) type { return T; } pub const C = Identity(struct { pub fn bad(_: Missing) u8 { return 0; } });");
        var root = Source("main.zig", "const api = @import(\"api.zig\"); pub fn main() u8 { return api.C.bad(undefined); }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(root)).Message.ShouldContain("Missing");
    }

    [Fact]
    public void Runtime_values_are_not_frozen_into_anonymous_method_captures()
    {
        var root = Source("main.zig", """
            fn Identity(comptime T: type) type { return T; }
            fn read(x: u8) u8 {
                const C = Identity(struct { pub fn answer() u8 { return x; } });
                return C.answer();
            }
            pub fn main() u8 { return read(42); }
            """);
        Should.Throw<CompileException>(() => Compiler.EmitObject(root)).Message.ShouldContain("x");
    }
}
