using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class ScalarTypedefAlignmentExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Scalar_aliases_preserve_real_storage_layout_and_unaligned_access(bool objects)
    {
        const string source = """
            #include <stdint.h>
            #include <stddef.h>
            #include <stdio.h>
            typedef __attribute__((aligned(1))) uint64_t Under;
            typedef uint64_t Post __attribute__((aligned(2)));
            typedef __attribute__((aligned(32))) uint64_t Over;
            typedef Under Chain;
            struct Record { char lead; Under value; char tail; };
            static Over global_value;
            int main(void) {
                unsigned char bytes[18] = {0};
                Under *unaligned = (Under *)(bytes + 1);
                *unaligned = 0x0102030405060708ULL;
                Over local_value = 42;
                global_value = local_value + 1;
                struct Record record = {0};
                record.value = *unaligned;
                record.tail = 7;
                printf("layout %d %d %d %d %d %d %d\n", (int)sizeof(Under),
                    (int)_Alignof(Under), (int)_Alignof(Post), (int)_Alignof(Over),
                    (int)_Alignof(Chain), (int)sizeof(record), (int)offsetof(struct Record, value));
                printf("storage %d %d values %d %d %d %d\n",
                    (int)((uintptr_t)&local_value & 31), (int)((uintptr_t)&global_value & 31),
                    (int)bytes[1], (int)bytes[8], (int)record.tail, (int)global_value);
                return 0;
            }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-scalar-alignment-" + Guid.NewGuid().ToString("N"));
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
            result.stdout.ReplaceLineEndings("\n").ShouldBe(
                "layout 8 1 2 32 1 10 1\nstorage 0 0 values 8 1 7 43\n");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public void Standard_escaped_question_mark_preserves_literal_bytes()
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-question-escape-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, """
            #include <stdio.h>
            int main(void) {
                const char *text = "\?\?=";
                printf("%s %d %d %d\n", text, (int)text[0], (int)text[1], (int)text[2]);
                return 0;
            }
            """);
        try
        {
            var result = FixtureRunner.CompileAndRunCapturingExit(
                Compiler.EmitCSharp(new[] { path }), Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.Trim().ShouldBe("??= 63 63 61");
        }
        finally { File.Delete(path); }
    }
}
