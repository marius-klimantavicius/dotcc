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
    [InlineData(false, SourceSplit.None, RuntimeProfile.C)]
    [InlineData(false, SourceSplit.Function, RuntimeProfile.C)]
    [InlineData(false, SourceSplit.Size, RuntimeProfile.C)]
    [InlineData(true, SourceSplit.None, RuntimeProfile.C)]
    [InlineData(true, SourceSplit.Function, RuntimeProfile.C)]
    [InlineData(true, SourceSplit.Size, RuntimeProfile.C)]
    [InlineData(false, SourceSplit.Function, RuntimeProfile.All)]
    public void Nested_translations_can_share_a_consumer_namespace_without_type_or_alias_collisions(bool link, SourceSplit split, RuntimeProfile runtime)
    {
        var dir=Path.Combine(Path.GetTempPath(),"dotcc-nested-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var path=Path.Combine(dir,"main.c");
            File.WriteAllText(path,"""
                #include <stddef.h>
                #include <stdlib.h>
                #include <errno.h>
                typedef struct { int first; long second; } Record;
                enum Mode { Good=7, Bad=8 }; int Mode=2;
                typedef int (*Callback)(int);
                static int count;
                int add(int n) { return n + ++count; }
                Callback get(void) { return add; }
                int offset(void) { return offsetof(Record, second); }
                int size(void) { return sizeof(Record); }
                int mode(void) { return Good+Mode; }
                int set_error(int n) { errno=n; return errno; }
                int get_error(void) { return errno; }
                int negative(int n) { return abs(n); }
                """);
            if(link) { var obj=Path.Combine(dir,"main.o"); File.WriteAllText(obj,Compiler.EmitObject(path)); path=obj; }
            var syntax=new System.Collections.Generic.List<SyntaxTree>();
            foreach(var owner in new[]{"First","Second"})
            {
                var options=new CSharpOutputOptions(NestTypes:true,Runtime:runtime);
                var files=link
                    ? Compiler.LinkObjectFiles(new[]{path},emit:EmitMode.ManagedLib,className:owner,namespaceName:"Shared",split:split,splitSize:400,outputOptions:options)
                    : Compiler.EmitCSharpFiles(new[]{path},emit:EmitMode.ManagedLib,className:owner,namespaceName:"Shared",split:split,splitSize:400,outputOptions:options);
                var text=string.Join("\n",files.Values);
                text.ShouldNotContain("global using");
                text.Split("static unsafe class "+owner+"FunctionPointers").Length.ShouldBe(2);
                text.ShouldContain("static unsafe class "+owner+"Globals");
                if(runtime==RuntimeProfile.C) { text.ShouldNotContain("// ---- Zig"); text.ShouldNotContain("// ---- Slice.cs"); }
                syntax.AddRange(files.Select(f=>ParseSource(f.Value,path:owner+"/"+f.Key)));
            }
            syntax.Add(ParseSource("""
                namespace Shared;
                public static unsafe class Consumer {
                    public static int Run() {
                        First.Record a=default; Second.Record b=default;
                        a.second=1; b.second=2;
                        if(First.get()!=First.FirstFunctionPointers.add || Second.get()!=Second.SecondFunctionPointers.add) return -1;
                        if(First.FirstFunctionPointers.add(10)!=11 || Second.SecondFunctionPointers.add(20)!=21 || First.add(10)!=12) return -2;
                        First.set_error(17); Second.set_error(19);
                        if(First.get_error()!=17 || Second.get_error()!=19) return -3;
                        if(First.offset()!=8 || Second.offset()!=8 || First.size()!=16 || Second.size()!=16) return -4;
                        if(First.mode()!=9 || Second.mode()!=9 || First.negative(-42)!=42) return -5;
                        return (int)(a.second+b.second);
                    }
                }
                """, cancellationToken:TestContext.Current.CancellationToken));
            var compilation=CSharpCompilation.Create("Nested"+Guid.NewGuid().ToString("N"),syntax,RuntimeReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,allowUnsafe:true));
            using var stream=new MemoryStream();
            var result=compilation.Emit(stream,cancellationToken:TestContext.Current.CancellationToken);
            result.Success.ShouldBeTrue(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
            stream.Position=0;
            var assembly=new AssemblyLoadContext("nested"+Guid.NewGuid(),isCollectible:false).LoadFromStream(stream);
            assembly.GetType("Shared.Consumer")!.GetMethod("Run")!.Invoke(null,null).ShouldBe(3);
            assembly.GetTypes().Where(t=>t.Namespace=="Shared" && !t.IsNested).Select(t=>t.Name).Order().ToArray()
                .ShouldBe(new[]{"Consumer","First","Second"});
        }
        finally { Directory.Delete(dir,true); }
    }
}
