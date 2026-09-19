#nullable enable
using System;
using System.Linq;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private int _statementExpressionCounter;

    private string RenderStatementExpression(StatementExpression expression)
    {
        var eager = _canHoist;
        var isVoid = expression.Type.Unqualified is CType.VoidType;
        if (!eager) ValidateStatementExpressionCapture(expression);
        // Nested statements own their pending prefixes. Preserve any prefixes
        // already accumulated by the containing expression while rendering them.
        var outerPending = _pending.ToArray();
        _pending.Clear();
        var body = new StringBuilder();
        var temp = "__statementExpression" + _statementExpressionCounter++;
        try
        {
            foreach (var statement in expression.Body.Stmts) Stmt(body, statement, 1);
            if (expression.Value is { } value)
            {
                if (isVoid) Stmt(body, new ExprStmt(value), 1);
                else
                {
                    var rendered = Hoist(body, Pad(1), () => Coerced(value, expression.Type));
                    body.Append(Pad(1));
                    if (eager) body.Append(temp).Append(" = ");
                    else body.Append("return ");
                    if (!eager && IsPointerType(expression.Type)) body.Append("(nint)(").Append(rendered).Append(')');
                    else body.Append(rendered);
                    body.Append(";\n");
                }
            }
        }
        finally
        {
            _pending.Clear();
            _pending.AddRange(outerPending);
            _canHoist = eager;
        }
        if (eager)
        {
            if (!isVoid) _pending.Add($"{Cs(expression.Type)} {temp}");
            _pending.Add("{\n" + body + "}");
            return isVoid ? "((global::System.Action)(() => { }))()" : temp;
        }
        if (isVoid) return $"((global::System.Action)(() => {{ {body} }}))()";
        var resultType = IsPointerType(expression.Type) ? "nint" : Cs(expression.Type);
        var call = $"((global::System.Func<{resultType}>)(() => {{ {body} }}))()";
        return IsPointerType(expression.Type) ? $"(({Cs(expression.Type)})({call}))" : call;
    }

    private static void ValidateStatementExpressionCapture(StatementExpression expression)
    {
        var locals = expression.Body.Stmts.OfType<DeclStmt>().SelectMany(d => d.Decls).Select(d => d.Sym).ToHashSet();
        VaListLifetimeValidator.Walk(expression, node =>
        {
            if (VaListLifetimeValidator.IsVaList(node.Type))
                throw new IrUnsupportedException("statement expression delegate containing va_list");
            if (node is VarRef variable && !variable.Sym.IsGlobal && !locals.Contains(variable.Sym) && variable.Sym.AddressTaken)
                throw new IrUnsupportedException("statement expression delegate capturing an address-taken local");
        });
    }
}
