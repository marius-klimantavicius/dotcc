using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private static bool IsOrdinaryIntegerCompound(string kind) => kind is
            "eok_add_assign" or "eok_subtract_assign" or "eok_multiply_assign" or "eok_divide_assign" or
            "eok_remainder_assign" or "eok_and_assign" or "eok_or_assign" or "eok_xor_assign";

        private static bool TryNaturalCompoundInteger(CppType type, out ArithmeticType arithmetic)
        {
            arithmetic = MemoryRepresentationType(type) as ArithmeticType ?? null!;
            return arithmetic is not null && arithmetic.Arithmetic.ArithmeticKind.Name is "signed_integer" or "unsigned_integer" &&
                arithmetic.Arithmetic.Signed is not null && !arithmetic.Arithmetic.IsBool && !arithmetic.Arithmetic.IsSaturating &&
                arithmetic.Arithmetic.Bits is 8 or 16 or 32 or 64 && type.Size == arithmetic.Arithmetic.Bits / 8 &&
                type.Alignment == type.Size && arithmetic.Size == type.Size && arithmetic.Alignment == type.Alignment;
        }

        private static bool HasDifferentCompoundRepresentation(OperatorOperation operation) =>
            operation.Operands.Count == 2 && TryNaturalCompoundInteger(operation.Operands[0].Type, out var left) &&
            TryNaturalCompoundInteger(operation.Operands[1].Type, out var right) &&
            (left.Arithmetic.Bits != right.Arithmetic.Bits || left.Arithmetic.Signed != right.Arithmetic.Signed);

        private static bool NeedsPlannedCompoundConversion(ExpressionOperation expression)
        {
            if (expression is not OperatorOperation operation || !IsOrdinaryIntegerCompound(operation.OperatorKind.Name) ||
                !HasDifferentCompoundRepresentation(operation)) return false;
            var left = (ArithmeticType)MemoryRepresentationType(operation.Operands[0].Type);
            var right = (ArithmeticType)MemoryRepresentationType(operation.Operands[1].Type);
            // The established narrow/int32 helper already performs these exact
            // computation/store conversions for pure ordinary locations.
            if (left.Arithmetic.Bits is 8 or 16 && right.Arithmetic.Bits == 32 && right.Arithmetic.Signed == true &&
                !operation.Operands.Any(HasEffects) && !IsBitFieldPlace(operation.Operands[0])) return false;
            return true;
        }

        private bool TryPlanCompoundConversion(OperatorOperation operation, CsExpression oldValue, CsExpression rightValue,
            out CsExpression storedValue)
        {
            storedValue = null!;
            if (!IsOrdinaryIntegerCompound(operation.OperatorKind.Name) || !HasDifferentCompoundRepresentation(operation)) return false;
            var computation = operation.Operands[1].Type;
            var raw = (ArithmeticType)MemoryRepresentationType(computation);
            if (raw.Arithmetic.Bits is not (32 or 64) || operation.Operator.OperationTypeKind?.Name != "tk_integer")
                throw Limit("This compound lacks a retained promoted ordinary integer computation representation.", "compound-assignment");
            // Stock il.c compound_assignment_operation_type: ordinary compound
            // operand2 is already converted to the computation type. Shifts,
            // pointer arithmetic and fixed-point exceptions are excluded above.
            // OperationTypeKind is only a class check, never width/sign authority.
            var type = CsType.Value(computation);
            var symbol = AssignmentOperators[operation.OperatorKind.Name][..^1];
            var value = new CsBinary(type, symbol, new CsCast(type, oldValue), new CsCast(type, rightValue));
            storedValue = new CsCast(CsType.Value(operation.Operands[0].Type), value);
            return true;
        }

        private static bool NeedsPlannedAggregateConditional(ExpressionOperation expression)
        {
            if (expression is not OperatorOperation { OperatorKind.Name: "eok_assign", Operands.Count: 2 } assignment ||
                MemoryRepresentationType(assignment.Operands[0].Type) is not RecordType) return false;
            var source = assignment.Operands[1];
            while (source is ConversionOperation conversion) source = conversion.Operand;
            return source is OperatorOperation { OperatorKind.Name: "eok_question" };
        }
    }
}
