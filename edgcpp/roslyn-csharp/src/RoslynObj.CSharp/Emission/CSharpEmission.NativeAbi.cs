using System.Text;
using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private const string NativeAbiLibraryName = "RoslynObj.Generated.Abi";
        private string? generatedAbiLibrary;

        private string RequireGeneratedAbiLibrary()
        {
            if (generatedAbiLibrary is null)
            {
                if (nativeLibraries.ContainsKey(NativeAbiLibraryName))
                    throw Limit("The generated ABI provider name is reserved and cannot alias an explicit native library binding.", "native-abi");
                generatedAbiLibrary = InternalName("__generatedAbiLibrary");
                nativeLibraries.Add(NativeAbiLibraryName, generatedAbiLibrary);
            }
            return generatedAbiLibrary;
        }

        private void EmitVariadicEntryBridges(CodeWriter initialization)
        {
            foreach (var address in functionAddresses.Where(a => a.VariadicEntry is not null))
            {
                var signature = address.Function.Signature;
                var formals = signature.Parameters.Select((p, i) => $"{TypeName(p.Type)} argument{i}").Append($"{VaType}* cursor");
                var bridgeType = "delegate* unmanaged[Cdecl]<" + string.Join(", ", signature.Parameters.Select(p => TypeName(p.Type)).Append(VaType + "*").Append(TypeName(signature.ReturnType))) + ">";
                var writer = new CodeWriter();
                writer.Line($"[DllImport(\"{NativeAbiLibraryName}\", EntryPoint = \"{address.VariadicRegistration}\", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]");
                writer.Line($"private static extern void {address.VariadicRegistration}({bridgeType} bridge);");
                writer.Line();
                writer.Line("[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]");
                writer.Line($"private static {TypeName(signature.ReturnType)} {address.VariadicBridge}({string.Join(", ", formals)})");
                writer.Open();
                var arguments = signature.Parameters.Select((_, i) => $"argument{i}").Append("cursor");
                writer.Line($"{(TypeName(signature.ReturnType) == "void" ? "" : "return ")}{OwnedName(address.Function)}({string.Join(", ", arguments)});");
                writer.Close();
                methods.Append(writer.TextWithIndent(1)).AppendLine();
                initialization.Line($"{Owner}.{address.VariadicRegistration}(({bridgeType})&{address.VariadicBridge});");
            }
        }

        private string NativeAbiSource()
        {
            var source = new StringBuilder("/* Generated concrete ABI forwarding only. Application bodies remain C#. */\n#include <stdarg.h>\n#include <stddef.h>\n#include <stdint.h>\n#include <stdlib.h>\n\n#if !defined(__linux__) || !defined(__x86_64__)\n#error This ABI provider requires Linux x64.\n#endif\n_Static_assert(sizeof(void *) == 8, \"64-bit native pointers required\");\n_Static_assert(sizeof(va_list) == 24 && _Alignof(va_list) == 8, \"SysV AMD64 cursor storage required\");\n#define ABI_EXPORT __attribute__((visibility(\"default\")))\n\n");
            foreach (var address in functionAddresses.Where(a => a.VariadicEntry is not null))
            {
                var signature = address.Function.Signature;
                var result = NativeScalarType(signature.ReturnType);
                var fixedTypes = signature.Parameters.Select(p => NativeScalarType(p.Type)).ToArray();
                var formals = fixedTypes.Select((t, i) => $"{t} argument{i}").ToArray();
                var bridge = address.VariadicBridge!;
                source.Append("typedef ").Append(result).Append(" (*").Append(bridge).Append("_type)(")
                    .Append(string.Join(", ", fixedTypes.Append("void *"))).AppendLine(");");
                source.Append("static ").Append(bridge).Append("_type ").Append(bridge).AppendLine("_callback;");
                source.Append("ABI_EXPORT void ").Append(address.VariadicRegistration).Append('(').Append(bridge).AppendLine("_type callback) {")
                    .Append("    ").Append(bridge).AppendLine("_callback = callback;\n}");
                source.Append("ABI_EXPORT ").Append(result).Append(' ').Append(address.VariadicEntry).Append('(')
                    .Append(string.Join(", ", formals)).AppendLine(", ...) {");
                source.Append("    if (!").Append(bridge).AppendLine("_callback) abort();");
                source.Append("    va_list cursor;\n    va_start(cursor, argument").Append(formals.Length - 1).AppendLine(");");
                source.Append("    ");
                if (result != "void") source.Append(result).Append(" result = ");
                source.Append(bridge).Append("_callback(").Append(string.Join(", ", signature.Parameters.Select((_, i) => $"argument{i}").Append("(void *)&cursor"))).AppendLine(");");
                source.AppendLine("    va_end(cursor);");
                if (result != "void") source.AppendLine("    return result;");
                source.AppendLine("}\n");
            }
            foreach (var call in variadicCalls.Where(c => c.NativeAdapter is not null))
            {
                var result = NativeScalarType(call.Signature.ReturnType);
                var prefix = call.Signature.Parameters.Select(p => NativeScalarType(p.Type));
                var type = call.NativeAdapter + "_type";
                source.Append("typedef ").Append(result).Append(" (*").Append(type).Append(")(").Append(string.Join(", ", prefix)).AppendLine(", ...);");
                source.Append("ABI_EXPORT ").Append(result).Append(' ').Append(call.NativeAdapter).Append("(void *address")
                    .Append(string.Concat(call.ArgumentTypes.Select((t, i) => $", {NativeScalarType(t)} argument{i}"))).AppendLine(") {");
                source.Append("    ").Append(result == "void" ? "" : "return ").Append("((").Append(type).Append(")address)(")
                    .Append(string.Join(", ", call.ArgumentTypes.Select((_, i) => $"argument{i}"))).AppendLine(");\n}\n");
            }
            AppendAggregateNativeSource(source);
            return source.ToString();
        }

        private string NativeScalarType(CppType type)
        {
            ValidateNativeScalarType(type, true);
            if (Unwrap(type) is PointerType) return "void *";
            return TypeName(type) switch
            {
                "void" => "void", "sbyte" => "int8_t", "byte" => "uint8_t", "short" => "int16_t", "ushort" => "uint16_t",
                "int" => "int32_t", "uint" => "uint32_t", "long" => "int64_t", "ulong" => "uint64_t", "float" => "float", "double" => "double",
                _ => throw Limit("A native ABI forwarding scalar has no captured exact C representation.", "native-abi")
            };
        }

        private string NativeAbiProjectItems() => generatedAbiLibrary is null ? "" : """
          <PropertyGroup>
            <RoslynObjAbiCompiler Condition="'$(RoslynObjAbiCompiler)' == ''">cc</RoslynObjAbiCompiler>
          </PropertyGroup>
          <ItemGroup>
            <Content Include="libRoslynObj.Generated.Abi.so" CopyToOutputDirectory="PreserveNewest" CopyToPublishDirectory="PreserveNewest" />
          </ItemGroup>
          <Target Name="BuildRoslynObjAbi" BeforeTargets="PrepareForBuild" Inputs="$(MSBuildProjectDirectory)/Generated.Abi.c" Outputs="$(MSBuildProjectDirectory)/libRoslynObj.Generated.Abi.so">
            <Error Condition="'$(OS)' != 'Unix' Or ('$(RuntimeIdentifier)' != '' And '$(RuntimeIdentifier)' != 'linux-x64')" Text="Generated ABI adapters require Linux x64." />
            <Exec Command="&quot;$(RoslynObjAbiCompiler)&quot; -std=c17 -O2 -fPIC -fvisibility=hidden -shared &quot;$(MSBuildProjectDirectory)/Generated.Abi.c&quot; -o &quot;$(MSBuildProjectDirectory)/libRoslynObj.Generated.Abi.so&quot;" />
          </Target>
        """ + "\n";
    }
}
