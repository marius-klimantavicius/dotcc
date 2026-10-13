namespace RoslynObj;

public sealed class BlockOperation : StatementOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitBlock(this);
    internal BlockOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public BlockStatementDetails Block => Object.Detail("block_statement",Id,r=>new BlockStatementDetails(Object,r));
    public BlockDescriptor? Descriptor => Block.Descriptor;
    public Scope? Scope => Descriptor?.Scope;
    public new Lifetime? Lifetime => Descriptor?.Lifetime;
    public IReadOnlyList<StatementOperation> Statements => Object.List("SELECT s.* FROM block_item b JOIN statement s ON s.id=b.statement_id WHERE b.owner_id=$owner ORDER BY b.ordinal",Id,r=>Object.Statement(r.Long("id")),"statement");
}

public sealed class ConditionalOperation : StatementOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitConditional(this);
    internal ConditionalOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public IfStatementDetails Conditional => Object.Detail("if_statement",Id,r=>new IfStatementDetails(Object,r));
    public ExpressionOperation? Condition => Conditional.Condition;
    public StatementOperation? Then => Conditional.Then;
    public StatementOperation? Else => Conditional.Else;
    public StatementOperation? Initialization => Conditional.Initialization;
}

public sealed class LoopOperation : StatementOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitLoop(this);
    internal LoopOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public LoopStatementDetails Loop => Object.Detail("loop_statement",Id,r=>new LoopStatementDetails(Object,r));
    public StatementOperation? Body => Loop.Body;
    public ExpressionOperation? Condition => Loop.Condition;
    public StatementOperation? Initialization => Loop.Initialization;
    public ExpressionOperation? Increment => Loop.Increment;
}

public sealed class JumpOperation : StatementOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitJump(this);
    internal JumpOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public JumpStatementDetails Jump => Object.Detail("jump_statement",Id,r=>new JumpStatementDetails(Object,r));
    public SemanticKind JumpKind => Jump.Kind;
    public ExpressionOperation? Value => Jump.Value;
    public LabelSymbol? Target => Jump.Label;
    public Lifetime? CommonLifetime => Jump.CommonLifetime;
}

public sealed class SwitchOperation : StatementOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitSwitch(this);
    internal SwitchOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public SwitchStatementDetails Switch => Object.Detail("switch_statement",Id,r=>new SwitchStatementDetails(Object,r));
    public ExpressionOperation? Selector => Switch.Selector;
    public StatementOperation? Body => Switch.Body;
    public SwitchDescriptor Descriptor => Switch.Descriptor;
    public IReadOnlyList<SwitchCaseEntry> Cases => Descriptor.Cases;
    public IReadOnlyList<SwitchCaseEntry> SortedCases => Descriptor.SortedCases;
    public SwitchCaseEntry? DefaultCase => Descriptor.DefaultCase;
}

public sealed class TryOperation : StatementOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitTry(this);
    internal TryOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public TryStatementDetails Try => Object.Detail("try_statement",Id,r=>new TryStatementDetails(Object,r));
    public TryDescriptor Definition => Try.Descriptor;
    public StatementOperation Body => Definition.Body;
    public IReadOnlyList<ExceptionHandler> Handlers => Definition.Handlers;
}

public sealed class ExceptionHandler : SemanticEntity
{
    internal ExceptionHandler(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public HandlerDetails Details { get; }
    public StatementOperation Body => Details.Body;
    public CppType? CaughtType => Details.CaughtType;
    public VariableSymbol? Parameter => Details.Parameter;
    public Initialization? ParameterInitialization => Details.ParameterInitialization;
    public Lifetime? BodyLifetime => (Body as BlockOperation)?.Descriptor?.Lifetime;
    public Lifetime? ParameterInitializationLifetime => ParameterInitialization?.Lifetime;
    public SourcePosition? Source => Details.SourcePosition;
}

public sealed class AssemblyOperation : StatementOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitAssembly(this);
    internal AssemblyOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public AsmStatementDetails Assembly => Object.Detail("asm_statement",Id,r=>new AsmStatementDetails(Object,r));
    public string? Text => Assembly.Text;
    public AssemblyDefinition? Definition => Assembly.Entry;
    public Constant? AsmString => Definition?.AsmString;
    public bool? IsVolatile => Definition?.IsVolatile;
    public bool? IsGoto => Definition?.IsGoto;
    public IReadOnlyList<AsmOperandDetails> Operands => Definition?.Operands ?? [];
    public IReadOnlyList<AsmClobberDetails> Clobbers => Definition?.Clobbers ?? [];
    public IReadOnlyList<AsmOperandConstraintDetails> OperandConstraints => Definition?.OperandConstraints ?? [];
    public IReadOnlyList<AsmLabelDetails> Labels => Definition?.Labels ?? [];
}

