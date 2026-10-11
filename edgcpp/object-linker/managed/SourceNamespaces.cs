namespace DotObj;

internal sealed record SourceNamespace(long Id,long? Parent,string Name,bool Unnamed,bool Inline,bool Internal);
internal sealed partial class ObjectFile
{
    public Dictionary<long,SourceNamespace> SourceNamespaces {get;}=[];
    public Dictionary<(string Kind,long Id),long> SourceNamespaceOwners {get;}=[];
    private readonly Dictionary<long,string> sourceNamespaceKeys=[];
    private readonly Dictionary<long,bool> sourceNamespacePrivate=[];
    private void ReadSourceNamespaces()
    {
        using(var q=database.Prepare("SELECT id,parent_id,source_name,is_unnamed,is_inline,has_internal_linkage,source_file_id,line,column,capture_proof FROM source_namespaces ORDER BY id"))
            while(q.Read())
            {
                var value=new SourceNamespace(q.Integer(0),q.OptionalInteger(1),q.Text(2),q.Integer(3)==1,q.Integer(4)==1,q.Integer(5)==1);
                if(value.Id<=0 || value.Parent==value.Id || value.Unnamed!=(value.Name.Length==0) ||
                    Enumerable.Range(3,3).Any(i=>q.Integer(i) is not(0 or 1)) || q.Integer(7)<0 || q.Integer(8)<0 ||
                    value.Unnamed && (q.OptionalInteger(6) is null || q.Integer(7)<=0 || q.Integer(8)<=0) ||
                    q.Text(9)!="original-namespace-declaration" || !SourceNamespaces.TryAdd(value.Id,value))
                    throw new InvalidDataException("Original namespace declaration lost its name, flags or location");
            }
        foreach(long id in SourceNamespaces.Keys) SourceNamespaceIdentity(id,[]);
        using(var q=database.Prepare("SELECT owner_kind,owner_id,namespace_id,capture_proof FROM source_namespace_owners ORDER BY owner_kind,owner_id"))
            while(q.Read())
            {
                var key=(q.Text(0),q.Integer(1));
                if(key.Item1 is not("type" or "declaration" or "concept" or "template") || key.Item2<=0 ||
                    !SourceNamespaces.ContainsKey(q.Integer(2)) || q.Text(3)!="original-enclosing-namespace" ||
                    !SourceNamespaceOwners.TryAdd(key,q.Integer(2)))
                    throw new InvalidDataException("Original declaration namespace edge lacks its exact owner");
            }
    }
    private string SourceNamespaceIdentity(long id,HashSet<long> active)
    {
        if(sourceNamespaceKeys.TryGetValue(id,out string? known))return known;
        if(active.Count>128 || !active.Add(id) || !SourceNamespaces.TryGetValue(id,out var value))
            throw new InvalidDataException("Original namespace parent graph is absent or cyclic");
        string parent=value.Parent is long parentId?SourceNamespaceIdentity(parentId,active):"";
        bool unitLocal=value.Unnamed || value.Parent is long enclosing && sourceNamespacePrivate[enclosing];
        if(value.Internal && !unitLocal)throw new InvalidDataException("Internal namespace lacks its original unnamed ancestor");
        string key=value.Unnamed?"unnamed:"+SourceIdentityFrame(parent)+SourceIdentityFrame(UnitId):"named:"+SourceIdentityFrame(parent)+SourceIdentityFrame(value.Name);
        sourceNamespacePrivate[id]=unitLocal;sourceNamespaceKeys[id]=key;active.Remove(id);return key;
    }
    private string SourcePrivateNamespace(string kind,long owner)
    {
        return SourceNamespaceOwners.TryGetValue((kind,owner),out long id) && sourceNamespacePrivate[id]?sourceNamespaceKeys[id]:"";
    }
    private string SourceNamespaceNominalIdentity(string kind,long owner,string nominal)
    {
        string key=SourcePrivateNamespace(kind,owner);
        return key.Length==0?nominal:"namespace-private:"+SourceIdentityFrame(key)+SourceIdentityFrame(nominal);
    }
    private string SourceNamespaceIdentitySuffix(string kind,long owner)
    {
        string key=SourcePrivateNamespace(kind,owner);return key.Length==0?"":"N"+SourceIdentityFrame(key);
    }
    private void VerifySourceNamespaces()
    {
        var required=new HashSet<(string,long)>();
        foreach(var value in SourceTypes.Values)if(value.HasNamespaceOwner)required.Add(("type",value.Id));
        foreach(var value in SourceExpressionDeclarations.Values)if(value.ScopeKind=="namespace")required.Add(("declaration",value.Id));
        foreach(var value in SourceConcepts.Values)if(value.HasNamespaceOwner)required.Add(("concept",value.Id));
        foreach(var pair in SourceNamedTemplates)if(pair.Value.HasNamespaceOwner)required.Add(("template",pair.Key));
        if(!required.SetEquals(SourceNamespaceOwners.Keys))throw new InvalidDataException("Original namespace ownership markers differ from the owned declaration edges");
        foreach(var pair in SourceNamespaceOwners)
        {
            long? typeOwner=pair.Key.Kind switch {
                "declaration"=>SourceExpressionDeclarations[pair.Key.Id].OwnerType,
                "template"=>SourceNamedTemplates[pair.Key.Id].Owner,
                _=>null
            };
            if(typeOwner is long original && (!SourceNamespaceOwners.TryGetValue(("type",original),out long parentNamespace) ||
                sourceNamespaceKeys[parentNamespace]!=sourceNamespaceKeys[pair.Value]))
                throw new InvalidDataException("Original member namespace differs from its owning type namespace");
        }
    }
}
