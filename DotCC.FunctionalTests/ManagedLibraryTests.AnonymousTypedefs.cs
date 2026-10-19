using System;
using System.IO;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Anonymous_typedef_keeps_the_public_owning_consumer_type(bool objects)
    {
        var directory=Path.Combine(Path.GetTempPath(),"dotcc-public-typedef-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path=Path.Combine(directory,"api.c");
            File.WriteAllText(path,"typedef struct { long value; } PublicState; PublicState make(void) { PublicState s={42}; return s; }");
            string text;
            var options=new CSharpOutputOptions(NestTypes:true);
            if(objects)
            {
                var obj=path+".cs";
                File.WriteAllText(obj,Compiler.EmitObject(path));
                text=Compiler.LinkObjects(new[]{obj},emit:EmitMode.ManagedLib,className:"Api",namespaceName:"Public",outputOptions:options);
            }
            else text=Compiler.EmitCSharp(new[]{path},emit:EmitMode.ManagedLib,className:"Api",namespaceName:"Public",outputOptions:options);
            var references=RuntimeReferences();
            var image=Compile("TypedefApi_"+Guid.NewGuid().ToString("N"),text,references);
            references.Add(MetadataReference.CreateFromImage(image));
            var consumer=Compile("TypedefConsumer_"+Guid.NewGuid().ToString("N"),"""
                public static class Consumer {
                    public static long Run() { Public.Api.PublicState state=Public.Api.make(); return state.value; }
                }
                """,references);
            var context=new AssemblyLoadContext("public-typedef-"+Guid.NewGuid(),isCollectible:false);
            var library=context.LoadFromStream(new MemoryStream(image));
            library.GetType("Public.Api+PublicState",true)!.IsNestedPublic.ShouldBeTrue();
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer")!.GetMethod("Run")!.Invoke(null,null).ShouldBe(42L);
        }
        finally { Directory.Delete(directory,true); }
    }
}
