using RoslynObj;

internal static class RetainedTypeProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture=="empty_scoped_enum.roslynobj")
        {
            var enums=unit.Entities.OfType<EnumSymbol>().ToArray();
            var root=enums.Single(e=>e.Name=="RootEmptyScoped");var nested=enums.Single(e=>e.Name=="EmptyScoped");var filled=enums.Single(e=>e.Name=="FilledScoped");
            var ns=unit.Namespaces.Single(n=>n.Name=="RootEnumNamespace");
            check(new[]{root,nested,filled}.All(e=>e.Scopes.Count==1&&e.Scope==e.Scopes[0]&&e.Scope!.Kind.Name=="sck_enum"&&e.Scope.AssociatedEntity==e.Type),"Each actual user enum must expose its unique retained enum scope and exact descriptor association, including empty enums.");
            check(root.Scope!.Parent==unit.GlobalScope&&filled.Scope!.Parent==unit.GlobalScope&&nested.Scope!.Parent==ns.Scope&&ns.Scope!.AssociatedEntity==ns,"Root and nested enum scopes must preserve their actual file/namespace parent identities.");
            check(root.Enumerators.Count==0&&nested.Enumerators.Count==0&&root.Scope!.Members.Count==0&&nested.Scope!.Members.Count==0,"Empty scoped enums must retain real empty scopes without invented enumerators or members.");
            check(filled.Scope!.Members.OfType<EnumeratorSymbol>().SequenceEqual(filled.Enumerators.Select(e=>(EnumeratorSymbol)e.Symbol))&&filled.Enumerators.Single().Symbol.Name=="Value","A populated enum scope must retain its canonical enumerator symbol identity independently from the constant occurrence.");
        }
        if(fixture=="member_pointers.roslynobj"&&source)
        {
            MemberPointerType Pointer(string name)=>(MemberPointerType)Underlying(unit.Variables.Single(v=>v.Name==name).Type);
            var baseType=unit.Types.Single(t=>t.Name=="PointerBase");var derivedType=unit.Types.Single(t=>t.Name=="PointerDerived");
            var data=Pointer("base_data");var convertedData=Pointer("converted_data");var method=Pointer("base_function");var convertedMethod=Pointer("converted_function");
            check(data.DeclaringType==baseType&&method.DeclaringType==baseType&&convertedData.DeclaringType==derivedType&&convertedMethod.DeclaringType==derivedType,"Actual data/function member-pointer type descriptors must preserve their own declaring class independently from the selected member value's base class.");
            check(new[]{data,method,convertedData,convertedMethod}.All(t=>t.OriginalDeclaringType==t.DeclaringType),"This written member-pointer fixture must retain its exact original class descriptor on each type without borrowing the value's declaring class.");
            var field=baseType.Fields.Single(f=>f.Name=="data");var routine=unit.Functions.Single(f=>f.Name=="method");
            check(data.MemberType==field.Type&&convertedData.MemberType==field.Type&&method.MemberType is RoutineType signature&&signature.ReturnType==routine.ReturnType&&signature.Parameters.Single().Type==routine.SourceParameters.Single().Type&&convertedMethod.MemberType is RoutineType converted&&converted.ReturnType==routine.ReturnType,"Member-pointer type payloads must retain the actual field type and routine return/formal types through their separate descriptors.");
        }
        if(fixture=="vector_values.roslynobj")
        {
            var integers=(VectorType)Underlying(unit.Variables.Single(v=>v.Name=="constant_vector").Type);
            var bytes=(VectorType)Underlying(unit.Variables.Single(v=>v.Name=="constant_byte_vector").Type);
            check(integers.ElementType is ArithmeticType {Arithmetic.Bits:32}&&bytes.ElementType is ArithmeticType {Arithmetic.Bits:8}&&integers.ElementType==integers.Vector.ElementType&&bytes.ElementType==bytes.Vector.ElementType,"Actual fixed vector descriptors must retain their distinct integer and byte element types through ordinary canonical navigation.");
            check(integers.SizeConstant is {Value:IntegerValue {Value:var integerSize}}&&integerSize==16&&bytes.SizeConstant is {Value:IntegerValue {Value:var byteSize}}&&byteSize==8,"User-written vector size constants must retain their exact sixteen/eight byte expressions without synthesizing constants for builtin vectors.");
        }
        if(fixture=="vector_arm.roslynobj")
        {
            var integers=(VectorType)Underlying(unit.Functions.Single(f=>f.Name=="constant_scalable_identity").ReturnType);
            var modal=(VectorType)Underlying(unit.Functions.Single(f=>f.Name=="constant_modal_scalable_identity").ReturnType);
            check(integers.ElementType is ArithmeticType {Arithmetic.Bits:32}&&modal.ElementType is StorageFloatingType {Kind.Name:"tk_mfp8"},"Actual scalable vector descriptors must navigate their integer and payloadless modal floating element types.");
            check(integers.Size is null&&modal.Size is null&&integers.SizeConstant is null&&modal.SizeConstant is null,"Builtin scalable vectors must preserve unavailable concrete size and absent size constants without reconstructed lane counts.");
        }
        if(fixture=="vector_riscv.roslynobj")
        {
            var vectors=new[]{"constant_riscv_identity","constant_riscv_fractional","constant_riscv_tuple","constant_riscv_boolean"}.Select(name=>(VectorType)Underlying(unit.Functions.Single(f=>f.Name==name).ReturnType)).ToArray();
            check(vectors.Take(3).All(v=>v.ElementType is ArithmeticType {Arithmetic.Bits:32})&&vectors.All(v=>v.ElementType==v.Vector.ElementType)&&vectors[3].ElementType is ArithmeticType {Arithmetic.Bits:8},"Actual RISC-V regular, fractional, tuple and mask vectors must preserve their canonical integer/byte element links independently from multiplier/tuple metadata.");
            check(vectors.All(v=>v.Size is null&&v.SizeConstant is null),"RISC-V builtin vector size must remain unavailable without an invented size expression.");
        }
    }

    private static CppType Underlying(CppType type)
    {
        while(type is TypeReference reference)type=reference.UnderlyingType;
        return type;
    }
}
