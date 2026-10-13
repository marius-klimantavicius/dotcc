using System.Numerics;
namespace RoslynObj;

public sealed class MicrosoftAttribute : SemanticEntity
{
    internal MicrosoftAttribute(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public MsAttributeDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public override string? Name => Details.Name;
    public string? Text => Details.Text;
    public SemanticEntity? Target => Details.Target;
    public SourcePosition? Position => Details.SourcePosition;
    public SourceEntry? SourceEntry => Details.SourceEntry;
    public MicrosoftAttribute? NextInBlock => Details.NextInBlock;
    public IReadOnlyList<MicrosoftAttributeArgument> Arguments => Object.List("SELECT a.* FROM ms_attribute_argument_link r JOIN ms_attribute_argument a ON a.id=r.argument_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.MicrosoftAttributeArgument(r.Long("id")),"ms_attribute_argument");
    public IReadOnlyList<SemanticKind> Targets => Object.List("SELECT * FROM ms_attribute_target WHERE owner_id=$owner ORDER BY ordinal",Id,r=>Object.Kind(r.Long("kind_id")));
}

public sealed class MicrosoftAttributeArgument : SemanticEntity
{
    internal MicrosoftAttributeArgument(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public MsAttributeArgumentDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public string? ParameterName => Details.ParameterName;
    public BigInteger? Integer => Details.IntegerValue;
    public bool? Boolean => Details.BooleanValue;
    public long? EnumerationOrdinal => Details.EnumerationOrdinal;
    public Constant? String => Details.StringConstant;
    public string? OtherText => Details.OtherText;
    public string? Uuid => Details.UuidText;
}

public sealed class ScopedExpression : SemanticEntity
{
    internal ScopedExpression(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public ScopedExpressionDetails Details { get; }
    public ExpressionOperation Expression => Details.Expression;
    public SourceInfo? Source => SourceCorrespondence?.Source;
}
