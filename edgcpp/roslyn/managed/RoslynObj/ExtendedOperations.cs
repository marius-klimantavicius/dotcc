namespace RoslynObj;

public sealed class DeclarationOperation : StatementOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitDeclarationStatement(this);
    internal DeclarationOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public DeclarationStatementDetails Declaration => Object.Detail("declaration_statement",Id,r=>new DeclarationStatementDetails(Object,r));
    public bool HasStaticOrThreadVariable => Declaration.HasStaticOrThreadVariable;
    public IReadOnlyList<SemanticEntity> Entities => Object.List("SELECT * FROM declaration_statement_entity WHERE owner_id=$owner ORDER BY ordinal",Id,r=>Object.Entity(r.Long("entity_id")));
}

public sealed class RangeForOperation : StatementOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitRangeFor(this);
    internal RangeForOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public RangeForDetails Range => Object.Detail("range_for",Id,r=>new RangeForDetails(Object,r));
    public StatementOperation? Body => Range.Body;
    public StatementOperation? Initialization => Range.Initialization;
    public VariableSymbol? RangeVariable => Range.RangeVariable;
    public VariableSymbol? BeginVariable => Range.BeginVariable;
    public VariableSymbol? EndVariable => Range.EndVariable;
    public VariableSymbol? IteratorVariable => Range.IteratorVariable;
    public ExpressionOperation? Comparison => Range.Comparison;
    public ExpressionOperation? Increment => Range.Increment;
    public Scope? Scope => Range.Scope;
    public Scope? IteratorScope => Range.IteratorScope;
    public bool UsesAwait => Range.UseAwait;
}

public sealed class ForEachOperation : StatementOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitForEach(this);
    internal ForEachOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public ForEachStatementDetails ForEach => Object.Detail("for_each_statement",Id,r=>new ForEachStatementDetails(Object,r));
    public StatementOperation? Body => ForEach.Body;
    public ExpressionOperation? Collection => ForEach.Collection;
    public SemanticKind PatternKind => ForEach.PatternKind;
    public VariableSymbol? Iterator => ForEach.Iterator;
    /// <summary>Actual evaluated initialization recipes in retained operand order.</summary>
    public IReadOnlyList<Initialization> Initializations => Object.List("SELECT i.* FROM structural_child c JOIN kind k ON k.id=c.role_id JOIN initialization i ON i.id=c.child_id WHERE c.owner_id=$owner AND k.name='initialization' ORDER BY c.ordinal",Id,r=>Object.Initialization(r.Long("id")),"initialization");
    public ExpressionOperation? Test => ForEach.Test;
    public ExpressionOperation? Increment => ForEach.Increment;
    public VariableSymbol? CollectionReference => ForEach.CollectionReference;
    public VariableSymbol? TemporaryVariable => ForEach.TemporaryVariable;
    public VariableSymbol? EndVariable => ForEach.EndVariable;
    public Scope? Scope => ForEach.Scope;
    public Scope? IteratorScope => ForEach.IteratorScope;
    public ExpressionOperation? Assignment => ForEach.Assignment;
    public bool UsesPreviousDeclarationIterator => ForEach.UsesPreviousDeclarationIterator;
    /// <summary>The recorded field reused as the iterator, when the previous-declaration field arm is retained.</summary>
    public FieldSymbol? PreviousField => ForEach.PreviousField;
}

public sealed class MicrosoftTryOperation : StatementOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitMicrosoftTry(this);
    internal MicrosoftTryOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public MicrosoftTryStatementDetails Try => Object.Detail("microsoft_try_statement",Id,r=>new MicrosoftTryStatementDetails(Object,r));
    public StatementOperation Body => Try.Body;
    public ExpressionOperation? Filter => Try.Filter;
    public StatementOperation? Handler => Try.Handler;
    public StatementOperation? Finally => Try.Finally;
    public bool IsFinally => Try.IsFinally;
}

public sealed class LabelOperation : StatementOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitLabel(this);
    internal LabelOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public LabelStatementDetails LabelStatement => Object.Detail("label_statement",Id,r=>new LabelStatementDetails(Object,r));
    public LabelSymbol Label => LabelStatement.Label;
    public Lifetime? Lifetime => Label.Lifetime;
}

