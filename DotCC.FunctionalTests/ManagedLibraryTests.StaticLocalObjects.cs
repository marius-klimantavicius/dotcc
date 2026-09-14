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
    public void Static_locals_in_multiple_units_keep_independent_storage(bool objectLink,bool nested)
    {
        var fixture=FixtureRunner.Discover().Single(row=>row.name=="static-local-multiple-units");
        var directory=Path.Combine(Path.GetTempPath(),"dotcc-static-units-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string emitted;
            var options=new CSharpOutputOptions{NestTypes=nested};
            if(objectLink)
            {
                var fragments=fixture.sources.Select((source,index)=>{
                    var path=Path.Combine(directory,index+".obj.cs");File.WriteAllText(path,Compiler.EmitObject(source));return path;
                }).ToArray();
                emitted=Compiler.LinkObjects(fragments,emit:EmitMode.ManagedLib,outputOptions:options);
            }
            else emitted=Compiler.EmitCSharp(fixture.sources,emit:EmitMode.ManagedLib,outputOptions:options);
            var references=RuntimeReferences();
            var library=Compile("StaticUnits_"+Guid.NewGuid().ToString("N"),emitted,references);
            references.Add(MetadataReference.CreateFromImage(library));
            var consumer=Compile("StaticUnitsConsumer_"+Guid.NewGuid().ToString("N"),"""
                public static unsafe class Consumer {
                    public static int Run() {
                        int* a=DotCcLib.left_buffer();int* b=DotCcLib.right_buffer();
                        if(a==b || a[0]!=0 || b[0]!=0)return -1;
                        a[0]=7;b[1]=9;
                        byte* left=DotCcLib.left_label();byte* right=DotCcLib.right_label();
                        if(left==right || left[0]!='l' || right[0]!='r')return -2;
                        for(int pass=0;pass<3;++pass){
                            System.GC.Collect(2,System.GCCollectionMode.Forced,true,true);
                            System.GC.WaitForPendingFinalizers();
                            if(DotCcLib.left_buffer()!=a || DotCcLib.right_buffer()!=b || a[0]!=7 || a[1]!=0 || b[0]!=0 || b[1]!=9)return -3;
                            if(DotCcLib.left_label()!=left || DotCcLib.right_label()!=right)return -4;
                        }
                        if(DotCcLib.left_tick()!=2 || DotCcLib.right_tick()!=101 || DotCcLib.left_tick()!=3 || DotCcLib.right_tick()!=102)return -5;
                        if(DotCcLib.left_record()!=6 || DotCcLib.right_record()!=51)return -6;
                        int* x=DotCcLib.left_alias();int* y=DotCcLib.right_alias();
                        return x!=y && x[0]==11 && x[1]==12 && y[0]==21 && y[1]==22?42:-7;
                    }
                }
                """,references);
            var context=new AssemblyLoadContext("static-units-"+Guid.NewGuid().ToString("N"),isCollectible:false);
            context.LoadFromStream(new MemoryStream(library));
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer",true)!.GetMethod("Run")!.Invoke(null,null).ShouldBe(42);
        }
        finally { Directory.Delete(directory,true); }
    }
}
