using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpTarget
{
    // A []**T view retains a T*** data pointer, but uses the final unmanaged
    // pointee as its generic argument. Pointer and array boundaries stay in
    // the original IR; this only selects the matching C# view representation.
    internal static CType? PtrPtrSliceTarget(CType element)
        => element.Unqualified is CType.Pointer { Pointee.Unqualified: CType.Pointer { Pointee: var target } }
            && target.Unqualified is not (CType.Pointer or CType.Array or CType.VoidType or CType.Func or CType.Opaque)
                ? target.Unqualified : null;
}
