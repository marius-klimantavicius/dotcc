#nullable enable
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private bool IsSourceNullLiteral(Item expression)
    {
        while (expression.Content is Zig.Grouped group) expression = group.Arg1;
        return expression.Content switch
        {
            Zig.NullLit => true,
            Zig.PreComptime forced => IsSourceNullLiteral(forced.Arg1),
            Zig.Ident identifier when _symbols.Resolve(Tok(identifier.Arg0)) is { } symbol
                && _ir.ComptimeGlobals.TryGetValue(symbol, out var value)
                => value is IrBuilder.CtNull { ZigUntypedLiteral: true },
            _ => false,
        };
    }
}
