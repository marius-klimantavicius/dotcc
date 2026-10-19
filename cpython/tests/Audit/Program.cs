using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;

if (args.Length != 2) throw new ArgumentException("Expected actual assembly and JSON report paths.");
string path = Path.GetFullPath(args[0]);
using var stream = File.OpenRead(path);
using var pe = new PEReader(stream);
if (!pe.HasMetadata) throw new BadImageFormatException("Managed assembly metadata is required: " + path);
var reader = pe.GetMetadataReader();
string assemblyName = reader.GetString(reader.GetAssemblyDefinition().Name);
var references = reader.AssemblyReferences.Select(handle => reader.GetString(reader.GetAssemblyReference(handle).Name)).Order().ToArray();
var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
    .Where(field => field.FieldType == typeof(OpCode))
    .Select(field => (OpCode)field.GetValue(null)!).ToDictionary(opcode => unchecked((ushort)opcode.Value));
var imports = new List<object>();
var methods = new List<object>();
var types = new List<string>();
var calls = new List<object>();
var fields = new List<object>();
var valueTypes = new List<string>();
foreach (var typeHandle in reader.TypeDefinitions)
{
    var type = reader.GetTypeDefinition(typeHandle);
    string callerType = TypeName(typeHandle);
    types.Add(callerType);
    if (!type.BaseType.IsNil && TypeName(type.BaseType) is "System.ValueType" or "System.Enum") valueTypes.Add(callerType);
    foreach (var fieldHandle in type.GetFields())
    {
        var field = reader.GetFieldDefinition(fieldHandle);
        var constantHandle = field.GetDefaultValue();
        object? constant = null;
        if (!constantHandle.IsNil)
        {
            var value = reader.GetConstant(constantHandle);
            constant = new { type_code = (int)value.TypeCode,
                value_hex = Convert.ToHexString(reader.GetBlobBytes(value.Value)) };
        }
        fields.Add(new { type = callerType, name = reader.GetString(field.Name), attributes = (int)field.Attributes,
            signature = Convert.ToHexString(reader.GetBlobBytes(field.Signature)), constant });
    }
    foreach (var methodHandle in type.GetMethods())
    {
        var method = reader.GetMethodDefinition(methodHandle);
        string methodName = reader.GetString(method.Name);
        var body = method.RelativeVirtualAddress == 0 ? null : pe.GetMethodBody(method.RelativeVirtualAddress);
        int localCount = 0;
        if (body != null && !body.LocalSignature.IsNil)
        {
            var signature = reader.GetBlobReader(reader.GetStandaloneSignature(body.LocalSignature).Signature);
            signature.ReadSignatureHeader();
            localCount = signature.ReadCompressedInteger();
        }
        int newarrCount = 0;
        var methodCalls = new List<object>();
        var callSites = new List<object>();
        var fieldSites = new List<object>();
        var indirectCalls = new List<object>();
        int methodIndex = methods.Count;
        methods.Add(new { type = callerType, name = methodName, attributes = (int)method.Attributes,
            signature = Convert.ToHexString(reader.GetBlobBytes(method.Signature)),
            il_bytes = body?.GetILBytes()?.Length ?? 0, max_stack = body?.MaxStack ?? 0,
            local_count = localCount, exception_regions = body?.ExceptionRegions.Length ?? 0 });
        if ((method.Attributes & MethodAttributes.PinvokeImpl) != 0)
        {
            var import = method.GetImport();
            imports.Add(new { type = callerType, method = methodName,
                library = reader.GetString(reader.GetModuleReference(import.Module).Name),
                symbol = reader.GetString(import.Name), attributes = (int)import.Attributes });
        }
        if (body == null) continue;
        byte[] bytes = body.GetILBytes()!;
        var seen = new HashSet<int>();
        for (int offset = 0; offset < bytes.Length;)
        {
            ushort value = bytes[offset++];
            if (value == 0xfe) value = (ushort)(0xfe00 | bytes[offset++]);
            if (!opcodes.TryGetValue(value, out var opcode)) throw new BadImageFormatException("Unknown IL opcode");
            int width = opcode.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => checked(4 + 4 * BitConverter.ToInt32(bytes, offset)),
                _ => 4
            };
            if (offset + width > bytes.Length) throw new BadImageFormatException("Truncated IL operand");
            if (opcode == OpCodes.Newarr) newarrCount++;
            if (opcode == OpCodes.Calli)
            {
                var signature = reader.GetStandaloneSignature(MetadataTokens.StandaloneSignatureHandle(BitConverter.ToInt32(bytes, offset) & 0xffffff));
                indirectCalls.Add(new { offset = offset - opcode.Size,
                    signature = Convert.ToHexString(reader.GetBlobBytes(signature.Signature)) });
            }
            if (opcode.OperandType == OperandType.InlineField)
            {
                var fieldOperand = MetadataTokens.EntityHandle(BitConverter.ToInt32(bytes, offset));
                if (fieldOperand.Kind == HandleKind.FieldDefinition)
                {
                    var field = reader.GetFieldDefinition((FieldDefinitionHandle)fieldOperand);
                    fieldSites.Add(new { offset = offset - opcode.Size, opcode = opcode.Name,
                        target_type = TypeName(field.GetDeclaringType()), target_field = reader.GetString(field.Name) });
                }
            }
            if (opcode.OperandType == OperandType.InlineMethod)
            {
                int token = BitConverter.ToInt32(bytes, offset);
                var definition = MetadataTokens.EntityHandle(token);
                while (definition.Kind == HandleKind.MethodSpecification)
                    definition = reader.GetMethodSpecification((MethodSpecificationHandle)definition).Method;
                var resolved = Method(definition);
                callSites.Add(new { offset = offset - opcode.Size, opcode = opcode.Name,
                    target_token = definition.Kind == HandleKind.MethodDefinition ? MetadataTokens.GetToken(definition) : 0,
                    target_type = definition.Kind == HandleKind.MemberReference
                        ? new MetadataTypes().Name(reader, reader.GetMemberReference((MemberReferenceHandle)definition).Parent)
                        : resolved.Type,
                    target_method = resolved.Name });
                if (seen.Add(token))
                {
                    var target = Method(MetadataTokens.EntityHandle(token));
                    var edge = new { caller_type = callerType, caller_method = methodName,
                        target_type = target.Type, target_method = target.Name, target_assembly = target.Assembly };
                    calls.Add(edge);
                    methodCalls.Add(edge);
                }
            }
            offset += width;
        }
        // Independent getter/helper evidence supplements the RVA reader. The
        // actual owner/token/hash and every call site bind the copy closure.
        // All local copy/helper methods need independent IL and edge evidence.
        // Keep the legacy global calls representation unchanged for lifetime reviews.
            methods[methodIndex] = new { type = callerType, name = methodName, attributes = (int)method.Attributes,
                token = MetadataTokens.GetToken(methodHandle),
                signature = Convert.ToHexString(reader.GetBlobBytes(method.Signature)),
                il_bytes = bytes.Length, max_stack = body.MaxStack, local_count = localCount,
                exception_regions = body.ExceptionRegions.Length, newarr_count = newarrCount,
                il_sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(), calls = methodCalls,
                call_sites = callSites, field_sites = fieldSites, indirect_calls = indirectCalls };
    }
}
var result = new { scope = "static assembly metadata and IL calls; no interpreter behavior claim", path,
    sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant(),
    mvid = reader.GetGuid(reader.GetModuleDefinition().Mvid).ToString(),
    assembly = assemblyName, references, native_imports = imports, types, value_types = valueTypes, methods, fields, calls };
