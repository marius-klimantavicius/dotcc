using System.Numerics;
namespace RoslynObj;

public sealed class SwitchDescriptor : SemanticEntity
{
    internal SwitchDescriptor(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public SwitchDescriptorDetails Details { get; }
    public SwitchCaseEntry? DefaultCase => Details.DefaultCase;
    /// <summary>Whether the actual sorted-list head is nonnull; this is not a dependency predicate.</summary>
    public bool HasSortedCases => Details.HasSortedCases;
    public IReadOnlyList<SwitchCaseEntry> Cases => Object.List("SELECT c.* FROM switch_descriptor_case r JOIN switch_case_entry c ON c.id=r.case_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.SwitchCase(r.Long("id")),"switch_case_entry");
    public IReadOnlyList<SwitchCaseEntry> SortedCases => Object.List("SELECT c.* FROM switch_descriptor_sorted_case r JOIN switch_case_entry c ON c.id=r.case_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.SwitchCase(r.Long("id")),"switch_case_entry");
}

public sealed class SwitchCaseEntry : SemanticEntity
{
    internal SwitchCaseEntry(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public SwitchCaseEntryDetails Details { get; }
    public StatementOperation? Statement => Details.Statement;
    public Constant? LowConstant => Details.LowConstant;
    public Constant? HighConstant => Details.HighConstant;
    public bool IsDefault => LowConstant is null;
    public bool ReachableByFallThrough => Details.ReachableByFallThrough;
    public SourceInfo? Source => Details.SourceOrigin;
}

public sealed class SwitchCaseOperation : StatementOperation
{
    internal SwitchCaseOperation(RoslynObject owner,long id,Row row) : base(owner,id,row) { }
    public SwitchCaseStatementDetails Case => Object.Detail("switch_case_statement",Id,r=>new SwitchCaseStatementDetails(Object,r));
    public SwitchCaseEntry Entry => Case.Entry;
    public StatementOperation EnclosingSwitch => Case.SwitchStatement;
    public override void Accept(SemanticVisitor visitor) => visitor.VisitSwitchCase(this);
}

public sealed class VlaDimension : SemanticEntity
{
    internal VlaDimension(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public VlaDimensionDetails Details { get; }
    public CppType? Type => Details.Type;
    public ExpressionOperation? Expression => Details.Expression;
    public VlaDimension? OriginalDimension => Details.OriginalDimension;
    public VariableSymbol? DimensionVariable => Details.DimensionVariable;
    public bool InPrototypeScope => Details.InPrototypeScope;
    public bool HasSizeStatement => Details.HasSizeStatement;
}

public sealed class UpcThreadsValue : ConstantValue
{
    internal UpcThreadsValue(RoslynObject owner,long id,Row row) : base(owner,id,row) { }
    public UpcThreadsValueDetails Details => Object.Detail("upc_threads_value",Id,r=>new UpcThreadsValueDetails(Object,r));
    public BigInteger Multiplier => Details.Multiplier;
}
