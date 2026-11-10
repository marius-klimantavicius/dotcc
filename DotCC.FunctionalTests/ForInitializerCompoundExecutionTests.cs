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
    public void For_initializer_comma_with_addressed_locals_preserves_scope_order_and_continue(bool objects)
    {
        const string source = """
            #include <stdio.h>
            static int effects;
            static void bounds(int *first, int *last) { *first = 2; *last = 5; effects++; }
            int main(void) {
                int first, last, total = 0, posts = 0;
                for (int scope = (bounds(&first, &last), first), end = scope + 3; scope < end; scope++, posts++) {
                    if (scope == 3) continue;
                    total += scope;
                }
                for (int scope = (bounds(&first, &last), first); scope < last; scope++, posts++) total += scope;
                printf("%d %d %d\n", total, effects, posts);
                return total != 15 || effects != 2 || posts != 6;
            }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-for-comma-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "main.c");
            File.WriteAllText(path, source);
            string emitted;
            if (objects)
            {
                var fragment = Path.ChangeExtension(path, ".cs");
                File.WriteAllText(fragment, Compiler.EmitObject(path));
                emitted = Compiler.LinkObjects([fragment]);
            }
            else emitted = Compiler.EmitCSharp([path]);
            var result = FixtureRunner.CompileAndRunCapturingExit(emitted, []);
            result.exit.ShouldBe(0);
            result.stdout.Trim().ShouldBe("15 2 6");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

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