File.WriteAllText(args[1], JsonSerializer.Serialize(result));
Console.WriteLine($"{assemblyName}: {types.Count} types, {methods.Count} methods, {calls.Count} IL edges, {imports.Count} native imports");

string TypeName(EntityHandle handle) => handle.Kind switch
{
    HandleKind.TypeDefinition => DefinitionName((TypeDefinitionHandle)handle),
    HandleKind.TypeReference => ReferenceName((TypeReferenceHandle)handle),
    HandleKind.TypeSpecification => "<type-specification>",
    HandleKind.MethodDefinition => TypeName(reader.GetMethodDefinition((MethodDefinitionHandle)handle).GetDeclaringType()),
    _ => "<" + handle.Kind + ">"
};
string DefinitionName(TypeDefinitionHandle handle)
{
    var type = reader.GetTypeDefinition(handle);
    string name = reader.GetString(type.Name);
    var parent = type.GetDeclaringType();
    if (!parent.IsNil) return DefinitionName(parent) + "+" + name;
    string ns = reader.GetString(type.Namespace);
    return ns.Length == 0 ? name : ns + "." + name;
}
string ReferenceName(TypeReferenceHandle handle)
{
    var type = reader.GetTypeReference(handle);
    string name = reader.GetString(type.Name);
    if (type.ResolutionScope.Kind == HandleKind.TypeReference)
        return ReferenceName((TypeReferenceHandle)type.ResolutionScope) + "+" + name;
    string ns = reader.GetString(type.Namespace);
    return ns.Length == 0 ? name : ns + "." + name;
}
string Assembly(EntityHandle handle)
{
    if (handle.Kind == HandleKind.TypeReference)
        return Assembly(reader.GetTypeReference((TypeReferenceHandle)handle).ResolutionScope);
    return handle.Kind == HandleKind.AssemblyReference
        ? reader.GetString(reader.GetAssemblyReference((AssemblyReferenceHandle)handle).Name) : assemblyName;
}
(string Type, string Name, string Assembly) Method(EntityHandle handle)
{
    if (handle.Kind == HandleKind.MethodSpecification)
        return Method(reader.GetMethodSpecification((MethodSpecificationHandle)handle).Method);
    if (handle.Kind == HandleKind.MethodDefinition)
    {
        var definition = reader.GetMethodDefinition((MethodDefinitionHandle)handle);
        return (TypeName(definition.GetDeclaringType()), reader.GetString(definition.Name), assemblyName);
    }
    if (handle.Kind == HandleKind.MemberReference)
    {
        var member = reader.GetMemberReference((MemberReferenceHandle)handle);
        return (TypeName(member.Parent), reader.GetString(member.Name), Assembly(member.Parent));
    }
    throw new BadImageFormatException("Invalid method operand: " + handle.Kind);
}
