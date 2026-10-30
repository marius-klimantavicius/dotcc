using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

[Collection("ZigFrontend")]
public sealed class ZigRealStdLazyExecutionTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "zig-realstd-lazy-run-" + Guid.NewGuid().ToString("N"));
    public ZigRealStdLazyExecutionTests() => Directory.CreateDirectory(_directory);
    public void Dispose() => Directory.Delete(_directory, recursive: true);

    private string Source(string name, string source)
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, source);
        return path;
    }

    private void Run(string root, ZigCompilationOptions? options = null, int expectedExit = 42)
    {
        var emitted = Compiler.EmitCSharp(new[] { root }, emit: EmitMode.Csproj, zigOptions: options);
        var result = FixtureRunner.CompileAndRunCapturingStreams(emitted, Array.Empty<string>());
        result.exit.ShouldBe(expectedExit);
        result.stdout.ShouldBeEmpty();
        result.stderr.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dotted_enum_alias_preserves_its_value_at_a_typed_argument(bool typed)
    {
        Source("provider.zig", "pub const Arch = enum { other, selected }; pub const cpu = .{ .arch = "
            + (typed ? "@as(Arch, .selected)" : ".selected") + " };");
        Source("api.zig", """
            const provider = @import("provider.zig");
            const arch = provider.cpu.arch;
            fn choose(value: provider.Arch) u8 { return if (value == .selected) 42 else 1; }
            pub fn answer() u8 { return choose(arch); }
            """);
        Run(Source("main.zig", "const api = @import(\"api.zig\"); pub fn main() u8 { return api.answer(); }"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Forward_import_and_alias_are_visible_to_an_early_conditional_type(bool typed)
    {
        Source("provider.zig", """
            pub const Arch = enum { other, selected,
                pub fn chosen(value: @This()) bool { return value == .selected; }
            };
            pub const cpu = .{ .arch = @as(Arch, .selected) };
            """);
        Source("api.zig", """
            pub const Value = if (arch.chosen()) u8 else u16;
            """ + "\nconst arch" + (typed ? ": provider.Arch" : "") + " = provider.cpu.arch;\n" + """
            const provider = @import("provider.zig");
            pub fn answer() Value { return 42; }
            """);
        Run(Source("main.zig", "const api = @import(\"api.zig\"); pub fn main() u8 { return api.answer(); }"));
    }

    [Fact]
    public void Enum_alias_is_an_implicit_comptime_argument_to_a_type_returning_function()
    {
        Source("provider.zig", "pub const Arch = enum { other, selected }; pub const cpu = .{ .arch = @as(Arch, .selected) };");
        Source("api.zig", """
            const provider = @import("provider.zig");
            const arch = provider.cpu.arch;
            pub const Value = Choose(arch);
            fn Choose(value: provider.Arch) type { return if (value == .selected) u8 else u16; }
            pub fn answer() Value { return 42; }
            """);
        Run(Source("main.zig", "const api = @import(\"api.zig\"); pub fn main() u8 { return api.answer(); }"));
    }

    [Fact]
    public void Named_dependency_aliases_keep_their_module_owner_across_requests()
    {
        var root = Source("main.zig", "const api = @import(\"api\"); pub fn main() u8 { return api.answer(); }");
        var api = Source("api.zig", "const selected = config.value; const config = @import(\"config\"); pub fn answer() u8 { return selected; }");
        var expected = Source("expected.zig", "pub const value: u8 = 42;");
        var unrelated = Source("unrelated.zig", "pub const value: u8 = 7;");
        for (var request = 0; request < 2; request++)
        {
            var options = new ZigCompilationOptions("entry", new[] {
                new ZigModuleDefinition("entry", root, new[] { new ZigModuleDependency("api", "library"), new ZigModuleDependency("config", request == 0 ? "wrong" : "right") }),
                new ZigModuleDefinition("library", api, new[] { new ZigModuleDependency("config", request == 0 ? "right" : "wrong") }),
                new ZigModuleDefinition("right", expected, Array.Empty<ZigModuleDependency>()),
                new ZigModuleDefinition("wrong", unrelated, Array.Empty<ZigModuleDependency>()),
            });
            Run(root, options, expectedExit: request == 0 ? 42 : 7);
        }
    }
}
