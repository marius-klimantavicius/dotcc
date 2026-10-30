using System;
using System.IO;
using System.Linq;
using DotCC.Frontends;
using DotCC.Ir;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigImportedExportTests
{
    [Theory]
    [InlineData("comptime { _ = @import(\"api.zig\"); }")]
    [InlineData("const api = @import(\"api.zig\"); comptime { _ = api; }")]
    [InlineData("const api = @import(\"api.zig\"); export fn answer() u32 { return api.marker; }")]
    public void Analyzed_import_roots_uncalled_export_body_and_rejects_poison(string root)
    {
        WithSource(root, "pub const marker: u32 = 42; export fn required() u32 { @compileError(\"REQUIRED_IMPORTED_EXPORT\"); }", path =>
        {
            foreach (var mode in new[] { ZigSourceOutputMode.Lib, ZigSourceOutputMode.Exe })
            {
                var options = Options(path, mode);
                Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: options)).Message.ShouldContain("REQUIRED_IMPORTED_EXPORT");
                Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, zigOptions: options))
                    .Message.ShouldContain("REQUIRED_IMPORTED_EXPORT");
            }
        });
    }

    [Theory]
    [InlineData("const unused = @import(\"api.zig\");")]
    [InlineData("comptime { if (false) { _ = @import(\"api.zig\"); } }")]
    public void Unforced_import_does_not_root_explicit_exports(string root)
    {
        WithSource(root + " export fn answer() u32 { return 42; }",
            "export fn required() u32 { @compileError(\"UNFORCED_EXPORT\"); }", path =>
            Compiler.EmitObject(path, zigOptions: Options(path)).ShouldContain("answer"));
    }

    [Fact]
    public void Analyzed_import_keeps_public_nonexport_body_lazy()
    {
        WithSource("comptime { _ = @import(\"api.zig\"); }", """
            pub fn unused() u32 { @compileError("UNUSED_PUBLIC_BODY"); }
            export fn required() u32 { return 42; }
            """, path => Compiler.EmitObject(path, zigOptions: Options(path)).ShouldContain("required"));
    }

    [Fact]
    public void Export_signature_is_analyzed_even_without_a_call()
    {
        WithSource("comptime { _ = @import(\"api.zig\"); }", """
            const Bad = @compileError("REQUIRED_EXPORT_SIGNATURE");
            export fn required(value: Bad) void { _ = value; }
            """, path => Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: Options(path)))
                .Message.ShouldContain("REQUIRED_EXPORT_SIGNATURE"));
    }

    [Fact]
    public void Explicit_export_uses_the_same_abi_type_boundary_as_export_builtin()
    {
        foreach (var declaration in new[]
        {
            "export fn required(value: []const u8) void { _ = value; }",
            "export fn required() []const u8 { return \"x\"; }",
            "const Auto = struct { value: u32 }; export fn required(value: Auto) Auto { return value; }",
        })
            WithSource("comptime { _ = @import(\"api.zig\"); }", declaration, path =>
            {
                Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: Options(path)))
                    .Message.ShouldContain("unsupported ABI parameter or return type");
                Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, zigOptions: Options(path)))
                    .Message.ShouldContain("unsupported ABI parameter or return type");
            });
    }

    [Fact]
    public void Transitive_analyzed_import_roots_its_explicit_export()
    {
        WithSource("comptime { _ = @import(\"api.zig\"); }", "comptime { _ = @import(\"leaf.zig\"); }", path =>
        {
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(path)!, "leaf.zig"),
                "export fn leaf() u32 { @compileError(\"TRANSITIVE_EXPORT\"); }");
            Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: Options(path))).Message.ShouldContain("TRANSITIVE_EXPORT");
        });
    }

    [Fact]
    public void Export_alias_and_repeated_import_share_one_canonical_definition()
    {
        WithSource("comptime { _ = @import(\"api.zig\"); _ = @import(\"./api.zig\"); }", """
            pub const alias = required;
            comptime { @export(&alias, .{ .name = "required_alias" }); }
            export fn required(value: c_int) c_int { return value + 2; }
            """, path =>
        {
            var ir = new ZigFrontend().BuildIr(new FrontendRequest(new[] { path }, ZigOptions: Options(path)));
            var definition = ir.Functions.Single(function => function.Sym.TargetName == "required");
            ir.Functions.Count(function => function.Sym.Name == "required").ShouldBe(1);
            var alias = ir.Functions.Single(function => function.Sym.TargetName == "required_alias");
            var call = alias.Body.Stmts.OfType<Return>().Single().Value.ShouldBeOfType<Call>();
            call.CalleeSym.ShouldBeSameAs(definition.Sym);
            definition.Sym.Storage.ShouldBe(Storage.None);
        });
    }

    [Fact]
    public void Duplicate_export_names_in_analyzed_modules_are_rejected()
    {
        WithSource("comptime { _ = @import(\"api.zig\"); _ = @import(\"other.zig\"); }",
            "export fn required() u32 { return 42; }", path =>
        {
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(path)!, "other.zig"), "export fn required() u32 { return 7; }");
            Should.Throw<CompileException>(() => Compiler.EmitObject(path, zigOptions: Options(path))).Message.ShouldContain("exported symbol collision");
        });
    }

    private static ZigCompilationOptions Options(string path, ZigSourceOutputMode mode = ZigSourceOutputMode.Lib)
        => new("root", new[] { new ZigModuleDefinition("root", path, Array.Empty<ZigModuleDependency>()) }, outputMode: mode);

    private static void WithSource(string root, string api, Action<string> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "zig-imported-export-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.zig");
            File.WriteAllText(path, root);
            File.WriteAllText(Path.Combine(directory, "api.zig"), api);
            test(path);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
