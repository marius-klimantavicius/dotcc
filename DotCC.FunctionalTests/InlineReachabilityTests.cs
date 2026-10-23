using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed class InlineReachabilityTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void An_unused_header_inline_cannot_impose_a_native_dependency(bool objects, bool deduplicate)
    {
        var folder = Path.Combine(Path.GetTempPath(), "dotcc-inline-root-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var source = Path.Combine(folder, "main.c");
            File.WriteAllText(source, """
                #include <stdio.h>
                int unavailable_native(void);
                static inline int uncalled(void) { return unavailable_native(); }
                static inline int leaf(int value) { return value + 1; }
                static inline int reachable(int value) { return leaf(value); }
                static inline int addressed(int value) { return value * 2; }
                int (*saved)(int) = addressed;
                int main(void) { printf("%d %d\n", reachable(4), saved(7)); return 0; }
                """);
            var options = new CSharpOutputOptions(DeduplicateInline: deduplicate, PruneUnusedInline: true);
            string generated;
            if (objects)
            {
                var fragment = Path.Combine(folder, "main.o");
                File.WriteAllText(fragment, Compiler.EmitObject(source));
                generated = Compiler.LinkObjects(new[] { fragment }, outputOptions: options);
            }
            else generated = Compiler.EmitCSharp(new[] { source }, outputOptions: options);
            var result = FixtureRunner.CompileAndRunCapturingExit(generated, Array.Empty<string>());
            result.exit.ShouldBe(0);
            result.stdout.Trim().ShouldBe("5 14");
        }
        finally { Directory.Delete(folder, true); }
    }
}
