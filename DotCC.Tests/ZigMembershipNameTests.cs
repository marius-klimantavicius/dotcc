using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigMembershipNameTests
{
    [Theory]
    [InlineData("@hasDecl(Target, name)")]
    [InlineData("@hasField(Target, name)")]
    [InlineData("@field(Target{ .value = 42 }, name) == 42")]
    public void Runtime_name_parameters_never_use_a_previous_functions_string_binding(string query)
    {
        var source = "const Target = struct { value: u8, pub const value_decl: u8 = 42; }; "
            + "fn first() bool { const name = \"value\"; return @hasField(Target, name); } "
            + "fn query(name: []const u8) bool { return " + query + "; } "
            + "pub fn main() u8 { return if (first() and query(\"value\")) 42 else 1; }";
        WithSource(source, path =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("name must be a comptime string"));
    }

    [Theory]
    [InlineData("fn pick(name: *const [5]u8) bool { return @hasField(Target, name); } pub fn main() u8 { return if (pick(\"value\")) 42 else 1; }")]
    [InlineData("var calls: u8 = 0; fn name() []const u8 { calls += 1; return \"value\"; } pub fn main() u8 { return if (@hasField(Target, name())) 42 else 1; }")]
    [InlineData("pub fn main() u8 { return if (@hasField(Target, [_]u16{118, 97, 108, 117, 101})) 42 else 1; }")]
    public void Runtime_pointers_effectful_calls_and_nonbyte_arrays_are_rejected(string body)
        => WithSource("const Target = struct { value: u8, }; " + body, path =>
            Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("name must be a comptime string"));

    [Fact]
    public void A_file_computed_name_cannot_capture_the_demanding_functions_parameter()
        => WithSource("const Target = struct { value: u8, }; const selected = if (choose) \"value\" else \"missing\"; "
            + "fn query(choose: bool) bool { return @hasField(Target, selected); } pub fn main() u8 { return if (query(true)) 42 else 1; }",
            path => Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("unresolved identifier 'choose'"));

    [Fact]
    public void A_reached_poisoned_name_remains_an_error()
        => WithSource("const Target = struct { value: u8, }; const selected = @compileError(\"R36_REACHED_NAME_POISON\"); "
            + "pub fn main() u8 { return if (@hasField(Target, selected)) 42 else 1; }",
            path => Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("R36_REACHED_NAME_POISON"));

    [Theory]
    [InlineData("\"caf\\xc3\\xa9\"")]
    [InlineData("\"caf\\u{e9}\"")]
    [InlineData("[_]u8{ 99, 97, 102, 195, 169 }")]
    [InlineData("\"xcaf\\xc3\\xa9x\"[1..6]")]
    public void Unknown_names_report_the_decoded_utf8_source_spelling(string name)
        => WithSource("const Target = struct { value: u8, }; pub fn main() u8 { return @field(Target{ .value = 42 }, "
            + name + "); }", path => Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("has no field 'café'"));

    [Fact]
    public void A_computed_name_dependency_cycle_is_bounded()
        => WithSource("const provider = @import(\"provider.zig\"); const Target = struct { value: u8, }; "
            + "pub fn main() u8 { return if (@hasField(Target, provider.a)) 42 else 1; }", path => {
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(path)!, "provider.zig"),
                    "pub const a = if (false) \"other\" else b; pub const b = a;");
                Should.Throw<CompileException>(() => Compiler.EmitObject(path)).Message.ShouldContain("dependency loop");
            });

    private static void WithSource(string source, Action<string> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "zig-membership-name-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.zig");
        File.WriteAllText(path, source);
        try { test(path); }
        finally { Directory.Delete(directory, true); }
    }
}
