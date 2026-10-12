using System.Collections;
using DotObj;

namespace DotObj.Relational;

// Point reads retain a bounded process-wide cache per row family. Enumeration
// streams SQLite rows and does not populate that cache.
internal sealed class SqlRowCatalog<T>(Sqlite database,string table,string columns,
    Func<Sqlite.Statement,T> read) : IReadOnlyDictionary<long,T>, IDisposable where T:class
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
            using var q=database.Prepare("SELECT id FROM "+table+" ORDER BY id");
            while(q.Read())yield return q.Integer(0);
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
        using var q=database.Prepare("SELECT "+columns+" FROM "+table+(where is null?"":" WHERE "+where)+" ORDER BY "+order);
        while(q.Read())yield return read(q);
    }
    public IEnumerator<KeyValuePair<long,T>> GetEnumerator()
    {
        using var q=database.Prepare("SELECT "+columns+" FROM "+table+" ORDER BY id");
        while(q.Read())yield return new(q.Integer(0),read(q));
    }
    IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
    public void Dispose()
    {
        lock(cacheLock)
            foreach(var key in cache.Keys.Where(key=>ReferenceEquals(key.Item1,this)).ToArray())
            {recent.Remove(cache[key]);cache.Remove(key);}
    }
}

internal sealed class SqlTypeParameterCatalog(Sqlite database) : IReadOnlyDictionary<long,List<long>>
{
    public int Count
    {
        get {using var q=database.Prepare("SELECT count(DISTINCT type_id) FROM type_parameters");q.Read();return checked((int)q.Integer(0));}
    }
    public IEnumerable<long> Keys
    {
        get {using var q=database.Prepare("SELECT DISTINCT type_id FROM type_parameters ORDER BY type_id");while(q.Read())yield return q.Integer(0);}
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
