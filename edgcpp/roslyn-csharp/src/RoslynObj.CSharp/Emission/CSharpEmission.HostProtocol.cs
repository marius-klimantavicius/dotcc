using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private sealed record HostNativeCall(RoutineType Signature, string Adapter);
        private readonly List<HostNativeCall> hostNativeCalls = [];
        private string? logicalErrnoEntry;
        private bool HostProtocolEnabled => options.NativeErrorProtocol == NativeErrorProtocol.LinuxX64LogicalErrno;

        // Called by admission after generated class names have been reserved.
        private bool ValidateNativeErrorProtocol()
        {
            if (!Enum.IsDefined(options.NativeErrorProtocol))
            {
                Add("CSHARP2600", "The selected native error protocol is unsupported.", null, "host-provider");
                return false;
            }
            if (!HostProtocolEnabled) return true;
            foreach (var input in inputs)
                if (input.Manifest.Target is not { Abi: "itanium", AddressableUnitBits: 8, Endianness: "little" } target ||
                    target.PointerBits != 64 || target.PointerAlignment != 8)
                {
                    Add("CSHARP2600", "Linux logical errno requires the recorded little-endian SysV AMD64 pointer profile.", null, "host-provider");
                    return false;
                }
            _ = RequireGeneratedAbiLibrary();
            logicalErrnoEntry = InternalName("__roslynobj_application_errno_location");
            return true;
        }

        private bool IsLogicalErrnoProvider(FunctionSymbol function)
        {
            if (!HostProtocolEnabled || (function.GnuSupplement?.AssemblyName ?? function.CurrentName) != "__errno_location") return false;
            var signature = function.Signature;
            if (signature.IsVariadic || signature.Parameters.Count != 0 || Unwrap(signature.ReturnType) is not PointerType pointer ||
                Unwrap(pointer.PointeeType) is not ArithmeticType { Arithmetic.Bits: 32, Arithmetic.Signed: true })
                throw Limit("The logical errno provider requires the exact retained int* (void) signature.", "host-provider");
            var binding = nativeImports.Single(i => i.Function.Equals(function)).Binding;
            if (binding.EntryPoint != "__errno_location")
                throw Limit("The logical errno substitution requires the explicit __errno_location provider contract.", "host-provider");
            return true;
        }

        private HostNativeCall RequireHostNativeCall(RoutineType signature)
        {
            ValidateNativeScalarSignature(signature);
            if (signature.IsVariadic) throw Limit("A native variadic host call needs its concrete selected argument gateway.", "host-provider");
            var item = hostNativeCalls.FirstOrDefault(c => c.Signature.Equals(signature));
            if (item is not null) return item;
            _ = RequireGeneratedAbiLibrary();
            item = new(signature, InternalName("__host_call_" + (hostNativeCalls.Count + 1)));
            hostNativeCalls.Add(item);
            return item;
        }

        private string HostNativeCallExpression(RoutineType signature, string address, IEnumerable<string> arguments)
        {
            var item = RequireHostNativeCall(signature);
            return $"{Owner}.{item.Adapter}({string.Join(", ", new[] { address }.Concat(arguments))})";
        }

        private void EmitHostImport(NativeImport import)
        {
            var signature = import.Function.Signature;
            var resultType = TypeName(signature.ReturnType);
            var formals = signature.Parameters.Select((p, i) => $"{TypeName(p.Type)} argument{i}");
            if (IsLogicalErrnoProvider(import.Function))
            {
                methods.Append("    [DllImport(\"").Append(NativeAbiLibraryName).Append("\", EntryPoint = \"").Append(logicalErrnoEntry)
                    .AppendLine("\", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]");
                methods.Append("    private static extern ").Append(resultType).Append(' ').Append(names[import.Function]).AppendLine("();").AppendLine();
                return;
            }
            var address = functionAddresses.Single(a => a.Function.Equals(import.Function));
            var invocation = HostNativeCallExpression(signature, Owner + "." + address.Field, signature.Parameters.Select((_, i) => $"argument{i}"));
            methods.Append("    private static ").Append(resultType).Append(' ').Append(names[import.Function]).Append('(')
                .Append(string.Join(", ", formals)).AppendLine(")").AppendLine("    {")
                .Append("        ").Append(resultType == "void" ? "" : "return ").Append(invocation).AppendLine(";")
                .AppendLine("    }").AppendLine();
        }

        private void EmitHostCallSupport(CodeWriter initialization)
        {
            if (!HostProtocolEnabled) return;
            initialization.Line("if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64) throw new PlatformNotSupportedException(\"The selected logical errno provider requires Linux x64.\");");
            foreach (var call in hostNativeCalls)
            {
                var signature = call.Signature;
                methods.Append("    [DllImport(\"").Append(NativeAbiLibraryName).Append("\", EntryPoint = \"").Append(call.Adapter)
                    .AppendLine("\", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]");
                methods.Append("    private static extern ").Append(TypeName(signature.ReturnType)).Append(' ').Append(call.Adapter).Append("(nint address")
                    .Append(string.Concat(signature.Parameters.Select((p, i) => $", {TypeName(p.Type)} argument{i}"))).AppendLine(");").AppendLine();
            }
            foreach (var address in functionAddresses.Where(a => a.HostEntry is not null))
            {
                var pointer = FunctionPointerType(address.Function.Signature);
                methods.Append("    [DllImport(\"").Append(NativeAbiLibraryName).Append("\", EntryPoint = \"").Append(address.HostRegistration)
                    .AppendLine("\", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]");
                methods.Append("    private static extern void ").Append(address.HostRegistration).Append('(').Append(pointer).AppendLine(" callback);").AppendLine();
                initialization.Line($"{Owner}.{address.HostRegistration}(({pointer})&{address.Wrapper});");
            }
        }

        private void EmitBoundaryBody(CodeWriter writer, string resultType, Action body)
        {
            if (!HostProtocolEnabled) { body(); return; }
            writer.Line("try"); writer.Open(); body(); writer.Close();
            writer.Line("catch (Exception exception)"); writer.Open();
            writer.Line("Environment.FailFast(\"An exception reached an unsupported native callback boundary.\", exception);");
            writer.Line(resultType == "void" ? "return;" : "return default;");
            writer.Close();
        }

        private void AppendHostProtocolPreamble(StringBuilder source)
        {
            if (!HostProtocolEnabled) return;
            source.AppendLine("#include <errno.h>");
            AppendHostLayoutAssertions(source);
            source.AppendLine("static _Thread_local int __roslynobj_application_errno;")
                .Append("ABI_EXPORT int *").Append(logicalErrnoEntry).AppendLine("(void) {")
                .AppendLine("    return &__roslynobj_application_errno;\n}\n");
        }

        private void AppendHostOutgoingStart(StringBuilder source)
        {
            if (!HostProtocolEnabled) return;
            source.Append("    int *const __application_error = ").Append(logicalErrnoEntry).AppendLine("();")
                .AppendLine("    int *const __native_error = __errno_location();")
                .AppendLine("    *__native_error = *__application_error;");
        }
        private void AppendHostOutgoingEnd(StringBuilder source)
        {
            if (!HostProtocolEnabled) return;
            source.AppendLine("    int const __captured_error = *__native_error;")
                .AppendLine("    *__application_error = __captured_error;");
        }
        private void AppendHostIncomingStart(StringBuilder source)
        {
            if (!HostProtocolEnabled) return;
            // Snapshot before TLS allocation/resolution or any managed entry.
            source.AppendLine("    int *const __native_error = __errno_location();")
                .AppendLine("    int const __incoming_error = *__native_error;")
                .Append("    int *const __application_error = ").Append(logicalErrnoEntry).AppendLine("();")
                .AppendLine("    *__application_error = __incoming_error;");
        }
        private void AppendHostIncomingEnd(StringBuilder source)
        {
            if (!HostProtocolEnabled) return;
            source.AppendLine("    int const __outgoing_error = *__application_error;")
                .AppendLine("    *__native_error = __outgoing_error;");
        }

        private void AppendHostNativeCalls(StringBuilder source)
        {
            if (!HostProtocolEnabled) return;
            foreach (var call in hostNativeCalls)
            {
                var signature = call.Signature;
                var result = NativeScalarType(signature.ReturnType);
                var parameterTypes = signature.Parameters.Select(p => NativeScalarType(p.Type));
                var pointer = call.Adapter + "_type";
                source.Append("typedef ").Append(result).Append(" (*").Append(pointer).Append(")(")
                    .Append(string.Join(", ", parameterTypes.DefaultIfEmpty("void"))).AppendLine(");");
                source.Append("ABI_EXPORT ").Append(result).Append(' ').Append(call.Adapter).Append("(void *address")
                    .Append(string.Concat(signature.Parameters.Select((p, i) => $", {NativeScalarType(p.Type)} argument{i}"))).AppendLine(") {");
                AppendHostOutgoingStart(source);
                source.Append("    ").Append(result == "void" ? "" : result + " result = ").Append("((").Append(pointer).Append(")address)(")
                    .Append(string.Join(", ", signature.Parameters.Select((_, i) => $"argument{i}"))).AppendLine(");");
                AppendHostOutgoingEnd(source);
                if (result != "void") source.AppendLine("    return result;");
                source.AppendLine("}\n");
            }
            foreach (var address in functionAddresses.Where(a => a.HostEntry is not null))
            {
                var signature = address.Function.Signature;
                var result = NativeScalarType(signature.ReturnType);
                var bridge = address.Wrapper!;
                source.Append("typedef ").Append(result).Append(" (*").Append(bridge).Append("_type)(")
                    .Append(string.Join(", ", signature.Parameters.Select(p => NativeScalarType(p.Type)).DefaultIfEmpty("void"))).AppendLine(");");
                source.Append("static ").Append(bridge).Append("_type ").Append(bridge).AppendLine("_callback;");
                source.Append("ABI_EXPORT void ").Append(address.HostRegistration).Append('(').Append(bridge).AppendLine("_type callback) {")
                    .Append("    ").Append(bridge).AppendLine("_callback = callback;\n}");
                source.Append("ABI_EXPORT ").Append(result).Append(' ').Append(address.HostEntry).Append('(')
                    .Append(string.Join(", ", signature.Parameters.Select((p, i) => $"{NativeScalarType(p.Type)} argument{i}").DefaultIfEmpty("void"))).AppendLine(") {");
                AppendHostIncomingStart(source);
                source.Append("    if (!").Append(bridge).AppendLine("_callback) abort();");
                source.Append("    ").Append(result == "void" ? "" : result + " result = ").Append(bridge).Append("_callback(")
                    .Append(string.Join(", ", signature.Parameters.Select((_, i) => $"argument{i}"))).AppendLine(");");
                AppendHostIncomingEnd(source);
                if (result != "void") source.AppendLine("    return result;");
                source.AppendLine("}\n");
            }
        }
    }
}
