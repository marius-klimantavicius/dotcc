namespace RoslynObj;

public sealed class Initialization : SemanticEntity
{
    internal Initialization(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public InitializationDetails Details { get; }
    public InitializationOperation? Operation => Object.Store.Get("semantic_node",Id) is null ? null : (InitializationOperation)Object.Node(Id);
    public SemanticKind Kind => Details.Kind;
    public SemanticEntity? Target => Details.Target;
    public Operation? Expression => Details.Expression;
    public Constant? Constant => Details.Constant;
    public LambdaDefinition? Lambda => Details.Lambda;
    public FunctionSymbol? Constructor => Details.Constructor;
    public FunctionSymbol? Destructor => Details.Destructor;
    public Lifetime? Lifetime => Details.Lifetime;
    public Lifetime? InitExpressionLifetime => Details.InitExpressionLifetime;
    public Lifetime? OverlappingLifetime => Details.OverlappingLifetime;
    public Initialization? Master => Details.Master;
    public Initialization? NextInDestructionList => Details.NextDestruction;
    /// <summary>Whether this is a generated local-static guard cleanup recipe. False in source IL does not describe the runtime guard obligation.</summary>
    public bool IsConditionalGuard => Details.IsConditionalGuard;
    /// <summary>Whether this is a generated VLA deallocation recipe. Source IL records false before such recipes are generated.</summary>
    public bool IsVlaDeallocation => Details.IsVlaDeallocation;
    /// <summary>Whether this is a generated table-based EH exception-object release recipe. False in other profiles does not exclude exception destruction.</summary>
    public bool IsFreeingOfExceptionObject => Details.IsFreeingOfExceptionObject;
    public IReadOnlyList<InitializationOperandDetails> Operands => Object.List("SELECT * FROM initialization_operand WHERE owner_id=$owner ORDER BY role_id,ordinal",Id,r=>new InitializationOperandDetails(Object,r));
}

public sealed class Lifetime : SemanticEntity
{
    internal Lifetime(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public LifetimeDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public Lifetime? Parent => Details.Parent;
    public SemanticEntity? BoundEntity => Details.BoundEntity;
    public Initialization? ParentCheckpoint => Details.ParentCheckpoint;
    public bool HasImplicitChild => Details.HasImplicitChild;
    public bool HasBlockAfterLabelChild => Details.HasBlockAfterLabelChild;
    /// <summary>Recorded children in reverse creation order. Implicit children may not appear in this list.</summary>
    public IReadOnlyList<Lifetime> Children => Object.List("SELECT l.* FROM lifetime_child r JOIN lifetime l ON l.id=r.child_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.Lifetime(r.Long("id")),"lifetime");
    public IReadOnlyList<LifetimeCleanup> Cleanups => Object.List("SELECT * FROM lifetime_cleanup WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new LifetimeCleanup(Object,r));
}

public sealed class ExceptionHandlingInfo
{
    private readonly FunctionSymbol function;
    internal ExceptionHandlingInfo(FunctionSymbol function) => this.function=function;
    public FunctionEhDetails? Details => function.Object.Store.Query("SELECT * FROM function_eh WHERE routine_id=$owner",function.Id).FirstOrDefault() is {} row ? new(function.Object,row):null;
    /// <summary>The actual retained cleanup-region table variable, independent of the decoded regions.</summary>
    public VariableSymbol? RegionTable => Details?.RegionTable;
    /// <summary>The actual retained array-cleanup table variable, absent when this EH representation has no table.</summary>
    public VariableSymbol? ArrayTable => Details?.ArrayTable;
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
    public bool? IsUnordered => Details.IsUnordered;
}

public sealed class CleanupStorageLocation : SemanticEntity
{
    internal CleanupStorageLocation(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public CleanupStorageLocationDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public VariableSymbol? Variable => Details.Variable;
    public System.Numerics.BigInteger Offset => Details.Offset;
    public bool IsIndirect => Details.IsIndirect;
    public bool UsesThisParameterOffset => Details.UsesThisParameterOffset;
}

public sealed class CleanupArray : SemanticEntity
{
    internal CleanupArray(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public CleanupArrayDetails Details { get; }
    public Constant HandleConstant => Details.HandleConstant;
    public IReadOnlyList<CleanupStorageLocation> StorageLocations => Object.List("SELECT s.* FROM cleanup_storage_location s WHERE s.id IN (SELECT storage_id FROM cleanup_action WHERE array_id=$owner AND storage_id IS NOT NULL) ORDER BY s.id",Id,r=>Object.Storage(r.Long("id")),"cleanup_storage_location");
    public CleanupStorageLocation? Storage
    {
        get { var locations=StorageLocations.Take(2).ToArray();return locations.Length==1?locations[0]:null; }
    }
    public System.Numerics.BigInteger ElementSize => Details.ElementSize;
    public System.Numerics.BigInteger ElementCount => Details.ElementCount;
    public IReadOnlyList<CleanupStorageLocation> CountLocations => Object.List("SELECT s.* FROM cleanup_storage_location s WHERE s.id IN (SELECT b.vla_count_location_id FROM cleanup_action a JOIN cleanup_abi_detail b ON b.id=a.abi_detail_id WHERE a.array_id=$owner AND b.vla_count_location_id IS NOT NULL) ORDER BY s.id",Id,r=>Object.Storage(r.Long("id")),"cleanup_storage_location");
    public CleanupStorageLocation? CountLocation
    {
        get { var locations=CountLocations.Take(2).ToArray();return locations.Length==1?locations[0]:null; }
    }
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
    public CleanupStorageLocation? VttLocation => Details.VttLocation;
    public CleanupStorageLocation? VlaCountLocation => Details.VlaCountLocation;
    public bool? IsDelegating => Details.IsDelegating;
    public CleanupStorageLocation? DelegationArgumentLocation => IsDelegating==true?VttLocation:null;
}
