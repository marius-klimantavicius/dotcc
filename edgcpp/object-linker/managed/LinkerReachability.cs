using System.Text.Json;

namespace DotObj;

internal sealed partial class Linker
{
    private readonly HashSet<ObjectFile> providerObjects = [];
    private readonly HashSet<ObjectFile> selectedProviderObjects = [];
    private int candidateFunctions, candidateStorages;

    // Provider objects retain their complete semantic contents. Selection only
    // changes this link's executable closure; primary library APIs stay roots.
    private void SelectProviderClosure(IReadOnlySet<ObjectFile>? providers)
    {
        candidateFunctions = definitions.Count;
        candidateStorages = storageDefinitions.Count;
        if (providers is null || providers.Count == 0) return;
        foreach (ObjectFile obj in providers)
        {
            if (!Objects.Contains(obj)) throw new InvalidDataException("Provider is not a supplied object");
            providerObjects.Add(obj);
        }
        var functions = new HashSet<string>(StringComparer.Ordinal);
        var storages = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Queue<(bool Storage, string Key)>();
        void Function(Symbol symbol)
        {
            if (functions.Add(symbol.Key)) pending.Enqueue((false, symbol.Key));
        }
        void Storage(StorageSymbol symbol)
        {
            if (storages.Add(symbol.Key)) pending.Enqueue((true, symbol.Key));
        }
        void ActivateObject(ObjectFile obj)
        {
            if (!providerObjects.Contains(obj) || !selectedProviderObjects.Add(obj)) return;
            // Extraction of a provider translation unit also retains its source
            // startup obligations, even when their functions have internal linkage.
            foreach (InitializationAction action in obj.InitializationActions)
                ReferenceFunction(obj, action.EntityId);
        }
        void ReferenceFunction(ObjectFile obj, long id)
        {
            string key = new Symbol(obj, obj.Entities[id]).Key;
            if (definitions.TryGetValue(key, out Symbol? symbol)) Function(symbol);
        }
        void ReferenceStorage(ObjectFile obj, long id)
        {
            string key = new StorageSymbol(obj, obj.Storages[id]).Key;
            if (storageDefinitions.TryGetValue(key, out StorageSymbol? symbol)) Storage(symbol);
        }
        // Root identity belongs to each primary definition even when a stronger
        // provider definition supplies its canonical implementation/storage.
        foreach (ObjectFile obj in Objects.Where(obj => !providerObjects.Contains(obj)))
        {
            foreach (Entity entity in obj.Entities.Values)
                if (entity.Definition != "declaration") ReferenceFunction(obj, entity.Id);
            foreach (Storage storage in obj.Storages.Values)
                if (storage.Definition != "declaration") ReferenceStorage(obj, storage.Id);
        }
        while (pending.TryDequeue(out var item))
        {
            if (item.Storage)
            {
                StorageSymbol symbol = storageDefinitions[item.Key];
                ActivateObject(symbol.Object);
                foreach (Relocation relocation in symbol.Object.Relocations.GetValueOrDefault(symbol.Storage.Id, []))
                    if (relocation.TargetEntityId is long entity) ReferenceFunction(symbol.Object, entity);
                    else ReferenceStorage(symbol.Object, relocation.TargetStorageId!.Value);
            }
            else
            {
                Symbol symbol = definitions[item.Key];
                ActivateObject(symbol.Object);
                FunctionBody body = symbol.Object.ReadBody(symbol.Entity.Id);
                foreach (Node node in body.Nodes.Values)
                {
                    if (node.EntityId is long entity) ReferenceFunction(symbol.Object, entity);
                    if (node.StorageId is long storage) ReferenceStorage(symbol.Object, storage);
                }
            }
        }
        foreach (string key in definitions.Keys.Where(key => !functions.Contains(key)).ToArray()) definitions.Remove(key);
        foreach (string key in storageDefinitions.Keys.Where(key => !storages.Contains(key)).ToArray()) storageDefinitions.Remove(key);
    }

    public void WriteSelection(string output)
    {
        var report = new
        {
            formatVersion = 1,
            policy = "primary-definitions-and-reachable-providers",
            candidateFunctions,
            selectedFunctions = Definitions.Count,
            candidateStorages,
            selectedStorages = StorageDefinitions.Count,
            startupActions = InitializationActions.Count,
            objects = Objects.Select(obj => new
            {
                unitId = obj.UnitId,
                digest = obj.Digest,
                role = providerObjects.Contains(obj) ? "provider" : "primary",
                selected = !providerObjects.Contains(obj) || selectedProviderObjects.Contains(obj)
            })
        };
        File.WriteAllText(Path.Combine(output, "link-selection.json"),
            JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + "\n");
    }
}
