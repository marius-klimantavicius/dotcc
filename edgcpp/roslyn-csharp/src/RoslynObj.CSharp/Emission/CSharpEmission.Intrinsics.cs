using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private const string Intrinsics = "global::RoslynObj.CSharp.Runtime.TargetIntrinsics";
        private static bool IsIntrinsicRoutine(FunctionSymbol function) => function.BuiltinKind is not null;

        // Admission only: no rendering, registration, current-function state or
        // canonicalization. Callers still plan the actual operand occurrences.
        private bool IsSupportedConstantHintExpectation(CallOperation call)
        {
            if (call.CallKind.Name != "direct" || call.Callee is not ReferenceOperation { Target: FunctionSymbol function } callee ||
                function.BuiltinKind?.Name != "bfk_expect" || call.TargetMethod is not { } target || !target.Equals(function) ||
                call.Receiver is not null || call.Call.ReturnSlot is not null || call.Operator.RetainedVirtualCall ||
                call.Operator.EvalLeftToRight && call.Operator.EvalRightToLeft || call.Details.VolatileFetch ||
                call.RepresentationRole.Name != "execution" || call.ValueCategory.Name != "prvalue" ||
                callee.Details.VolatileFetch || callee.Operands.Count != 0 || callee.RepresentationRole.Name != "execution")
                return false;
            var signature = function.Signature;
            if (function.IsDefined || function.Body is not null || function.BodyState.Name != "declaration_only" ||
                !signature.HasPrototype || signature.IsVariadic || signature.HasThisParameter || signature.ThisClass is not null ||
                signature.ReturnsValueAsParameter is not false || signature.ReturnsValueViaCopyConstructor ||
                signature.ReturnValueParameterFollowsThis is not false || signature.CallingConvention?.Name != "cc_default" ||
                call.Call.FunctionType is not { } selectedType || !selectedType.Equals(signature) ||
                !HasExpectationSigned64Storage(signature.ReturnType) || !HasExpectationSigned64Storage(call.Type))
                return false;
            var parameters = signature.Parameters;
            var arguments = call.Arguments;
            var operands = call.Operands;
            if (parameters.Count != 2 || arguments.Count != 2 || operands.Count != 3 || !operands[0].Equals(callee)) return false;
            for (var index = 0; index < 2; ++index)
            {
                var parameter = parameters[index];
                var argument = arguments[index];
                if (parameter.Ordinal != index || parameter.Role.Name != "ordinary_parameter" ||
                    parameter.BaseSubobject is not null || parameter.Descriptor.Details.PassedViaCopyConstructor ||
                    !HasExpectationSigned64Storage(parameter.Type) ||
                    parameter.DeclaredType is { } declared && !HasExpectationSigned64Storage(declared) ||
                    argument.Ordinal != index || argument.Role?.Name is not ("argument" or "ordinary_parameter") ||
                    argument.Parameter is not { } descriptor || !descriptor.Equals(parameter.Descriptor) ||
                    !argument.Value.Equals(operands[index + 1]) || argument.Value.RepresentationRole.Name != "execution" ||
                    argument.Value.ValueCategory.Name != "prvalue" || !HasExpectationSigned64Storage(argument.Value.Type))
                    return false;
            }
            return arguments[1].Value is ConstantOperation hint && !hint.Details.VolatileFetch &&
                hint.Operands.Count == 0 && hint.VlaDimensions.Count == 0 &&
                hint.Constant.Value is IntegerValue { Kind.Name: "ck_integer" } &&
                hint.Constant.Type is { } constantType && HasExpectationSigned64Storage(constantType);
        }

        private static bool HasExpectationSigned64Storage(CppType type)
        {
            var seen = new HashSet<CppType>();
            for (var current = type; seen.Count < 16 && seen.Add(current);)
            {
                if (current.Size != 8 || current.Alignment != 8 || current.Details.IsAtomic || current.Details.SizeKind.Name == "upc_threads") return false;
                if (current is QualifiedType qualified)
                {
                    var qualifiers = qualified.Qualification;
                    if (qualifiers.IsVolatile || qualifiers.IsAtomic || qualifiers.AddressSpace is not (null or 0) ||
                        qualifiers.IsShared || qualifiers.IsUpcStrict || qualifiers.IsUpcRelaxed ||
                        qualifiers.IsNear || qualifiers.IsFar || qualifiers.IsUnaligned) return false;
                }
                if (current is TypeReference reference) { current = reference.UnderlyingType; continue; }
                return current is ArithmeticType arithmetic && arithmetic.Arithmetic.Bits == 64 &&
                    arithmetic.Arithmetic.Signed is true && !arithmetic.Arithmetic.IsBool && arithmetic.Arithmetic.ArithmeticKind.Name == "signed_integer";
            }
            return false;
        }

        private bool TryIntrinsicCall(CallOperation call, IReadOnlyList<string> arguments, out string result)
        {
            result = "";
            if (call.Callee is not ReferenceOperation { Target: FunctionSymbol operand })
            {
                if (call.TargetMethod is { } selected && IsIntrinsicRoutine(selected))
                    throw Limit("A selected compiler intrinsic needs its actual direct routine operand.", "intrinsic");
                return false;
            }
            var function = CanonicalFunction(operand);
            if (!IsIntrinsicRoutine(function)) return false;
            var builtin = function.BuiltinKind!.Name;
            if (call.TargetMethod is { } target && !CanonicalFunction(target).Equals(function))
                throw Limit("Compiler intrinsic target and actual routine operand differ.", "intrinsic");
            if (call.Receiver is not null || call.Call.ReturnSlot is not null || arguments.Count != call.Arguments.Count)
                throw Limit("Compiler intrinsic calls require their ordinary selected argument occurrences.", "intrinsic");
            if (call.Operands.Count != call.Arguments.Count + 1 || !call.Operands[0].Equals(call.Callee) ||
                call.Arguments.Where((argument, index) => !call.Operands[index + 1].Equals(argument.Value)).Any())
                throw Limit("Compiler intrinsic associations differ from the recorded operand occurrences.", "intrinsic");
            if (call.Arguments.Any(argument => argument.Role?.Name is not (null or "argument" or "ordinary_parameter")))
                throw Limit("Compiler intrinsic hidden argument roles have no selected lowering.", "intrinsic");
            var signature = function.Signature;
            ValidateCallingConvention(signature);
            ValidateOrdinaryRoutineRoles(signature);
            if (!signature.HasPrototype || function.Body is not null)
                throw Limit("Compiler intrinsic lowering requires the retained prototype-only builtin.", "intrinsic");
            if (signature.ReturnsValueAsParameter is not false)
                throw Limit("Compiler intrinsic lowering requires the captured ordinary result role.", "intrinsic");

            if (builtin == "bfk_expect")
            {
                if (!IsSupportedConstantHintExpectation(call) || arguments.Count != 2)
                    throw Limit("The recorded expectation intrinsic requires natural signed64 values and an effect-free integer constant hint.", "intrinsic-expect");
                // Both operands reached this method through the existing call
                // evaluation path. The literal hint has no effects; project the
                // first already-planned value exactly once without a helper.
                result = $"({arguments[0]})";
                return true;
            }

            void Fixed(int count)
            {
                if (signature.IsVariadic || signature.Parameters.Count != count || arguments.Count != count)
                    throw Limit("The recorded compiler intrinsic signature/actual arity is unsupported.", "intrinsic");
            }
            CppType Actual(int index) => call.Arguments[index].Value.Type;
            var atomicWidth = builtin switch
            {
                "bfk_atomic_load_1" or "bfk_atomic_store_1" => 1,
                "bfk_atomic_load_2" or "bfk_atomic_store_2" => 2,
                "bfk_atomic_load_4" or "bfk_atomic_store_4" => 4,
                "bfk_atomic_store_8" => 8,
                _ => 0
            };
            if (atomicWidth != 0)
            {
                var store = builtin.StartsWith("bfk_atomic_store_", StringComparison.Ordinal);
                Fixed(store ? 3 : 2);
                ValidateIntrinsicAtomicPointer(signature.Parameters[0].Type, store);
                ValidateIntrinsicAtomicPointer(Actual(0), store);
                var orderIndex = store ? 2 : 1;
                ValidateIntrinsicInteger(signature.Parameters[orderIndex].Type, 32, true);
                ValidateIntrinsicInteger(Actual(orderIndex), 32, true);
                if (call.Arguments[orderIndex].Value is not ConstantOperation { Constant.Value: IntegerValue { Value.IsZero: true } })
                    throw Limit("Only the actual exercised captured constant relaxed atomic order is admitted.", "intrinsic-order");
                if (store)
                {
                    ValidateIntrinsicVoid(signature.ReturnType); ValidateIntrinsicVoid(call.Type);
                    ValidateIntrinsicInteger(signature.Parameters[1].Type, atomicWidth * 8, false);
                    ValidateIntrinsicInteger(Actual(1), atomicWidth * 8, false);
                    result = $"{Intrinsics}.AtomicStore{atomicWidth}((void*)({arguments[0]}), unchecked(({TypeName(Actual(1))})({arguments[1]})), {arguments[2]})";
                }
                else
                {
                    ValidateIntrinsicInteger(signature.ReturnType, atomicWidth * 8, false);
                    ValidateIntrinsicInteger(call.Type, atomicWidth * 8, false);
                    result = $"{Intrinsics}.AtomicLoad{atomicWidth}((void*)({arguments[0]}), {arguments[1]})";
                }
                return true;
            }
            switch (builtin)
            {
                case "bfk_sync_synchronize":
                    Fixed(0); ValidateIntrinsicVoid(signature.ReturnType); ValidateIntrinsicVoid(call.Type);
                    result = Intrinsics + ".Synchronize()"; return true;
                case "bfk_bswap16": case "bfk_bswap32": case "bfk_bswap64":
                {
                    Fixed(1);
                    var bits = builtin == "bfk_bswap16" ? 16 : builtin == "bfk_bswap32" ? 32 : 64;
                    ValidateIntrinsicInteger(signature.ReturnType, bits, false); ValidateIntrinsicInteger(call.Type, bits, false);
                    ValidateIntrinsicInteger(signature.Parameters[0].Type, bits, false); ValidateIntrinsicInteger(Actual(0), bits, false);
                    result = $"{Intrinsics}.ByteSwap{bits}({arguments[0]})"; return true;
                }
                case "bfk_clzll":
                    Fixed(1);
                    ValidateIntrinsicInteger(signature.ReturnType, 32, true); ValidateIntrinsicInteger(call.Type, 32, true);
                    ValidateIntrinsicInteger(signature.Parameters[0].Type, 64, false); ValidateIntrinsicInteger(Actual(0), 64, false);
                    result = $"{Intrinsics}.CountLeadingZeros64({arguments[0]})"; return true;
                case "bfk_add_overflow": case "bfk_sub_overflow": case "bfk_mul_overflow":
                {
                    // The generic builtin prototype has no fixed parameters.
                    // Admission comes from actual captured types, not varargs ABI rules.
                    if (!signature.IsVariadic || signature.Parameters.Count != 0 || arguments.Count != 3)
                        throw Limit("The selected overflow builtin prototype/arity is unsupported.", "intrinsic");
                    ValidateIntrinsicInteger(signature.ReturnType, 8, false, boolean: true);
                    ValidateIntrinsicInteger(call.Type, 8, false, boolean: true);
                    ValidateIntrinsicInteger(Actual(0), 64, true); ValidateIntrinsicInteger(Actual(1), 64, true);
                    var pointer = ValidateIntrinsicPointer(Actual(2));
                    ValidateIntrinsicInteger(pointer.PointeeType, 64, true, writable: true);
                    var method = builtin == "bfk_add_overflow" ? "AddOverflow64" : builtin == "bfk_sub_overflow" ? "SubtractOverflow64" : "MultiplyOverflow64";
                    result = BoolValue($"{Intrinsics}.{method}({arguments[0]}, {arguments[1]}, (long*)({arguments[2]}))", call.Type);
                    return true;
                }
                default:
                    throw Limit($"Recorded compiler builtin '{builtin}' has no exercised lowering.", "intrinsic");
            }
        }

        private static void ValidateIntrinsicQualifiers(CppType type, bool allowVolatile = false, bool writable = false)
        {
            var seen = new HashSet<CppType>();
            for (var current = type; seen.Add(current);)
            {
                if (current.Details.IsAtomic || current.Details.SizeKind.Name == "upc_threads")
                    throw Limit("Atomic-qualified/UPC storage needs a separately exercised intrinsic representation.", "intrinsic");
                if (current is QualifiedType qualified)
                {
                    var qualifiers = qualified.Qualification;
                    if ((!allowVolatile && qualifiers.IsVolatile) || (writable && qualifiers.IsConst) || qualifiers.IsAtomic ||
                        qualifiers.AddressSpace is not (null or 0) || qualifiers.IsShared || qualifiers.IsUpcStrict || qualifiers.IsUpcRelaxed ||
                        qualifiers.IsNear || qualifiers.IsFar || qualifiers.IsUnaligned)
                        throw Limit("Compiler intrinsic qualifiers/storage decoration are unsupported.", "intrinsic");
                }
                if (current is not TypeReference reference) return;
                current = reference.UnderlyingType;
            }
            throw Limit("A cyclic intrinsic type has no selected representation.", "intrinsic");
        }
        private void ValidateIntrinsicInteger(CppType type, int bits, bool signed, bool boolean = false, bool writable = false)
        {
            ValidateIntrinsicQualifiers(type, writable: writable);
            if (Unwrap(type) is not ArithmeticType arithmetic || arithmetic.Arithmetic.Bits != bits ||
                arithmetic.Arithmetic.Signed != signed || arithmetic.Arithmetic.IsBool != boolean ||
                arithmetic.Arithmetic.ArithmeticKind.Name != (boolean ? "boolean" : signed ? "signed_integer" : "unsigned_integer") ||
                type.Size != bits / 8 || type.Alignment != bits / 8 || arithmetic.Size != bits / 8 || arithmetic.Alignment != bits / 8)
                throw Limit("Compiler intrinsic scalar width/sign/effective alignment is outside the exercised representation.", "intrinsic");
            _ = TypeName(type);
        }
        private static PointerType ValidateIntrinsicPointer(CppType type)
        {
            ValidateIntrinsicQualifiers(type);
            if (Unwrap(type) is not PointerType { IsReference: false } pointer || type.Size != 8 || type.Alignment != 8 || pointer.Size != 8 || pointer.Alignment != 8)
                throw Limit("Compiler intrinsic pointer requires the captured natural 8-byte representation.", "intrinsic");
            return pointer;
        }
        private static void ValidateIntrinsicAtomicPointer(CppType type, bool store)
        {
            var pointer = ValidateIntrinsicPointer(type);
            ValidateIntrinsicQualifiers(pointer.PointeeType, allowVolatile: true, writable: store);
            // Qualification was checked above. The general emitter Unwrap
            // deliberately rejects volatile object execution, which does not
            // describe this admitted opaque atomic address view.
            var pointee = pointer.PointeeType;
            while (pointee is TypeReference reference) pointee = reference.UnderlyingType;
            if (pointee.Kind.Name != "tk_void")
                throw Limit("The exercised atomic builtin pointer view is the captured qualified void pointer.", "intrinsic");
            // A pointer value's alignment describes that value, not its target.
            // The runtime checks the actual accessed address against builtin width.
        }
        private static void ValidateIntrinsicVoid(CppType type)
        {
            ValidateIntrinsicQualifiers(type);
            if (Unwrap(type).Kind.Name != "tk_void") throw Limit("The selected intrinsic result is not void.", "intrinsic");
        }
    }
}
