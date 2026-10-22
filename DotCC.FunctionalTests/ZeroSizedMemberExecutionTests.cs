using System;
using System.IO;
using System.Linq;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class ZeroSizedMemberExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Nested_zero_sized_union_preserves_layout_and_overlapping_storage(bool objects)
    {
        var fixture = FixtureRunner.Discover().Single(f => f.name == "zero-array-union-tail");
        Execute(fixture.sources.Single(), objects, fixture.expectedStdout);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Nested_zero_sized_union_reads_and_writes_preserve_single_base_evaluation(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zero-sized-input-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var input = Path.Combine(directory, "main.c");
            File.WriteAllText(input, """
                #include <stdio.h>
                #include <stdlib.h>
                typedef struct String {
                    int wide;
                    union { unsigned char bytes[0]; unsigned short words[0]; } u;
                } String;
                static int calls;
                static String *once(String *s) { calls++; return s; }
                static int get(String *s, int index) {
                    return s->wide ? once(s)->u.words[index] : once(s)->u.bytes[index];
                }
                int main(void) {
                    String *s = (String *)malloc(sizeof(String) + 8);
                    if (!s) return 2;
                    s->wide = 0;
                    once(s)->u.bytes[1] = 42;
                    int narrow = get(s, 1);
                    if (calls != 2) return 3;
                    s->wide = 1;
                    once(s)->u.words[1] = 4097;
                    int wide = get(s, 1);
                    if (calls != 4) return 4;
                    printf("flexible union: %d %d calls=%d\n", narrow, wide, calls);
                    free(s);
                    return narrow != 42 || wide != 4097;
                }
                """);
            Execute(input, objects, "flexible union: 42 4097 calls=4");
        }
        finally { Directory.Delete(directory, true); }
    }

    private static void Execute(string input, bool objects, string expected)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-zero-sized-output-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string emitted;
            if (objects)
            {
                var obj = Path.Combine(directory, "input.cs");
                File.WriteAllText(obj, Compiler.EmitObject(input));
                emitted = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] { input }, emit: EmitMode.Csproj);
            var result = FixtureRunner.CompileAndRunCapturingExit(emitted, Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.ReplaceLineEndings("\n").TrimEnd('\n')
                .ShouldBe(expected.ReplaceLineEndings("\n").TrimEnd('\n'));
        }
        finally { Directory.Delete(directory, true); }
    }
}
