using System;
using System.IO;
using DotCC.Frontends;
using DotCC.Ir;
using DotCC.Layout;
using Shouldly;
using Xunit;
namespace DotCC.Tests;
[Collection("ZigFrontend")]
public sealed class ZigQuotedOffsetPathR46Tests
{
    [Fact]
    public void A_quoted_dot_field_and_nested_designator_have_distinct_metadata_paths()
    {
        var dir=Path.Combine(Path.GetTempPath(),"r46-offset-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
        var path=Path.Combine(dir,"main.zig");File.WriteAllText(path,"const Inner=extern struct { pad:u8,b:u8 }; const S=extern struct { @\"a.b\":u32,a:Inner }; pub fn main() u8 { return 42; }");
        try
        {
            var ir=new ZigFrontend().BuildIr(new FrontendRequest(new[]{path}));
            var single=new OffsetOf(new CType.Named("S"),new[]{"a.b"},CType.UInt){Type=CType.SizeT};
            var nested=new OffsetOf(new CType.Named("S"),new[]{"a","b"},CType.UChar){Type=CType.SizeT};
            var document=ir.CreateOffsetDocument();
            var singleId=OffsetDocument.RequestName("S",single.Path);
            var nestedId=OffsetDocument.RequestName("S",nested.Path);
            var result=new {singleId,nestedId,aliases=singleId==nestedId};
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(result));
            document.Requests.Add(new OffsetRequest{Name=singleId,Aggregate="S",Path=new[]{"a.b"},Expected=0});
            var round=System.Linq.Enumerable.Single(OffsetDocument.ReadSource(document.Serialize()));
            var original=ir.Functions[0];
            ir.Functions[0]=original with {Body=new Block(new CStmt[]{new Return(new Cast(CType.UChar,new Binary(BinOp.Add,new Binary(BinOp.Sub,nested,single){Type=CType.SizeT},new LitInt("37",37){Type=CType.SizeT}){Type=CType.SizeT}){Type=CType.UChar})})};
            var backend=DotCC.Backends.CSharpBackend.Run(ir,publicTypes:true,relocatable:true,aggregateTokens:true);
            singleId.ShouldNotBe(nestedId);
            round.Requests[0].Path.ShouldBe(new[]{"a.b"});
        }
        finally {Directory.Delete(dir,true);}
    }

    [Fact]
    public void An_aggregate_field_starting_with_a_bracket_is_not_an_array_index()
    {
        var document=new OffsetDocument();var aggregate=new LayoutAggregate{Name="S"};
        aggregate.Fields.Add(new LayoutField{Name="[0]",Type="p:1:1"});document.Aggregates.Add("S",aggregate);
        var model=new OffsetLayoutModel(name=>document.Aggregates[name]);
        model.Offset("S",new[]{"[0]"}).ShouldBe(0);
    }

    [Fact]
    public void Ordinary_C_request_names_and_metadata_stay_byte_for_byte_compatible()
    {
        var d=new OffsetDocument();var a=new LayoutAggregate{Name="S"};a.Fields.Add(new LayoutField{Name="a",Type="p:1:1"});d.Aggregates.Add("S",a);
        var id=OffsetDocument.RequestName("S",new[]{"a"});id.ShouldBe("__DotccOffset_532E61");
        d.Requests.Add(new OffsetRequest{Name=id,Aggregate="S",Path=new[]{"a"},Expected=0});
        var text=d.Serialize();text.ShouldContain("request\t__DotccOffset_532E61\tUw==\tYQ==\t0\n");text.ShouldNotContain("request-path-v2");
        System.Linq.Enumerable.Single(OffsetDocument.ReadSource(text)).Serialize().ShouldBe(text);
    }

    [Theory]
    [InlineData("request-path-v3","3:a.b")]
    [InlineData("request-path-v2","-1:a")]
    [InlineData("request-path-v2","99:a")]
    [InlineData("request-path-v2","2:a")]
    [InlineData("request-path-v2","1:a!")]
    [InlineData("request-path-v2","x:a")]
    [InlineData("request-path-v2","0:")]
    [InlineData("request-path-v2","")]
    public void Malformed_or_unknown_path_records_fail_explicitly(string record,string payload)
    {
        var text=OffsetDocument.Start+"abi\t"+OffsetLayoutModel.Abi+"\n"+record+"\t__DotccOffset_v2_31\tUw==\t"+OffsetDocument.Encode(payload)+"\t0\n"+OffsetDocument.End+"\n";
        Should.Throw<OffsetLayoutException>(()=>System.Linq.Enumerable.ToArray(OffsetDocument.ReadSource(text)));
    }

    [Fact]
    public void Array_indices_keep_the_original_descriptor_semantics()
    {
        var a=new LayoutAggregate{Name="A"};a.Fields.Add(new LayoutField{Name="v",Type="a:4:p:4:4"});
        var model=new OffsetLayoutModel(name=>a);model.Offset("A",new[]{"v","[2]"}).ShouldBe(8);
        Should.Throw<OffsetLayoutException>(()=>model.Offset("A",new[]{"v","[bad]"}));
    }
}
