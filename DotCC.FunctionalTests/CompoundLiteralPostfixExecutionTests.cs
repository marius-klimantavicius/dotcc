using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class CompoundLiteralPostfixExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Compound_literals_support_direct_member_and_subscript_chains(bool objects)
    {
        const string source = """
            #include <stdio.h>
            struct Row { unsigned long array[3]; };
            struct Scalar { int value; };
            int main(void) {
                int index = 1;
                unsigned long value = (struct Row){{40, 42, 44}}.array[index];
                int named = (struct Scalar){.value = 7}.value;
                printf("postfix %lu %d\n", value, named);
                return 0;
            }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-compound-postfix-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.c");
            File.WriteAllText(path, source);
            string emitted;
            if (objects)
            {
                var fragment = Path.Combine(directory, "main.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects(new[] { fragment });
            }
            else emitted = Compiler.EmitCSharp(new[] { path });
            var result = FixtureRunner.CompileAndRunCapturingExit(emitted, Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.Trim().ShouldBe("postfix 42 7");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
