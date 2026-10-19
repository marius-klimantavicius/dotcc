using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DotCC.FunctionalTests;

public sealed partial class ManagedLibraryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Bound_inline_exports_preserve_both_providers_state_and_addresses(bool link)
    {
        var dir=Path.Combine(Path.GetTempPath(),"dotcc-bound-inline-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var paths=new[] {Path.Combine(dir,"first.c"),Path.Combine(dir,"second.c")};
            for(var i=0;i<2;++i)
            {
                string prefix=i==0?"first":"second";
                File.WriteAllText(paths[i],"static inline int helper(int x) { static int count; return ++count+"+(i+1)+"*x; }\n"
                    +"typedef int (*Callback)(int); Callback "+prefix+"_pointer(void) { return helper; } int "+prefix+"(void) { return helper(10); }");
            }
            var objects=paths.Select(path=>Compiler.EmitObject(path)).ToArray();
            var bound=objects[0].Split('\n').Single(line=>line.StartsWith("//!!dotcc-obj inline:",StringComparison.Ordinal))
                ["//!!dotcc-obj inline:".Length..].Split(' ')[0];
            if (!link)
                bound = System.Text.RegularExpressions.Regex.Match(
                    Compiler.EmitCSharp(paths, emit: EmitMode.ManagedLib),
                    @"static unsafe int (helper(?:__\w+)?)\(").Groups[1].Value;
            if(link) paths=paths.Select((path,index)=>{var obj=Path.ChangeExtension(path,".o");File.WriteAllText(obj,objects[index]);return obj;}).ToArray();
            var options=new CSharpOutputOptions(NestTypes:true,Runtime:RuntimeProfile.C,DeduplicateInline:true,
                ExportInline:new[] {"helper="+bound});
            var files=link
                ? Compiler.LinkObjectFiles(paths,emit:EmitMode.ManagedLib,className:"Api",namespaceName:"Bound",outputOptions:options)
                : Compiler.EmitCSharpFiles(paths,emit:EmitMode.ManagedLib,className:"Api",namespaceName:"Bound",outputOptions:options);
            var references=RuntimeReferences();
            var compilation=CSharpCompilation.Create("BoundLibrary"+Guid.NewGuid().ToString("N"),
                files.Select(file=>ParseSource(file.Value,path:file.Key)),references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,allowUnsafe:true));
            using var image=new MemoryStream();
            var result=compilation.Emit(image,cancellationToken:TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
            references.Add(MetadataReference.CreateFromImage(image.ToArray()));
            var consumer=Compile("BoundConsumer"+Guid.NewGuid().ToString("N"),"""
                using Bound;
                public static unsafe class Consumer {
                    public static bool Check() {
                        int exported=Api.helper(3), first=Api.first(), second=Api.second();
                        var a=Api.first_pointer(); var b=Api.second_pointer();
                        delegate* managed<int,int> selected=&Api.helper;
                        int bv=b(3), av=a(5);
                        return exported==4 && first==12 && second==21 && bv==8 && av==8
                            && a==selected && a!=b;
                    }
                }
                """,references);
            var context=new AssemblyLoadContext("bound-consumer"+Guid.NewGuid(),isCollectible:false);
            image.Position=0;context.LoadFromStream(image);
            context.LoadFromStream(new MemoryStream(consumer)).GetType("Consumer")!.GetMethod("Check")!.Invoke(null,null).ShouldBe(true);
        }
        finally {Directory.Delete(dir,true);}
    }
}
