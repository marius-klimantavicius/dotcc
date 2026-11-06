#nullable enable
using System.Collections.Generic;
using System.Globalization;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private CExpr LowerRuntimeTagName(Item operand)
    {
        var subject = UnionTagOrSelf(LowerExpr(operand));
        if (subject.Type.Unqualified is not CType.Enum enumeration || MembersOfEnum(enumeration) is not { } members)
            throw new CompileException($"zig: @tagName requires an enum or tagged union ({_module?.Path ?? _fileStem}:{operand.Position.Line}, function '{_currentFnName}')");
        var resultType = SourceSlice(CType.UChar.WithQuals(TypeQual.Const));
        var arms = new List<SwitchExprArm>();
        // The backend governs the switch in the enum's promoted integer type.
        // Generated numeric patterns need that same type; a u16 literal would
        // otherwise render a uint suffix beside an int governing expression.
        var labelType = CType.IntegerPromote(enumeration.Underlying);
        foreach (var member in members)
        {
            var integer = new LitInt(member.Value.ToString(CultureInfo.InvariantCulture), member.Value) { Type = labelType };
            arms.Add(new SwitchExprArm([new SwitchLabel(integer)], CoerceToSlice(ZigStringLiteral(member.Name), resultType)));
        }
        // Non-exhaustive enums can contain unnamed values. Zig's @tagName is
        // illegal on those values; preserve the trap instead of naming a tag.
        arms.Add(new SwitchExprArm(null, CoerceNoreturn(UnreachableCall(), resultType)));
        return new SwitchExpr(subject, arms) { Type = resultType };
    }
}
