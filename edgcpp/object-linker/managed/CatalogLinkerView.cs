using System.Globalization;

namespace DotObj;

internal sealed partial class Linker
{
    private readonly CatalogEmissionPlan? catalogPlan;
    // This view lives only inside CatalogUnitReader.WithUnit. Resolution has
    // already finished; calls use this unit's typed declaration plus the global
    // selected key, without loading the provider's complete source graph.
    internal Linker(ObjectFile obj,CatalogUnitDescriptor unit,CatalogEmissionPlan plan)
    {
        catalogPlan=plan;allowExterns=true;sharedExceptionProvider=true;
        Objects=[obj];EmissionObjects=[obj];InitializationActions=[];
        Definitions=plan.Locators("function",unit.Id).Select(item=>new Symbol(obj,obj.Entities[item.Id])).ToArray();
        StorageDefinitions=plan.Locators("storage",unit.Id).Select(item=>new StorageSymbol(obj,obj.Storages[item.Id])).ToArray();
        foreach(var entity in obj.Entities.Values)
        {
            var symbol=new Symbol(obj,entity);
            if(plan.SymbolName("function",symbol.Key) is string name)Names.TryAdd(symbol.Key,name);
            if(plan.External("function",symbol.Key))
            {
                if(plan.ExternalOwner("function",symbol.Key,unit.Id,entity.Id) || !Externs.ContainsKey(symbol.Key))Externs[symbol.Key]=symbol;
            }
            else if(Names.ContainsKey(symbol.Key))definitions.TryAdd(symbol.Key,symbol);
            if(plan.UndefinedWeak(symbol.Key))unresolvedWeakFunctions.TryAdd(symbol.Key,symbol);
        }
        foreach(var storage in obj.Storages.Values)
        {
            var symbol=new StorageSymbol(obj,storage);
            if(plan.SymbolName("storage",symbol.Key) is string name)StorageNames.TryAdd(symbol.Key,name);
            if(plan.External("storage",symbol.Key))
            {
                if(plan.ExternalOwner("storage",symbol.Key,unit.Id,storage.Id) || !ExternStorages.ContainsKey(symbol.Key))ExternStorages[symbol.Key]=symbol;
            }
            else if(StorageNames.ContainsKey(symbol.Key))storageDefinitions.TryAdd(symbol.Key,symbol);
            string? extent=plan.Scalar("SELECT extent FROM storage_requirements WHERE symbol_key=?",symbol.Key);
            if(extent is not null)requiredStorageExtents.TryAdd(symbol.Key,long.Parse(extent,CultureInfo.InvariantCulture));
            string? alignment=plan.Scalar("SELECT max(required_alignment) FROM storages WHERE symbol_key=?",symbol.Key);
            if(alignment is not null)requiredStorageAlignments.TryAdd(symbol.Key,long.Parse(alignment,CultureInfo.InvariantCulture));
        }
        foreach(var record in obj.Records.Values)Records.TryAdd(record.Key,(obj,record));
        RequiresDynamicSpecifications=plan.RuntimeFlag("DynamicSpecifications",true);
        RequiresSourceHandlers=plan.RuntimeFlag("SourceHandlers",true) || RequiresDynamicSpecifications;
        RequiresExceptionProvider=plan.RuntimeFlag("ExceptionProvider",true) || RequiresDynamicSpecifications;
        foreach(var symbol in Definitions)
            if(obj.DynamicExceptionBoundaries.TryGetValue(symbol.Entity.Id,out long specification))
                foreach(var type in obj.DynamicExceptionSpecifications[specification].Types)
                    UsedCatchTypes.TryAdd((obj,type.MatchSourceTypeId),obj.SourceTypes[type.MatchSourceTypeId]);
        foreach(var use in plan.ExceptionUses(unit.Id))
        {
            if(use.Kind=="catch" && obj.SourceHandlers[use.Node].MatchTypeId is long match)
                UsedCatchTypes.TryAdd((obj,match),obj.SourceTypes[match]);
            if(use.Kind=="throw")
            {
                long id=obj.ThrowExpressions[use.Node].ExceptionTypeId;
                UsedExceptionTypes.TryAdd((obj,id),obj.ExceptionTypes[id]);
            }
        }
    }
}
