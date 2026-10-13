using RoslynObj;
using System.Numerics;

internal static class DependentPointerProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,string profile,Action<bool,string> check)
    {
        var source=profile=="source_semantic";
        if(fixture=="dependent_payloads.roslynobj")
        {
            VerifyTypes(unit,check);
            VerifyValues(unit,check);
            VerifyCaptures(unit,source,check);
        }
        if(fixture=="retained_name_forms.roslynobj")VerifyNames(unit,source,check);
    }

    private static void VerifyTypes(TranslationUnit unit,Action<bool,string> check)
    {
        var nested=unit.Types.OfType<DependentType>().Single(t=>t.OriginalNestedType is not null);
        var original=(RecordType)nested.OriginalNestedType!;
        var parent=(RecordType)original.SourceCorrespondence!.ParentScope!.AssociatedEntity!;
        check(original.Name=="Nested"&&original.Record.IsPrototypeInstantiation&&parent.Name=="NestedHolder"&&parent.Fields.Single(f=>f.Name=="nested").Type==nested&&original!=nested,"The substituted nested placeholder must retain the exact original Nested prototype type independently from its containing holder and current dependent field type.");
        var width=unit.Entities.OfType<TemplateParameter>().Single(p=>p.Name=="Width");
        var types=unit.Types.OfType<DependentType>().Where(t=>t.BitWidthConstant is not null).ToArray();
        var signed=types.Single(t=>!t.Dependency.IsUnsignedBitPreciseInteger);var unsigned=types.Single(t=>t.Dependency.IsUnsignedBitPreciseInteger);
        check(types.Length==2&&signed.BitWidthConstant!.Value==width.PlaceholderConstant!.Value&&signed.BitWidthConstant!=width.PlaceholderConstant&&signed.BitWidthConstant.Value is ConstantValue<DependentValueDetails> {Payload.Kind.Name:"tpck_param"},"The signed dependent bit width must retain its actual constant occurrence and shared Width parameter value without collapsing occurrence and placeholder identities.");
        var expression=(OperatorOperation)((ConstantValue<DependentValueDetails>)unsigned.BitWidthConstant!.Value).Payload.Expression!;
        check(expression.RepresentationRole.Name=="metadata"&&expression.OperatorKind.Name=="eok_add"&&expression.Operands[0] is ConstantOperation first&&first.Constant.Value==width.PlaceholderConstant!.Value&&expression.Operands[1] is ConstantOperation {Constant.Value:IntegerValue {Value:var one}}&&one==1,"The unsigned dependent width must retain its exact metadata Width-plus-one expression rather than a synthesized concrete width.");
        var reference=unit.Types.OfType<TypeReference>().Single(t=>t.ProxyClass is not null);
        var proxy=(RecordType)reference.ProxyClass!;
        check(reference.Name=="Result"&&proxy.Record.IsProxyClass&&proxy.ProxyOfType==reference&&proxy.DeclarationSymbol is null&&reference.ProxyClass==reference.Reference.ProxyClass,"The Result typeref must retain its canonical proxy class and reciprocal original-type association without a fabricated class symbol.");
    }

    private static void VerifyValues(TranslationUnit unit,Action<bool,string> check)
    {
        var conversion=unit.Constants.Select(c=>c.Value).OfType<ConstantValue<DependentValueDetails>>().Single(v=>v.Payload.ConversionType is not null);
        var convertible=unit.Types.OfType<RecordType>().Single(t=>t.Name=="Convertible");
        var method=convertible.Methods.Single(f=>f.ReturnType is ArithmeticType {Arithmetic.EdgIntegerKind.Name:"ik_int"});
        check(conversion.Payload.Kind.Name=="tpck_unknown_function"&&conversion.Payload.ConversionType==method.ReturnType&&unit.Types.Contains(conversion.Payload.ConversionType!),"The unknown dependent conversion function must retain the exact canonical int conversion type independently from its unresolved function identity.");
        var wrapper=unit.Constants.Single(c=>c.Value is ConstantValue<DependentValueDetails> {Payload.Kind.Name:"tpck_dependent_constant",Payload.ConstantValue:not null});
        var value=((ConstantValue<DependentValueDetails>)wrapper.Value).Payload;var component=wrapper.Components.Single();
        var child=component.Constant;var pointer=(ConstantValue<MemberPointerValueDetails>)value.ConstantValue!;
        var field=(FieldSymbol)pointer.Payload.Member!;
        check(component.Owner==wrapper&&component.Role.Name=="dependent"&&component.Ordinal==0&&child.Value==value.ConstantValue&&child.Type==value.ConstantType&&value.ConstantType is MemberPointerType type&&type.MemberType==field.Type&&field.Name=="field"&&field.ContainingType.Name=="MemberPattern"&&pointer.Payload.ContainingType==field.ContainingType,"The dependent member-constant wrapper must preserve its exact child occurrence, interned member-pointer value, separate type descriptor and canonical MemberPattern field target.");
    }

    private static void VerifyCaptures(TranslationUnit unit,bool source,Action<bool,string> check)
    {
        var lambdas=unit.Entities.OfType<LambdaDefinition>().ToArray();
        if(!source)
        {
            check(lambdas.SelectMany(l=>l.Captures).All(c=>c.IndirectCaptureField is null&&c.SourceClosureField is null),"The lowered profile retains no source capture pointer arms; the reader must not infer them from generated closure fields.");
            return;
        }
        var indirect=lambdas.SelectMany(l=>l.Captures).Single(c=>c.IndirectCaptureField is not null);
        var sourceField=indirect.IndirectCaptureField!;var outer=lambdas.Single(l=>l.ClosureType==sourceField.ContainingType);var originalCapture=outer.Captures.Single();
        check(indirect.Ordinal==0&&indirect.IsIndirectInitCapture&&indirect.Variable is null&&indirect.SourceClosureField==sourceField&&indirect.Field!=sourceField&&indirect.Field!.ContainingType==indirect.Owner.ClosureType&&sourceField.Details.IsInitCapture&&originalCapture.IsInitCapture&&originalCapture.Field==sourceField,"The inner indirect init capture must retain its exact outer init-capture field as both source arms, separately from the destination field in its current closure.");
        var ordinary=lambdas.SelectMany(l=>l.Captures).Single(c=>c.SourceClosureField is not null&&c.IndirectCaptureField is null);
        var ordinarySource=ordinary.SourceClosureField!;var originalLambda=lambdas.Single(l=>l.ClosureType==ordinarySource.ContainingType);
        var parameter=unit.Functions.Single(f=>f.Name=="nested_variable_capture").SourceParameters.Single().Symbol!;
        check(ordinary.Ordinal==0&&ordinary.Variable==parameter&&ordinary.Field!=ordinarySource&&ordinary.Field!.ContainingType==ordinary.Owner.ClosureType&&originalLambda.Captures.Single().Field==ordinarySource&&originalLambda.Captures.Single().Variable==parameter,"The ordinary nested capture must retain the original parameter and actual outer-closure source field independently from its current closure destination.");
    }

    private static void VerifyNames(TranslationUnit unit,bool source,Action<bool,string> check)
    {
        var references=unit.Entities.OfType<NameReference>().ToArray();
        if(!source)
        {
            check(references.Length==0,"This lowered profile retains no name-form descriptors; the reader must preserve their absence.");
            return;
        }
        var name=references.Single(n=>n.DestructorType is not null);var alias=unit.Types.OfType<TypeReference>().Single(t=>t.Name=="DestructorAlias");
        var record=unit.Types.OfType<RecordType>().Single(t=>t.Name=="NamedDestructor");
        check(name.DestructorType==record&&alias.UnderlyingType==record&&name.DestructorType!=alias,"The destructor name must retain the resolved NamedDestructor class pointer separately from the written DestructorAlias typeref.");
        var templates=references.Where(n=>n.IsTemplateId).ToArray();
        check(templates.Length==2&&templates.Select(n=>n.NumTemplateArguments!.Value).OrderDescending().SequenceEqual(new BigInteger[]{2,1})&&templates.All(n=>n.OriginalTemplateArguments.Count==0),"The two overloaded template names retain their actual explicit argument counts but no original argument-list payload; the reader must not substitute instance arguments.");
    }
}
