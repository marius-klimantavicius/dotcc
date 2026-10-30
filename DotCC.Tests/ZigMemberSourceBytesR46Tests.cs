using System;
using System.IO;
using System.Text;
using Shouldly;
using Xunit;
namespace DotCC.Tests;
[Collection("ZigFrontend")]
public sealed class ZigMemberSourceBytesR46Tests
{
    [Theory]
    [InlineData(false,false)] [InlineData(false,true)]
    [InlineData(true,false)] [InlineData(true,true)]
    public void Root_and_import_raw_invalid_utf8_are_explicit_errors(bool imported,bool objects)
    {
        InDirectory(dir=> {
            var prefix=Encoding.UTF8.GetBytes("pub const S=struct { @\"");
            var suffix=Encoding.UTF8.GetBytes("\":u8 }; "+(imported?"":"pub fn main()u8{return @sizeOf(S);}"));
            var bytes=new byte[prefix.Length+1+suffix.Length];prefix.CopyTo(bytes,0);bytes[prefix.Length]=255;suffix.CopyTo(bytes,prefix.Length+1);
            var path=Path.Combine(dir,"main.zig");File.WriteAllBytes(Path.Combine(dir,imported?"provider.zig":"main.zig"),bytes);
            if(imported)File.WriteAllText(path,"const S=@import(\"provider.zig\").S;pub fn main()u8{return @sizeOf(S);}");
            var error=Should.Throw<CompileException>(()=>Emit(path,objects));
            error.Message.ShouldContain("invalid UTF-8 source bytes FF");
        });
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Genuine_replacement_codepoint_is_valid_and_distinct_from_invalid_escaped_bytes(bool objects)
    {
        InDirectory(dir=> {
            var path=Path.Combine(dir,"main.zig");
            File.WriteAllText(path,"const S=struct { @\"�\":u8 };pub fn main()u8{const x=S{.@\"\\xef\\xbf\\xbd\"=42};return x.@\"�\";}");
            Emit(path,objects).ShouldContain("__zig_member_EFBFBD");
            File.WriteAllText(path,"const S=struct { @\"\\xff\":u8 };pub fn main()u8{return @sizeOf(S);}");
            var error=Should.Throw<CompileException>(()=>Emit(path,objects));
            error.Message.ShouldContain("invalid UTF-8 bytes: FF");
        });
    }

    [Theory]
    [InlineData(false,false)] [InlineData(false,true)]
    [InlineData(true,false)] [InlineData(true,true)]
    public void Utf8_bom_is_supported_for_roots_and_imports(bool imported,bool objects)
    {
        InDirectory(dir=> {
            var path=Path.Combine(dir,"main.zig");
            var source=imported?"pub fn answer()u8{return 42;}":"pub fn main()u8{return 42;}";
            File.WriteAllText(Path.Combine(dir,imported?"provider.zig":"main.zig"),source,new UTF8Encoding(true));
            if(imported)File.WriteAllText(path,"const p=@import(\"provider.zig\");pub fn main()u8{return p.answer();}");
            Emit(path,objects).ShouldNotBeEmpty();
        });
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Utf16_bom_is_not_auto_detected_as_a_Zig_source_encoding(bool objects)
    {
        InDirectory(dir=> {var path=Path.Combine(dir,"main.zig");File.WriteAllText(path,"pub fn main()u8{return 42;}",Encoding.Unicode);
            Should.Throw<CompileException>(()=>Emit(path,objects)).Message.ShouldContain("invalid UTF-8 source bytes");});
    }

    [Theory]
    [InlineData("const S=struct {pub fn a_b()u8{return 1;}};pub fn main()u8{return S.@\"a?b\"();}","quoted method 'a?b'")]
    [InlineData("const S=struct {pub const a_b:u8=1;};pub fn main()u8{return S.@\"a?b\";}","quoted declaration member 'a?b'")]
    public void Unmigrated_quoted_declarations_cannot_resolve_a_normalized_spelling(string source,string diagnostic)
        => InDirectory(dir=>{var p=Path.Combine(dir,"main.zig");File.WriteAllText(p,source);Should.Throw<CompileException>(()=>Emit(p,true)).Message.ShouldContain(diagnostic);});

    [Theory]
    [InlineData("fn f(name:[]const u8)bool{const selected=name[0..3];return @hasField(S,selected);}pub fn main()u8{return if(f(\"a-b\"))42 else 1;}")]
    [InlineData("var calls:u8=0;fn f()[]const u8{calls+=1;return \"a-b\";}pub fn main()u8{const selected=f()[0..3];return if(@hasField(S,selected))42 else 1;}")]
    public void Literal_name_recorder_does_not_cache_runtime_or_effectful_slices(string body)
        => InDirectory(dir=>{var p=Path.Combine(dir,"main.zig");File.WriteAllText(p,"const S=struct { @\"a-b\":u8 };"+body);Should.Throw<CompileException>(()=>Emit(p,true)).Message.ShouldContain("name must be a comptime string");});

    private static string Emit(string path,bool objects)=>objects?Compiler.EmitObject(path):Compiler.EmitCSharp(new[]{path});
    private static void InDirectory(Action<string> action)
    {
        var dir=Path.Combine(Path.GetTempPath(),"r46-source-bytes-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
        try{action(dir);}finally{Directory.Delete(dir,true);}
    }
}
