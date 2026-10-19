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
    public void External_entry_through_enclosing_scopes_is_lowered(string enclosing)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-external-switch-" + Guid.NewGuid().ToString("N") + ".c");
        var statement = enclosing.Replace("SWITCH", "switch (0) { case 0: handler: return 7; }");
        File.WriteAllText(path, "int main(void) { goto handler; " + statement + " return 0; }");
        try
        {
            Compiler.EmitCSharp(new[] { path }).ShouldNotBeNullOrEmpty();
        }
        finally { File.Delete(path); }
    }
}
