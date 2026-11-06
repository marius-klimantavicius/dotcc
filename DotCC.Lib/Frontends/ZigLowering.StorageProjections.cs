#nullable enable
using System.Collections.Generic;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;
internal sealed partial class ZigLowering
{
    // A nullable Value is a copy in C#. Only address/store positions need the
    // original payload storage; ordinary reads retain their existing checks.
    private CExpr OptionalStorageProjection(CExpr expression) => expression switch
    {
        Member { Field: "Value", Base.Type.Unqualified: CType.Optional optional } member
            when member.Base.IsLValue => new Unary(UnOp.Deref,
                new Call("ZigMem.OptionalPayload", new List<CExpr> { AddressOfLValue(OptionalStorageProjection(member.Base)) },
                    new List<CType> { new CType.Pointer(member.Base.Type) })
                    { Type = CapturePointerType(member.Base, optional.Inner) })
                { Type = optional.Inner, IsLValue = true },
        Member member => member with { Base = OptionalStorageProjection(member.Base) },
        DotCC.Ir.Index indexed => indexed with { Base = OptionalStorageProjection(indexed.Base) },
        Paren grouped => grouped with { Inner = OptionalStorageProjection(grouped.Inner) },
        _ => expression,
    };

    private void CoerceSliceValuePeers(ref CExpr left, ref CExpr right)
    {
        var slice = left.Type.Unqualified as CType.Slice ?? right.Type.Unqualified as CType.Slice;
        if (slice is null) return;
        if (!IsSliceValuePeer(left, slice) || !IsSliceValuePeer(right, slice)) return;
        if (left.Type.Unqualified is CType.Slice ls && ls.Element.Unqualified != slice.Element.Unqualified
            || right.Type.Unqualified is CType.Slice rs && rs.Element.Unqualified != slice.Element.Unqualified) return;
        var readOnly = slice.Element.IsConst || SliceValuePeerIsReadOnly(left) || SliceValuePeerIsReadOnly(right);
        var peer = slice with { Element = readOnly ? slice.Element.WithQuals(TypeQual.Const) : slice.Element };
        left = left.Type.Unqualified is CType.Slice ? left.Type == peer ? left : new Cast(peer, left) { Type = peer } : CoerceToSlice(left, peer);
        right = right.Type.Unqualified is CType.Slice ? right.Type == peer ? right : new Cast(peer, right) { Type = peer } : CoerceToSlice(right, peer);
    }

    private CExpr LowerPointerSliceView(Item operand, CType.Slice target)
    {
        var impure = _hoistImpureSeen;
        var source = LowerExpr(operand);
        var element = target.Element.Unqualified;
        var pointerType = new CType.Pointer(target.Element);
        CExpr address, count;
        if (source.Type.Unqualified is CType.Slice slice)
        {
            if (slice.Element.IsConst && !target.Element.IsConst)
                throw new CompileException("zig @ptrCast cannot discard const slice elements");
            var (pointer, length) = SliceParts(source, slice.Element, "@ptrCast", impure);
            address = pointer;
            var bytes = new Binary(BinOp.Mul, length, new SizeOfExpr(slice.Element.Unqualified) { Type = CType.ULong }) { Type = CType.ULong };
            count = new Binary(BinOp.Div, bytes, new SizeOfExpr(element) { Type = CType.ULong }) { Type = CType.ULong };
        }
        else if (source.Type.Unqualified is CType.Pointer { Pointee: var pointee } && pointee.Unqualified is not CType.VoidType)
        {
            if (pointee.IsConst && !target.Element.IsConst)
                throw new CompileException("zig @ptrCast cannot discard const pointer elements");
            address = source;
            count = new Binary(BinOp.Div, new SizeOfExpr(pointee.Unqualified) { Type = CType.ULong },
                new SizeOfExpr(element) { Type = CType.ULong }) { Type = CType.ULong };
        }
        else throw new IrUnsupportedException($"zig @ptrCast to a slice needs source slice or bounded single-item storage, got {source.Type.Describe()}");
        return new SliceNew(new Cast(pointerType, address) { Type = pointerType }, count, element, target.Element.IsConst) { Type = target };
    }
}
