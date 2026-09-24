using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class IncludeSearchOrderTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Angle_and_macro_angle_use_first_I_directory_while_quotes_use_local(bool macro)
    {
        WithTree((root, first, second) =>
        {
            File.WriteAllText(Path.Combine(root, "value.h"), "#define LOCAL 7\n");
            File.WriteAllText(Path.Combine(first, "value.h"), "#define CHOSEN 42\n");
            File.WriteAllText(Path.Combine(second, "value.h"), "#error wrong include priority\n");
            var source = Path.Combine(root, "main.c");
            File.WriteAllText(source, "#include \"value.h\"\n" +
                (macro ? "#define HEADER <value.h>\n#include HEADER\n" : "#include <value.h>\n") +
                "int main(void) { return CHOSEN - LOCAL; }\n");
            Compiler.EmitCSharp([source], includeDirs: [first, second]).ShouldContain("return 42 - 7;");
            var dependencies = Compiler.EmitDependencyRule(source, ["main.o"], true, [first, second]);
            dependencies.ShouldContain(Path.Combine(root, "value.h"));
            dependencies.ShouldContain(Path.Combine(first, "value.h"));
            dependencies.ShouldNotContain(Path.Combine(second, "value.h"));
        });
    }

    [Fact]
    public void Has_include_does_not_search_source_directory_for_angles()
    {
        WithTree((root, first, second) =>
        {
            File.WriteAllText(Path.Combine(root, "only-local.h"), "#define LOCAL 7\n");
            var source = Path.Combine(root, "main.c");
            File.WriteAllText(source, """
                #if __has_include(<only-local.h>) || !__has_include("only-local.h")
                #error incorrect header search
                #endif
                int main(void) { return 0; }
                """);
            Compiler.EmitCSharp([source], includeDirs: [first, second]).ShouldContain("return 0;");
        });
    }

    [Fact]
    public void Input_directory_does_not_shadow_embedded_angle_header()
    {
        WithTree((root, first, second) =>
        {
            File.WriteAllText(Path.Combine(root, "stddef.h"), "#error local shadow\n");
            var source = Path.Combine(root, "main.c");
            File.WriteAllText(source, "#include <stddef.h>\nint main(void) { return sizeof(size_t); }\n");
            Compiler.EmitCSharp([source]).ShouldContain("8");
        });
    }

    private static void WithTree(Action<string, string, string> action)
    {
        var root = Path.Combine(Path.GetTempPath(), "dotcc-search-" + Guid.NewGuid().ToString("N"));
        var first = Path.Combine(root, "first");
        var second = Path.Combine(root, "second");
        Directory.CreateDirectory(first);
        Directory.CreateDirectory(second);
        try { action(root, first, second); }
        finally { Directory.Delete(root, true); }
    }
}
