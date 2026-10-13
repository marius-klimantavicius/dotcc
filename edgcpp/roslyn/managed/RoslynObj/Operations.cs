namespace RoslynObj;

public abstract class SemanticNode : SemanticEntity
{
    internal SemanticNode(RoslynObject owner, long id, Row row) : base(owner, id) => NodeDetails = new(owner, row);
    internal SemanticNode(RoslynObject owner,long id) : base(owner,id) { }
    public SemanticNodeDetails? NodeDetails { get; }
    public virtual SemanticKind Kind => NodeDetails!.Kind;
    public virtual SourceInfo? Source => NodeDetails!.SourceOrigin;
    public virtual SemanticKind RepresentationRole => NodeDetails!.RepresentationRole;
    public virtual bool IsCompilerGenerated => NodeDetails!.CompilerGenerated;
    public virtual IReadOnlyList<SemanticNode> Children => Object.List(SnapshotStore.ChildrenQuery, Id, r => Object.Node(r.Long("id")), "semantic_node");
    public IReadOnlyList<StructuralChildDetails> ChildRelations => Object.List("SELECT * FROM structural_child WHERE owner_id=$owner ORDER BY role_id,ordinal", Id, r => new StructuralChildDetails(Object, r));
    public IReadOnlyList<SemanticNode> StructuralParents => Object.List("SELECT DISTINCT n.* FROM structural_child c JOIN semantic_node n ON n.id=c.owner_id WHERE c.child_id=$owner ORDER BY n.id", Id, r => Object.Node(r.Long("id")), "semantic_node");
    public IReadOnlyList<SemanticReferenceDetails> References => Object.List("SELECT * FROM semantic_reference WHERE owner_id=$owner ORDER BY role_id,ordinal", Id, r => new SemanticReferenceDetails(Object, r));
    public virtual void Accept(SemanticVisitor visitor) => visitor.VisitNode(this);

    public IEnumerable<Operation> DescendantOperations(CancellationToken cancellationToken = default)
    {
        var seen = new HashSet<SemanticNode>();
        var stack = new Stack<IEnumerator<SemanticNode>>();
        try
        {
            stack.Push(new[] { this }.AsEnumerable().GetEnumerator());
            while (stack.TryPeek(out var current))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!current.MoveNext()) { stack.Pop().Dispose(); continue; }
                var node = current.Current;
                if (!seen.Add(node) || node.RepresentationRole.Name == "metadata") continue;
                if (node is Operation operation) yield return operation;
                stack.Push(node.Children.GetEnumerator());
            }
        }
        finally { while (stack.TryPop(out var enumerator)) enumerator.Dispose(); }
    }
}

