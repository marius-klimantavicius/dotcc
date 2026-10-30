using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

[Collection("ZigFrontend")]
public sealed class ZigRealStdLazyTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "zig-realstd-lazy-" + Guid.NewGuid().ToString("N"));
    public ZigRealStdLazyTests() => Directory.CreateDirectory(_directory);
    public void Dispose() => Directory.Delete(_directory, recursive: true);

    private string Source(string name, string source)
    {
        var path = Path.Combine(_directory, name);
        File.WriteAllText(path, source);
        return path;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Imported_dotted_value_is_available_in_object_mode(bool forwardImport)
    {
        Source("provider.zig", "pub const answer: u8 = 42;");
        var import = "const provider = @import(\"provider.zig\");\n";
        var alias = "const selected = provider.answer;\n";
        Source("api.zig", (forwardImport ? alias + import : import + alias)
            + "pub fn answer() u8 { return selected; }");
        var root = Source("main.zig", "const api = @import(\"api.zig\"); pub fn main() u8 { return api.answer(); }");
        Compiler.EmitObject(root).ShouldContain("42");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Deprecation_is_an_error_only_when_the_alias_is_read(bool read)
    {
        Source("provider.zig", "pub const old = @compileError(\"R33_DEPRECATION\"); pub const answer: u8 = 42;");
        Source("api.zig", "const provider = @import(\"provider.zig\"); const selected = provider.old; "
            + "pub fn answer() u8 { return " + (read ? "selected" : "42") + "; }");
        var root = Source("main.zig", "const api = @import(\"api.zig\"); pub fn main() u8 { return api.answer(); }");
        if (read) Should.Throw<CompileException>(() => Compiler.EmitObject(root)).Message.ShouldContain("R33_DEPRECATION");
        else Compiler.EmitObject(root).ShouldContain("42");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Unused_import_stays_lazy_and_a_reached_module_runs_top_comptime(bool reached)
    {
        Source("provider.zig", "comptime { @compileError(\"R33_REACHED_MODULE\"); } pub const answer: u8 = 42;");
        var root = Source("main.zig", "const provider = @import(\"provider.zig\"); pub fn main() u8 { return "
            + (reached ? "provider.answer" : "42") + "; }");
        if (reached) Should.Throw<CompileException>(() => Compiler.EmitObject(root)).Message.ShouldContain("R33_REACHED_MODULE");
        else Compiler.EmitObject(root).ShouldContain("42");
    }

    [Fact]
    public void Alias_dependency_cycle_has_a_bounded_diagnostic()
    {
        Source("api.zig", "const first = second; const second = first; pub fn answer() u8 { return first; }");
        var root = Source("main.zig", "const api = @import(\"api.zig\"); pub fn main() u8 { return api.answer(); }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(root)).Message.ShouldContain("dependency loop");
    }

    [Fact]
    public void A_module_initializer_cannot_capture_a_function_parameter()
    {
        Source("api.zig", "const selected = missing; pub fn answer(missing: u8) u8 { return selected; }");
        var root = Source("main.zig", "const api = @import(\"api.zig\"); pub fn main() u8 { return api.answer(42); }");
        Should.Throw<CompileException>(() => Compiler.EmitObject(root)).Message.ShouldContain("unresolved identifier 'missing'");
    }
}
