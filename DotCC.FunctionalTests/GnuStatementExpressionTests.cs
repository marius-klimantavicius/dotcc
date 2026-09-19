#nullable enable
using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class GnuStatementExpressionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Statement_expressions_preserve_runtime_calls_and_inline_object_metadata(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-statement-expression-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var header = Path.Combine(directory, "value.h");
            File.WriteAllText(header, "#include <stdlib.h>\nstatic inline int value(int input) { return ({ int copy = abs(input); copy + 1; }); }\n");
            var first = Path.Combine(directory, "first.c");
            File.WriteAllText(first, "#include \"value.h\"\nint other(void) { return value(-8); }\n");
            var second = Path.Combine(directory, "second.c");
            File.WriteAllText(second, """
                #include "value.h"
                #include <stdio.h>
                int other(void);
                int main(void) {
                    int count = 0;
                    ({ count += 1; (void)0; });
                    printf("%d %d %d\n", value(-6), other(), count);
                    return 0;
                }
                """);
            string emitted;
            if (objects)
            {
                var a = Path.ChangeExtension(first, ".cs");
                var b = Path.ChangeExtension(second, ".cs");
                File.WriteAllText(a, Compiler.EmitObject(first));
                File.WriteAllText(b, Compiler.EmitObject(second));
                emitted = Compiler.LinkObjects(new[] { a, b }, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] { first, second }, emit: EmitMode.Csproj);
            FixtureRunner.CompileAndRun(emitted, Array.Empty<string>()).Trim().ShouldBe("7 9 1");
        }
        finally { Directory.Delete(directory, true); }
    }
}
