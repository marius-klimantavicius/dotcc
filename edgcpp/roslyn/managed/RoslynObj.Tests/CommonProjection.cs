using RoslynObj;

internal static class CommonProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture is not ("common.roslynobj" or "common_labels.roslynobj" or "common_microsoft.roslynobj"))return;
        var correspondences=unit.Entities.OfType<SourceCorrespondence>().ToArray();
        SemanticProjection.Sweep(correspondences.Select(c=>c.Details),"source_correspondence",check);
        SemanticProjection.Sweep(unit.Namespaces.Select(n=>n.Details),"namespace_symbol",check);
        var usings=unit.Entities.OfType<UsingDeclaration>().ToArray();
        SemanticProjection.Sweep(usings.Select(u=>u.Details),"using_declaration",check);
        var scopes=unit.Entities.OfType<Scope>().ToArray();
        SemanticProjection.Sweep(scopes.Select(s=>s.Details),"scope",check);
        var labels=unit.Entities.OfType<LabelSymbol>().ToArray();
        SemanticProjection.Sweep(labels.Select(l=>l.Details),"label_symbol",check);
        check(correspondences.Length>0&&correspondences.All(c=>c.Entities.Count>0&&c.Entities.All(e=>e.SourceCorrespondence==c)),"Canonical correspondence reverse navigation must preserve every exact associated semantic identity.");
        check(unit.Declarations.Where(d=>d.Secondary is null).All(d=>d.SourceCorrespondence==d.Symbol.SourceCorrespondence&&d.LexicalScope==d.Symbol.SourceCorrespondence?.ParentScope&&d.Attributes.SequenceEqual(d.Symbol.Attributes)),"Primary declaration metadata must delegate to its exact shared correspondence and actual symbol association.");
        check(scopes.All(s=>s.Children.All(c=>c.Parent==s)),"Recorded ordered child scopes must preserve their actual parent relationships.");
        if(fixture=="common.roslynobj")
        {
            if(!source)check(correspondences.Any(c=>c.UnnamedEntityGivenFabricatedName==true)&&correspondences.Where(c=>c.UnnamedEntityGivenFabricatedName==true).All(c=>c.SourceName is null&&c.CurrentName is not null&&c.FabricatedName==(c.NameHasBeenMangled==true?c.Details.UnmangledNameOrMangledEncoding:c.CurrentName))&&correspondences.Any(c=>c.NameHasBeenMangled==true&&c.SourceName is not null&&c.CurrentName!=c.SourceName),"Lowered names must preserve actual current/mangled/fabricated facts, including absent pre-mangling names, while suppressing fabricated spellings as source names.");
            var root=unit.Namespaces.Single(n=>n.Name=="CommonRoot");
            var alias=unit.Namespaces.Single(n=>n.Name=="CommonAlias");
            check(alias.IsNamespaceAlias&&alias.AliasTarget==root&&unit.Namespaces.Single(n=>n.Name=="CommonAliasAgain").AliasTarget==alias,"Namespace aliases must retain the exact alias-to-alias chain without collapsing its identities.");
            check(unit.Namespaces.Single(n=>n.Name=="debug").NamedInStrongUsing&&usings.Any(u=>u.IsDirective&&u.Details.IsStrong)&&unit.Namespaces.Single(n=>n.Name=="Version").HasGnuAbiTagAttribute==true,"Actual strong using and inline ABI-tag namespace facts must remain separate flags.");
            check(unit.Namespaces.Any(n=>n.Details.IsAnonymous&&n.HasInternalLinkage&&n.Name is null)&&unit.Namespaces.Single(n=>n.Name=="InnerAnonymous").HasInternalLinkage,"Anonymous namespace naming and retained nested internal linkage must remain exact.");
            check(usings.All(u=>u.LexicalScopes.Count==1?u.LexicalScope==u.LexicalScopes[0]:u.LexicalScope is null),"Using lexical-scope convenience must equal the unique actual list owner, with zero/plural associations remaining null.");
            if(source)check(unit.Functions.Single(f=>f.Name=="common_locals").Body!.Scope!.Children.Select(s=>s.IsStatementExpressionBlock).SequenceEqual(new[]{false,true}),"The source function must preserve the actual ordered ordinary-block then statement-expression child-scope list.");
            if(source)check(usings.Length==13&&usings.All(u=>u.LexicalScope is not null)&&usings.Count(u=>u.IsEnumerator&&u.TargetSymbol is EnumeratorSymbol)==2&&usings.Count(u=>u.NextInSet is not null)==3,"Source using declarations must retain their thirteen entries, two exact enumerator targets and three overload-set links before lowering removes written forms.");
            if(source)check(usings.Any(u=>u.IsGlobalQualified==true&&u.TargetSymbol is VariableSymbol {Name:"global_common"})&&usings.Any(u=>u.AdjustedAccess?.Name=="as_public"&&u.TargetSymbol is FieldSymbol {Name:"member"}),"Global and class-qualified using forms must retain exact targets and adjusted access.");
            check(scopes.Any(s=>s.IsConstexprRoutine)&&scopes.Any(s=>s.IsStatementExpressionBlock)&&scopes.Any(s=>s.ThisParameter is not null),"Actual constexpr/statement-expression scopes and current receiver variables must remain direct typed facts.");
            check(labels.Any(l=>l.Details.IsLikely)&&labels.Any(l=>l.Details.IsUnlikely),"Real likely and unlikely label flags must remain separate recorded facts.");
            var type=unit.Types.Single(t=>t.Name=="CommonDerived");
            check(type.DeclarationSymbol!.SourceCorrespondence==type.SourceCorrespondence&&type.SourceCorrespondence!.Entities.Contains(type)&&type.SourceCorrespondence.Entities.Contains(type.DeclarationSymbol),"A named type descriptor and its declaration symbol must share the actual correspondence without merged identities.");
        }
        if(fixture=="common_labels.roslynobj")
            check(labels.Count(l=>l.AddressTaken==true&&l.IsLocallyDeclared==true)==2,"Actual GNU local address-taken labels must preserve both independent flags.");
        if(fixture=="common_microsoft.roslynobj")
        {
            var ns=unit.Namespaces.Single(n=>n.Name=="CommonProxyNamespace");
            check(ns.ProxyClass is {DeclarationSymbol:null,Size:null,Alignment:null} proxy&&proxy.SourceCorrespondence!.SourceName==ns.Name,"A retained namespace proxy must expose its actual descriptor and copied naming metadata without a fabricated type declaration or concrete layout.");
            check(unit.Types.OfType<RecordType>().Count(t=>t.Record.IsProxyClass&&t.DeclarationSymbol is null&&t.Size is null&&t.Alignment is null)==2,"Both actual namespace and dependent proxy descriptors must preserve absent declared identity and unavailable placeholder layout.");
            check(correspondences.Any(c=>c.MicrosoftIdentifierUsed==true&&c.SourceName=="class")&&labels.Any(l=>l.IsLeave==true),"Native Microsoft identifier and leave forms must retain their direct semantic flags.");
        }
    }
}
