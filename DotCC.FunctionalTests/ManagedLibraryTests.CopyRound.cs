using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false,false)]
    [InlineData(true,false)]
    [InlineData(false,true)]
    [InlineData(true,true)]
    public void Generic_copy_round_primitives_work_with_direct_and_object_link(bool objectLink,bool nested)
    {
        var fixture=FixtureRunner.Discover().Single(row=>row.name=="libc-copy-round");
        var fragment=Path.Combine(Path.GetTempPath(),"dotcc-copy-round-"+Guid.NewGuid().ToString("N")+".cs");
        try
        {
            var options=new CSharpOutputOptions(NestTypes:nested,Runtime:RuntimeProfile.C);
            string emitted;
            if(objectLink)
            {
                File.WriteAllText(fragment,Compiler.EmitObject(fixture.sources.Single()));
                emitted=Compiler.LinkObjects(new[]{fragment},emit:EmitMode.ManagedLib,outputOptions:options);
            }
            else emitted=Compiler.EmitCSharp(fixture.sources,emit:EmitMode.ManagedLib,outputOptions:options);
            var references=RuntimeReferences();
            var library=Compile("CopyRound_"+Guid.NewGuid().ToString("N"),emitted,references);
            references.Add(MetadataReference.CreateFromImage(library));
            var consumer=Compile("CopyRoundConsumer_"+Guid.NewGuid().ToString("N"),"""
                public static class Consumer {
                    public static int Run() {
                        System.GC.Collect(2,System.GCCollectionMode.Forced,true,true);
                        return DotCcLib.probe();
                    }
                }
                """,references);
            var context=new AssemblyLoadContext("copy-round-"+Guid.NewGuid().ToString("N"),isCollectible:false);
            context.LoadFromStream(new MemoryStream(library));
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer",true)!.GetMethod("Run")!.Invoke(null,null).ShouldBe(0);
        }
        finally { File.Delete(fragment); }
    }
}
