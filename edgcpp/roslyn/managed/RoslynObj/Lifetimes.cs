namespace RoslynObj;

public sealed class Initialization : SemanticEntity
{
    internal Initialization(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public InitializationDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public SemanticEntity? Target => Details.Target;
    public Operation? Expression => Details.Expression;
    public Constant? Constant => Details.Constant;
    public FunctionSymbol? Constructor => Details.Constructor;
    public FunctionSymbol? Destructor => Details.Destructor;
    public Lifetime? Lifetime => Details.Lifetime;
    public IReadOnlyList<InitializationOperandDetails> Operands => Object.List("SELECT * FROM initialization_operand WHERE owner_id=$owner ORDER BY role_id,ordinal",Id,r=>new InitializationOperandDetails(Object,r));
    public IReadOnlyList<ConstructionDependencyDetails> Dependencies => Object.List("SELECT * FROM construction_dependency WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new ConstructionDependencyDetails(Object,r));
}

public sealed class Lifetime : SemanticEntity
{
    internal Lifetime(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public LifetimeDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public Lifetime? Parent => Details.Parent;
    public IReadOnlyList<LifetimeCleanupDetails> Cleanups => Object.List("SELECT * FROM lifetime_cleanup WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new LifetimeCleanupDetails(Object,r));
}

public sealed class ExceptionHandlingInfo
{
    private readonly FunctionSymbol function;
    internal ExceptionHandlingInfo(FunctionSymbol function) => this.function=function;
    public FunctionEhDetails? Details => function.Object.Store.Query("SELECT * FROM function_eh WHERE routine_id=$owner",function.Id).FirstOrDefault() is {} row ? new(function.Object,row):null;
    public IReadOnlyList<LoweredEhOperation> Epilogues => function.Object.List("SELECT n.* FROM lowered_eh_operation e JOIN semantic_node n ON n.id=e.id JOIN function_eh f ON f.id=e.function_eh_id JOIN kind k ON k.id=e.kind_id WHERE f.routine_id=$owner AND k.name='leck_function_epilogue' ORDER BY n.id",function.Id,r=>(LoweredEhOperation)function.Object.Node(r.Long("id")),"semantic_node");
    public IReadOnlyList<CleanupRegion> CleanupRegions => function.Object.List("SELECT r.* FROM cleanup_region r JOIN function_eh f ON f.id=r.function_eh_id WHERE f.routine_id=$owner ORDER BY r.ordinal",function.Id,r=>function.Object.Region(r.Long("id")),"cleanup_region");
    public IReadOnlyList<CleanupArray> Arrays => function.Object.List("SELECT a.* FROM cleanup_array a JOIN function_eh f ON f.id=a.function_eh_id WHERE f.routine_id=$owner ORDER BY a.ordinal",function.Id,r=>function.Object.Array(r.Long("id")),"cleanup_array");
}

public sealed class CleanupRegion : SemanticEntity
{
    internal CleanupRegion(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public CleanupRegionDetails Details { get; }
    public long Ordinal => Details.Ordinal;
    public CleanupRegion? NextRegion => Details.NextRegion;
    public CleanupCondition? Condition => Details.Condition;
    public IReadOnlyList<CleanupAction> Actions => Object.List("SELECT * FROM cleanup_action WHERE region_id=$owner ORDER BY ordinal",Id,r=>Object.Action(r.Long("id")),"cleanup_action");
}

public sealed class CleanupAction : SemanticEntity
{
    internal CleanupAction(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public CleanupActionDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public FunctionSymbol? TargetMethod => Details.Routine;
    public CleanupStorageLocation? Storage => Details.Storage;
    public CleanupArray? Array => Details.Array;
    public CleanupAbiDetail? AbiDetail => Details.AbiDetail;
}

public sealed class CleanupStorageLocation : SemanticEntity
{
    internal CleanupStorageLocation(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public CleanupStorageLocationDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public VariableSymbol? Variable => Details.Variable;
    public System.Numerics.BigInteger Offset => Details.Offset;
}

public sealed class CleanupArray : SemanticEntity
{
    internal CleanupArray(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public CleanupArrayDetails Details { get; }
    public CleanupStorageLocation Storage => Details.Storage;
    public System.Numerics.BigInteger ElementSize => Details.ElementSize;
    public System.Numerics.BigInteger ElementCount => Details.ElementCount;
    public CleanupStorageLocation? CountLocation => Details.CountLocation;
}

public sealed class CleanupCondition : SemanticEntity
{
    internal CleanupCondition(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public CleanupConditionDetails Details { get; }
    public CleanupStorageLocation Storage => Details.Storage;
    public SemanticKind Kind => Details.Kind;
}

public sealed class CleanupAbiDetail : SemanticEntity
{
    internal CleanupAbiDetail(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public CleanupAbiDetailDetails Details { get; }
}
