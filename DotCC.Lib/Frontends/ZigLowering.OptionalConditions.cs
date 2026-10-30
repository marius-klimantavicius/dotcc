using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private static CExpr OptionalConditionalArm(CExpr value, CType optional)
        => value.Type.Unqualified == optional.Unqualified || IsNoreturn(value)
            ? value : new Cast(optional, value) { Type = optional };
}
