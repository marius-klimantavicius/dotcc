using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace DotObj;

internal sealed partial class Emitter
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private string LiteralComment(StorageSymbol symbol, long offset)
    {
        if (symbol.Storage.SourceName.Length != 0 || offset < 0) return "";
        var data = symbol.Object.StorageData.GetValueOrDefault(symbol.Storage.Id, []);
        if (data.Count == 0) return "";
        long size = symbol.Object.StorageSize(symbol.Object.Type(symbol.Storage.TypeId));
        if (offset >= size) return "";
        byte[] sample = new byte[(int)Math.Min(256, size - offset)];
        foreach (StorageData part in data)
        {
            long begin = Math.Max(offset, part.ByteOffset), end = Math.Min(offset + sample.Length, part.ByteOffset + part.Bytes.Length);
            if (end > begin) part.Bytes.AsSpan((int)(begin - part.ByteOffset), (int)(end - begin)).CopyTo(sample.AsSpan((int)(begin - offset)));
        }
        int zero = Array.IndexOf(sample, (byte)0); if (zero >= 0) sample = sample[..zero];
        try
        {
            string value = StrictUtf8.GetString(sample);
            if (value.Length > 80) value = value[..80] + "...";
            return " /* " + JsonSerializer.Serialize(value).Replace("*/", "* /", StringComparison.Ordinal) + " */";
        }
        catch (DecoderFallbackException) { return ""; }
    }
    private void EmitByteData(StringBuilder code, byte[] data, string pointer, long offset, int indent)
    {
        for (int start = 0; start < data.Length; start += 8192)
        {
            byte[] bytes = data.AsSpan(start, Math.Min(8192, data.Length - start)).ToArray();
            if (bytes.All(value => value == 0)) continue;
            string initializer;
            try { initializer = JsonSerializer.Serialize(StrictUtf8.GetString(bytes)) + "u8"; }
            catch (DecoderFallbackException) { initializer = "[" + string.Join(", ", bytes) + "]"; }
            string name = generatedMembers.Allocate("__dot_initializer_" + storageDataDeclarations.Count, "initializer");
            storageDataDeclarations.Add("\n    private static global::System.ReadOnlySpan<byte> " + name + " => " + initializer + ";\n");
            code.Append(' ', indent).Append(name).Append(".CopyTo(new global::System.Span<byte>(").Append(pointer).Append(" + ")
                .Append(offset + start).Append(", ").Append(bytes.Length).Append("));\n");
        }
    }
    private static string Integer128Words(string type, string text)
    {
        UInt128 bits = type == "i128" ? unchecked((UInt128)Int128.Parse(text, CultureInfo.InvariantCulture)) : UInt128.Parse(text, CultureInfo.InvariantCulture);
        return "0x" + unchecked((ulong)bits).ToString("x", CultureInfo.InvariantCulture) + "UL, 0x" + ((ulong)(bits >> 64)).ToString("x", CultureInfo.InvariantCulture) + "UL";
    }
    private string PhysicalConstant(string type, ReadOnlySpan<byte> bytes)
    {
        string text = type switch
        {
            "i8" => unchecked((sbyte)bytes[0]).ToString(CultureInfo.InvariantCulture), "u8" or "bool" => bytes[0].ToString(CultureInfo.InvariantCulture),
            "i16" => BinaryPrimitives.ReadInt16LittleEndian(bytes).ToString(CultureInfo.InvariantCulture),
            "u16" => BinaryPrimitives.ReadUInt16LittleEndian(bytes).ToString(CultureInfo.InvariantCulture),
            "i32" => BinaryPrimitives.ReadInt32LittleEndian(bytes).ToString(CultureInfo.InvariantCulture),
            "u32" => BinaryPrimitives.ReadUInt32LittleEndian(bytes).ToString(CultureInfo.InvariantCulture),
            "i128" => BinaryPrimitives.ReadInt128LittleEndian(bytes).ToString(CultureInfo.InvariantCulture),
            "u128" => BinaryPrimitives.ReadUInt128LittleEndian(bytes).ToString(CultureInfo.InvariantCulture),
            "i64" => BinaryPrimitives.ReadInt64LittleEndian(bytes).ToString(CultureInfo.InvariantCulture),
            "u64" => BinaryPrimitives.ReadUInt64LittleEndian(bytes).ToString(CultureInfo.InvariantCulture),
            "f32" => BinaryPrimitives.ReadUInt32LittleEndian(bytes).ToString("x8", CultureInfo.InvariantCulture),
            "f80" => BinaryPrimitives.ReadUInt16LittleEndian(bytes[8..]).ToString("x4", CultureInfo.InvariantCulture) + BinaryPrimitives.ReadUInt64LittleEndian(bytes).ToString("x16", CultureInfo.InvariantCulture),
            "f64" => BinaryPrimitives.ReadUInt64LittleEndian(bytes).ToString("x16", CultureInfo.InvariantCulture),
            _ => throw new InvalidDataException("Static initializer is not a numeric representation")
        };
        return ScalarTypes.Literal(type, text);
    }
    private bool TryReadableInitialization(StringBuilder code, StorageSymbol symbol, string pointer, int indent, Names locals)
    {
        ObjectFile obj = symbol.Object; Storage storage = symbol.Storage; string type = obj.Type(storage.TypeId);
        if (type == "f80") return false; // Preserve the exact static ten-byte value and six zero padding bytes.
        long size = obj.StorageSize(type);
        if (size > 64 * 1024 * 1024) return false;
        byte[] bytes = new byte[(int)size];
        foreach (StorageData part in obj.StorageData.GetValueOrDefault(storage.Id, [])) part.Bytes.CopyTo(bytes, part.ByteOffset);
        var relocations = obj.Relocations.GetValueOrDefault(storage.Id, []).ToDictionary(value => value.ByteOffset);
        bool AnyData(long offset, long length) => bytes.AsSpan((int)offset, (int)length).ContainsAnyExcept((byte)0) ||
            relocations.Keys.Any(position => position >= offset && position < offset + length);
        bool CanConstruct(string current, int depth = 0)
        {
            if (depth > 128) return false;
            if (current == "f80") return false;
            if (ScalarTypes.IsScalar(current)) return true;
            if (!ScalarTypes.IsRecord(current)) return false;
            RecordType record = obj.RecordsByKey[current];
            return record.Size > 0 && record.LayoutKind != "union" && obj.RecordFields(record.TypeId)
                .All(field => field.BitWidth is null && CanConstruct(obj.Type(field.TypeId), depth + 1));
        }
        string Value(string current, long offset, int level)
        {
            if (ScalarTypes.IsPointer(current))
            {
                if (relocations.TryGetValue(offset, out Relocation? relocation))
                {
                    if (relocation.TargetStorageId is long target) return "(" + Type(current) + ")" + StorageAddress(linker.ResolveStorage(obj, target), relocation.Addend);
                    Symbol function = linker.Resolve(obj, relocation.TargetEntityId!.Value);
                    return "(" + Type(current) + ")(" + FunctionPointerType(function.Object.EntityRoutine(function.Entity.Id)) + ")&" + FunctionReference(function);
                }
                ulong value = BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan((int)offset, 8));
                return value == 0 ? "null" : "(" + Type(current) + ")(nuint)0x" + value.ToString("x", CultureInfo.InvariantCulture) + "UL";
            }
            if (ScalarTypes.IsNumeric(current) || current == "bool") return PhysicalConstant(current, bytes.AsSpan((int)offset));
            RecordType record = obj.RecordsByKey[current]; var valueCode = new StringBuilder("new()\n").Append(' ', level).Append("{\n");
            foreach (RecordField field in obj.RecordFields(record.TypeId))
                valueCode.Append(' ', level + 4).Append(fieldNames[(current, field.Ordinal)]).Append(" = ")
                    .Append(Value(obj.Type(field.TypeId), offset + field.ByteOffset, level + 4)).Append(",\n");
            return valueCode.Append(' ', level).Append('}').ToString();
        }
        bool ExactConstruction(string current, long offset, HashSet<long> pointerSlots, bool[] covered)
        {
            if (ScalarTypes.IsScalar(current))
            {
                long extent = obj.StorageSize(current);
                if (relocations.Keys.Any(position => position < offset + extent && position + 8 > offset &&
                    !(ScalarTypes.IsPointer(current) && position == offset))) return false;
                if (ScalarTypes.IsPointer(current)) pointerSlots.Add(offset);
                Array.Fill(covered, true, (int)offset, (int)extent); return true;
            }
            if (!CanConstruct(current)) return false;
            return obj.RecordFields(obj.RecordsByKey[current].TypeId).All(field => ExactConstruction(obj.Type(field.TypeId), offset + field.ByteOffset, pointerSlots, covered));
        }
        bool ExactRepresentation(string current, long elements, long stride)
        {
            var covered = new bool[bytes.Length]; var pointers = new HashSet<long>();
            for (long index = 0; index < elements; index++) if (!ExactConstruction(current, index * stride, pointers, covered)) return false;
            return !bytes.Where((value, index) => value != 0 && !covered[index]).Any() && relocations.Keys.All(pointers.Contains);
        }
        if (ScalarTypes.IsScalar(type))
        {
            if (!ExactRepresentation(type, 1, size)) return false;
            if (AnyData(0, size)) code.Append(' ', indent).Append(StorageReference(symbol)).Append(" = ").Append(Value(type, 0, indent)).Append(";\n");
            return true;
        }
        if (ScalarTypes.IsRecord(type) && CanConstruct(type) && ExactRepresentation(type, 1, size))
        {
            if (AnyData(0, size)) code.Append(' ', indent).Append(StorageReference(symbol)).Append(" = ").Append(Value(type, 0, indent)).Append(";\n");
            return true;
        }
        if (!ScalarTypes.IsArray(type)) return false;
        string leaf = ScalarTypes.Leaf(type); long count = ScalarTypes.FlatCount(type), stride = obj.StorageSize(leaf);
        if (leaf is "i8" or "u8") { if (relocations.Count != 0) return false; EmitByteData(code, bytes, pointer, storageOffsets[symbol.Key], indent); return true; }
        if (leaf == "f80") return false; // Exact bytes retain binary80 payload/padding without allocating managed literal arrays.
        if (ScalarTypes.IsWideInteger(leaf) && relocations.Count == 0)
        {
            for (long first = 0; first < count; first += 4096)
            {
                int length = (int)Math.Min(4096, count - first);
                if (!AnyData(first * stride, length * stride)) continue;
                string name = generatedMembers.Allocate("__dot_initializer_" + storageDataDeclarations.Count, "initializer");
                var declaration = new StringBuilder("\n    private static global::System.ReadOnlySpan<ulong> " + name + " => [\n");
                for (int index = 0; index < length; index++)
                {
                    ReadOnlySpan<byte> value = bytes.AsSpan((int)((first + index) * stride), 16);
                    string text = leaf == "i128" ? BinaryPrimitives.ReadInt128LittleEndian(value).ToString(CultureInfo.InvariantCulture) : BinaryPrimitives.ReadUInt128LittleEndian(value).ToString(CultureInfo.InvariantCulture);
                    declaration.Append("        ").Append(Integer128Words(leaf, text)).Append(", // ").Append(text).Append('\n');
                }
                storageDataDeclarations.Add(declaration.Append("    ];\n").ToString());
                code.Append(' ', indent).Append(name).Append(".CopyTo(new global::System.Span<ulong>(").Append(pointer).Append(" + ")
                    .Append(storageOffsets[symbol.Key] + first * stride).Append(", ").Append(length * 2).Append("));\n");
            }
            return true;
        }
        if (ScalarTypes.IsNumeric(leaf) && relocations.Count == 0)
        {
            // Preserve exact floating payloads in integer RVA data; finite values
            // otherwise use ordinary typed literals and a span.CopyTo operation.
            string dataType = leaf;
            if (leaf is "f32" or "f64")
                for (long index = 0; index < count; index++)
                {
                    bool finite = leaf == "f32" ? float.IsFinite(BinaryPrimitives.ReadSingleLittleEndian(bytes.AsSpan((int)(index * stride))))
                        : double.IsFinite(BinaryPrimitives.ReadDoubleLittleEndian(bytes.AsSpan((int)(index * stride))));
                    if (!finite) { dataType = leaf == "f32" ? "u32" : "u64"; break; }
                }
            for (long first = 0; first < count; first += 4096)
            {
                int length = (int)Math.Min(4096, count - first); if (!AnyData(first * stride, length * stride)) continue;
                string name = generatedMembers.Allocate("__dot_initializer_" + storageDataDeclarations.Count, "initializer");
                var literals = Enumerable.Range(0, length).Select(index => PhysicalConstant(dataType, bytes.AsSpan((int)((first + index) * stride))));
                storageDataDeclarations.Add("\n    private static global::System.ReadOnlySpan<" + Type(dataType) + "> " + name + " => [" + string.Join(", ", literals) + "];\n");
                string source = dataType == leaf ? name : "global::System.Runtime.InteropServices.MemoryMarshal.Cast<" + Type(dataType) + ", " + Type(leaf) + ">(" + name + ")";
                code.Append(' ', indent).Append(source).Append(".CopyTo(new global::System.Span<").Append(Type(leaf)).Append(">((").Append(Type(leaf)).Append("*)(")
                    .Append(pointer).Append(" + ").Append(storageOffsets[symbol.Key] + first * stride).Append("), ").Append(length).Append("));\n");
            }
            return true;
        }
        if (!CanConstruct(leaf) || !ExactRepresentation(leaf, count, stride)) return false;
        string table = locals.Allocate(storage.SourceName, "table_" + storage.Id);
        code.Append(' ', indent).Append("var ").Append(table).Append(" = (").Append(Type(leaf)).Append("*)(").Append(pointer).Append(" + ").Append(storageOffsets[symbol.Key]).Append(");\n");
        for (long index = 0; index < count; index++)
            if (AnyData(index * stride, stride)) code.Append(' ', indent).Append(table).Append('[').Append(index).Append("] = ").Append(Value(leaf, index * stride, indent)).Append(";\n");
        return true;
    }
}
