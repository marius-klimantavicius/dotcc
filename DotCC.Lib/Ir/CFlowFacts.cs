namespace DotCC.Ir;

/// <summary>Call termination facts shared by C flow diagnostics and emission.</summary>
internal static class CFlowFacts
{
    internal static CExpr StatementValue(CExpr expression)
    {
        while (true)
            switch (expression)
            {
                case Paren p: expression = p.Inner; break;
                case Cast { Target.Unqualified: CType.VoidType } c: expression = c.Operand; break;
                default: return expression;
            }
    }

    internal static bool IsNoReturnCall(CExpr expression) => StatementValue(expression) switch
    {
        Call { Type.Unqualified: CType.NoReturnType } or IndirectCall { Type.Unqualified: CType.NoReturnType } => true,
        Call call => call.SemanticTarget?.DoesNotReturn == true || call.CalleeSym?.IsNoReturn == true || call.CalleeSym is
            { FromSystemHeader: true, Name: "abort" or "exit" or "_Exit" },
        _ => false,
    };

    internal static bool TerminatesExpression(CExpr expression)
    {
        expression = StatementValue(expression);
        return expression.Type.Unqualified is CType.NoReturnType || IsNoReturnCall(expression) || expression switch
        {
            Call { Callee: "__dotcc_unreachable" } => true,
            CommaOp comma => comma.Items.Any(TerminatesExpression),
            StatementExpression { Value: null } statement => statement.Body.Stmts.Any(s => s is ExprStmt e && TerminatesExpression(e.Expr)),
            CondExpr { Type.Unqualified: CType.VoidType } conditional =>
                TerminatesExpression(conditional.Then) && TerminatesExpression(conditional.Else),
            _ => false,
        };
    }
}
