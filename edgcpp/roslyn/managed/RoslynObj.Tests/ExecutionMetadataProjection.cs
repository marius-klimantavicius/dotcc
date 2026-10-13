using RoslynObj;

internal static class ExecutionMetadataProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture=="file_scope_metadata.roslynobj")
        {
            var roots=unit.Roots.Where(r=>r.Kind.Name=="file_scope_statement").OrderBy(r=>r.Ordinal).Select(r=>(BlockOperation)r.Entity).ToArray();
            check(roots.Length==2&&roots.All(r=>r.RepresentationRole.Name=="metadata"&&r.Details.Parent is null),"The actual file-scope statement list must retain two metadata block roots with no inferred statement parent.");
            var direct=roots.Single(r=>r.Statements.Any(s=>s.Expression is CallOperation {TargetMethod.Name:"header_side_effect"}));
            var captured=Captured(direct).ToArray();
            var unevaluated=captured.OfType<CallOperation>().Single();
            check(captured.All(n=>n.RepresentationRole.Name=="metadata")&&unevaluated.TargetMethod?.Name=="header_side_effect"&&!direct.DescendantOperations().Any(),"The direct sizeof statement-expression must retain its actual selected call and structural subtree entirely as metadata.");
            var control=unit.Functions.Single(f=>f.Name=="actual_file_stmt_control");
            check(control.Body!.DescendantOperations().OfType<CallOperation>().Count(c=>c.TargetMethod==unevaluated.TargetMethod)==1,"The ordinary control function must evaluate its selected side-effect call exactly once.");
            check(unit.Functions.Where(f=>f.Body is not null).All(f=>!f.Body!.DescendantOperations().Contains(unevaluated)),"A source-sequence association must not place the unevaluated direct call in any executable routine body.");
            if(source)
            {
                check(unit.Entities.OfType<SourceEntry>().Any(e=>e.Target==direct)&&unit.Entities.OfType<SourceEndConstruct>().Any(e=>e.Target==direct),"The retained source entry and end association must point to the same metadata block without promoting it.");
                var scheduled=unit.GlobalScope.DynamicInitializations.Single();
                check(scheduled.Operation?.RepresentationRole.Name=="source_semantic"&&scheduled.Expression is CallOperation {TargetMethod.Name:"header_side_effect",RepresentationRole.Name:"source_semantic"},"The separate stock-scheduled local initializer must preserve its actual source execution role despite the enclosing sizeof block's metadata role.");
            }
            else
            {
                var startup=unit.Functions.Single(f=>f.Body is not null&&f!=control);
                check(startup.IsCompilerGenerated&&startup.Body!.DescendantOperations().OfType<CallOperation>().Count(c=>c.TargetMethod==unevaluated.TargetMethod)==1,"The actual generated startup body must retain its one scheduled initializer call independently from the unevaluated direct statement-expression; the header's separate-init routine list need not contain this lowering-generated routine.");
            }
        }
        if(fixture=="gnu_asm_modify.roslynobj"||fixture=="source_statements.roslynobj"&&source)
        {
            var assembly=unit.Entities.OfType<AssemblyOperation>().ToArray();
            check(assembly.Length>0&&assembly.All(a=>a.Definition is {IsGnu:true}),"Actual GNU asm statements must expose their canonical assembly descriptors.");
            foreach(var statement in assembly)
            {
                var operations=unit.Functions.Single(f=>f.Body?.DescendantOperations().Contains(statement)==true).Body!.DescendantOperations().ToArray();
                check(statement.Operands.All(o=>o.Owner==statement.Definition&&statement.ChildRelations.Single(c=>c.Role.Name=="argument"&&c.Ordinal==o.Ordinal).Child==o.Expression&&operations.Count(n=>n==o.Expression)==1&&o.Expression.RepresentationRole==statement.RepresentationRole),"Each retained asm operand must navigate its canonical owner and the same single evaluated expression edge in the function body.");
                check(statement.OperandConstraints.All(c=>c.Owner==statement.Definition&&statement.Operands.Any(o=>o.Ordinal==c.OperandOrdinal)),"Structured asm constraints must refer to actual operands of the same canonical assembly descriptor.");
            }
            if(fixture=="gnu_asm_modify.roslynobj")
            {
                var statement=assembly.Single();var operand=statement.Operands.Single();
                check(operand.Kind.Name=="inout"&&operand.ModifierKind?.Name=="aom_modify"&&operand.Expression is ReferenceOperation {Target:VariableSymbol {Name:"value"}},"A genuine +r operand must retain its exact read/write modifier and selected parameter expression.");
                check(statement.OperandConstraints.Single() is {OperandOrdinal:0,Kind.Name:"aoc_reg_integer"},"The +r modifier must remain separate from its actual integer-register constraint.");
            }
            else
            {
                var operands=assembly.SelectMany(a=>a.Operands).ToArray();
                check(operands.Count(o=>o.Kind.Name=="output"&&o.ModifierKind?.Name=="aom_output")==1&&operands.Count(o=>o.Kind.Name=="input"&&o.ModifierKind?.Name=="aom_input")==2,"Existing =r and r operands must retain their output and input modifiers independently from the read/write modifier.");
            }
        }
        if(fixture=="native_dependent_foreach.roslynobj")
        {
            var loop=unit.Entities.OfType<ForEachOperation>().Single();
            check(loop.PatternKind.Name=="sfepk_none"&&loop.RepresentationRole.Name=="metadata"&&loop.Body is not null&&loop.Iterator is {Name:"value"}&&loop.CollectionReference is not null&&loop.Scope is not null&&loop.IteratorScope is not null,"The actual unresolved native foreach prototype must retain its common body, iterator, collection reference and scopes as metadata.");
            check(loop.Collection is null&&loop.EndVariable is null&&loop.Test is null&&loop.Increment is null&&!loop.ChildRelations.Any(c=>c.Role.Name is "condition" or "increment"),"The inactive STL/array union must remain absent for sfepk_none without a synthesized collection expression or end/test/increment.");
            check(loop.Children.Contains(loop.Body!)&&loop.IteratorScope!.Members.Contains(loop.Iterator!),"The unresolved foreach must preserve its actual body edge and iterator-scope variable membership.");
            check(!unit.Functions.Single(f=>f.Name=="native_dependent_foreach"&&f.Body is not null).Body!.DescendantOperations().Any(),"The retained dependent foreach prototype must not enter executable function traversal.");
        }
        if(fixture is "native_dependent_foreach.roslynobj" or "native_microsoft.roslynobj")
            check(unit.Entities.OfType<ForEachOperation>().All(loop=>loop.ChildRelations.Where(c=>c.Role.Name=="initialization").OrderBy(c=>c.Ordinal).Select(c=>((InitializationOperation)c.Child).Initialization).SequenceEqual(loop.Initializations)),"ForEach.Initializations must preserve the exact plural evaluated recipe edges and their retained order without an inferred singleton.");
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
            SemanticProjection.Sweep(unit.Entities.OfType<AllocationOperation>().Select(a=>a.Allocation),"allocation_expression",check);
            var call=unit.Entities.OfType<CallOperation>().Single(c=>c.TargetMethod is {Name:"read",ContainingType.Name:"ExplicitObject"});
            check(call.Receiver is null&&call.Arguments.Count==1&&call.Arguments[0].Parameter is {IsExplicitThis:true} descriptor&&descriptor==call.TargetMethod!.Parameters[0].Descriptor,"A retained explicit-object call must bind its first ordinary argument to the actual explicit-this descriptor while keeping Receiver absent.");
        }
        if(fixture is "cleanup_semantics.roslynobj" or "source_statements.roslynobj")
        {
            var initializations=unit.Entities.OfType<InitializationStatementOperation>().ToArray();
            if(initializations.Length>0)
            {
                check(initializations.All(s=>s.Children.OfType<InitializationOperation>().Any(i=>i.Initialization==s.Initialization)),"A named initialization statement must expose the same actual initialization that appears in its evaluated children.");
                var visitor=new ResultVisitor();foreach(var statement in initializations)statement.Accept(visitor);
                check(visitor.Initializations==initializations.Length,"Each typed initialization statement must dispatch its named visitor.");
            }
            if(source&&fixture=="source_statements.roslynobj")
            {
                var function=unit.Functions.Single(f=>f.Name=="statement_result");
                var visitor=new ResultVisitor();visitor.Visit(function.Body!.Root);
                check(visitor.Results>0&&function.Body.DescendantOperations().OfType<StatementExpressionResultOperation>().Any(s=>s.Expression is not null),"A GNU statement-expression result must dispatch its named visitor and retain its actual result expression.");
            }
        }
        if(source&&fixture is ("cleanup_semantics.roslynobj" or "retained.roslynobj"))
        {
            var labels=unit.Entities.OfType<LabelOperation>().ToArray();
            check(labels.Length>0&&labels.All(l=>l.Lifetime is not null&&l.Lifetime==l.Label.Lifetime),"Actual source label statements must expose the exact retained LabelSymbol lifetime through ordinary LabelOperation navigation.");
        }
    }

    private static IEnumerable<SemanticNode> Captured(SemanticNode root)
    {
        var pending=new Stack<SemanticNode>();var seen=new HashSet<SemanticNode>();pending.Push(root);
        while(pending.TryPop(out var node))
        {
            if(!seen.Add(node))continue;
            yield return node;
            foreach(var child in node.Children)pending.Push(child);
        }
    }

    private sealed class ResultVisitor : SemanticVisitor
    {
        public int Results;
        public int Initializations;
        public override void VisitStatementExpressionResult(StatementExpressionResultOperation operation) { Results++;base.VisitStatementExpressionResult(operation); }
        public override void VisitInitializationStatement(InitializationStatementOperation operation) { Initializations++;base.VisitInitializationStatement(operation); }
    }
}
