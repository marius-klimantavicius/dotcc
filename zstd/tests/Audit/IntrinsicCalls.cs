using System.Buffers.Binary;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.RegularExpressions;

// Scan all translated IL for stale direct calls; positive BCL evidence is
// restricted to algorithm callers. Address/export wrappers retain identity.
internal static class IntrinsicCalls
{
    private const string ProductType = "Managed.Zstd.Native.Zstd";
    private const string AdapterType = "Managed.Zstd.Optimization.Primitives";
    private static readonly Regex UnitSuffix = new("__unit_[0-9A-Fa-f]{64}$");
    private static readonly Dictionary<ushort, OpCode> Opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.FieldType == typeof(OpCode)).Select(field => (OpCode)field.GetValue(null)!)
        .GroupBy(code => unchecked((ushort)code.Value)).ToDictionary(group => group.Key, group => group.First());

    internal static object Read(PEReader pe, MetadataReader metadata, string contract)
    {
        using var json = JsonDocument.Parse(File.ReadAllText(contract));
        var names = new HashSet<string>(); var intrinsicNames = new HashSet<string>(); var adapterNames = new HashSet<string>();
        foreach (var rule in json.RootElement.GetProperty("functionOverrides").EnumerateArray())
        {
            string name = rule.GetProperty("name").GetString()!;
            if (!names.Add(name)) throw new InvalidDataException("Duplicate primitive audit contract");
            string? kind = rule.GetProperty("target").GetProperty("kind").GetString();
            if (kind == "intrinsic") intrinsicNames.Add(name);
            else if (kind == "managedMethod") adapterNames.Add(name);
            else throw new InvalidDataException("Unexpected primitive target kind");
        }
        if (intrinsicNames.Count != 40 || !adapterNames.SetEquals(["MEM_readLE24", "MEM_writeLE24"]))
            throw new InvalidDataException("Expected 40 direct intrinsic contracts and two LE24 adapters");
        var groups = new SortedDictionary<string, int>();
        var evidence = new Dictionary<(string Caller, string Target, string Generic, string Opcode), (int Count, int Offset)>();
        var wrappers = new List<string>();
        var addresses = new List<object>();
        int algorithms = 0, scanned = 0;
        foreach (var handle in metadata.MethodDefinitions)
        {
            var method = metadata.GetMethodDefinition(handle);
            if (TypeName(metadata, method.GetDeclaringType()) != ProductType) continue;
            string caller = metadata.GetString(method.Name), sourceName = UnitSuffix.Replace(caller, "");
            if (names.Contains(sourceName)) { wrappers.Add(caller); continue; }
            if (method.RelativeVirtualAddress == 0) continue;
            bool algorithm = Algorithm(sourceName);
            scanned++;
            if (algorithm) algorithms++;
            byte[] bytes = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes() ?? [];
            int position = 0;
            while (position < bytes.Length)
            {
                int offset = position;
                ushort value = bytes[position++];
                if (value == 0xfe)
                {
                    if (position >= bytes.Length) throw new InvalidDataException("Truncated IL opcode");
                    value = (ushort)(0xfe00 | bytes[position++]);
                }
                if (!Opcodes.TryGetValue(value, out var opcode)) throw new InvalidDataException("Unknown IL opcode");
                int length = OperandLength(opcode.OperandType, bytes, position);
                if (length > bytes.Length - position) throw new InvalidDataException("Truncated IL operand");
                bool call = opcode == OpCodes.Call || opcode == OpCodes.Callvirt;
                bool field = opcode == OpCodes.Ldsfld || opcode == OpCodes.Ldfld;
                bool address = opcode == OpCodes.Ldftn || opcode == OpCodes.Ldvirtftn;
                if (call || field || address)
                {
                    int token = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(position, 4));
                    var target = Member(metadata, MetadataTokens.EntityHandle(token));
                    if (call && target.Owner == ProductType && intrinsicNames.Contains(UnitSuffix.Replace(target.Name, "")))
                        throw new InvalidDataException("Translated method calls retained intrinsic wrapper: " + caller + " -> " + target.Name);
                    if (call && target.Owner == AdapterType && !adapterNames.Contains(target.Name))
                        throw new InvalidDataException("Translated method calls obsolete authored primitive: " + caller + " -> " + target.Name);
                    if (address && target.Owner == ProductType && names.Contains(UnitSuffix.Replace(target.Name, "")))
                        addresses.Add(new { caller, wrapper = target.Name, opcode = opcode.Name, il_offset = offset });
                    string? group = address || !algorithm ? null : Group(target.Owner, target.Name, field);
                    if (group != null)
                    {
                        groups[group] = groups.GetValueOrDefault(group) + 1;
                        var key = (caller, target.Owner + "." + target.Name, target.Generic, opcode.Name!);
                        var previous = evidence.GetValueOrDefault(key);
                        evidence[key] = (previous.Count + 1, previous.Count == 0 ? offset : previous.Offset);
                    }
                }
                position += length;
            }
        }
        string[] required = ["native-load", "little-endian-load", "little-endian-store", "byte-swap", "leading-zero-count", "trailing-zero-count", "runtime-endian"];
        if (algorithms == 0 || required.Any(group => groups.GetValueOrDefault(group) == 0))
            throw new InvalidDataException("Direct BCL operations missing from actual algorithm callers: " + string.Join(", ", required.Where(group => groups.GetValueOrDefault(group) == 0)));
        return new
        {
            status = "passed", scanned_translated_methods = scanned, algorithm_methods = algorithms,
            excluded_primitive_wrapper_definitions = wrappers.Count,
            excluded_primitive_wrapper_identities = wrappers.Order(StringComparer.Ordinal).ToArray(), retained_wrapper_addresses = addresses,
            stale_intrinsic_wrapper_calls = 0, required_groups = required, caller_operation_counts = groups,
            caller_evidence = evidence.OrderBy(pair => pair.Key.Caller).ThenBy(pair => pair.Key.Target)
                .Select(pair => new { caller = pair.Key.Caller, target = pair.Key.Target, generic_signature_hex = pair.Key.Generic,
                    opcode = pair.Key.Opcode, count = pair.Value.Count, first_il_offset = pair.Value.Offset }).ToArray(),
            scope = "All non-wrapper translated methods scanned for stale direct calls; positive BCL evidence restricted to algorithm call/callvirt and field reads; wrapper bodies and addresses never satisfy evidence"
        };
    }

    private static bool Algorithm(string name) => new[] { "ZSTD_", "ZDICT_", "BIT_", "FSE_", "HUF_", "HIST_", "XXH", "COVER_", "FASTCOVER_" }
        .Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal));

    private static string? Group(string owner, string name, bool field)
    {
        if (field) return owner == "System.BitConverter" && name == "IsLittleEndian" ? "runtime-endian" : null;
        if (owner == "System.Runtime.CompilerServices.Unsafe")
            return name is "ReadUnaligned" ? "native-load" : name is "WriteUnaligned" ? "native-store" : null;
        if (owner == "System.Numerics.BitOperations")
            return name is "LeadingZeroCount" ? "leading-zero-count" : name is "TrailingZeroCount" ? "trailing-zero-count" : null;
        if (owner != "System.Buffers.Binary.BinaryPrimitives") return null;
        if (name == "ReverseEndianness") return "byte-swap";
        if (name.StartsWith("ReadUInt", StringComparison.Ordinal)) return name.EndsWith("LittleEndian", StringComparison.Ordinal) ? "little-endian-load" : name.EndsWith("BigEndian", StringComparison.Ordinal) ? "big-endian-load" : null;
        if (name.StartsWith("WriteUInt", StringComparison.Ordinal)) return name.EndsWith("LittleEndian", StringComparison.Ordinal) ? "little-endian-store" : name.EndsWith("BigEndian", StringComparison.Ordinal) ? "big-endian-store" : null;
        return null;
    }

    private static int OperandLength(OperandType kind, byte[] bytes, int offset) => kind switch
    {
        OperandType.InlineNone => 0,
        OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
        OperandType.InlineVar => 2,
        OperandType.InlineBrTarget or OperandType.InlineField or OperandType.InlineI or OperandType.InlineMethod
            or OperandType.InlineSig or OperandType.InlineString or OperandType.InlineTok or OperandType.InlineType or OperandType.ShortInlineR => 4,
        OperandType.InlineI8 or OperandType.InlineR => 8,
        OperandType.InlineSwitch => offset <= bytes.Length - 4 && BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(offset, 4)) is >= 0 and var count
            ? checked(4 + count * 4) : throw new InvalidDataException("Invalid IL switch"),
        _ => throw new InvalidDataException("Unsupported IL operand")
    };

    private static (string Owner, string Name, string Generic) Member(MetadataReader metadata, EntityHandle handle)
    {
        if (handle.Kind == HandleKind.MethodSpecification)
        {
            var specification = metadata.GetMethodSpecification((MethodSpecificationHandle)handle);
            var result = Member(metadata, specification.Method);
            return (result.Owner, result.Name, Convert.ToHexStringLower(metadata.GetBlobBytes(specification.Signature)));
        }
        if (handle.Kind == HandleKind.MethodDefinition)
        {
            var method = metadata.GetMethodDefinition((MethodDefinitionHandle)handle);
            return (TypeName(metadata, method.GetDeclaringType()), metadata.GetString(method.Name), "");
        }
        if (handle.Kind == HandleKind.MemberReference)
        {
            var member = metadata.GetMemberReference((MemberReferenceHandle)handle);
            return (TypeName(metadata, member.Parent), metadata.GetString(member.Name), "");
        }
        return ("", "", "");
    }

    private static string TypeName(MetadataReader metadata, EntityHandle handle)
    {
        if (handle.Kind == HandleKind.TypeDefinition)
        {
            var type = metadata.GetTypeDefinition((TypeDefinitionHandle)handle);
            string name = metadata.GetString(type.Name); var parent = type.GetDeclaringType();
            if (!parent.IsNil) return TypeName(metadata, parent) + "+" + name;
            string ns = metadata.GetString(type.Namespace); return ns.Length == 0 ? name : ns + "." + name;
        }
        if (handle.Kind == HandleKind.TypeReference)
        {
            var type = metadata.GetTypeReference((TypeReferenceHandle)handle);
            string ns = metadata.GetString(type.Namespace), name = metadata.GetString(type.Name);
            return ns.Length == 0 ? name : ns + "." + name;
        }
        return "";
    }
}