public sealed class CoroutineOperation : StatementOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitCoroutine(this);
    internal CoroutineOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public CoroutineDetails Coroutine => Object.Detail("coroutine",Id,r=>new CoroutineDetails(Object,r));
    public FunctionSymbol Function => Coroutine.Routine;
    public VariableSymbol? Promise => Coroutine.Promise;
    public VariableSymbol? Handle => Coroutine.Handle;
    public VariableSymbol? ThisParameterCopy => Coroutine.ThisParameterCopy;
    public ExpressionOperation? InitialSuspend => Coroutine.InitialSuspend;
    public ExpressionOperation? FinalSuspend => Coroutine.FinalSuspend;
    public ExpressionOperation? ReturnObject => Coroutine.ReturnObject;
    public ExpressionOperation? Exception => Coroutine.Exception;
    public CppType? TraitsType => Coroutine.TraitsType;
    public ExpressionOperation? AllocationFailureCall => Coroutine.AllocationFailureCall;
    public FunctionSymbol? Allocator => Coroutine.Allocator;
    public FunctionSymbol? Deallocator => Coroutine.Deallocator;
    public IReadOnlyList<CoroutineParameterCopyDetails> ParameterCopies => Object.List("SELECT c.* FROM coroutine_parameter_copy c JOIN variable_symbol v ON v.id=c.copy_id WHERE c.owner_id=$owner AND v.is_parameter=1 AND c.copy_id<>COALESCE((SELECT this_parameter_copy_id FROM coroutine WHERE id=$owner),0) ORDER BY c.ordinal",Id,r=>new CoroutineParameterCopyDetails(Object,r));
}

public sealed class VlaStatementOperation : StatementOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitVlaStatement(this);
    internal VlaStatementOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public VlaOperationDetails Vla => Object.Detail("vla_operation",Id,r=>new VlaOperationDetails(Object,r));
    public VariableSymbol? Variable => Vla.Variable;
    public override CppType? Type => Vla.Type;
    public VlaDimension? Dimension => Vla.Dimension;
}

public sealed class TemporaryOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitTemporary(this);
    internal TemporaryOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public TemporaryExpressionDetails TemporaryExpression => Object.Detail("temporary_expression",Id,r=>new TemporaryExpressionDetails(Object,r));
    public Initialization? Initialization => TemporaryExpression.Initialization;
    public SemanticEntity? Target => Initialization?.Target;
    public CppType? SourceType => TemporaryExpression.SourceType;
}

public sealed class ConditionOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitCondition(this);
    internal ConditionOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public ConditionExpressionDetails Condition => Object.Detail("condition_expression",Id,r=>new ConditionExpressionDetails(Object,r));
    public ExpressionOperation? Value => Condition.Expression;
    public StatementOperation? Initialization => Condition.Initialization;
    public Initialization? DynamicInitialization => Condition.DynamicInitialization;
    public Scope? Scope => Condition.Scope;
    public VariableSymbol? Variable => Condition.Variable;
}

public sealed class LifetimeOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitLifetime(this);
    internal LifetimeOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public LifetimeExpressionDetails LifetimeExpression => Object.Detail("lifetime_expression",Id,r=>new LifetimeExpressionDetails(Object,r));
    public ExpressionOperation Value => LifetimeExpression.Expression;
    public Lifetime? Lifetime => LifetimeExpression.Lifetime;
}

public sealed class TypeQueryOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitTypeQuery(this);
    internal TypeQueryOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public TypeQueryExpressionDetails Query => Object.Detail("type_query_expression",Id,r=>new TypeQueryExpressionDetails(Object,r));
    public SemanticKind QueryKind => Query.QueryKind;
    public CppType? OperandType => Query.OperandType;
    public ExpressionOperation? Operand => Query.OperandExpression;
    public bool IsDynamic => Query.IsDynamic;
    public bool IsStandardAlignof => Query.IsStandardAlignof;
    public IReadOnlyList<VlaDimension> EvaluatedDimensions => VlaDimensions;
}

public sealed class StatementExpressionOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitStatementExpression(this);
    internal StatementExpressionOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public StatementExpressionDetails StatementExpression => Object.Detail("statement_expression",Id,r=>new StatementExpressionDetails(Object,r));
    public StatementOperation Statement => StatementExpression.Statement;
}

public sealed class ReuseOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitReuse(this);
    internal ReuseOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public ReuseExpressionDetails Reuse => Object.Detail("reuse_expression",Id,r=>new ReuseExpressionDetails(Object,r));
    public Initialization Initialization => Reuse.Initialization;
}

