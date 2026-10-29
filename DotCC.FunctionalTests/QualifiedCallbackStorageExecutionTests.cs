using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class QualifiedCallbackStorageExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Qualified_callback_storage_executes_through_casts_and_pointer_chains(bool objects)
    {
        const string source = """
            #include <stdio.h>
            static int plus_one(int value) { return value + 1; }
            static int plus_two(int value) { return value + 2; }
            static int no_arguments(void) { return 7; }
            static int (*const global_callback)(int) = plus_one;
            static int (*const *global_slot)(int) = &global_callback;
            struct Holder { int (*const *slot)(int); };
            int main(void) {
                int (*const first)(int) = plus_one;
                int (*const second)(int) = plus_two;
                int (*const *slot)(int) = &first;
                int (*const third)(void) = no_arguments;
                int (*const *empty)(void) = &third;
                int before = (*slot)(40);
                slot = &second;
                int after = (*slot)(40);
                int (*const *cast_slot)(int) = (int (*const *)(int))slot;
                int (*const **chain)(int) = &slot;
                struct Holder holder = {cast_slot};
                printf("callbacks %d %d %d\n", before, (*cast_slot)(40), (*empty)());
                if (after != 42 || (**chain)(40) != 42 || (*holder.slot)(40) != 42 ||
                    (*global_slot)(40) != 41) return 1;
                return 0;
            }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-qualified-callback-" + Guid.NewGuid().ToString("N"));
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
            result.stdout.Trim().ShouldBe("callbacks 41 42 7");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
