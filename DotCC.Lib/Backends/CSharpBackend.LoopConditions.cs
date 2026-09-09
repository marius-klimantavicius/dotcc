using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    // C# reachability must see that a literal nonzero loop cannot exit through
    // its condition. A call to Cond.B(1) has the right runtime result but is not
    // a constant expression for definite-return analysis. Keep other numeric
    // expressions and all side effects on the existing runtime truth path.
    private string LoopCondition(CExpr condition) => condition switch
    {
        LitInt { Value: { } value } => value == 0 ? "false" : "true",
        EnumConstRef enumeration => enumeration.Sym.ConstValue == 0 ? "false" : "true",
        Paren parenthesized => LoopCondition(parenthesized.Inner),
        ComptimeFold { Resolved: { } resolved } => LoopCondition(resolved),
        _ => $"Cond.B({Expr(DecayEnum(condition))})",
    };
}
