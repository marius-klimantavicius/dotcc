using System.Globalization;
using System.Text.Json;

namespace DotObj;

// Global emission state is SQLite data. A view may cache facts for its current
// unit, but this owner never retains ObjectFile or FunctionBody instances.
internal sealed class CatalogEmissionPlan : IDisposable
{
    private readonly Sqlite database;
    public Names Members {get;}
    private readonly Dictionary<(string,bool),bool> runtimeFlags=[];
    public CatalogEmissionPlan(string path,string className,bool sharedExceptionProvider)
    {
        database=Sqlite.OpenCatalogForResolution(path);
        try
        {
            using var info=database.Prepare("SELECT format_version,verifier_build,status FROM catalog_info WHERE id=1");
            if(!info.Read() || info.Integer(0)!=LinkCatalog.FormatVersion ||
                info.Text(1)!=typeof(LinkCatalog).Assembly.ManifestModule.ModuleVersionId.ToString() ||
                info.Text(2)!="selected-not-generated")
                throw new InvalidDataException("Emission requires a resolved catalog from this reader build");
            using var options=database.Prepare("SELECT class_name,shared_exception_provider FROM resolution_options");
            if(!options.Read() || options.Text(0)!=className || (options.Integer(1)==1)!=sharedExceptionProvider || options.Read())
                throw new InvalidDataException("Generation options differ from the resolved catalog");
            database.Execute("PRAGMA cache_size=-8192");database.Execute("PRAGMA temp_store=FILE");database.Execute("PRAGMA mmap_size=0");
            database.Execute("BEGIN IMMEDIATE");
            database.Execute("CREATE TABLE emission_members(name TEXT PRIMARY KEY) STRICT");
            database.Execute("CREATE TABLE emission_names(kind TEXT NOT NULL,identity TEXT NOT NULL,ordinal INTEGER NOT NULL,name TEXT NOT NULL,PRIMARY KEY(kind,identity,ordinal)) STRICT");
            database.Execute("CREATE TABLE emission_claims(kind TEXT NOT NULL,identity TEXT NOT NULL,PRIMARY KEY(kind,identity)) STRICT");
            database.Execute("CREATE TABLE emission_storage(symbol_key TEXT PRIMARY KEY,byte_offset INTEGER NOT NULL,extent INTEGER NOT NULL,is_tls INTEGER NOT NULL,alignment INTEGER NOT NULL) STRICT");
            database.Execute("CREATE TABLE emission_reports(kind TEXT NOT NULL,identity TEXT NOT NULL,payload TEXT NOT NULL,PRIMARY KEY(kind,identity)) STRICT");
            database.Execute("INSERT INTO emission_members SELECT name FROM allocated_names");
            Members=new Names(AddMember,name=>Scalar("SELECT name FROM emission_members WHERE name=?",name) is not null);
        }
        catch { database.Dispose(); throw; }
    }
    private bool AddMember(string name)
    {
        Run("INSERT OR IGNORE INTO emission_members VALUES(?)",name);
        return Scalar("SELECT changes()")!="0";
    }
    public void Run(string sql,params object?[] values)
    {using var q=database.Prepare(sql);q.ExecuteBound(values);}
    public string? Scalar(string sql,params object?[] values)
    {using var q=database.Prepare(sql);q.Bind(values);return q.Read()?q.Text(0):null;}
    public IEnumerable<(string Key,long Id)> Locators(string kind,long unit)
    {
        using var q=database.Prepare("SELECT symbol_key,local_id FROM winners WHERE kind=? AND unit_id=? AND selected=1 ORDER BY symbol_key");
        q.Bind(kind,unit);while(q.Read())yield return(q.Text(0),q.Integer(1));
    }
    public IEnumerable<(long Node,string Kind)> ExceptionUses(long unit)
    {
        using var q=database.Prepare("SELECT f.local_id,c.payload FROM facts f JOIN contracts c ON c.kind=f.kind AND c.digest=f.contract_digest JOIN winners w ON w.kind='function' AND w.unit_id=f.unit_id AND w.local_id=cast(f.identity_key AS INTEGER) AND w.selected=1 WHERE f.kind='exception-use' AND f.unit_id=? ORDER BY w.symbol_key,f.local_id");
        q.Bind(unit);while(q.Read())
        {using var payload=JsonDocument.Parse(q.Text(1));yield return(q.Integer(0),payload.RootElement.GetProperty("Kind").GetString()!);}
    }
    public IEnumerable<CatalogUnitDescriptor> Units()
    {
        using var q=database.Prepare("SELECT id,path,file_sha256,byte_length,logical_sha256,unit_id,primary_source,source_language,provider,input_ordinal,function_count,verified_body_count FROM units ORDER BY unit_id");
        while(q.Read())yield return new(q.Integer(0),q.Text(1),q.Text(2),q.Integer(3),q.Text(4),q.Text(5),q.Text(6),q.Text(7),q.Integer(8)==1,q.Integer(9),q.Integer(10),q.Integer(11));
    }
    public void Report(string kind,string key,object value)=>Run("INSERT INTO emission_reports VALUES(?,?,?)",kind,key,JsonSerializer.Serialize(value));
    public void WriteReport(string path,string namespaceName,string className,bool programArguments,bool shared)
    {
        using var output=File.Create(path);using var writer=new Utf8JsonWriter(output,new JsonWriterOptions{Indented=true});
        writer.WriteStartObject();writer.WriteNumber("formatVersion",1);writer.WriteString("profile","scalar-control-v66");writer.WriteString("status","generated");
        writer.WriteString("targetAbi","portable-lp64-binary80-v2");writer.WriteString("runtimeAbi","dotobj-scalar-v1");
        writer.WritePropertyName("capabilities");JsonSerializer.Serialize(writer,new {
            sourceDeclarations=true,macroDefinitions=true,lexicalCleanups=true,cleanupStates=true,sourceLifetimes=true,prelowerSource=false,macros=false,lifetimes=false });
        writer.WriteString("namespace",namespaceName);writer.WriteString("class",className);writer.WriteString("resolution","disk-catalog");
        writer.WriteString("sourceMap","source-map.json");writer.WriteString("macroMap","macro-map.json");writer.WriteString("lifetimeMap","lifetime-map.json");
        writer.WritePropertyName("runtimeHooks");JsonSerializer.Serialize(writer,new {
            model="object-program-v2",container="global::Edg.Runtime.ObjectProgram",
            entryWrapper=programArguments?"Run(delegate* managed<int,sbyte**,int>,int,sbyte**)":"Run(delegate* managed<int>)",
            automaticLibraryEntry=false,programArguments=programArguments?"owned-writable-utf8-until-run-completes":"none",
            libraryInitialization="global::Edg.Runtime.ObjectProgram.InitializeLibrary",executableInitializesLibrary=true,
            startupActionsRunOutsideTypeInitializer=true,startupFailureCached=true,shutdownActionsSupported=false,
            staticDestructionRegistry=RuntimeFlag("StaticDestruction",true)?"global::Edg.Runtime.CppFinalizers.Program":null,
            libraryFinalization=RuntimeFlag("StaticDestruction",true)?"global::Edg.Runtime.ObjectProgram.FinalizeLibrary":null,
            optionalHooks=new[]{"Enter","Leave","NormalExit","TryHandleTransfer","ClassifyTransfer"},
            transferClassifier="global::Edg.Runtime.ObjectProgram.IsRuntimeTransfer",
            ownedThreadStorageRelease="global::Edg.Runtime.ObjectProgram.ReleaseThreadStorage",
            ownedThreadStorageReleaseOrder="after-thread-finalizers-and-tss-destructors" });
        bool exceptions=RuntimeFlag("ExceptionProvider",true)||RuntimeFlag("DynamicSpecifications",true);
        writer.WritePropertyName("sourceExceptionRuntime");JsonSerializer.Serialize(writer,new {
            required=exceptions,container="global::Edg.Runtime.CppExceptions",
            status=!exceptions?"not-required":shared?"shared-provider":"generation-only-unbound-provider",
            dynamicSpecificationsRequired=RuntimeFlag("DynamicSpecifications",true),
            dynamicSpecificationsStatus=RuntimeFlag("DynamicSpecifications",true)?"generation-only-unbound-unexpected-provider":"not-required",
            sourceHandlersSupported=true,sourceHandlerKinds=new[]{"ellipsis","typed"},sourceTryKinds=new[]{"ordinary","constructor","destructor"},
            ownsPayloadInitialization=true,ownsStartAndCompletionOrdering=true });
        foreach(string kind in new[]{"objects","symbols","records","runtimeRequirements","runtimeStorageRequirements"})
        {
            writer.WriteStartArray(kind);using var q=database.Prepare("SELECT payload FROM emission_reports WHERE kind=? ORDER BY identity");q.Bind(kind);
            while(q.Read()){using var value=JsonDocument.Parse(q.Text(0));value.RootElement.WriteTo(writer);}
            writer.WriteEndArray();
        }
        writer.WriteEndObject();
    }
    public void WriteSelection(string path)
    {
        using var output=File.Create(path);using var writer=new Utf8JsonWriter(output,new JsonWriterOptions{Indented=true});
        long Count(string sql)=>long.Parse(Scalar(sql)!,CultureInfo.InvariantCulture);
        writer.WriteStartObject();writer.WriteNumber("formatVersion",1);writer.WriteString("policy","primary-definitions-and-reachable-providers");
        writer.WriteNumber("candidateFunctions",Count("SELECT count(*) FROM winners WHERE kind='function'"));
        writer.WriteNumber("selectedFunctions",Count("SELECT count(*) FROM winners WHERE kind='function' AND selected=1"));
        writer.WriteNumber("candidateStorages",Count("SELECT count(*) FROM winners WHERE kind='storage'"));
        writer.WriteNumber("selectedStorages",Count("SELECT count(*) FROM winners WHERE kind='storage' AND selected=1"));
        writer.WriteNumber("startupActions",Count("SELECT count(*) FROM startup a JOIN selected_units s ON s.unit_id=a.unit_id"));
        writer.WriteStartArray("objects");
        using(var q=database.Prepare("SELECT u.unit_id,u.logical_sha256,u.provider,s.unit_id IS NOT NULL FROM units u LEFT JOIN selected_units s ON s.unit_id=u.id ORDER BY u.unit_id"))
            while(q.Read())
            {
                writer.WriteStartObject();writer.WriteString("unitId",q.Text(0));writer.WriteString("digest",q.Text(1));
                writer.WriteString("role",q.Integer(2)==1?"provider":"primary");writer.WriteBoolean("selected",q.Integer(3)==1);writer.WriteEndObject();
            }
        writer.WriteEndArray();writer.WriteEndObject();
    }
    public bool Selected(string kind,string key)=>Scalar("SELECT symbol_key FROM winners WHERE kind=? AND symbol_key=? AND selected=1",kind,key) is not null;
    public bool External(string kind,string key)=>Scalar("SELECT symbol_key FROM externs WHERE kind=? AND symbol_key=? AND is_weak=0",kind,key) is not null;
    public bool UndefinedWeak(string key)=>Scalar("SELECT symbol_key FROM externs WHERE kind='function' AND symbol_key=? AND is_weak=1",key) is not null;
    public bool ExternalOwner(string kind,string key,long unit,long id)=>Scalar("SELECT symbol_key FROM externs WHERE kind=? AND symbol_key=? AND unit_id=? AND local_id=? AND is_weak=0",kind,key,unit,id) is not null;
    public string? SymbolName(string kind,string key)=>Scalar("SELECT name FROM names WHERE kind=? AND symbol_key=?",kind,key);
    public string Name(string kind,string key,long ordinal=0)=>Scalar("SELECT name FROM emission_names WHERE kind=? AND identity=? AND ordinal=?",kind,key,ordinal)
        ??throw new InvalidDataException("Missing catalog emission name: "+kind+":"+key);
    public string Allocate(string kind,string key,string source,string fallback,long ordinal=0,bool property=false)
    {
        string? existing=Scalar("SELECT name FROM emission_names WHERE kind=? AND identity=? AND ordinal=?",kind,key,ordinal);
        if(existing is not null)return existing;
        string value=property?Members.AllocateProperty(source,fallback):Members.Allocate(source,fallback);
        Run("INSERT INTO emission_names VALUES(?,?,?,?)",kind,key,ordinal,value);return value;
    }
    public bool Claim(string kind,string key)
    {Run("INSERT OR IGNORE INTO emission_claims VALUES(?,?)",kind,key);return Scalar("SELECT changes()")!="0";}
    public bool Unresolved(string type)=>Scalar("SELECT raw FROM abi_normalized WHERE canonical=? AND resolved=0 LIMIT 1",type) is not null;
    public string Canonical(string type)=>Canonical(type,[],0);
    private string Canonical(string type,HashSet<string> active,int depth)
    {
        if(depth>128 || !active.Add(type))throw new InvalidDataException("Cyclic deferred ABI in catalog");
        string Resolve(string value)=>Canonical(value,active,depth+1);
        string? target=Scalar("SELECT target FROM abi_aliases WHERE raw=?",type);
        string value;
        if(target is not null && target!=type)value=Resolve(target);
        else if(ScalarTypes.IsPointer(type))value="ptr("+Resolve(ScalarTypes.Pointee(type))+")";
        else if(ScalarTypes.IsArray(type))value=type[..(type.IndexOf(',')+1)]+Resolve(ScalarTypes.Element(type))+")";
        else if(RoutineTypes.IsRoutine(type))
        {var signature=RoutineTypes.Signature(type);value=RoutineTypes.Key(Resolve(signature.Result),signature.Parameters.Select(Resolve),signature.Variadic,signature.ExceptionPolicy);}
        else value=type;
        active.Remove(type);return value;
    }
    public void PrepareNames()
    {
        using(var records=database.Prepare("SELECT r.identity_key,c.payload FROM selected_records r JOIN facts f ON f.kind='record' AND f.unit_id=r.unit_id AND f.local_id=r.local_id JOIN contracts c ON c.kind=f.kind AND c.digest=f.contract_digest ORDER BY r.identity_key"))
            while(records.Read())
            {
                string key=records.Text(0);using var payload=JsonDocument.Parse(records.Text(1));var value=payload.RootElement;
                string name=Allocate("record",key,value.GetProperty("SourceName").GetString()+(value.GetProperty("RepresentationKind").GetString()=="subobject"?"__subobject":""),"Record");
                var fields=new Names();fields.Reserve(name);long ordinal=0;
                foreach(var field in value.GetProperty("Fields").EnumerateArray())
                {Run("INSERT INTO emission_names VALUES('field',?,?,?)",key,ordinal,fields.Allocate(field.GetProperty("SourceName").GetString()!,"field_"+ordinal));ordinal++;}
            }
        // Canonical arrays can collapse across units. Deduplicate in SQLite.
        Run("CREATE TABLE emission_arrays(type TEXT PRIMARY KEY) STRICT");
        using(var arrays=database.Prepare("SELECT DISTINCT identity_key FROM facts f JOIN selected_units u ON u.unit_id=f.unit_id WHERE f.kind='array-type' ORDER BY identity_key"))
            while(arrays.Read())Run("INSERT OR IGNORE INTO emission_arrays VALUES(?)",Canonical(arrays.Text(0)));
        using(var arrays=database.Prepare("SELECT type FROM emission_arrays ORDER BY type"))
            while(arrays.Read())Allocate("array",arrays.Text(0),ScalarTypes.ArrayName(arrays.Text(0)),"__DotObjArray");
    }
    public long StorageOffset(string key)=>long.Parse(Scalar("SELECT byte_offset FROM emission_storage WHERE symbol_key=?",key)
        ??throw new InvalidDataException("Missing selected storage offset"),CultureInfo.InvariantCulture);
    public (long GlobalSize,long GlobalAlignment,long TlsSize,long TlsAlignment) PrepareStorage()
    {
        long globalSize=0,tlsSize=0,globalAlignment=8,tlsAlignment=8;
        using var storages=database.Prepare("SELECT s.symbol_key,s.extent,s.alignment,s.signature FROM winners w JOIN storages s ON s.unit_id=w.unit_id AND s.storage_id=w.local_id WHERE w.kind='storage' AND w.selected=1 ORDER BY s.sort_key");
        while(storages.Read())
        {
            bool tls=storages.Text(3).EndsWith(":tls",StringComparison.Ordinal);
            long alignment=storages.Integer(2),extent=storages.Integer(1);
            ref long size=ref(tls?ref tlsSize:ref globalSize);
            size=checked((size+alignment-1)/alignment*alignment);
            Run("INSERT INTO emission_storage VALUES(?,?,?,?,?)",storages.Text(0),size,extent,tls,alignment);
            size=checked(size+extent);
            if(tls)tlsAlignment=Math.Max(tlsAlignment,alignment);else globalAlignment=Math.Max(globalAlignment,alignment);
        }
        if(globalSize>int.MaxValue-globalAlignment || tlsSize>int.MaxValue-tlsAlignment)
            throw new InvalidDataException("Pinned storage arena exceeds managed array limit");
        return(globalSize,globalAlignment,tlsSize,tlsAlignment);
    }
    public IEnumerable<string> Arrays()
    {using var q=database.Prepare("SELECT type FROM emission_arrays ORDER BY type");while(q.Read())yield return q.Text(0);}
    public IEnumerable<long> StorageUnits(bool tls)
    {
        using var q=database.Prepare("SELECT DISTINCT w.unit_id FROM winners w JOIN emission_storage s ON s.symbol_key=w.symbol_key JOIN units u ON u.id=w.unit_id WHERE w.kind='storage' AND s.is_tls=? ORDER BY u.unit_id");
        q.Bind(tls);while(q.Read())yield return q.Integer(0);
    }
    public IEnumerable<(string Reference,bool Generated)> StartupCalls(string owner)
    {
        using var q=database.Prepare("SELECT n.name,c.payload FROM startup a JOIN selected_units s ON s.unit_id=a.unit_id JOIN units u ON u.id=a.unit_id JOIN names n ON n.kind='function' AND n.symbol_key=a.symbol_key JOIN facts f ON f.kind='startup-origin' AND f.unit_id=a.unit_id AND f.local_id=a.ordinal JOIN contracts c ON c.kind=f.kind AND c.digest=f.contract_digest ORDER BY a.priority,u.input_ordinal,a.ordinal");
        while(q.Read())yield return(owner+"."+q.Text(0),q.Text(1)=="true");
    }
    public bool RuntimeFlag(string property,bool functions)
    {
        if(runtimeFlags.TryGetValue((property,functions),out bool cached))return cached;
        string query=functions
            ?"SELECT c.payload FROM facts f JOIN contracts c ON c.kind=f.kind AND c.digest=f.contract_digest JOIN winners w ON w.kind='function' AND w.unit_id=f.unit_id AND w.local_id=f.local_id AND w.selected=1 WHERE f.kind='function-runtime'"
            :"SELECT c.payload FROM facts f JOIN contracts c ON c.kind=f.kind AND c.digest=f.contract_digest WHERE f.kind='unit-runtime'";
        using var q=database.Prepare(query);while(q.Read())
        {using var value=JsonDocument.Parse(q.Text(0));if(value.RootElement.GetProperty(property).GetBoolean())return runtimeFlags[(property,functions)]=true;}
        return runtimeFlags[(property,functions)]=false;
    }
    public void Complete()
    {Run("UPDATE catalog_info SET status='generated' WHERE id=1");database.Execute("COMMIT");}
    public void Dispose()=>database.Dispose();
}
