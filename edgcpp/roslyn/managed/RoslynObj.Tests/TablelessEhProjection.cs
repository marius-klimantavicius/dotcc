using RoslynObj;

internal static class TablelessEhProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,Action<bool,string> check)
    {
        if(!unit.Capabilities.Any(c=>c.Name=="DO_IL_LOWERING"&&c.Enabled==true)||!unit.Capabilities.Any(c=>c.Name=="GENERATE_EH_TABLES"&&c.Enabled==false))return;
        check(unit.Capabilities.Single(c=>c.Name=="DO_FULL_PORTABLE_EH_LOWERING").Enabled==false,"The genuine tableless profile must remain partial EH lowering.");
        var nodes=unit.Entities.OfType<LoweredEhOperation>().ToArray();
        var headers=unit.Entities.OfType<SemanticRecord<FunctionEhDetails>>().Select(h=>h.Details).ToArray();
        check(headers.All(h=>h.InitialState is null&&h.NullState is null&&h.RegionTable is null&&h.ArrayTable is null&&h.CleanupRegionRows.Count==0&&h.CleanupArrayRows.Count==0),"Tableless prologues must retain absent numeric states and tables without fabricating region zero or arrays.");
        check(headers.All(h=>h.Prologue is LoweredEhOperation {ExceptionKind.Name:"leck_function_prologue"} prologue&&prologue.ExceptionOperation.FunctionEh!.Routine==h.Routine),"Tableless prologues must navigate their actual routine and per-function EH context.");
        var states=nodes.OfType<CleanupStateOperation>().ToArray();
        check(states.All(s=>s.StateNumber is null&&s.Region is null&&s.ExceptionOperation.StateNumber is null),"Actual tableless cleanup states must expose pointer recipes with no numeric state or decoded region.");
        check(nodes.Where(o=>o.Initialization is not null).All(o=>o.Initialization!.Operation is {RepresentationRole.Name:"metadata"}&&o.Children.All(c=>c!=o.Initialization.Operation)),"Tableless cleanup/completion references must retain exact metadata recipes without an evaluated initialization child.");
        if(fixture=="cleanup_semantics.roslynobj")
            check(states.Count(s=>s.CleanupStart is not null)==35&&states.Count(s=>s.IsUnreachable)==3&&states.Any(s=>s.CleanupStart is null),"The real cleanup fixture must preserve all35 pointer cleanup starts, three unreachable states and valid null cleanup pointers.");
        if(fixture=="tableless_eh.roslynobj")
        {
            var body=unit.Functions.Single(f=>f.Name=="tableless_overlapping_initialization").Body!;
            var completed=nodes.Where(o=>o.ExceptionKind.Name=="leck_initialization_completed").ToArray();
            check(completed.Length==2&&completed.Select(o=>o.CompletedInitializer!.Target!.Name).Order().SequenceEqual(new[]{"first","second"})&&completed.All(o=>o.CompletedInitializer!.Destructor?.ContainingType?.Name=="TablelessObject"),"Actual completion markers must retain the two precise object initializations and their destructor targets.");
            check(completed.All(o=>body.Lifetimes.Contains(o.CompletedInitializer!.Lifetime!))&&states.Where(s=>s.CleanupStart?.Lifetime is not null).All(s=>body.Lifetimes.Contains(s.CleanupStart!.Lifetime!)),"Function lifetime reachability must include the actual completed and cleanup-start recipe lifetimes without assigning ownership.");
            var calls=body.DescendantOperations().OfType<CallOperation>().ToArray();
            check(calls.Count(c=>c.TargetMethod is {ContainingType.Name:"TablelessObject",SpecialFunctionKind.Name:"sfk_constructor"})==2&&calls.Count(c=>c.TargetMethod is {ContainingType.Name:"TablelessTemporary",SpecialFunctionKind.Name:"sfk_constructor"})==4,"Metadata completion and cleanup links must not repeat the actual two object and four temporary constructor evaluations.");
        }
    }
}
