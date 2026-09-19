using System.Collections.Generic;
using System.Linq;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private static string UserTypedefName(string name)
    {
        if (name == RuntimeIntrinsicNames.IsLittleEndian)
            throw new IrUnsupportedException("reserved runtime intrinsic name cannot be declared as a typedef: " + name);
        return name;
    }

    internal HashSet<RuntimeIntrinsicKind> RuntimeIntrinsicsUsed { get; } = new();

    // A structural dependency check, including unelected conditional arms. The
    // ordinary constant evaluator can short-circuit those arms; a runtime fact
    // must still never become a compiler-host constant or a static initializer.
    internal static bool ContainsRuntimeIntrinsic(CExpr? expr) => expr switch
    {
        RuntimeIntrinsic => true,
        Unary u => ContainsRuntimeIntrinsic(u.Operand),
        Binary b => ContainsRuntimeIntrinsic(b.Left) || ContainsRuntimeIntrinsic(b.Right),
        Assign a => ContainsRuntimeIntrinsic(a.Target) || ContainsRuntimeIntrinsic(a.Value),
        Cast c => ContainsRuntimeIntrinsic(c.Operand),
        BitCast c => ContainsRuntimeIntrinsic(c.Operand),
        Paren p => ContainsRuntimeIntrinsic(p.Inner),
        CondExpr c => ContainsRuntimeIntrinsic(c.Cond) || ContainsRuntimeIntrinsic(c.Then) || ContainsRuntimeIntrinsic(c.Else),
        Call c => c.Args.Any(ContainsRuntimeIntrinsic),
        IndirectCall c => ContainsRuntimeIntrinsic(c.Callee) || c.Args.Any(ContainsRuntimeIntrinsic),
        Member m => ContainsRuntimeIntrinsic(m.Base),
        Index i => ContainsRuntimeIntrinsic(i.Base) || ContainsRuntimeIntrinsic(i.Idx),
        CommaOp c => c.Items.Any(ContainsRuntimeIntrinsic),
        CommaSeq c => c.Items.Any(ContainsRuntimeIntrinsic),
        StatementExpression s => ContainsRuntimeIntrinsic(s.Value) || s.Body.Stmts.Any(statement => statement switch
        {
            DeclStmt d => d.Decls.Any(declaration => ContainsRuntimeIntrinsic(declaration.Init)),
            ExprStmt e => ContainsRuntimeIntrinsic(e.Expr),
            _ => false, // The builder rejects other statement-expression body shapes.
        }),
        StructInit s => s.Members.Any(m => ContainsRuntimeIntrinsic(m.Value)),
        InlineArrayInit a => a.Elems.Any(ContainsRuntimeIntrinsic),
        FlexibleAggregateInit f => ContainsRuntimeIntrinsic(f.Header) || f.Elems.Any(ContainsRuntimeIntrinsic),
        StackArray a => a.Elems.Any(ContainsRuntimeIntrinsic),
        PinnedArray a => ContainsRuntimeIntrinsic(a.Count) || (a.Elems?.Any(ContainsRuntimeIntrinsic) ?? false),
        VaArgGet a => ContainsRuntimeIntrinsic(a.Ap),
        _ => false, // C literal, reference, sizeof and offsetof leaves; no runtime operand.
    };

    internal static CExpr RequireNoRuntimeIntrinsic(CExpr expr, string context)
    {
        if (ContainsRuntimeIntrinsic(expr))
            throw new IrUnsupportedException("runtime intrinsic is not allowed in " + context);
        return expr;
    }
}
