using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DotCC.Frontends;
using DotCC.Ir;
using Shouldly;
using Xunit;
namespace DotCC.FunctionalTests;
[Collection("ZigFrontend")]
public sealed class ZigQuotedOffsetPathR46ExecutionTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Forced_offset_requests_distinguish_a_dot_field_from_a_nested_path(bool objects)
    {
        var dir=Path.Combine(Path.GetTempPath(),"r46-offset-run-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
        try
        {
            var path=Path.Combine(dir,"main.zig");File.WriteAllText(path,"const Inner=extern struct {pad:u8,b:u8};const S=extern struct {@\"a.b\":u32,a:Inner};pub fn main()u8{return 42;}");
            var ir=new ZigFrontend().BuildIr(new FrontendRequest(new[]{path}));
            var single=new OffsetOf(new CType.Named("S"),new[]{"a.b"},CType.UInt){Type=CType.SizeT};
            var nested=new OffsetOf(new CType.Named("S"),new[]{"a","b"},CType.UChar){Type=CType.SizeT};
            var original=ir.Functions[0];ir.Functions[0]=original with {Body=new Block(new CStmt[]{new Return(new Cast(CType.UChar,
                new Binary(BinOp.Add,new Binary(BinOp.Sub,nested,single){Type=CType.SizeT},new LitInt("37",37){Type=CType.SizeT}){Type=CType.SizeT}){Type=CType.UChar})})};
            var cg=DotCC.Backends.CSharpBackend.Run(ir,publicTypes:true,relocatable:objects,aggregateTokens:objects);
            string emitted;
            if(objects)
            {
                var serialize=typeof(Compiler).GetMethod("SerializeFragment",BindingFlags.Static|BindingFlags.NonPublic)!;
                var parameters=serialize.GetParameters();var args=parameters.Select(p=>p.HasDefaultValue?p.DefaultValue:null).ToArray();
                for(int i=0;i<parameters.Length;i++)args[i]=parameters[i].Name switch {
                    "functions"=>cg.Functions,"typeDecls"=>cg.TypeDeclarations,"aliases"=>cg.Aliases,"globals"=>cg.Globals,
                    "mainArity"=>cg.MainArity,"importSpecs"=>Array.Empty<(string,string)>(),"defNames"=>new[]{"main"},
                    "functionSources"=>cg.FunctionSources,"aggregateMetadata"=>cg.AggregateMetadata,
                    "aggregateIdentities"=>ir.AggregateIdentityGraph(cg.UsedFunctionAddresses),_=>args[i]};
                var fragment=Path.Combine(dir,"main.object.cs");File.WriteAllText(fragment,(string)serialize.Invoke(null,args)!);
                emitted=Compiler.LinkObjects(new[]{fragment},emit:EmitMode.Csproj);
            }
            else emitted="public static unsafe class Program { public static int Main()=>main();\n"+cg.Functions+"\n}\n"+cg.Aliases+cg.Structs;
            var result=FixtureRunner.CompileAndRunCapturingStreams(emitted,Array.Empty<string>());
            result.exit.ShouldBe(42);result.stdout.ShouldBeEmpty();result.stderr.ShouldBeEmpty();
        }
        finally{Directory.Delete(dir,true);}
    }
}
