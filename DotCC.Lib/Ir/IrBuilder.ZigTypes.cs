namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    internal Dictionary<string, List<StructField>> StructFields => _structFields;
    internal Dictionary<string, bool> StructIsUnion => _structIsUnion;
    internal HashSet<string> PackedStructs => _packedStructs;
    internal HashSet<string> PrunableTypes { get; } = new(StringComparer.Ordinal);

    /// <summary>Withdraw a registered struct / union so it is not emitted: a lazy Zig module's container
    /// found, after registering, to hold a container that could not be lowered (road-to-zig-std G3,
    /// <c>ZigLowering.FailDependentContainers</c>). A no-op for a name that is not registered.</summary>
    internal void WithdrawStructType(string name)
    {
        if (!_emittedTypes.Remove(name)) { return; }
        _structFields.Remove(name);
        _structIsUnion.Remove(name);
        _packedStructs.Remove(name);
        Types.RemoveAll(t => t.Name == name);
    }

    /// <summary>Replace a registered struct / union's field list (same names, rewritten types) in both the
    /// field table and the emitted type definition, keeping its position in <see cref="Types"/>.</summary>
    internal void ReplaceStructFields(string name, List<StructField> fields)
    {
        _structFields[name] = fields;
        var i = Types.FindIndex(t => t.Name == name);
        if (i >= 0) { Types[i] = Types[i] with { Fields = fields }; }
    }

}