public class Operation : SemanticNode
{
    internal Operation(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public virtual CppType? Type => this is ExpressionOperation expression ? expression.Type : null;
    public override void Accept(SemanticVisitor visitor) => visitor.VisitOperation(this);
}

public class ExpressionOperation : Operation
{
    internal ExpressionOperation(RoslynObject owner, long id, Row row) : base(owner, id, row) { }
    public ExpressionDetails Details => Object.Detail("expression", Id, r => new ExpressionDetails(Object, r));
    public new CppType Type => Details.Type;
    public SemanticKind ValueCategory => Details.ValueCategory;
    public IReadOnlyList<ExpressionOperation> Operands => Object.List("SELECT e.* FROM expression_operand r JOIN expression e ON e.id=r.expression_id WHERE r.owner_id=$owner ORDER BY r.role_id,r.ordinal", Id, r => Object.Expression(r.Long("id")), "expression");
    public IReadOnlyList<ExpressionOperandDetails> OperandRelations => Object.List("SELECT * FROM expression_operand WHERE owner_id=$owner ORDER BY role_id,ordinal", Id, r => new ExpressionOperandDetails(Object, r));
    public IReadOnlyList<VlaDimension> VlaDimensions => Object.List("SELECT d.* FROM expression_vla_dimension r JOIN vla_dimension d ON d.id=r.dimension_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.VlaDimension(r.Long("id")),"vla_dimension");
    public override void Accept(SemanticVisitor visitor) => visitor.VisitOperation(this);
}

public sealed class ExpressionOperation<TDetails> : ExpressionOperation where TDetails : SemanticDetail
{
    private readonly string table;
    private readonly Func<Row,TDetails> create;
    internal ExpressionOperation(RoslynObject owner, long id, Row row, string table, Func<Row,TDetails> create) : base(owner,id,row)
    { this.table = table; this.create = create; }
    public TDetails Payload => Object.Detail(table, Id, create);
}

public class OperatorOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitOperator(this);
    internal OperatorOperation(RoslynObject owner, long id, Row row) : base(owner,id,row) { }
    public OperatorExpressionDetails Operator => Object.Detail("operator_expression", Id, r => new OperatorExpressionDetails(Object,r));
    public SemanticKind OperatorKind => Operator.OperatorKind;
}

public sealed class CallOperation : OperatorOperation
{
    internal CallOperation(RoslynObject owner, long id, Row row) : base(owner,id,row) { }
    public CallExpressionDetails Call => Object.Detail("call_expression", Id, r => new CallExpressionDetails(Object,r));
    public ExpressionOperation Callee => Call.Callee;
    public FunctionSymbol? TargetMethod => Call.TargetRoutine;
    public FunctionSymbol? SourceTargetMethod => Call.SourceTargetRoutine;
    public ExpressionOperation? Receiver => Call.Receiver;
    public SemanticKind CallKind => Call.CallKind;
    public SemanticKind? SourceDispatchKind => Call.SourceDispatchKind;
    public IReadOnlyList<CallArgument> Arguments => Object.List("SELECT * FROM call_argument WHERE owner_id=$owner ORDER BY ordinal", Id, r => new CallArgument(Object,r));
    public override void Accept(SemanticVisitor visitor) => visitor.VisitCall(this);
}

public sealed class CallArgument
{
    internal CallArgument(RoslynObject owner, Row row) => Details = new(owner,row);
    public CallArgumentDetails Details { get; }
    public ExpressionOperation Value => Details.Expression;
    public ParameterType? Parameter => Details.ParameterType;
    public SemanticKind? Role => Details.Role;
    public long Ordinal => Details.Ordinal;
}

public sealed class ConversionOperation : OperatorOperation
{
    internal static bool IsConversionKind(string kind) => kind is "eok_cast" or "eok_lvalue_cast" or "eok_ref_cast"
        or "eok_lvalue_adjust" or "eok_class_rvalue_adjust" or "eok_base_class_cast" or "eok_derived_class_cast"
        or "eok_pm_base_class_cast" or "eok_pm_derived_class_cast" or "eok_dynamic_cast" or "eok_ref_dynamic_cast"
        or "eok_bool_cast" or "eok_array_to_pointer";
    public override void Accept(SemanticVisitor visitor) => visitor.VisitConversion(this);
    internal ConversionOperation(RoslynObject owner, long id, Row row) : base(owner,id,row) { }
    public ExpressionOperation Operand => Operands[0];
    public CppType SourceType => Operand.Type;
    public CppType TargetType => Type;
}

public sealed class ConstantOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitConstant(this);
    internal ConstantOperation(RoslynObject owner, long id, Row row) : base(owner,id,row) { }
    public Constant Constant => Object.Constant(Object.Store.Required("constant_expression",Id).Long("constant_id"));
    public NameReference? NameReference => Object.Detail("constant_expression",Id,r=>new ConstantExpressionDetails(Object,r)).NameReference;
}

public sealed class ReferenceOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitReference(this);
    internal ReferenceOperation(RoslynObject owner, long id, Row row) : base(owner,id,row) { }
    public ReferenceExpressionDetails Reference => Object.Detail("reference_expression",Id,r=>new ReferenceExpressionDetails(Object,r));
    public SemanticEntity? Target => Reference.Target;
    public NameReference? NameReference => Reference.NameReference;
}

public class LoweredEhOperation : ExpressionOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitLoweredEh(this);
    internal LoweredEhOperation(RoslynObject owner, long id, Row row) : base(owner,id,row) { }
    public LoweredEhOperationDetails ExceptionOperation => Object.Detail("lowered_eh_operation",Id,r=>new LoweredEhOperationDetails(Object,r));
    public SemanticKind ExceptionKind => ExceptionOperation.Kind;
    public Initialization? Initialization => ExceptionOperation.Initialization;
    public Initialization? CleanupStart => ExceptionKind.Name is "leck_cleanup_state" or "leck_unreachable_cleanup_state" ? Initialization : null;
    public Initialization? CompletedInitializer => ExceptionKind.Name=="leck_initialization_completed" ? Initialization : null;
}

public sealed class CleanupStateOperation : LoweredEhOperation
{
    public override void Accept(SemanticVisitor visitor) => visitor.VisitCleanupState(this);
    internal CleanupStateOperation(RoslynObject owner, long id, Row row) : base(owner,id,row) { }
    public CleanupStateOperationDetails State => Object.Detail("cleanup_state_operation",Id,r=>new CleanupStateOperationDetails(Object,r));
    public CleanupRegion? Region => State.Region;
    public System.Numerics.BigInteger? StateNumber => State.StateNumber;
    public bool IsUnreachable => State.IsUnreachable;
}

