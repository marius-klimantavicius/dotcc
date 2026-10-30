#nullable enable
namespace DotCC.Backends;

using DotCC.Ir;

internal sealed partial class CSharpBackend
{
    // Pointer-to-array values use a flat scalar pointer in C#. Its native C#
    // arithmetic advances one scalar, so restore the C row's scalar count.
    private static int PointerArrayStride(CType type)
    {
        var row = type.Unqualified switch
        {
            CType.Pointer { Pointee.Unqualified: CType.Array array } => array,
            CType.Array { Element.Unqualified: CType.Array array } => array,
            _ => null,
        };
        if (row is null) return 1;
        var count = FlatCount(row);
        if (count <= 0) throw new IrUnsupportedException("pointer arithmetic requires a fixed, nonempty array bound");
        return count;
    }

    private string PointerArrayLValue(CExpr target)
    {
        if (target.Type.IsAtomic || target.Type.IsVolatile)
            throw new IrUnsupportedException("atomic/volatile pointer-to-array updates are not supported");
        return StoredAsNint(target)
            ? $"*({Cs(target.Type)}*)({GlobalStorageAddress(target)})"
            : Sub(target, PUnary);
    }

    private static bool NeedsPointerUpdateLowering(CExpr target) =>
        PointerArrayStride(target.Type) != 1 ||
        (target.Type.Unqualified is CType.Pointer && StoredAsNint(target)
            && !target.Type.IsAtomic && !target.Type.IsVolatile);

    private (string, int) PointerSlotAssignment(Assign assignment, bool discard = false)
    {
        // Global pointer/callback slots use nint backing. Restore the logical
        // pointer type only when the C assignment's result is consumed.
        var slot = BareLValue(assignment.Target);
        var type = Cs(assignment.Target.Type.Unqualified);
        var value = assignment.CompoundOp is { } operation
            ? $"({type}){slot} {BinSym(operation)} {Sub(assignment.Value, Prec(operation) + 1)}"
            : Coerced(assignment.Value, assignment.Target.Type);
        var store = $"{slot} = (nint)({value})";
        return discard ? (store, PAssign) : ($"({type})({store})", PUnary);
    }

    private string? DiscardedPointerAssignment(CExpr expression)
    {
        if (expression is not Assign assignment || !StoredAsNint(assignment.Target)
            || assignment.Target.Type.IsAtomic || assignment.Target.Type.IsVolatile) return null;
        // Row-pointer arithmetic already emits a plain assignment while
        // applying the C row stride. Keep that specialized lowering intact.
        return assignment.CompoundOp is BinOp.Add or BinOp.Sub && PointerArrayStride(assignment.Target.Type) != 1
            ? Expr(assignment) : PointerSlotAssignment(assignment, discard: true).Item1;
    }

    private (string, int) PointerArrayUpdate(Unary value, bool discard = false)
    {
        var stride = PointerArrayStride(value.Operand.Type);
        var add = value.Op is UnOp.PreInc or UnOp.PostInc;
        var update = $"{PointerArrayLValue(value.Operand)} {(add ? "+" : "-")}= {stride}";
        return !discard && value.Op is UnOp.PostInc or UnOp.PostDec
            ? ($"({update}) {(add ? "-" : "+")} {stride}", PAdd)
            : (update, PAssign);
    }

    private (string, int) PointerArrayBinary(Binary binary)
    {
        var leftPointer = IsPointerType(binary.Left.Type);
        var rightPointer = IsPointerType(binary.Right.Type);
        if (leftPointer && rightPointer && binary.Op == BinOp.Add)
            throw new IrUnsupportedException("addition of two pointers is invalid");
        if (!leftPointer && rightPointer && binary.Op == BinOp.Sub)
            throw new IrUnsupportedException("subtraction of a pointer from an integer is invalid");
        if (leftPointer && rightPointer)
        {
            var stride = PointerArrayStride(binary.Left.Type);
            return ($"({Sub(binary.Left, PAdd)} - {Sub(binary.Right, PAdd + 1)}) / {stride}", PMul);
        }
        var pointer = leftPointer ? binary.Left : binary.Right;
        var integer = leftPointer ? binary.Right : binary.Left;
        var amount = $"{Sub(integer, PMul)} * {PointerArrayStride(pointer.Type)}";
        return leftPointer
            ? ($"{Sub(pointer, PAdd)} {BinSym(binary.Op)} {amount}", PAdd)
            : ($"{amount} + {Sub(pointer, PAdd + 1)}", PAdd);
    }

    private string ForPost(CExpr value)
    {
        while (value is Paren parenthesized) value = parenthesized.Inner;
        if (DiscardedPointerAssignment(CFlowFacts.StatementValue(value)) is { } pointerAssignment)
            return pointerAssignment;
        if (RequiresIntrinsicDiscard(value))
            return IntrinsicForPart(value);
        if (value is CommaOp comma)
            return string.Join(", ", comma.Items.Select(ForPost));
        if (value is CommaSeq sequence)
            return string.Join(", ", sequence.Items.Select(ForPost));
        if (!IsStmtExpr(value))
            return $"_ = {Expr(value)}";
        if (value is Assign { Target.Type.IsVolatile: true, Target.Type.IsPointerLowered: true }
            or Unary { Op: UnOp.PreInc or UnOp.PreDec or UnOp.PostInc or UnOp.PostDec,
                Operand.Type.IsVolatile: true, Operand.Type.IsPointerLowered: true })
            return $"_ = {Expr(value)}";
        return value is Unary { Op: UnOp.PreInc or UnOp.PreDec or UnOp.PostInc or UnOp.PostDec } unary
            && NeedsPointerUpdateLowering(unary.Operand)
            ? PointerArrayUpdate(unary, discard: true).Item1
            : Expr(value);
    }

    private string IntrinsicForPart(CExpr value)
    {
        while (value is Paren parenthesized) value = parenthesized.Inner;
        // Once a comma list needs intrinsic-result discards, every operand is
        // still an ordinary C discarded expression, including pure literals and
        // nested comma groups. C# for clauses separate these with commas.
        var items = value switch { CommaOp comma => comma.Items, CommaSeq sequence => sequence.Items, _ => null };
        return items is not null
            ? string.Join(", ", items.Where(item => !IsPure(item)).Select(IntrinsicForPart).Where(text => text.Length != 0))
            : value is Call call && SelectedIntrinsic(call) is { IsStatementExpression: false }
                ? RenderStmtExpr(value) : ForPost(value);
    }
}
