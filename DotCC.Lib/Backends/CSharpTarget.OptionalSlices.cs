using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpTarget
{
    // Nullable<T> is the existing managed element storage for these optionals,
    // but C# excludes T? from an unmanaged generic argument. The view takes T
    // as its argument and retains a T?* over that same storage. Explicit aligned,
    // zero-sized, array and niche carriers continue to use their existing slices.
    internal CType? NullableSliceTarget(CType element)
        => element.Unqualified is CType.Optional optional
            && RenderType(element) == RenderType(optional.Inner) + "?"
                ? optional.Inner.Unqualified : null;
}
