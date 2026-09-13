using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class OpaqueAggregateTests
{
    [Fact]
    public void Synthetic_headers_keep_runtime_aggregate_storage()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-runtime-aggregate-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, "#include <time.h>\n#include <locale.h>\nint main(void) { struct tm value; struct timespec stamp; struct lconv locale; return sizeof(value)+sizeof(stamp)+sizeof(locale); }");
        try
        {
            var emitted = Compiler.EmitCSharp(new[] { path });
            emitted.ShouldNotContain("unsafe partial struct tm");
            emitted.ShouldNotContain("unsafe partial struct timespec");
            emitted.ShouldNotContain("unsafe partial struct lconv");
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("int main(void) { return sizeof(struct Hidden); }")]
    [InlineData("int main(void) { return sizeof(struct tm); }")]
    [InlineData("typedef struct Hidden Hidden; int main(void) { return sizeof(Hidden); }")]
    [InlineData("struct Hidden *p; int main(void) { return sizeof(*p); }")]
    [InlineData("struct Hidden value; int main(void) { return 0; }")]
    [InlineData("int main(void) { struct Hidden value; return 0; }")]
    [InlineData("struct Outer { struct Hidden value; }; int main(void) { return 0; }")]
    [InlineData("int main(void) { struct Hidden value[2]; return 0; }")]
    [InlineData("struct Hidden *p; int main(void) { return p[1] == p[0]; }")]
    [InlineData("struct Hidden *p; int main(void) { p++; return 0; }")]
    public void Incomplete_aggregate_cannot_supply_object_storage_or_stride(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-opaque-" + Guid.NewGuid().ToString("N") + ".c");
        File.WriteAllText(path, source);
        try
        {
            Should.Throw<CompileException>(() => Compiler.EmitCSharp(new[] { path }))
                .Message.ShouldContain("incomplete aggregate");
        }
        finally { File.Delete(path); }
    }
}
