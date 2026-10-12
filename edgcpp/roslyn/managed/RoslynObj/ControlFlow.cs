namespace RoslynObj;

public sealed class BlockOperation : StatementOperation
{
    internal BlockOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public BlockStatementDetails Block => Object.Detail("block_statement",Id,r=>new BlockStatementDetails(Object,r));
    public Scope? Scope => Block.Scope;
    public IReadOnlyList<StatementOperation> Statements => Object.List("SELECT s.* FROM block_item b JOIN statement s ON s.id=b.statement_id WHERE b.owner_id=$owner ORDER BY b.ordinal",Id,r=>Object.Statement(r.Long("id")),"statement");
}

public sealed class ConditionalOperation : StatementOperation
{
    internal ConditionalOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public IfStatementDetails Conditional => Object.Detail("if_statement",Id,r=>new IfStatementDetails(Object,r));
    public ExpressionOperation? Condition => Conditional.Condition;
    public StatementOperation? Then => Conditional.Then;
    public StatementOperation? Else => Conditional.Else;
    public StatementOperation? Initialization => Conditional.Initialization;
}

public sealed class LoopOperation : StatementOperation
{
    internal LoopOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public LoopStatementDetails Loop => Object.Detail("loop_statement",Id,r=>new LoopStatementDetails(Object,r));
    public StatementOperation? Body => Loop.Body;
    public ExpressionOperation? Condition => Loop.Condition;
    public StatementOperation? Initialization => Loop.Initialization;
    public ExpressionOperation? Increment => Loop.Increment;
}

public sealed class JumpOperation : StatementOperation
{
    internal JumpOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public JumpStatementDetails Jump => Object.Detail("jump_statement",Id,r=>new JumpStatementDetails(Object,r));
    public SemanticKind JumpKind => Jump.Kind;
    public ExpressionOperation? Value => Jump.Value;
    public LabelSymbol? Target => Jump.Label;
}

public sealed class SwitchOperation : StatementOperation
{
    internal SwitchOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public SwitchStatementDetails Switch => Object.Detail("switch_statement",Id,r=>new SwitchStatementDetails(Object,r));
    public ExpressionOperation? Selector => Switch.Selector;
    public StatementOperation? Body => Switch.Body;
    public IReadOnlyList<SwitchCaseDetails> Cases => Object.List("SELECT * FROM switch_case WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new SwitchCaseDetails(Object,r));
}

public sealed class TryOperation : StatementOperation
{
    internal TryOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public TryStatementDetails Try => Object.Detail("try_statement",Id,r=>new TryStatementDetails(Object,r));
    public StatementOperation Body => Try.Body;
    public IReadOnlyList<ExceptionHandler> Handlers => Object.List("SELECT * FROM handler WHERE try_id=$owner ORDER BY ordinal",Id,r=>Object.Handler(r.Long("id")),"handler");
}

public sealed class ExceptionHandler : SemanticEntity
{
    internal ExceptionHandler(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public HandlerDetails Details { get; }
    public StatementOperation Body => Details.Body;
    public CppType? CaughtType => Details.CaughtType;
    public VariableSymbol? Parameter => Details.Parameter;
    public Initialization? ParameterInitialization => Details.ParameterInitialization;
    public Lifetime? Lifetime => Details.Lifetime;
    public SourcePosition? Source => Details.SourcePosition;
}

public sealed class AssemblyOperation : StatementOperation
{
    internal AssemblyOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public AsmStatementDetails Assembly => Object.Detail("asm_statement",Id,r=>new AsmStatementDetails(Object,r));
    public string Text => Assembly.Text;
    public IReadOnlyList<AsmOperandDetails> Operands => Object.List("SELECT * FROM asm_operand WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new AsmOperandDetails(Object,r));
    public IReadOnlyList<string> Clobbers => Object.List("SELECT * FROM asm_clobber WHERE owner_id=$owner ORDER BY ordinal",Id,r=>Object.Text(r.Long("text_id"))!);
    public IReadOnlyList<AsmLabelDetails> Labels => Object.List("SELECT * FROM asm_label WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new AsmLabelDetails(Object,r));
}

public sealed class ThrowOperation : ExpressionOperation
{
    internal ThrowOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public ThrowExpressionDetails Throw => Object.Detail("throw_expression",Id,r=>new ThrowExpressionDetails(Object,r));
    public ExpressionOperation? Operand => Throw.Operand;
    public CppType? ThrownType => Throw.ThrownType;
    public Initialization? Initialization => Throw.Initialization;
    public FunctionSymbol? Destructor => Throw.Destructor;
    public bool IsRethrow => Throw.IsRethrow;
}

public sealed class AwaitOperation : ExpressionOperation
{
    internal AwaitOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public CoroutineOperationDetails Protocol => Object.Detail("coroutine_operation",Id,r=>new CoroutineOperationDetails(Object,r));
    public ExpressionOperation? Operand => Protocol.Operand;
    public ExpressionOperation? Ready => Protocol.Ready;
    public ExpressionOperation? Suspend => Protocol.Suspend;
    public ExpressionOperation? Resume => Protocol.Resume;
}

public sealed class AllocationOperation : ExpressionOperation
{
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
    internal LambdaOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public LambdaExpressionDetails Lambda => Object.Detail("lambda_expression",Id,r=>new LambdaExpressionDetails(Object,r));
    public LambdaDefinitionDetails Definition => Lambda.Lambda;
    public CppType ClosureType => Definition.ClosureType;
    public FunctionSymbol? CallOperator => Definition.CallOperator;
    public IReadOnlyList<LambdaCaptureDetails> Captures => Object.List("SELECT * FROM lambda_capture WHERE owner_id=$owner ORDER BY ordinal",Object.Store.Required("lambda_expression",Id).Long("lambda_id"),r=>new LambdaCaptureDetails(Object,r));
}
