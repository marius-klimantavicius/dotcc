using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private const string VaType = "global::RoslynObj.CSharp.Runtime.SysVVaList";
        private const string VaRuntime = "global::RoslynObj.CSharp.Runtime.SysVVariadic";
        private sealed record VariadicCallItem(RoutineType Signature, FunctionSymbol? Function,
            IReadOnlyList<CppType> ArgumentTypes, string Helper, string? NativeAdapter);
        private readonly List<VariadicCallItem> variadicCalls = [];
        private readonly Dictionary<FunctionSymbol, string> variadicParameters = [];

        private string VariadicCall(RoutineType signature, FunctionSymbol? function,
            IReadOnlyList<CppType> argumentTypes, IReadOnlyList<string> arguments, string? address = null)
        {
            ValidateNativeScalarSignature(signature);
            if (argumentTypes.Count < signature.Parameters.Count || argumentTypes.Count != arguments.Count)
                throw Limit("A variadic call has no complete captured fixed prefix and actual argument list.", "varargs");
            for (var i = 0; i < argumentTypes.Count; ++i)
            {
                ValidateVariadicArgument(argumentTypes[i], i >= signature.Parameters.Count);
                if (i < signature.Parameters.Count && !SameScalar(signature.Parameters[i].Type, argumentTypes[i]))
                    throw Limit("A variadic fixed argument needs a selected representation adapter.", "varargs");
            }
            var helper = InternalName("__VariadicCall_" + (variadicCalls.Count + 1));
            string? nativeAdapter = null;
            if (function is null || !functions.Contains(function))
            {
                if (signature.Parameters.Count == 0)
                    throw Limit("Native outbound variadics require a retained fixed prefix for their concrete C17 ABI wrapper.", "varargs");
                address ??= function is not null ? FunctionAddress(function) : throw Limit("An indirect variadic call has no retained callee value.", "varargs");
                _ = RequireGeneratedAbiLibrary();
                nativeAdapter = InternalName("__outbound_variadic_" + (variadicCalls.Count + 1));
            }
            variadicCalls.Add(new(signature, function, argumentTypes.ToArray(), helper, nativeAdapter));
            return $"{Owner}.{helper}({(address is null ? "" : address + (arguments.Count == 0 ? "" : ", "))}{string.Join(", ", arguments)})";
        }

        private void ValidateVariadicArgument(CppType type, bool tail)
        {
            ValidateNativeScalarType(type, false);
            var name = TypeName(type);
            if (tail && name is "float" or "byte" or "sbyte" or "short" or "ushort")
                throw Limit("A variadic tail lacks the already-selected default-promoted argument representation.", "varargs");
        }

        private string VariadicOperator(OperatorOperation operation)
        {
            var operands = operation.Operands;
            if (operands.Count == 0) throw Limit("A variadic operator has no retained cursor operand.", "varargs");
            ValidateVaCursor(operands[0].Type);
            var cursor = Expression(operands[0]);
            switch (operation.OperatorKind.Name)
            {
                case "eok_va_start":
                    if (currentFunction is null || !variadicParameters.TryGetValue(currentFunction, out var arguments))
                        throw Limit("va_start has no selected variadic function argument pack.", "varargs");
                    if (operands.Count != 2 || operands[1] is not ReferenceOperation { Target: VariableSymbol last } ||
                        currentFunction.Parameters.LastOrDefault()?.Symbol is not { } parameter || !parameter.Equals(last))
                        throw Limit("va_start needs the retained simple last named parameter; other forms are unaccepted.", "varargs");
                    return $"{VaRuntime}.Start({cursor}, {arguments})";
                case "eok_va_copy":
                    if (operands.Count != 2) throw Limit("va_copy requires two retained cursor operands.", "varargs");
                    ValidateVaCursor(operands[1].Type);
                    return $"{VaRuntime}.Copy({cursor}, {Expression(operands[1])})";
                case "eok_va_end":
                    if (operands.Count != 1) throw Limit("va_end has an unsupported operand shape.", "varargs");
                    return $"{VaRuntime}.End({cursor})";
                case "eok_va_arg":
                    if (operands.Count != 1) throw Limit("va_arg has an unsupported operand shape.", "varargs");
                    if (operation.ValueCategory.Name != "prvalue")
                        throw Limit("A value-category-preserving va_arg slot/address requires a separate lvalue cursor plan.", "varargs");
                    ValidateVariadicArgument(operation.Type, true);
                    var name = TypeName(operation.Type);
                    var next = name switch
                    {
                        "int" => "NextInt32", "uint" => "NextUInt32", "long" => "NextInt64", "ulong" => "NextUInt64", "double" => "NextDouble",
                        _ when Unwrap(operation.Type) is PointerType => "NextPointer",
                        _ => throw Limit("va_arg requires a proven promoted integer/pointer/double cursor operation.", "varargs")
                    };
                    var value = $"{VaRuntime}.{next}({cursor})";
                    return next == "NextPointer" ? $"({name})({value})" : value;
                default: throw Limit("This variadic operator requires a different argument-pack contract.", "varargs");
            }
        }

        private static void ValidateVaCursor(CppType type)
        {
            if (Unwrap(type) is not PointerType pointer || Unwrap(pointer.PointeeType) is not RecordType record || !record.Record.IsVaListTag ||
                pointer.Size != 8 || pointer.Alignment != 8 || record.Size != 24 || record.Alignment != 8 || record.Layout is not { } layout)
                throw Limit("Variadic cursor storage is not the captured SysV AMD64 builtin tag/pointer representation.", "varargs");
            var fields = layout.Fields;
            if (fields.Count != 4 || fields.Any(f => f.Field.Details.IsBitField) ||
                !CursorOffset(fields[0], 0) || !CursorOffset(fields[1], 4) || !CursorPointer(fields[2], 8) || !CursorPointer(fields[3], 16))
                throw Limit("The captured builtin va_list field coordinates/types do not match the SysV AMD64 cursor ABI.", "varargs");
        }

        private static bool CursorOffset(FieldLayout field, int offset) => field.Offset == offset &&
            Unwrap(field.Field.Type) is ArithmeticType type && type.Size == 4 && type.Alignment == 4 && type.Arithmetic.Bits == 32 && type.Arithmetic.Signed == false;
        private static bool CursorPointer(FieldLayout field, int offset) => field.Offset == offset &&
            Unwrap(field.Field.Type) is PointerType type && type.Size == 8 && type.Alignment == 8 && Unwrap(type.PointeeType).Kind.Name == "tk_void";

        private void EmitVariadicSupport()
        {
            foreach (var call in variadicCalls)
            {
                var signature = call.Signature;
                var fixedCount = signature.Parameters.Count;
                var gp = 0; var fp = 0; var overflow = 0;
                var namedGp = 0; var namedFp = 0; var namedOverflow = 0;
                var offsets = new List<int>();
                for (var i = 0; i < call.ArgumentTypes.Count; ++i)
                {
                    var floating = TypeName(call.ArgumentTypes[i]) is "float" or "double";
                    if (floating && fp++ < 8) offsets.Add(48 + (fp - 1) * 16);
                    else if (!floating && gp++ < 6) offsets.Add((gp - 1) * 8);
                    else offsets.Add(176 + overflow++ * 8);
                    if (i + 1 == fixedCount) { namedGp = Math.Min(gp, 6) * 8; namedFp = Math.Min(fp, 8) * 16; namedOverflow = overflow; }
                }
                var bytes = checked(176 + overflow * 8);
                var writer = new CodeWriter();
                var formals = call.ArgumentTypes.Select((t, i) => $"{TypeName(t)} argument{i}").ToList();
                if (call.NativeAdapter is not null)
                {
                    formals.Insert(0, "nint address");
                    writer.Line($"[DllImport(\"{NativeAbiLibraryName}\", EntryPoint = \"{call.NativeAdapter}\", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]");
                    writer.Line($"private static extern {TypeName(signature.ReturnType)} {call.NativeAdapter}({string.Join(", ", formals)});");
                    writer.Line();
                }
                writer.Line($"private static {TypeName(signature.ReturnType)} {call.Helper}({string.Join(", ", formals)})");
                writer.Open();
                writer.Line($"byte* storage = stackalloc byte[{checked(bytes + 15)}];");
                writer.Line($"byte* registers = {VaRuntime}.Align16(storage);");
                writer.Line($"NativeMemory.Clear(registers, {bytes});");
                for (var i = 0; i < call.ArgumentTypes.Count; ++i)
                    writer.Line($"*({TypeName(call.ArgumentTypes[i])}*)(registers + {offsets[i]}) = argument{i};");
                writer.Line($"{VaType} pack = new() {{ GpOffset = {namedGp}U, FpOffset = {48 + namedFp}U, OverflowArgumentArea = registers + {176 + namedOverflow * 8}, RegisterSaveArea = registers }};");
                var arguments = string.Join(", ", signature.Parameters.Select((_, i) => $"argument{i}").Append("&pack"));
                var returnPrefix = TypeName(signature.ReturnType) == "void" ? "" : "return ";
                if (call.Function is { } function && functions.Contains(function))
                    writer.Line($"{returnPrefix}{OwnedName(function)}({arguments});");
                else
                {
                    foreach (var entry in functionAddresses.Where(a => a.VariadicEntry is not null && SameVariadicRepresentation(a.Function.Signature, signature)))
                    {
                        writer.Line($"if (address == {Owner}.{entry.Field})"); writer.Open();
                        writer.Line($"{returnPrefix}{OwnedName(entry.Function)}({arguments});");
                        if (returnPrefix.Length == 0) writer.Line("return;");
                        writer.Close();
                    }
                    var allArguments = string.Join(", ", call.ArgumentTypes.Select((_, i) => $"argument{i}"));
                    writer.Line($"{returnPrefix}{Owner}.{call.NativeAdapter}(address{(allArguments.Length == 0 ? "" : ", " + allArguments)});");
                }
                writer.Close();
                methods.Append(writer.TextWithIndent(1)).AppendLine();
            }
        }

        private bool SameVariadicRepresentation(RoutineType left, RoutineType right)
        {
            ValidateNativeScalarSignature(left);
            ValidateNativeScalarSignature(right);
            return left.IsVariadic && right.IsVariadic && SameScalar(left.ReturnType, right.ReturnType) &&
                left.Parameters.Count == right.Parameters.Count && left.Parameters.Zip(right.Parameters).All(p => SameScalar(p.First.Type, p.Second.Type));
        }
    }
}
