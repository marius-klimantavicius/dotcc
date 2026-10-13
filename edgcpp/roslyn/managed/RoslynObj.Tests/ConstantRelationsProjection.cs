using RoslynObj;

internal static class ConstantRelationsProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,string profile,Action<bool,string> check)
    {
        if(fixture=="member_pointers.roslynobj"&&source)
        {
            var data=unit.Variables.Single(v=>v.Name=="base_data").ConstantInitializer!;var converted=unit.Variables.Single(v=>v.Name=="converted_data").ConstantInitializer!;
            var method=unit.Variables.Single(v=>v.Name=="base_function").ConstantInitializer!;var convertedMethod=unit.Variables.Single(v=>v.Name=="converted_function").ConstantInitializer!;
            CppType ValueType(string name)=>((QualifiedType)unit.Variables.Single(v=>v.Name==name).Type).Qualification.UnderlyingType;
            check(data.Type==ValueType("base_data")&&converted.Type==ValueType("converted_data")&&method.Type==ValueType("base_function")&&convertedMethod.Type==ValueType("converted_function"),"Each member-pointer occurrence must retain its exact current unqualified value descriptor while the constexpr variable has a separate qualifier wrapper.");
            check(converted.OriginalType==data.Type&&convertedMethod.OriginalType==method.Type&&data.OriginalType is null&&method.OriginalType is null,"Actual cast occurrences must retain the original base member-pointer descriptors while unconverted occurrences preserve absent originals.");
            var owner=unit.Types.OfType<RecordType>().Single(t=>t.Name=="PointerBase");
            check(data.NameReference is not null&&data.NameReference==converted.NameReference&&method.NameReference is not null&&method.NameReference==convertedMethod.NameReference&&data.NameReference!=method.NameReference&&data.NameReference.Qualifier?.Target==owner&&method.NameReference.Qualifier?.Target==owner,"The retained field and method name-reference identities belong to their constant occurrences, with the original qualifier preserved through conversion rather than attached to shared values.");
            check(new[]{data,converted,method,convertedMethod}.All(c=>c.BackingExpression is ConstantOperation {RepresentationRole.Name:"metadata"} expression&&expression.Constant.Value==c.Value&&expression.Constant.Type==c.Type),"Every actual written member-pointer initializer must retain its canonical metadata backing expression and equivalent captured value/type while the expression's own constant occurrence remains distinct.");
        }
        if(fixture=="constant_occurrences.roslynobj")
        {
            var aggregate=unit.Variables.Single(v=>v.Name=="constant_aggregate").ConstantInitializer!;
            var parenthesized=unit.Variables.Single(v=>v.Name=="constant_parenthesized").ConstantInitializer!;
            check(aggregate.Type==unit.Types.Single(t=>t.Name=="ConstantAggregate")&&parenthesized.Type==aggregate.Type,"Aggregate occurrences must retain their exact complete record type independently from the const-qualified owning variables.");
            var first=aggregate.Elements;var second=parenthesized.Elements;
            check(first.Count==(source?3:2)&&second.Count==first.Count&&first[0].Elements.Single().Value is IntegerValue {Value:var baseValue}&&baseValue==3&&first.Last().Elements.Take(2).Select(c=>((IntegerValue)c.Value).Value).SequenceEqual(new System.Numerics.BigInteger[]{7,9}),"The actual aggregate occurrence list must preserve base/member order and values3,7,9; lowering removes the empty no-unique-address member rather than requiring its reconstruction.");
            if(source)check(first[1].Elements.Count==0&&second[1].Elements.Count==0,"The unlowered source aggregate must retain its actual empty member occurrence between the base and array.");
            check(second[0].Elements.Single().Value is IntegerValue {Value:var otherBase}&&otherBase==5&&second.Last().Elements.Take(2).Select(c=>((IntegerValue)c.Value).Value).SequenceEqual(new System.Numerics.BigInteger[]{11,13}),"The parenthesized occurrence must preserve its distinct written base and array entries in the retained list order.");
            check(first.Last()==aggregate.Components.Where(c=>c.Role.Name=="element").OrderBy(c=>c.Ordinal).Last().Constant&&((AggregateValue)aggregate.Value).Elements.Select(e=>e.Value).SequenceEqual(first.Select(c=>c.Value))&&((AggregateValue)parenthesized.Value).Elements.Select(e=>e.Value).SequenceEqual(second.Select(c=>c.Value)),"The actual last occurrence and ordered interned element values must remain separate projections linked by canonical value identity.");
            var dependent=unit.Constants.Where(c=>c.Value is ConstantValue<DependentValueDetails> {Payload.Kind.Name:"tpck_expression"}).ToArray();
            check(dependent.Length>0&&dependent.All(c=>((ConstantValue<DependentValueDetails>)c.Value).Payload.Expression is {RepresentationRole.Name:"metadata"} expression&&unit.Entities.Contains(expression)&&c.ContextualExpressions.Count==0)&&dependent.Any(c=>((ConstantValue<DependentValueDetails>)c.Value).Payload.Expression is OperatorOperation {OperatorKind.Name:"eok_cast"}),"Actual dependent constant expression arms must navigate their retained metadata generic cast and absent local-wrapper context without an inferred evaluated expression.");
        }
        if(fixture=="layout.roslynobj"&&profile=="cfront_lowered")
        {
            var root=unit.Types.OfType<RecordType>().Single(t=>t.Name=="LayoutRoot");
            var copy=unit.Functions.Single(f=>f.Name=="copy"&&f.ContainingType==root);
            var table=root.VirtualFunctionTableVariable!.ConstantInitializer!;
            check(table.Elements[1].Elements.Last().Value is AddressValue {BaseKind.Name:"abk_routine"} routineAddress&&routineAddress.BaseEntity==copy,"The actual Cfront table entry must retain the selected LayoutRoot copy routine as a typed address target.");
            var information=root.Details.TypeinfoVariable!.ConstantInitializer!;
            var address=(AddressValue)information.Elements[1].Value;
            var expected=unit.Constants.Single(c=>c.Value is StringValue value&&value.TargetBytes.SequenceEqual("LayoutRoot\0"u8.ToArray()));
            check(address.BaseKind.Name=="abk_constant"&&address.BaseEntity==expected&&address.BaseEntity is Constant {Value:StringValue}&&expected.Type is ArrayType,"The actual RTTI name pointer must retain its canonical string Constant occurrence, independently from that occurrence's interned bytes and the typeinfo variable.");
        }
        if(fixture=="cleanup_semantics.roslynobj"&&!source&&unit.Capabilities.Any(c=>c.Name=="GENERATE_EH_TABLES"&&c.Enabled==true))
        {
            var offsets=unit.Entities.OfType<ConstantValue<StackOffsetValueDetails>>().ToArray();
            check(offsets.Length==2&&offsets.Select(v=>v.Payload.Variable.Name).Order().SequenceEqual(new[]{"locals","values"})&&offsets.All(v=>v.Payload.Offset==0&&unit.Functions.Any(f=>f.Body?.Locals.Contains(v.Payload.Variable)==true)),"Actual partial-EH stack-offset values must retain the locals/values storage variables and exact zero displacement without treating them as exception-object offsets.");
            SemanticProjection.Sweep(offsets.Select(v=>v.Payload),"stack_offset_value",check);
        }
        if(fixture=="objects.roslynobj"&&!source)
        {
            var contexts=unit.Entities.OfType<Scope>().SelectMany(s=>s.LocalExpressionReferences).Where(r=>r.Kind.Name=="lerk_constant_expr").ToArray();
            check(contexts.Length==1&&contexts[0].Referrer is Constant constant&&constant.ContextualExpressions.Single().Expression==contexts[0].Expression&&constant.ContextualExpressions.Single().Owner==contexts[0].Owner&&constant.BackingExpression is {RepresentationRole.Name:"metadata"} backing&&backing!=contexts[0].Expression&&contexts[0].Expression.RepresentationRole.Name=="metadata","The actual constant must retain its direct backing expression separately from its scoped local-expression wrapper and exact scope/referrer association.");
        }
        if(fixture=="source_statements.roslynobj"&&source)
        {
            var prototype=unit.Functions.Single(f=>f.Name=="selected_branch"&&f.Instantiation?.IsPrototype==true);
            var parameter=prototype.Template!.Parameters.Single();
            var value=unit.Constants.Select(c=>c.Value).OfType<ConstantValue<DependentValueDetails>>().Single(v=>v.Payload.Kind.Name=="tpck_sizeof");
            check(value.Payload.Type==parameter.PlaceholderType&&value.Payload.Expression is null,"The actual dependent sizeof constant must retain its exact T placeholder descriptor and absent expression arm.");
        }
    }
}
