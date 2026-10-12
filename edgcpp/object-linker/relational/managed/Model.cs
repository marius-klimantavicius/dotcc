namespace DotObj.Relational;

internal sealed record ProgramType(long Id, string Kind, string Name, long? Size, long? Alignment,
    long? Pointee, long? Element, long? Count, long? Result, long? Underlying, bool Variadic, bool Incomplete,
    string LinkName, string Linkage, bool Volatile,string? BuiltinName,bool Dependent,string ReturnStorage,string? SemanticRole,string ExceptionPolicy,long? ObjectType,
    string? ArrayBoundKind,long? ArrayBoundNode,bool? ParametersIncludeReceiver);
internal sealed record Field(long Id, long Owner, string Name, long Type, long Ordinal, long Offset, long? BitOffset, long? BitWidth);
internal sealed record Symbol(long Id, string Kind, string Name, string LinkName, string Linkage, long Type, long? Parent, bool Definition, string DefinitionKind);
internal sealed record Node(long Id, long? Function, string Kind, long? Type, long? ExecutionType,
    string? ValueCategory, string? Operator, string? Constant, long? Symbol, long? Field, long? File, long? Line, long? Column, long? OriginalLvalueType,
    long? Initialization,long? Lifetime,string? EvaluationOrder,long? EnumValue,long? FloatingValue,string? ValueNormalization,long? OperationType,
    long? LiteralObject,long? LiteralByteOffset,long? CommonLifetime,long? ContainingLifetime);
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
    public bool HasReturnStorage {get;}
    public bool HasValueNormalization {get;}
    public bool HasNodeDomain {get;}
    public bool HasOperationType {get;}
    public bool HasControlLifetimes {get;}
    public Dictionary<long, ProgramType> Types { get; } = [];
    public Dictionary<long, Field> Fields { get; } = [];
    public Dictionary<long, Symbol> Symbols { get; } = [];
    public Dictionary<long, long> Functions { get; } = [];
    public Dictionary<long, List<long>> Parameters { get; } = [];
    public Dictionary<long, List<long>> TypeParameters { get; } = [];
    public Dictionary<long, Node> Nodes => Body.Nodes;
    public Dictionary<long, List<Edge>> Edges => Body.Edges;
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
            bool hasDependentTypes;
            using(var query=database.Prepare("SELECT 1 FROM pragma_table_info('types') WHERE name='is_dependent'"))
                hasDependentTypes=query.Read();
            bool hasSemanticRole;
            using(var query=database.Prepare("SELECT 1 FROM pragma_table_info('types') WHERE name='semantic_role'"))
                hasSemanticRole=query.Read();
            bool hasExceptionPolicy;
            using(var query=database.Prepare("SELECT 1 FROM pragma_table_info('types') WHERE name='exception_policy'"))
                hasExceptionPolicy=query.Read();
            bool hasObjectType;
            using(var query=database.Prepare("SELECT 1 FROM pragma_table_info('types') WHERE name='object_type_id'"))
                hasObjectType=query.Read();
            bool hasArrayBound;
            using(var query=database.Prepare("SELECT 1 FROM pragma_table_info('types') WHERE name='array_bound_kind'"))
                hasArrayBound=query.Read();
            bool hasReceiverParameterFact;
            using(var query=database.Prepare("SELECT 1 FROM pragma_table_info('types') WHERE name='parameters_include_receiver'"))
                hasReceiverParameterFact=query.Read();
            using(var query=database.Prepare("SELECT 1 FROM pragma_table_info('types') WHERE name='return_storage'"))
                HasReturnStorage=query.Read();
            using(var query=database.Prepare("SELECT 1 FROM pragma_table_info('nodes') WHERE name='value_normalization'"))
                HasValueNormalization=query.Read();
            using(var query=database.Prepare("SELECT 1 FROM pragma_table_info('nodes') WHERE name='domain'"))
                HasNodeDomain=query.Read();
            using(var query=database.Prepare("SELECT 1 FROM pragma_table_info('nodes') WHERE name='operation_type_id'"))
                HasOperationType=query.Read();
            using(var query=database.Prepare("SELECT 1 FROM pragma_table_info('nodes') WHERE name='common_lifetime_id'"))
                HasControlLifetimes=query.Read();
            using (var q = database.Prepare("SELECT id,path FROM files ORDER BY id"))
                while (q.Read()) Files.Add(q.Integer(0), q.Text(1));
            using (var q = database.Prepare("SELECT id,kind,name,size,alignment,pointee_type_id,element_type_id,element_count,return_type_id,underlying_type_id,is_variadic,is_incomplete,link_name,linkage,is_volatile,"+(HasLifecycles?"builtin_name":"NULL")+","+(hasDependentTypes?"is_dependent":"0")+","+(HasReturnStorage?"return_storage":"'value'")+","+(hasSemanticRole?"semantic_role":"NULL")+","+(hasExceptionPolicy?"exception_policy":"'unwind'")+","+(hasObjectType?"object_type_id":"NULL")+","+(hasArrayBound?"array_bound_kind,array_bound_node_id":"NULL,NULL")+","+(hasReceiverParameterFact?"parameters_include_receiver":"NULL")+" FROM types ORDER BY id"))
                while (q.Read()) Types.Add(q.Integer(0), new(q.Integer(0),q.Text(1),q.Text(2),q.OptionalInteger(3),q.OptionalInteger(4),q.OptionalInteger(5),q.OptionalInteger(6),q.OptionalInteger(7),q.OptionalInteger(8),q.OptionalInteger(9),q.Integer(10)!=0,q.Integer(11)!=0,q.Text(12),q.Text(13),q.Integer(14)!=0,q.OptionalText(15),q.Integer(16)!=0,q.Text(17),q.OptionalText(18),q.Text(19),q.OptionalInteger(20),q.OptionalText(21),q.OptionalInteger(22),q.OptionalInteger(23) is long receiverFact?receiverFact!=0:null));
            ReadExceptionSpecifications();
            ReadEnums();
            ReadFloatingFormats();
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
            using (var q = database.Prepare("SELECT symbol_id,initializer_node_id FROM global_initializers ORDER BY symbol_id"))
                while (q.Read()) Initializers.Add(q.Integer(0),q.Integer(1));
            ReadInheritance();
            ReadStartupGroups();
            ReadVirtualTables();
            ReadArrayFeatures();
            ReadRecordInitializerFeatures();
            ReadLiterals();
            ReadAllocationFeatures();
            ReadFunctionDetails();
            ReadNodes(null);
            ReadFloatingValues(null);
            ReadLifecycles(null);
            ReadClassConversions(null);
            ReadVirtualBody(null);
            ReadArrayInitializers(null);
            ReadRecordInitializers(null);
            ReadLiteralInitializers(null);
            ReadAllocations(null);
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
    public long FunctionBodyLoads {get;private set;}
    public long PeakFunctionBodyRows {get;private set;}
    public long PeakFunctionBodyNodes {get;private set;}
    public int LoadedFunctionBodies {get;private set;}
    public int PeakLoadedFunctionBodies {get;private set;}
    private readonly Dictionary<string,SymbolRef> external = new(StringComparer.Ordinal);
    public LinkedProgram(IEnumerable<string> paths)
    {
        try
        {
            foreach(string path in paths) Units.Add(new Unit(path));
            if(Units.Select(unit=>unit.Abi).Distinct(StringComparer.Ordinal).Count()!=1)
                throw new InvalidDataException("Cannot link objects with different recorded target ABIs");
            foreach(var unit in Units)
                foreach(var symbol in unit.Symbols.Values.Where(symbol=>symbol.Kind!="source-variable" && symbol.Definition && (symbol.Parent is null || symbol.Kind=="static-local")))
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
                foreach(var symbol in unit.Symbols.Values.Where(symbol=>symbol.Kind!="source-variable" && symbol.Definition && (symbol.Parent is null || symbol.Kind=="static-local")))
                {
                    var value=new SymbolRef(unit,symbol.Id);
                    if(Resolve(unit,symbol.Id)!=value)continue;
                    if(unit.Functions.ContainsKey(symbol.Id)) Functions.Add(value);
                    else if(symbol.Kind is "global" or "static-local" or "rtti-object") Globals.Add(value);
                }
        }
        catch { Dispose(); throw; }
    }
    public SymbolRef Resolve(Unit unit,long id)
    {
        var symbol=unit.Symbols[id];
        return symbol.Kind!="source-variable" && symbol.Linkage=="external" && external.TryGetValue(symbol.LinkName,out var selected)?selected:new(unit,id);
    }
    public IDisposable LoadFunction(SymbolRef function)
    {
        var body=function.Unit.LoadFunction(function.Id);
        ++FunctionBodyLoads;++LoadedFunctionBodies;
        PeakLoadedFunctionBodies=Math.Max(PeakLoadedFunctionBodies,LoadedFunctionBodies);
        PeakFunctionBodyRows=Math.Max(PeakFunctionBodyRows,Units.Sum(unit=>unit.LoadedFunctionRows));
        PeakFunctionBodyNodes=Math.Max(PeakFunctionBodyNodes,Units.Sum(unit=>unit.LoadedFunctionNodes));
        return new ReleaseScope(()=>{body.Dispose();--LoadedFunctionBodies;});
    }
    public void Dispose() { foreach(var unit in Units)unit.Dispose(); }
}
