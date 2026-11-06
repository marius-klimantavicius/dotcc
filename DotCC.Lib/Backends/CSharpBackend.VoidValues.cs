using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    // Zig void has one value and no field storage. Keep its initializer's
    // effects in the same evaluation slot as stored aggregate members. Unit is
    // only an expression/argument-pack carrier, never a source layout field.
    private string DiscardedUnitValue(CExpr value)
    {
        if (IsPure(value)) return "default(Unit)";
        var unit = new CType.Named("Unit");
        return RenderStatementExpression(new StatementExpression(new Block([new ExprStmt(value)]),
            new DefaultLit { Type = unit }) { Type = unit });
    }
}
