using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class NoreturnCallTests
{
    [Theory]
    [InlineData("abort();")]
    [InlineData("exit(7);")]
    [InlineData("_Exit(7);")]
    [InlineData("(void)(abort());")]
    [InlineData("(void)(x++, abort());")]
    [InlineData("x ? abort() : exit(7);")]
    public void Terminating_standard_call_gets_explicit_CSharp_terminator(string tail)
    {
        var path = Write("#include <stdlib.h>\nint f(int x) { if (x) return x; " + tail + " } int main(void) { return f(1)-1; }");
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { path });
            emitted.ShouldContain("A noreturn function returned.");
            if (tail.Contains("abort")) emitted.ShouldContain("abort();");
            if (tail.Contains("exit(7)")) emitted.ShouldContain("exit(7);");
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Prototype_noreturn_marker_applies_at_a_braceless_call()
    {
        var path = Write("_Noreturn void die(void); int f(int x) { if(x) die(); else return 2; } void die(void) { for(;;){} } int main(void) { return f(0)-2; }");
        try { Compiler.EmitCSharp(new[] { path }).ShouldContain("A noreturn function returned."); }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Same_named_user_function_is_not_assumed_to_terminate()
    {
        var path = Write("void abort(void) { } int main(void) { abort(); return 0; }");
        try { Compiler.EmitCSharp(new[] { path }).ShouldNotContain("A noreturn function returned."); }
        finally { File.Delete(path); }
    }

    private static string Write(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-noreturn-call-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source); return path;
    }
}
