namespace DotObj;

internal sealed partial class Linker
{
    public IReadOnlyList<ObjectFile> Objects { get; }
    public IReadOnlyList<ObjectFile> EmissionObjects { get; }
    public IReadOnlyList<Symbol> Definitions { get; }
    public IReadOnlyList<StorageSymbol> StorageDefinitions { get; }
    public Dictionary<string, Symbol> Externs { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, StorageSymbol> ExternStorages { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, string> StorageNames { get; } = new(StringComparer.Ordinal);
    private readonly Dictionary<string, StorageSymbol> storageDefinitions = new(StringComparer.Ordinal);
    private readonly Names names = new();
    private readonly bool allowExterns;
    private readonly Dictionary<string, Symbol> definitions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> signatures = new(StringComparer.Ordinal);
    public Dictionary<string, string> Names { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, (ObjectFile Object, RecordType Record)> Records { get; } = new(StringComparer.Ordinal);

    public Linker(IReadOnlyList<ObjectFile> objects, string className, bool allowExterns = false, IReadOnlySet<ObjectFile>? providers = null)
    {
        this.allowExterns = allowExterns;
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
                if (entity.Definition != "declaration")
                {
                    if (!definitions.TryGetValue(symbol.Key, out Symbol? existing)) definitions.Add(symbol.Key, symbol);
                    else if (existing.Entity.Definition == "strong" && entity.Definition == "strong")
                        throw new InvalidDataException($"Duplicate strong definition: {entity.ExternalKey ?? entity.SourceName}");
                    else if (entity.Definition == "strong") definitions[symbol.Key] = symbol;
                }
            }
        var storageSignatures = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (ObjectFile obj in Objects)
            foreach (Storage storage in obj.Storages.Values)
            {
                var symbol = new StorageSymbol(obj, storage);
                if (storageSignatures.TryGetValue(symbol.Key, out string? prior) && !CompatibleStorage(prior, symbol.Signature))
                    throw new InvalidDataException($"Storage type/TLS mismatch: {storage.ExternalKey ?? storage.SourceName}");
                storageSignatures[symbol.Key] = symbol.Signature;
                if (storage.Definition == "declaration") continue;
                if (!storageDefinitions.TryGetValue(symbol.Key, out StorageSymbol? existing)) storageDefinitions.Add(symbol.Key, symbol);
                else if (existing.Storage.Definition == "strong" && storage.Definition == "strong")
                    throw new InvalidDataException($"Duplicate strong storage definition: {storage.ExternalKey ?? storage.SourceName}");
                else if (storage.Definition == "strong" || existing.Storage.Definition == "tentative" && storage.Definition == "linkonce")
                    storageDefinitions[symbol.Key] = symbol;
            }
        SelectProviderClosure(providers);
        EmissionObjects = Objects.Where(obj => !providerObjects.Contains(obj) || selectedProviderObjects.Contains(obj)).ToArray();
        foreach (ObjectFile obj in EmissionObjects)
            foreach (RecordType record in obj.Records.Values)
            {
                if (Records.TryGetValue(record.Key, out var prior))
                {
                    if (prior.Record.Size == 0 || record.Size == 0)
                    {
                        if (prior.Record.LayoutKind != record.LayoutKind || prior.Record.SourceName != record.SourceName)
                            throw new InvalidDataException("Incomplete record identity mismatch");
                        if (prior.Record.Size == 0 && record.Size != 0) Records[record.Key] = (obj, record);
                        continue;
                    }
                    RecordField[] expected = prior.Object.RecordFields(prior.Record.TypeId), actual = obj.RecordFields(record.TypeId);
                    if (record.SourceName != prior.Record.SourceName || record.Size != prior.Record.Size || record.Alignment != prior.Record.Alignment || record.LayoutKind != prior.Record.LayoutKind ||
                        expected.Length != actual.Length || actual.Where((field, index) => field.SourceName != expected[index].SourceName ||
                            field.ByteOffset != expected[index].ByteOffset || field.BitOffset != expected[index].BitOffset || field.BitWidth != expected[index].BitWidth || field.BitSigned != expected[index].BitSigned || obj.Type(field.TypeId) != prior.Object.Type(expected[index].TypeId) ||
                            obj.SourceTypeSignature(field.SourceTypeId) != prior.Object.SourceTypeSignature(expected[index].SourceTypeId)).Any())
                        throw new InvalidDataException($"Record layout mismatch for {record.IdentityKey}");
                }
                else Records.Add(record.Key, (obj, record));
            }
        Definitions = definitions.Values.OrderBy(symbol => symbol.Key, StringComparer.Ordinal).ToArray();
        StorageDefinitions = storageDefinitions.Values.OrderBy(symbol => symbol.Key, StringComparer.Ordinal).ToArray();
        names.Reserve(className);
        foreach (Symbol symbol in Definitions)
            Names.Add(symbol.Key, names.Allocate(symbol.Entity.SourceName, "function_" + symbol.Entity.Id));
        foreach (StorageSymbol symbol in StorageDefinitions)
            StorageNames.Add(symbol.Key, names.Allocate(symbol.Storage.SourceName, "__literal_" + symbol.Storage.Id));
    }

    public Symbol Resolve(ObjectFile obj, long id)
    {
        var declaration = new Symbol(obj, obj.Entities[id]);
        if (definitions.TryGetValue(declaration.Key, out Symbol? definition)) return definition;
        if (!allowExterns || declaration.Entity.Linkage != "external")
            throw new InvalidDataException($"Unresolved function: {declaration.Entity.ExternalKey ?? declaration.Entity.SourceName} (referenced from {obj.PrimarySource})");
        if (Externs.TryAdd(declaration.Key, declaration)) Names.Add(declaration.Key, names.Allocate(declaration.Entity.SourceName, "extern_" + declaration.Entity.Id));
        return Externs[declaration.Key];
    }

    private static bool CompatibleStorage(string left, string right)
    {
        if (left == right) return true;
        int a = left.LastIndexOf(':'), b = right.LastIndexOf(':');
        if (a < 0 || b < 0 || left[a..] != right[b..]) return false;
        string x = left[..a], y = right[..b];
        return ScalarTypes.IsArray(x) && ScalarTypes.IsArray(y) && ScalarTypes.Element(x) == ScalarTypes.Element(y) &&
            (ScalarTypes.ElementCount(x) == 0 || ScalarTypes.ElementCount(y) == 0);
    }

    public StorageSymbol ResolveStorage(ObjectFile obj, long id)
    {
        var declaration = new StorageSymbol(obj, obj.Storages[id]);
        if (storageDefinitions.TryGetValue(declaration.Key, out StorageSymbol? definition)) return definition;
        if (!allowExterns || declaration.Storage.Linkage != "external")
            throw new InvalidDataException($"Unresolved storage: {declaration.Storage.ExternalKey ?? declaration.Storage.SourceName}");
        if (ExternStorages.TryAdd(declaration.Key, declaration))
            StorageNames.Add(declaration.Key, names.Allocate(declaration.Storage.SourceName, "extern_storage_" + declaration.Storage.Id));
        return ExternStorages[declaration.Key];
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
            {
                if (node.Kind is "call" or "register-cleanup" or "function-address") _ = Resolve(symbol.Object, node.EntityId!.Value);
                if (node.Kind is "storage" or "storage-address") _ = ResolveStorage(symbol.Object, node.StorageId!.Value);
            }
        }
        foreach (StorageSymbol symbol in StorageDefinitions)
            foreach (Relocation relocation in symbol.Object.Relocations.GetValueOrDefault(symbol.Storage.Id, []))
                if (relocation.TargetEntityId is long entity) _ = Resolve(symbol.Object, entity);
                else _ = ResolveStorage(symbol.Object, relocation.TargetStorageId!.Value);
    }
}
