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
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public void Runtime_function_addresses_keep_the_C_ABI_in_a_separate_public_consumer(bool objects, bool reverse, bool translatedProvider)
    {
        var fixture=Path.Combine(AppContext.BaseDirectory,"Fixtures","runtime-function-address-a21");
        var directory=Path.Combine(Path.GetTempPath(),"dotcc-runtime-address-a21-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var sources=new[]{"first.c","second.c"}.Select(name=>Path.Combine(fixture,name)).ToArray();
            if(translatedProvider) sources=sources.Append(Path.Combine(fixture,"native.c.txt")).ToArray();
            var options=new CSharpOutputOptions(NestTypes:true,Runtime:RuntimeProfile.C);
            string text;
            if(objects)
            {
                var fragments=sources.Select((source,index)=>{
                    var fragment=Path.Combine(directory,index+".o");
                    File.WriteAllText(fragment,Compiler.EmitObject(source,includeDirs:[fixture]));
                    return fragment;
                }).ToArray();
                if(reverse) Array.Reverse(fragments);
                text=Compiler.LinkObjects(fragments,emit:EmitMode.ManagedLib,className:"Api",namespaceName:"Binding",outputOptions:options);
            }
            else text=Compiler.EmitCSharp(sources,includeDirs:[fixture],emit:EmitMode.ManagedLib,className:"Api",namespaceName:"Binding",outputOptions:options);
            if(!translatedProvider) text+="\n"+File.ReadAllText(Path.Combine(fixture,"host.cs.txt")).Replace("namespace Binding;", "");
            var references=RuntimeReferences();
            var image=Compile("RuntimeAddressApi_"+Guid.NewGuid().ToString("N"),text,references);
            references.Add(MetadataReference.CreateFromImage(image));
            var consumer=Compile("RuntimeAddressConsumer_"+Guid.NewGuid().ToString("N"),File.ReadAllText(Path.Combine(fixture,"consumer.cs.txt")),references);
            var context=new AssemblyLoadContext("runtime-address-a21-"+Guid.NewGuid(),isCollectible:false);
            var library=context.LoadFromStream(new MemoryStream(image));
            library.GetType("Binding.Api+PublicRecord",true)!.IsNestedPublic.ShouldBeTrue();
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer")!.GetMethod("Run")!.Invoke(null,null).ShouldBe(0);
        }
        finally { Directory.Delete(directory,true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Runtime_pointer_return_adapter_keeps_instance_owner_and_disposal(bool objects)
    {
        var directory=Path.Combine(Path.GetTempPath(),"dotcc-instance-runtime-a21-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path=Path.Combine(directory,"probe.c");
            File.WriteAllText(path,"""
                #include <netdb.h>
                typedef struct protoent *(*Lookup)(const char *);
                Lookup address(void) { return getprotobyname; }
                """);
            var options=new CSharpOutputOptions(NestTypes:true,Runtime:RuntimeProfile.C,InstanceMethods:true);
            string text;
            if(objects)
            {
                var fragment=path+".o";
                File.WriteAllText(fragment,Compiler.EmitObject(path,outputOptions:new(InstanceMethods:true)));
                text=Compiler.LinkObjects([fragment],emit:EmitMode.ManagedLib,className:"Api",namespaceName:"Binding",outputOptions:options);
            }
            else text=Compiler.EmitCSharp([path],emit:EmitMode.ManagedLib,className:"Api",namespaceName:"Binding",outputOptions:options);
            var references=RuntimeReferences();
            var image=Compile("InstanceRuntimeApi_"+Guid.NewGuid().ToString("N"),text,references);
            references.Add(MetadataReference.CreateFromImage(image));
            var consumer=Compile("InstanceRuntimeConsumer_"+Guid.NewGuid().ToString("N"),"""
                public static unsafe class Consumer {
                    public static int Run() {
                        using var a=new Binding.Api(); using var b=new Binding.Api();
                        var lookup=a.address();
                        if(lookup!=b.address()) return 1;
                        using(b.__DotCcEnter()) {
                            var prior=Binding.Api.Libc.RuntimeContext.Current;
                            byte* name=stackalloc byte[4] {116,99,112,0};
                            var record=lookup(a,name);
                            if(record==null || record->p_proto!=6 || !object.ReferenceEquals(prior,Binding.Api.Libc.RuntimeContext.Current)) return 2;
                        }
                        a.Dispose();
                        try { lookup(a,null); return 3; } catch(System.ObjectDisposedException) {}
                        return 0;
                    }
                }
                """,references);
            var context=new AssemblyLoadContext("instance-runtime-a21-"+Guid.NewGuid(),isCollectible:false);
            context.LoadFromStream(new MemoryStream(image));
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer")!.GetMethod("Run")!.Invoke(null,null).ShouldBe(0);
        }
        finally { Directory.Delete(directory,true); }
    }
}
