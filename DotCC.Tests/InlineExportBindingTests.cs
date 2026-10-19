using System;
using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class InlineExportBindingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_bound_export_selects_one_provider_without_merging_variants(bool link)
    {
        var dir=Path.Combine(Path.GetTempPath(), "dotcc-inline-binding-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var paths=new[] {Path.Combine(dir,"first.c"),Path.Combine(dir,"second.c")};
            File.WriteAllText(paths[0],"static inline int helper(int x) { static int count; return ++count+x; } int first(void) { return helper(10); }");
            File.WriteAllText(paths[1],"static inline int helper(int x) { static int count; return ++count+2*x; } int second(void) { return helper(10); }");
            var objects=paths.Select(path => Compiler.EmitObject(path)).ToArray();
            var bound=objects[0].Split('\n').Single(line=>line.StartsWith("//!!dotcc-obj inline:",StringComparison.Ordinal))["//!!dotcc-obj inline:".Length..].Split(' ')[0];
            if (!link)
                bound = System.Text.RegularExpressions.Regex.Match(
                    Compiler.EmitCSharp(paths, emit: EmitMode.ManagedLib),
                    @"static unsafe int (helper(?:__\w+)?)\(").Groups[1].Value;
            if(link)
                paths=paths.Select((path,index)=> {var obj=Path.ChangeExtension(path,".o");File.WriteAllText(obj,objects[index]);return obj;}).ToArray();
            string Emit(params string[] exports) => link
                ? Compiler.LinkObjects(paths,emit:EmitMode.ManagedLib,outputOptions:new(DeduplicateInline:true,ExportInline:exports))
                : Compiler.EmitCSharp(paths,emit:EmitMode.ManagedLib,outputOptions:new(DeduplicateInline:true,ExportInline:exports));
            Should.Throw<CompileException>(()=>Emit("helper")).Message.ShouldContain("ambiguous --export-inline");
            var output=Emit("helper="+bound);
            output.ShouldContain("int helper(int x)");
            output.ShouldContain("return helper(10)");
            System.Text.RegularExpressions.Regex.Matches(output, @"static unsafe int helper(?:__\w+)?\(").Count.ShouldBe(2);
            Should.Throw<CompileException>(()=>Emit("helper=missing")).Message.ShouldContain("matched no inline definitions");
            Should.Throw<CompileException>(()=>Emit("helper=helper*")).Message.ShouldContain("ambiguous --export-inline");
            Should.Throw<CompileException>(()=>Emit("helper="+bound,"helper")).Message.ShouldContain("ambiguous --export-inline");
        }
        finally {Directory.Delete(dir,true);}
    }
}
