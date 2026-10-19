using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private static bool HasLazyEffects(CExpr expression)
    {
        var comma = false;
        var addressedLocal = false;
        var compoundArray = false;
        VaListLifetimeValidator.Walk(expression, node =>
        {
            if (node is CommaOp) comma = true;
            if (node is StackArray) compoundArray = true;
            if (node is VarRef { Sym.IsGlobal: false, Sym.AddressTaken: true }) addressedLocal = true;
        });
        // stackalloc needs an addressable local in its selected branch. A
        // conditional's common-type inference otherwise turns it into Span<T>,
        // and hoisting both arms would also execute unselected side effects.
        return compoundArray || comma && addressedLocal;
    }

    // Keep lazy comma/statement-expression effects in their original branch,
    // without a delegate capturing C stack locals that may be addressed elsewhere.
    private string RenderLazyConditional(CondExpr expression)
    {
        var condition = Expr(DecayEnum(expression.Cond));
        var prefixes = _pending.ToArray();
        _pending.Clear();
        var name = "__conditional" + _clCounter++;
        var body = new StringBuilder();
        var isVoid = expression.Type.Unqualified is CType.VoidType;
        void Arm(CExpr value)
        {
            if (isVoid) Stmt(body, new ExprStmt(value), 1);
            else
            {
                var rendered = Hoist(body, Pad(1), () => Coerced(value, expression.Type));
                body.Append(Pad(1)).Append(name).Append(" = ").Append(rendered).Append(";\n");
            }
        }
        try
        {
            body.Append("if (Cond.B(").Append(condition).Append("))\n{\n");
            Arm(expression.Then);
            body.Append("}\nelse\n{\n");
            Arm(expression.Else);
            body.Append("}");
        }
        finally
        {
            _pending.Clear();
            _pending.AddRange(prefixes);
        }
        if (!isVoid) _pending.Add(Cs(expression.Type) + " " + name + " = default");
        _pending.Add(body.ToString());
        return isVoid ? "((global::System.Action)(static () => { }))()" : name;
    }

    private (string, int) RenderLazyLogical(Binary expression)
    {
        var truth = new Unary(UnOp.LogNot, new Unary(UnOp.LogNot, expression.Right) { Type = CType.Int }) { Type = CType.Int };
        CExpr Literal(int value) => new LitInt(value.ToString(System.Globalization.CultureInfo.InvariantCulture), value) { Type = CType.Int };
        var conditional = expression.Op == BinOp.LogAnd
            ? new CondExpr(expression.Left, truth, Literal(0)) { Type = CType.Int }
            : new CondExpr(expression.Left, Literal(1), truth) { Type = CType.Int };
        return (RenderLazyConditional(conditional), PPrimary);
    }
}
