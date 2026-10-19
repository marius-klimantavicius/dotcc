using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class PreprocessorMacroOptionTests
{
    [Theory]
    [InlineData("A=7|-UA", "#ifdef A\n#error removed macro remains\n#endif\nint result = 19;", "19")]
    [InlineData("-UA|A=23", "#if A != 23\n#error restored definition lost\n#endif\nint result = A;", "23")]
    [InlineData("A=7|-UA|A=31", "#if A != 31\n#error definition order lost\n#endif\nint result = A;", "31")]
    [InlineData("-U__STDC_HOSTED__", "#ifdef __STDC_HOSTED__\n#error seeded macro remains\n#endif\nint result = 37;", "37")]
    [InlineData("-U__STDC_HOSTED__|__STDC_HOSTED__=0", "#if __STDC_HOSTED__ != 0\n#error seed restoration lost\n#endif\nint result = 41;", "41")]
    public void Ordered_macro_operations_control_preprocessing_and_actual_emission(string flags, string source, string value)
    {
        WithSource(source, path =>
        {
            using var output = new StringWriter();
            var operations = flags.Split('|');
            Compiler.Preprocess([path], output, defines: operations);
            output.ToString().ShouldContain(value);
            Compiler.EmitCSharp([path], defines: operations, emit: EmitMode.ManagedLib).ShouldContain("result");
        });
    }

    [Theory]
    [InlineData("-U")]
    [InlineData("-U9bad")]
    [InlineData("-UA=7")]
    public void Invalid_undefinition_is_reported(string operation)
        => WithSource("int result;", path => Should.Throw<CompileException>(() =>
            Compiler.EmitCSharp([path], defines: [operation])).Message.ShouldContain("invalid macro name for -U"));

    [Fact]
    public void Undefinition_does_not_leak_between_translation_units()
        => WithSource("#ifdef A\n#error macro leaked\n#endif\nint result;", path =>
        {
            Compiler.EmitObject(path, defines: ["A=7", "-UA"]).ShouldContain("result");
            Should.Throw<CompileException>(() => Compiler.EmitObject(path, defines: ["A=7"])).Message.ShouldContain("macro leaked");
        });

    [Fact]
    public void Removed_predefinition_allows_a_later_source_macro_override()
        => WithSource("#define A 0\nint result = A;", path =>
        {
            var options = new CPreprocessingOptions([new MacroOverride("A", "43", RequireMatch: true)]);
            Compiler.EmitCSharp([path], defines: ["A=7", "-UA"], emit: EmitMode.ManagedLib,
                preprocessing: options).ShouldContain("43");
            Should.Throw<CompileException>(() => Compiler.EmitCSharp([path], defines: ["-UA", "A=7"],
                emit: EmitMode.ManagedLib, preprocessing: options)).Message.ShouldContain("cannot combine -D");
        });

    private static void WithSource(string source, Action<string> action)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-macro-options-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source + "\n");
        try { action(path); }
        finally { File.Delete(path); }
    }
}
