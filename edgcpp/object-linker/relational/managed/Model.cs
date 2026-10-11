namespace DotObj.Relational;

internal sealed record ProgramType(long Id, string Kind, string Name, long? Size, long? Alignment,
    long? Pointee, long? Element, long? Count, long? Result, long? Underlying, bool Variadic, bool Incomplete,
    string LinkName, string Linkage, bool Volatile,string? BuiltinName);
internal sealed record Field(long Id, long Owner, string Name, long Type, long Ordinal, long Offset, long? BitOffset, long? BitWidth);
internal sealed record Symbol(long Id, string Kind, string Name, string LinkName, string Linkage, long Type, long? Parent, bool Definition, string DefinitionKind);
internal sealed record Node(long Id, long? Function, string Kind, long? Type, long? ExecutionType,
    string? ValueCategory, string? Operator, string? Constant, long? Symbol, long? Field, long? File, long? Line, long? Column, long? OriginalLvalueType,
    long? Initialization,long? Lifetime,string? EvaluationOrder);
internal sealed record Edge(string Role, long Ordinal, long Child);
internal readonly record struct SymbolRef(Unit Unit, long Id)
{
    public Symbol Symbol => Unit.Symbols[Id];
}

// This reader materializes explicit program relations. It does not type-check
// expressions, select overloads or reconstruct EDG's language decisions.
internal sealed partial class Unit : IDisposable
{
    public string Path { get; }
    public string Source { get; }
    public string Abi { get; }
    public bool HasLifecycles {get;}
    public Dictionary<long, ProgramType> Types { get; } = [];
    public Dictionary<long, Field> Fields { get; } = [];
    public Dictionary<long, Symbol> Symbols { get; } = [];
    public Dictionary<long, long> Functions { get; } = [];
    public Dictionary<long, List<long>> Parameters { get; } = [];
    public Dictionary<long, List<long>> TypeParameters { get; } = [];
    public Dictionary<long, Node> Nodes { get; } = [];
    public Dictionary<long, List<Edge>> Edges { get; } = [];
    public Dictionary<long, long> Initializers { get; } = [];
    public Dictionary<long, string> Files { get; } = [];
    private readonly Sqlite database;

    public Unit(string path)
    {
        Path = System.IO.Path.GetFullPath(path);
        database = new Sqlite(Path);
        try
        {
            using (var query = database.Prepare("SELECT format,primary_source,target_abi FROM object_info WHERE singleton=1"))
            {
                if (!query.Read() || query.Text(0) != "relational-program-v1")
                    throw new InvalidDataException("Unsupported relational object: " + Path);
                Source = query.Text(1); Abi = query.Text(2);
            }
            using(var query=database.Prepare("SELECT 1 FROM sqlite_master WHERE type='table' AND name='lifetimes'"))
                HasLifecycles=query.Read();
            using (var q = database.Prepare("SELECT id,path FROM files ORDER BY id"))
                while (q.Read()) Files.Add(q.Integer(0), q.Text(1));
            using (var q = database.Prepare("SELECT id,kind,name,size,alignment,pointee_type_id,element_type_id,element_count,return_type_id,underlying_type_id,is_variadic,is_incomplete,link_name,linkage,is_volatile,"+(HasLifecycles?"builtin_name":"NULL")+" FROM types ORDER BY id"))
                while (q.Read()) Types.Add(q.Integer(0), new(q.Integer(0),q.Text(1),q.Text(2),q.OptionalInteger(3),q.OptionalInteger(4),q.OptionalInteger(5),q.OptionalInteger(6),q.OptionalInteger(7),q.OptionalInteger(8),q.OptionalInteger(9),q.Integer(10)!=0,q.Integer(11)!=0,q.Text(12),q.Text(13),q.Integer(14)!=0,q.OptionalText(15)));
            using (var q = database.Prepare("SELECT id,owner_type_id,name,type_id,ordinal,byte_offset,bit_offset,bit_width FROM fields ORDER BY owner_type_id,ordinal"))
                while (q.Read()) Fields.Add(q.Integer(0), new(q.Integer(0),q.Integer(1),q.Text(2),q.Integer(3),q.Integer(4),q.Integer(5),q.OptionalInteger(6),q.OptionalInteger(7)));
            using (var q = database.Prepare("SELECT id,kind,name,link_name,linkage,type_id,parent_symbol_id,is_definition,definition_kind FROM symbols ORDER BY id"))
                while (q.Read()) Symbols.Add(q.Integer(0),new(q.Integer(0),q.Text(1),q.Text(2),q.Text(3),q.Text(4),q.Integer(5),q.OptionalInteger(6),q.Integer(7)!=0,q.Text(8)));
            using (var q = database.Prepare("SELECT symbol_id,body_node_id FROM functions ORDER BY symbol_id"))
                while (q.Read()) Functions.Add(q.Integer(0),q.Integer(1));
            using (var q = database.Prepare("SELECT function_id,symbol_id FROM parameters ORDER BY function_id,ordinal"))
                while (q.Read()) Add(Parameters,q.Integer(0),q.Integer(1));
            using (var q = database.Prepare("SELECT type_id,parameter_type_id FROM type_parameters ORDER BY type_id,ordinal"))
                while (q.Read()) Add(TypeParameters,q.Integer(0),q.Integer(1));
            using (var q = database.Prepare("SELECT id,function_id,kind,type_id,execution_type_id,value_category,operator,constant_text,symbol_id,field_id,file_id,line,column,original_lvalue_type_id,"+(HasLifecycles?"initialization_id,lifetime_id,evaluation_order":"NULL,NULL,NULL")+" FROM nodes ORDER BY id"))
                while (q.Read()) Nodes.Add(q.Integer(0),new(q.Integer(0),q.OptionalInteger(1),q.Text(2),q.OptionalInteger(3),q.OptionalInteger(4),q.OptionalText(5),q.OptionalText(6),q.OptionalText(7),q.OptionalInteger(8),q.OptionalInteger(9),q.OptionalInteger(10),q.OptionalInteger(11),q.OptionalInteger(12),q.OptionalInteger(13),q.OptionalInteger(14),q.OptionalInteger(15),q.OptionalText(16)));
            using (var q = database.Prepare("SELECT parent_id,role,ordinal,child_id FROM edges ORDER BY parent_id,role,ordinal"))
                while (q.Read()) Add(Edges,q.Integer(0),new Edge(q.Text(1),q.Integer(2),q.Integer(3)));
            using (var q = database.Prepare("SELECT symbol_id,initializer_node_id FROM global_initializers ORDER BY symbol_id"))
                while (q.Read()) Initializers.Add(q.Integer(0),q.Integer(1));
            ReadLifecycles();
        }
        catch { database.Dispose(); throw; }
    }

