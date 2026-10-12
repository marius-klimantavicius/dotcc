using DotObj;

namespace DotObj.Relational;

// Published objects are immutable for the duration of a link. A lightweight
// file witness detects ordinary replacement/change when an evicted handle is
// reopened; it is not a semantic check or a per-read database hash.
internal sealed class ObjectDatabase : IDisposable
{
    private const int Capacity=4;
    private readonly string path;
    private readonly (long Length,DateTime Modified,DateTime Created) witness;
    private bool disposed;
    private static readonly object poolLock=new();
    private static readonly Dictionary<ObjectDatabase,Connection> connections=[];
    private static readonly LinkedList<ObjectDatabase> recent=[];
    public static int OpenConnections {get {lock(poolLock)return connections.Count;}}
    public static int PeakConnections {get;private set;}
    public static long Evictions {get;private set;}
    public static int ConnectionLimit=>Capacity;
    private sealed class Connection(Sqlite database,LinkedListNode<ObjectDatabase> position)
    {
        public readonly Sqlite Database=database;
        public readonly LinkedListNode<ObjectDatabase> Position=position;
        public int Statements;
    }
    public ObjectDatabase(string path)
    {
        this.path=path;witness=FileWitness();
    }
    private (long,DateTime,DateTime) FileWitness()
    {
        var file=new FileInfo(path);
        if(!file.Exists)throw new FileNotFoundException("Object file does not exist",path);
        return (file.Length,file.LastWriteTimeUtc,file.CreationTimeUtc);
    }
    private Connection Acquire()
    {
        lock(poolLock)
        {
            ObjectDisposedException.ThrowIf(disposed,this);
            if(!connections.TryGetValue(this,out var connection))
            {
                if(FileWitness()!=witness)throw new IOException("Immutable object changed during linking: "+path);
                if(connections.Count==Capacity)
                {
                    var candidate=recent.First;
                    while(candidate is not null && connections[candidate.Value].Statements!=0)candidate=candidate.Next;
                    if(candidate is null)throw new InvalidOperationException("Object connection limit reached with all four handles pinned by active statements");
                    Remove(candidate.Value);++Evictions;
                }
                var database=new Sqlite(path);
                connection=new(database,recent.AddLast(this));connections.Add(this,connection);
                PeakConnections=Math.Max(PeakConnections,connections.Count);
            }
            recent.Remove(connection.Position);recent.AddLast(connection.Position);
            ++connection.Statements;return connection;
        }
    }
    private static void Remove(ObjectDatabase owner)
    {
        var connection=connections[owner];connections.Remove(owner);recent.Remove(connection.Position);connection.Database.Dispose();
    }
    private void Release()
    {
        lock(poolLock)
        {
            var connection=connections[this];--connection.Statements;
            if(disposed && connection.Statements==0)Remove(this);
        }
    }
    public Statement Prepare(string sql)
    {
        var connection=Acquire();
        try {return new(this,connection.Database.Prepare(sql));}
        catch {Release();throw;}
    }
    public void Dispose()
    {
        lock(poolLock)
        {
            disposed=true;
            if(connections.TryGetValue(this,out var connection) && connection.Statements==0)Remove(this);
        }
    }
    public sealed class Statement(ObjectDatabase owner,Sqlite.Statement statement) : IDisposable
    {
        private bool disposed;
        public bool Read()=>statement.Read();
        public long Integer(int column)=>statement.Integer(column);
        public int ColumnCount=>statement.ColumnCount;
        public int ColumnType(int column)=>statement.ColumnType(column);
        public bool IsNull(int column)=>statement.IsNull(column);
        public string Text(int column)=>statement.Text(column);
        public string? OptionalText(int column)=>statement.OptionalText(column);
        public long? OptionalInteger(int column)=>statement.OptionalInteger(column);
        public byte[] Bytes(int column)=>statement.Bytes(column);
        public void Dispose()
        {
            if(disposed)return;disposed=true;
            try {statement.Dispose();}finally {owner.Release();}
        }
    }
}
