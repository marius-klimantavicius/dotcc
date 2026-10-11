using System.Text.Json;
using System.Text.Json.Nodes;

namespace DotObj;

// Resolution retains no ObjectFile instances. Global winners, dependencies,
// provider activation and the work queue live in the separate SQLite catalog.
internal sealed class CatalogResolver : IDisposable
{
    private readonly Sqlite database;
    private sealed record Abi(CatalogExecutionType Execution, bool Deferred, Dictionary<string,string> Decisions);
    public CatalogResolver(string path)
    {
        database = Sqlite.OpenCatalogForResolution(path);
        using var info = database.Prepare("SELECT format_version,verifier_build,status FROM catalog_info WHERE id=1");
        if (!info.Read() || info.Integer(0)!=LinkCatalog.FormatVersion ||
            info.Text(1)!=typeof(LinkCatalog).Assembly.ManifestModule.ModuleVersionId.ToString() || info.Text(2)!="imported-not-resolved")
            throw new InvalidDataException("Catalog resolution requires a complete import by this reader build");
        database.Execute("PRAGMA cache_size=-8192");
        database.Execute("PRAGMA temp_store=FILE");
        database.Execute("PRAGMA mmap_size=0");
    }
    private void Run(string sql, params object?[] values)
    {
        using var query=database.Prepare(sql); query.ExecuteBound(values);
    }
    private string? Scalar(string sql, params object?[] values)
    {
        using var query=database.Prepare(sql); query.Bind(values);
        return query.Read()?query.Text(0):null;
    }
    public void Resolve(string className = "Program", bool allowExterns = false, bool sharedExceptionProvider = true)
    {
        database.Execute("BEGIN IMMEDIATE");
        try
        {
            database.Execute("CREATE TABLE abi_aliases(raw TEXT PRIMARY KEY,target TEXT NOT NULL,resolved INTEGER NOT NULL) STRICT");
            database.Execute("CREATE TABLE abi_families(raw TEXT NOT NULL,family TEXT NOT NULL,resolved INTEGER NOT NULL,PRIMARY KEY(raw,family)) STRICT");
            database.Execute("CREATE TABLE abi_candidates(family TEXT NOT NULL,raw TEXT NOT NULL,PRIMARY KEY(family,raw)) STRICT");
            database.Execute("CREATE TABLE abi_normalized(raw TEXT PRIMARY KEY,canonical TEXT NOT NULL,resolved INTEGER NOT NULL) STRICT");
            database.Execute("CREATE INDEX abi_normalized_target ON abi_normalized(canonical,resolved)");
            database.Execute("CREATE TABLE winners(kind TEXT NOT NULL,symbol_key TEXT NOT NULL,unit_id INTEGER NOT NULL,local_id INTEGER NOT NULL,selected INTEGER NOT NULL DEFAULT 0,processed INTEGER NOT NULL DEFAULT 0,PRIMARY KEY(kind,symbol_key)) STRICT");
            database.Execute("CREATE INDEX winner_frontier ON winners(selected,processed)");
            database.Execute("CREATE TABLE selected_units(unit_id INTEGER PRIMARY KEY REFERENCES units(id)) STRICT");
            database.Execute("CREATE TABLE weak_references(symbol_key TEXT PRIMARY KEY) STRICT");
            database.Execute("CREATE TABLE selected_records(identity_key TEXT PRIMARY KEY,unit_id INTEGER NOT NULL,local_id INTEGER NOT NULL) STRICT");
            database.Execute("CREATE TABLE allocated_names(name TEXT PRIMARY KEY) STRICT");
            database.Execute("CREATE TABLE names(kind TEXT NOT NULL,symbol_key TEXT NOT NULL,name TEXT NOT NULL,PRIMARY KEY(kind,symbol_key)) STRICT");
            database.Execute("CREATE TABLE externs(kind TEXT NOT NULL,symbol_key TEXT NOT NULL,unit_id INTEGER NOT NULL,local_id INTEGER NOT NULL,is_weak INTEGER NOT NULL,PRIMARY KEY(kind,symbol_key)) STRICT");
            database.Execute("CREATE TABLE storage_requirements(symbol_key TEXT PRIMARY KEY,extent INTEGER NOT NULL) STRICT");
            database.Execute("CREATE TABLE resolution_options(class_name TEXT NOT NULL,allow_externs INTEGER NOT NULL,shared_exception_provider INTEGER NOT NULL) STRICT");
            Run("INSERT INTO resolution_options VALUES(?,?,?)",className,allowExterns,sharedExceptionProvider);
            ResolveAbis();
            ResolveFunctions();
            ResolveStorages();
            SelectProviders();
            ResolveRecords();
            ResolveRequirements(allowExterns,sharedExceptionProvider);
            AllocateFunctionNames(className);
            Run("UPDATE catalog_info SET status='selected-not-generated' WHERE id=1");
            database.Execute("COMMIT");
        }
        catch { database.Execute("ROLLBACK"); throw; }
    }
    private void ResolveRequirements(bool allowExterns,bool sharedExceptionProvider)
    {
        using(var dependencies=database.Prepare("SELECT d.to_kind,d.target_key,d.unit_id,d.target_local_id,d.is_weak FROM dependencies d JOIN winners owner ON owner.kind=d.from_kind AND owner.unit_id=d.unit_id AND owner.local_id=d.from_local_id AND owner.selected=1 LEFT JOIN winners target ON target.kind=d.to_kind AND target.symbol_key=d.target_key AND target.selected=1 WHERE target.symbol_key IS NULL ORDER BY owner.kind,owner.symbol_key,d.origin_id,d.origin"))
            while(dependencies.Read())
            {
                string kind=dependencies.Text(0),key=dependencies.Text(1);long unit=dependencies.Integer(2),id=dependencies.Integer(3);bool weak=dependencies.Integer(4)==1;
                string table=kind=="function"?"functions":"storages",column=kind=="function"?"entity_id":"storage_id";
                using var declaration=database.Prepare($"SELECT linkage{(kind=="function"?",runtime_method,is_deferred":"")} FROM {table} WHERE unit_id=? AND {column}=?");
                declaration.Bind(unit,id);
                if(!declaration.Read())throw new InvalidDataException("Catalog dependency has no declaration");
                if(!weak && (declaration.Text(0)!="external" || !allowExterns && (kind!="function" || declaration.IsNull(1))))
                    throw new InvalidDataException("Unresolved "+kind+": "+key);
                if(kind=="function" && declaration.Integer(2)==1 && !weak)
                {
                    string? completed=Scalar("SELECT callable_execution FROM functions WHERE symbol_key=? AND is_deferred=0 LIMIT 1",key);
                    if(completed is null || Scalar("SELECT raw FROM abi_normalized WHERE canonical=? AND resolved=0 LIMIT 1",Canonical(completed)) is not null)
                        throw new InvalidDataException("Deferred function declaration requires a completed ABI provider: "+key);
                }
                Run("INSERT INTO externs VALUES(?,?,?,?,?) ON CONFLICT(kind,symbol_key) DO UPDATE SET is_weak=min(is_weak,excluded.is_weak)",kind,key,unit,id,weak);
            }
        using(var uses=database.Prepare("SELECT f.unit_id,c.payload FROM facts f JOIN contracts c ON c.kind=f.kind AND c.digest=f.contract_digest JOIN winners w ON w.kind='function' AND w.unit_id=f.unit_id AND w.local_id=cast(f.identity_key AS INTEGER) AND w.selected=1 WHERE f.kind='storage-extent-use'"))
            while(uses.Read())
            {
                using var fact=JsonDocument.Parse(uses.Text(1));var root=fact.RootElement;
                RequireExtent(uses.Integer(0),root.GetProperty("Storage").GetInt64(),root.GetProperty("Extent").GetInt64());
            }
        using(var relocations=database.Prepare("SELECT f.unit_id,c.payload FROM facts f JOIN contracts c ON c.kind=f.kind AND c.digest=f.contract_digest JOIN winners w ON w.kind='storage' AND w.unit_id=f.unit_id AND w.local_id=f.local_id AND w.selected=1 WHERE f.kind='relocation'"))
            while(relocations.Read())
            {
                using var fact=JsonDocument.Parse(relocations.Text(1));var root=fact.RootElement;
                if(root.GetProperty("TargetStorageId") is var target && target.ValueKind!=JsonValueKind.Null)
                    RequireExtent(relocations.Integer(0),target.GetInt64(),root.GetProperty("Addend").GetInt64());
            }
        using(var runtime=database.Prepare("SELECT c.payload FROM facts f JOIN contracts c ON c.kind=f.kind AND c.digest=f.contract_digest JOIN winners w ON w.kind='function' AND w.unit_id=f.unit_id AND w.local_id=f.local_id AND w.selected=1 WHERE f.kind='function-runtime'"))
            while(runtime.Read())
            {
                using var fact=JsonDocument.Parse(runtime.Text(0));var root=fact.RootElement;
                if(root.GetProperty("DynamicSpecifications").GetBoolean() && !allowExterns)
                    throw new InvalidDataException("Dynamic exception specifications require the shared unexpected provider; use --allow-externs for generation-only output");
                if(root.GetProperty("ExceptionProvider").GetBoolean() && !sharedExceptionProvider && !allowExterns)
                    throw new InvalidDataException("Source exceptions require the shared Edg.Runtime.CppExceptions provider; use --allow-externs for generation-only output");
            }
        using var descriptors=database.Prepare("SELECT f.identity_key,c.payload FROM facts f JOIN facts use ON use.unit_id=f.unit_id AND use.local_id=f.local_id AND use.kind='exception-use' JOIN contracts c ON c.kind=f.kind AND c.digest=f.contract_digest JOIN winners w ON w.kind='function' AND w.unit_id=f.unit_id AND w.local_id=cast(use.identity_key AS INTEGER) AND w.selected=1 WHERE f.kind='exception-descriptor' ORDER BY f.identity_key");
        string? identity=null;JsonNode? previous=null;
        while(descriptors.Read())
        {
            string next=descriptors.Text(0);var value=JsonNode.Parse(descriptors.Text(1));
            if(value!["Bases"] is JsonArray bases)value["Bases"]=new JsonArray(bases.OrderBy(item=>item!.ToJsonString(),StringComparer.Ordinal).Select(item=>item!.DeepClone()).ToArray());
            if(identity==next && !JsonNode.DeepEquals(previous,value))throw new InvalidDataException("Exception descriptor layout or resolved base facts disagree across translation units");
            identity=next;previous=value;
        }
    }
    private void RequireExtent(long unit,long id,long extent)
    {
        using var storage=database.Prepare("SELECT s.symbol_key,coalesce(definition.extent,s.declared_extent),w.symbol_key FROM storages s LEFT JOIN winners w ON w.kind='storage' AND w.symbol_key=s.symbol_key AND w.selected=1 LEFT JOIN storages definition ON definition.unit_id=w.unit_id AND definition.storage_id=w.local_id WHERE s.unit_id=? AND s.storage_id=?");
        storage.Bind(unit,id);
        if(!storage.Read())throw new InvalidDataException("Catalog storage use has no declaration");
        string key=storage.Text(0);long size=storage.Integer(1);
        if(extent<0 || (!storage.IsNull(2) || size!=0) && extent>size)throw new InvalidDataException("Storage address exceeds resolved extent: "+key);
        Run("INSERT INTO storage_requirements VALUES(?,?) ON CONFLICT(symbol_key) DO UPDATE SET extent=max(extent,excluded.extent)",key,extent);
    }
    private void AllocateFunctionNames(string className)
    {
        bool Contains(string name)=>Scalar("SELECT name FROM allocated_names WHERE name=?",name) is not null;
        bool Add(string name)
        {
            if(Contains(name))return false;
            Run("INSERT INTO allocated_names VALUES(?)",name);return true;
        }
        var names=new Names(Add,Contains);names.Reserve(className);
        using var functions=database.Prepare("SELECT w.symbol_key,f.source_name,f.entity_id FROM winners w JOIN functions f ON f.unit_id=w.unit_id AND f.entity_id=w.local_id WHERE w.kind='function' AND w.selected=1 ORDER BY f.sort_key");
        while(functions.Read())Run("INSERT INTO names VALUES('function',?,?)",functions.Text(0),names.Allocate(functions.Text(1),"function_"+functions.Integer(2)));
        using(var externs=database.Prepare("SELECT e.symbol_key,f.source_name,f.entity_id FROM externs e JOIN functions f ON f.unit_id=e.unit_id AND f.entity_id=e.local_id WHERE e.kind='function' AND e.is_weak=0 ORDER BY e.rowid"))
            while(externs.Read())Run("INSERT INTO names VALUES('function',?,?)",externs.Text(0),names.Allocate(externs.Text(1),"extern_"+externs.Integer(2)));
        using(var storages=database.Prepare("SELECT w.symbol_key,s.source_name,s.storage_id FROM winners w JOIN storages s ON s.unit_id=w.unit_id AND s.storage_id=w.local_id WHERE w.kind='storage' AND w.selected=1 ORDER BY s.sort_key"))
            while(storages.Read())Run("INSERT INTO names VALUES('storage',?,?)",storages.Text(0),names.AllocateProperty(storages.Text(1),"__literal_"+storages.Integer(2)));
        using(var storages=database.Prepare("SELECT e.symbol_key,s.source_name,s.storage_id FROM externs e JOIN storages s ON s.unit_id=e.unit_id AND s.storage_id=e.local_id WHERE e.kind='storage' ORDER BY s.sort_key"))
            while(storages.Read())Run("INSERT INTO names VALUES('storage',?,?)",storages.Text(0),names.AllocateProperty(storages.Text(1),"extern_storage_"+storages.Integer(2)));
    }
    private void ResolveAbis()
    {
        using(var families=database.Prepare("SELECT DISTINCT identity_key FROM facts WHERE kind='routine-abi' ORDER BY identity_key"))
            while(families.Read())
            {
                string family=families.Text(0);
                using var values=database.Prepare("SELECT c.payload FROM facts f JOIN contracts c ON c.kind=f.kind AND c.digest=f.contract_digest JOIN units u ON u.id=f.unit_id WHERE f.kind='routine-abi' AND f.identity_key=? ORDER BY u.unit_id,f.local_id");
                values.Bind(family); var observations=new List<Abi>();
                while(values.Read()) observations.Add(JsonSerializer.Deserialize<Abi>(values.Text(0))!);
                if(!observations.Any(value=>value.Deferred))continue;
                var decisions=new Dictionary<string,string>(StringComparer.Ordinal);
                foreach(var observation in observations)
                    foreach(var (slot,decision) in observation.Decisions)
                    {
                        if(decisions.TryGetValue(slot,out string? previous) && previous!=decision)
                            throw new InvalidDataException("Original routine signature has conflicting known partial ABI decisions");
                        decisions[slot]=decision;
                    }
                var complete=observations.Where(value=>!value.Deferred).ToArray();
                bool resolved=complete.Length!=0;
                string target=resolved?complete[0].Execution.RawExecutionType:observations.Select(value=>value.Execution.RawExecutionType).Order(StringComparer.Ordinal).First();
                foreach(var value in observations.Where(value=>value.Deferred))
                {
                    string raw=value.Execution.RawExecutionType;
                    string? previous=Scalar("SELECT target FROM abi_aliases WHERE raw=?",raw);
                    if(previous is not null && previous!=target)throw new InvalidDataException("Deferred routine has conflicting ABI resolutions");
                    Run("INSERT INTO abi_aliases VALUES(?,?,?) ON CONFLICT(raw) DO UPDATE SET resolved=max(resolved,excluded.resolved)",raw,target,resolved);
                    Run("INSERT OR IGNORE INTO abi_families VALUES(?,?,?)",raw,family,resolved);
                }
                foreach(var value in complete)Run("INSERT OR IGNORE INTO abi_candidates VALUES(?,?)",family,value.Execution.RawExecutionType);
            }
        if(Scalar("SELECT raw FROM abi_families GROUP BY raw HAVING min(resolved)<>max(resolved) LIMIT 1") is not null)
            throw new InvalidDataException("Ambiguous deferred routine key has complete and incomplete original semantic families");
        using var candidates=database.Prepare("SELECT family,raw FROM abi_candidates ORDER BY family,raw");
        string? current=null,prior=null;
        while(candidates.Read())
        {
            string family=candidates.Text(0), canonical=Canonical(candidates.Text(1));
            if(family==current && canonical!=prior)throw new InvalidDataException("Original routine signature has competing completed ABIs");
            current=family;prior=canonical;
        }
        using(var aliases=database.Prepare("SELECT raw,resolved FROM abi_aliases"))
            while(aliases.Read())Run("INSERT INTO abi_normalized VALUES(?,?,?)",aliases.Text(0),Canonical(aliases.Text(0)),aliases.Integer(1));
        if(Scalar("SELECT canonical FROM abi_normalized GROUP BY canonical HAVING min(resolved)<>max(resolved) LIMIT 1") is not null)
            throw new InvalidDataException("Completed routine ABI aliases an unresolved original semantic family");
    }
    internal string Canonical(string type) => Canonical(type,[],0);
    private string Canonical(string type, HashSet<string> active, int depth)
    {
        if(depth>128 || !active.Add(type))throw new InvalidDataException("Cyclic deferred routine ABI resolution");
        string Resolve(string nested)=>Canonical(nested,active,depth+1);
        string? target=Scalar("SELECT target FROM abi_aliases WHERE raw=?",type);
        string result;
        if(target is not null && target!=type)result=Resolve(target);
        else if(ScalarTypes.IsPointer(type))result="ptr("+Resolve(ScalarTypes.Pointee(type))+")";
        else if(ScalarTypes.IsArray(type))result=type[..(type.IndexOf(',')+1)]+Resolve(ScalarTypes.Element(type))+")";
        else if(RoutineTypes.IsRoutine(type))
        {
            var routine=RoutineTypes.Signature(type);
            result=RoutineTypes.Key(Resolve(routine.Result),routine.Parameters.Select(Resolve),routine.Variadic,routine.ExceptionPolicy);
        }
        else result=type;
        active.Remove(type);return result;
    }
    private void ResolveFunctions()
    {
        using var functions=database.Prepare("SELECT f.symbol_key,f.unit_id,f.entity_id,f.definition,f.callable_execution,f.source_signature,f.semantic_signature,f.is_deferred,f.is_original_ordinary,f.recorded_abi FROM functions f JOIN units u ON u.id=f.unit_id ORDER BY f.sort_key,u.provider,u.input_ordinal,f.entity_id");
        string? key=null,signature=null,source=null,semantic=null,strength=null,abi=null;
        long winnerUnit=0,winnerId=0; bool deferred=false,ordinary=true;
        void Finish()
        {
            if(key is null)return;
            if(deferred && !ordinary)throw new InvalidDataException("Deferred original declaration cannot resolve to a generated or special-member origin");
            if(strength is not null)Run("INSERT INTO winners(kind,symbol_key,unit_id,local_id) VALUES('function',?,?,?)",key,winnerUnit,winnerId);
        }
        while(functions.Read())
        {
            string next=functions.Text(0),candidate=Canonical(functions.Text(4));
            if(next!=key){Finish();key=next;signature=null;source=null;semantic=null;strength=null;abi=null;deferred=false;ordinary=true;}
            if(signature is not null && signature!=candidate)throw new InvalidDataException("Signature mismatch for "+key);
            signature=candidate;
            string? nextSource=functions.OptionalText(5),nextSemantic=functions.OptionalText(6);
            if(source is not null && nextSource is not null && !ObjectFile.CompatibleSourceSignatures(source,nextSource))throw new InvalidDataException("Source signature mismatch for "+key);
            if(semantic is not null && nextSemantic is not null && !ObjectFile.CompatibleSourceSignatures(semantic,nextSemantic))throw new InvalidDataException("Original semantic signature mismatch for "+key);
            if(!ObjectFile.IsLocalSourceObservation(nextSource))source??=nextSource;
            if(!ObjectFile.IsLocalSourceObservation(nextSemantic))semantic??=nextSemantic;
            string? nextAbi=functions.OptionalText(9);
            if(abi is not null && nextAbi is not null && abi!=nextAbi)throw new InvalidDataException("Recorded ABI role mismatch for "+key);
            abi??=nextAbi;
            deferred|=functions.Integer(7)==1;ordinary&=functions.Integer(8)==1;
            string definition=functions.Text(3);
            if(definition=="declaration")continue;
            if(strength=="strong" && definition=="strong")throw new InvalidDataException("Duplicate strong definition: "+key);
            if(strength is null || definition=="strong"){strength=definition;winnerUnit=functions.Integer(1);winnerId=functions.Integer(2);}
        }
        Finish();
    }
    private void ResolveStorages()
    {
        using var storages=database.Prepare("SELECT s.symbol_key,s.unit_id,s.storage_id,s.definition,s.signature,s.alignment,s.required_alignment,s.execution_type FROM storages s JOIN units u ON u.id=s.unit_id ORDER BY s.sort_key,u.provider,u.input_ordinal,s.storage_id");
        string? key=null,signature=null,strength=null,execution=null;long winnerUnit=0,winnerId=0,alignment=0,required=1;
        void Finish()
        {
            if(strength is null)return;
            if(alignment<required)throw new InvalidDataException("Storage definition does not satisfy the required declaration alignment: "+key);
            Run("INSERT INTO winners(kind,symbol_key,unit_id,local_id) VALUES('storage',?,?,?)",key,winnerUnit,winnerId);
        }
        while(storages.Read())
        {
            string next=storages.Text(0),candidate=storages.Text(4);
            if(next!=key){Finish();key=next;signature=null;strength=null;execution=null;required=1;}
            string nextExecution=Canonical(storages.Text(7))+candidate[candidate.LastIndexOf(':')..];
            if(execution is not null && !CompatibleStorage(execution,nextExecution))throw new InvalidDataException("Storage execution ABI/TLS mismatch: "+key);
            if(execution is null || ScalarTypes.IsIncompleteArray(execution[..execution.LastIndexOf(':')]))execution=nextExecution;
            if(signature is not null && !CompatibleStorage(signature,candidate,sourceObservations:true))throw new InvalidDataException("Storage type/TLS mismatch: "+key);
            if(!ObjectFile.IsLocalSourceObservation(candidate) && (signature is null || ScalarTypes.IsIncompleteArray(signature[..signature.LastIndexOf(':')])))signature=candidate;
            required=Math.Max(required,storages.Integer(6));
            string definition=storages.Text(3);if(definition=="declaration")continue;
            if(strength=="strong" && definition=="strong")throw new InvalidDataException("Duplicate strong storage definition: "+key);
            if(strength is null || definition=="strong" || strength=="tentative" && definition=="linkonce")
            {strength=definition;winnerUnit=storages.Integer(1);winnerId=storages.Integer(2);alignment=storages.Integer(5);}
        }
        Finish();
    }
    private static bool CompatibleStorage(string left,string right,bool sourceObservations=false)
    {
        if(left==right)return true;
        int a=left.LastIndexOf(':'),b=right.LastIndexOf(':');
        if(a<0 || b<0 || left[a..]!=right[b..])return false;
        string x=left[..a],y=right[..b];
        if(sourceObservations && (ObjectFile.IsLocalSourceObservation(x) || ObjectFile.IsLocalSourceObservation(y)))return true;
        return ScalarTypes.IsArray(x) && ScalarTypes.IsArray(y) && ScalarTypes.Element(x)==ScalarTypes.Element(y) &&
            (ScalarTypes.IsIncompleteArray(x) || ScalarTypes.IsIncompleteArray(y));
    }
    private void SelectProviders()
    {
        database.Execute("INSERT INTO selected_units SELECT id FROM units WHERE provider=0");
        database.Execute("UPDATE winners SET selected=1 WHERE (kind='function' AND symbol_key IN (SELECT f.symbol_key FROM functions f JOIN units u ON u.id=f.unit_id WHERE u.provider=0 AND f.definition<>'declaration')) OR (kind='storage' AND symbol_key IN (SELECT s.symbol_key FROM storages s JOIN units u ON u.id=s.unit_id WHERE u.provider=0 AND s.definition<>'declaration'))");
        while(true)
        {
            string kind,key;long unit,id;
            using(var next=database.Prepare("SELECT kind,symbol_key,unit_id,local_id FROM winners WHERE selected=1 AND processed=0 LIMIT 1"))
            {
                if(!next.Read())break;
                kind=next.Text(0);key=next.Text(1);unit=next.Integer(2);id=next.Integer(3);
            }
            Run("UPDATE winners SET processed=1 WHERE kind=? AND symbol_key=?",kind,key);
            if(Scalar("SELECT cast(unit_id AS TEXT) FROM selected_units WHERE unit_id=?",unit) is null)
            {
                Run("INSERT INTO selected_units VALUES(?)",unit);
                Run("UPDATE winners SET selected=1 WHERE kind='function' AND ((unit_id=? AND symbol_key IN (SELECT symbol_key FROM weak_references)) OR symbol_key IN (SELECT symbol_key FROM startup WHERE unit_id=?))",unit,unit);
            }
            using var dependencies=database.Prepare("SELECT to_kind,target_key,is_weak FROM dependencies WHERE unit_id=? AND from_kind=? AND from_local_id=?");
            dependencies.Bind(unit,kind,id);
            while(dependencies.Read())
            {
                string targetKind=dependencies.Text(0),target=dependencies.Text(1);
                bool weak=dependencies.Integer(2)==1;
                if(weak && Scalar("SELECT cast(w.unit_id AS TEXT) FROM winners w JOIN units u ON u.id=w.unit_id LEFT JOIN selected_units s ON s.unit_id=u.id WHERE w.kind='function' AND w.symbol_key=? AND u.provider=1 AND s.unit_id IS NULL",target) is not null)
                    Run("INSERT OR IGNORE INTO weak_references VALUES(?)",target);
                else Run("UPDATE winners SET selected=1 WHERE kind=? AND symbol_key=?",targetKind,target);
            }
        }
    }
    private JsonNode NormalizeContract(string payload)
    {
        JsonNode root=JsonNode.Parse(payload)!;
        void Visit(JsonNode? node)
        {
            if(node is JsonObject obj)
            {
                if(obj.TryGetPropertyValue("RawExecutionType",out var raw))obj["RawExecutionType"]=Canonical(raw!.GetValue<string>());
                foreach(var value in obj.ToArray())Visit(value.Value);
            }
            else if(node is JsonArray array)foreach(var value in array)Visit(value);
        }
        Visit(root);return root;
    }
    private void ResolveRecords()
    {
        using var records=database.Prepare("SELECT f.identity_key,f.unit_id,f.local_id,c.payload FROM facts f JOIN selected_units s ON s.unit_id=f.unit_id JOIN units u ON u.id=f.unit_id JOIN contracts c ON c.kind=f.kind AND c.digest=f.contract_digest WHERE f.kind='record' ORDER BY f.identity_key,u.unit_id,f.local_id");
        string? key=null;JsonNode? previous=null;long unit=0,id=0;
        void Finish(){if(key is not null)Run("INSERT INTO selected_records VALUES(?,?,?)",key,unit,id);}
        while(records.Read())
        {
            string next=records.Text(0);var value=NormalizeContract(records.Text(3));
            if(next!=key){Finish();key=next;previous=value;unit=records.Integer(1);id=records.Integer(2);continue;}
            long oldSize=previous!["Size"]!.GetValue<long>(),size=value["Size"]!.GetValue<long>();
            if(oldSize==0 || size==0)
            {
                foreach(string field in new[]{"SourceName","LayoutKind","RepresentationKind"})
                    if(!JsonNode.DeepEquals(previous[field],value[field]))throw new InvalidDataException("Incomplete record identity mismatch");
                if(oldSize==0 && size!=0){previous=value;unit=records.Integer(1);id=records.Integer(2);}
                continue;
            }
            var a=previous.DeepClone();var b=value.DeepClone();
            if(a["Source"] is JsonObject sourceA && b["Source"] is JsonObject sourceB)
            {
                if(sourceA.ContainsKey("LocalObservation") || sourceB.ContainsKey("LocalObservation"))
                { a["Source"]=null; b["Source"]=null; }
                else
                {
                if(sourceA["Language"]!.GetValue<string>()!="c++" || sourceB["Language"]!.GetValue<string>()!="c++")
                {sourceA.Remove("Operations");sourceB.Remove("Operations");}
                sourceA.Remove("Language");sourceB.Remove("Language");
                }
            }
            if(!CompatibleRecordContract(a,b))throw new InvalidDataException("Record layout mismatch for "+key);
        }
        Finish();
    }
    private static bool CompatibleRecordContract(JsonNode? left, JsonNode? right, bool sourceObservationAllowed = false)
    {
        if(sourceObservationAllowed && (left is JsonValue a && a.TryGetValue<string>(out var x) && ObjectFile.IsLocalSourceObservation(x) ||
            right is JsonValue b && b.TryGetValue<string>(out var y) && ObjectFile.IsLocalSourceObservation(y))) return true;
        if(left is JsonObject ao && right is JsonObject bo)
            return ao.Count==bo.Count && ao.All(pair=>bo.ContainsKey(pair.Key) && CompatibleRecordContract(pair.Value,bo[pair.Key],sourceObservationAllowed || pair.Key is "Source" or "SourceSignature" or "BitWidthIdentity"));
        if(left is JsonArray aa && right is JsonArray ba)
            return aa.Count==ba.Count && aa.Select((value,index)=>CompatibleRecordContract(value,ba[index],sourceObservationAllowed)).All(value=>value);
        return JsonNode.DeepEquals(left,right);
    }
    public void Dispose()=>database.Dispose();
}
