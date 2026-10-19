using System;
using System.Collections.Generic;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ParenthesizedStringArrayTests
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (var (type, prefix) in new[] { ("char", ""), ("char16_t", "u"), ("char32_t", "U"), ("wchar_t", "L"), ("char8_t", "u8") })
        foreach (var scope in new[] { "global", "static-global", "local", "static-local" })
        foreach (bool sized in new[] { false, true })
            yield return new object[] { type, prefix, scope, sized };
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Parentheses_preserve_string_array_initializers(string type, string prefix, string scope, bool sized)
    {
        var declaration = (scope.StartsWith("static", StringComparison.Ordinal) ? "static " : "")
            + type + " bytes[" + (sized ? "6" : "") + "] = ((" + prefix + "\"a\\0\" " + prefix + "\"b\"));";
        var source = scope.EndsWith("global", StringComparison.Ordinal)
            ? declaration + "\nint read(void) { return bytes[2]; }"
            : "int read(void) { " + declaration + " return bytes[2]; }";
        var path = Path.Combine(Path.GetTempPath(), "dotcc-paren-string-" + Guid.NewGuid().ToString("N") + ".c");
        try
        {
            File.WriteAllText(path, source);
            Compiler.EmitCSharp([path], emit: EmitMode.ManagedLib).ShouldContain("read");
        }
        finally { File.Delete(path); }
    }
}
