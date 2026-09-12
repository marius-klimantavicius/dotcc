using System.Collections.Generic;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    // C promotion is semantic metadata, not a convention inferred from generated
    // field names. These value properties add no storage and never return a ref
    // into a potentially movable managed struct.
    private void AppendPromotedProperties(StringBuilder output, StructTypeDef owner)
    {
        var names = new HashSet<string>(System.StringComparer.Ordinal);
        foreach (var field in owner.Fields)
            if (!field.IsAnonymousAggregate && field.Name.Length != 0) names.Add(field.Name);

        void Visit(StructTypeDef aggregate, string path, TypeQual inherited)
        {
            foreach (var field in aggregate.Fields)
            {
                if (field.Name.Length == 0) continue;
                var access = path + "." + EmitHelpers.Id(field.Name);
                if (field.IsAnonymousAggregate)
                {
                    if (field.Type.Unqualified is not CType.Named nested || !_aggregateDefinitions.TryGetValue(nested.Name, out var definition))
                        throw new IrUnsupportedException("missing anonymous aggregate definition: " + field.Name);
                    Visit(definition, access, inherited | field.Type.Quals);
                    continue;
                }
                if (!names.Add(field.Name))
                    throw new IrUnsupportedException("ambiguous anonymous member: " + field.Name);
                // Inline arrays have no C value-copy property representation.
                // Keep their existing physical buffer/InlineArray field surface;
                // translated C continues to use its resolved storage path.
                if (field.Type.Unqualified is CType.Array || field.Name == owner.Name) continue;
                var type = field.Type.WithQuals(inherited);
                if (type.IsAtomic || type.IsVolatile)
                {
                    // Bit-field properties cannot be passed by ref. Aggregate
                    // volatile copies need a separate defined storage operation.
                    // Retain physical access for these unsupported projections.
                    if (field.IsBitField || type.Unqualified is CType.Named) continue;
                    AppendQualifiedPromotedProperty(output, owner, field, type, access);
                    continue;
                }
                output.Append("    public ").Append(Cs(field.Type)).Append(' ').Append(EmitHelpers.Id(field.Name))
                    .Append("\n    {\n        get => ").Append(access).Append(";\n");
                if (!type.IsConst && !ContainsConst(field.Type))
                    output.Append("        set => ").Append(access).Append(" = value;\n");
                output.Append("    }\n");
            }
        }

        foreach (var field in owner.Fields)
            if (field.IsAnonymousAggregate && field.Type.Unqualified is CType.Named nested && _aggregateDefinitions.TryGetValue(nested.Name, out var definition))
                Visit(definition, "this." + EmitHelpers.Id(field.Name), field.Type.Quals);
    }

    private void AppendQualifiedPromotedProperty(StringBuilder output, StructTypeDef owner, StructField field, CType type, string access)
    {
        var storageType = type.IsPointerLowered ? "nint" : type.Unqualified is CType.Enum enumeration ? Cs(enumeration.Underlying) : Cs(type);
        var location = "*(" + storageType + "*)&" + access.Replace("this.", "__self->");
        var helper = type.IsAtomic ? "Atomic" : "global::System.Threading.Volatile";
        var read = helper + (type.IsAtomic ? ".Load" : ".Read") + "(ref " + location + ")";
        var write = helper + (type.IsAtomic ? ".Store" : ".Write") + "(ref " + location + ", (" + storageType + ")value)";
        output.Append("    public ").Append(Cs(type)).Append(' ').Append(EmitHelpers.Id(field.Name)).Append("\n    {\n")
            .Append("        get { fixed (").Append(owner.Name).Append("* __self = &this) { return (").Append(Cs(type)).Append(')').Append(read).Append("; } }\n");
        if (!type.IsConst)
            output.Append("        set { fixed (").Append(owner.Name).Append("* __self = &this) { ").Append(write).Append("; } }\n");
        output.Append("    }\n");
    }

    private bool ContainsConst(CType type)
    {
        if (type.IsConst) return true;
        if (type.Unqualified is CType.Array array) return ContainsConst(array.Element);
        if (type.Unqualified is CType.Named named && _aggregateDefinitions.TryGetValue(named.Name, out var aggregate))
            foreach (var field in aggregate.Fields)
                if (ContainsConst(field.Type)) return true;
        return false;
    }
}
