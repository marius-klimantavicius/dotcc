using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private const int AggregateDataThreshold = 1024;
    private const int AggregateDataChunkBytes = 4096;
    private readonly Dictionary<CType, bool> _aggregateDataLayouts = new();

    // Prove the entire initializer before registering payloads or emitting any
    // stores. A relocation, runtime read, effect, or unsupported representation
    // sends the complete object/table through the existing typed initializer.
    private bool TryAggregateBytes(CType element, IReadOnlyList<CExpr> values, out byte[] bytes,
        int minimumBytes = AggregateDataThreshold)
    {
        bytes = Array.Empty<byte>();
        if (!_aggregateData || _target is not CSharpTarget { StorageAbi: "lp64-le-dotcc-v1" }
            || element.Unqualified is not CType.Named || values.Count == 0) return false;
        try
        {
            if (!AggregateDataLayout(element, 0)) return false;
            int stride = _offsetModel.Type(IrBuilder.LayoutType(element)).Size;
            int size = checked(stride * values.Count);
            if (size < minimumBytes) return false;
            var result = new byte[size];
            for (int i = 0; i < values.Count; i++)
                if (!SerializeAggregateValue(element, values[i], result, checked(i * stride), 0)) return false;
            bytes = result;
            return true;
        }
        catch (Exception error) when (error is DotCC.Layout.OffsetLayoutException or IrUnsupportedException or OverflowException)
        {
            return false;
        }
    }

    private bool AggregateDataLayout(CType type, int depth)
    {
        if (depth > 64 || type.IsVolatile || type.IsAtomic) return false;
        if (_aggregateDataLayouts.TryGetValue(type, out bool known)) return known;
        _aggregateDataLayouts[type] = false; // Also rejects recursive by-value layouts.
        bool supported;
        switch (type.Unqualified)
        {
            case CType.Prim primitive:
                supported = AggregatePrimitive(primitive);
                break;
            case CType.Enum enumeration:
                supported = AggregateDataLayout(enumeration.Underlying, depth + 1);
                break;
            case CType.Array array:
                supported = array is { Count: > 0, RuntimeCount: null }
                    && AggregateDataLayout(array.Element, depth + 1);
                break;
            case CType.Named { IsExternal: false } named when _aggregateDefinitions.TryGetValue(named.Name, out var definition):
                if (definition.IsIncomplete) return false;
                var layout = _offsetModel.Aggregate(named.Name);
                if (layout.Size <= 0) return false; // CLR empty structs have a different extent.
                for (int i = 0; i < definition.Fields.Count; i++)
                {
                    var field = definition.Fields[i];
                    if (field.IsFlexibleArray || !AggregateDataLayout(field.Type, depth + 1)) return false;
                    if (field.BitWidth is { } width)
                    {
                        // Legacy Zig packed units do not provide a bit-position
                        // map. C packed fields use Pack and the authoritative map.
                        if (definition.Layout == AggregateLayout.Packed || width < 0 || width > 64) return false;
                        if (width == 0) continue;
                        if (!layout.BitFields.TryGetValue(i, out var place) || place.Offset < 0 || place.Bit < 0
                            || (long)place.Offset * 8 + place.Bit + width > (long)layout.Size * 8) return false;
                    }
                    else
                    {
                        int size = _offsetModel.Type(IrBuilder.LayoutType(field.Type)).Size;
                        if (size <= 0 || !layout.Offsets.TryGetValue(field.Name, out int offset)
                            || offset < 0 || (long)offset + size > layout.Size) return false;
                    }
                }
                supported = true;
                break;
            default:
                // This includes pointers (even omitted/null pointer members),
                // authored external storage, managed/runtime types and vectors.
                supported = false;
                break;
        }
        if (supported) _ = _offsetModel.Type(IrBuilder.LayoutType(type));
        return _aggregateDataLayouts[type] = supported;
    }

    private bool SerializeAggregateValue(CType type, CExpr value, byte[] bytes, int offset, int depth)
    {
        if (depth > 64) return false;
        if (value is DefaultLit) return true;
        if (type.Unqualified is CType.Named named)
        {
            if (value is not StructInit aggregate || aggregate.Type.Unqualified != named) return false;
            var definition = _aggregateDefinitions[named.Name];
            if (definition.IsUnion && aggregate.Members.Count > 1) return false;
            var layout = _offsetModel.Aggregate(named.Name);
            var assigned = new HashSet<string>(StringComparer.Ordinal);
            foreach (var member in aggregate.Members)
            {
                if (!assigned.Add(member.Name)) return false;
                int index = -1;
                for (int i = 0; i < definition.Fields.Count; i++)
                    if (definition.Fields[i].Name == member.Name) { index = i; break; }
                if (index < 0) return false;
                var field = definition.Fields[index];
                if (field.IsAnonBitField || member.FieldType.Unqualified != field.Type.Unqualified) return false;
                if (field.BitWidth is { } width)
                {
                    if (width <= 0 || !TryAggregateScalarBits(member.Value, field.Type, out ulong bits)) return false;
                    var place = layout.BitFields[index];
                    // A 64-bit C packed field may straddle NINE bytes. Shift
                    // the value by at most 63, never by its storage span width.
                    for (int bit = 0; bit < width; bit++)
                    {
                        int destination = checked(offset + place.Offset + (place.Bit + bit) / 8);
                        byte mask = (byte)(1 << ((place.Bit + bit) % 8));
                        bytes[destination] = (byte)((bytes[destination] & ~mask)
                            | (((bits >> bit) & 1) != 0 ? mask : 0));
                    }
                }
                else if (!SerializeAggregateValue(field.Type, member.Value, bytes,
                    checked(offset + layout.Offsets[field.Name]), depth + 1)) return false;
            }
            return true;
        }
        if (type.Unqualified is CType.Array array)
        {
            if (value is not InlineArrayInit initializer || initializer.Element.Unqualified != array.FlatElement.Unqualified)
                return false;
            int stride = _offsetModel.Type(IrBuilder.LayoutType(array.FlatElement)).Size;
            int count = _offsetModel.Type(IrBuilder.LayoutType(array)).Size / stride;
            if (initializer.Elems.Count > count) return false;
            for (int i = 0; i < initializer.Elems.Count; i++)
                if (!SerializeAggregateValue(array.FlatElement, initializer.Elems[i], bytes,
                    checked(offset + i * stride), depth + 1)) return false;
            return true;
        }
        if (!TryAggregateScalarBits(value, type, out ulong scalar)) return false;
        int widthBytes = _offsetModel.Type(IrBuilder.LayoutType(type)).Size;
        for (int i = 0; i < widthBytes; i++) bytes[offset + i] = (byte)(scalar >> (i * 8));
        return true;
    }

    private bool EmitAggregateDataCopies(StringBuilder output, string destination, CType element, IReadOnlyList<CExpr> values)
    {
        if (!TryAggregateBytes(element, values, out var bytes)) return false;
        AppendAggregateDataCopies(output, destination, element, bytes);
        return true;
    }

    private void AppendAggregateDataCopies(StringBuilder output, string destination, CType element, byte[] bytes)
    {
        int stride = _offsetModel.Type(IrBuilder.LayoutType(element)).Size;
        output.Append("        // aggregate-data: ").Append(bytes.Length.ToString(CultureInfo.InvariantCulture)).Append(" bytes\n")
            .Append("        if (!global::System.BitConverter.IsLittleEndian || sizeof(").Append(Cs(element)).Append(") != ")
            .Append(stride.ToString(CultureInfo.InvariantCulture)).Append(") throw new global::System.PlatformNotSupportedException(\"aggregate-data requires lp64-le-dotcc-v1 target layout\");\n");
        for (int offset = 0; offset < bytes.Length; offset += AggregateDataChunkBytes)
        {
            int count = Math.Min(AggregateDataChunkBytes, bytes.Length - offset);
            string payload = _literals.AddPayload(bytes.AsSpan(offset, count).ToArray());
            output.Append("        ").Append(payload).Append(".CopyTo(new global::System.Span<byte>((byte*)(")
                .Append(destination).Append(')');
            if (offset != 0) output.Append(" + ").Append(offset.ToString(CultureInfo.InvariantCulture));
            output.Append(", ").Append(count.ToString(CultureInfo.InvariantCulture)).Append("));\n");
        }
    }
}
