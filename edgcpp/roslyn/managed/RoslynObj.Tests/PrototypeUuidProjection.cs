using RoslynObj;
using System.Numerics;

internal static class PrototypeUuidProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture=="prototype_scope.roslynobj")
        {
            var function=unit.Functions.Single(f=>f.Name=="prototype_tags");var scope=function.Signature.PrototypeScope!;
            var signatures=unit.Types.OfType<RoutineType>().Where(t=>t.PrototypeScope==scope).ToArray();
            check(scope.Kind.Name=="sck_func_prototype"&&scope.Parent is null&&scope.AssociatedEntity is RoutineType associated&&signatures.Contains(associated)&&signatures.Length==(source?2:1)&&(!source||associated!=function.Signature),"The retained C prototype scope must preserve all actual signature references and its distinct associated type, without assuming a unique owning signature or inventing a parent.");
            var structure=unit.Types.OfType<RecordType>().Single(t=>t.Name=="PrototypeTag");var enumeration=unit.Entities.OfType<EnumSymbol>().Single(e=>e.Name=="PrototypeEnum");
            check(scope.Members.SequenceEqual(new SemanticEntity[]{structure,enumeration.Type})&&scope.Members.All(m=>m is CppType)&&function.Signature.Parameters.Select(p=>p.Ordinal).SequenceEqual(new long[]{0,1})&&function.Signature.Parameters[0].Type is PointerType pointer&&pointer.PointeeType==structure&&function.Signature.Parameters[1].Type==enumeration.Type&&enumeration.Enumerators.Select(e=>e.Symbol.Name).SequenceEqual(new[]{"prototype_first","prototype_second"}),"The actual prototype-only tag declarations and ordered parameters must retain their canonical type identities, while parameters remain outside the scope member list.");
        }
        if(fixture!="uuid_storage.roslynobj")return;
        var record=unit.Types.OfType<RecordType>().Single(t=>t.Name=="UuidClass");var enumerationSymbol=unit.Entities.OfType<EnumSymbol>().Single(e=>e.Name=="UuidEnum");
        check(record.Uuid=="12345678-1234-1234-1234-123456789abc"&&enumerationSymbol.Uuid=="23456789-2345-2345-2345-23456789abcd","Class and enum UUID metadata must retain their exact independent string values through the typed descriptors.");
        foreach(var (name,type,storage,first,shortPart,bytes) in new[]{
            ("class_uuid_address",(CppType)record,record.UuidVariable,0x12345678,0x1234,new byte[]{0x12,0x34,0x12,0x34,0x56,0x78,0x9a,0xbc}),
            ("enum_uuid_address",enumerationSymbol.Type,enumerationSymbol.UuidVariable,0x23456789,0x2345,new byte[]{0x23,0x45,0x23,0x45,0x67,0x89,0xab,0xcd})})
        {
            var body=unit.Functions.Single(f=>f.Name==name).Body!;var returned=body.DescendantOperations().OfType<StatementOperation>().Single(s=>s.Kind.Name=="stmk_return").Expression!;
            if(source)
            {
                check(storage is null&&returned is OperatorOperation {OperatorKind.Name:"eok_address_of"} addressOf&&addressOf.Operands.Single() is OperatorOperation {OperatorKind.Name:"eok_indirect"} indirect&&indirect.Operands.Single() is ConstantOperation {Constant.Value:AddressValue {BaseKind.Name:"abk_uuidof",BaseEntity:var target}}&&target==type,"The source UUID address must retain its exact class/enum type target and inactive lowered storage, preserving the actual address-of/indirection expression shape.");
            }
            else
            {
                check(storage is not null&&unit.Variables.Contains(storage)&&storage.ConstantInitializer?.Value is AggregateValue&&returned is ConstantOperation {Constant.Value:AddressValue {BaseKind.Name:"abk_variable",BaseEntity:var target,ByteOffset:var offset}}&&target==storage&&offset.IsZero,"The actual lowered UUID address must select its canonical generated class/enum storage variable and recorded zero byte offset independently from the source type target.");
                var guid=(AggregateValue)storage!.ConstantInitializer!.Value;
                var leading=guid.Elements.Take(3).Select(e=>((IntegerValue)e.Value).Value).ToArray();var trailing=(AggregateValue)guid.Elements[3].Value;
                check(guid.Elements.Count==4&&leading.SequenceEqual(new BigInteger[]{first,shortPart,shortPart})&&trailing.Elements.Select(e=>((IntegerValue)e.Value).Value).SequenceEqual(bytes.Select(b=>new BigInteger(b))),"Each actual GUID storage initializer must retain its three ordered integer fields and exact eight-byte array payload, without decoding or inventing a source storage object.");
            }
        }
        check(source?record.UuidVariable is null&&enumerationSymbol.UuidVariable is null:record.UuidVariable!=enumerationSymbol.UuidVariable,"Distinct class and enum UUID storage must remain distinct where retained, and absent together in the unlowered control.");
    }
}
