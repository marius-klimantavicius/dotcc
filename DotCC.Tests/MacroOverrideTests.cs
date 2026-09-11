using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class MacroOverrideTests
{
    private static void WithSource(string source, Action<string> action)
    {
        var root = Path.Combine(Path.GetTempPath(), "dotcc-overrides-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try { var path = Path.Combine(root, "main.c"); File.WriteAllText(path, source); action(path); }
        finally { Directory.Delete(root, true); }
    }
    private static string Pre(string source, params MacroOverride[] rules)
    {
        var result = "";
        WithSource(source, path => { using var writer = new StringWriter(); Compiler.Preprocess(new[] { path }, writer,
            preprocessing: new CPreprocessingOptions(rules)); result = Regex.Replace(writer.ToString(), @"\s+", ""); });
        return result;
    }
    [Fact]
    public void Selection_is_reconsidered_after_each_definition_and_undef()
    {
        var output = Pre("""
            #ifdef X
            wrong_early
            #endif
            #define X(n) do_call(n, 5)
            X(a++)
            #undef X
            #ifdef X
            wrong_undef
            #endif
            #define X(n) other_call(n)
            X(b++)
            #undef X
            #define X(n) do_call(n, 15)
            X(c++)
            """, new MacroOverride("X", "dotcc_mama(${__dotcc_n}, ${num})", Pattern: @"do_call\(\s*n\s*,\s*(?<num>\d+)\s*\)", RequireMatch: true));
        output.ShouldContain("dotcc_mama(a++,5)other_call(b++)dotcc_mama(c++,15)");
        output.ShouldNotContain("wrong_");
    }
    [Fact]
    public void Exact_tokens_ignore_comments_and_spacing_but_preserve_parentheses()
    {
        Pre("#define X ( n /* keep gap */ + 1 )\nX\n#define X ((n+1))\nX\n", new MacroOverride("X", "42", Exact: "(n+1)"))
            .ShouldEndWith("42((n+1))");
    }
    [Fact]
    public void Regex_uses_real_logical_spelling_after_splicing_and_comments()
    {
        Pre("#define X(n) do_call(n, \\\n  /*gap*/ 5)\nX(z)\n", new MacroOverride("X", "${num}+${__dotcc_n}", Pattern: @"do_call\(n,\s+(?<num>5)\)"))
            .ShouldEndWith("5+z");
    }
    [Fact]
    public void First_match_wins_and_name_only_fallback_keeps_function_signature()
    {
        Pre("#define X(n) n+1\nX(7)\n#define X(n) n+2\nX(7)\n#define X 99\nX\n",
            new MacroOverride("X", "${__dotcc_n}+3", Exact: "n+1"), new MacroOverride("X", "42"))
            .ShouldEndWith("7+34242");
        Should.Throw<CompileException>(() => new CPreprocessingOptions(new[] { new MacroOverride("X", "1"), new MacroOverride("X", "2", Exact: "0") }))
            .Message.ShouldContain("unreachable");
    }
    [Fact]
    public void Signature_nonmatch_is_not_a_missing_parameter_error()
    {
        var signature = new MacroSignature(true, new[] { "n" }, false);
        Pre("#define X 1\nX\n#define X(m) m\nX(3)\n#define X(n) n\nX(4)\n",
            new MacroOverride("X", "${__dotcc_n}+2", Signature: signature)).ShouldEndWith("134+2");
    }
    [Fact]
    public void Prescan_stringification_pasting_variadics_and_recursion_remain_C_macros()
    {
        Pre("#define X(n) n\nX(hello world)\n", new MacroOverride("X", "# ${__dotcc_n}")).ShouldEndWith("\"helloworld\"");
        Pre("#define X(n) n\nX(he)\n", new MacroOverride("X", "${__dotcc_n} ## llo")).ShouldEndWith("hello");
        Pre("#define X(n,...) n\nX(1,2,3)\n", new MacroOverride("X", "call(${__dotcc_n}, __VA_ARGS__)")).ShouldEndWith("call(1,2,3)");
        Pre("#define X(n) n\nX(1)\n", new MacroOverride("X", "X(${__dotcc_n})")).ShouldEndWith("X(1)");
        Pre("#define X() old\nX()\n", new MacroOverride("X", "42")).ShouldEndWith("42");
    }
    [Fact]
    public void No_match_is_permissive_unless_an_assertion_was_requested()
    {
        Pre("#define X 1\nX\n", new MacroOverride("X", "42", Exact: "2")).ShouldEndWith("1");
        Should.Throw<CompileException>(() => Pre("#define X 1\nX\n", new MacroOverride("X", "42", Exact: "2", RequireMatch: true)))
            .Message.ShouldContain("requireMatch");
        Should.Throw<CompileException>(() => Pre("#define X 1\nX\n", new MacroOverride("X", "42", Expect: new[] { "2" })))
            .Message.ShouldContain("expect");
        Pre("#if 0\n#define X 1\n#endif\n", new MacroOverride("X", "42", Expect: new[] { "2" }));
    }
    [Theory]
    [InlineData("${missing}", "(?<num>5)", "unknown named capture")]
    [InlineData("${num}", "(?<num>5)?6", "exactly once")]
    [InlineData("${num}", "(?<num>6)+", "exactly once")]
    [InlineData("${__dotcc_n}", "6", "missing formal")]
    [InlineData("\"${__dotcc_n}\"", "6", "complete token")]
    [InlineData("/* ${__dotcc_n} */", "6", "complete token")]
    [InlineData("// ${__dotcc_n}", "6", "complete token")]
    [InlineData("pre${__dotcc_n}", "6", "complete token")]
    [InlineData("0", "(?<__dotcc_n>6)", "reserved")]
    [InlineData("0", "[", "invalid regex")]
    [InlineData("#include \"x.h\"", "6", "directive")]
    [InlineData("0\n#define Z 1", "6", "newline")]
    [InlineData("## n", "6", "## requires")]
    [InlineData("__VA_ARGS__", "6", "nonvariadic")]
    public void Invalid_rules_and_selected_templates_fail_clearly(string replacement, string pattern, string diagnostic)
    {
        Should.Throw<CompileException>(() => Pre("#define X 66\nX\n#define X 6\nX\n", new MacroOverride("X", replacement, Pattern: pattern)))
            .Message.ShouldContain(diagnostic);
    }
    [Fact]
    public void Profile_validation_precedence_dependency_and_exports()
    {
        WithSource("#define X 1\nint main(void) { return X; }", path =>
        {
            var profile = Path.Combine(Path.GetDirectoryName(path)!, "overrides.json");
            File.WriteAllText(profile, """{"version":1,"macroOverrides":[{"name":"X","replacement":"42","requireMatch":true}]}""");
            var options = CPreprocessingOptions.Load(profile);
            Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib, preprocessing: options).ShouldContain("public const int X = unchecked((int)(42));");
            Compiler.EmitDependencyRule(path, new[] { "out.o" }, false, preprocessing: options).ShouldContain(profile);
            Compiler.EmitCSharp(new[] { path }, preprocessing: CPreprocessingOptions.Load(profile, new[] { "X=7" })).ShouldContain("return 7;");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }, defines: new[] { "X=9" }, preprocessing: options)).Message.ShouldContain("-D");
            Compiler.EmitCSharp(new[] { path }, defines: new[] { "X=9" }).ShouldContain("return 1;");
            File.WriteAllText(profile, """{"version":1,"macroOverrides":[],"typo":1}""");
            Should.Throw<CompileException>(() => CPreprocessingOptions.Load(profile)).Message.ShouldContain("unknown");
        });
    }
    [Fact]
    public void Match_assertions_are_per_invocation_and_report_is_reusable()
    {
        WithSource("#include \"body.h\"\nint value(void) { return X; }", path =>
        {
            var dir = Path.GetDirectoryName(path)!;
            File.WriteAllText(Path.Combine(dir, "body.h"), "#ifndef BODY_H\n#define BODY_H\n#define X 1\n#endif\n");
            var second = Path.Combine(dir, "second.c"); File.WriteAllText(second, "int main(void) { return 0; }");
            using var report = new StringWriter();
            var options = new CPreprocessingOptions(new[] { new MacroOverride("X", "42", RequireMatch: true) }, report: report);
            var emitted = Compiler.EmitCSharp(new[] { path, second }, preprocessing: options);
            emitted.ShouldContain("return 42;"); report.ToString().ShouldContain("selected");
            report.ToString().ShouldContain("body.h");
            emitted.ShouldBe(Compiler.EmitCSharp(new[] { path, second }, preprocessing: options));
        });
    }
}
