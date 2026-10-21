// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten interpret.c typed floating storage, conversion and operation
// dispatch. Numeric work is delegated to the explicit target-value module.
using System.Numerics;

namespace EdgSharp;

[Flags]
public enum FloatingEvaluationCondition
{
    None = 0, Inexact = 1, Tiny = 2, Underflow = 4, Overflow = 8,
    InvalidOperation = 16, DivideByZero = 32, IntegerOutOfRange = 64, ResourceLimit = 128
}

// Ephemeral input to one optional folding step. It never enters the returned
// source graph, and its payload is a numeric value rather than an object address.
internal sealed class NumericalFoldLeaf(SemanticType type, SourceRange range, ConstantRuntimeValue value)
    : Expression(type, ValueCategory.PRValue, range)
{
    internal ConstantRuntimeValue Numeric { get; } = value;
    public override bool HasSideEffects => false;
    public override bool IsValueDependent => false;
}

internal sealed partial class ConstantInterpreter
{
    private FloatingFlags floatingConditions;
    private FloatingLimits FloatingPolicy() => new(maxIntermediateBits: options.FloatingIntermediateBitLimit);

    private void CheckFloating(FloatingFlags flags)
    {
        floatingConditions |= flags;
        if ((flags & FloatingFlags.ResourceLimit) != 0)
            Stop(ConstantEvaluationStatus.ResourceLimit, "Floating arithmetic intermediate limit exceeded.");
        if ((flags & (FloatingFlags.Overflow | FloatingFlags.IntegerOutOfRange)) != 0 ||
            options.FloatingExceptions == FloatingExceptionPolicy.Strict && (flags & (FloatingFlags.InvalidOperation | FloatingFlags.DivideByZero)) != 0)
            Stop(ConstantEvaluationStatus.UndefinedBehavior, "Invalid or out-of-range floating operation in a constant expression.");
    }
    private ConstantRuntimeValue CheckedFloating(FloatingResult result)
    {
        CheckFloating(result.Flags);
        if (result.Value == null) Stop(ConstantEvaluationStatus.InvalidExpression, "Floating operation produced no value.");
        // The pinned arithmetic/conversion adapter diagnoses a finite nonzero
        // result lost entirely to zero, but accepts tiny inexact NONZERO results.
        // Literal conversion has its separately observed underflow policy.
        if ((result.Flags & FloatingFlags.Underflow) != 0 && result.Value!.IsZero)
            Stop(ConstantEvaluationStatus.UndefinedBehavior, "A floating arithmetic or conversion result underflows to zero.");
        return ConstantRuntimeValue.Float(result.Value!);
    }

    private static TargetFloatValue FloatingScalar(ConstantRuntimeValue value)
    {
        if (value.Floating == null) Stop(ConstantEvaluationStatus.InvalidExpression, "A floating value requires an explicit load or conversion.");
        return value.Floating!;
    }

    private ConstantRuntimeValue ConvertScalar(ConstantRuntimeValue value, SemanticType target)
    {
        if (target is FloatingType floating)
            return value.Floating is { } f ? CheckedFloating(FloatingValues.Convert(f, floating.Format, FloatingPolicy())) :
                CheckedFloating(FloatingValues.FromInteger(floating.Format, Scalar(value), FloatingPolicy()));
        if (target is not IntegerType integer)
        { Stop(ConstantEvaluationStatus.InvalidExpression, "Scalar conversion requires an arithmetic target."); return default; }
        if (value.Floating is { } source)
        {
            if (integer.IntegerKind == IntegerKind.Bool)
                return ConstantRuntimeValue.Scalar(FloatingValues.ToBoolean(source) ? BigInteger.One : BigInteger.Zero);
            FloatingIntegerResult converted = FloatingValues.ToInteger(source, integer.Width, integer.IsSigned, FloatingPolicy());
            CheckFloating(converted.Flags);
            if (!converted.Value.HasValue) Stop(ConstantEvaluationStatus.UndefinedBehavior, "A nonfinite or out-of-range floating value cannot convert to this integer type.");
            return ConstantRuntimeValue.Scalar(converted.Value!.Value);
        }
        return ConstantRuntimeValue.Scalar(TypeTable.ConvertInteger(Scalar(value), integer));
    }

    private ConstantRuntimeValue LoadScalar(ConstantRuntimeValue value)
    {
        ConstantCell cell = Location(value); EnsureAlive(cell);
        if (!cell.Known) Stop(ConstantEvaluationStatus.NotConstant, "The reference designates an unknown runtime object whose value is unavailable.");
        if (!cell.Initialized) Stop(ConstantEvaluationStatus.UninitializedRead, "Read of an uninitialized object.");
        if (!cell.Readable) Stop(ConstantEvaluationStatus.NotConstant, "Reading this existing mutable or runtime object is not allowed in a constant expression.");
        if (cell.Floating == null && !cell.Value.HasValue) Stop(ConstantEvaluationStatus.NotConstant, "The designated object's scalar value is unavailable.");
        NoteAccess(cell, write: false);
        return cell.Floating is { } floating ? ConstantRuntimeValue.Float(floating) : ConstantRuntimeValue.Scalar(cell.Value!.Value);
    }

    private static void StoreScalar(ConstantCell cell, ConstantRuntimeValue value)
    {
        if (cell.Type is FloatingType target)
        {
            TargetFloatValue floating = FloatingScalar(value);
            if (!ReferenceEquals(floating.Format, target.Format)) throw new InvalidOperationException("A floating store requires a converted target format.");
            cell.Floating = floating; cell.Value = null;
        }
        else { cell.Value = Scalar(value); cell.Floating = null; }
        cell.Initialized = true;
    }

    private ConstantRuntimeValue FloatingOperation(TokenKind op, TargetFloatValue left, TargetFloatValue right)
    {
        if (op is TokenKind.EqualEqual or TokenKind.NotEqual or TokenKind.Less or TokenKind.Greater or TokenKind.LessEqual or TokenKind.GreaterEqual)
        {
            FloatingOrder order = FloatingValues.Compare(left, right);
            bool result = op switch
            {
                TokenKind.EqualEqual => order == FloatingOrder.Equal,
                TokenKind.NotEqual => order != FloatingOrder.Equal,
                TokenKind.Less => order == FloatingOrder.Less,
                TokenKind.Greater => order == FloatingOrder.Greater,
                TokenKind.LessEqual => order is FloatingOrder.Less or FloatingOrder.Equal,
                _ => order is FloatingOrder.Greater or FloatingOrder.Equal
            };
            return ConstantRuntimeValue.Scalar(result ? BigInteger.One : BigInteger.Zero);
        }
        FloatingResult numeric = op switch
        {
            TokenKind.Plus => FloatingValues.Add(left, right, FloatingPolicy()),
            TokenKind.Minus => FloatingValues.Subtract(left, right, FloatingPolicy()),
            TokenKind.Star => FloatingValues.Multiply(left, right, FloatingPolicy()),
            TokenKind.Slash => FloatingValues.Divide(left, right, FloatingPolicy()),
            _ => throw new InvalidOperationException("Unexpected typed floating operator.")
        };
        return CheckedFloating(numeric);
    }
}