public class StatementOperation : Operation
{
    internal StatementOperation(RoslynObject owner, long id, Row row) : base(owner,id,row) { }
    public StatementDetails Details => Object.Detail("statement",Id,r=>new StatementDetails(Object,r));
    public ExpressionOperation? Expression => Details.Expression;
    public SourceEntry? SourceSequenceEntry => Details.SourceSequenceEntry;
    public override void Accept(SemanticVisitor visitor) => visitor.VisitStatement(this);
}

public sealed class StatementOperation<TDetails> : StatementOperation where TDetails : SemanticDetail
{
    private readonly string table;
    private readonly Func<Row,TDetails> create;
    internal StatementOperation(RoslynObject owner,long id,Row row,string table,Func<Row,TDetails> create) : base(owner,id,row)
    { this.table=table; this.create=create; }
    public TDetails Payload => Object.Detail(table,Id,create);
}

public sealed class FunctionBody
{
    internal FunctionBody(FunctionSymbol function,SemanticNode root,Scope? scope) { Function=function; Root=root; Scope=scope; }
    public FunctionSymbol Function { get; }
    public SemanticNode Root { get; }
    public Scope? Scope { get; }
    public IReadOnlyList<VariableSymbol> Locals => Function.Object.List("WITH RECURSIVE nested(id) AS (SELECT id FROM scope WHERE id=$owner UNION ALL SELECT s.id FROM scope s JOIN nested p ON s.parent_id=p.id JOIN kind k ON k.id=s.kind_id WHERE k.name NOT IN ('function','class','prototype','template','namespace','file','sck_function','sck_class_struct_union','sck_class_reactivation','sck_func_prototype','sck_template_declaration','sck_template_instantiation','sck_instantiation_context','sck_file','sck_namespace','sck_namespace_extension','sck_namespace_reactivation','sck_module_decl_import','sck_module_isolated')) SELECT DISTINCT v.* FROM nested n JOIN scope_member m ON m.owner_id=n.id JOIN variable_symbol v ON v.id=m.member_id WHERE v.is_parameter=0 ORDER BY v.id",Scope?.Id??0,r=>Function.Object.Variable(r.Long("id")),"variable_symbol");
    private const string LifetimesQuery = """
        WITH RECURSIVE own_scope(id) AS (
          SELECT body_scope_id FROM routine_symbol WHERE id=$owner AND body_scope_id IS NOT NULL
          UNION SELECT s.id FROM scope s JOIN own_scope p ON s.parent_id=p.id JOIN kind k ON k.id=s.kind_id
          WHERE k.name NOT IN ('function','class','prototype','template','namespace','file','sck_function','sck_class_struct_union','sck_class_reactivation','sck_func_prototype','sck_template_declaration','sck_template_instantiation','sck_instantiation_context','sck_file','sck_namespace','sck_namespace_extension','sck_namespace_reactivation','sck_module_decl_import','sck_module_isolated')
        ), node_root(id) AS (
          SELECT body_root_id FROM routine_symbol WHERE id=$owner AND body_root_id IS NOT NULL
          UNION SELECT s.initialization_id FROM scope_initialization s JOIN own_scope r ON r.id=s.owner_id
          UNION SELECT i.initialization_id FROM local_static_initialization i JOIN scope_local_static_initialization s ON s.initialization_id=i.id JOIN own_scope r ON r.id=s.owner_id WHERE i.initialization_id IS NOT NULL
          UNION SELECT i.initialization_id FROM constructor_initializer i JOIN scope_constructor_initializer s ON s.initializer_id=i.id JOIN own_scope r ON r.id=s.owner_id WHERE i.initialization_id IS NOT NULL
          UNION SELECT f.default_initialization_id FROM constructor_initializer i JOIN field_symbol f ON f.id=i.field_id JOIN scope_constructor_initializer s ON s.initializer_id=i.id JOIN own_scope r ON r.id=s.owner_id WHERE i.uses_field_initializer=1 AND f.default_initialization_id IS NOT NULL
        ), lifetime_root(id) AS (
          SELECT id FROM lifetime WHERE bound_entity_id=$owner
          UNION SELECT s.lifetime_id FROM scope s JOIN own_scope r ON r.id=s.id
          UNION SELECT s.function_static_lifetime_id FROM scope s JOIN own_scope r ON r.id=s.id
          UNION SELECT i.lifetime_id FROM local_static_initialization i JOIN scope_local_static_initialization s ON s.initialization_id=i.id JOIN own_scope r ON r.id=s.owner_id
        ), reachable(category,id) AS (
          SELECT 0,id FROM node_root WHERE id IS NOT NULL
          UNION SELECT 1,id FROM lifetime_root WHERE id IS NOT NULL
          UNION SELECT 0,c.child_id FROM structural_child c JOIN reachable r ON r.id=c.owner_id WHERE r.category=0
          UNION SELECT 0,e.initialization_id FROM lowered_eh_operation e JOIN reachable r ON r.id=e.id WHERE r.category=0 AND e.initialization_id IS NOT NULL
          UNION SELECT 0,i.master_id FROM initialization i JOIN reachable r ON r.id=i.id WHERE r.category=0 AND i.master_id IS NOT NULL
          UNION SELECT 0,i.next_destruction_id FROM initialization i JOIN reachable r ON r.id=i.id WHERE r.category=0 AND i.next_destruction_id IS NOT NULL
          UNION SELECT 0,h.parameter_initialization_id FROM handler h JOIN try_handler th ON th.handler_id=h.id JOIN try_statement s ON s.descriptor_id=th.owner_id JOIN reachable r ON r.id=s.id WHERE r.category=0 AND h.parameter_initialization_id IS NOT NULL
          UNION SELECT 1,b.lifetime_id FROM block_descriptor b JOIN block_statement s ON s.descriptor_id=b.id JOIN reachable r ON r.id=s.id WHERE r.category=0 AND b.lifetime_id IS NOT NULL
          UNION SELECT 1,t.lifetime_id FROM try_descriptor t JOIN try_statement s ON s.descriptor_id=t.id JOIN reachable r ON r.id=s.id WHERE r.category=0 AND t.lifetime_id IS NOT NULL
          UNION SELECT 1,l.lifetime_id FROM label_symbol l JOIN label_statement s ON s.label_id=l.id JOIN reachable r ON r.id=s.id WHERE r.category=0 AND l.lifetime_id IS NOT NULL
          UNION SELECT 1,i.lifetime_id FROM initialization i JOIN reachable r ON r.id=i.id WHERE r.category=0 AND i.lifetime_id IS NOT NULL
          UNION SELECT 1,i.init_expression_lifetime_id FROM initialization i JOIN reachable r ON r.id=i.id WHERE r.category=0 AND i.init_expression_lifetime_id IS NOT NULL
          UNION SELECT 1,i.overlapping_lifetime_id FROM initialization i JOIN reachable r ON r.id=i.id WHERE r.category=0 AND i.overlapping_lifetime_id IS NOT NULL
          UNION SELECT 1,e.lifetime_id FROM lifetime_expression e JOIN reachable r ON r.id=e.id WHERE r.category=0 AND e.lifetime_id IS NOT NULL
          UNION SELECT 1,c.child_id FROM lifetime_child c JOIN reachable r ON r.id=c.owner_id WHERE r.category=1
          UNION SELECT 0,c.initialization_id FROM lifetime_cleanup c JOIN reachable r ON r.id=c.owner_id WHERE r.category=1
        ) SELECT l.* FROM lifetime l JOIN reachable r ON r.id=l.id AND r.category=1 ORDER BY l.id
        """;
    /// <summary>Lifetimes reachable through actual scope, body and destruction-recipe relationships. This does not assign ownership to the function.</summary>
    public IReadOnlyList<Lifetime> Lifetimes => Function.Object.List(LifetimesQuery,Function.Id,r=>Function.Object.Lifetime(r.Long("id")),"lifetime");
    public ExceptionHandlingInfo ExceptionHandling => new(Function);
    public IEnumerable<Operation> DescendantOperations(CancellationToken cancellationToken=default) => Root.DescendantOperations(cancellationToken);
}

public abstract partial class SemanticVisitor
{
    public virtual void Visit(SemanticNode node) => node.Accept(this);
    public virtual void VisitNode(SemanticNode node) { foreach(var child in node.Children) Visit(child); }
    public virtual void VisitOperation(Operation operation) => VisitNode(operation);
    public virtual void VisitStatement(StatementOperation statement) => VisitOperation(statement);
    public virtual void VisitInitialization(InitializationOperation initialization) => VisitOperation(initialization);
    public virtual void VisitCall(CallOperation call) => VisitOperator(call);
}

public sealed class InitializationOperation : Operation
{
    internal InitializationOperation(RoslynObject owner,long id,Row row) : base(owner,id,row) { }
    public Initialization Initialization => Object.Initialization(Id);
    public InitializationDetails Details => Initialization.Details;
    public override CppType? Type => Details.Type;
    public SemanticEntity? Target => Details.Target;
    public ExpressionOperation? Expression => Details.Expression;
    public FunctionSymbol? Constructor => Details.Constructor;
    public FunctionSymbol? Destructor => Details.Destructor;
    public override void Accept(SemanticVisitor visitor) => visitor.VisitInitialization(this);
}
