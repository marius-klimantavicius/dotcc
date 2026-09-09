using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class SwitchPreludeTests
{
    [Theory]
    [InlineData("int values[n]; case 1: values[0] = 42; return values[0];")]
    [InlineData("case 0: { int values[n]; case 1: values[0] = 42; return values[0]; }")]
    public void Case_entry_cannot_bypass_a_variable_array_extent(string body)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-switch-vla-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "int f(int n, int choice) { switch (choice) { " + body + " default: return 0; } } int main(void) { return 0; }");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
                .Message.ShouldContain("variable-length array across a switch entry");
        }
        finally { File.Delete(path); }
    }
}
