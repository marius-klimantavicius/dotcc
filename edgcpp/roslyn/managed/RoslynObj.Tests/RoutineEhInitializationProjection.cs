using RoslynObj;

internal static class RoutineEhInitializationProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,bool source,Action<bool,string> check)
    {
        if(fixture=="routines.roslynobj")
        {
            var function=unit.Functions.Single(f=>f.Name=="routine_ifunc");var supplement=function.GnuSupplement!;
            if(source)check(supplement.ResolverVariable is null,"The unlowered ifunc must retain absent cache storage independently from its actual resolver routine.");
            else
            {
                var cache=supplement.ResolverVariable!;
                check(unit.Variables.Contains(cache)&&supplement.OwningRoutines.Contains(function)&&cache.Type is PointerType pointer&&pointer.PointeeType==function.Signature&&cache.ConstantInitializer?.Value is AddressValue {BaseEntity:var target}&&target==function&&function.IfuncResolver is {Name:"routine_resolver"} resolver&&resolver!=target,"The lowered ifunc cache must retain its canonical variable and actual pointer initializer to the ifunc entry, independently from the resolver function.");
            }
        }
        if(fixture=="routines_exception.roslynobj")
        {
            var specification=unit.Functions.Single(f=>f.Name=="routine_throw_list").Signature.ExceptionSpecification!;
            var rows=specification.Details.ExceptionSpecificationTypeRows;
            SemanticProjection.Sweep(rows,"exception_specification_type",check);
            check(rows.Count==2&&rows.Select(r=>r.Ordinal).SequenceEqual(new long[]{0,1})&&rows.All(r=>r.Owner==specification&&!r.IsRedundant&&!r.IsPackExpansion)&&rows.Select(r=>r.Type).SequenceEqual(specification.Types),"The actual throw-list entries must retain their own ordered type identities and direct false redundancy/pack flags independently from the parent exception descriptor.");
        }
        if(fixture=="cleanup_semantics.roslynobj")
        {
            if(source)VerifyConstructorArguments(unit,check);
            else if(unit.Capabilities.Any(c=>c.Name=="GENERATE_EH_TABLES"&&c.Enabled==true))VerifyTableVariables(unit,check);
        }
        if(fixture=="tableless_eh.roslynobj")
        {
            var body=unit.Functions.Single(f=>f.Name=="tableless_overlapping_initialization").Body!;
            var completed=unit.Entities.OfType<LoweredEhOperation>().Where(o=>o.CompletedInitializer is not null).ToArray();
            check(completed.Length==2&&completed.Select(o=>o.CompletedInitializer!.Target!.Name).Order().SequenceEqual(new[]{"first","second"})&&completed.All(o=>o.CompletedInitializer is {OverlappingLifetime: {Kind.Name:"olk_expr_temporary",BoundEntity:BlockDescriptor block} overlap} initialization&&overlap!=initialization.Lifetime&&block.Lifetime==overlap&&overlap.Cleanups.Count==2&&overlap.Cleanups.All(c=>c.Initialization.Destructor?.ContainingType?.Name=="TablelessTemporary")&&body.Lifetimes.Contains(overlap)&&initialization.Operation is {RepresentationRole.Name:"metadata"} operation&&!o.Children.Contains(operation)),"Both tableless completion recipes must retain their distinct overlapping temporary lifetime, exact bound block and two temporary cleanup recipes, reachable from the body without promoting the metadata recipe into execution.");
        }
    }

    private static void VerifyConstructorArguments(TranslationUnit unit,Action<bool,string> check)
    {
        var function=unit.Functions.Single(f=>f.Name=="cleanup_paths");var body=function.Body!;var input=function.SourceParameters.Single(p=>p.Name=="input").Symbol!;
        var names=new[]{"outer","inner","separate","thrown"};
        var initializers=unit.Entities.OfType<Initialization>().Where(i=>i.Target is VariableSymbol v&&names.Contains(v.Name)).ToArray();
        check(initializers.Length==4&&initializers.All(i=>i.Constructor?.ContainingType?.Name=="CleanupBase"&&i.Operands.Count==1&&i.Operands.Single() is {Ordinal:0,Role.Name:"argument",Expression:not null} operand&&operand.Owner==i&&i.Operation is {RepresentationRole.Name:"source_semantic"} operation&&operation.ChildRelations.Single(r=>r.Role.Name=="argument"&&r.Ordinal==0).Child==operand.Expression&&body.DescendantOperations().Count(o=>o==operand.Expression)==1),"The four actual object constructor recipes must retain their canonical argument-zero expressions and exact evaluated child edges, without adding the implied destination or copy source to the argument list.");
        var outer=initializers.Single(i=>i.Target!.Name=="outer");
        check(outer.Operands.Single().Expression is ReferenceOperation {Target:var target}&&target==input,"The outer constructor argument must resolve the exact enclosing function input parameter.");
        foreach(var (name,number) in new[]{("inner",1),("separate",2),("thrown",3)})
        {
            var expression=(OperatorOperation)initializers.Single(i=>i.Target!.Name==name).Operands.Single().Expression!;
            check(expression.OperatorKind.Name=="eok_add"&&expression.Operands.Count==2&&expression.Operands[0] is ReferenceOperation {Target:var parameter}&&parameter==input&&expression.Operands[1] is ConstantOperation {Constant.Value:IntegerValue integer}&&integer.Value==number,"Each retained constructor addition must preserve input then its exact one/two/three constant in operand order.");
        }
    }

    private static void VerifyTableVariables(TranslationUnit unit,Action<bool,string> check)
    {
        var function=unit.Functions.Single(f=>f.Name=="cleanup_array_parameter");var info=function.Body!.ExceptionHandling;
        var region=info.RegionTable!;var array=info.ArrayTable!;
        check(region!=array&&unit.Variables.Contains(region)&&unit.Variables.Contains(array)&&region.Type is ArrayType&&array.Type is ArrayType&&region.ExceptionTable is {Kind.Name:"cleanup"} regionProjection&&regionProjection.Owner==region&&regionProjection.FunctionEh.Routine==function&&regionProjection.FunctionEh.RegionTable==region&&array.ExceptionTable is {Kind.Name:"element"} arrayProjection&&arrayProjection.Owner==array&&arrayProjection.FunctionEh.Routine==function&&arrayProjection.FunctionEh.ArrayTable==array,"The actual EH region and array tables must navigate as two distinct canonical storage variables with exact variable-owned projection back to the same routine.");
        check(info.CleanupRegions.Count>0&&info.Arrays.Count>0&&info.CleanupRegions.All(r=>r.Details.FunctionEh.Routine==function)&&info.Arrays.All(a=>a.Details.FunctionEh.Routine==function)&&info.Details!.RegionTable==region&&info.Details.ArrayTable==array,"Decoded cleanup regions and arrays must remain separately navigable from their actual table variables and retain the same recorded function context.");
    }
}
