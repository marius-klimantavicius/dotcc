#nullable enable
using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ExternFnPtrTests
{
    [Theory]
    [InlineData("")]
    [InlineData("volatile ")]
    [InlineData("const ")]
    public void Extern_callback_declaration_shares_its_later_definition(string qualifier)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-extern-callback-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, $$"""
            extern int (*{{qualifier}}callback)(int);
            int invoke(void) { return callback(41); }
            static int increment(int x) { return x + 1; }
            int (*{{qualifier}}callback)(int) = increment;
            int main(void) { return invoke() - 42; }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { path });
            if (qualifier == "volatile ")
            {
                emitted.ShouldContain("nint callback");
                emitted.ShouldContain("Volatile.Read(ref Globals.callback)");
            }
            else emitted.ShouldContain("public delegate*<int, int> callback;");
        }
        finally { File.Delete(path); }
    }
}
