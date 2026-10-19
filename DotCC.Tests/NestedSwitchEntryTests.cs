using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class NestedSwitchEntryTests
{
    [Theory]
    [InlineData("{ SWITCH }")]
    [InlineData("if (0) { SWITCH }")]
    [InlineData("while (0) { SWITCH }")]
    [InlineData("for (int i=0;i<2;++i) { SWITCH }")]
    [InlineData("do { SWITCH } while(0);")]
    public void Function_scope_labels_can_enter_a_nested_switch(string enclosing)
    {
        var path=Path.Combine(Path.GetTempPath(), $"dotcc-nested-switch-entry-{Guid.NewGuid():N}.c");
        File.WriteAllText(path,"int main(void) { goto handler; "+enclosing.Replace("SWITCH",
            "switch (0) { case 0: if (0) { handler: return 7; } break; }")+" return 0; }");
        try { Assert.NotEmpty(Compiler.EmitCSharp(new[] {path})); }
        finally { File.Delete(path); }
    }
}
