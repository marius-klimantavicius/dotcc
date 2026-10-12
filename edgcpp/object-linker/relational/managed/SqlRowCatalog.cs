using System.Collections;

namespace DotObj.Relational;

// Point reads retain a bounded process-wide cache per row family. Enumeration
// streams SQLite rows and does not populate that cache.
internal sealed class SqlRowCatalog<T>(ObjectDatabase database,string table,string columns,
    Func<ObjectDatabase.Statement,T> read) : IReadOnlyDictionary<long,T>, IDisposable where T:class
{
    private const int Capacity=4096;
    private static readonly Dictionary<(SqlRowCatalog<T>,long),LinkedListNode<(SqlRowCatalog<T> Owner,long Id,T Value)>> cache=[];
    private static readonly LinkedList<(SqlRowCatalog<T> Owner,long Id,T Value)> recent=[];
    private static readonly object cacheLock=new();
    private int? count;
    public int Count
    {
        get
        {
            if(count is int total)return total;
            using var q=database.Prepare("SELECT count(*) FROM "+table);q.Read();
            return (count=checked((int)q.Integer(0))).Value;
        }
    }
    public int CachedRows {get {lock(cacheLock)return cache.Keys.Count(key=>ReferenceEquals(key.Item1,this));}}
    public IEnumerable<T> Values=>Query();
    public IEnumerable<long> Keys
    {
        get
        {
            long? after=null;
            while(true)
            {
                var batch=new List<long>(256);
                using(var q=database.Prepare("SELECT id FROM "+table+(after is null?"":" WHERE id>"+after)+" ORDER BY id LIMIT 256"))
                    while(q.Read())batch.Add(q.Integer(0));
                foreach(long id in batch)yield return id;
                if(batch.Count<256)yield break;after=batch[^1];
            }
        }
    }
    public T this[long id]=>TryGetValue(id,out var value)?value:throw new KeyNotFoundException(table+" row "+id);
    public bool ContainsKey(long id)=>TryGetValue(id,out _);
    public bool TryGetValue(long id,out T value)
    {
        lock(cacheLock)
            if(cache.TryGetValue((this,id),out var entry))
            {
                recent.Remove(entry);recent.AddLast(entry);value=entry.Value.Value;return true;
            }
        using var q=database.Prepare("SELECT "+columns+" FROM "+table+" WHERE id="+id);
        if(!q.Read()){value=null!;return false;}
        value=read(q);
        lock(cacheLock)
        {
            if(cache.TryGetValue((this,id),out var previous)){recent.Remove(previous);cache.Remove((this,id));}
            var entry=recent.AddLast((this,id,value));cache.Add((this,id),entry);
            if(cache.Count>Capacity){var first=recent.First!;cache.Remove((first.Value.Owner,first.Value.Id));recent.RemoveFirst();}
        }
        return true;
    }
    // Clauses come only from typed internal lookup methods and numeric IDs.
    public IEnumerable<T> Query(string? where=null,string order="id")
    {
        foreach(var pair in QueryPairs(where,order))yield return pair.Value;
    }
    public IEnumerator<KeyValuePair<long,T>> GetEnumerator()
    {
        return QueryPairs(null,"id").GetEnumerator();
    }
    private IEnumerable<KeyValuePair<long,T>> QueryPairs(string? where,string order)
    {
        long? after=null;
        for(int offset=0;;offset+=256)
        {
            string? filter=where;
            if(order=="id" && after is not null)filter=(where is null?"":"("+where+") AND ")+"id>"+after;
            var batch=new List<KeyValuePair<long,T>>(256);
            using(var q=database.Prepare("SELECT "+columns+" FROM "+table+(filter is null?"":" WHERE "+filter)+" ORDER BY "+order+" LIMIT 256"+(order=="id"?"":" OFFSET "+offset)))
                while(q.Read())batch.Add(new(q.Integer(0),read(q)));
            foreach(var row in batch)yield return row;
            if(batch.Count<256)yield break;after=batch[^1].Key;
        }
    }
    IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
    public void Dispose()
    {
        lock(cacheLock)
            foreach(var key in cache.Keys.Where(key=>ReferenceEquals(key.Item1,this)).ToArray())
            {recent.Remove(cache[key]);cache.Remove(key);}
    }
}

internal sealed class SqlTypeParameterCatalog(ObjectDatabase database) : IReadOnlyDictionary<long,List<long>>
{
    public int Count
    {
        get {using var q=database.Prepare("SELECT count(DISTINCT type_id) FROM type_parameters");q.Read();return checked((int)q.Integer(0));}
    }
    public IEnumerable<long> Keys
    {
        get
        {
            long? after=null;
            while(true)
            {
                var batch=new List<long>(256);
                using(var q=database.Prepare("SELECT DISTINCT type_id FROM type_parameters"+(after is null?"":" WHERE type_id>"+after)+" ORDER BY type_id LIMIT 256"))
                    while(q.Read())batch.Add(q.Integer(0));
                foreach(long id in batch)yield return id;
                if(batch.Count<256)yield break;after=batch[^1];
            }
        }
    }
    public IEnumerable<List<long>> Values=>Keys.Select(key=>this[key]);
    public List<long> this[long id]=>TryGetValue(id,out var value)?value:throw new KeyNotFoundException("type_parameters "+id);
    public bool ContainsKey(long id)=>TryGetValue(id,out _);
    public bool TryGetValue(long id,out List<long> value)
    {
        value=[];
        using var q=database.Prepare("SELECT parameter_type_id FROM type_parameters WHERE type_id="+id+" ORDER BY ordinal");
        while(q.Read())value.Add(q.Integer(0));return value.Count!=0;
    }
    public IEnumerator<KeyValuePair<long,List<long>>> GetEnumerator()=>Keys.Select(key=>new KeyValuePair<long,List<long>>(key,this[key])).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
}
