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
        // Returns and transfers targeting an enclosing function/loop cannot
        // cross a deferred delegate. Loops wholly inside this expression keep
        // their own break/continue targets in both eager and deferred lowering.
        bool Supported(CStmt statement, int loops = 0) => statement switch
        {
            DeclStmt or ExprStmt => true,
            Block block => block.Stmts.All(child => Supported(child, loops)),
            Seq sequence => sequence.Stmts.All(child => Supported(child, loops)),
            If conditional => Supported(conditional.Then, loops) &&
                (conditional.Else is null || Supported(conditional.Else, loops)),
            While loop => Supported(loop.Body, loops + 1),
            DoWhile loop => Supported(loop.Body, loops + 1),
            For loop => (loop.Init is null || Supported(loop.Init, loops)) && Supported(loop.Body, loops + 1),
            Break or Continue => loops > 0,
            _ => false,
        };
        if (body.Stmts.FirstOrDefault(statement => !Supported(statement)) is { } unsupported)
            throw new IrUnsupportedException($"statement expression with control flow or non-scalar declarations: {unsupported.GetType().Name} at {unsupported.Pos}");
        var value = (body.Stmts.LastOrDefault() as ExprStmt)?.Expr;
        var statements = value is null ? body.Stmts : body.Stmts.Take(body.Stmts.Count - 1).ToArray();
        return new StatementExpression(new Block(statements), value)
        {
            Type = value?.Type.Unqualified ?? CType.Void,
            IsLValue = false,
        };
    }
}
