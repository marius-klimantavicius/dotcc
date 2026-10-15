using System.Globalization;
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
        private sealed record FunctionAddressItem(FunctionSymbol Function, string Field, string? Wrapper,
            string? VariadicEntry = null, string? VariadicBridge = null, string? VariadicRegistration = null,
            string? AggregateEntry = null, string? AggregateBridge = null, string? AggregateRegistration = null,
            string? HostEntry = null, string? HostRegistration = null, bool LogicalErrnoProvider = false, bool LogicalDlerrorProvider = false);
        private readonly List<NativeImport> nativeImports = [];
        private readonly Dictionary<string, string> nativeLibraries = new(StringComparer.Ordinal);
        private readonly List<FunctionAddressItem> functionAddresses = [];
        private readonly Dictionary<RoutineType, string> indirectCalls = [];

        private string Owner => $"global::{options.NamespaceName}.{options.ClassName}";
        private string OwnedName(SemanticEntity entity) => Owner + "." + names[CanonicalEntity(entity)];

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
            ValidateOrdinaryRoutineRoles(signature);
            if (!signature.HasPrototype) throw Limit("Native calls require a retained current prototype.", "native-abi");
            foreach (var parameter in signature.Parameters) ValidateNativeScalarType(parameter.Type, false);
            ValidateNativeScalarType(signature.ReturnType, true);
        }

        private static void ValidateOrdinaryRoutineRoles(RoutineType signature)
        {
            if (signature.HasThisParameter || signature.ReturnsValueAsParameter == true || signature.ReturnsValueViaCopyConstructor ||
                signature.Parameters.Any(p => p.Role.Name != "ordinary_parameter" || p.Descriptor.Details.PassedViaCopyConstructor))
                throw Limit("Hidden this/result/other parameter roles require an exercised selected ABI adapter.", "native-abi");
        }

        private void ValidateNativeScalarType(CppType type, bool allowVoid)
        {
            var raw = Unwrap(type);
            if (raw.Kind.Name == "tk_void" && allowVoid) return;
            if (raw is PointerType pointer)
            {
                if (pointer.IsReference)
                    throw Limit("Native reference parameters/results require a separately exercised C++ boundary adapter.", "native-abi");
                if (type.Size != 8 || type.Alignment != 8 || raw.Size != 8 || raw.Alignment != 8)
                    throw Limit("Native pointers require the captured natural 8-byte scalar representation.", "native-abi");
                _ = TypeName(type);
                return;
            }
            if (raw is not ArithmeticType) throw Limit("Native aggregate/vector/extended-scalar calls require an exercised ABI adapter.", "native-abi");
            var size = TypeName(type) switch
            {
                "byte" or "sbyte" => 1, "short" or "ushort" => 2, "int" or "uint" or "float" => 4,
                "long" or "ulong" or "double" => 8,
                _ => throw Limit("The scalar native representation is unsupported.", "native-abi")
            };
            if (type.Size != size || type.Alignment != size || raw.Size != size || raw.Alignment != size)
                throw Limit("Native arithmetic scalars require their captured natural size and alignment.", "native-abi");
        }

        private void RequireCallable(FunctionSymbol function)
        {
            function = CanonicalFunction(function);
            if (IsIntrinsicRoutine(function)) throw Limit("A retained compiler intrinsic has no native provider fallback.", "intrinsic");
            if (functions.Contains(function) || nativeImports.Any(i => i.Function.Equals(function))) return;
            var linkageName = function.GnuSupplement?.AssemblyName ?? function.CurrentName;
            var bindings = options.ExternalFunctions.Where(b => b.LinkageName == linkageName).ToArray();
            if (bindings.Length != 1) throw Limit($"Function '{function.Name}' requires one explicit binding for captured linkage '{linkageName}'.", "external-function");
            ValidateCallingConvention(function.Signature);
            if (HasAggregateSignature(function.Signature)) ValidateNativeAggregateSignature(function.Signature);
            else ValidateNativeScalarSignature(function.Signature);
            _ = TypeName(function.ReturnType);
            foreach (var parameter in function.Signature.Parameters) _ = TypeName(parameter.Type);
            var binding = bindings[0];
            if (string.IsNullOrWhiteSpace(binding.LibraryPath) || string.IsNullOrWhiteSpace(binding.EntryPoint))
                throw Limit("An external binding needs an explicit provider library and entry point.", "external-function");
            if (!nativeLibraries.TryGetValue(binding.LibraryPath, out var library))
                nativeLibraries.Add(binding.LibraryPath, library = InternalName($"__library_{nativeLibraries.Count + 1}"));
            Name(function, function.Name ?? "import", globalNames);
            nativeImports.Add(new(function, binding, library));
            if (HostProtocolEnabled) { RequireHostLayouts(nativeImports[^1]); RequireHostLoaderImport(nativeImports[^1]); }
            if (HostProtocolEnabled && !function.Signature.IsVariadic && !HasAggregateSignature(function.Signature))
            {
                if (!IsLogicalErrnoProvider(function) && !IsLogicalDlerrorProvider(function)) _ = RequireHostNativeCall(function.Signature);
                _ = FunctionAddress(function);
            }
        }

        private string FunctionAddress(FunctionSymbol function)
        {
            function = CanonicalFunction(function);
            if (IsIntrinsicRoutine(function)) throw Limit("A retained compiler intrinsic has no native callable address.", "intrinsic");
            var existing = functionAddresses.FirstOrDefault(a => a.Function.Equals(function));
            if (existing is not null) return Owner + "." + existing.Field;
            RequireCallable(function);
            // Import registration may have allocated its protocol address.
            existing = functionAddresses.FirstOrDefault(a => a.Function.Equals(function));
            if (existing is not null) return Owner + "." + existing.Field;
            ValidateCallingConvention(function.Signature);
            if (HasAggregateSignature(function.Signature)) ValidateNativeAggregateSignature(function.Signature);
            else ValidateNativeScalarSignature(function.Signature);
            FunctionAddressItem address;
            if (!functions.Contains(function) && IsLogicalErrnoProvider(function))
                address = new(function, InternalName("__address_" + names[function].TrimStart('@')), null, LogicalErrnoProvider: true);
            else if (!functions.Contains(function) && IsLogicalDlerrorProvider(function))
                address = new(function, InternalName("__address_" + names[function].TrimStart('@')), null, LogicalDlerrorProvider: true);
            else if (HasAggregateSignature(function.Signature) && functions.Contains(function))
            {
                _ = RequireGeneratedAbiLibrary();
                var stem = names[function].TrimStart('@');
                address = new(function, InternalName("__address_" + stem), null,
                    AggregateEntry: InternalName("__aggregate_entry_" + stem), AggregateBridge: InternalName("__aggregate_bridge_" + stem),
                    AggregateRegistration: InternalName("__register_aggregate_" + stem));
            }
            else if (function.Signature.IsVariadic && functions.Contains(function))
            {
                if (function.Signature.Parameters.Count == 0)
                    throw Limit("A native variadic entry requires at least one captured fixed parameter for the C17 ABI wrapper.", "varargs");
                _ = RequireGeneratedAbiLibrary();
                var stem = names[function].TrimStart('@');
                address = new(function, InternalName("__address_" + stem), null,
                    InternalName("__variadic_entry_" + stem), InternalName("__variadic_bridge_" + stem), InternalName("__register_variadic_" + stem));
            }
            else if (HostProtocolEnabled && functions.Contains(function))
            {
                var stem = names[function].TrimStart('@');
                address = new(function, InternalName("__address_" + stem), InternalName("__native_" + stem),
                    HostEntry: InternalName("__host_entry_" + stem), HostRegistration: InternalName("__register_host_" + stem));
            }
            else address = new(function, InternalName("__address_" + names[function].TrimStart('@')),
                functions.Contains(function) ? InternalName("__native_" + names[function].TrimStart('@')) : null);
            functionAddresses.Add(address);
            return Owner + "." + address.Field;
        }

        private string EmitCall(CallOperation call)
        {
            ValidateGeneratedCallRoles(call);
            if (call.Operator.EvalRightToLeft && call.Arguments.Count > 1 && call.Arguments.Any(a => HasEffects(a.Value)))
                throw Limit("A call with right-to-left evaluation and effectful arguments requires explicit sequencing temporaries.", "sequencing");
            if (call.Operator.EvalRightToLeft && call.Callee is not ReferenceOperation { Target: FunctionSymbol } &&
                (HasEffects(call.Callee) || call.Arguments.Any(a => HasEffects(a.Value))))
                throw Limit("An effectful right-to-left indirect call requires explicit callee/argument planning.", "sequencing");
            var argumentValues = call.Arguments.Select(a => Expression(a.Value)).ToArray();
            var arguments = string.Join(", ", argumentValues);
            if (call.Callee is ReferenceOperation { Target: FunctionSymbol callee })
            {
                callee = CanonicalFunction(callee);
                if (call.TargetMethod is { } selected && !CanonicalFunction(selected).Equals(callee))
                    throw Limit("Selected call target and actual routine operand need an explicit adapter.", "call");
                if (TryIntrinsicCall(call, argumentValues, out var intrinsic)) return intrinsic;
                RequireCallable(callee);
                if (callee.Signature.IsVariadic) return VariadicCall(callee.Signature, callee, call.Arguments.Select(a => a.Value.Type).ToArray(), argumentValues);
                if (callee.Signature.Parameters.Count != call.Arguments.Count)
                    throw Limit("A selected fixed call needs explicit parameter adaptation.", "call");
                if (HasAggregateSignature(callee.Signature) && !functions.Contains(callee))
                    return AggregateCall(callee.Signature, callee, call.Arguments.Select(a => a.Value.Type).ToArray(), argumentValues);
                arguments = string.Join(", ", argumentValues.Select((value, i) => ProgramArgumentView(call.Arguments[i].Value.Type, callee.Signature.Parameters[i].Type, value)));
                return ProgramResultView(callee.ReturnType, call.Type, $"{OwnedName(callee)}({arguments})");
            }
            var type = Unwrap(call.Call.FunctionType ?? call.Callee.Type);
            if (type is PointerType pointer) type = Unwrap(pointer.PointeeType);
            if (type is not RoutineType signature) throw Limit("Indirect call has no selected current routine type.", "call");
            if (signature.IsVariadic)
            {
                var calleeAddress = call.Callee is OperatorOperation { OperatorKind.Name: "eok_indirect" } variadicIndirect
                    ? Expression(variadicIndirect.Operands[0]) : Expression(call.Callee);
                return VariadicCall(signature, null, call.Arguments.Select(a => a.Value.Type).ToArray(), argumentValues, calleeAddress);
            }
            if (HasAggregateSignature(signature))
            {
                var aggregateAddress = call.Callee is OperatorOperation { OperatorKind.Name: "eok_indirect" } aggregateIndirect
                    ? Expression(aggregateIndirect.Operands[0]) : Expression(call.Callee);
                return AggregateCall(signature, null, call.Arguments.Select(a => a.Value.Type).ToArray(), argumentValues, aggregateAddress);
            }
            ValidateCallingConvention(signature);
            ValidateNativeScalarSignature(signature);
            if (signature.Parameters.Count != call.Arguments.Count) throw Limit("Indirect call needs parameter adaptation beyond its current signature.", "call");
            if (!indirectCalls.TryGetValue(signature, out var helper))
                indirectCalls.Add(signature, helper = InternalName($"__Invoke_{indirectCalls.Count + 1}"));
            var addressExpression = call.Callee is OperatorOperation { OperatorKind.Name: "eok_indirect" } indirect
                ? Expression(indirect.Operands[0]) : Expression(call.Callee);
            arguments = string.Join(", ", argumentValues.Select((value, i) => ProgramArgumentView(call.Arguments[i].Value.Type, signature.Parameters[i].Type, value)));
            return ProgramResultView(signature.ReturnType, call.Type, $"{Owner}.{helper}({addressExpression}{(arguments.Length == 0 ? "" : ", " + arguments)})");
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
            if (TryProgramPointerRepresentation(left, right, out var pointerCompatible)) return pointerCompatible;
            left = Unwrap(left);
            right = Unwrap(right);
            return TypeName(left) == TypeName(right) && left.Size == right.Size && left.Alignment == right.Alignment &&
                (left is not ArithmeticType la || right is ArithmeticType ra && la.Arithmetic.Bits == ra.Arithmetic.Bits && la.Arithmetic.Signed == ra.Arithmetic.Signed);
        }

        private void EmitCallSupport(StringBuilder fields, CodeWriter initialization)
        {
            foreach (var library in nativeLibraries)
            {
                fields.Append("    private static readonly nint ").Append(library.Value).Append(" = NativeLibrary.Load(").Append(JsonSerializer.Serialize(library.Key));
                if (library.Key == NativeAbiLibraryName) fields.Append(", typeof(").Append(Owner).Append(").Assembly, DllImportSearchPath.AssemblyDirectory");
                fields.AppendLine(");");
            }
            foreach (var address in functionAddresses)
            {
                fields.Append("    private static readonly nint ").Append(address.Field).Append(" = ");
                if (address.LogicalErrnoProvider)
                    fields.Append("NativeLibrary.GetExport(").Append(RequireGeneratedAbiLibrary()).Append(", ").Append(JsonSerializer.Serialize(logicalErrnoEntry)).AppendLine(");");
                else if (address.LogicalDlerrorProvider)
                    fields.Append("NativeLibrary.GetExport(").Append(RequireGeneratedAbiLibrary()).Append(", ").Append(JsonSerializer.Serialize(logicalDlerrorEntry)).AppendLine(");");
                else if (address.HostEntry is not null)
                    fields.Append("NativeLibrary.GetExport(").Append(RequireGeneratedAbiLibrary()).Append(", ").Append(JsonSerializer.Serialize(address.HostEntry)).AppendLine(");");
                else if (address.AggregateEntry is not null)
                    fields.Append("NativeLibrary.GetExport(").Append(RequireGeneratedAbiLibrary()).Append(", ").Append(JsonSerializer.Serialize(address.AggregateEntry)).AppendLine(");");
                else if (address.VariadicEntry is not null)
                    fields.Append("NativeLibrary.GetExport(").Append(RequireGeneratedAbiLibrary()).Append(", ").Append(JsonSerializer.Serialize(address.VariadicEntry)).AppendLine(");");
                else if (address.Wrapper is not null)
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
                if (signature.IsVariadic || HasAggregateSignature(signature)) continue; // Only concrete scalar/pointer gateways may call these native signatures.
                if (HostProtocolEnabled) { EmitHostImport(import); continue; }
                methods.Append("    [DllImport(").Append(JsonSerializer.Serialize(import.Binding.LibraryPath))
                    .Append(", EntryPoint = ").Append(JsonSerializer.Serialize(import.Binding.EntryPoint)).AppendLine(", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]");
                methods.Append("    private static extern ").Append(TypeName(signature.ReturnType)).Append(' ').Append(names[import.Function]).Append('(')
                    .Append(string.Join(", ", signature.Parameters.Select((p, i) => $"{TypeName(p.Type)} argument{i}"))).AppendLine(");").AppendLine();
            }
            foreach (var address in functionAddresses.Where(a => a.Wrapper is not null))
            {
                var signature = address.Function.Signature;
                if (HostProtocolEnabled)
                {
                    var writer = new CodeWriter();
                    var resultType = TypeName(signature.ReturnType);
                    writer.Line("[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]");
                    writer.Line($"private static {resultType} {address.Wrapper}({string.Join(", ", signature.Parameters.Select((p, i) => $"{TypeName(p.Type)} argument{i}"))})");
                    writer.Open();
                    EmitBoundaryBody(writer, resultType, () => writer.Line($"{(resultType == "void" ? "" : "return ")}{OwnedName(address.Function)}({string.Join(", ", signature.Parameters.Select((_, i) => $"argument{i}"))});"));
                    writer.Close();
                    methods.Append(writer.TextWithIndent(1)).AppendLine();
                    continue;
                }
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
                    var selectedSignature = address.Function.Signature;
                    var selectedArguments = string.Join(", ", signature.Parameters.Select((p, i) => ProgramArgumentView(p.Type, selectedSignature.Parameters[i].Type, $"argument{i}")));
                    var selectedCall = ProgramResultView(address.Function.ReturnType, signature.ReturnType, $"{OwnedName(address.Function)}({selectedArguments})");
                    methods.Append("            ").Append(resultType == "void" ? "" : "return ").Append(selectedCall).AppendLine(";");
                    if (resultType == "void") methods.AppendLine("            return;");
                    methods.AppendLine("        }");
                }
                if (HostProtocolEnabled)
                    methods.Append("        ").Append(resultType == "void" ? "" : "return ").Append(HostNativeCallExpression(signature, "address", signature.Parameters.Select((_, i) => $"argument{i}"))).AppendLine(";");
                else
                    methods.Append("        ").Append(resultType == "void" ? "" : "return ").Append("((").Append(FunctionPointerType(signature)).Append(")address)(").Append(arguments).AppendLine(");");
                methods.AppendLine("    }").AppendLine();
            }
            EmitVariadicEntryBridges(initialization);
            EmitAggregateSupport(initialization);
            EmitHostCallSupport(initialization);
        }

        private string NativeImportReport() => JsonSerializer.Serialize(new
        {
            providerContract = "explicit native ABI bindings; no native application bodies",
            nativeErrorProtocol = options.NativeErrorProtocol.ToString(),
            imports = nativeImports.Select(i => new { linkageName = i.Function.GnuSupplement?.AssemblyName ?? i.Function.CurrentName,
                library = i.Binding.LibraryPath, entryPoint = i.Binding.EntryPoint, callingConvention = "cdecl",
                isVariadic = i.Function.Signature.IsVariadic,
                aggregateGateway = HasAggregateSignature(i.Function.Signature),
                logicalErrnoProvider = HostProtocolEnabled && (i.Function.GnuSupplement?.AssemblyName ?? i.Function.CurrentName) == "__errno_location",
                logicalDlerrorProvider = IsLogicalDlerrorProvider(i.Function),
                returnType = TypeName(i.Function.ReturnType), parameters = i.Function.Parameters.Select(p => TypeName(p.Type)) }),
            generatedAbi = generatedAbiLibrary is null ? null : new
            {
                library = NativeAbiLibraryName,
                source = "Generated.Abi.c",
                contract = "concrete SysV AMD64 ABI forwarding only; application bodies remain C#",
                logicalErrno = HostProtocolEnabled ? new { protocol = "linux-x64-logical-errno-v1", entry = logicalErrnoEntry,
                    addressContract = "native thread-local application slot; distinct from raw libc errno; coherent direct/indirect/address-exposed entry" } : null,
                logicalDlerror = HostLoaderEnabled ? new { protocol = "linux-x64-glibc-logical-dlerror-v1", entry = logicalDlerrorEntry,
                    registration = loaderRegistrationEntry,
                    pendingContract = "each selected loader call replaces pending error, including success clearing; NULL result alone is not error",
                    storageContract = "native per-thread owned byte snapshot; read-clear keeps returned buffer alive until next selected loader call; pthread destructor releases storage" } : null,
                loaderProviders = nativeLoaderImports.Select(i => new { linkageName = i.Function.GnuSupplement?.AssemblyName ?? i.Function.CurrentName,
                    library = i.Binding.LibraryPath, entryPoint = i.Binding.EntryPoint }),
                hostEntries = functionAddresses.Where(a => a.HostEntry is not null).Select(a => new
                {
                    linkageName = a.Function.GnuSupplement?.AssemblyName ?? a.Function.CurrentName,
                    entry = a.HostEntry, registration = a.HostRegistration, bridge = a.Wrapper
                }),
                hostOutboundShapes = hostNativeCalls.Select(c => new { entry = c.Adapter,
                    parameters = c.Signature.Parameters.Select(p => TypeName(p.Type)), returnType = TypeName(c.Signature.ReturnType) }),
                entries = functionAddresses.Where(a => a.VariadicEntry is not null).Select(a => new
                {
                    linkageName = a.Function.GnuSupplement?.AssemblyName ?? a.Function.CurrentName,
                    entry = a.VariadicEntry, registration = a.VariadicRegistration, bridge = a.VariadicBridge
                }),
                aggregateEntries = functionAddresses.Where(a => a.AggregateEntry is not null).Select(a => new
                {
                    linkageName = a.Function.GnuSupplement?.AssemblyName ?? a.Function.CurrentName,
                    entry = a.AggregateEntry, registration = a.AggregateRegistration, bridge = a.AggregateBridge
                }),
                aggregateLayouts = aggregateLayouts.Select(a => new
                {
                    type = TypeName(a.Type), nativeType = a.NativeName, shape = a.Shape,
                    size = a.Type.Size?.ToString(CultureInfo.InvariantCulture), alignment = a.Type.Alignment?.ToString(CultureInfo.InvariantCulture),
                    fields = a.Fields.Select(f => new { type = TypeName(f.Field.Type), offset = f.Offset?.ToString(CultureInfo.InvariantCulture) })
                }),
                aggregateOutboundShapes = aggregateInvocations.Select(a => new
                {
                    entry = a.Adapter, parameters = a.Signature.Parameters.Select(p => TypeName(p.Type)), returnType = TypeName(a.Signature.ReturnType),
                    selectedReturnSlot = a.Signature.ReturnsValueAsParameter == true, selectedCopyConstructorReturn = a.Signature.ReturnsValueViaCopyConstructor,
                    parameterRoles = a.Signature.Parameters.Select(p => p.Role.Name)
                }),
                outboundShapes = variadicCalls.Where(c => c.NativeAdapter is not null).Select(c => new
                {
                    entry = c.NativeAdapter, fixedParameterCount = c.Signature.Parameters.Count,
                    arguments = c.ArgumentTypes.Select(TypeName), returnType = TypeName(c.Signature.ReturnType)
                })
            }
        }, new JsonSerializerOptions { WriteIndented = true }) + "\n";
    }
}
