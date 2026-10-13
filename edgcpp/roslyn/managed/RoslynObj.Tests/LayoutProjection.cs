using RoslynObj;
using System.Numerics;

internal static class LayoutProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,string profile,Action<bool,string> check)
    {
        if(fixture is not ("layout.roslynobj" or "layout_c.roslynobj" or "layout_microsoft.roslynobj"))return;
        var records=unit.Types.OfType<RecordType>().ToArray();
        SemanticProjection.Sweep(records.Select(r=>r.Record),"record_type",check);
        SemanticProjection.Sweep(records.SelectMany(r=>r.Fields).Select(f=>f.Details),"field_symbol",check);
        SemanticProjection.Sweep(records.SelectMany(r=>r.BaseSubobjects).Select(b=>b.Details),"base_subobject",check);
        SemanticProjection.Sweep(records.Where(r=>r.Layout is not null).Select(r=>r.Layout!.Details),"record_layout",check);
        SemanticProjection.Sweep(records.Where(r=>r.Layout is not null).SelectMany(r=>r.Layout!.Fields).Select(f=>f.Details),"field_layout",check);
        SemanticProjection.Sweep(records.SelectMany(r=>r.VirtualCallOffsets).Select(o=>o.Details),"record_vcall_offset",check);
        SemanticProjection.Sweep(unit.Functions.SelectMany(f=>f.Overrides),"override_relation",check);
        BigInteger Constant(string name) => ((IntegerValue)unit.Variables.Single(v=>v.Name==name).ConstantInitializer!.Value).Value;
        RecordType Record(string name) => records.Single(r=>r.Name==name);
        if(fixture=="layout_c.roslynobj")
        {
            var record=Record("LayoutCFields");var layout=record.Layout!;
            check(layout.IsComplete&&record.Size==Constant("layout_c_size")&&record.Alignment==Constant("layout_c_alignment"),"A genuine C record must navigate its recorded effective size and alignment consistently with the actual sizeof/alignof constants.");
            check(layout.Fields.Single(f=>f.Field.Name=="value").Offset==Constant("layout_c_value_offset")&&record.Record.ContainsFlexibleArrayMember&&record.Fields.Single(f=>f.Name=="flexible").Type is ArrayType,"A flexible C member and its concrete neighboring field offset must follow actual recorded layout and type facts.");
            check(layout.Fields.Single(f=>f.Field.Name=="narrow") is {BitWidth:var width,BitOffset:var offset}&&width==3&&offset==0&&layout.Fields.Single(f=>f.Field.Name=="next") is {BitWidth:var nextWidth,BitOffset:var nextOffset}&&nextWidth==7&&nextOffset==3,"Adjacent genuine C bit fields must retain independent exact widths and bit offsets.");
            check(Record("LayoutCPacked") is {IsPacked:true,Alignment:var alignment} packed&&alignment==1&&packed.Size==Constant("layout_c_packed_size"),"The packed C record must retain its actual packing flag and effective size.");
            return;
        }
        var fields=Record("LayoutFields");var fieldLayout=fields.Layout!;
        var diamond=Record("LayoutDiamond");
        check(fields.Size==Constant("layout_fields_size")&&fields.Alignment==Constant("layout_fields_alignment")&&diamond.Size==Constant("layout_diamond_size")&&diamond.Alignment==Constant("layout_diamond_alignment"),"Real class size/alignment must navigate exact target quantities consistently with the captured sizeof/alignof constants.");
        check(fields.Fields.Take(3).Select(f=>f.Name).SequenceEqual(new[]{"first","empty","aligned"})&&fields.Fields.Single(f=>f.Name=="empty").HasNoUniqueAddressAttribute&&fields.Fields.Single(f=>f.Name=="aligned").ExplicitAlignment==16,"Ordered real fields, no-unique-address and explicit alignment must remain separate native facts.");
        var wide=fieldLayout.Fields.Single(f=>f.Field.Name=="wider_than_type");
        check(wide.Field.DeclaredBitSize==96&&wide.BitWidth==32,"An oversized genuine bit field must retain its declared width separately from the stored value width.");
        check(diamond.BasesInPostorder.Select(b=>b.BaseType.Name).SequenceEqual(new[]{"LayoutRoot","LayoutLeft","LayoutRight"})&&diamond.BasesInPostorder[0].IsVirtual&&diamond.BaseSubobjects.Single(b=>b.BaseType.Name=="LayoutRight").DirectBaseNumber==2,"Real base-subobject postorder must preserve the shared virtual base and numeric second direct-base sequence.");
        check(Record("LayoutFriends").Friends.Any(f=>f is RecordType {Name:"LayoutFriend"})&&Record("LayoutFriends").Friends.Any(f=>f is FunctionSymbol {Name:"layout_friend_function"}),"Class and routine friend arms must retain their distinct actual target identities.");
        check(diamond.BaseSubobjects.Any(b=>b.Overrides.Any(o=>o.Overridden.Name=="copy"&&o.ReturnAdjustmentSubobject is not null))&&diamond.BaseSubobjects.All(b=>b.Overrides.Select(o=>o.BaseOrdinal).SequenceEqual(b.Overrides.Select(o=>o.BaseOrdinal).Order())),"Covariant base overrides must retain their exact return-adjustment subobject and recorded base-list order.");
        check(fields.Fields.All(f=>f.IsGeneratedVirtualPointer is null),"Unavailable generated-vptr classification must stay unknown rather than a guessed false flag.");
        if(profile is "native_lowered" or "exported_templates_lowered")
        {
            check(diamond.BasesInPreorder.Select(b=>b.BaseType.Name).SequenceEqual(new[]{"LayoutLeft","LayoutRoot","LayoutRight"})&&diamond.VirtualCallOffsets.Any(o=>o.VirtualTableIndex<0),"The retained ABI preorder and signed virtual-call indices must preserve their exact producer order and negative values.");
            check(diamond.VirtualFunctionTableVariable is not null&&diamond.VirtualTableTableVariable is not null&&diamond.BaseSubobjects.Any(b=>b.VirtualFunctionTableIndex is not null&&b.ConstructionTableIndex is not null)&&diamond.HighestVirtualFunctionNumber==2,"Actual lowered class table variables and construction indices must be ordinary exact numeric navigation.");
        }
        if(fixture=="layout_microsoft.roslynobj")
        {
            var second=fieldLayout.Fields.Single(f=>f.Field.Name=="share_container_second");
            check(second is {Offset:var offset,BitOffset:var bitOffset,ContainerOffset:var containerOffset}&&offset==37&&bitOffset==1&&containerOffset==1,"Native Microsoft bit-field container displacement must retain one byte independently from absolute offset37 and bit offset1.");
            check(Record("LayoutNativeUuid").Uuid=="12345678-1234-1234-1234-123456789abc"&&Record("LayoutNativeEventSource").EventInterfaces.Single().Target.Name=="LayoutNativeEvents","Native Microsoft UUID and event-interface relationships must navigate their actual retained descriptors.");
        }
        else check(Record("LayoutPacked") is {IsPacked:true,Alignment:var alignment,Size:var size}&&alignment==Constant("layout_packed_alignment")&&size==Constant("layout_packed_size"),"A genuine GNU packed class must retain actual packing, size and alignment facts.");
    }
}
