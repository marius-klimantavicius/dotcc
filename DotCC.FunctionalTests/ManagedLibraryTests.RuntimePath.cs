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
    public void C_Path_function_does_not_shadow_embedded_framework_path(bool objectLink,bool nested)
    {
        var fixture=FixtureRunner.Discover().Single(row=>row.name=="runtime-path-shadow");
        var fragment=Path.Combine(Path.GetTempPath(),"dotcc-runtime-path-"+Guid.NewGuid().ToString("N")+".cs");
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
            var library=Compile("RuntimePath_"+Guid.NewGuid().ToString("N"),emitted,references);
            references.Add(MetadataReference.CreateFromImage(library));
            var nativeImports=nested?"DotCcLib.NativeImports":"NativeImports";
            var libc=nested?"DotCcLib.Libc":"Libc";
            var consumer=Compile("RuntimePathConsumer_"+Guid.NewGuid().ToString("N"),$$"""
                public static unsafe class Consumer {
                    public static int Run() {
                        if(DotCcLib.probe()!=0 || DotCcLib.Path(41)!=42)return -1;
                        // Exercise the native-loader Path.Combine branch without
                        // loading any actual library or invoking foreign code.
                        var missing="dotcc-no-library-"+System.Guid.NewGuid().ToString("N");
                        if({{nativeImports}}.LoadLibrary(missing,new[]{System.IO.Path.GetTempPath()})!=System.IntPtr.Zero)return -2;
                        byte* name={{libc}}.tmpnam(null);
                        return name!=null && name[0]!=0?42:-3;
                    }
                }
                """,references);
            var context=new AssemblyLoadContext("runtime-path-"+Guid.NewGuid().ToString("N"),isCollectible:false);
            context.LoadFromStream(new MemoryStream(library));
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer",true)!.GetMethod("Run")!.Invoke(null,null).ShouldBe(42);
        }
        finally { File.Delete(fragment); }
    }
}
