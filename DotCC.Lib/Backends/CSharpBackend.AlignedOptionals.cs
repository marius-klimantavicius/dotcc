using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private bool IsAlignedOptional(CType type) => type.Unqualified is CType.Optional optional
        && optional.Inner.Unqualified is not (CType.Pointer or CType.Optional or CType.VoidType)
        && (_offsetModel.Type(IrBuilder.LayoutType(optional.Inner)).Alignment > 8 || IsZeroOptional(type));

    private bool TryRenderAlignedOptional(CExpr expression, out (string, int) rendered)
    {
        rendered = default;
        if (expression is NullCoalesce coalesce && (IsAlignedOptional(coalesce.Left.Type)
            || coalesce.Left.Type.Unqualified is CType.Optional { Inner.Unqualified: CType.ErrorSetType }))
        {
            var name = "__optional" + _clCounter++;
            var value = Expr(coalesce.Left);
            _pending.Add(Cs(coalesce.Left.Type) + " " + name + " = " + value);
            var reference = new NameRef(name) { Type = coalesce.Left.Type };
            var condition = new Member(reference, "HasValue", false) { Type = CType.Bool };
            var payload = new Member(reference, "Value", false) { Type = coalesce.Type };
            rendered = (RenderLazyConditional(new CondExpr(condition, payload, coalesce.Right) { Type = coalesce.Type }), PPrimary);
            return true;
        }
        return false;
    }
}
