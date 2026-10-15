using System.Text;
using System.Text.Json;
using RoslynObj;

namespace RoslynObj.CSharp;

/// <summary>Explicit native provider binding. Signature and ABI come from the selected public object.</summary>
public sealed record CSharpExternalFunctionBinding(string LinkageName, string LibraryPath, string EntryPoint);

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record NativeImport(FunctionSymbol Function, CSharpExternalFunctionBinding Binding, string LibraryField);
        private sealed record FunctionAddressItem(FunctionSymbol Function, string Field, string? Wrapper);
        private readonly List<NativeImport> nativeImports = [];
        private readonly Dictionary<string, string> nativeLibraries = new(StringComparer.Ordinal);
        private readonly List<FunctionAddressItem> functionAddresses = [];
        private readonly Dictionary<RoutineType, string> indirectCalls = [];

        private string Owner => $"global::{options.NamespaceName}.{options.ClassName}";
        private string OwnedName(SemanticEntity entity) => Owner + "." + names[entity];

        private string InternalName(string proposed)
        {
            var name = proposed;
            var suffix = 1;
            while (!globalNames.Add(name)) name = proposed + "__" + (++suffix);
            return name;
        }

        private static void ValidateCallingConvention(RoutineType signature)
        {
            if (signature.CallingConvention?.Name is not (null or "default" or "cdecl" or "cc_default" or "cc_cdecl"))
                throw Limit($"Calling convention '{signature.CallingConvention}' has no current native adapter.", "calling-convention");
        }

        private void ValidateNativeScalarSignature(RoutineType signature)
        {
            ValidateCallingConvention(signature);
            foreach (var type in signature.Parameters.Select(p => p.Type).Append(signature.ReturnType))
            {
                var raw = Unwrap(type);
                if (raw.Kind.Name == "tk_void" || raw is PointerType || raw is ArithmeticType && StorageSize(raw) <= 8)
                {
                    _ = TypeName(type);
                    continue;
                }
                throw Limit("Native aggregate/vector/extended-scalar calls require an exercised ABI adapter.", "native-abi");
            }
        }

        private void RequireCallable(FunctionSymbol function)
        {
            if (functions.Contains(function) || nativeImports.Any(i => i.Function.Equals(function))) return;
            var linkageName = function.GnuSupplement?.AssemblyName ?? function.CurrentName;
            var bindings = options.ExternalFunctions.Where(b => b.LinkageName == linkageName).ToArray();
            if (bindings.Length != 1) throw Limit($"Function '{function.Name}' requires one explicit binding for captured linkage '{linkageName}'.", "external-function");
            if (function.Signature.IsVariadic) throw Limit("Native variadic imports require a fixed-signature/v-form ABI adapter.", "varargs");
            ValidateCallingConvention(function.Signature);
            ValidateNativeScalarSignature(function.Signature);
            _ = TypeName(function.ReturnType);
            foreach (var parameter in function.Signature.Parameters) _ = TypeName(parameter.Type);
            var binding = bindings[0];
            if (string.IsNullOrWhiteSpace(binding.LibraryPath) || string.IsNullOrWhiteSpace(binding.EntryPoint))
                throw Limit("An external binding needs an explicit provider library and entry point.", "external-function");
            if (!nativeLibraries.TryGetValue(binding.LibraryPath, out var library))
                nativeLibraries.Add(binding.LibraryPath, library = InternalName($"__library_{nativeLibraries.Count + 1}"));
            Name(function, function.Name ?? "import", globalNames);
            nativeImports.Add(new(function, binding, library));
        }

        private string FunctionAddress(FunctionSymbol function)
        {
            var existing = functionAddresses.FirstOrDefault(a => a.Function.Equals(function));
            if (existing is not null) return Owner + "." + existing.Field;
            RequireCallable(function);
            if (function.Signature.IsVariadic) throw Limit("An addressed variadic function requires its reviewed fixed-prefix native entry adapter.", "varargs");
            ValidateCallingConvention(function.Signature);
            ValidateNativeScalarSignature(function.Signature);
            var address = new FunctionAddressItem(function, InternalName("__address_" + names[function].TrimStart('@')),
                functions.Contains(function) ? InternalName("__native_" + names[function].TrimStart('@')) : null);
            functionAddresses.Add(address);
            return Owner + "." + address.Field;
        }

        private string EmitCall(CallOperation call)
        {
            if (call.Receiver is not null || call.Call.ReturnSlot is not null) throw Limit("Hidden receiver/result-slot adaptation requires a proven ABI representation.", "call");
            if (call.Operator.EvalRightToLeft && call.Arguments.Count > 1 && call.Arguments.Any(a => HasEffects(a.Value)))
                throw Limit("A call with right-to-left evaluation and effectful arguments requires explicit sequencing temporaries.", "sequencing");
            if (call.Operator.EvalRightToLeft && call.Callee is not ReferenceOperation { Target: FunctionSymbol } &&
                (HasEffects(call.Callee) || call.Arguments.Any(a => HasEffects(a.Value))))
                throw Limit("An effectful right-to-left indirect call requires explicit callee/argument planning.", "sequencing");
            var arguments = string.Join(", ", call.Arguments.Select(a => Expression(a.Value)));
            if (call.Callee is ReferenceOperation { Target: FunctionSymbol callee })
            {
                if (call.TargetMethod is { } selected && !selected.Equals(callee))
                    throw Limit("Selected call target and actual routine operand need an explicit adapter.", "call");
                RequireCallable(callee);
                if (callee.Signature.IsVariadic) throw Limit("Translated variadic calls require the target argument pack.", "varargs");
                return $"{OwnedName(callee)}({arguments})";
            }
            var type = Unwrap(call.Call.FunctionType ?? call.Callee.Type);
            if (type is PointerType pointer) type = Unwrap(pointer.PointeeType);
            if (type is not RoutineType signature) throw Limit("Indirect call has no selected current routine type.", "call");
            if (signature.IsVariadic) throw Limit("Indirect variadic calls require explicit argument packs and native entry adapters.", "varargs");
            ValidateCallingConvention(signature);
            ValidateNativeScalarSignature(signature);
            if (signature.Parameters.Count != call.Arguments.Count) throw Limit("Indirect call needs parameter adaptation beyond its current signature.", "call");
            if (!indirectCalls.TryGetValue(signature, out var helper))
                indirectCalls.Add(signature, helper = InternalName($"__Invoke_{indirectCalls.Count + 1}"));
            var addressExpression = call.Callee is OperatorOperation { OperatorKind.Name: "eok_indirect" } indirect
                ? Expression(indirect.Operands[0]) : Expression(call.Callee);
            return $"{Owner}.{helper}({addressExpression}{(arguments.Length == 0 ? "" : ", " + arguments)})";
        }

        private string FunctionPointerType(RoutineType signature) => "delegate* unmanaged[Cdecl]<" +
            string.Join(", ", signature.Parameters.Select(p => TypeName(p.Type)).Append(TypeName(signature.ReturnType))) + ">";

        private bool SameRepresentation(RoutineType left, RoutineType right)
        {
            ValidateNativeScalarSignature(left);
            ValidateNativeScalarSignature(right);
            return !left.IsVariadic && !right.IsVariadic && SameScalar(left.ReturnType, right.ReturnType) &&
                left.Parameters.Count == right.Parameters.Count && left.Parameters.Zip(right.Parameters).All(p => SameScalar(p.First.Type, p.Second.Type));
        }

        private bool SameScalar(CppType left, CppType right)
        {
            left = Unwrap(left);
            right = Unwrap(right);
            return TypeName(left) == TypeName(right) && left.Size == right.Size && left.Alignment == right.Alignment &&
                (left is not ArithmeticType la || right is ArithmeticType ra && la.Arithmetic.Bits == ra.Arithmetic.Bits && la.Arithmetic.Signed == ra.Arithmetic.Signed);
        }

        private void EmitCallSupport(StringBuilder fields)
        {
            foreach (var library in nativeLibraries)
                fields.Append("    private static readonly nint ").Append(library.Value).Append(" = NativeLibrary.Load(").Append(JsonSerializer.Serialize(library.Key)).AppendLine(");");
            foreach (var address in functionAddresses)
            {
                fields.Append("    private static readonly nint ").Append(address.Field).Append(" = ");
                if (address.Wrapper is not null)
                    fields.Append("(nint)(").Append(FunctionPointerType(address.Function.Signature)).Append(")&").Append(address.Wrapper).AppendLine(";");
                else
                {
                    var import = nativeImports.Single(i => i.Function.Equals(address.Function));
                    fields.Append("NativeLibrary.GetExport(").Append(import.LibraryField).Append(", ").Append(JsonSerializer.Serialize(import.Binding.EntryPoint)).AppendLine(");");
                }
            }
            if (nativeLibraries.Count != 0 || functionAddresses.Count != 0) fields.AppendLine();
            foreach (var import in nativeImports)
            {
                var signature = import.Function.Signature;
                methods.Append("    [DllImport(").Append(JsonSerializer.Serialize(import.Binding.LibraryPath))
                    .Append(", EntryPoint = ").Append(JsonSerializer.Serialize(import.Binding.EntryPoint)).AppendLine(", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]");
                methods.Append("    private static extern ").Append(TypeName(signature.ReturnType)).Append(' ').Append(names[import.Function]).Append('(')
                    .Append(string.Join(", ", signature.Parameters.Select((p, i) => $"{TypeName(p.Type)} argument{i}"))).AppendLine(");").AppendLine();
            }
            foreach (var address in functionAddresses.Where(a => a.Wrapper is not null))
            {
                var signature = address.Function.Signature;
                methods.AppendLine("    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]");
                methods.Append("    private static ").Append(TypeName(signature.ReturnType)).Append(' ').Append(address.Wrapper).Append('(')
                    .Append(string.Join(", ", signature.Parameters.Select((p, i) => $"{TypeName(p.Type)} argument{i}"))).AppendLine(")").AppendLine("    {");
                methods.Append("        ").Append(TypeName(signature.ReturnType) == "void" ? "" : "return ").Append(OwnedName(address.Function)).Append('(')
                    .Append(string.Join(", ", signature.Parameters.Select((_, i) => $"argument{i}"))).AppendLine(");").AppendLine("    }").AppendLine();
            }
            foreach (var indirect in indirectCalls)
            {
                var signature = indirect.Key;
                var resultType = TypeName(signature.ReturnType);
                var arguments = string.Join(", ", signature.Parameters.Select((_, i) => $"argument{i}"));
                methods.Append("    private static ").Append(resultType).Append(' ').Append(indirect.Value).Append("(nint address")
                    .Append(string.Concat(signature.Parameters.Select((p, i) => $", {TypeName(p.Type)} argument{i}"))).AppendLine(")").AppendLine("    {");
                foreach (var address in functionAddresses.Where(a => a.Wrapper is not null && SameRepresentation(a.Function.Signature, signature)))
                {
                    methods.Append("        if (address == ").Append(Owner).Append('.').Append(address.Field).AppendLine(")").AppendLine("        {");
                    methods.Append("            ").Append(resultType == "void" ? "" : "return ").Append(OwnedName(address.Function)).Append('(').Append(arguments).AppendLine(");");
                    if (resultType == "void") methods.AppendLine("            return;");
                    methods.AppendLine("        }");
                }
                methods.Append("        ").Append(resultType == "void" ? "" : "return ").Append("((").Append(FunctionPointerType(signature)).Append(")address)(").Append(arguments).AppendLine(");");
                methods.AppendLine("    }").AppendLine();
            }
        }

        private string NativeImportReport() => JsonSerializer.Serialize(new
        {
            providerContract = "explicit native ABI bindings; no native application bodies",
            imports = nativeImports.Select(i => new { linkageName = i.Function.GnuSupplement?.AssemblyName ?? i.Function.CurrentName,
                library = i.Binding.LibraryPath, entryPoint = i.Binding.EntryPoint, callingConvention = "cdecl",
                returnType = TypeName(i.Function.ReturnType), parameters = i.Function.Parameters.Select(p => TypeName(p.Type)) })
        }, new JsonSerializerOptions { WriteIndented = true }) + "\n";
    }
}
