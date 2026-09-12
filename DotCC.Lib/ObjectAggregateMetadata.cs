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
    internal static ObjectAggregateMetadata From(StructTypeDef type)
    {
        var shape = new StringBuilder();
        shape.Append(type.IsUnion ? "union" : "struct").Append('|')
            .Append((int)type.Layout).Append('|').Append(type.Alignment).Append('|').Append(type.Pack);
        foreach (var field in type.Fields)
            shape.Append('|').Append(Atom(field.Name)).Append(':').Append(Describe(field.Type))
                .Append(':').Append(field.BitWidth?.ToString(CultureInfo.InvariantCulture) ?? "-")
                .Append(':').Append(field.Alignment);
        return new(type.IsIncomplete, type.IsUnion,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(shape.ToString()))));
    }

    private static string Atom(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
    private static string Describe(IrType type) => ((int)type.Quals).ToString(CultureInfo.InvariantCulture) + ":" + (type switch
    {
        IrType.Prim p => $"prim({Atom(p.Name)},{p.Bytes},{p.Integer},{p.Signed})",
        IrType.VoidType => "void",
        IrType.Pointer p => "ptr(" + Describe(p.Pointee) + ")",
        IrType.Array a => "array(" + a.Count?.ToString(CultureInfo.InvariantCulture) + "," + Describe(a.Element) + ")",
        IrType.Named n => "named(" + Atom(n.Name) + ")",
        IrType.Enum e => "enum(" + Atom(e.Name) + "," + Describe(e.Underlying) + ")",
        IrType.Func f => "func(" + Describe(f.Return) + "," + f.Variadic + "," + f.IsNativeCallConv + "," + string.Join(";", f.Params.Select(Describe)) + ")",
        IrType.ComplexType => "complex",
        IrType.Float128Type => "float128",
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
