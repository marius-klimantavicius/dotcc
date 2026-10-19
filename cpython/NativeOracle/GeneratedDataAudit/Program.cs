using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;

// Inspect an actual Release PE, without loading/executing CPython or relying on
// source syntax. A separately built probe measures access/copy under JIT/AOT.
if (args.Length != 3) throw new ArgumentException("assembly manifest output.json");
using var input = JsonDocument.Parse(File.ReadAllText(args[1]));
if (input.RootElement.TryGetProperty("schemaVersion", out var schema) && schema.GetInt32() == 2)
{
    TypedAudit.Run(args, input.RootElement);
    return;
}
var expected = input.RootElement.GetProperty("properties").EnumerateArray()
    .ToDictionary(property => "get_" + property.GetProperty("name").GetString()!,
        property => (Length: property.GetProperty("length").GetInt32(), Sha256: property.GetProperty("sha256").GetString()!), StringComparer.Ordinal);
using var pe = new PEReader(File.OpenRead(args[0]));
var metadata = pe.GetMetadataReader();
var opcodes = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
    .Where(field => field.FieldType == typeof(OpCode)).Select(field => (OpCode)field.GetValue(null)!)
    .ToDictionary(code => unchecked((ushort)code.Value));
var getters = new Dictionary<int, (string Name, int Calls)>();
var observations = new List<object>();
foreach (var handle in metadata.MethodDefinitions)
{
    var method = metadata.GetMethodDefinition(handle);
    var name = metadata.GetString(method.Name);
    if (!expected.TryGetValue(name, out var wanted)) continue;
    if (method.RelativeVirtualAddress == 0) throw new InvalidDataException("getter has no IL: " + name);
    var il = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes()!;
    var instructions = Decode(il).ToArray();
    if (instructions.Any(instruction => instruction.Code == OpCodes.Newarr))
        throw new InvalidDataException("temporary array in " + name);
    var calls = instructions.Where(instruction => instruction.Code == OpCodes.Call || instruction.Code == OpCodes.Callvirt || instruction.Code == OpCodes.Newobj)
        .Select(instruction => MethodName(MetadataTokens.EntityHandle(instruction.Token))).ToArray();
    if (calls.Any(call => call is not ("RuntimeHelpers.CreateSpan" or "RuntimeHelpers.CreateReadOnlySpan" or "ReadOnlySpan`1..ctor")))
        throw new InvalidDataException("unexamined getter call: " + name + " " + string.Join(",", calls));
    var data = instructions.Where(instruction => instruction.Code == OpCodes.Ldtoken || instruction.Code == OpCodes.Ldsflda)
        .Where(instruction => (instruction.Token & unchecked((int)0xff000000)) == 0x04000000)
        .Select(instruction => metadata.GetFieldDefinition(MetadataTokens.FieldDefinitionHandle(instruction.Token & 0xffffff)))
        .Where(field => field.GetRelativeVirtualAddress() != 0)
        .Select(field => (Rva: field.GetRelativeVirtualAddress(), Bytes: pe.GetSectionData(field.GetRelativeVirtualAddress()).GetContent(0, wanted.Length).ToArray())).ToArray();
    var exact = data.Where(field => Convert.ToHexString(SHA256.HashData(field.Bytes)).Equals(wanted.Sha256, StringComparison.OrdinalIgnoreCase)).ToArray();
    if (exact.Length != 1) throw new InvalidDataException("getter does not reference exact RVA bytes: " + name);
    if (!getters.TryAdd(MetadataTokens.GetToken(handle), (name, 0))) throw new InvalidDataException("duplicate getter");
    observations.Add(new { name, ilBytes = il.Length, newarr = 0, calls, rva = exact[0].Rva, length = wanted.Length, sha256 = wanted.Sha256 });
}
if (getters.Count != expected.Count || getters.Count == 0) throw new InvalidDataException("missing constant properties");
foreach (var handle in metadata.MethodDefinitions)
{
    var method = metadata.GetMethodDefinition(handle);
    if (method.RelativeVirtualAddress == 0) continue;
    foreach (var instruction in Decode(pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes()!))
        if (instruction.Code == OpCodes.Call && getters.TryGetValue(instruction.Token, out var getter))
            getters[instruction.Token] = (getter.Name, getter.Calls + 1);
}
if (getters.Values.Any(getter => getter.Calls == 0)) throw new InvalidDataException("unreferenced payload getter");
var report = new {
    status = "passed", assembly = Path.GetFullPath(args[0]), assemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[0]))),
    manifest = Path.GetFullPath(args[1]), manifestSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[1]))),
    propertyCount = getters.Count, newarr = 0, rvaBytes = expected.Values.Sum(value => (long)value.Length),
    getterCallSites = getters.Values.Sum(getter => getter.Calls), properties = observations,
    scope = "actual Release IL/RVA/getter-call-site proof; runtime allocation and full interpreter execution are separate"
};
File.WriteAllText(args[2], JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"{getters.Count} getters, {report.getterCallSites} call sites, {report.rvaBytes} RVA bytes, no newarr");

string MethodName(EntityHandle handle)
{
    if (handle.Kind == HandleKind.MethodSpecification) return MethodName(metadata.GetMethodSpecification((MethodSpecificationHandle)handle).Method);
    if (handle.Kind == HandleKind.MemberReference)
    {
        var member = metadata.GetMemberReference((MemberReferenceHandle)handle);
        var owner = member.Parent.Kind switch {
            HandleKind.TypeReference => metadata.GetString(metadata.GetTypeReference((TypeReferenceHandle)member.Parent).Name),
            // Only a ReadOnlySpan<byte> value constructor is allowed above. Its
            // generic TypeSpec has ELEMENT_TYPE_GENERICINST, VALUETYPE, TypeDefOrRef.
            HandleKind.TypeSpecification => TypeSpecName((TypeSpecificationHandle)member.Parent),
            _ => "?"
        };
        return owner + "." + metadata.GetString(member.Name);
    }
    return "?";
}
string TypeSpecName(TypeSpecificationHandle handle)
{
    var reader = metadata.GetBlobReader(metadata.GetTypeSpecification(handle).Signature);
    if (reader.ReadByte() != 0x15 || reader.ReadByte() != 0x11) return "?";
    var type = reader.ReadTypeHandle();
    return type.Kind == HandleKind.TypeReference ? metadata.GetString(metadata.GetTypeReference((TypeReferenceHandle)type).Name) : "?";
}
IEnumerable<(OpCode Code, int Token)> Decode(byte[] il)
{
    for (int offset = 0; offset < il.Length;)
    {
        ushort value = il[offset++];
        if (value == 0xfe) value = (ushort)(0xfe00 | il[offset++]);
        var code = opcodes[value];
        int size = code.OperandType switch {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, offset),
            _ => 4
        };
        yield return (code, size == 4 ? BitConverter.ToInt32(il, offset) : 0);
        offset += size;
    }
}
