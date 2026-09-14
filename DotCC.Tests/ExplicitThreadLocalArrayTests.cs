using System;
using System.IO;
using DotCC;
using Shouldly;
using Xunit;
namespace DotCC.Tests;
public sealed class ExplicitThreadLocalArrayTests
{
    [Fact]
    public void explicit_tls_array_specs_do_not_leak_to_following_ordinary_arrays()
    {
        string path=Path.Combine(Path.GetTempPath(),"dotcc-explicit-tls-"+Guid.NewGuid().ToString("N")+".c");
        File.WriteAllText(path,"""
            struct Record { long value; };
            static _Thread_local struct Record local[2];
            static int shared[2];
            _Thread_local int table[2][3];
            static char text[] = "ok";
            int main(void) { local[1].value = 7; table[1][2] = 9; shared[0] = 2; return (int)local[1].value + table[1][2] + shared[0] + text[0]; }
            """);
        try
        {
            string emitted=Compiler.EmitCSharp(new[]{path});
            emitted.ShouldContain("__dotcc_tls_array_local");
            emitted.ShouldContain("__dotcc_tls_array_table");
            emitted.ShouldNotContain("__dotcc_tls_array_shared");
            emitted.ShouldNotContain("__dotcc_tls_array_text");
        }
        finally { File.Delete(path); }
    }
    [Theory]
    [InlineData("_Thread_local int data[2] = {1,2};")]
    [InlineData("_Thread_local char data[] = \"a\";")]
    public void unsupported_initialized_tls_array_fails_instead_of_emitting_shared_storage(string declaration)
    {
        string path=Path.Combine(Path.GetTempPath(),"dotcc-initialized-tls-"+Guid.NewGuid().ToString("N")+".c");
        File.WriteAllText(path,declaration+"\nint main(void) { return data[0]; }");
        try { Should.Throw<Exception>(()=>Compiler.EmitCSharp(new[]{path})).Message.ShouldContain("initialized thread-local arrays are not supported"); }
        finally { File.Delete(path); }
    }
}
