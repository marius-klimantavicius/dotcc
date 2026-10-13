using System.Numerics;
namespace RoslynObj;

/// <summary>The retained spelling and qualification of one name use, without symbol lookup.</summary>
public sealed class NameReference : SemanticEntity
{
    internal NameReference(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public NameReferenceDetails Details { get; }
    public SemanticKind? Kind => Details.Kind;
    public FieldSymbol? Property => Details.Property;
    public CppType? DestructorType => Details.DestructorType;
    public NameQualifier? Qualifier => Details.Qualifier;
    public BigInteger? NumTemplateArguments => Details.NumTemplateArguments;
    public bool IsGlobalQualifiedName => Details.IsGlobalQualifiedName;
    public bool IsTemplateId => Details.IsTemplateId;
    public bool IsSuperQualified => Details.IsSuperQualified;
    public bool IsDecltypeQualified => Details.IsDecltypeQualified;
    public bool FromPrototypeInstantiation => Details.FromPrototypeInstantiation;
    public bool UsedInPrimaryDeclarator => Details.UsedInPrimaryDeclarator;
    public IReadOnlyList<TemplateArgument> OriginalTemplateArguments => Object.List("SELECT a.* FROM original_template_argument r JOIN template_argument a ON a.id=r.argument_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.Argument(r.Long("id")),"template_argument");
}

/// <summary>One actual qualifier and its retained predecessor in a qualified name.</summary>
public sealed class NameQualifier : SemanticEntity
{
    internal NameQualifier(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public NameQualifierDetails Details { get; }
    public NameQualifier? Previous => Details.PreviousQualifier;
    public SemanticEntity? Target => Details.Target;
    public override string? Name => Details.Name;
    public bool IsClass => Details.IsClass;
}
