using System;
using System.IO;
using Shouldly;
using Xunit;
namespace DotCC.FunctionalTests;
[Collection("ZigFrontend")]
public sealed class ZigResultLocationWrapperExecutionTests
{
    private static readonly string[] Sources = {
        "fn pick(v:i24) ?u24 { return @bitCast(v); } pub fn main() u8 { return if(pick(-1).? == 0xffffff) 42 else 1; }",
        "fn pick(v:i24) error{Fail}!u24 { return @bitCast(v); } pub fn main() u8 { const n=pick(-1) catch return 2; return if(n==0xffffff)42 else 1; }",
        "fn pick(v:i24,ok:bool) ?u24 { return if(ok) @bitCast(v) else null; } pub fn main() u8 { return if(pick(-1,true).? == 0xffffff and pick(-1,false)==null)42 else 1; }",
        "fn pick(v:i24) ?u24 { return @as(?u24,@bitCast(v)); } pub fn main() u8 { return if(pick(-1).? == 0xffffff)42 else 1; }",
        "fn pick(v:?u24) i24 { return @bitCast(v.?); } pub fn main() u8 { return if(pick(0xffffff) == -1)42 else 1; }",
        "fn origin(v:u24)error{Fail}!u24{return v;} fn pick(v:u24)error{Fail}!i24{return @bitCast(try origin(v));} pub fn main()u8{const n=pick(0xffffff) catch return 2; return if(n == -1)42 else 1;}",
    };
    [Theory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    [InlineData(4, true)]
    [InlineData(5, false)]
    [InlineData(5, true)]

    public void Wrappers_keep_their_result_type_and_project_only_the_bitcast_payload(int variant, bool objects)
    {
        var directory=Path.Combine(Path.GetTempPath(),"dotcc-wrapper-result-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source=Path.Combine(directory,"main.zig"); File.WriteAllText(source,Sources[variant]);
            string emitted;
            if(objects){var fragment=Path.Combine(directory,"main.object.cs");File.WriteAllText(fragment,Compiler.EmitObject(source));emitted=Compiler.LinkObjects(new[]{fragment},emit:EmitMode.Csproj);}
            else emitted=Compiler.EmitCSharp(new[]{source},emit:EmitMode.Csproj);
            var result=FixtureRunner.CompileAndRunCapturingStreams(emitted,Array.Empty<string>());
            result.exit.ShouldBe(42); result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        }
        finally {Directory.Delete(directory,true);}
    }
}
