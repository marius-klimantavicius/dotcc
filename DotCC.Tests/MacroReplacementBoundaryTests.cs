using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace DotCC.Tests;

public sealed class MacroReplacementBoundaryTests
{
    [Theory]
    [InlineData("#define CAT_(a,b) a##b\n#define CAT(a,b) CAT_(a,b)\n#define CALL(a,b,x) CAT(a,b)(x)\n#define F2(x) ((x)+1)\nresult=CALL(F,2,41);", "result=((41)+1);")]
    [InlineData("#define NAME F\n#define ALIAS() NAME\n#define WRAP(x) ALIAS()(x)\n#define F(x) x\nresult=WRAP(42);", "result=42;")]
    [InlineData("#define CAT_(a,b) a##b\n#define CAT(a,b) CAT_(a,b)\n#define F2(x) x\n#define BOTH(x) CAT(F,2)(x)+CAT(F,2)(x)\nresult=BOTH(21);", "result=21+21;")]
    [InlineData("#define ID(x) x\n#define F(x) ID(x)\n#define CALL(x) ID(F)(x)\nresult=CALL(F(42));", "result=42;")]
    [InlineData("#define SELF(x) SELF\n#define CALL(x) SELF(x)(x)\nresult=CALL(42);", "result=SELF(42);")]
    [InlineData("#define CAT_(a,b) a##b\n#define CAT(a,b) CAT_(a,b)\n#define F2(x) x\n#define CALL(x) CAT(F,2)(x)\n#define PAIR(x) CALL(x)+CALL(x)\nresult=PAIR(21);", "result=21+21;")]
    public void Replacement_suffix_is_rescanned_with_caller_parentheses(string source, string expected)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-boundary-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, source);
        try
        {
            using var output = new StringWriter();
            Compiler.Preprocess(new[] { path }, output);
            Assert.Contains(expected, Regex.Replace(output.ToString(), @"\s+", ""));
        }
        finally { File.Delete(path); }
    }
}