public sealed class VlaExpressionOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitVlaExpression(this);
    internal VlaExpressionOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public VlaOperationDetails Vla => Object.Detail("vla_operation",Id,r=>new VlaOperationDetails(Object,r));
    public VariableSymbol? Variable => Vla.Variable;
    public CppType? VlaType => Vla.Type;
    public VlaDimension? Dimension => Vla.Dimension;
}

public sealed class BuiltinOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitBuiltin(this);
    internal BuiltinOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public BuiltinExpressionDetails Builtin => Object.Detail("builtin_expression",Id,r=>new BuiltinExpressionDetails(Object,r));
    public SemanticKind BuiltinKind => Builtin.Kind;
    public ExpressionOperation? SelectedExpression => Builtin.SelectedExpression;
}

public sealed class ParameterReferenceOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitParameterReference(this);
    internal ParameterReferenceOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public ParameterReferenceExpressionDetails ParameterReference => Object.Detail("parameter_reference_expression",Id,r=>new ParameterReferenceExpressionDetails(Object,r));
    public long ParameterNumber => ParameterReference.ParameterNumber;
    public long ParameterDepth => ParameterReference.ParameterDepth;
    public bool IsThis => ParameterReference.IsThis;
}

public sealed class DependentOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitDependent(this);
    internal DependentOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public DependentExpressionDetails Dependency => Object.Detail("dependent_expression",Id,r=>new DependentExpressionDetails(Object,r));
    public ExpressionOperation? WrappedExpression => Dependency.WrappedExpression;
    public TemplateDescriptor? Template => Dependency.Template;
    public Initialization? Initialization => Dependency.Initialization;
    public bool? NoexceptRequired => Dependency.NoexceptRequired;
    /// <summary>The recorded deferred-evaluation default-argument context flag; null when this state is not recorded.</summary>
    public bool? ReattemptDefaultArgument => Dependency.ReattemptDefaultArgument;
    /// <summary>The recorded deferred-evaluation default-member-initializer context flag; it may also be true in a default-argument context.</summary>
    public bool? ReattemptDefaultMemberInitializer => Dependency.ReattemptDefaultMemberInitializer;
    public ExpressionOperation? PackIndex => Dependency.PackIndex;
    public CppType? PackType => Dependency.PackType;
    public TemplateDescriptor? PackTemplate => Dependency.PackTemplate;
    public SemanticKind? FoldOperator => Dependency.FoldOperator;
    public IReadOnlyList<RequirementParameter> Parameters => Object.List("SELECT p.*,r.owner_id,r.ordinal,r.parameter_type_id FROM requirement_parameter r JOIN parameter_type p ON p.id=r.parameter_type_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>new RequirementParameter(Object,r),"parameter_type");
    public IReadOnlyList<TemplateArgument> TemplateArguments => Object.List("SELECT a.* FROM expression_template_argument r JOIN template_argument a ON a.id=r.argument_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.Argument(r.Long("id")),"template_argument");
    public TokenSequence? TokenSequence => Dependency.TokenSequence;
    public IReadOnlyList<DependentRequirementDetails> Requirements => Dependency.DependentRequirementRows;
}

public sealed class RequirementParameter
{
    internal RequirementParameter(RoslynObject owner,Row row) => Details=new(owner,row);
    public RequirementParameterDetails Details { get; }
    public ParameterType Descriptor => Details.ParameterType;
    public string? Name => Descriptor.Name;
    public CppType Type => Descriptor.Type;
    public CppType? DeclaredType => Descriptor.DeclaredType;
    public long Ordinal => Details.Ordinal;
}

public sealed class TypeOperandOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitTypeOperand(this);
    internal TypeOperandOperation(RoslynObject o,long id,Row r):base(o,id,r) { }
    public ReferenceExpressionDetails Reference => Object.Detail("reference_expression",Id,r=>new ReferenceExpressionDetails(Object,r));
    public CppType? OperandType => Reference.Target as CppType;
    public NameReference? NameReference => Reference.NameReference;
    public bool IsDefault => OperandType is null;
}

public sealed class TokenSequence : SemanticEntity
{
    internal TokenSequence(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public TokenSequenceDetails Details { get; }
    public IReadOnlyList<TokenSequenceTokenDetails> Tokens => Details.TokenSequenceTokenRows;
}
