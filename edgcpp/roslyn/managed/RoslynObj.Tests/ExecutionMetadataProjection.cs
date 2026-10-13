using RoslynObj;

internal static class ExecutionMetadataProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture=="native_dependent_foreach.roslynobj")
        {
            var loop=unit.Entities.OfType<ForEachOperation>().Single();
            check(loop.PatternKind.Name=="sfepk_none"&&loop.RepresentationRole.Name=="metadata"&&loop.Body is not null&&loop.Iterator is {Name:"value"}&&loop.CollectionReference is not null&&loop.Scope is not null&&loop.IteratorScope is not null,"The actual unresolved native foreach prototype must retain its common body, iterator, collection reference and scopes as metadata.");
            check(loop.Collection is null&&loop.EndVariable is null&&loop.Test is null&&loop.Increment is null&&!loop.ChildRelations.Any(c=>c.Role.Name is "condition" or "increment"),"The inactive STL/array union must remain absent for sfepk_none without a synthesized collection expression or end/test/increment.");
            check(loop.Children.Contains(loop.Body!)&&loop.IteratorScope!.Members.Contains(loop.Iterator!),"The unresolved foreach must preserve its actual body edge and iterator-scope variable membership.");
            check(!unit.Functions.Single(f=>f.Name=="native_dependent_foreach"&&f.Body is not null).Body!.DescendantOperations().Any(),"The retained dependent foreach prototype must not enter executable function traversal.");
        }
        if(fixture=="upc_affinity.roslynobj")
        {
            var body=unit.Functions.Single(f=>f.Name=="upc_affinity_loop").Body!;
            var operations=body.DescendantOperations().ToArray();
            var loop=operations.OfType<LoopOperation>().Single(l=>l.Kind.Name=="stmk_upc_forall");
            check(loop.Affinity is CallOperation {TargetMethod.Name:"upc_affinity_effect"} affinity&&operations.Count(o=>o==affinity)==1&&loop.ChildRelations.Single(c=>c.Role.Name=="affinity").Child==affinity,"UPC affinity must preserve the actual selected call and its single evaluated structural edge.");
            check(operations.OfType<CallOperation>().Count(c=>c.TargetMethod?.Name=="upc_affinity_effect")==1&&operations.OfType<CallOperation>().Count(c=>c.TargetMethod?.Name=="upc_affinity_body")==1,"Actual UPC affinity and body calls must each remain reachable once without duplicating the affinity recipe.");
        }
        if(fixture=="name_references.roslynobj")
        {
            var external=unit.Functions.Single(f=>f.Name=="name_external");
            var call=unit.Functions.Single(f=>f.Name=="name_external_use").Body!.DescendantOperations().OfType<CallOperation>().Single(c=>c.TargetMethod==external);
            check(external.Body is null&&external.Parameters.Single().Symbol is null&&call.Arguments.Count==2&&call.Arguments[0].Parameter==external.Parameters[0].Descriptor&&call.Arguments[1].Parameter is null,"A declaration-only call must retain its actual anonymous signature descriptor while a variadic trailing argument has no formal descriptor.");
            var names=unit.Entities.OfType<NameReference>().ToArray();
            var qualifiers=unit.Entities.OfType<NameQualifier>().ToArray();
            SemanticProjection.Sweep(names.Select(n=>n.Details),"name_reference",check);
            SemanticProjection.Sweep(qualifiers.Select(q=>q.Details),"name_qualifier",check);
            if(source)
            {
                var selected=unit.Functions.Single(f=>f.Name=="name_reference_use").Body!.DescendantOperations().OfType<CallOperation>().Single(c=>c.TargetMethod?.Name=="name_target");
                var reference=((ReferenceOperation)selected.Callee).NameReference!;
                check(reference.IsTemplateId&&reference.NumTemplateArguments==1&&reference.OriginalTemplateArguments.Count==0&&reference.Qualifier?.Target is NamespaceSymbol {Name:"Nested"}&&reference.Qualifier.Previous?.Target is NamespaceSymbol {Name:"NameReferenceScope"},"A written qualified template call must preserve its explicit argument count and qualifier chain while a cleared redundant original list remains empty.");
                check(unit.Entities.OfType<ReferenceOperation>().Any(r=>r.Target is VariableSymbol {Name:"name_reference_global"}&&r.NameReference?.IsGlobalQualifiedName==true),"An explicit global variable name must retain its actual name-reference flag.");
                check(unit.Entities.OfType<ReferenceOperation>().Any(r=>r.Target is FieldSymbol {Name:"value"}&&r.NameReference?.Qualifier is {IsClass:true,Target:RecordType {Name:"NameRecord"}}),"An explicit class-qualified field name must navigate its actual record descriptor.");
            }
        }
        if(fixture=="variable_initializers.roslynobj")
        {
            var variables=unit.Variables.Where(v=>v.EntitiesDefinedInInitializer.Count>0).ToArray();
            check(variables.Length>0&&variables.SelectMany(v=>v.EntitiesDefinedInInitializer).OfType<RecordType>().Any(t=>t.IsLambdaClosureClass),"Actual static-member initializer lists must retain reachable lambda closure descriptors through their owning variable.");
            check(variables.All(v=>v.Details.VariableInitializerEntityRows.Select(r=>r.Entity).SequenceEqual(v.EntitiesDefinedInInitializer)),"Initializer entity navigation must preserve the actual retained list order and identities.");
            SemanticProjection.Sweep(unit.Variables.Select(v=>v.Details),"variable_symbol",check);
        }
        if(fixture=="source_order.roslynobj"&&source)
        {
            var statements=unit.Entities.OfType<StatementOperation>().Where(s=>s.SourceSequenceEntry is not null).ToArray();
            check(statements.Length>0&&statements.All(s=>s.SourceSequenceEntry!.Target==s),"A designated statement source entry must navigate the exact retained pointer instead of selecting a reverse occurrence.");
        }
        if(fixture=="retained.roslynobj"&&source)
        {
            var call=unit.Entities.OfType<CallOperation>().Single(c=>c.TargetMethod is {Name:"read",ContainingType.Name:"ExplicitObject"});
            check(call.Receiver is null&&call.Arguments.Count==1&&call.Arguments[0].Parameter is {IsExplicitThis:true} descriptor&&descriptor==call.TargetMethod!.Parameters[0].Descriptor,"A retained explicit-object call must bind its first ordinary argument to the actual explicit-this descriptor while keeping Receiver absent.");
        }
        if(fixture is "cleanup_semantics.roslynobj" or "source_statements.roslynobj")
        {
            var initializations=unit.Entities.OfType<InitializationStatementOperation>().ToArray();
            if(initializations.Length>0)
                check(initializations.All(s=>s.Children.OfType<InitializationOperation>().Any(i=>i.Initialization==s.Initialization)),"A named initialization statement must expose the same actual initialization that appears in its evaluated children.");
            if(source&&fixture=="source_statements.roslynobj")
            {
                var function=unit.Functions.Single(f=>f.Name=="statement_result");
                var visitor=new ResultVisitor();visitor.Visit(function.Body!.Root);
                check(visitor.Results>0&&function.Body.DescendantOperations().OfType<StatementExpressionResultOperation>().Any(s=>s.Expression is not null),"A GNU statement-expression result must dispatch its named visitor and retain its actual result expression.");
            }
        }
    }

    private sealed class ResultVisitor : SemanticVisitor
    {
        public int Results;
        public override void VisitStatementExpressionResult(StatementExpressionResultOperation operation) { Results++;base.VisitStatementExpressionResult(operation); }
    }
}
