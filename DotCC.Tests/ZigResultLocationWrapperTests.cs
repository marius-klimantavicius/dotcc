using System;
using System.IO;
using Shouldly;
using Xunit;
namespace DotCC.Tests;
public sealed class ZigResultLocationWrapperTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Raw_wrappers_cannot_be_reinterpreted_as_their_payload(bool objects, bool optional)
    {
        var directory=Path.Combine(Path.GetTempPath(),"dotcc-raw-wrapper-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(directory);
        try
        {
            var path=Path.Combine(directory,"main.zig");
            File.WriteAllText(path,"fn f(x:u24) "+(optional?"?u24":"error{E}!u24")+" { return x; } fn g(x:u24) u24 { return @bitCast(f(x)); } pub fn main() u8 { return @intCast(g(7)); }");
            Should.Throw<CompileException>(() => { if(objects) _=Compiler.EmitObject(path); else _=Compiler.EmitCSharp(new[]{path}); }).Message.ShouldContain("bitCast");
        }
        finally {Directory.Delete(directory,true);}
    }
}
