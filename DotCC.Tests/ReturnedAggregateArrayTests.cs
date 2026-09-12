using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class ReturnedAggregateArrayTests
{
    [Fact]
    public void Fixed_buffer_return_member_gets_addressable_storage()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotcc-return-array-" + Guid.NewGuid() + ".c");
        try
        {
            File.WriteAllText(path, "struct Value { char bytes[4]; }; struct Value make(void) { struct Value value = {{1,2,3,0}}; return value; } int main(void) { return make().bytes[0] - 1; }");
            var emitted = Compiler.EmitCSharp(new[] { path });
            emitted.ShouldContain(" = make();");
            emitted.ShouldNotContain("make().bytes");
        }
        finally { File.Delete(path); }
    }
}
