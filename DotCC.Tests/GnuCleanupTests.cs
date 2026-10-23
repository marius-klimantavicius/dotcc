using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class GnuCleanupTests
{
    [Theory]
    [InlineData("void cleanup(int *p);", "int", true)]
    [InlineData("int cleanup(void *p);", "int", true)]
    [InlineData("void cleanup(long *p);", "int", false)]
    [InlineData("void cleanup(int *p, int extra);", "int", false)]
    [InlineData("", "int", false)]
    public void Cleanup_requires_a_compatible_declared_function(string prototype, string type, bool valid)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-cleanup-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, prototype + " int test(void) { __attribute__((cleanup(cleanup))) " + type + " value = 1; return value; }");
        try
        {
            if (valid)
            {
                var emitted = Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib);
                emitted.ShouldContain("finally");
                emitted.ShouldContain("cleanup(");
            }
            else Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }));
        }
        finally { File.Delete(path); }
    }
}
