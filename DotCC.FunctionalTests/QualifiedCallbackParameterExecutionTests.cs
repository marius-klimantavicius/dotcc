using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class QualifiedCallbackParameterExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Const_callback_parameters_keep_their_signature_and_call_target(bool objects)
    {
        const string source = """
            #include <stdio.h>
            static int increment(int value) { return value + 1; }
            static int invoke(int (*const callback)(int), int value) { return callback(value); }
            static int invoke_slot(int (*const *slot)(int), int value) { return (*slot)(value); }
            int main(void) {
                int (*const callback)(int) = increment;
                printf("parameters %d %d\n", invoke(callback, 41), invoke_slot(&callback, 40));
                return 0;
            }
            """;
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-callback-parameter-" + Guid.NewGuid().ToString("N"));
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
            result.stdout.Trim().ShouldBe("parameters 42 41");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
