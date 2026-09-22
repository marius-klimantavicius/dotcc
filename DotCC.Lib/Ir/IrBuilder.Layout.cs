using System;
using System.Globalization;
using DotCC.Layout;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private readonly System.Collections.Generic.Dictionary<string, int> _structPacks = new(StringComparer.Ordinal);
    private readonly System.Collections.Generic.Dictionary<string, int> _structAlignments = new(StringComparer.Ordinal);
    private readonly System.Collections.Generic.Dictionary<string, OffsetRequest> _constantOffsetRequests = new(StringComparer.Ordinal);
    internal static string LayoutType(CType type) => type.Unqualified switch
    {
        CType.Prim primitive => $"p:{primitive.Bytes}:{primitive.Bytes}",
        CType.Func { IsFunctionType: true } => "unsupported:function-type",
        CType.Pointer or CType.Func => "p:8:8",
        CType.Enum enumeration => LayoutType(enumeration.Underlying),
        CType.Array array => "a:" + (array.Count ?? 0).ToString(CultureInfo.InvariantCulture) + ":" + LayoutType(array.Element),
        CType.Named { IsExternal: true, ExternalLayout: { } layout } => $"p:{layout.Size}:{layout.Alignment}",
        CType.Named { IsExternal: true } named => throw new IrUnsupportedException("external type '" + named.Name + "' requires layout metadata (size and alignment) for this operation"),
        CType.Named named => "n:" + named.Name,
        // CType's record formatter prints computed self-referential properties
        // (Unqualified/FlatElement), so diagnostic serialization must never call
        // its ToString. Unrequested foreign layouts remain opaque descriptors;
        // requesting one still produces the strict unsupported-layout diagnostic.
        _ => "unsupported:" + type.Unqualified.GetType().Name,
    };

    private LayoutAggregate DescribeAggregate(string name)
    {
        if (_externalTypes.ContainsKey(name))
            throw new OffsetLayoutException("external type '" + name + "' has no C member layout; provide a C definition for member offsets");
        if (!_structFields.TryGetValue(name, out var fields))
            throw new OffsetLayoutException("Unknown or incomplete aggregate: " + name);
        var aggregate = new LayoutAggregate
        {
            Name = name, Union = _structIsUnion.GetValueOrDefault(name), Packed = _packedStructs.Contains(name), Alignment = _structAlignments.GetValueOrDefault(name), Pack = _structPacks.GetValueOrDefault(name),
        };
        foreach (var field in fields)
            aggregate.Fields.Add(new LayoutField { Name = field.Name, Type = LayoutType(field.Type), BitWidth = field.BitWidth, Alignment = field.Alignment });
        return aggregate;
    }

    internal OffsetDocument CreateOffsetDocument()
    {
        var document = new OffsetDocument();
        foreach (var name in _structFields.Keys) document.Aggregates.Add(name, DescribeAggregate(name));
        document.Requests.AddRange(_constantOffsetRequests.Values);
        return document;
    }

    private OffsetLayoutModel CreateLayoutModel() => new(DescribeAggregate);

    private (int Size, int Align) Layout(CType type)
    {
        try
        {
            var layout = CreateLayoutModel().Type(LayoutType(type));
            return (layout.Size, layout.Alignment);
        }
        catch (OffsetLayoutException error) { throw new IrUnsupportedException(error.Message); }
    }

    private int? OffsetOfConstPath(string structName, System.Collections.Generic.IReadOnlyList<string> path)
    {
        try
        {
            var offset = CreateLayoutModel().Offset(structName, path);
            var name = OffsetDocument.RequestName(structName, path);
            _constantOffsetRequests[name] = new OffsetRequest
            {
                Name = name, Aggregate = structName, Path = System.Linq.Enumerable.ToArray(path), Expected = offset,
            };
            return offset;
        }
        catch (OffsetLayoutException error) { throw new IrUnsupportedException(error.Message); }
    }
}
