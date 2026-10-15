using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        // A qualified void pointee has no object value that can be fetched.
        // Preserve its ordinary address representation without admitting any
        // volatile object access or losing the original intrinsic type facts.
        private static bool TryVoidPointerType(PointerType pointer, out string representation)
        {
            representation = "";
            var raw = pointer.PointeeType;
            while (raw is TypeReference reference) raw = reference.UnderlyingType;
            if (raw.Kind.Name != "tk_void") return false;
            ValidateProgramPointer(pointer);
            for (var current = pointer.PointeeType; ;)
            {
                if (current.Details.IsAtomic)
                    throw Limit("An atomic void pointee is outside the ordinary address contract.", "pointer");
                if (current is QualifiedType qualified)
                {
                    var qualifiers = qualified.Qualification;
                    if (qualifiers.IsAtomic || qualifiers.AddressSpace is not (null or 0) || qualifiers.IsShared
                        || qualifiers.IsUpcStrict || qualifiers.IsUpcRelaxed || qualifiers.IsNear || qualifiers.IsFar || qualifiers.IsUnaligned)
                        throw Limit("This qualified void address requires a separate pointer representation adapter.", "pointer");
                }
                if (current is not TypeReference wrapper) break;
                current = wrapper.UnderlyingType;
            }
            representation = "void*";
            return true;
        }
    }
}
