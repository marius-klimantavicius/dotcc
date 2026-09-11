using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class CompoundArrayConversionTests
{
    [Fact]
    public void Array_compound_literal_converts_each_typed_element()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-compound-conversion-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, """
            enum code { value = 260 };
            int main(void) {
                int x = 257;
                unsigned u = 259;
                enum code e = value;
                unsigned char *p = (unsigned char[]){x, u, x > 0, !!x, -1, e};
                void *raw = &x;
                int **q = (int *[]){0, raw};
                return p[0] + *q[1];
            }
            """);
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { path });
            emitted.ShouldContain("stackalloc byte[]{ (byte)(x), (byte)(u), (byte)(");
            emitted.ShouldContain("(byte)(e)");
            emitted.ShouldContain("stackalloc int*[]{ null, (int*)(raw) }");
        }
        finally { File.Delete(path); }
    }
}