public sealed class AssemblyDefinition : SemanticEntity
{
    internal AssemblyDefinition(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public AsmEntryDetails Details { get; }
    public Constant AsmString => Details.AsmString;
    public SourceInfo? Source => SourceCorrespondence?.Source;
    public bool IsGnu => Details.GnuAsmForm;
    public bool IsVolatile => Details.IsVolatile;
    public bool IsGoto => Details.IsGoto;
    public IReadOnlyList<AsmOperandDetails> Operands => Details.AsmOperandRows;
    public IReadOnlyList<AsmClobberDetails> Clobbers => Details.AsmClobberRows;
    public IReadOnlyList<AsmOperandConstraintDetails> OperandConstraints => Details.AsmOperandConstraintRows;
    public IReadOnlyList<AsmLabelDetails> Labels => Details.AsmLabelRows;
}

public sealed class ThrowOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitThrow(this);
    internal ThrowOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public ThrowExpressionDetails Throw => Object.Detail("throw_expression",Id,r=>new ThrowExpressionDetails(Object,r));
    public ExpressionOperation? Operand => Throw.Operand;
    public CppType? ThrownType => Throw.ThrownType;
    public Initialization? Initialization => Throw.Initialization;
    public FunctionSymbol? Destructor => Throw.Destructor;
    public bool IsRethrow => Throw.IsRethrow;
    public IReadOnlyList<BaseSubobject> AccessibleBases => Object.List("SELECT b.* FROM throw_accessible_base a JOIN base_subobject b ON b.id=a.subobject_id WHERE a.owner_id=$owner ORDER BY a.ordinal",Id,r=>Object.Subobject(r.Long("id")),"base_subobject");
}

public sealed class AwaitOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitAwait(this);
    internal AwaitOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public CoroutineOperationDetails Protocol => Object.Detail("coroutine_operation",Id,r=>new CoroutineOperationDetails(Object,r));
    public ExpressionOperation? Operand => Protocol.Operand;
    public ExpressionOperation? Ready => Protocol.Ready;
    public ExpressionOperation? Suspend => Protocol.Suspend;
    public ExpressionOperation? Resume => Protocol.Resume;
}

public sealed class AllocationOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitAllocation(this);
    internal AllocationOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public AllocationExpressionDetails Allocation => Object.Detail("allocation_expression",Id,r=>new AllocationExpressionDetails(Object,r));
    public FunctionSymbol? Allocator => Allocation.Allocator;
    public FunctionSymbol? Deallocator => Allocation.Deallocator;
    public FunctionSymbol? Constructor => Allocation.Constructor ?? Initialization?.Constructor;
    public FunctionSymbol? Destructor => Allocation.Destructor ?? Initialization?.Destructor;
    public Initialization? Initialization => Allocation.Initialization;
    public Initialization? ExceptionFreeing => Allocation.ExceptionFreeing;
    public CppType? AllocatedType => Allocation.AllocatedType;
    public ExpressionOperation? ArrayCount => Allocation.ArrayCount;
    public ExpressionOperation? Operand => Allocation.Operand;
    public bool IsDelete => Allocation.IsDelete;
    public bool IsArray => Allocation.IsArray;
    public bool IsPlacement => Allocation.IsPlacement;
    public IReadOnlyList<ExpressionOperation> Arguments => Object.List("SELECT e.* FROM allocation_argument a JOIN expression e ON e.id=a.expression_id WHERE a.owner_id=$owner ORDER BY a.ordinal",Id,r=>Object.Expression(r.Long("id")),"expression");
}

public sealed class LambdaOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitLambda(this);
    internal LambdaOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public LambdaExpressionDetails Lambda => Object.Detail("lambda_expression",Id,r=>new LambdaExpressionDetails(Object,r));
    public LambdaDefinition Definition => Lambda.Lambda;
    public CppType ClosureType => Definition.ClosureType;
    public FunctionSymbol? CallOperator => Definition.CallOperator;
    public IReadOnlyList<LambdaCaptureDetails> Captures => Definition.Captures;
}

public sealed class LambdaDefinition : SemanticEntity
{
    internal LambdaDefinition(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public LambdaDefinitionDetails Details { get; }
    public CppType ClosureType => Details.ClosureType;
    public FunctionSymbol? CallOperator => Details.CallOperator;
    public SourceInfo? Source => SourceCorrespondence?.Source;
    public bool IsGeneric => Details.IsGeneric;
    public bool IsMutable => Details.IsMutable;
    public bool HasCaptureDefault => Details.HasCaptureDefault;
    public bool DefaultIsReference => Details.DefaultIsReference;
    public IReadOnlyList<LambdaCaptureDetails> Captures => Details.LambdaCaptureRows;
}
