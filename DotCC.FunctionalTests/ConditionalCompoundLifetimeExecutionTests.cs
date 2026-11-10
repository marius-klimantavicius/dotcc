using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class ConditionalCompoundLifetimeExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Conditional_addressed_records_keep_lazy_initialization_and_pointer_storage(bool objects)
    {
        const string source = """
            #include <stdio.h>
            struct Box { int value; };
            static int effects;
            static int mark(int value) { effects++; return value; }
            static int read(struct Box *value) { return value->value; }
            int main(void) {
                int yes = 1;
                int no = 0;
                int value = yes ? read(&(struct Box){mark(40)}) : read(&(struct Box){mark(-1)});
                struct Box *retained = no ? &(struct Box){mark(-2)} : &(struct Box){mark(2)};
                int other = no ? read(&(struct Box){mark(-3)}) : read(&(struct Box){mark(7)});
                int skipped = no && read(&(struct Box){mark(-4)});
                retained->value += value;
                printf("%d %d %d %d %d\n", value, retained->value, other, effects, skipped);
                return value != 40 || retained->value != 42 || other != 7 || effects != 3 || skipped;
            }
            """;
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-conditional-record-" + Guid.NewGuid().ToString("N"));
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
            result.stdout.Trim().ShouldBe("40 42 7 3 0");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Conditional_compound_arrays_keep_effects_and_enclosing_block_storage(bool objects)
    {
        const string source = """
            #include <stdio.h>
            struct Row { int array[2]; };
            static int effects;
            static int mark(int value) { effects += 1; return value; }
            int main(void) {
                int flag = 1;
                int no = 0;
                int selected;
                int initial_effects;
                int *retained;
                int *current;
                int *first = 0;
                int iteration = 0;
                int skipped_and;
                int skipped_or;
                selected = flag
                    ? (struct Row){{mark(40), mark(2)}}.array[0]
                    : (struct Row){{mark(-1), mark(-2)}}.array[0];
                initial_effects = effects;
                retained = flag
                    ? (struct Row){{mark(42), mark(7)}}.array
                    : (struct Row){{mark(-3), mark(-4)}}.array;
            again:
                current = flag
                    ? (struct Row){{mark(40 + iteration), mark(2)}}.array
                    : (struct Row){{mark(-5), mark(-6)}}.array;
                if (iteration == 0) {
                    first = current;
                    iteration = 1;
                    goto again;
                }
                skipped_and = no && (struct Row){{mark(-7), mark(-8)}}.array[0];
                skipped_or = flag || (struct Row){{mark(-9), mark(-10)}}.array[0];
                printf("conditional %d %d %d %d %d %d %d %d %d\n", selected, initial_effects,
                    retained[0], retained[1], first == current, first[0], effects, skipped_and, skipped_or);
                return selected != 40 || initial_effects != 2 || retained[0] != 42 || retained[1] != 7 ||
                    first != current || first[0] != 41 || effects != 8 || skipped_and != 0 || skipped_or != 1;
            }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-conditional-compound-" + Guid.NewGuid().ToString("N"));
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
            result.stdout.Trim().ShouldBe("conditional 40 2 42 7 1 41 8 0 1");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
