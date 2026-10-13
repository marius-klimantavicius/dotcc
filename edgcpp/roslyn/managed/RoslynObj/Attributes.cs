using System.Numerics;

namespace RoslynObj;

/// <summary>One retained attribute descriptor, independent of the lists that refer to it.</summary>
public sealed class Attribute : SemanticEntity
{
    internal Attribute(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public AttributeDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public SemanticKind? Family => Details.Family;
    public SemanticKind? Location => Details.Location;
    public override string? Name => Details.Name;
    public string? NamespaceName => Details.NamespaceName;
    public AttributeGroup? Group => Details.Group;
    public SourceInfo? Source => Details.SourceOrigin;
    public bool IsPackExpansion => Details.IsPackExpansion;
    public IReadOnlyList<SemanticEntity> Owners => Object.List("SELECT e.* FROM entity e WHERE e.id IN (SELECT r.owner_id FROM owner_attribute r WHERE r.attribute_id=$owner AND NOT EXISTS (SELECT 1 FROM source_correspondence c WHERE c.id=r.owner_id) UNION SELECT e.owner_id FROM owner_attribute r JOIN entity_source_correspondence e ON e.source_correspondence_id=r.owner_id WHERE r.attribute_id=$owner) ORDER BY e.id",Id,r=>Object.Entity(r.Long("id")),"entity");
    public SemanticEntity? Owner
    {
        get { var owners=Owners.Take(2).ToArray();return owners.Length==1?owners[0]:null; }
    }
    public IReadOnlyList<AttributeArgument> Arguments => Object.List("SELECT a.* FROM attribute_argument_link r JOIN attribute_argument a ON a.id=r.argument_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.AttributeArgument(r.Long("id")),"attribute_argument");
}

/// <summary>An actual attribute argument, which can be shared by distinct attributes.</summary>
public sealed class AttributeArgument : SemanticEntity
{
    internal AttributeArgument(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public AttributeArgumentDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public Constant? Constant => Details.Constant;
    public CppType? Type => Details.Type;
    public ExpressionOperation? Expression => Details.Expression;
    public string? TokenText => Details.TokenText;
    public SemanticKind? TokenKind => Details.TokenKind;
    public bool IsPackExpansion => Details.IsPackExpansion;
    public bool IsLocalExpressionReference => Details.IsLocalExpressionReference;
    public ScopedExpression? ScopedExpression => Details.ScopedExpression;
    public SourceInfo? Source => Details.SourceOrigin;
    public IReadOnlyList<Attribute> OwningAttributes => Object.List("SELECT a.* FROM attribute a WHERE a.id IN (SELECT owner_id FROM attribute_argument_link WHERE argument_id=$owner) ORDER BY a.id",Id,r=>Object.Attribute(r.Long("id")),"attribute");
}

public sealed class AttributeGroup : SemanticEntity
{
    internal AttributeGroup(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public AttributeGroupDetails Details { get; }
    public SourceInfo? Source => Details.SourceOrigin;
    public IReadOnlyList<Attribute> Members => Object.List("SELECT * FROM attribute WHERE group_id=$owner ORDER BY id",Id,r=>Object.Attribute(r.Long("id")),"attribute");
}

/// <summary>A retained prospective member descriptor used by reflection.</summary>
public sealed class DataMemberSpecification : SemanticEntity
{
    internal DataMemberSpecification(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public DataMemberSpecDetails Details { get; }
    public CppType Type => Details.Type;
    public override string? Name => Details.Name;
    /// <summary>Requested target alignment; zero leaves the request unset.</summary>
    public BigInteger RequestedAlignment => Details.RequestedAlignment;
    /// <summary>Requested bit-field width; zero denotes an ordinary member.</summary>
    public BigInteger BitWidth => Details.BitWidth;
    public bool NoUniqueAddress => Details.NoUniqueAddress;
    public IReadOnlyList<Attribute> Annotations => Attributes;
}
