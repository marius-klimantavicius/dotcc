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
                if (node is Unary { Op: UnOp.AddrOf, Operand.IsLValue: false }) storage = true;
            });
        // A for initializer runs once. Give its compound objects statement
        // storage in the loop's enclosing scope, retaining their lifetime across
        // the body and post expression without an escaping delegate stack local.
        return storage;
    }
}
