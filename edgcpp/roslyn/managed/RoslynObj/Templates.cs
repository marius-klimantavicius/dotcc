namespace RoslynObj;

public sealed class TemplateDescriptor : SemanticEntity
{
    private readonly Lazy<IReadOnlyList<Symbol>> declarationSymbols;
    internal TemplateDescriptor(RoslynObject owner, long id, Row row) : base(owner, id)
    {
        Details = new(owner,row);
        declarationSymbols = new(() => Object.List("SELECT s.* FROM symbol s WHERE s.id IN (SELECT id FROM template_symbol WHERE descriptor_id=$owner UNION SELECT id FROM template_parameter WHERE placeholder_template_id=$owner) ORDER BY s.id",Id,r=>Object.Symbol(r.Long("id")),"symbol"));
    }
    public TemplateDefinitionDetails Details { get; }
    public IReadOnlyList<Symbol> DeclarationSymbols => declarationSymbols.Value;
    public Symbol? DeclarationSymbol => DeclarationSymbols.Count==1 ? DeclarationSymbols[0] : null;
    public TemplateParameter? DeclaredParameter => DeclarationSymbol as TemplateParameter;
    public IReadOnlyList<TemplateParameter> DeclaredParameters => Object.List("SELECT * FROM template_parameter WHERE placeholder_template_id=$owner ORDER BY id",Id,r=>Object.TemplateParameter(r.Long("id")),"template_parameter");
    public override string? Name => base.Name;
    public SourceInfo? Source => SourceCorrespondence?.Source;
    public SemanticKind Kind => Details.Kind;
    public SemanticKind TemplateKind => Details.Kind;
    public SemanticEntity? Pattern => Details.Pattern;
    public TemplateDescriptor? OriginalDefinition => Details.CanonicalTemplate;
    public TemplateDescriptor? Definition => Details.DefinitionTemplate;
    public TemplateDescriptor? Prototype => Details.PrototypeTemplate;
    public ExpressionOperation? Constraint => Details.Constraint;
    public bool IsPack => Details.IsPack;
    public IReadOnlyList<TemplateParameter> Parameters => Object.List("SELECT * FROM template_parameter WHERE template_id=$owner ORDER BY depth,ordinal", Id, r => Object.TemplateParameter(r.Long("id")), "template_parameter");
    public IReadOnlyList<TemplateHeader> Headers => Object.List("SELECT * FROM template_header WHERE template_id=$owner ORDER BY id",Id,r=>Object.Header(r.Long("id")),"template_header");
    public string? DefinitionText => Details.DefinitionText;
    public IReadOnlyList<TemplateInstantiation> Instances => Object.List("SELECT * FROM template_instance WHERE template_id=$owner ORDER BY id", Id, r => Object.Instance(r.Long("id")), "template_instance");
    public IReadOnlyList<TemplateSpecialization> Specializations => Object.List("SELECT * FROM template_specialization WHERE primary_template_id=$owner ORDER BY id", Id, r => Object.Specialization(r.Long("id")), "template_specialization");
}

public sealed class TemplateSymbol : Symbol
{
    internal TemplateSymbol(RoslynObject owner,long id,Row row) : base(owner,id) => TemplateDetails=new(owner,row);
    public TemplateSymbolDetails TemplateDetails { get; }
    public TemplateDescriptor Descriptor => TemplateDetails.Descriptor;
    public TemplateDefinitionDetails Details => Descriptor.Details;
    public SemanticKind TemplateKind => Descriptor.TemplateKind;
    public SemanticEntity? Pattern => Descriptor.Pattern;
    public TemplateDescriptor? OriginalDefinition => Descriptor.OriginalDefinition;
    public TemplateDescriptor? Definition => Descriptor.Definition;
    public TemplateDescriptor? Prototype => Descriptor.Prototype;
    public ExpressionOperation? Constraint => Descriptor.Constraint;
    public bool IsPack => Descriptor.IsPack;
    public IReadOnlyList<TemplateParameter> Parameters => Descriptor.Parameters;
    public IReadOnlyList<TemplateHeader> Headers => Descriptor.Headers;
    public string? DefinitionText => Descriptor.DefinitionText;
    public IReadOnlyList<TemplateInstantiation> Instances => Descriptor.Instances;
    public IReadOnlyList<TemplateSpecialization> Specializations => Descriptor.Specializations;
}

