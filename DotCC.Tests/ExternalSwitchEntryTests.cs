using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ExternalSwitchEntryTests
{
    [Theory]
    [InlineData("{ SWITCH }")]
    [InlineData("if (1) { SWITCH }")]
    [InlineData("while (1) { SWITCH break; }")]
    public void External_entry_through_unhandled_enclosing_scopes_is_diagnosed(string enclosing)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-external-switch-" + Guid.NewGuid().ToString("N") + ".c");
        var statement = enclosing.Replace("SWITCH", "switch (0) { case 0: handler: return 7; }");
        File.WriteAllText(path, "int main(void) { goto handler; " + statement + " return 0; }");
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
                .Message.ShouldContain("goto into a switch nested inside another statement");
        }
        finally { File.Delete(path); }
    }
}
