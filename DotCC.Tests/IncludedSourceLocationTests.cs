using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class IncludedSourceLocationTests
{
    [Fact]
    public void Nested_include_parse_error_names_its_physical_source()
    {
        WithFiles("#include \"outer.h\"\n", "#include \"inner.h\"\n", "int value = \\\n ;\n", path =>
        {
            var error = Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }));
            error.Message.ShouldContain("parse failed in inner.h:");
            error.Message.ShouldContain("line 2, column 2");
        });
    }

    [Fact]
    public void Nested_include_lexer_error_names_its_physical_source()
    {
        WithFiles("#include \"outer.h\"\n", "#include \"inner.h\"\n", "int value = \\\n @;\n", path =>
        {
            var error = Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }));
            error.Message.ShouldContain("lex failed in inner.h:");
            error.Message.ShouldContain("line 2, column 2");
        });
    }

    [Fact]
    public void Included_semantic_error_keeps_origin_through_ast_reductions()
    {
        WithFiles("#include \"outer.h\"\n", "#include \"inner.h\"\n",
            "int helper(void) { const int value = 0;\n value = 1; return value; }\n", path =>
        {
            var error = Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }));
            error.Message.ShouldContain("inner.h:2:");
            error.Message.ShouldContain("read-only variable");
        });
    }

    [Fact]
    public void Macro_expansion_error_names_invocation_header()
    {
        WithFiles("#include \"outer.h\"\n", "#define BAD int value = ;\n#include \"inner.h\"\n", "\nBAD\n", path =>
        {
            var error = Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }));
            error.Message.ShouldContain("parse failed in inner.h:");
            error.Message.ShouldContain("line 2,");
        });
    }

    [Fact]
    public void Parent_tokens_resume_the_parent_filename_after_include()
    {
        WithFiles("#include \"outer.h\"\nint value = ;\n", "#include \"inner.h\"\n", "int other;\n", path =>
        {
            var error = Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }));
            error.Message.ShouldContain("parse failed in main.c:");
            error.Message.ShouldContain("line 2,");
        });
    }

    private static void WithFiles(string source, string outer, string inner, Action<string> action)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-include-source-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "main.c");
        File.WriteAllText(path, source);
        File.WriteAllText(Path.Combine(directory, "outer.h"), outer);
        File.WriteAllText(Path.Combine(directory, "inner.h"), inner);
        try { action(path); }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
