#nullable enable

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    // A compiler-only environment expression inside the existing lexical jump
    // nodes. Ordinary Call walkers retain BOTH operands through normalization
    // and malloc rewriting; this never becomes an ordinary emitted C call.
    internal const string SignalJumpEnvironmentCallee = "__dotcc_sigsetjmp_environment";

    private bool IsSignalSetjmpCall(Call call) => call.Callee == "sigsetjmp"
        && _symbols.Resolve("sigsetjmp") is not { Kind: SymKind.Var or SymKind.Param };

    private static CExpr SignalJumpEnvironment(Call call) =>
        new Call(SignalJumpEnvironmentCallee, call.Args)
        { Type = new CType.Pointer(CType.ULongLong), Pos = call.Pos };

    // pg_get_line_append uses `context && sigsetjmp(..., 1) != 0`, then
    // performs work after the if. Cache the completed left operand, conditionally
    // arm, and capture the entire enclosing block tail just like an assignment
    // guard. Neither the prefix nor the jump arguments run again on recovery.
    private CStmt? SignalJumpShortCircuitGuardOf(CExpr condition, CStmt then, CStmt? els, SrcPos pos)
    {
        static CExpr Peel(CExpr expression)
        {
            while (expression is Paren paren) expression = paren.Inner;
            return expression;
        }

        if (Peel(condition) is not Binary { Op: BinOp.LogAnd, Left: var prefix, Right: var right }) return null;
        right = Peel(right);
        CExpr candidate = right;
        bool trueOnDirect = false;
        if (right is Unary { Op: UnOp.LogNot, Operand: var operand })
        { candidate = operand; trueOnDirect = true; }
        else if (right is Binary { Op: BinOp.Eq or BinOp.Ne } compare)
        {
            if (IsZeroLit(compare.Right)) candidate = compare.Left;
            else if (IsZeroLit(compare.Left)) candidate = compare.Right;
            else return null;
            trueOnDirect = compare.Op == BinOp.Eq;
        }
        if (!IsSetjmpCall(candidate, out var environment, out var call) || call.Callee != "sigsetjmp") return null;
        _setjmpCalls.Remove(call);

        var prefixSymbol = _symbols.Declare(new Symbol { Name = "__sjprefix" + _setjmpSeq,
            Kind = SymKind.Var, Type = CType.Int, Storage = Storage.Auto });
        var resultSymbol = _symbols.Declare(new Symbol { Name = "__sjval" + _setjmpSeq,
            Kind = SymKind.Var, Type = CType.Int, Storage = Storage.Auto });
        var prefixRef = new VarRef(prefixSymbol) { Type = CType.Int, IsLValue = true };
        var resultRef = new VarRef(resultSymbol) { Type = CType.Int, IsLValue = true };
        var prefixValue = new Unary(UnOp.LogNot, new Unary(UnOp.LogNot, prefix) { Type = CType.Int }) { Type = CType.Int };
        var declarations = new DeclStmt(new[] { new LocalDecl(prefixSymbol, prefixValue),
            new LocalDecl(resultSymbol, ZeroLit()) }) { Pos = pos };
        // The optional third environment operand is an already-evaluated guard.
        var conditionalEnvironment = new Call(SignalJumpEnvironmentCallee,
            new[] { call.Args[0], call.Args[1], prefixRef }) { Type = environment.Type, Pos = call.Pos };
        var test = new Binary(trueOnDirect ? BinOp.Eq : BinOp.Ne, resultRef, ZeroLit()) { Type = CType.Int };
        var cachedCondition = new Binary(BinOp.LogAnd, prefixRef, test) { Type = CType.Int, Pos = condition.Pos };
        var body = new If(cachedCondition, then, els) { Pos = pos };
        return new Seq(new CStmt[] { declarations,
            new SetjmpCapture(conditionalEnvironment, resultRef, body, _setjmpSeq++) { Pos = pos } }) { Pos = pos };
    }
}
