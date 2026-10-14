using RoslynObj;

internal static class ContextualAssociationProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture=="contextual_bases.roslynobj")VerifyBases(unit,source,check);
        if(fixture=="embedded_virtual_bases.roslynobj")VerifyDataSection(unit,check);
        if(fixture=="associated_pragma_orphans.roslynobj")VerifySelectedAbsences(unit,source,check);
    }

    private static void VerifyBases(TranslationUnit unit,bool source,Action<bool,string> check)
    {
        RecordType Type(string name)=>unit.Types.OfType<RecordType>().Single(t=>t.Name==name);
        var leaf=Type("OverrideLeaf");var middle=Type("OverrideMiddle");var root=Type("OverrideRoot");
        var contextualRoot=leaf.BaseSubobjects.Single(b=>b.BaseType==root);var contextualMiddle=leaf.BaseSubobjects.Single(b=>b.BaseType==middle);
        var method=middle.Methods.Single(f=>f.Name=="call");var overridden=root.Methods.Single(f=>f.Name=="call");
        var relation=contextualRoot.Overrides.Single(o=>o.OverridingSubobject is not null);
        check(relation.Subobject==contextualRoot&&relation.OverridingSubobject==contextualMiddle&&relation.Owner==method&&relation.Overridden==overridden&&relation.BaseOrdinal==0,"The Leaf contextual Root override must retain its exact Leaf contextual Middle overriding subobject and canonical overriding/overridden methods.");
        var overriding=relation.OverridingSubobject!;
        check(overriding.MostDerivedType==leaf&&overriding.BaseType==middle&&overriding!=middle.BaseSubobjects.Single(b=>b.BaseType==root),"The overriding base association must remain in the Leaf context, distinct from a base descriptor belonging to Middle itself.");
        var primaryLeaf=Type("PrimaryLeaf");var primaryMiddle=Type("PrimaryMiddle");var primaryRoot=Type("PrimaryRoot");
        var middleInLeaf=primaryLeaf.BaseSubobjects.Single(b=>b.BaseType==primaryMiddle);var rootInLeaf=primaryLeaf.BaseSubobjects.Single(b=>b.BaseType==primaryRoot);
        if(source)check(middleInLeaf.PrimaryBaseSubobject is null&&primaryLeaf.PrimaryBaseSubobject is null,"The source profile must preserve its absent layout primary pointers without computing them from inheritance.");
        else
        {
            check(middleInLeaf.PrimaryBaseSubobject==rootInLeaf&&middleInLeaf.Details.PrimaryBaseSubobject==rootInLeaf&&rootInLeaf.MostDerivedType==primaryLeaf,"The contextual Middle base must retain the actual contextual Root primary-base pointer in the same most-derived Leaf type.");
            check(primaryLeaf.PrimaryBaseSubobject==middleInLeaf&&primaryMiddle.PrimaryBaseSubobject!=rootInLeaf&&middleInLeaf.PrimaryBaseSubobject!=primaryLeaf.PrimaryBaseSubobject,"The contextual base primary link must remain distinct from record-level primary bases and another record's base context.");
        }
        var attributed=Type("AttributedDerived");var target=attributed.BaseSubobjects.Single(b=>b.BaseType==Type("AttributedBase"));
        if(source)
        {
            var attribute=target.Attributes.Single();
            check(attribute.Kind.Name=="ak_unrecognized"&&attribute.NamespaceName=="roslyn_probe"&&attribute.Name=="marker"&&attribute.Owner==target&&attribute.Owners.SequenceEqual(new SemanticEntity[]{target}),"The base-specifier marker must retain the exact contextual base as its canonical attribute owner.");
            check(target.MostDerivedType==attributed&&!attributed.Attributes.Contains(attribute)&&!target.BaseType.Attributes.Contains(attribute),"The base attribute must remain on its actual base descriptor rather than being reassigned to the derived or base type.");
        }
        else check(target.Attributes.Count==0,"The native profile must preserve its unretained unrecognized base marker instead of recovering it from source spelling.");
    }

    private static void VerifyDataSection(TranslationUnit unit,Action<bool,string> check)
    {
        var leaf=unit.Types.OfType<RecordType>().Single(t=>t.Name=="EmbeddedLeaf");
        var root=leaf.BaseSubobjects.Single(b=>b.BaseType.Name=="EmbeddedRoot");var middle=leaf.BaseSubobjects.Single(b=>b.BaseType.Name=="EmbeddedMiddle");
        check(root.IsVirtual&&middle.IsVirtual&&root.DataSectionBaseSubobject==middle&&root.Details.DataSectionBaseSubobject==middle&&root.MostDerivedType==leaf&&middle.MostDerivedType==leaf,"The actual USL virtual Root must retain its data-section base pointer to the exact contextual Middle in the same Leaf descriptor.");
        check(root!=middle&&root.Details.CompleteSubobject==true&&middle.DataSectionBaseSubobject is null&&unit.Entities.Contains(middle),"The data-section association must retain distinct canonical base identities and actual complete-subobject state without a reciprocal link or computed layout.");
    }

    private static void VerifySelectedAbsences(TranslationUnit unit,bool source,Action<bool,string> check)
    {
        var alias=unit.Namespaces.Single(n=>n.Name=="LocalAlias");var target=unit.Namespaces.Single(n=>n.Name=="RetainedNamespace");
        check(alias.AliasTarget==target&&unit.GlobalScope.Members.Contains(alias)&&unit.GlobalScope.Details.ScopeMemberRows.Single(r=>r.Member==alias).Owner==unit.GlobalScope,"The retained local namespace alias must preserve its actual target and file-scope storage membership without assigning it to an orphan bucket.");
        var headers=unit.Entities.OfType<SemanticRecord<OrphanScopeDetails>>().Select(h=>h.Details).ToArray();
        check(headers.SelectMany(h=>h.OrphanScopeMemberRows).All(r=>r.Kind.Name!="namespace"),"The selected objects contain no orphan namespace member; the global alias descriptor must not be moved into a fabricated orphan association.");
        if(source)
        {
            var function=unit.Functions.Single(f=>f.Name=="retained_local_alias");var header=headers.Single(h=>h.Routine==function);
            check(header.TranslationUnit==unit&&header.ScopeNumber==5&&header.OrphanScopeMemberRows.Single().Entity is SourceSublist&&header.OrphanScopeMemberRows.Single().Kind.Name=="source_sublist","The source orphan header must preserve its literal storage scope number5, actual routine and sole source-sublist payload, independently from the recorded namespace alias's file-scope storage.");
        }
        else check(headers.Length==0,"The native object must preserve its absent orphan headers after lowering rather than creating a lexical bucket from the alias's source use.");
        check(unit.Pragmas.Count==0,"These selected objects retain no unsupported test pragma descriptors or targets; no association is inferred from the following declaration or statement.");
    }
}
