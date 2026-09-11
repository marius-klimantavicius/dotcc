using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class MacroTokenPasteTests
{
    [Theory]
    [InlineData("pre, middle, _post", "premiddle_post")]
    [InlineData("pre, , _post", "pre_post")]
    [InlineData(", middle, _post", "middle_post")]
    [InlineData("pre, middle,", "premiddle")]
    [InlineData(", , end", "end")]
    [InlineData("start, ,", "start")]
    [InlineData(", middle,", "middle")]
    [InlineData(", ,", "")]
    [InlineData("first pre, middle tail, _post last", "first premiddle tail_post last")]
    public void Chained_paste_preserves_empty_and_multitoken_operands(string arguments, string expected)
    {
        WithSource("#define CAT(a,b,c) a ## b ## c\nresult = CAT(" + arguments + ");\n", path =>
        {
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output);
            output.ToString().ShouldNotContain("##");
            output.ToString().ShouldContain("result = " + (expected.Length == 0 ? "" : expected + " ") + ";");
        });
    }

    [Fact]
    public void Chained_paste_uses_raw_arguments_and_rescans_only_after_the_complete_chain()
    {
        WithSource("""
            #define middle wrong
            #define premiddle wrong
            #define premiddle_post_end 42
            #define CAT(name) pre ## name ## _post ## _end
            int result = CAT(middle);
            """, path =>
        {
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output);
            output.ToString().ShouldContain("result = 42 ;");
            output.ToString().ShouldNotContain("##");
        });
    }

    [Fact]
    public void Chained_paste_in_callback_typedef_emits_with_self_pointer()
    {
        WithSource("""
            #define CALLBACK(name) typedef struct st_##name##_t { int (*cb)(struct st_##name##_t *self); } name##_t
            CALLBACK(clock);
            static int callback(clock_t *self) { return self != 0; }
            int main(void) { clock_t value = {callback}; return value.cb(&value) != 1; }
            """, path => Compiler.EmitCSharp(new[] { path }).ShouldContain("st_clock_t"));
    }

    private static void WithSource(string source, Action<string> test)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-paste-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try { test(path); }
        finally { File.Delete(path); }
    }
}
