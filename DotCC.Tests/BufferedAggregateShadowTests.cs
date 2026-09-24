using System;
using System.IO;
using Xunit;

namespace DotCC.Tests;

public sealed class BufferedAggregateShadowTests
{
    [Theory]
    [InlineData("typedef struct outer { struct { int value; } entry; entry after; } outer;")]
    [InlineData("typedef struct outer { union { struct { int value; } entry; int number; } data; entry after; } outer;")]
    [InlineData("typedef struct outer { struct named { int value; } entry; entry after; } outer;")]
    [InlineData("typedef struct outer { enum { FIRST=1 } entry; entry after; } outer;")]
    [InlineData("typedef struct outer { struct { int value; } *entry; entry after; } outer;")]
    public void Closing_aggregate_completes_member_type_without_hiding_typedef(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), $"dotcc-buffered-shadow-{Guid.NewGuid():N}.c");
        File.WriteAllText(path, "typedef int entry; " + source + " entry restored;");
        try { Assert.NotEmpty(Compiler.EmitObject(path, dialect: CDialect.Parse("c11"))); }
        finally { File.Delete(path); }
    }
}
