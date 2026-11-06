using System.Collections.Generic;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private (CType Type, int? Bits) LowerDependentParameterType(Item annotation, IReadOnlyList<ValueSeed> precedingValues)
    {
        // Parameter annotations see earlier resolved formal values. The next
        // actual argument is evaluated after this temporary formal scope ends.
        using var scope = EnterSymbolScope();
        foreach (var seed in precedingValues) DeclareValueSeed(seed);
        var type = LowerType(annotation);
        return (type, DeclaredBitsOfTypeArg(annotation));
    }
}
