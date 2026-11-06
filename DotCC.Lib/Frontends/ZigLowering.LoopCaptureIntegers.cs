#nullable enable
using System.Collections.Generic;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private readonly HashSet<Symbol> _runtimeIntegerLoopCaptures = new();

    // Coercing a captured value into a narrower sink must respect its proved
    // source element type. This does not change arithmetic peer inference.
    private (bool Signed, int Bits)? CertainNarrowingInteger(Item source)
    {
        if (CertainZigInt(source) is { } existing) return existing;
        while (source.Content is Zig.Grouped group) source = group.Arg1;
        if (source.Content is not Zig.Ident identifier
            || _symbols.Resolve(Tok(identifier.Arg0)) is not { Kind: SymKind.Var, IsGlobal: false } symbol
            || !_runtimeIntegerLoopCaptures.Contains(symbol) || symbol.IsConstexpr
            || _comptimeVars.ContainsKey(symbol)
            || symbol.Type.Unqualified is not CType.Prim { Integer: true, IsComptimeInt: false, Name: not ("_Bool" or "char") } type
            || !_valueBits.TryGetValue(symbol, out var bits)
            || !_shared.BitcastWidths.Values.TryGetValue(symbol, out var location) || location.IntegerBits != bits
            || _ir.ConstEval(new VarRef(symbol) { Type = symbol.Type }) is not null)
            return null;
        return (type.Signed, bits);
    }
}
