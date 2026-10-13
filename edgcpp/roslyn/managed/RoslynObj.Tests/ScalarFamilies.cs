using RoslynObj;
using System.Reflection;

internal static class ScalarFamilies
{
    internal static void Verify(TranslationUnit unit,string fixture,Action<bool,string> check)
    {
        if(fixture is "types.roslynobj" or "type_edges.roslynobj" or "template_edges.roslynobj" or "vector_values.roslynobj" or "vector_arm.roslynobj" or "vector_riscv.roslynobj" or "member_pointers.roslynobj")
        {
            var types=unit.Types.ToArray();
            Family(types.Select(t=>t.Details),"type",check);
            Family(types.OfType<ArrayType>().Select(t=>t.Array),"array_type",check);
            Family(types.OfType<TypeReference>().Select(t=>t.Reference),"type_reference",check);
            Family(types.OfType<DependentType>().Select(t=>t.Dependency),"dependent_type",check);
            Family(types.OfType<VectorType>().Select(t=>t.Vector),"vector_type",check);
            Family(unit.Entities.OfType<EnumSymbol>().Select(t=>t.Details),"enum_symbol",check);
            Family(types.OfType<PointerType>().Select(t=>t.Pointer),"pointer_type",check);
            Family(types.OfType<MemberPointerType>().Select(t=>t.MemberPointer),"member_pointer_type",check);
            Family(types.OfType<QualifiedType>().Select(t=>t.Qualification),"qualified_type",check);
            var records=types.OfType<RecordType>().Select(t=>t.Record).ToArray();
            Family(records,"record_type",check);
            if(fixture=="types.roslynobj")
                check(records.Any(r=>r.ContainsError is null)&&records.Any(r=>r.ContainsError==false)&&records.All(r=>r.DoesNotContainDeprecatedOrUnavailableType is not false),"Recorded cache predicates must distinguish an established false fact from unknown, while strong negative predicates remain true or unknown.");
        }
        if(fixture is not ("common.roslynobj" or "common_labels.roslynobj" or "common_microsoft.roslynobj" or "name_references.roslynobj" or "module_interface.roslynobj" or "module_import.roslynobj" or "source_order.roslynobj" or "native_microsoft.roslynobj" or "native_attributes.roslynobj" or "upc_affinity.roslynobj"))return;
        var entities=unit.Entities.ToArray();
        Family(entities.OfType<NamespaceSymbol>().Select(n=>n.Details),"namespace_symbol",check);
        Family(entities.OfType<UsingDeclaration>().Select(u=>u.Details),"using_declaration",check);
        Family(entities.OfType<LabelSymbol>().Select(l=>l.Details),"label_symbol",check);
        Family(entities.OfType<Scope>().Select(s=>s.Details),"scope",check);
        Family(entities.OfType<NameReference>().Select(n=>n.Details),"name_reference",check);
        Family(entities.OfType<NameQualifier>().Select(n=>n.Details),"name_qualifier",check);
        Family(entities.OfType<CppModule>().Select(m=>m.Details),"module",check);
        Family(entities.OfType<ModuleImport>().Select(m=>m.Details),"module_import",check);
        Family(entities.OfType<Declaration>().Select(d=>d.Secondary).OfType<SecondaryDeclarationDetails>(),"secondary_declaration",check);
        Family(entities.OfType<SourceEntry>().Select(e=>e.Details),"source_entry",check);
        Family(entities.OfType<RecordDeclaration>().Select(e=>e.Details),"record_declaration",check);
        Family(entities.OfType<SourceEndConstruct>().Select(e=>e.Details),"source_end_construct",check);
        Family(entities.OfType<InstantiationDirective>().Select(e=>e.Details),"instantiation_directive",check);
        Family(entities.OfType<LinkageSpecification>().Select(e=>e.Details),"linkage_specification",check);
        Family(entities.OfType<MicrosoftIfExists>().Select(e=>e.Details),"microsoft_if_exists",check);
        Family(entities.OfType<MicrosoftAttribute>().Select(a=>a.Details),"ms_attribute",check);
        Family(entities.OfType<MicrosoftAttributeArgument>().Select(a=>a.Details),"ms_attribute_argument",check);
        var pragmas=entities.OfType<PragmaDirective>().ToArray();
        Family(pragmas.Select(p=>p.Details),"pragma",check);
        Family(pragmas.Select(p=>p.Standard).OfType<PragmaStdcDetails>(),"pragma_stdc",check);
        Family(pragmas.Select(p=>p.Upc).OfType<PragmaUpcDetails>(),"pragma_upc",check);
        Family(pragmas.Select(p=>p.Comment).OfType<PragmaCommentDetails>(),"pragma_comment",check);
        Family(pragmas.Select(p=>p.Conformance).OfType<PragmaConformDetails>(),"pragma_conform",check);
        Family(pragmas.Select(p=>p.IncludeAlias).OfType<PragmaIncludeAliasDetails>(),"pragma_include_alias",check);
        Family(pragmas.Select(p=>p.Gcc).OfType<PragmaGccDetails>(),"pragma_gcc",check);
        foreach(var loop in entities.OfType<LoopOperation>())_=loop.InitScope;
    }

    private static void Family<T>(IEnumerable<T> records,string family,Action<bool,string> check) where T : SemanticDetail
    {
        var actual=records.ToArray();
        if(actual.Length==0) { Console.WriteLine($"scalar-empty:{family}");return; }
        SemanticProjection.Sweep(actual,family,check);
        var nullableBooleans=typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance).Where(p=>p.PropertyType==typeof(bool?));
        var absent=nullableBooleans.Select(p=>$"{p.Name}={actual.Count(r=>p.GetValue(r) is null)}");
        Console.WriteLine($"scalar-null:{family},{actual.Length} records,{string.Join(',',absent)}");
    }
}
