using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class RelativeIncludeTests
{
    [Fact]
    public void Parent_relative_c_splice_resolves_its_own_local_headers()
    {
        WithTree(root =>
        {
            File.WriteAllText(Path.Combine(root, "lib", "main.c"), "#include \"../impl/body.c\"\n");
            File.WriteAllText(Path.Combine(root, "impl", "body.c"), "#include \"value.h\"\nint value(void) { return VALUE; }\n");
            File.WriteAllText(Path.Combine(root, "impl", "value.h"), "#define VALUE 42\n");
            var path = Path.Combine(root, "lib", "main.c");
            Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib).ShouldContain("return 42;");
            var dependencies = Compiler.EmitDependencyRule(path, ["main.o"], includeSystemHeaders: true);
            dependencies.ShouldContain(Path.Combine(root, "impl", "body.c"));
            dependencies.ShouldContain(Path.Combine(root, "impl", "value.h"));
        });
    }

    [Fact]
    public void Quoted_include_prefers_including_directory_and_has_include_uses_same_search()
    {
        WithTree(root =>
        {
            File.WriteAllText(Path.Combine(root, "lib", "value.h"), "#define WRONG 1\n");
            File.WriteAllText(Path.Combine(root, "lib", "main.c"), "#include \"../impl/body.c\"\n");
            File.WriteAllText(Path.Combine(root, "impl", "body.c"), """
                #line 100 "virtual.c"
                #if !__has_include("value.h") || !__has_include("../impl/value.h")
                #error relative header missing
                #endif
                #include "value.h"
                #ifdef WRONG
                #error wrong directory
                #endif
                int value(void) { return VALUE; }
                """);
            File.WriteAllText(Path.Combine(root, "impl", "value.h"), "#define VALUE 23\n");
            Compiler.EmitCSharp([Path.Combine(root, "lib", "main.c")], emit: EmitMode.ManagedLib).ShouldContain("return 23;");
        });
    }

    [Fact]
    public void Pragma_once_deduplicates_different_spellings_of_one_physical_header()
    {
        WithTree(root =>
        {
            File.WriteAllText(Path.Combine(root, "lib", "main.c"), "#include \"../impl/value.h\"\n#include \"../impl/../impl/value.h\"\nint value(void) { return seen; }\n");
            File.WriteAllText(Path.Combine(root, "impl", "value.h"), "#pragma once\nint seen;\n");
            using var output = new StringWriter();
            Compiler.Preprocess([Path.Combine(root, "lib", "main.c")], output);
            output.ToString().Split("seen", StringSplitOptions.None).Length.ShouldBe(3);
        });
    }

    private static void WithTree(Action<string> action)
    {
        var root = Path.Combine(Path.GetTempPath(), "dotcc-relative-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "lib"));
        Directory.CreateDirectory(Path.Combine(root, "impl"));
        try { action(root); }
        finally { Directory.Delete(root, recursive: true); }
    }
}
