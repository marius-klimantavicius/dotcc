using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DotCC.Ir;
using IrType = DotCC.Ir.CType;

namespace DotCC;

/// <summary>Semantic object-file metadata, independent of the generated C# body.
/// Named references are compared by identity; each referenced definition is checked
/// separately when its object declaration is merged.</summary>
internal sealed record ObjectAggregateMetadata(bool IsIncomplete, bool IsUnion, string Signature)
{
    // Full declared shape, bound to Signature. Cross-language managed adapters
    // use this witness; generated C# is never a source of layout/type evidence.
    internal string? SourceShape { get; init; }
    internal string? ZigStorageContract { get; init; }
    internal string? ZigStorageBaseShape { get; init; }

    internal static ObjectAggregateMetadata From(StructTypeDef type)
    {
        var shape = new StringBuilder();
        shape.Append(type.IsUnion ? "union" : "struct").Append('|')
            .Append((int)type.Layout).Append('|').Append(type.Alignment).Append('|').Append(type.Pack);
        foreach (var field in type.Fields)
        {
            string described;
            try { described = Describe(field.Type); }
            catch (CompileException error) { throw new CompileException($"object aggregate field '{type.Name}.{field.Name}': {error.Message}", error); }
            shape.Append('|').Append(Atom(field.Name)).Append(':').Append(described)
                .Append(':').Append(field.BitWidth?.ToString(CultureInfo.InvariantCulture) ?? "-")
                .Append(':').Append(field.Alignment).Append(':').Append(field.IsAnonymousAggregate);
        }
        var zig = type.ZigStorage is { } storage ? ZigStorageObjectMetadata.Contract(storage)
            : type.ZigZeroOwner is { } owner ? ZigStorageObjectMetadata.ZeroOwnerContract(owner) : null;
        var baseShape = zig is not null ? shape.ToString() : null;
        if (zig is not null) shape.Append("|zig-storage:1|").Append(zig);
        return new(type.IsIncomplete, type.IsUnion,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(shape.ToString())))) {
                SourceShape = type.Fields.All(f => ManagedBoundaryContracts.RepresentedType(f.Type)) ? shape.ToString() : null,
                ZigStorageContract = zig, ZigStorageBaseShape = baseShape };
    }

    private static string Atom(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    internal static string Describe(IrType type) => ((int)type.Quals).ToString(CultureInfo.InvariantCulture) + ":" + (type switch
    {
        IrType.Prim p => $"prim({Atom(p.Name)},{p.Bytes},{p.Integer},{p.Signed})",
        IrType.VoidType => "void",
        IrType.NoReturnType => "noreturn",
        IrType.Pointer p => "ptr(" + Describe(p.Pointee) + ")",
        IrType.Array a => "array(" + a.Count?.ToString(CultureInfo.InvariantCulture) + "," + Describe(a.Element) + ")",
        IrType.Vector v => "vector(" + v.Count.ToString(CultureInfo.InvariantCulture) + "," + Describe(v.Element) + ")",
        IrType.Opaque o => "opaque(" + Atom(o.Name) + ")",
        IrType.Named n => "named(" + Atom(n.Name) + ")",
        IrType.Enum e => "enum(" + Atom(e.Name) + "," + Describe(e.Underlying) + ")",
        IrType.Func f => "func(" + Describe(f.Return) + "," + f.Variadic + "," + f.IsNativeCallConv + "," + string.Join(";", f.Params.Select(Describe)) + ")",
        IrType.ComplexType => "complex",
        IrType.Float128Type => "float128",
        IrType.LongDoubleStorage16 => "longdouble-x87-storage16",
        IrType.Optional o => "optional(" + Describe(o.Inner) + ")",
        IrType.ErrorUnion e => "errorunion(" + Describe(e.Payload) + ")",
        IrType.ErrorSetType => "errorset",
        IrType.Slice s => "slice(" + Describe(s.Element) + ")",
        IrType.ZigList l => "list(" + Describe(l.Element) + ")",
        IrType.Allocator => "allocator",
        IrType.Tuple t => "tuple(" + string.Join(";", t.Elements.Select(Describe)) + ")",
        _ => throw new CompileException("unhandled object aggregate field type: " + type.Describe())
    });
}
