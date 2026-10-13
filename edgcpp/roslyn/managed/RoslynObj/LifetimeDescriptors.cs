namespace RoslynObj;

public sealed class BlockDescriptor : SemanticEntity
{
    internal BlockDescriptor(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public BlockDescriptorDetails Details { get; }
    public Scope? Scope => Details.Scope;
    public Lifetime? Lifetime => Details.Lifetime;
    public bool EndReachable => Details.EndReachable;
    public bool IsStatementExpression => Details.IsStatementExpression;
}

public sealed class TryDescriptor : SemanticEntity
{
    internal TryDescriptor(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public TryDescriptorDetails Details { get; }
    public StatementOperation Body => Details.Body;
    public bool IsFunctionTry => Details.IsFunctionTry;
    public Lifetime? Lifetime => Details.Lifetime;
    public IReadOnlyList<ExceptionHandler> Handlers => Object.List("SELECT h.* FROM try_handler r JOIN handler h ON h.id=r.handler_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.Handler(r.Long("id")),"handler");
}

public sealed class LocalStaticInitialization : SemanticEntity
{
    internal LocalStaticInitialization(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public LocalStaticInitializationDetails Details { get; }
    public VariableSymbol Variable => Details.Variable;
    public SemanticKind Kind => Details.InitializationKind;
    public Initialization? Initialization => Details.Initialization;
    public Constant? Constant => Details.Constant;
    public Lifetime? Lifetime => Details.Lifetime;
}

public sealed class ConstructorInitializer : SemanticEntity
{
    internal ConstructorInitializer(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public ConstructorInitializerDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public FieldSymbol? Field => Details.Field;
    public BaseSubobject? BaseSubobject => Details.BaseSubobject;
    public Initialization? Initialization => Details.Initialization;
    public Initialization? EffectiveInitialization => Initialization ?? (Details.UsesFieldInitializer?Field?.DefaultInitializer:null);
    public ExpressionOperation? SourceExpression => Details.SourceExpression;
    public bool UsesFieldInitializer => Details.UsesFieldInitializer;
    public bool IsCompilerGenerated => Details.CompilerGenerated;
    public bool IsPackExpansion => Details.IsPackExpansion;
    public bool IsBraced => Details.IsBraced;
    public CppType? OriginalType => Details.OriginalType;
}

/// <summary>An ordered recorded destruction recipe, which can be exceptional-only.</summary>
public sealed class LifetimeCleanup
{
    internal LifetimeCleanup(RoslynObject owner,Row row) => Details=new(owner,row);
    public LifetimeCleanupDetails Details { get; }
    public long Ordinal => Details.Ordinal;
    public Initialization Initialization => Details.Initialization;
    public FunctionSymbol? Destructor => Initialization.Destructor;
    public bool IsUnordered => Initialization.Details.IsUnordered;
    public bool IsConditional => Initialization.Details.IsConditional;
}
