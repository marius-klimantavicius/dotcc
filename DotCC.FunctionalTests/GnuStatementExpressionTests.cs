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
    public void Native_assert_conditionals_preserve_single_evaluation_and_lazy_branches(bool objects)
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotcc-native-assert-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.Combine(directory, "main.c");
            File.WriteAllText(source, """
                #include <stdio.h>
                static int failures;
                static void failed(const char *function) __attribute__((__nonnull__(1))) { failures++; printf("%s ", function); }
                #define CHECK(expr) ((void)sizeof((expr) ? 1 : 0), __extension__ ({ if (expr) ; else failed(__extension__ __PRETTY_FUNCTION__); }))
                static void check(void) {
                    int count = 0;
                    CHECK(++count == 1);
                    CHECK(++count == 1);
                    int lazy = 0 && __extension__ ({ if (++count) failed(__FUNCTION__); 1; });
                    int value = 1 ? ({ if (count == 2) { count += 3; } else count += 100; count; }) : 0;
                    printf("%d %d %d %d\n", count, failures, lazy, value);
                }
                int main(void) { check(); return 0; }
                """);
            string emitted;
            if (objects)
            {
                var obj = Path.ChangeExtension(source, ".cs");
                File.WriteAllText(obj, Compiler.EmitObject(source));
                emitted = Compiler.LinkObjects(new[] { obj }, emit: EmitMode.Csproj);
            }
            else emitted = Compiler.EmitCSharp(new[] { source }, emit: EmitMode.Csproj);
            FixtureRunner.CompileAndRun(emitted, Array.Empty<string>()).Trim().ShouldBe("check 5 1 0 5");
        }
        finally { Directory.Delete(directory, true); }
    }

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
