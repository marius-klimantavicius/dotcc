using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private void RenderWhile(StringBuilder output, While loop, int indent)
    {
        var outer = _pending.ToArray();
        _pending.Clear();
        var canHoist = _canHoist;
        // Statement storage is safe in the per-test block. Enable the existing
        // lazy branch lowering when it is needed, without changing ordinary
        // comma conditions that already have an inline representation.
        _canHoist = HasLazyEffects(loop.Cond);
        string condition;
        string[] preparation;
        try
        {
            condition = LoopCondition(loop.Cond);
            preparation = _pending.ToArray();
        }
        finally
        {
            _pending.Clear();
            _pending.AddRange(outer);
            _canHoist = canHoist;
        }
        var pad = Pad(indent);
        if (preparation.Length == 0)
        {
            output.Append(pad).Append("while (").Append(condition).Append(")\n");
            WithNormalBreak(() => Nested(output, loop.Body, indent));
            return;
        }
        // Some value carriers need real statement storage even when comma
        // hoisting is disabled. Those statements belong to each test, before
        // the body and after any continue, including the final false test.
        output.Append(pad).Append("while (true)\n").Append(pad).Append("{\n");
        foreach (var statement in preparation) output.Append(Pad(indent + 1)).Append(statement).Append(";\n");
        output.Append(Pad(indent + 1)).Append("if (!(").Append(condition).Append(")) break;\n");
        WithNormalBreak(() => Nested(output, loop.Body, indent + 1));
        output.Append(pad).Append("}\n");
    }

    // C# reachability must see that a literal nonzero loop cannot exit through
    // its condition. A call to Cond.B(1) has the right runtime result but is not
    // a constant expression for definite-return analysis. Keep other numeric
    // expressions and all side effects on the existing runtime truth path.
    private string LoopCondition(CExpr condition) => condition switch
    {
        LitInt { Value: { } value } => value == 0 ? "false" : "true",
        LitBool literal => literal.Value ? "true" : "false",
        EnumConstRef enumeration => enumeration.Sym.ConstValue == 0 ? "false" : "true",
        Paren parenthesized => LoopCondition(parenthesized.Inner),
        ComptimeFold { Resolved: { } resolved } => LoopCondition(resolved),
        _ => $"Cond.B({Expr(DecayEnum(condition))})",
    };
}
