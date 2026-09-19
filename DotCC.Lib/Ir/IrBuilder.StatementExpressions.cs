#nullable enable
using System.Linq;
using Item = global::LALR.CC.LexicalGrammar.Item;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private static void RejectStatementExpressionStorage(CExpr expression)
    {
        bool UsesValueTemporary(CExpr node) => node switch
        {
            StatementExpression => true,
            Paren p => UsesValueTemporary(p.Inner),
            Member { Arrow: false } member => UsesValueTemporary(member.Base),
            CommaOp comma => UsesValueTemporary(comma.Items[^1]),
            _ => false,
        };
        if (UsesValueTemporary(expression))
            throw new IrUnsupportedException("statement expression used as an lvalue");
    }

    private void ValidateGnuObjectAttribute(Item item)
    {
        if (item.Content is not C.GnuFunctionAttrs attributes)
            throw new IrUnsupportedException("unsupported GNU attribute on object or parameter");
        void Validate(Item current)
        {
            switch (current.Content)
            {
                case C.AttrListCons list: Validate(list.Arg0); Validate(list.Arg2); break;
                case C.AttrIdent name when Tok(name.Arg0).Trim('_') == "unused": break;
                default: throw new IrUnsupportedException("unsupported GNU attribute on object or parameter");
            }
        }
        Validate(attributes.Arg3);
    }

    private CExpr BuildStatementExpression(Item item)
    {
        var body = BuildBlock(item);
        // Keep control transfers in the enclosing C function from accidentally
        // becoming returns/breaks inside a generated delegate. Broader control
        // flow requires a dedicated lowering pass; reject it explicitly for now.
        if (body.Stmts.Any(s => s is not (DeclStmt or ExprStmt)))
            throw new IrUnsupportedException("statement expression with control flow or non-scalar declarations");
        var value = (body.Stmts.LastOrDefault() as ExprStmt)?.Expr;
        var statements = value is null ? body.Stmts : body.Stmts.Take(body.Stmts.Count - 1).ToArray();
        return new StatementExpression(new Block(statements), value)
        {
            Type = value?.Type.Unqualified ?? CType.Void,
            IsLValue = false,
        };
    }
}
