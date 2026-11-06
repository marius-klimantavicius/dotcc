#nullable enable
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private CExpr? TryArrayDataPointer(CExpr value, CType.Array array)
    {
        var literal = IsStringLiteralValue(value);
        var pointer = value.Type.Unqualified as CType.Pointer;
        if (!literal && pointer?.Pointee.Unqualified is not CType.Array) return null;
        var element = literal ? CType.UChar.WithQuals(TypeQual.Const) : array.Element;
        if (pointer?.Pointee.IsConst == true) element = element.WithQuals(TypeQual.Const);
        var target = new CType.Pointer(element)
        {
            ZigSourceSize = "many",
            ZigSourceAlignment = pointer?.ZigSourceAlignment ?? (literal ? 1 : 0),
        };
        // Both pointer-to-array and its element pointer have one physical
        // address. A cast preserves the original receiver's single evaluation.
        var result = new Cast(target, value) { Type = target };
        var source = BitcastLoweredLocation(value);
        _shared.BitcastWidths.Expressions[result] = new(target,
            Element: literal ? new(element, 8) : source.Element?.Element,
            Sentinel: array.ZigSentinel is { } sentinel ? new(true, sentinel)
                : literal ? new(true, 0) : new(false, null));
        return result;
    }
}
