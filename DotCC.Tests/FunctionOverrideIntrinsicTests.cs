using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class FunctionOverrideIntrinsicTests
{
    [Theory]
    [InlineData("unsigned int", "unsigned long long")]
    [InlineData("int", "unsigned int")]
    [InlineData("int", "long long")]
    [InlineData("int", "const unsigned char *")]
    public void Popcount_rejects_incompatible_scalar_contract(string result, string parameter)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-popcount-signature-" + Guid.NewGuid().ToString("N") + ".c");
        try
        {
            File.WriteAllText(path, result + " count_bits(" + parameter + " value);");
            var options = new CPreprocessingOptions(Array.Empty<MacroOverride>(), functionOverrides: new[]
            {
                new FunctionOverride("count_bits", new(result, new[] { parameter }),
                    new("intrinsic", "popcount.u64"), RequireMatch: true)
            });
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path },
                emit: EmitMode.ManagedLib, preprocessing: options)).Message
                .ShouldContain("signed 32-bit result and one unsigned 64-bit integer");
        }
        finally { File.Delete(path); }
    }
}
