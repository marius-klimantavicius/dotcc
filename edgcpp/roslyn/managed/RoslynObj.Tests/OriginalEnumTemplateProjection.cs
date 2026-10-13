using RoslynObj;

internal static class OriginalEnumTemplateProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture!="original_enum_templates.roslynobj")return;
        VerifyOriginalArguments(unit,source,check);
        VerifyEnumTemplate(unit,check);
    }

    private static void VerifyOriginalArguments(TranslationUnit unit,bool source,Action<bool,string> check)
    {
        var prime=unit.Functions.Single(f=>f.Name=="prime_original");var alias=unit.Functions.Single(f=>f.Name=="alias_original");
        var primeCall=prime.Body!.DescendantOperations().OfType<CallOperation>().Single();var aliasCall=alias.Body!.DescendantOperations().OfType<CallOperation>().Single();
        var selected=aliasCall.TargetMethod!;var instance=selected.Instantiation!;
        check(primeCall.TargetMethod==selected&&instance.Result==selected&&instance.OriginalDefinition==selected.Template,"The int and LocalInt calls must retain the same selected function and its actual template instantiation.");
        var selectedArguments=instance.Arguments;
        check(selectedArguments.Count==2&&selectedArguments[0] is {Kind.Name:"tak_type",Type:ArithmeticType {Arithmetic.EdgIntegerKind.Name:"ik_int"}}&&selectedArguments[1] is {Kind.Name:"tak_nontype",Constant.Value:IntegerValue {Value:var seven}}&&seven==7,"The selected instantiation must retain its actual int then7 argument descriptors independently from the original name list.");
        var primeReference=(ReferenceOperation)primeCall.Callee;var aliasReference=(ReferenceOperation)aliasCall.Callee;
        check(primeReference.Target==selected&&aliasReference.Target==selected,"Both retained callee references must target the same canonical selected function.");
        if(!source)
        {
            check(primeReference.NameReference is null&&aliasReference.NameReference is null&&!unit.Entities.OfType<NameReference>().Any(n=>n.OriginalTemplateArguments.Count!=0),"The native profile without name recording must preserve absent name references and original lists without substituting selected arguments.");
            return;
        }
        var name=aliasReference.NameReference!;var arguments=name.OriginalTemplateArguments;var rows=name.Details.OriginalTemplateArgumentRows;
        check(name.IsTemplateId&&name.NumTemplateArguments==2&&arguments.Count==2&&rows.Select(r=>r.Ordinal).SequenceEqual(new long[]{0,1})&&rows.All(r=>r.Owner==name)&&rows.Select(r=>r.Argument).SequenceEqual(arguments),"The alias callee name must retain its actual two original argument descriptors and canonical ordered list rows.");
        var writtenType=(TypeReference)arguments[0].Type!;var local=unit.Entities.OfType<AliasSymbol>().Single(a=>a.Name=="LocalInt");
        check(arguments[0].Kind.Name=="tak_type"&&writtenType!=selectedArguments[0].Type&&writtenType.Name=="LocalInt"&&local.Type==writtenType&&writtenType.UnderlyingType==selectedArguments[0].Type,"The original type argument must be the retained LocalInt typedef descriptor, distinct from the selected int type and linked to that exact underlying type.");
        check(arguments[1] is {Kind.Name:"tak_nontype",Constant.Value:IntegerValue {Value:var originalSeven}}&&originalSeven==7&&arguments[1]!=selectedArguments[1]&&arguments[1].Constant!=selectedArguments[1].Constant&&arguments[1].Constant!.Value==selectedArguments[1].Constant!.Value,"The original integer7 argument must retain its own argument and constant occurrence even when the selected instance shares the interned value.");
        check(primeReference.NameReference is {OriginalTemplateArguments.Count:0},"The lexically identical int call must retain its recorded empty original list rather than copying the selected instantiation arguments.");
    }

    private static void VerifyEnumTemplate(TranslationUnit unit,Action<bool,string> check)
    {
        var enums=unit.Entities.OfType<EnumSymbol>().Where(e=>e.Template is not null).ToArray();
        var prototype=enums.Single(e=>e.Details.IsPrototypeInstantiation);var concrete=enums.Single(e=>!e.Details.IsPrototypeInstantiation);var template=prototype.Template!;
        check(enums.Length==2&&prototype!=concrete&&prototype.Type!=concrete.Type&&concrete.Template==template&&prototype.Details.Template==template&&concrete.Details.Template==template,"The prototype and concrete enums must retain the same exact member-enum template pointer while preserving distinct enum/type descriptors.");
        check(template.TemplateKind.Name=="templk_member_enum"&&template.Pattern==prototype.Type&&prototype.Type.DeclarationSymbol==prototype&&concrete.Type.DeclarationSymbol==concrete,"The member-enum template must retain its actual enum prototype pattern and canonical enum declarations.");
        check(template.OriginalDefinition==template&&template.Definition==template&&template.Prototype is null&&template.DeclarationSymbol is TemplateSymbol symbol&&symbol.Descriptor==template&&template.Details.EnumSymbolRows.Count==2,"The enum template must retain its own recorded canonical/definition/symbol identities and absent prototype-template pointer.");
        var enclosing=unit.TemplateDescriptors.Single(t=>t.Name=="EnumHost"&&t.TemplateKind.Name=="templk_class");
        check(template!=enclosing&&concrete.Template!=enclosing&&prototype.Template!=enclosing,"The retained member-enum association must remain distinct from the enclosing class template.");
        check(prototype.Enumerators.Select(e=>e.Symbol.Name).SequenceEqual(new[]{"First","Sized"})&&concrete.Enumerators.Select(e=>e.Symbol.Name).SequenceEqual(new[]{"First","Sized"})&&concrete.Enumerators.All(e=>e.Owner==concrete),"Both enum declarations must retain their actual canonical enumerators in recorded order.");
        var variable=unit.Variables.Single(v=>v.Name=="enum_state");var sized=concrete.Enumerators.Single(e=>e.Symbol.Name=="Sized");
        check(variable.DeclaredType==concrete.Type&&variable.ConstantInitializer!.Value==sized.Constant.Value,"The concrete enum-state variable must retain the actual instantiated enum type and Sized enumerator value.");
    }
}
