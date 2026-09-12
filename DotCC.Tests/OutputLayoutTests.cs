using System;
using System.IO;
using Shouldly;
using Xunit;

namespace DotCC.Tests;

public sealed class OutputLayoutTests
{
    private static void Sources(Action<string, string> action)
    {
        var dir=Path.Combine(Path.GetTempPath(),"dotcc-layout-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var c=Path.Combine(dir,"main.c"); var zig=Path.Combine(dir,"main.zig");
            File.WriteAllText(c,"int main(void) { return 0; }");
            File.WriteAllText(zig,"pub fn main() u8 { return 0; }");
            action(c,zig);
        }
        finally { Directory.Delete(dir,true); }
    }
    [Fact]
    public void Runtime_selection_uses_source_and_object_language_provenance() => Sources((c,zig)=>
    {
        var auto=new CSharpOutputOptions(Runtime:RuntimeProfile.Auto);
        var onlyC=new CSharpOutputOptions(Runtime:RuntimeProfile.C);
        Compiler.EmitCSharp(new[]{c},outputOptions:auto).ShouldNotContain("// ---- Zig");
        Compiler.EmitCSharp(new[]{c}).ShouldContain("// ---- Zig");
        Compiler.EmitCSharp(new[]{zig},outputOptions:auto).ShouldContain("// ---- Zig");
        Should.Throw<CompileException>(()=>Compiler.EmitCSharp(new[]{zig},outputOptions:onlyC)).Message.ShouldContain("C-only");
        foreach(var path in new[]{c,zig})
        {
            var obj=Path.ChangeExtension(path,".obj"); File.WriteAllText(obj,Compiler.EmitObject(path));
            var result=Compiler.LinkObjects(new[]{obj},outputOptions:auto);
            if(path==c) { result.ShouldNotContain("// ---- Zig"); Compiler.LinkObjects(new[]{obj},outputOptions:onlyC); }
            else { result.ShouldContain("// ---- Zig"); Should.Throw<CompileException>(()=>Compiler.LinkObjects(new[]{obj},outputOptions:onlyC)); }
        }
        var legacy=Path.ChangeExtension(c,".old");
        File.WriteAllText(legacy,Compiler.EmitObject(c).Replace("//!!dotcc-obj source-language:c\n",""));
        Compiler.LinkObjects(new[]{legacy},outputOptions:auto).ShouldContain("// ---- Zig");
        Should.Throw<CompileException>(()=>Compiler.LinkObjects(new[]{legacy},outputOptions:onlyC)).Message.ShouldContain("older objects");
    });
    [Fact]
    public void Final_layout_options_require_a_final_library_for_nesting() => Sources((c,zig)=>
    {
        var nested=new CSharpOutputOptions(NestTypes:true);
        Should.Throw<CompileException>(()=>Compiler.EmitCSharp(new[]{c},outputOptions:nested)).Message.ShouldContain("library");
        Should.Throw<CompileException>(()=>Compiler.EmitCSharp(new[]{c},emit:EmitMode.Object,outputOptions:nested)).Message.ShouldContain("link time");
        Should.Throw<CompileException>(()=>Compiler.EmitCSharp(new[]{c},emit:EmitMode.Object,outputOptions:new(Runtime:RuntimeProfile.C))).Message.ShouldContain("link time");
    });
}
