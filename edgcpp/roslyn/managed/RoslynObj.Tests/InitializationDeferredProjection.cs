using RoslynObj;

internal static class InitializationDeferredProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,string profile,Action<bool,string> check)
    {
        if(fixture!="initialization_deferred.roslynobj")return;
        VerifyDeferredContexts(unit,check);
        var initializers=unit.Entities.OfType<Initialization>().ToArray();
        if(profile!="source_semantic")
        {
            check(initializers.All(i=>i.InitExpressionLifetime is null&&i.Master is null),"This lowered snapshot retains neither initializer-expression lifetime nor shared-master pointers; the reader must preserve those absences.");
            return;
        }
        VerifyInitializerLifetime(unit,check);
        VerifyMaster(unit,initializers,check);
    }

    private static void VerifyInitializerLifetime(TranslationUnit unit,Action<bool,string> check)
    {
        var function=unit.Functions.Single(f=>f.Name=="initialization_lifetime");var body=function.Body!;
        var variable=unit.Variables.Single(v=>v.Name=="value"&&v.SourceCorrespondence!.EnclosingRoutine==function);
        var initializer=variable.Initializer!;var temporary=initializer.InitExpressionLifetime!;var lifetime=initializer.Lifetime!;
        check(initializer.Constructor!.ContainingType!.Name=="Owner"&&temporary!=lifetime&&temporary.Kind.Name=="olk_expr_temporary"&&temporary.BoundEntity==initializer&&body.Lifetimes.Contains(temporary)&&body.Lifetimes.Contains(lifetime),"The Owner initializer must retain its exact full-expression temporary lifetime, reciprocal initializer binding and separate object lifetime through the body lifetime graph.");
        var cleanup=temporary.Cleanups.Single();
        check(cleanup.Initialization.Constructor!.ContainingType!.Name=="Tmp"&&cleanup.Initialization.Destructor!.ContainingType!.Name=="Tmp"&&lifetime.Cleanups.Any(c=>c.Initialization==initializer&&c.Initialization.Destructor!.ContainingType!.Name=="Owner"),"The initializer-expression lifetime must retain the actual Tmp construction/destruction recipe separately from the Owner object's cleanup obligation.");
    }

    private static void VerifyMaster(TranslationUnit unit,Initialization[] initializers,Action<bool,string> check)
    {
        var operands=initializers.Where(i=>i.Master is not null).ToArray();var master=operands.Select(i=>i.Master!).Distinct().Single();
        var function=unit.Functions.Single(f=>f.Name=="master_conditional");
        check(operands.Length==2&&operands.All(i=>i.Constructor!.ContainingType!.Name=="Item"&&i.Master==master)&&master.Details.IsClassResultMaster&&master.Expression is OperatorOperation {OperatorKind.Name:"eok_question"},"The two actual conditional Item construction recipes must share the exact class-result master initializer and its retained question expression.");
        check(operands.All(i=>!i.Operation!.Children.Contains(master.Operation!))&&function.Body!.DescendantOperations().OfType<InitializationOperation>().Count(o=>o.Initialization==master)==1&&master.Lifetime!.Cleanups.Single().Initialization==master,"A master pointer must add no evaluated operand edge or duplicate initialization; the actual master site and sole destruction recipe remain independently recorded.");
    }

    private static void VerifyDeferredContexts(TranslationUnit unit,Action<bool,string> check)
    {
        var wrappers=unit.Entities.OfType<DependentOperation>().Where(d=>d.Kind.Name=="enk_const_eval_deferred").ToArray();
        var function=unit.Functions.Single(f=>f.Name=="location_value");
        var field=unit.Types.OfType<RecordType>().Single(t=>t.Name=="Deferred").Fields.Single(f=>f.Name=="value");
        var member=(DependentOperation)field.DefaultInitializer!.Expression!;
        check(member.ReattemptDefaultArgument==false&&member.ReattemptDefaultMemberInitializer==true&&member.WrappedExpression is CallOperation call&&call.TargetMethod==function&&field.DefaultInitializer.Operation!.RepresentationRole.Name=="metadata","The declared member initializer must retain its exact DMI-only deferred state and metadata call to the canonical consteval routine.");
        var parameter=function.Signature.Parameters.Single(p=>p.Name=="line");
        var argument=(DependentOperation)parameter.DefaultExpression!.Operands.Single();
        check(argument.ReattemptDefaultArgument==true&&argument.ReattemptDefaultMemberInitializer==true&&argument.WrappedExpression is CallOperation {TargetMethod.Name:"__builtin_LINE"},"The retained default parameter must preserve both recorded retry-context flags and its actual builtin call; these flags are not mutually exclusive.");
        check(wrappers.Length>0&&wrappers.All(w=>w.ReattemptDefaultArgument is not null&&w.ReattemptDefaultMemberInitializer==true&&w.RepresentationRole.Name=="metadata"&&w.WrappedExpression is CallOperation {RepresentationRole.Name:"metadata"} target&&w.References.Single(r=>r.Role.Name=="metadata"&&r.Ordinal==0).Target==target&&!w.Children.Contains(target)),"Each retained deferred wrapper must expose its recorded state and exact metadata call reference without evaluating the call again.");
        var body=unit.Functions.Single(f=>f.Name=="use_deferred").Body!;
        var used=body.DescendantOperations().OfType<CallOperation>().Single(c=>c.TargetMethod?.Name=="defaulted");
        check(used.Arguments.Single().Value is ConstantOperation {Constant.Value:IntegerValue {Value:var value}}&&value==13&&!body.DescendantOperations().OfType<DependentOperation>().Any(d=>wrappers.Contains(d)),"The actual default use must retain the stock-folded integer argument13 in its executed call, independently from the metadata deferred declaration expressions.");
    }
}
