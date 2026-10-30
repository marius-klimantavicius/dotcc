using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ZigModuleConfigurationTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "dotcc-module-options-" + Guid.NewGuid().ToString("N"));
    public ZigModuleConfigurationTests() => Directory.CreateDirectory(_directory);
    public void Dispose() => Directory.Delete(_directory, true);
    private string Source(string name, string source)
    {
        var path = Path.Combine(_directory, name);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, source);
        return path;
    }
    private ZigModuleDefinition Module(string name, string root, params ZigModuleDependency[] deps) => new(name, root, deps);

    [Fact]
    public void Configuration_copies_caller_owned_collections()
    {
        var dependencies = new List<ZigModuleDependency> { new("settings", "config") };
        var modules = new List<ZigModuleDefinition> { new("root", Path.Combine(_directory, "main.zig"), dependencies), Module("config", Path.Combine(_directory, "config.zig")) };
        var options = new ZigCompilationOptions("root", modules);
        dependencies.Clear(); modules.Clear();
        options.Modules.Count.ShouldBe(2);
        options.Modules[0].Dependencies.Single().Name.ShouldBe("settings");
    }

    [Theory]
    [InlineData("{\"schema_version\":2,\"root_module\":\"root\",\"modules\":[]}", "schema_version")]
    [InlineData("{\"schema_version\":1,\"root_module\":\"root\",\"modules\":[]}", "root module")]
    [InlineData("{\"schema_version\":1,\"root_module\":\"root\",\"modules\":[],\"target\":\"x86_64\"}", "unknown field")]
    [InlineData("{\"schema_version\":1,\"schema_version\":1,\"root_module\":\"root\",\"modules\":[]}", "duplicate field")]
    public void Strict_graph_rejects_unsupported_or_ambiguous_fields(string json, string diagnostic)
    {
        var path = Source("modules.json", json);
        Should.Throw<CompileException>(() => ZigCompilationOptions.Load(path)).Message.ShouldContain(diagnostic);
    }

    [Fact]
    public void Paths_are_relative_to_manifest_rather_than_process_directory()
    {
        var path = Source("module-graph.json", """
            {"schema_version":1,"root_module":"entry","zig_lib_dir":"toolchain/lib",
             "modules":[{"name":"entry","root":"sources/main.zig","dependencies":[]}]}
            """);
        var options = ZigCompilationOptions.Load(path);
        options.Modules.Single().Root.ShouldBe(Path.Combine(_directory, "sources", "main.zig"));
        options.ZigLibDir.ShouldBe(Path.Combine(_directory, "toolchain", "lib"));
    }

    [Fact]
    public void Missing_dependency_target_is_diagnosed_before_lowering()
    {
        Should.Throw<CompileException>(() => new ZigCompilationOptions("root", new[] {
            Module("root", "main.zig", new ZigModuleDependency("config", "missing"))
        })).Message.ShouldContain("unknown module 'missing'");
    }

    [Fact]
    public void Duplicate_aliases_are_rejected_in_their_own_scope()
    {
        Should.Throw<CompileException>(() => new ZigCompilationOptions("root", new[] {
            Module("root", "main.zig", new ZigModuleDependency("config", "root"), new ZigModuleDependency("config", "root"))
        })).Message.ShouldContain("duplicate dependency");
    }

    [Fact]
    public void A_file_cannot_be_the_root_of_two_modules_as_in_native_Zig()
    {
        Should.Throw<CompileException>(() => new ZigCompilationOptions("root", new[] {
            Module("root", "same.zig"), Module("second", "same.zig")
        })).Message.ShouldContain("root of more than one module");
    }

    [Fact]
    public void Named_import_and_relative_child_keep_module_dependency_scope()
    {
        var root = Source("main.zig", "const p = @import(\"package\"); pub fn main() u8 { return p.answer(); }");
        var package = Source("package/api.zig", "const child = @import(\"child.zig\"); pub fn answer() u8 { return child.result(); }");
        Source("package/child.zig", "const c = @import(\"config\"); pub fn result() u8 { return c.value; }");
        var config = Source("generated/settings.zig", "pub const value: u8 = 42;");
        var options = new ZigCompilationOptions("entry", new[] { Module("entry", root, new ZigModuleDependency("package", "library")),
            Module("library", package, new ZigModuleDependency("config", "settings")), Module("settings", config) });
        var emitted = Compiler.EmitCSharp(new[] { root }, zigOptions: options);
        emitted.ShouldContain("result");
        emitted.ShouldContain("42");
    }

    [Fact]
    public void Configured_root_import_exposes_actual_root_declarations()
    {
        var root = Source("main.zig", "const p = @import(\"package\"); pub const chosen: u8 = 42; pub fn main() u8 { return p.answer(); }");
        var package = Source("lib.zig", "const r = @import(\"root\"); pub fn answer() u8 { return r.chosen; }");
        var options = new ZigCompilationOptions("entry", new[] { Module("entry", root, new ZigModuleDependency("package", "library")), Module("library", package) });
        Compiler.EmitCSharp(new[] { root }, zigOptions: options).ShouldContain("42");
    }

    [Fact]
    public void Configuration_is_not_applied_to_a_different_root()
    {
        var root = Source("main.zig", "pub fn main() u8 { return 42; }");
        var options = new ZigCompilationOptions("entry", new[] { Module("entry", Path.Combine(_directory, "other.zig")) });
        Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { root }, zigOptions: options)).Message.ShouldContain("match the configured root");
    }
}
