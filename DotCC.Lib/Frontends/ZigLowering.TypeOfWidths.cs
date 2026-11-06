#nullable enable
using System.Collections.Generic;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // @TypeOf(a, b) keeps the logical width of its integer peer. Carrier
    // equality is insufficient: u2 and u7 both lower to byte. Only witnessed
    // integer domains of one signedness establish the wider source width.
    private int? DeclaredBitsOfTypeOfPeers(IReadOnlyList<Item> arguments)
    {
        int? width = null;
        bool? signed = null;
        foreach (var argument in arguments)
        {
            if (IsSourceNullLiteral(argument)) continue;
            var (type, comptimeInteger) = TypeOfOperand(argument);
            if (comptimeInteger) continue;
            var domain = type.Unqualified is CType.Optional optional ? optional.Inner.Unqualified : type.Unqualified;
            if (domain is not CType.Prim { Integer: true, IsComptimeInt: false, Name: not "_Bool" } integer)
                return null;
            var bits = argument.Content is Zig.Ident identifier && _anytypeSeedBits.TryGetValue(Tok(identifier.Arg0), out var seeded)
                ? seeded : DeclaredBitsOfArgument(argument);
            if (bits is not (>= 0 and <= 128) || signed is { } prior && prior != integer.Signed)
                return null;
            signed = integer.Signed;
            width = width is { } known ? System.Math.Max(known, bits.Value) : bits;
        }
        return width;
    }
}
