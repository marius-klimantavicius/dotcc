using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private bool IsEmptyPayload(CType type) => type.Unqualified is CType.Named named
        && _aggregateDefinitions.TryGetValue(named.Name, out var definition)
        && definition.Fields.Count == 0 && definition.ZigStorage is null or { Kind: ZigStorageKind.Zero }
        && _offsetModel.Type(IrBuilder.LayoutType(type)) is { Size: 0, Alignment: 1 };

    private bool IsZeroOptional(CType type) => type.Unqualified is CType.Optional optional && IsEmptyPayload(optional.Inner);

    private bool TryRenderZeroOptional(CExpr expression, out (string, int) rendered)
    {
        rendered = default;
        switch (expression)
        {
            case Member { Field: "Value" } member when IsZeroOptional(member.Base.Type):
                rendered = ($"{MemberBase(member)}{(member.Arrow ? "->" : ".")}Value", PPostfix);
                return true;
            case Unary { Op: UnOp.Deref } read when IsEmptyPayload(read.Type):
                rendered = ($"ZigOptionalZero<{Cs(read.Type)}>.Read((void*)({Expr(read.Operand)}))", PPrimary);
                return true;
            case Assign { Target: Unary { Op: UnOp.Deref } place } write when IsEmptyPayload(place.Type):
                if (write.CompoundOp is not null) throw new IrUnsupportedException("compound assignment to an empty payload");
                rendered = ($"ZigOptionalZero<{Cs(place.Type)}>.Write((void*)({Expr(place.Operand)}), {Coerced(write.Value, place.Type)})", PPrimary);
                return true;
        }
        return false;
    }
}
