namespace RoslynObj;

public sealed class TranslationUnit : SemanticEntity
{
    internal TranslationUnit(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public TranslationUnitDetails Details { get; }
    public Scope GlobalScope => Details.GlobalScope;
    public string LanguageMode => Details.LanguageMode;
    public SourceFile? PrimaryInput => Details.PrimaryInput;
    public FunctionSymbol? EntryPoint => Details.EntryPoint;
    public IReadOnlyList<FunctionSymbol> Functions => Object.List("SELECT * FROM routine_symbol ORDER BY id", 0, r => Object.Function(r.Long("id")), "routine_symbol");
    public IReadOnlyList<VariableSymbol> Variables => Object.List("SELECT * FROM variable_symbol ORDER BY id", 0, r => Object.Variable(r.Long("id")), "variable_symbol");
    public IReadOnlyList<NamespaceSymbol> Namespaces => Object.List("SELECT * FROM namespace_symbol ORDER BY id", 0, r => Object.Namespace(r.Long("id")), "namespace_symbol");
    public IReadOnlyList<CppType> Types => Object.List("SELECT * FROM type ORDER BY id", 0, r => Object.Type(r.Long("id")), "type");
    public IReadOnlyList<TemplateSymbol> Templates => Object.List("SELECT * FROM template_definition ORDER BY id", 0, r => Object.Template(r.Long("id")), "template_definition");
    public IReadOnlyList<Declaration> Declarations => Object.List("SELECT * FROM declaration ORDER BY id", 0, r => Object.Declaration(r.Long("id")), "declaration");
    public IReadOnlyList<SemanticEntity> Entities => Object.List("SELECT * FROM entity ORDER BY id", 0, r => Object.Entity(r.Long("id")), "entity");
    public IReadOnlyList<Constant> Constants => Object.List("SELECT * FROM constant ORDER BY id",0,r=>Object.Constant(r.Long("id")),"constant");
    public IReadOnlyList<TranslationUnitRootDetails> Roots => Object.List("SELECT * FROM translation_unit_root WHERE owner_id=$owner ORDER BY kind_id,ordinal",Id,r=>new TranslationUnitRootDetails(Object,r));
    public IReadOnlyList<FileScopeDynamicInitRoutineDetails> FileScopeInitializationRoutines => Object.List("SELECT * FROM file_scope_dynamic_init_routine WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new FileScopeDynamicInitRoutineDetails(Object,r));
    public IReadOnlyList<VirtualTableDetails> VirtualTables => Object.List("SELECT * FROM virtual_table ORDER BY id",0,r=>new VirtualTableDetails(Object,r),"virtual_table");
    public IReadOnlyList<SourceFile> Sources => Object.List("SELECT * FROM source_file ORDER BY id", 0, r => Object.File(r.Long("id")), "source_file");
    public IReadOnlyList<SourceMappingSegmentDetails> SourceMappings => Object.List("SELECT * FROM source_mapping_segment ORDER BY id", 0, r => new SourceMappingSegmentDetails(Object, r), "source_mapping_segment");
    public IReadOnlyList<CppModule> Modules => Object.List("SELECT * FROM module ORDER BY id",0,r=>Object.Module(r.Long("id")),"module");
    public IReadOnlyList<ModuleImport> ModuleImports => Object.List("SELECT m.* FROM translation_unit_module_import r JOIN module_import m ON m.id=r.import_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.ModuleImport(r.Long("id")),"module_import");
    public IReadOnlyList<StaticInitializationDetails> StaticInitializations => Object.List("SELECT * FROM static_initialization WHERE owner_id=$owner ORDER BY ordinal", Id, r => new StaticInitializationDetails(Object, r));
    public IReadOnlyList<BuildCapability> Capabilities => Object.List("SELECT * FROM build_capability ORDER BY name_id", 0,
        r => new BuildCapability(Object.Text(r.Long("name_id"))!, Object.Text(r.OptionalLong("value_id")), r.Bool("required"), r.OptionalBool("enabled"), Object.Kind(r.Long("state_id"))));
    public IReadOnlyList<string> InvocationArguments => Object.List("SELECT * FROM invocation_argument ORDER BY ordinal", 0, r => Object.Text(r.Long("text_id"))!);
    public MacroCollection Macros => new(Object);
    public SemanticModel SemanticModel => new(Object);
}

public class Symbol : SemanticEntity
{
    internal Symbol(RoslynObject owner, long id) : base(owner, id) { }
    public SymbolDetails SymbolDetails => Object.Detail("symbol", Id, r => new SymbolDetails(Object, r));
    public string? Name => Object.Name(Object.Store.Required("symbol", Id).OptionalLong("name_id"));
    public SemanticKind Kind => SymbolDetails.Kind;
    public SemanticEntity? ContainingEntity => SymbolDetails.ContainingEntity;
    public CppType? DeclaredType => SymbolDetails.Type;
    public SourceInfo? Source => SymbolDetails.SourceOrigin;
    public bool IsCompilerGenerated => SymbolDetails.CompilerGenerated;
    public IReadOnlyList<Declaration> Declarations => Object.List("SELECT * FROM declaration WHERE symbol_id=$owner ORDER BY id", Id, r => Object.Declaration(r.Long("id")), "declaration");
    public override string ToString() => Name ?? Kind.Name;
}

public sealed class FunctionSymbol : Symbol
{
    internal FunctionSymbol(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public RoutineSymbolDetails Details { get; }
    public RoutineType Signature => (RoutineType)Details.RoutineType;
    public CppType ReturnType => Signature.ReturnType;
    public CppType? ContainingType => Details.ContainingType;
    private const string ParameterQuery = "SELECT p.*,t.owner_id,t.ordinal,t.parameter_type_id,t.role_id,t.base_subobject_id,COALESCE(r.role_id,t.role_id) AS resolved_role_id,r.parameter_id AS retained_variable_id,r.default_initialization_id AS retained_default_id,r.is_source_parameter AS retained_source FROM routine_type_parameter t JOIN parameter_type p ON p.id=t.parameter_type_id LEFT JOIN routine_parameter r ON r.owner_id=$owner AND r.ordinal=t.ordinal WHERE t.owner_id=(SELECT routine_type_id FROM routine_symbol WHERE id=$owner) ORDER BY t.ordinal";
    public IReadOnlyList<Parameter> Parameters => Object.List(ParameterQuery, Id, r => new Parameter(Object, r),"parameter_type");
    public IReadOnlyList<Parameter> SourceParameters => Object.List(ParameterQuery.Replace("ORDER BY t.ordinal", "AND r.is_source_parameter=1 ORDER BY t.ordinal"), Id, r => new Parameter(Object, r),"parameter_type");
    public FunctionBody? Body => Details.BodyRoot is { } root ? new(this, root, Details.BodyScope) : null;
    public TemplateSymbol? Template => Details.Template;
    public TemplateInstantiation? Instantiation => Details.Instance;
    public IReadOnlyList<TemplateArgument> TemplateArguments => Instantiation?.Arguments ?? [];
    public IReadOnlyList<CppType> TypeArguments => Instantiation?.TypeArguments ?? [];
    public FunctionSymbol? PrimaryEntry => Details.PrimaryEntry;
    public IReadOnlyList<AbiRoutineRelationshipDetails> AbiRelationships => Object.List("SELECT * FROM abi_routine_relationship WHERE owner_id=$owner ORDER BY ordinal", Id, r => new AbiRoutineRelationshipDetails(Object, r));
    public IReadOnlyList<OverrideRelationDetails> Overrides => Object.List("SELECT * FROM override_relation WHERE owner_id=$owner ORDER BY ordinal", Id, r => new OverrideRelationDetails(Object, r));
}

public class SignatureParameter
{
    internal SignatureParameter(RoslynObject owner,Row row) => Details=new(owner,row);
    public RoutineTypeParameterDetails Details { get; }
    public ParameterType Descriptor => Details.ParameterType;
    public virtual string? Name => Descriptor.Name;
    public CppType Type => Descriptor.Type;
    public CppType? DeclaredType => Descriptor.DeclaredType;
    public long Ordinal => Details.Ordinal;
    public virtual SemanticKind Role => Details.Role;
    public ExpressionOperation? DefaultExpression => Descriptor.DefaultExpression;
    public SourceInfo? Source => Descriptor.Source;
    public BaseSubobject? BaseSubobject => Details.BaseSubobject;
}

public sealed class Parameter : SignatureParameter
{
    private readonly RoslynObject owner;
    private readonly Row row;
    internal Parameter(RoslynObject owner, Row row) : base(owner,row) { this.owner=owner; this.row=row; }
    public VariableSymbol? Symbol => owner.Link(row.OptionalLong("retained_variable_id"),owner.Variable);
    public override string? Name => Symbol?.Name ?? base.Name;
    public override SemanticKind Role => owner.Kind(row.Long("resolved_role_id"));
    public Initialization? DefaultInitialization => owner.Link(row.OptionalLong("retained_default_id"),owner.Initialization);
    public bool IsSourceParameter => row.OptionalBool("retained_source") == true;
}

public sealed class ParameterType : SemanticEntity
{
    internal ParameterType(RoslynObject owner,long id,Row row) : base(owner,id) => Details=new(owner,row);
    public ParameterTypeDetails Details { get; }
    public CppType Type => Details.Type;
    public CppType? DeclaredType => Details.DeclaredType;
    public string? Name => Details.Name;
    public long ParameterNumber => Details.ParameterNumber;
    public SourceInfo? Source => Details.SourceOrigin;
    public ExpressionOperation? DefaultExpression => Details.DefaultExpression;
    public bool IsPack => Details.IsParameterPack;
    public bool IsPackElement => Details.IsPackElement;
    public bool IsExplicitThis => Details.IsExplicitThis;
    public IReadOnlyList<SemanticEntity> EntitiesDefinedInDefault => Object.List("SELECT * FROM parameter_default_entity WHERE owner_id=$owner ORDER BY ordinal",Id,r=>Object.Entity(r.Long("entity_id")));
}

public sealed class VariableSymbol : Symbol
{
    private readonly Lazy<StaticInitializationDetails?> regionInitializer;
    internal VariableSymbol(RoslynObject owner, long id, Row row) : base(owner, id)
    {
        Details = new(owner,row);
        regionInitializer = new(() => owner.Store.Query("SELECT * FROM static_initialization WHERE variable_id=$owner ORDER BY ordinal",id).FirstOrDefault() is {} initial ? new StaticInitializationDetails(owner,initial) : null);
    }
    public VariableSymbolDetails Details { get; }
    public CppType Type => Details.Type;
    public Initialization? Initializer => Details.Initializer ?? regionInitializer.Value?.Initialization;
    public SemanticKind InitializationKind => Details.InitializationKind;
    public TemplateInstantiation? Instantiation => Details.Instance;
    public TemplateSymbol? Template => Instantiation?.OriginalDefinition;
    public IReadOnlyList<TemplateArgument> TemplateArguments => Instantiation?.Arguments ?? [];
    public Constant? ConstantInitializer => Details.ConstantInitializer ?? regionInitializer.Value?.Constant;
    public Lifetime? Lifetime => Details.Lifetime;
    public IReadOnlyList<VariableBindingDetails> Bindings => Object.List("SELECT * FROM variable_binding WHERE owner_id=$owner ORDER BY ordinal", Id, r => new VariableBindingDetails(Object, r));
    public EhTableProjectionDetails? ExceptionTable => Object.Store.Query("SELECT * FROM eh_table_projection WHERE owner_id=$owner", Id).FirstOrDefault() is { } row ? new(Object, row) : null;
}

public sealed class FieldSymbol : Symbol
{
    internal FieldSymbol(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public FieldSymbolDetails Details { get; }
    public FieldPropertyDetails? Property => Object.Store.Get("field_property",Id) is {} row ? new(Object,row) : null;
    public CppType Type => Details.Type;
    public CppType ContainingType => Details.RecordType;
    public Initialization? DefaultInitializer => Details.DefaultInitialization;
}

public sealed class NamespaceSymbol : Symbol
{
    internal NamespaceSymbol(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public NamespaceSymbolDetails Details { get; }
    public Scope? Scope => Details.Scope;
    public IReadOnlyList<SemanticEntity> Members => Scope?.Members ?? [];
}

public sealed class LabelSymbol : Symbol
{
    internal LabelSymbol(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public LabelSymbolDetails Details { get; }
    public StatementOperation? Target => Details.Statement;
}

public sealed class Scope : SemanticEntity
{
    internal Scope(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public ScopeDetails Details { get; }
    public Scope? Parent => Details.Parent;
    public SemanticKind Kind => Details.Kind;
    public IReadOnlyList<SemanticEntity> Members => Object.List("SELECT * FROM scope_member WHERE owner_id=$owner ORDER BY ordinal", Id, r => Object.Entity(r.Long("member_id")));
    public IReadOnlyList<Initialization> DynamicInitializations => Object.List("SELECT i.* FROM scope_initialization r JOIN initialization i ON i.id=r.initialization_id WHERE r.owner_id=$owner ORDER BY r.ordinal",Id,r=>Object.Initialization(r.Long("id")),"initialization");
}

public sealed class Declaration : SemanticNode
{
    internal Declaration(RoslynObject owner, long id, Row row) : base(owner, id) => Details = new(owner, row);
    public DeclarationDetails Details { get; }
    public override SemanticKind Kind => Details.Kind;
    public SemanticEntity Symbol => Details.Symbol;
    public Scope? LexicalScope => Details.LexicalScope;
    public override SourceInfo? Source => Details.SourceOrigin;
    public override SemanticKind RepresentationRole => Object.Kind(7003);
    public override bool IsCompilerGenerated => Details.CompilerGenerated;
    public override IReadOnlyList<SemanticNode> Children => [];
    public override void Accept(SemanticVisitor visitor) => visitor.VisitDeclaration(this);
}

public sealed class EnumSymbol : TypeSymbol
{
    internal EnumSymbol(RoslynObject owner,long id,Row row) : base(owner,id,row) => Details=new(owner,row);
    public new EnumSymbolDetails Details { get; }
    public CppType? UnderlyingType => Details.UnderlyingType;
    public IReadOnlyList<EnumeratorDetails> Enumerators => Object.List("SELECT * FROM enumerator WHERE owner_id=$owner ORDER BY ordinal",Id,r=>new EnumeratorDetails(Object,r));
}

public sealed class EnumeratorSymbol : Symbol
{
    internal EnumeratorSymbol(RoslynObject owner,long id,Row row) : base(owner,id) { }
    public Constant Constant => Object.Store.Query("SELECT constant_id FROM enumerator WHERE symbol_id=$owner ORDER BY owner_id,ordinal",Id).FirstOrDefault() is {} row
        ? Object.Constant(row.Long("constant_id")) : throw new IncompatibleObjectException("An enumerator symbol has no captured constant relationship.");
    public ConstantValue Value => Constant.Value;
}

public sealed class AliasSymbol : TypeSymbol
{
    internal AliasSymbol(RoslynObject owner,long id,Row row) : base(owner,id,row) => Details=new(owner,row);
    public new AliasSymbolDetails Details { get; }
    public SemanticEntity? Target => Details.Target;
    public CppType? UnderlyingType => Details.UnderlyingType;
}

public class TypeSymbol : Symbol
{
    internal TypeSymbol(RoslynObject owner,long id,Row row) : base(owner,id) { }
    public TypeSymbolDetails Details => Object.Detail("type_symbol",Id,r=>new TypeSymbolDetails(Object,r));
    public CppType Type => Details.Type;
    public override IReadOnlyList<AttributeDetails> Attributes => Type.Attributes;
}
