using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class ForInitializerCompoundExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void For_initializer_compound_storage_survives_body_and_post(bool objects)
    {
        const string source = """
            #include <stdio.h>
            struct Box { int value; };
            static struct Box *retained;
            static int effects;
            static void keep(struct Box *box) { retained = box; effects++; }
            int main(void) {
                int total = 0;
                for (int scope = (keep(&(struct Box){40}), 0); scope < 3; scope++, retained->value++) {
                    if (scope == 1) continue;
                    total += retained->value;
                }
                for (int scope = (keep(&(struct Box){7}), 0); scope < 1; scope++) total += retained->value;
                printf("%d %d\n", total, effects);
                return total != 89 || effects != 2;
            }
            """;
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-for-compound-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "main.c");
            File.WriteAllText(path, source);
            string emitted;
            if (objects)
            {
                string fragment = Path.Combine(directory, "main.cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects(new[] { fragment });
            }
            else emitted = Compiler.EmitCSharp(new[] { path });
            var result = FixtureRunner.CompileAndRunCapturingExit(emitted, Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.Trim().ShouldBe("89 2");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
