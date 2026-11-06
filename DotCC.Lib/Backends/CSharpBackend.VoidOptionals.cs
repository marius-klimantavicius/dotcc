using DotCC.Ir;
using Index = DotCC.Ir.Index;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private static bool IsVoidOptional(CType type) => type.Unqualified is
        CType.Optional { Inner.Unqualified: CType.VoidType or CType.Named { Name: "Unit" } };

    private static bool IsVoidPayloadPlace(CExpr expression)
        => expression.Type.Unqualified is CType.Named { Name: "Unit" }
            && expression is Unary { Op: UnOp.Deref } or Index;

    private bool TryCoerceVoidOptional(CExpr value, CType target, out string text)
    {
        text = "";
        if (!IsVoidOptional(target)) return false;
        if (value is NullPtr) { text = "default(ZigOptionalVoid)"; return true; }
        if (value.Type.Unqualified is CType.VoidType or CType.Named { Name: "Unit" })
        {
            text = $"ZigOptionalVoid.Some({DiscardedUnitValue(value)})";
            return true;
        }
        return false;
    }

    private bool TryRenderVoidOptional(CExpr expression, out (string, int) rendered)
    {
        rendered = default;
        switch (expression)
        {
            case Unary { Op: UnOp.Deref } dereference when IsVoidPayloadPlace(dereference):
                rendered = (DiscardedUnitValue(dereference.Operand), PPrimary);
                return true;
            case Index index when IsVoidPayloadPlace(index):
                // Unit is the expression carrier for Zig void, whose element
                // occupies zero bytes. Evaluate the address operands without
                // turning the CLR carrier's minimum size into a memory access.
                rendered = (DiscardedUnitValue(new CommaOp([index.Base, index.Idx])
                    { Type = new CType.Named("Unit") }), PPrimary);
                return true;
            case Assign assignment when IsVoidPayloadPlace(assignment.Target):
                var unit = new CType.Named("Unit");
                CExpr[] operands = assignment.Target switch
                {
                    Unary dereference => [dereference.Operand, assignment.Value],
                    Index index => [index.Base, index.Idx, assignment.Value],
                    _ => throw new System.InvalidOperationException("unknown void storage place"),
                };
                rendered = (RenderStatementExpression(new StatementExpression(
                    new Block(operands.Select(value => (CStmt)new ExprStmt(value)).ToArray()),
                    new DefaultLit { Type = unit }) { Type = unit }), PPrimary);
                return true;
            case Cast cast when TryCoerceVoidOptional(cast.Operand, cast.Target, out var converted):
                rendered = (converted, PPrimary);
                return true;
            case Member { Field: "Value" } member when IsVoidOptional(member.Base.Type):
                rendered = ($"{MemberBase(member)}.Value", PPostfix);
                return true;
            case Binary { Op: BinOp.Eq or BinOp.Ne } binary:
                var optional = binary.Left is NullPtr ? binary.Right : binary.Right is NullPtr ? binary.Left : null;
                if (optional is null || !IsVoidOptional(optional.Type)) return false;
                rendered = ((binary.Op == BinOp.Eq ? "!" : "") + Sub(optional, PPostfix) + ".HasValue", PUnary);
                return true;
            case NullCoalesce coalesce when IsVoidOptional(coalesce.Left.Type):
                var condition = new Member(coalesce.Left, "HasValue", false) { Type = CType.Bool };
                rendered = (RenderLazyConditional(new CondExpr(condition,
                    new DefaultLit { Type = CType.Void }, coalesce.Right) { Type = CType.Void }), PPrimary);
                return true;
        }
        return false;
    }
}
