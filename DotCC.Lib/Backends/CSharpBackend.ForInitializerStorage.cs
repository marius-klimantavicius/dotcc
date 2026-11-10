using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private static bool ForInitializerNeedsStorage(CStmt initializer)
    {
        bool storage = false;
        foreach (var expression in AddressStatementExpressions(initializer))
            VaListLifetimeValidator.Walk(expression, node =>
            {
                if (node is CommaOp or Unary { Op: UnOp.AddrOf, Operand.IsLValue: false }) storage = true;
            });
        // A for initializer runs once. Give comma side effects and compound
        // objects statement storage in the loop's enclosing scope, retaining their lifetime across
        // the body and post expression without a delegate capturing addressed locals.
        return storage;
    }
}
