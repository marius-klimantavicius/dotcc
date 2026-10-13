using RoslynObj;

internal static class ScopeRelationsProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture is not ("common.roslynobj" or "source_order.roslynobj" or "source_statements.roslynobj" or "source_vla.roslynobj" or "templates.roslynobj" or "types.roslynobj" or "objects.roslynobj" or "cleanup_semantics.roslynobj" or "retained.roslynobj"))return;
        var scopes=unit.Entities.OfType<Scope>().ToArray();
        var members=new List<ScopeMemberDetails>();
        foreach(var scope in scopes.Where(s=>s.Kind.Name!="sck_enum"))
        {
            var rows=scope.Details.ScopeMemberRows.ToArray();members.AddRange(rows);
            if(rows.Length>0)check(rows.All(r=>r.Owner==scope)&&rows.Select(r=>r.Member).SequenceEqual(scope.Members)&&rows.Select(r=>r.Ordinal).SequenceEqual(rows.Select(r=>r.Ordinal).Order()),"An actual scope member list must preserve its canonical owner, each exact member identity and recorded grouped-list order.");
        }
        string Family(SemanticEntity member)=>member switch
        {
            VariableSymbol v=>v.SymbolDetails.Storage?.Name switch {"sc_auto" or "sc_register"=>"nonstatic_variables",null=>"variables_unknown_storage",_=>"variables"},
            Constant=>"constants",CppType=>"types",FunctionSymbol=>"routines",NamespaceSymbol=>"namespaces",LabelSymbol=>"labels",TemplateDescriptor=>"templates",
            UsingDeclaration u=>u.IsDirective?"using_directives":"using_declarations",VlaDimension=>"vla_dimensions",_=>member.GetType().Name
        };
        foreach(var group in members.GroupBy(r=>Family(r.Member)))
            Console.WriteLine($"scope-members:{group.Key},{group.Count()} members,{group.Select(r=>r.Owner).Distinct().Count()} owners");
        var functions=unit.Functions.Where(f=>f.Body?.Scope is {Kind.Name:"sck_function"}).ToArray();
        if(functions.Length>0)check(functions.All(f=>f.Body!.Scope!.AssociatedEntity==f),"Actual function scopes must navigate their exact retained routine association.");
        var namespaces=unit.Namespaces.Where(n=>!n.IsNamespaceAlias&&n.Scope is not null).ToArray();
        if(namespaces.Length>0)check(namespaces.All(n=>n.Scope!.AssociatedEntity==n),"Actual namespace scopes must preserve their defining namespace identity separately from alias scopes.");
        var handlers=unit.Entities.OfType<ExceptionHandler>().Where(h=>h.Body is BlockOperation {Scope:not null}).ToArray();
        if(handlers.Length>0)check(handlers.All(h=>((BlockOperation)h.Body).Scope!.AssociatedEntity==h),"An actual handler-body scope must navigate the canonical handler through its recorded association arm.");
        if(fixture=="retained.roslynobj"&&source)
        {
            var condition=unit.Functions.Single(f=>f.Name=="source_initializations").Body!.DescendantOperations().OfType<ConditionOperation>().Single();
            check(condition.Scope?.AssociatedEntity is ConditionalOperation statement&&statement.Condition==condition,"The actual condition scope must retain its associated if statement and that statement's exact condition expression.");
        }
        if(fixture=="source_statements.roslynobj"&&source)
        {
            var block=unit.Functions.Single(f=>f.Name=="statement_result").Body!.DescendantOperations().OfType<BlockOperation>().Single(b=>b.Descriptor?.IsStatementExpression==true);
            check(block.Scope==block.Descriptor!.Scope&&block.Scope!.Members.OfType<VariableSymbol>().Any(v=>v.Name=="temporary"),"The actual statement-expression block must retain its canonical associated scope and selected local variable.");
        }
        if(fixture=="source_order.roslynobj"&&source)
        {
            var owners=scopes.Where(s=>s.Pragmas.Count>0).ToArray();
            check(owners.Length==1&&owners.All(s=>s.Details.ScopePragmaRows.All(r=>r.Owner==s)&&s.Details.ScopePragmaRows.Select(r=>r.Pragma).SequenceEqual(s.Pragmas)&&s.Pragmas.All(unit.Pragmas.Contains)),"The actual scoped pragma must preserve its canonical owner and recorded pragma list identities.");
        }
    }
}
