using RoslynObj;

internal static class StatementRelationsProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,string profile,Action<bool,string> check)
    {
        if(source&&fixture=="execution_cpp.roslynobj")
        {
            var function=unit.Functions.Single(f=>f.Name=="execution_statement_result");var body=function.Body!;var operations=body.DescendantOperations().ToArray();
            var result=operations.OfType<StatementExpressionResultOperation>().Single();var returned=operations.OfType<JumpOperation>().Single(j=>j.JumpKind.Name=="return");
            var recipe=result.Initialization!;var returnRecipe=returned.Jump.Initialization!;
            check(result.Expression is null&&recipe is not null&&result.ChildRelations.Single(c=>c.Child is InitializationOperation).Child is InitializationOperation initializer&&initializer.Initialization==recipe&&operations.OfType<InitializationOperation>().Count(i=>i.Initialization==recipe)==1,"The class-valued GNU result statement must expose its actual initialization arm and single evaluated recipe with the expression arm absent.");
            check(returned.Value is null&&returnRecipe is not null&&returnRecipe!=recipe&&returned.ChildRelations.Single(c=>c.Child is InitializationOperation).Child is InitializationOperation returnInitializer&&returnInitializer.Initialization==returnRecipe&&operations.OfType<InitializationOperation>().Count(i=>i.Initialization==returnRecipe)==1,"The actual return recipe must remain distinct from the nested statement-expression result recipe and appear exactly once through its own evaluated edge.");
            var visitor=new ResultVisitor();result.Accept(visitor);check(visitor.Count==1,"The active initialized result statement must dispatch its named visitor.");
            var statementExpression=operations.OfType<StatementExpressionOperation>().Single();var compound=(BlockOperation)statementExpression.Statement;
            check(body.Root is StatementOperation root&&root.Details.Parent is null&&compound.Details.Parent is null&&compound.Statements.All(s=>s.Details.Parent==compound),"Actual function and GNU compound-expression roots must preserve legitimate null statement parents while their contained statements retain the exact block association.");
            var declaration=compound.Statements.OfType<DeclarationOperation>().Single();
            check(declaration.Entities.Single() is VariableSymbol {Name:"local"} local&&body.Locals.Contains(local)&&declaration.Declaration.DeclarationStatementEntityRows.Select(r=>r.Entity).SequenceEqual(declaration.Entities),"The actual declaration must retain the selected local entity and recorded list order independently from its executable initialization edge.");
        }
        if(profile=="cfront_lowered"&&fixture=="layout.roslynobj")
        {
            var ordinary=unit.Entities.OfType<ConditionalOperation>().Where(i=>i.Kind.Name=="stmk_if").ToArray();
            check(ordinary.Length>0&&ordinary.Any(i=>i.Else is not null)&&ordinary.All(i=>i.Then is not null&&i.ChildRelations.Single(c=>c.Role.Name=="then").Child==i.Then&&(i.Else is null?!i.ChildRelations.Any(c=>c.Role.Name=="else"):i.ChildRelations.Single(c=>c.Role.Name=="else").Child==i.Else)),"Actual lowered ordinary if statements must retain their exact then/else evaluated edges and legitimate absent else branches independently from source constexpr descriptors.");
        }
        if(fixture=="upc_affinity.roslynobj")
        {
            var loop=unit.Functions.Single(f=>f.Name=="upc_affinity_loop").Body!.DescendantOperations().OfType<LoopOperation>().Single(l=>l.Kind.Name=="stmk_upc_forall");
            check(loop.Body is not null&&loop.ChildRelations.Single(c=>c.Role.Name=="body").Child==loop.Body,"The actual UPC forall body must preserve its exact evaluated statement edge independently from the affinity call.");
        }
        if(source&&fixture=="retained.roslynobj")
        {
            var labels=unit.Entities.OfType<LabelOperation>().ToArray();var jumps=unit.Entities.OfType<JumpOperation>().Where(j=>j.JumpKind.Name=="goto").ToArray();
            check(jumps.Length>0&&jumps.All(j=>j.Target is {Target:LabelOperation target}&&target.Label==j.Target&&labels.Contains(target)),"Each actual goto must navigate the exact defining label statement and reciprocal canonical label identity.");
            check(jumps.All(j=>j.CommonLifetime is not null&&j.Target!.Lifetime is not null&&j.CommonLifetime==j.Jump.CommonLifetime),"Each source goto must retain its exact common lifetime independently from the defining label lifetime, even when these two captured pointers legitimately coincide.");
        }
    }

    private sealed class ResultVisitor : SemanticVisitor
    {
        public int Count;
        public override void VisitStatementExpressionResult(StatementExpressionResultOperation operation) {Count++;}
    }
}
