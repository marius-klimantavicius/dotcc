using RoslynObj;
using System.Numerics;

internal static class DependentConstantPayloadProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,string profile,Action<bool,string> check)
    {
        if(fixture!="dependent_constant_payloads.roslynobj")return;
        VerifyTemplateReference(unit,check);
        VerifyDestructor(unit,check);
        VerifyIntegerPack(unit,check);
        VerifyDesignators(unit,check);
        VerifyEmptyAggregate(unit,profile=="source_semantic",check);
    }

    private static void VerifyTemplateReference(TranslationUnit unit,Action<bool,string> check)
    {
        var occurrence=Find(unit,"tpck_template_ref");var value=((ConstantValue<DependentValueDetails>)occurrence.Value).Payload;
        var component=occurrence.Components.Single();var child=component.Constant;
        check(component.Owner==occurrence&&component.Role.Name=="dependent"&&component.Ordinal==0&&child.Value==value.ConstantValue&&child.Type==value.ConstantType&&child.Value is ConstantValue<DependentValueDetails> {Payload.Kind.Name:"tpck_unknown_function"},"The dependent template reference must retain its exact unresolved-function child occurrence, interned value and type through the canonical dependent component.");
        var arguments=value.DependentTemplateArgumentRows;
        check(arguments.Select(a=>a.Ordinal).SequenceEqual(new long[]{0,1})&&arguments[0].Argument is {Kind.Name:"tak_type",Type:ArithmeticType {Arithmetic.EdgIntegerKind.Name:"ik_int"}}&&arguments[1].Argument is {Kind.Name:"tak_nontype",Constant.Value:IntegerValue {Value:var three}}&&three==3&&arguments.All(a=>a.Owner.Kind==value.Kind&&a.Owner.ConstantValue==value.ConstantValue),"The explicit dependent template arguments must retain actual int then integer3 descriptors in recorded order, independently from selected function-instantiation arguments.");
    }

    private static void VerifyDestructor(TranslationUnit unit,Action<bool,string> check)
    {
        var value=((ConstantValue<DependentValueDetails>)Find(unit,"tpck_destructor").Value).Payload;
        var template=unit.TemplateDescriptors.Single(t=>t.Name=="dependent_destroy"&&t.TemplateKind.Name=="templk_function");
        var parameter=template.Parameters.Single(p=>p.Name=="T");
        check(value.Type==parameter.PlaceholderType&&value.Type is DependentType&&unit.Types.Contains(value.Type),"The dependent destructor constant must retain the exact T placeholder type of dependent_destroy without resolving it to the instantiated Destroyable class.");
    }

    private static void VerifyIntegerPack(TranslationUnit unit,Action<bool,string> check)
    {
        var occurrence=Find(unit,"tpck_integer_pack");var value=((ConstantValue<DependentValueDetails>)occurrence.Value).Payload;
        var template=unit.TemplateDescriptors.Single(t=>t.Name=="IntegerPackHost"&&t.TemplateKind.Name=="templk_class");
        var parameter=template.Parameters.Single(p=>p.Name=="N");var component=occurrence.Components.Single();
        check(component.Owner==occurrence&&component.Role.Name=="dependent"&&component.Ordinal==0&&component.Constant!=parameter.PlaceholderConstant&&component.Constant.Value==parameter.PlaceholderConstant!.Value&&value.ConstantValue==component.Constant.Value&&value.ConstantType==component.Constant.Type&&value.ConstantValue is ConstantValue<DependentValueDetails> {Payload.Kind.Name:"tpck_param"},"The integer-pack bound must preserve its actual dependent child occurrence and shared N parameter value with the recorded type, without constructing a pack expansion in the reader.");
    }

    private static void VerifyEmptyAggregate(TranslationUnit unit,bool source,Action<bool,string> check)
    {
        var occurrences=unit.Constants.Where(c=>c.InitializedField is not null||c.InitializedBaseSubobject is not null).ToArray();
        if(source)
        {
            check(occurrences.Length==0,"The source profile records no optimized-empty field/base associations before the lowering guard; no target is inferred from aggregate order.");
            return;
        }
        var aggregates=occurrences.Where(c=>c.Value.Kind.Name=="ck_aggregate").ToArray();
        var baseConstant=aggregates.Single(c=>c.InitializedBaseSubobject is not null);var fieldConstant=aggregates.Single(c=>c.InitializedField is not null);
        var subobject=baseConstant.InitializedBaseSubobject!;var record=(RecordType)subobject.MostDerivedType;
        var field=record.Fields.Single(f=>f.Name=="field");
        check(aggregates.Length==2&&baseConstant.Details.InitializesEmptyObject==true&&baseConstant.Details.ConstantForBaseClass&&baseConstant.InitializedField is null&&subobject.BaseType.Name=="EmptyBase"&&subobject.Details.IsOptimizedEmptyBase&&record.Name=="EmptyAggregate"&&fieldConstant.Details.InitializesEmptyObject==true&&!fieldConstant.Details.ConstantForBaseClass&&fieldConstant.InitializedBaseSubobject is null&&fieldConstant.InitializedField==field&&field.Details.IsOptimizedEmptyClass,"The two retained empty aggregate occurrences must preserve their exact contextual base and no_unique_address field targets under the actual union discriminator.");
        var parameter=unit.Entities.OfType<TemplateParameter>().Single(p=>p.Name=="Value"&&p.DefaultArgument?.Constant is not null);var root=parameter.DefaultArgument!.Constant!;
        var temporary=(TemporaryOperation)root.BackingExpression!;var recipe=temporary.Initialization!;var parent=recipe.Constant!;
        check(parent!=root&&temporary.RepresentationRole.Name=="metadata"&&recipe.Operation!.RepresentationRole.Name=="metadata"&&parent.Elements.Take(2).SequenceEqual(new[]{baseConstant,fieldConstant})&&parent.Components.Where(c=>c.Role.Name=="element").Take(2).All(c=>c.Owner==parent)&&parent.Components.Where(c=>c.Role.Name=="element").Take(2).Select(c=>c.Ordinal).SequenceEqual(new long[]{0,1}),"The retained template default must reach the distinct aggregate through its actual metadata backing temporary and initialization recipe, whose component list preserves base then field occurrences.");
    }

    private static void VerifyDesignators(TranslationUnit unit,Action<bool,string> check)
    {
        var occurrences=unit.Constants.Where(c=>c.Value is ConstantValue<DesignatorValueDetails> {Payload.IsGeneric:true}).ToArray();
        check(occurrences.Length==2&&occurrences[0]!=occurrences[1]&&occurrences[0].Value==occurrences[1].Value,"The two actual dependent designator occurrences must remain distinct even when their retained pure payload is shared.");
        foreach(var occurrence in occurrences)
        {
            var value=((ConstantValue<DesignatorValueDetails>)occurrence.Value).Payload;
            var row=value.DesignatorSubscriptRows.Single();var component=occurrence.Components.Single(c=>c.Role.Name=="designator");
            check(value.Kind.Name=="element"&&row.Ordinal==0&&component.Owner==occurrence&&component.Ordinal==0&&row.Value==component.Constant.Value&&row.Type==component.Constant.Type&&row.Value is ConstantValue<DependentValueDetails> {Payload.Kind.Name:"tpck_param",Payload.ParameterNumber:1,Payload.ParameterDepth:1},"Each generic designator must preserve its actual singleton bound-value/type row and the exact occurrence component; no unrecorded Last entry is synthesized for the range spelling.");
        }
    }

    private static Constant Find(TranslationUnit unit,string kind)
        => unit.Constants.Single(c=>c.Value is ConstantValue<DependentValueDetails> v&&v.Payload.Kind.Name==kind);

}
