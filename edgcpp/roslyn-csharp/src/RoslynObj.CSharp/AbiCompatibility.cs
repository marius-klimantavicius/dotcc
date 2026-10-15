namespace RoslynObj.CSharp;

public enum AbiCompatibilityStatus { Compatible, Incompatible, Unsupported }
public sealed record AbiCompatibilityResult(AbiCompatibilityStatus Status, string Path, string Reason)
{
    public bool Compatible => Status == AbiCompatibilityStatus.Compatible;
}

/// <summary>Checks the chosen shared native representation, never C++ type equivalence, ODR or overload rules.</summary>
public sealed class AbiCompatibility
{
    public AbiCompatibilityResult CompareFunctions(FunctionSymbol left, FunctionSymbol right)
    {
        ArgumentNullException.ThrowIfNull(left); ArgumentNullException.ThrowIfNull(right);
        var state = new Comparison();
        state.Routine(left.Signature, right.Signature, "signature");
        state.Parameters(left.Parameters, right.Parameters, "selected-parameters");
        return state.Result;
    }
    public AbiCompatibilityResult CompareStorage(VariableSymbol left, VariableSymbol right)
    {
        ArgumentNullException.ThrowIfNull(left); ArgumentNullException.ThrowIfNull(right);
        var state = new Comparison();
        // A selected object allocation must retain explicit typedef/type alignment as well as
        // the underlying value representation. Parameter-only source qualifiers do not apply here.
        if (left.Type.Size is { } leftSize && right.Type.Size is { } rightSize)
            state.Equal(leftSize, rightSize, "storage.size");
        if (left.Type.Alignment is { } leftAlignment && right.Type.Alignment is { } rightAlignment)
            state.Equal(leftAlignment, rightAlignment, "storage.alignment");
        state.Type(left.Type, right.Type, "storage");
        state.Equal(left.IsThreadLocal, right.IsThreadLocal, "storage.thread-local");
        return state.Result;
    }
    public AbiCompatibilityResult CompareTypes(CppType left, CppType right)
    {
        var state = new Comparison(); state.Type(left, right, "type"); return state.Result;
    }

    private sealed class Comparison
    {
        private readonly HashSet<(CppType, CppType)> pairs = [];
        internal AbiCompatibilityResult Result { get; private set; } = new(AbiCompatibilityStatus.Compatible, "", "Captured representations match the supported ABI subset.");
        private bool Active => Result.Compatible;
        internal void Equal<T>(T left, T right, string path)
        {
            if (Active && !EqualityComparer<T>.Default.Equals(left, right))
                Result = new(AbiCompatibilityStatus.Incompatible, path, "Captured representation facts differ.");
        }
        private void Unsupported(string path, string reason)
        { if (Active) Result = new(AbiCompatibilityStatus.Unsupported, path, reason); }
        private static string? Kind(SemanticKind? kind) => kind is null ? null : kind.Family + ":" + kind.Name;

