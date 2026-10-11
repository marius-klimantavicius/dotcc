using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DotObj;

internal sealed record CatalogInput(string Path,bool Provider);
internal sealed record CatalogUnitDescriptor(long Id,string Path,string FileHash,long ByteLength,string Digest,
    string UnitId,string PrimarySource,string Language,bool Provider,long InputOrdinal,long FunctionCount,long BodyCount);
// Execution atoms remain distinguishable until the global deferred-ABI pass.
internal sealed record CatalogExecutionType(string RawExecutionType);

internal sealed class LinkCatalog : IDisposable
{
    private readonly Sqlite database;
    private readonly Dictionary<string,Sqlite.Statement> inserts=new(StringComparer.Ordinal);
    private bool finished;
    public const int FormatVersion=1;
    public string Path {get;}
    public LinkCatalog(string path)
    {
        Path=System.IO.Path.GetFullPath(path);database=Sqlite.CreateCatalog(Path);
        database.Execute("PRAGMA application_id=1146047555");
        database.Execute("PRAGMA user_version=1");
        database.Execute("PRAGMA cache_size=-8192");
        database.Execute("PRAGMA temp_store=FILE");
        database.Execute("PRAGMA mmap_size=0");
        foreach(string command in Schema)database.Execute(command);
        Insert("INSERT INTO catalog_info VALUES(1,?,?,?,?)",FormatVersion,
            typeof(LinkCatalog).Assembly.ManifestModule.ModuleVersionId.ToString(),"importing","trusted-unit-import-v3");
    }
    public void BeginUnit()=>database.Execute("BEGIN IMMEDIATE");
    public void CommitUnit()=>database.Execute("COMMIT");
    public void RollbackUnit()=>database.Execute("ROLLBACK");
    public void Insert(string sql,params object?[] values)
    {
        if(finished)throw new InvalidOperationException("Catalog is complete");
        if(!inserts.TryGetValue(sql,out var command))inserts.Add(sql,command=database.Prepare(sql));
        command.ExecuteBound(values);
    }
    public string Contract(string kind,object value)
    {
        string payload=value is string text?text:JsonSerializer.Serialize(value);
        byte[] bytes=Encoding.UTF8.GetBytes(payload);string digest=Convert.ToHexStringLower(SHA256.HashData(bytes));
        Insert("INSERT OR IGNORE INTO contracts VALUES(?,?,?)",kind,digest,payload);
        using var q=database.Prepare("SELECT payload FROM contracts WHERE kind=? AND digest=?");q.Bind(kind,digest);
        if(!q.Read() || q.Text(0)!=payload || q.Read())throw new InvalidDataException("Catalog contract hash collision or corrupt interning");
        return digest;
    }
    public void Fact(long unit,string kind,long localId,string key,object value)
    {
        string digest=Contract(kind,value);
        Insert("INSERT INTO facts VALUES(?,?,?,?,?)",unit,kind,localId,key,digest);
    }
    public void Dependency(long unit,string fromKind,long localId,string toKind,string key,long targetId,bool weak,string origin,long originId)
        =>Insert("INSERT OR IGNORE INTO dependencies VALUES(?,?,?,?,?,?,?,?,?)",unit,fromKind,localId,toKind,key,targetId,weak,origin,originId);
    public void Complete()
    {
        using(var check=database.Prepare("PRAGMA foreign_key_check"))
            if(check.Read())throw new InvalidDataException("Catalog has broken foreign keys");
        using(var check=database.Prepare("PRAGMA integrity_check"))
            if(!check.Read() || check.Text(0)!="ok" || check.Read())throw new InvalidDataException("Catalog integrity check failed");
        Insert("UPDATE catalog_info SET status=? WHERE id=1","imported-not-resolved");finished=true;
    }
    public void Dispose()
    {
        foreach(var command in inserts.Values)command.Dispose();database.Dispose();
    }
    private static readonly string[] Schema=[
        "CREATE TABLE catalog_info(id INTEGER PRIMARY KEY CHECK(id=1),format_version INTEGER NOT NULL,verifier_build TEXT NOT NULL,status TEXT NOT NULL,contract_version TEXT NOT NULL) STRICT",
        "CREATE TABLE units(id INTEGER PRIMARY KEY,path TEXT NOT NULL,file_sha256 TEXT NOT NULL,byte_length INTEGER NOT NULL,logical_sha256 TEXT NOT NULL,unit_id TEXT NOT NULL UNIQUE,primary_source TEXT NOT NULL,source_language TEXT NOT NULL,provider INTEGER NOT NULL CHECK(provider IN(0,1)),input_ordinal INTEGER NOT NULL UNIQUE,function_count INTEGER NOT NULL,verified_body_count INTEGER NOT NULL) STRICT",
        "CREATE TABLE contracts(kind TEXT NOT NULL,digest TEXT NOT NULL,payload TEXT NOT NULL,PRIMARY KEY(kind,digest)) STRICT,WITHOUT ROWID",
        "CREATE TABLE facts(unit_id INTEGER NOT NULL REFERENCES units(id),kind TEXT NOT NULL,local_id INTEGER NOT NULL,identity_key TEXT NOT NULL,contract_digest TEXT NOT NULL,PRIMARY KEY(unit_id,kind,local_id,identity_key),FOREIGN KEY(kind,contract_digest) REFERENCES contracts(kind,digest)) STRICT,WITHOUT ROWID",
        "CREATE INDEX facts_identity ON facts(kind,identity_key,unit_id)",
        "CREATE TABLE functions(unit_id INTEGER NOT NULL REFERENCES units(id),entity_id INTEGER NOT NULL,symbol_key TEXT NOT NULL,sort_key BLOB NOT NULL,definition TEXT NOT NULL,source_name TEXT NOT NULL,linkage TEXT NOT NULL,is_weak INTEGER NOT NULL,raw_execution TEXT NOT NULL,callable_execution TEXT NOT NULL,source_signature TEXT,semantic_signature TEXT,is_deferred INTEGER NOT NULL,is_original_ordinary INTEGER NOT NULL,runtime_method TEXT,recorded_abi TEXT,PRIMARY KEY(unit_id,entity_id)) STRICT,WITHOUT ROWID",
        "CREATE INDEX functions_symbol ON functions(symbol_key,unit_id)",
        "CREATE TABLE storages(unit_id INTEGER NOT NULL REFERENCES units(id),storage_id INTEGER NOT NULL,symbol_key TEXT NOT NULL,sort_key BLOB NOT NULL,definition TEXT NOT NULL,source_name TEXT NOT NULL,linkage TEXT NOT NULL,signature TEXT NOT NULL,execution_type TEXT NOT NULL,alignment INTEGER NOT NULL,required_alignment INTEGER NOT NULL,extent INTEGER,declared_extent INTEGER NOT NULL,PRIMARY KEY(unit_id,storage_id)) STRICT,WITHOUT ROWID",
        "CREATE INDEX storages_symbol ON storages(symbol_key,unit_id)",
        "CREATE TABLE dependencies(unit_id INTEGER NOT NULL REFERENCES units(id),from_kind TEXT NOT NULL,from_local_id INTEGER NOT NULL,to_kind TEXT NOT NULL,target_key TEXT NOT NULL,target_local_id INTEGER NOT NULL,is_weak INTEGER NOT NULL,origin TEXT NOT NULL,origin_id INTEGER NOT NULL,PRIMARY KEY(unit_id,from_kind,from_local_id,to_kind,target_key,origin,origin_id)) STRICT,WITHOUT ROWID",
        "CREATE INDEX dependencies_source ON dependencies(unit_id,from_kind,from_local_id)",
        "CREATE TABLE startup(unit_id INTEGER NOT NULL REFERENCES units(id),ordinal INTEGER NOT NULL,priority INTEGER NOT NULL,entity_id INTEGER NOT NULL,symbol_key TEXT NOT NULL,PRIMARY KEY(unit_id,ordinal)) STRICT,WITHOUT ROWID",
        "CREATE TABLE unit_metrics(unit_id INTEGER PRIMARY KEY REFERENCES units(id),elapsed_milliseconds INTEGER NOT NULL,body_node_count INTEGER NOT NULL) STRICT"
    ];
}
