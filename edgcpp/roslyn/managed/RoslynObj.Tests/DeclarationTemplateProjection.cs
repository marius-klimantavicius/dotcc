using RoslynObj;

internal static class DeclarationTemplateProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture=="templates.roslynobj")
        {
            var function=unit.Functions.Single(f=>f.Name=="scalar_static");var variable=unit.Variables.Single(v=>v.Name=="retained_scalar");var correspondence=variable.SourceCorrespondence!;
            check(correspondence.ParentScope==function.Body!.Scope&&correspondence.EnclosingRoutine==function&&correspondence.ParentScope!.AssociatedEntity==function&&function.Body.Scope!.Members.Contains(variable),"The actual retained local-scope indirection must resolve the scalar's exact function scope and enclosing routine independently, without deriving a parent from a name or body traversal.");
        }
        if(fixture=="source_order.roslynobj"&&source)
        {
            var sequence=unit.Types.OfType<RecordType>().Single(t=>t.Name=="Sequence");var method=unit.Functions.Single(f=>f.Name=="method"&&f.ContainingType==sequence);
            var declaration=unit.Declarations.Single(d=>d.Symbol==method&&d.Secondary is not null);var secondary=declaration.Secondary!;
            check(secondary.DeclaredType is RoutineType {Routine.IsConst:true} written&&written.ReturnType==method.ReturnType&&written!=method.Signature,"The secondary method definition must retain its actual separately written const signature, independent from the current symbol signature.");
            var occurrence=unit.Entities.OfType<SourceEntry>().Single(e=>e.Target==declaration);
            check(secondary.NameReference is not null&&occurrence.Declaration==declaration&&occurrence.DeclaredSymbol==method&&declaration.Source==declaration.Details.SourceOrigin,"The actual secondary occurrence must preserve its own name-reference and declaration identity without a primary-symbol source fallback.");
            var directives=unit.Entities.OfType<InstantiationDirective>().ToArray();
            check(directives.Length==2&&directives.All(d=>d.Target is FunctionSymbol {Name:"instantiate"} selected&&d.DeclaredType is RoutineType signature&&signature!=selected.Signature&&signature.ReturnType==selected.ReturnType)&&directives.Select(d=>((RoutineType)d.DeclaredType!).ReturnType).Distinct().Count()==2,"The two explicit directives must retain their exact int/long target routines and separately written declaration signatures without deriving those signatures from their targets.");
            var assertion=unit.Entities.OfType<StaticAssertion>().Single();
            check(assertion.Condition.Value is IntegerValue {Value:var result}&&result==1&&assertion.Message is null&&assertion.Condition==assertion.Details.Condition&&unit.Entities.OfType<SourceEntry>().Any(e=>e.Target==assertion),"The terse assertion must navigate its actual evaluated true constant and absent message through the named entity and exact source target.");
        }
        if(fixture=="native_microsoft.roslynobj"&&source)
        {
            var field=unit.Types.OfType<RecordType>().Single(t=>t.Name=="NativeProperty").Fields.Single(f=>f.Name=="property");var references=unit.Entities.OfType<NameReference>().Where(n=>n.Property is not null).ToArray();
            check(references.Length==2&&references.All(n=>n.Property==field)&&field.Property is {GetName:"get",PutName:"set"},"The two actual native property name uses must retain the same canonical field descriptor and its separate get/put name metadata.");
        }
        if(fixture=="reflection.roslynobj")
        {
            var alias=unit.Entities.OfType<AliasSymbol>().Single(a=>a.Name=="info");var reference=(TypeReference)alias.UnderlyingType!;
            check(reference.Reference.ReferenceKind.Name=="trk_is_decltype"&&reference.DecltypeExpression is ConstantOperation {RepresentationRole.Name:"metadata",Constant.Value:ReflectionValue {Entity:CppType {Kind.Name:"tk_void"}}} expression&&reference.DecltypeExpression==reference.Reference.DecltypeExpression&&reference.ContextualExpressions.Count==0,"The written decltype(^^void) alias must retain its exact metadata reflection expression and genuinely absent contextual wrapper list.");
        }
        if(fixture=="execution_cpp.roslynobj")
        {
            var concept=unit.TemplateDescriptors.Single(t=>t.Name=="ExecutionNumber");
            check(concept.TemplateKind.Name=="templk_concept"&&concept.Constraint is {RepresentationRole.Name:"metadata"}&&concept.Pattern==concept.Constraint&&concept.DeclarationSymbol is TemplateSymbol symbol&&symbol.Constraint==concept.Constraint,"The actual concept definition must retain its canonical metadata boolean expression as both pattern and constraint, independently from concept-id uses.");
        }
        if(fixture=="routines_exception.roslynobj")
        {
            var descriptor=unit.TemplateDescriptors.Single(t=>t.Name=="subordinate"&&t.Prototype is not null);var prototype=descriptor.Prototype!;
            check(descriptor!=prototype&&descriptor.Pattern is FunctionSymbol {ContainingType:RecordType {Instantiation.IsPrototype:false} instance}&&prototype.Pattern is FunctionSymbol {ContainingType:RecordType {Instantiation.IsPrototype:true} original}&&instance.Instantiation!.OriginalDefinition==original.Instantiation!.OriginalDefinition&&descriptor.OriginalDefinition!=prototype,"The instantiated member template must preserve its exact prototype-class member descriptor independently from its canonical template identity.");
        }
        if(fixture=="retained.roslynobj"&&source)
        {
            var dependent=unit.Types.OfType<DependentType>().Single(t=>t.Name=="Result"&&t.ProxyClass is not null);
            check(dependent.ProxyClass is RecordType {Record.IsProxyClass:true} proxy&&proxy.ProxyOfType==dependent&&proxy.DeclarationSymbol is null,"The exact dependent Result placeholder must retain its proxy descriptor and reciprocal original-type link without a fabricated class declaration.");
        }
        if(fixture=="template_edges.roslynobj"&&source)
        {
            var dependent=unit.Types.OfType<DependentType>().Single(t=>t.Constraint is not null);
            check(dependent.Constraint is DependentOperation {Kind.Name:"enk_concept_id",RepresentationRole.Name:"metadata",Template.Name:"Addable"} constraint&&constraint.Template==unit.TemplateDescriptors.Single(t=>t.Name=="Addable"),"The constrained placeholder type must retain its exact concept-id metadata constraint and selected concept descriptor independently from the concept definition's boolean expression.");
        }
        if(fixture=="common.roslynobj")
        {
            var declaration=unit.Entities.OfType<UsingDeclaration>().Single(u=>u.Attributes.Count>0);var attribute=declaration.Attributes.Single();
            check(declaration.IsDirective&&declaration.Details.IsStrong&&declaration.Target is NamespaceSymbol {Name:"debug"} target&&attribute.Kind.Name=="ak_strong"&&attribute.Owners.Contains(declaration)&&!target.Attributes.Contains(attribute),"The actual attributed strong namespace directive must retain its own attribute descriptor and owner relationship without copying it to the selected namespace target.");
        }
    }
}