public sealed class TemplateInstantiation : SemanticEntity
{
    internal TemplateInstantiation(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public TemplateInstanceDetails Details { get; }
    public TemplateDescriptor? OriginalDefinition => Details.Template;
    public SemanticEntity Result => Details.Result;
    public SemanticKind State => Details.State;
    public SourcePosition? Source => Details.SourcePosition;
    public bool IsDependent => Details.IsDependent;
    public bool IsPrototype => Details.IsPrototype;
    public bool IsSuppressed => Details.IsSuppressed;
    public bool IsExplicit => Details.IsExplicit;
    public bool IsSpecialized => Details.IsSpecialized;
    public TemplateSpecialization? SelectedSpecialization => Details.SelectedSpecialization;
    public IReadOnlyList<TemplateArgument> Arguments => Object.List("SELECT a.* FROM template_instance_argument r JOIN template_argument a ON a.id=r.argument_id JOIN kind k ON k.id=r.role_id WHERE r.owner_id=$owner AND k.name IN ('primary','primary_template') ORDER BY r.ordinal", Id, r => Object.Argument(r.Long("id")), "template_argument");
    public IReadOnlyList<TemplateArgument> SelectedPartialArguments => Object.List("SELECT a.* FROM template_instance_argument r JOIN template_argument a ON a.id=r.argument_id JOIN kind k ON k.id=r.role_id WHERE r.owner_id=$owner AND k.name IN ('selected_partial','selected_partial_specialization') ORDER BY r.ordinal",Id,r=>Object.Argument(r.Long("id")),"template_argument");
    public IReadOnlyList<TemplateInstanceArgumentDetails> ArgumentMappings => Object.List("SELECT * FROM template_instance_argument WHERE owner_id=$owner ORDER BY role_id,ordinal", Id, r => new TemplateInstanceArgumentDetails(Object, r));
    public IReadOnlyList<CppType> TypeArguments => Object.List("SELECT t.* FROM template_instance_argument r JOIN template_argument a ON a.id=r.argument_id JOIN type t ON t.id=a.type_id JOIN kind k ON k.id=r.role_id WHERE r.owner_id=$owner AND k.name IN ('primary','primary_template') ORDER BY r.ordinal", Id, r => Object.Type(r.Long("id")), "type");
}

public sealed class TemplateSpecialization : SemanticEntity
{
    internal TemplateSpecialization(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public TemplateSpecializationDetails Details { get; }
    public TemplateDescriptor PrimaryTemplate => Details.PrimaryTemplate;
    public SemanticEntity Entity => Details.Entity;
    public SemanticKind Kind => Details.Kind;
    public ExpressionOperation? Constraint => Details.Constraint;
}

public sealed class TemplateArgument : SemanticEntity
{
    internal TemplateArgument(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public TemplateArgumentDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public CppType? Type => Details.Type;
    public Constant? Constant => Details.Constant;
    public Operation? Expression => Details.Expression;
    public TemplateDescriptor? Template => Details.Template;
    /// <summary>Whether this argument is a recorded pack or a pack-expansion marker. Details.IsPack exposes the retained flag itself.</summary>
    public bool IsPack => Details.IsPack || Kind.Name=="tak_start_of_pack_expansion";
    public bool IsPackElement => Details.IsPackElement;
    public bool IsExplicit => Details.IsExplicit;
    public bool IsProvisional => Details.IsProvisional;
    public IReadOnlyList<TemplateArgument> Elements => Object.List("SELECT a.* FROM template_argument_pack_element r JOIN template_argument a ON a.id=r.argument_id WHERE r.owner_id=$owner ORDER BY r.ordinal", Id, r => Object.Argument(r.Long("id")), "template_argument");
}

public sealed class TemplateHeader : SemanticEntity
{
    internal TemplateHeader(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public TemplateHeaderDetails Details { get; }
    public TemplateDescriptor? Template => Details.Template;
    public TemplateHeader? Parent => Details.Parent;
    public Scope? Scope => Details.Scope;
    public SourceInfo? Source => Details.SourceOrigin;
    public RequiresClause? RequiresClause => Details.RequiresClause;
    public ExpressionOperation? Constraint => RequiresClause?.Constraint;
    public bool IsGeneric => Details.IsGeneric;
    public IReadOnlyList<TemplateParameter> Parameters => Object.List("SELECT * FROM template_parameter WHERE header_id=$owner ORDER BY ordinal,id",Id,r=>Object.TemplateParameter(r.Long("id")),"template_parameter");
    public IReadOnlyList<GenericConstraintClause> GenericConstraints => Object.List("SELECT * FROM template_generic_constraint_clause WHERE header_id=$owner ORDER BY ordinal,id",Id,r=>Object.GenericClause(r.Long("id")),"template_generic_constraint_clause");
}

public sealed class TemplateParameter : Symbol
{
    internal TemplateParameter(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public TemplateParameterDetails Details { get; }
    public SemanticKind ParameterKind => Details.Kind;
    public long Ordinal => Details.Ordinal;
    public long Depth => Details.Depth;
    public bool IsPack => Details.IsPack;
    public bool IsAbbreviated => Details.IsAbbreviated;
    public TemplateHeader? Header => Details.Header;
    public CppType? PlaceholderType => Details.PlaceholderType;
    public Constant? PlaceholderConstant => Details.PlaceholderConstant;
    public TemplateDescriptor? PlaceholderTemplate => Details.PlaceholderTemplate;
    public TemplateArgument? DefaultArgument => Details.DefaultArgument;
    public ExpressionOperation? Constraint => Details.Constraint;
}

public sealed class GenericConstraintClause : SemanticEntity
{
    internal GenericConstraintClause(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public TemplateGenericConstraintClauseDetails Details { get; }
    public TemplateHeader Header => Details.Header;
    public CppType ParameterType => Details.ParameterType;
    public SourcePosition? TypePosition => Details.TypePosition;
    public SourcePosition? WherePosition => Details.WherePosition;
    public IReadOnlyList<TemplateGenericConstraintDetails> Constraints => Object.List("SELECT * FROM template_generic_constraint WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new TemplateGenericConstraintDetails(Object,r));
}
