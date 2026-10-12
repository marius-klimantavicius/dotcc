namespace RoslynObj;

public sealed class TemplateSymbol : SemanticEntity
{
    internal TemplateSymbol(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public TemplateDefinitionDetails Details { get; }
    public string? Name => Object.Name(Object.Store.Required("template_definition", Id).OptionalLong("name_id"));
    public SemanticEntity? Pattern => Details.Pattern;
    public SourceInfo? Source => Details.SourceOrigin;
    public IReadOnlyList<TemplateParameterDetails> Parameters => Object.List("SELECT * FROM template_parameter WHERE template_id=$owner ORDER BY depth,ordinal", Id, r => new TemplateParameterDetails(Object, r), "template_parameter");
    public IReadOnlyList<TemplateInstantiation> Instances => Object.List("SELECT * FROM template_instance WHERE template_id=$owner ORDER BY id", Id, r => Object.Instance(r.Long("id")), "template_instance");
    public IReadOnlyList<TemplateSpecializationDetails> Specializations => Object.List("SELECT * FROM template_specialization WHERE primary_template_id=$owner ORDER BY id", Id, r => new TemplateSpecializationDetails(Object, r), "template_specialization");
}

public sealed class TemplateInstantiation : SemanticEntity
{
    internal TemplateInstantiation(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public TemplateInstanceDetails Details { get; }
    public TemplateSymbol? OriginalDefinition => Details.Template;
    public SemanticEntity Result => Details.Result;
    public IReadOnlyList<TemplateArgument> Arguments => Object.List("SELECT a.* FROM template_instance_argument r JOIN template_argument a ON a.id=r.argument_id JOIN kind k ON k.id=r.role_id WHERE r.owner_id=$owner AND k.name IN ('primary','primary_template') ORDER BY r.ordinal", Id, r => Object.Argument(r.Long("id")), "template_argument");
    public IReadOnlyList<TemplateInstanceArgumentDetails> ArgumentMappings => Object.List("SELECT * FROM template_instance_argument WHERE owner_id=$owner ORDER BY role_id,ordinal", Id, r => new TemplateInstanceArgumentDetails(Object, r));
    public IReadOnlyList<CppType> TypeArguments => Object.List("SELECT t.* FROM template_instance_argument r JOIN template_argument a ON a.id=r.argument_id JOIN type t ON t.id=a.type_id JOIN kind k ON k.id=r.role_id WHERE r.owner_id=$owner AND k.name IN ('primary','primary_template') ORDER BY r.ordinal", Id, r => Object.Type(r.Long("id")), "type");
}

public sealed class TemplateArgument : SemanticEntity
{
    internal TemplateArgument(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public TemplateArgumentDetails Details { get; }
    public SemanticKind Kind => Details.Kind;
    public CppType? Type => Details.Type;
    public Constant? Constant => Details.Constant;
    public Operation? Expression => Details.Expression;
    public TemplateSymbol? Template => Details.Template;
    public bool IsPack => Details.IsPack;
    public IReadOnlyList<TemplateArgument> Elements => Object.List("SELECT a.* FROM template_argument_pack_element r JOIN template_argument a ON a.id=r.argument_id WHERE r.owner_id=$owner ORDER BY r.ordinal", Id, r => Object.Argument(r.Long("id")), "template_argument");
}
