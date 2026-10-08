namespace DotObj;

internal sealed class Linker
{
    public IReadOnlyList<ObjectFile> Objects { get; }
    public IReadOnlyList<Symbol> Definitions { get; }
    private readonly Dictionary<string, Symbol> definitions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> signatures = new(StringComparer.Ordinal);
    public Dictionary<string, string> Names { get; } = new(StringComparer.Ordinal);

    public Linker(IReadOnlyList<ObjectFile> objects, string className)
    {
        Objects = objects.OrderBy(obj => obj.UnitId, StringComparer.Ordinal).ToArray();
        if (Objects.Select(obj => obj.UnitId).Distinct(StringComparer.Ordinal).Count() != Objects.Count)
            throw new InvalidDataException("The same translation unit was supplied more than once");
        foreach (ObjectFile obj in Objects)
            foreach (Entity entity in obj.Entities.Values)
            {
                var symbol = new Symbol(obj, entity);
                if (signatures.TryGetValue(symbol.Key, out string? prior) && prior != symbol.Signature)
                    throw new InvalidDataException($"Signature mismatch for {entity.ExternalKey ?? entity.SourceName}: {prior} versus {symbol.Signature}");
                signatures[symbol.Key] = symbol.Signature;
                if (entity.Definition == "strong" && !definitions.TryAdd(symbol.Key, symbol))
                    throw new InvalidDataException($"Duplicate strong definition: {entity.ExternalKey ?? entity.SourceName}");
            }
        Definitions = definitions.Values.OrderBy(symbol => symbol.Key, StringComparer.Ordinal).ToArray();
        var names = new Names();
        names.Reserve(className);
        foreach (Symbol symbol in Definitions)
            Names.Add(symbol.Key, names.Allocate(symbol.Entity.SourceName, "function_" + symbol.Entity.Id));
    }

    public Symbol Resolve(ObjectFile obj, long id)
    {
        var declaration = new Symbol(obj, obj.Entities[id]);
        if (!definitions.TryGetValue(declaration.Key, out Symbol? definition))
            throw new InvalidDataException($"Unresolved function: {declaration.Entity.ExternalKey ?? declaration.Entity.SourceName} (referenced from {obj.PrimarySource})");
        return definition;
    }

    public Symbol Entry(string key)
    {
        var matches = Definitions.Where(symbol => symbol.Entity.Linkage == "external" &&
            (symbol.Entity.ExternalKey == key || symbol.Entity.SourceName == key)).ToArray();
        if (matches.Length != 1) throw new InvalidDataException($"Entry must identify one externally defined function: {key}");
        Symbol entry = matches[0];
        if (entry.Object.Parameters.GetValueOrDefault(entry.Entity.Id, []).Count != 0 ||
            entry.Object.Type(entry.Entity.ReturnTypeId) is not ("i32" or "void"))
            throw new InvalidDataException("Initial executable entry must be void() or i32()");
        return entry;
    }

    public void VerifyBodies()
    {
        foreach (Symbol symbol in Definitions)
        {
            FunctionBody body = symbol.Object.ReadBody(symbol.Entity.Id);
            foreach (Node node in body.Nodes.Values)
                if (node.Kind == "call") _ = Resolve(symbol.Object, node.EntityId!.Value);
        }
    }
}