        internal void Parameters<T>(IReadOnlyList<T> left, IReadOnlyList<T> right, string path) where T : SignatureParameter
        {
            Equal(left.Count, right.Count, path + ".count");
            for (var index = 0; Active && index < Math.Min(left.Count, right.Count); index++)
            {
                var l = left[index]; var r = right[index]; var location = path + $"[{index}]";
                Equal(l.Ordinal, r.Ordinal, location + ".ordinal");
                Equal(Kind(l.Role), Kind(r.Role), location + ".role");
                var ld = l.Descriptor.Details; var rd = r.Descriptor.Details;
                Equal(ld.PassedViaCopyConstructor, rd.PassedViaCopyConstructor, location + ".copy-constructor");
                Equal(ld.NamedAddressSpace, rd.NamedAddressSpace, location + ".address-space");
                Equal(ld.IsUnaligned, rd.IsUnaligned, location + ".unaligned");
                Equal(ld.IsNear, rd.IsNear, location + ".near"); Equal(ld.IsFar, rd.IsFar, location + ".far");
                Equal(ld.IsUpcShared, rd.IsUpcShared, location + ".shared");
                Equal(ld.IsUpcStrict, rd.IsUpcStrict, location + ".upc-strict");
                Equal(ld.IsUpcRelaxed, rd.IsUpcRelaxed, location + ".upc-relaxed");
                Type(l.Type, r.Type, location + ".type");
            }
        }
        internal void Routine(RoutineType left, RoutineType right, string path)
        {
            Equal(Kind(left.CallingConvention), Kind(right.CallingConvention), path + ".calling-convention");
            Equal(left.IsVariadic, right.IsVariadic, path + ".variadic");
            Equal(left.HasPrototype, right.HasPrototype, path + ".prototype");
            Equal(left.HasThisParameter, right.HasThisParameter, path + ".has-this");
            Equal(left.ReturnsValueAsParameter, right.ReturnsValueAsParameter, path + ".hidden-result");
            Equal(left.ReturnValueParameterFollowsThis, right.ReturnValueParameterFollowsThis, path + ".hidden-result-order");
            Equal(left.ReturnsValueViaCopyConstructor, right.ReturnsValueViaCopyConstructor, path + ".result-copy-constructor");
            Equal(left.Routine.AddressSpace, right.Routine.AddressSpace, path + ".address-space");
            Equal(left.Routine.ThisAddressSpace, right.Routine.ThisAddressSpace, path + ".this-address-space");
            // Receiver storage is described by selected hidden parameters. ThisClass and source
            // cv/ref qualifiers do not require structural equivalence of the owning classes.
            Type(left.ReturnType, right.ReturnType, path + ".return");
            Parameters(left.Parameters, right.Parameters, path + ".parameters");
        }
        internal void Type(CppType left, CppType right, string path)
        {
            if (!Active || left == right || !pairs.Add((left, right))) return;
            if (pairs.Count > 100_000) { Unsupported(path, "Representation comparison exceeds its explicit 100000 type-pair bound."); return; }
            if (left.IsDependent || right.IsDependent) { Unsupported(path, "Dependent representations are outside executable ABI comparison."); return; }
            if (left is QualifiedType lqualification && ExoticQualification(lqualification)) { Unsupported(path, "Address-space/UPC qualified representation has no selected adapter."); return; }
            if (right is QualifiedType rqualification && ExoticQualification(rqualification)) { Unsupported(path, "Address-space/UPC qualified representation has no selected adapter."); return; }
            if (left is TypeReference lr) { Type(lr.UnderlyingType, right, path); return; }
            if (right is TypeReference rr) { Type(left, rr.UnderlyingType, path); return; }
            if (left.Kind.Name is not ("tk_void" or "tk_routine") && (left.Size is null || left.Alignment is null)
                || right.Kind.Name is not ("tk_void" or "tk_routine") && (right.Size is null || right.Alignment is null))
            { Unsupported(path, "A by-value representation has unavailable size or alignment; no adapter is selected."); return; }
            Equal(left.Size, right.Size, path + ".size"); Equal(left.Alignment, right.Alignment, path + ".alignment");
            if (left is ArithmeticType la && right is ArithmeticType ra)
            {
                var l = la.Arithmetic; var r = ra.Arithmetic;
                Equal(Kind(l.ArithmeticKind), Kind(r.ArithmeticKind), path + ".arithmetic-kind");
                Equal(l.Bits, r.Bits, path + ".bits"); Equal(l.Signed, r.Signed, path + ".signed");
                Equal(l.Precision, r.Precision, path + ".precision"); Equal(l.Radix, r.Radix, path + ".radix"); Equal(l.Scale, r.Scale, path + ".scale");
                Equal(l.IsBool, r.IsBool, path + ".bool"); Equal(l.IsSaturating, r.IsSaturating, path + ".saturating"); return;
            }
            if (left is PointerType lp && right is PointerType rp)
            {
                var l = lp.Pointer; var r = rp.Pointer;
                Equal(l.IsReference, r.IsReference, path + ".reference"); Equal(l.IsRvalueReference, r.IsRvalueReference, path + ".rvalue-reference");
                Equal(l.IsPtr32, r.IsPtr32, path + ".ptr32"); Equal(l.IsPtr64, r.IsPtr64, path + ".ptr64");
                if (l.IsHandle || r.IsHandle || l.IsTrackingReference || r.IsTrackingReference || l.IsInterior || r.IsInterior || l.IsPinned || r.IsPinned
                    || l.IsSptr || r.IsSptr || l.IsUptr || r.IsUptr || l.BasedVariable is not null || r.BasedVariable is not null)
                    Unsupported(path, "Managed, based or address-space pointer representation has no selected adapter.");
                // Data pointers share the captured native address representation. Their pointee
                // declarations may be opaque, complete or differently named in independent TUs.
                // Field accesses keep each TU's own recorded layout; this is not an ODR check.
                static CppType UnwrapPointee(CppType type)
                {
                    var seen = new HashSet<CppType>();
                    while (type is TypeReference reference && seen.Add(type)) type = reference.UnderlyingType;
                    return type;
                }
                var lcallee = UnwrapPointee(lp.PointeeType) as RoutineType;
                var rcallee = UnwrapPointee(rp.PointeeType) as RoutineType;
                if (lcallee is not null && rcallee is not null) Routine(lcallee, rcallee, path + ".function-address");
                else if ((lcallee is null) != (rcallee is null))
                    Unsupported(path, "Function and data addresses need distinct selected calling representations.");
                return;
            }
            if (left is ArrayType lx && right is ArrayType rx)
            {
                Equal(Kind(lx.Array.BoundKind), Kind(rx.Array.BoundKind), path + ".bound-kind");
                Equal(lx.Bound, rx.Bound, path + ".bound");
                if (lx.Array.IsVla || rx.Array.IsVla || lx.Array.IsThreadsDimension || rx.Array.IsThreadsDimension)
                    Unsupported(path, "VLA/UPC array ABI comparison requires a selected runtime representation.");
                Type(lx.ElementType, rx.ElementType, path + ".element"); return;
            }
            if (left is RoutineType lf && right is RoutineType rf) { Routine(lf, rf, path); return; }
            if (left is RecordType ls && right is RecordType rs)
            {
                if (ls.IsIncomplete || rs.IsIncomplete || ls.Layout is null || rs.Layout is null)
                { Unsupported(path, "A shared record representation requires captured complete layouts; opaque adapters are not selected."); return; }
                if (ls.Bases.Count != 0 || rs.Bases.Count != 0 || ls.HasVirtualFunctions || rs.HasVirtualFunctions)
                { Unsupported(path, "C++ base/dispatch representation requires an explicit ABI adapter."); return; }
                Equal(ls.Kind.Name, rs.Kind.Name, path + ".record-kind");
                var lfields = ls.Layout.Fields; var rfields = rs.Layout.Fields;
                Equal(lfields.Count, rfields.Count, path + ".fields.count");
                for (var index = 0; Active && index < Math.Min(lfields.Count, rfields.Count); index++)
                {
                    var l = lfields[index]; var r = rfields[index]; var location = path + $".field[{index}]";
                    Equal(l.Offset, r.Offset, location + ".offset"); Equal(l.BitOffset, r.BitOffset, location + ".bit-offset");
                    Equal(l.BitWidth, r.BitWidth, location + ".bit-width"); Equal(l.ContainerOffset, r.ContainerOffset, location + ".container-offset");
                    Type(l.Field.Type, r.Field.Type, location + ".type");
                }
                return;
            }
            if (left.Kind.Name == "tk_void" && right.Kind.Name == "tk_void") return;
            if (left.Kind.Name != right.Kind.Name) Equal(left.Kind.Name, right.Kind.Name, path + ".kind");
            else Unsupported(path, "No shared native representation comparison exists for " + left.Kind.Name + ".");
        }
        private static bool ExoticQualification(QualifiedType type)
        {
            var qualifier = type.Qualification;
            return qualifier.AddressSpace is not (null or 0) || qualifier.IsShared || qualifier.IsUpcStrict
                || qualifier.IsUpcRelaxed || qualifier.IsNear || qualifier.IsFar;
        }
    }
}
