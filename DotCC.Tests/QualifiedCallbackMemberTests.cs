#nullable enable
using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class QualifiedCallbackMemberTests
{
    [Theory]
    [InlineData("int", "int value", "int value", "value")]
    [InlineData("void", "void", "void", "")]
    public void Const_callback_member_rejects_assignment_after_initialization(
        string result, string parameters, string implementationParameters, string returned)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-qualified-callback-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, $$"""
            struct Table { {{result}} (*const callback)({{parameters}}); };
            {{result}} callback({{implementationParameters}}) { return {{returned}}; }
            int main(void) { struct Table table = {callback}; table.callback = callback; return 0; }
            """);
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
                .Message.ShouldContain("read-only");
        }
        finally { File.Delete(path); }
    }
}
