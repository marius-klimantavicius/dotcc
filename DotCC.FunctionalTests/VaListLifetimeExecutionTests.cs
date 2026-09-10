using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class VaListLifetimeExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Borrowed_cursor_returns_and_locally_restarted_parameters_execute(bool objectLink)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-cursor-lifetime-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string source = Path.Combine(directory, "main.c");
        string fragment = Path.Combine(directory, "main.cs");
        File.WriteAllText(source, """
            #include <stdarg.h>
            #include <stdio.h>
            va_list relay(va_list incoming) {
                va_list copy;
                va_copy(copy, incoming);
                return copy;
            }
            int restarted(va_list incoming, int count, ...) {
                va_start(incoming, count);
                return va_arg(incoming,int);
            }
            int total(int count, ...) {
                va_list original,copy;
                va_start(original,count);
                copy = relay(original);
                int left=va_arg(copy,int);
                int right=va_arg(original,int);
                return left+right+restarted(original,1,2);
            }
            int main(void) { printf("%d\n",total(1,20)); return 0; }
            """);
        try
        {
            string emitted;
            if (objectLink)
            {
                File.WriteAllText(fragment, Compiler.EmitObject(source));
                emitted = Compiler.LinkObjects(new[] { fragment });
            }
            else emitted = Compiler.EmitCSharp(new[] { source });
            FixtureRunner.CompileAndRun(emitted, Array.Empty<string>()).ShouldBe("42\n");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
