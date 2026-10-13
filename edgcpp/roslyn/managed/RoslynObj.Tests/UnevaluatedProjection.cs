using RoslynObj;

internal static class UnevaluatedProjection
{
    internal static void Verify(TranslationUnit unit,string fixture,Action<bool,string> check)
    {
        if(fixture is not ("unevaluated_microsoft.roslynobj" or "unevaluated_clang.roslynobj"))return;
        var effect=unit.Functions.Single(f=>f.Name=="unevaluated_effect");
        var nothrow=unit.Functions.Single(f=>f.Name=="unevaluated_nothrow_effect");
        var control=unit.Functions.Single(f=>f.Name=="evaluated_effect_control").Body!;
        check(control.DescendantOperations().OfType<CallOperation>().Count(c=>c.TargetMethod==effect)==1,"The ordinary call contrast must retain its one actual evaluated side effect.");
        check(unit.Functions.Where(f=>f.Name?.StartsWith("unevaluated_")==true&&f.Body is not null).All(f=>!f.Body!.DescendantOperations().OfType<CallOperation>().Any(c=>c.TargetMethod==effect||c.TargetMethod==nothrow)),"Assume, noexcept and the selected builtin queries must exclude their side-effect calls from evaluated body containment.");
        var operators=unit.Entities.OfType<OperatorOperation>().Where(o=>o.OperatorKind.Name is "eok_assume" or "eok_noexcept").ToArray();
        check(operators.Length>0&&operators.All(o=>o.Operands.Count==1&&o.Operands[0].RepresentationRole.Name=="metadata"&&!o.Children.Contains(o.Operands[0])&&o.References.Any(r=>r.Target==o.Operands[0])),"Actual unevaluated operator operands must remain ordered metadata operands and references without an evaluated child edge.");
        if(fixture=="unevaluated_microsoft.roslynobj")
            check(operators.Any(o=>o.OperatorKind.Name=="eok_assume"),"The actual Microsoft assume operator must retain its captured unevaluated operand.");
        var calls=unit.Entities.OfType<CallOperation>().Where(c=>c.TargetMethod?.BuiltinKind?.Name is "bfk_assume" or "bfk_constant_p" or "bfk_classify_type").ToArray();
        if(fixture=="unevaluated_clang.roslynobj")
            check(calls.Any(c=>c.TargetMethod?.BuiltinKind?.Name=="bfk_assume"),"The actual builtin assume call must preserve its retained intrinsic identity.");
        check(calls.All(c=>c.Arguments.Count==1&&c.Arguments[0].Value.RepresentationRole.Name=="metadata"&&!c.Children.Contains(c.Arguments[0].Value)&&c.Children.Contains(c.Callee)&&c.Operands[1]==c.Arguments[0].Value),"Selected builtin pseudo-calls must keep their canonical callee and argument objects while only the callee has an evaluated operand edge.");
        check(unit.Entities.OfType<CallOperation>().Any(c=>c.TargetMethod==effect&&c.RepresentationRole.Name=="metadata"),"The real side-effect call inside an unevaluated operand must remain available as metadata.");
    }
}
