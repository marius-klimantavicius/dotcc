using RoslynObj;

internal static class DynamicEmptyTargetProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture=="dependent_array_class_bound.roslynobj")VerifyArray(unit,check);
        if(fixture!="dynamic_empty_bound.roslynobj")return;
        var function=unit.Functions.Single(f=>f.Name=="retained_dynamic_default");var parameter=function.Signature.Parameters.Single();
        var temporary=(TemporaryOperation)parameter.DefaultExpression!;var recipe=temporary.Initialization!;var aggregate=recipe.Constant!;
        var record=unit.Types.OfType<RecordType>().Single(t=>t.Name=="DynamicAggregate");
        check(parameter.Descriptor.DefaultExpression==temporary&&parameter.Type==record&&temporary.Type==record&&aggregate.Type==record&&aggregate.Value.Kind.Name=="ck_aggregate","The retained default must follow its exact signature parameter, metadata temporary and initialization recipe to the canonical DynamicAggregate constant.");
        check(temporary.RepresentationRole.Name=="metadata"&&recipe.Operation!.RepresentationRole.Name=="metadata"&&!temporary.DescendantOperations().Any(),"The parameter default and aggregate recipe must remain metadata without becoming executable operations.");
        var components=aggregate.Components.Where(c=>c.Role.Name=="element").ToArray();var elements=aggregate.Elements;
        check(components.Select(c=>c.Ordinal).SequenceEqual(new long[]{0,1,2})&&components.All(c=>c.Owner==aggregate)&&components.Select(c=>c.Constant).SequenceEqual(elements)&&elements[2].Value is IntegerValue {Value:var five}&&five==5,"The default aggregate must retain its actual base, field and integer5 occurrences in recorded element order.");
        var baseConstant=elements[0];var fieldConstant=elements[1];
        var contextualBase=record.BaseSubobjects.Single(b=>b.BaseType.Name=="DynamicEmptyBase");var field=record.Fields.Single(f=>f.Name=="field");
        foreach(var (constant,type) in new[]{(baseConstant,contextualBase.BaseType),(fieldConstant,field.DeclaredType!)})
        {
            var initialization=((ConstantValue<DynamicInitializationValueDetails>)constant.Value).Payload.Initialization;
            check(constant.Value.Kind.Name=="ck_dynamic_init"&&!constant.Details.IsGenericInitializer&&initialization.Kind.Name=="dik_constructor"&&initialization.Constructor!.ContainingType==type&&constant.Type==type,"Each dynamic occurrence must retain its actual typed constructor recipe and canonical class type without generic-initializer substitution.");
            var argument=initialization.Operands.Single();
            check(initialization.Operation!.RepresentationRole.Name=="metadata"&&argument.Ordinal==0&&argument.Owner==initialization&&argument.Expression is ConstantOperation {Constant.Value:IntegerValue {Value:var argumentFive}}&&argumentFive==5&&argument.Expression.RepresentationRole.Name=="metadata","Each constructor must retain its exact singleton integer5 argument and metadata role, independently from executable constructor calls.");
        }
        if(source)
        {
            check(new[]{baseConstant,fieldConstant}.All(c=>c.Details.InitializesEmptyObject is null&&c.InitializedField is null&&c.InitializedBaseSubobject is null),"The source profile must preserve unavailable lowered empty-object flags and targets without inferring them from aggregate position or type layout.");
        }
        else
        {
            check(baseConstant.Details.InitializesEmptyObject==true&&baseConstant.Details.ConstantForBaseClass&&baseConstant.InitializedBaseSubobject==contextualBase&&baseConstant.InitializedField is null&&contextualBase.MostDerivedType==record,"The optimized-empty dynamic base occurrence must retain its exact contextual base target under the recorded base discriminator.");
            check(fieldConstant.Details.InitializesEmptyObject==true&&!fieldConstant.Details.ConstantForBaseClass&&fieldConstant.InitializedField==field&&fieldConstant.InitializedBaseSubobject is null&&field.Details.RecordType==record,"The optimized-empty dynamic field occurrence must retain the exact no_unique_address field target under the recorded field discriminator.");
        }
        var metadata=new SemanticNode[]{temporary,recipe.Operation!,((ConstantValue<DynamicInitializationValueDetails>)baseConstant.Value).Payload.Initialization.Operation!,((ConstantValue<DynamicInitializationValueDetails>)fieldConstant.Value).Payload.Initialization.Operation!};
        check(!unit.Functions.Where(f=>f.Body is not null).SelectMany(f=>f.Body!.DescendantOperations()).Any(o=>metadata.Contains(o)),"Retained metadata defaults and component recipes must remain outside evaluated function traversal.");
    }

    private static void VerifyArray(TranslationUnit unit,Action<bool,string> check)
    {
        var array=unit.Types.OfType<ArrayType>().Single(a=>a.DependentBoundExpression is not null);var expression=array.DependentBoundExpression!;
        var reference=unit.Entities.OfType<Scope>().SelectMany(s=>s.LocalExpressionReferences).Single(r=>r.Referrer==array);
        var field=unit.Entities.OfType<FieldSymbol>().Single(f=>f.DeclaredType==array);var record=field.Details.RecordType;
        check(array.Array.DependentBoundExpression==expression&&array.Array.BoundKind.Name=="dependent"&&array.Bound is null&&field.Name=="values"&&record.Fields.Contains(field),"The local-class field must retain the exact dependent array descriptor and its own recorded bound-expression pointer.");
        var function=(FunctionSymbol)reference.Owner.AssociatedEntity!;
        check(function.Name=="retained_dependent_class_bound"&&reference.Owner.Kind.Name=="sck_function"&&reference.Kind.Name=="lerk_dep_array_bound"&&reference.Expression==expression&&reference.Referrer==array&&record.SourceCorrespondence!.EnclosingRoutine==function,"The array-owned local reference must preserve its actual kind, expression, array referrer and recorded routine-scope association independently from the local class's allocation context.");
        check(reference.Owner.LocalExpressionReferences.Any(r=>r.Ordinal==reference.Ordinal&&r.Referrer==array&&r.Expression==expression)&&expression.RepresentationRole.Name=="metadata"&&!expression.DescendantOperations().Any(),"The array bound must remain the canonical metadata expression in its recorded owner scope without execution promotion.");
        var extents=unit.Variables.Where(v=>v.Name=="local_extent"&&v.SourceCorrespondence?.EnclosingRoutine==function).ToArray();
        check(extents.Any(v=>v.ConstantInitializer?.Value is IntegerValue {Value:var two}&&two==2),"The retained local bound context must preserve the actual static local_extent initializer2 without reconstructing the dependent expression from it.");
        var concrete=unit.Types.OfType<ArrayType>().Single(a=>a.Bound==5);
        check(concrete!=array&&concrete.DependentBoundExpression is null&&concrete.Array.DependentBoundConstant is null&&unit.Entities.OfType<FieldSymbol>().Any(f=>f.Name=="values"&&f.DeclaredType==concrete),"The concrete instantiated field must retain its separate bound5 array with absent dependent links; it does not replace the prototype's retained expression.");
        check(!unit.Functions.Where(f=>f.Body is not null).SelectMany(f=>f.Body!.DescendantOperations()).Any(o=>o==expression),"The retained dependent array-bound expression must remain outside evaluated function traversal.");
    }
}
