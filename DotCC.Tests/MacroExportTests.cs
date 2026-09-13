using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class MacroExportTests
{
    [Fact]
    public void Errno_header_defines_every_runtime_error_number_with_the_same_value()
    {
        var errors = typeof(DotCC.Libc.Libc).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(int) && field.Name.StartsWith('E'))
            .OrderBy(field => field.Name, StringComparer.Ordinal).ToArray();
        var path = Path.Combine(Path.GetTempPath(), "dotcc-errno-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "#include <errno.h>\n" + string.Join(" ", errors.Select(field => field.Name)));
        try
        {
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output);
            var values = string.Join(" ", output.ToString().Split('\n').Where(line => !line.TrimStart().StartsWith('#')));
            System.Text.RegularExpressions.Regex.Split(values.Trim(), @"\s+")
                .ShouldBe(errors.Select(field => field.GetRawConstantValue()!.ToString()).ToArray());
        }
        finally { File.Delete(path); }
    }

    private static string Emit(string source, params string[] patterns)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-export-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try
        {
            return Compiler.EmitCSharp(new[] { path }, emit: EmitMode.ManagedLib,
                preprocessing: new CPreprocessingOptions(Array.Empty<MacroOverride>(), emitDefines: patterns));
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Macros_expand_function_wrappers_and_bind_typedefs(bool selected)
    {
        var output = Emit("""
            #include <errno.h>
            typedef unsigned int Status;
            #define STATUS_DEF(x) ((Status)(x))
            #define QUIC_STATUS_PENDING STATUS_DEF(-2)
            #define QUIC_STATUS_INVALID_PARAMETER STATUS_DEF(EINVAL)
            #define QUIC_STATUS_ALIAS QUIC_STATUS_INVALID_PARAMETER
            #define OTHER_STATUS STATUS_DEF(12)
            #define AUTOMATIC 42
            int value(void) { return QUIC_STATUS_INVALID_PARAMETER; }
            """, selected ? new[] { "QUIC_STATUS_*" } : Array.Empty<string>());
        output.ShouldContain("public const uint QUIC_STATUS_PENDING = unchecked((uint)(4294967294U));");
        output.ShouldContain("public const uint QUIC_STATUS_INVALID_PARAMETER = unchecked((uint)(22U));");
        output.ShouldContain("public const uint QUIC_STATUS_ALIAS = unchecked((uint)(22U));");
        output.ShouldContain("public const int AUTOMATIC");
        output.ShouldContain("const uint OTHER_STATUS");
        output.ShouldNotContain("const int STATUS_DEF");
    }

    [Fact]
    public void Exact_names_and_question_patterns_are_additive_and_case_sensitive()
    {
        // Source macros are automatic; system-header macros still require selection.
        var output = Emit("#include <errno.h>\nint value(void) { return 0; }", "EIO", "ENOEN?", "eacces", "EIO");
        output.ShouldContain("public const int EIO = unchecked");
        output.ShouldContain("public const int ENOENT = unchecked");
        output.ShouldNotContain("public const int EACCES = unchecked");
        output.ShouldNotContain("public const int EINVAL = unchecked");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Pointer_sentinel_is_readonly_and_enum_keeps_its_type(bool selected)
    {
        var output = Emit("""
            typedef void *Handle;
            typedef enum Mode { First = 1, Last = 7 } Mode;
            #define API_INVALID ((Handle)-1)
            #define API_MODE ((Mode)Last)
            #define API_SIZE sizeof(Mode)
            int value(void) { return 0; }
            """, selected ? new[] { "API_*" } : Array.Empty<string>());
        output.ShouldContain("public static readonly unsafe void* API_INVALID");
        output.ShouldContain("public const Mode API_MODE");
        output.ShouldContain("public const ulong API_SIZE");
    }

    [Theory]
    [InlineData("missing()")]
    [InlineData("__LINE__")]
    [InlineData("CONTEXT(0)")]
    [InlineData("1; int injected = 7")]
    [InlineData("state++")]
    [InlineData("state")]
    [InlineData("\"\\xff\"")]
    public void Unsupported_selected_expressions_report_the_macro(string body)
    {
        var error = Should.Throw<CompileException>(() => Emit(
            "int state;\n#define CONTEXT(x) __LINE__\n#define PICK " + body + "\nint value(void) { return 0; }", "PICK"));
        error.Message.ShouldContain("PICK");
    }

    [Fact]
    public void Automatic_discovery_expands_alias_calls_pasting_stringification_and_variadics()
    {
        var output = Emit("""
            typedef unsigned short Word;
            #define WRAP(x) ((Word)(x))
            #define ALIAS WRAP
            #define CAT(a,b) a ## b
            #define STR(x) #x
            #define XSTR(x) STR(x)
            #define SUM(x,...) ((x) __VA_OPT__(+ __VA_ARGS__))
            #define VALUE ALIAS(65539)
            #define PASTED CAT(12,34)
            #define RAW STR(VALUE)
            #define EXPANDED XSTR(CAT(12,34))
            #define VARIADIC SUM(7,5)
            #define EMPTY_VARIADIC SUM(7)
            #define CONTEXT XSTR(__LINE__)
            #define RAW_CONTEXT STR(__LINE__)
            #define IGNORE(x) 42
            #define DISCARDED_CONTEXT IGNORE(__LINE__)
            #define RECURSIVE RECURSIVE
            int value(void) { return 0; }
            """);
        output.ShouldContain("public const ushort VALUE = unchecked((ushort)(3u));");
        output.ShouldContain("public const int PASTED = unchecked((int)(1234));");
        output.ShouldContain("public const string RAW = \"VALUE\";");
        output.ShouldContain("public const string EXPANDED = \"1234\";");
        output.ShouldContain("public const int VARIADIC = unchecked((int)(12));");
        output.ShouldContain("public const int EMPTY_VARIADIC = unchecked((int)(7));");
        output.ShouldNotContain("const string CONTEXT");
        output.ShouldContain("public const string RAW_CONTEXT = \"__LINE__\";");
        output.ShouldContain("public const int DISCARDED_CONTEXT = unchecked((int)(42));");
        output.ShouldNotContain("const int RECURSIVE");
    }

    [Fact]
    public void Typed_layout_constants_do_not_mutate_the_translated_program()
    {
        const string source = """
            #include <stddef.h>
            typedef struct Record { char tag; double value; } Record;
            int *pointer;
            extern double external(void);
            extern double data;
            int value(void) { return 0; }
            """;
        var output = Emit(source + "\n" + """
            #define RECORD_SIZE sizeof(Record)
            #define VALUE_OFFSET offsetof(Record,value)
            #define RECORD_ALIGN _Alignof(Record)
            #define DATA_SIZE sizeof(data)
            #define CALL_SIZE sizeof(external())
            #define ADDRESS_SIZE sizeof(&pointer)
            #define UNKNOWN_SIZE sizeof(unknown)
            #define UNKNOWN_CALL_SIZE sizeof(unknown())
            #define UNUSED_ADDRESS (&pointer)
            #define UNUSED_CALL external()
            #define UNUSED_DATA data
            #define UNUSED_BAD_CALL __dotcc_unknown()
            #define INVALID_GENERIC _Generic(0, int: 1, int: 2)
            """);
        foreach (var (name, value) in new[] { ("RECORD_SIZE", 16), ("VALUE_OFFSET", 8), ("RECORD_ALIGN", 8), ("DATA_SIZE", 8), ("CALL_SIZE", 8), ("ADDRESS_SIZE", 8) })
            output.ShouldContain($"public const ulong {name} = unchecked((ulong)({value}UL));");
        foreach (var name in new[] { "UNKNOWN_SIZE", "UNKNOWN_CALL_SIZE", "UNUSED_ADDRESS", "UNUSED_CALL", "UNUSED_DATA", "UNUSED_BAD_CALL", "INVALID_GENERIC" })
            output.ShouldNotContain(" " + name + " =");
        // Remove only the discovered fields: imports, globals, functions, and
        // storage decisions must be byte-identical to the macro-free program.
        var withoutMetadata = string.Join("\n", output.Split('\n').Where(line => !line.TrimStart().StartsWith("public const ulong", StringComparison.Ordinal)));
        withoutMetadata.ShouldBe(Emit(source));
    }

    [Fact]
    public void Unused_expansion_with_exponentially_growing_token_text_is_bounded()
    {
        var expansion = "x";
        for (var i = 0; i < 30; i++) expansion = "DUP(" + expansion + ")";
        var source = "#define CAT_RAW(a,b) a ## b\n#define CAT(a,b) CAT_RAW(a,b)\n#define DUP(x) CAT(x,x)\n#define BIG "
            + expansion + "\n#define ANSWER 42\nint value(void) { return 0; }";
        var output = Emit(source);
        output.ShouldContain("public const int ANSWER = unchecked((int)(42));");
        output.ShouldNotContain(" BIG =");
        Should.Throw<CompileException>(() => Emit(source, "BIG")).Message.ShouldContain("BIG");
    }

    [Theory]
    [InlineData("")]
    [InlineData("[A-Z]+")] // These are globs, not regular expressions.
    [InlineData("BAD NAME")]
    public void Invalid_patterns_are_diagnosed(string pattern) =>
        Should.Throw<CompileException>(() => new CPreprocessingOptions(Array.Empty<MacroOverride>(), emitDefines: new[] { pattern }));
}
