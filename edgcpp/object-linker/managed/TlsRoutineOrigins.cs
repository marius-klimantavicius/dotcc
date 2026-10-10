namespace DotObj;

internal sealed record TlsRoutineOrigin(long EntityId,string Kind,long? SourceStorageId,long? TargetEntityId,string GuardKind);

internal sealed partial class ObjectFile
{
    public Dictionary<long,TlsRoutineOrigin> TlsRoutineOrigins { get; } = [];
    private void ReadTlsRoutineOrigins()
    {
        using var query=database.Prepare("SELECT entity_id,kind,source_storage_id,target_entity_id,guard_kind,capture_proof FROM tls_routine_origins ORDER BY entity_id");
        while(query.Read())
        {
            var value=new TlsRoutineOrigin(query.Integer(0),query.Text(1),query.OptionalInteger(2),query.OptionalInteger(3),query.Text(4));
            if(SourceLanguage!="c++" || !Entities.TryGetValue(value.EntityId,out var entity) ||
                Parameters.GetValueOrDefault(entity.Id,[]).Count!=0 || Routines[entity.RoutineTypeId].Variadic ||
                value.Kind is not ("variable-wrapper" or "initializer-alias" or "empty-initializer") ||
                query.Text(5)!="observed-tls-variable-factory" || !TlsRoutineOrigins.TryAdd(entity.Id,value))
                throw new InvalidDataException("TLS routine lacks its exact observed factory identity/ABI");
            if(value.Kind=="empty-initializer")
            {
                if(value.SourceStorageId is not null || value.TargetEntityId is not null || value.GuardKind!="none" ||
                    entity.Linkage!="internal" || entity.Definition!="strong" || Type(entity.ReturnTypeId)!="void" || !Functions.ContainsKey(entity.Id))
                    throw new InvalidDataException("Empty TLS initializer differs from its observed TU factory");
                continue;
            }
            if(value.SourceStorageId is not long storageId || !Storages.TryGetValue(storageId,out var storage) ||
                !storage.IsThreadLocal || storage.SourceTypeId is null)
                throw new InvalidDataException("TLS wrapper/alias lacks its original thread-local storage");
            if(value.Kind=="variable-wrapper")
            {
                if(value.GuardKind is not ("weak-address" or "always") || value.TargetEntityId is null ||
                    Type(entity.ReturnTypeId)!="ptr("+Type(storage.TypeId)+")" || !Functions.ContainsKey(entity.Id))
                    throw new InvalidDataException("TLS wrapper differs from its source variable pointer ABI");
            }
            else if(value.GuardKind!="none" || Type(entity.ReturnTypeId)!="void" ||
                Functions.ContainsKey(entity.Id)!=(value.TargetEntityId is not null) ||
                (value.TargetEntityId is null ? entity.Definition!="declaration" : storage.Definition=="declaration"))
                throw new InvalidDataException("TLS initializer alias differs from its imported or defined disposition");
        }
        foreach(var value in TlsRoutineOrigins.Values)
        {
            if(value.TargetEntityId is not long target)continue;
            if(!TlsRoutineOrigins.TryGetValue(target,out var origin) ||
                (value.Kind=="variable-wrapper" ? origin.Kind!="initializer-alias" || origin.SourceStorageId!=value.SourceStorageId
                    : value.Kind!="initializer-alias" || origin.Kind!="empty-initializer"))
                throw new InvalidDataException("TLS factory target is unrelated to the original source variable/TU");
            if(value.GuardKind=="weak-address" && !Entities[target].IsWeak)
                throw new InvalidDataException("TLS conditional initialization lacks an exact weak target");
        }
    }
    private string TlsRoutineSourceSignature(TlsRoutineOrigin value)
    {
        if(value.SourceStorageId is not long id)return "tls-empty-initializer("+EntityRoutine(value.EntityId)+")";
        var storage=Storages[id];
        return value.Kind+"("+new StorageSymbol(this,storage).Key+";"+SourceTypeSignature(storage.SourceTypeId!.Value)+";"+EntityRoutine(value.EntityId)+")";
    }
    public void VerifyTlsRoutineBody(long function,FunctionBody body)
    {
        if(!TlsRoutineOrigins.TryGetValue(function,out var origin))return;
        if(body.Bindings.Count!=0)throw new InvalidDataException("TLS factory unexpectedly owns local/parameter bindings");
        int calls=0,returns=0,guards=0;bool returned=false;
        bool Zero(Node node,int depth=0)
        {
            if(depth>128)throw new InvalidDataException("TLS factory expression exceeds nesting limit");
            if(node.Kind=="convert")return Zero(body.Child(node.Id,"operand"),depth+1);
            return node.Kind=="constant" && node.ConstantText=="0";
        }
        bool Address(Node node,int depth=0)
        {
            if(depth>128)throw new InvalidDataException("TLS factory expression exceeds nesting limit");
            if(node.Kind=="convert")return Address(body.Child(node.Id,"operand"),depth+1);
            return node.Kind=="function-address" && node.EntityId==origin.TargetEntityId;
        }
        bool Guard(Node node,int depth=0)
        {
            if(depth>128)throw new InvalidDataException("TLS factory expression exceeds nesting limit");
            if(node.Kind=="convert")return Guard(body.Child(node.Id,"operand"),depth+1);
            if(Address(node))return true;
            return node.Kind=="binary" && node.Operator=="ne" &&
                ((Address(body.Child(node.Id,"left")) && Zero(body.Child(node.Id,"right"))) ||
                 (Zero(body.Child(node.Id,"left")) && Address(body.Child(node.Id,"right"))));
        }
        bool StorageAddress(Node node,int depth=0)
        {
            if(depth>128)throw new InvalidDataException("TLS factory expression exceeds nesting limit");
            if(node.Kind=="convert")return StorageAddress(body.Child(node.Id,"operand"),depth+1);
            if(node.Kind=="address")
            {
                Node place=body.Child(node.Id,"operand");
                return place.Kind=="storage" && place.StorageId==origin.SourceStorageId;
            }
            return node.Kind=="storage-address" && node.StorageId==origin.SourceStorageId &&
                node.ConstantText is null or "0";
        }
        void Walk(Node node,bool guarded=false,int depth=0)
        {
            if(depth>128)throw new InvalidDataException("TLS factory body exceeds nesting limit");
            if(node.Kind=="block")
            {foreach(var edge in body.Children(node.Id,"statement"))Walk(body.Nodes[edge.ChildId],guarded,depth+1);return;}
            if(node.Kind=="expr-stmt") {Walk(body.Child(node.Id,"value"),guarded,depth+1);return;}
            if(returned)throw new InvalidDataException("TLS factory executes code after returning its variable");
            if(node.Kind=="call" && origin.TargetEntityId is long target && node.EntityId==target &&
                !body.Children(node.Id,"argument").Any() && origin.Kind!="empty-initializer" &&
                (origin.GuardKind=="weak-address")==guarded)
            {++calls;return;}
            if(node.Kind=="if" && origin.Kind=="variable-wrapper" && origin.GuardKind=="weak-address" &&
                !guarded && !body.Children(node.Id,"else").Any() && Guard(body.Child(node.Id,"condition")))
            {++guards;Walk(body.Child(node.Id,"then"),true,depth+1);return;}
            if(node.Kind=="return" && !guarded)
            {
                if(origin.Kind=="variable-wrapper" ? !StorageAddress(body.Child(node.Id,"value"))
                    : body.Children(node.Id,"value").Any())
                    throw new InvalidDataException("TLS factory returns an unrelated source storage/value");
                ++returns;returned=true;return;
            }
            throw new InvalidDataException("TLS factory body contains an unowned operation");
        }
        Walk(body.Nodes[body.RootId]);
        if(calls!=(origin.Kind=="empty-initializer"?0:1) || returns!=1 ||
            guards!=(origin.GuardKind=="weak-address"?1:0))
            throw new InvalidDataException("TLS factory lost its exact initializer/guard/return sequence");
    }
}
