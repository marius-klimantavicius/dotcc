using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotCC.Backends;
using DotCC.Frontends;
using DotCC.Ir;
using Shouldly;
using Xunit;
namespace DotCC.Tests;
[Collection("ZigFrontend")]
public sealed class ZigStorageCallProvenanceR49Tests
{
    [Fact]
    public void Resolved_extern_convention_survives_without_a_callee_symbol_and_managed_calls_keep_their_flag()
    {
        var dir=Path.Combine(Path.GetTempPath(),"r49-call-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
        try {
            var path=Path.Combine(dir,"main.zig");
            File.WriteAllText(path,"extern fn abs(x:c_int)c_int; fn managed(x:c_int)c_int{return x;}pub fn main()u8{_=abs(-7);_=managed(3);return 0;}");
            var ir=new ZigFrontend().BuildIr(new FrontendRequest(new[]{path}));
            var calls=ir.Functions.Single(f=>f.Sym.Name=="main").Body.Stmts.OfType<ExprStmt>().Select(s=>s.Expr).SelectMany(Calls).ToArray();
            var native=calls.Single(c=>c.Callee=="abs");native.CalleeSym.ShouldBeNull();native.CalleeType.ShouldNotBeNull();native.CalleeType!.IsNativeCallConv.ShouldBeTrue();
            var managed=calls.Single(c=>c.Callee=="managed");managed.CalleeType.ShouldNotBeNull();managed.CalleeType!.IsNativeCallConv.ShouldBeFalse();
            (native with {Args=native.Args.ToArray()}).CalleeType.ShouldBeSameAs(native.CalleeType);
            CSharpBackend.Run(ir).Functions.ShouldContain("abs");
        } finally {Directory.Delete(dir,true);}
    }
    [Fact]
    public void Same_IR_name_cannot_upgrade_an_ordinary_source_layout_to_packed_storage()
    {
        var dir=Path.Combine(Path.GetTempPath(),"r49-source-origin-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
        try {
            var path=Path.Combine(dir,"main.zig");File.WriteAllText(path,"const P=struct{a:u16};pub fn main()u8{return 0;}");
            var ir=new ZigFrontend().BuildIr(new FrontendRequest(new[]{path}));
            Should.Throw<CompileException>(()=>ir.RegisterStructType("P",ir.StructFieldsOf("P")!.ToList(),false,AggregateLayout.Packed)).Message.ShouldContain("storage origins");
        } finally {Directory.Delete(dir,true);}
    }

    private static IEnumerable<Call> Calls(CExpr expression){var result=new List<Call>();VaListLifetimeValidator.Walk(expression,e=>{if(e is Call c)result.Add(c);});return result;}

    [Fact]
    public void Unresolved_managed_packed_call_is_distinct_from_a_native_by_value_call()
    {
        var dir=Path.Combine(Path.GetTempPath(),"r49-unresolved-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
        try {
            var path=Path.Combine(dir,"main.zig");File.WriteAllText(path,"const P=packed struct(u16){a:u16};pub fn main()u8{return 0;}");
            var ir=new ZigFrontend().BuildIr(new FrontendRequest(new[]{path}));var p=new CType.Named("P");
            var call=new Call("unresolved_managed",new CExpr[]{new StructInit(new[]{new FieldInit("a",CType.UShort,new LitInt("3",3){Type=CType.UShort})}){Type=p}},new CType[]{p})
                {Type=p,CalleeType=new CType.Func(p,new CType[]{p},false)};
            var fn=ir.Functions.Single();ir.Functions[0]=fn with {Body=new Block(new CStmt[]{new ExprStmt(call),new Return(new LitInt("0",0){Type=CType.UChar})})};
            CSharpBackend.Run(ir).Functions.ShouldContain("unresolved_managed");
            var native=call with {CalleeType=call.CalleeType! with {IsNativeCallConv=true}};
            ir.Functions[0]=fn with {Body=new Block(new CStmt[]{new ExprStmt(native),new Return(new LitInt("0",0){Type=CType.UChar})})};
            Should.Throw<CompileException>(()=>CSharpBackend.Run(ir)).Message.ShouldContain("native ABI");
        } finally {Directory.Delete(dir,true);}
    }
}
