using System;
using System.IO;
using Shouldly;
using Xunit;
namespace DotCC.FunctionalTests;
[Collection("ZigFrontend")]
public sealed class ZigUnsignedEnumStorageExecutionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void High_unsigned_enum_values_keep_their_storage_bits(bool objects)
    {
        var directory=Path.Combine(Path.GetTempPath(),"dotcc-unsigned-enum-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var source=Path.Combine(directory,"main.zig");
            File.WriteAllText(source,"const Limit=enum(u64){ zero=0, high=0x8000000000000000, unlimited=0xffffffffffffffff, _ }; pub fn main() u8 { const h:Limit=.high; const m:Limit=.unlimited; if (@intFromEnum(h) != 0x8000000000000000) return 1; if (@intFromEnum(m) != 0xffffffffffffffff) return 2; return 42; }");
            string emitted;
            if(objects){var fragment=Path.Combine(directory,"main.object.cs"); File.WriteAllText(fragment,Compiler.EmitObject(source)); emitted=Compiler.LinkObjects(new[]{fragment},emit:EmitMode.Csproj);}
            else emitted=Compiler.EmitCSharp(new[]{source},emit:EmitMode.Csproj);
            var result=FixtureRunner.CompileAndRunCapturingStreams(emitted,Array.Empty<string>());
            result.exit.ShouldBe(42); result.stdout.ShouldBeEmpty(); result.stderr.ShouldBeEmpty();
        }
        finally {Directory.Delete(directory,true);}
    }
}
