using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace DotObj;

internal static class CatalogImporter
{
    public static void Import(string catalog,IEnumerable<CatalogInput> inputs,Action<CatalogUnitDescriptor>? imported=null,Action<WeakReference>? released=null)
    {
        using var output=new LinkCatalog(catalog);long ordinal=0;
        foreach(var input in inputs)
        {
            // ImportOne returns only strings/counts. No Symbol, ObjectFile or
            // FunctionBody survives across unit boundaries.
            var descriptor=ImportOne(output,input,ordinal++,out var objectLifetime);
            released?.Invoke(objectLifetime);imported?.Invoke(descriptor);
        }
        if(ordinal==0)throw new ArgumentException("A catalog requires at least one semantic object");
        output.Complete();
    }
    private static (long Size,string Hash) FileIdentity(string path)
    {
        using var input=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);
        return (input.Length,Convert.ToHexStringLower(SHA256.HashData(input)));
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static CatalogUnitDescriptor ImportOne(LinkCatalog output,CatalogInput input,long ordinal,out WeakReference objectLifetime)
    {
        string path=System.IO.Path.GetFullPath(input.Path);
        // A portable packaged object is a standalone database, not a live WAL
        // database whose semantic state is absent from its file fingerprint.
        if(File.Exists(path+"-wal") || File.Exists(path+"-journal"))
            throw new InvalidDataException("Catalog import requires a checkpointed standalone semantic object");
        var before=FileIdentity(path);var timer=Stopwatch.StartNew();
        using var obj=new ObjectFile(path);objectLifetime=new WeakReference(obj);long unit=checked(ordinal+1);
        output.BeginUnit();
        try
        {
            output.Insert("INSERT INTO units VALUES(?,?,?,?,?,?,?,?,?,?,?,?)",unit,path,before.Hash,before.Size,obj.Digest,
                obj.UnitId,obj.PrimarySource,obj.SourceLanguage,input.Provider,ordinal,obj.Functions.Count,0);
            WriteDeclarations(output,unit,obj);
            CatalogContracts.Write(output,unit,obj);
            long nodeCount=0;
            foreach(long function in obj.Functions.Keys.Order())
                nodeCount+=WriteBody(output,unit,obj,function);
            var after=FileIdentity(path);
            if(before!=after || File.Exists(path+"-wal") || File.Exists(path+"-journal"))
                throw new InvalidDataException("Semantic object changed during catalog import");
            output.Insert("UPDATE units SET verified_body_count=? WHERE id=?",obj.Functions.Count,unit);
            output.Insert("INSERT INTO unit_metrics VALUES(?,?,?)",unit,timer.ElapsedMilliseconds,nodeCount);
            output.CommitUnit();
            return new(unit,path,before.Hash,before.Size,obj.Digest,obj.UnitId,obj.PrimarySource,obj.SourceLanguage,
                input.Provider,ordinal,obj.Functions.Count,obj.Functions.Count);
        }
        catch { output.RollbackUnit();throw; }
    }
    private static void WriteDeclarations(LinkCatalog output,long unit,ObjectFile obj)
    {
        foreach(var entity in obj.Entities.Values.OrderBy(value=>value.Id))
        {
            var symbol=new Symbol(obj,entity);bool deferred=obj.DeferredFunctionDeclarations.ContainsKey(entity.Id);
            string? semantic=obj.EntitySourceRoutines.TryGetValue(entity.Id,out var original)?obj.SourceTypeSignature(original.TypeId):null;
            output.Insert("INSERT INTO functions VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)",unit,entity.Id,symbol.Key,
                Encoding.BigEndianUnicode.GetBytes(symbol.Key),entity.Definition,entity.SourceName,entity.Linkage,entity.IsWeak,
                symbol.Signature,obj.CallableRoutine(entity.Id),deferred?null:obj.FunctionSourceSignature(entity.Id),semantic,deferred,
                original is not null && obj.SourceFunctionKinds.GetValueOrDefault(entity.Id)=="ordinary",
                RuntimeArrayProvider.TryGetMethod(symbol));
        }
        foreach(var storage in obj.Storages.Values.OrderBy(value=>value.Id))
        {
            var symbol=new StorageSymbol(obj,storage);string execution=obj.Type(storage.TypeId);
            output.Insert("INSERT INTO storages VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?)",unit,storage.Id,symbol.Key,
                Encoding.BigEndianUnicode.GetBytes(symbol.Key),storage.Definition,storage.SourceName,storage.Linkage,symbol.Signature,
                execution,storage.Alignment,obj.StorageRequiredAlignment(storage),storage.Definition=="declaration"?null:obj.StorageSize(execution),obj.StorageSize(execution));
            foreach(var relocation in obj.Relocations.GetValueOrDefault(storage.Id,[]))
            {
                if(relocation.TargetEntityId is long function)FunctionDependency(output,unit,obj,"storage",storage.Id,function,"relocation",relocation.ByteOffset);
                else StorageDependency(output,unit,obj,"storage",storage.Id,relocation.TargetStorageId!.Value,"relocation",relocation.ByteOffset);
                output.Fact(unit,"relocation",storage.Id,relocation.ByteOffset.ToString(System.Globalization.CultureInfo.InvariantCulture),new {
                    relocation.ByteOffset,relocation.Addend,relocation.TargetEntityId,relocation.TargetStorageId });
            }
        }
        foreach(var action in obj.InitializationActions)
        {
            output.Insert("INSERT INTO startup VALUES(?,?,?,?,?)",unit,action.Ordinal,action.Priority,action.EntityId,new Symbol(obj,obj.Entities[action.EntityId]).Key);
            output.Fact(unit,"startup-origin",action.Ordinal,"",obj.GeneratedRoutineOrigins.ContainsKey(action.EntityId));
        }
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long WriteBody(LinkCatalog output,long unit,ObjectFile obj,long function)
    {
        FunctionBody body=obj.ReadBody(function);
        output.Fact(unit,"function-runtime",function,"",new {
            StaticDestruction=obj.StaticDestructionRegistrations.Values.Any(value=>value.FunctionEntityId==function),
            CleanupFrame=obj.FunctionCleanupStates.ContainsKey(function),
            CleanupArrays=obj.FunctionCleanupActions.GetValueOrDefault(function,[]).Any(value=>obj.CleanupArrays.ContainsKey(value.Id)),
            DynamicSpecifications=obj.DynamicExceptionBoundaries.ContainsKey(function),
            SourceHandlers=body.Nodes.Values.Any(value=>value.Kind=="try"),
            ExceptionProvider=body.Nodes.Values.Any(value=>value.Kind is "try" or "throw" or "rethrow") });
        foreach(var action in obj.FunctionCleanupActions.GetValueOrDefault(function,[]))
            FunctionDependency(output,unit,obj,"function",function,action.CallbackEntityId,"cleanup",action.Id);
        foreach(var action in obj.AllocationCleanupActions.Values.Where(value=>value.FunctionEntityId==function))
            FunctionDependency(output,unit,obj,"function",function,action.CallbackEntityId,"allocation-cleanup",action.Id);
        foreach(var node in body.Nodes.Values)
        {
            if(node.Kind=="throw" && obj.ThrowExpressions[node.Id].DestructorEntityId is long destructor)
                FunctionDependency(output,unit,obj,"function",function,destructor,"throw-destructor",node.Id);
            if(node.EntityId is long entity)FunctionDependency(output,unit,obj,"function",function,entity,"body-node",node.Id);
            if(node.StorageId is long storage)StorageDependency(output,unit,obj,"function",function,storage,"body-node",node.Id);
            if(node.Kind=="storage" && node.TypeId!=obj.Storages[node.StorageId!.Value].TypeId)
                output.Fact(unit,"storage-extent-use",node.Id,function.ToString(System.Globalization.CultureInfo.InvariantCulture),new {
                    Function=function,Storage=node.StorageId,Extent=obj.StorageSize(obj.Type(node.TypeId!.Value)) });
            if(node.Kind=="storage-address")output.Fact(unit,"storage-extent-use",node.Id,function.ToString(System.Globalization.CultureInfo.InvariantCulture),new {
                Function=function,Storage=node.StorageId,Extent=long.Parse(node.ConstantText!,System.Globalization.CultureInfo.InvariantCulture) });
            if(node.Kind is "catch" or "throw" or "rethrow" or "try")
                CatalogContracts.ExceptionUse(output,unit,obj,function,node);
        }
        if(obj.DynamicExceptionBoundaries.TryGetValue(function,out long specification))
            output.Fact(unit,"dynamic-exception-boundary",function,"",obj.DynamicExceptionSpecifications[specification].Types.Select(value=>new {
                Declared=obj.SourceTypeSignature(value.DeclaredSourceTypeId),Match=obj.SourceTypeSignature(value.MatchSourceTypeId) }).ToArray());
        return body.Nodes.Count;
    }
    private static void FunctionDependency(LinkCatalog output,long unit,ObjectFile obj,string kind,long from,long target,string origin,long originId)
    {
        var entity=obj.Entities[target];output.Dependency(unit,kind,from,"function",new Symbol(obj,entity).Key,target,
            entity.IsWeak && entity.Definition=="declaration",origin,originId);
    }
    private static void StorageDependency(LinkCatalog output,long unit,ObjectFile obj,string kind,long from,long target,string origin,long originId)
        =>output.Dependency(unit,kind,from,"storage",new StorageSymbol(obj,obj.Storages[target]).Key,target,false,origin,originId);
}
