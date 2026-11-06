using DotCC.Ir;
using DotCC.Layout;

namespace DotCC.Backends;

internal sealed partial class CSharpTarget
{
    private string RenderErrorUnion(CType payload)
    {
        var type = payload.Unqualified;
        var rendered = type is CType.VoidType ? "Unit"
            : type is CType.Array array ? ArrayValueName(array)
            // C# generic arguments cannot be pointers. Keep the existing
            // address-bit payload and cast at the source pointer projection.
            : type is CType.Pointer ? "nuint" : RenderType(payload);
        var zero = type is CType.VoidType;
        if (!zero && type is CType.Named or CType.Array or CType.Tuple)
        {
            // Select the zero carrier only with a source-layout proof. A type
            // whose storage has no model retains its existing scalar rendering;
            // any actual storage use still requires the layout model's proof.
            try { zero = OptionalPayloadLayout?.Invoke(payload) is { Size: 0, Alignment: 1 }; }
            catch (OffsetLayoutException) { }
        }
        return (zero ? "ZigErrorUnionZero<" : "ErrUnion<") + rendered + ">";
    }
}
