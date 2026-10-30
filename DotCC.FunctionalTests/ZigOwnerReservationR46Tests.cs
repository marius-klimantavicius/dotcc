using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;
namespace DotCC.FunctionalTests;
[Collection("ZigFrontend")]
public sealed class ZigOwnerReservationR46Tests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Different_Zig_owners_with_equal_shapes_do_not_coalesce(bool reverse)
    {
        var dir=Path.Combine(Path.GetTempPath(),"r46-owner-reservation-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
        try
        {
            var a=Path.Combine(dir,"a.zig");var b=Path.Combine(dir,"b.zig");
            File.WriteAllText(a,"const A=struct { A:u8 }; extern fn other()u8;pub fn main()u8{const x=A{.A=20};return x.A+other();}");
            File.WriteAllText(b,"const B=struct { A:u8 }; export fn other()u8{const x=B{.A=22};return x.A;}");
            var ao=Compiler.EmitObject(a);var bo=Compiler.EmitObject(b);
            var ag=AggregateIdentities.Read(ao)!;var bg=AggregateIdentities.Read(bo)!;
            var an=ag.Single(n=>n.Name=="A");var bn=bg.Single(n=>n.Name=="B");an.Shape.ShouldBe(bn.Shape);
            ao.ShouldContain("__zig_member_41");bo.ShouldContain("public byte A;");
            var plans=AggregateIdentities.Plan(new[]{("a.zig",ag),("b.zig",bg)});
            plans[0].Count.ShouldBe(0);plans[1].Count.ShouldBe(0);
            var ap=Path.Combine(dir,"a.object.cs");var bp=Path.Combine(dir,"b.object.cs");File.WriteAllText(ap,ao);File.WriteAllText(bp,bo);
            var linked=Compiler.LinkObjects(reverse?new[]{bp,ap}:new[]{ap,bp},emit:EmitMode.Csproj);
            var result=FixtureRunner.CompileAndRunCapturingStreams(linked,Array.Empty<string>());result.exit.ShouldBe(42);
            result.stdout.ShouldBeEmpty();result.stderr.ShouldBeEmpty();
        }
        finally{Directory.Delete(dir,true);}
    }
}