    private static void Add<T>(Dictionary<long,List<T>> map,long key,T value)
    {
        if (!map.TryGetValue(key,out var values)) map.Add(key,values=[]);
        values.Add(value);
    }
    public IEnumerable<Node> Children(Node node,string role) => Edges.GetValueOrDefault(node.Id,[])
        .Where(edge=>edge.Role==role).Select(edge=>Nodes[edge.Child]);
    public Node Child(Node node,string role) => Children(node,role).SingleOrDefault()
        ?? throw Missing(node,"missing " + role + " edge");
    public ProgramType Original(long id)
    {
        ProgramType value=Types[id];
        while (value.Kind is "qualified" or "typedef") value=Types[value.Underlying ?? throw new InvalidDataException("Type wrapper has no target")];
        return value;
    }
    public bool IsVolatile(long id)
    {
        ProgramType value=Types[id];
        while(!value.Volatile && value.Kind is "qualified" or "typedef") value=Types[value.Underlying!.Value];
        return value.Volatile;
    }
    public InvalidDataException Missing(Node node,string message) => new($"{Files.GetValueOrDefault(node.File??0,Source)}:{node.Line}:{node.Column}: node {node.Id} ({node.Kind}/{node.Operator}): {message}");
    public void Dispose() => database.Dispose();
}

internal sealed class LinkedProgram : IDisposable
{
    public List<Unit> Units { get; } = [];
    public List<SymbolRef> Functions { get; } = [];
    public List<SymbolRef> Globals { get; } = [];
    private readonly Dictionary<string,SymbolRef> external = new(StringComparer.Ordinal);
    public LinkedProgram(IEnumerable<string> paths)
    {
        try
        {
            foreach(string path in paths) Units.Add(new Unit(path));
            if(Units.Select(unit=>unit.Abi).Distinct(StringComparer.Ordinal).Count()!=1)
                throw new InvalidDataException("Cannot link objects with different recorded target ABIs");
            foreach(var unit in Units)
                foreach(var symbol in unit.Symbols.Values.Where(symbol=>symbol.Definition && symbol.Parent is null))
                {
                    var value=new SymbolRef(unit,symbol.Id);
                    if(symbol.Linkage=="external" && !external.TryAdd(symbol.LinkName,value))
                    {
                        var previous=external[symbol.LinkName];
                        if(previous.Symbol.DefinitionKind=="strong" && symbol.DefinitionKind=="strong")
                            throw new InvalidDataException("Multiple strong definitions of " + symbol.LinkName);
                        if(symbol.DefinitionKind=="strong") external[symbol.LinkName]=value;
                    }
                }
            foreach(var unit in Units)
                foreach(var symbol in unit.Symbols.Values.Where(symbol=>symbol.Definition && symbol.Parent is null))
                {
                    var value=new SymbolRef(unit,symbol.Id);
                    if(Resolve(unit,symbol.Id)!=value)continue;
                    if(unit.Functions.ContainsKey(symbol.Id)) Functions.Add(value);
                    else if(symbol.Kind=="global") Globals.Add(value);
                }
        }
        catch { Dispose(); throw; }
    }
    public SymbolRef Resolve(Unit unit,long id)
    {
        var symbol=unit.Symbols[id];
        return symbol.Linkage=="external" && external.TryGetValue(symbol.LinkName,out var selected)?selected:new(unit,id);
    }
    public void Dispose() { foreach(var unit in Units)unit.Dispose(); }
}
