using DotObj;

namespace DotObj.Relational;

// This temporary index contains ordinary link identities and selected winners,
// not a second semantic object graph. Input objects remain read-only.
internal sealed class LinkDefinitionIndex : IDisposable
{
    private readonly Sqlite database;
    private readonly IReadOnlyList<Unit> units;
    private readonly Dictionary<string,LinkedListNode<(string Name,SymbolRef? Value)>> cache=new(StringComparer.Ordinal);
    private readonly LinkedList<(string Name,SymbolRef? Value)> recent=[];
    public string Path {get;}
    public int CachedRows=>cache.Count;
    public LinkDefinitionIndex(IReadOnlyList<Unit> units,string? path)
    {
        this.units=units;
        Path=path??System.IO.Path.Combine(System.IO.Path.GetTempPath(),"dotobj-link-"+Guid.NewGuid().ToString("N")+".sqlite");
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Path))!);
        database=Sqlite.CreateCatalog(Path);
        try
        {
            database.Execute("CREATE TABLE inputs(unit_id INTEGER PRIMARY KEY,path TEXT NOT NULL,target_abi TEXT NOT NULL) STRICT");
            database.Execute("CREATE TABLE external_definitions(link_name TEXT PRIMARY KEY,unit_id INTEGER NOT NULL REFERENCES inputs(unit_id),symbol_id INTEGER NOT NULL,definition_kind TEXT NOT NULL) STRICT, WITHOUT ROWID");
            database.Execute("BEGIN");
            using var insert=database.Prepare("INSERT INTO inputs VALUES(?,?,?)");
            for(int index=0;index<units.Count;++index)insert.ExecuteBound(index,units[index].Path,units[index].Abi);
        }
        catch {database.Dispose();throw;}
    }
    public void Add(int unitId,Symbol symbol)
    {
        using(var query=database.Prepare("SELECT definition_kind FROM external_definitions WHERE link_name=?"))
        {
            query.Bind(symbol.LinkName);
            if(query.Read())
            {
                if(query.Text(0)=="strong" && symbol.DefinitionKind=="strong")
                    throw new InvalidDataException("Multiple strong definitions of "+symbol.LinkName);
                if(symbol.DefinitionKind!="strong")return;
            }
        }
        using var insert=database.Prepare("INSERT INTO external_definitions VALUES(?,?,?,?) ON CONFLICT(link_name) DO UPDATE SET unit_id=excluded.unit_id,symbol_id=excluded.symbol_id,definition_kind=excluded.definition_kind");
        insert.ExecuteBound(symbol.LinkName,unitId,symbol.Id,symbol.DefinitionKind);
    }
    public void Complete()=>database.Execute("COMMIT");
    public bool TryGet(string name,out SymbolRef value)
    {
        if(cache.TryGetValue(name,out var cached))
        {
            recent.Remove(cached);recent.AddLast(cached);
            value=cached.Value.Value.GetValueOrDefault();return cached.Value.Value is not null;
        }
        SymbolRef? found=null;
        using(var query=database.Prepare("SELECT unit_id,symbol_id FROM external_definitions WHERE link_name=?"))
        {
            query.Bind(name);
            if(query.Read())found=new(units[checked((int)query.Integer(0))],query.Integer(1));
        }
        cache.Add(name,recent.AddLast((name,found)));
        if(cache.Count>4096){cache.Remove(recent.First!.Value.Name);recent.RemoveFirst();}
        value=found.GetValueOrDefault();return found is not null;
    }
    public void Dispose()=>database.Dispose();
}
