namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private static CType SourceObjectType(CExpr expression)
    {
        while (expression is Paren parenthesized) expression = parenthesized.Inner;
        return expression is VarRef { Sym.DeclaredArrayType: { } array } ? array : expression.Type;
    }
}
