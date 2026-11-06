using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // Result-location builtins and enum literals still infer the payload type. An
    // already-typed optional fallback keeps its presence bit, including known none.
    private CExpr LowerOrElseFallback(Item expression, CType leftType)
    {
        if (leftType.Unqualified is not CType.Optional optional) return LowerExpr(expression);
        var value = LowerExprSink(expression, optional.Inner);
        return value is NullPtr ? new DefaultLit { Type = leftType } : value;
    }

    private static CType OrElseResultType(CType leftType, CExpr right)
    {
        if (leftType.Unqualified is not CType.Optional optional) return OptionalPointerPayloadType(leftType);
        if (right.Type.Unqualified is not CType.Optional fallback) return optional.Inner;
        if (!optional.Inner.Unqualified.Equals(fallback.Inner.Unqualified))
            throw new IrUnsupportedException($"zig `orelse` has incompatible optional fallback types: {leftType.Describe()} and {right.Type.Describe()}");
        return leftType;
    }

    private static CType OptionalPointerPayloadType(CType type)
        => type.Unqualified switch
        {
            CType.Pointer { ZigOptional: true } pointer => pointer with { ZigOptional = false },
            CType.Func { ZigOptional: true } function => function with { ZigOptional = false },
            _ => type,
        };

    private static CExpr OptionalPointerPayload(CExpr value)
        => OptionalPointerPayloadType(value.Type) is var type && !type.Equals(value.Type)
            ? new Cast(type, value) { Type = type } : value;

    private static CExpr OptionalConditionalArm(CExpr value, CType optional)
        => value.Type.Unqualified == optional.Unqualified || IsNoreturn(value)
            ? value : new Cast(optional, value) { Type = optional };
}
