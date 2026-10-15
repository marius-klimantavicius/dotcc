using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private static bool NeedsPlannedCompoundOperation(ExpressionOperation expression) =>
            expression is OperatorOperation { OperatorKind.Name: "eok_padd_assign" or "eok_psubtract_assign" or "eok_shiftl_assign" or "eok_shiftr_assign" };

        private void ValidatePlannedCompound(OperatorOperation operation)
        {
            if (!NeedsPlannedCompoundOperation(operation)) return;
            if (operation.Operands.Count != 2 || operation.Operator.PointerOperandIsSecond)
                throw Limit("A selected compound location requires its ordinary left-place/right-value operands.", "compound-assignment");
            var left = operation.Operands[0].Type;
            var right = operation.Operands[1].Type;
            var pointerCompound = operation.OperatorKind.Name is "eok_padd_assign" or "eok_psubtract_assign";
            if (!IsNaturalCompoundInteger(right, shiftCount: !pointerCompound))
                throw Limit("This compound offset/count lacks the exercised natural integer representation.", "compound-assignment");
            if (pointerCompound)
            {
                if (MemoryRepresentationType(left) is not PointerType pointer || pointer.IsReference ||
                    left.Size != 8 || left.Alignment != 8 || pointer.Size != 8 || pointer.Alignment != 8)
                    throw Limit("Pointer compound assignment requires an ordinary captured 8/8 data pointer.", "pointer-compound");
                var element = pointer.PointeeType;
                var rawElement = MemoryRepresentationType(element);
                if (rawElement is RoutineType || rawElement.Kind.Name == "tk_void" ||
                    element.Size != rawElement.Size || element.Alignment != rawElement.Alignment)
                    throw Limit("Pointer stride needs its immediate complete selected element storage.", "pointer-compound");
                ValidateValueAddress(element);
                _ = StorageSize(element);
                // C# scales by this immediate type. For T** this is the size
                // of T*, never the layout of T. Emitted records/array carriers
                // independently retain their captured physical storage size.
                _ = TypeName(element);
            }
            else
            {
                var raw = MemoryRepresentationType(left);
                if (IsUInt128Value(left)) ValidateUInt128Value(left);
                else if (raw is not ArithmeticType arithmetic || arithmetic.Arithmetic.ArithmeticKind.Name != "unsigned_integer" ||
                    arithmetic.Arithmetic.Signed != false || arithmetic.Arithmetic.Bits is not (32 or 64) || arithmetic.Arithmetic.IsBool || arithmetic.Arithmetic.IsSaturating ||
                    left.Size != arithmetic.Arithmetic.Bits / 8 || left.Alignment != left.Size || raw.Size != left.Size || raw.Alignment != left.Alignment)
                    throw Limit("Compound shifts require the exercised unsigned32/unsigned64 or selected unsigned128 value representation.", "shift");
            }
        }

        private static bool IsNaturalCompoundInteger(CppType type, bool shiftCount)
        {
            if (MemoryRepresentationType(type) is not ArithmeticType arithmetic || arithmetic.Arithmetic.IsBool || arithmetic.Arithmetic.IsSaturating)
                return false;
            var detail = arithmetic.Arithmetic;
            var selected = detail.ArithmeticKind.Name == "signed_integer" && detail.Signed == true && detail.Bits is 32 or 64 ||
                !shiftCount && detail.ArithmeticKind.Name == "unsigned_integer" && detail.Signed == false && detail.Bits is 8 or 16 or 32 or 64;
            return selected && type.Size == detail.Bits / 8 && type.Alignment == type.Size &&
                arithmetic.Size == type.Size && arithmetic.Alignment == type.Alignment;
        }

        // A CLR value local needs no native address to retain its identity.
        // This exception does not admit addresses, static allocations, fields,
        // aliased storage or pointers to the captured 16-byte UInt128 type.
        private bool IsStableUInt128LocalPlace(LValuePlan place)
        {
            if (place.Kind != LValueKind.Slot || place.VolatileTarget || place.Variable is not { } variable || currentFunction is null ||
                variable.IsThreadLocal || variable.LocalStaticInitializations.Count != 0 || variable.Details.StorageDuration?.Name is "static" or "thread" ||
                currentFunction.Body?.Locals.Contains(variable) != true || !OrdinaryLocalUpdateType(variable.Type) || !OrdinaryLocalUpdateType(place.Type) ||
                !MemoryRepresentationType(variable.Type).Equals(MemoryRepresentationType(place.Type))) return false;
            ValidateUInt128Value(variable.Type);
            ValidateUInt128Value(place.Type);
            return true;
        }
    }
}
