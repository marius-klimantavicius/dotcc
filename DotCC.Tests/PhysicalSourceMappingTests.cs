using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class PhysicalSourceMappingTests
{
    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Line_macro_tracks_both_sides_of_a_splice_and_following_lines(string newline)
    {
        WithSource("int first = __LINE__ + \\\n __LINE__;\nint last = __LINE__;\n".Replace("\n", newline), path =>
        {
            var text = Preprocess(path);
            text.ShouldContain("first = 1 + 2 ;");
            text.ShouldContain("last = 3 ;");
        });
    }

    [Fact]
    public void Continued_macro_definition_keeps_logical_adjacency_and_physical_line_numbers()
    {
        WithSource("#define ID\\\n(x) x\nint value = ID(__LINE__);\n", path =>
            Preprocess(path).ShouldContain("value = 3 ;"));
    }

    [Fact]
    public void Continued_line_directive_counts_physical_lines_from_directive_end()
    {
        WithSource("#line \\\n 100\nint first = __LINE__ + \\\n __LINE__;\nint last = __LINE__;\n", path =>
        {
            var text = Preprocess(path);
            text.ShouldContain("first = 100 + 101 ;");
            text.ShouldContain("last = 102 ;");
        });
    }

    [Fact]
    public void Header_and_translation_unit_keep_independent_physical_maps()
    {
        WithSource("#include \"continued.h\"\nint own = \\\n __LINE__;\n", path =>
        {
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(path)!, "continued.h"),
                "#define VALUE \\\n 42\nint header = __LINE__;\n");
            var text = Preprocess(path);
            text.ShouldContain("header = 3 ;");
            text.ShouldContain("own = 3 ;");
        });
    }

    [Fact]
    public void Macro_replacement_line_numbers_use_the_invocation()
    {
        WithSource("#define HERE __LINE__\n#define CALL() HERE\nint value = \\\n CALL();\n", path =>
            Preprocess(path).ShouldContain("value = 4 ;"));
    }

    [Fact]
    public void Lexer_diagnostic_retains_physical_utf8_byte_offset_and_column()
    {
        const string source = "/* λ */\nint x = \\\n @;\n";
        WithSource(source, path =>
        {
            var error = Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }));
            error.Message.ShouldContain("line 3, column 2");
            error.Message.ShouldContain("byte offset " + System.Text.Encoding.UTF8.GetByteCount(source[..source.IndexOf('@')]));
        });
    }

    [Fact]
    public void Parse_diagnostic_uses_physical_line_and_column_after_splice()
    {
        WithSource("int main(void) {\n return + \\\n  ;\n}\n", path =>
        {
            // A token on the continuation's second physical line must retain
            // that line and its original column in parser diagnostics.
            var error = Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }));
            error.Message.ShouldContain("line 3, column 3");
        });
    }

    private static string Preprocess(string path)
    {
        using var output = new StringWriter();
        Compiler.Preprocess(new[] { path }, output);
        return output.ToString();
    }

    private static void WithSource(string source, Action<string> action)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-source-map-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.c");
        File.WriteAllText(path, source);
        try { action(path); }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
