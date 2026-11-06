#nullable enable
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private ZigResultLocation? CoerceConditionalIntegerPeers(Item thenSource, ref CExpr then,
        Item elseSource, ref CExpr otherwise)
    {
        // The source integer peer supplies a literal's type before selecting
        // the conditional result. Conversion only folds proved constants; arm
        // statements and the condition keep their existing evaluation scopes.
        var originalThen = then;
        then = CoerceComptimeIntegerPeer(thenSource, then, elseSource, otherwise,
            arithmetic: true, out var thenLocation);
        otherwise = CoerceComptimeIntegerPeer(elseSource, otherwise, thenSource, originalThen,
            arithmetic: true, out var elseLocation);
        return thenLocation ?? elseLocation;
    }

    private CExpr RememberConditionalIntegerResult(CExpr value, ZigResultLocation? location)
    {
        if (location is null) return value;
        location = location with { Physical = value.Type };
        _shared.BitcastWidths.Expressions[value] = location;
        if (value is VarRef temporary)
        {
            RememberBitcastValue(temporary.Sym, location);
            RecordValueBits(temporary.Sym, location.IntegerBits, location.Element?.IntegerBits);
        }
        return value;
    }
}
