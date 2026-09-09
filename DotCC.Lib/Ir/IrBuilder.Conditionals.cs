namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private static CType ConditionalDecay(CType type) => type.Unqualified is CType.Array array
        ? new CType.Pointer(array.Element) : type.Unqualified;

    private bool IsNullPointerConstant(CExpr expression)
    {
        expression = Unparen(expression);
        if (expression is NullPtr) return true;
        // C permits an integer constant expression equal to zero, including
        // that expression cast to void*. A general void* value is not a null
        // constant and cannot be combined with a function pointer.
        if (expression is Cast { Target.Unqualified: CType.Pointer { Pointee.Unqualified: CType.VoidType } } cast)
            expression = cast.Operand;
        return expression.Type.Unqualified.IsInteger && ConstEval(expression) == 0;
    }

    private CType ConditionalPointerType(CExpr left, CExpr right, out bool leftNull, out bool rightNull)
    {
        var leftType = ConditionalDecay(left.Type);
        var rightType = ConditionalDecay(right.Type);
        leftNull = IsNullPointerConstant(left);
        rightNull = IsNullPointerConstant(right);
        if (leftNull && rightType is CType.Pointer or CType.Func) return rightType;
        if (rightNull && leftType is CType.Pointer or CType.Func) return leftType;

        if (leftType is CType.Pointer first && rightType is CType.Pointer second)
        {
            var firstTarget = first.Pointee.Unqualified;
            var secondTarget = second.Pointee.Unqualified;
            var qualifiers = first.Pointee.Quals | second.Pointee.Quals;
            if (firstTarget is CType.VoidType || secondTarget is CType.VoidType)
                return new CType.Pointer(CType.Void.WithQuals(qualifiers));
            if (CompatibleGlobalTypes(firstTarget, secondTarget))
            {
                // A compatible pointer-to-array may complete the other operand's
                // unknown bound; keep that information for later sizeof/stride.
                var target = firstTarget is CType.Array { Count: null } && secondTarget is CType.Array { Count: not null }
                    ? secondTarget : firstTarget;
                return new CType.Pointer(target.WithQuals(qualifiers));
            }
        }
        else if (leftType is CType.Func firstFunction && rightType is CType.Func secondFunction
            && CompatibleFunctionTypes(firstFunction, secondFunction))
            return firstFunction;

        throw new IrUnsupportedException("incompatible conditional pointer operands: "
            + left.Type.Describe() + " and " + right.Type.Describe());
    }
}
