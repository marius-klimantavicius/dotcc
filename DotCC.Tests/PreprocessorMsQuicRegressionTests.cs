using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class PreprocessorMsQuicRegressionTests
{
    [Theory]
    [InlineData("version.inc")]
    [InlineData("msquic.ver")]
    [InlineData("VERSION")]
    [InlineData("nested/version.inc")]
    public void Include_discovery_does_not_depend_on_extension(string name)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-include-extension-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(dir, name))!);
        File.WriteAllText(Path.Combine(dir, name), "#define VERSION 20700\n");
        var source = Path.Combine(dir, "main.c");
        File.WriteAllText(source, "#include \"" + name + "\"\nint main(void) { return VERSION; }\n");
        try { Compiler.EmitCSharp(new[] { source }).ShouldContain("return 20700;"); }
        finally { Directory.Delete(dir, recursive: true); }
    }

    [Fact]
    public void Gnu_comma_elision_distinguishes_omitted_and_empty_arguments()
    {
        var output = Preprocess("""
            #define EMPTY
            #define CALL(x, ...) sink(x, ##__VA_ARGS__)
            CALL(1)
            CALL(2,)
            CALL(3, EMPTY)
            CALL(4, 5, 6)
            #define ONLY(...) sink(7, ##__VA_ARGS__)
            ONLY()
            """);
        output.ShouldContain("sink ( 1 )");
        output.ShouldContain("sink ( 2 , )");
        output.ShouldContain("sink ( 3 , )");
        output.ShouldContain("sink ( 4 , 5 , 6 )");
        output.ShouldContain("sink ( 7 , )");
    }

    [Fact]
    public void Nested_variadic_forwarding_preserves_argument_boundaries_and_predefined_macros()
    {
        var output = Preprocess("""
            #define LOG(fmt, ...) sink((fmt), ##__VA_ARGS__)
            #define TRACE(name, fmt, ...) LOG((fmt " [" #name "]"), ##__VA_ARGS__, __FILE__, __LINE__)
            #define EVENT(name, fmt, ...) TRACE(name, fmt, ##__VA_ARGS__)
            #line 42 "logging.c"
            EVENT(AllocFailure, "%s %d", "key", 42)
            """);
        output.ShouldContain("sink ( ( ( \"%s %d\" \" [\" \"AllocFailure\" \"]\" ) ) , \"key\" , 42 , \"logging.c\" , 42 )");
        output.ShouldNotContain("__FILE__");
        output.ShouldNotContain("__LINE__");
    }

    [Theory]
    [InlineData("CIUQ", 1128879441)]
    [InlineData("AB", 16706)]
    [InlineData("ABCDE", 1111704645)]
    [InlineData("A\\n", 16650)]
    [InlineData("\\1\\2", 258)]
    [InlineData("\\x41G", 16711)]
    [InlineData("\\377ABC", -12500413)]
    public void Multichar_constants_pack_bytes_in_source_order(string inner, int expected)
    {
        CCharacterLiteral.Decode(inner).ShouldBe(expected);
        var output = Preprocess("#if '" + inner + "' == " + expected
            + "\nint selected;\n#else\nint wrong;\n#endif\n");
        output.ShouldContain("selected");
        output.ShouldNotContain("wrong");
    }

    [Theory]
    [InlineData("Aé")]
    [InlineData("éA")]
    [InlineData("\\400A")]
    public void Multichar_constants_reject_characters_without_a_defined_byte_encoding(string inner)
    {
        Should.Throw<DotCC.Ir.IrUnsupportedException>(() => CCharacterLiteral.Decode(inner));
    }

    private static string Preprocess(string source)
    {
        var dir = Path.Combine(Path.GetTempPath(), "dotcc-preprocess-regression-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "main.c");
        File.WriteAllText(path, source);
        try
        {
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output);
            return output.ToString();
        }
        finally { Directory.Delete(dir, recursive: true); }
    }
}
