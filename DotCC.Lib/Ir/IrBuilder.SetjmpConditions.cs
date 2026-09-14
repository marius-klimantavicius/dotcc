#nullable enable

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    // Keep a numeric result even when C only tests !setjmp. The restart handler
    // remains active in both branches and the enclosing block tail, including
    // repeated jumps initiated from recovery code.
    private CStmt? SetjmpNegatedGuardOf(CExpr cond, CStmt then, CStmt? els, SrcPos pos)
    {
        var inner = cond;
        while (inner is Paren paren) inner = paren.Inner;
        if (inner is not Unary { Op: UnOp.LogNot, Operand: var operand }
            || !IsSetjmpCall(operand, out var env, out var call)) return null;
        _setjmpCalls.Remove(call);
        var symbol = _symbols.Declare(new Symbol { Name = "__sjval" + _setjmpSeq,
            Kind = SymKind.Var, Type = CType.Int, Storage = Storage.Auto });
        var target = new VarRef(symbol) { Type = CType.Int, IsLValue = true };
        var declaration = new DeclStmt(new[] { new LocalDecl(symbol, ZeroLit()) }) { Pos = pos };
        var condition = new Unary(UnOp.LogNot, target) { Type = CType.Int, Pos = cond.Pos };
        var body = new If(condition, then, els) { Pos = pos };
        return new Seq(new CStmt[] { declaration,
            new SetjmpCapture(env, target, body, _setjmpSeq++) { Pos = pos } }) { Pos = pos };
    }

    /// <summary>Capture the value in `if (!(rc = setjmp(env)))` (or the
    /// unnegated condition), then rerun the condition and branches on a jump.
    /// Only simple variable targets are accepted: assigning through a pointer
    /// or indexed lvalue would require capturing that address exactly once.</summary>
    private CStmt? SetjmpAssignmentGuardOf(CExpr cond, CStmt then, CStmt? els, SrcPos pos)
    {
        var inner = cond;
        while (inner is Paren paren) inner = paren.Inner;
        var negated = inner is Unary { Op: UnOp.LogNot };
        if (negated) inner = ((Unary)inner).Operand;
        while (inner is Paren paren) inner = paren.Inner;
        if (inner is not Assign { CompoundOp: null, Target: VarRef target, Value: var value }
            || !IsSetjmpCall(value, out var env, out var call)) return null;

        _setjmpCalls.Remove(call);
        CExpr condition = negated
            ? new Unary(UnOp.LogNot, target) { Type = CType.Int, Pos = cond.Pos }
            : target;
        var body = new If(condition, then, els) { Pos = pos };
        return new Seq(new CStmt[] { EmptyStmt(pos), new SetjmpCapture(env, target, body, _setjmpSeq++)
            { Pos = pos, ResetTargetAfterArm = true } }) { Pos = pos };
    }
}
